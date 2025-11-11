using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.People;

public class DislikeRequest
{
    [Required]
    public long TelegramId { get; set; }
    
    [Required]
    public long TargetTelegramId { get; set; }
}

