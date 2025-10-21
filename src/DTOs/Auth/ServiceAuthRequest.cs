using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.DTOs.Auth;

public class ServiceAuthRequest
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
}
