using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using GeoStud.Api.Services.Interfaces;
using RoleCheckResponse = GeoStud.Api.DTOs.User.RoleCheckResponse;
using AssignRoleRequest = GeoStud.Api.DTOs.User.AssignRoleRequest;

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
    private readonly IRoleService _roleService;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(
        IUserService userService,
        ILocationService locationService,
        IFavoritesService favoritesService,
        ICategoryService categoryService,
        IRoleService roleService,
        ILogger<TelegramController> logger)
    {
        _userService = userService;
        _locationService = locationService;
        _favoritesService = favoritesService;
        _categoryService = categoryService;
        _roleService = roleService;
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

            // If user has interests, try to match categories with user interests
            // Otherwise, return all categories
            List<CategoryResponse> resultCategories;
            
            if (user.Interests != null && user.Interests.Any())
            {
                // Match categories with user interests
                var matchedCategories = categoriesList
                    .Where(c => user.Interests.Any(i =>
                        c.Name.Contains(i, StringComparison.OrdinalIgnoreCase) ||
                        i.Contains(c.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                // If we found matching categories, return them
                // Otherwise, return all categories so user can see what's available
                resultCategories = matchedCategories.Any() 
                    ? matchedCategories 
                    : categoriesList;
            }
            else
            {
                // No interests, return all categories
                resultCategories = categoriesList;
            }

            return Ok(resultCategories);
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
    /// <param name="telegramId">Telegram user ID</param>
    /// <returns>Created favorite location response</returns>
    [HttpPost("favorites")]
    [ProducesResponseType(typeof(FavoriteLocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteLocationRequest request, [FromQuery] long telegramId)
    {
        try
        {
            _logger.LogDebug("AddFavorite called with telegramId: {TelegramId}, LocationId: {LocationId}", telegramId, request?.LocationId);

            // Validate telegramId is provided and not default value
            if (telegramId == 0)
            {
                _logger.LogWarning("telegramId is missing or zero. Query string parameters: {QueryString}", Request.QueryString);
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var userId = await _favoritesService.GetUserIdFromTelegramIdAsync(telegramId);
            if (userId == null)
            {
                _logger.LogWarning("User not found for TelegramId: {TelegramId}", telegramId);
                return NotFound(new { error = $"User not found for TelegramId: {telegramId}. Please submit user profile first." });
            }

            _logger.LogDebug("Found userId: {UserId} for TelegramId: {TelegramId}, adding location {LocationId} to favorites", userId.Value, telegramId, request.LocationId);

            var response = await _favoritesService.AddFavoriteAsync(userId.Value, request);
            
            _logger.LogInformation("Successfully added location {LocationId} to favorites for user {UserId}", request.LocationId, userId.Value);
            
            return CreatedAtAction(nameof(GetFavorites), new { telegramId }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict adding favorite: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error adding favorite: {Message}", ex.Message);
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
    /// <param name="telegramId">Telegram user ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("favorites/{locationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(int locationId, [FromQuery] long telegramId)
    {
        try
        {
            // Validate telegramId is provided and not default value
            if (telegramId == 0)
            {
                _logger.LogWarning("telegramId is missing or zero in RemoveFavorite. Query string parameters: {QueryString}", Request.QueryString);
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            var userId = await _favoritesService.GetUserIdFromTelegramIdAsync(telegramId);
            if (userId == null)
            {
                return NotFound(new { error = "User not found" });
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
    /// Get recommended locations by category ID with pagination, or all locations if categoryId is not provided
    /// </summary>
    /// <param name="categoryId">Category ID to filter locations (optional - if not provided, returns all locations)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of locations in category or all locations</returns>
    [HttpGet("locations/recommended")]
    [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecommendedLocationsByCategory([FromQuery] int? categoryId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            IEnumerable<LocationResponse> allLocations;
            
            // If categoryId is provided, get locations by category
            // Otherwise, get all locations
            if (categoryId.HasValue)
            {
                allLocations = await _locationService.GetLocationsByCategoryAsync(categoryId.Value);
            }
            else
            {
                allLocations = await _locationService.GetLocationsAsync();
            }
            
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

    /// <summary>
    /// Check user role by Username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User role information</returns>
    [HttpGet("role")]
    [ProducesResponseType(typeof(RoleCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRole([FromQuery] string username)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { error = "username query parameter is required" });
            }

            var roleResponse = await _roleService.GetUserRoleByUsernameAsync(username);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(roleResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user role");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Assign role to user (only Admin can assign roles)
    /// </summary>
    /// <param name="request">Role assignment request with adminUsername and target Username</param>
    /// <returns>Success status</returns>
    [HttpPost("role/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, [FromQuery] string adminUsername)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(adminUsername))
            {
                return BadRequest(new { error = "adminUsername query parameter is required" });
            }

            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { error = "Username is required in request body" });
            }

            // Verify admin has permission
            var isAdmin = await _roleService.IsUserAdminByUsernameAsync(adminUsername);
            if (!isAdmin)
            {
                _logger.LogWarning("Unauthorized role assignment attempt. Username: {Username} is not an admin", adminUsername);
                return StatusCode(403, new { error = "Only administrators can assign roles" });
            }

            var success = await _roleService.AssignRoleAsync(adminUsername, request);
            if (!success)
            {
                return NotFound(new { error = "Failed to assign role. Target user may not exist or role is invalid." });
            }

            return Ok(new { message = "Role assigned successfully", username = request.Username, role = request.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role");
            return StatusCode(500, "Internal server error");
        }
    }
}

