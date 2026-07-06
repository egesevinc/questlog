using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Questlog.Application.Common;
using Questlog.Application.Games;
using Questlog.Application.Users;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;
using Questlog.Infrastructure.Services;

namespace Questlog.Tests;

public class FavoriteServiceTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static (FavoriteService svc, QuestlogDbContext db, Guid userId) Arrange()
    {
        var db = NewDb();
        var userId = Guid.NewGuid();
        db.Users.Add(new User { Id = userId, Username = "me", Email = "me@x.com", PasswordHash = "h" });
        // Games 1..6 exist locally.
        for (long i = 1; i <= 6; i++)
            db.Games.Add(new Game { IgdbId = i, Name = $"Game {i}" });
        db.SaveChanges();

        var games = Substitute.For<IGameService>();
        games.GetByIgdbIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(ci => new GameDetailDto(Guid.NewGuid(), ci.Arg<long>(), "x", null, null, null,
                Array.Empty<string>(), Array.Empty<string>()));

        var current = Substitute.For<ICurrentUser>();
        current.UserId.Returns(userId);

        return (new FavoriteService(db, games, current), db, userId);
    }

    [Fact]
    public async Task SetOwnFavorites_saves_in_order_and_get_returns_them()
    {
        var (svc, _, _) = Arrange();

        var result = await svc.SetOwnFavoritesAsync(new SetFavoritesRequest(new long[] { 3, 1, 2 }));

        result.Select(f => f.IgdbId).Should().ContainInOrder(3, 1, 2);
    }

    [Fact]
    public async Task SetOwnFavorites_caps_at_four_and_dedupes()
    {
        var (svc, _, _) = Arrange();

        var result = await svc.SetOwnFavoritesAsync(
            new SetFavoritesRequest(new long[] { 1, 1, 2, 3, 4, 5 }));

        result.Select(f => f.IgdbId).Should().Equal(1, 2, 3, 4); // deduped + capped
    }

    [Fact]
    public async Task SetOwnFavorites_replaces_the_previous_set()
    {
        var (svc, db, userId) = Arrange();
        await svc.SetOwnFavoritesAsync(new SetFavoritesRequest(new long[] { 1, 2 }));

        await svc.SetOwnFavoritesAsync(new SetFavoritesRequest(new long[] { 5 }));

        var favorites = await svc.GetFavoritesAsync(userId);
        favorites.Select(f => f.IgdbId).Should().Equal(5);
        (await db.FavoriteGames.CountAsync(f => f.UserId == userId)).Should().Be(1);
    }
}
