namespace Questlog.Application.Igdb;

/// <summary>
/// Abstraction over the IGDB API. Lives in Application so services depend on the
/// contract, not the concrete HTTP implementation (which lives in Infrastructure).
/// This is what makes the IGDB layer swappable and unit-testable.
/// </summary>
public interface IIgdbClient
{
    Task<IReadOnlyList<IgdbGameDto>> SearchGamesAsync(
        string query, int limit = 20, CancellationToken ct = default);

    Task<IgdbGameDto?> GetGameAsync(long igdbId, CancellationToken ct = default);
}
