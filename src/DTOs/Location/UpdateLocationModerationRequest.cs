using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Location;

public class UpdateLocationModerationRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(500)]
    public string? Website { get; set; }
    
    [MaxLength(2000)]
    public string? TelegramImageIds { get; set; } // Comma-separated Telegram image IDs
    
    public decimal? Rating { get; set; }
    
    [MaxLength(20)]
    public string? PriceRange { get; set; } // "budget", "acceptable", "expensive"
    
    [MaxLength(100)]
    public string? WorkingHours { get; set; }
    
    public int? CategoryId { get; set; }
    
    public List<int>? SubcategoryIds { get; set; }
}

