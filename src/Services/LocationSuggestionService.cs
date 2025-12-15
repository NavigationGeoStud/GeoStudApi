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
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<LocationSuggestionService> _logger;

    public LocationSuggestionService(
        GeoStudDbContext context,
        IFavoritesService favoritesService,
        INotificationService notificationService,
        IRecommendationService recommendationService,
        ILogger<LocationSuggestionService> logger)
    {
        _context = context;
        _favoritesService = favoritesService;
        _notificationService = notificationService;
        _recommendationService = recommendationService;
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

        // Используем новую систему рекомендаций с мутацией 20%
        return await _recommendationService.GetRecommendationsAsync(user.Id, page, pageSize, mutationRate: 0.2);
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

        // Записываем положительную обратную связь для обучения системы
        await _recommendationService.RecordPositiveFeedbackAsync(userId.Value, locationId);

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

        // Получаем userId для записи обратной связи
        var userId = await _favoritesService.GetUserIdFromTelegramIdAsync(telegramId);
        if (userId.HasValue)
        {
            // Записываем отрицательную обратную связь для обучения системы
            await _recommendationService.RecordNegativeFeedbackAsync(userId.Value, locationId);
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

