using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.Models;
using GeoStud.Api.Services;
using BCrypt.Net;

namespace GetStud.Api.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly GeoStudDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Настройка in-memory базы данных
        var options = new DbContextOptionsBuilder<GeoStudDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GeoStudDbContext(options);

        // Настройка моков
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("GeoStudApi");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("GeoStudApiUsers");
        _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("60");
        
        // Setup for GetValue<int> method
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x.Value).Returns("60");
        _mockConfiguration.Setup(x => x.GetSection("Jwt:ExpirationMinutes")).Returns(configSectionMock.Object);

        _mockLogger = new Mock<ILogger<AuthService>>();

        // Создание сервиса
        _authService = new AuthService(_context, _mockConfiguration.Object, _mockLogger.Object);

        // Добавление тестовых данных
        SeedTestData();
    }

    private void SeedTestData()
    {
        var testClient = new ServiceClient
        {
            Id = 1,
            ClientId = "test-client",
            ClientSecret = BCrypt.Net.BCrypt.HashPassword("test-secret"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow.AddDays(-1)
        };

        var inactiveClient = new ServiceClient
        {
            Id = 2,
            ClientId = "inactive-client",
            ClientSecret = BCrypt.Net.BCrypt.HashPassword("inactive-secret"),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.ServiceClients.AddRange(testClient, inactiveClient);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AuthenticateServiceAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        // Act
        var result = await _authService.AuthenticateServiceAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.True(result.ExpiresIn > 0);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticateServiceAsync_InvalidClientId_ReturnsNull()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "non-existent-client",
            ClientSecret = "test-secret"
        };

        // Act
        var result = await _authService.AuthenticateServiceAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateServiceAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "wrong-password"
        };

        // Act
        var result = await _authService.AuthenticateServiceAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateServiceAsync_InactiveClient_ReturnsNull()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "inactive-client",
            ClientSecret = "inactive-secret"
        };

        // Act
        var result = await _authService.AuthenticateServiceAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var authResponse = await _authService.AuthenticateServiceAsync(request);
        Assert.NotNull(authResponse);

        // Act
        var isValid = await _authService.ValidateTokenAsync(authResponse.AccessToken);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetServiceClientIdFromTokenAsync_ValidToken_ReturnsClientId()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var authResponse = await _authService.AuthenticateServiceAsync(request);
        Assert.NotNull(authResponse);

        // Act
        var clientId = await _authService.GetServiceClientIdFromTokenAsync(authResponse.AccessToken);

        // Assert
        Assert.Equal("test-client", clientId);
    }

    [Fact]
    public async Task GetServiceClientIdFromTokenAsync_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var clientId = await _authService.GetServiceClientIdFromTokenAsync(invalidToken);

        // Assert
        Assert.Null(clientId);
    }

    [Fact]
    public async Task AuthenticateServiceAsync_UpdatesLastUsedAt()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var client = _context.ServiceClients.First(c => c.ClientId == "test-client");
        var originalLastUsed = client.LastUsedAt;

        // Act
        await _authService.AuthenticateServiceAsync(request);

        // Assert
        _context.Entry(client).Reload();
        Assert.True(client.LastUsedAt > originalLastUsed);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}