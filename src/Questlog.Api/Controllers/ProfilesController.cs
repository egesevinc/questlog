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
    private readonly IUserService _users;
    private readonly IFollowService _follows;

    public ProfilesController(IGameLogService logs, IUserService users, IFollowService follows)
    {
        _logs = logs;
        _users = users;
        _follows = follows;
    }

    /// <summary>Find users by username.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Search([FromQuery] string q, CancellationToken ct)
        => Ok(await _users.SearchAsync(q, ct));

    /// <summary>Update the current user's own profile (bio, avatar).</summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _users.UpdateOwnProfileAsync(request, ct));

    /// <summary>A user's public profile (username, bio, avatar).</summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserProfileDto>> Profile(Guid userId, CancellationToken ct)
    {
        var profile = await _users.GetProfileAsync(userId, ct);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Aggregated taste stats for a user (counts, avg rating, top genres).</summary>
    [HttpGet("{userId:guid}/stats")]
    public async Task<ActionResult<ProfileStatsDto>> Stats(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetProfileStatsAsync(userId, ct));

    /// <summary>A user's public log grid.</summary>
    [HttpGet("{userId:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<GameLogDto>>> Logs(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetForUserAsync(userId, ct));

    /// <summary>A user's taste summary for a year (defaults to the current year).</summary>
    [HttpGet("{userId:guid}/year-in-review")]
    public async Task<ActionResult<YearInReviewDto>> YearInReview(Guid userId, [FromQuery] int? year, CancellationToken ct)
        => Ok(await _logs.GetYearInReviewAsync(userId, year ?? DateTimeOffset.UtcNow.Year, ct));

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
