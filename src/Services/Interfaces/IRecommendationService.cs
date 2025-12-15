using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;

namespace GeoStud.Api.Services.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Получить рекомендации локаций для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="mutationRate">Вероятность мутации (0.0-1.0), по умолчанию 0.2 (20%)</param>
    /// <returns>Страничный список рекомендованных локаций</returns>
    Task<PagedResponse<LocationResponse>> GetRecommendationsAsync(int userId, int page = 1, int pageSize = 20, double mutationRate = 0.2);

    /// <summary>
    /// Обработать обратную связь пользователя (добавление в избранное)
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="locationId">ID локации</param>
    Task RecordPositiveFeedbackAsync(int userId, int locationId);

    /// <summary>
    /// Обработать отрицательную обратную связь (дизлайк/скип)
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="locationId">ID локации</param>
    Task RecordNegativeFeedbackAsync(int userId, int locationId);

    /// <summary>
    /// Пересчитать профиль рекомендаций пользователя на основе его истории
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    Task RecalculateUserProfileAsync(int userId);
}

