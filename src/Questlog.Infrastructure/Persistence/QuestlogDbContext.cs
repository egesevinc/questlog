using Microsoft.EntityFrameworkCore;
using Questlog.Domain.Entities;

namespace Questlog.Infrastructure.Persistence;

public class QuestlogDbContext : DbContext
{
    public QuestlogDbContext(DbContextOptions<QuestlogDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<GameLog> GameLogs => Set<GameLog>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<GameList> GameLists => Set<GameList>();
    public DbSet<GameListItem> GameListItems => Set<GameListItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // --- User ---
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Username).HasMaxLength(32).IsRequired();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
        });

        // --- Game + IGDB cache ---
        b.Entity<Game>(e =>
        {
            e.HasIndex(g => g.IgdbId).IsUnique();
            e.Property(g => g.Name).HasMaxLength(512).IsRequired();
        });
        b.Entity<Genre>().HasIndex(g => g.IgdbId).IsUnique();
        b.Entity<Platform>().HasIndex(p => p.IgdbId).IsUnique();

        // --- GameLog: one log per (user, game) ---
        b.Entity<GameLog>(e =>
        {
            e.HasIndex(l => new { l.UserId, l.GameId }).IsUnique();

            e.HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Game)
                .WithMany(g => g.Logs)
                .HasForeignKey(l => l.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Review)
                .WithOne(r => r.GameLog)
                .HasForeignKey<Review>(r => r.GameLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Review ---
        b.Entity<Review>(e =>
        {
            e.Property(r => r.Body).IsRequired();
            e.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Lists ---
        b.Entity<GameList>(e =>
        {
            e.Property(l => l.Title).HasMaxLength(120).IsRequired();
            e.HasOne(l => l.User)
                .WithMany(u => u.Lists)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<GameListItem>(e =>
        {
            e.HasOne(i => i.GameList)
                .WithMany(l => l.Items)
                .HasForeignKey(i => i.GameListId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Game)
                .WithMany()
                .HasForeignKey(i => i.GameId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
