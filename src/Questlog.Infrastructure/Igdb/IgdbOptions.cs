namespace Questlog.Infrastructure.Igdb;

/// <summary>
/// IGDB / Twitch credentials and tuning. Bound from configuration.
/// The secrets (ClientId/ClientSecret) must come from user-secrets or
/// environment variables — never hard-coded or committed.
/// </summary>
public class IgdbOptions
{
    public const string SectionName = "Igdb";

    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;

    public string TokenUrl { get; set; } = "https://id.twitch.tv/oauth2/token";
    public string ApiBaseUrl { get; set; } = "https://api.igdb.com/v4/";

    /// <summary>
    /// IGDB allows 4 requests/second. We cap concurrent outbound calls below
    /// that to stay safely inside the limit.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 3;
}
