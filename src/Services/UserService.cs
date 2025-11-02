using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.User;
using UserResponseDto = GeoStud.Api.DTOs.User.UserResponse;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

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
        // Check if user already exists
        var username = $"service_{clientId}";
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(s => s.Username == username);

        User user;
        if (existingUser != null)
        {
            // Update existing user
            existingUser.TelegramId = request.TelegramId;
            existingUser.AgeRange = request.AgeRange;
            existingUser.IsStudent = request.IsStudent;
            existingUser.Gender = request.Gender;
            existingUser.IsLocal = request.IsLocal;
            existingUser.Interests = JsonSerializer.Serialize(request.Interests);
            existingUser.Budget = request.Budget;
            existingUser.ActivityTime = request.ActivityTime;
            existingUser.SocialPreference = request.SocialPreference;
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            user = existingUser;
        }
        else
        {
            // Create new user
            user = new User
            {
                Username = username,
                Email = $"{clientId}@service.local",
                PasswordHash = "service_user", // Not used for service users
                TelegramId = request.TelegramId,
                AgeRange = request.AgeRange,
                IsStudent = request.IsStudent,
                Gender = request.Gender,
                IsLocal = request.IsLocal,
                Interests = JsonSerializer.Serialize(request.Interests),
                Budget = request.Budget,
                ActivityTime = request.ActivityTime,
                SocialPreference = request.SocialPreference,
                IsActive = true
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync();

        // Create individual responses for analytics
        await CreateUserResponses(user.Id, request);

        return ToUserResponse(user, request.Interests);
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
        return ToUserResponse(user, interests);
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
            return ToUserResponse(s, interests);
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
        return ToUserResponse(user, interests);
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
        return ToUserResponse(user, interests);
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

        if (request.TelegramId.HasValue && user.TelegramId != request.TelegramId)
        {
            user.TelegramId = request.TelegramId;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.AgeRange) && user.AgeRange != request.AgeRange)
        {
            user.AgeRange = request.AgeRange;
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
            var newInterestsJson = JsonSerializer.Serialize(request.Interests);
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

        if (hasChanges)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update analytics responses
            await UpdateUserResponses(user.Id, request);
        }

        var interests = DeserializeInterests(user.Interests);
        return ToUserResponse(user, interests);
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
        user.AgeRange = request.AgeRange;
        user.IsStudent = request.IsStudent;
        user.Gender = request.Gender;
        user.IsLocal = request.IsLocal;
        user.Interests = JsonSerializer.Serialize(request.Interests);
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

        return ToUserResponse(user, request.Interests);
    }

    private async Task CreateUserResponses(int userId, UserRequest request)
    {
        var responses = new List<UserAnalyticsResponse>
        {
            new() { UserId = userId, Question = "Age Range", Answer = request.AgeRange, Category = "Demographics" },
            new() { UserId = userId, Question = "Is Student", Answer = request.IsStudent.ToString(), Category = "Demographics" },
            new() { UserId = userId, Question = "Gender", Answer = request.Gender, Category = "Demographics" },
            new() { UserId = userId, Question = "Is Local", Answer = request.IsLocal.ToString(), Category = "Demographics" },
            new() { UserId = userId, Question = "Interests", Answer = JsonSerializer.Serialize(request.Interests), Category = "Preferences" },
            new() { UserId = userId, Question = "Budget", Answer = request.Budget, Category = "Behavior" },
            new() { UserId = userId, Question = "Activity Time", Answer = request.ActivityTime, Category = "Behavior" },
            new() { UserId = userId, Question = "Social Preference", Answer = request.SocialPreference, Category = "Behavior" }
        };

        _context.UserAnalyticsResponses.AddRange(responses);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateUserResponses(int userId, UpdateUserRequest request)
    {
        // Update existing responses or create new ones if they don't exist
        var existingResponses = await _context.UserAnalyticsResponses
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .ToListAsync();

        // Update or create Age Range
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

    private static UserResponseDto ToUserResponse(User user, List<string> interests)
    {
        return new UserResponseDto
        {
            UserId = user.Id,
            TelegramId = user.TelegramId,
            Username = user.Username,
            Email = user.Email,
            AgeRange = user.AgeRange,
            IsStudent = user.IsStudent,
            Gender = user.Gender,
            IsLocal = user.IsLocal,
            Interests = interests,
            Budget = user.Budget,
            ActivityTime = user.ActivityTime,
            SocialPreference = user.SocialPreference,
            CreatedAt = user.CreatedAt
        };
    }

    private static List<string> DeserializeInterests(string? interestsJson)
    {
        return string.IsNullOrEmpty(interestsJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(interestsJson) ?? new List<string>();
    }
}

