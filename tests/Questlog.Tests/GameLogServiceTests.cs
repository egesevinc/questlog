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

        db.Users.Add(new User { Id = userId, Username = "tester", Email = "t@x.com", PasswordHash = "h" });
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
            new GameLog { UserId = userId, GameId = g2.Id, Status = LogStatus.Completed, Rating = 10, HoursPlayed = 30 },
            new GameLog { UserId = userId, GameId = g3.Id, Status = LogStatus.Playing, Rating = 8, HoursPlayed = 12 });
        await db.SaveChangesAsync();

        var stats = await svc.GetProfileStatsAsync(userId);

        stats.TotalLogged.Should().Be(2);
        stats.Completed.Should().Be(1);
        stats.Playing.Should().Be(1);
        stats.AverageRating.Should().Be(9);
        stats.TotalHoursPlayed.Should().Be(42);
        stats.RatingDistribution.Should().HaveCount(10);
        stats.RatingDistribution[9].Should().Be(1); // one 10/10
        stats.RatingDistribution[7].Should().Be(1); // one 8/10
    }

    [Fact]
    public async Task GetYearInReview_only_counts_logs_from_that_year()
    {
        var (svc, db, userId, _) = Arrange();
        var g2 = new Game { IgdbId = 2, Name = "Hades" };
        var g3 = new Game { IgdbId = 3, Name = "Celeste" };
        db.Games.AddRange(g2, g3);
        db.GameLogs.AddRange(
            new GameLog
            {
                UserId = userId, GameId = g2.Id, Status = LogStatus.Completed, Rating = 9,
                HoursPlayed = 20, CreatedAt = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new GameLog
            {
                UserId = userId, GameId = g3.Id, Status = LogStatus.Completed, Rating = 7,
                HoursPlayed = 5, CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
            });
        await db.SaveChangesAsync();

        var review = await svc.GetYearInReviewAsync(userId, 2025);

        review.Year.Should().Be(2025);
        review.TotalLogged.Should().Be(1);
        review.TotalHoursPlayed.Should().Be(20);
        review.AverageRating.Should().Be(9);
        review.TopRated.Should().ContainSingle(g => g.GameName == "Hades");
    }

    [Fact]
    public async Task GetGameCommunity_aggregates_ratings_and_returns_reviews()
    {
        var (svc, db, _, igdbId) = Arrange();
        var game = await db.Games.FirstAsync(g => g.IgdbId == igdbId);

        var a = new User { Username = "a", Email = "a@x.com", PasswordHash = "h" };
        var b = new User { Username = "b", Email = "b@x.com", PasswordHash = "h" };
        var c = new User { Username = "c", Email = "c@x.com", PasswordHash = "h" };
        db.Users.AddRange(a, b, c);
        db.GameLogs.AddRange(
            new GameLog
            {
                User = a, GameId = game.Id, Status = LogStatus.Completed, Rating = 10,
                Review = new Review { User = a, Body = "Masterpiece." }
            },
            new GameLog { User = b, GameId = game.Id, Status = LogStatus.Completed, Rating = 8 },
            new GameLog { User = c, GameId = game.Id, Status = LogStatus.Playing, Rating = null });
        await db.SaveChangesAsync();

        var community = await svc.GetGameCommunityAsync(igdbId);

        community.LogCount.Should().Be(3);
        community.RatingCount.Should().Be(2);
        community.AverageRating.Should().Be(9); // (10 + 8) / 2
        community.Reviews.Should().HaveCount(1);
        community.Reviews[0].Username.Should().Be("a");
    }

    [Fact]
    public async Task GetTrendingGames_orders_by_log_count()
    {
        var (svc, db, _, _) = Arrange();
        var popular = new Game { IgdbId = 100, Name = "Popular" };
        var niche = new Game { IgdbId = 200, Name = "Niche" };
        db.Games.AddRange(popular, niche);
        // Popular: 3 logs (with two ratings 8 and 10). Niche: 1 log.
        db.GameLogs.AddRange(
            new GameLog { UserId = Guid.NewGuid(), GameId = popular.Id, Status = LogStatus.Completed, Rating = 8 },
            new GameLog { UserId = Guid.NewGuid(), GameId = popular.Id, Status = LogStatus.Completed, Rating = 10 },
            new GameLog { UserId = Guid.NewGuid(), GameId = popular.Id, Status = LogStatus.Playing },
            new GameLog { UserId = Guid.NewGuid(), GameId = niche.Id, Status = LogStatus.Completed });
        await db.SaveChangesAsync();

        var trending = await svc.GetTrendingGamesAsync();

        trending[0].Name.Should().Be("Popular");
        trending[0].LogCount.Should().Be(3);
        trending[0].AverageRating.Should().Be(9); // (8 + 10) / 2
    }

    [Fact]
    public async Task GetGameCommunity_returns_empty_for_an_unlogged_game()
    {
        var (svc, _, _, igdbId) = Arrange();

        var community = await svc.GetGameCommunityAsync(igdbId);

        community.LogCount.Should().Be(0);
        community.AverageRating.Should().BeNull();
        community.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task AddComment_appears_in_the_log_detail()
    {
        var (svc, _, _, igdbId) = Arrange();
        var log = await svc.CreateAsync(new CreateGameLogRequest(igdbId, LogStatus.Completed, 9, null, null, null));

        await svc.AddCommentAsync(log.Id, new CreateCommentRequest("Great pick!"));

        var detail = await svc.GetLogDetailAsync(log.Id);
        detail.Should().NotBeNull();
        detail!.Comments.Should().HaveCount(1);
        detail.Comments[0].Body.Should().Be("Great pick!");
    }

    [Fact]
    public async Task DeleteComment_by_a_stranger_is_rejected()
    {
        var (svc, db, _, igdbId) = Arrange();
        var log = await svc.CreateAsync(new CreateGameLogRequest(igdbId, LogStatus.Completed, 9, null, null, null));
        var comment = await svc.AddCommentAsync(log.Id, new CreateCommentRequest("Mine."));

        // A different user tries to delete it.
        var stranger = Substitute.For<ICurrentUser>();
        stranger.UserId.Returns(Guid.NewGuid());
        var strangerSvc = new GameLogService(db, Substitute.For<IGameService>(), stranger);

        var act = () => strangerSvc.DeleteCommentAsync(comment.Id);

        await act.Should().ThrowAsync<AppException>().Where(e => e.StatusCode == 401);
    }
}
