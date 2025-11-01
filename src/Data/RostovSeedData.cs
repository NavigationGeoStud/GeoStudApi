using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public static class RostovSeedData
{
    public static async Task SeedRostovLocationsAsync(GeoStudDbContext context)
    {
        // Seed Rostov locations for each category
        if (!context.Locations.Any(l => l.City == "Ростов-на-Дону"))
        {
            // Get categories first
            var categories = await context.LocationCategories.ToListAsync();
            var cafeCategory = categories.FirstOrDefault(c => c.Name == "Кафе и рестораны");
            var entertainmentCategory = categories.FirstOrDefault(c => c.Name == "Развлечения");
            var educationCategory = categories.FirstOrDefault(c => c.Name == "Учеба");
            var sportCategory = categories.FirstOrDefault(c => c.Name == "Спорт");
            var relaxCategory = categories.FirstOrDefault(c => c.Name == "Отдых");
            var shoppingCategory = categories.FirstOrDefault(c => c.Name == "Торговля");
            var nightlifeCategory = categories.FirstOrDefault(c => c.Name == "Ночная жизнь");
            var cultureCategory = categories.FirstOrDefault(c => c.Name == "Культура");
            var transportCategory = categories.FirstOrDefault(c => c.Name == "Общественный транспорт");
            var servicesCategory = categories.FirstOrDefault(c => c.Name == "Услуги");
            
            // Use first category as fallback if specific category not found
            var defaultCategoryId = categories.FirstOrDefault()?.Id ?? 1;
            
            var rostovLocations = new List<Location>
            {
                // Кафе и рестораны
                new Location
                {
                    Name = "Ресторан 'Петровский'",
                    Description = "Традиционная русская кухня в историческом центре города",
                    Coordinates = "47.2225,39.7188",
                    Address = "ул. Большая Садовая, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 240-12-34",
                    Website = "https://petrovsky-rest.ru",
                    ImageUrl = "https://example.com/petrovsky.jpg",
                    Rating = 4.6m,
                    RatingCount = 234,
                    PriceRange = "$$$",
                    WorkingHours = "11:00-23:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Кафе 'Кофейня на Дону'",
                    Description = "Уютная кофейня с видом на реку Дон и авторским кофе",
                    Coordinates = "47.2256,39.7156",
                    Address = "наб. Дона, 12",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 245-67-89",
                    Website = "https://coffee-don.ru",
                    ImageUrl = "https://example.com/coffee-don.jpg",
                    Rating = 4.4m,
                    RatingCount = 189,
                    PriceRange = "$$",
                    WorkingHours = "08:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Пиццерия 'Мама Рома'",
                    Description = "Аутентичная итальянская пицца, приготовленная в дровяной печи",
                    Coordinates = "47.2201,39.7203",
                    Address = "ул. Пушкинская, 78",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 250-34-56",
                    Website = "https://mama-roma.ru",
                    ImageUrl = "https://example.com/mama-roma.jpg",
                    Rating = 4.7m,
                    RatingCount = 156,
                    PriceRange = "$$",
                    WorkingHours = "12:00-24:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Ресторан 'Донская кухня'",
                    Description = "Региональная кухня Дона с традиционными казачьими блюдами",
                    Coordinates = "47.2189,39.7221",
                    Address = "ул. Соколова, 23",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 255-78-90",
                    Website = "https://donskaya-kuhnya.ru",
                    ImageUrl = "https://example.com/donskaya.jpg",
                    Rating = 4.5m,
                    RatingCount = 278,
                    PriceRange = "$$$",
                    WorkingHours = "10:00-23:30",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Кафе 'Булочная на Садовой'",
                    Description = "Свежая выпечка, десерты и легкие закуски в центре города",
                    Coordinates = "47.2234,39.7167",
                    Address = "ул. Большая Садовая, 67",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 260-12-34",
                    Website = "https://bulochka-sadovaya.ru",
                    ImageUrl = "https://example.com/bulochka.jpg",
                    Rating = 4.3m,
                    RatingCount = 145,
                    PriceRange = "$",
                    WorkingHours = "07:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Развлечения
                new Location
                {
                    Name = "Кинотеатр 'Плаза'",
                    Description = "Современный многозальный кинотеатр с IMAX и 4DX залами",
                    Coordinates = "47.2267,39.7145",
                    Address = "пр. Буденновский, 34",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 270-45-67",
                    Website = "https://plaza-cinema.ru",
                    ImageUrl = "https://example.com/plaza-cinema.jpg",
                    Rating = 4.2m,
                    RatingCount = 312,
                    PriceRange = "$$",
                    WorkingHours = "09:00-02:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Боулинг 'Страйк'",
                    Description = "Современный боулинг-центр с баром и рестораном",
                    Coordinates = "47.2198,39.7256",
                    Address = "ул. Красноармейская, 89",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 275-89-01",
                    Website = "https://strike-bowling.ru",
                    ImageUrl = "https://example.com/strike-bowling.jpg",
                    Rating = 4.4m,
                    RatingCount = 198,
                    PriceRange = "$$",
                    WorkingHours = "10:00-24:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Караоке-бар 'Голос'",
                    Description = "Караоке-бар с современным оборудованием и широким выбором песен",
                    Coordinates = "47.2212,39.7189",
                    Address = "ул. Пушкинская, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 280-23-45",
                    Website = "https://golos-karaoke.ru",
                    ImageUrl = "https://example.com/golos-karaoke.jpg",
                    Rating = 4.1m,
                    RatingCount = 167,
                    PriceRange = "$$",
                    WorkingHours = "18:00-06:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Развлекательный центр 'Мега'",
                    Description = "Семейный развлекательный центр с аттракционами и игровыми автоматами",
                    Coordinates = "47.2245,39.7123",
                    Address = "ул. Малиновского, 12",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 285-67-89",
                    Website = "https://mega-entertainment.ru",
                    ImageUrl = "https://example.com/mega-entertainment.jpg",
                    Rating = 4.0m,
                    RatingCount = 234,
                    PriceRange = "$$",
                    WorkingHours = "10:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Бильярдный клуб 'Кубок'",
                    Description = "Профессиональные бильярдные столы и уютная атмосфера",
                    Coordinates = "47.2178,39.7198",
                    Address = "ул. Соколова, 56",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 290-12-34",
                    Website = "https://kubok-billiard.ru",
                    ImageUrl = "https://example.com/kubok-billiard.jpg",
                    Rating = 4.3m,
                    RatingCount = 123,
                    PriceRange = "$$",
                    WorkingHours = "12:00-02:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Учеба
                new Location
                {
                    Name = "Центральная библиотека им. Горького",
                    Description = "Главная библиотека города с богатым фондом и читальными залами",
                    Coordinates = "47.2205,39.7167",
                    Address = "ул. Большая Садовая, 33",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 295-45-67",
                    Website = "https://gorky-library.ru",
                    ImageUrl = "https://example.com/gorky-library.jpg",
                    Rating = 4.6m,
                    RatingCount = 189,
                    PriceRange = "Free",
                    WorkingHours = "09:00-20:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Коворкинг 'Рабочее место'",
                    Description = "Современный коворкинг с высокоскоростным интернетом и переговорными",
                    Coordinates = "47.2234,39.7145",
                    Address = "ул. Пушкинская, 89",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 300-78-90",
                    Website = "https://workspace-coworking.ru",
                    ImageUrl = "https://example.com/workspace-coworking.jpg",
                    Rating = 4.4m,
                    RatingCount = 145,
                    PriceRange = "$$",
                    WorkingHours = "08:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Учебный центр 'Знание'",
                    Description = "Центр дополнительного образования с курсами и семинарами",
                    Coordinates = "47.2189,39.7212",
                    Address = "ул. Красноармейская, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 305-23-45",
                    Website = "https://znanie-center.ru",
                    ImageUrl = "https://example.com/znanie-center.jpg",
                    Rating = 4.5m,
                    RatingCount = 178,
                    PriceRange = "$$",
                    WorkingHours = "09:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Библиотека ЮФУ",
                    Description = "Научная библиотека Южного федерального университета",
                    Coordinates = "47.2256,39.7189",
                    Address = "ул. Большая Садовая, 105",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 310-56-78",
                    Website = "https://sfedu-library.ru",
                    ImageUrl = "https://example.com/sfedu-library.jpg",
                    Rating = 4.7m,
                    RatingCount = 267,
                    PriceRange = "Free",
                    WorkingHours = "08:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Центр изучения языков 'Полиглот'",
                    Description = "Языковая школа с курсами английского, немецкого и других языков",
                    Coordinates = "47.2212,39.7203",
                    Address = "ул. Соколова, 78",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 315-89-01",
                    Website = "https://polyglot-school.ru",
                    ImageUrl = "https://example.com/polyglot-school.jpg",
                    Rating = 4.3m,
                    RatingCount = 156,
                    PriceRange = "$$",
                    WorkingHours = "09:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Спорт
                new Location
                {
                    Name = "Спорткомплекс 'Динамо'",
                    Description = "Многофункциональный спортивный комплекс с бассейном и тренажерным залом",
                    Coordinates = "47.2267,39.7123",
                    Address = "ул. Малиновского, 23",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 320-12-34",
                    Website = "https://dynamo-sport.ru",
                    ImageUrl = "https://example.com/dynamo-sport.jpg",
                    Rating = 4.4m,
                    RatingCount = 234,
                    PriceRange = "$$",
                    WorkingHours = "06:00-23:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Фитнес-клуб 'World Class'",
                    Description = "Премиальный фитнес-клуб с современным оборудованием и групповыми занятиями",
                    Coordinates = "47.2198,39.7145",
                    Address = "пр. Буденновский, 67",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 325-45-67",
                    Website = "https://worldclass-rostov.ru",
                    ImageUrl = "https://example.com/worldclass-rostov.jpg",
                    Rating = 4.6m,
                    RatingCount = 189,
                    PriceRange = "$$$",
                    WorkingHours = "05:30-23:30",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Стадион 'Ростов Арена'",
                    Description = "Современный футбольный стадион с возможностью проведения различных мероприятий",
                    Coordinates = "47.2245,39.7203",
                    Address = "ул. Левобережная, 2",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 330-78-90",
                    Website = "https://rostov-arena.ru",
                    ImageUrl = "https://example.com/rostov-arena.jpg",
                    Rating = 4.8m,
                    RatingCount = 456,
                    PriceRange = "$$",
                    WorkingHours = "08:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Теннисный клуб 'Аккорд'",
                    Description = "Теннисный клуб с крытыми и открытыми кортами",
                    Coordinates = "47.2178,39.7167",
                    Address = "ул. Красноармейская, 123",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 335-23-45",
                    Website = "https://akkord-tennis.ru",
                    ImageUrl = "https://example.com/akkord-tennis.jpg",
                    Rating = 4.5m,
                    RatingCount = 167,
                    PriceRange = "$$",
                    WorkingHours = "07:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Спортивная площадка 'Здоровье'",
                    Description = "Открытая спортивная площадка с тренажерами и беговыми дорожками",
                    Coordinates = "47.2212,39.7123",
                    Address = "парк им. Горького",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 340-56-78",
                    Website = "https://zdorovie-sport.ru",
                    ImageUrl = "https://example.com/zdorovie-sport.jpg",
                    Rating = 4.2m,
                    RatingCount = 123,
                    PriceRange = "Free",
                    WorkingHours = "06:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Отдых
                new Location
                {
                    Name = "Парк им. Горького",
                    Description = "Центральный парк города с аллеями, фонтанами и детскими площадками",
                    Coordinates = "47.2205,39.7123",
                    Address = "ул. Большая Садовая, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 345-89-01",
                    Website = "https://gorky-park-rostov.ru",
                    ImageUrl = "https://example.com/gorky-park-rostov.jpg",
                    Rating = 4.5m,
                    RatingCount = 345,
                    PriceRange = "Free",
                    WorkingHours = "06:00-23:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Набережная Дона",
                    Description = "Живописная набережная с видом на реку Дон и пешеходными зонами",
                    Coordinates = "47.2256,39.7145",
                    Address = "наб. Дона",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 350-12-34",
                    Website = "https://naberezhnaya-dona.ru",
                    ImageUrl = "https://example.com/naberezhnaya-dona.jpg",
                    Rating = 4.7m,
                    RatingCount = 278,
                    PriceRange = "Free",
                    WorkingHours = "24/7",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Сквер 'Покровский'",
                    Description = "Уютный сквер в историческом центре с памятниками и скамейками",
                    Coordinates = "47.2189,39.7189",
                    Address = "ул. Покровская, 15",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 355-45-67",
                    Website = "https://pokrovsky-skver.ru",
                    ImageUrl = "https://example.com/pokrovsky-skver.jpg",
                    Rating = 4.3m,
                    RatingCount = 156,
                    PriceRange = "Free",
                    WorkingHours = "06:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Ботанический сад ЮФУ",
                    Description = "Ботанический сад с коллекцией растений и экскурсионными программами",
                    Coordinates = "47.2234,39.7203",
                    Address = "ул. Ботаническая, 7",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 360-78-90",
                    Website = "https://botanical-garden-sfedu.ru",
                    ImageUrl = "https://example.com/botanical-garden-sfedu.jpg",
                    Rating = 4.6m,
                    RatingCount = 189,
                    PriceRange = "$",
                    WorkingHours = "09:00-18:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Пляж 'Левый берег'",
                    Description = "Городской пляж на левом берегу Дона с зонами отдыха",
                    Coordinates = "47.2267,39.7167",
                    Address = "Левобережная зона",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 365-23-45",
                    Website = "https://levy-bereg-beach.ru",
                    ImageUrl = "https://example.com/levy-bereg-beach.jpg",
                    Rating = 4.1m,
                    RatingCount = 234,
                    PriceRange = "Free",
                    WorkingHours = "06:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Торговля
                new Location
                {
                    Name = "ТРЦ 'Мега'",
                    Description = "Крупный торгово-развлекательный центр с множеством магазинов",
                    Coordinates = "47.2245,39.7123",
                    Address = "ул. Малиновского, 12",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 370-56-78",
                    Website = "https://mega-rostov.ru",
                    ImageUrl = "https://example.com/mega-rostov.jpg",
                    Rating = 4.4m,
                    RatingCount = 456,
                    PriceRange = "$$",
                    WorkingHours = "10:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "ТЦ 'Горизонт'",
                    Description = "Торговый центр с магазинами одежды, обуви и аксессуаров",
                    Coordinates = "47.2198,39.7145",
                    Address = "пр. Буденновский, 89",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 375-89-01",
                    Website = "https://gorizont-tc.ru",
                    ImageUrl = "https://example.com/gorizont-tc.jpg",
                    Rating = 4.2m,
                    RatingCount = 234,
                    PriceRange = "$$",
                    WorkingHours = "10:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Рынок 'Центральный'",
                    Description = "Центральный рынок с продуктами питания и товарами местного производства",
                    Coordinates = "47.2212,39.7203",
                    Address = "ул. Соколова, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 380-12-34",
                    Website = "https://central-market-rostov.ru",
                    ImageUrl = "https://example.com/central-market-rostov.jpg",
                    Rating = 4.0m,
                    RatingCount = 189,
                    PriceRange = "$",
                    WorkingHours = "07:00-19:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Магазин 'Донские сувениры'",
                    Description = "Магазин сувениров и подарков с символикой Ростова и Дона",
                    Coordinates = "47.2205,39.7167",
                    Address = "ул. Большая Садовая, 78",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 385-45-67",
                    Website = "https://donskie-suveniry.ru",
                    ImageUrl = "https://example.com/donskie-suveniry.jpg",
                    Rating = 4.3m,
                    RatingCount = 123,
                    PriceRange = "$$",
                    WorkingHours = "09:00-20:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "ТЦ 'Плаза'",
                    Description = "Современный торговый центр с бутиками и ресторанами",
                    Coordinates = "47.2178,39.7189",
                    Address = "ул. Красноармейская, 67",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 390-78-90",
                    Website = "https://plaza-tc-rostov.ru",
                    ImageUrl = "https://example.com/plaza-tc-rostov.jpg",
                    Rating = 4.1m,
                    RatingCount = 167,
                    PriceRange = "$$",
                    WorkingHours = "10:00-22:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Ночная жизнь
                new Location
                {
                    Name = "Клуб 'Платина'",
                    Description = "Премиальный ночной клуб с танцполом и VIP-зонами",
                    Coordinates = "47.2256,39.7145",
                    Address = "ул. Пушкинская, 123",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 395-23-45",
                    Website = "https://platina-club.ru",
                    ImageUrl = "https://example.com/platina-club.jpg",
                    Rating = 4.5m,
                    RatingCount = 189,
                    PriceRange = "$$$",
                    WorkingHours = "22:00-06:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Бар 'Донской'",
                    Description = "Стильный бар с коктейлями и живой музыкой",
                    Coordinates = "47.2234,39.7203",
                    Address = "наб. Дона, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 400-56-78",
                    Website = "https://donskoy-bar.ru",
                    ImageUrl = "https://example.com/donskoy-bar.jpg",
                    Rating = 4.3m,
                    RatingCount = 156,
                    PriceRange = "$$",
                    WorkingHours = "18:00-02:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Клуб 'Арена'",
                    Description = "Ночной клуб с современной музыкой и световыми эффектами",
                    Coordinates = "47.2198,39.7167",
                    Address = "ул. Соколова, 89",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 405-89-01",
                    Website = "https://arena-club-rostov.ru",
                    ImageUrl = "https://example.com/arena-club-rostov.jpg",
                    Rating = 4.2m,
                    RatingCount = 234,
                    PriceRange = "$$",
                    WorkingHours = "22:00-06:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Бар 'Пивной дом'",
                    Description = "Пивной бар с широким выбором крафтового пива и закусок",
                    Coordinates = "47.2212,39.7123",
                    Address = "ул. Большая Садовая, 56",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 410-12-34",
                    Website = "https://pivnoy-dom-rostov.ru",
                    ImageUrl = "https://example.com/pivnoy-dom-rostov.jpg",
                    Rating = 4.4m,
                    RatingCount = 178,
                    PriceRange = "$$",
                    WorkingHours = "16:00-02:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Клуб 'Энергия'",
                    Description = "Молодежный ночной клуб с танцами и развлекательными программами",
                    Coordinates = "47.2245,39.7189",
                    Address = "пр. Буденновский, 123",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 415-45-67",
                    Website = "https://energiya-club.ru",
                    ImageUrl = "https://example.com/energiya-club.jpg",
                    Rating = 4.1m,
                    RatingCount = 145,
                    PriceRange = "$$",
                    WorkingHours = "21:00-05:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Культура
                new Location
                {
                    Name = "Ростовский областной музей краеведения",
                    Description = "Краеведческий музей с экспозициями по истории Дона и казачества",
                    Coordinates = "47.2205,39.7167",
                    Address = "ул. Большая Садовая, 79",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 420-78-90",
                    Website = "https://rostov-museum.ru",
                    ImageUrl = "https://example.com/rostov-museum.jpg",
                    Rating = 4.6m,
                    RatingCount = 234,
                    PriceRange = "$",
                    WorkingHours = "10:00-18:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Ростовский академический театр драмы",
                    Description = "Старейший драматический театр города с классическим и современным репертуаром",
                    Coordinates = "47.2234,39.7145",
                    Address = "пл. Театральная, 1",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 425-23-45",
                    Website = "https://rostov-drama.ru",
                    ImageUrl = "https://example.com/rostov-drama.jpg",
                    Rating = 4.7m,
                    RatingCount = 345,
                    PriceRange = "$$",
                    WorkingHours = "10:00-19:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Галерея современного искусства '16-я линия'",
                    Description = "Галерея современного искусства с выставками местных и зарубежных художников",
                    Coordinates = "47.2189,39.7203",
                    Address = "ул. 16-я линия, 8",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 430-56-78",
                    Website = "https://16-line-gallery.ru",
                    ImageUrl = "https://example.com/16-line-gallery.jpg",
                    Rating = 4.4m,
                    RatingCount = 123,
                    PriceRange = "$",
                    WorkingHours = "11:00-19:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Музей изобразительных искусств",
                    Description = "Музей с коллекцией живописи, скульптуры и декоративно-прикладного искусства",
                    Coordinates = "47.2212,39.7189",
                    Address = "ул. Пушкинская, 115",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 435-89-01",
                    Website = "https://rostov-art-museum.ru",
                    ImageUrl = "https://example.com/rostov-art-museum.jpg",
                    Rating = 4.5m,
                    RatingCount = 189,
                    PriceRange = "$",
                    WorkingHours = "10:00-18:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Концертный зал 'Дон'",
                    Description = "Концертный зал для проведения музыкальных мероприятий и фестивалей",
                    Coordinates = "47.2256,39.7123",
                    Address = "ул. Малиновского, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 440-12-34",
                    Website = "https://don-concert-hall.ru",
                    ImageUrl = "https://example.com/don-concert-hall.jpg",
                    Rating = 4.3m,
                    RatingCount = 167,
                    PriceRange = "$$",
                    WorkingHours = "10:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },

                // Общественный транспорт
                new Location
                {
                    Name = "Железнодорожный вокзал 'Ростов-Главный'",
                    Description = "Главный железнодорожный вокзал города с кассами и залами ожидания",
                    Coordinates = "47.2189,39.7123",
                    Address = "пл. Привокзальная, 1",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 445-45-67",
                    Website = "https://rostov-vokzal.ru",
                    ImageUrl = "https://example.com/rostov-vokzal.jpg",
                    Rating = 4.2m,
                    RatingCount = 456,
                    PriceRange = "Free",
                    WorkingHours = "24/7",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Автовокзал 'Центральный'",
                    Description = "Центральный автовокзал с рейсами по области и соседним регионам",
                    Coordinates = "47.2245,39.7203",
                    Address = "ул. Сиверса, 1",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 450-78-90",
                    Website = "https://rostov-avtovokzal.ru",
                    ImageUrl = "https://example.com/rostov-avtovokzal.jpg",
                    Rating = 4.0m,
                    RatingCount = 234,
                    PriceRange = "Free",
                    WorkingHours = "05:00-23:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Остановка 'Центр'",
                    Description = "Центральная остановка общественного транспорта с автобусами и троллейбусами",
                    Coordinates = "47.2205,39.7167",
                    Address = "ул. Большая Садовая, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 455-23-45",
                    Website = "https://transport-rostov.ru",
                    ImageUrl = "https://example.com/transport-rostov.jpg",
                    Rating = 3.8m,
                    RatingCount = 189,
                    PriceRange = "$",
                    WorkingHours = "05:30-23:30",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Станция метро 'Центральная'",
                    Description = "Центральная станция метро с переходами и торговыми павильонами",
                    Coordinates = "47.2234,39.7145",
                    Address = "ул. Пушкинская, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 460-56-78",
                    Website = "https://metro-rostov.ru",
                    ImageUrl = "https://example.com/metro-rostov.jpg",
                    Rating = 4.1m,
                    RatingCount = 345,
                    PriceRange = "$",
                    WorkingHours = "05:30-00:30",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Такси-стоянка 'Дон'",
                    Description = "Организованная стоянка такси в центре города",
                    Coordinates = "47.2212,39.7203",
                    Address = "пл. Театральная, 5",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 465-89-01",
                    Website = "https://taxi-don-rostov.ru",
                    ImageUrl = "https://example.com/taxi-don-rostov.jpg",
                    Rating = 4.0m,
                    RatingCount = 123,
                    PriceRange = "$$",
                    WorkingHours = "24/7",
                    IsActive = true,
                    IsVerified = true
                },

                // Услуги
                new Location
                {
                    Name = "Сбербанк 'Центральный'",
                    Description = "Центральное отделение Сбербанка с полным спектром банковских услуг",
                    Coordinates = "47.2205,39.7167",
                    Address = "ул. Большая Садовая, 67",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 470-12-34",
                    Website = "https://sberbank.ru",
                    ImageUrl = "https://example.com/sberbank-central.jpg",
                    Rating = 4.2m,
                    RatingCount = 234,
                    PriceRange = "Free",
                    WorkingHours = "09:00-18:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Почта России 'Главпочтамт'",
                    Description = "Главное почтовое отделение с услугами почты и телеграфа",
                    Coordinates = "47.2234,39.7145",
                    Address = "ул. Пушкинская, 89",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 475-45-67",
                    Website = "https://pochta.ru",
                    ImageUrl = "https://example.com/pochta-central.jpg",
                    Rating = 4.0m,
                    RatingCount = 189,
                    PriceRange = "$",
                    WorkingHours = "08:00-20:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Салон красоты 'Элегант'",
                    Description = "Салон красоты с услугами парикмахера, мастера маникюра и косметолога",
                    Coordinates = "47.2189,39.7203",
                    Address = "ул. Соколова, 45",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 480-78-90",
                    Website = "https://elegant-salon.ru",
                    ImageUrl = "https://example.com/elegant-salon.jpg",
                    Rating = 4.4m,
                    RatingCount = 156,
                    PriceRange = "$$",
                    WorkingHours = "09:00-21:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Медицинский центр 'Здоровье'",
                    Description = "Многопрофильный медицинский центр с различными специалистами",
                    Coordinates = "47.2212,39.7123",
                    Address = "ул. Красноармейская, 78",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 485-23-45",
                    Website = "https://zdorovie-medical.ru",
                    ImageUrl = "https://example.com/zdorovie-medical.jpg",
                    Rating = 4.5m,
                    RatingCount = 267,
                    PriceRange = "$$",
                    WorkingHours = "08:00-20:00",
                    IsActive = true,
                    IsVerified = true
                },
                new Location
                {
                    Name = "Аптека 'Донская'",
                    Description = "Круглосуточная аптека с широким ассортиментом лекарств и медицинских товаров",
                    Coordinates = "47.2256,39.7189",
                    Address = "ул. Большая Садовая, 123",
                    City = "Ростов-на-Дону",
                    Phone = "+7 (863) 490-56-78",
                    Website = "https://donskaya-apteka.ru",
                    ImageUrl = "https://example.com/donskaya-apteka.jpg",
                    Rating = 4.3m,
                    RatingCount = 178,
                    PriceRange = "$$",
                    WorkingHours = "24/7",
                    IsActive = true,
                    IsVerified = true
                }
            };

            // Set CategoryId for each location based on their group
            foreach (var location in rostovLocations)
            {
                // Determine category based on location name patterns
                if (location.Name.Contains("Ресторан") || location.Name.Contains("Кафе") || 
                    location.Name.Contains("Пиццерия") || location.Name.Contains("Булочная"))
                {
                    location.CategoryId = cafeCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Кинотеатр") || location.Name.Contains("Боулинг") || 
                         location.Name.Contains("Караоке") || location.Name.Contains("Развлекательный") || 
                         location.Name.Contains("Бильярдный"))
                {
                    location.CategoryId = entertainmentCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Библиотека") || location.Name.Contains("Коворкинг") || 
                         location.Name.Contains("Учебный") || location.Name.Contains("Центр изучения"))
                {
                    location.CategoryId = educationCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Спорткомплекс") || location.Name.Contains("Фитнес") || 
                         location.Name.Contains("Стадион") || location.Name.Contains("Теннисный") || 
                         location.Name.Contains("Спортивная"))
                {
                    location.CategoryId = sportCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Парк") || location.Name.Contains("Набережная") || 
                         location.Name.Contains("Сквер") || location.Name.Contains("Ботанический") || 
                         location.Name.Contains("Пляж"))
                {
                    location.CategoryId = relaxCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("ТРЦ") || location.Name.Contains("ТЦ") || 
                         location.Name.Contains("Рынок") || location.Name.Contains("Магазин"))
                {
                    location.CategoryId = shoppingCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Клуб") || location.Name.Contains("Бар"))
                {
                    location.CategoryId = nightlifeCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Музей") || location.Name.Contains("Театр") || 
                         location.Name.Contains("Галерея") || location.Name.Contains("Концертный"))
                {
                    location.CategoryId = cultureCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Вокзал") || location.Name.Contains("Остановка") || 
                         location.Name.Contains("Станция") || location.Name.Contains("Такси"))
                {
                    location.CategoryId = transportCategory?.Id ?? defaultCategoryId;
                }
                else if (location.Name.Contains("Сбербанк") || location.Name.Contains("Почта") || 
                         location.Name.Contains("Салон") || location.Name.Contains("Медицинский") || 
                         location.Name.Contains("Аптека"))
                {
                    location.CategoryId = servicesCategory?.Id ?? defaultCategoryId;
                }
                else
                {
                    // Default to first category if no pattern matches
                    location.CategoryId = defaultCategoryId;
                }
            }

            context.Locations.AddRange(rostovLocations);
            await context.SaveChangesAsync();
        }
    }

    // This method is no longer needed as CategoryId is set during location creation
    // Keeping it empty for backward compatibility
    private static async Task AddRostovCategoryAssociations(GeoStudDbContext context)
    {
        // CategoryId is now set during location creation in SeedRostovLocationsAsync
        await Task.CompletedTask;
    }
}
