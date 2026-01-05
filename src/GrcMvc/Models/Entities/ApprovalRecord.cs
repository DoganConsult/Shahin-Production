using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks approval records for workflows through multi-level approval chains
    /// </summary>
    public class ApprovalRecord : BaseEntity
    {
        public new Guid TenantId { get; set; }
        public Guid WorkflowId { get; set; }
        public string? WorkflowNumber { get; set; }

        // Submission details
        public string? SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Approval chain details
        public int CurrentApprovalLevel { get; set; }
        public string? Status { get; set; } // Pending, Approved, Rejected, Delegated
        public string? AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public string? Priority { get; set; } // Normal, High, Critical

        // Approval details
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Rejection details
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Delegation details
        public string? DelegatedBy { get; set; }
        public DateTime? DelegatedAt { get; set; }
        public string? DelegationReason { get; set; }

        // Comments and notes
        public string? Comments { get; set; }

        // Navigation
        public Guid? WorkflowInstanceId { get; set; }
        public virtual WorkflowInstance? Workflow { get; set; }
    }
}
