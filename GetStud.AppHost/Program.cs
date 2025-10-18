var builder = DistributedApplication.CreateBuilder(args);

// Add the API project
var api = builder.AddProject<Projects.GetStud_Api>("getstud-api")
    .WithExternalHttpEndpoints()
    .WithHttpsEndpoint(port: 7281, name: "https")
    .WithHttpEndpoint(port: 5032, name: "http");

// Add health checks
api.WithHealthCheck("/health");
api.WithHealthCheck("/alive");

builder.Build().Run();
