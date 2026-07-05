using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.GameLogs;
using Questlog.Application.Social;
using Questlog.Application.Users;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IGameLogService _logs;
    private readonly IUserSearchService _users;
    private readonly IFollowService _follows;

    public ProfilesController(IGameLogService logs, IUserSearchService users, IFollowService follows)
    {
        _logs = logs;
        _users = users;
        _follows = follows;
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

    /// <summary>Follow counts and whether the current viewer follows this user.</summary>
    [HttpGet("{userId:guid}/follow-info")]
    public async Task<ActionResult<FollowInfoDto>> FollowInfo(Guid userId, CancellationToken ct)
        => Ok(await _follows.GetFollowInfoAsync(userId, ct));

    [Authorize]
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken ct)
    {
        await _follows.FollowAsync(userId, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{userId:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken ct)
    {
        await _follows.UnfollowAsync(userId, ct);
        return NoContent();
    }
}
