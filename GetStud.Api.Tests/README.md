# Тесты для GeoStud API

Этот проект содержит unit-тесты для GeoStud API, написанные с использованием xUnit, Moq и Entity Framework InMemory.

## Структура тестов

### 1. AuthServiceTests.cs
Тесты для сервиса аутентификации (`AuthService`):
- `AuthenticateServiceAsync_WithValidCredentials_ReturnsAuthResponse` - тест успешной аутентификации
- `AuthenticateServiceAsync_WithInvalidClientId_ReturnsNull` - тест с несуществующим клиентом
- `AuthenticateServiceAsync_WithInvalidPassword_ReturnsNull` - тест с неверным паролем
- `AuthenticateServiceAsync_WithInactiveClient_ReturnsNull` - тест с неактивным клиентом
- `ValidateTokenAsync_WithValidToken_ReturnsTrue` - тест валидации токена
- `ValidateTokenAsync_WithInvalidToken_ReturnsFalse` - тест с невалидным токеном
- `GetServiceClientIdFromTokenAsync_WithValidToken_ReturnsClientId` - тест извлечения ID клиента
- `GetServiceClientIdFromTokenAsync_WithInvalidToken_ReturnsNull` - тест с невалидным токеном
- `AuthenticateServiceAsync_UpdatesLastUsedAt` - тест обновления времени последнего использования

### 2. AuthControllerTests.cs
Тесты для контроллера аутентификации (`AuthController`):
- `AuthenticateService_WithValidRequest_ReturnsOkResult` - тест успешной аутентификации через API
- `AuthenticateService_WithInvalidCredentials_ReturnsUnauthorized` - тест с неверными учетными данными
- `AuthenticateService_WithNullRequest_ReturnsBadRequest` - тест с пустым запросом
- `ValidateToken_WithValidToken_ReturnsOkResult` - тест валидации токена через API
- `ValidateToken_WithInvalidToken_ReturnsUnauthorized` - тест с невалидным токеном
- `ValidateToken_WithEmptyToken_ReturnsBadRequest` - тест с пустым токеном
- `GetServiceClientId_WithValidToken_ReturnsClientId` - тест получения ID клиента
- `GetServiceClientId_WithInvalidToken_ReturnsUnauthorized` - тест с невалидным токеном
- `GetServiceClientId_WithEmptyToken_ReturnsBadRequest` - тест с пустым токеном

### 3. ServiceClientTests.cs
Тесты для модели `ServiceClient`:
- Валидация обязательных полей
- Проверка ограничений длины полей
- Тестирование значений по умолчанию
- Тестирование установки и получения свойств

### 4. DtoTests.cs
Тесты для DTO (Data Transfer Objects):
- `ServiceAuthRequest` - тесты валидации запроса аутентификации
- `AuthResponse` - тесты ответа аутентификации
- `SurveyRequest` - тесты валидации запроса опроса
- `SurveyResponse` - тесты ответа опроса

## Используемые технологии

- **xUnit** - фреймворк для unit-тестирования
- **Moq** - библиотека для создания моков
- **Entity Framework InMemory** - база данных в памяти для тестов
- **BCrypt.Net-Next** - хеширование паролей
- **Microsoft.Extensions.Logging** - логирование

## Запуск тестов

### Через командную строку
```bash
# Перейти в папку с тестами
cd GetStud.Api.Tests

# Восстановить пакеты
dotnet restore

# Запустить все тесты
dotnet test

# Запустить тесты с подробным выводом
dotnet test --verbosity normal

# Запустить тесты с покрытием кода
dotnet test --collect:"XPlat Code Coverage"
```

### Через Visual Studio
1. Откройте Solution Explorer
2. Правый клик на проекте `GetStud.Api.Tests`
3. Выберите "Run Tests"

### Через Test Explorer
1. Откройте Test Explorer (Test → Test Explorer)
2. Нажмите "Run All Tests"

## Особенности тестирования

### InMemory Database
Тесты используют InMemory базу данных для изоляции и быстрого выполнения. Каждый тест получает уникальную базу данных.

### Моки
Используются моки для:
- `IConfiguration` - настройки JWT
- `ILogger<AuthService>` - логирование
- `IAuthService` - сервис аутентификации

### Валидация моделей
Тесты включают проверку валидации моделей с использованием `DataAnnotations`.

### Асинхронные тесты
Все тесты, работающие с асинхронными методами, используют `async/await` паттерн.

## Добавление новых тестов

1. Создайте новый класс тестов
2. Наследуйтесь от `IDisposable` если используете контекст базы данных
3. Используйте атрибут `[Fact]` для обычных тестов
4. Используйте `[Theory]` и `[InlineData]` для параметризованных тестов
5. Следуйте паттерну Arrange-Act-Assert (AAA)

Пример:
```csharp
[Fact]
public async Task MyMethod_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var input = "test-input";
    var expected = "expected-result";

    // Act
    var result = await _service.MyMethod(input);

    // Assert
    Assert.Equal(expected, result);
}
```
