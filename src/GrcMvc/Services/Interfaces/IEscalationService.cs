using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing workflow escalations based on SLA breaches
    /// Monitors task deadlines and automatically escalates overdue approvals
    /// </summary>
    public interface IEscalationService
    {
        /// <summary>
        /// Check for overdue approvals and trigger escalations
        /// Runs periodically via background job
        /// </summary>
        Task<int> ProcessEscalationsAsync(Guid tenantId);

        /// <summary>
        /// Get escalations for a specific approval
        /// </summary>
        Task<List<EscalationDto>> GetEscalationsAsync(Guid tenantId, Guid approvalId);

        /// <summary>
        /// Manually trigger escalation for an approval
        /// </summary>
        Task<bool> EscalateApprovalAsync(Guid tenantId, Guid approvalId, string escalatedBy, string reason);

        /// <summary>
        /// Get escalation configuration for a workflow
        /// </summary>
        Task<EscalationConfigDto> GetEscalationConfigAsync(Guid tenantId, Guid workflowDefinitionId);

        /// <summary>
        /// Update escalation rules
        /// </summary>
        Task<bool> UpdateEscalationRulesAsync(Guid tenantId, Guid workflowDefinitionId, List<EscalationRuleDto> rules);

        /// <summary>
        /// Get escalation statistics for dashboard
        /// </summary>
        Task<EscalationStatsDto> GetEscalationStatsAsync(Guid tenantId);
    }

    /// <summary>
    /// DTO for escalation details
    /// </summary>
    public class EscalationDto
    {
        public Guid Id { get; set; }
        public Guid ApprovalId { get; set; }
        public Guid WorkflowId { get; set; }
        public string? Status { get; set; } // Active, Resolved, Dismissed
        public int EscalationLevel { get; set; } // 1st, 2nd, 3rd escalation
        public string? EscalatedTo { get; set; }
        public string? EscalatedToName { get; set; }
        public DateTime EscalatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Reason { get; set; }
        public int HoursOverdue { get; set; }
    }

    /// <summary>
    /// DTO for escalation configuration
    /// </summary>
    public class EscalationConfigDto
    {
        public Guid WorkflowDefinitionId { get; set; }
        public string? WorkflowName { get; set; }
        public List<EscalationRuleDto>? Rules { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for escalation rule
    /// </summary>
    public class EscalationRuleDto
    {
        public int Level { get; set; }
        public int HoursOverdue { get; set; }
        public string? EscalateTo { get; set; } // Role or user
        public string? NotificationTemplate { get; set; }
        public bool RequireAcknowledgment { get; set; }
    }

    /// <summary>
    /// DTO for escalation statistics
    /// </summary>
    public class EscalationStatsDto
    {
        public int TotalEscalations { get; set; }
        public int ActiveEscalations { get; set; }
        public int ResolvedEscalations { get; set; }
        public double AverageHoursToResolve { get; set; }
        public Dictionary<string, int>? ByLevel { get; set; }
        public Dictionary<string, int>? ByWorkflow { get; set; }
    }
}
