using Questlog.Domain.Enums;

namespace Questlog.Application.Games;

public record GameSummaryDto(Guid Id, long IgdbId, string Name, string? CoverUrl, DateTimeOffset? ReleaseDate);

public record GameDetailDto(
    Guid Id,
    long IgdbId,
    string Name,
    string? Summary,
    string? CoverUrl,
    DateTimeOffset? ReleaseDate,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms);

/// <summary>What the whole community thinks of a game: aggregate ratings + recent reviews.</summary>
public record GameCommunityDto(
    double? AverageRating,
    int LogCount,
    int RatingCount,
    IReadOnlyList<GameReviewDto> Reviews);

public record GameReviewDto(
    Guid LogId,
    Guid UserId,
    string Username,
    int? Rating,
    LogStatus Status,
    string Body,
    bool ContainsSpoilers,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByMe);
