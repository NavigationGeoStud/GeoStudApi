using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(GeoStudDbContext context)
    {
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
        
        // Seed location categories - ensure required categories exist
        var existingCategories = await context.LocationCategories.ToListAsync();
        var existingCategoryNames = existingCategories.Select(c => c.Name).ToHashSet();
        
        var categoriesToAdd = new List<LocationCategory>();
        
        if (!existingCategoryNames.Contains("Кино"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Кино",
                IconName = "movie",
                Description = "Кинотеатры и кино",
                DisplayOrder = 1,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Концерты"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Концерты",
                IconName = "concerts",
                Description = "Концерты и музыкальные мероприятия",
                DisplayOrder = 2,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Театры"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Театры",
                IconName = "theatre",
                Description = "Театральные постановки и спектакли",
                DisplayOrder = 3,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Музеи"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Музеи",
                IconName = "museums",
                Description = "Музеи и выставки",
                DisplayOrder = 4,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Памятники"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Памятники",
                IconName = "landmark",
                Description = "Памятники и исторические места",
                DisplayOrder = 5,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Загородный отдых"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Загородный отдых",
                IconName = "suburban",
                Description = "Места для загородного отдыха",
                DisplayOrder = 6,
                IsActive = true
            });
        }
        
        if (!existingCategoryNames.Contains("Туристические маршруты"))
        {
            categoriesToAdd.Add(new LocationCategory
            {
                Name = "Туристические маршруты",
                IconName = "tourist",
                Description = "Туристические маршруты и экскурсии",
                DisplayOrder = 7,
                IsActive = true
            });
        }
        
        if (categoriesToAdd.Any())
        {
            context.LocationCategories.AddRange(categoriesToAdd);
            await context.SaveChangesAsync();
        }
        
        // Seed location subcategories
        if (!context.LocationSubcategories.Any())
        {
            var subcategories = new List<LocationSubcategory>();
            
            // Reload categories from database to ensure we have the latest data including newly added categories
            var categories = await context.LocationCategories.ToListAsync();
            
            // Театры subcategories
            var theatreCategory = categories.FirstOrDefault(c => c.Name == "Театры" && !c.IsDeleted);
            if (theatreCategory == null)
            {
                throw new InvalidOperationException("Location category 'Театры' not found or is deleted. Ensure categories are seeded before subcategories.");
            }
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
            var movieCategory = categories.FirstOrDefault(c => c.Name == "Кино" && !c.IsDeleted);
            if (movieCategory == null)
            {
                throw new InvalidOperationException("Location category 'Кино' not found or is deleted. Ensure categories are seeded before subcategories.");
            }
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
            var concertsCategory = categories.FirstOrDefault(c => c.Name == "Концерты" && !c.IsDeleted);
            if (concertsCategory == null)
            {
                throw new InvalidOperationException("Location category 'Концерты' not found or is deleted. Ensure categories are seeded before subcategories.");
            }
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