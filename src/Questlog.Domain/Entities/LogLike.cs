using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A "like" by a user on someone's game log (and its review). Unique per
/// (user, log) so a user can like a given log at most once.
/// </summary>
public class LogLike : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GameLogId { get; set; }
    public GameLog GameLog { get; set; } = null!;
}
