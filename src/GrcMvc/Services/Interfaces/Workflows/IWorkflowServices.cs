using GrcMvc.Models.Entities;
using WorkflowApproval = GrcMvc.Models.Workflows.WorkflowApproval;

namespace GrcMvc.Services.Interfaces.Workflows
{
    /// <summary>
    /// Interface for Control Implementation Workflow management
    /// Handles workflow from control planning through deployment and monitoring
    /// </summary>
    public interface IControlImplementationWorkflowService
    {
        Task<WorkflowInstance> InitiateControlImplementationAsync(Guid controlId, Guid tenantId, string initiatedByUserId);
        Task<bool> MoveToPlanning(Guid workflowInstanceId, string notes);
        Task<bool> MoveToImplementation(Guid workflowInstanceId, string implementationDetails);
        Task<bool> SubmitForReview(Guid workflowInstanceId, string submittedByUserId);
        Task<bool> ApproveImplementation(Guid workflowInstanceId, string approvedByUserId, string comments);
        Task<bool> DeployControl(Guid workflowInstanceId);
        Task<bool> StartMonitoring(Guid workflowInstanceId);
        Task<bool> CompleteWorkflow(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetWorkflowAsync(Guid workflowInstanceId);
        Task<List<WorkflowTask>> GetPendingTasksAsync(Guid workflowInstanceId);
    }

    /// <summary>
    /// Interface for Risk Assessment Workflow management
    /// Handles risk identification, analysis, evaluation, and documentation
    /// </summary>
    public interface IRiskAssessmentWorkflowService
    {
        Task<WorkflowInstance> InitiateRiskAssessmentAsync(Guid riskId, Guid tenantId, string initiatedByUserId);
        Task<bool> StartDataGatheringAsync(Guid workflowInstanceId);
        Task<bool> SubmitAnalysisAsync(Guid workflowInstanceId, string analysisData);
        Task<bool> EvaluateRiskAsync(Guid workflowInstanceId, string riskLevel, string evaluation);
        Task<bool> SubmitForReviewAsync(Guid workflowInstanceId);
        Task<bool> ApproveAssessmentAsync(Guid workflowInstanceId, string approvedByUserId);
        Task<bool> DocumentAssessmentAsync(Guid workflowInstanceId, string documentation);
        Task<bool> StartMonitoringAsync(Guid workflowInstanceId);
        Task<bool> CloseAssessmentAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetAssessmentStatusAsync(Guid workflowInstanceId);
    }

    /// <summary>
    /// Interface for Approval/Sign-off Workflow management
    /// Handles multi-level approvals (Manager → Compliance → Executive)
    /// </summary>
    public interface IApprovalWorkflowService
    {
        Task<WorkflowInstance> SubmitForApprovalAsync(Guid entityId, string entityType, Guid tenantId, string submittedByUserId);
        Task<bool> SubmitToManagerAsync(Guid workflowInstanceId);
        Task<bool> ApproveAsManagerAsync(Guid workflowInstanceId, string managerId, string comments);
        Task<bool> RejectAsManagerAsync(Guid workflowInstanceId, string managerId, string rejectionReason);
        Task<bool> SubmitToComplianceAsync(Guid workflowInstanceId);
        Task<bool> ApproveAsComplianceAsync(Guid workflowInstanceId, string complianceOfficerId, string comments);
        Task<bool> RequestRevisionAsync(Guid workflowInstanceId, string reviewerId, string revisionNotes);
        Task<bool> SubmitToExecutiveAsync(Guid workflowInstanceId);
        Task<bool> ApproveAsExecutiveAsync(Guid workflowInstanceId, string executiveId, string comments);
        Task<bool> FinalizeApprovalAsync(Guid workflowInstanceId);
        Task<List<WorkflowApproval>> GetApprovalHistoryAsync(Guid workflowInstanceId);
        Task<string> GetCurrentApprovalLevelAsync(Guid workflowInstanceId);
    }

