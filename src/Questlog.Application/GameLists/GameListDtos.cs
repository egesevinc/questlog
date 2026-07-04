namespace Questlog.Application.GameLists;

public record CreateGameListRequest(string Title, string? Description, bool IsPublic);

public record UpdateGameListRequest(string Title, string? Description, bool IsPublic);

public record AddGameListItemRequest(long IgdbId, string? Note);

public record ReorderGameListItemsRequest(IReadOnlyList<Guid> OrderedItemIds);

public record GameListItemDto(
    Guid Id,
    long IgdbId,
    string GameName,
    string? CoverUrl,
    int Order,
    string? Note);

public record GameListDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsPublic,
    DateTimeOffset CreatedAt,
    IReadOnlyList<GameListItemDto> Items);

public record GameListSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsPublic,
    int ItemCount,
    DateTimeOffset CreatedAt);
