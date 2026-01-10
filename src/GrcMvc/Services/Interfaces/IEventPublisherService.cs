namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Event Publisher Service - Publishes domain events to subscribers
/// </summary>
public interface IEventPublisherService
{
    /// <summary>
    /// Publish a domain event
    /// </summary>
    Task<Guid> PublishEventAsync(
        string eventType,
        string objectType,
        Guid objectId,
        object payload,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish multiple events in batch
    /// </summary>
    Task<List<Guid>> PublishBatchAsync(
        List<DomainEventData> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate event against schema registry
    /// </summary>
    Task<EventValidationResult> ValidateEventAsync(
        string eventType,
        object payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending events count
    /// </summary>
    Task<int> GetPendingEventsCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Process pending events (dispatch to subscribers)
    /// </summary>
    Task ProcessPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
}

/// <summary>
/// Domain event data for batch publishing
/// </summary>
public class DomainEventData
{
    public required string EventType { get; set; }
    public required string ObjectType { get; set; }
    public required Guid ObjectId { get; set; }
    public required object Payload { get; set; }
    public Guid? TenantId { get; set; }
    public string? TriggeredBy { get; set; }
}

/// <summary>
/// Event validation result
/// </summary>
public class EventValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? SchemaVersion { get; set; }
}
