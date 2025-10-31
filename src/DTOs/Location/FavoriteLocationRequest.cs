using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Location;

public class FavoriteLocationRequest
{
    [Required]
    public int LocationId { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

