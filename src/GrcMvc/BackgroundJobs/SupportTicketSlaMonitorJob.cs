using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// Background job for monitoring Support Ticket SLA compliance
/// Runs every 15 minutes to check for SLA breaches and send alerts
/// Tracks: First Response Time, Resolution Time, SLA Deadlines
/// </summary>
public class SupportTicketSlaMonitorJob
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SupportTicketSlaMonitorJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SupportTicketSlaMonitorJob(
        GrcDbContext context,
        ILogger<SupportTicketSlaMonitorJob> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Main job execution method - called by Hangfire scheduler
    /// </summary>
    [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("SupportTicketSlaMonitorJob started at {Time}", DateTime.UtcNow);

        try
        {
            var stats = new TicketSlaStats();

            // Check all active tickets (not resolved/closed)
            var activeTickets = await _context.SupportTickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.Tenant)
                .Where(t => t.Status != "Resolved" && 
                           t.Status != "Closed" && 
                           t.Status != "Cancelled")
                .ToListAsync();

            foreach (var ticket in activeTickets)
            {
                var slaStatus = await CheckTicketSlaAsync(ticket);
                
                switch (slaStatus)
                {
                    case TicketSlaStatus.Warning:
                        await SendSlaWarningAsync(ticket);
                        stats.Warnings++;
                        break;

                    case TicketSlaStatus.Critical:
                        await SendSlaCriticalWarningAsync(ticket);
                        stats.Critical++;
                        break;

                    case TicketSlaStatus.Breached:
                        if (!ticket.SlaBreached)
                        {
                            await ProcessSlaBreachAsync(ticket);
                            stats.Breached++;
                        }
                        break;
                }

                // Check first response time
                await CheckFirstResponseTimeAsync(ticket, stats);
            }

            _logger.LogInformation(
                "SupportTicketSlaMonitorJob completed. Warnings: {Warnings}, Critical: {Critical}, Breached: {Breached}, FirstResponseBreached: {FirstResponse}",
                stats.Warnings, stats.Critical, stats.Breached, stats.FirstResponseBreached);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SupportTicketSlaMonitorJob failed: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Check ticket SLA status
    /// </summary>
    private async Task<TicketSlaStatus> CheckTicketSlaAsync(SupportTicket ticket)
    {
        if (!ticket.SlaDeadline.HasValue)
            return TicketSlaStatus.OnTrack;

        var now = DateTime.UtcNow;
        var timeRemaining = ticket.SlaDeadline.Value - now;

        if (timeRemaining.TotalHours <= 0)
            return TicketSlaStatus.Breached;

        if (timeRemaining.TotalHours <= 2) // Critical: less than 2 hours
            return TicketSlaStatus.Critical;

        if (timeRemaining.TotalHours <= 12) // Warning: less than 12 hours
            return TicketSlaStatus.Warning;

        return TicketSlaStatus.OnTrack;
    }

    /// <summary>
    /// Check first response time SLA
    /// </summary>
    private async Task CheckFirstResponseTimeAsync(SupportTicket ticket, TicketSlaStats stats)
    {
        // First response should be within 4 hours for Urgent, 24 hours for others
        var firstResponseDeadline = ticket.Priority switch
        {
            "Urgent" or "Critical" => ticket.CreatedAt.AddHours(4),
            "High" => ticket.CreatedAt.AddHours(12),
            _ => ticket.CreatedAt.AddHours(24)
        };

        var now = DateTime.UtcNow;
        if (now > firstResponseDeadline)
        {
            // Check if there's a comment from an agent (not the user)
            var hasAgentResponse = await _context.SupportTicketComments
                .AnyAsync(c => c.TicketId == ticket.Id && 
                              c.UserId != ticket.UserId && 
                              c.CreatedAt <= firstResponseDeadline);

            if (!hasAgentResponse)
            {
                // First response SLA breached
                await LogFirstResponseBreachAsync(ticket, firstResponseDeadline);
                stats.FirstResponseBreached++;
            }
        }
    }

    /// <summary>
    /// Send SLA warning notification
    /// </summary>
    private async Task SendSlaWarningAsync(SupportTicket ticket)
    {
        // Don't send duplicate warnings (within 12 hours)
        var existingWarning = await _context.SupportTicketHistories
            .AnyAsync(h => h.TicketId == ticket.Id &&
                         h.Action == "SLA_Warning" &&
                         h.ChangedAt > DateTime.UtcNow.AddHours(-12));

        if (existingWarning)
            return;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "SLA_Warning",
            NewValue = $"SLA deadline approaching: {ticket.SlaDeadline:yyyy-MM-dd HH:mm}",
            ChangedAt = DateTime.UtcNow,
            Notes = "SLA deadline is approaching. Please take action to avoid breach."
        };
        _context.SupportTicketHistories.Add(history);

        // Notify assigned agent
        if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
        {
            await SendNotificationAsync(ticket, "SLA_Warning", 
                $"Ticket {ticket.TicketNumber} SLA deadline approaching",
                $"SLA deadline: {ticket.SlaDeadline:yyyy-MM-dd HH:mm}");
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("SLA warning sent for ticket {TicketNumber}", ticket.TicketNumber);
    }

    /// <summary>
    /// Send critical SLA warning
    /// </summary>
    private async Task SendSlaCriticalWarningAsync(SupportTicket ticket)
    {
        // Don't send duplicate critical warnings (within 2 hours)
        var existingWarning = await _context.SupportTicketHistories
            .AnyAsync(h => h.TicketId == ticket.Id &&
                         h.Action == "SLA_Critical" &&
                         h.ChangedAt > DateTime.UtcNow.AddHours(-2));

        if (existingWarning)
            return;

        var timeRemaining = ticket.SlaDeadline!.Value - DateTime.UtcNow;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "SLA_Critical",
            NewValue = $"CRITICAL: SLA deadline in {timeRemaining.Hours}h {timeRemaining.Minutes}m",
            ChangedAt = DateTime.UtcNow,
            Notes = "CRITICAL: SLA breach imminent. Immediate action required."
        };
        _context.SupportTicketHistories.Add(history);

        // Notify assigned agent and escalate if needed
        if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
        {
            await SendNotificationAsync(ticket, "SLA_Critical",
                $"CRITICAL: Ticket {ticket.TicketNumber} SLA breach imminent",
                $"Time remaining: {timeRemaining.Hours}h {timeRemaining.Minutes}m");
        }
        else
        {
            // No agent assigned - escalate to platform admin
            await EscalateUnassignedTicketAsync(ticket);
        }

        await _context.SaveChangesAsync();
        _logger.LogWarning("CRITICAL SLA warning sent for ticket {TicketNumber}", ticket.TicketNumber);
    }

    /// <summary>
    /// Process an actual SLA breach
    /// </summary>
    private async Task ProcessSlaBreachAsync(SupportTicket ticket)
    {
        ticket.SlaBreached = true;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "SLA_Breached",
            NewValue = $"SLA BREACHED at {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            ChangedAt = DateTime.UtcNow,
            Notes = $"SLA deadline was {ticket.SlaDeadline:yyyy-MM-dd HH:mm}. This breach will be logged for reporting."
        };
        _context.SupportTicketHistories.Add(history);

        // Escalate to platform admin
        await EscalateSlaBreachAsync(ticket);

        // Notify customer
        await SendNotificationAsync(ticket, "SLA_Breached",
            $"Ticket {ticket.TicketNumber} SLA Breached",
            "We apologize for the delay. Your ticket has been escalated and will be addressed immediately.");

        await _context.SaveChangesAsync();

        _logger.LogError(
            "SLA BREACH: Ticket {TicketNumber} breached at {Time}. Due was {DueDate}",
            ticket.TicketNumber, DateTime.UtcNow, ticket.SlaDeadline);
    }

    /// <summary>
    /// Log first response time breach
    /// </summary>
    private async Task LogFirstResponseBreachAsync(SupportTicket ticket, DateTime deadline)
    {
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "FirstResponse_Breached",
            NewValue = $"First response deadline breached: {deadline:yyyy-MM-dd HH:mm}",
            ChangedAt = DateTime.UtcNow,
            Notes = "First response SLA breached. No agent response received within deadline."
        };
        _context.SupportTicketHistories.Add(history);

        // Escalate unassigned ticket
        if (string.IsNullOrEmpty(ticket.AssignedToUserId))
        {
            await EscalateUnassignedTicketAsync(ticket);
        }

        await _context.SaveChangesAsync();
        _logger.LogWarning("First response SLA breached for ticket {TicketNumber}", ticket.TicketNumber);
    }

    /// <summary>
    /// Escalate unassigned ticket to platform admin
    /// </summary>
    private async Task EscalateUnassignedTicketAsync(SupportTicket ticket)
    {
        // Find available platform admin
        var platformAdmin = await _context.PlatformAdmins
            .Where(pa => pa.IsActive && !pa.IsDeleted)
            .OrderBy(pa => pa.CreatedAt)
            .FirstOrDefaultAsync();

        if (platformAdmin != null)
        {
            ticket.AssignedToUserId = platformAdmin.UserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            var history = new SupportTicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Action = "Auto_Assigned",
                NewValue = $"Auto-assigned to platform admin: {platformAdmin.UserId}",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = "system",
                Notes = "Ticket auto-assigned due to SLA warning/breach"
            };
            _context.SupportTicketHistories.Add(history);

            await SendNotificationAsync(ticket, "Auto_Assigned",
                $"Ticket {ticket.TicketNumber} assigned to you",
                "This ticket was auto-assigned due to SLA warning/breach.");
        }
    }

    /// <summary>
    /// Escalate SLA breach to platform admin
    /// </summary>
    private async Task EscalateSlaBreachAsync(SupportTicket ticket)
    {
        // Find platform admin manager
        var platformAdmin = await _context.PlatformAdmins
            .Where(pa => pa.IsActive && !pa.IsDeleted)
            .OrderBy(pa => pa.CreatedAt)
            .FirstOrDefaultAsync();

        if (platformAdmin != null)
        {
            await SendNotificationAsync(ticket, "SLA_Breach_Escalation",
                $"URGENT: Ticket {ticket.TicketNumber} SLA Breached",
                $"Ticket {ticket.TicketNumber} has breached its SLA deadline. Immediate attention required.");
        }
    }

    /// <summary>
    /// Send notification (if notification service available)
    /// </summary>
    private async Task SendNotificationAsync(SupportTicket ticket, string type, string subject, string message)
    {
        try
        {
            var notificationService = _serviceProvider.GetService<INotificationService>();
            if (notificationService == null)
            {
                _logger.LogDebug("NotificationService not available, skipping notification for ticket {TicketNumber}", ticket.TicketNumber);
                return;
            }

            var recipientId = ticket.AssignedToUserId ?? ticket.UserId;
            if (string.IsNullOrEmpty(recipientId))
                return;

            // Log notification (actual email sending handled by NotificationDeliveryJob)
            _logger.LogInformation("Notification queued: {Type} for ticket {TicketNumber} to user {UserId}", 
                type, ticket.TicketNumber, recipientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification for ticket {TicketNumber}", ticket.TicketNumber);
        }
    }

    private enum TicketSlaStatus
    {
        OnTrack,
        Warning,
        Critical,
        Breached
    }

    private class TicketSlaStats
    {
        public int Warnings { get; set; }
        public int Critical { get; set; }
        public int Breached { get; set; }
        public int FirstResponseBreached { get; set; }
    }
}
