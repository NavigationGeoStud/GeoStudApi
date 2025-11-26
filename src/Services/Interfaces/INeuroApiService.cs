namespace GeoStud.Api.Services.Interfaces;

public interface INeuroApiService
{
    Task<string> GenerateLocationDescriptionAsync(string locationName, string? existingDescription, string? address, string? city, string? categoryName);
}

