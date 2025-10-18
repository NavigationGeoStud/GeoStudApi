var builder = DistributedApplication.CreateBuilder(args);

// Add the API project
var api = builder.AddProject<Projects.GetStud_Api>("getstud-api")
    .WithExternalHttpEndpoints()
    .WithHttpsEndpoint(port: 7281, name: "https")
    .WithHttpEndpoint(port: 5032, name: "http");

// Add health check
api.WithHealthCheck("/health");

// Add monitoring and observability
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation();
    });

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Build().Run();
