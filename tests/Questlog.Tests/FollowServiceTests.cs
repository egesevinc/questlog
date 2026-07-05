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

public class FollowServiceTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static (FollowService svc, QuestlogDbContext db, User me, User other) Arrange()
    {
        var db = NewDb();
        var me = new User { Username = "me", Email = "me@x.com", PasswordHash = "h" };
        var other = new User { Username = "other", Email = "other@x.com", PasswordHash = "h" };
        db.Users.AddRange(me, other);
        db.SaveChanges();

        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(me.Id);

        return (new FollowService(db, current), db, me, other);
    }

    [Fact]
    public async Task FollowAsync_creates_a_follow()
    {
        var (svc, db, me, other) = Arrange();

        await svc.FollowAsync(other.Id);

        (await db.Follows.AnyAsync(f => f.FollowerId == me.Id && f.FolloweeId == other.Id))
            .Should().BeTrue();
    }

    [Fact]
    public async Task FollowAsync_rejects_self_follow()
    {
        var (svc, _, me, _) = Arrange();

        var act = () => svc.FollowAsync(me.Id);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task FollowAsync_is_idempotent()
    {
        var (svc, db, me, other) = Arrange();

        await svc.FollowAsync(other.Id);
        await svc.FollowAsync(other.Id);

        (await db.Follows.CountAsync(f => f.FollowerId == me.Id && f.FolloweeId == other.Id))
            .Should().Be(1);
    }

    [Fact]
    public async Task UnfollowAsync_removes_the_follow()
    {
        var (svc, db, _, other) = Arrange();
        await svc.FollowAsync(other.Id);

        await svc.UnfollowAsync(other.Id);

        (await db.Follows.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task GetFeedAsync_returns_only_followed_users_logs_newest_first()
    {
        var (svc, db, _, other) = Arrange();

        // A game and two logs by "other": an older and a newer one.
        var game = new Game { IgdbId = 1, Name = "A" };
        var game2 = new Game { IgdbId = 2, Name = "B" };
        db.Games.AddRange(game, game2);
        db.GameLogs.AddRange(
            new GameLog { UserId = other.Id, GameId = game.Id, Status = LogStatus.Completed, CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new GameLog { UserId = other.Id, GameId = game2.Id, Status = LogStatus.Playing, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        // Before following, the feed is empty.
        (await svc.GetFeedAsync()).Should().BeEmpty();

        await svc.FollowAsync(other.Id);
        var feed = await svc.GetFeedAsync();

        feed.Should().HaveCount(2);
        feed[0].GameName.Should().Be("B"); // newest first
    }

    [Fact]
    public async Task GetFollowInfoAsync_reports_counts_and_viewer_relationship()
    {
        var (svc, _, _, other) = Arrange();
        await svc.FollowAsync(other.Id);

        var info = await svc.GetFollowInfoAsync(other.Id);

        info.FollowerCount.Should().Be(1);
        info.IsFollowedByMe.Should().BeTrue();
    }
}
