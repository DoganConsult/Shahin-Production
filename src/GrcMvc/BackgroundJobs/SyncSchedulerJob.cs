using GrcMvc.Services.Interfaces;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// Sync Scheduler Background Job - Executes scheduled sync jobs
/// Runs every 5 minutes via Hangfire recurring job
/// </summary>
public class SyncSchedulerJob
{
    private readonly ISyncExecutionService _syncExecutionService;
    private readonly ILogger<SyncSchedulerJob> _logger;

    public SyncSchedulerJob(
        ISyncExecutionService syncExecutionService,
        ILogger<SyncSchedulerJob> logger)
    {
        _syncExecutionService = syncExecutionService;
        _logger = logger;
    }

    public async Task ProcessScheduledSyncsAsync()
    {
        _logger.LogInformation("[SyncSchedulerJob] Starting scheduled sync job processing...");

        try
        {
            await _syncExecutionService.ExecuteScheduledSyncsAsync();
            _logger.LogInformation("[SyncSchedulerJob] Scheduled sync job processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SyncSchedulerJob] Error processing scheduled sync jobs");
            throw;
        }
    }
}
