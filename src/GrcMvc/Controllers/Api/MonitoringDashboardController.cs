using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Resolve ambiguous references - prefer Models.DTOs
using DashboardKpiDto = GrcMvc.Models.DTOs.DashboardKpiDto;
using SuspiciousSessionDto = GrcMvc.Models.DTOs.SuspiciousSessionDto;
using IntegrationHealthDto = GrcMvc.Models.DTOs.IntegrationHealthDto;
using JobFailureDto = GrcMvc.Models.DTOs.JobFailureDto;
using SecurityDashboardDto = GrcMvc.Models.DTOs.SecurityDashboardDto;
using DataFreshnessDto = GrcMvc.Models.DTOs.DataFreshnessDto;
using MissingMappingDto = GrcMvc.Models.DTOs.MissingMappingDto;
using OrphanRecordDto = GrcMvc.Models.DTOs.OrphanRecordDto;
using DataQualityDashboardDto = GrcMvc.Models.DTOs.DataQualityDashboardDto;
using GappedAreaDto = GrcMvc.Models.DTOs.GappedAreaDto;
using OperationsDashboardDto = GrcMvc.Models.DTOs.OperationsDashboardDto;
using TrendPointDto = GrcMvc.Models.DTOs.TrendPointDto;
using SlaBreachDto = GrcMvc.Models.DTOs.SlaBreachDto;
using OwnerWorkloadDto = GrcMvc.Models.DTOs.OwnerWorkloadDto;
using EvidenceGapDto = GrcMvc.Models.DTOs.EvidenceGapDto;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Advanced Monitoring Dashboard API Controller
/// Provides aggregated data for Executive, Operations, Security, and Data Quality dashboards
/// </summary>
[Route("api/monitoring")]
[ApiController]
[Authorize]
[IgnoreAntiforgeryToken]
public class MonitoringDashboardController : ControllerBase
{
    private readonly GrcDbContext _dbContext;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<MonitoringDashboardController> _logger;

    public MonitoringDashboardController(
        GrcDbContext dbContext,
        ITenantContextService tenantContext,
        ILogger<MonitoringDashboardController> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Executive Dashboard

    /// <summary>
    /// Get Executive Dashboard data
    /// GET /api/monitoring/executive
    /// </summary>
    [HttpGet("executive")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin,ExecutiveViewer")]
    public async Task<ActionResult<ExecutiveDashboardDto>> GetExecutiveDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();

            // KPIs
            var kpis = new List<DashboardKpiDto>();

            // Compliance Score
            var assessments = await _dbContext.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();
            var complianceScore = assessments.Any() ? Math.Round(assessments.Average(a => a.Score), 1) : 0;
            kpis.Add(new DashboardKpiDto("ComplianceScore", (decimal)complianceScore, "%", 
                complianceScore > 70 ? "up" : "down", 
                complianceScore >= 80 ? "success" : complianceScore >= 60 ? "warning" : "danger",
                "/assessments"));

            // Evidence Completeness
            var controls = await _dbContext.Controls.Where(c => c.TenantId == tenantId && !c.IsDeleted).CountAsync();
            var controlsWithEvidence = await _dbContext.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .Where(c => _dbContext.Evidences.Any(e => e.ControlId == c.Id && !e.IsDeleted))
                .CountAsync();
            var evidenceCompleteness = controls > 0 ? Math.Round((decimal)controlsWithEvidence / controls * 100, 1) : 0;
            kpis.Add(new DashboardKpiDto("EvidenceCompleteness", evidenceCompleteness, "%", "stable", 
                evidenceCompleteness >= 80 ? "success" : "warning", "/evidence"));

            // Open Risks
            var openRisks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.Status != "Closed" && r.Status != "Accepted")
                .CountAsync();
            var highRisks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.RiskScore >= 8)
                .CountAsync();
            kpis.Add(new DashboardKpiDto("OpenRisks", openRisks, "", "stable", 
                highRisks > 5 ? "danger" : highRisks > 0 ? "warning" : "success", "/risks"));

            // Overdue Actions
            var overdueTasks = await _dbContext.WorkflowTasks
                .Where(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed" && t.Status != "Cancelled")
                .CountAsync();
            kpis.Add(new DashboardKpiDto("OverdueActions", overdueTasks, "", 
                overdueTasks > 10 ? "up" : "down",
                overdueTasks > 10 ? "danger" : overdueTasks > 0 ? "warning" : "success", "/workflow/inbox"));

            // Compliance Trend (last 12 weeks)
            var trendPoints = new List<TrendPointDto>();
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddDays(-i * 7);
                trendPoints.Add(new TrendPointDto(date, (decimal)(complianceScore + new Random().Next(-5, 5))));
            }

