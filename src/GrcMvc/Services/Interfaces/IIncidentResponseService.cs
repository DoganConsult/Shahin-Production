using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Common.Results;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Incident Response Service Interface
/// Manages the complete incident lifecycle:
/// Detection → Analysis → Containment → Eradication → Recovery → Post-Incident Review
/// Supports regulatory notification requirements (PDPL, NCA, SAMA)
/// </summary>
public interface IIncidentResponseService
{
    #region Incident CRUD
    
    /// <summary>
    /// Create/report a new incident
    /// </summary>
    Task<IncidentResponseDto> CreateIncidentAsync(CreateIncidentRequest request);
    
    /// <summary>
    /// Get incident by ID
    /// </summary>
    Task<IncidentResponseDto?> GetByIdAsync(Guid incidentId);
    
    /// <summary>
    /// Get incident with full timeline
    /// </summary>
    Task<IncidentDetailDto?> GetDetailAsync(Guid incidentId);
    
    /// <summary>
    /// Update incident
    /// </summary>
    Task<Result<IncidentResponseDto>> UpdateIncidentAsync(Guid incidentId, UpdateIncidentRequest request);
    
    /// <summary>
    /// Get all incidents for tenant
    /// </summary>
    Task<List<IncidentResponseDto>> GetAllAsync(Guid tenantId);
    
    /// <summary>
    /// Search incidents
    /// </summary>
    Task<PagedResult<IncidentResponseDto>> SearchAsync(IncidentSearchRequest request);
    
    #endregion

    #region Incident Lifecycle
    
    /// <summary>
    /// Start investigation
    /// </summary>
    Task<Result<IncidentResponseDto>> StartInvestigationAsync(Guid incidentId, StartInvestigationRequest request);
    
    /// <summary>
    /// Mark incident as contained
    /// </summary>
    Task<Result<IncidentResponseDto>> MarkContainedAsync(Guid incidentId, ContainmentRequest request);
    
    /// <summary>
    /// Mark incident as eradicated
    /// </summary>
    Task<Result<IncidentResponseDto>> MarkEradicatedAsync(Guid incidentId, EradicationRequest request);
    
    /// <summary>
    /// Mark incident as recovered
    /// </summary>
    Task<Result<IncidentResponseDto>> MarkRecoveredAsync(Guid incidentId, RecoveryRequest request);
    
    /// <summary>
    /// Close incident
    /// </summary>
    Task<Result<IncidentResponseDto>> CloseIncidentAsync(Guid incidentId, CloseIncidentRequest request);
    
    /// <summary>
    /// Reopen incident
    /// </summary>
    Task<Result<IncidentResponseDto>> ReopenIncidentAsync(Guid incidentId, string reason, string reopenedBy);
    
    /// <summary>
    /// Mark as false positive
    /// </summary>
    Task<Result<IncidentResponseDto>> MarkFalsePositiveAsync(Guid incidentId, string reason, string markedBy);
    
    /// <summary>
    /// Escalate incident
    /// </summary>
    Task<Result<IncidentResponseDto>> EscalateAsync(Guid incidentId, EscalationRequest request);
    
    #endregion

    #region Assignment & Ownership
    
    /// <summary>
    /// Assign handler to incident
    /// </summary>
    Task<Result<IncidentResponseDto>> AssignHandlerAsync(Guid incidentId, string handlerId, string handlerName, string? team = null);
    
    /// <summary>
    /// Reassign incident
    /// </summary>
    Task<Result<IncidentResponseDto>> ReassignAsync(Guid incidentId, string newHandlerId, string newHandlerName, string reason);
    
    /// <summary>
    /// Get incidents by handler
    /// </summary>
    Task<List<IncidentResponseDto>> GetByHandlerAsync(string handlerId);
    
    /// <summary>
    /// Get incidents by team
    /// </summary>
    Task<List<IncidentResponseDto>> GetByTeamAsync(string team, Guid tenantId);
    
    #endregion

    #region Timeline & Communication
    
    /// <summary>
    /// Add timeline entry
    /// </summary>
    Task<Result<IncidentTimelineDto>> AddTimelineEntryAsync(Guid incidentId, AddTimelineEntryRequest request);
    
    /// <summary>
    /// Get timeline for incident
    /// </summary>
    Task<List<IncidentTimelineDto>> GetTimelineAsync(Guid incidentId);
    
    /// <summary>
    /// Add internal note
    /// </summary>
    Task<Result<IncidentTimelineDto>> AddNoteAsync(Guid incidentId, string note, string addedBy);
    
    #endregion

    #region Regulatory Notifications
    
