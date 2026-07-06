namespace Questlog.Application.Notifications;

public interface INotificationService
{
    /// <summary>The current user's recent notifications, newest first.</summary>
    Task<IReadOnlyList<NotificationDto>> GetMineAsync(int limit = 30, CancellationToken ct = default);

    /// <summary>How many unread notifications the current user has.</summary>
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);

    /// <summary>Mark all of the current user's notifications as read.</summary>
    Task MarkAllReadAsync(CancellationToken ct = default);
}
