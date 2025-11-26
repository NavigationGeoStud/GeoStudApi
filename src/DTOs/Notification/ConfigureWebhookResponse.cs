namespace GeoStud.Api.DTOs.Notification;

public class ConfigureWebhookResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? WebhookUrl { get; set; }
}

