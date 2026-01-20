using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing support tickets (platform admin ticketing system)
    /// </summary>
    public interface ISupportTicketService
    {
        /// <summary>
        /// Create a new support ticket
        /// </summary>
        Task<SupportTicket> CreateTicketAsync(CreateTicketDto dto);

        /// <summary>
        /// Get ticket by ID
        /// </summary>
        Task<SupportTicket?> GetTicketByIdAsync(Guid ticketId);

        /// <summary>
        /// Get ticket by ticket number
        /// </summary>
        Task<SupportTicket?> GetTicketByNumberAsync(string ticketNumber);

        /// <summary>
        /// Get all tickets with filtering
        /// </summary>
        Task<List<SupportTicket>> GetTicketsAsync(TicketFilterDto? filter = null);

        /// <summary>
        /// Get tickets for a specific tenant
        /// </summary>
        Task<List<SupportTicket>> GetTicketsByTenantAsync(Guid tenantId);

        /// <summary>
        /// Get tickets assigned to a platform admin
        /// </summary>
        Task<List<SupportTicket>> GetTicketsByAssigneeAsync(string userId);

        /// <summary>
        /// Update ticket status
        /// </summary>
        Task<SupportTicket> UpdateTicketStatusAsync(Guid ticketId, string status, string? notes = null, string? changedByUserId = null);

        /// <summary>
        /// Assign ticket to platform admin
        /// </summary>
        Task<SupportTicket> AssignTicketAsync(Guid ticketId, string assignedToUserId, string? changedByUserId = null);

        /// <summary>
        /// Update ticket priority
        /// </summary>
        Task<SupportTicket> UpdateTicketPriorityAsync(Guid ticketId, string priority, string? changedByUserId = null);

        /// <summary>
        /// Add comment to ticket
        /// </summary>
        Task<SupportTicketComment> AddCommentAsync(Guid ticketId, string content, string userId, bool isInternal = false);

        /// <summary>
        /// Resolve ticket
        /// </summary>
        Task<SupportTicket> ResolveTicketAsync(Guid ticketId, string resolutionNotes, string? resolvedByUserId = null);

        /// <summary>
        /// Close ticket
        /// </summary>
        Task<SupportTicket> CloseTicketAsync(Guid ticketId, string? closedByUserId = null);

        /// <summary>
        /// Get ticket statistics
        /// </summary>
        Task<TicketStatistics> GetStatisticsAsync(TicketStatisticsFilterDto? filter = null);

        /// <summary>
        /// Get tickets requiring attention (SLA breaches, overdue, etc.)
        /// </summary>
        Task<List<SupportTicket>> GetTicketsRequiringAttentionAsync();

        /// <summary>
        /// Escalate ticket to platform admin (tenant admin only)
        /// </summary>
        Task<SupportTicket> EscalateToPlatformAdminAsync(Guid ticketId, string? escalationNotes = null, string? escalatedByUserId = null);
    }

    /// <summary>
    /// DTO for creating a ticket
    /// </summary>
    public class CreateTicketDto
    {
        public Guid? TenantId { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Technical";
        public string Priority { get; set; } = "Medium";
        public string? Tags { get; set; }
        public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
    }

    /// <summary>
    /// DTO for filtering tickets
    /// </summary>
    public class TicketFilterDto
    {
        public Guid? TenantId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? AssignedToUserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Ticket statistics
    /// </summary>
    public class TicketStatistics
    {
        public int TotalTickets { get; set; }
        public int NewTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int UrgentTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int SlaBreachedTickets { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public double AverageFirstResponseTimeHours { get; set; }
        public Dictionary<string, int> TicketsByCategory { get; set; } = new();
        public Dictionary<string, int> TicketsByStatus { get; set; } = new();
    }

    /// <summary>
    /// Filter for statistics
    /// </summary>
    public class TicketStatisticsFilterDto
    {
        public Guid? TenantId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? AssignedToUserId { get; set; }
    }
}
