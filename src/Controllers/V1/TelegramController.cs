using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Tags("Telegram")]
public class TelegramController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILocationService _locationService;
    private readonly IFavoritesService _favoritesService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(
        IUserService userService,
        ILocationService locationService,
        IFavoritesService favoritesService,
        ICategoryService categoryService,
        ILogger<TelegramController> logger)
    {
        _userService = userService;
        _locationService = locationService;
        _favoritesService = favoritesService;
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Submit user for telegram user
    /// </summary>
    /// <param name="request">User data with Telegram ID</param>
    /// <returns>Created user response</returns>
    [HttpPost("user")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitUser([FromBody] UserRequest request)
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

            var response = await _userService.SubmitUserAsync(clientIdClaim, request);
            return CreatedAtAction(nameof(GetUser), null, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting user");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Update user for telegram user (partial update)
    /// </summary>
    /// <param name="request">User data to update (all fields optional)</param>
    /// <returns>Updated user response</returns>
    [HttpPut("user")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
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

            var response = await _userService.UpdateUserAsync(clientIdClaim, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get user for telegram user
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <returns>User response</returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromQuery] long telegramId)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get categories that match user interests
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <returns>List of categories matching user interests</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserCategories([FromQuery] long telegramId)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found. Please submit a user profile first." });
            }

            // Get all categories
            var allCategories = await _categoryService.GetCategoriesAsync();
            var categoriesList = allCategories.ToList();

            // Match categories with user interests
            var matchedCategories = categoriesList
                .Where(c => user.Interests.Any(i =>
                    c.Name.Contains(i, StringComparison.OrdinalIgnoreCase) ||
                    i.Contains(c.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Ok(matchedCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add location to favorites
    /// </summary>
    /// <param name="request">Favorite location request</param>
    /// <returns>Created favorite location response</returns>
    [HttpPost("favorites")]
    [ProducesResponseType(typeof(FavoriteLocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteLocationRequest request)
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

            var response = await _favoritesService.AddFavoriteAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetFavorites), null, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding favorite");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Remove location from favorites
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("favorites/{locationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(int locationId)
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

            var removed = await _favoritesService.RemoveFavoriteByLocationIdAsync(userId.Value, locationId);
            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing favorite");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all favorite locations for user
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <returns>List of favorite locations</returns>
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(IEnumerable<FavoriteLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFavorites([FromQuery] long telegramId)
    {
        try
        {
            var userId = await _favoritesService.GetUserIdFromTelegramIdAsync(telegramId);
            if (userId == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var favorites = await _favoritesService.GetFavoritesAsync(userId.Value);
            return Ok(favorites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorites");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get nearby locations by coordinates
    /// </summary>
    /// <param name="coordinates">Coordinates in format "latitude,longitude"</param>
    /// <param name="radiusKm">Search radius in kilometers (default: 5)</param>
    /// <returns>List of nearby locations</returns>
    [HttpGet("locations/nearby")]
    [ProducesResponseType(typeof(IEnumerable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNearbyLocations([FromQuery] string coordinates, [FromQuery] double radiusKm = 5)
    {
        try
        {
            if (string.IsNullOrEmpty(coordinates))
            {
                return BadRequest(new { error = "Coordinates are required" });
            }

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
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get recommended locations by category ID with pagination
    /// </summary>
    /// <param name="categoryId">Category ID to filter locations</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of locations in category</returns>
    [HttpGet("locations/recommended")]
    [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecommendedLocationsByCategory([FromQuery] int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var allLocations = await _locationService.GetLocationsByCategoryAsync(categoryId);
            var locationsList = allLocations.ToList();
            
            var totalCount = locationsList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedLocations = locationsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PagedResponse<LocationResponse>
            {
                Data = pagedLocations,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommended locations by category");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all locations for telegram user with pagination
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of all locations</returns>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllLocations([FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var allLocations = await _locationService.GetLocationsAsync();
            var locationsList = allLocations.ToList();
            
            var totalCount = locationsList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedLocations = locationsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PagedResponse<LocationResponse>
            {
                Data = pagedLocations,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all locations");
            return StatusCode(500, "Internal server error");
        }
    }
}

