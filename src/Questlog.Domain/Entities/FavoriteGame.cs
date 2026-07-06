using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A game a user has pinned as a favourite, shown prominently on their profile
/// (the Letterboxd "favourite films" showcase). Ordered; at most four per user
/// is enforced in the application layer.
/// </summary>
public class FavoriteGame : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    /// <summary>Position in the showcase, 0-based.</summary>
    public int Order { get; set; }
}
