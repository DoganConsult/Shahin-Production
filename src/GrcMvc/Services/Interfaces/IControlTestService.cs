using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Control Testing Service Interface
/// Handles the complete control testing lifecycle:
/// Schedule → Test → Score → Review → Report → Next Cycle
/// </summary>
public interface IControlTestService
{
    #region Test Execution
    
    /// <summary>
    /// Execute a control test
    /// </summary>
    Task<ControlTestResultDetailDto> ExecuteTestAsync(Guid controlId, ExecuteControlTestRequest request);
    
    /// <summary>
    /// Get test by ID
    /// </summary>
    Task<ControlTestResultDetailDto?> GetTestByIdAsync(Guid testId);
    
    /// <summary>
    /// Get all tests for a control
    /// </summary>
    Task<List<ControlTestResultDetailDto>> GetTestsForControlAsync(Guid controlId);
    
    /// <summary>
    /// Get test history for a control
    /// </summary>
    Task<List<ControlTestHistoryDto>> GetTestHistoryAsync(Guid controlId, int limit = 12);
    
    #endregion

    #region Test Review
    
    /// <summary>
    /// Submit test for review
    /// </summary>
    Task<ControlTestResultDetailDto> SubmitForReviewAsync(Guid testId, string submittedBy);
    
    /// <summary>
    /// Approve a test
    /// </summary>
    Task<ControlTestResultDetailDto> ApproveTestAsync(Guid testId, string reviewerId, string reviewerName, string? notes = null);
    
    /// <summary>
    /// Reject a test
    /// </summary>
    Task<ControlTestResultDetailDto> RejectTestAsync(Guid testId, string reviewerId, string reviewerName, string reason);
    
    /// <summary>
    /// Get tests pending review
    /// </summary>
    Task<List<ControlTestResultDetailDto>> GetPendingReviewsAsync(Guid tenantId);
    
    #endregion

    #region Scheduling
    
    /// <summary>
    /// Schedule a control test
    /// </summary>
    Task<ControlTestScheduleDto> ScheduleTestAsync(Guid controlId, ScheduleTestRequest request);
    
    /// <summary>
    /// Get upcoming tests
    /// </summary>
    Task<List<ControlTestScheduleDto>> GetUpcomingTestsAsync(Guid tenantId, int days = 30);
    
    /// <summary>
    /// Get overdue tests
    /// </summary>
    Task<List<ControlTestScheduleDto>> GetOverdueTestsAsync(Guid tenantId);
    
    /// <summary>
    /// Update test schedule
    /// </summary>
    Task<ControlTestScheduleDto> UpdateScheduleAsync(Guid controlId, DateTime newTestDate, string? reason = null);
    
    #endregion

    #region Effectiveness Scoring
    
    /// <summary>
    /// Calculate control effectiveness based on test history
    /// </summary>
    Task<ControlTestEffectivenessDto> CalculateEffectivenessAsync(Guid controlId);
    
    /// <summary>
    /// Get effectiveness trend for a control
    /// </summary>
    Task<List<EffectivenessTrendPointDto>> GetEffectivenessTrendAsync(Guid controlId, int months = 12);
    
    /// <summary>
    /// Get effectiveness summary for all controls in a tenant
    /// </summary>
    Task<ControlEffectivenessSummaryDto> GetEffectivenessSummaryAsync(Guid tenantId);
    
    #endregion

    #region Owner Management
    
    /// <summary>
    /// Assign owner to a control
    /// </summary>
    Task<ControlOwnerDto> AssignOwnerAsync(Guid controlId, AssignOwnerRequest request);
    
    /// <summary>
    /// Get current owner of a control
    /// </summary>
    Task<ControlOwnerDto?> GetCurrentOwnerAsync(Guid controlId);
    
    /// <summary>
    /// Get ownership history for a control
    /// </summary>
    Task<List<ControlOwnerDto>> GetOwnershipHistoryAsync(Guid controlId);
    
    /// <summary>
    /// Transfer ownership of a control
    /// </summary>
    Task<ControlOwnerDto> TransferOwnershipAsync(Guid controlId, TransferOwnershipRequest request);
    
    /// <summary>
    /// Get controls by owner
    /// </summary>
    Task<List<ControlWithEffectivenessDto>> GetControlsByOwnerAsync(string ownerId);
    
    #endregion

    #region Reporting
    
    /// <summary>
    /// Get control testing dashboard
    /// </summary>
    Task<ControlTestingDashboardDto> GetTestingDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get controls requiring testing (due or overdue)
    /// </summary>
    Task<List<ControlWithEffectivenessDto>> GetControlsRequiringTestingAsync(Guid tenantId);
    
    /// <summary>
    /// Get control testing coverage report
    /// </summary>
    Task<TestingCoverageReportDto> GetTestingCoverageAsync(Guid tenantId);
    
    #endregion
}

#region DTOs

/// <summary>
/// Request to execute a control test
/// </summary>
public class ExecuteControlTestRequest
{
    public string TestType { get; set; } = "Effectiveness";
    public string? TestMethodology { get; set; }
    public int? SampleSize { get; set; }
    public int? PopulationSize { get; set; }
    public int ExceptionsFound { get; set; }
    public int Score { get; set; }
    public string Result { get; set; } = "Pass";
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public string? TestNotes { get; set; }
    public string TesterId { get; set; } = string.Empty;
    public string TesterName { get; set; } = string.Empty;
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public List<Guid>? EvidenceIds { get; set; }
}

