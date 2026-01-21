using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Configuration;
using GrcMvc.Data;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Real-time workflow data API for dashboard views
/// Replaces mock data with database queries
/// </summary>
[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowDataController : ControllerBase
{
    private readonly GrcDbContext _context;
    private readonly ILogger<WorkflowDataController> _logger;

    public WorkflowDataController(GrcDbContext context, ILogger<WorkflowDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Risk Assessment Workflows

    /// <summary>
    /// Get risk workflow data for dashboard
    /// </summary>
    [HttpGet("risk")]
    public async Task<IActionResult> GetRiskWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            
            var risks = await _context.Risks
                .Where(r => tenantId == null || r.TenantId == tenantId)
                .ToListAsync();

            var stats = new
            {
                critical = risks.Count(r => r.RiskScore >= 20 || r.Status == "Critical"),
                high = risks.Count(r => r.RiskScore >= 12 && r.RiskScore < 20),
                medium = risks.Count(r => r.RiskScore >= 6 && r.RiskScore < 12),
                low = risks.Count(r => r.RiskScore < 6)
            };

            var activeRisks = risks
                .Where(r => r.Status != "Closed" && r.Status != "Archived")
                .OrderByDescending(r => r.RiskScore)
                .Take(20)
                .Select(r => new
                {
                    id = r.Id,
                    riskId = r.RiskNumber ?? $"RSK-{r.Id.ToString()[..8].ToUpper()}",
                    title = r.Title ?? r.Name,
                    category = r.Category,
                    riskLevel = GetRiskLevel(r.RiskScore),
                    likelihood = r.Likelihood,
                    impact = r.Impact,
                    riskScore = r.RiskScore,
                    treatment = r.MitigationStrategy ?? "Pending",
                    owner = r.Owner,
                    status = r.Status,
                    dueDate = r.DueDate,
                    reviewDate = r.ReviewDate
                });

            var byCategory = risks
                .GroupBy(r => r.Category)
                .Select(g => new { category = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(6);

            var byTreatment = risks
                .GroupBy(r => r.MitigationStrategy ?? "Unassigned")
                .Select(g => new { treatment = g.Key, count = g.Count() });

            return Ok(new
            {
                stats,
                risks = activeRisks,
                charts = new { byCategory, byTreatment }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading risk workflows");
            return StatusCode(500, new { error = "Error loading risk data" });
        }
    }

    #endregion

    #region Compliance Testing Workflows

    /// <summary>
    /// Get control testing workflow data (using Assessments with Type=Control)
    /// </summary>
    [HttpGet("testing")]
    public async Task<IActionResult> GetTestingWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var assessments = await _context.Assessments
                .Where(a => tenantId == null || a.TenantId == tenantId)
                .Where(a => a.Type == "Control" || a.Type == "Testing")
                .OrderByDescending(a => a.CreatedDate)
                .Take(50)
                .ToListAsync();

            var stats = new
            {
                scheduled = assessments.Count(a => a.Status == "Planned" || a.Status == "Scheduled"),
                inProgress = assessments.Count(a => a.Status == "InProgress"),
                passed = assessments.Count(a => a.Status == "Completed" && a.Score >= 80),
                failed = assessments.Count(a => a.Status == "Completed" && a.Score < 80)
            };

            var tests = assessments.Select(a => new
            {
                id = a.Id,
                testId = $"TST-{a.CreatedDate:yyyyMMdd}-{a.Id.ToString()[..4].ToUpper()}",
                controlId = a.AssessmentCode ?? a.AssessmentNumber,
                controlTitle = a.Name ?? "Control Assessment",
                testType = a.Type ?? "Effectiveness",
                tester = a.AssignedTo ?? "Unassigned",
                dueDate = a.DueDate ?? a.EndDate ?? a.CreatedDate.AddDays(14),
                status = MapTestStatus(a.Status),
                result = a.Status == "Completed" ? (a.Score >= 80 ? "Pass" : "Fail") : "-",
                score = a.Score,
                comments = a.Findings
            });

            return Ok(new { stats, tests });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading testing workflows");
            return StatusCode(500, new { error = "Error loading testing data" });
        }
    }

    #endregion

    #region Remediation Workflows

    /// <summary>
    /// Get remediation/action plan workflow data
    /// </summary>
    [HttpGet("remediation")]
    public async Task<IActionResult> GetRemediationWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var actionPlans = await _context.ActionPlans
                .Where(a => tenantId == null || a.TenantId == tenantId)
                .OrderBy(a => a.DueDate)
                .Take(50)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var stats = new
            {
                overdue = actionPlans.Count(a => a.DueDate < now && a.Status != "Closed" && a.Status != "Completed"),
                inProgress = actionPlans.Count(a => a.Status == "InProgress" || a.Status == "Active"),
                pendingVerification = actionPlans.Count(a => a.Status == "PendingVerification" || a.Status == "Review"),
                closed = actionPlans.Count(a => a.Status == "Closed" || a.Status == "Completed")
            };

            var remediations = actionPlans
                .Where(a => a.Status != "Closed" && a.Status != "Completed")
                .Select(a => new
                {
                    id = a.Id,
                    remediationId = $"REM-{a.CreatedDate:yyyyMMdd}-{a.Id.ToString()[..4].ToUpper()}",
                    finding = a.Title,
                    description = a.Description,
                    source = a.Category ?? "Assessment",
                    owner = a.AssignedTo ?? "Unassigned",
                    dueDate = a.DueDate,
                    isOverdue = a.DueDate < now,
                    priority = a.Priority ?? "Medium",
                    status = a.Status,
                    progress = 0, // ActionPlan doesn't have completion percentage
                    createdDate = a.CreatedDate
                });

            return Ok(new { stats, remediations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading remediation workflows");
            return StatusCode(500, new { error = "Error loading remediation data" });
        }
    }

    #endregion

    #region Policy Workflows

    /// <summary>
    /// Get policy workflow data
    /// </summary>
    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicyWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var policies = await _context.Policies
                .Where(p => tenantId == null || p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedDate)
                .Take(50)
                .ToListAsync();

            var stats = new
            {
                draft = policies.Count(p => p.Status == "Draft"),
                underReview = policies.Count(p => p.Status == "UnderReview" || p.Status == "Review"),
                pendingApproval = policies.Count(p => p.Status == "PendingApproval"),
                published = policies.Count(p => p.Status == "Published" || p.Status == "Active")
            };

            var policyList = policies.Select(p => new
            {
                id = p.Id,
                title = p.Title,
                version = p.Version ?? "1.0",
                category = p.Category ?? "General",
                owner = p.Owner ?? "Unassigned",
                status = p.Status,
                reviewDate = p.NextReviewDate,
                approvedBy = p.ApprovedBy,
                approvedDate = p.ApprovalDate
            });

            return Ok(new { stats, policies = policyList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading policy workflows");
            return StatusCode(500, new { error = "Error loading policy data" });
        }
    }

    /// <summary>
    /// Approve a policy
    /// </summary>
    [HttpPost("policies/{id}/approve")]
    public async Task<IActionResult> ApprovePolicy(Guid id)
    {
        try
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
                return NotFound(new { error = "Policy not found" });

            policy.Status = "Published";
            policy.ApprovedBy = User.Identity?.Name ?? "System";
            policy.ApprovalDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Policy approved successfully", status = "Published" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving policy {PolicyId}", id);
            return StatusCode(500, new { error = "Error approving policy" });
        }
    }

    /// <summary>
    /// Reject a policy
    /// </summary>
    [HttpPost("policies/{id}/reject")]
    public async Task<IActionResult> RejectPolicy(Guid id, [FromBody] WorkflowRejectRequest? request)
    {
        try
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
                return NotFound(new { error = "Policy not found" });

            policy.Status = "Draft";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Policy returned for revision", status = "Draft" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting policy {PolicyId}", id);
            return StatusCode(500, new { error = "Error rejecting policy" });
        }
    }

    #endregion

    #region Exception Workflows

    /// <summary>
    /// Get exception workflow data
    /// </summary>
    [HttpGet("exceptions")]
    public async Task<IActionResult> GetExceptionWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var exceptions = await _context.ControlExceptions
                .Where(e => tenantId == null || e.TenantId == tenantId)
                .OrderByDescending(e => e.CreatedDate)
                .Take(50)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var stats = new
            {
                pendingReview = exceptions.Count(e => e.Status == "Pending"),
                approved = exceptions.Count(e => e.Status == "Approved"),
                expiringSoon = exceptions.Count(e => e.Status == "Approved" && e.ExpiryDate > now && e.ExpiryDate <= now.AddDays(30)),
                expired = exceptions.Count(e => e.Status == "Approved" && e.ExpiryDate <= now)
            };

            var exceptionList = exceptions.Select(e => new
            {
                id = e.Id,
                exceptionId = e.ExceptionCode,
                controlId = e.ControlId.ToString()[..8],
                controlTitle = "Control Exception",
                requestedBy = e.RiskAcceptanceOwnerName ?? "Unknown",
                expiryDate = e.ExpiryDate,
                status = e.Status,
                riskLevel = e.RiskImpact,
                justification = e.Reason,
                approvedBy = e.ApprovedBy
            });

            return Ok(new { stats, exceptions = exceptionList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exception workflows");
            return StatusCode(500, new { error = "Error loading exception data" });
        }
    }

    /// <summary>
    /// Create a new exception request
    /// </summary>
    [HttpPost("exceptions")]
    public async Task<IActionResult> CreateException([FromBody] WorkflowCreateExceptionRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId() ?? Guid.Empty;

            var exception = new Models.Entities.ControlException
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ControlId = request.ControlId ?? Guid.Empty,
                ExceptionCode = $"EXC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                Scope = request.ControlTitle ?? "All",
                RiskAcceptanceOwnerName = User.Identity?.Name ?? "System",
                Reason = request.Justification ?? "",
                RiskImpact = request.RiskLevel ?? "Medium",
                ExpiryDate = request.ExpiryDate ?? DateTime.UtcNow.AddMonths(3),
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.ControlExceptions.Add(exception);
            await _context.SaveChangesAsync();

            return Ok(new { id = exception.Id, exceptionNumber = exception.ExceptionCode, status = "Pending" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exception");
            return StatusCode(500, new { error = "Error creating exception" });
        }
    }

    /// <summary>
    /// Approve an exception
    /// </summary>
    [HttpPost("exceptions/{id}/approve")]
    public async Task<IActionResult> ApproveException(Guid id)
    {
        try
        {
            var exception = await _context.ControlExceptions.FindAsync(id);
            if (exception == null)
                return NotFound(new { error = "Exception not found" });

            exception.Status = "Approved";
            exception.ApprovedBy = User.Identity?.Name ?? "System";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exception approved", status = "Approved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving exception {ExceptionId}", id);
            return StatusCode(500, new { error = "Error approving exception" });
        }
    }

    /// <summary>
    /// Reject an exception
    /// </summary>
    [HttpPost("exceptions/{id}/reject")]
    public async Task<IActionResult> RejectException(Guid id, [FromBody] WorkflowRejectRequest? request)
    {
        try
        {
            var exception = await _context.ControlExceptions.FindAsync(id);
            if (exception == null)
                return NotFound(new { error = "Exception not found" });

            exception.Status = "Rejected";
            exception.ApprovedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exception rejected", status = "Rejected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting exception {ExceptionId}", id);
            return StatusCode(500, new { error = "Error rejecting exception" });
        }
    }

    #endregion

    #region Approvals Workflows

    /// <summary>
    /// Get all pending approvals
    /// </summary>
    [HttpGet("approvals")]
    public async Task<IActionResult> GetApprovals()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var tasks = await _context.WorkflowTasks
                .Where(t => tenantId == null || t.TenantId == tenantId)
                .Where(t => t.Status == "Pending" || t.Status == "InProgress")
                .OrderBy(t => t.DueDate)
                .Take(50)
                .ToListAsync();

            var pendingPolicies = await _context.Policies
                .Where(p => tenantId == null || p.TenantId == tenantId)
                .Where(p => p.Status == "PendingApproval")
                .CountAsync();

            var pendingExceptions = await _context.ControlExceptions
                .Where(e => tenantId == null || e.TenantId == tenantId)
                .Where(e => e.Status == "Pending")
                .CountAsync();

            var pendingEvidence = await _context.Evidences
                .Where(e => tenantId == null || e.TenantId == tenantId)
                .Where(e => e.VerificationStatus == "Pending")
                .CountAsync();

            var stats = new
            {
                total = tasks.Count + pendingPolicies + pendingExceptions + pendingEvidence,
                tasks = tasks.Count,
                policies = pendingPolicies,
                exceptions = pendingExceptions,
                evidence = pendingEvidence
            };

            var approvalList = tasks.Select(t => new
            {
                id = t.Id,
                type = "Task",
                title = t.TaskName,
                description = t.Description,
                requestedBy = t.AssignedToUserName,
                dueDate = t.DueDate,
                priority = t.Priority switch { 1 => "High", 3 => "Low", _ => "Medium" },
                status = t.Status
            });

            return Ok(new { stats, approvals = approvalList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading approvals");
            return StatusCode(500, new { error = "Error loading approvals" });
        }
    }

    #endregion

    #region Evidence Workflows

    /// <summary>
    /// Get evidence workflow data
    /// </summary>
    [HttpGet("evidence")]
    [HttpGet("list/evidence")]
    public async Task<IActionResult> GetEvidenceWorkflows()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var evidence = await _context.Evidences
                .Where(e => tenantId == null || e.TenantId == tenantId)
                .OrderByDescending(e => e.CreatedDate)
                .Take(50)
                .ToListAsync();

            var stats = new
            {
                pending = evidence.Count(e => e.VerificationStatus == "Pending"),
                underReview = evidence.Count(e => e.VerificationStatus == "UnderReview"),
                approved = evidence.Count(e => e.VerificationStatus == "Verified"),
                rejected = evidence.Count(e => e.VerificationStatus == "Rejected")
            };

            var workflows = evidence.Select(e => new
            {
                id = e.Id,
                workflowId = e.Id,
                controlId = e.EvidenceNumber ?? e.Title,
                submittedByUserId = e.CollectedBy ?? "Unknown",
                submittedDate = e.CollectionDate,
                status = e.VerificationStatus,
                fileCount = 1,
                description = e.Description
            });

            return Ok(new { stats, workflows });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading evidence workflows");
            return StatusCode(500, new { error = "Error loading evidence data" });
        }
    }

    /// <summary>
    /// Initiate evidence workflow for a control
    /// </summary>
    [HttpPost("evidence/initiate/{controlId}")]
    public async Task<IActionResult> InitiateEvidenceWorkflow(Guid controlId)
    {
        try
        {
            var tenantId = GetCurrentTenantId() ?? Guid.Empty;

            var evidence = new Models.Entities.Evidence
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EvidenceNumber = $"EV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                Title = $"Evidence for Control {controlId.ToString()[..8]}",
                VerificationStatus = "Pending",
                CollectedBy = User.Identity?.Name ?? "System",
                CollectionDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.Evidences.Add(evidence);
            await _context.SaveChangesAsync();

            return Ok(new { workflowId = evidence.Id, status = "Pending" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating evidence workflow");
            return StatusCode(500, new { error = "Error initiating workflow" });
        }
    }

    /// <summary>
    /// Submit evidence
    /// </summary>
    [HttpPost("evidence/{workflowId}/submit")]
    public async Task<IActionResult> SubmitEvidence(Guid workflowId, [FromBody] WorkflowSubmitEvidenceRequest? request)
    {
        try
        {
            var evidence = await _context.Evidences.FindAsync(workflowId);
            if (evidence == null)
                return NotFound(new { error = "Evidence workflow not found" });

            evidence.VerificationStatus = "Pending";
            evidence.Description = request?.Description ?? evidence.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Evidence submitted", status = "Pending" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting evidence {WorkflowId}", workflowId);
            return StatusCode(500, new { error = "Error submitting evidence" });
        }
    }

    /// <summary>
    /// Approve evidence
    /// </summary>
    [HttpPost("evidence/{workflowId}/approve")]
    public async Task<IActionResult> ApproveEvidence(Guid workflowId, [FromBody] WorkflowApproveEvidenceRequest? request)
    {
        try
        {
            var evidence = await _context.Evidences.FindAsync(workflowId);
            if (evidence == null)
                return NotFound(new { error = "Evidence not found" });

            evidence.VerificationStatus = "Verified";
            evidence.VerifiedBy = User.Identity?.Name ?? "System";
            evidence.VerificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Evidence approved", status = "Verified" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving evidence {WorkflowId}", workflowId);
            return StatusCode(500, new { error = "Error approving evidence" });
        }
    }

    #endregion

    #region Helper Methods

    private Guid? GetCurrentTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantClaim, out var tenantId))
            return tenantId;
        return null;
    }

    private static string GetRiskLevel(int score)
    {
        // Use centralized risk scoring configuration
        return RiskScoringConfiguration.GetRiskLevel(score);
    }

    private static string MapTestStatus(string? status)
    {
        return status switch
        {
            "Scheduled" or "Pending" => "Scheduled",
            "InProgress" => "In Progress",
            "Compliant" or "Effective" => "Completed",
            "NonCompliant" or "Ineffective" => "Completed",
            _ => status ?? "Unknown"
        };
    }

    private static string MapTestResult(string? status)
    {
        return status switch
        {
            "Compliant" or "Effective" => "Pass",
            "NonCompliant" or "Ineffective" => "Fail",
            "PartiallyCompliant" => "Partial",
            _ => "-"
        };
    }

    #endregion
}

#region Request DTOs

public class WorkflowRejectRequest
{
    public string? Reason { get; set; }
}

public class WorkflowCreateExceptionRequest
{
    public Guid? ControlId { get; set; }
    public string? ControlTitle { get; set; }
    public string? Justification { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class WorkflowSubmitEvidenceRequest
{
    public string? Description { get; set; }
    public List<string>? FileUrls { get; set; }
}

public class WorkflowApproveEvidenceRequest
{
    public string? Comments { get; set; }
}

#endregion
