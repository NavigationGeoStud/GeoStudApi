using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Analytics;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Tags("Private")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive analytics data
    /// </summary>
    /// <returns>Complete analytics report</returns>
    [HttpGet("comprehensive")]
    [ProducesResponseType(typeof(ComprehensiveAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComprehensiveAnalytics()
    {
        try
        {
            var analytics = await _analyticsService.GetComprehensiveAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get demographics analytics
    /// </summary>
    /// <returns>Demographics analytics</returns>
    [HttpGet("demographics")]
    [ProducesResponseType(typeof(DemographicsAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDemographicsAnalyticsEndpoint()
    {
        try
        {
            var analytics = await _analyticsService.GetDemographicsAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving demographics analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get interests analytics
    /// </summary>
    /// <returns>Interests analytics</returns>
    [HttpGet("interests")]
    [ProducesResponseType(typeof(InterestsAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInterestsAnalyticsEndpoint()
    {
        try
        {
            var analytics = await _analyticsService.GetInterestsAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interests analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get behavior analytics
    /// </summary>
    /// <returns>Behavior analytics</returns>
    [HttpGet("behavior")]
    [ProducesResponseType(typeof(BehaviorAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBehaviorAnalyticsEndpoint()
    {
        try
        {
            var analytics = await _analyticsService.GetBehaviorAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving behavior analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get cached analytics data
    /// </summary>
    /// <param name="category">Analytics category</param>
    /// <returns>Cached analytics data</returns>
    [HttpGet("cached/{category}")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCachedAnalytics(string category)
    {
        try
        {
            var cachedData = await _analyticsService.GetCachedAnalyticsAsync(category);
            return Ok(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached analytics for category {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }
}
