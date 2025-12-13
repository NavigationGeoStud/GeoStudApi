using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;
using GeoStud.Api.Helpers;

namespace GeoStud.Api.Services;

public class UserService : IUserService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(GeoStudDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserResponseDto> SubmitUserAsync(string clientId, UserRequest request)
    {
        User? existingUser = null;
        
        if (request.TelegramId.HasValue)
        {
            existingUser = await _context.Users
                .FirstOrDefaultAsync(s => s.TelegramId == request.TelegramId);
        }
        
        if (existingUser == null)
        {
            existingUser = await _context.Users
                .FirstOrDefaultAsync(s => s.Username == request.Username);
        }

        User user;
        if (existingUser != null)
        {
            existingUser.Username = request.Username;
            if (!string.IsNullOrEmpty(request.FirstName))
            {
                existingUser.FirstName = request.FirstName;
            }
            existingUser.TelegramId = request.TelegramId;
            if (request.Age.HasValue)
            {
                existingUser.Age = request.Age.Value;
                if (string.IsNullOrEmpty(request.AgeRange))
                {
                    existingUser.AgeRange = null;
                }
            }
            else if (!string.IsNullOrEmpty(request.AgeRange))
            {
                existingUser.AgeRange = request.AgeRange;
                existingUser.Age = ExtractAgeFromRange(request.AgeRange);
            }
            existingUser.IsStudent = request.IsStudent;
            existingUser.Gender = request.Gender;
            existingUser.IsLocal = request.IsLocal;
            var expandedInterests = InterestCategoryHelper.ExpandCategoriesToSubcategories(request.Interests);
            existingUser.Interests = JsonSerializer.Serialize(expandedInterests);
            existingUser.Budget = request.Budget;
            existingUser.ActivityTime = request.ActivityTime;
            existingUser.SocialPreference = request.SocialPreference;
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            user = existingUser;
        }
        else
        {
            user = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                Email = null,
                PasswordHash = null,
                TelegramId = request.TelegramId,
                Age = request.Age ?? ExtractAgeFromRange(request.AgeRange),
                AgeRange = request.AgeRange,
                IsStudent = request.IsStudent,
                Gender = request.Gender,
                IsLocal = request.IsLocal,
                Interests = JsonSerializer.Serialize(InterestCategoryHelper.ExpandCategoriesToSubcategories(request.Interests)),
                Budget = request.Budget,
                ActivityTime = request.ActivityTime,
                SocialPreference = request.SocialPreference,
                IsActive = true
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync();

        await CreateUserResponses(user.Id, request);

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (user == null)
        {
            return null;
        }

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return users.Select(s =>
        {
            var interests = DeserializeInterests(s.Interests);
            var profilePhotos = DeserializeProfilePhotos(s.ProfilePhotos);
            return ToUserResponse(s, interests, profilePhotos);
        }).ToList();
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(string clientId)
    {
        var username = $"service_{clientId}";
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username && !s.IsDeleted);

        if (user == null)
        {
            return null;
        }

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<UserResponseDto?> GetUserByTelegramIdAsync(long telegramId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.TelegramId == telegramId && !s.IsDeleted);

        if (user == null)
        {
            return null;
        }

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<UserResponseDto> UpdateUserAsync(string clientId, UpdateUserRequest request)
    {
        var username = $"service_{clientId}";
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username && !s.IsDeleted);

        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(clientId));
        }

        // Update only provided fields
        bool hasChanges = false;

        if (!string.IsNullOrEmpty(request.Username) && user.Username != request.Username)
        {
            user.Username = request.Username;
            hasChanges = true;
        }

        if (request.FirstName != null && user.FirstName != request.FirstName)
        {
            user.FirstName = request.FirstName;
            hasChanges = true;
        }

        if (request.TelegramId.HasValue && user.TelegramId != request.TelegramId)
        {
            user.TelegramId = request.TelegramId;
            hasChanges = true;
        }

        // Handle age: prefer Age over AgeRange, but support both for backward compatibility
        if (request.Age.HasValue && user.Age != request.Age.Value)
        {
            user.Age = request.Age.Value;
            hasChanges = true;
        }
        else if (!string.IsNullOrEmpty(request.AgeRange) && user.AgeRange != request.AgeRange)
        {
            user.AgeRange = request.AgeRange;
            // Try to extract age from range if possible (for backward compatibility)
            var extractedAge = ExtractAgeFromRange(request.AgeRange);
            if (extractedAge.HasValue && user.Age != extractedAge.Value)
            {
                user.Age = extractedAge.Value;
            }
            hasChanges = true;
        }

        if (request.IsStudent.HasValue && user.IsStudent != request.IsStudent.Value)
        {
            user.IsStudent = request.IsStudent.Value;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.Gender) && user.Gender != request.Gender)
        {
            user.Gender = request.Gender;
            hasChanges = true;
        }

        if (request.IsLocal.HasValue && user.IsLocal != request.IsLocal.Value)
        {
            user.IsLocal = request.IsLocal.Value;
            hasChanges = true;
        }

        if (request.Interests != null)
        {
            // Expand categories to subcategories automatically
            var expandedInterests = InterestCategoryHelper.ExpandCategoriesToSubcategories(request.Interests);
            var newInterestsJson = JsonSerializer.Serialize(expandedInterests);
            if (user.Interests != newInterestsJson)
            {
                user.Interests = newInterestsJson;
                hasChanges = true;
            }
        }

        if (!string.IsNullOrEmpty(request.Budget) && user.Budget != request.Budget)
        {
            user.Budget = request.Budget;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.ActivityTime) && user.ActivityTime != request.ActivityTime)
        {
            user.ActivityTime = request.ActivityTime;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.SocialPreference) && user.SocialPreference != request.SocialPreference)
        {
            user.SocialPreference = request.SocialPreference;
            hasChanges = true;
        }

        if (request.ProfileDescription != null && user.ProfileDescription != request.ProfileDescription)
        {
            user.ProfileDescription = request.ProfileDescription;
            hasChanges = true;
        }

        if (request.ProfilePhotos != null)
        {
            // Validate max 5 photos
            if (request.ProfilePhotos.Count > 5)
            {
                throw new ArgumentException("Maximum 5 profile photos allowed", nameof(request.ProfilePhotos));
            }

            var newProfilePhotosJson = JsonSerializer.Serialize(request.ProfilePhotos);
            if (user.ProfilePhotos != newProfilePhotosJson)
            {
                user.ProfilePhotos = newProfilePhotosJson;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update analytics responses
            await UpdateUserResponses(user.Id, request);
        }

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<UserResponseDto> UpdateUserByTelegramIdAsync(long telegramId, UpdateUserRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.TelegramId == telegramId && !s.IsDeleted);

        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(telegramId));
        }

        // Update only provided fields
        bool hasChanges = false;

        if (!string.IsNullOrEmpty(request.Username) && user.Username != request.Username)
        {
            user.Username = request.Username;
            hasChanges = true;
        }

        if (request.FirstName != null && user.FirstName != request.FirstName)
        {
            user.FirstName = request.FirstName;
            hasChanges = true;
        }

        if (request.TelegramId.HasValue && user.TelegramId != request.TelegramId)
        {
            user.TelegramId = request.TelegramId;
            hasChanges = true;
        }

        // Handle age: prefer Age over AgeRange, but support both for backward compatibility
        if (request.Age.HasValue && user.Age != request.Age.Value)
        {
            user.Age = request.Age.Value;
            hasChanges = true;
        }
        else if (!string.IsNullOrEmpty(request.AgeRange) && user.AgeRange != request.AgeRange)
        {
            user.AgeRange = request.AgeRange;
            // Try to extract age from range if possible (for backward compatibility)
            var extractedAge = ExtractAgeFromRange(request.AgeRange);
            if (extractedAge.HasValue && user.Age != extractedAge.Value)
            {
                user.Age = extractedAge.Value;
            }
            hasChanges = true;
        }

        if (request.IsStudent.HasValue && user.IsStudent != request.IsStudent.Value)
        {
            user.IsStudent = request.IsStudent.Value;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.Gender) && user.Gender != request.Gender)
        {
            user.Gender = request.Gender;
            hasChanges = true;
        }

        if (request.IsLocal.HasValue && user.IsLocal != request.IsLocal.Value)
        {
            user.IsLocal = request.IsLocal.Value;
            hasChanges = true;
        }

        if (request.Interests != null)
        {
            // Expand categories to subcategories automatically
            var expandedInterests = InterestCategoryHelper.ExpandCategoriesToSubcategories(request.Interests);
            var newInterestsJson = JsonSerializer.Serialize(expandedInterests);
            if (user.Interests != newInterestsJson)
            {
                user.Interests = newInterestsJson;
                hasChanges = true;
            }
        }

        if (!string.IsNullOrEmpty(request.Budget) && user.Budget != request.Budget)
        {
            user.Budget = request.Budget;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.ActivityTime) && user.ActivityTime != request.ActivityTime)
        {
            user.ActivityTime = request.ActivityTime;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.SocialPreference) && user.SocialPreference != request.SocialPreference)
        {
            user.SocialPreference = request.SocialPreference;
            hasChanges = true;
        }

        if (request.ProfileDescription != null && user.ProfileDescription != request.ProfileDescription)
        {
            user.ProfileDescription = request.ProfileDescription;
            hasChanges = true;
        }

        if (request.ProfilePhotos != null)
        {
            // Validate max 5 photos
            if (request.ProfilePhotos.Count > 5)
            {
                throw new ArgumentException("Maximum 5 profile photos allowed", nameof(request.ProfilePhotos));
            }

            var newProfilePhotosJson = JsonSerializer.Serialize(request.ProfilePhotos);
            if (user.ProfilePhotos != newProfilePhotosJson)
            {
                user.ProfilePhotos = newProfilePhotosJson;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update analytics responses
            await UpdateUserResponses(user.Id, request);
        }

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    public async Task<UserResponseDto> UpdateUserFullAsync(string clientId, UserRequest request)
    {
        var username = $"service_{clientId}";
        var user = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username && !s.IsDeleted);

        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(clientId));
        }

        // Update all fields
        user.TelegramId = request.TelegramId;
        // Handle age: prefer Age over AgeRange, but support both for backward compatibility
        if (request.Age.HasValue)
        {
            user.Age = request.Age.Value;
        }
        else if (!string.IsNullOrEmpty(request.AgeRange))
        {
            user.AgeRange = request.AgeRange;
            user.Age = ExtractAgeFromRange(request.AgeRange);
        }
        user.IsStudent = request.IsStudent;
        user.Gender = request.Gender;
        user.IsLocal = request.IsLocal;
        // Expand categories to subcategories automatically
        user.Interests = JsonSerializer.Serialize(InterestCategoryHelper.ExpandCategoriesToSubcategories(request.Interests));
        user.Budget = request.Budget;
        user.ActivityTime = request.ActivityTime;
        user.SocialPreference = request.SocialPreference;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update analytics responses
        await UpdateUserResponses(user.Id, new UpdateUserRequest
        {
            AgeRange = request.AgeRange,
            IsStudent = request.IsStudent,
            Gender = request.Gender,
            IsLocal = request.IsLocal,
            Interests = request.Interests,
            Budget = request.Budget,
            ActivityTime = request.ActivityTime,
            SocialPreference = request.SocialPreference
        });

        var interests = DeserializeInterests(user.Interests);
        var profilePhotos = DeserializeProfilePhotos(user.ProfilePhotos);
        return ToUserResponse(user, interests, profilePhotos);
    }

    private async Task CreateUserResponses(int userId, UserRequest request)
    {
        var responses = new List<UserAnalyticsResponse>
        {
            new() { UserId = userId, Question = "Is Student", Answer = request.IsStudent.ToString(), Category = "Demographics" },
            new() { UserId = userId, Question = "Gender", Answer = request.Gender, Category = "Demographics" },
            new() { UserId = userId, Question = "Is Local", Answer = request.IsLocal.ToString(), Category = "Demographics" },
            new() { UserId = userId, Question = "Interests", Answer = JsonSerializer.Serialize(request.Interests), Category = "Preferences" },
            new() { UserId = userId, Question = "Budget", Answer = request.Budget, Category = "Behavior" },
            new() { UserId = userId, Question = "Activity Time", Answer = request.ActivityTime, Category = "Behavior" },
            new() { UserId = userId, Question = "Social Preference", Answer = request.SocialPreference, Category = "Behavior" }
        };

        // Add Age if provided
        if (request.Age.HasValue)
        {
            responses.Add(new UserAnalyticsResponse
            {
                UserId = userId,
                Question = "Age",
                Answer = request.Age.Value.ToString(),
                Category = "Demographics"
            });
        }

        // Add Age Range if provided (for backward compatibility)
        if (!string.IsNullOrEmpty(request.AgeRange))
        {
            responses.Add(new UserAnalyticsResponse
            {
                UserId = userId,
                Question = "Age Range",
                Answer = request.AgeRange,
                Category = "Demographics"
            });
        }

        _context.UserAnalyticsResponses.AddRange(responses);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateUserResponses(int userId, UpdateUserRequest request)
    {
        // Update existing responses or create new ones if they don't exist
        var existingResponses = await _context.UserAnalyticsResponses
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .ToListAsync();

        // Update or create Age
        if (request.Age.HasValue)
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Age");
            if (response != null)
            {
                response.Answer = request.Age.Value.ToString();
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Age",
                    Answer = request.Age.Value.ToString(),
                    Category = "Demographics"
                });
            }
        }

        // Update or create Age Range (for backward compatibility)
        if (!string.IsNullOrEmpty(request.AgeRange))
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Age Range");
            if (response != null)
            {
                response.Answer = request.AgeRange;
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Age Range",
                    Answer = request.AgeRange,
                    Category = "Demographics"
                });
            }
        }

        // Update or create Is Student
        if (request.IsStudent.HasValue)
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Is Student");
            if (response != null)
            {
                response.Answer = request.IsStudent.Value.ToString();
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Is Student",
                    Answer = request.IsStudent.Value.ToString(),
                    Category = "Demographics"
                });
            }
        }

        // Update or create Gender
        if (!string.IsNullOrEmpty(request.Gender))
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Gender");
            if (response != null)
            {
                response.Answer = request.Gender;
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Gender",
                    Answer = request.Gender,
                    Category = "Demographics"
                });
            }
        }

        // Update or create Is Local
        if (request.IsLocal.HasValue)
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Is Local");
            if (response != null)
            {
                response.Answer = request.IsLocal.Value.ToString();
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Is Local",
                    Answer = request.IsLocal.Value.ToString(),
                    Category = "Demographics"
                });
            }
        }

        // Update or create Interests
        if (request.Interests != null)
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Interests");
            if (response != null)
            {
                response.Answer = JsonSerializer.Serialize(request.Interests);
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Interests",
                    Answer = JsonSerializer.Serialize(request.Interests),
                    Category = "Preferences"
                });
            }
        }

        // Update or create Budget
        if (!string.IsNullOrEmpty(request.Budget))
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Budget");
            if (response != null)
            {
                response.Answer = request.Budget;
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Budget",
                    Answer = request.Budget,
                    Category = "Behavior"
                });
            }
        }

        // Update or create Activity Time
        if (!string.IsNullOrEmpty(request.ActivityTime))
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Activity Time");
            if (response != null)
            {
                response.Answer = request.ActivityTime;
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Activity Time",
                    Answer = request.ActivityTime,
                    Category = "Behavior"
                });
            }
        }

        // Update or create Social Preference
        if (!string.IsNullOrEmpty(request.SocialPreference))
        {
            var response = existingResponses.FirstOrDefault(r => r.Question == "Social Preference");
            if (response != null)
            {
                response.Answer = request.SocialPreference;
                response.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserAnalyticsResponses.Add(new UserAnalyticsResponse
                {
                    UserId = userId,
                    Question = "Social Preference",
                    Answer = request.SocialPreference,
                    Category = "Behavior"
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private static UserResponseDto ToUserResponse(User user, List<string> interests, List<string> profilePhotos)
    {
        return new UserResponseDto
        {
            UserId = user.Id,
            TelegramId = user.TelegramId,
            Username = user.Username,
            FirstName = user.FirstName,
            Email = user.Email,
            AgeRange = user.AgeRange,
            Age = user.Age,
            IsStudent = user.IsStudent,
            Gender = user.Gender,
            IsLocal = user.IsLocal,
            Interests = interests,
            Budget = user.Budget,
            ActivityTime = user.ActivityTime,
            SocialPreference = user.SocialPreference,
            Role = user.Role,
            ProfileDescription = user.ProfileDescription,
            ProfilePhotos = profilePhotos,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? user.CreatedAt
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
    /// Extracts age from age range string (for backward compatibility)
    /// Examples: "17-22" -> 19 (middle), "23-25" -> 24, "30+" -> 30
    /// </summary>
    private static int? ExtractAgeFromRange(string? ageRange)
    {
        if (string.IsNullOrWhiteSpace(ageRange))
            return null;

        // Handle "30+" format
        if (ageRange.EndsWith("+"))
        {
            if (int.TryParse(ageRange.TrimEnd('+'), out var minAge))
            {
                return minAge;
            }
        }

        // Handle "17-22" format
        var parts = ageRange.Split('-');
        if (parts.Length == 2)
        {
            if (int.TryParse(parts[0].Trim(), out var min) && int.TryParse(parts[1].Trim(), out var max))
            {
                // Return middle of the range
                return (min + max) / 2;
            }
        }

        // Try to parse as single number
        if (int.TryParse(ageRange.Trim(), out var singleAge))
        {
            return singleAge;
        }

        return null;
    }
}

