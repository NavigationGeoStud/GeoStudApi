using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GetStud.Api.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _thresholdBytes;

    public MemoryHealthCheck(long thresholdBytes = 1024 * 1024 * 1024) // 1GB default
    {
        _thresholdBytes = thresholdBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var isHealthy = workingSet < _thresholdBytes;

            var data = new Dictionary<string, object>
            {
                { "workingSetBytes", workingSet },
                { "workingSetMB", Math.Round(workingSet / 1024.0 / 1024.0, 2) },
                { "thresholdBytes", _thresholdBytes },
                { "thresholdMB", Math.Round(_thresholdBytes / 1024.0 / 1024.0, 2) }
            };

            return Task.FromResult(isHealthy 
                ? HealthCheckResult.Healthy($"Memory usage is within limits: {Math.Round(workingSet / 1024.0 / 1024.0, 2)} MB", data)
                : HealthCheckResult.Degraded($"Memory usage is high: {Math.Round(workingSet / 1024.0 / 1024.0, 2)} MB", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Memory health check failed: {ex.Message}"));
        }
    }
}
