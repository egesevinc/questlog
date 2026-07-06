using Questlog.Domain.Common;
using Questlog.Domain.Enums;

namespace Questlog.Domain.Entities;

/// <summary>
/// An event surfaced to a user: someone followed them, or liked/commented on
/// their log. Created as a side effect of the follow/like/comment actions.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>Who receives the notification.</summary>
    public Guid RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    /// <summary>Who triggered it.</summary>
    public Guid ActorId { get; set; }
    public User Actor { get; set; } = null!;

    public NotificationType Type { get; set; }

    /// <summary>The log involved (for Like/Comment); null for Follow.</summary>
    public Guid? GameLogId { get; set; }
    public GameLog? GameLog { get; set; }

    public bool IsRead { get; set; }
}
