using GeoStud.Api.Models;

namespace GeoStud.Api.DTOs.User;

public class RoleCheckResponse
{
    public string Username { get; set; } = string.Empty;
    public long? TelegramId { get; set; }
    public UserRole Role { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsManager { get; set; }
    public bool IsUser { get; set; }
}

