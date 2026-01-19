using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// AM-11: Periodic Access Reviews Service
/// Handles creation, execution, and reporting of access reviews.
/// Control Objective: Detect and remediate inappropriate access.
/// </summary>
public interface IAccessReviewService
{
    #region Review Campaign Management

    /// <summary>
    /// Create a new access review campaign
    /// </summary>
    Task<AccessReview> CreateReviewAsync(CreateAccessReviewDto dto, string createdBy);

    /// <summary>
    /// Get access review by ID
    /// </summary>
    Task<AccessReview?> GetReviewByIdAsync(Guid reviewId);

    /// <summary>
    /// Get access review with all items
    /// </summary>
    Task<AccessReview?> GetReviewWithItemsAsync(Guid reviewId);

    /// <summary>
    /// Get all reviews for a tenant
    /// </summary>
    Task<List<AccessReview>> GetReviewsByTenantAsync(Guid tenantId, AccessReviewFilterDto? filter = null);

    /// <summary>
    /// Update access review details
    /// </summary>
    Task<AccessReview?> UpdateReviewAsync(Guid reviewId, UpdateAccessReviewDto dto, string updatedBy);

    /// <summary>
    /// Cancel an access review
    /// </summary>
    Task<bool> CancelReviewAsync(Guid reviewId, string cancelledBy, string reason);

    /// <summary>
    /// Delete an access review (only if Draft)
    /// </summary>
    Task<bool> DeleteReviewAsync(Guid reviewId, string deletedBy);

    #endregion

    #region Review Execution

    /// <summary>
    /// Start an access review (generates items based on scope)
    /// </summary>
    Task<AccessReview?> StartReviewAsync(Guid reviewId, string startedBy);

    /// <summary>
    /// Generate review items based on review scope
    /// </summary>
    Task<int> GenerateReviewItemsAsync(Guid reviewId);

    /// <summary>
    /// Get review items with pagination and filtering
    /// </summary>
    Task<PagedResult<AccessReviewItem>> GetReviewItemsAsync(
        Guid reviewId,
        int page = 1,
        int pageSize = 50,
        string? statusFilter = null);

    /// <summary>
    /// Make a decision on a review item
    /// </summary>
    Task<AccessReviewItem?> MakeDecisionAsync(
        Guid itemId,
        string decision,
        string decidedBy,
        string? justification = null);

    /// <summary>
    /// Bulk decision on multiple items
    /// </summary>
    Task<int> BulkDecisionAsync(
        IEnumerable<Guid> itemIds,
        string decision,
        string decidedBy,
        string? justification = null);

    /// <summary>
    /// Flag a review item for attention
    /// </summary>
    Task<AccessReviewItem?> FlagItemAsync(Guid itemId, string flaggedBy, string reason);

    /// <summary>
    /// Complete the review (all items must be decided)
    /// </summary>
    Task<AccessReview?> CompleteReviewAsync(Guid reviewId, string completedBy, string? notes = null);

    /// <summary>
    /// Approve a completed review (final sign-off)
    /// </summary>
    Task<AccessReview?> ApproveReviewAsync(Guid reviewId, string approvedBy);

    #endregion

    #region Remediation

    /// <summary>
    /// Execute remediation for revoked items (remove access)
    /// </summary>
    Task<int> ExecuteRemediationAsync(Guid reviewId, string executedBy);

    /// <summary>
    /// Mark remediation as complete for an item
    /// </summary>
    Task<AccessReviewItem?> CompleteRemediationAsync(Guid itemId, string completedBy, string notes);

    /// <summary>
    /// Get items pending remediation
    /// </summary>
    Task<List<AccessReviewItem>> GetPendingRemediationItemsAsync(Guid tenantId);

    #endregion

    #region Scheduling and Automation

    /// <summary>
    /// Schedule a review for a future date
    /// </summary>
    Task<AccessReview?> ScheduleReviewAsync(Guid reviewId, DateTime startDate, string scheduledBy);

    /// <summary>
    /// Create recurring review based on pattern
    /// </summary>
    Task<AccessReview?> CreateNextRecurringReviewAsync(Guid completedReviewId);

