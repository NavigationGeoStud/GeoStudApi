namespace GeoStud.Api.DTOs.Location;

public class LocationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Coordinates { get; set; } = string.Empty; // Format: "latitude,longitude"
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Rating { get; set; }
    public int? RatingCount { get; set; }
    public string? PriceRange { get; set; }
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public CategoryInfo Category { get; set; } = null!;
    
    public List<SubcategoryInfo> Subcategories { get; set; } = new();
    
    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
    }
    
    public class SubcategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}

