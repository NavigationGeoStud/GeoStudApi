using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class UserAnalyticsResponse : BaseEntity
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Answer { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Category { get; set; } // "Demographics", "Preferences", "Behavior"
    
    public int? Score { get; set; } // For numeric responses
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}
