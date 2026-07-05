using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Social;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IFollowService _follows;
    public FeedController(IFollowService follows) => _follows = follows;

    /// <summary>Recent activity from the users the current user follows.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeedItemDto>>> Get([FromQuery] int limit = 30, CancellationToken ct = default)
        => Ok(await _follows.GetFeedAsync(limit, ct));
}
