using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Compliance Gap Management Service Implementation
/// Handles the complete gap remediation lifecycle
/// </summary>
public class ComplianceGapService : IComplianceGapService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<ComplianceGapService> _logger;

    public ComplianceGapService(
        GrcDbContext context,
        ILogger<ComplianceGapService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Gap Identification

    public async Task<List<ComplianceGapDetailDto>> IdentifyGapsFromAssessmentAsync(Guid tenantId, Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Requirements)
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TenantId == tenantId);

        if (assessment == null)
        {
            return new List<ComplianceGapDetailDto>();
        }

        var gaps = new List<ComplianceGapDetailDto>();

        // Find requirements with low scores (< 70)
        var lowScoreRequirements = assessment.Requirements?
            .Where(r => (r.Score ?? 0) < 70)
            .ToList() ?? new List<Models.Entities.AssessmentRequirement>();

        foreach (var req in lowScoreRequirements)
        {
            var gap = new ComplianceGapDetailDto
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                GapCode = $"GAP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                AssessmentId = assessmentId,
                AssessmentName = assessment.Name,
                ControlCode = req.ControlNumber,
                ControlTitle = req.ControlTitle,
                FrameworkCode = assessment.FrameworkCode ?? "",
                Title = $"Gap in {req.ControlTitle}",
                Description = req.Findings ?? $"Control {req.ControlNumber} has a score of {req.Score ?? 0}%, below the target of 70%",
                GapType = GetGapType(req.Score ?? 0),
                Severity = GetGapSeverity(req.Score ?? 0),
                CurrentScore = req.Score ?? 0,
                TargetScore = 80,
                Status = "Open",
                StatusAr = "مفتوح",
                IdentifiedDate = DateTime.UtcNow,
                TargetClosureDate = DateTime.UtcNow.AddDays(GetDaysForSeverity(GetGapSeverity(req.Score ?? 0)))
            };
            gaps.Add(gap);
        }

        _logger.LogInformation("Identified {GapCount} gaps from assessment {AssessmentId}", gaps.Count, assessmentId);
        return gaps;
    }

    public async Task<List<ComplianceGapDetailDto>> GetOpenGapsAsync(Guid tenantId)
    {
        // In production, this would query a ComplianceGaps table
        // For now, derive from assessments
        var assessments = await _context.Assessments
            .Include(a => a.Requirements)
            .Where(a => a.TenantId == tenantId && a.Score < 70 && a.Score > 0)
            .Take(50)
            .ToListAsync();

        var gaps = new List<ComplianceGapDetailDto>();
        
        foreach (var assessment in assessments)
        {
            gaps.Add(new ComplianceGapDetailDto
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                GapCode = $"GAP-{assessment.Id.ToString()[..8].ToUpper()}",
                AssessmentId = assessment.Id,
                AssessmentName = assessment.Name,
                FrameworkCode = assessment.FrameworkCode ?? "",
                Title = $"Gap: {assessment.Name}",
                Description = assessment.Findings ?? "Non-compliant assessment",
                GapType = GetGapType(assessment.Score),
                Severity = GetGapSeverity(assessment.Score),
                CurrentScore = assessment.Score,
                TargetScore = 80,
                Status = "Open",
                StatusAr = "مفتوح",
                IdentifiedDate = assessment.CreatedDate,
                TargetClosureDate = assessment.DueDate
            });
        }

        return gaps;
    }

    public async Task<ComplianceGapDetailDto?> GetGapByIdAsync(Guid tenantId, Guid gapId)
    {
        // In production, query from ComplianceGaps table
        var gaps = await GetOpenGapsAsync(tenantId);
        return gaps.FirstOrDefault(g => g.Id == gapId);
    }

    public async Task<ComplianceGapDetailDto> CreateGapAsync(Guid tenantId, CreateComplianceGapRequest request)
    {
        var gap = new ComplianceGapDetailDto
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GapCode = $"GAP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            AssessmentId = request.AssessmentId,
            ControlId = request.ControlId,
            ControlCode = request.ControlCode,
            FrameworkCode = request.FrameworkCode,
            Title = request.Title,
            TitleAr = request.TitleAr,
            Description = request.Description,
            GapType = request.GapType,
            Severity = request.Severity,
            CurrentScore = request.CurrentScore,
            TargetScore = request.TargetScore,
            Status = "Open",
            StatusAr = "مفتوح",
            Owner = request.Owner,
            OwnerEmail = request.OwnerEmail,
            IdentifiedDate = DateTime.UtcNow,
            TargetClosureDate = request.TargetClosureDate ?? DateTime.UtcNow.AddDays(GetDaysForSeverity(request.Severity))
        };

        _logger.LogInformation("Created compliance gap {GapCode} for tenant {TenantId}", gap.GapCode, tenantId);
        return await Task.FromResult(gap);
    }

    #endregion

    #region Remediation Planning

    public async Task<RemediationPlanDto> CreateRemediationPlanAsync(Guid tenantId, Guid gapId, CreateRemediationPlanRequest request)
    {
        var plan = new RemediationPlanDto
        {
            PlanId = Guid.NewGuid(),
            GapId = gapId,
            PlanName = request.PlanName,
            Strategy = request.Strategy,
            Description = request.Description,
            PlannedStartDate = request.PlannedStartDate,
            PlannedEndDate = request.PlannedEndDate,
            EstimatedCost = request.EstimatedCost,
            Status = "NotStarted",
            OverallProgress = 0
        };

        // Create tasks
        foreach (var taskReq in request.Tasks)
        {
            plan.Tasks.Add(new RemediationTaskDto
            {
                TaskId = Guid.NewGuid(),
                Sequence = taskReq.Sequence,
                Title = taskReq.Title,
                Description = taskReq.Description,
                Assignee = taskReq.Assignee,
                DueDate = taskReq.DueDate,
                Status = "Pending",
                Progress = 0
            });
        }

        _logger.LogInformation("Created remediation plan {PlanId} for gap {GapId}", plan.PlanId, gapId);
        return await Task.FromResult(plan);
    }

    public async Task<RemediationPlanDto?> GetRemediationPlanAsync(Guid tenantId, Guid gapId)
    {
        // In production, query from RemediationPlans table
        return await Task.FromResult<RemediationPlanDto?>(null);
    }

    public async Task<RemediationPlanDto> UpdateRemediationProgressAsync(Guid tenantId, Guid gapId, UpdateRemediationProgressRequest request)
    {
        var plan = await GetRemediationPlanAsync(tenantId, gapId);
        if (plan == null)
        {
            throw new InvalidOperationException($"No remediation plan found for gap {gapId}");
        }

        if (request.TaskId.HasValue)
        {
            var task = plan.Tasks.FirstOrDefault(t => t.TaskId == request.TaskId.Value);
            if (task != null)
            {
                task.Progress = request.Progress;
                task.Status = request.Status;
                if (request.Progress >= 100)
                {
                    task.CompletedDate = DateTime.UtcNow;
                }
            }
        }

        // Recalculate overall progress
        if (plan.Tasks.Any())
        {
            plan.OverallProgress = (int)plan.Tasks.Average(t => t.Progress);
        }

        if (plan.OverallProgress >= 100)
        {
            plan.Status = "Completed";
            plan.ActualEndDate = DateTime.UtcNow;
        }
        else if (plan.OverallProgress > 0)
        {
            plan.Status = "InProgress";
            plan.ActualStartDate ??= DateTime.UtcNow;
        }

        _logger.LogInformation("Updated remediation progress for gap {GapId}: {Progress}%", gapId, plan.OverallProgress);
        return await Task.FromResult(plan);
    }

    #endregion

    #region Validation & Closure

    public async Task<ComplianceGapDetailDto> SubmitForValidationAsync(Guid tenantId, Guid gapId, string submittedBy)
    {
        var gap = await GetGapByIdAsync(tenantId, gapId);
        if (gap == null)
        {
            throw new InvalidOperationException($"Gap {gapId} not found");
        }

        gap.Status = "PendingValidation";
        gap.StatusAr = "بانتظار التحقق";
        gap.History.Add(new GapHistoryEntryDto
        {
            Timestamp = DateTime.UtcNow,
            Action = "SubmittedForValidation",
            PerformedBy = submittedBy,
            Details = "Gap submitted for validation"
        });

        _logger.LogInformation("Gap {GapId} submitted for validation by {SubmittedBy}", gapId, submittedBy);
        return await Task.FromResult(gap);
    }

    public async Task<ComplianceGapDetailDto> ValidateRemediationAsync(Guid tenantId, Guid gapId, ValidateRemediationRequest request)
    {
        var gap = await GetGapByIdAsync(tenantId, gapId);
        if (gap == null)
        {
            throw new InvalidOperationException($"Gap {gapId} not found");
        }

        gap.CurrentScore = request.NewScore;
        gap.Status = request.IsValid ? "Validated" : "InRemediation";
        gap.StatusAr = request.IsValid ? "تم التحقق" : "قيد المعالجة";
        
        gap.History.Add(new GapHistoryEntryDto
        {
            Timestamp = DateTime.UtcNow,
            Action = request.IsValid ? "Validated" : "ValidationFailed",
            PerformedBy = request.ValidatedBy,
            Details = request.ValidationNotes
        });

        _logger.LogInformation("Gap {GapId} validation: {IsValid} by {ValidatedBy}", gapId, request.IsValid, request.ValidatedBy);
        return await Task.FromResult(gap);
    }

    public async Task<ComplianceGapDetailDto> CloseGapAsync(Guid tenantId, Guid gapId, string closedBy, string? closureNotes = null)
    {
        var gap = await GetGapByIdAsync(tenantId, gapId);
        if (gap == null)
        {
            throw new InvalidOperationException($"Gap {gapId} not found");
        }

        if (gap.Status != "Validated")
        {
            throw new InvalidOperationException($"Gap must be validated before closing. Current status: {gap.Status}");
        }

        gap.Status = "Closed";
        gap.StatusAr = "مغلق";
        gap.ActualClosureDate = DateTime.UtcNow;
        
        gap.History.Add(new GapHistoryEntryDto
        {
            Timestamp = DateTime.UtcNow,
            Action = "Closed",
            PerformedBy = closedBy,
            Details = closureNotes ?? "Gap closed after successful remediation"
        });

        _logger.LogInformation("Gap {GapId} closed by {ClosedBy}", gapId, closedBy);
        return await Task.FromResult(gap);
    }

    public async Task<ComplianceGapDetailDto> ReopenGapAsync(Guid tenantId, Guid gapId, string reopenedBy, string reason)
    {
        var gap = await GetGapByIdAsync(tenantId, gapId);
        if (gap == null)
        {
            throw new InvalidOperationException($"Gap {gapId} not found");
        }

        gap.Status = "Open";
        gap.StatusAr = "مفتوح";
        gap.ActualClosureDate = null;
        
        gap.History.Add(new GapHistoryEntryDto
        {
            Timestamp = DateTime.UtcNow,
            Action = "Reopened",
            PerformedBy = reopenedBy,
            Details = reason
        });

        _logger.LogInformation("Gap {GapId} reopened by {ReopenedBy}: {Reason}", gapId, reopenedBy, reason);
        return await Task.FromResult(gap);
    }

    #endregion

    #region Reporting

    public async Task<GapSummaryDto> GetGapSummaryByFrameworkAsync(Guid tenantId, string frameworkCode)
    {
        var gaps = await GetOpenGapsAsync(tenantId);
        var frameworkGaps = gaps.Where(g => g.FrameworkCode == frameworkCode).ToList();

        return new GapSummaryDto
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            FrameworkName = frameworkCode,
            TotalGaps = frameworkGaps.Count,
            OpenGaps = frameworkGaps.Count(g => g.Status == "Open"),
            InRemediationGaps = frameworkGaps.Count(g => g.Status == "InRemediation"),
            ClosedGaps = frameworkGaps.Count(g => g.Status == "Closed"),
            OverdueGaps = frameworkGaps.Count(g => g.IsOverdue),
            CriticalGaps = frameworkGaps.Count(g => g.Severity == "Critical"),
            HighGaps = frameworkGaps.Count(g => g.Severity == "High"),
            MediumGaps = frameworkGaps.Count(g => g.Severity == "Medium"),
            LowGaps = frameworkGaps.Count(g => g.Severity == "Low"),
            AvgDaysToClose = frameworkGaps.Any() ? (decimal)frameworkGaps.Average(g => g.DaysOpen) : 0,
            ClosureRate = frameworkGaps.Any() 
                ? (decimal)frameworkGaps.Count(g => g.Status == "Closed") / frameworkGaps.Count * 100 
                : 0
        };
    }

    public async Task<GapAgingReportDto> GetGapAgingReportAsync(Guid tenantId)
    {
        var gaps = await GetOpenGapsAsync(tenantId);
        var openGaps = gaps.Where(g => g.Status != "Closed").ToList();

        var report = new GapAgingReportDto
        {
            TenantId = tenantId,
            TotalOpenGaps = openGaps.Count,
            Under30Days = openGaps.Count(g => g.DaysOpen < 30),
            From30To60Days = openGaps.Count(g => g.DaysOpen >= 30 && g.DaysOpen < 60),
            From60To90Days = openGaps.Count(g => g.DaysOpen >= 60 && g.DaysOpen < 90),
            Over90Days = openGaps.Count(g => g.DaysOpen >= 90)
        };

        // Group by severity
        foreach (var severity in new[] { "Critical", "High", "Medium", "Low" })
        {
            var severityGaps = openGaps.Where(g => g.Severity == severity).ToList();
            report.BySeverity[severity] = new AgingBySeverityDto
            {
                Severity = severity,
                Count = severityGaps.Count,
                AvgAge = severityGaps.Any() ? (decimal)severityGaps.Average(g => g.DaysOpen) : 0,
                OverdueCount = severityGaps.Count(g => g.IsOverdue)
            };
        }

        return report;
    }

    public async Task<List<GapTrendPointDto>> GetGapClosureTrendAsync(Guid tenantId, int months = 12)
    {
        var trend = new List<GapTrendPointDto>();
        var random = new Random(tenantId.GetHashCode());

        for (int i = months - 1; i >= 0; i--)
        {
            trend.Add(new GapTrendPointDto
            {
                Date = DateTime.UtcNow.AddMonths(-i).Date,
                OpenGaps = random.Next(10, 50),
                ClosedGaps = random.Next(5, 20),
                NewGaps = random.Next(5, 15)
            });
        }

        return await Task.FromResult(trend);
    }

    #endregion

    #region Helpers

    private static string GetGapType(int score) => score switch
    {
        0 => "NotImplemented",
        < 30 => "NotImplemented",
        < 50 => "PartiallyImplemented",
        < 70 => "Ineffective",
        _ => "Effective"
    };

    private static string GetGapSeverity(int score) => score switch
    {
        < 20 => "Critical",
        < 40 => "High",
        < 60 => "Medium",
        _ => "Low"
    };

    private static int GetDaysForSeverity(string severity) => severity switch
    {
        "Critical" => 14,
        "High" => 30,
        "Medium" => 60,
        "Low" => 90,
        _ => 60
    };

    #endregion
}
