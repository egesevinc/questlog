using Questlog.Domain.Enums;

namespace Questlog.Application.GameLogs;

public record CreateGameLogRequest(
    long IgdbId,
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ReviewBody = null,
    bool ContainsSpoilers = false);

public record UpdateGameLogRequest(
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ReviewBody = null,
    bool ContainsSpoilers = false);

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
    DateTimeOffset CreatedAt,
    string? ReviewBody = null,
    bool ContainsSpoilers = false);

public record ProfileStatsDto(
    int TotalLogged,
    int Completed,
    int Playing,
    int Backlog,
    int Abandoned,
    double? AverageRating,
    IReadOnlyList<GenreCountDto> TopGenres);

public record GenreCountDto(string Genre, int Count);
