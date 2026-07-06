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

public class LikeServiceTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static (LikeService svc, QuestlogDbContext db, Guid meId, GameLog log) Arrange()
    {
        var db = NewDb();
        var me = new User { Username = "me", Email = "me@x.com", PasswordHash = "h" };
        var author = new User { Username = "author", Email = "a@x.com", PasswordHash = "h" };
        var game = new Game { IgdbId = 1, Name = "A" };
        var log = new GameLog { User = author, Game = game, Status = LogStatus.Completed };
        db.AddRange(me, author, game, log);
        db.SaveChanges();

        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(me.Id);
        return (new LikeService(db, current), db, me.Id, log);
    }

    [Fact]
    public async Task LikeAsync_creates_a_like_and_is_idempotent()
    {
        var (svc, db, meId, log) = Arrange();

        await svc.LikeAsync(log.Id);
        await svc.LikeAsync(log.Id);

        (await db.LogLikes.CountAsync(x => x.UserId == meId && x.GameLogId == log.Id))
            .Should().Be(1);
    }

    [Fact]
    public async Task UnlikeAsync_removes_the_like()
    {
        var (svc, db, _, log) = Arrange();
        await svc.LikeAsync(log.Id);

        await svc.UnlikeAsync(log.Id);

        (await db.LogLikes.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task LikeAsync_notifies_the_logs_owner()
    {
        var (svc, db, meId, log) = Arrange(); // log is authored by someone else

        await svc.LikeAsync(log.Id);

        var note = await db.Notifications.SingleAsync();
        note.RecipientId.Should().Be(log.UserId);
        note.ActorId.Should().Be(meId);
        note.Type.Should().Be(NotificationType.Like);
        note.GameLogId.Should().Be(log.Id);
    }

    [Fact]
    public async Task LikeAsync_does_not_notify_when_liking_your_own_log()
    {
        var db = NewDb();
        var me = new User { Username = "me", Email = "me@x.com", PasswordHash = "h" };
        var game = new Game { IgdbId = 1, Name = "A" };
        var myLog = new GameLog { User = me, Game = game, Status = LogStatus.Completed };
        db.AddRange(me, game, myLog);
        db.SaveChanges();

        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(me.Id);
        var svc = new LikeService(db, current);

        await svc.LikeAsync(myLog.Id);

        (await db.Notifications.AnyAsync()).Should().BeFalse();
    }
}
