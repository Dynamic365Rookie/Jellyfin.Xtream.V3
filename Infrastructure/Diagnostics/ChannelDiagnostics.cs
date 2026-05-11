using System.Net.Http;
using System.Text.Json;
using Jellyfin.Xtream.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.V3.Infrastructure.Diagnostics;

/// <summary>
/// Diagnostic tool to debug channel icon URLs and EPG data issues.
/// </summary>
public sealed class ChannelDiagnostics
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChannelDiagnostics> _logger;

    public ChannelDiagnostics(HttpClient httpClient, ILogger<ChannelDiagnostics> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Diagnose why channel icons are not displaying.
    /// </summary>
    public async Task<ChannelIconDiagnosisResult> DiagnoseChannelIconsAsync(
        List<XtreamChannel> channels,
        string serverUrl,
        CancellationToken cancellationToken = default)
    {
        var result = new ChannelIconDiagnosisResult();

        if (channels == null || channels.Count == 0)
        {
            result.Issues.Add("No channels provided");
            return result;
        }

        var sampleChannels = channels.Take(5).ToList();
        result.TotalChannels = channels.Count;
        result.SampleSize = sampleChannels.Count;

        foreach (var channel in sampleChannels)
        {
            var diagnosis = new ChannelIconDiagnosis
            {
                ChannelId = channel.StreamId,
                ChannelName = channel.Name,
                IconUrl = channel.Icon,
            };

            // Check 1: Icon field populated?
            if (string.IsNullOrWhiteSpace(channel.Icon))
            {
                diagnosis.Issues.Add("Icon URL is null or empty");
                result.ChannelDiagnoses.Add(diagnosis);
                continue;
            }

            // Check 2: Is URL relative?
            if (channel.Icon.StartsWith("/") && !channel.Icon.StartsWith("//"))
            {
                diagnosis.Issues.Add($"Icon is relative path: {channel.Icon}");
                diagnosis.SuggestedFix = $"Prefix with server URL: {serverUrl}{channel.Icon}";
                result.ChannelDiagnoses.Add(diagnosis);
                continue;
            }

            // Check 3: Is URL accessible?
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, channel.Icon);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    diagnosis.IsAccessible = true;
                    diagnosis.HttpStatusCode = (int)response.StatusCode;
                    diagnosis.ContentType = response.Content.Headers.ContentType?.ToString() ?? "unknown";
                }
                else
                {
                    diagnosis.Issues.Add($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
                    diagnosis.HttpStatusCode = (int)response.StatusCode;

                    // Try with auth if 401/403
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        diagnosis.SuggestedFix = "Icon URL requires authentication - consider using a proxy endpoint";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                diagnosis.Issues.Add($"Network error: {ex.Message}");
                diagnosis.SuggestedFix = "URL may be invalid or unreachable";
            }
            catch (TaskCanceledException)
            {
                diagnosis.Issues.Add("Request timeout - URL may be slow or blocked");
            }

            result.ChannelDiagnoses.Add(diagnosis);
        }

        // Summary
        var accessibleCount = result.ChannelDiagnoses.Count(x => x.IsAccessible);
        var emptyCount = result.ChannelDiagnoses.Count(x => string.IsNullOrWhiteSpace(x.IconUrl));
        var relativeCount = result.ChannelDiagnoses.Count(x =>
            !string.IsNullOrWhiteSpace(x.IconUrl) && x.IconUrl.StartsWith("/"));

        if (emptyCount > 0)
            result.Issues.Add($"{emptyCount}/{result.SampleSize} channels have empty Icon field");

        if (relativeCount > 0)
            result.Issues.Add($"{relativeCount}/{result.SampleSize} channels have relative icon paths");

        if (accessibleCount == 0 && emptyCount == 0)
            result.Issues.Add("All icon URLs are inaccessible");

        result.AccessibleCount = accessibleCount;
        return result;
    }

    /// <summary>
    /// Diagnose why EPG programs are not appearing.
    /// </summary>
    public async Task<EpgDiagnosisResult> DiagnoseEpgAsync(
        List<XtreamChannel> channels,
        string serverUrl,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = new EpgDiagnosisResult();

        if (channels == null || channels.Count == 0)
        {
            result.Issues.Add("No channels provided");
            return result;
        }

        var sampleChannels = channels.Take(3).ToList();
        result.TotalChannels = channels.Count;
        result.SampleSize = sampleChannels.Count;

        var now = DateTime.UtcNow;

        foreach (var channel in sampleChannels)
        {
            var epgDiag = new EpgChannelDiagnosis
            {
                ChannelId = channel.StreamId,
                ChannelName = channel.Name,
                EpgChannelId = channel.EpgChannelId,
            };

            // Check 1: EPG Channel ID configured?
            if (!channel.EpgChannelId.HasValue || channel.EpgChannelId <= 0)
            {
                epgDiag.Issues.Add("EpgChannelId is not set (null or 0)");
                epgDiag.SuggestedFix = "Ensure Xtream API returns 'epg_channel_id' for this channel";
                result.ChannelDiagnoses.Add(epgDiag);
                continue;
            }

            // Check 2: Query EPG endpoint
            var epgUrl = $"{serverUrl.TrimEnd('/')}/player_api.php?username={username}&password={password}&action=get_simple_data_table&stream_id={channel.StreamId}";
            epgDiag.EpgEndpoint = epgUrl;

            try
            {
                var response = await _httpClient.GetAsync(epgUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    epgDiag.Issues.Add($"EPG API returned HTTP {response.StatusCode}");
                    epgDiag.HttpStatusCode = (int)response.StatusCode;
                    result.ChannelDiagnoses.Add(epgDiag);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(json))
                {
                    epgDiag.Issues.Add("EPG API returned empty response");
                    result.ChannelDiagnoses.Add(epgDiag);
                    continue;
                }

                // Try to parse JSON
                try
                {
                    var epgResponse = JsonSerializer.Deserialize<XtreamEpgResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (epgResponse?.Listings == null || epgResponse.Listings.Count == 0)
                    {
                        epgDiag.Issues.Add("EPG response has no listings");
                        epgDiag.SuggestedFix = "Check if stream_id is valid in EPG database";
                        result.ChannelDiagnoses.Add(epgDiag);
                        continue;
                    }

                    epgDiag.TotalListings = epgResponse.Listings.Count;

                    // Check 3: Analyze listings
                    var futureListings = epgResponse.Listings.Where(l =>
                    {
                        var end = ParseTimestamp(l.StopTimestamp, l.End);
                        return end > now;
                    }).ToList();

                    epgDiag.FutureListings = futureListings.Count;
                    epgDiag.PastListings = epgResponse.Listings.Count - futureListings.Count;

                    if (futureListings.Count == 0)
                    {
                        epgDiag.Issues.Add("No future programs in EPG (all are in the past)");
                    }

                    // Sample program
                    if (futureListings.Count > 0)
                    {
                        var sample = futureListings.First();
                        epgDiag.SampleProgram = new ProgramSample
                        {
                            Title = sample.Title,
                            StartTime = ParseTimestamp(sample.StartTimestamp, sample.Start),
                            EndTime = ParseTimestamp(sample.StopTimestamp, sample.End),
                            HasDescription = !string.IsNullOrWhiteSpace(sample.Description),
                        };
                    }
                }
                catch (JsonException ex)
                {
                    epgDiag.Issues.Add($"Failed to parse EPG JSON: {ex.Message}");
                    epgDiag.RawResponse = json[..Math.Min(200, json.Length)]; // First 200 chars
                }
            }
            catch (HttpRequestException ex)
            {
                epgDiag.Issues.Add($"Network error: {ex.Message}");
            }

            result.ChannelDiagnoses.Add(epgDiag);
        }

        // Summary
        var hasEpgData = result.ChannelDiagnoses.Count(x => x.TotalListings > 0);
        var hasFuturePrograms = result.ChannelDiagnoses.Count(x => x.FutureListings > 0);

        if (hasEpgData == 0)
            result.Issues.Add("No channels have EPG data - check API configuration");

        if (hasFuturePrograms == 0 && hasEpgData > 0)
            result.Issues.Add("EPG data exists but all programs are in the past - check system time");

        result.ChannelsWithEpg = hasEpgData;
        result.ChannelsWithFuturePrograms = hasFuturePrograms;

        return result;
    }

    private static DateTime ParseTimestamp(long? epochTimestamp, string? stringDate)
    {
        if (epochTimestamp.HasValue && epochTimestamp > 0)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epochTimestamp.Value).UtcDateTime;
        }

        if (!string.IsNullOrWhiteSpace(stringDate) && DateTime.TryParse(stringDate, out var dt))
        {
            return dt.ToUniversalTime();
        }

        return DateTime.UtcNow;
    }
}

