using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for automated database backups.
    /// Backs up all tenant databases and the main platform database.
    /// Runs daily at 2 AM
    /// </summary>
    public class DatabaseBackupJob
    {
        private readonly GrcDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DatabaseBackupJob> _logger;

        public DatabaseBackupJob(
            GrcDbContext context,
            IConfiguration configuration,
            INotificationService notificationService,
            ILogger<DatabaseBackupJob> logger)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 })]
        public async Task BackupAllDatabasesAsync()
        {
            _logger.LogInformation("DatabaseBackupJob started at {Time}", DateTime.UtcNow);

            var backupEnabled = _configuration.GetValue("Backup:Enabled", true);
            if (!backupEnabled)
            {
                _logger.LogInformation("Database backup is disabled in configuration");
                return;
            }

            var backupDir = _configuration["Backup:Directory"] ?? "/app/backups";
            var retentionDays = _configuration.GetValue("Backup:RetentionDays", 30);

            try
            {
                // Ensure backup directory exists
                Directory.CreateDirectory(backupDir);

                var backupResults = new List<BackupResult>();

                // Backup main database
                var mainDbResult = await BackupDatabaseAsync("main", GetMainConnectionString(), backupDir);
                backupResults.Add(mainDbResult);

                // Backup auth database
                var authDbResult = await BackupDatabaseAsync("auth", GetAuthConnectionString(), backupDir);
                backupResults.Add(authDbResult);

                // Backup each tenant database if using database-per-tenant
                var tenantDatabasesEnabled = _configuration.GetValue("MultiTenancy:DatabasePerTenant", false);
                if (tenantDatabasesEnabled)
                {
                    var tenants = await _context.Tenants
                        .AsNoTracking()
                        .Where(t => t.IsActive)
                        .Select(t => new { t.Id, t.TenantSlug })
                        .ToListAsync();

                    foreach (var tenant in tenants)
                    {
                        var tenantDbResult = await BackupDatabaseAsync(
                            $"tenant_{tenant.TenantSlug}",
                            GetTenantConnectionString(tenant.Id),
                            backupDir);
                        backupResults.Add(tenantDbResult);
                    }
                }

                // Cleanup old backups
                await CleanupOldBackupsAsync(backupDir, retentionDays);

                // Log results
                var successful = backupResults.Count(r => r.Success);
                var failed = backupResults.Count(r => !r.Success);

                _logger.LogInformation(
                    "DatabaseBackupJob completed. Successful: {Success}, Failed: {Failed}",
                    successful, failed);

                // Alert on failures
                if (failed > 0)
                {
                    await AlertBackupFailureAsync(backupResults.Where(r => !r.Success).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DatabaseBackupJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<BackupResult> BackupDatabaseAsync(string dbName, string connectionString, string backupDir)
        {
            var result = new BackupResult { DatabaseName = dbName, StartTime = DateTime.UtcNow };

            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFile = Path.Combine(backupDir, $"{dbName}_{timestamp}.sql");

                // Parse connection string for pg_dump
                var connParts = ParseConnectionString(connectionString);

                var pgDumpPath = _configuration["Backup:PgDumpPath"] ?? "pg_dump";

                var startInfo = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"-h {connParts.Host} -p {connParts.Port} -U {connParts.Username} -d {connParts.Database} -F c -f \"{backupFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables["PGPASSWORD"] = connParts.Password;

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Failed to start pg_dump process";
                    return result;
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && File.Exists(backupFile))
                {
                    var fileInfo = new FileInfo(backupFile);
                    result.Success = true;
                    result.BackupFile = backupFile;
                    result.BackupSize = fileInfo.Length;
                    result.EndTime = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Backup successful: {DbName} -> {File} ({Size:N0} bytes)",
                        dbName, backupFile, fileInfo.Length);
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    result.Success = false;
                    result.ErrorMessage = error;
                    result.EndTime = DateTime.UtcNow;

                    _logger.LogError("Backup failed for {DbName}: {Error}", dbName, error);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;

                _logger.LogError(ex, "Exception during backup of {DbName}", dbName);
            }

            return result;
        }

        private async Task CleanupOldBackupsAsync(string backupDir, int retentionDays)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                var files = Directory.GetFiles(backupDir, "*.sql");
                var deleted = 0;

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate)
                    {
                        File.Delete(file);
                        deleted++;
                    }
                }

                if (deleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old backup files", deleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during backup cleanup: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }

        private async Task AlertBackupFailureAsync(List<BackupResult> failures)
        {
            // Notify platform admins
            var admins = await _context.PlatformAdmins
                .Where(a => a.IsActive)
                .Select(a => a.UserId)
                .ToListAsync();

            var failedDbs = string.Join(", ", failures.Select(f => f.DatabaseName));

            foreach (var adminId in admins)
            {
                await _notificationService.SendNotificationAsync(
                    Guid.NewGuid(),
                    adminId.ToString(),
                    "BackupFailure",
                    $"[CRITICAL] Database Backup Failure",
                    $"Database backup failed for: {failedDbs}\n\n" +
                    $"Errors:\n" + string.Join("\n", failures.Select(f => $"- {f.DatabaseName}: {f.ErrorMessage}")),
                    "Critical",
                    Guid.Empty); // Platform-level notification
            }
        }

        private string GetMainConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ??
                   _configuration["ConnectionStrings__DefaultConnection"] ??
                   throw new InvalidOperationException("Main database connection string not configured");
        }

        private string GetAuthConnectionString()
        {
            return _configuration.GetConnectionString("GrcAuthDb") ??
                   _configuration["ConnectionStrings__GrcAuthDb"] ??
                   GetMainConnectionString(); // Fall back to main if not configured
        }

        private string GetTenantConnectionString(Guid tenantId)
        {
            var template = _configuration["MultiTenancy:ConnectionStringTemplate"] ??
                          GetMainConnectionString();
            return template.Replace("{TenantId}", tenantId.ToString());
        }

        private (string Host, string Port, string Database, string Username, string Password) ParseConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';')
                .Select(p => p.Split('='))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());

            return (
                Host: parts.GetValueOrDefault("host") ?? parts.GetValueOrDefault("server") ?? "localhost",
                Port: parts.GetValueOrDefault("port") ?? "5432",
                Database: parts.GetValueOrDefault("database") ?? parts.GetValueOrDefault("db") ?? "postgres",
                Username: parts.GetValueOrDefault("username") ?? parts.GetValueOrDefault("user") ?? parts.GetValueOrDefault("uid") ?? "postgres",
                Password: parts.GetValueOrDefault("password") ?? parts.GetValueOrDefault("pwd") ?? ""
            );
        }

        private class BackupResult
        {
            public string DatabaseName { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string? BackupFile { get; set; }
            public long BackupSize { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}