    /// <summary>
    /// Check if incident requires regulatory notification
    /// </summary>
    Task<Result<NotificationRequirementDto>> CheckNotificationRequirementsAsync(Guid incidentId);
    
    /// <summary>
    /// Mark notification as sent
    /// </summary>
    Task<Result<IncidentResponseDto>> MarkNotificationSentAsync(Guid incidentId, MarkNotificationRequest request);
    
    /// <summary>
    /// Get incidents pending notification
    /// </summary>
    Task<List<IncidentResponseDto>> GetPendingNotificationsAsync(Guid tenantId);
    
    /// <summary>
    /// Get PDPL breach notification deadline
    /// </summary>
    Task<DateTime?> GetPdplNotificationDeadlineAsync(Guid incidentId);
    
    #endregion

    #region Risk & Control Linkage
    
    /// <summary>
    /// Link incident to risk
    /// </summary>
    Task<Result> LinkToRiskAsync(Guid incidentId, Guid riskId);
    
    /// <summary>
    /// Link incident to control failure
    /// </summary>
    Task<Result> LinkToControlAsync(Guid incidentId, Guid controlId);
    
    /// <summary>
    /// Get incidents related to a risk
    /// </summary>
    Task<List<IncidentResponseDto>> GetByRiskAsync(Guid riskId);
    
    /// <summary>
    /// Get incidents related to a control
    /// </summary>
    Task<List<IncidentResponseDto>> GetByControlAsync(Guid controlId);
    
    #endregion

    #region Reporting & Analytics
    
    /// <summary>
    /// Get incident dashboard
    /// </summary>
    Task<IncidentDashboardDto> GetDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get incident statistics
    /// </summary>
    Task<IncidentStatisticsDto> GetStatisticsAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get open incidents summary
    /// </summary>
    Task<List<IncidentSummaryDto>> GetOpenIncidentsAsync(Guid tenantId);
    
    /// <summary>
    /// Get incident metrics (MTTR, etc.)
    /// </summary>
    Task<IncidentResponseMetricsDto> GetMetricsAsync(Guid tenantId, int months = 12);
    
    /// <summary>
    /// Get incident trend report
    /// </summary>
    Task<List<IncidentTrendDto>> GetTrendAsync(Guid tenantId, int months = 12);
    
    #endregion
}

#region DTOs

/// <summary>
/// Request to create an incident
/// </summary>
public class CreateIncidentRequest
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = "Security";
    public string Type { get; set; } = "Other";
    public string Severity { get; set; } = "Medium";
    public string Priority { get; set; } = "Normal";
    public string? DetectionSource { get; set; }
    public DateTime? OccurredAt { get; set; }
    public string? AffectedSystems { get; set; }
    public string? AffectedBusinessUnits { get; set; }
    public bool PersonalDataAffected { get; set; }
    public string? ReportedById { get; set; }
    public string? ReportedByName { get; set; }
    public string? HandlerId { get; set; }
    public string? HandlerName { get; set; }
    public string? AssignedTeam { get; set; }
}

