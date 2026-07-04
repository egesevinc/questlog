using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Games;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _games;
    public GamesController(IGameService games) => _games = games;

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
}
