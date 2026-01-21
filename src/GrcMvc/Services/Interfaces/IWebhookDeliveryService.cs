namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Webhook Delivery Service - Delivers HTTP webhooks to external systems
/// </summary>
public interface IWebhookDeliveryService
{
    /// <summary>
    /// Deliver webhook via HTTP POST
    /// </summary>
    Task<WebhookDeliveryResult> DeliverWebhookAsync(
        string webhookUrl,
        object payload,
        Dictionary<string, string>? headers = null,
        int timeoutSeconds = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deliver webhook with retry policy
    /// </summary>
    Task<WebhookDeliveryResult> DeliverWithRetryAsync(
        string webhookUrl,
        object payload,
        string retryPolicy = "Exponential",
        int maxRetries = 3,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate webhook endpoint (ping test)
    /// </summary>
    Task<bool> ValidateWebhookEndpointAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Webhook delivery result
/// </summary>
public class WebhookDeliveryResult
{
    public bool Success { get; set; }
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public int LatencyMs { get; set; }
    public int AttemptNumber { get; set; }
}
