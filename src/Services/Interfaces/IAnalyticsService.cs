using GeoStud.Api.DTOs.Analytics;

namespace GeoStud.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task<ComprehensiveAnalytics> GetComprehensiveAnalyticsAsync();
    Task<DemographicsAnalytics> GetDemographicsAnalyticsAsync();
    Task<InterestsAnalytics> GetInterestsAnalyticsAsync();
    Task<BehaviorAnalytics> GetBehaviorAnalyticsAsync();
    Task<IEnumerable<AnalyticsResponse>> GetCachedAnalyticsAsync(string category);
}

