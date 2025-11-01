namespace GeoStud.Api.DTOs.Location;

public class FavoriteLocationResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int LocationId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Location details
    public string LocationName { get; set; } = string.Empty;
    public string? LocationDescription { get; set; }
    public string Coordinates { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public decimal? Rating { get; set; }
    public string? ImageUrl { get; set; }
}

