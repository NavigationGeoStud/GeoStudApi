using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

/// <summary>
/// Сервис рекомендаций локаций с самообучением
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<RecommendationService> _logger;
    private readonly Random _random = new();

    // Коэффициенты для расчета скорa рекомендаций
    private const double CategoryMatchWeight = 30.0;        // Соответствие категории из избранного
    private const double SubcategoryMatchWeight = 20.0;      // Соответствие подкатегории из избранного
    private const double BudgetMatchWeight = 15.0;          // Соответствие бюджету
    private const double ActivityTimeMatchWeight = 10.0;     // Соответствие времени активности
    private const double RatingWeight = 15.0;                // Рейтинг локации
    private const double RatingCountWeight = 5.0;             // Количество оценок
    private const double VerifiedBonus = 5.0;                // Бонус за верификацию
    private const double DislikePenalty = -50.0;             // Штраф за дизлайк
    private const double RejectedPenalty = -30.0;            // Штраф за отклонение
    private const double FavoriteBoost = 10.0;                // Бонус если локация в избранном у похожих пользователей
    private const double RecentActivityDecay = 0.95;         // Затухание для старых взаимодействий

    public RecommendationService(
        GeoStudDbContext context,
        ILogger<RecommendationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<LocationResponse>> GetRecommendationsAsync(
        int userId, 
        int page = 1, 
        int pageSize = 20, 
        double mutationRate = 0.2)
    {
        _logger.LogDebug("Getting recommendations for user {UserId}, page {Page}, pageSize {PageSize}, mutationRate {MutationRate}",
            userId, page, pageSize, mutationRate);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return new PagedResponse<LocationResponse>
            {
                Data = new List<LocationResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Получаем историю взаимодействий пользователя
        var userProfile = await GetUserProfileAsync(userId);

        // Определяем, использовать ли мутацию (случайные рекомендации)
        var useMutation = _random.NextDouble() < mutationRate;

        // Получаем кандидатов для рекомендаций
        var candidateLocations = await GetCandidateLocationsAsync(userId, userProfile, useMutation);

        // Рассчитываем скор для каждой локации
        var scoredLocations = await CalculateScoresAsync(candidateLocations, user, userProfile);

        // Сортируем по скору
        var sortedLocations = scoredLocations
            .OrderByDescending(x => x.Score)
            .ToList();

        // Пагинация
        var totalCount = sortedLocations.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedLocations = sortedLocations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Location)
            .ToList();

        _logger.LogInformation("Generated {Count} recommendations for user {UserId}, mutation: {UseMutation}",
            pagedLocations.Count, userId, useMutation);

        return new PagedResponse<LocationResponse>
        {
            Data = pagedLocations,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task RecordPositiveFeedbackAsync(int userId, int locationId)
    {
        _logger.LogDebug("Recording positive feedback: userId={UserId}, locationId={LocationId}", userId, locationId);

        // Получаем информацию о локации для обучения
        var location = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);

        if (location == null)
        {
            _logger.LogWarning("Location {LocationId} not found for positive feedback", locationId);
            return;
        }

        // Логируем для анализа (можно расширить для сохранения статистики)
        _logger.LogInformation("User {UserId} added location {LocationId} (Category: {CategoryId}) to favorites",
            userId, locationId, location.CategoryId);

        // Можно добавить периодический пересчет профиля
        // Пока просто логируем
    }

    public async Task RecordNegativeFeedbackAsync(int userId, int locationId)
    {
        _logger.LogDebug("Recording negative feedback: userId={UserId}, locationId={LocationId}", userId, locationId);

        // Проверяем, существует ли уже дизлайк
        var existingDislike = await _context.LocationDislikes
            .FirstOrDefaultAsync(ld => ld.UserId == userId && ld.LocationId == locationId && !ld.IsDeleted);

        if (existingDislike == null)
        {
            var dislike = new LocationDislike
            {
                UserId = userId,
                LocationId = locationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LocationDislikes.Add(dislike);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} disliked location {LocationId}", userId, locationId);
        }
    }

    public async Task RecalculateUserProfileAsync(int userId)
    {
        _logger.LogDebug("Recalculating user profile for userId={UserId}", userId);

        // Получаем статистику пользователя
        var favorites = await _context.FavoriteLocations
            .Where(fl => fl.UserId == userId && !fl.IsDeleted)
            .Include(fl => fl.Location)
                .ThenInclude(l => l.Category)
            .Include(fl => fl.Location)
                .ThenInclude(l => l.SubcategoryJoins)
                    .ThenInclude(sj => sj.Subcategory)
            .ToListAsync();

        var dislikes = await _context.LocationDislikes
            .Where(ld => ld.UserId == userId && !ld.IsDeleted)
            .Include(ld => ld.Location)
                .ThenInclude(l => l.Category)
            .ToListAsync();

        // Анализируем предпочтения
        var categoryStats = favorites
            .GroupBy(fl => fl.Location.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var subcategoryStats = favorites
            .SelectMany(fl => fl.Location.SubcategoryJoins
                .Where(sj => !sj.IsDeleted)
                .Select(sj => sj.SubcategoryId))
            .GroupBy(id => id)
            .Select(g => new { SubcategoryId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        _logger.LogInformation("User {UserId} profile: {FavoriteCount} favorites, {DislikeCount} dislikes, " +
            "Top categories: {TopCategories}, Top subcategories: {TopSubcategories}",
            userId, favorites.Count, dislikes.Count,
            string.Join(", ", categoryStats.Take(3).Select(c => $"Category{c.CategoryId}({c.Count})")),
            string.Join(", ", subcategoryStats.Take(3).Select(s => $"Subcategory{s.SubcategoryId}({s.Count})")));
    }

    private async Task<UserRecommendationProfile> GetUserProfileAsync(int userId)
    {
        var favorites = await _context.FavoriteLocations
            .Where(fl => fl.UserId == userId && !fl.IsDeleted)
            .Include(fl => fl.Location)
            .ToListAsync();

        var dislikes = await _context.LocationDislikes
            .Where(ld => ld.UserId == userId && !ld.IsDeleted)
            .Select(ld => ld.LocationId)
            .ToListAsync();

        // Получаем TelegramId пользователя для поиска отклоненных локаций
        var userTelegramId = await _context.Users
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => u.TelegramId)
            .FirstOrDefaultAsync();

        var rejected = new List<int>();
        if (userTelegramId.HasValue)
        {
            rejected = await _context.LocationSuggestions
                .Where(ls => ls.TelegramId == userTelegramId.Value && 
                            !ls.IsDeleted && 
                            ls.Status == "rejected")
                .Select(ls => ls.LocationId)
                .ToListAsync();
        }

        var favoriteCategoryIds = favorites
            .Select(fl => fl.Location.CategoryId)
            .Distinct()
            .ToList();

        var favoriteSubcategoryIds = favorites
            .SelectMany(fl => fl.Location.SubcategoryJoins
                .Where(sj => !sj.IsDeleted)
                .Select(sj => sj.SubcategoryId))
            .Distinct()
            .ToList();

        return new UserRecommendationProfile
        {
            FavoriteCategoryIds = favoriteCategoryIds,
            FavoriteSubcategoryIds = favoriteSubcategoryIds,
            DislikedLocationIds = dislikes,
            RejectedLocationIds = rejected,
            FavoriteLocations = favorites.Select(fl => fl.Location).ToList()
        };
    }

    private async Task<List<Location>> GetCandidateLocationsAsync(
        int userId,
        UserRecommendationProfile userProfile,
        bool useMutation)
    {
        var query = _context.Locations
            .Where(l => !l.IsDeleted && !l.NeedModerate && l.IsActive)
            .Where(l => !userProfile.DislikedLocationIds.Contains(l.Id))
            .Where(l => !userProfile.RejectedLocationIds.Contains(l.Id))
            .Where(l => !userProfile.FavoriteLocations.Select(fl => fl.Id).Contains(l.Id))
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .AsQueryable();

        if (useMutation)
        {
            // Мутация: исключаем любимые категории, чтобы предложить что-то новое
            if (userProfile.FavoriteCategoryIds.Any())
            {
                query = query.Where(l => !userProfile.FavoriteCategoryIds.Contains(l.CategoryId));
            }
        }
        else
        {
            // Обычный режим: предпочитаем любимые категории
            if (userProfile.FavoriteCategoryIds.Any())
            {
                query = query.Where(l => userProfile.FavoriteCategoryIds.Contains(l.CategoryId));
            }
        }

        return await query.ToListAsync();
    }

    private async Task<List<ScoredLocation>> CalculateScoresAsync(
        List<Location> locations,
        User user,
        UserRecommendationProfile userProfile)
    {
        var scoredLocations = new List<ScoredLocation>();

        foreach (var location in locations)
        {
            double score = 0.0;

            // 1. Соответствие категории из избранного
            if (userProfile.FavoriteCategoryIds.Contains(location.CategoryId))
            {
                score += CategoryMatchWeight;
            }

            // 2. Соответствие подкатегории из избранного
            var locationSubcategoryIds = location.SubcategoryJoins
                .Where(sj => !sj.IsDeleted)
                .Select(sj => sj.SubcategoryId)
                .ToList();

            var matchingSubcategories = locationSubcategoryIds
                .Intersect(userProfile.FavoriteSubcategoryIds)
                .Count();

            if (matchingSubcategories > 0)
            {
                score += SubcategoryMatchWeight * (matchingSubcategories / (double)locationSubcategoryIds.Count);
            }

            // 3. Соответствие бюджету
            if (!string.IsNullOrEmpty(user.Budget) && !string.IsNullOrEmpty(location.PriceRange))
            {
                if (IsBudgetCompatible(user.Budget, location.PriceRange))
                {
                    score += BudgetMatchWeight;
                }
            }

            // 4. Соответствие времени активности (упрощенная проверка)
            if (!string.IsNullOrEmpty(user.ActivityTime) && !string.IsNullOrEmpty(location.WorkingHours))
            {
                if (IsActivityTimeCompatible(user.ActivityTime, location.WorkingHours))
                {
                    score += ActivityTimeMatchWeight;
                }
            }

            // 5. Рейтинг локации
            if (location.Rating.HasValue)
            {
                score += RatingWeight * (double)location.Rating.Value / 5.0; // Нормализация до 0-1
            }

            // 6. Количество оценок (логарифмическая шкала)
            if (location.RatingCount.HasValue && location.RatingCount.Value > 0)
            {
                score += RatingCountWeight * Math.Log10(location.RatingCount.Value + 1) / 3.0; // Нормализация
            }

            // 7. Бонус за верификацию
            if (location.IsVerified)
            {
                score += VerifiedBonus;
            }

            // 8. Бонус если локация популярна среди похожих пользователей
            var similarUsersFavoriteCount = await GetSimilarUsersFavoriteCountAsync(location.Id, userProfile);
            if (similarUsersFavoriteCount > 0)
            {
                score += FavoriteBoost * Math.Min(similarUsersFavoriteCount / 10.0, 1.0);
            }

            scoredLocations.Add(new ScoredLocation
            {
                Location = ToLocationResponse(location),
                Score = score
            });
        }

        return scoredLocations;
    }

    private async Task<int> GetSimilarUsersFavoriteCountAsync(int locationId, UserRecommendationProfile userProfile)
    {
        // Находим пользователей с похожими предпочтениями (общие категории)
        if (!userProfile.FavoriteCategoryIds.Any())
        {
            return 0;
        }

        var similarUserIds = await _context.FavoriteLocations
            .Where(fl => !fl.IsDeleted && 
                        userProfile.FavoriteCategoryIds.Contains(fl.Location.CategoryId) &&
                        fl.LocationId != locationId)
            .Select(fl => fl.UserId)
            .Distinct()
            .ToListAsync();

        if (!similarUserIds.Any())
        {
            return 0;
        }

        var favoriteCount = await _context.FavoriteLocations
            .CountAsync(fl => fl.LocationId == locationId && 
                             similarUserIds.Contains(fl.UserId) && 
                             !fl.IsDeleted);

        return favoriteCount;
    }

    private static bool IsBudgetCompatible(string userBudget, string locationPriceRange)
    {
        // Упрощенная проверка совместимости бюджета
        var budgetMap = new Dictionary<string, int>
        {
            { "Free", 0 },
            { "500", 1 },
            { "1000", 2 },
            { "Unlimited", 3 }
        };

        if (!budgetMap.TryGetValue(userBudget, out var userBudgetLevel))
        {
            return true; // Если бюджет неизвестен, не фильтруем
        }

        // Парсим ценовой диапазон локации (может быть "500-1000" или просто "500")
        var locationLevel = 0;
        if (locationPriceRange.Contains("Free", StringComparison.OrdinalIgnoreCase))
            locationLevel = 0;
        else if (locationPriceRange.Contains("500"))
            locationLevel = 1;
        else if (locationPriceRange.Contains("1000"))
            locationLevel = 2;
        else
            locationLevel = 3;

        // Пользователь может позволить себе локации своего уровня и ниже
        return locationLevel <= userBudgetLevel;
    }

    private static bool IsActivityTimeCompatible(string userActivityTime, string workingHours)
    {
        // Упрощенная проверка совместимости времени активности
        var timeMap = new Dictionary<string, string[]>
        {
            { "Morning", new[] { "утро", "morning", "09", "10", "11", "08" } },
            { "Day", new[] { "день", "day", "12", "13", "14", "15", "16", "17" } },
            { "Evening", new[] { "вечер", "evening", "18", "19", "20", "21" } },
            { "Night", new[] { "ночь", "night", "22", "23", "00", "01", "02", "03" } }
        };

        if (!timeMap.TryGetValue(userActivityTime, out var keywords))
        {
            return true; // Если время неизвестно, не фильтруем
        }

        return keywords.Any(keyword => 
            workingHours.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static LocationResponse ToLocationResponse(Location location)
    {
        return new LocationResponse
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Coordinates = location.Coordinates,
            Address = location.Address,
            City = location.City,
            Phone = location.Phone,
            Website = location.Website,
            TelegramImageIds = location.TelegramImageIds,
            Rating = location.Rating,
            RatingCount = location.RatingCount,
            PriceRange = location.PriceRange,
            WorkingHours = location.WorkingHours,
            IsActive = location.IsActive,
            IsVerified = location.IsVerified,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            Category = new LocationResponse.CategoryInfo
            {
                Id = location.Category.Id,
                Name = location.Category.Name,
                IconName = location.Category.IconName
            },
            Subcategories = location.SubcategoryJoins
                .Where(sj => !sj.IsDeleted)
                .Select(sj => new LocationResponse.SubcategoryInfo
                {
                    Id = sj.Subcategory.Id,
                    Name = sj.Subcategory.Name,
                    CategoryId = sj.Subcategory.CategoryId
                })
                .ToList()
        };
    }

    private class UserRecommendationProfile
    {
        public List<int> FavoriteCategoryIds { get; set; } = new();
        public List<int> FavoriteSubcategoryIds { get; set; } = new();
        public List<int> DislikedLocationIds { get; set; } = new();
        public List<int> RejectedLocationIds { get; set; } = new();
        public List<Location> FavoriteLocations { get; set; } = new();
    }

    private class ScoredLocation
    {
        public LocationResponse Location { get; set; } = null!;
        public double Score { get; set; }
    }
}

