using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class UserDislike : BaseEntity
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int TargetUserId { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey(nameof(TargetUserId))]
    public virtual User TargetUser { get; set; } = null!;
}

