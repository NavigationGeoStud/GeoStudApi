using System.Text.Json;
using GeoStud.Api.Models;

namespace GeoStud.Api.Examples;

public static class TestDataGenerator
{
    public static List<User> GenerateSampleUsers()
    {
        var random = new Random();
        var interests = new[]
        {
            "Учеба", "Спорт", "Искусство", "Еда", "Музыка", "Природа",
            "Технологии", "Путешествия", "Кино", "Книги", "Игры", "Фотография"
        };

        var ageRanges = new[] { "17-22", "23-25", "26-30", "30+" };
        var genders = new[] { "Male", "Female" };
        var budgets = new[] { "Free", "500", "1000", "Unlimited" };
        var activityTimes = new[] { "Morning", "Day", "Evening", "Night" };
        var socialPreferences = new[] { "Alone", "Couple", "Group", "Party" };

        var users = new List<User>();

        for (int i = 1; i <= 50; i++)
        {
            var userInterests = interests
                .OrderBy(x => random.Next())
                .Take(random.Next(2, 5))
                .ToList();

            var user = new User
            {
                Username = $"user{i}",
                Email = $"user{i}@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = $"User{i}",
                LastName = "Test",
                AgeRange = ageRanges[random.Next(ageRanges.Length)],
                IsStudent = random.NextDouble() > 0.2, // 80% are students
                Gender = genders[random.Next(genders.Length)],
                IsLocal = random.NextDouble() > 0.3, // 70% are local
                Interests = JsonSerializer.Serialize(userInterests),
                Budget = budgets[random.Next(budgets.Length)],
                ActivityTime = activityTimes[random.Next(activityTimes.Length)],
                SocialPreference = socialPreferences[random.Next(socialPreferences.Length)],
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
            };

            users.Add(user);
        }

        return users;
    }

    public static void PrintSampleData()
    {
        Console.WriteLine("=== Sample Test Data for GeoStud API ===\n");

        var users = GenerateSampleUsers();

        Console.WriteLine($"Generated {users.Count} sample users\n");

        // Show age distribution
        var ageDistribution = users.GroupBy(s => s.AgeRange)
            .Select(g => new { Age = g.Key, Count = g.Count() })
            .OrderBy(x => x.Age);

        Console.WriteLine("Age Distribution:");
        foreach (var age in ageDistribution)
        {
            Console.WriteLine($"  {age.Age}: {age.Count} users");
        }

        // Show gender distribution
        var genderDistribution = users.GroupBy(s => s.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .OrderBy(x => x.Gender);

        Console.WriteLine("\nGender Distribution:");
        foreach (var gender in genderDistribution)
        {
            Console.WriteLine($"  {gender.Gender}: {gender.Count} users");
        }

        // Show student status
        var studentStatus = users.GroupBy(s => s.IsStudent)
            .Select(g => new { Status = g.Key ? "Student" : "Non-Student", Count = g.Count() })
            .OrderBy(x => x.Status);

        Console.WriteLine("\nStudent Status:");
        foreach (var status in studentStatus)
        {
            Console.WriteLine($"  {status.Status}: {status.Count} users");
        }

        // Show top interests
        var allInterests = new Dictionary<string, int>();
        foreach (var user in users)
        {
            if (!string.IsNullOrEmpty(user.Interests))
            {
                try
                {
                    var interests = JsonSerializer.Deserialize<List<string>>(user.Interests);
                    if (interests != null)
                    {
                        foreach (var interest in interests)
                        {
                            if (allInterests.ContainsKey(interest))
                                allInterests[interest]++;
                            else
                                allInterests[interest] = 1;
                        }
                    }
                }
                catch
                {
                    // Skip invalid JSON
                }
            }
        }

        var topInterests = allInterests
            .OrderByDescending(kvp => kvp.Value)
            .Take(5);

        Console.WriteLine("\nTop 5 Interests:");
        foreach (var interest in topInterests)
        {
            Console.WriteLine($"  {interest.Key}: {interest.Value} users");
        }

        Console.WriteLine("\n=== Sample data generation completed ===");
    }
}
