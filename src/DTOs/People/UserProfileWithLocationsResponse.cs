namespace GeoStud.Api.DTOs.People;

public class UserProfileWithLocationsResponse : UserProfileResponse
{
    public List<LocationInfo> MatchingLocations { get; set; } = new();
    
    public class LocationInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}

