namespace GeoStud.Api.DTOs.Analytics;

public class AnalyticsResponse
{
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class DemographicsAnalytics
{
    public List<AnalyticsResponse> AgeDistribution { get; set; } = new();
    public List<AnalyticsResponse> GenderDistribution { get; set; } = new();
    public List<AnalyticsResponse> StudentStatusDistribution { get; set; } = new();
    public List<AnalyticsResponse> LocalStatusDistribution { get; set; } = new();
}

public class InterestsAnalytics
{
    public List<AnalyticsResponse> TopInterests { get; set; } = new();
    public List<AnalyticsResponse> InterestCombinations { get; set; } = new();
}

public class BehaviorAnalytics
{
    public List<AnalyticsResponse> BudgetDistribution { get; set; } = new();
    public List<AnalyticsResponse> ActivityTimeDistribution { get; set; } = new();
    public List<AnalyticsResponse> SocialPreferenceDistribution { get; set; } = new();
}

public class ComprehensiveAnalytics
{
    public DemographicsAnalytics Demographics { get; set; } = new();
    public InterestsAnalytics Interests { get; set; } = new();
    public BehaviorAnalytics Behavior { get; set; } = new();
    public int TotalResponses { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
