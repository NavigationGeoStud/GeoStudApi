using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStud.Api.Models;

public class Match : BaseEntity
{
    [Required]
    public int UserId1 { get; set; }
    
    [Required]
    public int UserId2 { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId1))]
    public virtual User User1 { get; set; } = null!;
    
    [ForeignKey(nameof(UserId2))]
    public virtual User User2 { get; set; } = null!;
}

