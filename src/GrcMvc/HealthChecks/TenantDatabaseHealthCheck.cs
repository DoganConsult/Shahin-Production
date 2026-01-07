using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;
using GrcMvc.Services.Interfaces;
using MsHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace GrcMvc.HealthChecks
{
    /// <summary>
    /// Health check for tenant-specific databases
    /// Checks connectivity and migration status for each tenant
    /// </summary>
    public class TenantDatabaseHealthCheck : IHealthCheck
    {
        private readonly ITenantDatabaseResolver _databaseResolver;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<TenantDatabaseHealthCheck> _logger;

        public TenantDatabaseHealthCheck(
            ITenantDatabaseResolver databaseResolver,
            ITenantContextService tenantContext,
            ILogger<TenantDatabaseHealthCheck> logger)
        {
            _databaseResolver = databaseResolver;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<MsHealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tenantId = _tenantContext.GetCurrentTenantId();
                
                if (tenantId == Guid.Empty)
                {
                    return MsHealthCheckResult.Unhealthy(
                        "No tenant context available",
                        data: new Dictionary<string, object>
                        {
                            ["tenantId"] = "empty",
                            ["reason"] = "User not associated with tenant"
                        });
                }

                // Check if database exists
                var dbExists = await _databaseResolver.DatabaseExistsAsync(tenantId, cancellationToken);
                if (!dbExists)
                {
                    return MsHealthCheckResult.Unhealthy(
                        $"Database for tenant {tenantId} does not exist",
                        data: new Dictionary<string, object>
                        {
                            ["tenantId"] = tenantId.ToString(),
                            ["databaseName"] = _databaseResolver.GetDatabaseName(tenantId),
                            ["reason"] = "Database not provisioned"
                        });
                }

                // Check database connectivity
                var connectionString = _databaseResolver.GetConnectionString(tenantId);
                await using var connection = new NpgsqlConnection(connectionString);
                
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    
                    // Check if we can query
                    await using var command = new NpgsqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync(cancellationToken);

                    // Get database size
                    await using var sizeCommand = new NpgsqlCommand(
                        "SELECT pg_size_pretty(pg_database_size(current_database()))", connection);
                    var dbSize = await sizeCommand.ExecuteScalarAsync(cancellationToken) as string;

                    return MsHealthCheckResult.Healthy(
                        $"Tenant database is accessible",
                        data: new Dictionary<string, object>
                        {
                            ["tenantId"] = tenantId.ToString(),
                            ["databaseName"] = _databaseResolver.GetDatabaseName(tenantId),
                            ["databaseSize"] = dbSize ?? "unknown",
                            ["status"] = "healthy"
                        });
                }
                catch (Exception ex)
                {
                    return MsHealthCheckResult.Unhealthy(
                        $"Cannot connect to tenant database: {ex.Message}",
                        ex,
                        data: new Dictionary<string, object>
                        {
                            ["tenantId"] = tenantId.ToString(),
                            ["databaseName"] = _databaseResolver.GetDatabaseName(tenantId),
                            ["error"] = ex.Message
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tenant database health");
                return MsHealthCheckResult.Unhealthy(
                    "Error checking tenant database health",
                    ex);
            }
        }
    }
}
