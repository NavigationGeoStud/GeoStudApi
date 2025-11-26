using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class LocationSuggestion : BaseEntity
{
    [Required]
    public long TelegramId { get; set; }
    
    [Required]
    public int LocationId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // "pending", "accepted", "rejected"
    
    // Navigation properties
    [ForeignKey(nameof(LocationId))]
    public virtual Location Location { get; set; } = null!;
}

