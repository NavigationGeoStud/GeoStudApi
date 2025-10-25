using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using GeoStud.Api.Controllers.V1;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.Services;

namespace GetStud.Api.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ServiceLogin_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        var expectedResponse = new AuthResponse
        {
            AccessToken = "test-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockAuthService.Setup(x => x.AuthenticateServiceAsync(request))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ServiceLogin(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.TokenType, response.TokenType);
        Assert.Equal(expectedResponse.ExpiresIn, response.ExpiresIn);
    }

    [Fact]
    public async Task ServiceLogin_InvalidRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "invalid-client",
            ClientSecret = "invalid-secret"
        };

        _mockAuthService.Setup(x => x.AuthenticateServiceAsync(request))
                       .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = await _controller.ServiceLogin(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ServiceLogin_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "", // Invalid - empty string
            ClientSecret = "test-secret"
        };

        _controller.ModelState.AddModelError("ClientId", "ClientId is required");

        // Act
        var result = await _controller.ServiceLogin(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsOkResult()
    {
        // Arrange
        var token = "valid-token";
        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        // Check if the response has a 'valid' property
        var validProperty = response.GetType().GetProperty("valid");
        Assert.NotNull(validProperty);
        Assert.True((bool)validProperty.GetValue(response));
    }

    [Fact]
    public async Task ValidateToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var token = "invalid-token";
        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
                       .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ServiceLogin_Exception_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ServiceAuthRequest
        {
            ClientId = "test-client",
            ClientSecret = "test-secret"
        };

        _mockAuthService.Setup(x => x.AuthenticateServiceAsync(request))
                       .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ServiceLogin(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ValidateToken_Exception_ReturnsInternalServerError()
    {
        // Arrange
        var token = "test-token";
        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
                       .ThrowsAsync(new Exception("Token validation error"));

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}