/// <summary>
/// Detailed control test result
/// </summary>
public class ControlTestResultDetailDto
{
    public Guid TestId { get; set; }
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string? TestMethodology { get; set; }
    public int? SampleSize { get; set; }
    public int? PopulationSize { get; set; }
    public int ExceptionsFound { get; set; }
    public int Score { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public string? TestNotes { get; set; }
    public string TesterName { get; set; } = string.Empty;
    public DateTime TestedDate { get; set; }
    public string ReviewStatus { get; set; } = string.Empty;
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public int PreviousEffectiveness { get; set; }
    public int NewEffectiveness { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public List<Guid> EvidenceIds { get; set; } = new();
}

/// <summary>
/// Control test history entry
/// </summary>
public record ControlTestHistoryDto(
    Guid TestId,
    DateTime TestedDate,
    string TestType,
    int Score,
    string Result,
    string TesterName,
    string ReviewStatus
);

/// <summary>
/// Control test schedule
/// </summary>
public class ControlTestScheduleDto
{
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime? LastTestDate { get; set; }
    public DateTime? NextTestDate { get; set; }
    public int DaysUntilDue { get; set; }
    public bool IsOverdue { get; set; }
    public int CurrentEffectiveness { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Request to schedule a test
/// </summary>
public class ScheduleTestRequest
{
    public DateTime ScheduledDate { get; set; }
    public string TestType { get; set; } = "Effectiveness";
    public string? AssignedTesterId { get; set; }
    public string? AssignedTesterName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Control effectiveness calculation for testing
/// </summary>
public class ControlTestEffectivenessDto
{
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public int CurrentEffectiveness { get; set; }
    public int DesignEffectiveness { get; set; }
    public int OperatingEffectiveness { get; set; }
    public string EffectivenessLevel { get; set; } = "Unknown"; // Effective, PartiallyEffective, Ineffective, NotTested
    public string EffectivenessLevelAr { get; set; } = "غير معروف";
    public DateTime? LastTestDate { get; set; }
    public int TestCount { get; set; }
    public decimal AverageScore { get; set; }
    public string Trend { get; set; } = "Stable"; // Improving, Stable, Declining
}

/// <summary>
/// Effectiveness trend point
/// </summary>
public record EffectivenessTrendPointDto(
    DateTime Date,
    int Score,
    string TestType,
    string Result
);

/// <summary>
/// Summary of control effectiveness across tenant
/// </summary>
public class ControlEffectivenessSummaryDto
{
    public Guid TenantId { get; set; }
    public int TotalControls { get; set; }
    public int EffectiveControls { get; set; }
    public int PartiallyEffectiveControls { get; set; }
    public int IneffectiveControls { get; set; }
    public int NotTestedControls { get; set; }
    public decimal OverallEffectivenessRate { get; set; }
    public decimal AverageEffectivenessScore { get; set; }
    public int ControlsTestedThisQuarter { get; set; }
    public int ControlsDueForTesting { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request to assign owner
/// </summary>
public class AssignOwnerRequest
{
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerEmail { get; set; }
    public string? Department { get; set; }
    public string AssignmentType { get; set; } = "Primary";
    public string? Reason { get; set; }
    public string AssignedById { get; set; } = string.Empty;
    public string AssignedByName { get; set; } = string.Empty;
}

/// <summary>
/// Request to transfer ownership
/// </summary>
public class TransferOwnershipRequest
{
    public string NewOwnerId { get; set; } = string.Empty;
    public string NewOwnerName { get; set; } = string.Empty;
    public string? NewOwnerEmail { get; set; }
    public string? NewDepartment { get; set; }
    public string TransferReason { get; set; } = string.Empty;
    public string TransferredById { get; set; } = string.Empty;
    public string TransferredByName { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// Control owner information
/// </summary>
public class ControlOwnerDto
{
    public Guid AssignmentId { get; set; }
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerEmail { get; set; }
    public string? Department { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? AssignedByName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Control with effectiveness information
/// </summary>
public class ControlWithEffectivenessDto
{
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EffectivenessScore { get; set; }
    public string EffectivenessLevel { get; set; } = string.Empty;
    public DateTime? LastTestDate { get; set; }
    public DateTime? NextTestDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
}

/// <summary>
/// Control testing dashboard
/// </summary>
public class ControlTestingDashboardDto
{
    public Guid TenantId { get; set; }
    public ControlEffectivenessSummaryDto EffectivenessSummary { get; set; } = new();
    public List<ControlTestScheduleDto> UpcomingTests { get; set; } = new();
    public List<ControlTestScheduleDto> OverdueTests { get; set; } = new();
    public List<ControlTestResultDetailDto> RecentTests { get; set; } = new();
    public List<ControlTestResultDetailDto> PendingReviews { get; set; } = new();
    public TestingCoverageReportDto Coverage { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Testing coverage report
/// </summary>
public class TestingCoverageReportDto
{
    public Guid TenantId { get; set; }
    public int TotalControls { get; set; }
    public int ControlsTested { get; set; }
    public int ControlsNotTested { get; set; }
    public decimal CoveragePercentage { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByFrequency { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

#endregion
