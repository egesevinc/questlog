namespace Questlog.Application.Social;

public interface ILikeService
{
    /// <summary>Like a log. Idempotent.</summary>
    Task LikeAsync(Guid logId, CancellationToken ct = default);

    /// <summary>Remove a like. No-op if not liked.</summary>
    Task UnlikeAsync(Guid logId, CancellationToken ct = default);
}
