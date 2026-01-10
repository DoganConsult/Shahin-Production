using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Control Testing
/// Supports control testing lifecycle, effectiveness scoring, and owner management
/// </summary>
[ApiController]
[Route("api/v1/control-tests")]
[Authorize]
public class ControlTestController : ControllerBase
{
    private readonly IControlTestService _controlTestService;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<ControlTestController> _logger;

    public ControlTestController(
        IControlTestService controlTestService,
        ITenantContextService tenantContext,
        ILogger<ControlTestController> logger)
    {
        _controlTestService = controlTestService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Test Execution

    /// <summary>
    /// Execute a control test
    /// </summary>
    [HttpPost("{controlId}/execute")]
    public async Task<IActionResult> ExecuteTest(Guid controlId, [FromBody] ExecuteControlTestRequest request)
    {
        try
        {
            var result = await _controlTestService.ExecuteTestAsync(controlId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing test for control {ControlId}", controlId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get test by ID
    /// </summary>
    [HttpGet("{testId}")]
    public async Task<IActionResult> GetTest(Guid testId)
    {
        var result = await _controlTestService.GetTestByIdAsync(testId);
        if (result == null) return NotFound(new { success = false, error = "Test not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get all tests for a control
    /// </summary>
    [HttpGet("control/{controlId}")]
    public async Task<IActionResult> GetTestsForControl(Guid controlId)
    {
        var result = await _controlTestService.GetTestsForControlAsync(controlId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get test history for a control
    /// </summary>
    [HttpGet("control/{controlId}/history")]
    public async Task<IActionResult> GetTestHistory(Guid controlId, [FromQuery] int limit = 12)
    {
        var result = await _controlTestService.GetTestHistoryAsync(controlId, limit);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Test Review

    /// <summary>
    /// Submit test for review
    /// </summary>
    [HttpPost("{testId}/submit")]
    public async Task<IActionResult> SubmitForReview(Guid testId)
    {
        try
        {
            var userName = User.Identity?.Name ?? "Unknown";
            var result = await _controlTestService.SubmitForReviewAsync(testId, userName);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting test {TestId} for review", testId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Approve a test
    /// </summary>
    [HttpPost("{testId}/approve")]
    public async Task<IActionResult> ApproveTest(Guid testId, [FromBody] ApproveTestRequest request)
    {
        try
        {
            var result = await _controlTestService.ApproveTestAsync(testId, request.ReviewerId, request.ReviewerName, request.Notes);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving test {TestId}", testId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a test
    /// </summary>
    [HttpPost("{testId}/reject")]
    public async Task<IActionResult> RejectTest(Guid testId, [FromBody] RejectTestRequest request)
    {
        try
        {
            var result = await _controlTestService.RejectTestAsync(testId, request.ReviewerId, request.ReviewerName, request.Reason);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting test {TestId}", testId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get tests pending review
    /// </summary>
    [HttpGet("pending-reviews")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetPendingReviewsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Scheduling

    /// <summary>
    /// Get upcoming tests
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingTests([FromQuery] int days = 30)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetUpcomingTestsAsync(tenantId, days);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get overdue tests
    /// </summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTests()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetOverdueTestsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Schedule a test
    /// </summary>
    [HttpPost("control/{controlId}/schedule")]
    public async Task<IActionResult> ScheduleTest(Guid controlId, [FromBody] ScheduleTestRequest request)
    {
        try
        {
            var result = await _controlTestService.ScheduleTestAsync(controlId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling test for control {ControlId}", controlId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Effectiveness

    /// <summary>
    /// Calculate control effectiveness
    /// </summary>
    [HttpGet("control/{controlId}/effectiveness")]
    public async Task<IActionResult> GetEffectiveness(Guid controlId)
    {
        try
        {
            var result = await _controlTestService.CalculateEffectivenessAsync(controlId);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating effectiveness for control {ControlId}", controlId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get effectiveness trend
    /// </summary>
    [HttpGet("control/{controlId}/effectiveness/trend")]
    public async Task<IActionResult> GetEffectivenessTrend(Guid controlId, [FromQuery] int months = 12)
    {
        var result = await _controlTestService.GetEffectivenessTrendAsync(controlId, months);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get effectiveness summary for tenant
    /// </summary>
    [HttpGet("effectiveness/summary")]
    public async Task<IActionResult> GetEffectivenessSummary()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetEffectivenessSummaryAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Owner Management

    /// <summary>
    /// Assign owner to control
    /// </summary>
    [HttpPost("control/{controlId}/owner")]
    public async Task<IActionResult> AssignOwner(Guid controlId, [FromBody] AssignOwnerRequest request)
    {
        try
        {
            var result = await _controlTestService.AssignOwnerAsync(controlId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning owner to control {ControlId}", controlId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get current owner of control
    /// </summary>
    [HttpGet("control/{controlId}/owner")]
    public async Task<IActionResult> GetCurrentOwner(Guid controlId)
    {
        var result = await _controlTestService.GetCurrentOwnerAsync(controlId);
        if (result == null) return NotFound(new { success = false, error = "No owner assigned" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get ownership history
    /// </summary>
    [HttpGet("control/{controlId}/owner/history")]
    public async Task<IActionResult> GetOwnershipHistory(Guid controlId)
    {
        var result = await _controlTestService.GetOwnershipHistoryAsync(controlId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Transfer ownership
    /// </summary>
    [HttpPost("control/{controlId}/owner/transfer")]
    public async Task<IActionResult> TransferOwnership(Guid controlId, [FromBody] TransferOwnershipRequest request)
    {
        try
        {
            var result = await _controlTestService.TransferOwnershipAsync(controlId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring ownership of control {ControlId}", controlId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get controls by owner
    /// </summary>
    [HttpGet("owner/{ownerId}/controls")]
    public async Task<IActionResult> GetControlsByOwner(string ownerId)
    {
        var result = await _controlTestService.GetControlsByOwnerAsync(ownerId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Get control testing dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetTestingDashboardAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get controls requiring testing
    /// </summary>
    [HttpGet("requiring-testing")]
    public async Task<IActionResult> GetControlsRequiringTesting()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetControlsRequiringTestingAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get testing coverage report
    /// </summary>
    [HttpGet("coverage")]
    public async Task<IActionResult> GetTestingCoverage()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _controlTestService.GetTestingCoverageAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    #endregion
}

#region Request DTOs

public class ApproveTestRequest
{
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RejectTestRequest
{
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

#endregion
