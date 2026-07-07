using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Social;
using Questlog.Application.Users;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class FollowService : IFollowService
{
    private readonly QuestlogDbContext _db;
    private readonly ICurrentUser _currentUser;

    public FollowService(QuestlogDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() => _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task FollowAsync(Guid followeeId, CancellationToken ct = default)
    {
        var followerId = RequireUserId();
        if (followeeId == followerId)
            throw new AppException("You can't follow yourself.");

        if (!await _db.Users.AnyAsync(u => u.Id == followeeId, ct))
            throw AppException.NotFound("User");

        var already = await _db.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);
        if (already) return; // idempotent

        _db.Follows.Add(new Follow { FollowerId = followerId, FolloweeId = followeeId });
        _db.Notifications.Add(new Notification
        {
            RecipientId = followeeId,
            ActorId = followerId,
            Type = NotificationType.Follow,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnfollowAsync(Guid followeeId, CancellationToken ct = default)
    {
        var followerId = RequireUserId();
        var follow = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);
        if (follow is null) return; // no-op

        _db.Follows.Remove(follow);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetFollowersAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Follows
            .Where(f => f.FolloweeId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new UserSummaryDto(f.Follower.Id, f.Follower.Username, f.Follower.AvatarUrl))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetFollowingAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Follows
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new UserSummaryDto(f.Followee.Id, f.Followee.Username, f.Followee.AvatarUrl))
            .ToListAsync(ct);
    }

    public async Task<FollowInfoDto> GetFollowInfoAsync(Guid userId, CancellationToken ct = default)
    {
        var followerCount = await _db.Follows.CountAsync(f => f.FolloweeId == userId, ct);
        var followingCount = await _db.Follows.CountAsync(f => f.FollowerId == userId, ct);

        var me = _currentUser.UserId;
        var isFollowedByMe = me is not null &&
            await _db.Follows.AnyAsync(f => f.FollowerId == me && f.FolloweeId == userId, ct);

        return new FollowInfoDto(followerCount, followingCount, isFollowedByMe);
    }

    public async Task<IReadOnlyList<FeedItemDto>> GetFeedAsync(int limit = 30, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        limit = Math.Clamp(limit, 1, 100);

        var followeeIds = _db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FolloweeId);

        return await _db.GameLogs
            .Where(l => followeeIds.Contains(l.UserId))
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .Select(l => new FeedItemDto(
                l.Id, l.UserId, l.User.Username,
                l.Game.IgdbId, l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating,
                l.Review != null ? l.Review.Body : null,
                l.CreatedAt,
                _db.LogLikes.Count(x => x.GameLogId == l.Id),
                _db.LogLikes.Any(x => x.GameLogId == l.Id && x.UserId == userId)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FeedItemDto>> GetGlobalFeedAsync(int limit = 30, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        // Public discovery: anonymous viewers get LikedByMe = false (empty Guid).
        var me = _currentUser.UserId ?? Guid.Empty;

        return await _db.GameLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .Select(l => new FeedItemDto(
                l.Id, l.UserId, l.User.Username,
                l.Game.IgdbId, l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating,
                l.Review != null ? l.Review.Body : null,
                l.CreatedAt,
                _db.LogLikes.Count(x => x.GameLogId == l.Id),
                _db.LogLikes.Any(x => x.GameLogId == l.Id && x.UserId == me)))
            .ToListAsync(ct);
    }
}
