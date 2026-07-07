using Questlog.Application.Users;

namespace Questlog.Application.Social;

public interface IFollowService
{
    /// <summary>Users who follow the given user.</summary>
    Task<IReadOnlyList<UserSummaryDto>> GetFollowersAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Users the given user follows.</summary>
    Task<IReadOnlyList<UserSummaryDto>> GetFollowingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Follow a user. Idempotent; rejects self-follow.</summary>
    Task FollowAsync(Guid followeeId, CancellationToken ct = default);

    /// <summary>Unfollow a user. No-op if not currently following.</summary>
    Task UnfollowAsync(Guid followeeId, CancellationToken ct = default);

    /// <summary>Follower/following counts for a user, and whether the current viewer follows them.</summary>
    Task<FollowInfoDto> GetFollowInfoAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Recent activity from the users the current user follows, newest first.</summary>
    Task<IReadOnlyList<FeedItemDto>> GetFeedAsync(int limit = 30, CancellationToken ct = default);

    /// <summary>Recent activity from everyone (public discovery feed), newest first.</summary>
    Task<IReadOnlyList<FeedItemDto>> GetGlobalFeedAsync(int limit = 30, CancellationToken ct = default);
}
