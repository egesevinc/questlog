namespace Questlog.Application.Users;

public interface IUserSearchService
{
    Task<IReadOnlyList<UserSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
}
