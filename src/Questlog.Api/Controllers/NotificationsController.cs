using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Notifications;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    /// <summary>The current user's recent notifications.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get([FromQuery] int limit = 30, CancellationToken ct = default)
        => Ok(await _notifications.GetMineAsync(limit, ct));

    /// <summary>Count of unread notifications (for the nav badge).</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount(CancellationToken ct)
        => Ok(await _notifications.GetUnreadCountAsync(ct));

    /// <summary>Mark all of the current user's notifications as read.</summary>
    [HttpPost("read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _notifications.MarkAllReadAsync(ct);
        return NoContent();
    }
}
