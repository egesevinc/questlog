using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A written review. Attached to a GameLog (one review per log) so a review is
/// always anchored to a specific user's specific playthrough — which playthrough
/// a review refers to matters for games in a way it never does for films.
/// </summary>
public class Review : BaseEntity
{
    public Guid GameLogId { get; set; }
    public GameLog GameLog { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Body { get; set; } = null!;
    public bool ContainsSpoilers { get; set; }
}
