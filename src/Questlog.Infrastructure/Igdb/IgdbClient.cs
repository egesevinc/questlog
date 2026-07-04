using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Questlog.Application.Igdb;

namespace Questlog.Infrastructure.Igdb;

/// <summary>
/// Concrete IGDB v4 client. IGDB uses a POST-based query language (Apicalypse)
/// and returns JSON. This client:
///   * attaches the Client-ID header and the bearer token on every request,
///   * throttles concurrent outbound calls with a SemaphoreSlim to respect the
///     4 req/sec limit,
///   * maps IGDB's raw shape into our own IgdbGameDto so nothing else in the
///     app depends on IGDB's wire format,
///   * builds absolute cover URLs from IGDB's image_id at a sensible size.
/// </summary>
public class IgdbClient : IIgdbClient
{
    private readonly HttpClient _http;
    private readonly ITwitchTokenProvider _tokenProvider;
    private readonly IgdbOptions _options;
    private readonly SemaphoreSlim _throttle;

    public IgdbClient(HttpClient http, ITwitchTokenProvider tokenProvider, IOptions<IgdbOptions> options)
    {
        _http = http;
        _tokenProvider = tokenProvider;
        _options = options.Value;
        _throttle = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
    }

    private const string GameFields =
        "fields name,summary,first_release_date,cover.image_id," +
        "genres.name,platforms.name,platforms.abbreviation;";

    public async Task<IReadOnlyList<IgdbGameDto>> SearchGamesAsync(
        string query, int limit = 20, CancellationToken ct = default)
    {
        var safe = query.Replace("\"", "\\\"");
        var body = $"search \"{safe}\"; {GameFields} limit {limit};";
        var games = await PostGamesAsync(body, ct);
        return games;
    }

    public async Task<IgdbGameDto?> GetGameAsync(long igdbId, CancellationToken ct = default)
    {
        var body = $"{GameFields} where id = {igdbId}; limit 1;";
        var games = await PostGamesAsync(body, ct);
        return games.Count > 0 ? games[0] : null;
    }

    private async Task<List<IgdbGameDto>> PostGamesAsync(string apicalypseBody, CancellationToken ct)
    {
        await _throttle.WaitAsync(ct);
        try
        {
            var token = await _tokenProvider.GetAccessTokenAsync(ct);

            using var request = new HttpRequestMessage(HttpMethod.Post, "games")
            {
                Content = new StringContent(apicalypseBody)
            };
            request.Headers.Add("Client-ID", _options.ClientId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var result = new List<IgdbGameDto>();
            foreach (var el in doc.RootElement.EnumerateArray())
                result.Add(MapGame(el));
            return result;
        }
        finally
        {
            _throttle.Release();
        }
    }

    private static IgdbGameDto MapGame(JsonElement el)
    {
        var igdbId = el.GetProperty("id").GetInt64();
        var name = el.TryGetProperty("name", out var n) ? n.GetString()! : "(unknown)";
        string? summary = el.TryGetProperty("summary", out var s) ? s.GetString() : null;

        DateTimeOffset? release = null;
        if (el.TryGetProperty("first_release_date", out var rd) && rd.ValueKind == JsonValueKind.Number)
            release = DateTimeOffset.FromUnixTimeSeconds(rd.GetInt64());

        string? coverUrl = null;
        if (el.TryGetProperty("cover", out var cover) &&
            cover.TryGetProperty("image_id", out var imageId))
        {
            // t_cover_big = 264x374. Other sizes documented by IGDB if needed.
            coverUrl = $"https://images.igdb.com/igdb/image/upload/t_cover_big/{imageId.GetString()}.jpg";
        }

        var genres = ReadNamed(el, "genres");
        var platforms = ReadNamed(el, "platforms", includeAbbrev: true);

        return new IgdbGameDto(igdbId, name, summary, coverUrl, release, genres, platforms);
    }

    private static List<IgdbNamedDto> ReadNamed(JsonElement parent, string prop, bool includeAbbrev = false)
    {
        var list = new List<IgdbNamedDto>();
        if (parent.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                var id = item.GetProperty("id").GetInt64();
                var name = item.TryGetProperty("name", out var nm) ? nm.GetString() ?? "" : "";
                string? abbrev = includeAbbrev && item.TryGetProperty("abbreviation", out var ab)
                    ? ab.GetString() : null;
                list.Add(new IgdbNamedDto(id, name, abbrev));
            }
        }
        return list;
    }
}
