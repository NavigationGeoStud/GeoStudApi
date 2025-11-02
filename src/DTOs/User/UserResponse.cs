namespace GeoStud.Api.DTOs.User;

public class UserResponse
{
    public int UserId { get; set; }
    public long? TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? Email { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public bool IsStudent { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public List<string> Interests { get; set; } = new();
    public string Budget { get; set; } = string.Empty;
    public string ActivityTime { get; set; } = string.Empty;
    public string SocialPreference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
