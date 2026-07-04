using System.ComponentModel.DataAnnotations;
using Questlog.Domain.Enums;

namespace Questlog.Application.GameLogs;

public record CreateGameLogRequest(
    long IgdbId,
    LogStatus Status,
    [Range(1, 10)] int? Rating,
    [Range(0, 100_000)] int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    [StringLength(10_000)] string? ReviewBody = null,
    bool ContainsSpoilers = false);

public record UpdateGameLogRequest(
    LogStatus Status,
    [Range(1, 10)] int? Rating,
    [Range(0, 100_000)] int? HoursPlayed,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    [StringLength(10_000)] string? ReviewBody = null,
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
