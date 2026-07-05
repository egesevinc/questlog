using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Common;
using Questlog.Application.GameLogs;
using Questlog.Application.Social;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class GameLogsController : ControllerBase
{
    private readonly IGameLogService _logs;
    private readonly ILikeService _likes;
    private readonly ICurrentUser _currentUser;

    public GameLogsController(IGameLogService logs, ILikeService likes, ICurrentUser currentUser)
    {
        _logs = logs;
        _likes = likes;
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

    /// <summary>The current user's log for a specific game, or 404 if not logged.</summary>
    [HttpGet("game/{igdbId:long}")]
    public async Task<ActionResult<GameLogDto>> GetMineForGame(long igdbId, CancellationToken ct)
    {
        var log = await _logs.GetMineForGameAsync(igdbId, ct);
        return log is null ? NotFound() : Ok(log);
    }

    /// <summary>Like a log (and its review).</summary>
    [HttpPost("{logId:guid}/like")]
    public async Task<IActionResult> Like(Guid logId, CancellationToken ct)
    {
        await _likes.LikeAsync(logId, ct);
        return NoContent();
    }

    [HttpDelete("{logId:guid}/like")]
    public async Task<IActionResult> Unlike(Guid logId, CancellationToken ct)
    {
        await _likes.UnlikeAsync(logId, ct);
        return NoContent();
    }

    /// <summary>A single public log/review with its author, likes, and comments.</summary>
    [AllowAnonymous]
    [HttpGet("{logId:guid}")]
    public async Task<ActionResult<LogDetailDto>> GetDetail(Guid logId, CancellationToken ct)
    {
        var detail = await _logs.GetLogDetailAsync(logId, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>Add a comment to a log.</summary>
    [HttpPost("{logId:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid logId, CreateCommentRequest request, CancellationToken ct)
        => Ok(await _logs.AddCommentAsync(logId, request, ct));

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken ct)
        => await _logs.DeleteCommentAsync(commentId, ct) ? NoContent() : NotFound();
}
