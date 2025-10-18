using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GetStud.Api.HealthChecks;

public class DependenciesHealthCheck : IHealthCheck
{
    private readonly ILogger<DependenciesHealthCheck> _logger;
    private readonly HttpClient _httpClient;

    public DependenciesHealthCheck(ILogger<DependenciesHealthCheck> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var dependencies = new List<string>();
            var failedDependencies = new List<string>();

            // Check if we can make HTTP requests (basic connectivity)
            try
            {
                var response = await _httpClient.GetAsync("https://httpbin.org/status/200", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    dependencies.Add("HTTP Client: OK");
                }
                else
                {
                    failedDependencies.Add($"HTTP Client: Failed with status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                failedDependencies.Add($"HTTP Client: {ex.Message}");
            }

            // Check if we can access environment variables
            try
            {
                var envVars = Environment.GetEnvironmentVariables();
                dependencies.Add($"Environment Variables: {envVars.Count} available");
            }
            catch (Exception ex)
            {
                failedDependencies.Add($"Environment Variables: {ex.Message}");
            }

            // Check if we can access file system (for logging)
            try
            {
                var tempPath = Path.GetTempPath();
                var canWrite = Directory.Exists(tempPath) && Directory.GetAccessControl(tempPath) != null;
                dependencies.Add($"File System: {(canWrite ? "Writable" : "Read-only")}");
            }
            catch (Exception ex)
            {
                failedDependencies.Add($"File System: {ex.Message}");
            }

            var data = new Dictionary<string, object>
            {
                { "healthyDependencies", dependencies },
                { "failedDependencies", failedDependencies },
                { "totalDependencies", dependencies.Count + failedDependencies.Count },
                { "healthyCount", dependencies.Count },
                { "failedCount", failedDependencies.Count },
                { "timestamp", DateTime.UtcNow }
            };

            _logger.LogInformation("Dependencies Health Check: {HealthyCount} healthy, {FailedCount} failed", 
                dependencies.Count, failedDependencies.Count);

            if (failedDependencies.Count == 0)
            {
                return HealthCheckResult.Healthy($"All {dependencies.Count} dependencies are healthy", data);
            }
            else if (failedDependencies.Count < dependencies.Count)
            {
                return HealthCheckResult.Degraded($"{failedDependencies.Count} dependencies failed, {dependencies.Count} healthy", data: data);
            }
            else
            {
                return HealthCheckResult.Unhealthy($"All {failedDependencies.Count} dependencies failed", data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dependencies Health Check failed");
            return HealthCheckResult.Unhealthy($"Dependencies health check failed: {ex.Message}");
        }
    }
}
