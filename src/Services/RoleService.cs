using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.User;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class RoleService : IRoleService
{
    private readonly GeoStudDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RoleService> _logger;
    private const string RoleCachePrefix = "user_role_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public RoleService(
        GeoStudDbContext context,
        IMemoryCache cache,
        ILogger<RoleService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RoleCheckResponse?> GetUserRoleByUsernameAsync(string username)
    {
        var cacheKey = $"{RoleCachePrefix}username_{username}";
        
        if (_cache.TryGetValue(cacheKey, out RoleCheckResponse? cachedResponse))
        {
            _logger.LogDebug("Role cache hit for Username: {Username}", username);
            return cachedResponse;
        }

        _logger.LogDebug("Role cache miss for Username: {Username}, querying database", username);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

        if (user == null)
        {
            return null;
        }

        var response = new RoleCheckResponse
        {
            Username = user.Username,
            TelegramId = user.TelegramId,
            Role = user.Role,
            IsAdmin = user.Role == UserRole.Admin,
            IsManager = user.Role == UserRole.Manager,
            IsUser = user.Role == UserRole.User
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            Size = 1
        };
        _cache.Set(cacheKey, response, cacheOptions);
        _logger.LogDebug("Cached role for Username: {Username}, Role: {Role}", username, user.Role);

        return response;
    }

    public async Task<RoleCheckResponse?> GetUserRoleAsync(long telegramId)
    {
        var cacheKey = $"{RoleCachePrefix}telegram_{telegramId}";
        
        if (_cache.TryGetValue(cacheKey, out RoleCheckResponse? cachedResponse))
        {
            _logger.LogDebug("Role cache hit for TelegramId: {TelegramId}", telegramId);
            return cachedResponse;
        }

        _logger.LogDebug("Role cache miss for TelegramId: {TelegramId}, querying database", telegramId);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);

        if (user == null)
        {
            return null;
        }

        var response = new RoleCheckResponse
        {
            Username = user.Username,
            TelegramId = telegramId,
            Role = user.Role,
            IsAdmin = user.Role == UserRole.Admin,
            IsManager = user.Role == UserRole.Manager,
            IsUser = user.Role == UserRole.User
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            Size = 1
        };
        _cache.Set(cacheKey, response, cacheOptions);
        _logger.LogDebug("Cached role for TelegramId: {TelegramId}, Role: {Role}", telegramId, user.Role);

        return response;
    }

    public async Task<bool> AssignRoleAsync(string adminUsername, AssignRoleRequest request)
    {
        // Verify that the requester is an admin
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == adminUsername && !u.IsDeleted);

        if (adminUser == null || adminUser.Role != UserRole.Admin)
        {
            _logger.LogWarning("Unauthorized role assignment attempt. Username: {Username} is not an admin", adminUsername);
            return false;
        }

        if (request.Role != UserRole.Manager && request.Role != UserRole.Admin)
        {
            _logger.LogWarning("Invalid role assignment attempt. Role: {Role} is not allowed for assignment", request.Role);
            return false;
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

        if (targetUser == null)
        {
            _logger.LogWarning("Target user not found for Username: {Username}", request.Username);
            return false;
        }

        targetUser.Role = request.Role;
        targetUser.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        InvalidateRoleCache(targetUser.Username);
        if (targetUser.TelegramId.HasValue)
        {
            InvalidateRoleCache(targetUser.TelegramId.Value);
        }

        _logger.LogInformation("Role assigned successfully. Admin: {AdminUsername}, Target: {TargetUsername}, New Role: {Role}",
            adminUsername, request.Username, request.Role);

        return true;
    }

    public async Task<bool> IsUserAdminByUsernameAsync(string username)
    {
        var roleResponse = await GetUserRoleByUsernameAsync(username);
        return roleResponse?.IsAdmin ?? false;
    }

    public async Task<bool> IsUserAdminAsync(long telegramId)
    {
        var roleResponse = await GetUserRoleAsync(telegramId);
        return roleResponse?.IsAdmin ?? false;
    }

    public async Task<bool> IsUserManagerAsync(long telegramId)
    {
        var roleResponse = await GetUserRoleAsync(telegramId);
        return roleResponse?.IsManager ?? false;
    }

    public void InvalidateRoleCache(string username)
    {
        var cacheKey = $"{RoleCachePrefix}username_{username}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated role cache for Username: {Username}", username);
    }

    public void InvalidateRoleCache(long telegramId)
    {
        var cacheKey = $"{RoleCachePrefix}telegram_{telegramId}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated role cache for TelegramId: {TelegramId}", telegramId);
    }
}

