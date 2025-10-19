using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class ServiceClient : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string ClientSecret { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastUsedAt { get; set; }
    
    [MaxLength(1000)]
    public string? AllowedScopes { get; set; }
}
