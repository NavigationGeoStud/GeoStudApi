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
        
        // Seed location subcategories
        if (!context.LocationSubcategories.Any())
        {
            var subcategories = new List<LocationSubcategory>();
            
            // Кафе и рестораны subcategories
            var cafeCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Кафе и рестораны");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Кафе", CategoryId = cafeCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Рестораны", CategoryId = cafeCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Фастфуд", CategoryId = cafeCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Пиццерии", CategoryId = cafeCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Кулинарии", CategoryId = cafeCategory.Id, DisplayOrder = 5, IsActive = true }
            });
            
            // Развлечения subcategories
            var entertainmentCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Развлечения");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Кинотеатры", CategoryId = entertainmentCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Боулинг", CategoryId = entertainmentCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Караоке", CategoryId = entertainmentCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Бильярд", CategoryId = entertainmentCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Квесты", CategoryId = entertainmentCategory.Id, DisplayOrder = 5, IsActive = true },
                new LocationSubcategory { Name = "Виртуальная реальность", CategoryId = entertainmentCategory.Id, DisplayOrder = 6, IsActive = true }
            });
            
            // Культура subcategories
            var cultureCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Культура");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Театры", CategoryId = cultureCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Музеи", CategoryId = cultureCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Галереи", CategoryId = cultureCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Выставочные центры", CategoryId = cultureCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Концертные залы", CategoryId = cultureCategory.Id, DisplayOrder = 5, IsActive = true }
            });
            
            // Учеба subcategories
            var educationCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Учеба");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Библиотеки", CategoryId = educationCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Коворкинги", CategoryId = educationCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Учебные центры", CategoryId = educationCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Лектории", CategoryId = educationCategory.Id, DisplayOrder = 4, IsActive = true }
            });
            
            // Спорт subcategories
            var sportCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Спорт");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Спортзалы", CategoryId = sportCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Бассейны", CategoryId = sportCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Стадионы", CategoryId = sportCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Спортивные площадки", CategoryId = sportCategory.Id, DisplayOrder = 4, IsActive = true },
                new LocationSubcategory { Name = "Фитнес-клубы", CategoryId = sportCategory.Id, DisplayOrder = 5, IsActive = true }
            });
            
            // Отдых subcategories
            var relaxCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Отдых");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Парки", CategoryId = relaxCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Скверы", CategoryId = relaxCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Набережные", CategoryId = relaxCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Смотровые площадки", CategoryId = relaxCategory.Id, DisplayOrder = 4, IsActive = true }
            });
            
            // Торговля subcategories
            var shoppingCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Торговля");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Торговые центры", CategoryId = shoppingCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Супермаркеты", CategoryId = shoppingCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Бутики", CategoryId = shoppingCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Магазины одежды", CategoryId = shoppingCategory.Id, DisplayOrder = 4, IsActive = true }
            });
            
            // Ночная жизнь subcategories
            var nightlifeCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Ночная жизнь");
            subcategories.AddRange(new[]
            {
                new LocationSubcategory { Name = "Клубы", CategoryId = nightlifeCategory.Id, DisplayOrder = 1, IsActive = true },
                new LocationSubcategory { Name = "Бары", CategoryId = nightlifeCategory.Id, DisplayOrder = 2, IsActive = true },
                new LocationSubcategory { Name = "Караоке-бары", CategoryId = nightlifeCategory.Id, DisplayOrder = 3, IsActive = true },
                new LocationSubcategory { Name = "Пабы", CategoryId = nightlifeCategory.Id, DisplayOrder = 4, IsActive = true }
            });
            
            context.LocationSubcategories.AddRange(subcategories);
            await context.SaveChangesAsync();
        }
        
        // Seed test locations
        if (!context.Locations.Any())
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Name = "Кафе Центр",
                    Description = "Уютное кафе в центре города с отличным кофе",
                    Coordinates = "55.7558,37.6176",
                    Address = "ул. Тверская, 1",
                    City = "Москва",
                    Phone = "+7 (495) 123-45-67",
                    Website = "https://cafe-center.ru",
                    ImageUrl = "https://example.com/cafe-center.jpg",
                    Rating = 4.5m,
                    RatingCount = 120,
                    PriceRange = "$$",
                    WorkingHours = "09:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Библиотека им. Ленина",
                    Description = "Главная библиотека города с большим выбором книг",
                    Coordinates = "55.7522,37.6156",
                    Address = "ул. Воздвиженка, 3/5",
                    City = "Москва",
                    Phone = "+7 (495) 202-57-90",
                    Website = "https://leninka.ru",
                    ImageUrl = "https://example.com/library.jpg",
                    Rating = 4.8m,
                    RatingCount = 89,
                    PriceRange = "Free",
                    WorkingHours = "09:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Спорткомплекс Олимпийский",
                    Description = "Современный спортивный комплекс с бассейном и тренажерным залом",
                    Coordinates = "55.7896,37.6218",
                    Address = "Олимпийский проспект, 16",
                    City = "Москва",
                    Phone = "+7 (495) 234-56-78",
                    Website = "https://olympic-sport.ru",
                    ImageUrl = "https://example.com/sport.jpg",
                    Rating = 4.2m,
                    RatingCount = 156,
                    PriceRange = "$$$",
                    WorkingHours = "06:00-24:00",
                    IsActive = true,
                    IsVerified = true
                }
            };
            
            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
            
            // Add category associations
            var cafeCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Кафе и рестораны");
            var educationCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Учеба");
            var sportCategory = await context.LocationCategories.FirstAsync(c => c.Name == "Спорт");
            
            var cafeLocation = await context.Locations.FirstAsync(l => l.Name == "Кафе Центр");
            var libraryLocation = await context.Locations.FirstAsync(l => l.Name == "Библиотека им. Ленина");
            var sportLocation = await context.Locations.FirstAsync(l => l.Name == "Спорткомплекс Олимпийский");
            
            var categoryJoins = new List<LocationCategoryJoin>
            {
                new LocationCategoryJoin { LocationId = cafeLocation.Id, CategoryId = cafeCategory.Id },
                new LocationCategoryJoin { LocationId = libraryLocation.Id, CategoryId = educationCategory.Id },
                new LocationCategoryJoin { LocationId = sportLocation.Id, CategoryId = sportCategory.Id }
            };
            
            context.LocationCategoryJoins.AddRange(categoryJoins);
            await context.SaveChangesAsync();
        }
    }
}