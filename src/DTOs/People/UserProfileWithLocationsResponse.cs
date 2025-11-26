namespace GeoStud.Api.DTOs.People;

public class UserProfileWithLocationsResponse : UserProfileResponse
{
    public List<LocationInfo> MatchingLocations { get; set; } = new();
    
    public class LocationInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}

