using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Questlog.Infrastructure.Igdb;

/// <summary>
/// Obtains and caches the Twitch app access token used to authenticate IGDB
/// calls. IGDB auth is Twitch OAuth2 client-credentials: POST client_id +
/// client_secret to the token endpoint, receive a bearer token with an
/// expires_in lifetime. We cache the token in memory until shortly before it
/// expires and refresh on demand, so we don't request a new token per call.
/// A SemaphoreSlim prevents a thundering herd of token requests when many
/// calls arrive after expiry.
/// </summary>
public interface ITwitchTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}

public class TwitchTokenProvider : ITwitchTokenProvider
{
    private const string CacheKey = "igdb_access_token";
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly IgdbOptions _options;

    public TwitchTokenProvider(HttpClient http, IMemoryCache cache, IOptions<IgdbOptions> options)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<string>(CacheKey, out var cached) && cached is not null)
            return cached;

        await Gate.WaitAsync(ct);
        try
        {
            // Re-check inside the lock: another caller may have refreshed it.
            if (_cache.TryGetValue<string>(CacheKey, out var token) && token is not null)
                return token;

            var url = $"{_options.TokenUrl}" +
                      $"?client_id={Uri.EscapeDataString(_options.ClientId)}" +
                      $"&client_secret={Uri.EscapeDataString(_options.ClientSecret)}" +
                      "&grant_type=client_credentials";

            using var response = await _http.PostAsync(url, null, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, cancellationToken: ct)
                          ?? throw new InvalidOperationException("Empty token response from Twitch.");

            // Refresh a minute early to avoid using a token that expires mid-flight.
            var ttl = TimeSpan.FromSeconds(Math.Max(60, payload.ExpiresIn - 60));
            _cache.Set(CacheKey, payload.AccessToken, ttl);

            return payload.AccessToken;
        }
        finally
        {
            Gate.Release();
        }
    }

    private sealed record TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = null!;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("token_type")] public string TokenType { get; init; } = null!;
    }
}
