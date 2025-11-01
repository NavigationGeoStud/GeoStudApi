using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Survey;

public class SurveyRequest
{
    public long? TelegramId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string AgeRange { get; set; } = string.Empty;
    
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
