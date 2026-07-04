namespace Questlog.Domain.Common;

/// <summary>
/// Base type for persisted entities. Centralises the surrogate key and audit
/// timestamps so every table gets consistent created/updated tracking.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
