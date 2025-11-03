using GeoStud.Api.DTOs.Location;
using CreateLocationTelegramRequest = GeoStud.Api.DTOs.Location.CreateLocationTelegramRequest;

namespace GeoStud.Api.Services.Interfaces;

public interface ILocationService
{
    Task<IEnumerable<LocationResponse>> GetLocationsAsync(int? categoryId = null, int? subcategoryId = null, string? city = null);
    Task<LocationResponse?> GetLocationByIdAsync(int id);
    Task<IEnumerable<LocationResponse>> GetLocationsByCategoryAsync(int categoryId);
    Task<LocationResponse> CreateLocationAsync(LocationRequest request);
    Task<LocationResponse> CreateLocationFromTelegramAsync(CreateLocationTelegramRequest request);
    Task<LocationResponse> UpdateLocationAsync(int id, LocationRequest request);
    Task<bool> DeleteLocationAsync(int id);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<IEnumerable<LocationResponse>> GetNearbyLocationsAsync(string coordinates, double radiusKm = 5);
}

