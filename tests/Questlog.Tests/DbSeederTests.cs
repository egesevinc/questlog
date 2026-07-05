using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Questlog.Application.Auth;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Tests;

public class DbSeederTests
{
    private static QuestlogDbContext NewDb() =>
        new(new DbContextOptionsBuilder<QuestlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static IPasswordHasher Hasher()
    {
        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash(Arg.Any<string>()).Returns("hashed");
        return hasher;
    }

    [Fact]
    public async Task SeedAsync_populates_demo_data_into_an_empty_db()
    {
        var db = NewDb();

        await DbSeeder.SeedAsync(db, Hasher());

        (await db.Users.CountAsync()).Should().Be(2);
        (await db.Games.CountAsync()).Should().BeGreaterThan(0);
        (await db.GameLogs.CountAsync()).Should().BeGreaterThan(0);
        (await db.Reviews.CountAsync()).Should().BeGreaterThan(0);
        (await db.GameLists.CountAsync()).Should().Be(1);
        (await db.GameListItems.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SeedAsync_is_idempotent()
    {
        var db = NewDb();

        await DbSeeder.SeedAsync(db, Hasher());
        var usersAfterFirst = await db.Users.CountAsync();

        await DbSeeder.SeedAsync(db, Hasher());
        var usersAfterSecond = await db.Users.CountAsync();

        usersAfterSecond.Should().Be(usersAfterFirst);
    }
}
