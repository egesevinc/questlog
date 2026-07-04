using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.GameLogs;
using Questlog.Application.Games;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class GameLogService : IGameLogService
{
    private readonly QuestlogDbContext _db;
    private readonly IGameService _games;
    private readonly ICurrentUser _currentUser;

    public GameLogService(QuestlogDbContext db, IGameService games, ICurrentUser currentUser)
    {
        _db = db;
        _games = games;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task<GameLogDto> CreateAsync(CreateGameLogRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        ValidateRating(request.Rating);

        // Ensure the game exists locally (fetches + caches from IGDB if needed).
        var detail = await _games.GetByIgdbIdAsync(request.IgdbId, ct)
                     ?? throw AppException.NotFound("Game");

        var game = await _db.Games.FirstAsync(g => g.IgdbId == detail.IgdbId, ct);

        var exists = await _db.GameLogs.AnyAsync(l => l.UserId == userId && l.GameId == game.Id, ct);
        if (exists)
            throw AppException.Conflict("You already have a log for this game. Update it instead.");

        var log = new GameLog
        {
            UserId = userId,
            GameId = game.Id,
            Status = request.Status,
            Rating = request.Rating,
            HoursPlayed = request.HoursPlayed,
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt
        };
        _db.GameLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return ToDto(log, game);
    }

    public async Task<GameLogDto?> UpdateAsync(Guid logId, UpdateGameLogRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        ValidateRating(request.Rating);

        var log = await _db.GameLogs.Include(l => l.Game)
            .FirstOrDefaultAsync(l => l.Id == logId, ct);
        if (log is null) return null;
        if (log.UserId != userId) throw AppException.Unauthorized("This log doesn't belong to you.");

        log.Status = request.Status;
        log.Rating = request.Rating;
        log.HoursPlayed = request.HoursPlayed;
        log.StartedAt = request.StartedAt;
        log.FinishedAt = request.FinishedAt;
        log.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(log, log.Game);
    }

    public async Task<bool> DeleteAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var log = await _db.GameLogs.FirstOrDefaultAsync(l => l.Id == logId, ct);
        if (log is null) return false;
        if (log.UserId != userId) throw AppException.Unauthorized("This log doesn't belong to you.");

        _db.GameLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<GameLogDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.GameLogs
            .Where(l => l.UserId == userId)
            .Include(l => l.Game)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new GameLogDto(
                l.Id, l.Game.IgdbId, l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating, l.HoursPlayed, l.StartedAt, l.FinishedAt, l.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var logs = _db.GameLogs.Where(l => l.UserId == userId);

        var total = await logs.CountAsync(ct);
        var completed = await logs.CountAsync(l => l.Status == LogStatus.Completed, ct);
        var playing = await logs.CountAsync(l => l.Status == LogStatus.Playing, ct);
        var backlog = await logs.CountAsync(l => l.Status == LogStatus.Backlog, ct);
        var abandoned = await logs.CountAsync(l => l.Status == LogStatus.Abandoned, ct);

        double? avg = await logs.Where(l => l.Rating != null)
            .Select(l => (double?)l.Rating!.Value).AverageAsync(ct);

        // Top genres across the user's logged games.
        var topGenres = await _db.GameLogs
            .Where(l => l.UserId == userId)
            .SelectMany(l => l.Game.Genres)
            .GroupBy(g => g.Name)
            .Select(grp => new GenreCountDto(grp.Key, grp.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        return new ProfileStatsDto(total, completed, playing, backlog, abandoned,
            avg.HasValue ? Math.Round(avg.Value, 2) : null, topGenres);
    }

    private static void ValidateRating(int? rating)
    {
        if (rating is < 1 or > 10)
            throw new AppException("Rating must be between 1 and 10.");
    }

    private static GameLogDto ToDto(GameLog l, Game g) => new(
        l.Id, g.IgdbId, g.Name, g.CoverUrl,
        l.Status, l.Rating, l.HoursPlayed, l.StartedAt, l.FinishedAt, l.CreatedAt);
}
