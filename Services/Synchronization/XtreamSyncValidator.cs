using Jellyfin.Xtream.Api;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.Synchronization;

/// <summary>
/// Validates Xtream API configuration and connectivity before synchronization.
/// </summary>
public sealed class XtreamSyncValidator
{
    private readonly XtreamApiClient _api;
    private readonly ILogger<XtreamSyncValidator> _logger;

    public XtreamSyncValidator(XtreamApiClient api, ILogger<XtreamSyncValidator> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <summary>
    /// Validates configuration parameters.
    /// </summary>
    public ValidationResult ValidateConfiguration(string baseUrl, string username, string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(baseUrl))
            errors.Add("Server URL is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(username))
            errors.Add("Username is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(password))
            errors.Add("Password is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(baseUrl) || !IsValidUrl(baseUrl))
            errors.Add($"Server URL is invalid: {baseUrl}");

        if (errors.Any())
        {
            _logger.LogError("Configuration validation failed: {Errors}", string.Join("; ", errors));
            return ValidationResult.Failure(errors);
        }

        _logger.LogInformation("Configuration validation passed for {BaseUrl}", baseUrl);
        return ValidationResult.Success();
    }

    /// <summary>
    /// Tests connectivity to the Xtream API server.
    /// </summary>
    public async Task<ValidationResult> TestConnectivityAsync(string baseUrl, string username, string password, CancellationToken ct)
    {
        var errors = new List<string>();

        try
        {
            _logger.LogInformation("Testing connectivity to Xtream API at {BaseUrl}", baseUrl);

            // Try to get a single movie or series to test connectivity
            var testUrl = XtreamApiEndpoints.Movies(baseUrl, username, password);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout for connectivity test

            try
            {
                var result = await _api.GetAsync<dynamic>(testUrl, cts.Token);
                if (result == null)
                {
                    errors.Add("API returned null response - credentials may be invalid");
                    _logger.LogWarning("Connectivity test received null response");
                }
                else
                {
                    _logger.LogInformation("Connectivity test successful");
                }
            }
            catch (HttpRequestException ex)
            {
                errors.Add($"Cannot reach Xtream API: {ex.Message}");
                _logger.LogError(ex, "Connectivity test failed - cannot reach API");
            }
            catch (OperationCanceledException)
            {
                errors.Add("Connectivity test timed out - API server may be unreachable");
                _logger.LogError("Connectivity test timed out after 10 seconds");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error during connectivity test: {ex.Message}");
            _logger.LogError(ex, "Unexpected error in connectivity test");
        }

        if (errors.Any())
            return ValidationResult.Failure(errors);

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates all required endpoints are accessible.
    /// </summary>
    public async Task<ValidationResult> ValidateEndpointsAsync(string baseUrl, string username, string password, CancellationToken ct)
    {
        var errors = new List<string>();
        var endpoints = new[]
        {
            ("Movies", XtreamApiEndpoints.Movies(baseUrl, username, password)),
            ("Series", XtreamApiEndpoints.Series(baseUrl, username, password)),
            ("LiveStreams", XtreamApiEndpoints.LiveStreams(baseUrl, username, password))
        };

        foreach (var (name, url) in endpoints)
        {
            try
            {
                _logger.LogDebug("Testing {EndpointName} endpoint", name);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                await _api.GetAsync<dynamic>(url, cts.Token);
            }
            catch (HttpRequestException ex)
            {
                errors.Add($"{name} endpoint is not accessible: {ex.Message}");
                _logger.LogWarning(ex, "{EndpointName} endpoint validation failed", name);
            }
            catch (OperationCanceledException)
            {
                errors.Add($"{name} endpoint timed out");
                _logger.LogWarning("{EndpointName} endpoint validation timed out", name);
            }
            catch (Exception ex)
            {
                errors.Add($"{name} endpoint error: {ex.Message}");
                _logger.LogWarning(ex, "{EndpointName} endpoint validation error", name);
            }
        }

        if (errors.Any())
            return ValidationResult.Failure(errors);

        _logger.LogInformation("All endpoints validated successfully");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Performs full validation before sync (configuration + connectivity + endpoints).
    /// </summary>
    public async Task<ValidationResult> ValidateBeforeSyncAsync(string baseUrl, string username, string password, CancellationToken ct)
    {
        _logger.LogInformation("Starting pre-sync validation");

        // Step 1: Validate configuration
        var configResult = ValidateConfiguration(baseUrl, username, password);
        if (!configResult.IsValid)
            return configResult;

        // Step 2: Test connectivity
        var connectivityResult = await TestConnectivityAsync(baseUrl, username, password, ct);
        if (!connectivityResult.IsValid)
            return connectivityResult;

        // Step 3: Validate endpoints
        var endpointsResult = await ValidateEndpointsAsync(baseUrl, username, password, ct);
        if (!endpointsResult.IsValid)
            return endpointsResult;

        _logger.LogInformation("Pre-sync validation completed successfully");
        return ValidationResult.Success();
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    public static ValidationResult Success() => new(true, Array.Empty<string>());

    public static ValidationResult Failure(IEnumerable<string> errors) =>
        new(false, errors.ToList().AsReadOnly());

    public override string ToString() =>
        IsValid ? "Valid" : $"Invalid: {string.Join("; ", Errors)}";
}
