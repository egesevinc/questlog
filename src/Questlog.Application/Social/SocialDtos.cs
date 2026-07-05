using Questlog.Domain.Enums;

namespace Questlog.Application.Social;

/// <summary>Follow counts for a profile, plus whether the current viewer follows them.</summary>
public record FollowInfoDto(int FollowerCount, int FollowingCount, bool IsFollowedByMe);

/// <summary>One entry in the activity feed: who logged what, and how.</summary>
public record FeedItemDto(
    Guid LogId,
    Guid UserId,
    string Username,
    long IgdbId,
    string GameName,
    string? CoverUrl,
    LogStatus Status,
    int? Rating,
    string? ReviewBody,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByMe);
