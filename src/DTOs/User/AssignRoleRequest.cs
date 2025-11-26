using System.ComponentModel.DataAnnotations;
using GeoStud.Api.Models;

namespace GeoStud.Api.DTOs.User;

public class AssignRoleRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; }
}

