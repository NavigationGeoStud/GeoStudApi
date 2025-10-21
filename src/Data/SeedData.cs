using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(GeoStudDbContext context)
    {
        // Seed service clients
        if (!context.ServiceClients.Any())
        {
            var serviceClients = new List<ServiceClient>
            {
                new ServiceClient
                {
                    ClientId = "mobile-app",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("MobileAppSecret123!"),
                    ServiceName = "Mobile Application",
                    Description = "Mobile application service client",
                    IsActive = true,
                    AllowedScopes = "read:surveys,write:surveys,read:analytics"
                },
                new ServiceClient
                {
                    ClientId = "web-app",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("WebAppSecret123!"),
                    ServiceName = "Web Application",
                    Description = "Web application service client",
                    IsActive = true,
                    AllowedScopes = "read:surveys,write:surveys,read:analytics,delete:surveys"
                },
                new ServiceClient
                {
                    ClientId = "analytics-service",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("AnalyticsServiceSecret123!"),
                    ServiceName = "Analytics Service",
                    Description = "Analytics processing service",
                    IsActive = true,
                    AllowedScopes = "read:surveys,read:analytics,write:analytics"
                }
            };

            context.ServiceClients.AddRange(serviceClients);
            await context.SaveChangesAsync();
        }
    }
}