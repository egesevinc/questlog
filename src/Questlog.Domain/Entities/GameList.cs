using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A user-curated, ordered list of games (e.g. "Best Soulslikes",
/// "2025 Backlog"). The Letterboxd-style curation layer.
/// </summary>
public class GameList : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = true;

    public ICollection<GameListItem> Items { get; set; } = new List<GameListItem>();
}

public class GameListItem : BaseEntity
{
    public Guid GameListId { get; set; }
    public GameList GameList { get; set; } = null!;

    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    /// <summary>Position within the list, 0-based.</summary>
    public int Order { get; set; }
    public string? Note { get; set; }
}
