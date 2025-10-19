using System.Text.Json;
using GeoStud.Api.Models;

namespace GeoStud.Api.Examples;

public static class TestDataGenerator
{
    public static List<Student> GenerateSampleStudents()
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

        var students = new List<Student>();

        for (int i = 1; i <= 50; i++)
        {
            var studentInterests = interests
                .OrderBy(x => random.Next())
                .Take(random.Next(2, 5))
                .ToList();

            var student = new Student
            {
                Username = $"student{i}",
                Email = $"student{i}@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = $"Student{i}",
                LastName = "Test",
                AgeRange = ageRanges[random.Next(ageRanges.Length)],
                IsStudent = random.NextDouble() > 0.2, // 80% are students
                Gender = genders[random.Next(genders.Length)],
                IsLocal = random.NextDouble() > 0.3, // 70% are local
                Interests = JsonSerializer.Serialize(studentInterests),
                Budget = budgets[random.Next(budgets.Length)],
                ActivityTime = activityTimes[random.Next(activityTimes.Length)],
                SocialPreference = socialPreferences[random.Next(socialPreferences.Length)],
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
            };

            students.Add(student);
        }

        return students;
    }

    public static void PrintSampleData()
    {
        Console.WriteLine("=== Sample Test Data for GeoStud API ===\n");

        var students = GenerateSampleStudents();

        Console.WriteLine($"Generated {students.Count} sample students\n");

        // Show age distribution
        var ageDistribution = students.GroupBy(s => s.AgeRange)
            .Select(g => new { Age = g.Key, Count = g.Count() })
            .OrderBy(x => x.Age);

        Console.WriteLine("Age Distribution:");
        foreach (var age in ageDistribution)
        {
            Console.WriteLine($"  {age.Age}: {age.Count} students");
        }

        // Show gender distribution
        var genderDistribution = students.GroupBy(s => s.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .OrderBy(x => x.Gender);

        Console.WriteLine("\nGender Distribution:");
        foreach (var gender in genderDistribution)
        {
            Console.WriteLine($"  {gender.Gender}: {gender.Count} students");
        }

        // Show student status
        var studentStatus = students.GroupBy(s => s.IsStudent)
            .Select(g => new { Status = g.Key ? "Student" : "Non-Student", Count = g.Count() })
            .OrderBy(x => x.Status);

        Console.WriteLine("\nStudent Status:");
        foreach (var status in studentStatus)
        {
            Console.WriteLine($"  {status.Status}: {status.Count} students");
        }

        // Show top interests
        var allInterests = new Dictionary<string, int>();
        foreach (var student in students)
        {
            if (!string.IsNullOrEmpty(student.Interests))
            {
                try
                {
                    var interests = JsonSerializer.Deserialize<List<string>>(student.Interests);
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
            Console.WriteLine($"  {interest.Key}: {interest.Value} students");
        }

        Console.WriteLine("\n=== Sample data generation completed ===");
    }
}
