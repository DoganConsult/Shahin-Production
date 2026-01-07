using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing webhook subscriptions and delivering webhook events.
    /// </summary>
    public interface IWebhookService
    {
        // Subscription Management
        Task<WebhookSubscription> CreateSubscriptionAsync(Guid tenantId, CreateWebhookRequest request);
        Task<WebhookSubscription?> GetSubscriptionAsync(Guid subscriptionId);
        Task<List<WebhookSubscription>> GetSubscriptionsAsync(Guid tenantId);
        Task<WebhookSubscription> UpdateSubscriptionAsync(Guid subscriptionId, UpdateWebhookRequest request);
        Task DeleteSubscriptionAsync(Guid subscriptionId);
        Task<bool> ToggleSubscriptionAsync(Guid subscriptionId, bool isActive);

        // Event Delivery
        Task TriggerEventAsync(Guid tenantId, string eventType, string eventId, object payload);
        Task<WebhookDeliveryLog> DeliverWebhookAsync(WebhookSubscription subscription, string eventType, string eventId, object payload);
        Task RetryFailedDeliveriesAsync();

        // Delivery Logs
        Task<List<WebhookDeliveryLog>> GetDeliveryLogsAsync(Guid subscriptionId, int pageSize = 50, int page = 1);
        Task<WebhookDeliveryLog?> GetDeliveryLogAsync(Guid deliveryLogId);

        // Health & Testing
        Task<WebhookTestResult> TestWebhookAsync(Guid subscriptionId);
        Task<string> GenerateSecretAsync();
        string SignPayload(string payload, string secret);
        bool VerifySignature(string payload, string signature, string secret);
    }

    /// <summary>
    /// Request model for creating a webhook subscription
    /// </summary>
    public class CreateWebhookRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EventTypes { get; set; } = "*";
        public Dictionary<string, string>? CustomHeaders { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 5;
    }

    /// <summary>
    /// Request model for updating a webhook subscription
    /// </summary>
    public class UpdateWebhookRequest
    {
        public string? Name { get; set; }
        public string? TargetUrl { get; set; }
        public string? Description { get; set; }
        public string? EventTypes { get; set; }
        public Dictionary<string, string>? CustomHeaders { get; set; }
        public int? TimeoutSeconds { get; set; }
        public int? MaxRetries { get; set; }
        public bool? RegenerateSecret { get; set; }
    }

    /// <summary>
    /// Result model for webhook test
    /// </summary>
    public class WebhookTestResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
