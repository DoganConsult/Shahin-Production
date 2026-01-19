using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Incident Response Service Implementation
/// Manages the complete incident lifecycle for resilience stage
/// </summary>
public class IncidentResponseService : IIncidentResponseService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<IncidentResponseService> _logger;
    private readonly ITenantContextService _tenantContext;

    public IncidentResponseService(
        GrcDbContext context,
        ILogger<IncidentResponseService> logger,
        ITenantContextService tenantContext)
    {
        _context = context;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    #region Incident CRUD

    public async Task<IncidentResponseDto> CreateIncidentAsync(CreateIncidentRequest request)
    {
        var incidentNumber = await GenerateIncidentNumberAsync(request.TenantId);

        var incident = new Incident
        {
            TenantId = request.TenantId,
            IncidentNumber = incidentNumber,
            Title = request.Title,
            TitleAr = request.TitleAr,
            Description = request.Description,
            Category = request.Category,
            Type = request.Type,
            Severity = request.Severity,
            Priority = request.Priority,
            Status = "Open",
            Phase = "Detection",
            DetectionSource = request.DetectionSource,
            DetectedAt = DateTime.UtcNow,
            OccurredAt = request.OccurredAt,
            AffectedSystems = request.AffectedSystems,
            AffectedBusinessUnits = request.AffectedBusinessUnits,
            PersonalDataAffected = request.PersonalDataAffected,
            ReportedById = request.ReportedById,
            ReportedByName = request.ReportedByName,
            HandlerId = request.HandlerId,
            HandlerName = request.HandlerName,
            AssignedTeam = request.AssignedTeam
        };

        // Check if notification is required (PDPL/NCA/SAMA)
        incident.RequiresNotification = CheckNotificationRequired(incident);
        if (incident.RequiresNotification)
        {
            incident.NotificationDeadline = CalculateNotificationDeadline(incident);
            incident.RegulatorsToNotify = DetermineRegulatorsToNotify(incident);
        }

        _context.Set<Incident>().Add(incident);

        // Add initial timeline entry
        var timelineEntry = new IncidentTimelineEntry
        {
            TenantId = request.TenantId,
            IncidentId = incident.Id,
            EntryType = "Detection",
            Title = "Incident Reported",
            Description = $"Incident '{request.Title}' was reported with severity: {request.Severity}",
            Phase = "Detection",
            PerformedById = request.ReportedById,
            PerformedByName = request.ReportedByName ?? "System",
            Timestamp = DateTime.UtcNow
        };
        _context.Set<IncidentTimelineEntry>().Add(timelineEntry);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created incident {IncidentNumber} with severity {Severity}", 
            incidentNumber, request.Severity);

        return MapToDto(incident);
    }

    public async Task<IncidentResponseDto?> GetByIdAsync(Guid incidentId)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        return incident != null ? MapToDto(incident) : null;
    }

    public async Task<IncidentDetailDto?> GetDetailAsync(Guid incidentId)
    {
        var incident = await _context.Set<Incident>()
            .Include(i => i.TimelineEntries)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        return incident != null ? MapToDetailDto(incident) : null;
    }

    public async Task<Result<IncidentResponseDto>> UpdateIncidentAsync(Guid incidentId, UpdateIncidentRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        if (request.Title != null) incident.Title = request.Title;
        if (request.TitleAr != null) incident.TitleAr = request.TitleAr;
        if (request.Description != null) incident.Description = request.Description;
        if (request.Category != null) incident.Category = request.Category;
        if (request.Type != null) incident.Type = request.Type;
        if (request.Severity != null) incident.Severity = request.Severity;
        if (request.Priority != null) incident.Priority = request.Priority;
        if (request.AffectedSystems != null) incident.AffectedSystems = request.AffectedSystems;
        if (request.AffectedBusinessUnits != null) incident.AffectedBusinessUnits = request.AffectedBusinessUnits;
        if (request.AffectedUsersCount.HasValue) incident.AffectedUsersCount = request.AffectedUsersCount;
        if (request.AffectedRecordsCount.HasValue) incident.AffectedRecordsCount = request.AffectedRecordsCount;
        if (request.PersonalDataAffected.HasValue) incident.PersonalDataAffected = request.PersonalDataAffected.Value;
        if (request.EstimatedImpact.HasValue) incident.EstimatedImpact = request.EstimatedImpact;

        incident.ModifiedDate = DateTime.UtcNow;

        // Add timeline entry
        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Incident Updated",
            Description = request.UpdateReason ?? "Incident details were updated",
            PerformedByName = request.UpdatedBy
        });

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<List<IncidentResponseDto>> GetAllAsync(Guid tenantId)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<IncidentResponseDto>> SearchAsync(IncidentSearchRequest request)
    {
        var query = _context.Set<Incident>()
            .Where(i => i.TenantId == request.TenantId);

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(i => 
                i.Title.Contains(request.SearchText) || 
                i.IncidentNumber.Contains(request.SearchText) ||
                (i.Description != null && i.Description.Contains(request.SearchText)));
        }

        if (request.Statuses?.Any() == true)
            query = query.Where(i => request.Statuses.Contains(i.Status));

        if (request.Severities?.Any() == true)
            query = query.Where(i => request.Severities.Contains(i.Severity));

        if (request.Categories?.Any() == true)
            query = query.Where(i => request.Categories.Contains(i.Category));

        if (!string.IsNullOrEmpty(request.HandlerId))
            query = query.Where(i => i.HandlerId == request.HandlerId);

        if (request.FromDate.HasValue)
            query = query.Where(i => i.DetectedAt >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(i => i.DetectedAt <= request.ToDate);

        if (request.RequiresNotification.HasValue)
            query = query.Where(i => i.RequiresNotification == request.RequiresNotification);

        var totalCount = await query.CountAsync();

        query = request.SortBy switch
        {
            "Severity" => request.SortDescending ? query.OrderByDescending(i => i.Severity) : query.OrderBy(i => i.Severity),
            "Status" => request.SortDescending ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            _ => request.SortDescending ? query.OrderByDescending(i => i.DetectedAt) : query.OrderBy(i => i.DetectedAt)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<IncidentResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    #endregion

    #region Incident Lifecycle

    public async Task<Result<IncidentResponseDto>> StartInvestigationAsync(Guid incidentId, StartInvestigationRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Investigating";
        incident.Phase = "Analysis";
        incident.HandlerId = request.HandlerId;
        incident.HandlerName = request.HandlerName;
        incident.AssignedTeam = request.Team;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Investigation Started",
            Description = request.InitialAssessment ?? $"Investigation assigned to {request.HandlerName}",
            PerformedByName = request.HandlerName
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Started investigation for incident {IncidentNumber}", incident.IncidentNumber);

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> MarkContainedAsync(Guid incidentId, ContainmentRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Contained";
        incident.Phase = "Containment";
        incident.ContainedAt = request.ContainedAt ?? DateTime.UtcNow;
        incident.ContainmentActions = request.ContainmentActions;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Containment",
            Title = "Incident Contained",
            Description = request.ContainmentActions,
            PerformedByName = request.PerformedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> MarkEradicatedAsync(Guid incidentId, EradicationRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Eradicated";
        incident.Phase = "Eradication";
        incident.EradicatedAt = DateTime.UtcNow;
        incident.EradicationActions = request.EradicationActions;
        incident.RootCause = request.RootCause;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Incident Eradicated",
            Description = $"Root cause: {request.RootCause}\n\nActions: {request.EradicationActions}",
            PerformedByName = request.PerformedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> MarkRecoveredAsync(Guid incidentId, RecoveryRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Recovered";
        incident.Phase = "Recovery";
        incident.RecoveredAt = request.RecoveredAt ?? DateTime.UtcNow;
        incident.RecoveryActions = request.RecoveryActions;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Recovery",
            Title = "Incident Recovered",
            Description = request.RecoveryActions,
            PerformedByName = request.PerformedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> CloseIncidentAsync(Guid incidentId, CloseIncidentRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Closed";
        incident.Phase = "PostIncident";
        incident.ClosedAt = DateTime.UtcNow;
        incident.LessonsLearned = request.LessonsLearned;
        incident.Recommendations = request.Recommendations;
        if (request.ActualImpact.HasValue) incident.ActualImpact = request.ActualImpact;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Closure",
            Title = "Incident Closed",
            Description = request.ClosureNotes ?? "Incident has been resolved and closed",
            PerformedByName = request.ClosedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Closed incident {IncidentNumber}", incident.IncidentNumber);

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> ReopenIncidentAsync(Guid incidentId, string reason, string reopenedBy)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "Open";
        incident.Phase = "Analysis";
        incident.ClosedAt = null;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Incident Reopened",
            Description = reason,
            PerformedByName = reopenedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> MarkFalsePositiveAsync(Guid incidentId, string reason, string markedBy)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = "False Positive";
        incident.ClosedAt = DateTime.UtcNow;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Closure",
            Title = "Marked as False Positive",
            Description = reason,
            PerformedByName = markedBy
        }, previousStatus, incident.Status);

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> EscalateAsync(Guid incidentId, EscalationRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousSeverity = incident.Severity;
        incident.Severity = request.NewSeverity;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Escalation",
            Title = $"Escalated to {request.NewSeverity}",
            Description = $"Escalation reason: {request.Reason}\nEscalated to: {request.EscalateTo}",
            PerformedByName = request.EscalatedBy
        });

        await _context.SaveChangesAsync();

        _logger.LogWarning("Escalated incident {IncidentNumber} from {Previous} to {New}", 
            incident.IncidentNumber, previousSeverity, request.NewSeverity);

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    #endregion

    #region Assignment & Ownership

    public async Task<Result<IncidentResponseDto>> AssignHandlerAsync(Guid incidentId, string handlerId, string handlerName, string? team = null)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        incident.HandlerId = handlerId;
        incident.HandlerName = handlerName;
        if (!string.IsNullOrEmpty(team)) incident.AssignedTeam = team;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Handler Assigned",
            Description = $"Assigned to {handlerName}" + (team != null ? $" ({team})" : ""),
            PerformedByName = handlerName
        });

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<Result<IncidentResponseDto>> ReassignAsync(Guid incidentId, string newHandlerId, string newHandlerName, string reason)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var previousHandler = incident.HandlerName;
        incident.HandlerId = newHandlerId;
        incident.HandlerName = newHandlerName;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Incident Reassigned",
            Description = $"Reassigned from {previousHandler} to {newHandlerName}. Reason: {reason}",
            PerformedByName = newHandlerName
        });

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<List<IncidentResponseDto>> GetByHandlerAsync(string handlerId)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.HandlerId == handlerId && i.Status != "Closed" && i.Status != "False Positive")
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    public async Task<List<IncidentResponseDto>> GetByTeamAsync(string team, Guid tenantId)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && i.AssignedTeam == team)
            .OrderByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    #endregion

    #region Timeline & Communication

    public async Task<Result<IncidentTimelineDto>> AddTimelineEntryAsync(Guid incidentId, AddTimelineEntryRequest request)
    {
        var result = await AddTimelineEntryInternalAsync(incidentId, request);
        if (result.IsSuccess)
        {
            return Result<IncidentTimelineDto>.Success(result.Value!);
        }
        return Result<IncidentTimelineDto>.Failure(result.Error);
    }

    public async Task<List<IncidentTimelineDto>> GetTimelineAsync(Guid incidentId)
    {
        var entries = await _context.Set<IncidentTimelineEntry>()
            .Where(e => e.IncidentId == incidentId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();

        return entries.Select(MapToTimelineDto).ToList();
    }

    public async Task<Result<IncidentTimelineDto>> AddNoteAsync(Guid incidentId, string note, string addedBy)
    {
        var result = await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Update",
            Title = "Note Added",
            Description = note,
            PerformedByName = addedBy,
            IsInternal = true
        });
        if (result.IsSuccess)
        {
            return Result<IncidentTimelineDto>.Success(result.Value!);
        }
        return Result<IncidentTimelineDto>.Failure(result.Error);
    }

    #endregion

    #region Regulatory Notifications

    public async Task<Result<NotificationRequirementDto>> CheckNotificationRequirementsAsync(Guid incidentId)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<NotificationRequirementDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var result = new NotificationRequirementDto
        {
            IncidentId = incidentId,
            RequiresNotification = incident.RequiresNotification
        };

        // PDPL: 72 hours for personal data breaches
        if (incident.PersonalDataAffected && 
            (incident.Category == "Security" || incident.Category == "Privacy"))
        {
            result.PdplApplicable = true;
            result.Regulators.Add("PDPL");
            result.Deadlines["PDPL"] = incident.DetectedAt.AddHours(72);
        }

        // NCA: Critical security incidents
        if (incident.Category == "Security" && 
            (incident.Severity == "Critical" || incident.Severity == "High"))
        {
            result.NcaApplicable = true;
            result.Regulators.Add("NCA");
            result.Deadlines["NCA"] = incident.DetectedAt.AddHours(24);
        }

        // SAMA: Financial institution incidents
        if (incident.Category == "Security" && incident.AffectedRecordsCount > 0)
        {
            result.SamaApplicable = true;
            result.Regulators.Add("SAMA");
            result.Deadlines["SAMA"] = incident.DetectedAt.AddHours(48);
        }

        result.RequiresNotification = result.Regulators.Any();

        return Result<NotificationRequirementDto>.Success(result);
    }

    public async Task<Result<IncidentResponseDto>> MarkNotificationSentAsync(Guid incidentId, MarkNotificationRequest request)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentResponseDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        incident.NotificationSent = true;
        incident.NotificationSentDate = request.SentDate;
        incident.ModifiedDate = DateTime.UtcNow;

        await AddTimelineEntryInternalAsync(incidentId, new AddTimelineEntryRequest
        {
            EntryType = "Communication",
            Title = "Regulatory Notification Sent",
            Description = $"Notification sent to: {string.Join(", ", request.Regulators)}\nReference: {request.ReferenceNumber}",
            PerformedByName = request.SentBy
        });

        await _context.SaveChangesAsync();

        return Result<IncidentResponseDto>.Success(MapToDto(incident));
    }

    public async Task<List<IncidentResponseDto>> GetPendingNotificationsAsync(Guid tenantId)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && 
                       i.RequiresNotification && 
                       !i.NotificationSent &&
                       i.Status != "Closed" && 
                       i.Status != "False Positive")
            .OrderBy(i => i.NotificationDeadline)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    public Task<DateTime?> GetPdplNotificationDeadlineAsync(Guid incidentId)
    {
        // PDPL requires notification within 72 hours
        return Task.FromResult<DateTime?>(DateTime.UtcNow.AddHours(72));
    }

    #endregion

    #region Risk & Control Linkage

    public async Task<Result> LinkToRiskAsync(Guid incidentId, Guid riskId)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result.Failure(Error.NotFound("Incident", incidentId));
        }

        var riskIds = string.IsNullOrEmpty(incident.RelatedRiskIds)
            ? new List<Guid>()
            : JsonSerializer.Deserialize<List<Guid>>(incident.RelatedRiskIds) ?? new List<Guid>();

        if (!riskIds.Contains(riskId))
        {
            riskIds.Add(riskId);
            incident.RelatedRiskIds = JsonSerializer.Serialize(riskIds);
            incident.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return Result.Success();
    }

    public async Task<Result> LinkToControlAsync(Guid incidentId, Guid controlId)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result.Failure(Error.NotFound("Incident", incidentId));
        }

        var controlIds = string.IsNullOrEmpty(incident.RelatedControlIds)
            ? new List<Guid>()
            : JsonSerializer.Deserialize<List<Guid>>(incident.RelatedControlIds) ?? new List<Guid>();

        if (!controlIds.Contains(controlId))
        {
            controlIds.Add(controlId);
            incident.RelatedControlIds = JsonSerializer.Serialize(controlIds);
            incident.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return Result.Success();
    }

    public async Task<List<IncidentResponseDto>> GetByRiskAsync(Guid riskId)
    {
        var searchStr = riskId.ToString();
        var incidents = await _context.Set<Incident>()
            .Where(i => i.RelatedRiskIds != null && i.RelatedRiskIds.Contains(searchStr))
            .OrderByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    public async Task<List<IncidentResponseDto>> GetByControlAsync(Guid controlId)
    {
        var searchStr = controlId.ToString();
        var incidents = await _context.Set<Incident>()
            .Where(i => i.RelatedControlIds != null && i.RelatedControlIds.Contains(searchStr))
            .OrderByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    #endregion

    #region Reporting & Analytics

    public async Task<IncidentDashboardDto> GetDashboardAsync(Guid tenantId)
    {
        return new IncidentDashboardDto
        {
            TenantId = tenantId,
            Statistics = await GetStatisticsAsync(tenantId),
            OpenIncidents = await GetOpenIncidentsAsync(tenantId),
            RecentIncidents = (await GetRecentIncidentsAsync(tenantId, 10)).ToList(),
            CriticalIncidents = await GetCriticalIncidentsAsync(tenantId),
            PendingNotifications = await GetPendingNotificationsAsync(tenantId),
            Metrics = await GetMetricsAsync(tenantId),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IncidentStatisticsDto> GetStatisticsAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Incident>().Where(i => i.TenantId == tenantId);

        if (fromDate.HasValue) query = query.Where(i => i.DetectedAt >= fromDate);
        if (toDate.HasValue) query = query.Where(i => i.DetectedAt <= toDate);

        var incidents = await query.ToListAsync();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        return new IncidentStatisticsDto
        {
            TotalIncidents = incidents.Count,
            OpenIncidents = incidents.Count(i => i.Status != "Closed" && i.Status != "False Positive"),
            CriticalOpen = incidents.Count(i => i.Severity == "Critical" && i.Status != "Closed"),
            HighOpen = incidents.Count(i => i.Severity == "High" && i.Status != "Closed"),
            ClosedThisMonth = incidents.Count(i => i.ClosedAt.HasValue && i.ClosedAt >= monthStart),
            NewThisMonth = incidents.Count(i => i.DetectedAt >= monthStart),
            BySeverity = incidents.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count()),
            ByStatus = incidents.GroupBy(i => i.Status).ToDictionary(g => g.Key, g => g.Count()),
            ByCategory = incidents.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.Count()),
            ByPhase = incidents.GroupBy(i => i.Phase).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<List<IncidentSummaryDto>> GetOpenIncidentsAsync(Guid tenantId)
    {
        var now = DateTime.UtcNow;
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && 
                       i.Status != "Closed" && 
                       i.Status != "False Positive")
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(i => new IncidentSummaryDto
        {
            Id = i.Id,
            IncidentNumber = i.IncidentNumber,
            Title = i.Title,
            Severity = i.Severity,
            Status = i.Status,
            Phase = i.Phase,
            DetectedAt = i.DetectedAt,
            DaysOpen = (int)(now - i.DetectedAt).TotalDays,
            HandlerName = i.HandlerName
        }).ToList();
    }

    public async Task<IncidentResponseMetricsDto> GetMetricsAsync(Guid tenantId, int months = 12)
    {
        var fromDate = DateTime.UtcNow.AddMonths(-months);

        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && 
                       i.DetectedAt >= fromDate && 
                       i.Status == "Closed")
            .ToListAsync();

        var mttr = incidents.Where(i => i.ClosedAt.HasValue)
            .Select(i => (i.ClosedAt!.Value - i.DetectedAt).TotalHours)
            .DefaultIfEmpty(0)
            .Average();

        var mttc = incidents.Where(i => i.ContainedAt.HasValue)
            .Select(i => (i.ContainedAt!.Value - i.DetectedAt).TotalHours)
            .DefaultIfEmpty(0)
            .Average();

        var allIncidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && i.DetectedAt >= fromDate)
            .ToListAsync();

        var falsePositives = allIncidents.Count(i => i.Status == "False Positive");

        return new IncidentResponseMetricsDto
        {
            TenantId = tenantId,
            MeanTimeToDetect = 0, // Would require OccurredAt tracking
            MeanTimeToContain = (decimal)mttc,
            MeanTimeToRecover = (decimal)(incidents.Where(i => i.RecoveredAt.HasValue)
                .Select(i => (i.RecoveredAt!.Value - i.DetectedAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average()),
            MeanTimeToClose = (decimal)mttr,
            TotalIncidentsAnalyzed = incidents.Count,
            ResolutionRate = allIncidents.Any() 
                ? (decimal)incidents.Count / allIncidents.Count * 100 
                : 0,
            FalsePositiveRate = allIncidents.Any() 
                ? (decimal)falsePositives / allIncidents.Count * 100 
                : 0,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public async Task<List<IncidentTrendDto>> GetTrendAsync(Guid tenantId, int months = 12)
    {
        var fromDate = DateTime.UtcNow.AddMonths(-months);

        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && i.DetectedAt >= fromDate)
            .ToListAsync();

        return incidents
            .GroupBy(i => new { i.DetectedAt.Year, i.DetectedAt.Month })
            .Select(g => new IncidentTrendDto(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Count(i => i.ClosedAt.HasValue && i.ClosedAt.Value.Year == g.Key.Year && i.ClosedAt.Value.Month == g.Key.Month),
                g.Count(i => i.Severity == "Critical"),
                g.Where(i => i.ClosedAt.HasValue)
                    .Select(i => (decimal)(i.ClosedAt!.Value - i.DetectedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            ))
            .OrderBy(t => t.Year)
            .ThenBy(t => t.Month)
            .ToList();
    }

    #endregion

    #region Private Helper Methods

    private async Task<string> GenerateIncidentNumberAsync(Guid tenantId)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && i.CreatedDate.Year == year)
            .CountAsync();

        return $"INC-{year}-{(count + 1):D5}";
    }

    private async Task<Result<IncidentTimelineDto>> AddTimelineEntryInternalAsync(
        Guid incidentId, 
        AddTimelineEntryRequest request,
        string? statusBefore = null,
        string? statusAfter = null)
    {
        var incident = await _context.Set<Incident>().FindAsync(incidentId);
        if (incident == null)
        {
            return Result<IncidentTimelineDto>.Failure(Error.NotFound("Incident", incidentId));
        }

        var entry = new IncidentTimelineEntry
        {
            TenantId = incident.TenantId ?? Guid.Empty,
            IncidentId = incidentId,
            EntryType = request.EntryType,
            Title = request.Title,
            Description = request.Description,
            Phase = incident.Phase,
            StatusBefore = statusBefore,
            StatusAfter = statusAfter,
            PerformedById = request.PerformedById,
            PerformedByName = request.PerformedByName,
            IsInternal = request.IsInternal,
            Attachments = request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : null,
            Timestamp = DateTime.UtcNow
        };

        _context.Set<IncidentTimelineEntry>().Add(entry);
        await _context.SaveChangesAsync();

        return Result<IncidentTimelineDto>.Success(MapToTimelineDto(entry));
    }

    private async Task<IEnumerable<IncidentResponseDto>> GetRecentIncidentsAsync(Guid tenantId, int limit)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.DetectedAt)
            .Take(limit)
            .ToListAsync();

        return incidents.Select(MapToDto);
    }

    private async Task<List<IncidentResponseDto>> GetCriticalIncidentsAsync(Guid tenantId)
    {
        var incidents = await _context.Set<Incident>()
            .Where(i => i.TenantId == tenantId && 
                       (i.Severity == "Critical" || i.Severity == "High") &&
                       i.Status != "Closed" && 
                       i.Status != "False Positive")
            .OrderByDescending(i => i.Severity)
            .ThenByDescending(i => i.DetectedAt)
            .ToListAsync();

        return incidents.Select(MapToDto).ToList();
    }

    private static bool CheckNotificationRequired(Incident incident)
    {
        // PDPL requires notification for personal data breaches
        if (incident.PersonalDataAffected) return true;

        // NCA requires notification for critical security incidents
        if (incident.Category == "Security" && incident.Severity == "Critical") return true;

        return false;
    }

    private static DateTime? CalculateNotificationDeadline(Incident incident)
    {
        // PDPL: 72 hours
        // NCA: 24 hours for critical
        // SAMA: 48 hours
        
        if (incident.PersonalDataAffected)
            return incident.DetectedAt.AddHours(72);

        if (incident.Severity == "Critical")
            return incident.DetectedAt.AddHours(24);

        return incident.DetectedAt.AddHours(72);
    }

    private static string DetermineRegulatorsToNotify(Incident incident)
    {
        var regulators = new List<string>();

        if (incident.PersonalDataAffected)
            regulators.Add("PDPL");

        if (incident.Category == "Security" && (incident.Severity == "Critical" || incident.Severity == "High"))
            regulators.Add("NCA");

        // Financial sector would add SAMA
        // Telecom sector would add CITC

        return JsonSerializer.Serialize(regulators);
    }

    private static IncidentResponseDto MapToDto(Incident incident)
    {
        return new IncidentResponseDto
        {
            Id = incident.Id,
            TenantId = incident.TenantId ?? Guid.Empty,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            TitleAr = incident.TitleAr,
            Description = incident.Description,
            Category = incident.Category,
            Type = incident.Type,
            Severity = incident.Severity,
            Priority = incident.Priority,
            Status = incident.Status,
            Phase = incident.Phase,
            DetectionSource = incident.DetectionSource,
            DetectedAt = incident.DetectedAt,
            OccurredAt = incident.OccurredAt,
            ContainedAt = incident.ContainedAt,
            RecoveredAt = incident.RecoveredAt,
            ClosedAt = incident.ClosedAt,
            HandlerName = incident.HandlerName,
            AssignedTeam = incident.AssignedTeam,
            AffectedSystems = incident.AffectedSystems,
            PersonalDataAffected = incident.PersonalDataAffected,
            RequiresNotification = incident.RequiresNotification,
            NotificationSent = incident.NotificationSent,
            NotificationDeadline = incident.NotificationDeadline,
            EstimatedImpact = incident.EstimatedImpact,
            CreatedDate = incident.CreatedDate,
            ModifiedDate = incident.ModifiedDate
        };
    }

    private static IncidentDetailDto MapToDetailDto(Incident incident)
    {
        var dto = new IncidentDetailDto
        {
            Id = incident.Id,
            TenantId = incident.TenantId ?? Guid.Empty,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            TitleAr = incident.TitleAr,
            Description = incident.Description,
            Category = incident.Category,
            Type = incident.Type,
            Severity = incident.Severity,
            Priority = incident.Priority,
            Status = incident.Status,
            Phase = incident.Phase,
            DetectionSource = incident.DetectionSource,
            DetectedAt = incident.DetectedAt,
            OccurredAt = incident.OccurredAt,
            ContainedAt = incident.ContainedAt,
            RecoveredAt = incident.RecoveredAt,
            ClosedAt = incident.ClosedAt,
            HandlerName = incident.HandlerName,
            AssignedTeam = incident.AssignedTeam,
            AffectedSystems = incident.AffectedSystems,
            PersonalDataAffected = incident.PersonalDataAffected,
            RequiresNotification = incident.RequiresNotification,
            NotificationSent = incident.NotificationSent,
            NotificationDeadline = incident.NotificationDeadline,
            EstimatedImpact = incident.EstimatedImpact,
            CreatedDate = incident.CreatedDate,
            ModifiedDate = incident.ModifiedDate,
            RootCause = incident.RootCause,
            ContainmentActions = incident.ContainmentActions,
            EradicationActions = incident.EradicationActions,
            RecoveryActions = incident.RecoveryActions,
            LessonsLearned = incident.LessonsLearned,
            Recommendations = incident.Recommendations,
            ActualImpact = incident.ActualImpact,
            Timeline = incident.TimelineEntries.Select(MapToTimelineDto).ToList(),
            RelatedRiskIds = !string.IsNullOrEmpty(incident.RelatedRiskIds)
                ? JsonSerializer.Deserialize<List<Guid>>(incident.RelatedRiskIds) ?? new List<Guid>()
                : new List<Guid>(),
            RelatedControlIds = !string.IsNullOrEmpty(incident.RelatedControlIds)
                ? JsonSerializer.Deserialize<List<Guid>>(incident.RelatedControlIds) ?? new List<Guid>()
                : new List<Guid>()
        };

        return dto;
    }

    private static IncidentTimelineDto MapToTimelineDto(IncidentTimelineEntry entry)
    {
        return new IncidentTimelineDto
        {
            Id = entry.Id,
            IncidentId = entry.IncidentId,
            EntryType = entry.EntryType,
            Title = entry.Title,
            Description = entry.Description,
            Phase = entry.Phase,
            StatusBefore = entry.StatusBefore,
            StatusAfter = entry.StatusAfter,
            PerformedByName = entry.PerformedByName,
            Timestamp = entry.Timestamp,
            IsInternal = entry.IsInternal
        };
    }

    #endregion
}
