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
                    ClientId = "tgbot",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("TgBotSecret"),
                    ServiceName = "TelegramBot",
                    Description = "TelegramBot service client",
                    IsActive = true,
                    AllowedScopes = "read:surveys,write:surveys,read:analytics"
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