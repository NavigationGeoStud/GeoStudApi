using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Notification;

public class MarkNotificationAsReadRequest
{
    [Required]
    public long TelegramId { get; set; }
}

