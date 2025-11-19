using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.Common;
using CreateLocationTelegramRequest = GeoStud.Api.DTOs.Location.CreateLocationTelegramRequest;
using UpdateLocationModerationRequest = GeoStud.Api.DTOs.Location.UpdateLocationModerationRequest;

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
    Task<PagedResponse<LocationResponse>> GetLocationsForModerationAsync(int page = 1, int pageSize = 20);
    Task<LocationResponse?> UpdateLocationForModerationAsync(int id, UpdateLocationModerationRequest request);
    Task<bool> ApproveLocationAsync(int id);
    Task<LocationResponse?> ModerateLocationWithAIAsync(int locationId);
    Task<MassModerationResponse> ModerateAllLocationsWithAIAsync();
}

