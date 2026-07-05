using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Questlog.Tests.Integration;

/// <summary>
/// End-to-end tests through the real HTTP pipeline: routing, model binding,
/// validation, the error contract, EF, and JWT issue + validate.
/// </summary>
public class AuthApiTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(TestWebAppFactory factory) => _client = factory.CreateClient();

    private record AuthResponse(string Token, Guid UserId, string Username);

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("ok");
    }

    [Fact]
    public async Task Register_then_use_the_token_on_a_protected_endpoint()
    {
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "ituser",
            email = "ituser@example.com",
            password = "password123",
        });
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.Username.Should().Be("ituser");

        // The protected "my logs" endpoint should accept the freshly issued token.
        using var authed = new HttpRequestMessage(HttpMethod.Get, "/api/logs/me");
        authed.Headers.Authorization = new("Bearer", auth.Token);
        var logs = await _client.SendAsync(authed);

        logs.StatusCode.Should().Be(HttpStatusCode.OK);
        (await logs.Content.ReadAsStringAsync()).Should().Be("[]");
    }

    [Fact]
    public async Task Register_with_a_short_password_returns_400_with_a_detail()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "shorty",
            email = "shorty@example.com",
            password = "short", // below the 8-char minimum
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("detail");
    }

    [Fact]
    public async Task Protected_endpoint_without_a_token_returns_401()
    {
        var response = await _client.GetAsync("/api/logs/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
