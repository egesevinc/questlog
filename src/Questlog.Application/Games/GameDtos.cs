namespace Questlog.Application.Games;

public record GameSummaryDto(Guid Id, long IgdbId, string Name, string? CoverUrl, DateTimeOffset? ReleaseDate);

public record GameDetailDto(
    Guid Id,
    long IgdbId,
    string Name,
    string? Summary,
    string? CoverUrl,
    DateTimeOffset? ReleaseDate,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms);
