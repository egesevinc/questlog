namespace Questlog.Application.GameLists;

public interface IGameListService
{
    Task<GameListDto> CreateAsync(CreateGameListRequest request, CancellationToken ct = default);
    Task<GameListDto?> UpdateAsync(Guid listId, UpdateGameListRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid listId, CancellationToken ct = default);
    Task<GameListDto?> GetAsync(Guid listId, CancellationToken ct = default);
    Task<IReadOnlyList<GameListSummaryDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Recent public lists across all users, for discovery.</summary>
    Task<IReadOnlyList<PublicListDto>> GetPublicListsAsync(int limit = 12, CancellationToken ct = default);

    Task<GameListDto?> AddItemAsync(Guid listId, AddGameListItemRequest request, CancellationToken ct = default);
    Task<bool> RemoveItemAsync(Guid listId, Guid itemId, CancellationToken ct = default);
    Task<GameListDto?> ReorderItemsAsync(Guid listId, ReorderGameListItemsRequest request, CancellationToken ct = default);
}
