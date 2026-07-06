using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Games;
using Questlog.Application.Users;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private const int MaxFavorites = 4;

    private readonly QuestlogDbContext _db;
    private readonly IGameService _games;
    private readonly ICurrentUser _currentUser;

    public FavoriteService(QuestlogDbContext db, IGameService games, ICurrentUser currentUser)
    {
        _db = db;
        _games = games;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<FavoriteGameDto>> GetFavoritesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.FavoriteGames
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Order)
            .Select(f => new FavoriteGameDto(f.Game.IgdbId, f.Game.Name, f.Game.CoverUrl))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FavoriteGameDto>> SetOwnFavoritesAsync(SetFavoritesRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw AppException.Unauthorized();

        // Keep the first occurrence of each id (dedupe) and cap at four, order preserved.
        var igdbIds = request.IgdbIds.Distinct().Take(MaxFavorites).ToList();

        // Ensure each game exists locally (fetches + caches from IGDB on a miss).
        foreach (var igdbId in igdbIds)
        {
            _ = await _games.GetByIgdbIdAsync(igdbId, ct)
                ?? throw AppException.NotFound($"Game {igdbId}");
        }

        var gamesByIgdb = await _db.Games
            .Where(g => igdbIds.Contains(g.IgdbId))
            .ToDictionaryAsync(g => g.IgdbId, ct);

        // Replace the whole set: drop the old, insert the new in the requested order.
        var existing = await _db.FavoriteGames.Where(f => f.UserId == userId).ToListAsync(ct);
        _db.FavoriteGames.RemoveRange(existing);

        for (var i = 0; i < igdbIds.Count; i++)
        {
            if (gamesByIgdb.TryGetValue(igdbIds[i], out var game))
                _db.FavoriteGames.Add(new FavoriteGame { UserId = userId, GameId = game.Id, Order = i });
        }

        await _db.SaveChangesAsync(ct);
        return await GetFavoritesAsync(userId, ct);
    }
}
