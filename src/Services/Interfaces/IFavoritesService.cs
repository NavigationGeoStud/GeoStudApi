using GeoStud.Api.DTOs.Location;

namespace GeoStud.Api.Services.Interfaces;

public interface IFavoritesService
{
    Task<int?> GetUserIdFromClientIdAsync(string clientId);
    Task<int?> GetUserIdFromTelegramIdAsync(long telegramId);
    Task<IEnumerable<FavoriteLocationResponse>> GetFavoritesAsync(int userId);
    Task<FavoriteLocationResponse> AddFavoriteAsync(int userId, FavoriteLocationRequest request);
    Task<FavoriteLocationResponse?> GetFavoriteByIdAsync(int userId, int id);
    Task<FavoriteLocationResponse> UpdateFavoriteAsync(int userId, int id, FavoriteLocationRequest request);
    Task<bool> RemoveFavoriteAsync(int userId, int id);
    Task<bool> RemoveFavoriteByLocationIdAsync(int userId, int locationId);
    Task<bool> CheckFavoriteAsync(int userId, int locationId);
}

