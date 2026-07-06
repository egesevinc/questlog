using Microsoft.AspNetCore.Mvc;
using Questlog.Application.GameLogs;
using Questlog.Application.Games;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _games;
    private readonly IGameLogService _logs;

    public GamesController(IGameService games, IGameLogService logs)
    {
        _games = games;
        _logs = logs;
    }

    /// <summary>Search games by name (proxied + cached from IGDB).</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<GameSummaryDto>>> Search(
        [FromQuery] string q, CancellationToken ct)
        => Ok(await _games.SearchAsync(q, ct));

    /// <summary>Get a single game's details by its IGDB id.</summary>
    [HttpGet("{igdbId:long}")]
    public async Task<ActionResult<GameDetailDto>> Get(long igdbId, CancellationToken ct)
    {
        var game = await _games.GetByIgdbIdAsync(igdbId, ct);
        return game is null ? NotFound() : Ok(game);
    }

    /// <summary>The community view of a game: average rating, counts, recent reviews.</summary>
    [HttpGet("{igdbId:long}/community")]
    public async Task<ActionResult<GameCommunityDto>> Community(long igdbId, CancellationToken ct)
        => Ok(await _logs.GetGameCommunityAsync(igdbId, ct));

    /// <summary>The most-logged games across all users (for discovery).</summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<TrendingGameDto>>> Trending([FromQuery] int limit = 12, CancellationToken ct = default)
        => Ok(await _logs.GetTrendingGamesAsync(limit, ct));
}
