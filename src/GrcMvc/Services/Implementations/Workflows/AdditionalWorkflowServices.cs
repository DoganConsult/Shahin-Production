using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkflowInstance = GrcMvc.Models.Entities.WorkflowInstance;
using WorkflowTask = GrcMvc.Models.Entities.WorkflowTask;

namespace GrcMvc.Services.Implementations.Workflows
{
    public class EvidenceCollectionWorkflowService : BaseWorkflowService, IEvidenceCollectionWorkflowService
    {
        public EvidenceCollectionWorkflowService(GrcDbContext context, ILogger<EvidenceCollectionWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> InitiateEvidenceCollectionAsync(Guid controlId, Guid tenantId, string initiatedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "EvidenceCollection", controlId, "Control",
                EvidenceCollectionState.NotStarted.ToString(), initiatedByUserId);
        }

        public async Task<bool> NotifyEvidenceSubmissionAsync(Guid workflowInstanceId, string assignedToUserId, DateTime dueDate)
        {
            await AssignTaskAsync(workflowInstanceId, "Submit Evidence", assignedToUserId, dueDate, 1,
                "Please submit required evidence for this control");
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.NotStarted.ToString(),
                EvidenceCollectionState.PendingSubmission.ToString(), "system");
        }

        public async Task<bool> SubmitEvidenceAsync(Guid workflowInstanceId, string submittedByUserId, string evidenceDescription, List<string> fileUrls)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.Metadata = JsonSerializer.Serialize(new { fileUrls, description = evidenceDescription });

            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.PendingSubmission.ToString(),
                EvidenceCollectionState.Submitted.ToString(), submittedByUserId, evidenceDescription);
        }

        public async Task<bool> ReviewEvidenceAsync(Guid workflowInstanceId, string reviewedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.Submitted.ToString(),
                EvidenceCollectionState.UnderReview.ToString(), reviewedByUserId);
        }

        public async Task<bool> RequestEvidenceRevisionAsync(Guid workflowInstanceId, string reviewedByUserId, string revisionNotes)
        {
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.UnderReview.ToString(),
                EvidenceCollectionState.RequestedRevisions.ToString(), reviewedByUserId, revisionNotes);
        }

        public async Task<bool> ApproveEvidenceAsync(Guid workflowInstanceId, string approvedByUserId, string comments)
        {
            await ApproveAsync(workflowInstanceId, "EvidenceReview", approvedByUserId, "Approved", comments);
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.UnderReview.ToString(),
                EvidenceCollectionState.Approved.ToString(), approvedByUserId);
        }

        public async Task<bool> ArchiveEvidenceAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.Approved.ToString(),
                EvidenceCollectionState.Archived.ToString(), "system");
        }

        public async Task<bool> ExpireEvidenceAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, EvidenceCollectionState.Approved.ToString(),
                EvidenceCollectionState.Expired.ToString(), "system");
        }

        public async Task<WorkflowInstance?> GetEvidenceWorkflowAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowTask>> GetOutstandingEvidenceTasksAsync(Guid tenantId)
        {
            return await _context.WorkflowTasks
                .Where(t => t.TenantId == tenantId && t.Status == "Pending")
                .ToListAsync();
        }
    }

    public class ComplianceTestingWorkflowService : BaseWorkflowService, IComplianceTestingWorkflowService
    {
        public ComplianceTestingWorkflowService(GrcDbContext context, ILogger<ComplianceTestingWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> InitiateComplianceTestAsync(Guid controlId, Guid tenantId, string initiatedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "ComplianceTesting", controlId, "Control",
                ComplianceTestingState.NotStarted.ToString(), initiatedByUserId);
        }

        public async Task<bool> CreateTestPlanAsync(Guid workflowInstanceId, string testPlanData)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.NotStarted.ToString(),
                ComplianceTestingState.TestPlanCreated.ToString(), "system", testPlanData);
        }

        public async Task<bool> StartTestExecutionAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.TestPlanCreated.ToString(),
                ComplianceTestingState.TestsInProgress.ToString(), "system");
        }

        public async Task<bool> CompleteTestExecutionAsync(Guid workflowInstanceId, string testResults)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.TestsInProgress.ToString(),
                ComplianceTestingState.TestsCompleted.ToString(), "system", testResults);
        }

        public async Task<bool> SubmitResultsForReviewAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.TestsCompleted.ToString(),
                ComplianceTestingState.ResultsReview.ToString(), "system");
        }

        public async Task<bool> MarkAsCompliantAsync(Guid workflowInstanceId, string reviewedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.ResultsReview.ToString(),
                ComplianceTestingState.Compliant.ToString(), reviewedByUserId);
        }

        public async Task<bool> MarkAsNonCompliantAsync(Guid workflowInstanceId, string reviewedByUserId, string nonComplianceDetails)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.ResultsReview.ToString(),
                ComplianceTestingState.NonCompliance.ToString(), reviewedByUserId, nonComplianceDetails);
        }

        public async Task<bool> InitiateRemediationAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.NonCompliance.ToString(),
                ComplianceTestingState.Remediation.ToString(), "system");
        }

        public async Task<bool> VerifyRemediationAsync(Guid workflowInstanceId, string verifiedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, ComplianceTestingState.Remediation.ToString(),
                ComplianceTestingState.Verified.ToString(), verifiedByUserId);
        }

        public async Task<WorkflowInstance?> GetTestStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }
    }

    public class RemediationWorkflowService : BaseWorkflowService, IRemediationWorkflowService
    {
        public RemediationWorkflowService(GrcDbContext context, ILogger<RemediationWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> IdentifyRemediationAsync(Guid issueId, Guid tenantId, string initiatedByUserId, string description)
        {
            return await CreateWorkflowAsync(tenantId, "Remediation", issueId, "Issue",
                RemediationState.Identified.ToString(), initiatedByUserId);
        }

        public async Task<bool> CreateRemediationPlanAsync(Guid workflowInstanceId, string planData)
        {
            return await TransitionStateAsync(workflowInstanceId, RemediationState.Identified.ToString(),
                RemediationState.PlanningPhase.ToString(), "system", planData);
        }

        public async Task<bool> StartRemediationAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RemediationState.PlanningPhase.ToString(),
                RemediationState.RemediationInProgress.ToString(), "system");
        }

        public async Task<bool> LogProgressAsync(Guid workflowInstanceId, string progressNotes)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitForVerificationAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RemediationState.RemediationInProgress.ToString(),
                RemediationState.UnderVerification.ToString(), "system");
        }

        public async Task<bool> VerifyRemediationAsync(Guid workflowInstanceId, string verifiedByUserId, bool isSuccessful)
        {
            if (isSuccessful)
                return await TransitionStateAsync(workflowInstanceId, RemediationState.UnderVerification.ToString(),
                    RemediationState.Verified.ToString(), verifiedByUserId);
            else
                return await TransitionStateAsync(workflowInstanceId, RemediationState.UnderVerification.ToString(),
                    RemediationState.RemediationInProgress.ToString(), verifiedByUserId);
        }

        public async Task<bool> StartMonitoringAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, RemediationState.Verified.ToString(),
                RemediationState.Monitored.ToString(), "system");
        }

        public async Task<bool> CloseRemediationAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = RemediationState.Closed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowInstance?> GetRemediationStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowTask>> GetOutstandingRemediationTasksAsync(Guid tenantId)
        {
            return await _context.WorkflowTasks
                .Where(t => t.TenantId == tenantId && t.Status == "Pending")
                .ToListAsync();
        }
    }

    public class PolicyReviewWorkflowService : BaseWorkflowService, IPolicyReviewWorkflowService
    {
        public PolicyReviewWorkflowService(GrcDbContext context, ILogger<PolicyReviewWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> SchedulePolicyReviewAsync(Guid policyId, Guid tenantId, DateTime reviewDate)
        {
            return await CreateWorkflowAsync(tenantId, "PolicyReview", policyId, "Policy",
                PolicyReviewState.ScheduledForReview.ToString(), "system");
        }

        public async Task<bool> BeginPolicyReviewAsync(Guid workflowInstanceId, string reviewedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.ScheduledForReview.ToString(),
                PolicyReviewState.InReview.ToString(), reviewedByUserId);
        }

        public async Task<bool> RequestPolicyRevisionAsync(Guid workflowInstanceId, string reviewedByUserId, string revisionNotes)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.InReview.ToString(),
                PolicyReviewState.RequestedRevisions.ToString(), reviewedByUserId, revisionNotes);
        }

        public async Task<bool> SubmitRevisionAsync(Guid workflowInstanceId, string revisedByUserId, string revisionData)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.RequestedRevisions.ToString(),
                PolicyReviewState.InReview.ToString(), revisedByUserId, revisionData);
        }

        public async Task<bool> SendForApprovalAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.InReview.ToString(),
                PolicyReviewState.UnderApproval.ToString(), "system");
        }

        public async Task<bool> ApprovePolicyAsync(Guid workflowInstanceId, string approvedByUserId)
        {
            await ApproveAsync(workflowInstanceId, "PolicyApproval", approvedByUserId, "Approved");
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.UnderApproval.ToString(),
                PolicyReviewState.Approved.ToString(), approvedByUserId);
        }

        public async Task<bool> PublishPolicyAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.Approved.ToString(),
                PolicyReviewState.Published.ToString(), "system");
        }

        public async Task<bool> RetirePolicyAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, PolicyReviewState.InEffect.ToString(),
                PolicyReviewState.Obsolete.ToString(), "system");
        }

        public async Task<WorkflowInstance?> GetPolicyReviewStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowInstance>> GetScheduledPolicyReviewsAsync(Guid tenantId)
        {
            return await _context.WorkflowInstances
                .Where(w => w.TenantId == tenantId && w.WorkflowType == "PolicyReview" && w.Status == "Active")
                .ToListAsync();
        }
    }

    public class TrainingAssignmentWorkflowService : BaseWorkflowService, ITrainingAssignmentWorkflowService
    {
        public TrainingAssignmentWorkflowService(GrcDbContext context, ILogger<TrainingAssignmentWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> AssignTrainingAsync(Guid employeeId, Guid trainingModuleId, Guid tenantId, string assignedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "TrainingAssignment", trainingModuleId, "TrainingModule",
                TrainingAssignmentState.Assigned.ToString(), assignedByUserId);
        }

        public async Task<bool> NotifyEmployeeAsync(Guid workflowInstanceId)
        {
            return await NotifyAsync(workflowInstanceId, "employee", "TaskAssigned",
                "You have been assigned mandatory training", "Training Assignment");
        }

        public async Task<bool> AcknowledgeTrainingAsync(Guid workflowInstanceId, string employeeId)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Assigned.ToString(),
                TrainingAssignmentState.Acknowledged.ToString(), employeeId);
        }

        public async Task<bool> StartTrainingAsync(Guid workflowInstanceId, string employeeId)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Acknowledged.ToString(),
                TrainingAssignmentState.InProgress.ToString(), employeeId);
        }

        public async Task<bool> CompleteTrainingAsync(Guid workflowInstanceId, string employeeId)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.InProgress.ToString(),
                TrainingAssignmentState.Completed.ToString(), employeeId);
        }

        public async Task<bool> MarkAsPassedAsync(Guid workflowInstanceId, int score)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Completed.ToString(),
                TrainingAssignmentState.Passed.ToString(), "system", $"Score: {score}");
        }

        public async Task<bool> MarkAsFailedAsync(Guid workflowInstanceId, string failureReason)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Completed.ToString(),
                TrainingAssignmentState.Failed.ToString(), "system", failureReason);
        }

        public async Task<bool> ReassignTrainingAsync(Guid workflowInstanceId, Guid newEmployeeId)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Failed.ToString(),
                TrainingAssignmentState.Reassigned.ToString(), "system");
        }

        public async Task<bool> ArchiveTrainingAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, TrainingAssignmentState.Reassigned.ToString(),
                TrainingAssignmentState.Archived.ToString(), "system");
        }

        public async Task<List<WorkflowInstance>> GetPendingTrainingAsync(Guid tenantId)
        {
            return await _context.WorkflowInstances
                .Where(w => w.TenantId == tenantId && w.Status == "Pending")
                .ToListAsync();
        }

        public async Task<List<WorkflowInstance>> GetEmployeeTrainingHistoryAsync(string employeeId)
        {
            if (!Guid.TryParse(employeeId, out var empGuid)) return new List<WorkflowInstance>();
            return await _context.WorkflowInstances
                .Where(w => w.InitiatedByUserId == empGuid)
                .ToListAsync();
        }
    }

    public class AuditWorkflowService : BaseWorkflowService, IAuditWorkflowService
    {
        public AuditWorkflowService(GrcDbContext context, ILogger<AuditWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> InitiateAuditAsync(Guid auditId, Guid tenantId, string initiatedByUserId)
        {
            return await CreateWorkflowAsync(tenantId, "Audit", auditId, "Audit",
                AuditState.NotStarted.ToString(), initiatedByUserId);
        }

        public async Task<bool> CreateAuditPlanAsync(Guid workflowInstanceId, string auditPlan)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.NotStarted.ToString(),
                AuditState.PlanningPhase.ToString(), "system", auditPlan);
        }

        public async Task<bool> StartFieldworkAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.PlanningPhase.ToString(),
                AuditState.FieldworkInProgress.ToString(), "system");
        }

        public async Task<bool> LogFieldworkProgressAsync(Guid workflowInstanceId, string progressNotes)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteFieldworkAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.FieldworkInProgress.ToString(),
                AuditState.DocumentationPhase.ToString(), "system");
        }

        public async Task<bool> SubmitDraftReportAsync(Guid workflowInstanceId, string draftReport)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.DocumentationPhase.ToString(),
                AuditState.UnderReview.ToString(), "system", draftReport);
        }

        public async Task<bool> RequestManagementResponseAsync(Guid workflowInstanceId, string responseDeadline)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.UnderReview.ToString(),
                AuditState.DraftReportIssued.ToString(), "system", $"Response deadline: {responseDeadline}");
        }

        public async Task<bool> ReceiveManagementResponseAsync(Guid workflowInstanceId, string response)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.DraftReportIssued.ToString(),
                AuditState.AwaitingManagementResponse.ToString(), "system", response);
        }

        public async Task<bool> IssueFinalReportAsync(Guid workflowInstanceId, string finalReport)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.AwaitingManagementResponse.ToString(),
                AuditState.FinalReportIssued.ToString(), "system", finalReport);
        }

        public async Task<bool> ScheduleFollowUpAsync(Guid workflowInstanceId, DateTime followUpDate)
        {
            return await TransitionStateAsync(workflowInstanceId, AuditState.FinalReportIssued.ToString(),
                AuditState.FollowUpScheduled.ToString(), "system", followUpDate.ToString());
        }

        public async Task<bool> CloseAuditAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = AuditState.Closed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowInstance?> GetAuditStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }
    }

    public class ExceptionHandlingWorkflowService : BaseWorkflowService, IExceptionHandlingWorkflowService
    {
        public ExceptionHandlingWorkflowService(GrcDbContext context, ILogger<ExceptionHandlingWorkflowService> logger,
            INotificationService notificationService)
            : base(context, logger, notificationService) { }

        public async Task<WorkflowInstance> SubmitExceptionAsync(Guid tenantId, string submittedByUserId, string exceptionDescription, string justification)
        {
            return await CreateWorkflowAsync(tenantId, "ExceptionHandling", Guid.Empty, "Exception",
                ExceptionHandlingState.Submitted.ToString(), submittedByUserId);
        }

        public async Task<bool> AcknowledgeExceptionAsync(Guid workflowInstanceId, string reviewedByUserId)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.Submitted.ToString(),
                ExceptionHandlingState.PendingReview.ToString(), reviewedByUserId);
        }

        public async Task<bool> InvestigateExceptionAsync(Guid workflowInstanceId, string investigationFindings)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.PendingReview.ToString(),
                ExceptionHandlingState.UnderInvestigation.ToString(), "system", investigationFindings);
        }

        public async Task<bool> AssessRiskAsync(Guid workflowInstanceId, string riskAssessment)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.UnderInvestigation.ToString(),
                ExceptionHandlingState.RiskAssessed.ToString(), "system", riskAssessment);
        }

        public async Task<bool> SubmitForApprovalAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.RiskAssessed.ToString(),
                ExceptionHandlingState.PendingApproval.ToString(), "system");
        }

        public async Task<bool> ApproveExceptionAsync(Guid workflowInstanceId, string approvedByUserId, string approvalConditions)
        {
            await ApproveAsync(workflowInstanceId, "Exception", approvedByUserId, "Approved", approvalConditions);
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.PendingApproval.ToString(),
                ExceptionHandlingState.Approved.ToString(), approvedByUserId);
        }

        public async Task<bool> RejectExceptionAsync(Guid workflowInstanceId, string rejectedByUserId, string rejectionExplanation)
        {
            await ApproveAsync(workflowInstanceId, "Exception", rejectedByUserId, "Rejected", rejectionExplanation);
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.PendingApproval.ToString(),
                ExceptionHandlingState.RejectedWithExplanation.ToString(), rejectedByUserId);
        }

        public async Task<bool> MonitorExceptionAsync(Guid workflowInstanceId)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.Approved.ToString(),
                ExceptionHandlingState.Monitoring.ToString(), "system");
        }

        public async Task<bool> ResolveExceptionAsync(Guid workflowInstanceId, string resolutionDetails)
        {
            return await TransitionStateAsync(workflowInstanceId, ExceptionHandlingState.Monitoring.ToString(),
                ExceptionHandlingState.Resolved.ToString(), "system", resolutionDetails);
        }

        public async Task<bool> CloseExceptionAsync(Guid workflowInstanceId)
        {
            var workflow = await _context.WorkflowInstances.FindAsync(workflowInstanceId);
            if (workflow == null) return false;

            workflow.CurrentState = ExceptionHandlingState.Closed.ToString();
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            _context.WorkflowInstances.Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkflowInstance?> GetExceptionStatusAsync(Guid workflowInstanceId)
        {
            return await _context.WorkflowInstances.FindAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowInstance>> GetPendingExceptionsAsync(Guid tenantId)
        {
            return await _context.WorkflowInstances
                .Where(w => w.TenantId == tenantId && w.WorkflowType == "ExceptionHandling" && w.Status == "Active")
                .ToListAsync();
        }
    }
}