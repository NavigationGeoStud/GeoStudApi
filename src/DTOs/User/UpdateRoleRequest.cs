using System.ComponentModel.DataAnnotations;
using GeoStud.Api.Models;

namespace GeoStud.Api.DTOs.User;

public class UpdateRoleRequest
{
    [Required]
    public UserRole Role { get; set; }
}

