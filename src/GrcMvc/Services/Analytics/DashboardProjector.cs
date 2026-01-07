using GrcMvc.Data;
using GrcMvc.Hubs;
using GrcMvc.Messaging.Messages;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Analytics
{
    /// <summary>
    /// Dashboard Projector - Builds and updates dashboard snapshots
    /// Subscribes to domain events and updates pre-aggregated analytics data
    /// </summary>
    public interface IDashboardProjector
    {
        Task ProjectSnapshotAsync(Guid tenantId);
        Task ProjectComplianceTrendsAsync(Guid tenantId);
        Task ProjectRiskHeatmapAsync(Guid tenantId);
        Task ProjectFrameworkComparisonAsync(Guid tenantId);
        Task ProjectTaskMetricsAsync(Guid tenantId);
        Task ProjectEvidenceMetricsAsync(Guid tenantId);
        Task ProjectTopActionsAsync(Guid tenantId);
        Task ProjectAllAsync(Guid tenantId);
        Task HandleEventAsync(IGrcEvent domainEvent);
    }

    public class DashboardProjector : IDashboardProjector
    {
        private readonly GrcDbContext _context;
        private readonly IClickHouseService _clickHouse;
        private readonly IDashboardHubService _hubService;
        private readonly ILogger<DashboardProjector> _logger;

        public DashboardProjector(
            GrcDbContext context,
            IClickHouseService clickHouse,
            IDashboardHubService hubService,
            ILogger<DashboardProjector> logger)
        {
            _context = context;
            _clickHouse = clickHouse;
            _hubService = hubService;
            _logger = logger;
        }

        /// <summary>
        /// Project complete dashboard snapshot for a tenant
        /// </summary>
        public async Task ProjectSnapshotAsync(Guid tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var snapshotHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

                // Build snapshot from operational data
                var snapshot = new DashboardSnapshotDto
                {
                    TenantId = tenantId,
                    SnapshotDate = snapshotHour.Date,
                    SnapshotHour = snapshotHour,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                // Compliance metrics from AssessmentRequirements
                var requirements = await _context.AssessmentRequirements
                    .Include(r => r.Assessment)
                    .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                snapshot.TotalControls = requirements.Count;
                snapshot.CompliantControls = requirements.Count(r => r.Status == "Compliant");
                snapshot.PartialControls = requirements.Count(r => r.Status == "PartiallyCompliant");
                snapshot.NonCompliantControls = requirements.Count(r => r.Status == "NonCompliant");
                snapshot.NotStartedControls = requirements.Count(r => r.Status == "NotStarted" || string.IsNullOrEmpty(r.Status));
                snapshot.ComplianceScore = requirements.Any()
                    ? (decimal)requirements.Average(r => r.Score ?? 0)
                    : 0;

                // Risk metrics
                var risks = await _context.Risks
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                snapshot.TotalRisks = risks.Count;
                snapshot.CriticalRisks = risks.Count(r => r.RiskScore >= 20);
                snapshot.HighRisks = risks.Count(r => r.RiskScore >= 12 && r.RiskScore < 20);
                snapshot.MediumRisks = risks.Count(r => r.RiskScore >= 6 && r.RiskScore < 12);
                snapshot.LowRisks = risks.Count(r => r.RiskScore < 6);
                snapshot.OpenRisks = risks.Count(r => r.Status == "Open" || r.Status == "Identified");
                snapshot.MitigatedRisks = risks.Count(r => r.Status == "Mitigated" || r.Status == "Closed");
                snapshot.RiskScoreAvg = risks.Any() ? (decimal)risks.Average(r => r.RiskScore) : 0;

                // Task metrics
                var tasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.WorkflowInstance.TenantId == tenantId && !t.IsDeleted)
                    .ToListAsync();

                var weekFromNow = now.AddDays(7);
                snapshot.TotalTasks = tasks.Count;
                snapshot.PendingTasks = tasks.Count(t => t.Status == "Pending");
                snapshot.InProgressTasks = tasks.Count(t => t.Status == "InProgress");
                snapshot.CompletedTasks = tasks.Count(t => t.Status == "Completed");
                snapshot.OverdueTasks = tasks.Count(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled");
                snapshot.DueThisWeek = tasks.Count(t => t.DueDate >= now && t.DueDate <= weekFromNow && t.Status != "Completed");

                // Evidence metrics
                var evidence = await _context.Evidences
                    .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                    .ToListAsync();

                snapshot.TotalEvidence = evidence.Count;
                snapshot.EvidenceSubmitted = evidence.Count(e => e.VerificationStatus == "Submitted");
                snapshot.EvidenceApproved = evidence.Count(e => e.VerificationStatus == "Approved");
                snapshot.EvidenceRejected = evidence.Count(e => e.VerificationStatus == "Rejected");
                snapshot.EvidencePending = evidence.Count(e => e.VerificationStatus == "Pending" || string.IsNullOrEmpty(e.VerificationStatus));

                // Assessment metrics
                var assessments = await _context.Assessments
                    .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                    .ToListAsync();

                snapshot.TotalAssessments = assessments.Count;
                snapshot.ActiveAssessments = assessments.Count(a => a.Status == "Active" || a.Status == "InProgress");
                snapshot.CompletedAssessments = assessments.Count(a => a.Status == "Completed");

                // Plan metrics
                var plans = await _context.Plans
                    .Include(p => p.Phases)
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .ToListAsync();

                snapshot.TotalPlans = plans.Count;
                snapshot.ActivePlans = plans.Count(p => p.Status == "Active" || p.Status == "InProgress");
                snapshot.CompletedPlans = plans.Count(p => p.Status == "Completed");
                snapshot.OverallPlanProgress = plans.Any()
                    ? plans.Average(p => p.Phases.Any()
                        ? (decimal)p.Phases.Count(ph => ph.Status == "Completed") / p.Phases.Count * 100
                        : 0)
                    : 0;

                // Save to ClickHouse
                await _clickHouse.UpsertSnapshotAsync(snapshot);

                // Notify connected clients
                await _hubService.NotifyDashboardUpdateAsync(tenantId, "executive", snapshot);

                _logger.LogInformation("Projected dashboard snapshot for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting dashboard snapshot for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project compliance trends by framework
        /// </summary>
        public async Task ProjectComplianceTrendsAsync(Guid tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var measureHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

                // Get requirements grouped by framework
                var requirements = await _context.AssessmentRequirements
                    .Include(r => r.Assessment)
                    .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                var byFramework = requirements.GroupBy(r => r.Assessment?.FrameworkCode ?? "Unknown");

                foreach (var group in byFramework)
                {
                    var trend = new ComplianceTrendDto
                    {
                        TenantId = tenantId,
                        FrameworkCode = group.Key,
                        BaselineCode = group.Key,
                        MeasureDate = measureHour.Date,
                        MeasureHour = measureHour,
                        TotalControls = group.Count(),
                        CompliantControls = group.Count(r => r.Status == "Compliant"),
                        PartialControls = group.Count(r => r.Status == "PartiallyCompliant"),
                        NonCompliantControls = group.Count(r => r.Status == "NonCompliant"),
                        ComplianceScore = group.Any() ? (decimal)group.Average(r => r.Score ?? 0) : 0
                    };

                    // Calculate delta from previous
                    var previousTrends = await _clickHouse.GetComplianceTrendsByFrameworkAsync(tenantId, group.Key, 1);
                    var previousScore = previousTrends.LastOrDefault()?.ComplianceScore ?? 0;
                    trend.DeltaFromPrevious = trend.ComplianceScore - previousScore;

                    await _clickHouse.UpsertComplianceTrendAsync(trend);

                    // Notify if significant change
                    if (Math.Abs(trend.DeltaFromPrevious) >= 5)
                    {
                        await _hubService.NotifyComplianceScoreChangeAsync(
                            tenantId, group.Key, previousScore, trend.ComplianceScore);
                    }
                }

                _logger.LogInformation("Projected compliance trends for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting compliance trends for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project risk heatmap (5x5 matrix)
        /// </summary>
        public async Task ProjectRiskHeatmapAsync(Guid tenantId)
        {
            try
            {
                var risks = await _context.Risks
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.Status != "Closed")
                    .ToListAsync();

                var cells = new List<RiskHeatmapCell>();

                // Build 5x5 matrix
                for (int likelihood = 1; likelihood <= 5; likelihood++)
                {
                    for (int impact = 1; impact <= 5; impact++)
                    {
                        var matchingRisks = risks.Where(r => r.Likelihood == likelihood && r.Impact == impact).ToList();
                        if (matchingRisks.Any())
                        {
                            cells.Add(new RiskHeatmapCell
                            {
                                Likelihood = likelihood,
                                Impact = impact,
                                RiskCount = matchingRisks.Count,
                                RiskIds = matchingRisks.Select(r => r.Id).ToList()
                            });
                        }
                    }
                }

                await _clickHouse.UpsertRiskHeatmapAsync(tenantId, cells);
                await _hubService.NotifyWidgetUpdateAsync(tenantId, "riskHeatmap", cells);

                _logger.LogInformation("Projected risk heatmap for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting risk heatmap for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project framework comparison view
        /// </summary>
        public async Task ProjectFrameworkComparisonAsync(Guid tenantId)
        {
            try
            {
                var baselines = await _context.TenantBaselines
                    .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                    .ToListAsync();

                var requirements = await _context.AssessmentRequirements
                    .Include(r => r.Assessment)
                    .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                foreach (var baseline in baselines)
                {
                    var baselineReqs = requirements.Where(r => r.Assessment?.FrameworkCode == baseline.BaselineCode).ToList();

                    var comparison = new FrameworkComparisonDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = DateTime.UtcNow.Date,
                        FrameworkCode = baseline.BaselineCode,
                        FrameworkName = baseline.BaselineName,
                        TotalRequirements = baselineReqs.Count,
                        CompliantCount = baselineReqs.Count(r => r.Status == "Compliant"),
                        PartialCount = baselineReqs.Count(r => r.Status == "PartiallyCompliant"),
                        NonCompliantCount = baselineReqs.Count(r => r.Status == "NonCompliant"),
                        NotAssessedCount = baselineReqs.Count(r => r.Status == "NotStarted" || string.IsNullOrEmpty(r.Status)),
                        ComplianceScore = baselineReqs.Any() ? (decimal)baselineReqs.Average(r => r.Score ?? 0) : 0,
                        MaturityLevel = CalculateMaturityLevel(baselineReqs)
                    };

                    // Calculate trends
                    var previousTrends = await _clickHouse.GetComplianceTrendsByFrameworkAsync(tenantId, baseline.BaselineCode, 1);
                    if (previousTrends.Any())
                    {
                        var trend7d = previousTrends.Where(t => t.MeasureDate >= DateTime.UtcNow.AddDays(-7)).ToList();
                        var trend30d = previousTrends.Where(t => t.MeasureDate >= DateTime.UtcNow.AddDays(-30)).ToList();

                        comparison.Trend7d = trend7d.Any() ? comparison.ComplianceScore - trend7d.First().ComplianceScore : 0;
                        comparison.Trend30d = trend30d.Any() ? comparison.ComplianceScore - trend30d.First().ComplianceScore : 0;
                    }

                    await _clickHouse.UpsertFrameworkComparisonAsync(comparison);
                }

                _logger.LogInformation("Projected framework comparison for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting framework comparison for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project task metrics by role
        /// </summary>
        public async Task ProjectTaskMetricsAsync(Guid tenantId)
        {
            try
            {
                var tasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.WorkflowInstance.TenantId == tenantId && !t.IsDeleted)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var byRole = tasks.GroupBy(t => t.AssignedToUserName ?? "Unassigned");

                foreach (var group in byRole)
                {
                    var completedTasks = group.Where(t => t.Status == "Completed" && t.CompletedAt.HasValue).ToList();

                    var metrics = new TaskMetricsByRoleDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = now.Date,
                        RoleCode = group.Key,
                        TotalTasks = group.Count(),
                        PendingTasks = group.Count(t => t.Status == "Pending"),
                        InProgressTasks = group.Count(t => t.Status == "InProgress"),
                        CompletedTasks = group.Count(t => t.Status == "Completed"),
                        OverdueTasks = group.Count(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled"),
                        AvgCompletionDays = completedTasks.Any()
                            ? (decimal)completedTasks.Average(t => (t.CompletedAt!.Value - t.CreatedDate).TotalDays)
                            : 0,
                        SlaComplianceRate = completedTasks.Any()
                            ? (decimal)completedTasks.Count(t => t.CompletedAt <= t.DueDate) / completedTasks.Count * 100
                            : 100
                    };

                    await _clickHouse.UpsertTaskMetricsByRoleAsync(metrics);
                }

                _logger.LogInformation("Projected task metrics for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting task metrics for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project evidence collection metrics
        /// </summary>
        public async Task ProjectEvidenceMetricsAsync(Guid tenantId)
        {
            try
            {
                var evidence = await _context.Evidences
                    .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var byType = evidence.GroupBy(e => e.Type ?? "Other");

                foreach (var group in byType)
                {
                    var approved = group.Where(e => e.VerificationStatus == "Approved").ToList();

                    var metrics = new EvidenceMetricsDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = now.Date,
                        EvidenceType = group.Key,
                        TotalCollected = group.Count(),
                        TotalApproved = approved.Count,
                        TotalRejected = group.Count(e => e.VerificationStatus == "Rejected"),
                        CollectionRate = group.Any() ? (decimal)group.Count() / Math.Max(1, group.Count()) * 100 : 0,
                        ApprovalRate = group.Any() ? (decimal)approved.Count / group.Count() * 100 : 0,
                        AvgReviewDays = approved.Any() && approved.All(e => e.VerificationDate.HasValue)
                            ? (decimal)approved.Average(e => (e.VerificationDate!.Value - e.CollectionDate).TotalDays)
                            : 0
                    };

                    await _clickHouse.UpsertEvidenceMetricsAsync(metrics);
                }

                _logger.LogInformation("Projected evidence metrics for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting evidence metrics for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project top 10 priority actions
        /// </summary>
        public async Task ProjectTopActionsAsync(Guid tenantId)
        {
            try
            {
                var actions = new List<TopActionDto>();
                var now = DateTime.UtcNow;
                var rank = 1;

                // 1. Overdue tasks (Critical)
                var overdueTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                        t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled" && !t.IsDeleted)
                    .OrderBy(t => t.DueDate)
                    .Take(3)
                    .ToListAsync();

                foreach (var task in overdueTasks)
                {
                    actions.Add(new TopActionDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = now.Date,
                        ActionRank = rank++,
                        ActionType = "OverdueTask",
                        ActionTitle = $"Complete: {task.TaskName}",
                        ActionDescription = $"Task is {(int)(now - (task.DueDate ?? now)).TotalDays} days overdue",
                        EntityType = "WorkflowTask",
                        EntityId = task.Id,
                        Urgency = "Critical",
                        DueDate = task.DueDate,
                        AssignedTo = task.AssignedToUserName ?? ""
                    });
                }

                // 2. High risks (High)
                var highRisks = await _context.Risks
                    .Where(r => r.TenantId == tenantId && r.RiskScore >= 12 &&
                        r.Status != "Mitigated" && r.Status != "Closed" && !r.IsDeleted)
                    .OrderByDescending(r => r.RiskScore)
                    .Take(3)
                    .ToListAsync();

                foreach (var risk in highRisks)
                {
                    actions.Add(new TopActionDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = now.Date,
                        ActionRank = rank++,
                        ActionType = "HighRisk",
                        ActionTitle = $"Mitigate: {risk.Name}",
                        ActionDescription = $"Risk score: {risk.RiskScore}",
                        EntityType = "Risk",
                        EntityId = risk.Id,
                        Urgency = "High",
                        AssignedTo = risk.Owner ?? ""
                    });
                }

                // 3. Non-compliant controls (Medium)
                var nonCompliant = await _context.AssessmentRequirements
                    .Include(r => r.Assessment)
                    .Where(r => r.Assessment.TenantId == tenantId && r.Status == "NonCompliant" && !r.IsDeleted)
                    .Take(4)
                    .ToListAsync();

                foreach (var req in nonCompliant)
                {
                    actions.Add(new TopActionDto
                    {
                        TenantId = tenantId,
                        SnapshotDate = now.Date,
                        ActionRank = rank++,
                        ActionType = "NonCompliantControl",
                        ActionTitle = $"Remediate: {req.ControlNumber}",
                        ActionDescription = $"{req.ControlTitle} - Score: {req.Score ?? 0}%",
                        EntityType = "AssessmentRequirement",
                        EntityId = req.Id,
                        Urgency = "Medium"
                    });
                }

                await _clickHouse.UpsertTopActionsAsync(tenantId, actions.Take(10).ToList());
                await _hubService.NotifyWidgetUpdateAsync(tenantId, "topActions", actions.Take(10));

                _logger.LogInformation("Projected top actions for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting top actions for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Project all analytics for a tenant
        /// </summary>
        public async Task ProjectAllAsync(Guid tenantId)
        {
            _logger.LogInformation("Starting full analytics projection for tenant {TenantId}", tenantId);

            await ProjectSnapshotAsync(tenantId);
            await ProjectComplianceTrendsAsync(tenantId);
            await ProjectRiskHeatmapAsync(tenantId);
            await ProjectFrameworkComparisonAsync(tenantId);
            await ProjectTaskMetricsAsync(tenantId);
            await ProjectEvidenceMetricsAsync(tenantId);
            await ProjectTopActionsAsync(tenantId);

            _logger.LogInformation("Completed full analytics projection for tenant {TenantId}", tenantId);
        }

        /// <summary>
        /// Handle domain event and trigger appropriate projections
        /// </summary>
        public async Task HandleEventAsync(IGrcEvent domainEvent)
        {
            // Log event to ClickHouse
            await _clickHouse.InsertEventAsync(new AnalyticsEventDto
            {
                EventId = domainEvent.EventId,
                TenantId = domainEvent.TenantId,
                EventType = domainEvent.EventType,
                EventTimestamp = domainEvent.Timestamp
            });

            // Trigger relevant projections based on event type
            switch (domainEvent)
            {
                case RiskAssessedEvent:
                    await ProjectRiskHeatmapAsync(domainEvent.TenantId);
                    await ProjectSnapshotAsync(domainEvent.TenantId);
                    break;

                case ControlUpdatedEvent:
                case ComplianceStatusChangedEvent:
                    await ProjectComplianceTrendsAsync(domainEvent.TenantId);
                    await ProjectFrameworkComparisonAsync(domainEvent.TenantId);
                    await ProjectSnapshotAsync(domainEvent.TenantId);
                    break;

                case TaskAssignedEvent:
                case SlaBreachedEvent:
                    await ProjectTaskMetricsAsync(domainEvent.TenantId);
                    await ProjectTopActionsAsync(domainEvent.TenantId);
                    break;

                case EvidenceSubmittedEvent:
                    await ProjectEvidenceMetricsAsync(domainEvent.TenantId);
                    break;
            }
        }

        private static int CalculateMaturityLevel(List<Models.Entities.AssessmentRequirement> requirements)
        {
            if (!requirements.Any()) return 1;

            var score = requirements.Average(r => r.Score ?? 0);
            return score switch
            {
                >= 90 => 5, // Optimized
                >= 75 => 4, // Managed
                >= 50 => 3, // Defined
                >= 25 => 2, // Developing
                _ => 1      // Initial
            };
        }
    }
}
