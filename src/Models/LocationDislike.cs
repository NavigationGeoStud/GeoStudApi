using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class LocationDislike : BaseEntity
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int LocationId { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey(nameof(LocationId))]
    public virtual Location Location { get; set; } = null!;
}

