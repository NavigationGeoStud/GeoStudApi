using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class FavoritesService : IFavoritesService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<FavoritesService> _logger;

    public FavoritesService(GeoStudDbContext context, ILogger<FavoritesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetUserIdFromClientIdAsync(string clientId)
    {
        var username = $"service_{clientId}";
        _logger.LogDebug("Looking up user with username: {Username}", username);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username);

        if (user == null)
        {
            _logger.LogWarning("User not found with username: {Username}. Checking if any users exist with service_ prefix...", username);
            var serviceUsers = await _context.Users
                .Where(u => u.Username.StartsWith("service_"))
                .Select(u => u.Username)
                .ToListAsync();
            _logger.LogDebug("Found service users: {Users}", string.Join(", ", serviceUsers));
        }
        else
        {
            _logger.LogDebug("Found user ID: {UserId} for username: {Username}", user.Id, username);
        }

        return user?.Id;
    }

    public async Task<int?> GetUserIdFromTelegramIdAsync(long telegramId)
    {
        _logger.LogDebug("Looking up user with TelegramId: {TelegramId}", telegramId);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.TelegramId == telegramId && !s.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found with TelegramId: {TelegramId}. Checking if any users exist...", telegramId);
            var allUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new { u.Id, u.TelegramId, u.Username })
                .ToListAsync();
            _logger.LogDebug("Found {Count} users in database", allUsers.Count);
            if (allUsers.Any())
            {
                var userList = string.Join(", ", allUsers.Select(u => $"Id={u.Id}, TelegramId={u.TelegramId}, Username={u.Username}"));
                _logger.LogDebug("Users in database: {Users}", userList);
            }
        }
        else
        {
            _logger.LogDebug("Found user ID: {UserId} for TelegramId: {TelegramId}", user.Id, telegramId);
        }

        return user?.Id;
    }

    public async Task<IEnumerable<FavoriteLocationResponse>> GetFavoritesAsync(int userId)
    {
        var favorites = await _context.FavoriteLocations
            .Include(f => f.Location)
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return favorites.Select(f => ToFavoriteLocationResponse(f)).ToList();
    }

    public async Task<FavoriteLocationResponse> AddFavoriteAsync(int userId, FavoriteLocationRequest request)
    {
        _logger.LogDebug("AddFavoriteAsync called for userId: {UserId}, LocationId: {LocationId}", userId, request.LocationId);

        // Check if location exists
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId && !l.IsDeleted);

        if (location == null)
        {
            _logger.LogWarning("Location {LocationId} not found", request.LocationId);
            throw new ArgumentException("Location not found", nameof(request));
        }

        _logger.LogDebug("Location {LocationId} found: {LocationName}", location.Id, location.Name);

        // Check if already in favorites (including deleted ones, to handle unique index constraint)
        var existingFavorite = await _context.FavoriteLocations
            .IgnoreQueryFilters() // Check even deleted records to handle unique index
            .FirstOrDefaultAsync(f => f.UserId == userId && f.LocationId == request.LocationId);

        FavoriteLocation favorite;
        bool isRestored = false;

        if (existingFavorite != null)
        {
            if (!existingFavorite.IsDeleted)
            {
                _logger.LogWarning("Location {LocationId} is already in favorites for user {UserId}", request.LocationId, userId);
                throw new InvalidOperationException("Location is already in favorites");
            }

            // Restore deleted favorite
            _logger.LogDebug("Restoring deleted FavoriteLocation: Id={FavoriteId}, UserId={UserId}, LocationId={LocationId}", 
                existingFavorite.Id, userId, request.LocationId);
            
            existingFavorite.IsDeleted = false;
            existingFavorite.CreatedAt = DateTime.UtcNow;
            existingFavorite.UpdatedAt = null;
            

            favorite = existingFavorite;
            isRestored = true;
        }
        else
        {
            // Create new favorite
            favorite = new FavoriteLocation
            {
                UserId = userId,
                LocationId = request.LocationId,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Creating FavoriteLocation: UserId={UserId}, LocationId={LocationId}", favorite.UserId, favorite.LocationId);
            _context.FavoriteLocations.Add(favorite);
        }
        
        try
        {
            var savedCount = await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} changes to database. FavoriteLocation Id: {FavoriteId}", savedCount, favorite.Id);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error saving FavoriteLocation. Inner exception: {InnerException}", dbEx.InnerException?.Message);
            _logger.LogError("FavoriteLocation details: UserId={UserId}, LocationId={LocationId}, CreatedAt={CreatedAt}", 
                favorite.UserId, favorite.LocationId, favorite.CreatedAt);
            throw new InvalidOperationException($"Error saving favorite location: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving FavoriteLocation to database. FavoriteLocation details: UserId={UserId}, LocationId={LocationId}", 
                favorite.UserId, favorite.LocationId);
            throw;
        }

        // Reload with location
        await _context.Entry(favorite)
            .Reference(f => f.Location)
            .LoadAsync();

        _logger.LogDebug("FavoriteLocation {FavoriteId} {Action} successfully for user {UserId}", 
            favorite.Id, isRestored ? "restored" : "created", userId);

        return ToFavoriteLocationResponse(favorite);
    }

    public async Task<FavoriteLocationResponse?> GetFavoriteByIdAsync(int userId, int id)
    {
        var favorite = await _context.FavoriteLocations
            .Include(f => f.Location)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.IsDeleted);

        if (favorite == null)
        {
            return null;
        }

        return ToFavoriteLocationResponse(favorite);
    }

    public async Task<FavoriteLocationResponse> UpdateFavoriteAsync(int userId, int id, FavoriteLocationRequest request)
    {
        var favorite = await _context.FavoriteLocations
            .Include(f => f.Location)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.IsDeleted);

        if (favorite == null)
        {
            throw new ArgumentException("Favorite location not found", nameof(id));
        }

        await _context.SaveChangesAsync();

        return ToFavoriteLocationResponse(favorite);
    }

    public async Task<bool> RemoveFavoriteAsync(int userId, int id)
    {
        var favorite = await _context.FavoriteLocations
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.IsDeleted);

        if (favorite == null)
        {
            return false;
        }

        favorite.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFavoriteByLocationIdAsync(int userId, int locationId)
    {
        var favorite = await _context.FavoriteLocations
            .FirstOrDefaultAsync(f => f.LocationId == locationId && f.UserId == userId && !f.IsDeleted);

        if (favorite == null)
        {
            return false;
        }

        favorite.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CheckFavoriteAsync(int userId, int locationId)
    {
        return await _context.FavoriteLocations
            .AnyAsync(f => f.UserId == userId && f.LocationId == locationId && !f.IsDeleted);
    }

    private static FavoriteLocationResponse ToFavoriteLocationResponse(FavoriteLocation favorite)
    {
        return new FavoriteLocationResponse
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            LocationId = favorite.LocationId,
            Notes = favorite.Notes,
            CreatedAt = favorite.CreatedAt,
            LocationName = favorite.Location.Name,
            LocationDescription = favorite.Location.Description,
            Coordinates = favorite.Location.Coordinates,
            Address = favorite.Location.Address,
            City = favorite.Location.City,
            Rating = favorite.Location.Rating,
            TelegramImageIds = favorite.Location.TelegramImageIds
        };
    }
}

