using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.User;
using GeoStud.Api.DTOs.People;
using GeoStud.Api.DTOs.Notification;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using MarkNotificationAsReadRequest = GeoStud.Api.DTOs.Notification.MarkNotificationAsReadRequest;
using GeoStud.Api.Services.Interfaces;
using RoleCheckResponse = GeoStud.Api.DTOs.User.RoleCheckResponse;
using AssignRoleRequest = GeoStud.Api.DTOs.User.AssignRoleRequest;
using UpdateRoleRequest = GeoStud.Api.DTOs.User.UpdateRoleRequest;
using CreateLocationTelegramRequest = GeoStud.Api.DTOs.Location.CreateLocationTelegramRequest;
using UpdateLocationModerationRequest = GeoStud.Api.DTOs.Location.UpdateLocationModerationRequest;
using GeoStud.Api.Models;

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
    private readonly IPeopleService _peopleService;
    private readonly INotificationService _notificationService;
    private readonly ILocationSuggestionService _locationSuggestionService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(
        IUserService userService,
        ILocationService locationService,
        IFavoritesService favoritesService,
        ICategoryService categoryService,
        IRoleService roleService,
        IPeopleService peopleService,
        INotificationService notificationService,
        ILocationSuggestionService locationSuggestionService,
        IWebhookService webhookService,
        ILogger<TelegramController> logger)
    {
        _userService = userService;
        _locationService = locationService;
        _favoritesService = favoritesService;
        _categoryService = categoryService;
        _roleService = roleService;
        _peopleService = peopleService;
        _notificationService = notificationService;
        _locationSuggestionService = locationSuggestionService;
        _webhookService = webhookService;
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
    /// <param name="request">User data to update (all fields optional, must include telegramId)</param>
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

            // For Telegram users, update by telegramId from request
            if (!request.TelegramId.HasValue)
            {
                return BadRequest(new { error = "telegramId is required" });
            }

            var response = await _userService.UpdateUserByTelegramIdAsync(request.TelegramId.Value, request);
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

            _logger.LogDebug("Found userId: {UserId} for TelegramId: {TelegramId}, adding location {LocationId} to favorites", userId.Value, telegramId, request?.LocationId ?? 0);

            var response = await _favoritesService.AddFavoriteAsync(userId.Value, request!);
            
            _logger.LogInformation("Successfully added location {LocationId} to favorites for user {UserId}", request!.LocationId, userId.Value);
            
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

    /// <summary>
    /// Update user role (only Admin can update roles)
    /// </summary>
    /// <param name="username">Username of the user whose role should be updated</param>
    /// <param name="request">Role update request with new role</param>
    /// <param name="adminUsername">Username of the admin making the request</param>
    /// <returns>Success status</returns>
    [HttpPut("role/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(string username, [FromBody] UpdateRoleRequest request, [FromQuery] string adminUsername)
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

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { error = "Username is required in route" });
            }

            // Verify admin has permission
            var isAdmin = await _roleService.IsUserAdminByUsernameAsync(adminUsername);
            if (!isAdmin)
            {
                _logger.LogWarning("Unauthorized role update attempt. Username: {Username} is not an admin", adminUsername);
                return StatusCode(403, new { error = "Only administrators can update roles" });
            }

            // Create AssignRoleRequest from route and body
            var assignRoleRequest = new AssignRoleRequest
            {
                Username = username,
                Role = request.Role
            };

            var success = await _roleService.AssignRoleAsync(adminUsername, assignRoleRequest);
            if (!success)
            {
                return NotFound(new { error = "Failed to update role. Target user may not exist or role is invalid." });
            }

            return Ok(new { message = "Role updated successfully", username = username, role = request.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create location (only Admin or Manager can create locations)
    /// </summary>
    /// <param name="request">Location creation request</param>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <returns>Created location response</returns>
    [HttpPost("location")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationTelegramRequest request, [FromQuery] long telegramId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            // Check user role - only Admin (2) or Manager (1) can create locations
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized location creation attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can create locations" });
            }

            var location = await _locationService.CreateLocationFromTelegramAsync(request);
            return CreatedAtAction(nameof(GetAllLocations), new { telegramId }, location);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid location creation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get companies (users) by location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of users who favorited this location</returns>
    [HttpGet("locations/{locationId}/companies")]
    [ProducesResponseType(typeof(PagedResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompaniesByLocation(int locationId, [FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _peopleService.GetCompaniesByLocationAsync(locationId, telegramId, page, pageSize);
            
            if (response.TotalCount == 0)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies by location");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search people with common favorite locations
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of users with common favorite locations</returns>
    [HttpGet("people/search")]
    [ProducesResponseType(typeof(PagedResponse<UserProfileWithLocationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchPeople([FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _peopleService.SearchPeopleAsync(telegramId, page, pageSize);
            
            if (response.TotalCount == 0)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching people");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search people by locations (users with common favorite locations and filled profiles)
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of users with common favorite locations and filled profiles</returns>
    [HttpGet("people/by-locations")]
    [ProducesResponseType(typeof(PagedResponse<UserProfileWithLocationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchPeopleByLocations([FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _peopleService.SearchPeopleByLocationsAsync(telegramId, page, pageSize);
            
            if (response.TotalCount == 0)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching people by locations");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Like a user (swipe right)
    /// </summary>
    /// <param name="request">Like request with telegramId and targetTelegramId</param>
    /// <returns>Like response with match information</returns>
    [HttpPost("people/like")]
    [ProducesResponseType(typeof(LikeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeUser([FromBody] LikeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _peopleService.LikeUserAsync(request.TelegramId, request.TargetTelegramId, request.Message);
            
            if (response.IsMatch)
            {
                return StatusCode(201, response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid like request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Dislike a user (swipe left / skip)
    /// </summary>
    /// <param name="request">Dislike request with telegramId and targetTelegramId</param>
    /// <returns>Success status</returns>
    [HttpPost("people/dislike")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DislikeUser([FromBody] DislikeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _peopleService.DislikeUserAsync(request.TelegramId, request.TargetTelegramId);
            
            if (success)
            {
                return StatusCode(201, new { success = true });
            }

            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid dislike request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disliking user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get notifications for user
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="unreadOnly">Show only unread notifications (default: false)</param>
    /// <returns>List of notifications</returns>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications([FromQuery] long telegramId, [FromQuery] bool unreadOnly = false)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            var notifications = await _notificationService.GetNotificationsAsync(telegramId, unreadOnly);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <param name="request">Request with telegramId</param>
    /// <returns>Success response</returns>
    [HttpPost("notifications/{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationAsRead(int notificationId, [FromBody] MarkNotificationAsReadRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.TelegramId == 0)
            {
                return BadRequest(new { error = "telegramId is required and must be a valid Telegram user ID" });
            }

            var success = await _notificationService.MarkNotificationAsReadAsync(notificationId, request.TelegramId);
            if (!success)
            {
                return NotFound(new { error = "Notification not found" });
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get location suggestions for user
    /// </summary>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of location suggestions</returns>
    [HttpGet("locations/suggestions")]
    [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationSuggestions([FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _locationSuggestionService.GetLocationSuggestionsAsync(telegramId, page, pageSize);
            
            if (response.TotalCount == 0)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location suggestions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Accept location suggestion
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="request">Request with optional notificationId</param>
    /// <returns>Success response</returns>
    [HttpPost("locations/suggestions/{locationId}/accept")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptLocationSuggestion(int locationId, [FromQuery] long telegramId, [FromBody] AcceptLocationSuggestionRequest? request = null)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            var response = await _locationSuggestionService.AcceptLocationSuggestionAsync(
                locationId, 
                telegramId, 
                request?.NotificationId);

            return StatusCode(201, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid accept location suggestion request");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Location already in favorites");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting location suggestion");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reject location suggestion
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram user ID</param>
    /// <param name="request">Request with optional notificationId</param>
    /// <returns>Success response</returns>
    [HttpPost("locations/suggestions/{locationId}/reject")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectLocationSuggestion(int locationId, [FromQuery] long telegramId, [FromBody] RejectLocationSuggestionRequest? request = null)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            var response = await _locationSuggestionService.RejectLocationSuggestionAsync(
                locationId, 
                telegramId, 
                request?.NotificationId);

            return StatusCode(201, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid reject location suggestion request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting location suggestion");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Configure webhook URL for notifications
    /// </summary>
    /// <param name="request">Webhook configuration request</param>
    /// <returns>Configuration response</returns>
    [HttpPost("webhook/configure")]
    [ProducesResponseType(typeof(ConfigureWebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfigureWebhook([FromBody] ConfigureWebhookRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _webhookService.ConfigureWebhookAsync(request.WebhookUrl, request.Secret);
            
            if (!success)
            {
                return BadRequest(new { error = "Invalid webhook URL. Must be a valid HTTP or HTTPS URL." });
            }

            var response = new ConfigureWebhookResponse
            {
                Success = true,
                Message = "Webhook configured successfully",
                WebhookUrl = _webhookService.GetWebhookUrl()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get current webhook configuration
    /// </summary>
    /// <returns>Current webhook URL</returns>
    [HttpGet("webhook")]
    [ProducesResponseType(typeof(ConfigureWebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetWebhook()
    {
        try
        {
            var webhookUrl = _webhookService.GetWebhookUrl();
            
            var response = new ConfigureWebhookResponse
            {
                Success = true,
                Message = string.IsNullOrEmpty(webhookUrl) ? "Webhook is not configured" : "Webhook is configured",
                WebhookUrl = webhookUrl
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook configuration");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get list of locations for moderation (only Manager or Admin)
    /// </summary>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of locations requiring moderation</returns>
    [HttpGet("locations/moderation")]
    [ProducesResponseType(typeof(PagedResponse<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLocationsForModeration([FromQuery] long telegramId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            // Check user role - only Admin (2) or Manager (1) can access moderation
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized moderation access attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can access moderation" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _locationService.GetLocationsForModerationAsync(page, pageSize);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations for moderation");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get location by ID (only Manager or Admin)
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <returns>Location response</returns>
    [HttpGet("locations/{locationId}")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationById(int locationId, [FromQuery] long telegramId)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            // Check user role - only Admin (2) or Manager (1) can access moderation
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized moderation access attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can access moderation" });
            }

            var location = await _locationService.GetLocationByIdAsync(locationId);
            if (location == null)
            {
                return NotFound(new { error = "Location not found" });
            }

            return Ok(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location by ID");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update location during moderation (only Manager or Admin)
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="request">Location update request (all fields optional)</param>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <returns>Updated location response</returns>
    [HttpPut("locations/{locationId}/moderation")]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocationForModeration(int locationId, [FromBody] UpdateLocationModerationRequest request, [FromQuery] long telegramId)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check user role - only Admin (2) or Manager (1) can access moderation
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized moderation access attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can access moderation" });
            }

            var location = await _locationService.UpdateLocationForModerationAsync(locationId, request);
            if (location == null)
            {
                return NotFound(new { error = "Location not found" });
            }

            return Ok(location);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid location update request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location for moderation");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Approve location (only Manager or Admin)
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <returns>Success status</returns>
    [HttpPost("locations/{locationId}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLocation(int locationId, [FromQuery] long telegramId)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            // Check user role - only Admin (2) or Manager (1) can access moderation
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized moderation access attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can access moderation" });
            }

            var success = await _locationService.ApproveLocationAsync(locationId);
            if (!success)
            {
                return NotFound(new { error = "Location not found" });
            }

            return Ok(new { message = "Location approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving location");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete location during moderation (only Manager or Admin)
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="telegramId">Telegram ID of the user making the request</param>
    /// <returns>Success status</returns>
    [HttpDelete("locations/{locationId}/moderation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLocationForModeration(int locationId, [FromQuery] long telegramId)
    {
        try
        {
            if (telegramId == 0)
            {
                return BadRequest(new { error = "telegramId query parameter is required and must be a valid Telegram user ID" });
            }

            // Check user role - only Admin (2) or Manager (1) can access moderation
            var roleResponse = await _roleService.GetUserRoleAsync(telegramId);
            if (roleResponse == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!roleResponse.IsAdmin && !roleResponse.IsManager)
            {
                _logger.LogWarning("Unauthorized moderation access attempt. TelegramId: {TelegramId} does not have required role", telegramId);
                return StatusCode(403, new { error = "Only administrators and managers can access moderation" });
            }

            var success = await _locationService.DeleteLocationAsync(locationId);
            if (!success)
            {
                return NotFound(new { error = "Location not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location for moderation");
            return StatusCode(500, "Internal server error");
        }
    }
}

