using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API controller for diagnostic agent services
/// Provides endpoints for error analysis, health diagnosis, and pattern detection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class DiagnosticController : ControllerBase
{
    private readonly IDiagnosticAgentService _diagnosticAgent;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(
        IDiagnosticAgentService diagnosticAgent,
        ILogger<DiagnosticController> logger)
    {
        _diagnosticAgent = diagnosticAgent;
        _logger = logger;
    }

    /// <summary>
    /// Analyze recent errors and get diagnostic report
    /// GET /api/diagnostic/errors?hoursBack=24&severity=Critical
    /// </summary>
    [HttpGet("errors")]
    public async Task<IActionResult> AnalyzeErrors(
        [FromQuery] int? hoursBack = 24,
        [FromQuery] string? severity = null,
        [FromQuery] Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _diagnosticAgent.AnalyzeErrorsAsync(
                hoursBack,
                severity,
                tenantId,
                cancellationToken);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing errors");
            return StatusCode(500, new { error = "Failed to analyze errors", message = ex.Message });
        }
    }

    /// <summary>
    /// Diagnose a specific error
    /// POST /api/diagnostic/errors/diagnose
    /// </summary>
    [HttpPost("errors/diagnose")]
    public async Task<IActionResult> DiagnoseError(
        [FromBody] DiagnoseErrorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrEmpty(request.ErrorId))
        {
            return BadRequest(new { error = "ErrorId is required" });
        }

        try
        {
            var diagnosis = await _diagnosticAgent.DiagnoseErrorAsync(
                request.ErrorId,
                request.ExceptionType,
                request.StackTrace,
                request.Context,
                cancellationToken);

            return Ok(diagnosis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error diagnosing error {ErrorId}", request.ErrorId);
            return StatusCode(500, new { error = "Failed to diagnose error", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyze application health
    /// GET /api/diagnostic/health?tenantId=...
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> AnalyzeHealth(
        [FromQuery] Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var diagnosis = await _diagnosticAgent.AnalyzeHealthAsync(tenantId, cancellationToken);
            return Ok(diagnosis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing health");
            return StatusCode(500, new { error = "Failed to analyze health", message = ex.Message });
        }
    }

    /// <summary>
    /// Detect patterns in errors
    /// GET /api/diagnostic/patterns?daysBack=7&tenantId=...
    /// </summary>
    [HttpGet("patterns")]
    public async Task<IActionResult> DetectPatterns(
        [FromQuery] int daysBack = 7,
        [FromQuery] Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await _diagnosticAgent.DetectPatternsAsync(daysBack, tenantId, cancellationToken);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting patterns");
            return StatusCode(500, new { error = "Failed to detect patterns", message = ex.Message });
        }
    }

    /// <summary>
    /// Perform root cause analysis
    /// POST /api/diagnostic/root-cause
    /// </summary>
    [HttpPost("root-cause")]
    public async Task<IActionResult> AnalyzeRootCause(
        [FromBody] RootCauseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrEmpty(request.ProblemDescription))
        {
            return BadRequest(new { error = "ProblemDescription is required" });
        }

        try
        {
            var analysis = await _diagnosticAgent.AnalyzeRootCauseAsync(
                request.ProblemDescription,
                request.Context,
                cancellationToken);

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing root cause");
            return StatusCode(500, new { error = "Failed to analyze root cause", message = ex.Message });
        }
    }

    /// <summary>
    /// Get proactive recommendations
    /// GET /api/diagnostic/recommendations?tenantId=...
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = await _diagnosticAgent.GetRecommendationsAsync(tenantId, cancellationToken);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, new { error = "Failed to get recommendations", message = ex.Message });
        }
    }

    /// <summary>
    /// Monitor conditions and get alerts
    /// GET /api/diagnostic/alerts?tenantId=...
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> MonitorConditions(
        [FromQuery] Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _diagnosticAgent.MonitorConditionsAsync(tenantId, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring conditions");
            return StatusCode(500, new { error = "Failed to monitor conditions", message = ex.Message });
        }
    }
}

#region Request DTOs

public class DiagnoseErrorRequest
{
    [Required]
    public string ErrorId { get; set; } = string.Empty;
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? Context { get; set; }
}

public class RootCauseRequest
{
    [Required]
    public string ProblemDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Context { get; set; }
}

#endregion
