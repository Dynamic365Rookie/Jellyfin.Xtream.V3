using System.ComponentModel.DataAnnotations;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.Media;
using Jellyfin.Xtream.V3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Api.Controllers;

/// <summary>
/// Developer utilities for managing Xtream plugin data.
/// </summary>
[ApiController]
[Route("Xtream/Developer")]
[Authorize(Policy = "RequiresElevation")]
public class XtreamDeveloperController : ControllerBase
{
    private readonly IXtreamRepository<XtreamMovie> _movieRepo;
    private readonly IXtreamRepository<XtreamSeries> _seriesRepo;
    private readonly IXtreamRepository<XtreamChannel> _channelRepo;
    private readonly StrmFileGenerator _strmGenerator;
    private readonly ILogger<XtreamDeveloperController> _logger;

    public XtreamDeveloperController(
        IXtreamRepository<XtreamMovie> movieRepo,
        IXtreamRepository<XtreamSeries> seriesRepo,
        IXtreamRepository<XtreamChannel> channelRepo,
        StrmFileGenerator strmGenerator,
        ILogger<XtreamDeveloperController> logger)
    {
        _movieRepo = movieRepo;
        _seriesRepo = seriesRepo;
        _channelRepo = channelRepo;
        _strmGenerator = strmGenerator;
        _logger = logger;
    }

    [HttpGet("Stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<DatabaseStats> GetStats()
    {
        return Ok(new DatabaseStats
        {
            MovieCount = _movieRepo.Count(),
            SeriesCount = _seriesRepo.Count(),
            ChannelCount = _channelRepo.Count()
        });
    }

    [HttpPost("ClearDatabase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ClearDatabaseResult> ClearDatabase()
    {
        _logger.LogWarning("[Xtream] Developer: Clearing database + STRM files");

        try
        {
            var moviesDeleted = _movieRepo.DeleteAll();
            var seriesDeleted = _seriesRepo.DeleteAll();
            var channelsDeleted = _channelRepo.DeleteAll();

            var strmDeleted = DeleteStrmFiles(true, true);

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                MoviesDeleted = moviesDeleted,
                SeriesDeleted = seriesDeleted,
                ChannelsDeleted = channelsDeleted,
                Message = $"Cleared: {moviesDeleted} movies, {seriesDeleted} series, {channelsDeleted} channels, {strmDeleted} STRM files"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Xtream] Developer: Failed to clear database");
            return StatusCode(500, new ClearDatabaseResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("DeleteMovies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ClearDatabaseResult> DeleteMovies()
    {
        try
        {
            var deleted = _movieRepo.DeleteAll();
            var strmDeleted = DeleteStrmFiles(true, false);

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                MoviesDeleted = deleted,
                Message = $"Deleted: {deleted} movies, {strmDeleted} STRM files"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ClearDatabaseResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("DeleteSeries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ClearDatabaseResult> DeleteSeries()
    {
        try
        {
            var deleted = _seriesRepo.DeleteAll();
            var strmDeleted = DeleteStrmFiles(false, true);

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                SeriesDeleted = deleted,
                Message = $"Deleted: {deleted} series, {strmDeleted} STRM files"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ClearDatabaseResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("DeleteChannels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ClearDatabaseResult> DeleteChannels()
    {
        try
        {
            var deleted = _channelRepo.DeleteAll();

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                ChannelsDeleted = deleted,
                Message = $"Deleted: {deleted} channels"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ClearDatabaseResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("RegenerateStrm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClearDatabaseResult>> RegenerateStrm(CancellationToken ct)
    {
        try
        {
            var strmDeleted = DeleteStrmFiles(true, true);
            await _strmGenerator.GenerateAllAsync(ct).ConfigureAwait(false);

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                Message = $"Regenerated STRM files ({strmDeleted} old files removed)"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ClearDatabaseResult { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("Movies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<XtreamMovie>> GetMovies([FromQuery] int limit = 50)
    {
        limit = Math.Min(Math.Max(limit, 1), 500);
        return Ok(_movieRepo.GetAll().Take(limit));
    }

    [HttpGet("Series")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<XtreamSeries>> GetSeries([FromQuery] int limit = 50)
    {
        limit = Math.Min(Math.Max(limit, 1), 500);
        return Ok(_seriesRepo.GetAll().Take(limit));
    }

    [HttpGet("Channels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<XtreamChannel>> GetChannels([FromQuery] int limit = 50)
    {
        limit = Math.Min(Math.Max(limit, 1), 500);
        return Ok(_channelRepo.GetAll().Take(limit));
    }

    private int DeleteStrmFiles(bool movies, bool series)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null) return 0;

        var count = 0;

        if (movies && !string.IsNullOrWhiteSpace(config.StrmMoviesPath) && Directory.Exists(config.StrmMoviesPath))
        {
            count += CountFiles(config.StrmMoviesPath, "*.strm");
            Directory.Delete(config.StrmMoviesPath, true);
        }

        if (series && !string.IsNullOrWhiteSpace(config.StrmSeriesPath) && Directory.Exists(config.StrmSeriesPath))
        {
            count += CountFiles(config.StrmSeriesPath, "*.strm");
            Directory.Delete(config.StrmSeriesPath, true);
        }

        return count;
    }

    private static int CountFiles(string path, string pattern)
    {
        try { return Directory.GetFiles(path, pattern, SearchOption.AllDirectories).Length; }
        catch { return 0; }
    }
}

/// <summary>
/// Database statistics.
/// </summary>
public class DatabaseStats
{
    /// <summary>
    /// Gets or sets the number of movies.
    /// </summary>
    [Required]
    public int MovieCount { get; set; }

    /// <summary>
    /// Gets or sets the number of series.
    /// </summary>
    [Required]
    public int SeriesCount { get; set; }

    /// <summary>
    /// Gets or sets the number of channels.
    /// </summary>
    [Required]
    public int ChannelCount { get; set; }
}

/// <summary>
/// Result of clearing the database.
/// </summary>
public class ClearDatabaseResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    [Required]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of movies deleted.
    /// </summary>
    public int MoviesDeleted { get; set; }

    /// <summary>
    /// Gets or sets the number of series deleted.
    /// </summary>
    public int SeriesDeleted { get; set; }

    /// <summary>
    /// Gets or sets the number of channels deleted.
    /// </summary>
    public int ChannelsDeleted { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result.
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;
}
