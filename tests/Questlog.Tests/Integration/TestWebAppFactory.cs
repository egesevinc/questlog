using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Questlog.Infrastructure.Persistence;

namespace Questlog.Tests.Integration;

/// <summary>
/// Boots the real API in-process for integration tests, swapping the Postgres
/// DbContext for a private in-memory database so tests need no external services.
/// Runs under the "Testing" environment, so startup seeding/Swagger stay off.
/// </summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"it-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<QuestlogDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<QuestlogDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
