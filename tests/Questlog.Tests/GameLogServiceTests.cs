using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Questlog.Application.Common;
using Questlog.Application.GameLogs;
using Questlog.Application.Games;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;
using Questlog.Infrastructure.Persistence;
using Questlog.Infrastructure.Services;

namespace Questlog.Tests;

public class GameLogServiceTests
{
    private static QuestlogDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new QuestlogDbContext(options);
    }

    private static (GameLogService svc, QuestlogDbContext db, Guid userId, long igdbId)
        Arrange(IGameService? gameService = null)
    {
        var db = NewDb();
        var userId = Guid.NewGuid();
        const long igdbId = 1942;

        var game = new Game { IgdbId = igdbId, Name = "The Witcher 3" };
        db.Games.Add(game);
        db.SaveChanges();

        var games = gameService ?? Substitute.For<IGameService>();
        games.GetByIgdbIdAsync(igdbId, Arg.Any<CancellationToken>())
            .Returns(new GameDetailDto(game.Id, igdbId, game.Name, null, null, null,
                Array.Empty<string>(), Array.Empty<string>()));

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);

        return (new GameLogService(db, games, currentUser), db, userId, igdbId);
    }

    [Fact]
    public async Task CreateAsync_logs_a_game_for_the_user()
    {
        var (svc, db, userId, igdbId) = Arrange();

        var dto = await svc.CreateAsync(new CreateGameLogRequest(
            igdbId, LogStatus.Completed, Rating: 9, HoursPlayed: 120, null, null));

        dto.Status.Should().Be(LogStatus.Completed);
        dto.Rating.Should().Be(9);
        (await db.GameLogs.CountAsync(l => l.UserId == userId)).Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_rejects_a_duplicate_log_for_the_same_game()
    {
        var (svc, _, _, igdbId) = Arrange();
        await svc.CreateAsync(new CreateGameLogRequest(igdbId, LogStatus.Playing, null, null, null, null));

        var act = () => svc.CreateAsync(new CreateGameLogRequest(igdbId, LogStatus.Playing, null, null, null, null));

        await act.Should().ThrowAsync<AppException>().Where(e => e.StatusCode == 409);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task CreateAsync_rejects_out_of_range_ratings(int rating)
    {
        var (svc, _, _, igdbId) = Arrange();

        var act = () => svc.CreateAsync(new CreateGameLogRequest(igdbId, LogStatus.Completed, rating, null, null, null));

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task GetProfileStats_aggregates_counts_and_average_rating()
    {
        var (svc, db, userId, _) = Arrange();
        var g2 = new Game { IgdbId = 2, Name = "Hades" };
        var g3 = new Game { IgdbId = 3, Name = "Celeste" };
        db.Games.AddRange(g2, g3);
        db.GameLogs.AddRange(
            new GameLog { UserId = userId, GameId = g2.Id, Status = LogStatus.Completed, Rating = 10 },
            new GameLog { UserId = userId, GameId = g3.Id, Status = LogStatus.Playing, Rating = 8 });
        await db.SaveChangesAsync();

        var stats = await svc.GetProfileStatsAsync(userId);

        stats.TotalLogged.Should().Be(2);
        stats.Completed.Should().Be(1);
        stats.Playing.Should().Be(1);
        stats.AverageRating.Should().Be(9);
    }
}
