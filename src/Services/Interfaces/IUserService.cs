using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;

namespace GeoStud.Api.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> SubmitUserAsync(string clientId, UserRequest request);
    Task<UserResponseDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetCurrentUserAsync(string clientId);
    Task<UserResponseDto?> GetUserByTelegramIdAsync(long telegramId);
    Task<UserResponseDto> UpdateUserAsync(string clientId, UpdateUserRequest request);
    Task<UserResponseDto> UpdateUserByTelegramIdAsync(long telegramId, UpdateUserRequest request);
    Task<UserResponseDto> UpdateUserFullAsync(string clientId, UserRequest request);
}

