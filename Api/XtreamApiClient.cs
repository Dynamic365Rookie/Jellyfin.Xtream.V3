using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Api;

public sealed class XtreamApiClient
{
    private readonly HttpClient _http;
    private readonly Jellyfin.Xtream.Api.XtreamApiRateLimiter _rateLimiter;
    private readonly ILogger<XtreamApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public XtreamApiClient(
        HttpClient http, 
        Jellyfin.Xtream.Api.XtreamApiRateLimiter rateLimiter,
        ILogger<XtreamApiClient> logger)
    {
        _http = http;
        _rateLimiter = rateLimiter;
        _logger = logger;
        
        // Options de sérialisation optimisées
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultBufferSize = 65536, // 64KB buffer pour meilleures performances
            AllowTrailingCommas = true
        };
        
        // Configurer le timeout et le buffer
        _http.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        await _rateLimiter.WaitAsync(ct);

        var requestStart = DateTime.UtcNow;
        
        try
        {
            using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);

            var result = await JsonSerializer.DeserializeAsync<T>(
                stream,
                _jsonOptions,
                cancellationToken: ct);

            if (result == null)
            {
                _logger.LogWarning("Xtream API returned null for URL: {Url}", url);
                throw new InvalidOperationException("Xtream API returned null");
            }

            var duration = DateTime.UtcNow - requestStart;
            _logger.LogDebug("API call to {Url} completed in {Duration}ms", url, duration.TotalMilliseconds);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Xtream API: {Url}", url);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for URL: {Url}", url);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("API call to {Url} was cancelled", url);
            throw;
        }
    }

    public async Task<T> GetWithRetryAsync<T>(string url, int maxRetries, CancellationToken ct)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                return await GetAsync<T>(url, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                lastException = ex;
                attempt++;
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                    _logger.LogWarning("API call failed (attempt {Attempt}/{MaxRetries}), retrying in {Delay}s...", 
                        attempt, maxRetries, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                }
            }
        }

        _logger.LogError(lastException, "API call failed after {MaxRetries} attempts", maxRetries);
        throw lastException ?? new InvalidOperationException("API call failed");
    }
}
