using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Usage Tracking Service Interface
    /// Tracks and enforces usage limits for trial and paid subscriptions
    /// </summary>
    public interface IUsageTrackingService
    {
        // ===== USAGE TRACKING =====
        
        /// <summary>Track AI API call usage</summary>
        Task<UsageResult> TrackAiCallAsync(Guid tenantId, string model, int tokensUsed, CancellationToken ct = default);
        
        /// <summary>Track storage usage (file upload)</summary>
        Task<UsageResult> TrackStorageAsync(Guid tenantId, long bytesUsed, CancellationToken ct = default);
        
        /// <summary>Track team member addition</summary>
        Task<UsageResult> TrackTeamMemberAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Track framework activation</summary>
        Task<UsageResult> TrackFrameworkAsync(Guid tenantId, string frameworkCode, CancellationToken ct = default);
        
        /// <summary>Track report generation</summary>
        Task<UsageResult> TrackReportAsync(Guid tenantId, string reportType, CancellationToken ct = default);
        
        // ===== USAGE QUERIES =====
        
        /// <summary>Get current usage summary for a tenant</summary>
        Task<UsageSummaryDto> GetUsageSummaryAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Get usage history for a tenant</summary>
        Task<List<UsageRecordDto>> GetUsageHistoryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
        
        /// <summary>Get daily AI usage for current period</summary>
        Task<int> GetDailyAiCallsAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Get total storage used in bytes</summary>
        Task<long> GetStorageUsedAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Get active team member count</summary>
        Task<int> GetTeamMemberCountAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Get active framework count</summary>
        Task<int> GetFrameworkCountAsync(Guid tenantId, CancellationToken ct = default);
        
        // ===== LIMIT CHECKS =====
        
        /// <summary>Check if AI calls limit is reached</summary>
        Task<LimitCheckResult> CheckAiLimitAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Check if storage limit is reached</summary>
        Task<LimitCheckResult> CheckStorageLimitAsync(Guid tenantId, long additionalBytes = 0, CancellationToken ct = default);
        
        /// <summary>Check if team member limit is reached</summary>
        Task<LimitCheckResult> CheckTeamMemberLimitAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Check if framework limit is reached</summary>
        Task<LimitCheckResult> CheckFrameworkLimitAsync(Guid tenantId, CancellationToken ct = default);
        
        /// <summary>Check if feature is available for tenant's plan</summary>
        Task<bool> IsFeatureAvailableAsync(Guid tenantId, string featureName, CancellationToken ct = default);
        
        // ===== RESET & CLEANUP =====
        
        /// <summary>Reset daily usage counters (called by background job)</summary>
        Task ResetDailyUsageAsync(CancellationToken ct = default);
        
        /// <summary>Reset monthly usage counters (called by background job)</summary>
        Task ResetMonthlyUsageAsync(CancellationToken ct = default);
    }
    
    // ===== DTOs =====
    
    public class UsageResult
    {
        public bool Success { get; set; }
        public bool LimitReached { get; set; }
        public int CurrentUsage { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    public class UsageSummaryDto
    {
        public Guid TenantId { get; set; }
        public string PlanCode { get; set; } = "trial";
        public bool IsTrial { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public int DaysRemaining { get; set; }
        
        // AI Usage
        public int AiCallsToday { get; set; }
        public int AiCallsLimit { get; set; }
        public int AiCallsRemaining => Math.Max(0, AiCallsLimit - AiCallsToday);
        public double AiUsagePercent => AiCallsLimit > 0 ? (double)AiCallsToday / AiCallsLimit * 100 : 0;
        
        // Storage
        public long StorageUsedBytes { get; set; }
        public long StorageLimitBytes { get; set; }
        public long StorageRemainingBytes => Math.Max(0, StorageLimitBytes - StorageUsedBytes);
        public double StorageUsagePercent => StorageLimitBytes > 0 ? (double)StorageUsedBytes / StorageLimitBytes * 100 : 0;
        public string StorageUsedFormatted => FormatBytes(StorageUsedBytes);
        public string StorageLimitFormatted => FormatBytes(StorageLimitBytes);
        
        // Team Members
        public int TeamMemberCount { get; set; }
        public int TeamMemberLimit { get; set; }
        public int TeamMemberRemaining => Math.Max(0, TeamMemberLimit - TeamMemberCount);
        public double TeamMemberUsagePercent => TeamMemberLimit > 0 ? (double)TeamMemberCount / TeamMemberLimit * 100 : 0;
        
        // Frameworks
        public int FrameworkCount { get; set; }
        public int FrameworkLimit { get; set; }
        public int FrameworkRemaining => Math.Max(0, FrameworkLimit - FrameworkCount);
        public double FrameworkUsagePercent => FrameworkLimit > 0 ? (double)FrameworkCount / FrameworkLimit * 100 : 0;
        
        // Features
        public bool HasAdvancedReporting { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasSso { get; set; }
        public bool HasCustomBranding { get; set; }
        
        // Warnings
        public List<UsageWarning> Warnings { get; set; } = new();
        
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
    
    public class UsageWarning
    {
        public string Type { get; set; } = string.Empty; // ai, storage, team, framework, trial
        public string Level { get; set; } = "warning"; // warning, critical
        public string Message { get; set; } = string.Empty;
        public string MessageAr { get; set; } = string.Empty;
        public int UsagePercent { get; set; }
    }
    
    public class UsageRecordDto
    {
        public Guid Id { get; set; }
        public string UsageType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Metadata { get; set; }
        public DateTime RecordedAt { get; set; }
    }
    
    public class LimitCheckResult
    {
        public bool IsAllowed { get; set; }
        public bool IsNearLimit { get; set; } // 80%+
        public bool IsAtLimit { get; set; }
        public int Current { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageAr { get; set; } = string.Empty;
    }
}
