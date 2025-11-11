namespace GeoStud.Api.DTOs.People;

public class UserProfileResponse
{
    public long TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public bool IsStudent { get; set; }
    public List<string> Interests { get; set; } = new();
    public string? ProfileDescription { get; set; }
    public List<string> ProfilePhotos { get; set; } = new();
}

