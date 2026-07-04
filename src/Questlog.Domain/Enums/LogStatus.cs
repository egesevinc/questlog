namespace Questlog.Domain.Enums;

/// <summary>
/// The state of a user's relationship with a game.
/// Unlike films, games are rarely a single binary "watched" event — they get
/// abandoned, replayed, sit in a backlog, or run for hundreds of hours. The log
/// status is what makes the data model game-shaped rather than film-shaped.
/// </summary>
public enum LogStatus
{
    Wishlist = 0,
    Backlog = 1,
    Playing = 2,
    Completed = 3,
    Abandoned = 4,
    Replaying = 5
}
