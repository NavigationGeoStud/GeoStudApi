using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class FavoriteLocation : BaseEntity
{
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int LocationId { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
}

