using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.Common;
using Questlog.Application.GameLists;

namespace Questlog.Api.Controllers;

[ApiController]
[Route("api/lists")]
public class GameListsController : ControllerBase
{
    private readonly IGameListService _lists;
    private readonly ICurrentUser _currentUser;

    public GameListsController(IGameListService lists, ICurrentUser currentUser)
    {
        _lists = lists;
        _currentUser = currentUser;
    }

    /// <summary>The current user's lists.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<GameListSummaryDto>>> GetMine(CancellationToken ct)
    {
        var userId = _currentUser.UserId!.Value;
        return Ok(await _lists.GetForUserAsync(userId, ct));
    }

    /// <summary>Recent public lists across all users (for discovery).</summary>
    [HttpGet("discover")]
    public async Task<ActionResult<IReadOnlyList<PublicListDto>>> Discover([FromQuery] int limit = 12, CancellationToken ct = default)
        => Ok(await _lists.GetPublicListsAsync(limit, ct));

    /// <summary>A single list, with its items.</summary>
    [HttpGet("{listId:guid}")]
    public async Task<ActionResult<GameListDto>> Get(Guid listId, CancellationToken ct)
    {
        var list = await _lists.GetAsync(listId, ct);
        return list is null ? NotFound() : Ok(list);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GameListDto>> Create(CreateGameListRequest request, CancellationToken ct)
    {
        var created = await _lists.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { listId = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{listId:guid}")]
    public async Task<ActionResult<GameListDto>> Update(Guid listId, UpdateGameListRequest request, CancellationToken ct)
    {
        var updated = await _lists.UpdateAsync(listId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize]
    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> Delete(Guid listId, CancellationToken ct)
        => await _lists.DeleteAsync(listId, ct) ? NoContent() : NotFound();

    [Authorize]
    [HttpPost("{listId:guid}/items")]
    public async Task<ActionResult<GameListDto>> AddItem(Guid listId, AddGameListItemRequest request, CancellationToken ct)
    {
        var updated = await _lists.AddItemAsync(listId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize]
    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid listId, Guid itemId, CancellationToken ct)
        => await _lists.RemoveItemAsync(listId, itemId, ct) ? NoContent() : NotFound();

    [Authorize]
    [HttpPut("{listId:guid}/items/order")]
    public async Task<ActionResult<GameListDto>> Reorder(Guid listId, ReorderGameListItemsRequest request, CancellationToken ct)
    {
        var updated = await _lists.ReorderItemsAsync(listId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
}
