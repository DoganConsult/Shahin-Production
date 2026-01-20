namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for retrieving real-time dashboard metrics
/// </summary>
public interface IDashboardMetricsService
{
    /// <summary>
    /// Get Platform Admin dashboard metrics
    /// </summary>
    Task<PlatformDashboardMetrics> GetPlatformMetricsAsync();

    /// <summary>
    /// Get Tenant Admin dashboard metrics for a specific tenant
    /// </summary>
    Task<TenantDashboardMetrics> GetTenantMetricsAsync(Guid tenantId);
}

public class PlatformDashboardMetrics
{
    // Tenant Statistics
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int PendingTenants { get; set; }
    public int TrialTenants { get; set; }
    public int PaidTenants { get; set; }
    
    // User Statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PlatformAdmins { get; set; }
    public int TenantAdmins { get; set; }
    
    // Activity Statistics
    public int TodayLogins { get; set; }
    public int WeeklyLogins { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewTenantsThisMonth { get; set; }
    
    // Role Statistics
    public int TotalRoles { get; set; }
    public int ActiveRoles { get; set; }
    
    // System Health
    public int PendingInvitations { get; set; }
    public int FailedLogins24h { get; set; }
    public int AuditEventsToday { get; set; }
    
    // Recent Activity
    public List<RecentActivityItem> RecentActivity { get; set; } = new();
    public List<TenantSummaryItem> TopTenants { get; set; } = new();
}

public class TenantDashboardMetrics
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    
    // User Statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingInvitations { get; set; }
    
    // Compliance Statistics
    public int TotalControls { get; set; }
    public int CompletedControls { get; set; }
    public int PendingControls { get; set; }
    public double ComplianceScore { get; set; }
    
    // Assessment Statistics
    public int TotalAssessments { get; set; }
    public int ActiveAssessments { get; set; }
    public int CompletedAssessments { get; set; }
    
    // Task Statistics
    public int OpenTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CompletedTasksThisWeek { get; set; }
    
    // Evidence Statistics
    public int TotalEvidence { get; set; }
    public int PendingReview { get; set; }
    public int ApprovedEvidence { get; set; }
    
    // Activity
    public List<RecentActivityItem> RecentActivity { get; set; } = new();
    public List<UserActivityItem> TopContributors { get; set; } = new();
}

public class RecentActivityItem
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = "";
    public string Description { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
}

public class TenantSummaryItem
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public int UserCount { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class UserActivityItem
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public int TasksCompleted { get; set; }
    public int EvidenceSubmitted { get; set; }
    public double ContributionScore { get; set; }
}
