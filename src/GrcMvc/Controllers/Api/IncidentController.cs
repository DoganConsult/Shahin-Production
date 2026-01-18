using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Common.Results;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Incident Response
/// Supports incident lifecycle: Detection, Analysis, Containment, Eradication, Recovery, Post-Incident
/// Includes regulatory notification tracking (PDPL, NCA, SAMA)
/// </summary>
[ApiController]
[Route("api/v1/incidents")]
[Authorize]
public class IncidentController : ControllerBase
{
    private readonly IIncidentResponseService _incidentService;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<IncidentController> _logger;

    public IncidentController(
        IIncidentResponseService incidentService,
        ITenantContextService tenantContext,
        ILogger<IncidentController> logger)
    {
        _incidentService = incidentService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Incident CRUD

    /// <summary>
    /// Create/report a new incident
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        try
        {
            request.TenantId = _tenantContext.GetCurrentTenantId();
            var result = await _incidentService.CreateIncidentAsync(request);
            return CreatedAtAction(nameof(GetIncident), new { id = result.Id }, new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident");
            return BadRequest(new { success = false, error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Get incident by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetIncident(Guid id)
    {
        var result = await _incidentService.GetByIdAsync(id);
        if (result == null) return NotFound(new { success = false, error = "Incident not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incident with full timeline
    /// </summary>
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetIncidentDetail(Guid id)
    {
        var result = await _incidentService.GetDetailAsync(id);
        if (result == null) return NotFound(new { success = false, error = "Incident not found" });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Update incident
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateIncidentRequest request)
    {
        var result = await _incidentService.UpdateIncidentAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Get all incidents
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllIncidents()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetAllAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Search incidents
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchIncidents([FromBody] IncidentSearchRequest request)
    {
        request.TenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.SearchAsync(request);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Incident Lifecycle

    /// <summary>
    /// Start investigation
    /// </summary>
    [HttpPost("{id}/investigate")]
    public async Task<IActionResult> StartInvestigation(Guid id, [FromBody] StartInvestigationRequest request)
    {
        var result = await _incidentService.StartInvestigationAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Mark incident as contained
    /// </summary>
    [HttpPost("{id}/contain")]
    public async Task<IActionResult> MarkContained(Guid id, [FromBody] ContainmentRequest request)
    {
        var result = await _incidentService.MarkContainedAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Mark incident as eradicated
    /// </summary>
    [HttpPost("{id}/eradicate")]
    public async Task<IActionResult> MarkEradicated(Guid id, [FromBody] EradicationRequest request)
    {
        var result = await _incidentService.MarkEradicatedAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Mark incident as recovered
    /// </summary>
    [HttpPost("{id}/recover")]
    public async Task<IActionResult> MarkRecovered(Guid id, [FromBody] RecoveryRequest request)
    {
        var result = await _incidentService.MarkRecoveredAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Close incident
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseIncident(Guid id, [FromBody] CloseIncidentRequest request)
    {
        var result = await _incidentService.CloseIncidentAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Reopen incident
    /// </summary>
    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> ReopenIncident(Guid id, [FromBody] ReopenIncidentRequest request)
    {
        var result = await _incidentService.ReopenIncidentAsync(id, request.Reason, request.ReopenedBy);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Mark as false positive
    /// </summary>
    [HttpPost("{id}/false-positive")]
    public async Task<IActionResult> MarkFalsePositive(Guid id, [FromBody] FalsePositiveRequest request)
    {
        var result = await _incidentService.MarkFalsePositiveAsync(id, request.Reason, request.MarkedBy);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Escalate incident
    /// </summary>
    [HttpPost("{id}/escalate")]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] EscalationRequest request)
    {
        var result = await _incidentService.EscalateAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    #endregion

    #region Assignment

    /// <summary>
    /// Assign handler
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignHandler(Guid id, [FromBody] AssignHandlerRequest request)
    {
        var result = await _incidentService.AssignHandlerAsync(id, request.HandlerId, request.HandlerName, request.Team);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Get incidents by handler
    /// </summary>
    [HttpGet("handler/{handlerId}")]
    public async Task<IActionResult> GetByHandler(string handlerId)
    {
        var result = await _incidentService.GetByHandlerAsync(handlerId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incidents by team
    /// </summary>
    [HttpGet("team/{team}")]
    public async Task<IActionResult> GetByTeam(string team)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetByTeamAsync(team, tenantId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Timeline

    /// <summary>
    /// Add timeline entry
    /// </summary>
    [HttpPost("{id}/timeline")]
    public async Task<IActionResult> AddTimelineEntry(Guid id, [FromBody] AddTimelineEntryRequest request)
    {
        var result = await _incidentService.AddTimelineEntryAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Get timeline
    /// </summary>
    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id)
    {
        var result = await _incidentService.GetTimelineAsync(id);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Add note
    /// </summary>
    [HttpPost("{id}/note")]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest request)
    {
        var result = await _incidentService.AddNoteAsync(id, request.Note, request.AddedBy);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    #endregion

    #region Regulatory Notifications

    /// <summary>
    /// Check notification requirements
    /// </summary>
    [HttpGet("{id}/notification-requirements")]
    public async Task<IActionResult> CheckNotificationRequirements(Guid id)
    {
        var result = await _incidentService.CheckNotificationRequirementsAsync(id);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Mark notification as sent
    /// </summary>
    [HttpPost("{id}/notification-sent")]
    public async Task<IActionResult> MarkNotificationSent(Guid id, [FromBody] MarkNotificationRequest request)
    {
        var result = await _incidentService.MarkNotificationSentAsync(id, request);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, data = result.Value });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Get pending notifications
    /// </summary>
    [HttpGet("pending-notifications")]
    public async Task<IActionResult> GetPendingNotifications()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetPendingNotificationsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Risk & Control Linkage

    /// <summary>
    /// Link to risk
    /// </summary>
    [HttpPost("{id}/link-risk/{riskId}")]
    public async Task<IActionResult> LinkToRisk(Guid id, Guid riskId)
    {
        var result = await _incidentService.LinkToRiskAsync(id, riskId);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, message = "Linked to risk" });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Link to control
    /// </summary>
    [HttpPost("{id}/link-control/{controlId}")]
    public async Task<IActionResult> LinkToControl(Guid id, Guid controlId)
    {
        var result = await _incidentService.LinkToControlAsync(id, controlId);
        if (result.IsSuccess)
        {
            return Ok(new { success = true, message = "Linked to control" });
        }
        
        return result.Error.Code switch
        {
            ErrorCode.NotFound => NotFound(new { success = false, error = result.Error.Message }),
            _ => BadRequest(new { success = false, error = result.Error.Message })
        };
    }

    /// <summary>
    /// Get incidents by risk
    /// </summary>
    [HttpGet("by-risk/{riskId}")]
    public async Task<IActionResult> GetByRisk(Guid riskId)
    {
        var result = await _incidentService.GetByRiskAsync(riskId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incidents by control
    /// </summary>
    [HttpGet("by-control/{controlId}")]
    public async Task<IActionResult> GetByControl(Guid controlId)
    {
        var result = await _incidentService.GetByControlAsync(controlId);
        return Ok(new { success = true, data = result });
    }

    #endregion

    #region Dashboard & Reporting

    /// <summary>
    /// Get incident dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetDashboardAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incident statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetStatisticsAsync(tenantId, fromDate, toDate);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get open incidents summary
    /// </summary>
    [HttpGet("open")]
    public async Task<IActionResult> GetOpenIncidents()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetOpenIncidentsAsync(tenantId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incident metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics([FromQuery] int months = 12)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetMetricsAsync(tenantId, months);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Get incident trend
    /// </summary>
    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend([FromQuery] int months = 12)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var result = await _incidentService.GetTrendAsync(tenantId, months);
        return Ok(new { success = true, data = result });
    }

    #endregion
}

#region Request DTOs

public class ReopenIncidentRequest
{
    public string Reason { get; set; } = string.Empty;
    public string ReopenedBy { get; set; } = string.Empty;
}

public class FalsePositiveRequest
{
    public string Reason { get; set; } = string.Empty;
    public string MarkedBy { get; set; } = string.Empty;
}

public class AssignHandlerRequest
{
    public string HandlerId { get; set; } = string.Empty;
    public string HandlerName { get; set; } = string.Empty;
    public string? Team { get; set; }
}

public class AddNoteRequest
{
    public string Note { get; set; } = string.Empty;
    public string AddedBy { get; set; } = string.Empty;
}

#endregion
