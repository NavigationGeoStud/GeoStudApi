using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Notification;

public class ConfigureWebhookRequest
{
    [Required]
    [Url]
    public string WebhookUrl { get; set; } = string.Empty;
    
    public string? Secret { get; set; }
}

