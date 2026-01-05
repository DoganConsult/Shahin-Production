using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs.Workflows;
using GrcMvc.Resources;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.Extensions.Localization;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for all workflow operations
    /// Handles all 10 workflow types with complete CRUD and state management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowsController : ControllerBase
    {
        private readonly IControlImplementationWorkflowService _controlWorkflow;
        private readonly IRiskAssessmentWorkflowService _riskWorkflow;
        private readonly IApprovalWorkflowService _approvalWorkflow;
        private readonly IEvidenceCollectionWorkflowService _evidenceWorkflow;
        private readonly IComplianceTestingWorkflowService _testingWorkflow;
        private readonly IRemediationWorkflowService _remediationWorkflow;
        private readonly IPolicyReviewWorkflowService _policyWorkflow;
        private readonly ITrainingAssignmentWorkflowService _trainingWorkflow;
        private readonly IAuditWorkflowService _auditWorkflow;
        private readonly IExceptionHandlingWorkflowService _exceptionWorkflow;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<WorkflowsController> _logger;
        private readonly GrcDbContext _context;

        public WorkflowsController(
            IControlImplementationWorkflowService controlWorkflow,
            IRiskAssessmentWorkflowService riskWorkflow,
            IApprovalWorkflowService approvalWorkflow,
            IEvidenceCollectionWorkflowService evidenceWorkflow,
            IComplianceTestingWorkflowService testingWorkflow,
            IRemediationWorkflowService remediationWorkflow,
            IPolicyReviewWorkflowService policyWorkflow,
            ITrainingAssignmentWorkflowService trainingWorkflow,
            IAuditWorkflowService auditWorkflow,
            IExceptionHandlingWorkflowService exceptionWorkflow,
            ILogger<WorkflowsController> logger,
            GrcDbContext context,
            IStringLocalizer<SharedResource> localizer)
        {
            _controlWorkflow = controlWorkflow;
            _riskWorkflow = riskWorkflow;
            _approvalWorkflow = approvalWorkflow;
            _evidenceWorkflow = evidenceWorkflow;
            _testingWorkflow = testingWorkflow;
            _remediationWorkflow = remediationWorkflow;
            _policyWorkflow = policyWorkflow;
            _trainingWorkflow = trainingWorkflow;
            _auditWorkflow = auditWorkflow;
            _exceptionWorkflow = exceptionWorkflow;
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }

        // ===== CONTROL IMPLEMENTATION WORKFLOWS =====

        private Guid GetTenantId() => Guid.TryParse(User.FindFirst("TenantId")?.Value, out var tid) ? tid : Guid.Empty;

        /// <summary>Initiate control implementation workflow</summary>
        [HttpPost("control/implement/{controlId:guid}")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> InitiateControlImplementation(Guid controlId)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _controlWorkflow.InitiateControlImplementationAsync(controlId, tenantId, userId);
            _logger.LogInformation($"Control implementation workflow initiated for control {controlId}");

            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("control/{workflowId:guid}/planning")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> MoveControlToPlanning(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var result = await _controlWorkflow.MoveToPlanning(workflowId, action.Notes);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("control/{workflowId:guid}/implementation")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> MoveControlToImplementation(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var result = await _controlWorkflow.MoveToImplementation(workflowId, action.Details);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("control/{workflowId:guid}/submit-review")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> SubmitControlForReview(Guid workflowId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _controlWorkflow.SubmitForReview(workflowId, userId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("control/{workflowId:guid}/approve")]
        [Authorize(Roles = "Admin,ControlReviewer")]
        public async Task<IActionResult> ApproveControl(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _controlWorkflow.ApproveImplementation(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("control/{workflowId:guid}/deploy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeployControl(Guid workflowId)
        {
            var result = await _controlWorkflow.DeployControl(workflowId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("control/{workflowId:guid}/monitor")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> StartControlMonitoring(Guid workflowId)
        {
            var result = await _controlWorkflow.StartMonitoring(workflowId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpGet("control/{workflowId:guid}")]
        [Authorize]
        public async Task<ActionResult<WorkflowInstanceDto>> GetControlWorkflow(Guid workflowId)
        {
            var workflow = await _controlWorkflow.GetWorkflowAsync(workflowId);
            return workflow != null ? Ok(MapWorkflowToDto(workflow)) : NotFound(_localizer["Error_NotFound"]);
        }

        // ===== RISK ASSESSMENT WORKFLOWS =====

        [HttpPost("risk/assess/{riskId:guid}")]
        [Authorize(Roles = "RiskManager,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> InitiateRiskAssessment(Guid riskId)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _riskWorkflow.InitiateRiskAssessmentAsync(riskId, tenantId, userId);
            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("risk/{workflowId:guid}/start-assessment")]
        [Authorize(Roles = "RiskManager,Admin")]
        public async Task<IActionResult> StartRiskAssessment(Guid workflowId)
        {
            var result = await _riskWorkflow.StartDataGatheringAsync(workflowId);
            return result ? Ok(_localizer["Success_Created"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("risk/{workflowId:guid}/submit-analysis")]
        [Authorize(Roles = "RiskManager,Admin")]
        public async Task<IActionResult> SubmitRiskAnalysis(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var result = await _riskWorkflow.SubmitAnalysisAsync(workflowId, action.Details);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpGet("risk/{workflowId:guid}")]
        [Authorize]
        public async Task<ActionResult<WorkflowInstanceDto>> GetRiskWorkflow(Guid workflowId)
        {
            var workflow = await _riskWorkflow.GetAssessmentStatusAsync(workflowId);
            return workflow != null ? Ok(MapWorkflowToDto(workflow)) : NotFound(_localizer["Error_NotFound"]);
        }

        // ===== APPROVAL WORKFLOWS =====

        [HttpPost("approval/submit")]
        [Authorize]
        public async Task<ActionResult<WorkflowInstanceDto>> SubmitForApproval([FromBody] ApprovalSubmissionDto submission)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _approvalWorkflow.SubmitForApprovalAsync(
                submission.EntityId, submission.EntityType, tenantId, userId);

            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("approval/{workflowId:guid}/approve-manager")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveAsManager(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _approvalWorkflow.ApproveAsManagerAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("approval/{workflowId:guid}/approve-compliance")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> ApproveAsCompliance(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _approvalWorkflow.ApproveAsComplianceAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("approval/{workflowId:guid}/approve-executive")]
        [Authorize(Roles = "Executive,Admin")]
        public async Task<IActionResult> ApproveAsExecutive(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _approvalWorkflow.ApproveAsExecutiveAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("approval/{workflowId:guid}/reject")]
        [Authorize(Roles = "Manager,ComplianceOfficer,Executive,Admin")]
        public async Task<IActionResult> RejectApproval(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _approvalWorkflow.RejectAsManagerAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpGet("approval/{workflowId:guid}/history")]
        [Authorize]
        public async Task<ActionResult<List<WorkflowApprovalDto>>> GetApprovalHistory(Guid workflowId)
        {
            var approvals = await _approvalWorkflow.GetApprovalHistoryAsync(workflowId);
            var dtos = approvals.Select(a => new WorkflowApprovalDto
            {
                ApprovalLevel = a.ApprovalLevel,
                Decision = a.Decision,
                Comments = a.Comments,
                ApprovedAt = a.ApprovedAt
            }).ToList();

            return Ok(dtos);
        }

        // ===== EVIDENCE COLLECTION WORKFLOWS =====

        [HttpPost("evidence/collect/{controlId:guid}")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> InitiateEvidenceCollection(Guid controlId)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _evidenceWorkflow.InitiateEvidenceCollectionAsync(controlId, tenantId, userId);
            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("evidence/{workflowId:guid}/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitEvidence(Guid workflowId, [FromBody] EvidenceSubmissionDto submission)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _evidenceWorkflow.SubmitEvidenceAsync(
                workflowId, userId, submission.Description, submission.FileUrls);

            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("evidence/{workflowId:guid}/approve")]
        [Authorize(Roles = "ComplianceOfficer,Admin")]
        public async Task<IActionResult> ApproveEvidence(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _evidenceWorkflow.ApproveEvidenceAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== COMPLIANCE TESTING WORKFLOWS =====

        [HttpPost("testing/initiate/{controlId:guid}")]
        [Authorize(Roles = "Auditor,ComplianceOfficer,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> InitiateComplianceTest(Guid controlId)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _testingWorkflow.InitiateComplianceTestAsync(controlId, tenantId, userId);
            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("testing/{workflowId:guid}/complete")]
        [Authorize(Roles = "Auditor,ComplianceOfficer,Admin")]
        public async Task<IActionResult> CompleteComplianceTest(Guid workflowId, [FromBody] TestResultsDto results)
        {
            var outcome = await _testingWorkflow.CompleteTestExecutionAsync(workflowId, results.Results);
            return outcome ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("testing/{workflowId:guid}/mark-compliant")]
        [Authorize(Roles = "Auditor,Admin")]
        public async Task<IActionResult> MarkAsCompliant(Guid workflowId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _testingWorkflow.MarkAsCompliantAsync(workflowId, userId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("testing/{workflowId:guid}/mark-non-compliant")]
        [Authorize(Roles = "Auditor,Admin")]
        public async Task<IActionResult> MarkAsNonCompliant(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _testingWorkflow.MarkAsNonCompliantAsync(workflowId, userId, action.Details);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== REMEDIATION WORKFLOWS =====

        [HttpPost("remediation/identify")]
        [Authorize(Roles = "RiskManager,Auditor,ComplianceOfficer,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> IdentifyRemediation([FromBody] RemediationIdentificationDto identification)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _remediationWorkflow.IdentifyRemediationAsync(
                identification.IssueId, tenantId, userId, identification.Description);

            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("remediation/{workflowId:guid}/start")]
        [Authorize(Roles = "RiskManager,Admin")]
        public async Task<IActionResult> StartRemediation(Guid workflowId)
        {
            var result = await _remediationWorkflow.StartRemediationAsync(workflowId);
            return result ? Ok(_localizer["Success_Created"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("remediation/{workflowId:guid}/verify")]
        [Authorize(Roles = "Auditor,ComplianceOfficer,Admin")]
        public async Task<IActionResult> VerifyRemediation(Guid workflowId, [FromBody] RemediationVerificationDto verification)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _remediationWorkflow.VerifyRemediationAsync(
                workflowId, userId, verification.IsSuccessful);

            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== POLICY REVIEW WORKFLOWS =====

        [HttpPost("policy/review/{policyId:guid}")]
        [Authorize(Roles = "Admin,PolicyOwner")]
        public async Task<ActionResult<WorkflowInstanceDto>> SchedulePolicyReview(Guid policyId, [FromBody] PolicyReviewScheduleDto schedule)
        {
            var tenantId = GetTenantId();

            var workflow = await _policyWorkflow.SchedulePolicyReviewAsync(policyId, tenantId, schedule.ReviewDate);
            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("policy/{workflowId:guid}/publish")]
        [Authorize(Roles = "Admin,PolicyOwner")]
        public async Task<IActionResult> PublishPolicy(Guid workflowId)
        {
            var result = await _policyWorkflow.PublishPolicyAsync(workflowId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== TRAINING ASSIGNMENT WORKFLOWS =====

        [HttpPost("training/assign")]
        [Authorize(Roles = "HRManager,ComplianceOfficer,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> AssignTraining([FromBody] TrainingAssignmentDto assignment)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _trainingWorkflow.AssignTrainingAsync(
                assignment.EmployeeId, assignment.TrainingModuleId, tenantId, userId);

            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("training/{workflowId:guid}/complete")]
        [Authorize]
        public async Task<IActionResult> CompleteTraining(Guid workflowId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _trainingWorkflow.CompleteTrainingAsync(workflowId, userId);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("training/{workflowId:guid}/pass")]
        [Authorize(Roles = "Trainer,ComplianceOfficer,Admin")]
        public async Task<IActionResult> PassTraining(Guid workflowId, [FromBody] TrainingResultDto result)
        {
            var outcome = await _trainingWorkflow.MarkAsPassedAsync(workflowId, result.Score);
            return outcome ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== AUDIT WORKFLOWS =====

        [HttpPost("audit/initiate/{auditId:guid}")]
        [Authorize(Roles = "Auditor,Admin")]
        public async Task<ActionResult<WorkflowInstanceDto>> InitiateAudit(Guid auditId)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _auditWorkflow.InitiateAuditAsync(auditId, tenantId, userId);
            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("audit/{workflowId:guid}/issue-report")]
        [Authorize(Roles = "Auditor,Admin")]
        public async Task<IActionResult> IssueAuditReport(Guid workflowId, [FromBody] AuditReportDto report)
        {
            var result = await _auditWorkflow.IssueFinalReportAsync(workflowId, report.Report);
            return result ? Ok(_localizer["Success_Created"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== EXCEPTION HANDLING WORKFLOWS =====

        [HttpPost("exception/submit")]
        [Authorize]
        public async Task<ActionResult<WorkflowInstanceDto>> SubmitException([FromBody] ExceptionSubmissionDto submission)
        {
            var tenantId = GetTenantId();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var workflow = await _exceptionWorkflow.SubmitExceptionAsync(
                tenantId, userId, submission.Description, submission.Justification);

            return Ok(MapWorkflowToDto(workflow));
        }

        [HttpPut("exception/{workflowId:guid}/approve")]
        [Authorize(Roles = "Admin,ExceptionApprover")]
        public async Task<IActionResult> ApproveException(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _exceptionWorkflow.ApproveExceptionAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        [HttpPut("exception/{workflowId:guid}/reject")]
        [Authorize(Roles = "Admin,ExceptionApprover")]
        public async Task<IActionResult> RejectException(Guid workflowId, [FromBody] WorkflowActionDto action)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _exceptionWorkflow.RejectExceptionAsync(workflowId, userId, action.Comments);
            return result ? Ok(_localizer["Success_Updated"]) : BadRequest(_localizer["Error_BadRequest"]);
        }

        // ===== HELPER METHODS =====

        /// <summary>Get all pending workflows for user</summary>
        [HttpGet("pending")]
        [Authorize]
        public async Task<ActionResult<List<WorkflowInstanceDto>>> GetPendingWorkflows()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var tenantId = GetTenantId();

            var userGuid = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;
            var tasks = await _context.WorkflowTasks
                .Where(t => t.AssignedToUserId == userGuid && t.Status == "Pending")
                .ToListAsync();

            var workflows = await _context.WorkflowInstances
                .Where(w => w.TenantId == tenantId && w.Status == "Active")
                .ToListAsync();

            return Ok(workflows.Select(MapWorkflowToDto).ToList());
        }

        /// <summary>Get dashboard statistics</summary>
        [HttpGet("dashboard")]
        [Authorize]
        public async Task<ActionResult<WorkflowDashboardDto>> GetWorkflowDashboard()
        {
            var tenantId = GetTenantId();

            var active = await _context.WorkflowInstances
                .CountAsync(w => w.TenantId == tenantId && w.Status == "Active");

            var completed = await _context.WorkflowInstances
                .CountAsync(w => w.TenantId == tenantId && w.Status == "Completed");

            var pending = await _context.WorkflowTasks
                .CountAsync(t => t.Status == "Pending");

            return Ok(new WorkflowDashboardDto
            {
                ActiveWorkflows = active,
                CompletedWorkflows = completed,
                PendingTasks = pending,
                CompletionRate = active > 0 ? (decimal)completed / (completed + active) * 100 : 0
            });
        }

        private WorkflowInstanceDto MapWorkflowToDto(WorkflowInstance workflow)
        {
            return new WorkflowInstanceDto
            {
                Id = workflow.Id,
                WorkflowType = workflow.WorkflowType,
                EntityType = workflow.EntityType,
                EntityId = workflow.EntityId,
                CurrentState = workflow.CurrentState,
                Status = workflow.Status,
                CreatedAt = workflow.StartedAt,
                CompletedAt = workflow.CompletedAt
            };
        }
    }
}
