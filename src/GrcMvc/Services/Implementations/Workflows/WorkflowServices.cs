using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.EntityFrameworkCore;
using WorkflowApproval = GrcMvc.Models.Workflows.WorkflowApproval;
using WorkflowInstance = GrcMvc.Models.Entities.WorkflowInstance;
using WorkflowTask = GrcMvc.Models.Entities.WorkflowTask;

namespace GrcMvc.Services.Implementations.Workflows
{
    /// <summary>
    /// Base workflow service - shared functionality for all workflows
    /// Uses Guid IDs consistently for distributed system compatibility
    /// </summary>
    public class BaseWorkflowService
    {
        protected readonly GrcDbContext _context;
        protected readonly ILogger<BaseWorkflowService> _logger;
        protected readonly INotificationService? _notificationService;

        public BaseWorkflowService(GrcDbContext context, ILogger<BaseWorkflowService> logger, INotificationService? notificationService = null)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        protected async Task<WorkflowInstance> CreateWorkflowAsync(Guid tenantId, string workflowType, Guid entityId,
            string entityType, string initialState, string initiatedByUserId)
        {
            var workflow = new WorkflowInstance
            {
                TenantId = tenantId,
                WorkflowType = workflowType,
                EntityId = entityId,
                EntityType = entityType,
                CurrentState = initialState,
                InitiatedByUserId = Guid.TryParse(initiatedByUserId, out var userId) ? userId : null,
                StartedAt = DateTime.UtcNow,
                Status = "Active"
            };

            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {WorkflowType} workflow {WorkflowId} for {EntityType} {EntityId}",
                workflowType, workflow.Id, entityType, entityId);
            return workflow;
        }

