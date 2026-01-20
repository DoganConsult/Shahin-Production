using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Support Ticket Service - Custom implementation (not ABP module)
/// Manages support tickets for platform admin ticketing system
/// Follows the same pattern as other custom services (registered in Program.cs)
/// </summary>
public class SupportTicketService : ISupportTicketService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SupportTicketService> _logger;
    private readonly ITenantContextService _tenantContext;
    private readonly IGrcEmailService? _emailService;
    private readonly UserManager<ApplicationUser>? _userManager;

    public SupportTicketService(
        GrcDbContext context,
        ILogger<SupportTicketService> logger,
        ITenantContextService tenantContext,
        IGrcEmailService? emailService = null,
        UserManager<ApplicationUser>? userManager = null)
    {
        _context = context;
        _logger = logger;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _userManager = userManager;
    }

    #region Ticket CRUD

    public async Task<SupportTicket> CreateTicketAsync(CreateTicketDto dto)
    {
        // Generate ticket number
        var ticketNumber = await GenerateTicketNumberAsync();

        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            TenantId = dto.TenantId,
            UserId = dto.UserId,
            UserEmail = dto.UserEmail,
            Subject = dto.Subject,
            Description = dto.Description,
            Category = dto.Category ?? "Technical",
            Priority = dto.Priority ?? "Medium",
            Status = "New",
            Tags = dto.Tags,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            CreatedAt = DateTime.UtcNow,
            SlaDeadline = CalculateSlaDeadline(dto.Priority ?? "Medium")
        };

        // Auto-assign to tenant admin if tenant ID is provided
        if (dto.TenantId.HasValue)
        {
            var tenantAdmin = await GetTenantAdminAsync(dto.TenantId.Value);
            if (tenantAdmin != null)
            {
                ticket.AssignedToUserId = tenantAdmin.UserId;
                ticket.Status = "Open"; // Auto-open when assigned to tenant admin
            }
        }

        _context.SupportTickets.Add(ticket);

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Created",
            NewValue = $"Status: {ticket.Status}, Priority: {ticket.Priority}, AssignedTo: {ticket.AssignedToUserId ?? "Unassigned"}",
            ChangedAt = DateTime.UtcNow,
            Notes = ticket.AssignedToUserId != null ? "Auto-assigned to tenant admin" : "Created, awaiting assignment"
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created support ticket {TicketNumber} for tenant {TenantId}, assigned to {AssignedTo}", 
            ticketNumber, dto.TenantId, ticket.AssignedToUserId ?? "Unassigned");

        // Send email notification to customer
        await SendTicketCreatedEmailAsync(ticket);

        // Send email notification to tenant admin if assigned
        if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
        {
            await SendTicketAssignedEmailAsync(ticket, ticket.AssignedToUserId);
        }

        return ticket;
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(Guid ticketId)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Tenant)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<SupportTicket?> GetTicketByNumberAsync(string ticketNumber)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Tenant)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
    }

    public async Task<List<SupportTicket>> GetTicketsAsync(TicketFilterDto? filter = null)
    {
        var query = _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Tenant)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.TenantId.HasValue)
                query = query.Where(t => t.TenantId == filter.TenantId);

            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(t => t.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.Priority))
                query = query.Where(t => t.Priority == filter.Priority);

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(t => t.Category == filter.Category);

            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Subject.ToLower().Contains(searchTerm) ||
                    t.Description.ToLower().Contains(searchTerm) ||
                    t.TicketNumber.ToLower().Contains(searchTerm));
            }
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        if (filter != null && filter.PageSize > 0)
        {
            query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
        }

        return await query.ToListAsync();
    }

    public async Task<List<SupportTicket>> GetTicketsByTenantAsync(Guid tenantId)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedToUser)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupportTicket>> GetTicketsByAssigneeAsync(string userId)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Tenant)
            .Where(t => t.AssignedToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    #endregion

    #region Ticket Updates

    public async Task<SupportTicket> UpdateTicketStatusAsync(
        Guid ticketId, 
        string status, 
        string? notes = null, 
        string? changedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        var previousStatus = ticket.Status;
        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (status == "Resolved" && !ticket.ResolvedAt.HasValue)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.ResolutionNotes = notes;
        }

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Status Changed",
            PreviousValue = previousStatus,
            NewValue = status,
            ChangedByUserId = changedByUserId,
            Notes = notes,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated ticket {TicketNumber} status from {OldStatus} to {NewStatus}",
            ticket.TicketNumber, previousStatus, status);

        // Send email notification
        await SendTicketStatusUpdateEmailAsync(ticket, previousStatus, changedByUserId);

        return ticket;
    }

    public async Task<SupportTicket> AssignTicketAsync(
        Guid ticketId, 
        string assignedToUserId, 
        string? changedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        var previousAssignee = ticket.AssignedToUserId;
        ticket.AssignedToUserId = assignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Assigned",
            PreviousValue = previousAssignee ?? "Unassigned",
            NewValue = assignedToUserId,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned ticket {TicketNumber} to user {UserId}",
            ticket.TicketNumber, assignedToUserId);

        // Send email notification to assigned agent
        await SendTicketAssignedEmailAsync(ticket, assignedToUserId);

        return ticket;
    }

    public async Task<SupportTicket> UpdateTicketPriorityAsync(
        Guid ticketId, 
        string priority, 
        string? changedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        var previousPriority = ticket.Priority;
        ticket.Priority = priority;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.SlaDeadline = CalculateSlaDeadline(priority);

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Priority Changed",
            PreviousValue = previousPriority,
            NewValue = priority,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated ticket {TicketNumber} priority from {OldPriority} to {NewPriority}",
            ticket.TicketNumber, previousPriority, priority);

        return ticket;
    }

    public async Task<SupportTicketComment> AddCommentAsync(
        Guid ticketId, 
        string content, 
        string userId, 
        bool isInternal = false)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        // Check if this is the first agent response (for first response time tracking)
        var isFirstAgentResponse = !isInternal && 
                                   userId != ticket.UserId && 
                                   !await _context.SupportTicketComments
                                       .AnyAsync(c => c.TicketId == ticketId && 
                                                     c.UserId != ticket.UserId);

        var comment = new SupportTicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId,
            Content = content,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        };

        _context.SupportTicketComments.Add(comment);

        // Update ticket status if it's still "New" and this is an agent response
        if (!isInternal && userId != ticket.UserId && ticket.Status == "New")
        {
            ticket.Status = "Open";
            var statusHistory = new SupportTicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Action = "Status Changed",
                PreviousValue = "New",
                NewValue = "Open",
                ChangedByUserId = userId,
                Notes = "Status auto-updated on first agent response",
                ChangedAt = DateTime.UtcNow
            };
            _context.SupportTicketHistories.Add(statusHistory);
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        // Add history entry with detailed tracking
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = isInternal ? "Internal Comment Added" : "Comment Added",
            NewValue = $"Comment by {userId}",
            ChangedByUserId = userId,
            Notes = isFirstAgentResponse ? "First agent response - First Response Time recorded" : null,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Added comment to ticket {TicketNumber} by user {UserId}. FirstResponse: {IsFirst}",
            ticket.TicketNumber, userId, isFirstAgentResponse);

        return comment;
    }

    public async Task<SupportTicket> ResolveTicketAsync(
        Guid ticketId, 
        string resolutionNotes, 
        string? resolvedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        ticket.Status = "Resolved";
        ticket.ResolutionNotes = resolutionNotes;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Resolved",
            NewValue = $"Resolved by {resolvedByUserId ?? "System"}",
            ChangedByUserId = resolvedByUserId,
            Notes = resolutionNotes,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Resolved ticket {TicketNumber} by user {UserId}",
            ticket.TicketNumber, resolvedByUserId);

        return ticket;
    }

    public async Task<SupportTicket> CloseTicketAsync(
        Guid ticketId, 
        string? closedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        ticket.Status = "Closed";
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Closed",
            NewValue = $"Closed by {closedByUserId ?? "System"}",
            ChangedByUserId = closedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Closed ticket {TicketNumber} by user {UserId}",
            ticket.TicketNumber, closedByUserId);

        return ticket;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<TicketStatistics> GetStatisticsAsync(TicketStatisticsFilterDto? filter = null)
    {
        var query = _context.SupportTickets.AsQueryable();

        if (filter != null)
        {
            if (filter.TenantId.HasValue)
                query = query.Where(t => t.TenantId == filter.TenantId);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);
        }

        var tickets = await query.ToListAsync();

        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgResolutionTime = resolvedTickets.Any()
            ? resolvedTickets.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var stats = new TicketStatistics
        {
            TotalTickets = tickets.Count,
            NewTickets = tickets.Count(t => t.Status == "New"),
            OpenTickets = tickets.Count(t => t.Status == "Open" || t.Status == "New"),
            InProgressTickets = tickets.Count(t => t.Status == "In Progress"),
            ResolvedTickets = tickets.Count(t => t.Status == "Resolved"),
            ClosedTickets = tickets.Count(t => t.Status == "Closed"),
            UrgentTickets = tickets.Count(t => t.Priority == "Urgent" || t.Priority == "Critical"),
            HighPriorityTickets = tickets.Count(t => t.Priority == "High"),
            SlaBreachedTickets = tickets.Count(t => t.SlaBreached),
            AverageResolutionTimeHours = avgResolutionTime,
            AverageFirstResponseTimeHours = 0, // TODO: Calculate from first comment timestamp
            TicketsByCategory = tickets.GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Count()),
            TicketsByStatus = tickets.GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    public async Task<List<SupportTicket>> GetTicketsRequiringAttentionAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Tenant)
            .Where(t => 
                (t.Status == "New" || t.Status == "Open" || t.Status == "In Progress") &&
                (
                    t.SlaBreached ||
                    (t.SlaDeadline.HasValue && t.SlaDeadline.Value < now) ||
                    (t.Priority == "Urgent" || t.Priority == "Critical") ||
                    (t.Status == "New" && t.CreatedAt < now.AddHours(-24))
                ))
            .OrderByDescending(t => t.Priority == "Urgent" || t.Priority == "Critical")
            .ThenBy(t => t.SlaDeadline)
            .ToListAsync();
    }

    #endregion

    #region Email Notifications

    /// <summary>
    /// Send email notification when ticket is created
    /// </summary>
    private async Task SendTicketCreatedEmailAsync(SupportTicket ticket)
    {
        if (_emailService == null || string.IsNullOrEmpty(ticket.UserEmail))
            return;

        try
        {
            var ticketUrl = $"{GetBaseUrl()}/Support/Status?ticketNumber={ticket.TicketNumber}";
            var model = new
            {
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                SlaDeadline = ticket.SlaDeadline,
                TicketUrl = ticketUrl,
                IsArabic = true
            };

            await _emailService.SendTemplatedEmailAsync(
                ticket.UserEmail,
                "SupportTicketCreated",
                model,
                isArabic: true);

            _logger.LogInformation("Ticket created email sent to {Email} for ticket {TicketNumber}",
                ticket.UserEmail, ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ticket created email for {TicketNumber}", ticket.TicketNumber);
        }
    }

    /// <summary>
    /// Send email notification when ticket status changes
    /// </summary>
    private async Task SendTicketStatusUpdateEmailAsync(SupportTicket ticket, string previousStatus, string? changedByUserId = null)
    {
        if (_emailService == null || string.IsNullOrEmpty(ticket.UserEmail))
            return;

        try
        {
            var changedBy = "System";
            if (!string.IsNullOrEmpty(changedByUserId) && _userManager != null)
            {
                var user = await _userManager.FindByIdAsync(changedByUserId);
                changedBy = user?.Email ?? changedByUserId;
            }

            var ticketUrl = $"{GetBaseUrl()}/Support/Status?ticketNumber={ticket.TicketNumber}";
            var model = new
            {
                TicketNumber = ticket.TicketNumber,
                PreviousStatus = previousStatus,
                NewStatus = ticket.Status,
                ChangedBy = changedBy,
                TicketUrl = ticketUrl,
                IsArabic = true
            };

            await _emailService.SendTemplatedEmailAsync(
                ticket.UserEmail,
                "SupportTicketUpdated",
                model,
                isArabic: true);

            _logger.LogInformation("Ticket status update email sent to {Email} for ticket {TicketNumber}",
                ticket.UserEmail, ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ticket status update email for {TicketNumber}", ticket.TicketNumber);
        }
    }

    /// <summary>
    /// Send email notification when ticket is assigned
    /// </summary>
    private async Task SendTicketAssignedEmailAsync(SupportTicket ticket, string assignedToUserId)
    {
        if (_emailService == null || _userManager == null)
            return;

        try
        {
            var assignedToUser = await _userManager.FindByIdAsync(assignedToUserId);
            if (assignedToUser == null || string.IsNullOrEmpty(assignedToUser.Email))
                return;

            var ticketUrl = $"{GetBaseUrl()}/admin/tickets/{ticket.Id}";
            var model = new
            {
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Priority = ticket.Priority,
                Status = ticket.Status,
                SlaDeadline = ticket.SlaDeadline,
                TicketUrl = ticketUrl,
                IsArabic = true
            };

            await _emailService.SendTemplatedEmailAsync(
                assignedToUser.Email,
                "SupportTicketAssigned",
                model,
                isArabic: true);

            _logger.LogInformation("Ticket assigned email sent to {Email} for ticket {TicketNumber}",
                assignedToUser.Email, ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ticket assigned email for {TicketNumber}", ticket.TicketNumber);
        }
    }

    /// <summary>
    /// Get base URL for email links
    /// </summary>
    private string GetBaseUrl()
    {
        // Default to production URL, can be configured
        return "https://shahin-ai.com";
    }

    #endregion

    #region Escalation

    /// <summary>
    /// Escalate ticket to platform admin (called by tenant admin)
    /// </summary>
    public async Task<SupportTicket> EscalateToPlatformAdminAsync(
        Guid ticketId, 
        string? escalationNotes = null, 
        string? escalatedByUserId = null)
    {
        var ticket = await _context.SupportTickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} not found");

        // Find available platform admin
        var platformAdmin = await _context.PlatformAdmins
            .Where(pa => pa.IsActive && !pa.IsDeleted)
            .OrderBy(pa => pa.CreatedAt)
            .FirstOrDefaultAsync();

        if (platformAdmin == null)
        {
            throw new InvalidOperationException("No active platform admin available for escalation");
        }

        var previousAssignee = ticket.AssignedToUserId;
        ticket.AssignedToUserId = platformAdmin.UserId;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.Priority = ticket.Priority switch
        {
            "Low" => "Medium",
            "Medium" => "High",
            "High" => "Urgent",
            _ => "Urgent"
        };
        ticket.SlaDeadline = CalculateSlaDeadline(ticket.Priority);

        // Add history entry
        var history = new SupportTicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Action = "Escalated_To_PlatformAdmin",
            PreviousValue = previousAssignee ?? "Unassigned",
            NewValue = platformAdmin.UserId,
            ChangedByUserId = escalatedByUserId,
            Notes = escalationNotes ?? "Ticket escalated to platform admin by tenant admin",
            ChangedAt = DateTime.UtcNow
        };
        _context.SupportTicketHistories.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketNumber} escalated to platform admin {AdminId} by {UserId}",
            ticket.TicketNumber, platformAdmin.UserId, escalatedByUserId);

        // Send email notification to platform admin
        await SendTicketAssignedEmailAsync(ticket, platformAdmin.UserId);

        // Send email notification to customer about escalation
        if (!string.IsNullOrEmpty(ticket.UserEmail) && _emailService != null)
        {
            try
            {
                var ticketUrl = $"{GetBaseUrl()}/Support/Status?ticketNumber={ticket.TicketNumber}";
                var model = new
                {
                    TicketNumber = ticket.TicketNumber,
                    Subject = ticket.Subject,
                    EscalationNotes = escalationNotes ?? "Your ticket has been escalated to platform support for priority handling.",
                    TicketUrl = ticketUrl,
                    IsArabic = true
                };

                await _emailService.SendTemplatedEmailAsync(
                    ticket.UserEmail,
                    "SupportTicketEscalated",
                    model,
                    isArabic: true);

                _logger.LogInformation("Escalation notification sent to customer {Email} for ticket {TicketNumber}",
                    ticket.UserEmail, ticket.TicketNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send escalation email to customer for ticket {TicketNumber}", ticket.TicketNumber);
            }
        }

        return ticket;
    }

    /// <summary>
    /// Get tenant admin for a tenant
    /// </summary>
    private async Task<TenantUser?> GetTenantAdminAsync(Guid tenantId)
    {
        return await _context.TenantUsers
            .Where(tu => tu.TenantId == tenantId &&
                        tu.RoleCode == "Admin" &&
                        tu.Status == "Active" &&
                        !tu.IsDeleted)
            .OrderBy(tu => tu.CreatedAt) // Get the first/oldest admin
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateTicketNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"TKT-{year}";

        // Get the highest sequence number for this year
        var lastTicket = await _context.SupportTickets
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTicket != null)
        {
            // Extract sequence from ticket number (e.g., TKT-2026-000123)
            var parts = lastTicket.TicketNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}-{sequence:D6}";
    }

    private DateTime? CalculateSlaDeadline(string priority)
    {
        var now = DateTime.UtcNow;
        return priority switch
        {
            "Critical" or "Urgent" => now.AddHours(4),
            "High" => now.AddHours(24),
            "Medium" => now.AddDays(3),
            "Low" => now.AddDays(7),
            _ => now.AddDays(3)
        };
    }

    #endregion
}
