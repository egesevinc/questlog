using System.ComponentModel.DataAnnotations;

namespace Questlog.Application.GameLists;

public record CreateGameListRequest(
    [Required, StringLength(120, MinimumLength = 1)] string Title,
    [StringLength(2_000)] string? Description,
    bool IsPublic);

public record UpdateGameListRequest(
    [Required, StringLength(120, MinimumLength = 1)] string Title,
    [StringLength(2_000)] string? Description,
    bool IsPublic);

public record AddGameListItemRequest(long IgdbId, [StringLength(500)] string? Note);

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
    Guid UserId,
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

/// <summary>A public list surfaced for discovery, with its owner and a few cover thumbnails.</summary>
public record PublicListDto(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerId,
    string OwnerUsername,
    int ItemCount,
    IReadOnlyList<string> CoverUrls,
    DateTimeOffset CreatedAt);
