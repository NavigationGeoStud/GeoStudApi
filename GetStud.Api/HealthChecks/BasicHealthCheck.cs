using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GetStud.Api.HealthChecks;

public class BasicHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic application health check
            var isHealthy = true;
            var data = new Dictionary<string, object>
            {
                { "timestamp", DateTime.UtcNow },
                { "status", "healthy" },
                { "uptime", Environment.TickCount64 }
            };

            return Task.FromResult(isHealthy 
                ? HealthCheckResult.Healthy("Application is running", data)
                : HealthCheckResult.Unhealthy("Application is not healthy", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}"));
        }
    }
}
