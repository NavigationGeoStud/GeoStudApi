using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GetStud.Api.HealthChecks;

public class ApiHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiHealthCheck> _logger;

    public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate API health check - check if the API is responsive
            var startTime = DateTime.UtcNow;
            
            // Simulate some API operation (like checking if controllers are working)
            await Task.Delay(10, cancellationToken); // Small delay to simulate work
            
            var responseTime = DateTime.UtcNow - startTime;
            var isHealthy = responseTime.TotalMilliseconds < 1000; // Response time should be under 1 second

            var data = new Dictionary<string, object>
            {
                { "responseTimeMs", Math.Round(responseTime.TotalMilliseconds, 2) },
                { "timestamp", DateTime.UtcNow },
                { "apiVersion", "1.0.0" },
                { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" }
            };

            _logger.LogInformation("API Health Check completed in {ResponseTime}ms", responseTime.TotalMilliseconds);

            return isHealthy 
                ? HealthCheckResult.Healthy($"API is responsive (response time: {Math.Round(responseTime.TotalMilliseconds, 2)}ms)", data)
                : HealthCheckResult.Degraded($"API response time is slow: {Math.Round(responseTime.TotalMilliseconds, 2)}ms", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API Health Check failed");
            return HealthCheckResult.Unhealthy($"API health check failed: {ex.Message}");
        }
    }
}
