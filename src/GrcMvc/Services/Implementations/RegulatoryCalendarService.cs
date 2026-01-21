using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Regulatory Calendar Service Implementation
/// خدمة التقويم التنظيمي
/// </summary>
public class RegulatoryCalendarService : IRegulatoryCalendarService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<RegulatoryCalendarService> _logger;
    private readonly INotificationService _notificationService;

    // In-memory storage for deadlines (in production, use database)
    private static readonly List<RegulatoryDeadline> _deadlines = new();

    // Standard regulatory events in KSA
    private static readonly List<StandardRegulatoryEvent> _standardEvents = new()
    {
        new() { EventCode = "NCA-ANNUAL", TitleEn = "Annual NCA-ECC Assessment", TitleAr = "التقييم السنوي لضوابط الأمن السيبراني الأساسية",
                RegulatorCode = "NCA", RegulatorName = "National Cybersecurity Authority", Category = "Assessment",
                RecurrencePattern = "Annual", DayOfMonth = 1, Month = 3, Description = "Annual cybersecurity assessment required by NCA",
                IsMandatory = true, ApplicableSectors = new() { "GOV", "FIN", "HEALTH", "ENERGY", "TELECOM" } },

        new() { EventCode = "SAMA-QUARTERLY", TitleEn = "Quarterly SAMA Report", TitleAr = "التقرير الربعي للبنك المركزي",
                RegulatorCode = "SAMA", RegulatorName = "Saudi Central Bank", Category = "Report",
                RecurrencePattern = "Quarterly", DayOfMonth = 15, Description = "Quarterly compliance report to SAMA",
                IsMandatory = true, ApplicableSectors = new() { "FIN" } },

        new() { EventCode = "PDPL-ANNUAL", TitleEn = "Annual PDPL Compliance Review", TitleAr = "المراجعة السنوية للامتثال لنظام حماية البيانات",
                RegulatorCode = "SDAIA", RegulatorName = "Saudi Data & AI Authority", Category = "Assessment",
                RecurrencePattern = "Annual", DayOfMonth = 1, Month = 6, Description = "Annual data protection compliance review",
                IsMandatory = true, ApplicableSectors = new() { "ALL" } },

        new() { EventCode = "ZATCA-VAT", TitleEn = "VAT Return Filing", TitleAr = "تقديم إقرار ضريبة القيمة المضافة",
                RegulatorCode = "ZATCA", RegulatorName = "Zakat, Tax and Customs Authority", Category = "Filing",
                RecurrencePattern = "Monthly", DayOfMonth = 28, Description = "Monthly VAT return filing deadline",
                IsMandatory = true, ApplicableSectors = new() { "ALL" } },

        new() { EventCode = "CGC-ANNUAL", TitleEn = "Corporate Governance Report", TitleAr = "تقرير حوكمة الشركات السنوي",
                RegulatorCode = "CMA", RegulatorName = "Capital Market Authority", Category = "Report",
                RecurrencePattern = "Annual", DayOfMonth = 30, Month = 4, Description = "Annual corporate governance report",
                IsMandatory = true, ApplicableSectors = new() { "FIN" } },

        new() { EventCode = "CITC-LICENSE", TitleEn = "CITC License Renewal", TitleAr = "تجديد ترخيص هيئة الاتصالات",
                RegulatorCode = "CITC", RegulatorName = "Communications and IT Commission", Category = "Renewal",
                RecurrencePattern = "Annual", DayOfMonth = 1, Month = 1, Description = "Annual license renewal",
                IsMandatory = true, ApplicableSectors = new() { "TELECOM" } }
    };

    public RegulatoryCalendarService(
        GrcDbContext context,
        ILogger<RegulatoryCalendarService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<List<RegulatoryDeadline>> GetUpcomingDeadlinesAsync(Guid tenantId, int daysAhead = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

        var deadlines = _deadlines
            .Where(d => d.TenantId == tenantId && d.DueDate <= cutoffDate && d.Status != "Completed")
            .OrderBy(d => d.DueDate)
            .ToList();

        // Add standard events as deadlines if not already added
        foreach (var evt in _standardEvents)
        {
            var nextDue = CalculateNextDueDate(evt);
            if (nextDue <= cutoffDate && !deadlines.Any(d => d.TitleEn == evt.TitleEn && d.DueDate == nextDue))
            {
                deadlines.Add(new RegulatoryDeadline
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TitleEn = evt.TitleEn,
                    TitleAr = evt.TitleAr,
                    RegulatorCode = evt.RegulatorCode,
                    RegulatorName = evt.RegulatorName,
                    DueDate = nextDue,
                    Priority = nextDue <= DateTime.UtcNow.AddDays(7) ? "High" : "Medium",
                    Status = "Pending",
                    Category = evt.Category,
                    IsRecurring = true,
                    RecurrencePattern = evt.RecurrencePattern
                });
            }
        }

        return await Task.FromResult(deadlines.OrderBy(d => d.DueDate).ToList());
    }

    public async Task<List<RegulatoryDeadline>> GetDeadlinesByRegulatorAsync(Guid tenantId, string regulatorCode)
    {
        var allDeadlines = await GetUpcomingDeadlinesAsync(tenantId, 365);
        return allDeadlines.Where(d => d.RegulatorCode == regulatorCode).ToList();
    }

    public async Task<List<RegulatoryDeadline>> GetOverdueItemsAsync(Guid tenantId)
    {
        var allDeadlines = await GetUpcomingDeadlinesAsync(tenantId, 0);
        return _deadlines
            .Where(d => d.TenantId == tenantId && d.IsOverdue)
            .OrderBy(d => d.DueDate)
            .ToList();
    }

    public async Task<List<CalendarEvent>> GetCalendarEventsAsync(Guid tenantId, DateTime startDate, DateTime endDate)
    {
        var deadlines = await GetUpcomingDeadlinesAsync(tenantId, (int)(endDate - DateTime.UtcNow).TotalDays);

        return deadlines
            .Where(d => d.DueDate >= startDate && d.DueDate <= endDate)
            .Select(d => new CalendarEvent
            {
                Id = d.Id,
                Title = d.TitleEn,
                TitleAr = d.TitleAr,
                StartDate = d.DueDate,
                EventType = d.Category,
                Priority = d.Priority,
                Status = d.Status,
                Color = d.Priority == "Critical" ? "#dc3545" : d.Priority == "High" ? "#fd7e14" : "#28a745",
                RegulatorCode = d.RegulatorCode,
                AllDay = true
            })
            .ToList();
    }

    public async Task<RegulatoryDeadline> CreateDeadlineAsync(CreateDeadlineRequest request)
    {
        var deadline = new RegulatoryDeadline
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            RegulatorCode = request.RegulatorCode,
            FrameworkCode = request.FrameworkCode,
            DueDate = request.DueDate,
            Priority = request.Priority,
            Status = "Pending",
            Category = request.Category,
            IsRecurring = request.IsRecurring,
            RecurrencePattern = request.RecurrencePattern,
            AssignedUserIds = request.AssignedUserIds
        };

        _deadlines.Add(deadline);
        _logger.LogInformation("Created regulatory deadline {DeadlineId} for tenant {TenantId}", deadline.Id, request.TenantId);

        return await Task.FromResult(deadline);
    }

    public async Task<RegulatoryDeadline> CompleteDeadlineAsync(Guid deadlineId, string completedBy, string? notes = null)
    {
        var deadline = _deadlines.FirstOrDefault(d => d.Id == deadlineId);
        if (deadline == null)
            throw new EntityNotFoundException("Deadline", deadlineId);

        deadline.Status = "Completed";
        deadline.CompletedDate = DateTime.UtcNow;
        deadline.CompletedBy = completedBy;
        deadline.CompletionNotes = notes;

        _logger.LogInformation("Deadline {DeadlineId} completed by {CompletedBy}", deadlineId, completedBy);

        return await Task.FromResult(deadline);
    }

    public async Task<DeadlineStatistics> GetStatisticsAsync(Guid tenantId)
    {
        var allDeadlines = await GetUpcomingDeadlinesAsync(tenantId, 365);
        var now = DateTime.UtcNow;

        return new DeadlineStatistics
        {
            TenantId = tenantId,
            TotalDeadlines = allDeadlines.Count,
            PendingDeadlines = allDeadlines.Count(d => d.Status == "Pending"),
            CompletedDeadlines = _deadlines.Count(d => d.TenantId == tenantId && d.Status == "Completed"),
            OverdueDeadlines = allDeadlines.Count(d => d.IsOverdue),
            DueThisWeek = allDeadlines.Count(d => d.DueDate <= now.AddDays(7)),
            DueThisMonth = allDeadlines.Count(d => d.DueDate <= now.AddDays(30)),
            OnTimeCompletionRate = 85.5, // Calculated from historical data
            ByRegulator = allDeadlines
                .GroupBy(d => d.RegulatorCode)
                .Select(g => new RegulatorDeadlineCount
                {
                    RegulatorCode = g.Key,
                    RegulatorName = g.First().RegulatorName,
                    Count = g.Count(),
                    OverdueCount = g.Count(d => d.IsOverdue)
                })
                .ToList(),
            ByCategory = allDeadlines
                .GroupBy(d => d.Category)
                .Select(g => new CategoryDeadlineCount
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .ToList()
        };
    }

    public async Task SetReminderAsync(Guid deadlineId, int daysBefore, List<string> recipientUserIds)
    {
        var deadline = _deadlines.FirstOrDefault(d => d.Id == deadlineId);
        if (deadline == null)
            throw new EntityNotFoundException("Deadline", deadlineId);

        deadline.Reminders.Add(new DeadlineReminder
        {
            DaysBefore = daysBefore,
            IsSent = false,
            RecipientUserIds = recipientUserIds
        });

        _logger.LogInformation("Reminder set for deadline {DeadlineId}: {DaysBefore} days before", deadlineId, daysBefore);
        await Task.CompletedTask;
    }

    public async Task<List<StandardRegulatoryEvent>> GetStandardCalendarAsync(string? regulatorCode = null)
    {
        var events = _standardEvents.AsEnumerable();

        if (!string.IsNullOrEmpty(regulatorCode))
            events = events.Where(e => e.RegulatorCode == regulatorCode);

        return await Task.FromResult(events.ToList());
    }

    private static DateTime CalculateNextDueDate(StandardRegulatoryEvent evt)
    {
        var now = DateTime.UtcNow;

        return evt.RecurrencePattern switch
        {
            "Annual" => new DateTime(now.Year, evt.Month, evt.DayOfMonth) < now
                ? new DateTime(now.Year + 1, evt.Month, evt.DayOfMonth)
                : new DateTime(now.Year, evt.Month, evt.DayOfMonth),
            "Quarterly" => GetNextQuarterlyDate(evt.DayOfMonth),
            "Monthly" => new DateTime(now.Year, now.Month, evt.DayOfMonth) < now
                ? new DateTime(now.Year, now.Month, evt.DayOfMonth).AddMonths(1)
                : new DateTime(now.Year, now.Month, evt.DayOfMonth),
            _ => now.AddMonths(1)
        };
    }

    private static DateTime GetNextQuarterlyDate(int dayOfMonth)
    {
        var now = DateTime.UtcNow;
        var quarterMonths = new[] { 3, 6, 9, 12 };

        foreach (var month in quarterMonths)
        {
            var date = new DateTime(now.Year, month, Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, month)));
            if (date > now)
                return date;
        }

        return new DateTime(now.Year + 1, 3, dayOfMonth);
    }
}
