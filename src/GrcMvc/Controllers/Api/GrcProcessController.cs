using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// GRC Process API Controller
/// Complete lifecycle management: Assessment → Compliance → Resilience → Excellence
/// Route: /api/grc
/// </summary>
[ApiController]
[Route("api/grc")]
[Authorize]
public class GrcProcessController : ControllerBase
{
    private readonly IGrcProcessOrchestrator _orchestrator;
    private readonly ILogger<GrcProcessController> _logger;

    public GrcProcessController(
        IGrcProcessOrchestrator orchestrator,
        ILogger<GrcProcessController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    #region Dashboard & Status

    /// <summary>
    /// Get complete GRC dashboard with all metrics
    /// </summary>
    [HttpGet("tenants/{tenantId}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid tenantId)
    {
        try
        {
            var dashboard = await _orchestrator.GetDashboardAsync(tenantId);
            return Ok(new { success = true, data = dashboard });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GRC dashboard for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading dashboard" });
        }
    }

    /// <summary>
    /// Get GRC maturity score (CMM Levels 1-5)
    /// </summary>
    [HttpGet("tenants/{tenantId}/maturity")]
    public async Task<IActionResult> GetMaturityScore(Guid tenantId)
    {
        try
        {
            var maturity = await _orchestrator.GetMaturityScoreAsync(tenantId);
            return Ok(new { success = true, data = maturity });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maturity score for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error calculating maturity" });
        }
    }

    #endregion

    #region Assessment Lifecycle

    /// <summary>
    /// Initialize assessment cycle for a framework
    /// </summary>
    [HttpPost("tenants/{tenantId}/assessment-cycle")]
    public async Task<IActionResult> InitializeAssessmentCycle(Guid tenantId, [FromBody] InitiateCycleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.FrameworkCode))
                return BadRequest(new { success = false, error = "Framework code is required" });

            var cycle = await _orchestrator.InitializeAssessmentCycleAsync(tenantId, request.FrameworkCode);
            return Ok(new { success = true, data = cycle });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing assessment cycle for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error initializing cycle" });
        }
    }

    /// <summary>
    /// Get assessment lifecycle status
    /// </summary>
    [HttpGet("tenants/{tenantId}/assessments/{assessmentId}/lifecycle")]
    public async Task<IActionResult> GetAssessmentLifecycle(Guid tenantId, Guid assessmentId)
    {
        try
        {
            var lifecycle = await _orchestrator.GetAssessmentLifecycleAsync(tenantId, assessmentId);
            return Ok(new { success = true, data = lifecycle });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessment lifecycle for {AssessmentId}", assessmentId);
            return StatusCode(500, new { success = false, error = "Error loading lifecycle" });
        }
    }

    /// <summary>
    /// Execute assessment with scoring
    /// </summary>
    [HttpPost("tenants/{tenantId}/assessments/{assessmentId}/execute")]
    public async Task<IActionResult> ExecuteAssessment(Guid tenantId, Guid assessmentId)
    {
        try
        {
            var result = await _orchestrator.ExecuteAssessmentAsync(tenantId, assessmentId);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing assessment {AssessmentId}", assessmentId);
            return StatusCode(500, new { success = false, error = "Error executing assessment" });
        }
    }

    #endregion

    #region Compliance Monitoring

