using Microsoft.AspNetCore.Mvc;
using Jellyfin.Xtream.V3.Infrastructure.Diagnostics;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.V3.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.V3.Api.Controllers;

/// <summary>
/// Debug endpoints for diagnosing channel icons and EPG issues.
/// </summary>
[ApiController]
[Route("Xtream/Debug")]
public sealed class DebugController : ControllerBase
{
    private readonly IXtreamRepository<XtreamChannel> _channelRepository;
    private readonly ChannelDiagnostics _diagnostics;
    private readonly PluginConfiguration _config;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        IXtreamRepository<XtreamChannel> channelRepository,
        ChannelDiagnostics diagnostics,
        PluginConfiguration config,
        ILogger<DebugController> logger)
    {
        _channelRepository = channelRepository;
        _diagnostics = diagnostics;
        _config = config;
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
            _logger.LogInformation("[Debug] Diagnosing channel icons");

            var channels = _channelRepository.GetAll().ToList();
            if (channels.Count == 0)
                return Ok(new { message = "No channels in database" });

            var result = await _diagnostics.DiagnoseChannelIconsAsync(
                channels,
                _config.ServerUrl,
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
            _logger.LogInformation("[Debug] Diagnosing EPG data");

            if (string.IsNullOrWhiteSpace(_config.ServerUrl) ||
                string.IsNullOrWhiteSpace(_config.Username) ||
                string.IsNullOrWhiteSpace(_config.Password))
            {
                return BadRequest(new { error = "Xtream configuration incomplete" });
            }

            var channels = _channelRepository.GetAll().ToList();
            if (channels.Count == 0)
                return Ok(new { message = "No channels in database" });

            var result = await _diagnostics.DiagnoseEpgAsync(
                channels,
                _config.ServerUrl,
                _config.Username,
                _config.Password,
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
            return Ok(new
            {
                ServerUrl = _config.ServerUrl,
                Username = string.IsNullOrEmpty(_config.Username) ? "(not set)" : "***",
                Password = string.IsNullOrEmpty(_config.Password) ? "(not set)" : "***",
                EnableEPG = _config.EnableEPG,
                EnableLiveTV = _config.EnableLiveTV,
                EnableChannelNameCleaning = _config.EnableChannelNameCleaning,
                ShowChannelLanguageTags = _config.ShowChannelLanguageTags,
                AppendLanguageToChannelName = _config.AppendLanguageToChannelName,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Debug] Configuration retrieval failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
