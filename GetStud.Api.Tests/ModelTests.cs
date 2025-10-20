using GeoStud.Api.Models;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.DTOs.Survey;
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

    [Fact]
    public void Student_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var student = new Student();

        // Assert
        Assert.Equal(0, student.Id);
        Assert.Null(student.FirstName); // FirstName is nullable
        Assert.Null(student.LastName); // LastName is nullable
        Assert.Equal(string.Empty, student.Email);
        Assert.True(student.CreatedAt > DateTime.MinValue);
        Assert.Null(student.UpdatedAt);
    }

    [Fact]
    public void Student_CanSetAndGetProperties()
    {
        // Arrange
        var student = new Student();
        var id = 1;
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        student.Id = id;
        student.FirstName = firstName;
        student.LastName = lastName;
        student.Email = email;
        student.CreatedAt = createdAt;
        student.UpdatedAt = updatedAt;

        // Assert
        Assert.Equal(id, student.Id);
        Assert.Equal(firstName, student.FirstName);
        Assert.Equal(lastName, student.LastName);
        Assert.Equal(email, student.Email);
        Assert.True(student.CreatedAt > DateTime.MinValue);
        Assert.Equal(updatedAt, student.UpdatedAt);
    }

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
    public void SurveyRequest_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var request = new SurveyRequest();

        // Assert
        Assert.Equal(string.Empty, request.AgeRange);
        Assert.False(request.IsStudent);
        Assert.Equal(string.Empty, request.Gender);
        Assert.False(request.IsLocal);
        Assert.Empty(request.Interests);
        Assert.Equal(string.Empty, request.Budget);
        Assert.Equal(string.Empty, request.ActivityTime);
        Assert.Equal(string.Empty, request.SocialPreference);
    }

    [Fact]
    public void SurveyRequest_CanSetAndGetProperties()
    {
        // Arrange
        var request = new SurveyRequest();
        var ageRange = "18-25";
        var isStudent = true;
        var gender = "Male";
        var isLocal = true;
        var interests = new List<string> { "Sports", "Music" };
        var budget = "100-500";
        var activityTime = "Evening";
        var socialPreference = "Group";

        // Act
        request.AgeRange = ageRange;
        request.IsStudent = isStudent;
        request.Gender = gender;
        request.IsLocal = isLocal;
        request.Interests = interests;
        request.Budget = budget;
        request.ActivityTime = activityTime;
        request.SocialPreference = socialPreference;

        // Assert
        Assert.Equal(ageRange, request.AgeRange);
        Assert.Equal(isStudent, request.IsStudent);
        Assert.Equal(gender, request.Gender);
        Assert.Equal(isLocal, request.IsLocal);
        Assert.Equal(interests, request.Interests);
        Assert.Equal(budget, request.Budget);
        Assert.Equal(activityTime, request.ActivityTime);
        Assert.Equal(socialPreference, request.SocialPreference);
    }

    [Fact]
    public void SurveyResponse_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var response = new SurveyResponse();

        // Assert
        Assert.Equal(0, response.StudentId);
        Assert.Equal(string.Empty, response.Username);
        Assert.Equal(string.Empty, response.Email);
        Assert.Equal(string.Empty, response.AgeRange);
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
    public void SurveyResponse_CanSetAndGetProperties()
    {
        // Arrange
        var response = new SurveyResponse();
        var studentId = 1;
        var username = "john_doe";
        var email = "john@example.com";
        var ageRange = "18-25";
        var isStudent = true;
        var gender = "Male";
        var isLocal = true;
        var interests = new List<string> { "Sports", "Music" };
        var budget = "100-500";
        var activityTime = "Evening";
        var socialPreference = "Group";
        var createdAt = DateTime.UtcNow;

        // Act
        response.StudentId = studentId;
        response.Username = username;
        response.Email = email;
        response.AgeRange = ageRange;
        response.IsStudent = isStudent;
        response.Gender = gender;
        response.IsLocal = isLocal;
        response.Interests = interests;
        response.Budget = budget;
        response.ActivityTime = activityTime;
        response.SocialPreference = socialPreference;
        response.CreatedAt = createdAt;

        // Assert
        Assert.Equal(studentId, response.StudentId);
        Assert.Equal(username, response.Username);
        Assert.Equal(email, response.Email);
        Assert.Equal(ageRange, response.AgeRange);
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