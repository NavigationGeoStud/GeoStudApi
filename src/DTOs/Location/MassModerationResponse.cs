namespace GeoStud.Api.DTOs.Location;

public class MassModerationResponse
{
    public int TotalLocations { get; set; }
    public int SuccessfullyModerated { get; set; }
    public int Failed { get; set; }
    public List<ModerationResult> Results { get; set; } = new();
    
    public class ModerationResult
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GeneratedDescription { get; set; }
    }
}

