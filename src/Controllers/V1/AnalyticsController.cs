using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Analytics;
using GeoStud.Api.Models;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(GeoStudDbContext context, ILogger<AnalyticsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive analytics data
    /// </summary>
    /// <returns>Complete analytics report</returns>
    [HttpGet("comprehensive")]
    [ProducesResponseType(typeof(ComprehensiveAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComprehensiveAnalytics()
    {
        try
        {
            var totalResponses = await _context.Students.CountAsync(s => !s.IsDeleted);

            var demographics = await GetDemographicsAnalytics();
            var interests = await GetInterestsAnalytics();
            var behavior = await GetBehaviorAnalytics();

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

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get demographics analytics
    /// </summary>
    /// <returns>Demographics analytics</returns>
    [HttpGet("demographics")]
    [ProducesResponseType(typeof(DemographicsAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDemographicsAnalyticsEndpoint()
    {
        try
        {
            var analytics = await GetDemographicsAnalytics();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving demographics analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get interests analytics
    /// </summary>
    /// <returns>Interests analytics</returns>
    [HttpGet("interests")]
    [ProducesResponseType(typeof(InterestsAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInterestsAnalyticsEndpoint()
    {
        try
        {
            var analytics = await GetInterestsAnalytics();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interests analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get behavior analytics
    /// </summary>
    /// <returns>Behavior analytics</returns>
    [HttpGet("behavior")]
    [ProducesResponseType(typeof(BehaviorAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBehaviorAnalyticsEndpoint()
    {
        try
        {
            var analytics = await GetBehaviorAnalytics();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving behavior analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get cached analytics data
    /// </summary>
    /// <param name="category">Analytics category</param>
    /// <returns>Cached analytics data</returns>
    [HttpGet("cached/{category}")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCachedAnalytics(string category)
    {
        try
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

            return Ok(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached analytics for category {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<DemographicsAnalytics> GetDemographicsAnalytics()
    {
        var totalCount = await _context.Students.CountAsync(s => !s.IsDeleted);

        // Age distribution
        var ageDistribution = await _context.Students
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.AgeRange)
            .Select(g => new AnalyticsResponse
            {
                MetricName = "Age Distribution",
                Category = "Demographics",
                Value = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalCount * 100, 2),
                CalculatedAt = DateTime.UtcNow
            })
            .ToListAsync();

        // Gender distribution
        var genderDistribution = await _context.Students
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
        var studentStatusDistribution = await _context.Students
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
        var localStatusDistribution = await _context.Students
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

    private async Task<InterestsAnalytics> GetInterestsAnalytics()
    {
        var totalCount = await _context.Students.CountAsync(s => !s.IsDeleted);
        var allInterests = new Dictionary<string, int>();

        // Collect all interests
        var students = await _context.Students
            .Where(s => !s.IsDeleted && !string.IsNullOrEmpty(s.Interests))
            .Select(s => s.Interests)
            .ToListAsync();

        foreach (var interestsJson in students)
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

    private async Task<BehaviorAnalytics> GetBehaviorAnalytics()
    {
        var totalCount = await _context.Students.CountAsync(s => !s.IsDeleted);

        // Budget distribution
        var budgetDistribution = await _context.Students
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
        var activityTimeDistribution = await _context.Students
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
        var socialPreferenceDistribution = await _context.Students
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
