using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A comment on someone's game log (and its review). The conversation layer
/// under a review — think Letterboxd/Instagram comment threads.
/// </summary>
public class Comment : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GameLogId { get; set; }
    public GameLog GameLog { get; set; } = null!;

    public string Body { get; set; } = null!;
}
