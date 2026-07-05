using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Social;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class LikeService : ILikeService
{
    private readonly QuestlogDbContext _db;
    private readonly ICurrentUser _currentUser;

    public LikeService(QuestlogDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() => _currentUser.UserId ?? throw AppException.Unauthorized();

    public async Task LikeAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();

        if (!await _db.GameLogs.AnyAsync(l => l.Id == logId, ct))
            throw AppException.NotFound("Log");

        var already = await _db.LogLikes.AnyAsync(x => x.UserId == userId && x.GameLogId == logId, ct);
        if (already) return; // idempotent

        _db.LogLikes.Add(new LogLike { UserId = userId, GameLogId = logId });
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnlikeAsync(Guid logId, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var like = await _db.LogLikes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.GameLogId == logId, ct);
        if (like is null) return; // no-op

        _db.LogLikes.Remove(like);
        await _db.SaveChangesAsync(ct);
    }
}
