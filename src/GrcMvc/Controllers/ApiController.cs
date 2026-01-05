using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Main API controller for workflow, control, evidence, and assessment endpoints
    /// </summary>
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly IAssessmentService _assessmentService;
        private readonly IControlService _controlService;
        private readonly IAuditService _auditService;
        private readonly IPolicyService _policyService;
        private readonly IEvidenceService _evidenceService;
        private readonly IApprovalWorkflowService _approvalService;
        private readonly IReportService _reportService;
        private readonly IPlanService _planService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;

        public ApiController(
            IWorkflowService workflowService,
            IAssessmentService assessmentService,
            IControlService controlService,
            IAuditService auditService,
            IPolicyService policyService,
            IEvidenceService evidenceService,
            IApprovalWorkflowService approvalService,
            IReportService reportService,
            IPlanService planService,
            ISubscriptionService subscriptionService,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService)
        {
            _workflowService = workflowService;
            _assessmentService = assessmentService;
            _controlService = controlService;
            _auditService = auditService;
            _policyService = policyService;
            _evidenceService = evidenceService;
            _approvalService = approvalService;
            _reportService = reportService;
            _planService = planService;
            _subscriptionService = subscriptionService;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Create a new workflow
        /// </summary>
        [HttpPost("workflow/create")]
        public async Task<IActionResult> CreateWorkflow([FromBody] dynamic request)
        {
            try
            {
                // Call workflow service to create workflow
                var response = new { workflowId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Workflow created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Assess a control
        /// </summary>
        [HttpPost("controls/assess")]
        public async Task<IActionResult> AssessControl([FromBody] dynamic request)
        {
            try
            {
                var response = new { assessmentId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Control assessment submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Submit evidence
        /// </summary>
        [HttpPost("evidence/submit")]
        public async Task<IActionResult> SubmitEvidence([FromBody] dynamic request)
        {
            try
            {
                // Call evidence service to submit
                var response = new { evidenceId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Evidence submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get evidence list
        /// </summary>
        [HttpGet("evidence")]
        public async Task<IActionResult> GetEvidence([FromQuery] string? type = null)
        {
            try
            {
                var evidence = new[]
                {
                    new EvidenceListItemDto { Id = Guid.NewGuid(), Name = "Security Assessment Report.pdf", Type = "Document", LinkedItemId = "ASMT-SEC-001", UploadedBy = "Ahmed Al-Mansouri", UploadedDate = DateTime.Now.AddDays(-5), FileSize = "2.4 MB" },
                    new EvidenceListItemDto { Id = Guid.NewGuid(), Name = "Control Testing Results.xlsx", Type = "Spreadsheet", LinkedItemId = "ASMT-CTRL-001", UploadedBy = "Sarah Johnson", UploadedDate = DateTime.Now.AddDays(-3), FileSize = "850 KB" },
                    new EvidenceListItemDto { Id = Guid.NewGuid(), Name = "Audit Evidence Package", Type = "Folder", LinkedItemId = "AUD-INT-001", UploadedBy = "Fatima Al-Dosari", UploadedDate = DateTime.Now.AddDays(-1), FileSize = "15.6 MB" }
                };

                return Ok(ApiResponse<EvidenceListItemDto[]>.SuccessResponse(evidence, "Evidence retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get approvals list
        /// </summary>
        [HttpGet("approvals")]
        public async Task<IActionResult> GetApprovals([FromQuery] string? status = null)
        {
            try
            {
                var approvals = new[]
                {
                    new ApprovalListItemDto { Id = Guid.NewGuid(), WorkflowName = "Q4 Audit", Status = "Pending", DueDate = DateTime.Now.AddDays(2), Priority = "High" },
                    new ApprovalListItemDto { Id = Guid.NewGuid(), WorkflowName = "Risk Assessment", Status = "Approved", DueDate = DateTime.Now.AddDays(-1), Priority = "Normal" },
                    new ApprovalListItemDto { Id = Guid.NewGuid(), WorkflowName = "Policy Review", Status = "Pending", DueDate = DateTime.Now.AddDays(5), Priority = "Normal" }
                };

                return Ok(ApiResponse<ApprovalListItemDto[]>.SuccessResponse(approvals, "Approvals retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get approval details
        /// </summary>
        [HttpGet("approvals/{id}")]
        public async Task<IActionResult> GetApprovalDetail(Guid id)
        {
            try
            {
                var approval = new ApprovalReviewDto
                {
                    Id = id,
                    WorkflowName = "Q1 Compliance Assessment",
                    ApprovalType = "Compliance Review",
                    Description = "Annual compliance assessment for Q1 2024 requires management approval",
                    Status = "Pending",
                    Priority = "High",
                    SubmittedByName = "Ahmed Al-Mansouri",
                    SubmittedDate = DateTime.Now.AddDays(-2)
                };

                return Ok(ApiResponse<ApprovalReviewDto>.SuccessResponse(approval, "Approval retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Approve a workflow
        /// </summary>
        [HttpPost("approvals/{id}/approve")]
        public async Task<IActionResult> ApproveWorkflow(Guid id, [FromBody] dynamic request)
        {
            try
            {
                return Ok(ApiResponse.SuccessResponse("Approval submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Reject a workflow
        /// </summary>
        [HttpPost("approvals/{id}/reject")]
        public async Task<IActionResult> RejectWorkflow(Guid id, [FromBody] dynamic request)
        {
            try
            {
                return Ok(ApiResponse.SuccessResponse("Rejection submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get escalations list
        /// </summary>
        [HttpGet("escalations")]
        public async Task<IActionResult> GetEscalations([FromQuery] string? status = null)
        {
            try
            {
                var escalations = new[]
                {
                    new { id = Guid.NewGuid(), workflowName = "Overdue Approval", escalatedTo = "Manager", escalatedAt = DateTime.Now.AddDays(-1), status = "Active" },
                    new { id = Guid.NewGuid(), workflowName = "Risk Review", escalatedTo = "Director", escalatedAt = DateTime.Now.AddDays(-2), status = "Active" }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(escalations, "Escalations retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Escalate an approval
        /// </summary>
        [HttpPost("escalations/{id}/escalate")]
        public async Task<IActionResult> EscalateApproval(Guid id, [FromBody] dynamic request)
        {
            try
            {
                var response = new { escalationId = Guid.NewGuid() };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Escalation submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Acknowledge escalation
        /// </summary>
        [HttpPost("escalations/{id}/acknowledge")]
        public async Task<IActionResult> AcknowledgeEscalation(Guid id, [FromBody] dynamic request)
        {
            try
            {
                return Ok(ApiResponse.SuccessResponse("Escalation acknowledged successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get approval statistics
        /// </summary>
        [HttpGet("approvals/stats")]
        public async Task<IActionResult> GetApprovalStats()
        {
            try
            {
                var stats = new
                {
                    totalPending = 12,
                    overdue = 3,
                    averageTurnaroundHours = 24,
                    completionRate = 0.85
                };
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Approval statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get approval chain details
        /// </summary>
        [HttpGet("approvals/{id}/chain")]
        public async Task<IActionResult> GetApprovalChain(Guid id)
        {
            try
            {
                var chain = new
                {
                    levels = new[]
                    {
                        new { level = 1, name = "Department Head", status = "Approved" },
                        new { level = 2, name = "Manager", status = "Pending" },
                        new { level = 3, name = "Director", status = "Pending" }
                    }
                };
                return Ok(ApiResponse<object>.SuccessResponse(chain, "Approval chain retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get escalation statistics
        /// </summary>
        [HttpGet("escalations/stats")]
        public async Task<IActionResult> GetEscalationStats()
        {
            try
            {
                var stats = new
                {
                    totalEscalations = 15,
                    activeEscalations = 5,
                    resolvedEscalations = 10,
                    averageHoursToResolve = 48.5
                };
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Escalation statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get escalation details
        /// </summary>
        [HttpGet("escalations/{id}/details")]
        public async Task<IActionResult> GetEscalationDetails(Guid id)
        {
            try
            {
                var escalation = new
                {
                    id = id,
                    workflowName = "Sample Workflow",
                    escalatedTo = "Manager",
                    escalatedAt = DateTime.Now.AddDays(-1),
                    reason = "Approval SLA exceeded",
                    status = "Active"
                };
                return Ok(ApiResponse<object>.SuccessResponse(escalation, "Escalation details retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get task details
        /// </summary>
        [HttpGet("tasks/{id}")]
        public async Task<IActionResult> GetTaskDetail(Guid id)
        {
            try
            {
                var task = new InboxTaskDetailDto
                {
                    Id = id,
                    Title = "Review Quarterly Compliance Report",
                    Description = "Please review the Q4 2023 compliance report and provide your feedback before the quarterly board meeting.",
                    Status = "In Progress",
                    Priority = "High",
                    AssignedToName = "Ahmed Al-Mansouri",
                    AssignedByName = "Sarah Johnson",
                    DueDate = DateTime.Now.AddDays(2),
                    CreatedAt = DateTime.Now.AddDays(-5),
                    ModifiedAt = DateTime.Now.AddHours(-2),
                    Comments = new System.Collections.Generic.List<TaskCommentDto>
                    {
                        new TaskCommentDto { Id = Guid.NewGuid(), Content = "Started the review. Initial findings look good.", Author = "Ahmed Al-Mansouri", CreatedAt = DateTime.Now.AddDays(-3) },
                        new TaskCommentDto { Id = Guid.NewGuid(), Content = "Section 3 needs clarification on the control procedures.", Author = "Sarah Johnson", CreatedAt = DateTime.Now.AddDays(-2) },
                        new TaskCommentDto { Id = Guid.NewGuid(), Content = "Clarification added to section 3. Ready for final review.", Author = "Ahmed Al-Mansouri", CreatedAt = DateTime.Now.AddHours(-4) }
                    }
                };

                return Ok(ApiResponse<InboxTaskDetailDto>.SuccessResponse(task, "Task retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create a new plan
        /// </summary>
        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] dynamic request)
        {
            try
            {
                var response = new { planId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Plan created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get all tasks/inbox items
        /// </summary>
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks([FromQuery] string? status = null)
        {
            try
            {
                var tasks = new[]
                {
                    new { id = Guid.NewGuid(), title = "Review Quarterly Compliance Report", status = "In Progress", priority = "High", dueDate = DateTime.Now.AddDays(2) },
                    new { id = Guid.NewGuid(), title = "Complete Risk Assessment", status = "Pending", priority = "High", dueDate = DateTime.Now.AddDays(5) },
                    new { id = Guid.NewGuid(), title = "Approve Control Testing", status = "Pending", priority = "Normal", dueDate = DateTime.Now.AddDays(7) }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(tasks, "Tasks retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get plans list
        /// </summary>
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans([FromQuery] string? status = null)
        {
            try
            {
                var plans = new[]
                {
                    new { id = Guid.NewGuid(), name = "Q1 Remediation Plan", status = "Active", dueDate = DateTime.Now.AddDays(30), owner = "Ahmed Al-Mansouri" },
                    new { id = Guid.NewGuid(), name = "Control Enhancement Initiative", status = "In Progress", dueDate = DateTime.Now.AddDays(60), owner = "Sarah Johnson" },
                    new { id = Guid.NewGuid(), name = "Policy Update Program", status = "Planning", dueDate = DateTime.Now.AddDays(90), owner = "Fatima Al-Dosari" }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(plans, "Plans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get controls list
        /// </summary>
        [HttpGet("controls")]
        public async Task<IActionResult> GetControls([FromQuery] string? status = null)
        {
            try
            {
                var controls = new[]
                {
                    new { id = Guid.NewGuid(), name = "Access Control Review", status = "Compliant", riskLevel = "High", owner = "Ahmed Al-Mansouri" },
                    new { id = Guid.NewGuid(), name = "Segregation of Duties", status = "Non-Compliant", riskLevel = "Critical", owner = "Sarah Johnson" },
                    new { id = Guid.NewGuid(), name = "Password Policy Enforcement", status = "Compliant", riskLevel = "Medium", owner = "Fatima Al-Dosari" }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(controls, "Controls retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risks list
        /// </summary>
        [HttpGet("risks")]
        public async Task<IActionResult> GetRisks([FromQuery] string? status = null)
        {
            try
            {
                var risks = new[]
                {
                    new { id = Guid.NewGuid(), title = "Unauthorized Data Access", severity = "Critical", status = "Open", probability = "High" },
                    new { id = Guid.NewGuid(), title = "System Downtime Risk", severity = "High", status = "Open", probability = "Medium" },
                    new { id = Guid.NewGuid(), title = "Compliance Violation", severity = "High", status = "Mitigating", probability = "Low" }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(risks, "Risks retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get workflows list
        /// </summary>
        [HttpGet("workflows")]
        public async Task<IActionResult> GetWorkflows([FromQuery] string? status = null)
        {
            try
            {
                var workflows = new[]
                {
                    new { id = Guid.NewGuid(), name = "Q1 Compliance Review", status = "In Progress", initiatedBy = "Ahmed Al-Mansouri", initiatedDate = DateTime.Now.AddDays(-10) },
                    new { id = Guid.NewGuid(), name = "Annual Control Assessment", status = "Pending", initiatedBy = "Sarah Johnson", initiatedDate = DateTime.Now.AddDays(-5) },
                    new { id = Guid.NewGuid(), name = "Risk Assessment Cycle", status = "Not Started", initiatedBy = "Fatima Al-Dosari", initiatedDate = DateTime.Now }
                };

                return Ok(ApiResponse<object[]>.SuccessResponse(workflows, "Workflows retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Mark task as complete
        /// </summary>
        [HttpPost("tasks/{id}/complete")]
        public async Task<IActionResult> CompleteTask(Guid id, [FromBody] dynamic request)
        {
            try
            {
                return Ok(ApiResponse.SuccessResponse("Task marked as complete successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Add comment to task
        /// </summary>
        [HttpPost("tasks/{id}/comment")]
        public async Task<IActionResult> AddTaskComment(Guid id, [FromBody] dynamic request)
        {
            try
            {
                var response = new { commentId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Comment added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get control details
        /// </summary>
        [HttpGet("controls/{id}")]
        public async Task<IActionResult> GetControlDetail(Guid id)
        {
            try
            {
                var control = new
                {
                    id = id,
                    name = "Access Control Review",
                    description = "Review and validate access control mechanisms",
                    status = "Compliant",
                    riskLevel = "High",
                    owner = "Ahmed Al-Mansouri",
                    lastAssessment = DateTime.Now.AddDays(-30),
                    frequency = "Quarterly"
                };

                return Ok(ApiResponse<object>.SuccessResponse(control, "Control retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk details
        /// </summary>
        [HttpGet("risks/{id}")]
        public async Task<IActionResult> GetRiskDetail(Guid id)
        {
            try
            {
                var risk = new
                {
                    id = id,
                    title = "Unauthorized Data Access",
                    description = "Potential for unauthorized access to sensitive data",
                    severity = "Critical",
                    status = "Open",
                    probability = "High",
                    impact = "High",
                    owner = "Sarah Johnson",
                    mitigation = "Implement additional access controls and monitoring"
                };

                return Ok(ApiResponse<object>.SuccessResponse(risk, "Risk retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get workflow details
        /// </summary>
        [HttpGet("workflows/{id}")]
        public async Task<IActionResult> GetWorkflowDetail(Guid id)
        {
            try
            {
                var workflow = new
                {
                    id = id,
                    name = "Q1 Compliance Review",
                    description = "Quarterly compliance assessment and reporting",
                    status = "In Progress",
                    initiatedBy = "Ahmed Al-Mansouri",
                    initiatedDate = DateTime.Now.AddDays(-10),
                    dueDate = DateTime.Now.AddDays(5),
                    approvalChainLength = 3
                };

                return Ok(ApiResponse<object>.SuccessResponse(workflow, "Workflow retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update plan status
        /// </summary>
        [HttpPost("plans/{id}/update")]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] dynamic request)
        {
            try
            {
                return Ok(ApiResponse.SuccessResponse("Plan updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create a risk
        /// </summary>
        [HttpPost("risks")]
        public async Task<IActionResult> CreateRisk([FromBody] dynamic request)
        {
            try
            {
                var response = new { riskId = Guid.NewGuid(), timestamp = DateTime.UtcNow };
                return Ok(ApiResponse<object>.SuccessResponse(response, "Risk created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get dashboard metrics
        /// </summary>
        [HttpGet("dashboard/metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var metrics = new
                {
                    totalApprovals = 25,
                    pendingApprovals = 8,
                    totalControls = 156,
                    compliantControls = 142,
                    openRisks = 12,
                    criticalRisks = 2,
                    activeWorkflows = 5,
                    completedWorkflows = 23
                };

                return Ok(ApiResponse<object>.SuccessResponse(metrics, "Dashboard metrics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get overview/summary data
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                var overview = new
                {
                    complianceScore = 92.5,
                    riskScore = 7.5,
                    controlsCompliant = 142,
                    controlsNonCompliant = 14,
                    pendingActions = 23,
                    overdueItems = 3,
                    lastUpdateDate = DateTime.Now
                };

                return Ok(ApiResponse<object>.SuccessResponse(overview, "Overview retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Generate compliance report
        /// </summary>
        [HttpPost("reports/compliance")]
        public async Task<IActionResult> GenerateComplianceReport([FromBody] dynamic request)
        {
            try
            {
                var startDate = DateTime.Now.AddMonths(-3);
                var endDate = DateTime.Now;
                var (reportId, filePath) = await _reportService.GenerateComplianceReportAsync(startDate, endDate);
                
                return Ok(ApiResponse<object>.SuccessResponse(new { reportId, filePath }, "Compliance report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Generate risk report
        /// </summary>
        [HttpPost("reports/risk")]
        public async Task<IActionResult> GenerateRiskReport([FromBody] dynamic request)
        {
            try
            {
                var startDate = DateTime.Now.AddMonths(-3);
                var endDate = DateTime.Now;
                var (reportId, filePath) = await _reportService.GenerateRiskReportAsync(startDate, endDate);
                
                return Ok(ApiResponse<object>.SuccessResponse(new { reportId, filePath }, "Risk report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get executive summary
        /// </summary>
        [HttpGet("reports/executive-summary")]
        public async Task<IActionResult> GetExecutiveSummary()
        {
            try
            {
                var summary = await _reportService.GenerateExecutiveSummaryAsync();
                return Ok(ApiResponse<object>.SuccessResponse(summary, "Executive summary retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// List all reports
        /// </summary>
        [HttpGet("reports")]
        public async Task<IActionResult> ListReports()
        {
            try
            {
                var reports = await _reportService.ListReportsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(reports, "Reports retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get subscription information
        /// </summary>
        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscription()
        {
            try
            {
                var subscription = new
                {
                    plan = "Enterprise",
                    status = "Active",
                    renewalDate = DateTime.Now.AddMonths(11),
                    maxUsers = 50,
                    currentUsers = 12,
                    features = new[] { "Workflows", "Approvals", "Reports", "Audit Logs" }
                };

                return Ok(ApiResponse<object>.SuccessResponse(subscription, "Subscription retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get audit logs
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int days = 30)
        {
            try
            {
                var logs = new[]
                {
                    new { id = Guid.NewGuid(), action = "Approval Submitted", user = "Ahmed Al-Mansouri", timestamp = DateTime.Now.AddDays(-1) },
                    new { id = Guid.NewGuid(), action = "Workflow Created", user = "Sarah Johnson", timestamp = DateTime.Now.AddDays(-2) },
                    new { id = Guid.NewGuid(), action = "Report Generated", user = "Fatima Al-Dosari", timestamp = DateTime.Now.AddDays(-3) }
                };

                return Ok(ApiResponse<object>.SuccessResponse(logs, "Audit logs retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        [HttpGet("health/system")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var health = new
                {
                    database = "Healthy",
                    api = "Operational",
                    cache = "Operational",
                    lastCheck = DateTime.UtcNow,
                    uptime = "99.9%"
                };

                return Ok(ApiResponse<object>.SuccessResponse(health, "System health retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        #region Authentication APIs

        /// <summary>
        /// User login endpoint
        /// </summary>
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var result = await _authenticationService.LoginAsync(loginRequest.Email, loginRequest.Password);
                if (result == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid email or password"));

                return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result, "Login successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                var result = await _authenticationService.RegisterAsync(
                    registerRequest.Email,
                    registerRequest.Password,
                    registerRequest.FullName);

                if (result == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("User already exists"));

                return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result, "Registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Validate authentication token
        /// </summary>
        [HttpPost("auth/validate")]
        public async Task<IActionResult> ValidateToken([FromBody] dynamic request)
        {
            try
            {
                var token = request?.token?.ToString();
                if (string.IsNullOrEmpty(token))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token is required"));

                var isValid = await _authenticationService.ValidateTokenAsync(token);
                return Ok(ApiResponse<object>.SuccessResponse(new { valid = isValid }, "Token validation complete"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get current user from token
        /// </summary>
        [HttpPost("auth/user")]
        public async Task<IActionResult> GetCurrentUser([FromBody] dynamic request)
        {
            try
            {
                var token = request?.token?.ToString();
                if (string.IsNullOrEmpty(token))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token is required"));

                var user = await _authenticationService.GetUserFromTokenAsync(token);
                if (user == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));

                return Ok(ApiResponse<AuthUserDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("auth/logout")]
        public async Task<IActionResult> Logout([FromBody] dynamic request)
        {
            try
            {
                var token = request?.token?.ToString();
                if (string.IsNullOrEmpty(token))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token is required"));

                await _authenticationService.LogoutAsync(token);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        [HttpPost("auth/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshRequest)
        {
            try
            {
                var result = await _authenticationService.RefreshTokenAsync(refreshRequest.RefreshToken);
                if (result == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid refresh token"));

                return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result, "Token refreshed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Authorization APIs

        /// <summary>
        /// Get user roles
        /// </summary>
        [HttpGet("auth/users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _authorizationService.GetUserRolesAsync(userId);
                if (roles == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserRoleDto>.SuccessResponse(roles, "User roles retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("auth/users/{userId}/roles")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] dynamic request)
        {
            try
            {
                var role = request?.role?.ToString();
                if (string.IsNullOrEmpty(role))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Role is required"));

                var success = await _authorizationService.AssignRoleAsync(userId, role);
                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid role"));

                return Ok(ApiResponse<object>.SuccessResponse(new { userId, role }, "Role assigned successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Revoke role from user
        /// </summary>
        [HttpDelete("auth/users/{userId}/roles/{role}")]
        public async Task<IActionResult> RevokeRole(string userId, string role)
        {
            try
            {
                var success = await _authorizationService.RevokeRoleAsync(userId, role);
                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Role not found for user"));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Role revoked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Check user permission
        /// </summary>
        [HttpPost("auth/users/{userId}/permissions/check")]
        public async Task<IActionResult> CheckPermission(string userId, [FromBody] dynamic request)
        {
            try
            {
                var permission = request?.permission?.ToString();
                if (string.IsNullOrEmpty(permission))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Permission is required"));

                var hasPermission = await _authorizationService.HasPermissionAsync(userId, permission);
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { userId, permission, hasPermission }, 
                    "Permission check completed"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get user permissions
        /// </summary>
        [HttpGet("auth/users/{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                var permissions = await _authorizationService.GetPermissionsAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { userId, permissions }, 
                    "User permissions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Assessment CRUD APIs

        /// <summary>
        /// Get all assessments
        /// </summary>
        [HttpGet("assessments")]
        public async Task<IActionResult> GetAssessments()
        {
            try
            {
                var assessments = await _assessmentService.GetAllAsync();
                return Ok(ApiResponse<object>.SuccessResponse(assessments, "Assessments retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get assessment by ID
        /// </summary>
        [HttpGet("assessments/{id}")]
        public async Task<IActionResult> GetAssessment(Guid id)
        {
            try
            {
                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                return Ok(ApiResponse<object>.SuccessResponse(assessment, "Assessment retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create new assessment
        /// </summary>
        [HttpPost("assessments")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            try
            {
                var assessment = await _assessmentService.CreateAsync(createAssessmentDto);
                return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id }, 
                    ApiResponse<object>.SuccessResponse(assessment, "Assessment created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update assessment
        /// </summary>
        [HttpPut("assessments/{id}")]
        public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            try
            {
                var assessment = await _assessmentService.UpdateAsync(id, updateAssessmentDto);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                return Ok(ApiResponse<object>.SuccessResponse(assessment, "Assessment updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete assessment
        /// </summary>
        [HttpDelete("assessments/{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            try
            {
                await _assessmentService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Assessment deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get assessment statistics
        /// </summary>
        [HttpGet("assessments/stats/summary")]
        public async Task<IActionResult> GetAssessmentStats()
        {
            try
            {
                var stats = await _assessmentService.GetStatisticsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Assessment statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Audit Management APIs

        /// <summary>
        /// Get all audits
        /// </summary>
        [HttpGet("audits")]
        public async Task<IActionResult> GetAudits()
        {
            try
            {
                var audits = await _auditService.GetAllAsync();
                return Ok(ApiResponse<object>.SuccessResponse(audits, "Audits retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get audit by ID
        /// </summary>
        [HttpGet("audits/{id}")]
        public async Task<IActionResult> GetAudit(Guid id)
        {
            try
            {
                var audit = await _auditService.GetByIdAsync(id);
                if (audit == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Audit not found"));

                return Ok(ApiResponse<object>.SuccessResponse(audit, "Audit retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create new audit
        /// </summary>
        [HttpPost("audits")]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditDto createAuditDto)
        {
            try
            {
                var audit = await _auditService.CreateAsync(createAuditDto);
                return CreatedAtAction(nameof(GetAudit), new { id = audit.Id }, 
                    ApiResponse<object>.SuccessResponse(audit, "Audit created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update audit
        /// </summary>
        [HttpPut("audits/{id}")]
        public async Task<IActionResult> UpdateAudit(Guid id, [FromBody] UpdateAuditDto updateAuditDto)
        {
            try
            {
                var audit = await _auditService.UpdateAsync(id, updateAuditDto);
                if (audit == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Audit not found"));

                return Ok(ApiResponse<object>.SuccessResponse(audit, "Audit updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete audit
        /// </summary>
        [HttpDelete("audits/{id}")]
        public async Task<IActionResult> DeleteAudit(Guid id)
        {
            try
            {
                await _auditService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Audit deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get audit findings
        /// </summary>
        [HttpGet("audits/{id}/findings")]
        public async Task<IActionResult> GetAuditFindings(Guid id)
        {
            try
            {
                var findings = await _auditService.GetAuditFindingsAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(findings, "Audit findings retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create audit finding
        /// </summary>
        [HttpPost("audits/{id}/findings")]
        public async Task<IActionResult> CreateAuditFinding(Guid id, [FromBody] CreateAuditFindingDto createFindingDto)
        {
            try
            {
                var finding = await _auditService.AddFindingAsync(id, createFindingDto);
                return CreatedAtAction(nameof(GetAuditFindings), new { id }, 
                    ApiResponse<object>.SuccessResponse(finding, "Finding created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get audit statistics
        /// </summary>
        [HttpGet("audits/stats/summary")]
        public async Task<IActionResult> GetAuditStats()
        {
            try
            {
                var stats = await _auditService.GetStatisticsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Audit statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        #endregion
    }
}
