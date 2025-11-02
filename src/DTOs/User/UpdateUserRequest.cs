using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.User;

/// <summary>
/// Request DTO for updating user data (all fields optional for partial update)
/// </summary>
public class UpdateUserRequest
{
    public long? TelegramId { get; set; }
    
    [MaxLength(20)]
    public string? AgeRange { get; set; }
    
    public bool? IsStudent { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    public bool? IsLocal { get; set; }
    
    public List<string>? Interests { get; set; }
    
    [MaxLength(50)]
    public string? Budget { get; set; }
    
    [MaxLength(50)]
    public string? ActivityTime { get; set; }
    
    [MaxLength(50)]
    public string? SocialPreference { get; set; }
}
