using GeoStud.Api.DTOs.User;
using GeoStud.Api.Models;

namespace GeoStud.Api.Services.Interfaces;

public interface IRoleService
{
    Task<RoleCheckResponse?> GetUserRoleByUsernameAsync(string username);
    Task<RoleCheckResponse?> GetUserRoleAsync(long telegramId);
    Task<bool> AssignRoleAsync(string adminUsername, AssignRoleRequest request);
    Task<bool> IsUserAdminByUsernameAsync(string username);
    Task<bool> IsUserAdminAsync(long telegramId);
    Task<bool> IsUserManagerAsync(long telegramId);
    void InvalidateRoleCache(string username);
    void InvalidateRoleCache(long telegramId);
}

