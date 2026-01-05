using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Workflow Escalation - Records when a workflow task is escalated due to SLA breach
    /// Used by background jobs to track escalations and send notifications
    /// </summary>
    public class WorkflowEscalation : BaseEntity
    {
        public Guid TenantId { get; set; } // Multi-tenant isolation
        public Guid WorkflowInstanceId { get; set; }
        public Guid TaskId { get; set; }
        
        public int EscalationLevel { get; set; } // 1=First, 2=Second, 3=Manager, 4=Critical
        public string EscalationReason { get; set; } = string.Empty;
        
        public DateTime EscalatedAt { get; set; }
        public Guid OriginalAssignee { get; set; } // Original task assignee
        public Guid? EscalatedToUserId { get; set; } // Who task was escalated to
        
        public string Status { get; set; } = "Pending"; // Pending, Acknowledged, Resolved
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        
        // Navigation properties
        public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
        public virtual WorkflowTask WorkflowTask { get; set; } = null!;
    }
}
