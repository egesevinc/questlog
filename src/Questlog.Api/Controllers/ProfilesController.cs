using Microsoft.AspNetCore.Mvc;
using Questlog.Application.GameLogs;
using Questlog.Application.Users;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IGameLogService _logs;
    private readonly IUserSearchService _users;

    public ProfilesController(IGameLogService logs, IUserSearchService users)
    {
        _logs = logs;
        _users = users;
    }

    /// <summary>Find users by username.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Search([FromQuery] string q, CancellationToken ct)
        => Ok(await _users.SearchAsync(q, ct));

    /// <summary>Aggregated taste stats for a user (counts, avg rating, top genres).</summary>
    [HttpGet("{userId:guid}/stats")]
    public async Task<ActionResult<ProfileStatsDto>> Stats(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetProfileStatsAsync(userId, ct));

    /// <summary>A user's public log grid.</summary>
    [HttpGet("{userId:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<GameLogDto>>> Logs(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetForUserAsync(userId, ct));
}
