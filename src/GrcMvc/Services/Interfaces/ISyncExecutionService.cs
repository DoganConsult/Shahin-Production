namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Sync Execution Service - Executes data synchronization jobs between external systems and GRC
/// </summary>
public interface ISyncExecutionService
{
    /// <summary>
    /// Execute a sync job manually
    /// </summary>
    Task<Guid> ExecuteSyncJobAsync(Guid syncJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute all scheduled sync jobs that are due
    /// </summary>
    Task ExecuteScheduledSyncsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a running sync job
    /// </summary>
    Task CancelSyncJobAsync(Guid executionLogId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sync execution status
    /// </summary>
    Task<SyncExecutionStatus> GetExecutionStatusAsync(Guid executionLogId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry a failed sync job
    /// </summary>
    Task<Guid> RetrySyncJobAsync(Guid failedExecutionLogId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sync execution status result
/// </summary>
public class SyncExecutionStatus
{
    public Guid ExecutionLogId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsSkipped { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}
