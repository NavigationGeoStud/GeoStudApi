using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class NeuroApiService : INeuroApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NeuroApiService> _logger;
    private readonly string _apiKey;
    private const string ApiBaseUrl = "https://neuroapi.host/v1";

    public NeuroApiService(HttpClient httpClient, ILogger<NeuroApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["NeuroApi:ApiKey"] ?? "sk-LTmOvWR5cjz5o9bB0SgrpKOxzX9mH3giAV9A4jKpz9UESw4o";
        
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(ApiBaseUrl);
        }
    }

    public async Task<string> GenerateLocationDescriptionAsync(
        string locationName, 
        string? existingDescription, 
        string? address, 
        string? city, 
        string? categoryName)
    {
        try
        {
            var prompt = BuildPrompt(locationName, existingDescription, address, city, categoryName);

            var requestBody = new
            {
                model = "gpt-5-mini",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7,
                max_tokens = 200
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to NeuroAPI for location: {LocationName}", locationName);

            var requestUri = _httpClient.BaseAddress != null 
                ? new Uri(_httpClient.BaseAddress, "/chat/completions")
                : new Uri($"{ApiBaseUrl}/chat/completions");
            
            // Create request message with Authorization header
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);

            var description = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(description))
            {
                _logger.LogWarning("NeuroAPI returned empty description for location: {LocationName}", locationName);
                return existingDescription ?? $"Интересное место в категории {categoryName ?? "развлечений"}.";
            }

            _logger.LogInformation("Successfully generated description for location: {LocationName}", locationName);
            return description.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating description via NeuroAPI for location: {LocationName}", locationName);
            // Return existing description or fallback
            return existingDescription ?? $"Интересное место в категории {categoryName ?? "развлечений"}.";
        }
    }

    private string BuildPrompt(string locationName, string? existingDescription, string? address, string? city, string? categoryName)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Напиши краткое описание для локации в 2-3 строки. Описание должно быть интересным и привлекательным для студентов.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"Название локации: {locationName}");

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            promptBuilder.AppendLine($"Категория: {categoryName}");
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            promptBuilder.AppendLine($"Город: {city}");
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            promptBuilder.AppendLine($"Адрес: {address}");
        }

        if (!string.IsNullOrWhiteSpace(existingDescription))
        {
            promptBuilder.AppendLine($"Текущее описание: {existingDescription}");
            promptBuilder.AppendLine("Улучши и дополни текущее описание, сделай его более интересным.");
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Напиши описание в 2-3 строки, которое будет привлекательным для студентов и молодых людей.");

        return promptBuilder.ToString();
    }
}

