using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class LocationCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? IconName { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<LocationSubcategory> Subcategories { get; set; } = new List<LocationSubcategory>();
}

