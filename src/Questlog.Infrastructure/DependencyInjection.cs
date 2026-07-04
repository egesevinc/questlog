using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Questlog.Application.Auth;
using Questlog.Application.Games;
using Questlog.Application.GameLists;
using Questlog.Application.GameLogs;
using Questlog.Application.Igdb;
using Questlog.Infrastructure.Auth;
using Questlog.Infrastructure.Igdb;
using Questlog.Infrastructure.Persistence;
using Questlog.Infrastructure.Services;

namespace Questlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // --- Database ---
        services.AddDbContext<QuestlogDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default")));

        // --- Options ---
        services.Configure<IgdbOptions>(config.GetSection(IgdbOptions.SectionName));
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        services.AddMemoryCache();

        // --- IGDB integration. Typed HttpClients so each gets its own
        //     configured base address and pipeline. ---
        services.AddHttpClient<ITwitchTokenProvider, TwitchTokenProvider>();
        services.AddHttpClient<IIgdbClient, IgdbClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<IgdbOptions>>().Value;
            client.BaseAddress = new Uri(opts.ApiBaseUrl);
        });

        // --- Auth ---
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // --- Domain services ---
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IGameLogService, GameLogService>();
        services.AddScoped<IGameListService, GameListService>();

        return services;
    }
}
