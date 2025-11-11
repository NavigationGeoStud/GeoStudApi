using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class Notification : BaseEntity
{
    [Required]
    public long TelegramId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // "like", "match", "location_suggestion"
    
    [Required]
    public bool IsRead { get; set; } = false;
    
    public long? FromTelegramId { get; set; } // Для like и match
    
    public int? LocationId { get; set; } // Для location_suggestion
    
    [MaxLength(500)]
    public string? Message { get; set; } // Сообщение для like уведомлений
    
    // Navigation properties
    [ForeignKey(nameof(LocationId))]
    public virtual Location? Location { get; set; }
}

