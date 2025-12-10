using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Common;
using GeoStud.Api.DTOs.People;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class PeopleService : IPeopleService
{
    private readonly GeoStudDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PeopleService> _logger;

    public PeopleService(
        GeoStudDbContext context, 
        INotificationService notificationService,
        ILogger<PeopleService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PagedResponse<UserProfileResponse>> GetCompaniesByLocationAsync(int locationId, long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("GetCompaniesByLocationAsync: locationId={LocationId}, telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            locationId, telegramId, page, pageSize);

        // Validate location exists
        var locationExists = await _context.Locations
            .AnyAsync(l => l.Id == locationId && !l.IsDeleted);
        
        if (!locationExists)
        {
            _logger.LogWarning("Location {LocationId} not found", locationId);
            return new PagedResponse<UserProfileResponse>
            {
                Data = new List<UserProfileResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get current user ID
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            return new PagedResponse<UserProfileResponse>
            {
                Data = new List<UserProfileResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get all users who have this location in favorites (excluding current user)
        var query = _context.FavoriteLocations
            .Where(fl => fl.LocationId == locationId && !fl.IsDeleted)
            .Where(fl => fl.UserId != currentUser.Id)
            .Select(fl => fl.User)
            .Where(u => u != null && !u.IsDeleted && u.IsActive)
            .Distinct();

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var profiles = users.Select(u => ToUserProfileResponse(u)).ToList();

        return new PagedResponse<UserProfileResponse>
        {
            Data = profiles,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleAsync(long telegramId, string? searchBy = null, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("SearchPeopleAsync: telegramId={TelegramId}, searchBy={SearchBy}, page={Page}, pageSize={PageSize}", 
            telegramId, searchBy, page, pageSize);

        // Route to appropriate search method
        if (searchBy == "all")
        {
            return await SearchAllPeopleAsync(telegramId, page, pageSize);
        }
        else if (searchBy == "interests")
        {
            return await SearchPeopleByInterestsAsync(telegramId, page, pageSize);
        }
        else if (searchBy == "locations")
        {
            // Поиск по локациям - только люди с общими локациями
            return await SearchPeopleByLocationsAsync(telegramId, page, pageSize);
        }
        else
        {
            // Default: комбинированный поиск - сначала люди с общими локациями, потом с общими интересами (2+ категории)
            // Это основной режим поиска
            return await SearchPeopleCombinedAsync(telegramId, page, pageSize);
        }
    }

    /// <summary>
    /// Combined search: сначала люди с общими локациями, потом с общими интересами (2+ категории)
    /// </summary>
    private async Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleCombinedAsync(long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("SearchPeopleCombinedAsync: telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            telegramId, page, pageSize);

        // Get current user
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get excluded user IDs
        var currentUserLikedIds = await _context.UserLikes
            .Where(ul => ul.UserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.TargetUserId)
            .ToListAsync();

        var currentUserDislikedIds = await _context.UserDislikes
            .Where(ud => ud.UserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.TargetUserId)
            .ToListAsync();

        var excludedUserIds = new HashSet<int> { currentUser.Id }
            .Union(currentUserLikedIds)
            .Union(currentUserDislikedIds)
            .ToList();

        var allResults = new List<(User User, int Priority, List<int> CommonLocationIds, int MatchingCategoryCount)>();

        // 1. Get users with common locations (Priority 1 - highest)
        var currentUserFavoriteLocationIds = await _context.FavoriteLocations
            .Where(fl => fl.UserId == currentUser.Id && !fl.IsDeleted)
            .Select(fl => fl.LocationId)
            .ToListAsync();

        if (currentUserFavoriteLocationIds.Any())
        {
            var favoriteLocationsQuery = _context.FavoriteLocations
                .Where(fl => currentUserFavoriteLocationIds.Contains(fl.LocationId) && !fl.IsDeleted)
                .Where(fl => !excludedUserIds.Contains(fl.UserId))
                .Include(fl => fl.User)
                .Where(fl => fl.User != null && !fl.User.IsDeleted && fl.User.IsActive)
                .Where(fl => fl.User!.ProfileDescription != null && 
                            !string.IsNullOrEmpty(fl.User.ProfileDescription) &&
                            fl.User.ProfilePhotos != null && 
                            !string.IsNullOrEmpty(fl.User.ProfilePhotos));

            var favoriteLocations = await favoriteLocationsQuery.ToListAsync();

            var usersWithCommonLocations = favoriteLocations
                .GroupBy(fl => fl.UserId)
                .Select(g => new
                {
                    User = g.First().User!,
                    CommonLocationIds = g.Select(fl => fl.LocationId).Distinct().ToList()
                })
                .Where(x => AreUsersGenderCompatible(currentUser, x.User))
                .ToList();

            foreach (var item in usersWithCommonLocations)
            {
                allResults.Add((item.User, 1, item.CommonLocationIds, 0));
                excludedUserIds.Add(item.User.Id); // Exclude from interests search
            }
        }

        // 2. Get users with common interests (2+ categories) (Priority 2)
        var currentUserInterests = DeserializeInterests(currentUser.Interests);
        var currentUserCategories = ExtractInterestCategories(currentUserInterests);

        if (currentUserCategories.Any())
        {
            var allUsers = await _context.Users
                .Where(u => !u.IsDeleted && u.IsActive)
                .Where(u => u.Id != currentUser.Id)
                .Where(u => !excludedUserIds.Contains(u.Id))
                .Where(u => u.ProfileDescription != null && 
                           !string.IsNullOrEmpty(u.ProfileDescription) &&
                           u.ProfilePhotos != null && 
                           !string.IsNullOrEmpty(u.ProfilePhotos))
                .Where(u => u.Interests != null && !string.IsNullOrEmpty(u.Interests))
                .ToListAsync();

            var usersWithMatchingInterests = allUsers
                .Where(u => AreUsersGenderCompatible(currentUser, u))
                .Select(u =>
                {
                    var userInterests = DeserializeInterests(u.Interests);
                    var userCategories = ExtractInterestCategories(userInterests);
                    var matchingCategories = currentUserCategories.Intersect(userCategories).ToList();
                    return new
                    {
                        User = u,
                        MatchingCount = matchingCategories.Count
                    };
                })
                .Where(x => x.MatchingCount >= 2) // Минимум 2 категории
                .OrderByDescending(x => x.MatchingCount)
                .ThenBy(x => x.User.Username)
                .ToList();

            foreach (var item in usersWithMatchingInterests)
            {
                allResults.Add((item.User, 2, new List<int>(), item.MatchingCount));
            }
        }

        // Sort: Priority 1 (locations) first, then Priority 2 (interests)
        var sortedResults = allResults
            .OrderBy(x => x.Priority)
            .ThenByDescending(x => x.MatchingCategoryCount)
            .ThenBy(x => x.User.Username)
            .ToList();

        var totalCount = sortedResults.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedResults = sortedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get location details
        var allLocationIds = pagedResults
            .SelectMany(r => r.CommonLocationIds)
            .Distinct()
            .ToList();

        var locations = allLocationIds.Any()
            ? await _context.Locations
                .Where(l => allLocationIds.Contains(l.Id) && !l.IsDeleted)
                .ToListAsync()
            : new List<Location>();

        // Build response
        var profiles = pagedResults.Select(r =>
        {
            var profile = ToUserProfileWithLocationsResponse(r.User);
            profile.MatchingLocations = r.CommonLocationIds
                .Select(locId =>
                {
                    var loc = locations.FirstOrDefault(l => l.Id == locId);
                    return loc != null
                        ? new UserProfileWithLocationsResponse.LocationInfo
                        {
                            Name = loc.Name
                        }
                        : null;
                })
                .Where(loc => loc != null)
                .Cast<UserProfileWithLocationsResponse.LocationInfo>()
                .ToList();
            return profile;
        }).ToList();

        return new PagedResponse<UserProfileWithLocationsResponse>
        {
            Data = profiles,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleByLocationsAsync(long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("SearchPeopleByLocationsAsync: telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            telegramId, page, pageSize);

        // Get current user
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get current user's favorite location IDs (only non-deleted)
        var currentUserFavoriteLocationIds = await _context.FavoriteLocations
            .Where(fl => fl.UserId == currentUser.Id && !fl.IsDeleted)
            .Select(fl => fl.LocationId)
            .ToListAsync();

        if (!currentUserFavoriteLocationIds.Any())
        {
            _logger.LogDebug("User {TelegramId} has no favorite locations", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get users who have liked/disliked current user
        var likedUserIds = await _context.UserLikes
            .Where(ul => ul.TargetUserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.UserId)
            .ToListAsync();

        var dislikedUserIds = await _context.UserDislikes
            .Where(ud => ud.TargetUserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.UserId)
            .ToListAsync();

        // Get users who current user has liked/disliked
        var currentUserLikedIds = await _context.UserLikes
            .Where(ul => ul.UserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.TargetUserId)
            .ToListAsync();

        var currentUserDislikedIds = await _context.UserDislikes
            .Where(ud => ud.UserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.TargetUserId)
            .ToListAsync();

        // Get all users with common favorite locations
        // Exclude: current user, users already liked/disliked by current user
        var excludedUserIds = new HashSet<int> { currentUser.Id }
            .Union(currentUserLikedIds)
            .Union(currentUserDislikedIds)
            .ToList();

        // Get all favorite locations for users with common locations
        // Filter by users with filled profiles (profileDescription and profilePhotos not empty)
        var favoriteLocationsQuery = _context.FavoriteLocations
            .Where(fl => currentUserFavoriteLocationIds.Contains(fl.LocationId) && !fl.IsDeleted)
            .Where(fl => !excludedUserIds.Contains(fl.UserId))
            .Include(fl => fl.User)
            .Where(fl => fl.User != null && !fl.User.IsDeleted && fl.User.IsActive)
            .Where(fl => fl.User!.ProfileDescription != null && 
                        !string.IsNullOrEmpty(fl.User.ProfileDescription) &&
                        fl.User.ProfilePhotos != null && 
                        !string.IsNullOrEmpty(fl.User.ProfilePhotos));

        var favoriteLocations = await favoriteLocationsQuery.ToListAsync();

        // Group by user and get common locations, filter by gender compatibility
        var usersWithCommonLocations = favoriteLocations
            .GroupBy(fl => fl.UserId)
            .Select(g => new
            {
                User = g.First().User!,
                CommonLocationIds = g.Select(fl => fl.LocationId).Distinct().ToList(),
                CommonLocationCount = g.Select(fl => fl.LocationId).Distinct().Count()
            })
            .Where(x => AreUsersGenderCompatible(currentUser, x.User))
            .OrderByDescending(x => x.CommonLocationCount)
            .ThenBy(x => x.User.Username)
            .ToList();

        var totalCount = usersWithCommonLocations.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedUsers = usersWithCommonLocations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get location details for matching locations
        var allLocationIds = pagedUsers
            .SelectMany(u => u.CommonLocationIds)
            .Distinct()
            .ToList();

        var locations = await _context.Locations
            .Where(l => allLocationIds.Contains(l.Id) && !l.IsDeleted)
            .ToListAsync();

        var profiles = pagedUsers.Select(x =>
        {
            var profile = ToUserProfileWithLocationsResponse(x.User);
            profile.MatchingLocations = x.CommonLocationIds
                .Select(locId =>
                {
                    var loc = locations.FirstOrDefault(l => l.Id == locId);
                    return loc != null
                        ? new UserProfileWithLocationsResponse.LocationInfo
                        {
                            Name = loc.Name
                        }
                        : null;
                })
                .Where(loc => loc != null)
                .Cast<UserProfileWithLocationsResponse.LocationInfo>()
                .ToList();
            return profile;
        }).ToList();

        return new PagedResponse<UserProfileWithLocationsResponse>
        {
            Data = profiles,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<PagedResponse<UserProfileWithLocationsResponse>> SearchPeopleByInterestsAsync(long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("SearchPeopleByInterestsAsync: telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            telegramId, page, pageSize);

        // Get current user
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get current user's interests and extract categories
        var currentUserInterests = DeserializeInterests(currentUser.Interests);
        var currentUserCategories = ExtractInterestCategories(currentUserInterests);

        if (!currentUserCategories.Any())
        {
            _logger.LogDebug("User {TelegramId} has no interests", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get users who current user has liked/disliked
        var currentUserLikedIds = await _context.UserLikes
            .Where(ul => ul.UserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.TargetUserId)
            .ToListAsync();

        var currentUserDislikedIds = await _context.UserDislikes
            .Where(ud => ud.UserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.TargetUserId)
            .ToListAsync();

        // Get excluded user IDs
        var excludedUserIds = new HashSet<int> { currentUser.Id }
            .Union(currentUserLikedIds)
            .Union(currentUserDislikedIds)
            .ToList();

        // Get all users with filled profiles and interests
        var allUsers = await _context.Users
            .Where(u => !u.IsDeleted && u.IsActive)
            .Where(u => u.Id != currentUser.Id)
            .Where(u => !excludedUserIds.Contains(u.Id))
            .Where(u => u.ProfileDescription != null && 
                       !string.IsNullOrEmpty(u.ProfileDescription) &&
                       u.ProfilePhotos != null && 
                       !string.IsNullOrEmpty(u.ProfilePhotos))
            .Where(u => u.Interests != null && !string.IsNullOrEmpty(u.Interests))
            .ToListAsync();

        // Calculate matching interests for each user, filter by gender compatibility
        var usersWithMatchingInterests = allUsers
            .Where(u => AreUsersGenderCompatible(currentUser, u))
            .Select(u =>
            {
                var userInterests = DeserializeInterests(u.Interests);
                var userCategories = ExtractInterestCategories(userInterests);
                
                // Count matching categories
                var matchingCategories = currentUserCategories
                    .Intersect(userCategories)
                    .ToList();

                return new
                {
                    User = u,
                    MatchingCategories = matchingCategories,
                    MatchingCount = matchingCategories.Count
                };
            })
            .Where(x => x.MatchingCount >= 2) // Минимум 2 категории
            .OrderByDescending(x => x.MatchingCount)
            .ThenBy(x => x.User.Username)
            .ToList();

        var totalCount = usersWithMatchingInterests.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedUsers = usersWithMatchingInterests
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get favorite locations for paged users to populate matchingLocations
        var pagedUserIds = pagedUsers.Select(u => u.User.Id).ToList();
        var currentUserFavoriteLocationIds = await _context.FavoriteLocations
            .Where(fl => fl.UserId == currentUser.Id && !fl.IsDeleted)
            .Select(fl => fl.LocationId)
            .ToListAsync();

        var favoriteLocations = await _context.FavoriteLocations
            .Where(fl => pagedUserIds.Contains(fl.UserId) && 
                         currentUserFavoriteLocationIds.Contains(fl.LocationId) && 
                         !fl.IsDeleted)
            .Include(fl => fl.Location)
            .ToListAsync();

        var allLocationIds = favoriteLocations
            .Select(fl => fl.LocationId)
            .Distinct()
            .ToList();

        var locations = await _context.Locations
            .Where(l => allLocationIds.Contains(l.Id) && !l.IsDeleted)
            .ToListAsync();

        var profiles = pagedUsers.Select(x =>
        {
            var profile = ToUserProfileWithLocationsResponse(x.User);
            
            // Get matching locations for this user
            var userFavoriteLocations = favoriteLocations
                .Where(fl => fl.UserId == x.User.Id)
                .Select(fl => fl.LocationId)
                .Distinct()
                .ToList();

            profile.MatchingLocations = userFavoriteLocations
                .Select(locId =>
                {
                    var loc = locations.FirstOrDefault(l => l.Id == locId);
                    return loc != null
                        ? new UserProfileWithLocationsResponse.LocationInfo
                        {
                            Name = loc.Name
                        }
                        : null;
                })
                .Where(loc => loc != null)
                .Cast<UserProfileWithLocationsResponse.LocationInfo>()
                .ToList();

            return profile;
        }).ToList();

        return new PagedResponse<UserProfileWithLocationsResponse>
        {
            Data = profiles,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<PagedResponse<UserProfileWithLocationsResponse>> SearchAllPeopleAsync(long telegramId, int page = 1, int pageSize = 20)
    {
        _logger.LogDebug("SearchAllPeopleAsync: telegramId={TelegramId}, page={Page}, pageSize={PageSize}", 
            telegramId, page, pageSize);

        // Get current user
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            return new PagedResponse<UserProfileWithLocationsResponse>
            {
                Data = new List<UserProfileWithLocationsResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        // Get users who current user has liked/disliked
        var currentUserLikedIds = await _context.UserLikes
            .Where(ul => ul.UserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.TargetUserId)
            .ToListAsync();

        var currentUserDislikedIds = await _context.UserDislikes
            .Where(ud => ud.UserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.TargetUserId)
            .ToListAsync();

        // Get users who have liked/disliked current user (exclude them too to avoid showing people who already interacted)
        var likedUserIds = await _context.UserLikes
            .Where(ul => ul.TargetUserId == currentUser.Id && !ul.IsDeleted)
            .Select(ul => ul.UserId)
            .ToListAsync();

        var dislikedUserIds = await _context.UserDislikes
            .Where(ud => ud.TargetUserId == currentUser.Id && !ud.IsDeleted)
            .Select(ud => ud.UserId)
            .ToListAsync();

        // Combine all excluded user IDs
        var excludedUserIds = new HashSet<int> { currentUser.Id }
            .Union(currentUserLikedIds)
            .Union(currentUserDislikedIds)
            .Union(likedUserIds)
            .Union(dislikedUserIds)
            .ToList();

        // Get all users with filled profiles (simple search like dating apps)
        // First get all candidates, then filter by gender compatibility in memory
        var allCandidates = await _context.Users
            .Where(u => !u.IsDeleted && u.IsActive)
            .Where(u => u.Id != currentUser.Id)
            .Where(u => !excludedUserIds.Contains(u.Id))
            .Where(u => u.ProfileDescription != null && 
                       !string.IsNullOrEmpty(u.ProfileDescription) &&
                       u.ProfilePhotos != null && 
                       !string.IsNullOrEmpty(u.ProfilePhotos))
            .ToListAsync();

        // Filter by gender compatibility
        var compatibleUsers = allCandidates
            .Where(u => AreUsersGenderCompatible(currentUser, u))
            .ToList();

        var totalCount = compatibleUsers.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = compatibleUsers
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get favorite locations for current user and paged users to populate matchingLocations
        var currentUserFavoriteLocationIds = await _context.FavoriteLocations
            .Where(fl => fl.UserId == currentUser.Id && !fl.IsDeleted)
            .Select(fl => fl.LocationId)
            .ToListAsync();

        var pagedUserIds = users.Select(u => u.Id).ToList();
        var favoriteLocations = await _context.FavoriteLocations
            .Where(fl => pagedUserIds.Contains(fl.UserId) && 
                         currentUserFavoriteLocationIds.Contains(fl.LocationId) && 
                         !fl.IsDeleted)
            .Include(fl => fl.Location)
            .ToListAsync();

        var allLocationIds = favoriteLocations
            .Select(fl => fl.LocationId)
            .Distinct()
            .ToList();

        var locations = await _context.Locations
            .Where(l => allLocationIds.Contains(l.Id) && !l.IsDeleted)
            .ToListAsync();

        var profiles = users.Select(u =>
        {
            var profile = ToUserProfileWithLocationsResponse(u);
            
            // Get matching locations for this user
            var userFavoriteLocations = favoriteLocations
                .Where(fl => fl.UserId == u.Id)
                .Select(fl => fl.LocationId)
                .Distinct()
                .ToList();

            profile.MatchingLocations = userFavoriteLocations
                .Select(locId =>
                {
                    var loc = locations.FirstOrDefault(l => l.Id == locId);
                    return loc != null
                        ? new UserProfileWithLocationsResponse.LocationInfo
                        {
                            Name = loc.Name
                        }
                        : null;
                })
                .Where(loc => loc != null)
                .Cast<UserProfileWithLocationsResponse.LocationInfo>()
                .ToList();

            return profile;
        }).ToList();

        return new PagedResponse<UserProfileWithLocationsResponse>
        {
            Data = profiles,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<LikeResponse> LikeUserAsync(long telegramId, long targetTelegramId, string? message = null)
    {
        _logger.LogDebug("LikeUserAsync: telegramId={TelegramId}, targetTelegramId={TargetTelegramId}, message={Message}", 
            telegramId, targetTelegramId, message);

        // Validate users
        if (telegramId == targetTelegramId)
        {
            throw new ArgumentException("Cannot like yourself", nameof(targetTelegramId));
        }

        // Validate message length
        if (!string.IsNullOrEmpty(message) && message.Length > 500)
        {
            throw new ArgumentException("Message cannot exceed 500 characters", nameof(message));
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            throw new ArgumentException("User not found", nameof(telegramId));
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == targetTelegramId && !u.IsDeleted);
        
        if (targetUser == null)
        {
            throw new ArgumentException("Target user not found", nameof(targetTelegramId));
        }

        // Check if already liked
        var existingLike = await _context.UserLikes
            .FirstOrDefaultAsync(ul => ul.UserId == currentUser.Id && 
                                       ul.TargetUserId == targetUser.Id && 
                                       !ul.IsDeleted);

        if (existingLike != null)
        {
            _logger.LogDebug("User {UserId} already liked user {TargetUserId}", currentUser.Id, targetUser.Id);
            // Check if it's a match
            var reverseLike = await _context.UserLikes
                .FirstOrDefaultAsync(ul => ul.UserId == targetUser.Id && 
                                          ul.TargetUserId == currentUser.Id && 
                                          !ul.IsDeleted);
            
            if (reverseLike != null)
            {
                // It's a match, get match info
                var match = await _context.Matches
                    .FirstOrDefaultAsync(m => 
                        ((m.UserId1 == Math.Min(currentUser.Id, targetUser.Id) && 
                          m.UserId2 == Math.Max(currentUser.Id, targetUser.Id)) ||
                         (m.UserId1 == Math.Max(currentUser.Id, targetUser.Id) && 
                          m.UserId2 == Math.Min(currentUser.Id, targetUser.Id))) &&
                        !m.IsDeleted);

                if (match != null)
                {
                    return new LikeResponse
                    {
                        Success = true,
                        IsMatch = true,
                        Match = new LikeResponse.MatchInfo
                        {
                            Id = match.Id,
                            CreatedAt = match.CreatedAt,
                            User1 = new LikeResponse.UserInfo
                            {
                                TelegramId = currentUser.TelegramId ?? 0,
                                Username = currentUser.Username,
                                FirstName = currentUser.FirstName
                            },
                            User2 = new LikeResponse.UserInfo
                            {
                                TelegramId = targetUser.TelegramId ?? 0,
                                Username = targetUser.Username,
                                FirstName = targetUser.FirstName
                            }
                        }
                    };
                }
            }
            
            return new LikeResponse
            {
                Success = true,
                IsMatch = false
            };
        }

        // Check if it's a mutual like BEFORE creating the like
        // This is the key: if target user already liked current user, it's a match, not a like notification
        var reverseLikeExists = await _context.UserLikes
            .FirstOrDefaultAsync(ul => ul.UserId == targetUser.Id && 
                                      ul.TargetUserId == currentUser.Id && 
                                      !ul.IsDeleted);

        // Create like
        var like = new UserLike
        {
            UserId = currentUser.Id,
            TargetUserId = targetUser.Id,
            Message = message
        };

        _context.UserLikes.Add(like);
        await _context.SaveChangesAsync();

        if (reverseLikeExists != null)
        {
            // It's a mutual like - create match and match notifications, NOT like notification
            var match = new Match
            {
                UserId1 = Math.Min(currentUser.Id, targetUser.Id),
                UserId2 = Math.Max(currentUser.Id, targetUser.Id)
            };

            // Check if match already exists
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => 
                    ((m.UserId1 == match.UserId1 && m.UserId2 == match.UserId2) ||
                     (m.UserId1 == match.UserId2 && m.UserId2 == match.UserId1)) &&
                    !m.IsDeleted);

            if (existingMatch == null)
            {
                _context.Matches.Add(match);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Match created between users {UserId1} and {UserId2}", 
                    currentUser.Id, targetUser.Id);

                // Create match notifications for both users (NOT like notification)
                try
                {
                    await _notificationService.CreateMatchNotificationAsync(
                        targetTelegramId, 
                        telegramId);
                    await _notificationService.CreateMatchNotificationAsync(
                        telegramId, 
                        targetTelegramId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create match notifications");
                    // Don't fail the match operation if notification creation fails
                }
            }

            // Get the match for response
            var createdMatch = await _context.Matches
                .FirstOrDefaultAsync(m => 
                    ((m.UserId1 == match.UserId1 && m.UserId2 == match.UserId2) ||
                     (m.UserId1 == match.UserId2 && m.UserId2 == match.UserId1)) &&
                    !m.IsDeleted);

            return new LikeResponse
            {
                Success = true,
                IsMatch = true,
                Match = createdMatch != null ? new LikeResponse.MatchInfo
                {
                    Id = createdMatch.Id,
                    CreatedAt = createdMatch.CreatedAt,
                    User1 = new LikeResponse.UserInfo
                    {
                        TelegramId = currentUser.TelegramId ?? 0,
                        Username = currentUser.Username,
                        FirstName = currentUser.FirstName
                    },
                    User2 = new LikeResponse.UserInfo
                    {
                        TelegramId = targetUser.TelegramId ?? 0,
                        Username = targetUser.Username,
                        FirstName = targetUser.FirstName
                    }
                } : null
            };
        }
        else
        {
            // Not a mutual like - create like notification (with message if provided)
            try
            {
                await _notificationService.CreateLikeNotificationAsync(
                    targetTelegramId, 
                    telegramId,
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create like notification");
                // Don't fail the like operation if notification creation fails
            }
        }

        return new LikeResponse
        {
            Success = true,
            IsMatch = false
        };
    }

    public async Task<bool> DislikeUserAsync(long telegramId, long targetTelegramId)
    {
        _logger.LogDebug("DislikeUserAsync: telegramId={TelegramId}, targetTelegramId={TargetTelegramId}", 
            telegramId, targetTelegramId);

        // Validate users
        if (telegramId == targetTelegramId)
        {
            throw new ArgumentException("Cannot dislike yourself", nameof(targetTelegramId));
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId && !u.IsDeleted);
        
        if (currentUser == null)
        {
            throw new ArgumentException("User not found", nameof(telegramId));
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == targetTelegramId && !u.IsDeleted);
        
        if (targetUser == null)
        {
            throw new ArgumentException("Target user not found", nameof(targetTelegramId));
        }

        // Check if already disliked
        var existingDislike = await _context.UserDislikes
            .FirstOrDefaultAsync(ud => ud.UserId == currentUser.Id && 
                                       ud.TargetUserId == targetUser.Id && 
                                       !ud.IsDeleted);

        if (existingDislike != null)
        {
            _logger.LogDebug("User {UserId} already disliked user {TargetUserId}", currentUser.Id, targetUser.Id);
            return true;
        }

        // Create dislike
        var dislike = new UserDislike
        {
            UserId = currentUser.Id,
            TargetUserId = targetUser.Id
        };

        _context.UserDislikes.Add(dislike);
        await _context.SaveChangesAsync();

        return true;
    }

    private static UserProfileResponse ToUserProfileResponse(User user)
    {
        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        
        return new UserProfileResponse
        {
            TelegramId = user.TelegramId ?? 0,
            Username = user.Username,
            FirstName = user.FirstName,
            AgeRange = user.AgeRange,
            Age = user.Age,
            Gender = user.Gender,
            IsStudent = user.IsStudent,
            Interests = interests,
            ProfileDescription = user.ProfileDescription,
            ProfilePhotos = profilePhotos
        };
    }

    private static UserProfileWithLocationsResponse ToUserProfileWithLocationsResponse(User user)
    {
        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        
        return new UserProfileWithLocationsResponse
        {
            TelegramId = user.TelegramId ?? 0,
            Username = user.Username,
            FirstName = user.FirstName,
            AgeRange = user.AgeRange,
            Age = user.Age,
            Gender = user.Gender,
            IsStudent = user.IsStudent,
            Interests = interests,
            ProfileDescription = user.ProfileDescription,
            ProfilePhotos = profilePhotos,
            MatchingLocations = new List<UserProfileWithLocationsResponse.LocationInfo>()
        };
    }

    private static List<string> DeserializeInterests(string? interestsJson)
    {
        return string.IsNullOrEmpty(interestsJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(interestsJson) ?? new List<string>();
    }

    private static List<string> DeserializeProfilePhotos(string? profilePhotosJson)
    {
        return string.IsNullOrEmpty(profilePhotosJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(profilePhotosJson) ?? new List<string>();
    }

    /// <summary>
    /// Extracts interest categories from interests list, ignoring subgenres.
    /// For example: "theatre:drama" -> "theatre", "movie:comedy" -> "movie", "concerts" -> "concerts"
    /// </summary>
    private static HashSet<string> ExtractInterestCategories(List<string> interests)
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var interest in interests)
        {
            if (string.IsNullOrWhiteSpace(interest))
                continue;

            // Split by colon to separate category from subgenre
            var parts = interest.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var category = parts[0].Trim().ToLowerInvariant();
            
            if (!string.IsNullOrEmpty(category))
            {
                categories.Add(category);
            }
        }

        return categories;
    }

    /// <summary>
    /// Checks if two users are compatible based on gender preferences (SocialPreference)
    /// Current user's SocialPreference determines who they're looking for
    /// Target user's SocialPreference must also match current user's gender (mutual compatibility)
    /// </summary>
    private static bool AreUsersGenderCompatible(User currentUser, User targetUser)
    {
        // If current user is looking for "alone", they don't want to see anyone
        if (currentUser.SocialPreference.Equals("alone", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If current user is looking for "any", they accept everyone (but still need to check target user's preference)
        if (currentUser.SocialPreference.Equals("any", StringComparison.OrdinalIgnoreCase))
        {
            // Check if target user also accepts current user's gender
            return DoesUserAcceptGender(targetUser, currentUser.Gender);
        }

        // Current user is looking for specific gender
        var lookingForGender = currentUser.SocialPreference.Equals("male", StringComparison.OrdinalIgnoreCase) 
            ? "Male" 
            : currentUser.SocialPreference.Equals("female", StringComparison.OrdinalIgnoreCase) 
                ? "Female" 
                : null;

        if (lookingForGender == null)
        {
            // Unknown preference, allow by default
            return true;
        }

        // Check if target user's gender matches what current user is looking for
        if (!targetUser.Gender.Equals(lookingForGender, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Also check if target user accepts current user's gender (mutual compatibility)
        return DoesUserAcceptGender(targetUser, currentUser.Gender);
    }

    /// <summary>
    /// Checks if a user accepts a specific gender based on their SocialPreference
    /// </summary>
    private static bool DoesUserAcceptGender(User user, string gender)
    {
        // If user is looking for "alone", they don't accept anyone
        if (user.SocialPreference.Equals("alone", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If user is looking for "any", they accept everyone
        if (user.SocialPreference.Equals("any", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check if user's preference matches the gender
        var lookingForGender = user.SocialPreference.Equals("male", StringComparison.OrdinalIgnoreCase) 
            ? "Male" 
            : user.SocialPreference.Equals("female", StringComparison.OrdinalIgnoreCase) 
                ? "Female" 
                : null;

        if (lookingForGender == null)
        {
            // Unknown preference, allow by default
            return true;
        }

        return gender.Equals(lookingForGender, StringComparison.OrdinalIgnoreCase);
    }
}

