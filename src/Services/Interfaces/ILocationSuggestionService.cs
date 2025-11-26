using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.DTOs.Notification;

namespace GeoStud.Api.Services.Interfaces;

public interface ILocationSuggestionService
{
    Task<PagedResponse<LocationResponse>> GetLocationSuggestionsAsync(long telegramId, int page = 1, int pageSize = 20);
    Task<SuccessResponse> AcceptLocationSuggestionAsync(int locationId, long telegramId, int? notificationId = null);
    Task<SuccessResponse> RejectLocationSuggestionAsync(int locationId, long telegramId, int? notificationId = null);
}

