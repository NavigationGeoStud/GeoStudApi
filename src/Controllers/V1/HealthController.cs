using Microsoft.AspNetCore.Mvc;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
            description = "API for collecting student survey data and analytics",
            version = "1.0.0",
            endpoints = new
            {
                authentication = "/api/v1/auth",
                survey = "/api/v1/survey",
                analytics = "/api/v1/analytics",
                users = "/api/v1/user"
            },
            features = new[]
            {
                "Student survey data collection",
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