    /// <summary>
    /// Get compliance score (overall or per framework)
    /// </summary>
    [HttpGet("tenants/{tenantId}/compliance")]
    public async Task<IActionResult> GetComplianceScore(Guid tenantId, [FromQuery] string? frameworkCode = null)
    {
        try
        {
            var compliance = await _orchestrator.GetComplianceScoreAsync(tenantId, frameworkCode);
            return Ok(new { success = true, data = compliance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance score for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error calculating compliance" });
        }
    }

    /// <summary>
    /// Get compliance gaps requiring remediation
    /// </summary>
    [HttpGet("tenants/{tenantId}/compliance/gaps")]
    public async Task<IActionResult> GetComplianceGaps(Guid tenantId)
    {
        try
        {
            var gaps = await _orchestrator.GetComplianceGapsAsync(tenantId);
            return Ok(new { success = true, data = gaps });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance gaps for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading gaps" });
        }
    }

    /// <summary>
    /// Get regulatory deadlines
    /// </summary>
    [HttpGet("tenants/{tenantId}/deadlines")]
    public async Task<IActionResult> GetRegulatoryDeadlines(Guid tenantId, [FromQuery] string? regulatorCode = null)
    {
        try
        {
            var deadlines = await _orchestrator.GetRegulatoryDeadlinesAsync(tenantId, regulatorCode);
            return Ok(new { success = true, data = deadlines });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deadlines for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading deadlines" });
        }
    }

    /// <summary>
    /// Generate regulatory report (NCA, SAMA, SDAIA)
    /// </summary>
    [HttpGet("tenants/{tenantId}/reports/{regulatorCode}")]
    public async Task<IActionResult> GenerateRegulatoryReport(Guid tenantId, string regulatorCode)
    {
        try
        {
            var report = await _orchestrator.GenerateRegulatoryReportAsync(tenantId, regulatorCode);
            return Ok(new { success = true, data = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for {RegulatorCode}", regulatorCode);
            return StatusCode(500, new { success = false, error = "Error generating report" });
        }
    }

    #endregion

    #region Risk & Control Integration

    /// <summary>
    /// Get risk posture summary
    /// </summary>
    [HttpGet("tenants/{tenantId}/risk-posture")]
    public async Task<IActionResult> GetRiskPosture(Guid tenantId)
    {
        try
        {
            var posture = await _orchestrator.GetRiskPostureAsync(tenantId);
            return Ok(new { success = true, data = posture });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk posture for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error calculating risk posture" });
        }
    }

    /// <summary>
    /// Calculate control effectiveness
    /// </summary>
    [HttpGet("tenants/{tenantId}/controls/{controlId}/effectiveness")]
    public async Task<IActionResult> CalculateControlEffectiveness(Guid tenantId, Guid controlId)
    {
        try
        {
            var effectiveness = await _orchestrator.CalculateControlEffectivenessAsync(tenantId, controlId);
            return Ok(new { success = true, data = effectiveness });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating control effectiveness for {ControlId}", controlId);
            return StatusCode(500, new { success = false, error = "Error calculating effectiveness" });
        }
    }

    /// <summary>
    /// Get risk-control mapping analysis
    /// </summary>
    [HttpGet("tenants/{tenantId}/risk-control-mapping")]
    public async Task<IActionResult> GetRiskControlMapping(Guid tenantId)
    {
        try
        {
            var mapping = await _orchestrator.GetRiskControlMappingAsync(tenantId);
            return Ok(new { success = true, data = mapping });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk-control mapping for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading mapping" });
        }
    }

    #endregion

    #region Resilience Tracking

    /// <summary>
    /// Get resilience score (BCM, DR, Incident Response)
    /// </summary>
    [HttpGet("tenants/{tenantId}/resilience")]
    public async Task<IActionResult> GetResilienceScore(Guid tenantId)
    {
        try
        {
            var resilience = await _orchestrator.GetResilienceScoreAsync(tenantId);
            return Ok(new { success = true, data = resilience });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resilience score for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error calculating resilience" });
        }
    }

    /// <summary>
    /// Get continuous improvement status
    /// </summary>
    [HttpGet("tenants/{tenantId}/continuous-improvement")]
    public async Task<IActionResult> GetContinuousImprovement(Guid tenantId)
    {
        try
        {
            var improvement = await _orchestrator.GetContinuousImprovementStatusAsync(tenantId);
            return Ok(new { success = true, data = improvement });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting continuous improvement for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error loading improvement data" });
        }
    }

    #endregion

    #region Excellence & Benchmarking

    /// <summary>
    /// Get excellence score and KSA sector ranking
    /// </summary>
    [HttpGet("tenants/{tenantId}/excellence")]
    public async Task<IActionResult> GetExcellenceScore(Guid tenantId)
    {
        try
        {
            var excellence = await _orchestrator.GetExcellenceScoreAsync(tenantId);
            return Ok(new { success = true, data = excellence });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting excellence score for tenant {TenantId}", tenantId);
            return StatusCode(500, new { success = false, error = "Error calculating excellence" });
        }
    }

    /// <summary>
    /// Benchmark against KSA sector peers
    /// </summary>
    [HttpGet("tenants/{tenantId}/benchmark/{sectorCode}")]
    public async Task<IActionResult> BenchmarkAgainstSector(Guid tenantId, string sectorCode)
    {
        try
        {
            var benchmark = await _orchestrator.BenchmarkAgainstSectorAsync(tenantId, sectorCode);
            return Ok(new { success = true, data = benchmark });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error benchmarking against sector {SectorCode}", sectorCode);
            return StatusCode(500, new { success = false, error = "Error calculating benchmark" });
        }
    }

    /// <summary>
    /// Get certification readiness for a framework
    /// </summary>
    [HttpGet("tenants/{tenantId}/certification-readiness/{frameworkCode}")]
    public async Task<IActionResult> GetCertificationReadiness(Guid tenantId, string frameworkCode)
    {
        try
        {
            var readiness = await _orchestrator.GetCertificationReadinessAsync(tenantId, frameworkCode);
            return Ok(new { success = true, data = readiness });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certification readiness for {FrameworkCode}", frameworkCode);
            return StatusCode(500, new { success = false, error = "Error calculating readiness" });
        }
    }

    #endregion
}

/// <summary>
/// Request DTO for initiating assessment cycle
/// </summary>
public class InitiateCycleRequest
{
    public string FrameworkCode { get; set; } = string.Empty;
}
