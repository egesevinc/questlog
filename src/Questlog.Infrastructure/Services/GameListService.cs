using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.GameLists;
using Questlog.Application.Games;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class GameListService : IGameListService
{
    private readonly QuestlogDbContext _db;
    private readonly IGameService _games;
    private readonly ICurrentUser _currentUser;

    public GameListService(QuestlogDbContext db, IGameService games, ICurrentUser currentUser)
    {
        _db = db;
        _games = games;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() => _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task<GameListDto> CreateAsync(CreateGameListRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = new GameList
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            IsPublic = request.IsPublic
        };
        _db.GameLists.Add(list);
        await _db.SaveChangesAsync(ct);
        return ToDto(list);
    }

    public async Task<GameListDto?> UpdateAsync(Guid listId, UpdateGameListRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = await LoadOwnedListAsync(listId, userId, ct);
        if (list is null) return null;

        list.Title = request.Title;
        list.Description = request.Description;
        list.IsPublic = request.IsPublic;
        list.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(list);
    }

    public async Task<bool> DeleteAsync(Guid listId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = await _db.GameLists.FirstOrDefaultAsync(l => l.Id == listId, ct);
        if (list is null) return false;
        if (list.UserId != userId) throw AppException.Unauthorized("This list doesn't belong to you.");

        _db.GameLists.Remove(list);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<GameListDto?> GetAsync(Guid listId, CancellationToken ct = default)
    {
        var list = await _db.GameLists
            .Include(l => l.Items).ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);
        return list is null ? null : ToDto(list);
    }

    public async Task<IReadOnlyList<GameListSummaryDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.GameLists
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new GameListSummaryDto(
                l.Id, l.Title, l.Description, l.IsPublic, l.Items.Count, l.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<GameListDto?> AddItemAsync(Guid listId, AddGameListItemRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = await LoadOwnedListAsync(listId, userId, ct);
        if (list is null) return null;

        var detail = await _games.GetByIgdbIdAsync(request.IgdbId, ct)
                     ?? throw AppException.NotFound("Game");
        var game = await _db.Games.FirstAsync(g => g.IgdbId == detail.IgdbId, ct);

        if (list.Items.Any(i => i.GameId == game.Id))
            throw AppException.Conflict("This game is already in the list.");

        var nextOrder = list.Items.Count == 0 ? 0 : list.Items.Max(i => i.Order) + 1;
        list.Items.Add(new GameListItem
        {
            GameListId = list.Id,
            GameId = game.Id,
            Order = nextOrder,
            Note = request.Note
        });

        await _db.SaveChangesAsync(ct);
        return await GetAsync(listId, ct);
    }

    public async Task<bool> RemoveItemAsync(Guid listId, Guid itemId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = await LoadOwnedListAsync(listId, userId, ct);
        if (list is null) return false;

        var item = list.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return false;

        _db.Remove(item);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<GameListDto?> ReorderItemsAsync(Guid listId, ReorderGameListItemsRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var list = await LoadOwnedListAsync(listId, userId, ct);
        if (list is null) return null;

        for (var i = 0; i < request.OrderedItemIds.Count; i++)
        {
            var item = list.Items.FirstOrDefault(x => x.Id == request.OrderedItemIds[i]);
            if (item is not null) item.Order = i;
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(listId, ct);
    }

    private async Task<GameList?> LoadOwnedListAsync(Guid listId, Guid userId, CancellationToken ct)
    {
        var list = await _db.GameLists
            .Include(l => l.Items).ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);
        if (list is null) return null;
        if (list.UserId != userId) throw AppException.Unauthorized("This list doesn't belong to you.");
        return list;
    }

    private static GameListDto ToDto(GameList l) => new(
        l.Id, l.Title, l.Description, l.IsPublic, l.CreatedAt,
        l.Items.OrderBy(i => i.Order).Select(i => new GameListItemDto(
            i.Id, i.Game.IgdbId, i.Game.Name, i.Game.CoverUrl, i.Order, i.Note)).ToList());
}
