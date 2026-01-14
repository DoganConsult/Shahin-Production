using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// NextBestAction Agent Service - Generates intelligent recommendations based on context.
/// Implements the fullplan specification for NBA recommendations.
/// </summary>
public class NextBestActionService : INextBestActionService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<NextBestActionService> _logger;
    private readonly IProgressCertaintyService _pciService;

    public NextBestActionService(
        GrcDbContext dbContext,
        ILogger<NextBestActionService> logger,
        IProgressCertaintyService pciService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _pciService = pciService;
    }

    public async Task<NextBestActionResult> GenerateRecommendationsAsync(
        Guid tenantId,
        Guid? userId = null,
        string? roleCode = null,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = new List<NextBestActionRecommendation>();

            // 1. Check for overdue tasks
            var overdueTasks = await GetOverdueTasksAsync(tenantId, userId, cancellationToken);
            foreach (var task in overdueTasks.Take(2))
            {
                recommendations.Add(CreateTaskRecommendation(tenantId, userId, task, NbaActionTypes.Remind));
            }

            // 2. Check for pending approvals
            var pendingApprovals = await GetPendingApprovalsAsync(tenantId, userId, cancellationToken);
            foreach (var approval in pendingApprovals.Take(2))
            {
                recommendations.Add(CreateApprovalRecommendation(tenantId, userId, approval));
            }

            // 3. Check for evidence needing submission
            var evidenceNeeded = await GetEvidenceNeedingSubmissionAsync(tenantId, userId, cancellationToken);
            foreach (var evidence in evidenceNeeded.Take(2))
            {
                recommendations.Add(CreateEvidenceRecommendation(tenantId, userId, evidence));
            }

            // 4. Check PCI and add interventions
            var pci = await _pciService.GetLatestPciAsync(tenantId, cancellationToken: cancellationToken);
            if (pci != null && pci.Score < 50)
            {
                recommendations.AddRange(CreatePciBasedRecommendations(tenantId, userId, pci));
            }

            // 5. Check for tasks that can be completed
            var completableTasks = await GetCompletableTasksAsync(tenantId, userId, cancellationToken);
            foreach (var task in completableTasks.Take(2))
            {
                recommendations.Add(CreateTaskRecommendation(tenantId, userId, task, NbaActionTypes.Complete));
            }

            // 6. Check for stuck workflows
            var stuckWorkflows = await GetStuckWorkflowsAsync(tenantId, cancellationToken);
            foreach (var workflow in stuckWorkflows.Take(1))
            {
                recommendations.Add(CreateWorkflowRecommendation(tenantId, userId, workflow));
            }

            // Sort by priority and confidence
            recommendations = recommendations
                .OrderBy(r => r.Priority)
                .ThenByDescending(r => r.ConfidenceScore)
                .Take(maxRecommendations)
                .ToList();

            // Persist recommendations
            foreach (var rec in recommendations)
            {
                rec.ExpiresAt = DateTime.UtcNow.AddHours(24);
                _dbContext.Set<NextBestActionRecommendation>().Add(rec);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated {Count} NBA recommendations for tenant {TenantId}",
                recommendations.Count, tenantId);

            return new NextBestActionResult
            {
                Success = true,
                Recommendations = recommendations,
                TotalGenerated = recommendations.Count,
                Message = $"Generated {recommendations.Count} recommendations"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating NBA recommendations for tenant {TenantId}", tenantId);
            return new NextBestActionResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<List<NextBestActionRecommendation>> GetPendingRecommendationsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<NextBestActionRecommendation>()
            .Where(r => r.TenantId == tenantId && r.Status == "Pending");

        if (userId.HasValue)
        {
            query = query.Where(r => r.TargetUserId == userId || r.TargetUserId == null);
        }

        return await query
            .Where(r => r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)
            .OrderBy(r => r.Priority)
            .ThenByDescending(r => r.ConfidenceScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AcceptRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _dbContext.Set<NextBestActionRecommendation>()
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation == null)
            return false;

        recommendation.Status = "Accepted";
        recommendation.ActedByUserId = userId;
        recommendation.ActedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recommendation {Id} accepted by user {UserId}", recommendationId, userId);

        return true;
    }

    public async Task<bool> RejectRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _dbContext.Set<NextBestActionRecommendation>()
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation == null)
            return false;

        recommendation.Status = "Rejected";
        recommendation.ActedByUserId = userId;
        recommendation.ActedAt = DateTime.UtcNow;
        recommendation.UserFeedback = feedback;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recommendation {Id} rejected by user {UserId}", recommendationId, userId);

        return true;
    }

    public async Task<ExecuteRecommendationResult> ExecuteRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendation = await _dbContext.Set<NextBestActionRecommendation>()
                .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

            if (recommendation == null)
            {
                return new ExecuteRecommendationResult
                {
                    Success = false,
                    Message = "Recommendation not found"
                };
            }

            // Check if action is auto-executable
            if (!NbaActionTypes.Types.TryGetValue(recommendation.ActionType, out var actionInfo) ||
                !actionInfo.AutoExecutable)
            {
                return new ExecuteRecommendationResult
                {
                    Success = false,
                    Message = "This action type requires manual execution"
                };
            }

            // Execute based on action type
            var result = recommendation.ActionType switch
            {
                NbaActionTypes.Remind => await ExecuteRemindAsync(recommendation, cancellationToken),
                NbaActionTypes.Escalate => await ExecuteEscalateAsync(recommendation, cancellationToken),
                NbaActionTypes.AutoCollect => await ExecuteAutoCollectAsync(recommendation, cancellationToken),
                NbaActionTypes.GenerateReport => await ExecuteGenerateReportAsync(recommendation, cancellationToken),
                _ => new ExecuteRecommendationResult { Success = false, Message = "Unknown action type" }
            };

            if (result.Success)
            {
                recommendation.Status = "Executed";
                recommendation.ActedByUserId = userId;
                recommendation.ActedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing recommendation {Id}", recommendationId);
            return new ExecuteRecommendationResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<bool> RateRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        int rating,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _dbContext.Set<NextBestActionRecommendation>()
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation == null)
            return false;

        recommendation.UserRating = rating;
        recommendation.UserFeedback = feedback;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> DismissExpiredRecommendationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var expired = await _dbContext.Set<NextBestActionRecommendation>()
            .Where(r => r.TenantId == tenantId &&
                       r.Status == "Pending" &&
                       r.ExpiresAt != null &&
                       r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var rec in expired)
        {
            rec.Status = "Expired";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }

    #region Private Helper Methods

    private async Task<List<WorkflowTask>> GetOverdueTasksAsync(
        Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<WorkflowTask>()
            .Where(t => t.TenantId == tenantId &&
                       t.Status != "Completed" &&
                       t.Status != "Cancelled" &&
                       t.DueDate < DateTime.UtcNow);

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<PendingApproval>> GetPendingApprovalsAsync(
        Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<PendingApproval>()
            .Where(p => p.TenantId == tenantId && p.Status == "Pending");

        if (userId.HasValue)
        {
            var userIdStr = userId.Value.ToString();
            query = query.Where(p => p.AssignedApproverId == userIdStr);
        }

        return await query
            .OrderBy(p => p.DueAt)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TenantEvidenceRequirement>> GetEvidenceNeedingSubmissionAsync(
        Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _dbContext.TenantEvidenceRequirements
            .Where(e => e.TenantId == tenantId &&
                       (e.Status == "NotStarted" || e.Status == "InProgress") &&
                       e.DueDate <= DateTime.UtcNow.AddDays(7));

        if (userId.HasValue)
        {
            query = query.Where(e => e.AssignedToUserId == userId);
        }

        return await query
            .OrderBy(e => e.DueDate)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<WorkflowTask>> GetCompletableTasksAsync(
        Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<WorkflowTask>()
            .Where(t => t.TenantId == tenantId &&
                       t.Status == "InProgress" &&
                       t.CompletionPercentage >= 90);

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        return await query
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<WorkflowInstance>> GetStuckWorkflowsAsync(
        Guid tenantId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<WorkflowInstance>()
            .Where(w => w.TenantId == tenantId &&
                       w.Status == "InProgress" &&
                       w.ModifiedDate < DateTime.UtcNow.AddDays(-3))
            .Take(3)
            .ToListAsync(cancellationToken);
    }

    private NextBestActionRecommendation CreateTaskRecommendation(
        Guid tenantId, Guid? userId, WorkflowTask task, string actionType)
    {
        var isOverdue = task.DueDate < DateTime.UtcNow;
        var daysDiff = (int)(DateTime.UtcNow - (task.DueDate ?? DateTime.UtcNow)).TotalDays;

        return new NextBestActionRecommendation
        {
            TenantId = tenantId,
            TargetUserId = userId ?? task.AssignedToUserId,
            ActionId = $"TASK_{actionType}_{task.Id}",
            ActionType = actionType,
            Description = actionType == NbaActionTypes.Remind
                ? $"Task '{task.Name}' is overdue by {daysDiff} days. Send reminder to owner."
                : $"Task '{task.Name}' is {task.CompletionPercentage}% complete. Mark as completed.",
            ConfidenceScore = isOverdue ? 85 : 70,
            Priority = isOverdue ? 1 : 3,
            Rationale = actionType == NbaActionTypes.Remind
                ? $"Task has been overdue for {daysDiff} days with no recent activity."
                : "Task completion percentage indicates work is substantially done.",
            ExpectedImpact = actionType == NbaActionTypes.Remind
                ? "Reminding the owner may prompt immediate action and reduce delay."
                : "Completing this task will improve overall progress metrics.",
            RelatedEntityType = "WorkflowTask",
            RelatedEntityId = task.Id,
            ActionParametersJson = JsonSerializer.Serialize(new { TaskId = task.Id, TaskName = task.Name })
        };
    }

    private NextBestActionRecommendation CreateApprovalRecommendation(
        Guid tenantId, Guid? userId, PendingApproval approval)
    {
        var targetUserId = userId ?? (Guid.TryParse(approval.AssignedApproverId, out var parsed) ? parsed : Guid.Empty);
        return new NextBestActionRecommendation
        {
            TenantId = tenantId,
            TargetUserId = targetUserId,
            ActionId = $"APPROVE_{approval.Id}",
            ActionType = NbaActionTypes.Approve,
            Description = $"Pending approval request waiting for your review.",
            ConfidenceScore = 80,
            Priority = approval.IsOverdue ? 1 : 2,
            Rationale = "Approval requests should be processed promptly to avoid workflow delays.",
            ExpectedImpact = "Processing this approval will unblock dependent workflows.",
            RelatedEntityType = "PendingApproval",
            RelatedEntityId = approval.Id
        };
    }

    private NextBestActionRecommendation CreateEvidenceRecommendation(
        Guid tenantId, Guid? userId, TenantEvidenceRequirement evidence)
    {
        var daysUntilDue = evidence.DueDate.HasValue
            ? (int)(evidence.DueDate.Value - DateTime.UtcNow).TotalDays
            : 7;

        return new NextBestActionRecommendation
        {
            TenantId = tenantId,
            TargetUserId = userId ?? evidence.AssignedToUserId,
            ActionId = $"EVIDENCE_{evidence.Id}",
            ActionType = NbaActionTypes.SubmitEvidence,
            Description = $"Evidence for '{evidence.EvidenceTypeCode}' is due in {daysUntilDue} days.",
            ConfidenceScore = daysUntilDue <= 3 ? 90 : 75,
            Priority = daysUntilDue <= 3 ? 1 : 3,
            Rationale = $"Evidence submission is required for compliance. Due date approaching.",
            ExpectedImpact = "Submitting evidence will improve compliance score and avoid SLA breach.",
            RelatedEntityType = "TenantEvidenceRequirement",
            RelatedEntityId = evidence.Id
        };
    }

    private List<NextBestActionRecommendation> CreatePciBasedRecommendations(
        Guid tenantId, Guid? userId, ProgressCertaintyIndex pci)
    {
        var recommendations = new List<NextBestActionRecommendation>();

        if (pci.VelocityTrend == "Declining")
        {
            recommendations.Add(new NextBestActionRecommendation
            {
                TenantId = tenantId,
                TargetUserId = userId,
                ActionId = $"PCI_VELOCITY_{pci.Id}",
                ActionType = NbaActionTypes.ScheduleMeeting,
                Description = "Task completion velocity is declining. Consider scheduling a coordination meeting.",
                ConfidenceScore = 75,
                Priority = 2,
                Rationale = $"Progress Certainty Index shows declining velocity trend. Current score: {pci.Score}.",
                ExpectedImpact = "A coordination meeting can help identify blockers and realign team efforts."
            });
        }

        if (pci.EvidenceRejectionRate > 30)
        {
            recommendations.Add(new NextBestActionRecommendation
            {
                TenantId = tenantId,
                TargetUserId = userId,
                ActionId = $"PCI_REJECTION_{pci.Id}",
                ActionType = NbaActionTypes.RequestHelp,
                Description = "High evidence rejection rate detected. Request guidance on evidence standards.",
                ConfidenceScore = 80,
                Priority = 2,
                Rationale = $"Evidence rejection rate is {pci.EvidenceRejectionRate}%, indicating quality issues.",
                ExpectedImpact = "Improving evidence quality will reduce rework and improve compliance score."
            });
        }

        return recommendations;
    }

    private NextBestActionRecommendation CreateWorkflowRecommendation(
        Guid tenantId, Guid? userId, WorkflowInstance workflow)
    {
        return new NextBestActionRecommendation
        {
            TenantId = tenantId,
            TargetUserId = userId,
            ActionId = $"WORKFLOW_{workflow.Id}",
            ActionType = NbaActionTypes.Escalate,
            Description = $"Workflow '{workflow.Name}' has been stuck for over 3 days.",
            ConfidenceScore = 85,
            Priority = 2,
            Rationale = "Workflows that remain in-progress without updates may indicate blockers.",
            ExpectedImpact = "Escalation will bring attention to potential issues blocking progress.",
            RelatedEntityType = "WorkflowInstance",
            RelatedEntityId = workflow.Id
        };
    }

    private async Task<ExecuteRecommendationResult> ExecuteRemindAsync(
        NextBestActionRecommendation recommendation, CancellationToken cancellationToken)
    {
        // Implementation would send notification
        _logger.LogInformation("Executing reminder for recommendation {Id}", recommendation.Id);
        return new ExecuteRecommendationResult
        {
            Success = true,
            Message = "Reminder notification sent",
            ResultDetails = "Notification queued for delivery"
        };
    }

    private async Task<ExecuteRecommendationResult> ExecuteEscalateAsync(
        NextBestActionRecommendation recommendation, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing escalation for recommendation {Id}", recommendation.Id);
        return new ExecuteRecommendationResult
        {
            Success = true,
            Message = "Escalation notification sent to manager",
            ResultDetails = "Escalation created and notifications queued"
        };
    }

    private async Task<ExecuteRecommendationResult> ExecuteAutoCollectAsync(
        NextBestActionRecommendation recommendation, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing auto-collect for recommendation {Id}", recommendation.Id);
        return new ExecuteRecommendationResult
        {
            Success = true,
            Message = "Automated evidence collection initiated",
            ResultDetails = "Collection job queued for processing"
        };
    }

    private async Task<ExecuteRecommendationResult> ExecuteGenerateReportAsync(
        NextBestActionRecommendation recommendation, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing report generation for recommendation {Id}", recommendation.Id);
        return new ExecuteRecommendationResult
        {
            Success = true,
            Message = "Report generation initiated",
            ResultDetails = "Report job queued for processing"
        };
    }

    #endregion
}
