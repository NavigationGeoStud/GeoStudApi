using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GeoStud.Api.DTOs.Notification;
using GeoStud.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GeoStud.Api.Services;

public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookService> _logger;
    private string? _webhookUrl;
    private string? _webhookSecret;

    public WebhookService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        // Load webhook URL from configuration
        _webhookUrl = _configuration["Webhook:Url"];
        _webhookSecret = _configuration["Webhook:Secret"];
        
        // Set timeout for webhook requests
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<bool> SendNotificationWebhookAsync(long telegramId, NotificationResponse notification)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
        {
            _logger.LogDebug("Webhook URL is not configured, skipping webhook notification");
            return false;
        }

        try
        {
            _logger.LogDebug("Sending webhook notification to {WebhookUrl} for telegramId {TelegramId}", 
                _webhookUrl, telegramId);

            var payload = new
            {
                telegramId = telegramId,
                notification = notification
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _webhookUrl)
            {
                Content = JsonContent.Create(payload, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            // Add secret token to header if configured
            if (!string.IsNullOrEmpty(_webhookSecret))
            {
                request.Headers.Add("X-Webhook-Secret", _webhookSecret);
            }

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook notification sent successfully for telegramId {TelegramId}, notificationId {NotificationId}", 
                    telegramId, notification.Id);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Webhook notification failed with status {StatusCode} for telegramId {TelegramId}. Response: {Response}", 
                    response.StatusCode, telegramId, errorContent);
                return false;
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Webhook request timeout for telegramId {TelegramId}", telegramId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook notification for telegramId {TelegramId}", telegramId);
            return false;
        }
    }

    public Task<bool> ConfigureWebhookAsync(string webhookUrl, string? secret = null)
    {
        try
        {
            // Validate URL
            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
            {
                _logger.LogWarning("Invalid webhook URL: {WebhookUrl}", webhookUrl);
                return Task.FromResult(false);
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                _logger.LogWarning("Webhook URL must use http or https scheme: {WebhookUrl}", webhookUrl);
                return Task.FromResult(false);
            }

            _webhookUrl = webhookUrl;
            _webhookSecret = secret;

            _logger.LogInformation("Webhook configured: URL={WebhookUrl}, HasSecret={HasSecret}", 
                webhookUrl, !string.IsNullOrEmpty(secret));

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring webhook: {WebhookUrl}", webhookUrl);
            return Task.FromResult(false);
        }
    }

    public string? GetWebhookUrl()
    {
        return _webhookUrl;
    }
}

