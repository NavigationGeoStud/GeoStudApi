using System.Text;
using System.Text.Json;

namespace GeoStud.Api.Examples;

public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _accessToken;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Аутентификация пользователя
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new
            {
                username,
                password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                
                if (authResponse != null)
                {
                    _accessToken = authResponse.AccessToken;
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Аутентификация сервиса
    /// </summary>
    public async Task<bool> ServiceLoginAsync(string clientId, string clientSecret)
    {
        try
        {
            var serviceRequest = new
            {
                clientId,
                clientSecret
            };

            var json = JsonSerializer.Serialize(serviceRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/service-login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                
                if (authResponse != null)
                {
                    _accessToken = authResponse.AccessToken;
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Service login error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Получить список геолокаций
    /// </summary>
    public async Task<string?> GetGeoLocationsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/geolocation");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get geo locations error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Создать новую геолокацию
    /// </summary>
    public async Task<string?> CreateGeoLocationAsync(object location)
    {
        try
        {
            var json = JsonSerializer.Serialize(location);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/geolocation", content);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create geo location error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    public async Task<string?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/user/profile");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get current user error: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}

/// <summary>
/// Пример использования API клиента
/// </summary>
public static class ApiClientExample
{
    public static async Task RunExample()
    {
        const string baseUrl = "https://localhost:5001";
        
        using var client = new ApiClient(baseUrl);

        Console.WriteLine("=== GeoStud API Client Example ===\n");

        // Пример 1: Аутентификация сервиса
        Console.WriteLine("1. Service Authentication:");
        var serviceLoginSuccess = await client.ServiceLoginAsync("mobile-app", "MobileAppSecret123!");
        Console.WriteLine($"Service login result: {serviceLoginSuccess}\n");

        if (serviceLoginSuccess)
        {
            // Получить геолокации
            Console.WriteLine("3. Getting Geo Locations:");
            var locations = await client.GetGeoLocationsAsync();
            Console.WriteLine($"Locations: {locations}\n");

            // Создать новую геолокацию
            Console.WriteLine("4. Creating New Geo Location:");
            var newLocation = new
            {
                Name = "Test Location",
                Description = "Test location created by API client",
                Latitude = 55.7558,
                Longitude = 37.6176,
                Country = "Russia",
                City = "Moscow"
            };

            var createResult = await client.CreateGeoLocationAsync(newLocation);
            Console.WriteLine($"Create result: {createResult}\n");
        }

        Console.WriteLine("=== Example completed ===");
    }
}
