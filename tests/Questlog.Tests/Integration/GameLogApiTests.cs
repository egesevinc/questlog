using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Questlog.Domain.Entities;
using Questlog.Infrastructure.Persistence;
using Xunit;

namespace Questlog.Tests.Integration;

/// <summary>
/// Exercises the log + list write paths through the real HTTP + JSON pipeline.
/// Guards the JSON contract the SPA relies on — most importantly that enum fields
/// (log status) bind from and serialize back as their string names, not integers.
/// </summary>
public class GameLogApiTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;

    public GameLogApiTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private record AuthResponse(string Token, Guid UserId, string Username);

    /// <summary>Seed a fresh cached game so the write paths don't reach out to IGDB.</summary>
    private async Task<long> SeedGameAsync(long igdbId, string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuestlogDbContext>();
        db.Games.Add(new Game { IgdbId = igdbId, Name = name, CachedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        return igdbId;
    }

    private async Task<string> RegisterAsync(string username)
    {
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"{username}@example.com",
            password = "password123",
        });
        register.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    [Fact]
    public async Task Create_log_accepts_a_string_status_and_echoes_it_back()
    {
        var igdbId = await SeedGameAsync(4242001, "Status Test Game");
        var token = await RegisterAsync("logtester");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/logs")
        {
            // The SPA sends the enum as a string ("Completed"). Before the
            // JsonStringEnumConverter was registered this failed to bind and the
            // API returned 400 "The request field is required."
            Content = JsonContent.Create(new
            {
                igdbId,
                status = "Completed",
                rating = 9,
                hoursPlayed = 20,
                reviewBody = "goated game",
            }),
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"status\":\"Completed\""); // enum serialized as a string, not 3
    }

    [Fact]
    public async Task Add_game_to_a_list_returns_the_list_with_the_item()
    {
        var igdbId = await SeedGameAsync(4242002, "List Test Game");
        var token = await RegisterAsync("listtester");

        // Create a list.
        using var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/lists")
        {
            Content = JsonContent.Create(new { title = "Deneme", description = (string?)null, isPublic = true }),
        };
        createReq.Headers.Authorization = new("Bearer", token);
        var createRes = await _client.SendAsync(createReq);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var list = await createRes.Content.ReadFromJsonAsync<ListResponse>();

        // Add the seeded game to it.
        using var addReq = new HttpRequestMessage(HttpMethod.Post, $"/api/lists/{list!.Id}/items")
        {
            Content = JsonContent.Create(new { igdbId, note = (string?)null }),
        };
        addReq.Headers.Authorization = new("Bearer", token);
        var addRes = await _client.SendAsync(addReq);

        addRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await addRes.Content.ReadFromJsonAsync<ListResponse>();
        updated!.Items.Should().ContainSingle().Which.IgdbId.Should().Be(igdbId);
    }

    private record ListResponse(Guid Id, string Title, List<ListItemResponse> Items);
    private record ListItemResponse(Guid Id, long IgdbId, string GameName);
}
