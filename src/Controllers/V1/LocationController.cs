using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Tags("Private")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all locations
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="subcategoryId">Optional subcategory filter</param>
    /// <param name="city">Optional city filter</param>
    /// <returns>List of locations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLocations([FromQuery] int? categoryId = null, [FromQuery] int? subcategoryId = null, [FromQuery] string? city = null)
    {
        try
        {
            var locations = await _locationService.GetLocationsAsync(categoryId, subcategoryId, city);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>Location details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLocation(int id)
    {
        try
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return Ok(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get all locations by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of locations in the specified category</returns>
    [HttpGet("by-category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLocationsByCategory(int categoryId)
    {
        try
        {
            var locations = await _locationService.GetLocationsByCategoryAsync(categoryId);
            return Ok(locations);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations for category {CategoryId}", categoryId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Create new location
    /// </summary>
    /// <param name="request">Location data</param>
    /// <returns>Created location</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateLocation([FromBody] LocationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var location = await _locationService.CreateLocationAsync(request);
            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing location
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="request">Updated location data</param>
    /// <returns>Updated location</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var location = await _locationService.UpdateLocationAsync(id, request);
            return Ok(location);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete location (soft delete)
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        try
        {
            var deleted = await _locationService.DeleteLocationAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get location categories
    /// </summary>
    /// <returns>List of location categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _locationService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location categories");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get nearby locations by coordinates
    /// </summary>
    /// <param name="coordinates">Coordinates in format "latitude,longitude"</param>
    /// <param name="radiusKm">Radius in kilometers (default: 5)</param>
    /// <returns>List of nearby locations</returns>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNearbyLocations([FromQuery] string coordinates, [FromQuery] double radiusKm = 5)
    {
        try
        {
            var locations = await _locationService.GetNearbyLocationsAsync(coordinates, radiusKm);
            return Ok(locations);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nearby locations");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

