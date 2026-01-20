using GrcMvc.Abp;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Dashboard Metrics Service - Provides real-time metrics for dashboards
/// </summary>
public class DashboardMetricsService : IDashboardMetricsService
{
    private readonly GrcDbContext _context;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<DashboardMetricsService> _logger;

    public DashboardMetricsService(
        GrcDbContext context,
        ITenantRepository tenantRepository,
        ILogger<DashboardMetricsService> logger)
    {
        _context = context;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<PlatformDashboardMetrics> GetPlatformMetricsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var metrics = new PlatformDashboardMetrics();

        try
        {
            // Tenant Statistics from ABP
            var tenants = await _tenantRepository.GetListAsync();
            metrics.TotalTenants = tenants.Count;
            metrics.ActiveTenants = tenants.Count(t => t.GetIsActive());
            metrics.PendingTenants = tenants.Count(t => t.GetStatus() == "Pending");
            metrics.TrialTenants = tenants.Count(t => t.GetSubscriptionTier() == "Trial" || t.GetSubscriptionTier() == "MVP");
            metrics.PaidTenants = tenants.Count(t => t.GetSubscriptionTier() != "Trial" && t.GetSubscriptionTier() != "MVP");
            metrics.NewTenantsThisMonth = tenants.Count(t => t.CreationTime >= monthAgo);

            // User Statistics
            metrics.TotalUsers = await _context.Users.CountAsync();
            metrics.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            metrics.PlatformAdmins = await _context.PlatformAdmins.CountAsync(p => !p.IsDeleted && p.Status == "Active");
            metrics.TenantAdmins = await _context.TenantUsers.CountAsync(tu => tu.RoleCode == "Admin" && tu.Status == "Active");
            metrics.NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedDate >= monthAgo);

            // Login Statistics (from audit events)
            metrics.TodayLogins = await _context.Set<AuditEvent>()
                .CountAsync(a => a.EventType == "UserLogin" && a.CreatedDate >= today);
            metrics.WeeklyLogins = await _context.Set<AuditEvent>()
                .CountAsync(a => a.EventType == "UserLogin" && a.CreatedDate >= weekAgo);
            metrics.FailedLogins24h = await _context.Set<AuditEvent>()
                .CountAsync(a => a.EventType == "LoginFailed" && a.CreatedDate >= now.AddHours(-24));

            // Role Statistics
            metrics.TotalRoles = await _context.RoleCatalogs.CountAsync(r => !r.IsDeleted);
            metrics.ActiveRoles = await _context.RoleCatalogs.CountAsync(r => !r.IsDeleted && r.IsActive);

            // Pending Invitations
            metrics.PendingInvitations = await _context.TenantUsers
                .CountAsync(tu => tu.Status == "Pending" && !tu.IsDeleted);

            // Audit Events Today
            metrics.AuditEventsToday = await _context.Set<AuditEvent>()
                .CountAsync(a => a.CreatedDate >= today);

            // Recent Activity
            metrics.RecentActivity = await _context.Set<AuditEvent>()
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    Timestamp = a.CreatedDate,
                    EventType = a.EventType,
                    Description = a.Description ?? a.EventType,
                    UserId = a.Actor,
                    UserName = a.Actor == "SYSTEM" ? "System" : a.Actor
                })
                .ToListAsync();

            // Top Tenants
            metrics.TopTenants = tenants
                .OrderByDescending(t => t.CreationTime)
                .Take(5)
                .Select(t => new TenantSummaryItem
                {
                    TenantId = t.Id,
                    TenantName = t.GetOrganizationName() ?? t.Name,
                    Status = t.GetStatus() ?? "Unknown",
                    CreatedAt = t.CreationTime
                })
                .ToList();

            // Get user counts per tenant
            var tenantUserCounts = await _context.TenantUsers
                .Where(tu => !tu.IsDeleted)
                .GroupBy(tu => tu.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TenantId, x => x.Count);

            foreach (var item in metrics.TopTenants)
            {
                item.UserCount = tenantUserCounts.GetValueOrDefault(item.TenantId, 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform metrics");
        }

        return metrics;
    }

    public async Task<TenantDashboardMetrics> GetTenantMetricsAsync(Guid tenantId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);

        var metrics = new TenantDashboardMetrics { TenantId = tenantId };

        try
        {
            // Get tenant info
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant != null)
            {
                metrics.TenantName = tenant.GetOrganizationName() ?? tenant.Name;
            }

            // User Statistics
            metrics.TotalUsers = await _context.TenantUsers
                .CountAsync(tu => tu.TenantId == tenantId && !tu.IsDeleted);
            metrics.ActiveUsers = await _context.TenantUsers
                .CountAsync(tu => tu.TenantId == tenantId && tu.Status == "Active" && !tu.IsDeleted);
            metrics.PendingInvitations = await _context.TenantUsers
                .CountAsync(tu => tu.TenantId == tenantId && tu.Status == "Pending" && !tu.IsDeleted);

            // Assessment Statistics
            metrics.TotalAssessments = await _context.Set<Assessment>()
                .CountAsync(a => a.TenantId == tenantId && !a.IsDeleted);
            metrics.ActiveAssessments = await _context.Set<Assessment>()
                .CountAsync(a => a.TenantId == tenantId && a.Status == "InProgress" && !a.IsDeleted);
            metrics.CompletedAssessments = await _context.Set<Assessment>()
                .CountAsync(a => a.TenantId == tenantId && a.Status == "Completed" && !a.IsDeleted);

            // Task Statistics
            metrics.OpenTasks = await _context.Set<WorkflowTask>()
                .CountAsync(t => t.TenantId == tenantId && t.Status == "Open" && !t.IsDeleted);
            metrics.OverdueTasks = await _context.Set<WorkflowTask>()
                .CountAsync(t => t.TenantId == tenantId && t.Status == "Open" && t.DueDate < now && !t.IsDeleted);
            metrics.CompletedTasksThisWeek = await _context.Set<WorkflowTask>()
                .CountAsync(t => t.TenantId == tenantId && t.Status == "Completed" && t.CompletedAt >= weekAgo && !t.IsDeleted);

            // Evidence Statistics (count only, no status filter)
            metrics.TotalEvidence = await _context.Set<Evidence>()
                .CountAsync(e => e.TenantId == tenantId && !e.IsDeleted);

            // Recent Activity
            metrics.RecentActivity = await _context.Set<AuditEvent>()
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    Timestamp = a.CreatedDate,
                    EventType = a.EventType,
                    Description = a.Description ?? a.EventType,
                    UserId = a.Actor,
                    UserName = a.Actor == "SYSTEM" ? "System" : a.Actor
                })
                .ToListAsync();

            // Top Contributors
            metrics.TopContributors = await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId && tu.Status == "Active" && !tu.IsDeleted)
                .OrderByDescending(tu => tu.ContributionScore)
                .Take(5)
                .Select(tu => new UserActivityItem
                {
                    UserId = tu.UserId,
                    UserName = $"{tu.FirstName} {tu.LastName}",
                    TasksCompleted = tu.ActionsCompleted,
                    ContributionScore = tu.ContributionScore
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant metrics for {TenantId}", tenantId);
        }

        return metrics;
    }
}
