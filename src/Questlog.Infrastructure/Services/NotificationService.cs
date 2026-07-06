using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Notifications;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly QuestlogDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NotificationService(QuestlogDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() => _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(int limit = 30, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        limit = Math.Clamp(limit, 1, 100);

        return await _db.Notifications
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.ActorId, n.Actor.Username,
                n.GameLogId,
                n.GameLog != null ? n.GameLog.Game.IgdbId : (long?)null,
                n.GameLog != null ? n.GameLog.Game.Name : null,
                n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return await _db.Notifications.CountAsync(n => n.RecipientId == userId && !n.IsRead, ct);
    }

    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var unread = await _db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }
}
