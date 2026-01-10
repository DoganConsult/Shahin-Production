using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Sync Execution Service - Executes data synchronization between external systems
/// Follows ASP.NET Core best practices with dependency injection and async patterns
/// </summary>
public class SyncExecutionService : ISyncExecutionService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SyncExecutionService> _logger;
    private readonly IEventPublisherService _eventPublisher;
    private readonly ICredentialEncryptionService _encryption;
    private readonly IHttpClientFactory _httpClientFactory;

    public SyncExecutionService(
        GrcDbContext context,
        ILogger<SyncExecutionService> logger,
        IEventPublisherService eventPublisher,
        ICredentialEncryptionService encryption,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _encryption = encryption;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Guid> ExecuteSyncJobAsync(Guid syncJobId, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        Guid executionLogId = Guid.Empty;

        try
        {
            // Load sync job with connector
            var syncJob = await _context.Set<SyncJob>()
                .Include(s => s.Connector)
                .FirstOrDefaultAsync(s => s.Id == syncJobId && s.IsDeleted == false, cancellationToken);

            if (syncJob == null)
            {
                throw new InvalidOperationException($"SyncJob {syncJobId} not found or deleted");
            }

            if (!syncJob.IsActive)
            {
                throw new InvalidOperationException($"SyncJob {syncJobId} is not active");
            }

            _logger.LogInformation(
                "Starting sync job execution: {JobCode} ({JobId}) - {Direction} sync of {ObjectType}",
                syncJob.JobCode, syncJobId, syncJob.Direction, syncJob.ObjectType);

            // Create execution log
            var executionLog = new SyncExecutionLog
            {
                Id = Guid.NewGuid(),
                SyncJobId = syncJobId,
                Status = "Running",
                StartedAt = startTime,
                RecordsProcessed = 0,
                RecordsCreated = 0,
                RecordsUpdated = 0,
                RecordsFailed = 0,
                RecordsSkipped = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Set<SyncExecutionLog>().Add(executionLog);
            await _context.SaveChangesAsync(cancellationToken);
            executionLogId = executionLog.Id;

            // Execute sync based on direction
            switch (syncJob.Direction)
            {
                case "Inbound":
                    await ExecuteInboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                case "Outbound":
                    await ExecuteOutboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                case "Bidirectional":
                    await ExecuteInboundSyncAsync(syncJob, executionLog, cancellationToken);
                    await ExecuteOutboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown sync direction: {syncJob.Direction}");
            }

            // Mark as completed
            executionLog.Status = "Completed";
            executionLog.CompletedAt = DateTime.UtcNow;
            executionLog.DurationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            executionLog.ModifiedDate = DateTime.UtcNow;

            // Update sync job last run info
            syncJob.LastRunAt = DateTime.UtcNow;
            syncJob.LastRunStatus = "Success";
            syncJob.LastRunRecordCount = executionLog.RecordsProcessed;
            syncJob.NextRunAt = CalculateNextRunTime(syncJob);
            syncJob.ModifiedDate = DateTime.UtcNow;

            // Update connector health
            syncJob.Connector.LastSuccessfulSync = DateTime.UtcNow;
            syncJob.Connector.ErrorCount = 0;
            syncJob.Connector.ConnectionStatus = "Connected";
            syncJob.Connector.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sync job completed successfully: {JobCode} - {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                syncJob.JobCode, executionLog.RecordsProcessed, executionLog.RecordsCreated,
                executionLog.RecordsUpdated, executionLog.RecordsFailed);

            // Publish sync completed event
            await _eventPublisher.PublishEventAsync(
                "SyncJobCompleted",
                "SyncJob",
                syncJobId,
                new
                {
                    JobCode = syncJob.JobCode,
                    Direction = syncJob.Direction,
                    ObjectType = syncJob.ObjectType,
                    RecordsProcessed = executionLog.RecordsProcessed,
                    RecordsCreated = executionLog.RecordsCreated,
                    RecordsUpdated = executionLog.RecordsUpdated,
                    DurationSeconds = executionLog.DurationSeconds
                },
                syncJob.TenantId,
                cancellationToken);

            return executionLogId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync job execution failed: {JobId}", syncJobId);

            if (executionLogId != Guid.Empty)
            {
                // Update execution log with error
                var executionLog = await _context.Set<SyncExecutionLog>()
                    .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

                if (executionLog != null)
                {
                    executionLog.Status = "Failed";
                    executionLog.CompletedAt = DateTime.UtcNow;
                    executionLog.DurationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
                    executionLog.ErrorsJson = JsonSerializer.Serialize(new[] { new { Message = ex.Message, StackTrace = ex.StackTrace } });
                    executionLog.ModifiedDate = DateTime.UtcNow;

                    // Update sync job
                    var syncJob = await _context.Set<SyncJob>()
                        .Include(s => s.Connector)
                        .FirstOrDefaultAsync(s => s.Id == syncJobId, cancellationToken);

                    if (syncJob != null)
                    {
                        syncJob.LastRunAt = DateTime.UtcNow;
                        syncJob.LastRunStatus = "Failed";
                        syncJob.ModifiedDate = DateTime.UtcNow;

                        // Update connector error count
                        syncJob.Connector.ErrorCount++;
                        syncJob.Connector.ConnectionStatus = syncJob.Connector.ErrorCount >= 5 ? "Error" : "Disconnected";
                        syncJob.Connector.ModifiedDate = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            throw;
        }
    }

    public async Task ExecuteScheduledSyncsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for scheduled sync jobs...");

        var dueJobs = await _context.Set<SyncJob>()
            .Include(s => s.Connector)
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => s.NextRunAt.HasValue && s.NextRunAt.Value <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} scheduled sync jobs due for execution", dueJobs.Count);

        foreach (var job in dueJobs)
        {
            try
            {
                await ExecuteSyncJobAsync(job.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute scheduled sync job: {JobCode}", job.JobCode);
                // Continue with next job
            }
        }
    }

    public async Task CancelSyncJobAsync(Guid executionLogId, CancellationToken cancellationToken = default)
    {
        var executionLog = await _context.Set<SyncExecutionLog>()
            .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

        if (executionLog == null)
        {
            throw new InvalidOperationException($"Execution log {executionLogId} not found");
        }

        if (executionLog.Status != "Running")
        {
            throw new InvalidOperationException($"Cannot cancel sync job in status: {executionLog.Status}");
        }

        executionLog.Status = "Cancelled";
        executionLog.CompletedAt = DateTime.UtcNow;
        executionLog.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sync job cancelled: {ExecutionLogId}", executionLogId);
    }

    public async Task<SyncExecutionStatus> GetExecutionStatusAsync(Guid executionLogId, CancellationToken cancellationToken = default)
    {
        var executionLog = await _context.Set<SyncExecutionLog>()
            .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

        if (executionLog == null)
        {
            throw new InvalidOperationException($"Execution log {executionLogId} not found");
        }

        var errors = new List<string>();
        if (!string.IsNullOrEmpty(executionLog.ErrorsJson))
        {
            try
            {
                var errorList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(executionLog.ErrorsJson);
                errors = errorList?.Select(e => e.GetValueOrDefault("Message", "Unknown error")).ToList() ?? new List<string>();
            }
            catch
            {
                errors.Add("Failed to parse error details");
            }
        }

        return new SyncExecutionStatus
        {
            ExecutionLogId = executionLog.Id,
            Status = executionLog.Status,
            RecordsProcessed = executionLog.RecordsProcessed,
            RecordsCreated = executionLog.RecordsCreated,
            RecordsUpdated = executionLog.RecordsUpdated,
            RecordsFailed = executionLog.RecordsFailed,
            RecordsSkipped = executionLog.RecordsSkipped,
            StartedAt = executionLog.StartedAt,
            CompletedAt = executionLog.CompletedAt,
            DurationSeconds = executionLog.DurationSeconds,
            Errors = errors
        };
    }

    public async Task<Guid> RetrySyncJobAsync(Guid failedExecutionLogId, CancellationToken cancellationToken = default)
    {
        var failedLog = await _context.Set<SyncExecutionLog>()
            .Include(e => e.SyncJob)
            .FirstOrDefaultAsync(e => e.Id == failedExecutionLogId, cancellationToken);

        if (failedLog == null)
        {
            throw new InvalidOperationException($"Execution log {failedExecutionLogId} not found");
        }

        if (failedLog.Status != "Failed")
        {
            throw new InvalidOperationException($"Can only retry failed sync jobs. Current status: {failedLog.Status}");
        }

        _logger.LogInformation("Retrying failed sync job: {JobCode}", failedLog.SyncJob.JobCode);

        return await ExecuteSyncJobAsync(failedLog.SyncJobId, cancellationToken);
    }

    // Private helper methods

    private async Task ExecuteInboundSyncAsync(SyncJob syncJob, SyncExecutionLog executionLog, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing inbound sync: {ObjectType} from {TargetSystem}",
            syncJob.ObjectType, syncJob.Connector.TargetSystem);

        // TODO: Implement actual external system data fetching based on ConnectorType
        // For now, this is a placeholder structure showing the pattern

        // 1. Connect to external system
        // 2. Fetch data based on FilterExpression
        // 3. Apply field mappings
        // 4. Upsert to local database
        // 5. Update CrossReferenceMapping
        // 6. Publish domain events

        // Placeholder: Simulate processing
        await Task.Delay(100, cancellationToken); // Simulate API call

        _logger.LogWarning("Inbound sync implementation pending for connector type: {ConnectorType}",
            syncJob.Connector.ConnectorType);
    }

    private async Task ExecuteOutboundSyncAsync(SyncJob syncJob, SyncExecutionLog executionLog, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing outbound sync: {ObjectType} to {TargetSystem}",
            syncJob.ObjectType, syncJob.Connector.TargetSystem);

        // TODO: Implement actual external system data pushing
        // Placeholder structure showing the pattern

        await Task.Delay(100, cancellationToken); // Simulate API call

        _logger.LogWarning("Outbound sync implementation pending for connector type: {ConnectorType}",
            syncJob.Connector.ConnectorType);
    }

    private DateTime? CalculateNextRunTime(SyncJob syncJob)
    {
        if (string.IsNullOrEmpty(syncJob.CronExpression))
        {
            // Default scheduling based on frequency
            return syncJob.Frequency switch
            {
                "RealTime" => DateTime.UtcNow.AddMinutes(5),
                "Hourly" => DateTime.UtcNow.AddHours(1),
                "Daily" => DateTime.UtcNow.AddDays(1),
                "Weekly" => DateTime.UtcNow.AddDays(7),
                _ => DateTime.UtcNow.AddDays(1)
            };
        }

        // TODO: Parse cron expression for more precise scheduling
        // For now, default to daily
        return DateTime.UtcNow.AddDays(1);
    }
}
