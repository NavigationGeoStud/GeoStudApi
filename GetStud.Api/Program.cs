using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GetStud.Api.HealthChecks;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/getstud-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<BasicHealthCheck>("basic")
    .AddCheck<MemoryHealthCheck>("memory")
    .AddCheck<ApiHealthCheck>("api")
    .AddCheck<DependenciesHealthCheck>("dependencies");

// Add HttpClient for dependencies health check
builder.Services.AddHttpClient<DependenciesHealthCheck>();

// Add Health Checks UI
builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(15);
    opt.MaximumHistoryEntriesPerEndpoint(60);
    opt.SetApiMaxActiveRequests(1);
    opt.AddHealthCheckEndpoint("GetStud API", "/health");
}).AddInMemoryStorage();

// Add metrics
builder.Services.AddMetrics();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(opt =>
{
    opt.UIPath = "/health-ui";
    opt.AddCustomStylesheet("health-ui.css");
});

// Legacy health endpoint
app.Map("/api", () => "health");

app.UseAuthorization();
app.MapControllers();

app.Run();
