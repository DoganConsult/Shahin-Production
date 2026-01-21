using GrcMvc.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Webhook Delivery Service - Delivers HTTP webhooks with retry logic
/// Follows ASP.NET Core HttpClient best practices with IHttpClientFactory
/// </summary>
public class WebhookDeliveryService : IWebhookDeliveryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDeliveryService> _logger;

    public WebhookDeliveryService(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookDeliveryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<WebhookDeliveryResult> DeliverWebhookAsync(
        string webhookUrl,
        object payload,
        Dictionary<string, string>? headers = null,
        int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        var stopwatch = Stopwatch.StartNew();
        var httpClient = _httpClientFactory.CreateClient("WebhookClient");
        httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

        try
        {
            _logger.LogInformation("Delivering webhook to: {WebhookUrl}", webhookUrl);

            // Prepare request
            var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
            {
                Content = JsonContent.Create(payload)
            };

            // Add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add standard headers
            request.Headers.TryAddWithoutValidation("User-Agent", "GRC-Integration-Service/1.0");
            request.Headers.TryAddWithoutValidation("X-GRC-Webhook", "true");

            // Send request
            var response = await httpClient.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = new WebhookDeliveryResult
            {
                Success = response.IsSuccessStatusCode,
                HttpStatusCode = (int)response.StatusCode,
                ResponseBody = responseBody.Length > 2000 ? responseBody.Substring(0, 2000) + "..." : responseBody,
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                AttemptNumber = 1
            };

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook delivered successfully: {WebhookUrl} - {StatusCode} ({LatencyMs}ms)",
                    webhookUrl, result.HttpStatusCode, result.LatencyMs);
            }
            else
            {
                _logger.LogWarning("Webhook delivery failed: {WebhookUrl} - {StatusCode} - {ResponseBody}",
                    webhookUrl, result.HttpStatusCode, responseBody);
                result.ErrorMessage = $"HTTP {result.HttpStatusCode}: {responseBody}";
            }

            return result;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Webhook delivery timeout: {WebhookUrl} after {TimeoutSeconds}s", webhookUrl, timeoutSeconds);

            return new WebhookDeliveryResult
            {
                Success = false,
                HttpStatusCode = 0,
                ErrorMessage = $"Request timeout after {timeoutSeconds} seconds",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                AttemptNumber = 1
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Webhook delivery HTTP error: {WebhookUrl}", webhookUrl);

            return new WebhookDeliveryResult
            {
                Success = false,
                HttpStatusCode = 0,
                ErrorMessage = $"HTTP error: {ex.Message}",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                AttemptNumber = 1
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Webhook delivery unexpected error: {WebhookUrl}", webhookUrl);

            return new WebhookDeliveryResult
            {
                Success = false,
                HttpStatusCode = 0,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                AttemptNumber = 1
            };
        }
    }

    public async Task<WebhookDeliveryResult> DeliverWithRetryAsync(
        string webhookUrl,
        object payload,
        string retryPolicy = "Exponential",
        int maxRetries = 3,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        WebhookDeliveryResult? lastResult = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            lastResult = await DeliverWebhookAsync(webhookUrl, payload, headers, 30, cancellationToken);
            lastResult.AttemptNumber = attempt;

            if (lastResult.Success)
            {
                return lastResult;
            }

            // Don't retry on client errors (4xx)
            if (lastResult.HttpStatusCode >= 400 && lastResult.HttpStatusCode < 500)
            {
                _logger.LogWarning("Client error {StatusCode}, not retrying: {WebhookUrl}",
                    lastResult.HttpStatusCode, webhookUrl);
                return lastResult;
            }

            if (attempt < maxRetries)
            {
                var delayMs = CalculateRetryDelay(attempt, retryPolicy);
                _logger.LogInformation("Retrying webhook delivery (attempt {Attempt}/{MaxRetries}) after {DelayMs}ms: {WebhookUrl}",
                    attempt + 1, maxRetries, delayMs, webhookUrl);

                await Task.Delay(delayMs, cancellationToken);
            }
        }

        _logger.LogError("Webhook delivery failed after {MaxRetries} attempts: {WebhookUrl}", maxRetries, webhookUrl);
        return lastResult!;
    }

    public async Task<bool> ValidateWebhookEndpointAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);

        try
        {
            _logger.LogInformation("Validating webhook endpoint: {WebhookUrl}", webhookUrl);

            var httpClient = _httpClientFactory.CreateClient("WebhookClient");
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Send ping test
            var pingPayload = new
            {
                Type = "ping",
                Source = "GRC-Integration-Service",
                Timestamp = DateTime.UtcNow
            };

            var result = await DeliverWebhookAsync(webhookUrl, pingPayload, null, 10, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Webhook endpoint validation successful: {WebhookUrl}", webhookUrl);
                return true;
            }

            _logger.LogWarning("Webhook endpoint validation failed: {WebhookUrl} - {StatusCode}",
                webhookUrl, result.HttpStatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook endpoint validation error: {WebhookUrl}", webhookUrl);
            return false;
        }
    }

    private int CalculateRetryDelay(int attemptNumber, string retryPolicy)
    {
        return retryPolicy switch
        {
            "Linear" => attemptNumber * 1000, // 1s, 2s, 3s...
            "Exponential" => (int)Math.Pow(2, attemptNumber) * 1000, // 2s, 4s, 8s...
            "None" => 0,
            _ => (int)Math.Pow(2, attemptNumber) * 1000 // Default to exponential
        };
    }
}