        protected async Task<bool> TransitionStateAsync(Guid workflowInstanceId, string fromState, string toState,
            string triggeredByUserId, string? reason = null)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null || workflow.CurrentState != fromState)
                return false;

            workflow.CurrentState = toState;

            var transition = new WorkflowTransition
            {
                WorkflowInstanceId = workflowInstanceId,
                FromState = fromState,
                ToState = toState,
                TriggeredBy = triggeredByUserId,
                TransitionDate = DateTime.UtcNow,
                Reason = reason
            };

            // TODO: Add WorkflowTransitions DbSet to GrcDbContext
            // _context.WorkflowTransitions.Add(transition);
            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Workflow {WorkflowId} transitioned from {FromState} to {ToState}",
                workflowInstanceId, fromState, toState);
            return true;
        }

        protected async Task<bool> AssignTaskAsync(Guid workflowInstanceId, string taskName, string assignedToUserId,
            DateTime dueDate, int priority = 2, string? description = null)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            var task = new WorkflowTask
            {
                TenantId = workflow.TenantId,
                WorkflowInstanceId = workflowInstanceId,
                TaskName = taskName,
                Description = description ?? string.Empty,
                AssignedToUserId = Guid.TryParse(assignedToUserId, out var userId) ? userId : null,
                DueDate = dueDate,
                Priority = priority,
                Status = "Pending"
            };

            _context.WorkflowTasks.Add(task);
            await _context.SaveChangesAsync();

            // Notify assignee
            await NotifyAsync(workflowInstanceId, assignedToUserId, "TaskAssigned",
                $"You have been assigned task: {taskName}", $"New Task: {taskName}");

            return true;
        }

        protected async Task<bool> NotifyAsync(Guid workflowInstanceId, string recipientUserId, string notificationType,
            string message, string subject)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            // Create the notification record in database
            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = workflowInstanceId,
                TenantId = workflow.TenantId,
                NotificationType = notificationType,
                RecipientUserId = recipientUserId,
                Recipient = recipientUserId,
                Message = message,
                Subject = subject,
                Body = message,
                CreatedAt = DateTime.UtcNow,
                IsSent = false
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send via NotificationService if available
            if (_notificationService != null)
            {
                try
                {
                    var templateData = new Dictionary<string, object>
                    {
                        { "WorkflowType", workflow.WorkflowType },
                        { "WorkflowId", workflowInstanceId },
                        { "CurrentState", workflow.CurrentState },
                        { "Message", message }
                    };

                    var result = await _notificationService.SendNotificationAsync(
                        workflowInstanceId,
                        recipientUserId,
                        notificationType,
                        subject,
                        message,
                        "Normal",
                        workflow.TenantId,
                        templateData);

                    if (result.IsSuccess)
                    {
                        notification.IsSent = true;
                        notification.IsDelivered = true;
                        notification.DeliveredAt = DateTime.UtcNow;
                        _context.WorkflowNotifications.Update(notification);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification for workflow {WorkflowId}", workflowInstanceId);
                }
            }

            return true;
        }

        protected async Task<bool> ApproveAsync(Guid workflowInstanceId, string approvalLevel, string approvedByUserId,
            string decision, string? comments = null)
        {
            var approval = new WorkflowApproval
            {
                WorkflowInstanceId = workflowInstanceId,
                ApprovalLevel = approvalLevel,
                ApprovedByUserId = approvedByUserId,
                Decision = decision,
                Comments = comments,
                ApprovedAt = DateTime.UtcNow,
                ApproversRole = approvalLevel
            };

            _context.WorkflowApprovals.Add(approval);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // ===== IMPLEMENTATION CLASSES =====

    public class ControlImplementationWorkflowService : BaseWorkflowService, IControlImplementationWorkflowService
    {
        public ControlImplementationWorkflowService(GrcDbContext context, ILogger<ControlImplementationWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> InitiateControlImplementationAsync(Guid controlId, Guid tenantId, string initiatedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "ControlImplementation", controlId, "Control",
                ControlImplementationState.NotStarted.ToString(), initiatedByUserId);
        }

        public async Task<bool> MoveToPlanning(Guid workflowInstanceId, string notes)
        {
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.NotStarted.ToString(),
                ControlImplementationState.InPlanning.ToString(), "system", notes);
        }

        public async Task<bool> MoveToImplementation(Guid workflowInstanceId, string implementationDetails)
        {
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.InPlanning.ToString(),
                ControlImplementationState.InImplementation.ToString(), "system", implementationDetails);
        }

        public async Task<bool> SubmitForReview(Guid workflowInstanceId, string submittedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.InImplementation.ToString(),
                ControlImplementationState.UnderReview.ToString(), submittedByUserId);
        }

        public async Task<bool> ApproveImplementation(Guid workflowInstanceId, string approvedByUserId, string comments)
        {
            await ApproveAsync(workflowInstanceId, "ControlReview", approvedByUserId, "Approved", comments);
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.UnderReview.ToString(),
                ControlImplementationState.Approved.ToString(), approvedByUserId);
        }

        public async Task<bool> DeployControl(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.Approved.ToString(),
                ControlImplementationState.Deployed.ToString(), "system");
        }

        public async Task<bool> StartMonitoring(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ControlImplementationState.Deployed.ToString(),
                ControlImplementationState.Monitored.ToString(), "system");
        }

        public async Task<bool> CompleteWorkflow(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = ControlImplementationState.Completed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowInstance?> GetWorkflowAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowTask>> GetPendingTasksAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowTasks
                .Where(t => t.WorkflowInstanceId == workflowInstanceId && t.Status == "Pending")
                .ToListAsync();
        }
    }

    public class RiskAssessmentWorkflowService : BaseWorkflowService, IRiskAssessmentWorkflowService
    {
        public RiskAssessmentWorkflowService(GrcDbContext context, ILogger<RiskAssessmentWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> InitiateRiskAssessmentAsync(Guid riskId, Guid tenantId, string initiatedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "RiskAssessment", riskId, "Risk",
                RiskAssessmentState.NotStarted.ToString(), initiatedByUserId);
        }

        public async Task<bool> StartDataGatheringAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.NotStarted.ToString(),
                RiskAssessmentState.DataGathering.ToString(), "system");
        }

        public async Task<bool> SubmitAnalysisAsync(Guid workflowInstanceId, string analysisData)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.DataGathering.ToString(),
                RiskAssessmentState.Analysis.ToString(), "system", analysisData);
        }

        public async Task<bool> EvaluateRiskAsync(Guid workflowInstanceId, string riskLevel, string evaluation)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.Analysis.ToString(),
                RiskAssessmentState.Evaluation.ToString(), "system", $"Risk Level: {riskLevel}. {evaluation}");
        }

        public async Task<bool> SubmitForReviewAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.Evaluation.ToString(),
                RiskAssessmentState.UnderReview.ToString(), "system");
        }

        public async Task<bool> ApproveAssessmentAsync(Guid workflowInstanceId, string approvedByUserId)
        {
            await ApproveAsync(workflowInstanceId, "RiskReview", approvedByUserId, "Approved");
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.UnderReview.ToString(),
                RiskAssessmentState.Approved.ToString(), approvedByUserId);
        }

        public async Task<bool> DocumentAssessmentAsync(Guid workflowInstanceId, string documentation)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.Approved.ToString(),
                RiskAssessmentState.Documented.ToString(), "system", documentation);
        }

        public async Task<bool> StartMonitoringAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RiskAssessmentState.Documented.ToString(),
                RiskAssessmentState.Monitored.ToString(), "system");
        }

        public async Task<bool> CloseAssessmentAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = RiskAssessmentState.Closed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowInstance?> GetAssessmentStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }
    }

    public class ApprovalWorkflowService : BaseWorkflowService, IApprovalWorkflowService
    {
        public ApprovalWorkflowService(GrcDbContext context, ILogger<ApprovalWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> SubmitForApprovalAsync(Guid entityId, string entityType, Guid tenantId, string submittedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "Approval", entityId, entityType,
                ApprovalState.Submitted.ToString(), submittedByUserId);
        }

        public async Task<bool> SubmitToManagerAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.Submitted.ToString(),
                ApprovalState.PendingManagerReview.ToString(), "system");
        }

        public async Task<bool> ApproveAsManagerAsync(Guid workflowInstanceId, string managerId, string comments)
        {
            await ApproveAsync(workflowInstanceId, "Manager", managerId, "Approved", comments);
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.PendingManagerReview.ToString(),
                ApprovalState.ManagerApproved.ToString(), managerId);
        }

        public async Task<bool> RejectAsManagerAsync(Guid workflowInstanceId, string managerId, string rejectionReason)
        {
            await ApproveAsync(workflowInstanceId, "Manager", managerId, "Rejected", rejectionReason);
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.PendingManagerReview.ToString(),
                ApprovalState.Rejected.ToString(), managerId);
        }

        public async Task<bool> SubmitToComplianceAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.ManagerApproved.ToString(),
                ApprovalState.PendingComplianceReview.ToString(), "system");
        }

        public async Task<bool> ApproveAsComplianceAsync(Guid workflowInstanceId, string complianceOfficerId, string comments)
        {
            await ApproveAsync(workflowInstanceId, "Compliance", complianceOfficerId, "Approved", comments);
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.PendingComplianceReview.ToString(),
                ApprovalState.ComplianceApproved.ToString(), complianceOfficerId);
        }

        public async Task<bool> RequestRevisionAsync(Guid workflowInstanceId, string reviewerId, string revisionNotes)
        {
            await ApproveAsync(workflowInstanceId, "Review", reviewerId, "NeedsRevision", revisionNotes);
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.PendingComplianceReview.ToString(),
                ApprovalState.Submitted.ToString(), reviewerId);
        }

        public async Task<bool> SubmitToExecutiveAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.ComplianceApproved.ToString(),
                ApprovalState.PendingExecutiveSignOff.ToString(), "system");
        }

        public async Task<bool> ApproveAsExecutiveAsync(Guid workflowInstanceId, string executiveId, string comments)
        {
            await ApproveAsync(workflowInstanceId, "Executive", executiveId, "Approved", comments);
            return await TransitionStateAsync(workflowInstanceId, ApprovalState.PendingExecutiveSignOff.ToString(),
                ApprovalState.ExecutiveApproved.ToString(), executiveId);
        }

        public async Task<bool> FinalizeApprovalAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = ApprovalState.Completed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<WorkflowApproval>> GetApprovalHistoryAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowApprovals
                .Where(a => a.WorkflowInstanceId == workflowInstanceId)
                .OrderBy(a => a.ApprovedAt)
                .ToListAsync();
        }

        public async Task<string> GetCurrentApprovalLevelAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            return workflow?.CurrentState ?? "Unknown";
        }
    }
}
