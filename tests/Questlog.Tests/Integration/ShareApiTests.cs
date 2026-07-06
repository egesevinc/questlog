using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Questlog.Tests.Integration;

public class ShareApiTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public ShareApiTests(TestWebAppFactory factory) => _client = factory.CreateClient();

    private record AuthResponse(string Token, Guid UserId, string Username);

    [Fact]
    public async Task Share_profile_renders_open_graph_html()
    {
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "sharee",
            email = "sharee@example.com",
            password = "password123",
        });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await _client.GetAsync($"/share/profiles/{auth!.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("og:title");
        html.Should().Contain("og:description");
        html.Should().Contain("sharee"); // the username appears in the title
    }

    [Fact]
    public async Task Share_profile_404s_for_an_unknown_user()
    {
        var response = await _client.GetAsync($"/share/profiles/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
