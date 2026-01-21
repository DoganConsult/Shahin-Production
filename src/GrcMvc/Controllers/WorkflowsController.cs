using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces.Workflows;
using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrcMvc.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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
        private readonly IAccessControlService _accessControl;
        private readonly ILogger<WorkflowsController> _logger;

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
            IAccessControlService accessControl,
            ILogger<WorkflowsController> logger)
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
            _accessControl = accessControl;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        private Guid GetTenantId() => Guid.TryParse(User.FindFirst("TenantId")?.Value, out var tid) ? tid : Guid.Empty;
        private Guid GetRbacTenantId() => GetTenantId(); // RBAC now uses Guid for TenantId

        // ===== CONTROL IMPLEMENTATION WORKFLOW =====

        [HttpPost("control-implementation/initiate/{controlId:guid}")]
        [Authorize(Roles = "Admin,ComplianceOfficer,RiskManager")]
        public async Task<IActionResult> InitiateControlImplementation(Guid controlId)
        {
            try
            {
                var canCreate = await _accessControl.CanUserPerformActionAsync(
                    GetUserId(), "Control.Implement", GetRbacTenantId());

                if (!canCreate)
                    return Forbid("You don't have permission to implement controls");

                var workflow = await _controlWorkflow.InitiateControlImplementationAsync(
                    controlId, GetTenantId(), GetUserId());

                _logger.LogInformation($"Control implementation workflow initiated for control {controlId}");
                return Ok(new { message = "Workflow initiated", workflowId = workflow.Id, status = workflow.CurrentState });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initiating control implementation: {ex.Message}");
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("control-implementation/{workflowId:guid}/move-to-planning")]
        [Authorize(Roles = "Admin,ComplianceOfficer,RiskManager")]
        public async Task<IActionResult> MoveControlToPlanning(Guid workflowId, [FromBody] string notes)
        {
            try
            {
                var canEdit = await _accessControl.CanUserPerformActionAsync(
                    GetUserId(), "Control.Implement", GetRbacTenantId());

                if (!canEdit)
                    return Forbid();

                var result = await _controlWorkflow.MoveToPlanning(workflowId, notes);

                if (!result)
                    return BadRequest("Cannot transition to planning state");

                return Ok(new { message = "Moved to planning", status = "InPlanning" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("control-implementation/{workflowId:guid}/move-to-implementation")]
        [Authorize(Roles = "Admin,ComplianceOfficer,RiskManager")]
        public async Task<IActionResult> MoveControlToImplementation(Guid workflowId, [FromBody] string details)
        {
            try
            {
                var result = await _controlWorkflow.MoveToImplementation(workflowId, details);
                return result ? Ok(new { message = "Moved to implementation", status = "InImplementation" })
                             : BadRequest("Cannot transition");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("control-implementation/{workflowId:guid}/submit-for-review")]
        [Authorize(Roles = "Admin,ComplianceOfficer,RiskManager")]
        public async Task<IActionResult> SubmitControlForReview(Guid workflowId)
        {
            try
            {
                var result = await _controlWorkflow.SubmitForReview(workflowId, GetUserId());
                return result ? Ok(new { message = "Submitted for review", status = "UnderReview" })
                             : BadRequest("Cannot transition");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("control-implementation/{workflowId:guid}/approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveControlImplementation(Guid workflowId, [FromBody] string comments)
        {
            try
            {
                var canApprove = await _accessControl.CanUserApproveWorkflowAsync(GetUserId(), workflowId);
                if (!canApprove)
                    return Forbid("You don't have approval authority");

                var result = await _controlWorkflow.ApproveImplementation(workflowId, GetUserId(), comments);
                return result ? Ok(new { message = "Control approved", status = "Approved" })
                             : BadRequest("Cannot approve");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("control-implementation/{workflowId:guid}/deploy")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> DeployControl(Guid workflowId)
        {
            try
            {
                var result = await _controlWorkflow.DeployControl(workflowId);
                return result ? Ok(new { message = "Control deployed", status = "Deployed" })
                             : BadRequest("Cannot deploy");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpGet("control-implementation/{workflowId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetControlWorkflow(Guid workflowId)
        {
            try
            {
                var workflow = await _controlWorkflow.GetWorkflowAsync(workflowId);
                if (workflow == null)
                    return NotFound();

                var tasks = await _controlWorkflow.GetPendingTasksAsync(workflowId);
                return Ok(new { workflow, pendingTasks = tasks });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        // ===== APPROVAL WORKFLOW =====

        [HttpPost("approval/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitForApproval([FromBody] ApprovalSubmissionDto dto)
        {
            try
            {
                var workflow = await _approvalWorkflow.SubmitForApprovalAsync(
                    dto.EntityId, dto.EntityType, GetTenantId(), GetUserId());

                return Ok(new { message = "Submitted for approval", workflowId = workflow.Id, status = workflow.CurrentState });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("approval/{workflowId:guid}/manager-approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveAsManager(Guid workflowId, [FromBody] ApprovalDto dto)
        {
            try
            {
                await _approvalWorkflow.ApproveAsManagerAsync(workflowId, GetUserId(), dto.Comments);
                return Ok(new { message = "Approved by manager", status = "ManagerApproved" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("approval/{workflowId:guid}/manager-reject")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> RejectAsManager(Guid workflowId, [FromBody] ApprovalDto dto)
        {
            try
            {
                await _approvalWorkflow.RejectAsManagerAsync(workflowId, GetUserId(), dto.Comments);
                return Ok(new { message = "Rejected by manager", status = "Rejected" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("approval/{workflowId:guid}/compliance-approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveAsCompliance(Guid workflowId, [FromBody] ApprovalDto dto)
        {
            try
            {
                await _approvalWorkflow.ApproveAsComplianceAsync(workflowId, GetUserId(), dto.Comments);
                return Ok(new { message = "Approved by compliance", status = "ComplianceApproved" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("approval/{workflowId:guid}/request-revision")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> RequestRevision(Guid workflowId, [FromBody] ApprovalDto dto)
        {
            try
            {
                await _approvalWorkflow.RequestRevisionAsync(workflowId, GetUserId(), dto.Comments);
                return Ok(new { message = "Revision requested", status = "Submitted" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("approval/{workflowId:guid}/executive-approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveAsExecutive(Guid workflowId, [FromBody] ApprovalDto dto)
        {
            try
            {
                await _approvalWorkflow.ApproveAsExecutiveAsync(workflowId, GetUserId(), dto.Comments);
                return Ok(new { message = "Approved by executive", status = "ExecutiveApproved" });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpGet("approval/{workflowId:guid}/history")]
        [Authorize]
        public async Task<IActionResult> GetApprovalHistory(Guid workflowId)
        {
            try
            {
                var history = await _approvalWorkflow.GetApprovalHistoryAsync(workflowId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        // ===== EVIDENCE COLLECTION WORKFLOW =====

        [HttpPost("evidence/initiate/{controlId:guid}")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> InitiateEvidenceCollection(Guid controlId)
        {
            try
            {
                var canSubmit = await _accessControl.CanUserPerformActionAsync(
                    GetUserId(), "Evidence.Submit", GetRbacTenantId());

                if (!canSubmit)
                    return Forbid();

                var workflow = await _evidenceWorkflow.InitiateEvidenceCollectionAsync(
                    controlId, GetTenantId(), GetUserId());

                return Ok(new { message = "Evidence collection initiated", workflowId = workflow.Id });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("evidence/{workflowId:guid}/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitEvidence(Guid workflowId, [FromBody] EvidenceSubmissionDto dto)
        {
            try
            {
                var canSubmit = await _accessControl.CanUserPerformActionAsync(
                    GetUserId(), "Evidence.Submit", GetRbacTenantId());

                if (!canSubmit)
                    return Forbid();

                var result = await _evidenceWorkflow.SubmitEvidenceAsync(
                    workflowId, GetUserId(), dto.Description, dto.FileUrls);

                return result ? Ok(new { message = "Evidence submitted", status = "Submitted" })
                             : BadRequest("Cannot submit");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("evidence/{workflowId:guid}/approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveEvidence(Guid workflowId, [FromBody] string comments)
        {
            try
            {
                var canReview = await _accessControl.CanUserReviewEvidenceAsync(GetUserId(), GetRbacTenantId());
                if (!canReview)
                    return Forbid();

                var result = await _evidenceWorkflow.ApproveEvidenceAsync(workflowId, GetUserId(), comments);
                return result ? Ok(new { message = "Evidence approved", status = "Approved" })
                             : BadRequest("Cannot approve");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        // ===== AUDIT WORKFLOW =====

        [HttpPost("audit/initiate")]
        [Authorize(Roles = "Admin,Auditor")]
        public async Task<IActionResult> InitiateAudit([FromBody] AuditInitiationDto dto)
        {
            try
            {
                var canAudit = await _accessControl.CanUserAuditAsync(GetUserId(), GetRbacTenantId());
                if (!canAudit)
                    return Forbid();

                var workflow = await _auditWorkflow.InitiateAuditAsync(
                    dto.AuditId, GetTenantId(), GetUserId());

                return Ok(new { message = "Audit initiated", workflowId = workflow.Id });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("audit/{workflowId:guid}/create-plan")]
        [Authorize(Roles = "Admin,Auditor")]
        public async Task<IActionResult> CreateAuditPlan(Guid workflowId, [FromBody] string auditPlan)
        {
            try
            {
                var result = await _auditWorkflow.CreateAuditPlanAsync(workflowId, auditPlan);
                return result ? Ok(new { message = "Plan created", status = "PlanningPhase" })
                             : BadRequest("Cannot create plan");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("audit/{workflowId:guid}/start-fieldwork")]
        [Authorize(Roles = "Admin,Auditor")]
        public async Task<IActionResult> StartAuditFieldwork(Guid workflowId)
        {
            try
            {
                var result = await _auditWorkflow.StartFieldworkAsync(workflowId);
                return result ? Ok(new { message = "Fieldwork started", status = "FieldworkInProgress" })
                             : BadRequest("Cannot start fieldwork");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("audit/{workflowId:guid}/submit-draft-report")]
        [Authorize(Roles = "Admin,Auditor")]
        public async Task<IActionResult> SubmitAuditDraftReport(Guid workflowId, [FromBody] string report)
        {
            try
            {
                var result = await _auditWorkflow.SubmitDraftReportAsync(workflowId, report);
                return result ? Ok(new { message = "Draft report submitted", status = "DraftReportIssued" })
                             : BadRequest("Cannot submit report");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpGet("audit/{workflowId:guid}/status")]
        [Authorize]
        public async Task<IActionResult> GetAuditStatus(Guid workflowId)
        {
            try
            {
                var workflow = await _auditWorkflow.GetAuditStatusAsync(workflowId);
                return Ok(workflow);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        // ===== EXCEPTION HANDLING WORKFLOW =====

        [HttpPost("exception/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitException([FromBody] ExceptionSubmissionDto dto)
        {
            try
            {
                var workflow = await _exceptionWorkflow.SubmitExceptionAsync(
                    GetTenantId(), GetUserId(), dto.Description, dto.Justification);

                return Ok(new { message = "Exception submitted", workflowId = workflow.Id });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("exception/{workflowId:guid}/approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveException(Guid workflowId, [FromBody] ExceptionApprovalDto dto)
        {
            try
            {
                var result = await _exceptionWorkflow.ApproveExceptionAsync(
                    workflowId, GetUserId(), dto.ApprovalConditions);

                return result ? Ok(new { message = "Exception approved", status = "Approved" })
                             : BadRequest("Cannot approve");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        [HttpPost("exception/{workflowId:guid}/reject")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> RejectException(Guid workflowId, [FromBody] ExceptionApprovalDto dto)
        {
            try
            {
                var result = await _exceptionWorkflow.RejectExceptionAsync(
                    workflowId, GetUserId(), dto.ApprovalConditions);

                return result ? Ok(new { message = "Exception rejected", status = "RejectedWithExplanation" })
                             : BadRequest("Cannot reject");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }

        // ===== UTILITY ENDPOINTS =====

        [HttpGet("list/{workflowType}")]
        [Authorize]
        public async Task<IActionResult> ListWorkflows(string workflowType)
        {
            try
            {
                // This would typically query the database for workflows of a specific type
                // For now, returning a mock structure
                return Ok(new { message = $"Workflows of type {workflowType}", workflows = new List<object>() });
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred processing your request.");
            }
        }
    }

    // ===== DTOs =====

    public class ApprovalSubmissionDto
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
    }

    public class ApprovalDto
    {
        public string Comments { get; set; }
    }

    public class EvidenceSubmissionDto
    {
        public string Description { get; set; }
        public List<string> FileUrls { get; set; }
    }

    public class AuditInitiationDto
    {
        public Guid AuditId { get; set; }
    }

    public class ExceptionSubmissionDto
    {
        public string Description { get; set; }
        public string Justification { get; set; }
    }

    public class ExceptionApprovalDto
    {
        public string ApprovalConditions { get; set; }
    }
}
