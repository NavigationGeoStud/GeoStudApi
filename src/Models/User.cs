using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(255)]
    public string? PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    public long? TelegramId { get; set; }
    
    // User profile data
    [Required]
    [MaxLength(20)]
    public string AgeRange { get; set; } = string.Empty; // "17-22", "23-25", "26-30", "30+"
    
    [Required]
    public bool IsStudent { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty; // "Male", "Female"
    
    [Required]
    public bool IsLocal { get; set; } // true = местный, false = приезжий
    
    [MaxLength(1000)]
    public string? Interests { get; set; } // JSON array of interests
    
    [Required]
    [MaxLength(50)]
    public string Budget { get; set; } = string.Empty; // "Free", "500", "1000", "Unlimited"
    
    [Required]
    [MaxLength(50)]
    public string ActivityTime { get; set; } = string.Empty; // "Morning", "Day", "Evening", "Night"
    
    [Required]
    [MaxLength(50)]
    public string SocialPreference { get; set; } = string.Empty; // "Alone", "Couple", "Group", "Party"
    
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserAnalyticsResponse> Responses { get; set; } = new List<UserAnalyticsResponse>();
    public virtual ICollection<FavoriteLocation> FavoriteLocations { get; set; } = new List<FavoriteLocation>();
}
