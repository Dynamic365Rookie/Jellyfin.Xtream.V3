using Jellyfin.Xtream.BackgroundTasks;
using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Jellyfin.Xtream.Services.Synchronization;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Xtream.BackgroundTasks;

/// <summary>
/// Extension methods for registering Xtream background tasks in the DI container.
/// </summary>
public static class XtreamTasksServiceCollectionExtensions
{
    /// <summary>
    /// Registers Xtream IPTV scheduled tasks.
    /// </summary>
    public static IServiceCollection AddXtreamTasks(this IServiceCollection services)
    {
        // Register the incremental sync task
        services.AddScoped<XtreamIncrementalSyncTask>();

        // Ensure dependencies are registered
        services.AddScoped<XtreamSyncService>();
        services.AddScoped<PerformanceMonitor>();
        services.AddScoped<MemoryManager>();

        return services;
    }
}
