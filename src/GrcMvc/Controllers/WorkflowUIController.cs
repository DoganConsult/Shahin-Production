using GrcMvc.Services.Interfaces.Workflows;
using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrcMvc.Controllers
{
    [Authorize]
    public class WorkflowUIController : Controller
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
        private readonly IFeatureService _featureService;
        private readonly ILogger<WorkflowUIController> _logger;

        public WorkflowUIController(
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
            IFeatureService featureService,
            ILogger<WorkflowUIController> logger)
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
            _featureService = featureService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        private Guid GetTenantId() => Guid.TryParse(User.FindFirst("TenantId")?.Value, out var tid) ? tid : Guid.Empty;
        private Guid GetRbacTenantId() => GetTenantId(); // RBAC now uses Guid for TenantId

        // ===== DASHBOARD =====

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                var tenantId = GetTenantId();

                // Get visible features for user
                var rbacTenantId = GetRbacTenantId();
                var features = await _accessControl.GetUserAccessibleFeaturesAsync(userId, rbacTenantId);
                var permissions = await _accessControl.GetUserPermissionsAsync(userId, rbacTenantId);

                ViewData["UserPermissions"] = permissions;
                ViewData["VisibleFeatures"] = features;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard: {ex.Message}");
                return View();
            }
        }

        // ===== CONTROL IMPLEMENTATION WORKFLOW =====

        [HttpGet]
        [Route("control-implementation")]
        public async Task<IActionResult> ControlImplementation()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Workflow.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Control Implementation Workflow";
            return View();
        }

        [HttpGet]
        [Route("control-implementation/{id:guid}")]
        public async Task<IActionResult> ControlImplementationDetail(Guid id)
        {
            try
            {
                var workflow = await _controlWorkflow.GetWorkflowAsync(id);
                if (workflow == null)
                    return NotFound();

                var tasks = await _controlWorkflow.GetPendingTasksAsync(id);

                ViewData["Workflow"] = workflow;
                ViewData["PendingTasks"] = tasks;
                ViewData["Title"] = "Control Implementation Detail";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading control detail: {ex.Message}");
                return View("Error");
            }
        }

        // ===== APPROVAL WORKFLOW =====

        [HttpGet]
        [Route("approvals")]
        public async Task<IActionResult> Approvals()
        {
            var canApprove = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Workflow.Approve", GetRbacTenantId());

            if (!canApprove)
                return Forbid();

            ViewData["Title"] = "Approvals";
            return View();
        }

        [HttpGet]
        [Route("approval/{id:guid}")]
        public async Task<IActionResult> ApprovalDetail(Guid id)
        {
            try
            {
                var history = await _approvalWorkflow.GetApprovalHistoryAsync(id);

                ViewData["ApprovalHistory"] = history;
                ViewData["Title"] = "Approval Detail";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading approval detail: {ex.Message}");
                return View("Error");
            }
        }

        // ===== EVIDENCE COLLECTION =====

        [HttpGet]
        [Route("evidence")]
        public async Task<IActionResult> Evidence()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Evidence.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Evidence Collection";
            return View();
        }

        [HttpGet]
        [Route("evidence/{id:guid}")]
        public async Task<IActionResult> EvidenceDetail(Guid id)
        {
            try
            {
                ViewData["EvidenceId"] = id;
                ViewData["Title"] = "Evidence Detail";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading evidence detail: {ex.Message}");
                return View("Error");
            }
        }

        // ===== AUDIT WORKFLOW =====

        [HttpGet]
        [Route("audits")]
        public async Task<IActionResult> Audits()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Audit.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Audits";
            return View();
        }

        [HttpGet]
        [Route("audit/{id:guid}")]
        public async Task<IActionResult> AuditDetail(Guid id)
        {
            try
            {
                var workflow = await _auditWorkflow.GetAuditStatusAsync(id);

                ViewData["Audit"] = workflow;
                ViewData["Title"] = "Audit Detail";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading audit detail: {ex.Message}");
                return View("Error");
            }
        }

        // ===== EXCEPTION HANDLING =====

        [HttpGet]
        [Route("exceptions")]
        public async Task<IActionResult> Exceptions()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Policy.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Exceptions";
            return View();
        }

        [HttpGet]
        [Route("exception/{id:guid}")]
        public async Task<IActionResult> ExceptionDetail(Guid id)
        {
            try
            {
                ViewData["ExceptionId"] = id;
                ViewData["Title"] = "Exception Detail";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading exception detail: {ex.Message}");
                return View("Error");
            }
        }

        // ===== RISK ASSESSMENT =====

        [HttpGet]
        [Route("risks")]
        public async Task<IActionResult> Risks()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Risk.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Risk Assessment";
            return View();
        }

        // ===== COMPLIANCE TESTING =====

        [HttpGet]
        [Route("testing")]
        public async Task<IActionResult> Testing()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Control.Test", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Compliance Testing";
            return View();
        }

        // ===== REMEDIATION =====

        [HttpGet]
        [Route("remediation")]
        public async Task<IActionResult> Remediation()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Workflow.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Remediation";
            return View();
        }

        // ===== POLICY REVIEW =====

        [HttpGet]
        [Route("policies")]
        public async Task<IActionResult> Policies()
        {
            var canView = await _accessControl.CanUserPerformActionAsync(
                GetUserId(), "Policy.View", GetRbacTenantId());

            if (!canView)
                return Forbid();

            ViewData["Title"] = "Policy Management";
            return View();
        }

        // ===== TRAINING ASSIGNMENT =====

        [HttpGet]
        [Route("training")]
        public async Task<IActionResult> Training()
        {
            ViewData["Title"] = "Training Management";
            return View();
        }
    }
}
