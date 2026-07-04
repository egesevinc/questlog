using Questlog.Domain.Common;
using Questlog.Domain.Enums;

namespace Questlog.Domain.Entities;

/// <summary>
/// The core entity: one user's logged relationship with one game.
/// A user has at most one log per game (enforced by a unique index on
/// UserId + GameId). Rating is on a 1–10 scale (nullable: you can log a game
/// without rating it). HoursPlayed and the started/finished dates capture the
/// game-specific nuance that a film logger never needs.
/// </summary>
public class GameLog : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public LogStatus Status { get; set; } = LogStatus.Backlog;

    /// <summary>1–10, or null if unrated. Validated in the application layer.</summary>
    public int? Rating { get; set; }

    public int? HoursPlayed { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public Review? Review { get; set; }
}
