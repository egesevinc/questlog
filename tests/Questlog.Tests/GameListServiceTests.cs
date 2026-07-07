using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Questlog.Application.Common;
using Questlog.Application.Games;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;
using Questlog.Infrastructure.Services;

namespace Questlog.Tests;

public class GameListServiceTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GameListService Svc(QuestlogDbContext db) =>
        new(db, Substitute.For<IGameService>(), Substitute.For<ICurrentUser>());

    private static GameListService SvcAs(QuestlogDbContext db, Guid userId)
    {
        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(userId);
        return new GameListService(db, Substitute.For<IGameService>(), current);
    }

    [Fact]
    public async Task ReorderItems_applies_the_new_order()
    {
        var db = NewDb();
        var owner = new User { Username = "o", Email = "o@x.com", PasswordHash = "h" };
        var g1 = new Game { IgdbId = 1, Name = "One" };
        var g2 = new Game { IgdbId = 2, Name = "Two" };
        var g3 = new Game { IgdbId = 3, Name = "Three" };
        var i1 = new GameListItem { Game = g1, Order = 0 };
        var i2 = new GameListItem { Game = g2, Order = 1 };
        var i3 = new GameListItem { Game = g3, Order = 2 };
        var list = new GameList { User = owner, Title = "L", Items = { i1, i2, i3 } };
        db.AddRange(owner, g1, g2, g3, list);
        await db.SaveChangesAsync();

        // Reverse the order.
        var result = await SvcAs(db, owner.Id).ReorderItemsAsync(
            list.Id, new Questlog.Application.GameLists.ReorderGameListItemsRequest(
                new[] { i3.Id, i2.Id, i1.Id }));

        result!.Items.Select(i => i.GameName).Should().ContainInOrder("Three", "Two", "One");
    }

    [Fact]
    public async Task GetPublicLists_returns_only_public_lists_with_owner_and_count()
    {
        var db = NewDb();
        var owner = new User { Username = "curator", Email = "c@x.com", PasswordHash = "h" };
        var game = new Game { IgdbId = 1, Name = "A", CoverUrl = "http://cover/a.jpg" };
        db.AddRange(owner, game);
        db.GameLists.AddRange(
            new GameList
            {
                User = owner, Title = "Public one", IsPublic = true,
                Items = { new GameListItem { Game = game, Order = 0 } }
            },
            new GameList { User = owner, Title = "Private one", IsPublic = false });
        await db.SaveChangesAsync();

        var lists = await Svc(db).GetPublicListsAsync();

        lists.Should().ContainSingle();
        var only = lists[0];
        only.Title.Should().Be("Public one");
        only.OwnerUsername.Should().Be("curator");
        only.ItemCount.Should().Be(1);
        only.CoverUrls.Should().ContainSingle().Which.Should().Be("http://cover/a.jpg");
    }
}
