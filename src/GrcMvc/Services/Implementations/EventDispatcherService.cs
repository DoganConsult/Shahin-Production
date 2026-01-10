using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Event Dispatcher Service - Dispatches events to subscribers via webhooks, queues, or direct calls
/// Follows ASP.NET Core patterns with dependency injection
/// </summary>
public class EventDispatcherService : IEventDispatcherService
{
    private readonly GrcDbContext _context;
    private readonly IWebhookDeliveryService _webhookService;
    private readonly ILogger<EventDispatcherService> _logger;

    public EventDispatcherService(
        GrcDbContext context,
        IWebhookDeliveryService webhookService,
        ILogger<EventDispatcherService> logger)
    {
        _context = context;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task<bool> DispatchEventAsync(Guid eventDeliveryLogId, CancellationToken cancellationToken = default)
    {
        var deliveryLog = await _context.Set<EventDeliveryLog>()
            .Include(d => d.Event)
            .Include(d => d.Subscription)
            .FirstOrDefaultAsync(d => d.Id == eventDeliveryLogId, cancellationToken);

        if (deliveryLog == null)
        {
            _logger.LogWarning("Event delivery log not found: {DeliveryLogId}", eventDeliveryLogId);
            return false;
        }

        _logger.LogInformation("Dispatching event delivery: {EventType} to {SubscriberSystem} via {DeliveryMethod}",
            deliveryLog.Event.EventType, deliveryLog.Subscription.SubscriberSystem, deliveryLog.Subscription.DeliveryMethod);

        deliveryLog.AttemptNumber++;
        deliveryLog.AttemptedAt = DateTime.UtcNow;
        deliveryLog.ModifiedDate = DateTime.UtcNow;

        try
        {
            // Parse payload
            var payload = JsonSerializer.Deserialize<object>(deliveryLog.Event.PayloadJson);

            // Dispatch based on delivery method
            bool success = deliveryLog.Subscription.DeliveryMethod switch
            {
                "Webhook" => await DispatchWebhookAsync(deliveryLog, payload, cancellationToken),
                "Queue" => await DispatchQueueAsync(deliveryLog, payload, cancellationToken),
                "DirectCall" => await DispatchDirectCallAsync(deliveryLog, payload, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown delivery method: {deliveryLog.Subscription.DeliveryMethod}")
            };

            if (success)
            {
                deliveryLog.Status = "Delivered";
                deliveryLog.Event.Status = "Processed";
                deliveryLog.Event.ProcessedAt = DateTime.UtcNow;
                deliveryLog.Event.ModifiedDate = DateTime.UtcNow;

                _logger.LogInformation("Event delivered successfully: {EventType} to {SubscriberSystem}",
                    deliveryLog.Event.EventType, deliveryLog.Subscription.SubscriberSystem);
            }
            else
            {
                deliveryLog.Status = "Failed";
                CalculateNextRetry(deliveryLog);

                _logger.LogWarning("Event delivery failed: {EventType} to {SubscriberSystem} (attempt {AttemptNumber})",
                    deliveryLog.Event.EventType, deliveryLog.Subscription.SubscriberSystem, deliveryLog.AttemptNumber);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event dispatch error: {EventType} to {SubscriberSystem}",
                deliveryLog.Event.EventType, deliveryLog.Subscription.SubscriberSystem);

            deliveryLog.Status = "Failed";
            deliveryLog.ErrorMessage = ex.Message.Length > 2000 ? ex.Message.Substring(0, 2000) : ex.Message;
            deliveryLog.ModifiedDate = DateTime.UtcNow;

            CalculateNextRetry(deliveryLog);

            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    public async Task<int> DispatchPendingDeliveriesAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dispatching pending event deliveries (batch size: {BatchSize})", batchSize);

        var pendingDeliveries = await _context.Set<EventDeliveryLog>()
            .Include(d => d.Event)
            .Include(d => d.Subscription)
            .Where(d => !d.IsDeleted && d.Status == "Pending")
            .OrderBy(d => d.AttemptedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} pending deliveries to dispatch", pendingDeliveries.Count);

        int successCount = 0;
        foreach (var delivery in pendingDeliveries)
        {
            bool success = await DispatchEventAsync(delivery.Id, cancellationToken);
            if (success)
            {
                successCount++;
            }
        }

        _logger.LogInformation("Dispatched {SuccessCount}/{TotalCount} event deliveries successfully",
            successCount, pendingDeliveries.Count);

        return successCount;
    }

    public async Task<int> RetryFailedDeliveriesAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying failed event deliveries (max retries: {MaxRetries})", maxRetries);

        var now = DateTime.UtcNow;
        var failedDeliveries = await _context.Set<EventDeliveryLog>()
            .Include(d => d.Event)
            .Include(d => d.Subscription)
            .Where(d => !d.IsDeleted && d.Status == "Failed")
            .Where(d => d.AttemptNumber < maxRetries)
            .Where(d => d.NextRetryAt.HasValue && d.NextRetryAt.Value <= now)
            .OrderBy(d => d.NextRetryAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} failed deliveries ready for retry", failedDeliveries.Count);

        int successCount = 0;
        foreach (var delivery in failedDeliveries)
        {
            bool success = await DispatchEventAsync(delivery.Id, cancellationToken);
            if (success)
            {
                successCount++;
            }
        }

        _logger.LogInformation("Retried {SuccessCount}/{TotalCount} failed deliveries successfully",
            successCount, failedDeliveries.Count);

        return successCount;
    }

    public async Task<int> MoveToDeadLetterQueueAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Moving exhausted event deliveries to dead letter queue (max retries: {MaxRetries})", maxRetries);

        var exhaustedDeliveries = await _context.Set<EventDeliveryLog>()
            .Include(d => d.Event)
            .Include(d => d.Subscription)
            .Where(d => !d.IsDeleted && d.Status == "Failed")
            .Where(d => d.AttemptNumber >= maxRetries)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} exhausted deliveries to move to DLQ", exhaustedDeliveries.Count);

        int movedCount = 0;
        foreach (var delivery in exhaustedDeliveries)
        {
            var deadLetterEntry = new DeadLetterEntry
            {
                Id = Guid.NewGuid(),
                EventId = delivery.EventId,
                EntryType = "Event",
                OriginalPayloadJson = delivery.Event.PayloadJson,
                ErrorMessage = delivery.ErrorMessage ?? "Max retries exceeded",
                FailureCount = delivery.AttemptNumber,
                Status = "Pending",
                FailedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Set<DeadLetterEntry>().Add(deadLetterEntry);

            // Mark delivery as skipped
            delivery.Status = "Skipped";
            delivery.ModifiedDate = DateTime.UtcNow;

            movedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved {Count} deliveries to dead letter queue", movedCount);

        return movedCount;
    }

    // Private helper methods

    private async Task<bool> DispatchWebhookAsync(EventDeliveryLog deliveryLog, object? payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(deliveryLog.Subscription.DeliveryEndpoint))
        {
            _logger.LogWarning("Webhook delivery endpoint not configured for subscription: {SubscriptionCode}",
                deliveryLog.Subscription.SubscriptionCode);
            return false;
        }

        var webhookPayload = new
        {
            EventId = deliveryLog.Event.Id,
            EventType = deliveryLog.Event.EventType,
            ObjectType = deliveryLog.Event.ObjectType,
            ObjectId = deliveryLog.Event.ObjectId,
            Payload = payload,
            OccurredAt = deliveryLog.Event.OccurredAt,
            SchemaVersion = deliveryLog.Event.SchemaVersion
        };

        var result = await _webhookService.DeliverWebhookAsync(
            deliveryLog.Subscription.DeliveryEndpoint,
            webhookPayload,
            null,
            30,
            cancellationToken);

        deliveryLog.HttpStatusCode = result.HttpStatusCode;
        deliveryLog.ResponseBody = result.ResponseBody;
        deliveryLog.ErrorMessage = result.ErrorMessage;
        deliveryLog.LatencyMs = result.LatencyMs;

        return result.Success;
    }

    private async Task<bool> DispatchQueueAsync(EventDeliveryLog deliveryLog, object? payload, CancellationToken cancellationToken)
    {
        // TODO: Implement message queue delivery (Kafka, RabbitMQ, etc.)
        _logger.LogWarning("Queue delivery not yet implemented for subscription: {SubscriptionCode}",
            deliveryLog.Subscription.SubscriptionCode);

        await Task.CompletedTask;
        return false;
    }

    private async Task<bool> DispatchDirectCallAsync(EventDeliveryLog deliveryLog, object? payload, CancellationToken cancellationToken)
    {
        // TODO: Implement direct in-process service call
        _logger.LogWarning("DirectCall delivery not yet implemented for subscription: {SubscriptionCode}",
            deliveryLog.Subscription.SubscriptionCode);

        await Task.CompletedTask;
        return false;
    }

    private void CalculateNextRetry(EventDeliveryLog deliveryLog)
    {
        if (deliveryLog.AttemptNumber >= deliveryLog.Subscription.MaxRetries)
        {
            deliveryLog.NextRetryAt = null;
            return;
        }

        var delayMinutes = deliveryLog.Subscription.RetryPolicy switch
        {
            "Linear" => deliveryLog.AttemptNumber * 5, // 5, 10, 15 minutes
            "Exponential" => (int)Math.Pow(2, deliveryLog.AttemptNumber) * 2, // 4, 8, 16 minutes
            "None" => 0,
            _ => (int)Math.Pow(2, deliveryLog.AttemptNumber) * 2 // Default to exponential
        };

        deliveryLog.NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);

        _logger.LogInformation("Scheduled retry for event delivery in {DelayMinutes} minutes: {EventType}",
            delayMinutes, deliveryLog.Event.EventType);
    }
}
