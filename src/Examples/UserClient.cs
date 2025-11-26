using System.Text;
using System.Text.Json;

namespace GeoStud.Api.Examples;

public class UserClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _accessToken;

    public UserClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
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
    /// Отправить данные пользователя
    /// </summary>
    public async Task<string?> SubmitUserAsync(UserRequestData userData)
    {
        try
        {
            var json = JsonSerializer.Serialize(userData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/user/submit", content);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Submit user error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Получить аналитику
    /// </summary>
    public async Task<string?> GetAnalyticsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/analytics/comprehensive");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get analytics error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Получить демографическую аналитику
    /// </summary>
    public async Task<string?> GetDemographicsAnalyticsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/analytics/demographics");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get demographics analytics error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    public async Task<string?> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/user");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get all users error: {ex.Message}");
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

public class UserRequestData
{
    public string AgeRange { get; set; } = string.Empty;
    public bool IsStudent { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public List<string> Interests { get; set; } = new();
    public string Budget { get; set; } = string.Empty;
    public string ActivityTime { get; set; } = string.Empty;
    public string SocialPreference { get; set; } = string.Empty;
}

/// <summary>
/// Пример использования User клиента
/// </summary>
public static class UserClientExample
{
    public static async Task RunExample()
    {
        const string baseUrl = "https://localhost:5001";
        
        using var client = new UserClient(baseUrl);

        Console.WriteLine("=== GeoStud User API Client Example ===\n");

        // Пример 1: Аутентификация сервиса
        Console.WriteLine("1. Service Authentication:");
        var loginSuccess = await client.ServiceLoginAsync("mobile-app", "MobileAppSecret123!");
        Console.WriteLine($"Service login result: {loginSuccess}\n");

        if (loginSuccess)
        {
            // Пример 2: Отправка данных пользователя
            Console.WriteLine("2. Submitting User:");
            var userData = new UserRequestData
            {
                AgeRange = "17-22",
                IsStudent = true,
                Gender = "Male",
                IsLocal = true,
                Interests = new List<string> { "Учеба", "Спорт", "Музыка" },
                Budget = "500",
                ActivityTime = "Evening",
                SocialPreference = "Group"
            };

            var submitResult = await client.SubmitUserAsync(userData);
            Console.WriteLine($"Submit result: {submitResult}\n");

            // Пример 3: Получение аналитики
            Console.WriteLine("3. Getting Analytics:");
            var analytics = await client.GetAnalyticsAsync();
            Console.WriteLine($"Analytics: {analytics}\n");

            // Пример 4: Демографическая аналитика
            Console.WriteLine("4. Getting Demographics Analytics:");
            var demographics = await client.GetDemographicsAnalyticsAsync();
            Console.WriteLine($"Demographics: {demographics}\n");

            // Пример 5: Все пользователи
            Console.WriteLine("5. Getting All Users:");
            var allUsers = await client.GetAllUsersAsync();
            Console.WriteLine($"All users: {allUsers}\n");
        }

        Console.WriteLine("=== Example completed ===");
    }
}
