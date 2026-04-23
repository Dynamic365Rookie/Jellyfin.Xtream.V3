using System.ComponentModel.DataAnnotations;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
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
    private readonly ILogger<XtreamDeveloperController> _logger;

    public XtreamDeveloperController(
        IXtreamRepository<XtreamMovie> movieRepo,
        IXtreamRepository<XtreamSeries> seriesRepo,
        IXtreamRepository<XtreamChannel> channelRepo,
        ILogger<XtreamDeveloperController> logger)
    {
        _movieRepo = movieRepo;
        _seriesRepo = seriesRepo;
        _channelRepo = channelRepo;
        _logger = logger;
    }

    /// <summary>
    /// Get statistics about entities in the database.
    /// </summary>
    /// <returns>Entity counts.</returns>
    [HttpGet("Stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<DatabaseStats> GetStats()
    {
        _logger.LogInformation("[Xtream] Developer: Getting database stats");

        var stats = new DatabaseStats
        {
            MovieCount = _movieRepo.Count(),
            SeriesCount = _seriesRepo.Count(),
            ChannelCount = _channelRepo.Count()
        };

        return Ok(stats);
    }

    /// <summary>
    /// Clear all data from the LiteDB database.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [HttpPost("ClearDatabase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ClearDatabaseResult> ClearDatabase()
    {
        _logger.LogWarning("[Xtream] Developer: Clearing database - ALL DATA WILL BE DELETED");

        try
        {
            var moviesDeleted = _movieRepo.DeleteAll();
            var seriesDeleted = _seriesRepo.DeleteAll();
            var channelsDeleted = _channelRepo.DeleteAll();

            _logger.LogInformation(
                "[Xtream] Developer: Database cleared - {Movies} movies, {Series} series, {Channels} channels deleted",
                moviesDeleted, seriesDeleted, channelsDeleted);

            return Ok(new ClearDatabaseResult
            {
                Success = true,
                MoviesDeleted = moviesDeleted,
                SeriesDeleted = seriesDeleted,
                ChannelsDeleted = channelsDeleted,
                Message = $"Database cleared: {moviesDeleted} movies, {seriesDeleted} series, {channelsDeleted} channels deleted"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Xtream] Developer: Failed to clear database");
            return StatusCode(500, new ClearDatabaseResult
            {
                Success = false,
                Message = $"Failed to clear database: {ex.Message}"
            });
        }
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
