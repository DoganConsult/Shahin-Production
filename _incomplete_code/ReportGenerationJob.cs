using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for automated report generation - Stage 5: Excellence.
    /// Generates daily, weekly, monthly, and quarterly reports.
    /// Runs daily at 5 AM
    /// </summary>
    public class ReportGenerationJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReportGenerationJob> _logger;

        public ReportGenerationJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<ReportGenerationJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task GenerateScheduledReportsAsync()
        {
            _logger.LogInformation("ReportGenerationJob started at {Time}", DateTime.UtcNow);

            try
            {
                var tenants = await _context.Tenants.AsNoTracking()
                    .Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED")
                    .Select(t => t.Id).ToListAsync();

                var totalReports = 0;
                foreach (var tenantId in tenants)
                {
                    var generated = await ProcessTenantReportsAsync(tenantId);
                    totalReports += generated;
                }

                _logger.LogInformation("ReportGenerationJob completed. Generated {Count} reports", totalReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportGenerationJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<int> ProcessTenantReportsAsync(Guid tenantId)
        {
            var generatedCount = 0;
            var today = DateTime.UtcNow.Date;
            var dayOfWeek = today.DayOfWeek;
            var dayOfMonth = today.Day;
            var isQuarterStart = dayOfMonth == 1 && today.Month is 1 or 4 or 7 or 10;

            // Daily summary
            var dailySummary = await GenerateDailySummaryAsync(tenantId, today);
            if (dailySummary != null) generatedCount++;

            // Weekly report (Monday)
            if (dayOfWeek == DayOfWeek.Monday)
            {
                var weeklyReport = await GenerateWeeklyReportAsync(tenantId, today);
                if (weeklyReport != null) generatedCount++;
            }

            // Monthly report (1st of month)
            if (dayOfMonth == 1)
            {
                var monthlyReport = await GenerateMonthlyReportAsync(tenantId, today);
                if (monthlyReport != null) generatedCount++;
            }

            // Quarterly report (1st of quarter)
            if (isQuarterStart)
            {
                var quarterlyReport = await GenerateQuarterlyReportAsync(tenantId, today);
                if (quarterlyReport != null) generatedCount++;
            }

            await _context.SaveChangesAsync();
            return generatedCount;
        }

        private async Task<GeneratedReport?> GenerateDailySummaryAsync(Guid tenantId, DateTime date)
        {
            try
            {
                var metrics = new
                {
                    Date = date,
                    TotalControls = await _context.TenantControlSets.CountAsync(c => c.TenantId == tenantId && c.IsActive),
                    CompliantControls = await _context.TenantControlSets.CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ComplianceStatus == "COMPLIANT"),
                    OpenRisks = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.IsActive && r.Status != "Closed"),
                    CriticalRisks = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.IsActive && r.ResidualRiskLevel == "Critical"),
                    PendingTasks = await _context.WorkflowTasks.CountAsync(t => t.TenantId == tenantId && t.Status == "Pending"),
                    OverdueTasks = await _context.WorkflowTasks.CountAsync(t => t.TenantId == tenantId && t.Status != "Completed" && t.DueDate < date),
                    EvidenceExpiring = await _context.EvidencePacks.CountAsync(e => e.TenantId == tenantId && e.ValidUntil >= date && e.ValidUntil <= date.AddDays(30))
                };

                var report = new GeneratedReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportType = "DailyComplianceSummary",
                    Title = $"Daily Compliance Summary - {date:yyyy-MM-dd}",
                    GeneratedAt = DateTime.UtcNow,
                    PeriodStart = date,
                    PeriodEnd = date,
                    Status = "Generated",
                    Format = "JSON",
                    DataJson = JsonSerializer.Serialize(metrics),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:ReportGenerationJob",
                    IsActive = true
                };

                _context.GeneratedReports.Add(report);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily summary for tenant {TenantId}", tenantId);
                return null;
            }
        }

        private async Task<GeneratedReport?> GenerateWeeklyReportAsync(Guid tenantId, DateTime date)
        {
            try
            {
                var weekStart = date.AddDays(-7);
                var metrics = new
                {
                    WeekStart = weekStart,
                    WeekEnd = date,
                    TasksCompleted = await _context.WorkflowTasks.CountAsync(t => t.TenantId == tenantId && t.CompletedAt >= weekStart && t.CompletedAt <= date),
                    AssessmentsCompleted = await _context.Assessments.CountAsync(a => a.TenantId == tenantId && a.CompletedDate >= weekStart && a.CompletedDate <= date),
                    NewRisks = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.CreatedDate >= weekStart && r.CreatedDate <= date),
                    RisksMitigated = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.Status == "Closed" && r.ModifiedDate >= weekStart),
                    Escalations = await _context.WorkflowEscalations.CountAsync(e => e.TenantId == tenantId && e.EscalatedAt >= weekStart && e.EscalatedAt <= date)
                };

                var report = new GeneratedReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportType = "WeeklyGRCSummary",
                    Title = $"Weekly GRC Summary - Week of {weekStart:yyyy-MM-dd}",
                    GeneratedAt = DateTime.UtcNow,
                    PeriodStart = weekStart,
                    PeriodEnd = date,
                    Status = "Generated",
                    Format = "JSON",
                    DataJson = JsonSerializer.Serialize(metrics),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:ReportGenerationJob",
                    IsActive = true
                };

                _context.GeneratedReports.Add(report);
                await NotifyStakeholdersAsync(tenantId, report);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly report for tenant {TenantId}", tenantId);
                return null;
            }
        }

        private async Task<GeneratedReport?> GenerateMonthlyReportAsync(Guid tenantId, DateTime date)
        {
            try
            {
                var monthStart = date.AddMonths(-1);
                var complianceScores = await _context.TenantFrameworkSelections
                    .Where(f => f.TenantId == tenantId && f.IsActive)
                    .Select(f => new { f.FrameworkCode, f.FrameworkName, f.ComplianceScore })
                    .ToListAsync();

                var metrics = new
                {
                    MonthStart = monthStart,
                    MonthEnd = date.AddDays(-1),
                    FrameworkScores = complianceScores,
                    OverallComplianceScore = complianceScores.Any() ? complianceScores.Average(f => f.ComplianceScore ?? 0) : 0,
                    AssessmentsCompleted = await _context.Assessments.CountAsync(a => a.TenantId == tenantId && a.Status == "Approved" && a.CompletedDate >= monthStart)
                };

                var report = new GeneratedReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportType = "MonthlyComplianceReport",
                    Title = $"Monthly Compliance Report - {monthStart:MMMM yyyy}",
                    GeneratedAt = DateTime.UtcNow,
                    PeriodStart = monthStart,
                    PeriodEnd = date.AddDays(-1),
                    Status = "Generated",
                    Format = "JSON",
                    DataJson = JsonSerializer.Serialize(metrics),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:ReportGenerationJob",
                    IsActive = true
                };

                _context.GeneratedReports.Add(report);
                await NotifyStakeholdersAsync(tenantId, report);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly report for tenant {TenantId}", tenantId);
                return null;
            }
        }

        private async Task<GeneratedReport?> GenerateQuarterlyReportAsync(Guid tenantId, DateTime date)
        {
            try
            {
                var quarterStart = date.AddMonths(-3);
                var quarter = (date.Month - 1) / 3 + 1;

                var riskProfile = await _context.TenantRiskProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);

                var metrics = new
                {
                    Quarter = $"Q{quarter} {date.Year}",
                    QuarterStart = quarterStart,
                    QuarterEnd = date.AddDays(-1),
                    RiskProfile = riskProfile != null ? new
                    {
                        riskProfile.TotalRiskCount,
                        riskProfile.CriticalRiskCount,
                        riskProfile.HighRiskCount,
                        riskProfile.AverageRiskScore,
                        riskProfile.RiskTier
                    } : null,
                    Milestones = await _context.ComplianceCalendarEvents
                        .Where(e => e.TenantId == tenantId && e.DueDate >= date && e.DueDate <= date.AddDays(90))
                        .OrderBy(e => e.DueDate)
                        .Take(5)
                        .Select(e => $"{e.Title} - {e.DueDate:yyyy-MM-dd}")
                        .ToListAsync()
                };

                var report = new GeneratedReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportType = "QuarterlyExecutiveReport",
                    Title = $"Quarterly Executive Report - Q{quarter} {date.Year}",
                    GeneratedAt = DateTime.UtcNow,
                    PeriodStart = quarterStart,
                    PeriodEnd = date.AddDays(-1),
                    Status = "Generated",
                    Format = "JSON",
                    DataJson = JsonSerializer.Serialize(metrics),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:ReportGenerationJob",
                    IsActive = true
                };

                _context.GeneratedReports.Add(report);
                await NotifyExecutivesAsync(tenantId, report);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quarterly report for tenant {TenantId}", tenantId);
                return null;
            }
        }

        private async Task NotifyStakeholdersAsync(Guid tenantId, GeneratedReport report)
        {
            var managers = await _context.TenantUsers
                .Where(u => u.TenantId == tenantId && u.IsActive && (u.Role == "ComplianceManager" || u.Role == "Admin"))
                .Select(u => u.UserId).Take(5).ToListAsync();

            foreach (var managerId in managers)
            {
                await _notificationService.SendNotificationAsync(
                    report.Id, managerId.ToString(), "ReportGenerated",
                    $"New Report: {report.Title}",
                    $"A scheduled report has been generated.\n\nReport: {report.Title}\nType: {report.ReportType}",
                    "Low", tenantId);
            }
        }

        private async Task NotifyExecutivesAsync(Guid tenantId, GeneratedReport report)
        {
            var executives = await _context.TenantUsers
                .Where(u => u.TenantId == tenantId && u.IsActive && (u.Role == "Executive" || u.Role == "CISO" || u.Role == "Admin"))
                .Select(u => u.UserId).Take(5).ToListAsync();

            foreach (var execId in executives)
            {
                await _notificationService.SendNotificationAsync(
                    report.Id, execId.ToString(), "ExecutiveReportGenerated",
                    $"[EXECUTIVE] {report.Title}",
                    $"Your quarterly executive GRC report is ready.\n\nReport: {report.Title}",
                    "Medium", tenantId);
            }
        }
    }

    /// <summary>
    /// Maturity Score calculation job - recalculates maturity scores quarterly.
    /// Runs quarterly on the 1st at 3 AM
    /// </summary>
    public class MaturityScoreJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MaturityScoreJob> _logger;

        public MaturityScoreJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<MaturityScoreJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task RecalculateMaturityAsync()
        {
            _logger.LogInformation("MaturityScoreJob started at {Time}", DateTime.UtcNow);

            var tenants = await _context.Tenants.AsNoTracking()
                .Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED")
                .Select(t => t.Id).ToListAsync();

            var updated = 0;
            foreach (var tenantId in tenants)
            {
                if (await CalculateTenantMaturityAsync(tenantId))
                    updated++;
            }

            _logger.LogInformation("MaturityScoreJob completed. Updated {Count} tenants", updated);
        }

        private async Task<bool> CalculateTenantMaturityAsync(Guid tenantId)
        {
            try
            {
                // Get or create maturity assessment
                var maturity = await _context.MaturityAssessments
                    .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.IsActive);

                if (maturity == null)
                {
                    maturity = new MaturityAssessment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System:MaturityScoreJob",
                        IsActive = true
                    };
                    _context.MaturityAssessments.Add(maturity);
                }

                // Calculate dimension scores
                maturity.GovernanceScore = await CalculateGovernanceScoreAsync(tenantId);
                maturity.RiskManagementScore = await CalculateRiskManagementScoreAsync(tenantId);
                maturity.ComplianceScore = await CalculateComplianceScoreAsync(tenantId);
                maturity.OperationsScore = await CalculateOperationsScoreAsync(tenantId);
                maturity.CultureScore = await CalculateCultureScoreAsync(tenantId);

                // Overall score (weighted average)
                maturity.OverallScore = (
                    maturity.GovernanceScore * 0.2m +
                    maturity.RiskManagementScore * 0.25m +
                    maturity.ComplianceScore * 0.25m +
                    maturity.OperationsScore * 0.2m +
                    maturity.CultureScore * 0.1m
                );

                // Determine maturity level (CMM 1-5)
                maturity.MaturityLevel = maturity.OverallScore switch
                {
                    >= 80 => 5,   // Optimizing
                    >= 60 => 4,   // Quantitatively Managed
                    >= 40 => 3,   // Defined
                    >= 20 => 2,   // Managed
                    _ => 1        // Initial
                };

                maturity.LastCalculatedAt = DateTime.UtcNow;
                maturity.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating maturity for tenant {TenantId}", tenantId);
                return false;
            }
        }

        private async Task<decimal> CalculateGovernanceScoreAsync(Guid tenantId)
        {
            var hasRoles = await _context.TenantUsers.AnyAsync(u => u.TenantId == tenantId && u.IsActive);
            var hasWorkflows = await _context.WorkflowDefinitions.AnyAsync(w => w.TenantId == tenantId && w.IsActive);
            var hasPolicies = await _context.Policies.AnyAsync(p => p.TenantId == tenantId && p.IsActive);
            
            return (hasRoles ? 30 : 0) + (hasWorkflows ? 35 : 0) + (hasPolicies ? 35 : 0);
        }

        private async Task<decimal> CalculateRiskManagementScoreAsync(Guid tenantId)
        {
            var totalRisks = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.IsActive);
            var treatedRisks = await _context.Risks.CountAsync(r => r.TenantId == tenantId && r.IsActive && 
                (r.Status == "Mitigating" || r.Status == "Monitored" || r.Status == "Closed"));
            
            if (totalRisks == 0) return 50; // Neutral if no risks
            return (decimal)treatedRisks / totalRisks * 100;
        }

        private async Task<decimal> CalculateComplianceScoreAsync(Guid tenantId)
        {
            var total = await _context.TenantControlSets.CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ApplicabilityStatus == "Applicable");
            var compliant = await _context.TenantControlSets.CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ComplianceStatus == "COMPLIANT");
            
            if (total == 0) return 0;
            return (decimal)compliant / total * 100;
        }

        private async Task<decimal> CalculateOperationsScoreAsync(Guid tenantId)
        {
            var totalTasks = await _context.WorkflowTasks.CountAsync(t => t.TenantId == tenantId);
            var completedOnTime = await _context.WorkflowTasks.CountAsync(t => t.TenantId == tenantId && 
                t.Status == "Completed" && t.CompletedAt.HasValue && t.DueDate.HasValue && t.CompletedAt <= t.DueDate);
            
            if (totalTasks == 0) return 50;
            return (decimal)completedOnTime / totalTasks * 100;
        }

        private async Task<decimal> CalculateCultureScoreAsync(Guid tenantId)
        {
            // Culture score based on engagement metrics
            var activeUsers = await _context.TenantUsers.CountAsync(u => u.TenantId == tenantId && u.IsActive);
            var hasTraining = await _context.TrainingRecords.AnyAsync(t => t.TenantId == tenantId && t.IsActive);
            
            return (activeUsers > 0 ? 50 : 0) + (hasTraining ? 50 : 0);
        }
    }
}
