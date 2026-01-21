using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Compliance Gap Management API Controller
/// Handles gap lifecycle: Identify → Plan → Remediate → Validate → Close
/// Route: /api/compliance-gaps
/// </summary>
[ApiController]
[Route("api/compliance-gaps")]
[Authorize]
public class ComplianceGapController : ControllerBase
{
    private readonly IComplianceGapService _gapService;
    private readonly ILogger<ComplianceGapController> _logger;

    public ComplianceGapController(
        IComplianceGapService gapService,
        ILogger<ComplianceGapController> logger)
    {
        _gapService = gapService;
        _logger = logger;
    }

    #region Gap Identification

    /// <summary>
    /// Get all open gaps for a tenant
    /// </summary>
    [HttpGet("tenants/{tenantId}")]
    public async Task<IActionResult> GetOpenGaps(Guid tenantId)
    {
        try
        {
            var gaps = await _gapService.GetOpenGapsAsync(tenantId);
            return Ok(new { success = true, data = gaps });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting open gaps for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading gaps" });
        }
    }

    /// <summary>
    /// Identify gaps from assessment
    /// </summary>
    [HttpPost("tenants/{tenantId}/identify-from-assessment/{assessmentId}")]
    public async Task<IActionResult> IdentifyGapsFromAssessment(Guid tenantId, Guid assessmentId)
    {
        try
        {
            var gaps = await _gapService.IdentifyGapsFromAssessmentAsync(tenantId, assessmentId);
            return Ok(new { success = true, data = gaps, message = $"Identified {gaps.Count} gaps" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying gaps from assessment {AssessmentId}", assessmentId);
            return StatusCode(500, new { success = false, error = "Error identifying gaps" });
        }
    }

    /// <summary>
    /// Get gap by ID
    /// </summary>
    [HttpGet("tenants/{tenantId}/gaps/{gapId}")]
    public async Task<IActionResult> GetGapById(Guid tenantId, Guid gapId)
    {
        try
        {
            var gap = await _gapService.GetGapByIdAsync(tenantId, gapId);
            if (gap == null)
                return NotFound(new { success = false, error = "Gap not found" });

            return Ok(new { success = true, data = gap });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error loading gap" });
        }
    }

    /// <summary>
    /// Create a new gap manually
    /// </summary>
    [HttpPost("tenants/{tenantId}")]
    public async Task<IActionResult> CreateGap(Guid tenantId, [FromBody] CreateComplianceGapRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Title))
                return BadRequest(new { success = false, error = "Title is required" });

            var gap = await _gapService.CreateGapAsync(tenantId, request);
            return CreatedAtAction(nameof(GetGapById), new { tenantId, gapId = gap.Id },
                new { success = true, data = gap, message = "Gap created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating gap for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error creating gap" });
        }
    }

    #endregion

    #region Remediation Planning

    /// <summary>
    /// Create remediation plan for a gap
    /// </summary>
    [HttpPost("tenants/{tenantId}/gaps/{gapId}/remediation-plan")]
    public async Task<IActionResult> CreateRemediationPlan(Guid tenantId, Guid gapId, [FromBody] CreateRemediationPlanRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.PlanName))
                return BadRequest(new { success = false, error = "Plan name is required" });

            var plan = await _gapService.CreateRemediationPlanAsync(tenantId, gapId, request);
            return Ok(new { success = true, data = plan, message = "Remediation plan created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating remediation plan for gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error creating remediation plan" });
        }
    }

    /// <summary>
    /// Get remediation plan for a gap
    /// </summary>
    [HttpGet("tenants/{tenantId}/gaps/{gapId}/remediation-plan")]
    public async Task<IActionResult> GetRemediationPlan(Guid tenantId, Guid gapId)
    {
        try
        {
            var plan = await _gapService.GetRemediationPlanAsync(tenantId, gapId);
            if (plan == null)
                return NotFound(new { success = false, error = "No remediation plan found" });

            return Ok(new { success = true, data = plan });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remediation plan for gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error loading remediation plan" });
        }
    }

    /// <summary>
    /// Update remediation progress
    /// </summary>
    [HttpPut("tenants/{tenantId}/gaps/{gapId}/progress")]
    public async Task<IActionResult> UpdateRemediationProgress(Guid tenantId, Guid gapId, [FromBody] UpdateRemediationProgressRequest request)
    {
        try
        {
            var plan = await _gapService.UpdateRemediationProgressAsync(tenantId, gapId, request);
            return Ok(new { success = true, data = plan, message = "Progress updated" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, error = "The requested resource was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating remediation progress for gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error updating progress" });
        }
    }

    #endregion

    #region Validation & Closure

    /// <summary>
    /// Submit gap for validation
    /// </summary>
    [HttpPost("tenants/{tenantId}/gaps/{gapId}/submit-validation")]
    public async Task<IActionResult> SubmitForValidation(Guid tenantId, Guid gapId)
    {
        try
        {
            var submittedBy = User.Identity?.Name ?? "System";
            var gap = await _gapService.SubmitForValidationAsync(tenantId, gapId, submittedBy);
            return Ok(new { success = true, data = gap, message = "Gap submitted for validation" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, error = "The requested resource was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting gap {GapId} for validation", gapId);
            return StatusCode(500, new { success = false, error = "Error submitting for validation" });
        }
    }

    /// <summary>
    /// Validate gap remediation
    /// </summary>
    [HttpPost("tenants/{tenantId}/gaps/{gapId}/validate")]
    public async Task<IActionResult> ValidateRemediation(Guid tenantId, Guid gapId, [FromBody] ValidateRemediationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.ValidatedBy))
            {
                request = request ?? new ValidateRemediationRequest();
                request.ValidatedBy = User.Identity?.Name ?? "System";
            }

            var gap = await _gapService.ValidateRemediationAsync(tenantId, gapId, request);
            return Ok(new { success = true, data = gap, message = request.IsValid ? "Gap validated" : "Validation failed" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, error = "The requested resource was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error validating gap" });
        }
    }

    /// <summary>
    /// Close a validated gap
    /// </summary>
    [HttpPost("tenants/{tenantId}/gaps/{gapId}/close")]
    public async Task<IActionResult> CloseGap(Guid tenantId, Guid gapId, [FromBody] CloseGapRequest? request = null)
    {
        try
        {
            var closedBy = User.Identity?.Name ?? "System";
            var gap = await _gapService.CloseGapAsync(tenantId, gapId, closedBy, request?.ClosureNotes);
            return Ok(new { success = true, data = gap, message = "Gap closed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = "An error occurred processing your request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error closing gap" });
        }
    }

    /// <summary>
    /// Reopen a closed gap
    /// </summary>
    [HttpPost("tenants/{tenantId}/gaps/{gapId}/reopen")]
    public async Task<IActionResult> ReopenGap(Guid tenantId, Guid gapId, [FromBody] ReopenGapRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.Reason))
                return BadRequest(new { success = false, error = "Reason is required for reopening" });

            var reopenedBy = User.Identity?.Name ?? "System";
            var gap = await _gapService.ReopenGapAsync(tenantId, gapId, reopenedBy, request.Reason);
            return Ok(new { success = true, data = gap, message = "Gap reopened" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, error = "The requested resource was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening gap {GapId}", gapId);
            return StatusCode(500, new { success = false, error = "Error reopening gap" });
        }
    }

    #endregion

    #region Reporting

    /// <summary>
    /// Get gap summary by framework
    /// </summary>
    [HttpGet("tenants/{tenantId}/summary/{frameworkCode}")]
    public async Task<IActionResult> GetGapSummaryByFramework(Guid tenantId, string frameworkCode)
    {
        try
        {
            var summary = await _gapService.GetGapSummaryByFrameworkAsync(tenantId, frameworkCode);
            return Ok(new { success = true, data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gap summary for framework {FrameworkCode}", frameworkCode);
            return StatusCode(500, new { success = false, error = "Error loading summary" });
        }
    }

    /// <summary>
    /// Get gap aging report
    /// </summary>
    [HttpGet("tenants/{tenantId}/aging-report")]
    public async Task<IActionResult> GetGapAgingReport(Guid tenantId)
    {
        try
        {
            var report = await _gapService.GetGapAgingReportAsync(tenantId);
            return Ok(new { success = true, data = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gap aging report for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading aging report" });
        }
    }

    /// <summary>
    /// Get gap closure trend
    /// </summary>
    [HttpGet("tenants/{tenantId}/closure-trend")]
    public async Task<IActionResult> GetGapClosureTrend(Guid tenantId, [FromQuery] int months = 12)
    {
        try
        {
            var trend = await _gapService.GetGapClosureTrendAsync(tenantId, months);
            return Ok(new { success = true, data = trend });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gap closure trend for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading trend data" });
        }
    }

    #endregion
}

/// <summary>
/// Request to close a gap
/// </summary>
public class CloseGapRequest
{
    public string? ClosureNotes { get; set; }
}

/// <summary>
/// Request to reopen a gap
/// </summary>
public class ReopenGapRequest
{
    public string Reason { get; set; } = string.Empty;
}
