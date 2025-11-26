using GeoStud.Api.DTOs.Notification;

namespace GeoStud.Api.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(long telegramId, bool unreadOnly = false);
    Task<bool> MarkNotificationAsReadAsync(int notificationId, long telegramId);
    Task<NotificationResponse?> CreateLikeNotificationAsync(long toTelegramId, long fromTelegramId, string? message = null);
    Task<NotificationResponse?> CreateMatchNotificationAsync(long toTelegramId, long fromTelegramId);
    Task<NotificationResponse?> CreateLocationSuggestionNotificationAsync(long telegramId, int locationId);
}

