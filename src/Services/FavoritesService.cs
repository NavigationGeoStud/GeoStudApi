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
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username);

        return user?.Id;
    }

    public async Task<int?> GetUserIdFromTelegramIdAsync(long telegramId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.TelegramId == telegramId);

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
        // Check if location exists
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId && !l.IsDeleted);

        if (location == null)
        {
            throw new ArgumentException("Location not found", nameof(request));
        }

        // Check if already in favorites
        var existingFavorite = await _context.FavoriteLocations
            .FirstOrDefaultAsync(f => f.UserId == userId && f.LocationId == request.LocationId && !f.IsDeleted);

        if (existingFavorite != null)
        {
            throw new InvalidOperationException("Location is already in favorites");
        }

        // Create favorite
        var favorite = new FavoriteLocation
        {
            UserId = userId,
            LocationId = request.LocationId,
            Notes = request.Notes
        };

        _context.FavoriteLocations.Add(favorite);
        await _context.SaveChangesAsync();

        // Reload with location
        await _context.Entry(favorite)
            .Reference(f => f.Location)
            .LoadAsync();

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

        favorite.Notes = request.Notes;
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
            ImageUrl = favorite.Location.ImageUrl
        };
    }
}

