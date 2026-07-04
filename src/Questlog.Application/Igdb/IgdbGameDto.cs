namespace Questlog.Application.Igdb;

/// <summary>
/// Shape of a game as returned by our IGDB integration layer to the rest of the
/// app. Deliberately decoupled from IGDB's raw response so the rest of the
/// codebase never depends on IGDB's wire format.
/// </summary>
public record IgdbGameDto(
    long IgdbId,
    string Name,
    string? Summary,
    string? CoverUrl,
    DateTimeOffset? ReleaseDate,
    IReadOnlyList<IgdbNamedDto> Genres,
    IReadOnlyList<IgdbNamedDto> Platforms);

public record IgdbNamedDto(long IgdbId, string Name, string? Abbreviation = null);
