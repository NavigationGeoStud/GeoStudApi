using GeoStud.Api.DTOs.People;

namespace GeoStud.Api.DTOs.Notification;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "like", "match", "location_suggestion"
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserProfileResponse? FromUser { get; set; }
    public NotificationLocationInfo? Location { get; set; }
    public string? Message { get; set; } // Сообщение для like уведомлений
    
    public class NotificationLocationInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public int CategoryId { get; set; }
    }
}

