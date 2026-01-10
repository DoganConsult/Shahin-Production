using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Certification Tracking
/// Supports certification lifecycle: Obtain, Maintain, Renew, Audit Tracking, Expiry Monitoring
/// Tracks ISO 27001, SOC 2, NCA certifications, PCI-DSS, etc.
/// </summary>
[ApiController]
[Route("api/v1/certifications")]
[Authorize]
public class CertificationController : ControllerBase
{
    private readonly ICertificationService _certificationService;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<CertificationController> _logger;

    public CertificationController(
        ICertificationService certificationService,
        ITenantContextService tenantContext,
        ILogger<CertificationController> logger)
    {
        _certificationService = certificationService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Certification CRUD

    /// <summary>
    /// Create a new certification record
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCertification([FromBody] CreateCertificationRequest request)
    {
        try
        {
            request.TenantId = _tenantContext.GetCurrentTenantId();
            var result = await _certificationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetCertification), new { id = result.Id }, new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating certification");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get certification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCertification(Guid id)
    {
        var result = await _certificationService.GetByIdAsync(id);
        if (result == null) return NotFound(new { success = false, error = "Certification not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certification with audit history
    /// </summary>
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetCertificationDetail(Guid id)
    {
        var result = await _certificationService.GetDetailAsync(id);
        if (result == null) return NotFound(new { success = false, error = "Certification not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Update certification
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCertification(Guid id, [FromBody] UpdateCertificationRequest request)
    {
        try
        {
            var result = await _certificationService.UpdateAsync(id, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Delete certification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCertification(Guid id)
    {
        try
        {
            await _certificationService.DeleteAsync(id);
            return Ok(new { success = true, message = "Certification deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get all certifications
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCertifications()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetAllAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certifications by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetByStatusAsync(tenantId, status);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certifications by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetByCategoryAsync(tenantId, category);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Lifecycle Management

    /// <summary>
    /// Start certification process
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartCertification(Guid id, [FromBody] StartCertificationRequest request)
    {
        try
        {
            var result = await _certificationService.StartCertificationAsync(id, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Mark certification as issued
    /// </summary>
    [HttpPost("{id}/issue")]
    public async Task<IActionResult> MarkIssued(Guid id, [FromBody] MarkIssuedRequest request)
    {
        try
        {
            var result = await _certificationService.MarkIssuedAsync(id, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking certification {Id} as issued", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Renew certification
    /// </summary>
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewCertification(Guid id, [FromBody] RenewCertificationRequest request)
    {
        try
        {
            var result = await _certificationService.RenewAsync(id, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Suspend certification
    /// </summary>
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendCertification(Guid id, [FromBody] SuspendCertificationRequest request)
    {
        try
        {
            var result = await _certificationService.SuspendAsync(id, request.Reason, request.SuspendedBy);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Reinstate certification
    /// </summary>
    [HttpPost("{id}/reinstate")]
    public async Task<IActionResult> ReinstateCertification(Guid id, [FromBody] ReinstateCertificationRequest request)
    {
        try
        {
            var result = await _certificationService.ReinstateAsync(id, request.Notes, request.ReinstatedBy);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reinstating certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Mark certification as expired
    /// </summary>
    [HttpPost("{id}/expire")]
    public async Task<IActionResult> MarkExpired(Guid id)
    {
        try
        {
            var result = await _certificationService.MarkExpiredAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking certification {Id} as expired", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Audit Management

    /// <summary>
    /// Schedule an audit
    /// </summary>
    [HttpPost("{certificationId}/audits")]
    public async Task<IActionResult> ScheduleAudit(Guid certificationId, [FromBody] ScheduleAuditRequest request)
    {
        try
        {
            var result = await _certificationService.ScheduleAuditAsync(certificationId, request);
            return CreatedAtAction(nameof(GetAudit), new { auditId = result.Id }, new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling audit for certification {Id}", certificationId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Record audit result
    /// </summary>
    [HttpPut("audits/{auditId}/result")]
    public async Task<IActionResult> RecordAuditResult(Guid auditId, [FromBody] RecordAuditResultRequest request)
    {
        try
        {
            var result = await _certificationService.RecordAuditResultAsync(auditId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording audit result for {AuditId}", auditId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get audit by ID
    /// </summary>
    [HttpGet("audits/{auditId}")]
    public async Task<IActionResult> GetAudit(Guid auditId)
    {
        var result = await _certificationService.GetAuditByIdAsync(auditId);
        if (result == null) return NotFound(new { success = false, error = "Audit not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get audit history for certification
    /// </summary>
    [HttpGet("{certificationId}/audits")]
    public async Task<IActionResult> GetAuditHistory(Guid certificationId)
    {
        var result = await _certificationService.GetAuditHistoryAsync(certificationId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Complete corrective actions
    /// </summary>
    [HttpPost("audits/{auditId}/corrective-actions-complete")]
    public async Task<IActionResult> CompleteCorrectiveActions(Guid auditId, [FromBody] CompleteCorrectiveActionsRequest request)
    {
        try
        {
            var result = await _certificationService.CompleteCorrectiveActionsAsync(auditId, request.Notes, request.CompletedBy);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing corrective actions for audit {AuditId}", auditId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get upcoming audits
    /// </summary>
    [HttpGet("audits/upcoming")]
    public async Task<IActionResult> GetUpcomingAudits([FromQuery] int days = 90)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetUpcomingAuditsAsync(tenantId, days);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Expiry & Renewal Tracking

    /// <summary>
    /// Get certifications expiring soon
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 90)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetExpiringSoonAsync(tenantId, days);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get expired certifications
    /// </summary>
    [HttpGet("expired")]
    public async Task<IActionResult> GetExpired()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetExpiredAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get renewal actions needed
    /// </summary>
    [HttpGet("renewal-actions")]
    public async Task<IActionResult> GetRenewalActions()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetRenewalActionsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Update surveillance date
    /// </summary>
    [HttpPut("{id}/surveillance-date")]
    public async Task<IActionResult> UpdateSurveillanceDate(Guid id, [FromBody] UpdateSurveillanceDateRequest request)
    {
        try
        {
            var result = await _certificationService.UpdateSurveillanceDateAsync(id, request.NextDate);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating surveillance date for certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Dashboard & Reporting

    /// <summary>
    /// Get certification dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetDashboardAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certification statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetStatisticsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certification compliance matrix
    /// </summary>
    [HttpGet("matrix")]
    public async Task<IActionResult> GetComplianceMatrix()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetComplianceMatrixAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certification cost summary
    /// </summary>
    [HttpGet("cost-summary")]
    public async Task<IActionResult> GetCostSummary([FromQuery] int year)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetCostSummaryAsync(tenantId, year);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Ownership

    /// <summary>
    /// Assign owner
    /// </summary>
    [HttpPost("{id}/owner")]
    public async Task<IActionResult> AssignOwner(Guid id, [FromBody] AssignCertificationOwnerRequest request)
    {
        try
        {
            var result = await _certificationService.AssignOwnerAsync(id, request.OwnerId, request.OwnerName, request.Department);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning owner to certification {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get certifications by owner
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
    {
        var result = await _certificationService.GetByOwnerAsync(ownerId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get certifications by department
    /// </summary>
    [HttpGet("department/{department}")]
    public async Task<IActionResult> GetByDepartment(string department)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _certificationService.GetByDepartmentAsync(tenantId, department);
        return Ok(new { success = true, data = result });
    }

    #endregion
}

#region Request DTOs

public class SuspendCertificationRequest
{
    public string Reason { get; set; } = string.Empty;
    public string SuspendedBy { get; set; } = string.Empty;
}

public class ReinstateCertificationRequest
{
    public string Notes { get; set; } = string.Empty;
    public string ReinstatedBy { get; set; } = string.Empty;
}

public class CompleteCorrectiveActionsRequest
{
    public string Notes { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
}

public class UpdateSurveillanceDateRequest
{
    public DateTime NextDate { get; set; }
}

public class AssignCertificationOwnerRequest
{
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? Department { get; set; }
}

#endregion
