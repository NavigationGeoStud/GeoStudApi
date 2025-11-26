using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Location;

public class LocationRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Coordinates { get; set; } = string.Empty; // Format: "latitude,longitude"
    
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
    
    public int? RatingCount { get; set; }
    
    [MaxLength(20)]
    public string? PriceRange { get; set; }
    
    [MaxLength(100)]
    public string? WorkingHours { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsVerified { get; set; } = false;
    
    public bool NeedModerate { get; set; } = false;
    
    [Required]
    public int CategoryId { get; set; }
    
    public List<int> SubcategoryIds { get; set; } = new();
}

