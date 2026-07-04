using Questlog.Domain.Common;

namespace Questlog.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation
    public ICollection<GameLog> Logs { get; set; } = new List<GameLog>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<GameList> Lists { get; set; } = new List<GameList>();
}
