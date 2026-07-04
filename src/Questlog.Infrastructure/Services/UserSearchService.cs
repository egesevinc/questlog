using Microsoft.EntityFrameworkCore;
using Questlog.Application.Users;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Infrastructure.Services;

public class UserSearchService : IUserSearchService
{
    private readonly QuestlogDbContext _db;

    public UserSearchService(QuestlogDbContext db) => _db = db;

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
}
