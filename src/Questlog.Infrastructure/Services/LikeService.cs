using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Social;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class LikeService : ILikeService
{
    private readonly QuestlogDbContext _db;
    private readonly ICurrentUser _currentUser;

    public LikeService(QuestlogDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() => _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task LikeAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();

        var ownerId = await _db.GameLogs.Where(l => l.Id == logId)
            .Select(l => (Guid?)l.UserId).FirstOrDefaultAsync(ct);
        if (ownerId is null)
            throw AppException.NotFound("Log");

        var already = await _db.LogLikes.AnyAsync(x => x.UserId == userId && x.GameLogId == logId, ct);
        if (already) return; // idempotent

        _db.LogLikes.Add(new LogLike { UserId = userId, GameLogId = logId });

        // Notify the log's owner — but not for liking your own log.
        if (ownerId.Value != userId)
        {
            _db.Notifications.Add(new Notification
            {
                RecipientId = ownerId.Value,
                ActorId = userId,
                Type = NotificationType.Like,
                GameLogId = logId,
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task UnlikeAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var like = await _db.LogLikes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.GameLogId == logId, ct);
        if (like is null) return; // no-op

        _db.LogLikes.Remove(like);
        await _db.SaveChangesAsync(ct);
    }
}
