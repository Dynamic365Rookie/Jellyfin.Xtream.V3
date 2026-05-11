using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jellyfin.Xtream.V3.Infrastructure.Diagnostics;
using Jellyfin.Xtream.V3;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.V3.Api.Controllers;

/// <summary>
/// Debug endpoints for diagnosing channel icons and EPG issues.
/// Requires Jellyfin administrator authentication.
/// </summary>
[ApiController]
[Route("Xtream/Debug")]
[Authorize(Policy = "RequiresElevation")]
public sealed class DebugController : ControllerBase
{
    private readonly IXtreamRepository<XtreamChannel> _channelRepository;
    private readonly ChannelDiagnostics _diagnostics;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        IXtreamRepository<XtreamChannel> channelRepository,
        ChannelDiagnostics diagnostics,
        ILogger<DebugController> logger)
    {
        _channelRepository = channelRepository;
        _diagnostics = diagnostics;
        _logger = logger;
    }

    /// <summary>
    /// Diagnose channel icon URLs and accessibility.
    /// </summary>
    [HttpGet("channels/icons")]
    public async Task<IActionResult> DiagnoseChannelIcons(CancellationToken cancellationToken)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            _logger.LogInformation("[Debug] Diagnosing channel icons");

            if (config == null || string.IsNullOrWhiteSpace(config.ServerUrl))
            {
                return BadRequest(new { error = "Xtream configuration incomplete" });
            }

            var channels = _channelRepository.GetAll().ToList();
            if (channels.Count == 0)
                return Ok(new { message = "No channels in database" });

            var result = await _diagnostics.DiagnoseChannelIconsAsync(
                channels,
                config.ServerUrl,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Debug] Channel icon diagnosis failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Diagnose EPG data retrieval and parsing.
    /// </summary>
    [HttpGet("channels/epg")]
    public async Task<IActionResult> DiagnoseEpg(CancellationToken cancellationToken)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            _logger.LogInformation("[Debug] Diagnosing EPG data");

            if (config == null || string.IsNullOrWhiteSpace(config.ServerUrl) ||
                string.IsNullOrWhiteSpace(config.Username) ||
                string.IsNullOrWhiteSpace(config.Password))
            {
                return BadRequest(new { error = "Xtream configuration incomplete" });
            }

            var channels = _channelRepository.GetAll().ToList();
            if (channels.Count == 0)
                return Ok(new { message = "No channels in database" });

            var result = await _diagnostics.DiagnoseEpgAsync(
                channels,
                config.ServerUrl,
                config.Username,
                config.Password,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Debug] EPG diagnosis failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get configuration summary.
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfiguration()
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
                return BadRequest(new { error = "Plugin not initialized" });

            return Ok(new
            {
                ServerUrl = config.ServerUrl,
                Username = string.IsNullOrEmpty(config.Username) ? "(not set)" : "***",
                Password = string.IsNullOrEmpty(config.Password) ? "(not set)" : "***",
                EnableEPG = config.EnableEPG,
                EnableLiveTV = config.EnableLiveTV,
                EnableChannelNameCleaning = config.EnableChannelNameCleaning,
                ShowChannelLanguageTags = config.ShowChannelLanguageTags,
                AppendLanguageToChannelName = config.AppendLanguageToChannelName,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Debug] Configuration retrieval failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test EPG fetching for a specific channel (debug only).
    /// </summary>
    /// <param name="streamId">Stream ID to test EPG for</param>
    [HttpGet("epg/test/{streamId}")]
    public async Task<IActionResult> TestEpgFetch(int streamId, CancellationToken cancellationToken)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            _logger.LogInformation("[Debug] Testing EPG fetch for stream {StreamId}", streamId);

            if (config == null || !config.EnableEPG)
                return BadRequest(new { error = "EPG disabled in configuration" });

            var channel = _channelRepository.GetAll()
                .FirstOrDefault(c => c.StreamId == streamId);
            if (channel == null)
                return NotFound(new { error = $"Channel with stream_id {streamId} not found" });

            var result = await _diagnostics.DiagnoseEpgAsync(
                new List<XtreamChannel> { channel },
                config.ServerUrl,
                config.Username,
                config.Password,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Debug] EPG test failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
