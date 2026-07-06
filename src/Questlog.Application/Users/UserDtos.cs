using System.ComponentModel.DataAnnotations;

namespace Questlog.Application.Users;

public record UserSummaryDto(Guid Id, string Username, string? AvatarUrl);

public record UserProfileDto(Guid Id, string Username, string? Bio, string? AvatarUrl);

public record UpdateProfileRequest(
    [StringLength(300)] string? Bio,
    [StringLength(500), Url] string? AvatarUrl);

public record FavoriteGameDto(long IgdbId, string GameName, string? CoverUrl);

/// <summary>Up to four game IGDB ids, in showcase order.</summary>
public record SetFavoritesRequest([MaxLength(4)] IReadOnlyList<long> IgdbIds);
