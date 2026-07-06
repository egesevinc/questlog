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
