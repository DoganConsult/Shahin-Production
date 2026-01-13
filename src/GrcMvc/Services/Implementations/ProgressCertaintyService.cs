using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Progress Certainty Index Service - Calculates predictive completion scores.
/// Implements the fullplan specification for PCI tracking.
/// </summary>
public class ProgressCertaintyService : IProgressCertaintyService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<ProgressCertaintyService> _logger;

    public ProgressCertaintyService(
        GrcDbContext dbContext,
        ILogger<ProgressCertaintyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProgressCertaintyIndex> CalculatePciAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get previous PCI for trend tracking
            var previousPci = await GetLatestPciAsync(tenantId, entityType, entityId, cancellationToken);

            // Gather metrics
            var metrics = await GatherMetricsAsync(tenantId, entityId, cancellationToken);

            // Calculate component scores
            var taskScore = CalculateTaskScore(metrics);
            var velocityScore = CalculateVelocityScore(metrics);
            var qualityScore = CalculateQualityScore(metrics);
            var slaScore = CalculateSlaScore(metrics);
            var maturityScore = CalculateMaturityScore(metrics);

            // Calculate weighted overall score
            var overallScore = (int)Math.Round(
                taskScore * 0.30 +
                velocityScore * 0.25 +
                qualityScore * 0.20 +
                slaScore * 0.15 +
                maturityScore * 0.10
            );

            // Determine velocity trend
            var velocityTrend = DetermineVelocityTrend(metrics, previousPci);

            // Identify risk factors
            var riskFactors = IdentifyRiskFactors(metrics, overallScore);

            // Predict completion date
            var predictedCompletion = PredictCompletionDate(metrics);

            // Calculate days from baseline
            int? daysFromBaseline = null;
            if (predictedCompletion.HasValue && metrics.TargetCompletionDate.HasValue)
            {
                daysFromBaseline = (int)(metrics.TargetCompletionDate.Value - predictedCompletion.Value).TotalDays;
            }

            var pci = new ProgressCertaintyIndex
            {
                TenantId = tenantId,
                EntityType = entityType,
                EntityId = entityId,
                Score = overallScore,
                RiskBand = PciRiskBands.GetRiskBand(overallScore),
                ConfidenceLevel = CalculateConfidenceLevel(metrics),
                TasksCompletedPercent = metrics.TaskCompletionPercent,
                TaskVelocity = metrics.TaskVelocity,
                VelocityTrend = velocityTrend,
                EvidenceRejectionRate = metrics.EvidenceRejectionRate,
                SlaBreachCount = metrics.SlaBreachCount,
                SlaAdherencePercent = metrics.SlaAdherencePercent,
                AverageOverdueDays = metrics.AverageOverdueDays,
                OrgMaturityLevel = metrics.OrgMaturityLevel,
                ComplexityScore = metrics.ComplexityScore,
                TotalTasks = metrics.TotalTasks,
                CompletedTasks = metrics.CompletedTasks,
                OverdueTasks = metrics.OverdueTasks,
                AtRiskTasks = metrics.AtRiskTasks,
                PrimaryRiskFactors = riskFactors,
                FactorBreakdownJson = JsonSerializer.Serialize(new
                {
                    TaskScore = taskScore,
                    VelocityScore = velocityScore,
                    QualityScore = qualityScore,
                    SlaScore = slaScore,
                    MaturityScore = maturityScore
                }),
                RecommendedIntervention = GetRecommendedIntervention(overallScore, riskFactors),
                PredictedCompletionDate = predictedCompletion,
                TargetCompletionDate = metrics.TargetCompletionDate,
                DaysFromBaseline = daysFromBaseline,
                PreviousScore = previousPci?.Score,
                ScoreChange = previousPci != null ? overallScore - previousPci.Score : null,
                CalculatedAt = DateTime.UtcNow
            };

            _dbContext.Set<ProgressCertaintyIndex>().Add(pci);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Calculated PCI for tenant {TenantId}: Score={Score}, RiskBand={RiskBand}",
                tenantId, pci.Score, pci.RiskBand);

            return pci;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating PCI for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<ProgressCertaintyIndex?> GetLatestPciAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ProgressCertaintyIndex>()
            .Where(p => p.TenantId == tenantId && p.EntityType == entityType);

        if (entityId.HasValue)
        {
            query = query.Where(p => p.EntityId == entityId);
        }
        else
        {
            query = query.Where(p => p.EntityId == null);
        }

        return await query
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ProgressCertaintyIndex>> GetPciHistoryAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        var query = _dbContext.Set<ProgressCertaintyIndex>()
            .Where(p => p.TenantId == tenantId &&
                       p.EntityType == entityType &&
                       p.CalculatedAt >= since);

        if (entityId.HasValue)
        {
            query = query.Where(p => p.EntityId == entityId);
        }
        else
        {
            query = query.Where(p => p.EntityId == null);
        }

        return await query
            .OrderByDescending(p => p.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetRecommendedInterventionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var pci = await GetLatestPciAsync(tenantId, cancellationToken: cancellationToken);
        if (pci == null)
        {
            return new List<string> { "Calculate PCI first to get recommendations" };
        }

        var interventions = new List<string>();

        if (pci.Score < 40)
        {
            interventions.Add("Consider reducing scope or deferring non-critical tasks");
            interventions.Add("Schedule urgent coordination meeting with team");
        }

        if (pci.VelocityTrend == "Declining")
        {
            interventions.Add("Investigate blockers causing velocity decline");
            interventions.Add("Consider reassigning tasks to available resources");
        }

        if (pci.EvidenceRejectionRate > 30)
        {
            interventions.Add("Provide evidence quality training to team");
            interventions.Add("Review and clarify evidence requirements");
        }

        if (pci.SlaBreachCount > 3)
        {
            interventions.Add("Review SLA targets for feasibility");
            interventions.Add("Implement early warning alerts for approaching deadlines");
        }

        if (pci.OverdueTasks > 5)
        {
            interventions.Add("Prioritize clearing overdue task backlog");
            interventions.Add("Consider task automation opportunities");
        }

        return interventions;
    }

    public async Task<PredictionResult> PredictCompletionAsync(
        Guid tenantId,
        Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GatherMetricsAsync(tenantId, planId, cancellationToken);

            if (metrics.TaskVelocity <= 0 || metrics.TotalTasks == 0)
            {
                return new PredictionResult
                {
                    Success = false,
                    RiskAssessment = "Insufficient data for prediction"
                };
            }

            var remainingTasks = metrics.TotalTasks - metrics.CompletedTasks;
            var weeksNeeded = remainingTasks / metrics.TaskVelocity;
            var predictedDate = DateTime.UtcNow.AddDays(weeksNeeded * 7);

            int? variance = null;
            if (metrics.TargetCompletionDate.HasValue)
            {
                variance = (int)(metrics.TargetCompletionDate.Value - predictedDate).TotalDays;
            }

            var riskFactors = new List<string>();
            if (variance.HasValue && variance < 0)
            {
                riskFactors.Add($"Currently {Math.Abs(variance.Value)} days behind schedule");
            }
            if (metrics.VelocityTrend == "Declining")
            {
                riskFactors.Add("Velocity trend is declining");
            }
            if (metrics.OverdueTasks > 5)
            {
                riskFactors.Add($"{metrics.OverdueTasks} tasks are overdue");
            }

            return new PredictionResult
            {
                Success = true,
                PredictedDate = predictedDate,
                TargetDate = metrics.TargetCompletionDate,
                DaysVariance = variance,
                ConfidencePercent = CalculateConfidenceLevel(metrics),
                RiskAssessment = variance.HasValue && variance >= 0 ? "On Track" :
                                variance.HasValue && variance >= -7 ? "At Risk" : "Critical",
                RiskFactors = riskFactors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting completion for tenant {TenantId}", tenantId);
            return new PredictionResult
            {
                Success = false,
                RiskAssessment = ex.Message
            };
        }
    }

    #region Private Helper Methods

    private async Task<PciMetrics> GatherMetricsAsync(
        Guid tenantId, Guid? entityId, CancellationToken cancellationToken)
    {
        var metrics = new PciMetrics();

        // Task metrics
        var tasks = await _dbContext.Set<WorkflowTask>()
            .Where(t => t.TenantId == tenantId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        metrics.TotalTasks = tasks.Count;
        metrics.CompletedTasks = tasks.Count(t => t.Status == "Completed");
        metrics.OverdueTasks = tasks.Count(t => t.Status != "Completed" && t.DueDate < DateTime.UtcNow);
        metrics.AtRiskTasks = tasks.Count(t => t.Status != "Completed" &&
                                              t.DueDate >= DateTime.UtcNow &&
                                              t.DueDate <= DateTime.UtcNow.AddDays(3));

        metrics.TaskCompletionPercent = metrics.TotalTasks > 0
            ? (double)metrics.CompletedTasks / metrics.TotalTasks * 100
            : 0;

        // Calculate velocity (tasks completed per week over last 4 weeks)
        var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);
        var recentlyCompleted = tasks.Count(t =>
            t.Status == "Completed" &&
            t.CompletedAt >= fourWeeksAgo);
        metrics.TaskVelocity = recentlyCompleted / 4.0;

        // Evidence metrics
        var evidenceSubmissions = await _dbContext.Evidences
            .Where(e => e.TenantId == tenantId &&
                       e.CreatedDate >= DateTime.UtcNow.AddDays(-30))
            .ToListAsync(cancellationToken);

        var totalSubmissions = evidenceSubmissions.Count;
        var rejectedSubmissions = evidenceSubmissions.Count(e => e.VerificationStatus == "Rejected");
        metrics.EvidenceRejectionRate = totalSubmissions > 0
            ? (double)rejectedSubmissions / totalSubmissions * 100
            : 0;

        // SLA metrics
        var overdueItems = tasks.Where(t => t.Status != "Completed" && t.DueDate < DateTime.UtcNow).ToList();
        metrics.SlaBreachCount = overdueItems.Count;
        metrics.AverageOverdueDays = overdueItems.Any()
            ? overdueItems.Average(t => (DateTime.UtcNow - (t.DueDate ?? DateTime.UtcNow)).TotalDays)
            : 0;

        var totalWithDeadlines = tasks.Count(t => t.DueDate.HasValue);
        var onTimeCompletions = tasks.Count(t =>
            t.Status == "Completed" &&
            t.DueDate.HasValue &&
            t.CompletedAt <= t.DueDate);
        metrics.SlaAdherencePercent = totalWithDeadlines > 0
            ? (double)onTimeCompletions / totalWithDeadlines * 100
            : 100;

        // Org maturity (from onboarding)
        var wizard = await _dbContext.Set<OnboardingWizard>()
            .Where(w => w.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
        metrics.OrgMaturityLevel = wizard?.MaturityLevel ?? 3;

        // Complexity score (based on frameworks and scope)
        var frameworkCount = await _dbContext.TenantBaselines
            .Where(b => b.TenantId == tenantId)
            .CountAsync(cancellationToken);
        metrics.ComplexityScore = Math.Min(10, frameworkCount + 2);

        // Target completion date from active plan
        var activePlan = await _dbContext.Set<Plan>()
            .Where(p => p.TenantId == tenantId && p.Status == "Active")
            .FirstOrDefaultAsync(cancellationToken);
        metrics.TargetCompletionDate = activePlan?.TargetEndDate;

        // Determine velocity trend
        var previousPci = await _dbContext.Set<ProgressCertaintyIndex>()
            .Where(p => p.TenantId == tenantId && p.CalculatedAt < DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (previousPci != null)
        {
            var velocityChange = metrics.TaskVelocity - previousPci.TaskVelocity;
            metrics.VelocityTrend = velocityChange > 0.5 ? "Improving" :
                                   velocityChange < -0.5 ? "Declining" : "Stable";
        }
        else
        {
            metrics.VelocityTrend = "Stable";
        }

        return metrics;
    }

    private int CalculateTaskScore(PciMetrics metrics)
    {
        if (metrics.TotalTasks == 0) return 50;

        var completionScore = metrics.TaskCompletionPercent;
        var overdueDeduction = Math.Min(30, metrics.OverdueTasks * 5);
        var atRiskDeduction = Math.Min(15, metrics.AtRiskTasks * 3);

        return (int)Math.Max(0, Math.Min(100, completionScore - overdueDeduction - atRiskDeduction));
    }

    private int CalculateVelocityScore(PciMetrics metrics)
    {
        // Expected velocity based on remaining work and time
        var expectedVelocity = metrics.TotalTasks > 0 ? (metrics.TotalTasks - metrics.CompletedTasks) / 12.0 : 1;
        var velocityRatio = expectedVelocity > 0 ? metrics.TaskVelocity / expectedVelocity : 1;

        var baseScore = Math.Min(100, velocityRatio * 70);

        // Adjust for trend
        var trendAdjustment = metrics.VelocityTrend switch
        {
            "Improving" => 15,
            "Declining" => -15,
            _ => 0
        };

        return (int)Math.Max(0, Math.Min(100, baseScore + trendAdjustment));
    }

    private int CalculateQualityScore(PciMetrics metrics)
    {
        // Lower rejection rate = higher score
        var rejectionDeduction = Math.Min(50, metrics.EvidenceRejectionRate);
        return (int)Math.Max(0, 100 - rejectionDeduction);
    }

    private int CalculateSlaScore(PciMetrics metrics)
    {
        var baseScore = metrics.SlaAdherencePercent;
        var breachDeduction = Math.Min(30, metrics.SlaBreachCount * 5);
        return (int)Math.Max(0, Math.Min(100, baseScore - breachDeduction));
    }

    private int CalculateMaturityScore(PciMetrics metrics)
    {
        // Higher maturity = better predictability
        return metrics.OrgMaturityLevel.HasValue
            ? metrics.OrgMaturityLevel.Value * 20
            : 50;
    }

    private int CalculateConfidenceLevel(PciMetrics metrics)
    {
        // Confidence is higher when we have more data
        var dataPoints = 0;
        if (metrics.TotalTasks > 10) dataPoints += 20;
        if (metrics.TaskVelocity > 0) dataPoints += 25;
        if (metrics.CompletedTasks > 5) dataPoints += 20;
        if (metrics.OrgMaturityLevel.HasValue) dataPoints += 15;
        if (metrics.TargetCompletionDate.HasValue) dataPoints += 20;

        return Math.Min(100, dataPoints);
    }

    private string DetermineVelocityTrend(PciMetrics metrics, ProgressCertaintyIndex? previousPci)
    {
        if (previousPci == null) return "Stable";

        var velocityChange = metrics.TaskVelocity - previousPci.TaskVelocity;
        return velocityChange > 0.5 ? "Improving" :
               velocityChange < -0.5 ? "Declining" : "Stable";
    }

    private List<string> IdentifyRiskFactors(PciMetrics metrics, int overallScore)
    {
        var factors = new List<string>();

        if (metrics.OverdueTasks > 5)
            factors.Add($"High number of overdue tasks ({metrics.OverdueTasks})");

        if (metrics.EvidenceRejectionRate > 30)
            factors.Add($"High evidence rejection rate ({metrics.EvidenceRejectionRate:F1}%)");

        if (metrics.VelocityTrend == "Declining")
            factors.Add("Task completion velocity is declining");

        if (metrics.SlaBreachCount > 3)
            factors.Add($"Multiple SLA breaches ({metrics.SlaBreachCount})");

        if (metrics.AverageOverdueDays > 7)
            factors.Add($"Average overdue duration is {metrics.AverageOverdueDays:F1} days");

        if (overallScore < 40)
            factors.Add("Overall progress certainty is critical");

        return factors;
    }

    private DateTime? PredictCompletionDate(PciMetrics metrics)
    {
        if (metrics.TaskVelocity <= 0 || metrics.TotalTasks == 0)
            return null;

        var remainingTasks = metrics.TotalTasks - metrics.CompletedTasks;
        var weeksNeeded = remainingTasks / metrics.TaskVelocity;

        return DateTime.UtcNow.AddDays(weeksNeeded * 7);
    }

    private string GetRecommendedIntervention(int score, List<string> riskFactors)
    {
        if (score >= 80)
            return "Continue current pace. Monitor for any emerging risks.";
        if (score >= 60)
            return "Consider addressing identified risk factors to maintain progress.";
        if (score >= 40)
            return "Increase automation and escalate blockers. Review resource allocation.";

        return "Urgent intervention needed. Consider scope reduction and emergency resource allocation.";
    }

    #endregion

    private class PciMetrics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int AtRiskTasks { get; set; }
        public double TaskCompletionPercent { get; set; }
        public double TaskVelocity { get; set; }
        public string VelocityTrend { get; set; } = "Stable";
        public double EvidenceRejectionRate { get; set; }
        public int SlaBreachCount { get; set; }
        public double SlaAdherencePercent { get; set; }
        public double AverageOverdueDays { get; set; }
        public int? OrgMaturityLevel { get; set; }
        public int? ComplexityScore { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
    }
}
