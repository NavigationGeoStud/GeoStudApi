using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class LocationSubcategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int CategoryId { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual LocationCategory Category { get; set; } = null!;
}

