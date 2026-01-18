using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Implementation of database backup service for PostgreSQL
/// </summary>
public class BackupService : IBackupService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupDirectory;

    public BackupService(IConfiguration config, ILogger<BackupService> logger)
    {
        _config = config;
        _logger = logger;
        _backupDirectory = config["Backup:Directory"] ?? Path.Combine(Path.GetTempPath(), "grc-backups");
        
        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<BackupResult> CreateBackupAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        
        try
        {
            string connectionString;
            string databaseName;
            
            if (tenantId.HasValue)
            {
                // For tenant-specific backup, get tenant connection string
                // In a full implementation, this would use ITenantDatabaseResolver
                var masterConnection = _config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(masterConnection))
                {
                    return new BackupResult(false, null, "Master connection string not configured", 0);
                }
                
                var builder = new NpgsqlConnectionStringBuilder(masterConnection);
                databaseName = $"tenant_{tenantId:N}";
                builder.Database = databaseName;
                connectionString = builder.ConnectionString;
            }
            else
            {
                // Master database backup
                connectionString = _config.GetConnectionString("DefaultConnection") ?? "";
                if (string.IsNullOrEmpty(connectionString))
                {
                    return new BackupResult(false, null, "Connection string not configured", 0);
                }
                
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                databaseName = builder.Database ?? "GrcMvcDb";
            }
            
            var backupFileName = $"{databaseName}_{timestamp}.sql.gz";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);
            
            var builder2 = new NpgsqlConnectionStringBuilder(connectionString);
            
            // Use pg_dump for backup with compression
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetPgDumpPath(),
                    Arguments = $"-h {builder2.Host} -p {builder2.Port} -U {builder2.Username} -d {builder2.Database} -Fc",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            // Set PGPASSWORD environment variable
            process.StartInfo.Environment["PGPASSWORD"] = builder2.Password;
            
            _logger.LogInformation("Starting backup of database {Database} to {Path}", builder2.Database, backupPath);
            
            await using var outputFile = File.Create(backupPath);
            await using var gzip = new GZipStream(outputFile, CompressionLevel.Optimal);
            
            process.Start();
            
            // Copy pg_dump output to compressed file
            await process.StandardOutput.BaseStream.CopyToAsync(gzip, ct);
            await process.WaitForExitAsync(ct);
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                _logger.LogError("pg_dump failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                
                // Clean up failed backup file
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                
                return new BackupResult(false, null, $"pg_dump failed: {error}", 0);
            }
            
            var fileInfo = new FileInfo(backupPath);
            _logger.LogInformation("Backup completed: {Path} ({Size:N0} bytes)", backupPath, fileInfo.Length);
            
            return new BackupResult(
                Success: true,
                BackupPath: backupPath,
                Error: null,
                SizeBytes: fileInfo.Length,
                CreatedAt: DateTime.UtcNow,
                TenantId: tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed for tenant {TenantId}", tenantId);
            return new BackupResult(false, null, ex.Message, 0);
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupPath, Guid? tenantId = null, CancellationToken ct = default)
    {
        if (!File.Exists(backupPath))
        {
            _logger.LogError("Backup file not found: {Path}", backupPath);
            return false;
        }
        
        try
        {
            string connectionString;
            
            if (tenantId.HasValue)
            {
                var masterConnection = _config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(masterConnection))
                {
                    _logger.LogError("Master connection string not configured");
                    return false;
                }
                
                var builder = new NpgsqlConnectionStringBuilder(masterConnection);
                builder.Database = $"tenant_{tenantId:N}";
                connectionString = builder.ConnectionString;
            }
            else
            {
                connectionString = _config.GetConnectionString("DefaultConnection") ?? "";
            }
            
            var builder2 = new NpgsqlConnectionStringBuilder(connectionString);
            
            _logger.LogWarning("Starting restore of database {Database} from {Path}", builder2.Database, backupPath);
            
            // Use pg_restore for restoration
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetPgRestorePath(),
                    Arguments = $"-h {builder2.Host} -p {builder2.Port} -U {builder2.Username} -d {builder2.Database} -c --if-exists",
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.StartInfo.Environment["PGPASSWORD"] = builder2.Password;
            
            process.Start();
            
            // Decompress and pipe to pg_restore
            await using var inputFile = File.OpenRead(backupPath);
            await using var gzip = new GZipStream(inputFile, CompressionMode.Decompress);
            await gzip.CopyToAsync(process.StandardInput.BaseStream, ct);
            process.StandardInput.Close();
            
            await process.WaitForExitAsync(ct);
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                _logger.LogError("pg_restore failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                return false;
            }
            
            _logger.LogInformation("Restore completed for database {Database}", builder2.Database);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed for {Path}", backupPath);
            return false;
        }
    }

    public Task<List<BackupInfo>> ListBackupsAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        var backups = new List<BackupInfo>();
        
        if (!Directory.Exists(_backupDirectory))
        {
            return Task.FromResult(backups);
        }
        
        var files = Directory.GetFiles(_backupDirectory, "*.sql.gz");
        
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)); // Remove .sql.gz
            
            // Parse filename: {database}_{timestamp}
            var parts = fileName.Split('_');
            if (parts.Length < 2) continue;
            
            var dbName = parts[0];
            
            // Check tenant filter
            Guid? fileTenantId = null;
            if (dbName.StartsWith("tenant_"))
            {
                var tenantIdStr = dbName.Replace("tenant_", "");
                if (Guid.TryParse(tenantIdStr, out var tid))
                {
                    fileTenantId = tid;
                }
            }
            
            if (tenantId.HasValue && fileTenantId != tenantId)
            {
                continue;
            }
            
            backups.Add(new BackupInfo(
                Path: file,
                CreatedAt: fileInfo.CreationTimeUtc,
                SizeBytes: fileInfo.Length,
                TenantId: fileTenantId,
                DatabaseName: dbName));
        }
        
        return Task.FromResult(backups.OrderByDescending(b => b.CreatedAt).ToList());
    }

    public async Task CleanupOldBackupsAsync(int retentionDays = 30, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var backups = await ListBackupsAsync(ct: ct);
        
        var oldBackups = backups.Where(b => b.CreatedAt < cutoffDate).ToList();
        
        _logger.LogInformation("Cleaning up {Count} backups older than {Days} days", oldBackups.Count, retentionDays);
        
        foreach (var backup in oldBackups)
        {
            try
            {
                File.Delete(backup.Path);
                _logger.LogInformation("Deleted old backup: {Path}", backup.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete backup: {Path}", backup.Path);
            }
        }
    }

    public Task<(bool Success, string? Error)> TestConfigurationAsync(CancellationToken ct = default)
    {
        try
        {
            // Check backup directory is writable
            var testFile = Path.Combine(_backupDirectory, ".write_test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            
            // Check pg_dump is available
            var pgDumpPath = GetPgDumpPath();
            if (!File.Exists(pgDumpPath) && !IsCommandAvailable("pg_dump"))
            {
                return Task.FromResult<(bool, string?)>((false, "pg_dump not found. Install PostgreSQL client tools."));
            }
            
            // Check connection string
            var connectionString = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                return Task.FromResult<(bool, string?)>((false, "DefaultConnection not configured"));
            }
            
            return Task.FromResult<(bool, string?)>((true, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(bool, string?)>((false, ex.Message));
        }
    }

    private string GetPgDumpPath()
    {
        // Try common locations
        var paths = new[]
        {
            "/usr/bin/pg_dump",
            "/usr/local/bin/pg_dump",
            @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
            "pg_dump" // Rely on PATH
        };
        
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        
        return "pg_dump"; // Hope it's in PATH
    }
    
    private string GetPgRestorePath()
    {
        var pgDumpPath = GetPgDumpPath();
        return pgDumpPath.Replace("pg_dump", "pg_restore");
    }
    
    private bool IsCommandAvailable(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "where" : "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
