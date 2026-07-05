using System.ComponentModel.DataAnnotations;

namespace Questlog.Application.Users;

public record UserSummaryDto(Guid Id, string Username, string? AvatarUrl);

public record UserProfileDto(Guid Id, string Username, string? Bio, string? AvatarUrl);

public record UpdateProfileRequest(
    [StringLength(300)] string? Bio,
    [StringLength(500), Url] string? AvatarUrl);
