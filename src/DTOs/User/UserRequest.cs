using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.User;

public class UserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    public long? TelegramId { get; set; }
    
    [MaxLength(20)]
    public string? AgeRange { get; set; } // Deprecated, kept for backward compatibility
    
    [Range(16, 100)]
    public int? Age { get; set; } // Age as integer (16-100)
    
    [Required]
    public bool IsStudent { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty;
    
    [Required]
    public bool IsLocal { get; set; }
    
    [Required]
    public List<string> Interests { get; set; } = new();
    
    [Required]
    [MaxLength(50)]
    public string Budget { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ActivityTime { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string SocialPreference { get; set; } = string.Empty;
}
