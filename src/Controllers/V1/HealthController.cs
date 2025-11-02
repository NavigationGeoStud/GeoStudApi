using Microsoft.AspNetCore.Mvc;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("General")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            service = "GeoStud API"
        });
    }

    /// <summary>
    /// Get API information
    /// </summary>
    /// <returns>API information</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            name = "GeoStud API",
            description = "API for collecting user data and analytics",
            version = "1.0.0",
            endpoints = new
            {
                authentication = "/api/v1/auth",
                users = "/api/v1/user",
                analytics = "/api/v1/analytics",
                telegram = "/api/v1/telegram"
            },
            features = new[]
            {
                "User data collection",
                "Demographics analytics",
                "Interests analysis",
                "Behavior analytics",
                "JWT authentication",
                "API versioning",
                "Swagger documentation"
            }
        });
    }
}
