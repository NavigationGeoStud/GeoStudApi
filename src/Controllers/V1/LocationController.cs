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
public class LocationController : ControllerBase
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<LocationController> _logger;

    public LocationController(GeoStudDbContext context, ILogger<LocationController> logger)
    {
        _context = context;
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
            var query = _context.Locations.AsQueryable();

            // Filter by category if provided
            if (categoryId.HasValue)
            {
                query = query.Where(l => l.CategoryId == categoryId.Value);
            }

            // Filter by subcategory if provided
            if (subcategoryId.HasValue)
            {
                query = query.Where(l => l.SubcategoryJoins.Any(sj => sj.SubcategoryId == subcategoryId.Value));
            }

            // Filter by city if provided
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(l => l.City == city);
            }

            var locations = await query
                .Include(l => l.Category)
                .Include(l => l.SubcategoryJoins)
                    .ThenInclude(sj => sj.Subcategory)
                .ToListAsync();

            var responses = locations.Select(ToLocationResponse).ToList();
            return Ok(responses);
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
            var location = await _context.Locations
                .Include(l => l.Category)
                .Include(l => l.SubcategoryJoins)
                    .ThenInclude(sj => sj.Subcategory)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(ToLocationResponse(location));
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
            // Check if category exists
            var category = await _context.LocationCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted);

            if (category == null)
            {
                return NotFound(new { error = "Category not found" });
            }

            var locations = await _context.Locations
                .Where(l => l.CategoryId == categoryId && !l.IsDeleted)
                .Include(l => l.Category)
                .Include(l => l.SubcategoryJoins)
                    .ThenInclude(sj => sj.Subcategory)
                .ToListAsync();

            var responses = locations.Select(ToLocationResponse).ToList();
            return Ok(responses);
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

            // Validate coordinates format
            if (!LocationCoordinatesHelper.IsValidCoordinates(request.Coordinates))
            {
                return BadRequest(new { error = "Invalid coordinates format. Expected format: 'latitude,longitude'" });
            }

            // Validate category exists and is active
            var category = await _context.LocationCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsDeleted);

            if (category == null)
            {
                return BadRequest(new { error = "Category not found or inactive" });
            }

            // Validate subcategories belong to the category
            if (request.SubcategoryIds.Any())
            {
                var invalidSubcategories = await _context.LocationSubcategories
                    .Where(s => request.SubcategoryIds.Contains(s.Id) && (s.CategoryId != request.CategoryId || !s.IsActive))
                    .ToListAsync();

                if (invalidSubcategories.Any())
                {
                    return BadRequest(new { error = "Some subcategories do not belong to the selected category or are inactive" });
                }
            }

            var location = new Location
            {
                Name = request.Name,
                Description = request.Description,
                Coordinates = request.Coordinates,
                Address = request.Address,
                City = request.City,
                Phone = request.Phone,
                Website = request.Website,
                ImageUrl = request.ImageUrl,
                Rating = request.Rating,
                RatingCount = request.RatingCount,
                PriceRange = request.PriceRange,
                WorkingHours = request.WorkingHours,
                IsActive = request.IsActive,
                IsVerified = request.IsVerified,
                CategoryId = request.CategoryId
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            // Add subcategory associations
            if (request.SubcategoryIds.Any())
            {
                var validSubcategories = await _context.LocationSubcategories
                    .Where(s => request.SubcategoryIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                var subcategoryJoins = validSubcategories.Select(s => new LocationSubcategoryJoin
                {
                    LocationId = location.Id,
                    SubcategoryId = s.Id
                }).ToList();

                _context.LocationSubcategoryJoins.AddRange(subcategoryJoins);
                await _context.SaveChangesAsync();
            }

            // Reload with category and subcategories
            await _context.Entry(location)
                .Reference(l => l.Category)
                .LoadAsync();
                
            await _context.Entry(location)
                .Collection(l => l.SubcategoryJoins)
                .Query()
                .Include(sj => sj.Subcategory)
                .LoadAsync();

            var response = ToLocationResponse(location);
            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, response);
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

            // Validate coordinates format
            if (!LocationCoordinatesHelper.IsValidCoordinates(request.Coordinates))
            {
                return BadRequest(new { error = "Invalid coordinates format. Expected format: 'latitude,longitude'" });
            }

            // Validate category exists and is active
            var category = await _context.LocationCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsDeleted);

            if (category == null)
            {
                return BadRequest(new { error = "Category not found or inactive" });
            }

            var location = await _context.Locations
                .Include(l => l.SubcategoryJoins)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (location == null)
            {
                return NotFound();
            }

            // Validate subcategories belong to the category
            if (request.SubcategoryIds.Any())
            {
                var invalidSubcategories = await _context.LocationSubcategories
                    .Where(s => request.SubcategoryIds.Contains(s.Id) && (s.CategoryId != request.CategoryId || !s.IsActive))
                    .ToListAsync();

                if (invalidSubcategories.Any())
                {
                    return BadRequest(new { error = "Some subcategories do not belong to the selected category or are inactive" });
                }
            }

            // Update location properties
            location.Name = request.Name;
            location.Description = request.Description;
            location.Coordinates = request.Coordinates;
            location.Address = request.Address;
            location.City = request.City;
            location.Phone = request.Phone;
            location.Website = request.Website;
            location.ImageUrl = request.ImageUrl;
            location.Rating = request.Rating;
            location.RatingCount = request.RatingCount;
            location.PriceRange = request.PriceRange;
            location.WorkingHours = request.WorkingHours;
            location.IsActive = request.IsActive;
            location.IsVerified = request.IsVerified;
            location.CategoryId = request.CategoryId;

            // Update subcategory associations
            var existingSubcategoryIds = location.SubcategoryJoins.Select(sj => sj.SubcategoryId).ToList();
            var newSubcategoryIds = request.SubcategoryIds.ToList();

            // Remove old subcategory associations
            var subcategoriesToRemove = location.SubcategoryJoins
                .Where(sj => !newSubcategoryIds.Contains(sj.SubcategoryId))
                .ToList();
            foreach (var join in subcategoriesToRemove)
            {
                _context.LocationSubcategoryJoins.Remove(join);
            }

            // Add new subcategory associations
            var subcategoriesToAdd = newSubcategoryIds
                .Where(sid => !existingSubcategoryIds.Contains(sid))
                .ToList();

            if (subcategoriesToAdd.Any())
            {
                var validSubcategories = await _context.LocationSubcategories
                    .Where(s => subcategoriesToAdd.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                var newSubcategoryJoins = validSubcategories.Select(s => new LocationSubcategoryJoin
                {
                    LocationId = location.Id,
                    SubcategoryId = s.Id
                }).ToList();

                _context.LocationSubcategoryJoins.AddRange(newSubcategoryJoins);
            }

            await _context.SaveChangesAsync();

            // Reload with category and subcategories
            await _context.Entry(location)
                .Reference(l => l.Category)
                .LoadAsync();
                
            await _context.Entry(location)
                .Collection(l => l.SubcategoryJoins)
                .Query()
                .Include(sj => sj.Subcategory)
                .LoadAsync();

            var response = ToLocationResponse(location);
            return Ok(response);
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
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (location == null)
            {
                return NotFound();
            }

            location.IsDeleted = true;
            location.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
            var categories = await _context.LocationCategories
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var responses = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                IconName = c.IconName,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(responses);
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
            var (lat, lng) = LocationCoordinatesHelper.ParseCoordinates(coordinates);
            if (!lat.HasValue || !lng.HasValue)
            {
                return BadRequest(new { error = "Invalid coordinates format. Expected format: 'latitude,longitude'" });
            }

            var locations = await _context.Locations
                .Include(l => l.Category)
                .Include(l => l.SubcategoryJoins)
                    .ThenInclude(sj => sj.Subcategory)
                .ToListAsync();

            var nearby = locations.Where(l =>
            {
                var (locLat, locLng) = LocationCoordinatesHelper.ParseCoordinates(l.Coordinates);
                if (!locLat.HasValue || !locLng.HasValue)
                    return false;

                var distance = CalculateDistance(
                    lat.Value, lng.Value,
                    locLat.Value, locLng.Value
                );

                return distance <= radiusKm;
            }).ToList();

            var responses = nearby.Select(ToLocationResponse).ToList();
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nearby locations");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private static LocationResponse ToLocationResponse(Location location)
    {
        return new LocationResponse
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Coordinates = location.Coordinates,
            Address = location.Address,
            City = location.City,
            Phone = location.Phone,
            Website = location.Website,
            ImageUrl = location.ImageUrl,
            Rating = location.Rating,
            RatingCount = location.RatingCount,
            PriceRange = location.PriceRange,
            WorkingHours = location.WorkingHours,
            IsActive = location.IsActive,
            IsVerified = location.IsVerified,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            Category = new LocationResponse.CategoryInfo
            {
                Id = location.Category.Id,
                Name = location.Category.Name,
                IconName = location.Category.IconName
            },
            Subcategories = location.SubcategoryJoins
                .Select(sj => new LocationResponse.SubcategoryInfo
                {
                    Id = sj.Subcategory.Id,
                    Name = sj.Subcategory.Name,
                    CategoryId = sj.Subcategory.CategoryId
                })
                .ToList()
        };
    }

    private static double CalculateDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians((double)(lat2 - lat1));
        var dLng = ToRadians((double)(lng2 - lng1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}

