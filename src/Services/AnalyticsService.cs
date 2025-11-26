using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Analytics;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(GeoStudDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ComprehensiveAnalytics> GetComprehensiveAnalyticsAsync()
    {
        var totalResponses = await _context.Users.CountAsync(s => !s.IsDeleted);

        var demographics = await GetDemographicsAnalyticsAsync();
        var interests = await GetInterestsAnalyticsAsync();
        var behavior = await GetBehaviorAnalyticsAsync();

        var analytics = new ComprehensiveAnalytics
        {
            Demographics = demographics,
            Interests = interests,
            Behavior = behavior,
            TotalResponses = totalResponses,
            GeneratedAt = DateTime.UtcNow
        };

        // Store analytics data for caching
        await StoreAnalyticsData(analytics);

        return analytics;
    }

    public async Task<DemographicsAnalytics> GetDemographicsAnalyticsAsync()
    {
        var totalCount = await _context.Users.CountAsync(s => !s.IsDeleted);

        // Age distribution (using AgeRange for backward compatibility, but prefer Age if available)
        var ageDistribution = await _context.Users
            .Where(s => !s.IsDeleted && s.AgeRange != null)
            .GroupBy(s => s.AgeRange)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Age Distribution",
                Category = "Demographics",
                Value = g.Key ?? "Unknown",
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Gender distribution
        var genderDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.Gender)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Gender Distribution",
                Category = "Demographics",
                Value = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Student status distribution
        var studentStatusDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.IsStudent)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Student Status",
                Category = "Demographics",
                Value = g.Key ? "Student" : "Non-Student",
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Local status distribution
        var localStatusDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.IsLocal)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Local Status",
                Category = "Demographics",
                Value = g.Key ? "Local" : "Non-Local",
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        return new DemographicsAnalytics
        {
            AgeDistribution = ageDistribution,
            GenderDistribution = genderDistribution,
            StudentStatusDistribution = studentStatusDistribution,
            LocalStatusDistribution = localStatusDistribution
        };
    }

    public async Task<InterestsAnalytics> GetInterestsAnalyticsAsync()
    {
        var totalCount = await _context.Users.CountAsync(s => !s.IsDeleted);
        var allInterests = new Dictionary<string, int>();

        // Collect all interests
        var users = await _context.Users
            .Where(s => !s.IsDeleted && !string.IsNullOrEmpty(s.Interests))
            .Select(s => s.Interests)
            .ToListAsync();

        foreach (var interestsJson in users)
        {
            try
            {
                if (!string.IsNullOrEmpty(interestsJson))
                {
                    var interests = JsonSerializer.Deserialize<List<string>>(interestsJson);
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
            }
            catch
            {
                // Skip invalid JSON
            }
        }

        var topInterests = allInterests
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => new AnalyticsResponse
            {
                MetricName = "Top Interests",
                Category = "Interests",
                Value = kvp.Key,
                Count = kvp.Value,
                Percentage = Math.Round((double)kvp.Value / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToList();

        return new InterestsAnalytics
        {
            TopInterests = topInterests,
            InterestCombinations = new List<AnalyticsResponse>() // Could be implemented for combination analysis
        };
    }

    public async Task<BehaviorAnalytics> GetBehaviorAnalyticsAsync()
    {
        var totalCount = await _context.Users.CountAsync(s => !s.IsDeleted);

        // Budget distribution
        var budgetDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.Budget)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Budget Distribution",
                Category = "Behavior",
                Value = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Activity time distribution
        var activityTimeDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.ActivityTime)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Activity Time",
                Category = "Behavior",
                Value = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Social preference distribution
        var socialPreferenceDistribution = await _context.Users
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.SocialPreference)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Social Preference",
                Category = "Behavior",
                Value = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        return new BehaviorAnalytics
        {
            BudgetDistribution = budgetDistribution,
            ActivityTimeDistribution = activityTimeDistribution,
            SocialPreferenceDistribution = socialPreferenceDistribution
        };
    }

    public async Task<IEnumerable<AnalyticsResponse>> GetCachedAnalyticsAsync(string category)
    {
        var cachedData = await _context.AnalyticsData
            .Where(a => a.Category == category && !a.IsDeleted)
            .OrderByDescending(a => a.CalculatedAt)
            .Select(a => new AnalyticsResponse
            {
                MetricName = a.MetricName,
                Category = a.Category,
                Value = a.Value,
                Count = a.Count,
                Percentage = a.Percentage,
                CalculatedAt = a.CalculatedAt
            })
            .ToListAsync();

        return cachedData;
    }

    private async Task StoreAnalyticsData(ComprehensiveAnalytics analytics)
    {
        var analyticsData = new List<AnalyticsData>();

        // Store demographics data
        foreach (var item in analytics.Demographics.AgeDistribution)
        {
            analyticsData.Add(new AnalyticsData
            {
                MetricName = item.MetricName,
                Category = item.Category,
                Value = item.Value,
                Count = item.Count,
                Percentage = item.Percentage,
                CalculatedAt = item.CalculatedAt
            });
        }

        // Store other categories similarly...
        // (Implementation would continue for all analytics data)

        if (analyticsData.Any())
        {
            _context.AnalyticsData.AddRange(analyticsData);
            await _context.SaveChangesAsync();
        }
    }
}

