using GrcMvc.Data;
using GrcMvc.Services.Analytics;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job to project analytics snapshots for all tenants
    /// Runs periodically via Hangfire to keep ClickHouse data fresh
    /// </summary>
    public class AnalyticsProjectionJob
    {
        private readonly GrcDbContext _context;
        private readonly IDashboardProjector _projector;
        private readonly IClickHouseService _clickHouse;
        private readonly ILogger<AnalyticsProjectionJob> _logger;

        public AnalyticsProjectionJob(
            GrcDbContext context,
            IDashboardProjector projector,
            IClickHouseService clickHouse,
            ILogger<AnalyticsProjectionJob> logger)
        {
            _context = context;
            _projector = projector;
            _clickHouse = clickHouse;
            _logger = logger;
        }

        /// <summary>
        /// Execute analytics projection for all active tenants
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting analytics projection job");

            try
            {
                // Check ClickHouse health first
                var isHealthy = await _clickHouse.IsHealthyAsync();
                if (!isHealthy)
                {
                    _logger.LogWarning("ClickHouse is not healthy, skipping projection");
                    return;
                }

                // Get all active tenants
                var tenants = await _context.Tenants
                    .Where(t => t.IsActive && !t.IsDeleted)
                    .Select(t => t.Id)
                    .ToListAsync();

                _logger.LogInformation("Projecting analytics for {Count} tenants", tenants.Count);

                var successCount = 0;
                var errorCount = 0;

                foreach (var tenantId in tenants)
                {
                    try
                    {
                        await _projector.ProjectAllAsync(tenantId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error projecting analytics for tenant {TenantId}", tenantId);
                        errorCount++;
                    }
                }

                _logger.LogInformation(
                    "Analytics projection completed: {Success} succeeded, {Errors} failed",
                    successCount, errorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics projection job");
            }
        }

        /// <summary>
        /// Project snapshots only (lighter weight, runs more frequently)
        /// </summary>
        public async Task ExecuteSnapshotsOnlyAsync()
        {
            _logger.LogDebug("Starting snapshot-only projection");

            try
            {
                var isHealthy = await _clickHouse.IsHealthyAsync();
                if (!isHealthy) return;

                var tenants = await _context.Tenants
                    .Where(t => t.IsActive && !t.IsDeleted)
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tenantId in tenants)
                {
                    try
                    {
                        await _projector.ProjectSnapshotAsync(tenantId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error projecting snapshot for tenant {TenantId}", tenantId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in snapshot projection job");
            }
        }

        /// <summary>
        /// Project top actions only (runs frequently for real-time feel)
        /// </summary>
        public async Task ExecuteTopActionsAsync()
        {
            try
            {
                var isHealthy = await _clickHouse.IsHealthyAsync();
                if (!isHealthy) return;

                var tenants = await _context.Tenants
                    .Where(t => t.IsActive && !t.IsDeleted)
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tenantId in tenants)
                {
                    try
                    {
                        await _projector.ProjectTopActionsAsync(tenantId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error projecting top actions for tenant {TenantId}", tenantId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in top actions projection job");
            }
        }
    }
}
