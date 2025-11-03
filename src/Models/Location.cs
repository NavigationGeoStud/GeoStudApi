using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class Location : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Coordinates { get; set; } = string.Empty; 
    
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
    
    [Required]
    public int CategoryId { get; set; }
    
    // Navigation properties
    public virtual LocationCategory Category { get; set; } = null!;
    public virtual ICollection<LocationSubcategoryJoin> SubcategoryJoins { get; set; } = new List<LocationSubcategoryJoin>();
    public virtual ICollection<FavoriteLocation> FavoriteLocations { get; set; } = new List<FavoriteLocation>();
}

