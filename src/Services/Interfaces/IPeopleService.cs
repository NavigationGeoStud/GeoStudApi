using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.People;

namespace GeoStud.Api.Services.Interfaces;

public interface IPeopleService
{
    Task<PagedResponse<UserProfileResponse>> GetCompaniesByLocationAsync(int locationId, long telegramId, int page = 1, int pageSize = 20);
    Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleAsync(long telegramId, int page = 1, int pageSize = 20);
    Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleByLocationsAsync(long telegramId, int page = 1, int pageSize = 20);
    Task<LikeResponse> LikeUserAsync(long telegramId, long targetTelegramId, string? message = null);
    Task<bool> DislikeUserAsync(long telegramId, long targetTelegramId);
}

