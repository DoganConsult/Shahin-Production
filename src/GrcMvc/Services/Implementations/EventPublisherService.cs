using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Event Publisher Service - Publishes domain events to subscribers
/// Follows ASP.NET Core patterns with dependency injection and async/await
/// </summary>
public class EventPublisherService : IEventPublisherService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<EventPublisherService> _logger;

    public EventPublisherService(
        GrcDbContext context,
        ILogger<EventPublisherService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> PublishEventAsync(
        string eventType,
        string objectType,
        Guid objectId,
        object payload,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(payload);

        _logger.LogInformation("Publishing event: {EventType} for {ObjectType} {ObjectId}",
            eventType, objectType, objectId);

        // Validate event schema (if exists in registry)
        var validation = await ValidateEventAsync(eventType, payload, cancellationToken);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Event validation failed for {EventType}: {Errors}",
                eventType, string.Join(", ", validation.Errors));
            // Continue anyway - validation is optional
        }

        // Create domain event
        var domainEvent = new DomainEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? Guid.Empty,
            CorrelationId = Guid.NewGuid().ToString(),
            EventType = eventType,
            SchemaVersion = validation.SchemaVersion ?? "1.0",
            SourceSystem = "GRC",
            ObjectType = objectType,
            ObjectId = objectId,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = "Pending",
            ProcessingAttempts = 0,
            OccurredAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _context.Set<DomainEvent>().Add(domainEvent);

        // Find matching subscriptions
        var subscriptions = await _context.Set<EventSubscription>()
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => tenantId == null || s.TenantId == null || s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        // Filter by event type pattern (supports wildcards)
        var matchingSubscriptions = subscriptions
            .Where(s => MatchesEventPattern(eventType, s.EventTypePattern))
            .ToList();

        _logger.LogInformation("Found {Count} matching subscriptions for event type: {EventType}",
            matchingSubscriptions.Count, eventType);

        // Create event delivery log entries for each subscription
        foreach (var subscription in matchingSubscriptions)
        {
            var deliveryLog = new EventDeliveryLog
            {
                Id = Guid.NewGuid(),
                EventId = domainEvent.Id,
                SubscriptionId = subscription.Id,
                Status = "Pending",
                AttemptNumber = 0,
                AttemptedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Set<EventDeliveryLog>().Add(deliveryLog);
        }

        // Mark event as published
        domainEvent.Status = "Published";
        domainEvent.PublishedAt = DateTime.UtcNow;
        domainEvent.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Event published successfully: {EventId} with {DeliveryCount} deliveries",
            domainEvent.Id, matchingSubscriptions.Count);

        return domainEvent.Id;
    }

    public async Task<List<Guid>> PublishBatchAsync(
        List<DomainEventData> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        _logger.LogInformation("Publishing batch of {Count} events", events.Count);

        var eventIds = new List<Guid>();

        foreach (var eventData in events)
        {
            var eventId = await PublishEventAsync(
                eventData.EventType,
                eventData.ObjectType,
                eventData.ObjectId,
                eventData.Payload,
                eventData.TenantId,
                cancellationToken);

            eventIds.Add(eventId);
        }

        _logger.LogInformation("Batch published successfully: {Count} events", eventIds.Count);

        return eventIds;
    }

    public async Task<EventValidationResult> ValidateEventAsync(
        string eventType,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var schema = await _context.Set<EventSchemaRegistry>()
            .Where(s => !s.IsDeleted && s.EventType == eventType && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (schema == null)
        {
            // No schema defined - validation passes by default
            return new EventValidationResult
            {
                IsValid = true,
                Errors = new List<string>()
            };
        }

        // TODO: Implement JSON schema validation
        // For now, just check required fields
        var errors = new List<string>();
        var payloadJson = JsonSerializer.Serialize(payload);

        if (!string.IsNullOrEmpty(schema.RequiredFields))
        {
            var requiredFields = schema.RequiredFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var field in requiredFields)
            {
                if (!payloadJson.Contains($"\"{field}\""))
                {
                    errors.Add($"Required field missing: {field}");
                }
            }
        }

        return new EventValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            SchemaVersion = schema.SchemaVersion
        };
    }

    public async Task<int> GetPendingEventsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<DomainEvent>()
            .Where(e => !e.IsDeleted && e.Status == "Pending")
            .CountAsync(cancellationToken);
    }

    public async Task ProcessPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing pending events (batch size: {BatchSize})", batchSize);

        var pendingEvents = await _context.Set<DomainEvent>()
            .Where(e => !e.IsDeleted && e.Status == "Pending")
            .OrderBy(e => e.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} pending events to process", pendingEvents.Count);

        foreach (var domainEvent in pendingEvents)
        {
            try
            {
                // Find matching subscriptions
                var subscriptions = await _context.Set<EventSubscription>()
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Where(s => domainEvent.TenantId == Guid.Empty || s.TenantId == null || s.TenantId == domainEvent.TenantId)
                    .ToListAsync(cancellationToken);

                var matchingSubscriptions = subscriptions
                    .Where(s => MatchesEventPattern(domainEvent.EventType, s.EventTypePattern))
                    .ToList();

                // Create delivery logs
                foreach (var subscription in matchingSubscriptions)
                {
                    var deliveryLog = new EventDeliveryLog
                    {
                        Id = Guid.NewGuid(),
                        EventId = domainEvent.Id,
                        SubscriptionId = subscription.Id,
                        Status = "Pending",
                        AttemptNumber = 0,
                        AttemptedAt = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.Set<EventDeliveryLog>().Add(deliveryLog);
                }

                // Mark event as published
                domainEvent.Status = "Published";
                domainEvent.PublishedAt = DateTime.UtcNow;
                domainEvent.ModifiedDate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventId}", domainEvent.Id);

                domainEvent.Status = "Failed";
                domainEvent.ProcessingAttempts++;
                domainEvent.LastError = ex.Message;
                domainEvent.ModifiedDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Processed {Count} pending events", pendingEvents.Count);
    }

    // Helper method to match event type against wildcard pattern
    private bool MatchesEventPattern(string eventType, string pattern)
    {
        if (pattern == "*")
        {
            return true;
        }

        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(eventType, regexPattern, RegexOptions.IgnoreCase);
    }
}
