using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Questlog.Application.GameLogs;
using Questlog.Application.Games;
using Questlog.Application.Users;

namespace Questlog.Api.Controllers;

/// <summary>
/// Server-rendered pages carrying Open Graph / Twitter Card metadata, so a link
/// shared on LinkedIn/Twitter/etc. gets a rich preview (title, description,
/// cover image). The SPA can't do this — crawlers don't run its JS — so these
/// endpoints render minimal HTML with the tags and redirect a human visitor on
/// to the real client route.
/// </summary>
[ApiController]
[Route("share")]
public class ShareController : ControllerBase
{
    private readonly IGameService _games;
    private readonly IGameLogService _logs;
    private readonly IUserService _users;
    private readonly string _webBase;

    public ShareController(IGameService games, IGameLogService logs, IUserService users, IConfiguration config)
    {
        _games = games;
        _logs = logs;
        _users = users;
        // Where humans should land — the web app's origin.
        _webBase = (config["Cors:Origin"] ?? "http://localhost:5173").TrimEnd('/');
    }

    [HttpGet("games/{igdbId:long}")]
    public async Task<IActionResult> Game(long igdbId, CancellationToken ct)
    {
        var game = await _games.GetByIgdbIdAsync(igdbId, ct);
        if (game is null) return NotFound();

        var desc = string.IsNullOrWhiteSpace(game.Summary)
            ? "See ratings and reviews on Questlog."
            : game.Summary;
        return Page(game.Name, desc, game.CoverUrl, $"/games/{igdbId}");
    }

    [HttpGet("logs/{logId:guid}")]
    public async Task<IActionResult> Log(Guid logId, CancellationToken ct)
    {
        var log = await _logs.GetLogDetailAsync(logId, ct);
        if (log is null) return NotFound();

        var title = $"{log.Username} on {log.GameName}";
        var desc = !string.IsNullOrWhiteSpace(log.ReviewBody)
            ? log.ReviewBody
            : log.Rating is not null
                ? $"Rated {log.Rating}/10 · {log.Status}"
                : log.Status.ToString();
        return Page(title, desc, log.CoverUrl, $"/logs/{logId}");
    }

    [HttpGet("profiles/{userId:guid}")]
    public async Task<IActionResult> Profile(Guid userId, CancellationToken ct)
    {
        var profile = await _users.GetProfileAsync(userId, ct);
        if (profile is null) return NotFound();

        var desc = string.IsNullOrWhiteSpace(profile.Bio)
            ? $"See {profile.Username}'s games, reviews, and lists on Questlog."
            : profile.Bio;
        return Page($"{profile.Username} · Questlog", desc, profile.AvatarUrl, $"/profiles/{userId}");
    }

    private ContentResult Page(string title, string description, string? imageUrl, string spaPath)
    {
        var url = _webBase + spaPath;
        // Trim overly long descriptions for a clean card.
        if (description.Length > 200) description = description[..197] + "…";

        string E(string s) => WebUtility.HtmlEncode(s);
        var image = string.IsNullOrWhiteSpace(imageUrl) ? "" :
            $"""
              <meta property="og:image" content="{E(imageUrl)}" />
              <meta name="twitter:image" content="{E(imageUrl)}" />
            """;

        var html = $"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <title>{E(title)}</title>
              <meta property="og:type" content="website" />
              <meta property="og:site_name" content="Questlog" />
              <meta property="og:title" content="{E(title)}" />
              <meta property="og:description" content="{E(description)}" />
              <meta property="og:url" content="{E(url)}" />
              <meta name="twitter:card" content="summary_large_image" />
              <meta name="twitter:title" content="{E(title)}" />
              <meta name="twitter:description" content="{E(description)}" />
            {image}
              <meta http-equiv="refresh" content="0; url={E(url)}" />
            </head>
            <body>
              <p>Redirecting to <a href="{E(url)}">Questlog</a>…</p>
            </body>
            </html>
            """;

        return Content(html, "text/html", Encoding.UTF8);
    }
}
