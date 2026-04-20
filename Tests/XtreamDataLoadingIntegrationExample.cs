using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Tests;

/// <summary>
/// Example integration test showing how to validate and load Xtream data.
/// </summary>
public sealed class XtreamDataLoadingIntegrationExample
{
    private readonly ILogger<XtreamDataLoadingIntegrationExample> _logger;
    private readonly XtreamSyncValidator _validator;
    private readonly XtreamSyncService _syncService;

    public XtreamDataLoadingIntegrationExample(
        ILogger<XtreamDataLoadingIntegrationExample> logger,
        XtreamSyncValidator validator,
        XtreamSyncService syncService)
    {
        _logger = logger;
        _validator = validator;
        _syncService = syncService;
    }

    /// <summary>
    /// Example: Complete workflow with validation before loading data.
    /// </summary>
    public async Task RunCompleteWorkflowAsync(
        string serverUrl,
        string username,
        string password,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting complete Xtream data loading workflow");
        _logger.LogInformation("Server: {ServerUrl}", serverUrl);

        // Step 1: Validate configuration
        _logger.LogInformation("Step 1: Validating configuration...");
        var configResult = _validator.ValidateConfiguration(serverUrl, username, password);
        if (!configResult.IsValid)
        {
            _logger.LogError("Configuration validation failed:");
            foreach (var error in configResult.Errors)
            {
                _logger.LogError("  - {Error}", error);
            }
            return;
        }
        _logger.LogInformation("Configuration validation passed ✓");

        // Step 2: Test connectivity
        _logger.LogInformation("Step 2: Testing connectivity...");
        var connectivityResult = await _validator.TestConnectivityAsync(serverUrl, username, password, ct);
        if (!connectivityResult.IsValid)
        {
            _logger.LogError("Connectivity test failed:");
            foreach (var error in connectivityResult.Errors)
            {
                _logger.LogError("  - {Error}", error);
            }
            return;
        }
        _logger.LogInformation("Connectivity test passed ✓");

        // Step 3: Validate endpoints
        _logger.LogInformation("Step 3: Validating endpoints...");
        var endpointsResult = await _validator.ValidateEndpointsAsync(serverUrl, username, password, ct);
        if (!endpointsResult.IsValid)
        {
            _logger.LogError("Endpoint validation failed:");
            foreach (var error in endpointsResult.Errors)
            {
                _logger.LogError("  - {Error}", error);
            }
            return;
        }
        _logger.LogInformation("Endpoint validation passed ✓");

        // Step 4: Start synchronization
        _logger.LogInformation("Step 4: Starting data synchronization...");
        var syncResult = await _syncService.SyncAllWithValidationAsync(serverUrl, username, password, ct);
        if (!syncResult.IsSuccess)
        {
            _logger.LogError("Synchronization failed:");
            foreach (var error in syncResult.Errors)
            {
                _logger.LogError("  - {Error}", error);
            }
            return;
        }
        _logger.LogInformation("Data synchronization completed successfully ✓");

        _logger.LogInformation("=== Complete workflow finished successfully ===");
    }

    /// <summary>
    /// Example: Quick validation without full sync.
    /// </summary>
    public async Task<bool> QuickValidateAsync(
        string serverUrl,
        string username,
        string password,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Running quick validation...");

        var result = await _validator.ValidateBeforeSyncAsync(serverUrl, username, password, ct);

        if (result.IsValid)
        {
            _logger.LogInformation("Quick validation passed ✓");
            return true;
        }

        _logger.LogError("Quick validation failed:");
        foreach (var error in result.Errors)
        {
            _logger.LogError("  - {Error}", error);
        }
        return false;
    }
}

/// <summary>
/// Configuration helper for setting up Xtream data loading in the plugin.
/// </summary>
public sealed class XtreamDataLoadingConfiguration
{
    /// <summary>
    /// Example: How to register services for Xtream data loading in your DI container.
    /// Note: Requires Microsoft.Extensions.Http package for AddHttpClient.
    /// </summary>
    public static void ConfigureXtreamServices(IServiceCollection services)
    {
        // Register rate limiter
        services.AddSingleton<Jellyfin.Xtream.Api.XtreamApiRateLimiter>();

        // Register validator
        services.AddScoped<XtreamSyncValidator>();

        // Register sync service
        services.AddScoped<XtreamSyncService>();

        // Register test suite
        services.AddScoped<XtreamDataLoadingTests>();

        // TODO: Add these when Microsoft.Extensions.Http is available:
        // services.AddHttpClient<XtreamApiClient>()
        //     .ConfigureHttpClient(client =>
        //     {
        //         client.Timeout = TimeSpan.FromMinutes(5);
        //         client.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-Xtream-Plugin/1.0");
        //     });
    }
}
