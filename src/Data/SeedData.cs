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
        
        // Seed location categories
        if (!context.LocationCategories.Any())
        {
            var categories = new List<LocationCategory>
            {
                new LocationCategory
                {
                    Name = "Кафе и рестораны",
                    IconName = "cafe",
                    Description = "Кафе, рестораны, пиццерии",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Развлечения",
                    IconName = "entertainment",
                    Description = "Кинотеатры, боулинг, караоке",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Учеба",
                    IconName = "education",
                    Description = "Библиотеки, коворкинги, учебные центры",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Спорт",
                    IconName = "sport",
                    Description = "Спортзалы, стадионы, спортивные площадки",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Отдых",
                    IconName = "relax",
                    Description = "Парки, скверы, места для отдыха",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Торговля",
                    IconName = "shopping",
                    Description = "Магазины, торговые центры",
                    DisplayOrder = 6,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Ночная жизнь",
                    IconName = "nightlife",
                    Description = "Клубы, бары, ночные заведения",
                    DisplayOrder = 7,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Культура",
                    IconName = "culture",
                    Description = "Музеи, театры, галереи",
                    DisplayOrder = 8,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Общественный транспорт",
                    IconName = "transport",
                    Description = "Остановки, станции метро",
                    DisplayOrder = 9,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Услуги",
                    IconName = "services",
                    Description = "Банки, почта, салоны",
                    DisplayOrder = 10,
                    IsActive = true
                }
            };
            
            context.LocationCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}