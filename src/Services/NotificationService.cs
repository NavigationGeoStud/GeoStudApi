using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Notification;
using GeoStud.Api.DTOs.People;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class NotificationService : INotificationService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(GeoStudDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(long telegramId, bool unreadOnly = false)
    {
        _logger.LogDebug("GetNotificationsAsync: telegramId={TelegramId}, unreadOnly={UnreadOnly}", 
            telegramId, unreadOnly);

        var query = _context.Notifications
            .Where(n => n.TelegramId == telegramId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to last 50 notifications
            .ToListAsync();

        var result = new List<NotificationResponse>();

        foreach (var notification in notifications)
        {
            var response = new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            // Load fromUser for like and match notifications
            if ((notification.Type == "like" || notification.Type == "match") && notification.FromTelegramId.HasValue)
            {
                var fromUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.TelegramId == notification.FromTelegramId.Value && !u.IsDeleted);

                if (fromUser != null)
                {
                    var interests = DeserializeInterests(fromUser.Interests);
                    response.FromUser = new UserProfileResponse
                    {
                        TelegramId = fromUser.TelegramId ?? 0,
                        Username = fromUser.Username,
                        FirstName = fromUser.FirstName,
                        AgeRange = fromUser.AgeRange,
                        Gender = fromUser.Gender,
                        IsStudent = fromUser.IsStudent,
                        Interests = interests
                    };
                }
            }

            // Load location for location_suggestion notifications
            if (notification.Type == "location_suggestion" && notification.LocationId.HasValue)
            {
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == notification.LocationId.Value && !l.IsDeleted);

                if (location != null)
                {
                    response.Location = new NotificationResponse.NotificationLocationInfo
                    {
                        Id = location.Id,
                        Name = location.Name,
                        Description = location.Description,
                        Address = location.Address,
                        City = location.City,
                        CategoryId = location.CategoryId
                    };
                }
            }

            result.Add(response);
        }

        return result;
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, long telegramId)
    {
        _logger.LogDebug("MarkNotificationAsReadAsync: notificationId={NotificationId}, telegramId={TelegramId}", 
            notificationId, telegramId);

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && 
                                     n.TelegramId == telegramId && 
                                     !n.IsDeleted);

        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<NotificationResponse?> CreateLikeNotificationAsync(long toTelegramId, long fromTelegramId)
    {
        _logger.LogDebug("CreateLikeNotificationAsync: toTelegramId={ToTelegramId}, fromTelegramId={FromTelegramId}", 
            toTelegramId, fromTelegramId);

        var notification = new Notification
        {
            TelegramId = toTelegramId,
            Type = "like",
            FromTelegramId = fromTelegramId,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Like notification created: Id={NotificationId}", notification.Id);

        // Return notification response
        return await GetNotificationResponseAsync(notification);
    }

    public async Task<NotificationResponse?> CreateMatchNotificationAsync(long toTelegramId, long fromTelegramId)
    {
        _logger.LogDebug("CreateMatchNotificationAsync: toTelegramId={ToTelegramId}, fromTelegramId={FromTelegramId}", 
            toTelegramId, fromTelegramId);

        var notification = new Notification
        {
            TelegramId = toTelegramId,
            Type = "match",
            FromTelegramId = fromTelegramId,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Match notification created: Id={NotificationId}", notification.Id);

        // Return notification response
        return await GetNotificationResponseAsync(notification);
    }

    public async Task<NotificationResponse?> CreateLocationSuggestionNotificationAsync(long telegramId, int locationId)
    {
        _logger.LogDebug("CreateLocationSuggestionNotificationAsync: telegramId={TelegramId}, locationId={LocationId}", 
            telegramId, locationId);

        // Check if notification already exists for this location
        var existingNotification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.TelegramId == telegramId && 
                                     n.LocationId == locationId && 
                                     n.Type == "location_suggestion" && 
                                     !n.IsDeleted && 
                                     !n.IsRead);

        if (existingNotification != null)
        {
            _logger.LogDebug("Location suggestion notification already exists: Id={NotificationId}", existingNotification.Id);
            return await GetNotificationResponseAsync(existingNotification);
        }

        var notification = new Notification
        {
            TelegramId = telegramId,
            Type = "location_suggestion",
            LocationId = locationId,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Location suggestion notification created: Id={NotificationId}", notification.Id);

        // Return notification response
        return await GetNotificationResponseAsync(notification);
    }

    private async Task<NotificationResponse?> GetNotificationResponseAsync(Notification notification)
    {
        var response = new NotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        // Load fromUser for like and match notifications
        if ((notification.Type == "like" || notification.Type == "match") && notification.FromTelegramId.HasValue)
        {
            var fromUser = await _context.Users
                .FirstOrDefaultAsync(u => u.TelegramId == notification.FromTelegramId.Value && !u.IsDeleted);

            if (fromUser != null)
            {
                var interests = DeserializeInterests(fromUser.Interests);
                response.FromUser = new UserProfileResponse
                {
                    TelegramId = fromUser.TelegramId ?? 0,
                    Username = fromUser.Username,
                    FirstName = fromUser.FirstName,
                    AgeRange = fromUser.AgeRange,
                    Gender = fromUser.Gender,
                    IsStudent = fromUser.IsStudent,
                    Interests = interests
                };
            }
        }

        // Load location for location_suggestion notifications
        if (notification.Type == "location_suggestion" && notification.LocationId.HasValue)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == notification.LocationId.Value && !l.IsDeleted);

            if (location != null)
            {
                response.Location = new NotificationResponse.NotificationLocationInfo
                {
                    Id = location.Id,
                    Name = location.Name,
                    Description = location.Description,
                    Address = location.Address,
                    City = location.City,
                    CategoryId = location.CategoryId
                };
            }
        }

        return response;
    }

    private static List<string> DeserializeInterests(string? interestsJson)
    {
        return string.IsNullOrEmpty(interestsJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(interestsJson) ?? new List<string>();
    }
}

