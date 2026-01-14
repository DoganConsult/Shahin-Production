using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 9: Dashboard Service Implementation
    /// Provides compliance dashboards, reporting, and analytics
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(GrcDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Executive Dashboard

        public async Task<ExecutiveDashboard> GetExecutiveDashboardAsync(Guid tenantId)
        {
            var requirements = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var plans = await _context.Plans
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .ToListAsync();

            var tasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var baselines = await _context.TenantBaselines
                .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                .ToListAsync();

            var complianceByBaseline = baselines.Select(baseline =>
            {
                var baselineReqs = requirements.Where(r =>
                    r.Assessment?.FrameworkCode == baseline.BaselineCode).ToList();

                return new ComplianceByBaseline
                {
                    BaselineCode = baseline.BaselineCode,
                    BaselineName = baseline.BaselineName,
                    TotalControls = baselineReqs.Count,
                    CompliantControls = baselineReqs.Count(r => r.Status == "Compliant"),
                    Score = baselineReqs.Any() ? (decimal)baselineReqs.Average(r => r.Score ?? 0) : 0
                };
            }).ToList();

            return new ExecutiveDashboard
            {
                OverallComplianceScore = requirements.Any() ? (decimal)requirements.Average(r => r.Score ?? 0) : 0,
                TotalRequirements = requirements.Count,
                CompliantRequirements = requirements.Count(r => r.Status == "Compliant"),
                PartialRequirements = requirements.Count(r => r.Status == "PartiallyCompliant"),
                NonCompliantRequirements = requirements.Count(r => r.Status == "NonCompliant"),
                NotStartedRequirements = requirements.Count(r => r.Status == "NotStarted" || string.IsNullOrEmpty(r.Status)),
                ActivePlans = plans.Count(p => p.Status == "Active" || p.Status == "InProgress"),
                CompletedPlans = plans.Count(p => p.Status == "Completed"),
                OpenTasks = tasks.Count(t => t.Status != "Completed" && t.Status != "Cancelled"),
                OverdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow &&
                    t.Status != "Completed" && t.Status != "Cancelled"),
                HighRisks = risks.Count(r => r.RiskScore >= 8),
                MediumRisks = risks.Count(r => r.RiskScore >= 4 && r.RiskScore < 8),
                LowRisks = risks.Count(r => r.RiskScore < 4),
                ComplianceByBaseline = complianceByBaseline,
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Compliance Dashboard

        public async Task<ComplianceDashboard> GetComplianceDashboardAsync(Guid tenantId)
        {
            var requirements = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var byFramework = requirements
                .GroupBy(r => r.Assessment?.FrameworkCode ?? "Unknown")
                .Select(g => new FrameworkCompliance
                {
                    FrameworkCode = g.Key,
                    FrameworkName = g.Key,
                    TotalControls = g.Count(),
                    Compliant = g.Count(r => r.Status == "Compliant"),
                    Partial = g.Count(r => r.Status == "PartiallyCompliant"),
                    NonCompliant = g.Count(r => r.Status == "NonCompliant"),
                    NotStarted = g.Count(r => r.Status == "NotStarted" || string.IsNullOrEmpty(r.Status)),
                    Score = g.Any() ? (decimal)g.Average(r => r.Score ?? 0) : 0
                })
                .ToList();

            var byDomain = requirements
                .GroupBy(r => r.Domain ?? "Unknown")
                .Select(g => new DomainCompliance
                {
                    Domain = g.Key,
                    TotalControls = g.Count(),
                    Compliant = g.Count(r => r.Status == "Compliant"),
                    Score = g.Any() ? (decimal)g.Average(r => r.Score ?? 0) : 0
                })
                .OrderByDescending(d => d.Score)
                .ToList();

            var total = requirements.Count;
            var byStatus = requirements
                .GroupBy(r => r.Status ?? "NotStarted")
                .Select(g => new StatusBreakdown
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = total > 0 ? (decimal)g.Count() / total * 100 : 0
                })
                .ToList();

            return new ComplianceDashboard
            {
                OverallScore = requirements.Any() ? (decimal)requirements.Average(r => r.Score ?? 0) : 0,
                ByFramework = byFramework,
                ByDomain = byDomain,
                ByStatus = byStatus,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<List<PackageComplianceScore>> GetComplianceByPackageAsync(Guid tenantId)
        {
            var packages = await _context.TenantPackages
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .ToListAsync();

            var requirements = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .Where(r => r.Assessment.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            return packages.Select(p =>
            {
                var pkgReqs = requirements.Where(r =>
                    r.Assessment?.TemplateCode == p.PackageCode).ToList();

                var compliant = pkgReqs.Count(r => r.Status == "Compliant");
                var partial = pkgReqs.Count(r => r.Status == "PartiallyCompliant");
                var nonCompliant = pkgReqs.Count(r => r.Status == "NonCompliant");
                var score = pkgReqs.Any() ? (decimal)pkgReqs.Average(r => r.Score ?? 0) : 0;

                return new PackageComplianceScore
                {
                    PackageCode = p.PackageCode,
                    PackageName = p.PackageName,
                    TotalRequirements = pkgReqs.Count,
                    Compliant = compliant,
                    Partial = partial,
                    NonCompliant = nonCompliant,
                    Score = score,
                    Status = score >= 90 ? "Compliant" : score >= 70 ? "Partial" : "NonCompliant"
                };
            }).ToList();
        }

        public async Task<List<ComplianceTrendPoint>> GetComplianceTrendAsync(Guid tenantId, int months = 12)
        {
            var trend = new List<ComplianceTrendPoint>();
            for (int i = months; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var requirements = await _context.AssessmentRequirements
                    .Include(r => r.Assessment)
                    .Where(r => r.Assessment.TenantId == tenantId &&
                        r.CreatedDate <= monthEnd && !r.IsDeleted)
                    .ToListAsync();

                trend.Add(new ComplianceTrendPoint
                {
                    Date = monthStart,
                    TotalRequirements = requirements.Count,
                    Compliant = requirements.Count(r => r.Status == "Compliant"),
                    Score = requirements.Any() ? (decimal)requirements.Average(r => r.Score ?? 0) : 0
                });
            }

            return trend;
        }

        #endregion

        #region Plan Progress

        public async Task<PlanProgressDashboard> GetPlanProgressAsync(Guid tenantId, Guid? planId = null)
        {
            var query = _context.Plans
                .Include(p => p.Phases)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted);

            if (planId.HasValue)
                query = query.Where(p => p.Id == planId.Value);

            var plans = await query.ToListAsync();

            // OPTIMIZED: Batch load all assessments for all plans in a single query
            var planIds = plans.Select(p => p.Id).ToList();
            var allAssessments = await _context.Assessments
                .Where(a => planIds.Contains(a.PlanId ?? Guid.Empty) && !a.IsDeleted)
                .ToListAsync();

            // Group assessments by PlanId for O(1) lookup
            var assessmentsByPlan = allAssessments
                .GroupBy(a => a.PlanId ?? Guid.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            var planSummaries = plans.Select(plan =>
            {
                var assessments = assessmentsByPlan.GetValueOrDefault(plan.Id, new List<Assessment>());
                var completedPhases = plan.Phases.Count(p => p.Status == "Completed");
                var completedAssessments = assessments.Count(a => a.Status == "Completed");
                var progress = plan.Phases.Any()
                    ? (decimal)completedPhases / plan.Phases.Count * 100
                    : 0;

                return new PlanSummary
                {
                    PlanId = plan.Id,
                    PlanCode = plan.PlanCode,
                    PlanName = plan.Name,
                    Status = plan.Status,
                    Progress = progress,
                    TotalPhases = plan.Phases.Count,
                    CompletedPhases = completedPhases,
                    TotalAssessments = assessments.Count,
                    CompletedAssessments = completedAssessments,
                    StartDate = plan.StartDate,
                    TargetEndDate = plan.TargetEndDate
                };
            }).ToList();

            return new PlanProgressDashboard
            {
                TotalPlans = plans.Count,
                ActivePlans = plans.Count(p => p.Status == "Active" || p.Status == "InProgress"),
                CompletedPlans = plans.Count(p => p.Status == "Completed"),
                OverallProgress = planSummaries.Any() ? planSummaries.Average(p => p.Progress) : 0,
                Plans = planSummaries,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<List<AssessmentProgress>> GetAssessmentProgressAsync(Guid planId)
        {
            var assessments = await _context.Assessments
                .Where(a => a.PlanId == planId && !a.IsDeleted)
                .ToListAsync();

            // OPTIMIZED: Batch load all requirements for all assessments in a single query
            var assessmentIds = assessments.Select(a => a.Id).ToList();
            var allRequirements = await _context.AssessmentRequirements
                .Where(r => assessmentIds.Contains(r.AssessmentId) && !r.IsDeleted)
                .ToListAsync();

            // Group requirements by AssessmentId for O(1) lookup
            var requirementsByAssessment = allRequirements
                .GroupBy(r => r.AssessmentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return assessments.Select(assessment =>
            {
                var requirements = requirementsByAssessment.GetValueOrDefault(assessment.Id, new List<AssessmentRequirement>());
                var completed = requirements.Count(r =>
                    r.Status == "Compliant" || r.Status == "NonCompliant");
                var progress = requirements.Any()
                    ? (decimal)completed / requirements.Count * 100
                    : 0;

                return new AssessmentProgress
                {
                    AssessmentId = assessment.Id,
                    AssessmentNumber = assessment.AssessmentNumber,
                    Name = assessment.Name,
                    Status = assessment.Status,
                    Progress = progress,
                    Score = assessment.Score,
                    TotalRequirements = requirements.Count,
                    CompletedRequirements = completed
                };
            }).ToList();
        }

        #endregion

        #region Task Dashboard

        public async Task<TaskDashboard> GetTaskDashboardAsync(Guid tenantId)
        {
            var tasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekFromNow = now.AddDays(7);

            var byRole = tasks
                .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                .GroupBy(t => t.AssignedToUserName ?? "Unassigned")
                .Select(g => new TasksByRole
                {
                    RoleCode = g.Key,
                    Count = g.Count(),
                    Overdue = g.Count(t => t.DueDate < now)
                })
                .ToList();

            var byPriority = tasks
                .Where(t => t.Status != "Completed" && t.Status != "Cancelled")
                .GroupBy(t => t.Priority)
                .Select(g => new TasksByPriority
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            return new TaskDashboard
            {
                TotalTasks = tasks.Count,
                OpenTasks = tasks.Count(t => t.Status == "Pending"),
                InProgressTasks = tasks.Count(t => t.Status == "InProgress"),
                CompletedTasks = tasks.Count(t => t.Status == "Completed"),
                OverdueTasks = tasks.Count(t => t.DueDate < now &&
                    t.Status != "Completed" && t.Status != "Cancelled"),
                DueThisWeek = tasks.Count(t => t.DueDate >= now && t.DueDate <= weekFromNow &&
                    t.Status != "Completed" && t.Status != "Cancelled"),
                ByRole = byRole,
                ByPriority = byPriority,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<List<OverdueTask>> GetOverdueTasksAsync(Guid tenantId, int limit = 20)
        {
            var now = DateTime.UtcNow;

            var tasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.WorkflowDefinition)
                .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                    t.DueDate < now &&
                    t.Status != "Completed" && t.Status != "Cancelled" &&
                    !t.IsDeleted)
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .ToListAsync();

            return tasks.Select(t => new OverdueTask
            {
                TaskId = t.Id,
                TaskName = t.TaskName,
                AssignedToRole = t.AssignedToUserName ?? "",
                AssignedToUser = t.AssignedToUserId?.ToString() ?? "",
                DueDate = t.DueDate ?? now,
                DaysOverdue = (int)(now - (t.DueDate ?? now)).TotalDays,
                Priority = t.Priority.ToString(),
                WorkflowName = t.WorkflowInstance.WorkflowDefinition.Name
            }).ToList();
        }

        public async Task<List<UpcomingTask>> GetUpcomingTasksAsync(Guid tenantId, int days = 7, int limit = 20)
        {
            var now = DateTime.UtcNow;
            var endDate = now.AddDays(days);

            var tasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                    t.DueDate >= now && t.DueDate <= endDate &&
                    t.Status != "Completed" && t.Status != "Cancelled" &&
                    !t.IsDeleted)
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .ToListAsync();

            return tasks.Select(t => new UpcomingTask
            {
                TaskId = t.Id,
                TaskName = t.TaskName,
                AssignedToRole = t.AssignedToUserName ?? "",
                DueDate = t.DueDate ?? now,
                DaysUntilDue = (int)((t.DueDate ?? now) - now).TotalDays,
                Priority = t.Priority.ToString()
            }).ToList();
        }

        #endregion

        #region Risk Dashboard

        public async Task<RiskDashboard> GetRiskDashboardAsync(Guid tenantId)
        {
            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var byCategory = risks
                .GroupBy(r => r.Category ?? "Uncategorized")
                .Select(g => new RiskByCategory
                {
                    Category = g.Key,
                    Count = g.Count(),
                    High = g.Count(r => r.RiskScore >= 8),
                    Medium = g.Count(r => r.RiskScore >= 4 && r.RiskScore < 8),
                    Low = g.Count(r => r.RiskScore < 4)
                })
                .ToList();

            var topRisks = risks
                .OrderByDescending(r => r.RiskScore)
                .Take(10)
                .Select(r => new TopRisk
                {
                    RiskId = r.Id,
                    Name = r.Name,
                    Category = r.Category ?? "",
                    Severity = r.Impact,
                    Likelihood = r.Likelihood,
                    RiskScore = r.RiskScore,
                    Status = r.Status
                })
                .ToList();

            return new RiskDashboard
            {
                TotalRisks = risks.Count,
                HighRisks = risks.Count(r => r.RiskScore >= 8),
                MediumRisks = risks.Count(r => r.RiskScore >= 4 && r.RiskScore < 8),
                LowRisks = risks.Count(r => r.RiskScore < 4),
                OpenRisks = risks.Count(r => r.Status == "Open" || r.Status == "Identified"),
                MitigatedRisks = risks.Count(r => r.Status == "Mitigated" || r.Status == "Closed"),
                ByCategory = byCategory,
                TopRisks = topRisks,
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Top Actions

        public async Task<List<NextAction>> GetTopNextActionsAsync(Guid tenantId, int limit = 10)
        {
            var actions = new List<NextAction>();
            var now = DateTime.UtcNow;

            // 1. Overdue tasks (highest priority)
            var overdueTasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                    t.DueDate < now &&
                    t.Status != "Completed" && t.Status != "Cancelled" &&
                    !t.IsDeleted)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .ToListAsync();

            actions.AddRange(overdueTasks.Select((t, i) => new NextAction
            {
                Priority = i + 1,
                ActionType = "OverdueTask",
                Title = $"Complete overdue task: {t.TaskName}",
                Description = $"Task is {(int)(now - (t.DueDate ?? now)).TotalDays} days overdue",
                EntityId = t.Id,
                EntityType = "WorkflowTask",
                DueDate = t.DueDate,
                AssignedTo = t.AssignedToUserName ?? "",
                Urgency = "Critical"
            }));

            // 2. High-risk items
            var highRisks = await _context.Risks
                .Where(r => r.TenantId == tenantId &&
                    r.RiskScore >= 8 &&
                    r.Status != "Mitigated" && r.Status != "Closed" &&
                    !r.IsDeleted)
                .OrderByDescending(r => r.RiskScore)
                .Take(3)
                .ToListAsync();

            actions.AddRange(highRisks.Select((r, i) => new NextAction
            {
                Priority = actions.Count + i + 1,
                ActionType = "HighRisk",
                Title = $"Address high risk: {r.Name}",
                Description = $"Risk score: {r.RiskScore}",
                EntityId = r.Id,
                EntityType = "Risk",
                DueDate = null,
                AssignedTo = r.Owner ?? "",
                Urgency = "High"
            }));

            // 3. Non-compliant requirements
            var nonCompliant = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .Where(r => r.Assessment.TenantId == tenantId &&
                    r.Status == "NonCompliant" &&
                    !r.IsDeleted)
                .Take(5)
                .ToListAsync();

            actions.AddRange(nonCompliant.Select((r, i) => new NextAction
            {
                Priority = actions.Count + i + 1,
                ActionType = "NonCompliantRequirement",
                Title = $"Remediate: {r.ControlNumber} - {r.ControlTitle}",
                Description = $"Current score: {r.Score ?? 0}%",
                EntityId = r.Id,
                EntityType = "AssessmentRequirement",
                DueDate = null,
                AssignedTo = "",
                Urgency = "Medium"
            }));

            return actions.Take(limit).ToList();
        }

        #endregion

        #region Drill-Down

        public async Task<PackageDrillDown> DrillDownPackageAsync(Guid tenantId, string packageCode)
        {
            var package = await _context.TenantPackages
                .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                    p.PackageCode == packageCode && !p.IsDeleted);

            if (package == null)
                throw new InvalidOperationException("Package not found.");

            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId &&
                    a.TemplateCode == packageCode && !a.IsDeleted)
                .ToListAsync();

            var assessmentSummaries = new List<AssessmentSummary>();
            foreach (var assessment in assessments)
            {
                var requirements = await _context.AssessmentRequirements
                    .Where(r => r.AssessmentId == assessment.Id && !r.IsDeleted)
                    .ToListAsync();

                assessmentSummaries.Add(new AssessmentSummary
                {
                    AssessmentId = assessment.Id,
                    AssessmentNumber = assessment.AssessmentNumber,
                    Name = assessment.Name,
                    Score = assessment.Score,
                    Status = assessment.Status,
                    TotalRequirements = requirements.Count,
                    Compliant = requirements.Count(r => r.Status == "Compliant")
                });
            }

            return new PackageDrillDown
            {
                PackageCode = package.PackageCode,
                PackageName = package.PackageName,
                Score = assessmentSummaries.Any() ? (decimal)assessmentSummaries.Average(a => a.Score) : 0,
                Assessments = assessmentSummaries
            };
        }

        public async Task<AssessmentDrillDown> DrillDownAssessmentAsync(Guid assessmentId)
        {
            var assessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDeleted);

            if (assessment == null)
                throw new EntityNotFoundException("Assessment", assessmentId);

            var requirements = await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && !r.IsDeleted)
                .ToListAsync();

            // OPTIMIZED: Single count query instead of N queries in loop
            var evidenceCount = await _context.Evidences
                .CountAsync(e => e.AssessmentId == assessmentId && !e.IsDeleted);

            var requirementSummaries = requirements.Select(req => new RequirementSummary
            {
                RequirementId = req.Id,
                ControlNumber = req.ControlNumber,
                ControlTitle = req.ControlTitle,
                Domain = req.Domain,
                Status = req.Status ?? "NotStarted",
                Score = req.Score ?? 0,
                EvidenceCount = evidenceCount
            }).ToList();

            return new AssessmentDrillDown
            {
                AssessmentId = assessment.Id,
                AssessmentNumber = assessment.AssessmentNumber,
                Name = assessment.Name,
                Score = assessment.Score,
                Requirements = requirementSummaries.OrderBy(r => r.ControlNumber).ToList()
            };
        }

        public async Task<RequirementDrillDown> DrillDownRequirementAsync(Guid requirementId)
        {
            var requirement = await _context.AssessmentRequirements
                .FirstOrDefaultAsync(r => r.Id == requirementId && !r.IsDeleted);

            if (requirement == null)
                throw new EntityNotFoundException("AssessmentRequirement", requirementId);

            var evidences = await _context.Evidences
                .Where(e => e.AssessmentId == requirement.AssessmentId && !e.IsDeleted)
                .ToListAsync();

            // OPTIMIZED: Batch load all scores in a single query instead of N queries in loop
            var evidenceIds = evidences.Select(e => e.Id).ToList();
            var scores = await _context.EvidenceScores
                .Where(s => evidenceIds.Contains(s.EvidenceId) && s.IsFinal)
                .ToDictionaryAsync(s => s.EvidenceId, s => (int?)s.Score);

            var evidenceSummaries = evidences.Select(evidence => new EvidenceSummary
            {
                EvidenceId = evidence.Id,
                EvidenceNumber = evidence.EvidenceNumber,
                Title = evidence.Title,
                Type = evidence.Type,
                Status = evidence.VerificationStatus,
                Score = scores.GetValueOrDefault(evidence.Id),
                CollectionDate = evidence.CollectionDate
            }).ToList();

            return new RequirementDrillDown
            {
                RequirementId = requirement.Id,
                ControlNumber = requirement.ControlNumber,
                ControlTitle = requirement.ControlTitle,
                Status = requirement.Status ?? "NotStarted",
                Score = requirement.Score ?? 0,
                Evidences = evidenceSummaries.OrderByDescending(e => e.CollectionDate).ToList()
            };
        }

        #endregion

        #region Audit Pack

        public async Task<AuditPackData> GenerateAuditPackAsync(Guid tenantId, Guid? assessmentId = null)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            var executiveSummary = await GetExecutiveDashboardAsync(tenantId);

            var assessmentsQuery = _context.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted);

            if (assessmentId.HasValue)
                assessmentsQuery = assessmentsQuery.Where(a => a.Id == assessmentId.Value);

            var assessments = await assessmentsQuery.ToListAsync();
            var assessmentIds = assessments.Select(a => a.Id).ToList();

            // OPTIMIZED: Batch load all requirements for all assessments in a single query
            var allRequirements = await _context.AssessmentRequirements
                .Where(r => assessmentIds.Contains(r.AssessmentId) && !r.IsDeleted)
                .ToListAsync();
            var requirementsByAssessment = allRequirements
                .GroupBy(r => r.AssessmentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // OPTIMIZED: Batch load all evidences for all assessments in a single query
            var allEvidencesFromDb = await _context.Evidences
                .Where(e => assessmentIds.Contains(e.AssessmentId ?? Guid.Empty) && !e.IsDeleted)
                .ToListAsync();
            var evidencesByAssessment = allEvidencesFromDb
                .GroupBy(e => e.AssessmentId ?? Guid.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            // OPTIMIZED: Batch load all evidence scores in a single query
            var allEvidenceIds = allEvidencesFromDb.Select(e => e.Id).ToList();
            var allScores = await _context.EvidenceScores
                .Where(s => allEvidenceIds.Contains(s.EvidenceId) && s.IsFinal)
                .ToDictionaryAsync(s => s.EvidenceId, s => (int?)s.Score);

            var assessmentAuditData = new List<AssessmentAuditData>();
            var allEvidences = new List<EvidenceAuditData>();
            var processedEvidenceIds = new HashSet<Guid>();

            foreach (var assessment in assessments)
            {
                var requirements = requirementsByAssessment.GetValueOrDefault(assessment.Id, new List<AssessmentRequirement>());
                var evidences = evidencesByAssessment.GetValueOrDefault(assessment.Id, new List<Evidence>());

                var requirementAuditData = requirements.Select(req => new RequirementAuditData
                {
                    ControlNumber = req.ControlNumber,
                    ControlTitle = req.ControlTitle,
                    Status = req.Status ?? "NotStarted",
                    Score = req.Score ?? 0,
                    EvidenceIds = string.Join(",", evidences.Select(e => e.EvidenceNumber))
                }).ToList();

                // Add evidences only once (avoid duplicates across requirements)
                foreach (var evidence in evidences)
                {
                    if (!processedEvidenceIds.Contains(evidence.Id))
                    {
                        processedEvidenceIds.Add(evidence.Id);
                        allEvidences.Add(new EvidenceAuditData
                        {
                            EvidenceId = evidence.Id,
                            EvidenceNumber = evidence.EvidenceNumber,
                            Title = evidence.Title,
                            Type = evidence.Type,
                            FileName = evidence.FileName,
                            Status = evidence.VerificationStatus,
                            Score = allScores.GetValueOrDefault(evidence.Id),
                            CollectionDate = evidence.CollectionDate,
                            CollectedBy = evidence.CollectedBy,
                            VerifiedBy = evidence.VerifiedBy,
                            VerificationDate = evidence.VerificationDate
                        });
                    }
                }

                assessmentAuditData.Add(new AssessmentAuditData
                {
                    AssessmentId = assessment.Id,
                    AssessmentNumber = assessment.AssessmentNumber,
                    Name = assessment.Name,
                    Framework = assessment.FrameworkCode ?? "",
                    Score = assessment.Score,
                    Status = assessment.Status,
                    CompletedDate = assessment.EndDate,
                    Requirements = requirementAuditData
                });
            }

            return new AuditPackData
            {
                TenantId = tenantId,
                OrganizationName = tenant.OrganizationName,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = "System",
                ExecutiveSummary = executiveSummary,
                Assessments = assessmentAuditData,
                Evidences = allEvidences
            };
        }

        #endregion
    }
}
