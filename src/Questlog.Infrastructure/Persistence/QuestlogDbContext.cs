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
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<LogLike> LogLikes => Set<LogLike>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();

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

        // --- Follows: one row per (follower, followee) ---
        b.Entity<Follow>(e =>
        {
            e.HasIndex(f => new { f.FollowerId, f.FolloweeId }).IsUnique();

            e.HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Likes: one row per (user, log) ---
        b.Entity<LogLike>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.GameLogId }).IsUnique();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.GameLog)
                .WithMany()
                .HasForeignKey(x => x.GameLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Comments ---
        b.Entity<Comment>(e =>
        {
            e.Property(c => c.Body).IsRequired().HasMaxLength(2000);
            e.HasIndex(c => c.GameLogId);

            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.GameLog)
                .WithMany()
                .HasForeignKey(c => c.GameLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Notifications ---
        b.Entity<Notification>(e =>
        {
            // Fast "my unread notifications, newest first" lookup.
            e.HasIndex(n => new { n.RecipientId, n.IsRead });

            e.HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.GameLog)
                .WithMany()
                .HasForeignKey(n => n.GameLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
