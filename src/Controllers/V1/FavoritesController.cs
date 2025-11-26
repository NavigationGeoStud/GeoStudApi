using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Tags("Private")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _favoritesService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(IFavoritesService favoritesService, ILogger<FavoritesController> logger)
    {
        _favoritesService = favoritesService;
        _logger = logger;
    }

    /// <summary>
    /// Get all favorite locations for current user
    /// </summary>
    /// <returns>List of favorite locations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FavoriteLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites()
    {
        try
        {
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorites = await _favoritesService.GetFavoritesAsync(userId.Value);
            return Ok(favorites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite locations");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Add location to favorites
    /// </summary>
    /// <param name="request">Favorite location data</param>
    /// <returns>Created favorite location</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FavoriteLocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteLocationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try multiple ways to get client_id claim
            var clientIdClaim = User.FindFirst("client_id")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            // Log all available claims for debugging
            var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            _logger.LogDebug("Available claims: {Claims}", string.Join(", ", allClaims));

            if (string.IsNullOrEmpty(clientIdClaim))
            {
                _logger.LogWarning("client_id claim not found in token. Available claims: {Claims}", string.Join(", ", allClaims));
                return Unauthorized("Invalid service token: client_id claim not found");
            }

            _logger.LogDebug("Found client_id: {ClientId}", clientIdClaim);

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                _logger.LogWarning("User not found for client_id: {ClientId}", clientIdClaim);
                return Unauthorized($"Invalid service token: user not found for client_id '{clientIdClaim}'");
            }

            _logger.LogDebug("Found user ID: {UserId} for client_id: {ClientId}", userId.Value, clientIdClaim);

            var response = await _favoritesService.AddFavoriteAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetFavorite), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding favorite location");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get favorite location by ID
    /// </summary>
    /// <param name="id">Favorite location ID</param>
    /// <returns>Favorite location details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FavoriteLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorite(int id)
    {
        try
        {
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorite = await _favoritesService.GetFavoriteByIdAsync(userId.Value, id);
            if (favorite == null)
            {
                return NotFound();
            }
            return Ok(favorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Update favorite location notes
    /// </summary>
    /// <param name="id">Favorite location ID</param>
    /// <param name="request">Updated favorite location data</param>
    /// <returns>Updated favorite location</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FavoriteLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateFavorite(int id, [FromBody] FavoriteLocationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var response = await _favoritesService.UpdateFavoriteAsync(userId.Value, id, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating favorite location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Remove location from favorites
    /// </summary>
    /// <param name="id">Favorite location ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        try
        {
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var deleted = await _favoritesService.RemoveFavoriteAsync(userId.Value, id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing favorite location {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Check if location is in favorites
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>True if location is in favorites</returns>
    [HttpGet("check/{locationId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckFavorite(int locationId)
    {
        try
        {
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var userId = await _favoritesService.GetUserIdFromClientIdAsync(clientIdClaim);
            if (userId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var isFavorite = await _favoritesService.CheckFavoriteAsync(userId.Value, locationId);
            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking favorite location {LocationId}", locationId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

