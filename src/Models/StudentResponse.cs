using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class StudentResponse : BaseEntity
{
    [Required]
    public int StudentId { get; set; }
    
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
    public virtual Student Student { get; set; } = null!;
}
