using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

/// <summary>
/// A directed follow relationship: <see cref="Follower"/> follows
/// <see cref="Followee"/>. Powers the activity feed (see what people you follow
/// have been playing). Enforced unique on (FollowerId, FolloweeId); self-follows
/// are rejected in the application layer.
/// </summary>
public class Follow : BaseEntity
{
    public Guid FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public Guid FolloweeId { get; set; }
    public User Followee { get; set; } = null!;
}
