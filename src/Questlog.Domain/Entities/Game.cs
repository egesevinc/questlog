using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A local cache of game metadata sourced from IGDB. We persist games we've seen
/// rather than re-fetching from IGDB on every request: IGDB rate-limits to
/// 4 req/sec and blocks browser-origin calls, so caching here keeps the app fast,
/// keeps us inside the rate limit, and lets us run rich relational queries
/// (e.g. "top genres for this user") that the external API can't.
/// </summary>
public class Game : BaseEntity
{
    /// <summary>The IGDB id. Unique; used to dedupe on import.</summary>
    public long IgdbId { get; set; }

    public string Name { get; set; } = null!;
    public string? Summary { get; set; }
    public string? CoverUrl { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }

    /// <summary>When we last refreshed this record from IGDB.</summary>
    public DateTimeOffset CachedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Platform> Platforms { get; set; } = new List<Platform>();
    public ICollection<GameLog> Logs { get; set; } = new List<GameLog>();
}

public class Genre : BaseEntity
{
    public long IgdbId { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Game> Games { get; set; } = new List<Game>();
}

public class Platform : BaseEntity
{
    public long IgdbId { get; set; }
    public string Name { get; set; } = null!;
    public string? Abbreviation { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
}
