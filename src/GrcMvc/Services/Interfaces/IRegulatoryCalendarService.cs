using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Regulatory Calendar Service Interface
/// خدمة التقويم التنظيمي
/// Tracks regulatory deadlines, reporting periods, and compliance events
/// </summary>
public interface IRegulatoryCalendarService
{
    /// <summary>
    /// Get all upcoming deadlines for a tenant
    /// </summary>
    Task<List<RegulatoryDeadline>> GetUpcomingDeadlinesAsync(Guid tenantId, int daysAhead = 90);

    /// <summary>
    /// Get deadlines by regulator
    /// </summary>
    Task<List<RegulatoryDeadline>> GetDeadlinesByRegulatorAsync(Guid tenantId, string regulatorCode);

    /// <summary>
    /// Get overdue items
    /// </summary>
    Task<List<RegulatoryDeadline>> GetOverdueItemsAsync(Guid tenantId);

    /// <summary>
    /// Get calendar events for a date range
    /// </summary>
    Task<List<CalendarEvent>> GetCalendarEventsAsync(Guid tenantId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Create a regulatory deadline
    /// </summary>
    Task<RegulatoryDeadline> CreateDeadlineAsync(CreateDeadlineRequest request);

    /// <summary>
    /// Mark deadline as completed
    /// </summary>
    Task<RegulatoryDeadline> CompleteDeadlineAsync(Guid deadlineId, string completedBy, string? notes = null);

    /// <summary>
    /// Get deadline statistics
    /// </summary>
    Task<DeadlineStatistics> GetStatisticsAsync(Guid tenantId);

    /// <summary>
    /// Set up automatic reminders
    /// </summary>
    Task SetReminderAsync(Guid deadlineId, int daysBefore, List<string> recipientUserIds);

    /// <summary>
    /// Get standard regulatory calendar (recurring deadlines)
    /// </summary>
    Task<List<StandardRegulatoryEvent>> GetStandardCalendarAsync(string? regulatorCode = null);
}

/// <summary>
/// Regulatory deadline
/// </summary>
public class RegulatoryDeadline
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Overdue
    public string Category { get; set; } = string.Empty; // Report, Assessment, Audit, Renewal, Filing

    public bool IsRecurring { get; set; }
    public string RecurrencePattern { get; set; } = string.Empty; // Annual, Quarterly, Monthly

    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionNotes { get; set; }

    public List<string> AssignedUserIds { get; set; } = new();
    public List<DeadlineReminder> Reminders { get; set; } = new();

    public int DaysRemaining => (DueDate - DateTime.UtcNow).Days;
    public bool IsOverdue => DateTime.UtcNow > DueDate && Status != "Completed";
}

/// <summary>
/// Deadline reminder configuration
/// </summary>
public class DeadlineReminder
{
    public int DaysBefore { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public List<string> RecipientUserIds { get; set; } = new();
}

/// <summary>
/// Calendar event for display
/// </summary>
public class CalendarEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string EventType { get; set; } = string.Empty; // Deadline, Assessment, Audit, Training
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // For UI display
    public string RegulatorCode { get; set; } = string.Empty;
    public bool AllDay { get; set; } = true;
}

/// <summary>
/// Request to create a deadline
/// </summary>
public class CreateDeadlineRequest
{
    public Guid TenantId { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string RegulatorCode { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string RecurrencePattern { get; set; } = string.Empty;
    public List<string> AssignedUserIds { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Deadline statistics
/// </summary>
public class DeadlineStatistics
{
    public Guid TenantId { get; set; }
    public int TotalDeadlines { get; set; }
    public int PendingDeadlines { get; set; }
    public int CompletedDeadlines { get; set; }
    public int OverdueDeadlines { get; set; }
    public int DueThisWeek { get; set; }
    public int DueThisMonth { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public List<RegulatorDeadlineCount> ByRegulator { get; set; } = new();
    public List<CategoryDeadlineCount> ByCategory { get; set; } = new();
}

/// <summary>
/// Deadline count by regulator
/// </summary>
public class RegulatorDeadlineCount
{
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OverdueCount { get; set; }
}

/// <summary>
/// Deadline count by category
/// </summary>
public class CategoryDeadlineCount
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Standard recurring regulatory event
/// </summary>
public class StandardRegulatoryEvent
{
    public string EventCode { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RecurrencePattern { get; set; } = string.Empty;
    public int DayOfMonth { get; set; } // For monthly/annual
    public int Month { get; set; } // For annual (1-12)
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public List<string> ApplicableSectors { get; set; } = new();
}