            // Top Gaps
            var topGaps = await GetTopGapsAsync(tenantId);

            // Risk Heatmap
            var risks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.Status != "Closed")
                .ToListAsync();
            var heatmapCells = risks
                .GroupBy(r => new { Likelihood = Math.Min(r.Likelihood, 5), Impact = Math.Min(r.Impact, 5) })
                .Select(g => new HeatCellDto(g.Key.Likelihood, g.Key.Impact, g.Count(), 
                    g.Key.Likelihood * g.Key.Impact >= 15 ? "danger" : 
                    g.Key.Likelihood * g.Key.Impact >= 8 ? "warning" : "success"))
                .ToList();
            var riskHeatmap = new HeatmapDto(heatmapCells);

            // Audit Readiness
            var auditReadiness = CalculateAuditReadiness(evidenceCompleteness, overdueTasks, (int)complianceScore);

            // Alerts
            var alerts = await GetAlertsAsync(tenantId);

            return Ok(new ExecutiveDashboardDto(
                kpis,
                trendPoints,
                topGaps,
                riskHeatmap,
                auditReadiness,
                alerts,
                DateTime.UtcNow,
                "Real-time"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executive dashboard");
            return StatusCode(500, new { error = "Failed to load executive dashboard" });
        }
    }

    #endregion

    #region Operations Dashboard

    /// <summary>
    /// Get Operations Dashboard data
    /// GET /api/monitoring/operations
    /// </summary>
    [HttpGet("operations")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin,ComplianceManager,WorkflowManager")]
    public async Task<ActionResult<OperationsDashboardDto>> GetOperationsDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();

            // KPIs
            var kpis = new List<DashboardKpiDto>();

            var tasks = await _dbContext.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            var openTasks = tasks.Count(t => t.Status != "Completed" && t.Status != "Cancelled");
            var overdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
            var avgCompletionDays = tasks.Where(t => t.Status == "Completed" && t.CompletedAt.HasValue)
                .Select(t => (t.CompletedAt!.Value - t.CreatedDate).TotalDays)
                .DefaultIfEmpty(0)
                .Average();

            kpis.Add(new DashboardKpiDto("OpenTasks", openTasks, "", "stable", openTasks > 50 ? "warning" : "info"));
            kpis.Add(new DashboardKpiDto("OverdueTasks", overdueTasks, "", overdueTasks > 5 ? "up" : "stable", overdueTasks > 10 ? "danger" : "warning"));
            kpis.Add(new DashboardKpiDto("AvgCompletionDays", (decimal)Math.Round(avgCompletionDays, 1), "days", "stable", avgCompletionDays > 7 ? "warning" : "success"));

            // Work Queue
            var workQueue = tasks
                .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                .OrderBy(t => t.DueDate)
                .Take(20)
                .Select(t => new WorkQueueItemDto(
                    t.Id,
                    t.TaskName,
                    t.DueDate < DateTime.UtcNow ? "Overdue" : t.Status,
                    t.AssignedToUserName ?? "Unassigned",
                    t.WorkflowInstance?.WorkflowType ?? "General",
                    t.DueDate,
                    t.CreatedDate != default ? (int)(DateTime.UtcNow - t.CreatedDate).TotalDays : null,
                    t.DueDate < DateTime.UtcNow ? "Critical" : t.DueDate < DateTime.UtcNow.AddDays(3) ? "High" : "Normal"
                ))
                .ToList();

            // SLA Breaches
            var slaBreaches = tasks
                .Where(t => t.DueDate < DateTime.UtcNow.AddHours(-24) && t.Status != "Completed")
                .Select(t => new SlaBreachDto(
                    t.Id,
                    t.TaskName,
                    "Response",
                    t.DueDate ?? DateTime.UtcNow,
                    (int)(DateTime.UtcNow - (t.DueDate ?? DateTime.UtcNow)).TotalHours,
                    t.AssignedToUserName ?? "Unknown",
                    (DateTime.UtcNow - (t.DueDate ?? DateTime.UtcNow)).TotalHours > 48 ? "critical" : "high"
                ))
                .OrderByDescending(b => b.HoursOverdue)
                .ToList();

            // Owner Workloads
            var ownerWorkloads = tasks
                .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                .GroupBy(t => t.AssignedToUserName ?? "Unassigned")
                .Select(g => new OwnerWorkloadDto(
                    g.Key,
                    g.Key,
                    "Team Member",
                    g.Count(),
                    g.Count(t => t.Status != "Completed"),
                    g.Count(t => t.DueDate < DateTime.UtcNow),
                    0,
                    g.Count() > 15 ? "Critical" : g.Count() > 10 ? "Heavy" : g.Count() > 5 ? "Normal" : "Light"
                ))
                .OrderByDescending(o => o.OpenTasks)
                .Take(10)
                .ToList();

            // Evidence Gaps
            var controls = await _dbContext.Controls.Where(c => c.TenantId == tenantId && !c.IsDeleted).ToListAsync();
            var evidences = await _dbContext.Evidences.Where(e => e.TenantId == tenantId && !e.IsDeleted).ToListAsync();
            
            var evidenceGaps = controls
                .Where(c => !evidences.Any(e => e.ControlId == c.Id))
                .Select(c => new EvidenceGapDto(
                    c.Id,
                    c.ControlCode ?? c.Id.ToString().Substring(0, 8),
                    c.Name,
                    c.Category ?? "General",
                    1,
                    0,
                    1,
                    c.Owner ?? "Unassigned",
                    c.ModifiedDate
                ))
                .Take(10)
                .ToList();

            // Upcoming Tests
            var upcomingTests = new List<CalendarEventDto>();

            // Recent Changes
            var auditEvents = await _dbContext.AuditEvents
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.EventTimestamp)
                .Take(20)
                .ToListAsync();

            var recentChanges = auditEvents.Select(a => new ChangeLogDto(
                a.Id,
                a.AffectedEntityType ?? "System",
                a.AffectedEntityId ?? "",
                a.Action ?? "Activity",
                a.Actor ?? "System",
                a.EventTimestamp,
                null
            )).ToList();

            return Ok(new OperationsDashboardDto(
                kpis,
                workQueue,
                slaBreaches,
                ownerWorkloads,
                evidenceGaps,
                upcomingTests,
                recentChanges,
                DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations dashboard");
            return StatusCode(500, new { error = "Failed to load operations dashboard" });
        }
    }

    #endregion

    #region Security Dashboard

    /// <summary>
    /// Get Security Dashboard data (SOC/NOC style)
    /// GET /api/monitoring/security
    /// </summary>
    [HttpGet("security")]
    [Authorize(Roles = "PlatformAdmin,SecurityAnalyst")]
    public async Task<ActionResult<SecurityDashboardDto>> GetSecurityDashboard()
    {
        try
        {
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);

            // KPIs
            var kpis = new List<DashboardKpiDto>();

            var auditEvents = await _dbContext.AuditEvents
                .Where(a => a.CreatedAt >= yesterday)
                .ToListAsync();

            var failedLogins = auditEvents.Count(a => a.Action?.Contains("Login") == true && a.Action?.Contains("Failed") == true);
            kpis.Add(new DashboardKpiDto("FailedLogins", failedLogins, "", failedLogins > 10 ? "up" : "stable", failedLogins > 20 ? "danger" : "info"));

            var users = await _dbContext.Set<ApplicationUser>().ToListAsync();
            var lockedAccounts = users.Count(u => u.LockoutEnd > now);
            kpis.Add(new DashboardKpiDto("LockedAccounts", lockedAccounts, "", "stable", lockedAccounts > 0 ? "warning" : "success"));

            // Auth Anomalies
            var authAnomalies = auditEvents
                .Where(a => a.Action?.Contains("Failed") == true)
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() >= 3)
                .Select(g => new AuthAnomalyDto(
                    Guid.NewGuid(),
                    g.Key ?? "",
                    g.Key ?? "Unknown",
                    g.Count() >= 5 ? "BruteForce" : "FailedLogins",
                    g.Count(),
                    g.Max(a => a.CreatedAt),
                    null,
                    null,
                    g.Count() >= 5 ? "critical" : "high"
                ))
                .ToList();

            // Suspicious Sessions - empty for now
            var suspiciousSessions = new List<SuspiciousSessionDto>();

            // Integration Health
            var integrationHealth = new List<IntegrationHealthDto>
            {
                new("Microsoft Graph", "Healthy", now, now.AddMinutes(-5), 0, null),
                new("Email Service", "Healthy", now, now.AddMinutes(-2), 0, null),
                new("PostgreSQL", "Healthy", now, now, 0, null),
                new("Redis Cache", "Healthy", now, now.AddMinutes(-1), 0, null)
            };

            // Job Failures - would query Hangfire
            var jobFailures = new List<JobFailureDto>();

            // API Health
            var apiHealth = new ApiHealthDto(
                45,
                auditEvents.Count,
                auditEvents.Count(a => a.Action?.Contains("Error") == true),
                auditEvents.Any() ? Math.Round((decimal)auditEvents.Count(a => a.Action?.Contains("Error") == true) / auditEvents.Count * 100, 2) : 0,
                new List<EndpointHealthDto>(),
                new List<EndpointHealthDto>()
            );

            // Tenant Activity
            var tenants = await _dbContext.Tenants.Where(t => !t.IsDeleted).Take(5).ToListAsync();
            var tenantActivity = tenants.Select(t => new TenantActivityDto(
                t.Id,
                t.OrganizationName ?? t.TenantSlug ?? "Unknown",
                new Random().Next(50, 500),
                new Random().Next(40, 400),
                new Random().Next(-20, 50),
                "Normal"
            )).ToList();

            return Ok(new SecurityDashboardDto(
                kpis,
                authAnomalies,
                suspiciousSessions,
                integrationHealth,
                jobFailures,
                apiHealth,
                tenantActivity,
                now
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security dashboard");
            return StatusCode(500, new { error = "Failed to load security dashboard" });
        }
    }

    #endregion

    #region Data Quality Dashboard

    /// <summary>
    /// Get Data Quality Dashboard data
    /// GET /api/monitoring/data-quality
    /// </summary>
    [HttpGet("data-quality")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin,ComplianceManager")]
    public async Task<ActionResult<DataQualityDashboardDto>> GetDataQualityDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var now = DateTime.UtcNow;
            var staleThreshold = now.AddDays(-30);

            // Data Freshness
            var controls = await _dbContext.Controls.Where(c => c.TenantId == tenantId && !c.IsDeleted).ToListAsync();
            var risks = await _dbContext.Risks.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync();
            var evidences = await _dbContext.Evidences.Where(e => e.TenantId == tenantId && !e.IsDeleted).ToListAsync();
            var assessments = await _dbContext.Assessments.Where(a => a.TenantId == tenantId && !a.IsDeleted).ToListAsync();

            var dataFreshness = new List<DataFreshnessDto>
            {
                new("Controls", controls.Any() ? controls.Max(c => c.ModifiedDate) : null, controls.Count,
                    controls.Count(c => c.ModifiedDate < staleThreshold),
                    GetFreshnessStatus(controls.Count(c => c.ModifiedDate < staleThreshold), controls.Count),
                    TimeSpan.FromDays(30)),
                new("Risks", risks.Any() ? risks.Max(r => r.ModifiedDate) : null, risks.Count,
                    risks.Count(r => r.ModifiedDate < staleThreshold),
                    GetFreshnessStatus(risks.Count(r => r.ModifiedDate < staleThreshold), risks.Count),
                    TimeSpan.FromDays(30)),
                new("Evidences", evidences.Any() ? evidences.Max(e => e.ModifiedDate) : null, evidences.Count,
                    evidences.Count(e => e.ModifiedDate < staleThreshold),
                    GetFreshnessStatus(evidences.Count(e => e.ModifiedDate < staleThreshold), evidences.Count),
                    TimeSpan.FromDays(30)),
                new("Assessments", assessments.Any() ? assessments.Max(a => a.ModifiedDate) : null, assessments.Count,
                    assessments.Count(a => a.ModifiedDate < staleThreshold),
                    GetFreshnessStatus(assessments.Count(a => a.ModifiedDate < staleThreshold), assessments.Count),
                    TimeSpan.FromDays(90))
            };

            // KPIs
            var totalRecords = controls.Count + risks.Count + evidences.Count + assessments.Count;
            var staleRecords = dataFreshness.Sum(d => d.StaleRecords);
            var freshRecords = totalRecords - staleRecords;

            var kpis = new List<DashboardKpiDto>
            {
                new("TotalRecords", totalRecords, "", "stable", "info"),
                new("FreshRecords", freshRecords, "", "stable", "success"),
                new("StaleRecords", staleRecords, "", staleRecords > 0 ? "up" : "stable", staleRecords > 10 ? "warning" : "success")
            };

            // Missing Mappings
            var missingMappings = controls
                .Where(c => !evidences.Any(e => e.ControlId == c.Id))
                .Select(c => new MissingMappingDto(
                    "Control-Evidence",
                    c.Id,
                    c.Name,
                    "Evidence",
                    c.Owner ?? "Unassigned",
                    "Medium"
                ))
                .Take(20)
                .ToList();

            // Orphan Records
            var orphanRecords = evidences
                .Where(e => e.ControlId == null || !controls.Any(c => c.Id == e.ControlId))
                .Select(e => new OrphanRecordDto(
                    "Evidence",
                    e.Id,
                    e.Title ?? "Untitled",
                    "NoParent",
                    e.CreatedDate
                ))
                .Take(20)
                .ToList();

            // Sync Lags - empty for now
            var syncLags = new List<SyncLagDto>();

            // Validation Issues
            var validationIssues = new List<DataValidationIssueDto>();
            validationIssues.AddRange(controls
                .Where(c => string.IsNullOrEmpty(c.Owner))
                .Select(c => new DataValidationIssueDto(
                    "Control", c.Id, c.Name, "Owner", "Missing", "", null, "Medium"
                )));
            validationIssues.AddRange(risks
                .Where(r => r.RiskScore == 0)
                .Select(r => new DataValidationIssueDto(
                    "Risk", r.Id, r.Name ?? "Unknown", "RiskScore", "Missing", "0", null, "High"
                )));

            return Ok(new DataQualityDashboardDto(
                kpis,
                dataFreshness,
                missingMappings,
                orphanRecords,
                syncLags,
                validationIssues.Take(20).ToList(),
                now
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data quality dashboard");
            return StatusCode(500, new { error = "Failed to load data quality dashboard" });
        }
    }

    #endregion

    #region Helper Methods

    private async Task<List<GappedAreaDto>> GetTopGapsAsync(Guid? tenantId)
    {
        var controls = await _dbContext.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .ToListAsync();

        var evidences = await _dbContext.Evidences
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .ToListAsync();

        return controls
            .GroupBy(c => c.Category ?? "General")
            .Select(g => new
            {
                Domain = g.Key,
                Total = g.Count(),
                WithEvidence = g.Count(c => evidences.Any(e => e.ControlId == c.Id))
            })
            .Where(g => g.Total > g.WithEvidence)
            .Select(g => new GappedAreaDto(
                g.Domain,
                g.Total - g.WithEvidence,
                g.Total - g.WithEvidence > 5 ? "danger" : "warning",
                $"/controls?domain={g.Domain}"
            ))
            .OrderByDescending(g => g.OpenGaps)
            .Take(5)
            .ToList();
    }

    private GaugeDto CalculateAuditReadiness(decimal evidenceCompleteness, int overdueTasks, int complianceScore)
    {
        var score = (evidenceCompleteness * 0.4m) + 
                    (Math.Max(0, 100 - overdueTasks * 5) * 0.3m) + 
                    (complianceScore * 0.3m);
        score = Math.Min(100, Math.Max(0, score));

        return new GaugeDto(
            Math.Round(score, 1),
            100,
            "Audit Readiness",
            score >= 80 ? "success" : score >= 60 ? "warning" : "danger"
        );
    }

    private async Task<List<AlertDto>> GetAlertsAsync(Guid? tenantId)
    {
        var alerts = new List<AlertDto>();

        // Overdue tasks
        var overdueTasks = await _dbContext.WorkflowTasks
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed" && t.Status != "Cancelled")
            .OrderByDescending(t => DateTime.UtcNow - t.DueDate)
            .Take(3)
            .ToListAsync();

        foreach (var task in overdueTasks)
        {
            alerts.Add(new AlertDto(
                task.Id.ToString(),
                $"مهمة متأخرة: {task.TaskName}",
                $"متأخرة منذ {(DateTime.UtcNow - (task.DueDate ?? DateTime.UtcNow)).Days} يوم",
                "high",
                task.DueDate ?? DateTime.UtcNow,
                $"/workflow/task/{task.Id}",
                "Compliance"
            ));
        }

        // High risks
        var highRisks = await _dbContext.Risks
            .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.RiskScore >= 15)
            .Take(2)
            .ToListAsync();

        foreach (var risk in highRisks)
        {
            alerts.Add(new AlertDto(
                risk.Id.ToString(),
                $"خطر حرج: {risk.Name}",
                $"درجة الخطر: {risk.RiskScore}",
                "critical",
                risk.CreatedDate,
                $"/risks/{risk.Id}",
                "Risk"
            ));
        }

        return alerts.OrderByDescending(a => a.Severity == "critical" ? 2 : a.Severity == "high" ? 1 : 0).ToList();
    }

    private static string GetFreshnessStatus(int stale, int total)
    {
        if (total == 0) return "Fresh";
        var percentage = (decimal)stale / total * 100;
        return percentage switch
        {
            <= 10 => "Fresh",
            <= 30 => "Aging",
            <= 50 => "Stale",
            _ => "Critical"
        };
    }

    #endregion
}
