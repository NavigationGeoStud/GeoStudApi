using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Models;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(GeoStudDbContext context, ILogger<FavoritesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<int?> GetStudentIdFromContext()
    {
        // Get service client ID from token
        var clientIdClaim = User.FindFirst("client_id")?.Value;
        if (string.IsNullOrEmpty(clientIdClaim))
        {
            return null;
        }

        var username = $"service_{clientIdClaim}";
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Username == username);

        return student?.Id;
    }

    /// <summary>
    /// Get all favorite locations for current student
    /// </summary>
    /// <returns>List of favorite locations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FavoriteLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites()
    {
        try
        {
            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorites = await _context.FavoriteLocations
                .Include(f => f.Location)
                .Where(f => f.StudentId == studentId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var responses = favorites.Select(f => new FavoriteLocationResponse
            {
                Id = f.Id,
                StudentId = f.StudentId,
                LocationId = f.LocationId,
                Notes = f.Notes,
                CreatedAt = f.CreatedAt,
                LocationName = f.Location.Name,
                LocationDescription = f.Location.Description,
                Coordinates = f.Location.Coordinates,
                Address = f.Location.Address,
                City = f.Location.City,
                Rating = f.Location.Rating,
                ImageUrl = f.Location.ImageUrl
            }).ToList();

            return Ok(responses);
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

            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            // Check if location exists
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId && !l.IsDeleted);

            if (location == null)
            {
                return NotFound(new { error = "Location not found" });
            }

            // Check if already in favorites
            var existingFavorite = await _context.FavoriteLocations
                .FirstOrDefaultAsync(f => f.StudentId == studentId && f.LocationId == request.LocationId && !f.IsDeleted);

            if (existingFavorite != null)
            {
                return BadRequest(new { error = "Location is already in favorites" });
            }

            // Create favorite
            var favorite = new FavoriteLocation
            {
                StudentId = studentId.Value,
                LocationId = request.LocationId,
                Notes = request.Notes
            };

            _context.FavoriteLocations.Add(favorite);
            await _context.SaveChangesAsync();

            var response = new FavoriteLocationResponse
            {
                Id = favorite.Id,
                StudentId = favorite.StudentId,
                LocationId = favorite.LocationId,
                Notes = favorite.Notes,
                CreatedAt = favorite.CreatedAt,
                LocationName = location.Name,
                LocationDescription = location.Description,
                Coordinates = location.Coordinates,
                Address = location.Address,
                City = location.City,
                Rating = location.Rating,
                ImageUrl = location.ImageUrl
            };

            return CreatedAtAction(nameof(GetFavorite), new { id = favorite.Id }, response);
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
            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorite = await _context.FavoriteLocations
                .Include(f => f.Location)
                .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == studentId && !f.IsDeleted);

            if (favorite == null)
            {
                return NotFound();
            }

            var response = new FavoriteLocationResponse
            {
                Id = favorite.Id,
                StudentId = favorite.StudentId,
                LocationId = favorite.LocationId,
                Notes = favorite.Notes,
                CreatedAt = favorite.CreatedAt,
                LocationName = favorite.Location.Name,
                LocationDescription = favorite.Location.Description,
                Coordinates = favorite.Location.Coordinates,
                Address = favorite.Location.Address,
                City = favorite.Location.City,
                Rating = favorite.Location.Rating,
                ImageUrl = favorite.Location.ImageUrl
            };

            return Ok(response);
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

            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorite = await _context.FavoriteLocations
                .Include(f => f.Location)
                .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == studentId && !f.IsDeleted);

            if (favorite == null)
            {
                return NotFound();
            }

            favorite.Notes = request.Notes;
            await _context.SaveChangesAsync();

            var response = new FavoriteLocationResponse
            {
                Id = favorite.Id,
                StudentId = favorite.StudentId,
                LocationId = favorite.LocationId,
                Notes = favorite.Notes,
                CreatedAt = favorite.CreatedAt,
                LocationName = favorite.Location.Name,
                LocationDescription = favorite.Location.Description,
                Coordinates = favorite.Location.Coordinates,
                Address = favorite.Location.Address,
                City = favorite.Location.City,
                Rating = favorite.Location.Rating,
                ImageUrl = favorite.Location.ImageUrl
            };

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
            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var favorite = await _context.FavoriteLocations
                .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == studentId && !f.IsDeleted);

            if (favorite == null)
            {
                return NotFound();
            }

            favorite.IsDeleted = true;
            await _context.SaveChangesAsync();

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
            var studentId = await GetStudentIdFromContext();
            if (studentId == null)
            {
                return Unauthorized("Invalid service token");
            }

            var isFavorite = await _context.FavoriteLocations
                .AnyAsync(f => f.StudentId == studentId && f.LocationId == locationId && !f.IsDeleted);

            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking favorite location {LocationId}", locationId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

