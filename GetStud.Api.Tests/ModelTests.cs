using GeoStud.Api.Models;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.DTOs.User;
using GeoStud.Api.DTOs.Analytics;

namespace GetStud.Api.Tests;

public class ModelTests
{
    [Fact]
    public void ServiceClient_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var client = new ServiceClient();

        // Assert
        Assert.Equal(0, client.Id);
        Assert.Equal(string.Empty, client.ClientId);
        Assert.Equal(string.Empty, client.ClientSecret);
        Assert.True(client.IsActive);
        Assert.True(client.CreatedAt > DateTime.MinValue);
        Assert.Null(client.LastUsedAt);
    }

    [Fact]
    public void ServiceClient_CanSetAndGetProperties()
    {
        // Arrange
        var client = new ServiceClient();
        var id = 1;
        var clientId = "test-client";
        var clientSecret = "test-secret";
        var isActive = true;
        var createdAt = DateTime.UtcNow;
        var lastUsedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        client.Id = id;
        client.ClientId = clientId;
        client.ClientSecret = clientSecret;
        client.IsActive = isActive;
        client.CreatedAt = createdAt;
        client.LastUsedAt = lastUsedAt;

        // Assert
        Assert.Equal(id, client.Id);
        Assert.Equal(clientId, client.ClientId);
        Assert.Equal(clientSecret, client.ClientSecret);
        Assert.Equal(isActive, client.IsActive);
        Assert.True(client.CreatedAt > DateTime.MinValue);
        Assert.Equal(lastUsedAt, client.LastUsedAt);
    }

    // Student model was removed - tests removed

    [Fact]
    public void ServiceAuthRequest_DefaultValues_ShouldBeEmpty()
    {
        // Arrange & Act
        var request = new ServiceAuthRequest();

        // Assert
        Assert.Equal(string.Empty, request.ClientId);
        Assert.Equal(string.Empty, request.ClientSecret);
    }

    [Fact]
    public void ServiceAuthRequest_CanSetAndGetProperties()
    {
        // Arrange
        var request = new ServiceAuthRequest();
        var clientId = "test-client";
        var clientSecret = "test-secret";

        // Act
        request.ClientId = clientId;
        request.ClientSecret = clientSecret;

        // Assert
        Assert.Equal(clientId, request.ClientId);
        Assert.Equal(clientSecret, request.ClientSecret);
    }

    [Fact]
    public void AuthResponse_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var response = new AuthResponse();

        // Assert
        Assert.Equal(string.Empty, response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(0, response.ExpiresIn);
        Assert.Equal(DateTime.MinValue, response.ExpiresAt);
        Assert.Null(response.RefreshToken);
    }

    [Fact]
    public void AuthResponse_CanSetAndGetProperties()
    {
        // Arrange
        var response = new AuthResponse();
        var accessToken = "test-token";
        var tokenType = "Bearer";
        var expiresIn = 3600;
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var refreshToken = "refresh-token";

        // Act
        response.AccessToken = accessToken;
        response.TokenType = tokenType;
        response.ExpiresIn = expiresIn;
        response.ExpiresAt = expiresAt;
        response.RefreshToken = refreshToken;

        // Assert
        Assert.Equal(accessToken, response.AccessToken);
        Assert.Equal(tokenType, response.TokenType);
        Assert.Equal(expiresIn, response.ExpiresIn);
        Assert.Equal(expiresAt, response.ExpiresAt);
        Assert.Equal(refreshToken, response.RefreshToken);
    }

    [Fact]
    public void UserRequest_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var request = new UserRequest();

        // Assert
        Assert.Equal(string.Empty, request.Username);
        Assert.Null(request.AgeRange); // Now nullable
        Assert.Null(request.Age); // New field
        Assert.False(request.IsStudent);
        Assert.Equal(string.Empty, request.Gender);
        Assert.False(request.IsLocal);
        Assert.Empty(request.Interests);
        Assert.Equal(string.Empty, request.Budget);
        Assert.Equal(string.Empty, request.ActivityTime);
        Assert.Equal(string.Empty, request.SocialPreference);
    }

    [Fact]
    public void UserRequest_CanSetAndGetProperties()
    {
        // Arrange
        var request = new UserRequest();
        var username = "test_user";
        var age = 25;
        var isStudent = true;
        var gender = "Male";
        var isLocal = true;
        var interests = new List<string> { "theatre", "movie" };
        var budget = "500";
        var activityTime = "Evening";
        var socialPreference = "male";

        // Act
        request.Username = username;
        request.Age = age;
        request.IsStudent = isStudent;
        request.Gender = gender;
        request.IsLocal = isLocal;
        request.Interests = interests;
        request.Budget = budget;
        request.ActivityTime = activityTime;
        request.SocialPreference = socialPreference;

        // Assert
        Assert.Equal(username, request.Username);
        Assert.Equal(age, request.Age);
        Assert.Equal(isStudent, request.IsStudent);
        Assert.Equal(gender, request.Gender);
        Assert.Equal(isLocal, request.IsLocal);
        Assert.Equal(interests, request.Interests);
        Assert.Equal(budget, request.Budget);
        Assert.Equal(activityTime, request.ActivityTime);
        Assert.Equal(socialPreference, request.SocialPreference);
    }

    [Fact]
    public void UserResponse_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var response = new UserResponse();

        // Assert
        Assert.Equal(0, response.UserId);
        Assert.Equal(string.Empty, response.Username);
        Assert.Null(response.AgeRange); // Now nullable
        Assert.Null(response.Age); // New field
        Assert.False(response.IsStudent);
        Assert.Equal(string.Empty, response.Gender);
        Assert.False(response.IsLocal);
        Assert.Empty(response.Interests);
        Assert.Equal(string.Empty, response.Budget);
        Assert.Equal(string.Empty, response.ActivityTime);
        Assert.Equal(string.Empty, response.SocialPreference);
        Assert.Equal(DateTime.MinValue, response.CreatedAt);
    }

    [Fact]
    public void UserResponse_CanSetAndGetProperties()
    {
        // Arrange
        var response = new UserResponse();
        var userId = 1;
        var username = "john_doe";
        var age = 25;
        var isStudent = true;
        var gender = "Male";
        var isLocal = true;
        var interests = new List<string> { "theatre", "movie" };
        var budget = "500";
        var activityTime = "Evening";
        var socialPreference = "male";
        var createdAt = DateTime.UtcNow;

        // Act
        response.UserId = userId;
        response.Username = username;
        response.Age = age;
        response.IsStudent = isStudent;
        response.Gender = gender;
        response.IsLocal = isLocal;
        response.Interests = interests;
        response.Budget = budget;
        response.ActivityTime = activityTime;
        response.SocialPreference = socialPreference;
        response.CreatedAt = createdAt;

        // Assert
        Assert.Equal(userId, response.UserId);
        Assert.Equal(username, response.Username);
        Assert.Equal(age, response.Age);
        Assert.Equal(isStudent, response.IsStudent);
        Assert.Equal(gender, response.Gender);
        Assert.Equal(isLocal, response.IsLocal);
        Assert.Equal(interests, response.Interests);
        Assert.Equal(budget, response.Budget);
        Assert.Equal(activityTime, response.ActivityTime);
        Assert.Equal(socialPreference, response.SocialPreference);
        Assert.Equal(createdAt, response.CreatedAt);
    }

    [Fact]
    public void AnalyticsResponse_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var response = new AnalyticsResponse();

        // Assert
        Assert.Equal(string.Empty, response.MetricName);
        Assert.Equal(string.Empty, response.Category);
        Assert.Equal(string.Empty, response.Value);
        Assert.Equal(0, response.Count);
        Assert.Equal(0.0, response.Percentage);
        Assert.Equal(DateTime.MinValue, response.CalculatedAt);
    }

    [Fact]
    public void AnalyticsResponse_CanSetAndGetProperties()
    {
        // Arrange
        var response = new AnalyticsResponse();
        var metricName = "Age Distribution";
        var category = "Demographics";
        var value = "18-25";
        var count = 50;
        var percentage = 25.5;
        var calculatedAt = DateTime.UtcNow;

        // Act
        response.MetricName = metricName;
        response.Category = category;
        response.Value = value;
        response.Count = count;
        response.Percentage = percentage;
        response.CalculatedAt = calculatedAt;

        // Assert
        Assert.Equal(metricName, response.MetricName);
        Assert.Equal(category, response.Category);
        Assert.Equal(value, response.Value);
        Assert.Equal(count, response.Count);
        Assert.Equal(percentage, response.Percentage);
        Assert.Equal(calculatedAt, response.CalculatedAt);
    }

    [Fact]
    public void ComprehensiveAnalytics_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var analytics = new ComprehensiveAnalytics();

        // Assert
        Assert.NotNull(analytics.Demographics);
        Assert.NotNull(analytics.Interests);
        Assert.NotNull(analytics.Behavior);
        Assert.Equal(0, analytics.TotalResponses);
        Assert.True(analytics.GeneratedAt > DateTime.MinValue);
    }

    [Fact]
    public void ComprehensiveAnalytics_CanSetAndGetProperties()
    {
        // Arrange
        var analytics = new ComprehensiveAnalytics();
        var totalResponses = 100;
        var generatedAt = DateTime.UtcNow;

        // Act
        analytics.TotalResponses = totalResponses;
        analytics.GeneratedAt = generatedAt;

        // Assert
        Assert.Equal(totalResponses, analytics.TotalResponses);
        Assert.Equal(generatedAt, analytics.GeneratedAt);
    }
}