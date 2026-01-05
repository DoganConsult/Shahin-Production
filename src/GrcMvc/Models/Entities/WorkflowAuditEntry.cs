using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Workflow Audit Entry - Immutable audit trail for all workflow events
    /// Tracks InstanceStarted, TaskCreated, TaskCompleted, ApprovalApproved/Rejected, InstanceCompleted, InstanceRejected
    /// </summary>
    public class WorkflowAuditEntry : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant
        public Guid WorkflowInstanceId { get; set; }
        
        public string EventType { get; set; } = string.Empty; // "InstanceStarted", "TaskCreated", "TaskCompleted", "ApprovalApproved", "ApprovalRejected", "InstanceCompleted", "InstanceRejected"
        public string SourceEntity { get; set; } = string.Empty; // "WorkflowInstance", "WorkflowTask", "ApprovalInstance"
        public Guid SourceEntityId { get; set; }
        
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        
        public Guid ActingUserId { get; set; } // User who performed the action
        public string? ActingUserName { get; set; }
        
        public string? Description { get; set; } // Human-readable summary
        
        // Additional context (JSON): {"taskName": "...", "approverRole": "...", "reason": "..."}
        public string? AdditionalData { get; set; }
        
        public DateTime EventTime { get; set; }
    }
}