// ===== DIAGNOSTIC RESULT TYPES =====

public sealed class ChannelIconDiagnosisResult
{
    public int TotalChannels { get; set; }
    public int SampleSize { get; set; }
    public int AccessibleCount { get; set; }
    public List<string> Issues { get; } = new();
    public List<ChannelIconDiagnosis> ChannelDiagnoses { get; } = new();
}

public sealed class ChannelIconDiagnosis
{
    public int ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public string? IconUrl { get; set; }
    public bool IsAccessible { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ContentType { get; set; }
    public List<string> Issues { get; } = new();
    public string? SuggestedFix { get; set; }
}

public sealed class EpgDiagnosisResult
{
    public int TotalChannels { get; set; }
    public int SampleSize { get; set; }
    public int ChannelsWithEpg { get; set; }
    public int ChannelsWithFuturePrograms { get; set; }
    public List<string> Issues { get; } = new();
    public List<EpgChannelDiagnosis> ChannelDiagnoses { get; } = new();
}

public sealed class EpgChannelDiagnosis
{
    public int ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public int? EpgChannelId { get; set; }
    public string? EpgEndpoint { get; set; }
    public int? HttpStatusCode { get; set; }
    public int TotalListings { get; set; }
    public int FutureListings { get; set; }
    public int PastListings { get; set; }
    public ProgramSample? SampleProgram { get; set; }
    public List<string> Issues { get; } = new();
    public string? SuggestedFix { get; set; }
    public string? RawResponse { get; set; }
}

public sealed class ProgramSample
{
    public string? Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool HasDescription { get; set; }
}
