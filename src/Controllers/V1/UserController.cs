using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Tags("Private")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Submit user data
    /// </summary>
    /// <param name="request">User data</param>
    /// <returns>Created user response</returns>
    [HttpPost("submit")]
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

            // Get service client ID from token
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var response = await _userService.SubmitUserAsync(clientIdClaim, request);
            return CreatedAtAction(nameof(GetUser), new { id = response.UserId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user data by user ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User response</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var response = await _userService.GetUserByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all user responses (for analytics)
    /// </summary>
    /// <returns>List of user responses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var responses = await _userService.GetAllUsersAsync();
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get current user's data
    /// </summary>
    /// <returns>Current user's response</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var clientIdClaim = User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim))
            {
                return Unauthorized("Invalid service token");
            }

            var response = await _userService.GetCurrentUserAsync(clientIdClaim);
            if (response == null)
            {
                return NotFound("User not found");
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update user data (partial update - only provided fields will be updated)
    /// </summary>
    /// <param name="request">User update data</param>
    /// <returns>Updated user response</returns>
    [HttpPatch]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        try
        {
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
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update user data (full update - requires all fields)
    /// </summary>
    /// <param name="request">Complete user data</param>
    /// <returns>Updated user response</returns>
    [HttpPut]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUserFull([FromBody] UserRequest request)
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

            var response = await _userService.UpdateUserFullAsync(clientIdClaim, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user (full)");
            return StatusCode(500, "Internal server error");
        }
    }
}

