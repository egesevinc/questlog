using Questlog.Domain.Enums;

namespace Questlog.Application.GameLogs;

public record CreateGameLogRequest(
    long IgdbId,
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);

public record UpdateGameLogRequest(
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);

public record GameLogDto(
    Guid Id,
    long IgdbId,
    string GameName,
    string? CoverUrl,
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset CreatedAt);

public record ProfileStatsDto(
    int TotalLogged,
    int Completed,
    int Playing,
    int Backlog,
    int Abandoned,
    double? AverageRating,
    IReadOnlyList<GenreCountDto> TopGenres);

public record GenreCountDto(string Genre, int Count);
