using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Usage Tracking Service Implementation
    /// Tracks AI calls, storage, team members, frameworks and enforces limits
    /// </summary>
    public class UsageTrackingService : IUsageTrackingService
    {
        private readonly GrcDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<UsageTrackingService> _logger;

        // Default limits for trial accounts
        private const int TRIAL_AI_CALLS_PER_DAY = 10;
        private const long TRIAL_STORAGE_BYTES = 500 * 1024 * 1024; // 500 MB
        private const int TRIAL_TEAM_MEMBERS = 5;
        private const int TRIAL_FRAMEWORKS = 2;

        public UsageTrackingService(
            GrcDbContext context,
            ISubscriptionService subscriptionService,
            ILogger<UsageTrackingService> logger)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        #region Usage Tracking

        public async Task<UsageResult> TrackAiCallAsync(Guid tenantId, string model, int tokensUsed, CancellationToken ct = default)
        {
            var limitCheck = await CheckAiLimitAsync(tenantId, ct);
            if (!limitCheck.IsAllowed)
            {
                return new UsageResult
                {
                    Success = false,
                    LimitReached = true,
                    CurrentUsage = limitCheck.Current,
                    Limit = limitCheck.Limit,
                    Remaining = 0,
                    ErrorMessage = limitCheck.Message
                };
            }

            // Record usage
            var usage = new TenantUsageRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UsageType = "AI_CALL",
                Quantity = 1,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new { model, tokensUsed }),
                RecordedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.TenantUsageRecords.Add(usage);
            await _context.SaveChangesAsync(ct);

            _logger.LogDebug("AI call tracked for tenant {TenantId}: {Model}, {Tokens} tokens", tenantId, model, tokensUsed);

            return new UsageResult
            {
                Success = true,
                LimitReached = limitCheck.IsAtLimit,
                CurrentUsage = limitCheck.Current + 1,
                Limit = limitCheck.Limit,
                Remaining = limitCheck.Remaining - 1
            };
        }

        public async Task<UsageResult> TrackStorageAsync(Guid tenantId, long bytesUsed, CancellationToken ct = default)
        {
            var limitCheck = await CheckStorageLimitAsync(tenantId, bytesUsed, ct);
            if (!limitCheck.IsAllowed)
            {
                return new UsageResult
                {
                    Success = false,
                    LimitReached = true,
                    CurrentUsage = (int)(limitCheck.Current / 1024 / 1024), // MB
                    Limit = (int)(limitCheck.Limit / 1024 / 1024),
                    ErrorMessage = limitCheck.Message
                };
            }

            // Update tenant storage usage
            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, ct);
            if (tenant != null)
            {
                tenant.StorageUsedBytes = (tenant.StorageUsedBytes ?? 0) + bytesUsed;
                await _context.SaveChangesAsync(ct);
            }

            return new UsageResult
            {
                Success = true,
                CurrentUsage = (int)((limitCheck.Current + bytesUsed) / 1024 / 1024),
                Limit = (int)(limitCheck.Limit / 1024 / 1024),
                Remaining = (int)((limitCheck.Remaining - bytesUsed) / 1024 / 1024)
            };
        }

        public async Task<UsageResult> TrackTeamMemberAsync(Guid tenantId, CancellationToken ct = default)
        {
            var limitCheck = await CheckTeamMemberLimitAsync(tenantId, ct);
            if (!limitCheck.IsAllowed)
            {
                return new UsageResult
                {
                    Success = false,
                    LimitReached = true,
                    CurrentUsage = limitCheck.Current,
                    Limit = limitCheck.Limit,
                    ErrorMessage = limitCheck.Message
                };
            }

            return new UsageResult
            {
                Success = true,
                CurrentUsage = limitCheck.Current,
                Limit = limitCheck.Limit,
                Remaining = limitCheck.Remaining
            };
        }

        public async Task<UsageResult> TrackFrameworkAsync(Guid tenantId, string frameworkCode, CancellationToken ct = default)
        {
            var limitCheck = await CheckFrameworkLimitAsync(tenantId, ct);
            if (!limitCheck.IsAllowed)
            {
                return new UsageResult
                {
                    Success = false,
                    LimitReached = true,
                    CurrentUsage = limitCheck.Current,
                    Limit = limitCheck.Limit,
                    ErrorMessage = limitCheck.Message
                };
            }

            // Record usage
            var usage = new TenantUsageRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UsageType = "FRAMEWORK_ACTIVATION",
                Quantity = 1,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new { frameworkCode }),
                RecordedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.TenantUsageRecords.Add(usage);
            await _context.SaveChangesAsync(ct);

            return new UsageResult
            {
                Success = true,
                CurrentUsage = limitCheck.Current + 1,
                Limit = limitCheck.Limit,
                Remaining = limitCheck.Remaining - 1
            };
        }

        public async Task<UsageResult> TrackReportAsync(Guid tenantId, string reportType, CancellationToken ct = default)
        {
            // Check if advanced reporting is available
            var hasAdvanced = await IsFeatureAvailableAsync(tenantId, "AdvancedReporting", ct);
            if (reportType.StartsWith("Advanced") && !hasAdvanced)
            {
                return new UsageResult
                {
                    Success = false,
                    LimitReached = true,
                    ErrorMessage = "Advanced reporting requires a Professional or Enterprise plan"
                };
            }

            var usage = new TenantUsageRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UsageType = "REPORT_GENERATED",
                Quantity = 1,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new { reportType }),
                RecordedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.TenantUsageRecords.Add(usage);
            await _context.SaveChangesAsync(ct);

            return new UsageResult { Success = true };
        }

        #endregion

        #region Usage Queries

        public async Task<UsageSummaryDto> GetUsageSummaryAsync(Guid tenantId, CancellationToken ct = default)
        {
            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, ct);
            if (tenant == null)
            {
                return new UsageSummaryDto { TenantId = tenantId };
            }

            var limits = await GetTenantLimitsAsync(tenantId, ct);
            var aiCallsToday = await GetDailyAiCallsAsync(tenantId, ct);
            var storageUsed = await GetStorageUsedAsync(tenantId, ct);
            var teamCount = await GetTeamMemberCountAsync(tenantId, ct);
            var frameworkCount = await GetFrameworkCountAsync(tenantId, ct);

            var summary = new UsageSummaryDto
            {
                TenantId = tenantId,
                PlanCode = tenant.IsTrial ? "trial" : (tenant.SubscriptionPlan ?? "starter"),
                IsTrial = tenant.IsTrial,
                TrialEndsAt = tenant.TrialEndsAt,
                DaysRemaining = tenant.TrialEndsAt.HasValue 
                    ? Math.Max(0, (tenant.TrialEndsAt.Value - DateTime.UtcNow).Days) 
                    : 0,

                AiCallsToday = aiCallsToday,
                AiCallsLimit = limits.AiCallsPerDay,

                StorageUsedBytes = storageUsed,
                StorageLimitBytes = limits.StorageBytes,

                TeamMemberCount = teamCount,
                TeamMemberLimit = limits.TeamMembers,

                FrameworkCount = frameworkCount,
                FrameworkLimit = limits.Frameworks,

                HasAdvancedReporting = limits.HasAdvancedReporting,
                HasApiAccess = limits.HasApiAccess,
                HasPrioritySupport = limits.HasPrioritySupport,
                HasSso = limits.HasSso,
                HasCustomBranding = limits.HasCustomBranding
            };

            // Add warnings
            if (summary.IsTrial && summary.DaysRemaining <= 2)
            {
                summary.Warnings.Add(new UsageWarning
                {
                    Type = "trial",
                    Level = summary.DaysRemaining == 0 ? "critical" : "warning",
                    Message = summary.DaysRemaining == 0 
                        ? "Your trial has expired. Upgrade to continue." 
                        : $"Your trial expires in {summary.DaysRemaining} day(s).",
                    MessageAr = summary.DaysRemaining == 0 
                        ? "انتهت الفترة التجريبية. قم بالترقية للمتابعة." 
                        : $"تنتهي الفترة التجريبية خلال {summary.DaysRemaining} يوم/أيام."
                });
            }

            if (summary.AiUsagePercent >= 80)
            {
                summary.Warnings.Add(new UsageWarning
                {
                    Type = "ai",
                    Level = summary.AiUsagePercent >= 100 ? "critical" : "warning",
                    Message = $"AI usage at {summary.AiUsagePercent:0}% ({summary.AiCallsRemaining} calls remaining today)",
                    MessageAr = $"استخدام الذكاء الاصطناعي عند {summary.AiUsagePercent:0}% ({summary.AiCallsRemaining} مكالمات متبقية اليوم)",
                    UsagePercent = (int)summary.AiUsagePercent
                });
            }

            if (summary.StorageUsagePercent >= 80)
            {
                summary.Warnings.Add(new UsageWarning
                {
                    Type = "storage",
                    Level = summary.StorageUsagePercent >= 100 ? "critical" : "warning",
                    Message = $"Storage at {summary.StorageUsagePercent:0}% ({summary.StorageUsedFormatted} / {summary.StorageLimitFormatted})",
                    MessageAr = $"التخزين عند {summary.StorageUsagePercent:0}%",
                    UsagePercent = (int)summary.StorageUsagePercent
                });
            }

            return summary;
        }

        public async Task<List<UsageRecordDto>> GetUsageHistoryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
        {
            return await _context.TenantUsageRecords
                .Where(r => r.TenantId == tenantId && r.RecordedAt >= from && r.RecordedAt <= to)
                .OrderByDescending(r => r.RecordedAt)
                .Take(1000)
                .Select(r => new UsageRecordDto
                {
                    Id = r.Id,
                    UsageType = r.UsageType,
                    Quantity = r.Quantity,
                    Metadata = r.Metadata,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync(ct);
        }

        public async Task<int> GetDailyAiCallsAsync(Guid tenantId, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.TenantUsageRecords
                .Where(r => r.TenantId == tenantId && 
                            r.UsageType == "AI_CALL" && 
                            r.RecordedAt >= today)
                .SumAsync(r => r.Quantity, ct);
        }

        public async Task<long> GetStorageUsedAsync(Guid tenantId, CancellationToken ct = default)
        {
            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, ct);
            return tenant?.StorageUsedBytes ?? 0;
        }

        public async Task<int> GetTeamMemberCountAsync(Guid tenantId, CancellationToken ct = default)
        {
            return await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId && !tu.IsDeleted && tu.IsActive)
                .CountAsync(ct);
        }

        public async Task<int> GetFrameworkCountAsync(Guid tenantId, CancellationToken ct = default)
        {
            return await _context.TenantFrameworkSelections
                .Where(fs => fs.TenantId == tenantId && fs.IsActive)
                .CountAsync(ct);
        }

        #endregion

        #region Limit Checks

        public async Task<LimitCheckResult> CheckAiLimitAsync(Guid tenantId, CancellationToken ct = default)
        {
            var limits = await GetTenantLimitsAsync(tenantId, ct);
            var current = await GetDailyAiCallsAsync(tenantId, ct);

            var remaining = limits.AiCallsPerDay - current;
            var isAtLimit = remaining <= 0;
            var isNearLimit = (double)current / limits.AiCallsPerDay >= 0.8;

            return new LimitCheckResult
            {
                IsAllowed = !isAtLimit,
                IsNearLimit = isNearLimit,
                IsAtLimit = isAtLimit,
                Current = current,
                Limit = limits.AiCallsPerDay,
                Remaining = Math.Max(0, remaining),
                Message = isAtLimit 
                    ? "Daily AI call limit reached. Upgrade your plan for more calls." 
                    : "",
                MessageAr = isAtLimit 
                    ? "تم الوصول إلى الحد اليومي لمكالمات الذكاء الاصطناعي. قم بترقية خطتك للمزيد." 
                    : ""
            };
        }

        public async Task<LimitCheckResult> CheckStorageLimitAsync(Guid tenantId, long additionalBytes = 0, CancellationToken ct = default)
        {
            var limits = await GetTenantLimitsAsync(tenantId, ct);
            var current = await GetStorageUsedAsync(tenantId, ct);
            var projected = current + additionalBytes;

            var remaining = limits.StorageBytes - current;
            var isAtLimit = projected > limits.StorageBytes;
            var isNearLimit = (double)current / limits.StorageBytes >= 0.8;

            return new LimitCheckResult
            {
                IsAllowed = !isAtLimit,
                IsNearLimit = isNearLimit,
                IsAtLimit = isAtLimit,
                Current = (int)(current / 1024 / 1024),
                Limit = (int)(limits.StorageBytes / 1024 / 1024),
                Remaining = (int)(Math.Max(0, remaining) / 1024 / 1024),
                Message = isAtLimit 
                    ? "Storage limit reached. Upgrade your plan for more storage." 
                    : "",
                MessageAr = isAtLimit 
                    ? "تم الوصول إلى حد التخزين. قم بترقية خطتك للمزيد." 
                    : ""
            };
        }

        public async Task<LimitCheckResult> CheckTeamMemberLimitAsync(Guid tenantId, CancellationToken ct = default)
        {
            var limits = await GetTenantLimitsAsync(tenantId, ct);
            var current = await GetTeamMemberCountAsync(tenantId, ct);

            var remaining = limits.TeamMembers - current;
            var isAtLimit = remaining <= 0;

            return new LimitCheckResult
            {
                IsAllowed = !isAtLimit,
                IsNearLimit = false,
                IsAtLimit = isAtLimit,
                Current = current,
                Limit = limits.TeamMembers,
                Remaining = Math.Max(0, remaining),
                Message = isAtLimit 
                    ? "Team member limit reached. Upgrade your plan to add more users." 
                    : "",
                MessageAr = isAtLimit 
                    ? "تم الوصول إلى حد أعضاء الفريق. قم بترقية خطتك لإضافة المزيد." 
                    : ""
            };
        }

        public async Task<LimitCheckResult> CheckFrameworkLimitAsync(Guid tenantId, CancellationToken ct = default)
        {
            var limits = await GetTenantLimitsAsync(tenantId, ct);
            var current = await GetFrameworkCountAsync(tenantId, ct);

            var remaining = limits.Frameworks - current;
            var isAtLimit = remaining <= 0;

            return new LimitCheckResult
            {
                IsAllowed = !isAtLimit,
                IsNearLimit = false,
                IsAtLimit = isAtLimit,
                Current = current,
                Limit = limits.Frameworks,
                Remaining = Math.Max(0, remaining),
                Message = isAtLimit 
                    ? "Framework limit reached. Upgrade your plan for unlimited frameworks." 
                    : "",
                MessageAr = isAtLimit 
                    ? "تم الوصول إلى حد الأطر. قم بترقية خطتك للأطر غير المحدودة." 
                    : ""
            };
        }

        public async Task<bool> IsFeatureAvailableAsync(Guid tenantId, string featureName, CancellationToken ct = default)
        {
            var limits = await GetTenantLimitsAsync(tenantId, ct);
            
            return featureName switch
            {
                "AdvancedReporting" => limits.HasAdvancedReporting,
                "ApiAccess" => limits.HasApiAccess,
                "PrioritySupport" => limits.HasPrioritySupport,
                "SSO" => limits.HasSso,
                "CustomBranding" => limits.HasCustomBranding,
                _ => true // Unknown features are allowed by default
            };
        }

        #endregion

        #region Reset & Cleanup

        public async Task ResetDailyUsageAsync(CancellationToken ct = default)
        {
            // AI calls reset daily - delete records older than today
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var oldRecords = await _context.TenantUsageRecords
                .Where(r => r.UsageType == "AI_CALL" && r.RecordedAt < yesterday)
                .ToListAsync(ct);

            _context.TenantUsageRecords.RemoveRange(oldRecords);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Reset daily usage: removed {Count} old AI call records", oldRecords.Count);
        }

        public async Task ResetMonthlyUsageAsync(CancellationToken ct = default)
        {
            // Monthly cleanup of old usage records (keep last 90 days)
            var cutoff = DateTime.UtcNow.AddDays(-90);
            var oldRecords = await _context.TenantUsageRecords
                .Where(r => r.RecordedAt < cutoff)
                .ToListAsync(ct);

            _context.TenantUsageRecords.RemoveRange(oldRecords);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Monthly cleanup: removed {Count} old usage records", oldRecords.Count);
        }

        #endregion

        #region Private Methods

        private async Task<TenantLimits> GetTenantLimitsAsync(Guid tenantId, CancellationToken ct)
        {
            var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, ct);
            if (tenant == null)
            {
                return GetTrialLimits();
            }

            if (tenant.IsTrial)
            {
                return GetTrialLimits();
            }

            // Get limits from subscription plan
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == "Active", ct);

            if (subscription?.Plan == null)
            {
                return GetTrialLimits();
            }

            return new TenantLimits
            {
                AiCallsPerDay = subscription.Plan.MaxAiCallsPerDay ?? 50,
                StorageBytes = (subscription.Plan.MaxStorageMB ?? 5120) * 1024 * 1024,
                TeamMembers = subscription.Plan.MaxUsers ?? 10,
                Frameworks = subscription.Plan.MaxFrameworks ?? int.MaxValue,
                HasAdvancedReporting = subscription.Plan.HasAdvancedReporting,
                HasApiAccess = subscription.Plan.HasApiAccess,
                HasPrioritySupport = subscription.Plan.HasPrioritySupport,
                HasSso = subscription.Plan.HasSSO ?? false,
                HasCustomBranding = subscription.Plan.HasCustomBranding ?? false
            };
        }

        private static TenantLimits GetTrialLimits()
        {
            return new TenantLimits
            {
                AiCallsPerDay = TRIAL_AI_CALLS_PER_DAY,
                StorageBytes = TRIAL_STORAGE_BYTES,
                TeamMembers = TRIAL_TEAM_MEMBERS,
                Frameworks = TRIAL_FRAMEWORKS,
                HasAdvancedReporting = false,
                HasApiAccess = false,
                HasPrioritySupport = false,
                HasSso = false,
                HasCustomBranding = false
            };
        }

        private class TenantLimits
        {
            public int AiCallsPerDay { get; set; }
            public long StorageBytes { get; set; }
            public int TeamMembers { get; set; }
            public int Frameworks { get; set; }
            public bool HasAdvancedReporting { get; set; }
            public bool HasApiAccess { get; set; }
            public bool HasPrioritySupport { get; set; }
            public bool HasSso { get; set; }
            public bool HasCustomBranding { get; set; }
        }

        #endregion
    }
}
