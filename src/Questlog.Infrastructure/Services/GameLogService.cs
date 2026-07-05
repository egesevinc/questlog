using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.GameLogs;
using Questlog.Application.Games;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class GameLogService : IGameLogService
{
    private readonly QuestlogDbContext _db;
    private readonly IGameService _games;
    private readonly ICurrentUser _currentUser;

    public GameLogService(QuestlogDbContext db, IGameService games, ICurrentUser currentUser)
    {
        _db = db;
        _games = games;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task<GameLogDto> CreateAsync(CreateGameLogRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        ValidateRating(request.Rating);

        // Ensure the game exists locally (fetches + caches from IGDB if needed).
        var detail = await _games.GetByIgdbIdAsync(request.IgdbId, ct)
                     ?? throw AppException.NotFound("Game");

        var game = await _db.Games.FirstAsync(g => g.IgdbId == detail.IgdbId, ct);

        var exists = await _db.GameLogs.AnyAsync(l => l.UserId == userId && l.GameId == game.Id, ct);
        if (exists)
            throw AppException.Conflict("You already have a log for this game. Update it instead.");

        var log = new GameLog
        {
            UserId = userId,
            GameId = game.Id,
            Status = request.Status,
            Rating = request.Rating,
            HoursPlayed = request.HoursPlayed,
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt
        };
        _db.GameLogs.Add(log);

        if (!string.IsNullOrWhiteSpace(request.ReviewBody))
        {
            log.Review = new Review
            {
                UserId = userId,
                Body = request.ReviewBody,
                ContainsSpoilers = request.ContainsSpoilers
            };
        }

        await _db.SaveChangesAsync(ct);

        return ToDto(log, game);
    }

    public async Task<GameLogDto?> UpdateAsync(Guid logId, UpdateGameLogRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        ValidateRating(request.Rating);

        var log = await _db.GameLogs.Include(l => l.Game).Include(l => l.Review)
            .FirstOrDefaultAsync(l => l.Id == logId, ct);
        if (log is null) return null;
        if (log.UserId != userId) throw AppException.Unauthorized("This log doesn't belong to you.");

        log.Status = request.Status;
        log.Rating = request.Rating;
        log.HoursPlayed = request.HoursPlayed;
        log.StartedAt = request.StartedAt;
        log.FinishedAt = request.FinishedAt;
        log.UpdatedAt = DateTimeOffset.UtcNow;

        if (string.IsNullOrWhiteSpace(request.ReviewBody))
        {
            if (log.Review is not null)
                _db.Reviews.Remove(log.Review);
        }
        else if (log.Review is null)
        {
            log.Review = new Review
            {
                UserId = userId,
                Body = request.ReviewBody,
                ContainsSpoilers = request.ContainsSpoilers
            };
        }
        else
        {
            log.Review.Body = request.ReviewBody;
            log.Review.ContainsSpoilers = request.ContainsSpoilers;
            log.Review.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(log, log.Game);
    }

    public async Task<bool> DeleteAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var log = await _db.GameLogs.FirstOrDefaultAsync(l => l.Id == logId, ct);
        if (log is null) return false;
        if (log.UserId != userId) throw AppException.Unauthorized("This log doesn't belong to you.");

        _db.GameLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<GameLogDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.GameLogs
            .Where(l => l.UserId == userId)
            .Include(l => l.Game)
            .Include(l => l.Review)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new GameLogDto(
                l.Id, l.Game.IgdbId, l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating, l.HoursPlayed, l.StartedAt, l.FinishedAt, l.CreatedAt,
                l.Review != null ? l.Review.Body : null,
                l.Review != null && l.Review.ContainsSpoilers))
            .ToListAsync(ct);
    }

    public async Task<GameLogDto?> GetMineForGameAsync(long igdbId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return await _db.GameLogs
            .Where(l => l.UserId == userId && l.Game.IgdbId == igdbId)
            .Include(l => l.Game)
            .Include(l => l.Review)
            .Select(l => new GameLogDto(
                l.Id, l.Game.IgdbId, l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating, l.HoursPlayed, l.StartedAt, l.FinishedAt, l.CreatedAt,
                l.Review != null ? l.Review.Body : null,
                l.Review != null && l.Review.ContainsSpoilers))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var logs = _db.GameLogs.Where(l => l.UserId == userId);

        var total = await logs.CountAsync(ct);
        var completed = await logs.CountAsync(l => l.Status == LogStatus.Completed, ct);
        var playing = await logs.CountAsync(l => l.Status == LogStatus.Playing, ct);
        var backlog = await logs.CountAsync(l => l.Status == LogStatus.Backlog, ct);
        var abandoned = await logs.CountAsync(l => l.Status == LogStatus.Abandoned, ct);

        double? avg = await logs.Where(l => l.Rating != null)
            .Select(l => (double?)l.Rating!.Value).AverageAsync(ct);

        // Top genres across the user's logged games.
        // Grouped in memory: the many-to-many GroupBy can't be translated by
        // every provider (e.g. EF Core's InMemory provider used in tests).
        var genreNames = await _db.GameLogs
            .Where(l => l.UserId == userId)
            .SelectMany(l => l.Game.Genres)
            .Select(g => g.Name)
            .ToListAsync(ct);

        var topGenres = genreNames
            .GroupBy(name => name)
            .Select(grp => new GenreCountDto(grp.Key, grp.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        return new ProfileStatsDto(total, completed, playing, backlog, abandoned,
            avg.HasValue ? Math.Round(avg.Value, 2) : null, topGenres);
    }

    public async Task<GameCommunityDto> GetGameCommunityAsync(long igdbId, CancellationToken ct = default)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.IgdbId == igdbId, ct);
        if (game is null)
            return new GameCommunityDto(null, 0, 0, Array.Empty<GameReviewDto>());

        var logs = _db.GameLogs.Where(l => l.GameId == game.Id);

        var logCount = await logs.CountAsync(ct);
        var ratingCount = await logs.CountAsync(l => l.Rating != null, ct);
        double? avg = await logs.Where(l => l.Rating != null)
            .Select(l => (double?)l.Rating!.Value).AverageAsync(ct);

        // Empty Guid never matches a real user, so anonymous viewers get LikedByMe = false.
        var me = _currentUser.UserId ?? Guid.Empty;
        var reviews = await logs
            .Where(l => l.Review != null)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .Select(l => new GameReviewDto(
                l.Id, l.UserId, l.User.Username, l.Rating, l.Status,
                l.Review!.Body, l.Review.ContainsSpoilers, l.CreatedAt,
                _db.LogLikes.Count(x => x.GameLogId == l.Id),
                _db.LogLikes.Any(x => x.GameLogId == l.Id && x.UserId == me)))
            .ToListAsync(ct);

        return new GameCommunityDto(
            avg.HasValue ? Math.Round(avg.Value, 2) : null,
            logCount, ratingCount, reviews);
    }

    public async Task<LogDetailDto?> GetLogDetailAsync(Guid logId, CancellationToken ct = default)
    {
        var me = _currentUser.UserId ?? Guid.Empty;

        var detail = await _db.GameLogs
            .Where(l => l.Id == logId)
            .Select(l => new
            {
                l.Id, l.UserId, Username = l.User.Username,
                l.Game.IgdbId, GameName = l.Game.Name, l.Game.CoverUrl,
                l.Status, l.Rating, l.HoursPlayed,
                ReviewBody = l.Review != null ? l.Review.Body : null,
                ContainsSpoilers = l.Review != null && l.Review.ContainsSpoilers,
                l.CreatedAt,
                LikeCount = _db.LogLikes.Count(x => x.GameLogId == l.Id),
                LikedByMe = _db.LogLikes.Any(x => x.GameLogId == l.Id && x.UserId == me),
            })
            .FirstOrDefaultAsync(ct);

        if (detail is null) return null;

        var comments = await _db.Comments
            .Where(c => c.GameLogId == logId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.UserId, c.User.Username, c.Body, c.CreatedAt))
            .ToListAsync(ct);

        return new LogDetailDto(
            detail.Id, detail.UserId, detail.Username,
            detail.IgdbId, detail.GameName, detail.CoverUrl,
            detail.Status, detail.Rating, detail.HoursPlayed,
            detail.ReviewBody, detail.ContainsSpoilers, detail.CreatedAt,
            detail.LikeCount, detail.LikedByMe, comments);
    }

    public async Task<CommentDto> AddCommentAsync(Guid logId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        if (!await _db.GameLogs.AnyAsync(l => l.Id == logId, ct))
            throw AppException.NotFound("Log");

        var comment = new Comment { UserId = userId, GameLogId = logId, Body = request.Body.Trim() };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        var username = await _db.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstAsync(ct);
        return new CommentDto(comment.Id, userId, username, comment.Body, comment.CreatedAt);
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var comment = await _db.Comments
            .Include(c => c.GameLog)
            .FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null) return false;

        // The comment's author or the log's owner may delete it.
        if (comment.UserId != userId && comment.GameLog.UserId != userId)
            throw AppException.Unauthorized("You can't delete this comment.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static void ValidateRating(int? rating)
    {
        if (rating is < 1 or > 10)
            throw new AppException("Rating must be between 1 and 10.");
    }

    private static GameLogDto ToDto(GameLog l, Game g) => new(
        l.Id, g.IgdbId, g.Name, g.CoverUrl,
        l.Status, l.Rating, l.HoursPlayed, l.StartedAt, l.FinishedAt, l.CreatedAt,
        l.Review?.Body, l.Review?.ContainsSpoilers ?? false);
}
