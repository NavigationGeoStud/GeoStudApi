namespace GeoStud.Api.Models;

public class LocationCategoryJoin : BaseEntity
{
    public int LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public virtual LocationCategory Category { get; set; } = null!;
}

