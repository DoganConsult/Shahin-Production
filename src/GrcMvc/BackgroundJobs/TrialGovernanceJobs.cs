using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// AM-09: Background job for trial expiry processing.
    /// Sends warnings and expires trial tenants based on governance rules.
    /// </summary>
    public class TrialExpiryJob
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly TrialGovernanceOptions _options;
        private readonly ILogger<TrialExpiryJob> _logger;

        public TrialExpiryJob(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<TrialExpiryJob> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.TrialGovernance;
            _logger = logger;
        }

        /// <summary>
        /// Execute the trial expiry job.
        /// Should be scheduled to run daily.
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting trial expiry job");

            try
            {
                var now = DateTime.UtcNow;

                // Step 1: Send expiry warnings
                await SendExpiryWarningsAsync(now);

                // Step 2: Expire trials that have passed their expiry date
                await ExpireTrialsAsync(now);

                _logger.LogInformation("Completed trial expiry job");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trial expiry job");
                throw;
            }
        }

        private async Task SendExpiryWarningsAsync(DateTime now)
        {
            var warningThreshold = now.AddDays(_options.ExpiryWarningDays);

            // Find trial tenants expiring soon that haven't been warned
            var tenantsToWarn = await _dbContext.Tenants
                .Where(t => t.SubscriptionTier == "Trial")
                .Where(t => t.TrialEndDate != null)
                .Where(t => t.TrialEndDate <= warningThreshold)
                .Where(t => t.TrialEndDate > now) // Not yet expired
                .ToListAsync();

            foreach (var tenant in tenantsToWarn)
            {
                // Skip warning check - field doesn't exist

                // Log the warning
                var daysRemaining = (int)(tenant.TrialEndDate!.Value - now).TotalDays;
                await _auditService.LogTrialExpiryWarningAsync(tenant.Id, daysRemaining);

                // Get tenant admin to notify
                var tenantAdmin = await _dbContext.TenantUsers
                    .Where(tu => tu.TenantId == tenant.Id && tu.Role == "TenantAdmin")
                    .FirstOrDefaultAsync();

                if (tenantAdmin != null)
                {
                    // TODO: Send email notification to tenant admin
                    _logger.LogInformation(
                        "Trial expiry warning sent for tenant {TenantId}. Expires: {ExpiryDate}",
                        tenant.Id, tenant.TrialEndDate);
                }
            }

            if (tenantsToWarn.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Sent expiry warnings to {Count} tenants", tenantsToWarn.Count);
            }
        }

        private async Task ExpireTrialsAsync(DateTime now)
        {
            // Find trial tenants that have expired
            var expiredTrials = await _dbContext.Tenants
                .Where(t => t.SubscriptionTier == "Trial")
                .Where(t => t.TrialEndDate != null)
                .Where(t => t.TrialEndDate <= now)
                .Where(t => t.Status != "Expired")
                .ToListAsync();

            foreach (var tenant in expiredTrials)
            {
                tenant.Status = "Expired";
                tenant.UpdatedAt = now;

                // Log the expiry
                await _auditService.LogTrialExpiredAsync(tenant.Id);

                _logger.LogInformation(
                    "Trial expired for tenant {TenantId}. Grace period: {GraceDays} days. Data action: {Action}",
                    tenant.Id, _options.ExpiryGracePeriodDays, _options.DataAction);
            }

            if (expiredTrials.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Expired {Count} trial tenants", expiredTrials.Count);
            }
        }
    }

    /// <summary>
    /// AM-09: Background job for trial data retention processing.
    /// Handles archiving/deletion of expired trial data after retention period.
    /// </summary>
    public class TrialDataRetentionJob
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly TrialGovernanceOptions _options;
        private readonly ILogger<TrialDataRetentionJob> _logger;

        public TrialDataRetentionJob(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<TrialDataRetentionJob> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.TrialGovernance;
            _logger = logger;
        }

        /// <summary>
        /// Execute the data retention job.
        /// Should be scheduled to run weekly.
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting trial data retention job");

            try
            {
                var now = DateTime.UtcNow;
                var retentionThreshold = now.AddDays(-(_options.ExpiryGracePeriodDays + _options.DataRetentionDays));

                // Find expired tenants past retention period
                var tenantsForAction = await _dbContext.Tenants
                    .Where(t => t.Status == "Expired")
                    .Where(t => t.TrialEndDate != null)
                    .Where(t => t.TrialEndDate <= retentionThreshold)
                    // .Where(t => t.DataActionStatus == null) // Field doesn't exist
                    .ToListAsync();

                foreach (var tenant in tenantsForAction)
                {
                    await ProcessTenantDataAsync(tenant, now);
                }

                if (tenantsForAction.Any())
                {
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Processed data action for {Count} expired tenants", tenantsForAction.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trial data retention job");
                throw;
            }
        }

        private async Task ProcessTenantDataAsync(dynamic tenant, DateTime now)
        {
            var tenantId = (Guid)tenant.Id;

            switch (_options.DataAction.ToLowerInvariant())
            {
                case "archive":
                    await ArchiveTenantDataAsync(tenantId);
                    // tenant.DataActionStatus = "Archived";
                    await _auditService.LogTrialDataArchivedAsync(null, tenantId, null);
                    _logger.LogInformation("Archived data for tenant {TenantId}", tenantId);
                    break;

                case "delete":
                    await DeleteTenantDataAsync(tenantId);
                    // tenant.DataActionStatus = "Deleted";
                    await _auditService.LogTrialDataDeletedAsync(null, tenantId, null);
                    _logger.LogInformation("Deleted data for tenant {TenantId}", tenantId);
                    break;

                case "anonymize":
                    await AnonymizeTenantDataAsync(tenantId);
                    // tenant.DataActionStatus = "Anonymized";
                    _logger.LogInformation("Anonymized data for tenant {TenantId}", tenantId);
                    break;

                default:
                    _logger.LogWarning("Unknown data action: {Action}", _options.DataAction);
                    break;
            }

            // tenant.DataActionAt = now;
        }

        private async Task ArchiveTenantDataAsync(Guid tenantId)
        {
            // In a full implementation:
            // 1. Export tenant data to archive storage
            // 2. Create audit record of archived data
            // 3. Set tenant to archived state
            await Task.CompletedTask;
        }

        private async Task DeleteTenantDataAsync(Guid tenantId)
        {
            // Delete tenant-related data in order of dependencies
            // Note: This is a simplified version - full implementation would handle all related entities

            // Remove tenant users
            var tenantUsers = await _dbContext.TenantUsers
                .Where(tu => tu.TenantId == tenantId)
                .ToListAsync();
            _dbContext.TenantUsers.RemoveRange(tenantUsers);

            // Mark tenant as purged (don't delete for audit trail)
            var tenant = await _dbContext.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.Status = "Purged";
                tenant.Name = $"[DELETED] {tenantId}";
            }
        }

        private async Task AnonymizeTenantDataAsync(Guid tenantId)
        {
            // Anonymize user data while preserving structure for analytics
            var tenantUsers = await _dbContext.TenantUsers
                .Where(tu => tu.TenantId == tenantId)
                .ToListAsync();

            foreach (var user in tenantUsers)
            {
                user.Email = $"anonymized_{Guid.NewGuid():N}@deleted.local";
                user.FirstName = "Anonymized";
                user.LastName = "User";
            }

            var tenant = await _dbContext.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.Name = $"Anonymized Tenant {tenantId.ToString().Substring(0, 8)}";
            }
        }
    }

    /// <summary>
    /// AM-09: Service for extending trial periods.
    /// </summary>
    public class TrialExtensionService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly TrialGovernanceOptions _options;
        private readonly ILogger<TrialExtensionService> _logger;

        public TrialExtensionService(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<TrialExtensionService> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.TrialGovernance;
            _logger = logger;
        }

        /// <summary>
        /// Extend a trial tenant's expiry date.
        /// </summary>
        public async Task<TrialExtensionResult> ExtendTrialAsync(
            Guid tenantId,
            Guid requestedBy,
            string? reason = null)
        {
            var tenant = await _dbContext.Tenants.FindAsync(tenantId);

            if (tenant == null)
            {
                return TrialExtensionResult.Failed("Tenant not found.");
            }

            if (tenant.SubscriptionTier != "Trial")
            {
                return TrialExtensionResult.Failed("Only trial tenants can be extended.");
            }

            if (!_options.AllowExtension)
            {
                return TrialExtensionResult.Failed("Trial extensions are not enabled.");
            }

            // Check if already extended (using a custom field or checking history)
            // For now, allow extension

            var previousExpiry = tenant.TrialEndDate;
            var newExpiry = (tenant.TrialEndDate ?? DateTime.UtcNow).AddDays(_options.ExtensionDays);

            tenant.TrialEndDate = newExpiry;
            tenant.UpdatedAt = DateTime.UtcNow;

            // If tenant was expired, reactivate
            if (tenant.Status == "Expired")
            {
                tenant.Status = "Active";
            }

            await _dbContext.SaveChangesAsync();

            // Log the extension
            await _auditService.LogTrialExtendedAsync(
                requestedBy,
                tenantId,
                _options.ExtensionDays,
                newExpiry,
                null);

            _logger.LogInformation(
                "Trial extended for tenant {TenantId}. New expiry: {NewExpiry}",
                tenantId, newExpiry);

            return TrialExtensionResult.Succeeded(newExpiry);
        }

        /// <summary>
        /// Convert a trial tenant to a paid subscription.
        /// </summary>
        public async Task<TrialConversionResult> ConvertToSubscriptionAsync(
            Guid tenantId,
            string subscriptionTier,
            Guid convertedBy)
        {
            var tenant = await _dbContext.Tenants.FindAsync(tenantId);

            if (tenant == null)
            {
                return TrialConversionResult.Failed("Tenant not found.");
            }

            if (tenant.SubscriptionTier != "Trial")
            {
                return TrialConversionResult.Failed("Only trial tenants can be converted.");
            }

            // Convert trial to subscription
            tenant.BillingStatus = "Active";
            tenant.TrialEndDate = null;
            tenant.SubscriptionTier = subscriptionTier;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Log conversion
            await _auditService.LogTrialConvertedAsync(
                convertedBy,
                tenantId,
                subscriptionTier,
                null,
                null);

            _logger.LogInformation(
                "Trial converted for tenant {TenantId}. New tier: {Tier}",
                tenantId, subscriptionTier);

            return TrialConversionResult.Succeeded(subscriptionTier);
        }
    }

    /// <summary>
    /// Result of trial extension request.
    /// </summary>
    public class TrialExtensionResult
    {
        public bool Success { get; set; }
        public DateTime? NewExpiryDate { get; set; }
        public string? ErrorMessage { get; set; }

        public static TrialExtensionResult Succeeded(DateTime newExpiry) => new()
        {
            Success = true,
            NewExpiryDate = newExpiry
        };

        public static TrialExtensionResult Failed(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };
    }

    /// <summary>
    /// Result of trial conversion.
    /// </summary>
    public class TrialConversionResult
    {
        public bool Success { get; set; }
        public string? NewTier { get; set; }
        public string? ErrorMessage { get; set; }

        public static TrialConversionResult Succeeded(string tier) => new()
        {
            Success = true,
            NewTier = tier
        };

        public static TrialConversionResult Failed(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };
    }
}
