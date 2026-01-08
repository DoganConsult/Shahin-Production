using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for managing webhooks and delivering events to external systems.
    /// Implements HMAC-SHA256 signature verification and exponential backoff retry.
    /// </summary>
    public class WebhookService : IWebhookService
    {
        private readonly GrcDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebhookService> _logger;
        private readonly WebhookSettings _settings;

        public WebhookService(
            GrcDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<WebhookService> logger,
            IOptions<WebhookSettings> settings)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings.Value;
        }

        #region Subscription Management

        public async Task<WebhookSubscription> CreateSubscriptionAsync(Guid tenantId, CreateWebhookRequest request)
        {
            var subscription = new WebhookSubscription
            {
                TenantId = tenantId,
                Name = request.Name,
                TargetUrl = request.TargetUrl,
                Description = request.Description,
                EventTypes = request.EventTypes,
                Secret = await GenerateSecretAsync(),
                TimeoutSeconds = request.TimeoutSeconds,
                MaxRetries = request.MaxRetries,
                CustomHeaders = request.CustomHeaders != null
                    ? JsonSerializer.Serialize(request.CustomHeaders)
                    : null,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.WebhookSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Webhook subscription created: {SubscriptionId} for tenant {TenantId}",
                subscription.Id, tenantId);

            return subscription;
        }

        public async Task<WebhookSubscription?> GetSubscriptionAsync(Guid subscriptionId)
        {
            return await _context.WebhookSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == subscriptionId && !w.IsDeleted);
        }

        public async Task<List<WebhookSubscription>> GetSubscriptionsAsync(Guid tenantId)
        {
            return await _context.WebhookSubscriptions
                .AsNoTracking()
                .Where(w => w.TenantId == tenantId && !w.IsDeleted)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<WebhookSubscription> UpdateSubscriptionAsync(Guid subscriptionId, UpdateWebhookRequest request)
        {
            var subscription = await _context.WebhookSubscriptions
                .FirstOrDefaultAsync(w => w.Id == subscriptionId && !w.IsDeleted)
                ?? throw new EntityNotFoundException("WebhookSubscription", subscriptionId);

            if (request.Name != null) subscription.Name = request.Name;
            if (request.TargetUrl != null) subscription.TargetUrl = request.TargetUrl;
            if (request.Description != null) subscription.Description = request.Description;
            if (request.EventTypes != null) subscription.EventTypes = request.EventTypes;
            if (request.TimeoutSeconds.HasValue) subscription.TimeoutSeconds = request.TimeoutSeconds.Value;
            if (request.MaxRetries.HasValue) subscription.MaxRetries = request.MaxRetries.Value;
            if (request.CustomHeaders != null)
                subscription.CustomHeaders = JsonSerializer.Serialize(request.CustomHeaders);
            if (request.RegenerateSecret == true)
                subscription.Secret = await GenerateSecretAsync();

            subscription.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Webhook subscription updated: {SubscriptionId}", subscriptionId);
            return subscription;
        }

        public async Task DeleteSubscriptionAsync(Guid subscriptionId)
        {
            var subscription = await _context.WebhookSubscriptions
                .FirstOrDefaultAsync(w => w.Id == subscriptionId && !w.IsDeleted);

            if (subscription != null)
            {
                subscription.IsDeleted = true;
                subscription.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Webhook subscription deleted: {SubscriptionId}", subscriptionId);
            }
        }

        public async Task<bool> ToggleSubscriptionAsync(Guid subscriptionId, bool isActive)
        {
            var subscription = await _context.WebhookSubscriptions
                .FirstOrDefaultAsync(w => w.Id == subscriptionId && !w.IsDeleted);

            if (subscription == null) return false;

            subscription.IsActive = isActive;
            subscription.ModifiedDate = DateTime.UtcNow;

            if (isActive)
            {
                subscription.ConsecutiveFailures = 0;
                subscription.DisabledAt = null;
                subscription.DisabledReason = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Webhook subscription {SubscriptionId} {Status}",
                subscriptionId, isActive ? "activated" : "deactivated");

            return true;
        }

        #endregion

        #region Event Delivery

        public async Task TriggerEventAsync(Guid tenantId, string eventType, string eventId, object payload)
        {
            var subscriptions = await _context.WebhookSubscriptions
                .Where(w => w.TenantId == tenantId &&
                           w.IsActive &&
                           !w.IsDeleted)
                .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                if (ShouldTriggerSubscription(subscription, eventType))
                {
                    try
                    {
                        await DeliverWebhookAsync(subscription, eventType, eventId, payload);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error delivering webhook to {SubscriptionId} for event {EventType}",
                            subscription.Id, eventType);
                    }
                }
            }
        }

        private bool ShouldTriggerSubscription(WebhookSubscription subscription, string eventType)
        {
            if (subscription.EventTypes == "*") return true;

            var subscribedEvents = subscription.EventTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return subscribedEvents.Any(e =>
                e.Equals(eventType, StringComparison.OrdinalIgnoreCase) ||
                eventType.StartsWith(e.TrimEnd('*'), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<WebhookDeliveryLog> DeliverWebhookAsync(
            WebhookSubscription subscription,
            string eventType,
            string eventId,
            object payload)
        {
            var webhookPayload = new
            {
                id = eventId,
                type = eventType,
                timestamp = DateTime.UtcNow.ToString("o"),
                tenantId = subscription.TenantId,
                data = payload
            };

            var payloadJson = JsonSerializer.Serialize(webhookPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var signature = SignPayload(payloadJson, subscription.Secret);

            var deliveryLog = new WebhookDeliveryLog
            {
                TenantId = subscription.TenantId,
                WebhookSubscriptionId = subscription.Id,
                EventType = eventType,
                EventId = eventId,
                PayloadJson = payloadJson,
                TargetUrl = subscription.TargetUrl,
                Signature = signature,
                Status = WebhookDeliveryStatus.Pending,
                FirstAttemptAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.WebhookDeliveryLogs.Add(deliveryLog);
            await _context.SaveChangesAsync();

            // Attempt delivery
            await AttemptDeliveryAsync(deliveryLog, subscription);

            return deliveryLog;
        }

        private async Task AttemptDeliveryAsync(WebhookDeliveryLog deliveryLog, WebhookSubscription subscription)
        {
            var stopwatch = Stopwatch.StartNew();
            deliveryLog.AttemptCount++;
            deliveryLog.LastAttemptAt = DateTime.UtcNow;

            try
            {
                using var client = _httpClientFactory.CreateClient("WebhookClient");
                client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds);

                var request = new HttpRequestMessage(HttpMethod.Post, deliveryLog.TargetUrl);
                request.Content = new StringContent(deliveryLog.PayloadJson, Encoding.UTF8, subscription.ContentType);

                // Add signature header
                request.Headers.Add("X-Webhook-Signature", $"sha256={deliveryLog.Signature}");
                request.Headers.Add("X-Webhook-Event", deliveryLog.EventType);
                request.Headers.Add("X-Webhook-Delivery", deliveryLog.Id.ToString());
                request.Headers.Add("X-Webhook-Timestamp", DateTime.UtcNow.ToString("o"));

                // Add custom headers
                if (!string.IsNullOrEmpty(subscription.CustomHeaders))
                {
                    var customHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(subscription.CustomHeaders);
                    if (customHeaders != null)
                    {
                        foreach (var header in customHeaders)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                deliveryLog.RequestHeaders = JsonSerializer.Serialize(
                    request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)));

                var response = await client.SendAsync(request);
                stopwatch.Stop();

                deliveryLog.ResponseStatusCode = (int)response.StatusCode;
                deliveryLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                deliveryLog.ResponseBody = await response.Content.ReadAsStringAsync();
                if (deliveryLog.ResponseBody?.Length > 2000)
                    deliveryLog.ResponseBody = deliveryLog.ResponseBody[..2000];

                deliveryLog.ResponseHeaders = JsonSerializer.Serialize(
                    response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)));

                if (response.IsSuccessStatusCode)
                {
                    deliveryLog.Status = WebhookDeliveryStatus.Delivered;
                    deliveryLog.DeliveredAt = DateTime.UtcNow;

                    // Update subscription stats
                    subscription.LastSuccessAt = DateTime.UtcNow;
                    subscription.SuccessCount++;
                    subscription.ConsecutiveFailures = 0;

                    _logger.LogInformation(
                        "Webhook delivered successfully: {DeliveryId} to {Url} in {Ms}ms",
                        deliveryLog.Id, deliveryLog.TargetUrl, deliveryLog.ResponseTimeMs);
                }
                else
                {
                    await HandleDeliveryFailureAsync(deliveryLog, subscription,
                        $"HTTP {deliveryLog.ResponseStatusCode}: {response.ReasonPhrase}");
                }
            }
            catch (TaskCanceledException)
            {
                stopwatch.Stop();
                deliveryLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                await HandleDeliveryFailureAsync(deliveryLog, subscription, "Request timeout");
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                deliveryLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                await HandleDeliveryFailureAsync(deliveryLog, subscription, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                deliveryLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                deliveryLog.ErrorStackTrace = ex.StackTrace;
                await HandleDeliveryFailureAsync(deliveryLog, subscription, ex.Message);
            }

            await _context.SaveChangesAsync();
        }

        private async Task HandleDeliveryFailureAsync(
            WebhookDeliveryLog deliveryLog,
            WebhookSubscription subscription,
            string errorMessage)
        {
            deliveryLog.ErrorMessage = errorMessage;
            subscription.LastFailureAt = DateTime.UtcNow;
            subscription.LastErrorMessage = errorMessage;
            subscription.FailureCount++;
            subscription.ConsecutiveFailures++;

            _logger.LogWarning(
                "Webhook delivery failed: {DeliveryId} attempt {Attempt}/{Max} - {Error}",
                deliveryLog.Id, deliveryLog.AttemptCount, subscription.MaxRetries, errorMessage);

            if (deliveryLog.AttemptCount < subscription.MaxRetries)
            {
                deliveryLog.Status = WebhookDeliveryStatus.Retrying;

                // Calculate next retry with exponential backoff
                var delays = subscription.RetryDelays
                    .Split(',')
                    .Select(d => int.TryParse(d.Trim(), out var v) ? v : 60)
                    .ToArray();

                var delayIndex = Math.Min(deliveryLog.AttemptCount - 1, delays.Length - 1);
                var delaySeconds = delays[delayIndex];

                deliveryLog.NextRetryAt = DateTime.UtcNow.AddSeconds(delaySeconds);

                _logger.LogInformation(
                    "Webhook retry scheduled: {DeliveryId} at {RetryAt}",
                    deliveryLog.Id, deliveryLog.NextRetryAt);
            }
            else
            {
                deliveryLog.Status = WebhookDeliveryStatus.Failed;

                // Check if subscription should be disabled
                if (subscription.DisableAfterFailures > 0 &&
                    subscription.ConsecutiveFailures >= subscription.DisableAfterFailures)
                {
                    subscription.IsActive = false;
                    subscription.DisabledAt = DateTime.UtcNow;
                    subscription.DisabledReason = $"Auto-disabled after {subscription.ConsecutiveFailures} consecutive failures";

                    _logger.LogWarning(
                        "Webhook subscription auto-disabled: {SubscriptionId} after {Failures} failures",
                        subscription.Id, subscription.ConsecutiveFailures);
                }
            }

            await Task.CompletedTask;
        }

        public async Task RetryFailedDeliveriesAsync()
        {
            var pendingRetries = await _context.WebhookDeliveryLogs
                .Include(d => d.WebhookSubscription)
                .Where(d => d.Status == WebhookDeliveryStatus.Retrying &&
                           d.NextRetryAt.HasValue &&
                           d.NextRetryAt.Value <= DateTime.UtcNow)
                .Take(100)
                .ToListAsync();

            _logger.LogInformation("Processing {Count} pending webhook retries", pendingRetries.Count);

            foreach (var deliveryLog in pendingRetries)
            {
                if (deliveryLog.WebhookSubscription != null &&
                    deliveryLog.WebhookSubscription.IsActive &&
                    !deliveryLog.WebhookSubscription.IsDeleted)
                {
                    await AttemptDeliveryAsync(deliveryLog, deliveryLog.WebhookSubscription);
                }
                else
                {
                    deliveryLog.Status = WebhookDeliveryStatus.Cancelled;
                    deliveryLog.ErrorMessage = "Subscription no longer active";
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Delivery Logs

        public async Task<List<WebhookDeliveryLog>> GetDeliveryLogsAsync(
            Guid subscriptionId, int pageSize = 50, int page = 1)
        {
            return await _context.WebhookDeliveryLogs
                .AsNoTracking()
                .Where(d => d.WebhookSubscriptionId == subscriptionId)
                .OrderByDescending(d => d.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WebhookDeliveryLog?> GetDeliveryLogAsync(Guid deliveryLogId)
        {
            return await _context.WebhookDeliveryLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deliveryLogId);
        }

        #endregion

        #region Health & Testing

        public async Task<WebhookTestResult> TestWebhookAsync(Guid subscriptionId)
        {
            var subscription = await _context.WebhookSubscriptions
                .FirstOrDefaultAsync(w => w.Id == subscriptionId && !w.IsDeleted)
                ?? throw new EntityNotFoundException("WebhookSubscription", subscriptionId);

            var testPayload = new
            {
                test = true,
                message = "This is a test webhook from Shahin GRC",
                timestamp = DateTime.UtcNow.ToString("o")
            };

            var payloadJson = JsonSerializer.Serialize(testPayload);
            var signature = SignPayload(payloadJson, subscription.Secret);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = _httpClientFactory.CreateClient("WebhookClient");
                client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds);

                var request = new HttpRequestMessage(HttpMethod.Post, subscription.TargetUrl);
                request.Content = new StringContent(payloadJson, Encoding.UTF8, subscription.ContentType);
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
                request.Headers.Add("X-Webhook-Event", "test");
                request.Headers.Add("X-Webhook-Test", "true");

                var response = await client.SendAsync(request);
                stopwatch.Stop();

                return new WebhookTestResult
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ResponseBody = await response.Content.ReadAsStringAsync(),
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new WebhookTestResult
                {
                    Success = false,
                    StatusCode = 0,
                    ErrorMessage = ex.Message,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public Task<string> GenerateSecretAsync()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Task.FromResult(Convert.ToBase64String(bytes));
        }

        public string SignPayload(string payload, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public bool VerifySignature(string payload, string signature, string secret)
        {
            var expectedSignature = SignPayload(payload, secret);
            return string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }

    /// <summary>
    /// Webhook configuration settings
    /// </summary>
    public class WebhookSettings
    {
        public int MaxRetries { get; set; } = 5;
        public int[] RetryDelaySeconds { get; set; } = { 10, 30, 120, 600, 3600 };
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxConcurrentDeliveries { get; set; } = 10;
    }
}