    /// <summary>
    /// Get overdue reviews
    /// </summary>
    Task<List<AccessReview>> GetOverdueReviewsAsync(Guid? tenantId = null);

    /// <summary>
    /// Send reminder notifications for upcoming reviews
    /// </summary>
    Task<int> SendReviewRemindersAsync();

    /// <summary>
    /// Check and update overdue review statuses
    /// </summary>
    Task<int> ProcessOverdueReviewsAsync();

    #endregion

    #region Reporting and Analytics

    /// <summary>
    /// Get review statistics for a tenant
    /// </summary>
    Task<AccessReviewStatistics> GetReviewStatisticsAsync(Guid tenantId, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Get compliance summary for access reviews
    /// </summary>
    Task<AccessReviewComplianceSummary> GetComplianceSummaryAsync(Guid tenantId);

    /// <summary>
    /// Export review results
    /// </summary>
    Task<byte[]> ExportReviewResultsAsync(Guid reviewId, string format = "csv");

    /// <summary>
    /// Generate review report
    /// </summary>
    Task<AccessReviewReport> GenerateReportAsync(Guid reviewId);

    #endregion
}

#region DTOs

/// <summary>
/// DTO for creating an access review
/// </summary>
public class CreateAccessReviewDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReviewType { get; set; } = "Quarterly";
    public string Scope { get; set; } = "AllUsers";
    public string? ScopeFilterJson { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? ReviewerId { get; set; }
    public string? ComplianceFramework { get; set; }
    public string RecurrencePattern { get; set; } = "None";
    public int ReminderDays { get; set; } = 7;
}

/// <summary>
/// DTO for updating an access review
/// </summary>
public class UpdateAccessReviewDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ReviewerId { get; set; }
    public string? ReviewerNotes { get; set; }
    public int? ReminderDays { get; set; }
}

/// <summary>
/// Filter for access reviews
/// </summary>
public class AccessReviewFilterDto
{
    public string? Status { get; set; }
    public string? ReviewType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool? IncludeCompleted { get; set; }
}

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Access review statistics
/// </summary>
public class AccessReviewStatistics
{
    public int TotalReviews { get; set; }
    public int CompletedReviews { get; set; }
    public int OverdueReviews { get; set; }
    public int InProgressReviews { get; set; }
    public int TotalItemsReviewed { get; set; }
    public int TotalAccessRevoked { get; set; }
    public double AverageCompletionDays { get; set; }
    public double ComplianceRate { get; set; }
    public Dictionary<string, int> ReviewsByType { get; set; } = new();
    public Dictionary<string, int> DecisionsByType { get; set; } = new();
}

/// <summary>
/// Access review compliance summary
/// </summary>
public class AccessReviewComplianceSummary
{
    public bool IsCompliant { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDue { get; set; }
    public int DaysUntilNextReview { get; set; }
    public int TotalPrivilegedUsers { get; set; }
    public int PrivilegedUsersReviewed { get; set; }
    public int PendingRemediations { get; set; }
    public List<string> ComplianceGaps { get; set; } = new();
    public string ControlReference { get; set; } = "AM-11";
}

/// <summary>
/// Access review report
/// </summary>
public class AccessReviewReport
{
    public Guid ReviewId { get; set; }
    public string ReviewCode { get; set; } = string.Empty;
    public string ReviewName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;

    // Summary
    public int TotalItems { get; set; }
    public int ApprovedItems { get; set; }
    public int RevokedItems { get; set; }
    public int FlaggedItemsCount { get; set; }
    public int SkippedItems { get; set; }
    public double CompletionRate { get; set; }

    // Details
    public List<AccessReviewReportItem> HighRiskItems { get; set; } = new();
    public List<AccessReviewReportItem> RevokedAccessItems { get; set; } = new();
    public List<AccessReviewReportItem> FlaggedItems { get; set; } = new();

    // Audit trail
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReviewerName { get; set; }
    public string? ApproverName { get; set; }
}

/// <summary>
/// Report item detail
/// </summary>
public class AccessReviewReportItem
{
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string AccessItem { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string? Justification { get; set; }
    public DateTime? DecisionDate { get; set; }
}

#endregion
