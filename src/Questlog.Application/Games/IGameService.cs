namespace Questlog.Application.Games;

public interface IGameService
{
    /// <summary>Search IGDB by name. Results are also upserted into the local cache.</summary>
    Task<IReadOnlyList<GameSummaryDto>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Get a game by its IGDB id. Serves from the local cache when present and
    /// fresh; otherwise fetches from IGDB and caches it. Returns null if no such game.
    /// </summary>
    Task<GameDetailDto?> GetByIgdbIdAsync(long igdbId, CancellationToken ct = default);
}
