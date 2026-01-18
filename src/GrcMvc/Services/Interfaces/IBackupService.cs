using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for database backup and restore operations
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a backup of the specified database
    /// </summary>
    /// <param name="tenantId">Optional tenant ID for tenant-specific backup</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing backup path and metadata</returns>
    Task<BackupResult> CreateBackupAsync(Guid? tenantId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Restores a database from backup
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <param name="tenantId">Optional tenant ID for tenant-specific restore</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if restore was successful</returns>
    Task<bool> RestoreBackupAsync(string backupPath, Guid? tenantId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Lists all available backups
    /// </summary>
    /// <param name="tenantId">Optional filter by tenant</param>
    /// <param name="ct">Cancellation token</param>
    Task<List<BackupInfo>> ListBackupsAsync(Guid? tenantId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Removes backups older than the retention period
    /// </summary>
    /// <param name="retentionDays">Number of days to retain backups</param>
    /// <param name="ct">Cancellation token</param>
    Task CleanupOldBackupsAsync(int retentionDays = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Tests the backup configuration (connection, permissions, etc.)
    /// </summary>
    Task<(bool Success, string? Error)> TestConfigurationAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of a backup operation
/// </summary>
public record BackupResult(
    bool Success, 
    string? BackupPath, 
    string? Error, 
    long SizeBytes,
    DateTime? CreatedAt = null,
    Guid? TenantId = null);

/// <summary>
/// Information about an existing backup
/// </summary>
public record BackupInfo(
    string Path, 
    DateTime CreatedAt, 
    long SizeBytes, 
    Guid? TenantId,
    string DatabaseName);
