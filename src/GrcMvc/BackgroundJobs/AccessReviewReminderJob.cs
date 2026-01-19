using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// AM-11: Access Review Reminder Job
/// Hangfire job that sends reminders for overdue access reviews.
/// Control Reference: AM-04 (Access Reviews), AM-11 (Periodic Access Reviews)
/// BP-01 Critical Job: AccessReviewReminder
/// </summary>
public class AccessReviewReminderJob
{
    private readonly GrcDbContext _context;
    private readonly IAuditEventService _auditEventService;
    private readonly ILogger<AccessReviewReminderJob> _logger;

    /// <summary>
    /// Reminder interval in hours (default: 24 hours between reminders)
    /// </summary>
    private const int ReminderIntervalHours = 24;

    public AccessReviewReminderJob(
        GrcDbContext context,
        IAuditEventService auditEventService,
        ILogger<AccessReviewReminderJob> logger)
    {
        _context = context;
        _auditEventService = auditEventService;
        _logger = logger;
    }

    /// <summary>
    /// Main execution method called by Hangfire
    /// Finds overdue reviews and sends reminders
    /// </summary>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("AccessReviewReminderJob started at {Time}", DateTime.UtcNow);

        try
        {
            var now = DateTime.UtcNow;
            var reminderThreshold = now.AddHours(-ReminderIntervalHours);

            // Find access reviews that need reminders:
            // 1. Status is InProgress (active reviews)
            // 2. DueDate has passed (overdue)
            // 3. LastReminderSentAt is null OR older than interval
            var overdueReviews = await _context.AccessReviews
                .Where(r => r.Status == AccessReviewStatus.InProgress)
                .Where(r => r.DueDate < now)
                .Where(r => r.LastReminderSentAt == null || r.LastReminderSentAt < reminderThreshold)
                .Include(r => r.Tenant)
                .ToListAsync();

            _logger.LogInformation("Found {Count} overdue access reviews requiring reminders", overdueReviews.Count);

            foreach (var review in overdueReviews)
            {
                await ProcessReviewReminderAsync(review, now);
            }

            _logger.LogInformation("AccessReviewReminderJob completed. Processed {Count} reviews", overdueReviews.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessReviewReminderJob failed with error: {Message}", ex.Message);
            throw; // Re-throw to let Hangfire handle retry
        }
    }

    /// <summary>
    /// Process a single review reminder
    /// </summary>
    private async Task ProcessReviewReminderAsync(AccessReview review, DateTime now)
    {
        try
        {
            var daysOverdue = (now - review.DueDate).Days;

            _logger.LogInformation(
                "Sending reminder for review {ReviewCode} (TenantId: {TenantId}, {DaysOverdue} days overdue)",
                review.ReviewCode,
                review.TenantId,
                daysOverdue);

            // Update the review's reminder tracking
            review.LastReminderSentAt = now;
            review.ReminderSent = true;

            // Mark as overdue if not already
            if (review.Status != AccessReviewStatus.Overdue && daysOverdue > 0)
            {
                review.Status = AccessReviewStatus.Overdue;
            }

            var reminderType = daysOverdue switch
            {
                <= 3 => "Initial",
                <= 7 => "Warning",
                <= 14 => "Escalation",
                _ => "Critical"
            };

            // Emit audit event with correct signature
            var payload = JsonSerializer.Serialize(new
            {
                ReviewId = review.Id,
                ReviewCode = review.ReviewCode,
                DueDate = review.DueDate,
                DaysOverdue = daysOverdue,
                ProgressPercent = review.ProgressPercent,
                TotalItems = review.TotalItems,
                ReviewedItems = review.ReviewedItems,
                ReviewerId = review.ReviewerId,
                ReminderType = reminderType
            });

            await _auditEventService.LogEventAsync(
                tenantId: review.TenantId,
                eventType: AccessManagementAuditEvents.AM11_ReminderSent,
                affectedEntityType: "AccessReview",
                affectedEntityId: review.Id.ToString(),
                action: $"Reminder sent ({reminderType})",
                actor: "system",
                payloadJson: payload);

            // Also emit overdue event for compliance tracking
            if (daysOverdue > 0)
            {
                var overduePayload = JsonSerializer.Serialize(new
                {
                    ReviewId = review.Id,
                    ReviewCode = review.ReviewCode,
                    DaysOverdue = daysOverdue,
                    ControlReference = review.ControlReference
                });

                await _auditEventService.LogEventAsync(
                    tenantId: review.TenantId,
                    eventType: AccessManagementAuditEvents.AM11_ReviewOverdue,
                    affectedEntityType: "AccessReview",
                    affectedEntityId: review.Id.ToString(),
                    action: $"Review overdue by {daysOverdue} days",
                    actor: "system",
                    payloadJson: overduePayload);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Reminder sent successfully for review {ReviewCode}",
                review.ReviewCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send reminder for review {ReviewCode}: {Message}",
                review.ReviewCode,
                ex.Message);
            // Don't rethrow - continue processing other reviews
        }
    }

    /// <summary>
    /// Check for reviews approaching their due date (proactive reminders)
    /// </summary>
    public async Task SendApproachingDueDateRemindersAsync()
    {
        var now = DateTime.UtcNow;

        // Find reviews approaching due date (within ReminderDays)
        var approachingReviews = await _context.AccessReviews
            .Where(r => r.Status == AccessReviewStatus.InProgress)
            .Where(r => r.DueDate > now)
            .Where(r => r.DueDate <= now.AddDays(7)) // Default 7 days warning
            .Where(r => !r.ReminderSent)
            .Include(r => r.Tenant)
            .ToListAsync();

        _logger.LogInformation("Found {Count} access reviews approaching due date", approachingReviews.Count);

        foreach (var review in approachingReviews)
        {
            var daysUntilDue = (review.DueDate - now).Days;

            // Only send if within the review's configured reminder days
            if (daysUntilDue <= review.ReminderDays)
            {
                review.ReminderSent = true;
                review.LastReminderSentAt = now;

                var payload = JsonSerializer.Serialize(new
                {
                    ReviewId = review.Id,
                    ReviewCode = review.ReviewCode,
                    DueDate = review.DueDate,
                    DaysUntilDue = daysUntilDue,
                    ReminderType = "ApproachingDeadline"
                });

                await _auditEventService.LogEventAsync(
                    tenantId: review.TenantId,
                    eventType: AccessManagementAuditEvents.AM11_ReminderSent,
                    affectedEntityType: "AccessReview",
                    affectedEntityId: review.Id.ToString(),
                    action: $"Approaching deadline reminder ({daysUntilDue} days)",
                    actor: "system",
                    payloadJson: payload);
            }
        }

        await _context.SaveChangesAsync();
    }
}
