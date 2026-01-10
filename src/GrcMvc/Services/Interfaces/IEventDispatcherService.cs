namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Event Dispatcher Service - Delivers events to subscribers
/// </summary>
public interface IEventDispatcherService
{
    /// <summary>
    /// Dispatch a single event delivery to subscriber
    /// </summary>
    Task<bool> DispatchEventAsync(Guid eventDeliveryLogId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatch all pending event deliveries
    /// </summary>
    Task<int> DispatchPendingDeliveriesAsync(int batchSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry failed event deliveries
    /// </summary>
    Task<int> RetryFailedDeliveriesAsync(int maxRetries = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move failed deliveries to dead letter queue
    /// </summary>
    Task<int> MoveToDeadLetterQueueAsync(int maxRetries = 3, CancellationToken cancellationToken = default);
}
