namespace GeoStud.Api.DTOs.People;

public class LikeResponse
{
    public bool Success { get; set; }
    public bool IsMatch { get; set; }
    public MatchInfo? Match { get; set; }
    
    public class MatchInfo
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserInfo User1 { get; set; } = null!;
        public UserInfo User2 { get; set; } = null!;
    }
    
    public class UserInfo
    {
        public long TelegramId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
    }
}

