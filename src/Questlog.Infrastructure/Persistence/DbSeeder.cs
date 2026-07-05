using Microsoft.EntityFrameworkCore;
using Questlog.Application.Auth;
using Questlog.Domain.Entities;
using Questlog.Domain.Enums;

namespace Questlog.Infrastructure.Persistence;

/// <summary>
/// Seeds a small, coherent demo dataset so a fresh database isn't empty — useful
/// for local demos and a deployed showcase. Idempotent: it does nothing if any
/// users already exist, so it never touches real data. Only wired up in
/// Development (see Program.cs).
///
/// Game metadata (ids + cover image ids) are real IGDB values so covers render
/// without needing a live IGDB call at startup.
/// </summary>
public static class DbSeeder
{
    // Demo accounts — documented so they can be logged into during a demo.
    public const string DemoPassword = "questlog-demo";

    private static string Cover(string imageId) =>
        $"https://images.igdb.com/igdb/image/upload/t_cover_big/{imageId}.jpg";

    public static async Task SeedAsync(QuestlogDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return; // Already has data — never overwrite.

        // --- Genres ---
        var adventure = new Genre { IgdbId = 31, Name = "Adventure" };
        var rpg = new Genre { IgdbId = 12, Name = "Role-playing (RPG)" };
        var puzzle = new Genre { IgdbId = 9, Name = "Puzzle" };

        // --- Games (real IGDB ids + cover image ids) ---
        var loz = new Game
        {
            IgdbId = 1022, Name = "The Legend of Zelda", CoverUrl = Cover("co1uii"),
            ReleaseDate = new DateTimeOffset(1986, 2, 21, 0, 0, 0, TimeSpan.Zero),
            Summary = "The one that started it all — explore Hyrule, find the Triforce.",
            Genres = { adventure, rpg }
        };
        var zelda2 = new Game
        {
            IgdbId = 1025, Name = "Zelda II: The Adventure of Link", CoverUrl = Cover("co1uje"),
            ReleaseDate = new DateTimeOffset(1987, 1, 14, 0, 0, 0, TimeSpan.Zero),
            Summary = "A side-scrolling, RPG-flavoured detour for the series.",
            Genres = { adventure, rpg }
        };
        var alttp = new Game
        {
            IgdbId = 1026, Name = "The Legend of Zelda: A Link to the Past", CoverUrl = Cover("co3vzn"),
            ReleaseDate = new DateTimeOffset(1991, 11, 21, 0, 0, 0, TimeSpan.Zero),
            Summary = "The template every top-down Zelda since has been measured against.",
            Genres = { adventure, rpg, puzzle }
        };
        var oot = new Game
        {
            IgdbId = 1029, Name = "The Legend of Zelda: Ocarina of Time", CoverUrl = Cover("co3nnx"),
            ReleaseDate = new DateTimeOffset(1998, 11, 21, 0, 0, 0, TimeSpan.Zero),
            Summary = "The leap to 3D that rewrote what an action-adventure could be.",
            Genres = { adventure, rpg }
        };
        var ages = new Game
        {
            IgdbId = 1041, Name = "The Legend of Zelda: Oracle of Ages", CoverUrl = Cover("co2tw1"),
            ReleaseDate = new DateTimeOffset(2001, 2, 27, 0, 0, 0, TimeSpan.Zero),
            Summary = "The puzzle-leaning half of the Oracle duology.",
            Genres = { adventure, puzzle }
        };
        var seasons = new Game
        {
            IgdbId = 1032, Name = "The Legend of Zelda: Oracle of Seasons", CoverUrl = Cover("co2tw0"),
            ReleaseDate = new DateTimeOffset(2001, 2, 27, 0, 0, 0, TimeSpan.Zero),
            Summary = "The action-leaning half of the Oracle duology.",
            Genres = { adventure }
        };

        db.Games.AddRange(loz, zelda2, alttp, oot, ages, seasons);

        // --- Demo users ---
        var link = new User
        {
            Username = "link",
            Email = "link@questlog.demo",
            PasswordHash = hasher.Hash(DemoPassword),
            Bio = "Perpetually rescuing Hyrule. Adventure and puzzle games mostly."
        };
        var zelda = new User
        {
            Username = "zelda",
            Email = "zelda@questlog.demo",
            PasswordHash = hasher.Hash(DemoPassword),
            Bio = "Strategy and RPGs. I finish what I start."
        };
        db.Users.AddRange(link, zelda);

        // --- Logs (with reviews) ---
        db.GameLogs.AddRange(
            new GameLog
            {
                User = link, Game = oot, Status = LogStatus.Completed, Rating = 10, HoursPlayed = 45,
                Review = new Review
                {
                    User = link,
                    Body = "Still the high-water mark. The Water Temple aged badly but everything else is timeless.",
                }
            },
            new GameLog
            {
                User = link, Game = alttp, Status = LogStatus.Completed, Rating = 9, HoursPlayed = 20,
                Review = new Review { User = link, Body = "The dark world twist is one of the great mid-game reveals." }
            },
            new GameLog { User = link, Game = loz, Status = LogStatus.Completed, Rating = 8, HoursPlayed = 15 },
            new GameLog { User = link, Game = ages, Status = LogStatus.Playing, Rating = null, HoursPlayed = 6 },
            new GameLog
            {
                User = zelda, Game = alttp, Status = LogStatus.Completed, Rating = 10, HoursPlayed = 24,
                Review = new Review { User = zelda, Body = "My favourite in the series. Tight design, no wasted screens." }
            },
            new GameLog { User = zelda, Game = seasons, Status = LogStatus.Completed, Rating = 8, HoursPlayed = 14 },
            new GameLog { User = zelda, Game = zelda2, Status = LogStatus.Abandoned, Rating = 5, HoursPlayed = 4 });

        // --- A curated list ---
        var top = new GameList
        {
            User = link,
            Title = "Zelda, ranked",
            Description = "A running personal ranking of the classics.",
            IsPublic = true,
            Items =
            {
                new GameListItem { Game = oot, Order = 0, Note = "Peak." },
                new GameListItem { Game = alttp, Order = 1 },
                new GameListItem { Game = loz, Order = 2 }
            }
        };
        db.GameLists.Add(top);

        // --- Follows: the two demo users follow each other, so each feed has content ---
        db.Follows.AddRange(
            new Follow { Follower = link, Followee = zelda },
            new Follow { Follower = zelda, Followee = link });

        await db.SaveChangesAsync(ct);
    }
}
