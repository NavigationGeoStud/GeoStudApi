using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.Notification;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class LocationSuggestionService : ILocationSuggestionService
{
    private readonly GeoStudDbContext _context;
    private readonly IFavoritesService _favoritesService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<LocationSuggestionService> _logger;

    public LocationSuggestionService(
        GeoStudDbContext context,
        IFavoritesService favoritesService,
        INotificationService notificationService,
        ILogger<LocationSuggestionService> logger)
    {
        _context = context;
        _favoritesService = favoritesService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PagedResponse<LocationResponse>> GetLocationSuggestionsAsync(long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("GetLocationSuggestionsAsync: telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            telegramId, page, pageSize);

        // Get user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);

        if (user == null)
        {
            return new PagedResponse<LocationResponse>
            {
                Data = new List<LocationResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get user's favorite location IDs
        var favoriteLocationIds = await _context.FavoriteLocations
            .Where(fl => fl.UserId == user.Id && !fl.IsDeleted)
            .Select(fl => fl.LocationId)
            .ToListAsync();

        // Get rejected location IDs
        var rejectedLocationIds = await _context.LocationSuggestions
            .Where(ls => ls.TelegramId == telegramId && 
                        ls.Status == "rejected" && 
                        !ls.IsDeleted)
            .Select(ls => ls.LocationId)
            .ToListAsync();

        // Get user interests
        var interests = DeserializeInterests(user.Interests);

        // Get user's favorite location categories
        var favoriteCategories = await _context.FavoriteLocations
            .Where(fl => fl.UserId == user.Id && !fl.IsDeleted)
            .Select(fl => fl.Location.CategoryId)
            .Distinct()
            .ToListAsync();

        // Build query for suggested locations
        var query = _context.Locations
            .Where(l => !l.IsDeleted && l.IsActive)
            .Where(l => !favoriteLocationIds.Contains(l.Id))
            .Where(l => !rejectedLocationIds.Contains(l.Id));

        // Filter by interests if user has interests
        if (interests.Any())
        {
            // This is a simplified matching - in production, you might want more sophisticated matching
            query = query.Where(l => 
                interests.Any(i => 
                    l.Name.Contains(i, StringComparison.OrdinalIgnoreCase) ||
                    (l.Description != null && l.Description.Contains(i, StringComparison.OrdinalIgnoreCase))));
        }

        // Prefer locations from favorite categories
        var locations = await query
            .OrderByDescending(l => favoriteCategories.Contains(l.CategoryId))
            .ThenByDescending(l => l.Rating ?? 0)
            .ThenByDescending(l => l.RatingCount ?? 0)
            .ToListAsync();

        var totalCount = locations.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedLocations = locations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Convert to LocationResponse
        var locationResponses = pagedLocations.Select(l => new LocationResponse
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description,
            Coordinates = l.Coordinates,
            Address = l.Address,
            City = l.City,
            Phone = l.Phone,
            Website = l.Website,
            TelegramImageIds = l.TelegramImageIds,
            Rating = l.Rating,
            RatingCount = l.RatingCount,
            PriceRange = l.PriceRange,
            WorkingHours = l.WorkingHours,
            IsActive = l.IsActive,
            IsVerified = l.IsVerified,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            Category = new LocationResponse.CategoryInfo
            {
                Id = l.CategoryId,
                Name = l.Category.Name,
                IconName = l.Category.IconName
            },
            Subcategories = l.SubcategoryJoins
                .Where(sj => !sj.IsDeleted)
                .Select(sj => new LocationResponse.SubcategoryInfo
                {
                    Id = sj.Subcategory.Id,
                    Name = sj.Subcategory.Name,
                    CategoryId = sj.Subcategory.CategoryId
                })
                .ToList()
        }).ToList();

        return new PagedResponse<LocationResponse>
        {
            Data = locationResponses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<SuccessResponse> AcceptLocationSuggestionAsync(int locationId, long telegramId, int? notificationId = null)
    {
        _logger.LogDebug("AcceptLocationSuggestionAsync: locationId={LocationId}, telegramId={TelegramId}, notificationId={NotificationId}", 
            locationId, telegramId, notificationId);

        // Get user ID
        var userId = await _favoritesService.GetUserIdFromTelegramIdAsync(telegramId);
        if (userId == null)
        {
            throw new ArgumentException("User not found", nameof(telegramId));
        }

        // Check if location exists
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);

        if (location == null)
        {
            throw new ArgumentException("Location not found", nameof(locationId));
        }

        // Check if already in favorites
        var isFavorite = await _favoritesService.CheckFavoriteAsync(userId.Value, locationId);
        if (isFavorite)
        {
            throw new InvalidOperationException("Location is already in favorites");
        }

        // Add to favorites
        await _favoritesService.AddFavoriteAsync(userId.Value, new DTOs.Location.FavoriteLocationRequest
        {
            LocationId = locationId
        });

        // Update or create location suggestion record
        var suggestion = await _context.LocationSuggestions
            .FirstOrDefaultAsync(ls => ls.TelegramId == telegramId && 
                                      ls.LocationId == locationId && 
                                      !ls.IsDeleted);

        if (suggestion == null)
        {
            suggestion = new LocationSuggestion
            {
                TelegramId = telegramId,
                LocationId = locationId,
                Status = "accepted"
            };
            _context.LocationSuggestions.Add(suggestion);
        }
        else
        {
            suggestion.Status = "accepted";
            suggestion.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Mark notification as read if provided
        if (notificationId.HasValue)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId.Value, telegramId);
        }

        return new SuccessResponse
        {
            Success = true,
            Message = "Локация добавлена в избранное"
        };
    }

    public async Task<SuccessResponse> RejectLocationSuggestionAsync(int locationId, long telegramId, int? notificationId = null)
    {
        _logger.LogDebug("RejectLocationSuggestionAsync: locationId={LocationId}, telegramId={TelegramId}, notificationId={NotificationId}", 
            locationId, telegramId, notificationId);

        // Check if location exists
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);

        if (location == null)
        {
            throw new ArgumentException("Location not found", nameof(locationId));
        }

        // Update or create location suggestion record
        var suggestion = await _context.LocationSuggestions
            .FirstOrDefaultAsync(ls => ls.TelegramId == telegramId && 
                                      ls.LocationId == locationId && 
                                      !ls.IsDeleted);

        if (suggestion == null)
        {
            suggestion = new LocationSuggestion
            {
                TelegramId = telegramId,
                LocationId = locationId,
                Status = "rejected"
            };
            _context.LocationSuggestions.Add(suggestion);
        }
        else
        {
            suggestion.Status = "rejected";
            suggestion.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Mark notification as read if provided
        if (notificationId.HasValue)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId.Value, telegramId);
        }

        return new SuccessResponse
        {
            Success = true,
            Message = "Предложение отклонено"
        };
    }

    private static List<string> DeserializeInterests(string? interestsJson)
    {
        return string.IsNullOrEmpty(interestsJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(interestsJson) ?? new List<string>();
    }
}

