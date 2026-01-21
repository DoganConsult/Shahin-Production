using GrcMvc.Services.Interfaces;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// Event Dispatcher Background Job - Dispatches pending event deliveries
/// Runs every minute via Hangfire recurring job
/// </summary>
public class EventDispatcherJob
{
    private readonly IEventDispatcherService _eventDispatcher;
    private readonly ILogger<EventDispatcherJob> _logger;

    public EventDispatcherJob(
        IEventDispatcherService eventDispatcher,
        ILogger<EventDispatcherJob> logger)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task ProcessPendingEventsAsync()
    {
        _logger.LogInformation("[EventDispatcherJob] Starting pending event delivery processing...");

        try
        {
            var dispatched = await _eventDispatcher.DispatchPendingDeliveriesAsync(batchSize: 50);
            _logger.LogInformation("[EventDispatcherJob] Dispatched {Count} pending event deliveries", dispatched);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EventDispatcherJob] Error processing pending event deliveries");
            throw;
        }
    }

    public async Task RetryFailedEventsAsync()
    {
        _logger.LogInformation("[EventDispatcherJob] Starting failed event delivery retry...");

        try
        {
            var retried = await _eventDispatcher.RetryFailedDeliveriesAsync(maxRetries: 3);
            _logger.LogInformation("[EventDispatcherJob] Retried {Count} failed event deliveries", retried);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EventDispatcherJob] Error retrying failed event deliveries");
            throw;
        }
    }

    public async Task MoveToDeadLetterQueueAsync()
    {
        _logger.LogInformation("[EventDispatcherJob] Moving exhausted deliveries to dead letter queue...");

        try
        {
            var moved = await _eventDispatcher.MoveToDeadLetterQueueAsync(maxRetries: 3);
            _logger.LogInformation("[EventDispatcherJob] Moved {Count} deliveries to dead letter queue", moved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EventDispatcherJob] Error moving to dead letter queue");
            throw;
        }
    }
}
