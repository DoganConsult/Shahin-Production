using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Compliance Gap Management Service
/// Handles the complete gap remediation lifecycle: Identify → Assess → Plan → Remediate → Validate → Close
/// </summary>
public interface IComplianceGapService
{
    #region Gap Identification
    
    /// <summary>
    /// Identify gaps from assessment results
    /// </summary>
    Task<List<ComplianceGapDetailDto>> IdentifyGapsFromAssessmentAsync(Guid tenantId, Guid assessmentId);
    
    /// <summary>
    /// Get all open gaps for a tenant
    /// </summary>
    Task<List<ComplianceGapDetailDto>> GetOpenGapsAsync(Guid tenantId);
    
    /// <summary>
    /// Get gap by ID
    /// </summary>
    Task<ComplianceGapDetailDto?> GetGapByIdAsync(Guid tenantId, Guid gapId);
    
    /// <summary>
    /// Create a gap manually
    /// </summary>
    Task<ComplianceGapDetailDto> CreateGapAsync(Guid tenantId, CreateComplianceGapRequest request);
    
    #endregion

    #region Remediation Planning
    
    /// <summary>
    /// Create remediation plan for a gap
    /// </summary>
    Task<RemediationPlanDto> CreateRemediationPlanAsync(Guid tenantId, Guid gapId, CreateRemediationPlanRequest request);
    
    /// <summary>
    /// Get remediation plan for a gap
    /// </summary>
    Task<RemediationPlanDto?> GetRemediationPlanAsync(Guid tenantId, Guid gapId);
    
    /// <summary>
    /// Update remediation plan progress
    /// </summary>
    Task<RemediationPlanDto> UpdateRemediationProgressAsync(Guid tenantId, Guid gapId, UpdateRemediationProgressRequest request);
    
    #endregion

    #region Validation & Closure
    
    /// <summary>
    /// Submit gap for validation
    /// </summary>
    Task<ComplianceGapDetailDto> SubmitForValidationAsync(Guid tenantId, Guid gapId, string submittedBy);
    
    /// <summary>
    /// Validate gap remediation
    /// </summary>
    Task<ComplianceGapDetailDto> ValidateRemediationAsync(Guid tenantId, Guid gapId, ValidateRemediationRequest request);
    
    /// <summary>
    /// Close a validated gap
    /// </summary>
    Task<ComplianceGapDetailDto> CloseGapAsync(Guid tenantId, Guid gapId, string closedBy, string? closureNotes = null);
    
    /// <summary>
    /// Reopen a closed gap
    /// </summary>
    Task<ComplianceGapDetailDto> ReopenGapAsync(Guid tenantId, Guid gapId, string reopenedBy, string reason);
    
    #endregion

    #region Reporting
    
    /// <summary>
    /// Get gap summary by framework
    /// </summary>
    Task<GapSummaryDto> GetGapSummaryByFrameworkAsync(Guid tenantId, string frameworkCode);
    
    /// <summary>
    /// Get gap aging report
    /// </summary>
    Task<GapAgingReportDto> GetGapAgingReportAsync(Guid tenantId);
    
    /// <summary>
    /// Get gap closure trend
    /// </summary>
    Task<List<GapTrendPointDto>> GetGapClosureTrendAsync(Guid tenantId, int months = 12);
    
    #endregion
}

#region DTOs

