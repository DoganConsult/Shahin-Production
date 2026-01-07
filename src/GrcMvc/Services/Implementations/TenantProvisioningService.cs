using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Provisions tenant databases and initial data
    /// </summary>
    public class TenantProvisioningService : ITenantProvisioningService
    {
        private readonly ITenantDatabaseResolver _databaseResolver;
        private readonly ILogger<TenantProvisioningService> _logger;

        public TenantProvisioningService(
            ITenantDatabaseResolver databaseResolver,
            ILogger<TenantProvisioningService> logger)
        {
            _databaseResolver = databaseResolver;
            _logger = logger;
        }

        public async Task<bool> ProvisionTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting provisioning for tenant {TenantId}", tenantId);

                // Step 1: Create database
                var dbCreated = await _databaseResolver.CreateTenantDatabaseAsync(tenantId, cancellationToken);
                if (!dbCreated)
                {
                    _logger.LogError("Failed to create database for tenant {TenantId}", tenantId);
                    return false;
                }

                // Step 2: Migrations are handled by CreateTenantDatabaseAsync
                // Step 3: Seed initial data (if needed)
                // This can be extended to seed tenant-specific data

                _logger.LogInformation("Successfully provisioned tenant {TenantId}", tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to provision tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> IsTenantProvisionedAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _databaseResolver.DatabaseExistsAsync(tenantId, cancellationToken);
                if (!exists)
                    return false;

                // Verify database has been migrated (check for key tables)
                var connectionString = _databaseResolver.GetConnectionString(tenantId);
                var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                await using var context = new GrcDbContext(optionsBuilder.Options);
                
                // Check if key tables exist
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                    return false;

                // Check if migrations have been applied
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                return !pendingMigrations.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if tenant {TenantId} is provisioned", tenantId);
                return false;
            }
        }
    }
}
