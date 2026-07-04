using Microsoft.AspNetCore.Mvc;
using Questlog.Application.GameLogs;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IGameLogService _logs;
    public ProfilesController(IGameLogService logs) => _logs = logs;

    /// <summary>Aggregated taste stats for a user (counts, avg rating, top genres).</summary>
    [HttpGet("{userId:guid}/stats")]
    public async Task<ActionResult<ProfileStatsDto>> Stats(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetProfileStatsAsync(userId, ct));

    /// <summary>A user's public log grid.</summary>
    [HttpGet("{userId:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<GameLogDto>>> Logs(Guid userId, CancellationToken ct)
        => Ok(await _logs.GetForUserAsync(userId, ct));
}
