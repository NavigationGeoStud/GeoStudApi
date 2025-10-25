# GeoStud API

## Описание

GeoStud API - это веб-API для работы со студенческими данными и опросами. Проект включает в себя систему аутентификации, управление студентами, проведение опросов и аналитику.

## Структура проекта

```
GeoStudApi/
├── src/                          # Основной код приложения
│   ├── Controllers/              # API контроллеры
│   │   └── V1/                  # Версия 1 API
│   ├── Data/                    # Контекст базы данных и данные для инициализации
│   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Analytics/           # DTO для аналитики
│   │   ├── Auth/                # DTO для аутентификации
│   │   └── Survey/              # DTO для опросов
│   ├── Middleware/              # Промежуточное ПО
│   ├── Models/                  # Модели данных
│   └── Services/                # Бизнес-логика
├── GetStud.Api.Tests/           # Тесты
└── README.md                    # Этот файл
```

## Технологии

- **.NET 9.0** - Основная платформа
- **ASP.NET Core** - Веб-фреймворк
- **Entity Framework Core** - ORM для работы с базой данных
- **SQLite** - База данных (для разработки)
- **JWT** - Аутентификация
- **BCrypt** - Хеширование паролей
- **xUnit** - Фреймворк для тестирования
- **Moq** - Библиотека для мокирования

## Запуск приложения

### Предварительные требования

- .NET 9.0 SDK
- Visual Studio 2022 или VS Code

### Установка зависимостей

```bash
dotnet restore
```

### Запуск приложения

```bash
dotnet run --project src
```

Приложение будет доступно по адресу: `https://localhost:7000` или `http://localhost:5000`

## Тестирование

### Запуск всех тестов

```bash
dotnet test
```

### Запуск тестов с подробным выводом

```bash
dotnet test --verbosity normal
```

### Запуск тестов конкретного проекта

```bash
dotnet test GetStud.Api.Tests/GetStud.Api.Tests.csproj
```

### Запуск конкретного теста

```bash
dotnet test --filter "TestName"
```

## Структура тестов

Проект содержит следующие тестовые файлы:

- **UnitTest1.cs** - Базовые тесты
- **AuthServiceTests.cs** - Тесты для сервиса аутентификации
- **AuthControllerTests.cs** - Тесты для контроллера аутентификации
- **ModelTests.cs** - Тесты для моделей и DTO

### Типы тестов

1. **Unit Tests** - Тестирование отдельных компонентов
2. **Integration Tests** - Тестирование взаимодействия компонентов
3. **Model Tests** - Тестирование моделей данных

## API Endpoints

### Аутентификация

- `POST /api/v1/auth/service-login` - Вход для сервисов
- `POST /api/v1/auth/validate` - Валидация токена

### Опросы

- `POST /api/v1/survey` - Создание опроса
- `GET /api/v1/survey` - Получение опросов

### Аналитика

- `GET /api/v1/analytics` - Получение аналитических данных

### Здоровье

- `GET /api/v1/health` - Проверка состояния API

## Конфигурация

Настройки приложения находятся в файлах:
- `src/appsettings.json` - Основные настройки
- `src/appsettings.Development.json` - Настройки для разработки

## Разработка

### Добавление новых тестов

1. Создайте новый класс в папке `GetStud.Api.Tests/`
2. Добавьте методы с атрибутом `[Fact]`
3. Используйте `Assert` для проверки результатов

Пример:

```csharp
[Fact]
public void MyTest()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = SomeMethod(input);
    
    // Assert
    Assert.Equal("expected", result);
}
```

### Мокирование зависимостей

Используйте библиотеку Moq для мокирования зависимостей:

```csharp
var mockService = new Mock<IService>();
mockService.Setup(x => x.Method()).Returns("value");
```

## Лицензия

Этот проект создан в образовательных целях.