/// <summary>
/// Detailed compliance gap information
/// </summary>
public class ComplianceGapDetailDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string GapCode { get; set; } = string.Empty;
    
    // Source
    public Guid? AssessmentId { get; set; }
    public string AssessmentName { get; set; } = string.Empty;
    public Guid? ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlTitle { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    
    // Gap Details
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GapType { get; set; } = "NotImplemented"; // NotImplemented, PartiallyImplemented, Ineffective, Outdated
    public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string RegulatoryImpact { get; set; } = string.Empty;
    
    // Scoring
    public int CurrentScore { get; set; }
    public int TargetScore { get; set; }
    public int GapScore => TargetScore - CurrentScore;
    
    // Status
    public string Status { get; set; } = "Open"; // Open, InRemediation, PendingValidation, Validated, Closed, Accepted
    public string StatusAr { get; set; } = "مفتوح";
    
    // Ownership
    public string Owner { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    
    // Dates
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public DateTime? ActualClosureDate { get; set; }
    public int DaysOpen => (int)(DateTime.UtcNow - IdentifiedDate).TotalDays;
    public bool IsOverdue => TargetClosureDate.HasValue && TargetClosureDate.Value < DateTime.UtcNow && Status != "Closed";
    
    // Remediation
    public bool HasRemediationPlan { get; set; }
    public int RemediationProgress { get; set; }
    public string RemediationPlanSummary { get; set; } = string.Empty;
    
    // Evidence
    public List<GapEvidenceDto> Evidence { get; set; } = new();
    
    // History
    public List<GapHistoryEntryDto> History { get; set; } = new();
}

public class GapEvidenceDto
{
    public Guid EvidenceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

public class GapHistoryEntryDto
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a compliance gap
/// </summary>
public class CreateComplianceGapRequest
{
    public Guid? AssessmentId { get; set; }
    public Guid? ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GapType { get; set; } = "NotImplemented";
    public string Severity { get; set; } = "Medium";
    public int CurrentScore { get; set; }
    public int TargetScore { get; set; } = 100;
    public string Owner { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public DateTime? TargetClosureDate { get; set; }
}

/// <summary>
/// Remediation plan for a gap
/// </summary>
public class RemediationPlanDto
{
    public Guid PlanId { get; set; }
    public Guid GapId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty; // Implement, Compensate, Accept, Transfer
    public string Description { get; set; } = string.Empty;
    
    // Timeline
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    
    // Resources
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public List<string> RequiredResources { get; set; } = new();
    
    // Tasks
    public List<RemediationTaskDto> Tasks { get; set; } = new();
    
    // Progress
    public int OverallProgress { get; set; }
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, OnHold, Completed, Cancelled
}

public class RemediationTaskDto
{
    public Guid TaskId { get; set; }
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Skipped
    public int Progress { get; set; }
    public DateTime? CompletedDate { get; set; }
}

/// <summary>
/// Request to create remediation plan
/// </summary>
public class CreateRemediationPlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public string Strategy { get; set; } = "Implement";
    public string Description { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public List<CreateRemediationTaskRequest> Tasks { get; set; } = new();
}

public class CreateRemediationTaskRequest
{
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Request to update remediation progress
/// </summary>
public class UpdateRemediationProgressRequest
{
    public Guid? TaskId { get; set; }
    public int Progress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<Guid>? EvidenceIds { get; set; }
}

/// <summary>
/// Request to validate remediation
/// </summary>
public class ValidateRemediationRequest
{
    public string ValidatedBy { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int NewScore { get; set; }
    public string ValidationNotes { get; set; } = string.Empty;
    public List<Guid>? EvidenceIds { get; set; }
}

/// <summary>
/// Gap summary by framework
/// </summary>
public class GapSummaryDto
{
    public Guid TenantId { get; set; }
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int TotalGaps { get; set; }
    public int OpenGaps { get; set; }
    public int InRemediationGaps { get; set; }
    public int ClosedGaps { get; set; }
    public int OverdueGaps { get; set; }
    public int CriticalGaps { get; set; }
    public int HighGaps { get; set; }
    public int MediumGaps { get; set; }
    public int LowGaps { get; set; }
    public decimal AvgDaysToClose { get; set; }
    public decimal ClosureRate { get; set; } // Percentage
}

/// <summary>
/// Gap aging report
/// </summary>
public class GapAgingReportDto
{
    public Guid TenantId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalOpenGaps { get; set; }
    
    // Aging buckets
    public int Under30Days { get; set; }
    public int From30To60Days { get; set; }
    public int From60To90Days { get; set; }
    public int Over90Days { get; set; }
    
    // By severity
    public Dictionary<string, AgingBySeverityDto> BySeverity { get; set; } = new();
}

public class AgingBySeverityDto
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal AvgAge { get; set; }
    public int OverdueCount { get; set; }
}

/// <summary>
/// Gap trend point
/// </summary>
public class GapTrendPointDto
{
    public DateTime Date { get; set; }
    public int OpenGaps { get; set; }
    public int ClosedGaps { get; set; }
    public int NewGaps { get; set; }
}

#endregion
