using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Common;
using Questlog.Application.GameLogs;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class GameLogsController : ControllerBase
{
    private readonly IGameLogService _logs;
    private readonly ICurrentUser _currentUser;

    public GameLogsController(IGameLogService logs, ICurrentUser currentUser)
    {
        _logs = logs;
        _currentUser = currentUser;
    }

    /// <summary>Log a game for the current user.</summary>
    [HttpPost]
    public async Task<ActionResult<GameLogDto>> Create(CreateGameLogRequest request, CancellationToken ct)
    {
        var created = await _logs.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetMine), new { }, created);
    }

    [HttpPut("{logId:guid}")]
    public async Task<ActionResult<GameLogDto>> Update(Guid logId, UpdateGameLogRequest request, CancellationToken ct)
    {
        var updated = await _logs.UpdateAsync(logId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{logId:guid}")]
    public async Task<IActionResult> Delete(Guid logId, CancellationToken ct)
        => await _logs.DeleteAsync(logId, ct) ? NoContent() : NotFound();

    /// <summary>All logs for the current user.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<GameLogDto>>> GetMine(CancellationToken ct)
    {
        var userId = _currentUser.UserId!.Value;
        return Ok(await _logs.GetForUserAsync(userId, ct));
    }
}