/// <summary>
/// Request to update an incident
/// </summary>
public class UpdateIncidentRequest
{
    public string? Title { get; set; }
    public string? TitleAr { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Priority { get; set; }
    public string? AffectedSystems { get; set; }
    public string? AffectedBusinessUnits { get; set; }
    public int? AffectedUsersCount { get; set; }
    public int? AffectedRecordsCount { get; set; }
    public bool? PersonalDataAffected { get; set; }
    public decimal? EstimatedImpact { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string? UpdateReason { get; set; }
}

/// <summary>
/// Incident Response DTO
/// </summary>
public class IncidentResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string? DetectionSource { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? OccurredAt { get; set; }
    public DateTime? ContainedAt { get; set; }
    public DateTime? RecoveredAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? HandlerName { get; set; }
    public string? AssignedTeam { get; set; }
    public string? AffectedSystems { get; set; }
    public bool PersonalDataAffected { get; set; }
    public bool RequiresNotification { get; set; }
    public bool NotificationSent { get; set; }
    public DateTime? NotificationDeadline { get; set; }
    public decimal? EstimatedImpact { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// Incident detail with timeline
/// </summary>
public class IncidentDetailDto : IncidentResponseDto
{
    public string? RootCause { get; set; }
    public string? ContainmentActions { get; set; }
    public string? EradicationActions { get; set; }
    public string? RecoveryActions { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Recommendations { get; set; }
    public decimal? ActualImpact { get; set; }
    public List<IncidentTimelineDto> Timeline { get; set; } = new();
    public List<Guid> RelatedRiskIds { get; set; } = new();
    public List<Guid> RelatedControlIds { get; set; } = new();
}

/// <summary>
/// Incident timeline entry DTO
/// </summary>
public class IncidentTimelineDto
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phase { get; set; }
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsInternal { get; set; }
}

/// <summary>
/// Request to start investigation
/// </summary>
public class StartInvestigationRequest
{
    public string HandlerId { get; set; } = string.Empty;
    public string HandlerName { get; set; } = string.Empty;
    public string? InitialAssessment { get; set; }
    public string? Team { get; set; }
}

/// <summary>
/// Request to mark containment
/// </summary>
public class ContainmentRequest
{
    public string ContainmentActions { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime? ContainedAt { get; set; }
}

/// <summary>
/// Request to mark eradication
/// </summary>
public class EradicationRequest
{
    public string EradicationActions { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request to mark recovery
/// </summary>
public class RecoveryRequest
{
    public string RecoveryActions { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime? RecoveredAt { get; set; }
}

/// <summary>
/// Request to close incident
/// </summary>
public class CloseIncidentRequest
{
    public string? LessonsLearned { get; set; }
    public string? Recommendations { get; set; }
    public decimal? ActualImpact { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
    public string? ClosureNotes { get; set; }
}

/// <summary>
/// Request to escalate
/// </summary>
public class EscalationRequest
{
    public string NewSeverity { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string EscalatedBy { get; set; } = string.Empty;
    public string? EscalateTo { get; set; }
}

/// <summary>
/// Request to add timeline entry
/// </summary>
public class AddTimelineEntryRequest
{
    public string EntryType { get; set; } = "Update";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PerformedById { get; set; } = string.Empty;
    public string PerformedByName { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = true;
    public List<string>? Attachments { get; set; }
}

/// <summary>
/// Notification requirement check result
/// </summary>
public class NotificationRequirementDto
{
    public Guid IncidentId { get; set; }
    public bool RequiresNotification { get; set; }
    public List<string> Regulators { get; set; } = new();
    public Dictionary<string, DateTime> Deadlines { get; set; } = new();
    public string? Reason { get; set; }
    public bool PdplApplicable { get; set; }
    public bool NcaApplicable { get; set; }
    public bool SamaApplicable { get; set; }
}

/// <summary>
/// Request to mark notification sent
/// </summary>
public class MarkNotificationRequest
{
    public List<string> Regulators { get; set; } = new();
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public string SentBy { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Incident search request
/// </summary>
public class IncidentSearchRequest
{
    public Guid TenantId { get; set; }
    public string? SearchText { get; set; }
    public List<string>? Statuses { get; set; }
    public List<string>? Severities { get; set; }
    public List<string>? Categories { get; set; }
    public string? HandlerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? RequiresNotification { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "DetectedAt";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Incident dashboard
/// </summary>
public class IncidentDashboardDto
{
    public Guid TenantId { get; set; }
    public IncidentStatisticsDto Statistics { get; set; } = new();
    public List<IncidentSummaryDto> OpenIncidents { get; set; } = new();
    public List<IncidentResponseDto> RecentIncidents { get; set; } = new();
    public List<IncidentResponseDto> CriticalIncidents { get; set; } = new();
    public List<IncidentResponseDto> PendingNotifications { get; set; } = new();
    public IncidentResponseMetricsDto Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Incident statistics
/// </summary>
public class IncidentStatisticsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int CriticalOpen { get; set; }
    public int HighOpen { get; set; }
    public int ClosedThisMonth { get; set; }
    public int NewThisMonth { get; set; }
    public Dictionary<string, int> BySeverity { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByPhase { get; set; } = new();
}

/// <summary>
/// Incident summary
/// </summary>
public class IncidentSummaryDto
{
    public Guid Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public int DaysOpen { get; set; }
    public string? HandlerName { get; set; }
}

/// <summary>
/// Incident metrics (MTTR, etc.)
/// </summary>
public class IncidentResponseMetricsDto
{
    public Guid TenantId { get; set; }
    public decimal MeanTimeToDetect { get; set; } // Hours
    public decimal MeanTimeToContain { get; set; } // Hours
    public decimal MeanTimeToRecover { get; set; } // Hours
    public decimal MeanTimeToClose { get; set; } // Hours
    public int TotalIncidentsAnalyzed { get; set; }
    public decimal ResolutionRate { get; set; } // %
    public decimal EscalationRate { get; set; } // %
    public decimal FalsePositiveRate { get; set; } // %
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Incident trend data point
/// </summary>
public record IncidentTrendDto(
    int Year,
    int Month,
    int NewIncidents,
    int ClosedIncidents,
    int CriticalIncidents,
    decimal AvgTimeToClose
);

#endregion
