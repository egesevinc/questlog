namespace Questlog.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserSummaryDto>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>Public profile (username, bio, avatar) for a user, or null if not found.</summary>
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Update the current user's own bio/avatar.</summary>
    Task<UserProfileDto> UpdateOwnProfileAsync(UpdateProfileRequest request, CancellationToken ct = default);
}