    /// <summary>
    /// Interface for Evidence Collection Workflow management
    /// Manages evidence submission, review, and approval
    /// </summary>
    public interface IEvidenceCollectionWorkflowService
    {
        Task<WorkflowInstance> InitiateEvidenceCollectionAsync(Guid controlId, Guid tenantId, string initiatedByUserId);
        Task<bool> NotifyEvidenceSubmissionAsync(Guid workflowInstanceId, string assignedToUserId, DateTime dueDate);
        Task<bool> SubmitEvidenceAsync(Guid workflowInstanceId, string submittedByUserId, string evidenceDescription, List<string> fileUrls);
        Task<bool> ReviewEvidenceAsync(Guid workflowInstanceId, string reviewedByUserId);
        Task<bool> RequestEvidenceRevisionAsync(Guid workflowInstanceId, string reviewedByUserId, string revisionNotes);
        Task<bool> ApproveEvidenceAsync(Guid workflowInstanceId, string approvedByUserId, string comments);
        Task<bool> ArchiveEvidenceAsync(Guid workflowInstanceId);
        Task<bool> ExpireEvidenceAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetEvidenceWorkflowAsync(Guid workflowInstanceId);
        Task<List<WorkflowTask>> GetOutstandingEvidenceTasksAsync(Guid tenantId);
    }

    /// <summary>
    /// Interface for Compliance Testing Workflow management
    /// Manages test planning, execution, results, and remediation
    /// </summary>
    public interface IComplianceTestingWorkflowService
    {
        Task<WorkflowInstance> InitiateComplianceTestAsync(Guid controlId, Guid tenantId, string initiatedByUserId);
        Task<bool> CreateTestPlanAsync(Guid workflowInstanceId, string testPlanData);
        Task<bool> StartTestExecutionAsync(Guid workflowInstanceId);
        Task<bool> CompleteTestExecutionAsync(Guid workflowInstanceId, string testResults);
        Task<bool> SubmitResultsForReviewAsync(Guid workflowInstanceId);
        Task<bool> MarkAsCompliantAsync(Guid workflowInstanceId, string reviewedByUserId);
        Task<bool> MarkAsNonCompliantAsync(Guid workflowInstanceId, string reviewedByUserId, string nonComplianceDetails);
        Task<bool> InitiateRemediationAsync(Guid workflowInstanceId);
        Task<bool> VerifyRemediationAsync(Guid workflowInstanceId, string verifiedByUserId);
        Task<WorkflowInstance?> GetTestStatusAsync(Guid workflowInstanceId);
    }

    /// <summary>
    /// Interface for Remediation Workflow management
    /// Tracks remediation efforts from identification through closure
    /// </summary>
    public interface IRemediationWorkflowService
    {
        Task<WorkflowInstance> IdentifyRemediationAsync(Guid issueId, Guid tenantId, string initiatedByUserId, string description);
        Task<bool> CreateRemediationPlanAsync(Guid workflowInstanceId, string planData);
        Task<bool> StartRemediationAsync(Guid workflowInstanceId);
        Task<bool> LogProgressAsync(Guid workflowInstanceId, string progressNotes);
        Task<bool> SubmitForVerificationAsync(Guid workflowInstanceId);
        Task<bool> VerifyRemediationAsync(Guid workflowInstanceId, string verifiedByUserId, bool isSuccessful);
        Task<bool> StartMonitoringAsync(Guid workflowInstanceId);
        Task<bool> CloseRemediationAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetRemediationStatusAsync(Guid workflowInstanceId);
        Task<List<WorkflowTask>> GetOutstandingRemediationTasksAsync(Guid tenantId);
    }

    /// <summary>
    /// Interface for Policy Review Workflow management
    /// Manages policy lifecycle from review through publication and retirement
    /// </summary>
    public interface IPolicyReviewWorkflowService
    {
        Task<WorkflowInstance> SchedulePolicyReviewAsync(Guid policyId, Guid tenantId, DateTime reviewDate);
        Task<bool> BeginPolicyReviewAsync(Guid workflowInstanceId, string reviewedByUserId);
        Task<bool> RequestPolicyRevisionAsync(Guid workflowInstanceId, string reviewedByUserId, string revisionNotes);
        Task<bool> SubmitRevisionAsync(Guid workflowInstanceId, string revisedByUserId, string revisionData);
        Task<bool> SendForApprovalAsync(Guid workflowInstanceId);
        Task<bool> ApprovePolicyAsync(Guid workflowInstanceId, string approvedByUserId);
        Task<bool> PublishPolicyAsync(Guid workflowInstanceId);
        Task<bool> RetirePolicyAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetPolicyReviewStatusAsync(Guid workflowInstanceId);
        Task<List<WorkflowInstance>> GetScheduledPolicyReviewsAsync(Guid tenantId);
    }

