using Microsoft.EntityFrameworkCore;
using Questlog.Application.Common;
using Questlog.Application.Users;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly QuestlogDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UserService(QuestlogDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<UserSummaryDto>();

        var term = query.Trim().ToLower();
        return await _db.Users
            .Where(u => u.Username.ToLower().Contains(term))
            .OrderBy(u => u.Username)
            .Take(20)
            .Select(u => new UserSummaryDto(u.Id, u.Username, u.AvatarUrl))
            .ToListAsync(ct);
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileDto(u.Id, u.Username, u.Bio, u.AvatarUrl))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserProfileDto> UpdateOwnProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw AppException.Unauthorized();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw AppException.NotFound("User");

        user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return new UserProfileDto(user.Id, user.Username, user.Bio, user.AvatarUrl);
    }
}
