using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Location;

public class FavoriteLocationRequest
{
    [Required]
    public int LocationId { get; set; }
    
}

