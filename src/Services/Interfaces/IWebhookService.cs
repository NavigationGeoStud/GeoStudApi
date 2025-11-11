using GeoStud.Api.DTOs.Notification;

namespace GeoStud.Api.Services.Interfaces;

public interface IWebhookService
{
    Task<bool> SendNotificationWebhookAsync(long telegramId, NotificationResponse notification);
    Task<bool> ConfigureWebhookAsync(string webhookUrl, string? secret = null);
    string? GetWebhookUrl();
}

