namespace GeoStud.Api.Models;

public class LocationSubcategoryJoin : BaseEntity
{
    public int LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public int SubcategoryId { get; set; }
    public virtual LocationSubcategory Subcategory { get; set; } = null!;
}

