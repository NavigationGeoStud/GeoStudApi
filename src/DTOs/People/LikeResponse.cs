namespace GeoStud.Api.DTOs.People;

public class LikeResponse
{
    public bool Success { get; set; }
    public bool IsMatch { get; set; }
    public string? MatchMessage { get; set; }
}

