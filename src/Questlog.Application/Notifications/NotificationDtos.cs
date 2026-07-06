using Questlog.Domain.Enums;

namespace Questlog.Application.Notifications;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    Guid ActorId,
    string ActorUsername,
    Guid? LogId,
    long? IgdbId,
    string? GameName,
    bool IsRead,
    DateTimeOffset CreatedAt);
