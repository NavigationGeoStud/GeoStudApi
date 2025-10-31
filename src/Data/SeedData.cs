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
                    Name = "Кино",
                    IconName = "movie",
                    Description = "Кинотеатры и кино",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Концерты",
                    IconName = "concerts",
                    Description = "Концерты и музыкальные мероприятия",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Театры",
                    IconName = "theatre",
                    Description = "Театральные постановки и спектакли",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Музеи",
                    IconName = "museums",
                    Description = "Музеи и выставки",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Памятники",
                    IconName = "landmark",
                    Description = "Памятники и исторические места",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Загородный отдых",
                    IconName = "suburban",
                    Description = "Места для загородного отдыха",
                    DisplayOrder = 6,
                    IsActive = true
                },
                new LocationCategory
                {
                    Name = "Туристические маршруты",
                    IconName = "tourist",
                    Description = "Туристические маршруты и экскурсии",
                    DisplayOrder = 7,
                    IsActive = true
                }
            };
            
            context.LocationCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }
        
        // Seed location subcategories
        if (!context.LocationSubcategories.Any())
        {
            var subcategories = new List<LocationSubcategory>();
            
            // Театры subcategories
            var theatreCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Театры");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Драма", CategoryId = theatreCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Комедия", CategoryId = theatreCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Мюзикл", CategoryId = theatreCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Детские спектакли", CategoryId = theatreCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Современное искусство", CategoryId = theatreCategory.Id, DisplayOrder = 5, IsActive = true },
                new LocationSubcategory { Name = "Классика", CategoryId = theatreCategory.Id, DisplayOrder = 6, IsActive = true }
            });
            
            // Кино subcategories
            var movieCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Кино");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Драма", CategoryId = movieCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Комедия", CategoryId = movieCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Боевик", CategoryId = movieCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Ужасы", CategoryId = movieCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Фантастика", CategoryId = movieCategory.Id, DisplayOrder = 5, IsActive = true },
                new LocationSubcategory { Name = "Мелодрама", CategoryId = movieCategory.Id, DisplayOrder = 6, IsActive = true },
                new LocationSubcategory { Name = "Детектив", CategoryId = movieCategory.Id, DisplayOrder = 7, IsActive = true },
                new LocationSubcategory { Name = "Арт-хаус", CategoryId = movieCategory.Id, DisplayOrder = 8, IsActive = true }
            });
            
            // Концерты subcategories
            var concertsCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Концерты");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Рок", CategoryId = concertsCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Поп", CategoryId = concertsCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Джаз", CategoryId = concertsCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Электронная музыка", CategoryId = concertsCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Классика", CategoryId = concertsCategory.Id, DisplayOrder = 5, IsActive = true },
                new LocationSubcategory { Name = "Инди", CategoryId = concertsCategory.Id, DisplayOrder = 6, IsActive = true },
                new LocationSubcategory { Name = "Альтернатива", CategoryId = concertsCategory.Id, DisplayOrder = 7, IsActive = true }
            });
            
            context.LocationSubcategories.AddRange(subcategories);
            await context.SaveChangesAsync();
        }
        
        // Seed test locations (optional - можно удалить или оставить для тестов)
        // if (!context.Locations.Any())
        // {
        //     // Примеры локаций можно добавить здесь
        // }
    }
}