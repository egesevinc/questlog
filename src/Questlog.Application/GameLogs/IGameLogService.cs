using Questlog.Application.Games;

namespace Questlog.Application.GameLogs;

public interface IGameLogService
{
    Task<GameLogDto> CreateAsync(CreateGameLogRequest request, CancellationToken ct = default);
    Task<GameLogDto?> UpdateAsync(Guid logId, UpdateGameLogRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid logId, CancellationToken ct = default);
    Task<IReadOnlyList<GameLogDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>The current user's log for a given game, or null if they haven't logged it.</summary>
    Task<GameLogDto?> GetMineForGameAsync(long igdbId, CancellationToken ct = default);
    Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Aggregate community view of a game: average rating, counts, and recent reviews.</summary>
    Task<GameCommunityDto> GetGameCommunityAsync(long igdbId, CancellationToken ct = default);

    /// <summary>The most-logged games across all users, for discovery.</summary>
    Task<IReadOnlyList<TrendingGameDto>> GetTrendingGamesAsync(int limit = 12, CancellationToken ct = default);

    /// <summary>A user's taste summary for a single year (logs created in that year).</summary>
    Task<YearInReviewDto> GetYearInReviewAsync(Guid userId, int year, CancellationToken ct = default);

    /// <summary>A single public log/review with its author, like info, and comments.</summary>
    Task<LogDetailDto?> GetLogDetailAsync(Guid logId, CancellationToken ct = default);

    /// <summary>Add a comment to a log. Returns the created comment.</summary>
    Task<CommentDto> AddCommentAsync(Guid logId, CreateCommentRequest request, CancellationToken ct = default);

    /// <summary>Delete a comment. Allowed for the comment's author or the log's owner.</summary>
    Task<bool> DeleteCommentAsync(Guid commentId, CancellationToken ct = default);
}
