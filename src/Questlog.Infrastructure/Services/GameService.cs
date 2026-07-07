using Microsoft.EntityFrameworkCore;
using Questlog.Application.Games;
using Questlog.Application.Igdb;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

/// <summary>
/// Game lookup with a read-through cache. Search always hits IGDB (queries are
/// open-ended) but upserts the results so subsequent detail lookups are local.
/// GetByIgdbId serves from the DB when the cached copy is fresh, and only calls
/// IGDB on a miss or when the cache is stale — keeping us well inside IGDB's
/// 4 req/sec limit and making the app fast.
/// </summary>
public class GameService : IGameService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(7);

    private readonly QuestlogDbContext _db;
    private readonly IIgdbClient _igdb;

    public GameService(QuestlogDbContext db, IIgdbClient igdb)
    {
        _db = db;
        _igdb = igdb;
    }

    public async Task<IReadOnlyList<GameSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<GameSummaryDto>();

        // A larger page gives the client's genre/year filters something to work with.
        var results = await _igdb.SearchGamesAsync(query, limit: 40, ct);
        var summaries = new List<GameSummaryDto>(results.Count);

        foreach (var dto in results)
        {
            var game = await UpsertGameAsync(dto, ct);
            summaries.Add(new GameSummaryDto(
                game.Id, game.IgdbId, game.Name, game.CoverUrl, game.ReleaseDate,
                game.Genres.Select(g => g.Name).ToList()));
        }

        await _db.SaveChangesAsync(ct);
        return summaries;
    }

    public async Task<GameDetailDto?> GetByIgdbIdAsync(long igdbId, CancellationToken ct = default)
    {
        var cached = await _db.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.IgdbId == igdbId, ct);

        if (cached is not null && DateTimeOffset.UtcNow - cached.CachedAt < CacheTtl)
            return ToDetail(cached);

        var dto = await _igdb.GetGameAsync(igdbId, ct);
        if (dto is null)
            return cached is null ? null : ToDetail(cached);

        var game = await UpsertGameAsync(dto, ct);
        await _db.SaveChangesAsync(ct);
        return ToDetail(game);
    }

    /// <summary>Insert or refresh a game (and its genres/platforms) from an IGDB DTO.</summary>
    private async Task<Game> UpsertGameAsync(IgdbGameDto dto, CancellationToken ct)
    {
        var game = await _db.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.IgdbId == dto.IgdbId, ct);

        if (game is null)
        {
            game = new Game { IgdbId = dto.IgdbId };
            _db.Games.Add(game);
        }

        game.Name = dto.Name;
        game.Summary = dto.Summary;
        game.CoverUrl = dto.CoverUrl;
        game.ReleaseDate = dto.ReleaseDate;
        game.CachedAt = DateTimeOffset.UtcNow;

        game.Genres = await ResolveGenresAsync(dto.Genres, ct);
        game.Platforms = await ResolvePlatformsAsync(dto.Platforms, ct);

        return game;
    }

    private async Task<List<Genre>> ResolveGenresAsync(IReadOnlyList<IgdbNamedDto> names, CancellationToken ct)
    {
        var resolved = new List<Genre>();
        foreach (var n in names)
        {
            // Check tracked-but-unsaved entities first: a search batches many games into
            // one SaveChanges call, so a shared genre may already be pending from an
            // earlier game in this same batch and won't be visible to a DB query yet.
            var genre = _db.Genres.Local.FirstOrDefault(g => g.IgdbId == n.IgdbId)
                ?? await _db.Genres.FirstOrDefaultAsync(g => g.IgdbId == n.IgdbId, ct);
            if (genre is null)
            {
                genre = new Genre { IgdbId = n.IgdbId, Name = n.Name };
                _db.Genres.Add(genre);
            }
            resolved.Add(genre);
        }
        return resolved;
    }

    private async Task<List<Platform>> ResolvePlatformsAsync(IReadOnlyList<IgdbNamedDto> names, CancellationToken ct)
    {
        var resolved = new List<Platform>();
        foreach (var n in names)
        {
            var platform = _db.Platforms.Local.FirstOrDefault(p => p.IgdbId == n.IgdbId)
                ?? await _db.Platforms.FirstOrDefaultAsync(p => p.IgdbId == n.IgdbId, ct);
            if (platform is null)
            {
                platform = new Platform { IgdbId = n.IgdbId, Name = n.Name, Abbreviation = n.Abbreviation };
                _db.Platforms.Add(platform);
            }
            resolved.Add(platform);
        }
        return resolved;
    }

    private static GameDetailDto ToDetail(Game g) => new(
        g.Id, g.IgdbId, g.Name, g.Summary, g.CoverUrl, g.ReleaseDate,
        g.Genres.Select(x => x.Name).ToList(),
        g.Platforms.Select(x => x.Name).ToList());
}
