using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Questlog.Application.Common;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;
using Questlog.Infrastructure.Services;

namespace Questlog.Tests;

public class NotificationServiceTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task UnreadCount_and_MarkAllRead_work()
    {
        var db = NewDb();
        var me = new User { Username = "me", Email = "me@x.com", PasswordHash = "h" };
        var actor = new User { Username = "actor", Email = "a@x.com", PasswordHash = "h" };
        db.AddRange(me, actor);
        db.Notifications.AddRange(
            new Notification { Recipient = me, Actor = actor, Type = NotificationType.Follow },
            new Notification { Recipient = me, Actor = actor, Type = NotificationType.Follow, IsRead = true });
        await db.SaveChangesAsync();

        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(me.Id);
        var svc = new NotificationService(db, current);

        (await svc.GetUnreadCountAsync()).Should().Be(1);

        await svc.MarkAllReadAsync();

        (await svc.GetUnreadCountAsync()).Should().Be(0);
        (await svc.GetMineAsync()).Should().HaveCount(2);
    }
}
