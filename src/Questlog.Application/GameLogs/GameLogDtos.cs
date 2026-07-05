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

/// <summary>A single log/review, with its author, like info, and comment thread.</summary>
public record LogDetailDto(
    Guid Id,
    Guid UserId,
    string Username,
    long IgdbId,
    string GameName,
    string? CoverUrl,
    LogStatus Status,
    int? Rating,
    int? HoursPlayed,
    string? ReviewBody,
    bool ContainsSpoilers,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByMe,
    IReadOnlyList<CommentDto> Comments);

public record CommentDto(Guid Id, Guid UserId, string Username, string Body, DateTimeOffset CreatedAt);

public record CreateCommentRequest([Required, StringLength(2000, MinimumLength = 1)] string Body);