    /// <summary>
    /// Interface for Training Assignment Workflow management
    /// Tracks employee training assignments and completion
    /// </summary>
    public interface ITrainingAssignmentWorkflowService
    {
        Task<WorkflowInstance> AssignTrainingAsync(Guid employeeId, Guid trainingModuleId, Guid tenantId, string assignedByUserId);
        Task<bool> NotifyEmployeeAsync(Guid workflowInstanceId);
        Task<bool> AcknowledgeTrainingAsync(Guid workflowInstanceId, string employeeId);
        Task<bool> StartTrainingAsync(Guid workflowInstanceId, string employeeId);
        Task<bool> CompleteTrainingAsync(Guid workflowInstanceId, string employeeId);
        Task<bool> MarkAsPassedAsync(Guid workflowInstanceId, int score);
        Task<bool> MarkAsFailedAsync(Guid workflowInstanceId, string failureReason);
        Task<bool> ReassignTrainingAsync(Guid workflowInstanceId, Guid newEmployeeId);
        Task<bool> ArchiveTrainingAsync(Guid workflowInstanceId);
        Task<List<WorkflowInstance>> GetPendingTrainingAsync(Guid tenantId);
        Task<List<WorkflowInstance>> GetEmployeeTrainingHistoryAsync(string employeeId);
    }

    /// <summary>
    /// Interface for Audit Workflow management
    /// Manages complete audit lifecycle from planning through closure
    /// </summary>
    public interface IAuditWorkflowService
    {
        Task<WorkflowInstance> InitiateAuditAsync(Guid auditId, Guid tenantId, string initiatedByUserId);
        Task<bool> CreateAuditPlanAsync(Guid workflowInstanceId, string auditPlan);
        Task<bool> StartFieldworkAsync(Guid workflowInstanceId);
        Task<bool> LogFieldworkProgressAsync(Guid workflowInstanceId, string progressNotes);
        Task<bool> CompleteFieldworkAsync(Guid workflowInstanceId);
        Task<bool> SubmitDraftReportAsync(Guid workflowInstanceId, string draftReport);
        Task<bool> RequestManagementResponseAsync(Guid workflowInstanceId, string responseDeadline);
        Task<bool> ReceiveManagementResponseAsync(Guid workflowInstanceId, string response);
        Task<bool> IssueFinalReportAsync(Guid workflowInstanceId, string finalReport);
        Task<bool> ScheduleFollowUpAsync(Guid workflowInstanceId, DateTime followUpDate);
        Task<bool> CloseAuditAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetAuditStatusAsync(Guid workflowInstanceId);
    }

    /// <summary>
    /// Interface for Exception Handling Workflow management
    /// Manages exceptions/deviations from policies and controls
    /// </summary>
    public interface IExceptionHandlingWorkflowService
    {
        Task<WorkflowInstance> SubmitExceptionAsync(Guid tenantId, string submittedByUserId, string exceptionDescription, string justification);
        Task<bool> AcknowledgeExceptionAsync(Guid workflowInstanceId, string reviewedByUserId);
        Task<bool> InvestigateExceptionAsync(Guid workflowInstanceId, string investigationFindings);
        Task<bool> AssessRiskAsync(Guid workflowInstanceId, string riskAssessment);
        Task<bool> SubmitForApprovalAsync(Guid workflowInstanceId);
        Task<bool> ApproveExceptionAsync(Guid workflowInstanceId, string approvedByUserId, string approvalConditions);
        Task<bool> RejectExceptionAsync(Guid workflowInstanceId, string rejectedByUserId, string rejectionExplanation);
        Task<bool> MonitorExceptionAsync(Guid workflowInstanceId);
        Task<bool> ResolveExceptionAsync(Guid workflowInstanceId, string resolutionDetails);
        Task<bool> CloseExceptionAsync(Guid workflowInstanceId);
        Task<WorkflowInstance?> GetExceptionStatusAsync(Guid workflowInstanceId);
        Task<List<WorkflowInstance>> GetPendingExceptionsAsync(Guid tenantId);
    }
}
