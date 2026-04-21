using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.LiveTv;
using Jellyfin.Xtream.Services.Synchronization;
using LiteDB;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.JellyfinIntegration;

/// <summary>
/// Registers all Xtream plugin services with the Jellyfin DI container.
/// Must have a parameterless constructor per Jellyfin SDK requirement.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Infrastructure — rate limiter (singleton, stateful semaphore)
        serviceCollection.AddSingleton<XtreamApiRateLimiter>();

        // Infrastructure — LiteDB database (singleton, thread-safe in shared mode)
        serviceCollection.AddSingleton<LiteDatabase>(sp =>
        {
            var appPaths = sp.GetRequiredService<IApplicationPaths>();
            var dbDir = Path.Combine(appPaths.DataPath, "xtream");
            Directory.CreateDirectory(dbDir);
            var dbPath = Path.Combine(dbDir, "xtream.db");
            return LiteDbConfiguration.CreateOptimizedDatabase(dbPath);
        });

        // Repositories — one per entity type
        serviceCollection.AddSingleton<IXtreamRepository<XtreamMovie>>(sp =>
            new LiteDbXtreamRepository<XtreamMovie>(sp.GetRequiredService<LiteDatabase>(), "movies"));

        serviceCollection.AddSingleton<IXtreamRepository<XtreamSeries>>(sp =>
            new LiteDbXtreamRepository<XtreamSeries>(sp.GetRequiredService<LiteDatabase>(), "series"));

        serviceCollection.AddSingleton<IXtreamRepository<XtreamChannel>>(sp =>
            new LiteDbXtreamRepository<XtreamChannel>(sp.GetRequiredService<LiteDatabase>(), "channels"));

        // API client — singleton with its own HttpClient
        serviceCollection.AddSingleton<XtreamApiClient>(sp =>
            new XtreamApiClient(
                new HttpClient(),
                sp.GetRequiredService<XtreamApiRateLimiter>(),
                sp.GetRequiredService<ILogger<XtreamApiClient>>()));

        // Services
        serviceCollection.AddSingleton<XtreamSyncValidator>();
        serviceCollection.AddSingleton<XtreamSyncService>();

        // Live TV — exposes channels to Jellyfin's Live TV system
        // Register as concrete type first, then forward to interface
        // This pattern ensures the service is discoverable both via DI and assembly scanning
        serviceCollection.AddSingleton<XtreamLiveTvService>();
        serviceCollection.AddSingleton<ILiveTvService>(sp => sp.GetRequiredService<XtreamLiveTvService>());
    }
}
