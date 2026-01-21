using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Approval Instance - Runtime execution of an ApprovalChain
    /// Tracks approval progression through multiple approvers
    /// </summary>
    public class ApprovalInstance : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant
        public Guid ApprovalChainId { get; set; }
        
        public string InstanceNumber { get; set; } = string.Empty; // APPR-2026-0001
        public Guid EntityId { get; set; } // ID of Evidence, Finding, Policy, etc.
        public string EntityType { get; set; } = string.Empty; // "Evidence", "Finding", "Policy"
        
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Approved, Rejected, Cancelled
        public string CurrentApproverRole { get; set; } = string.Empty; // Current step in approval chain
        public int CurrentStepIndex { get; set; } = 0;
        
        public DateTime InitiatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public Guid InitiatedByUserId { get; set; }
        public string? InitiatedByUserName { get; set; }
        
        // Final decision reason (JSON): {"approverRole": "AuditManager", "decision": "Approved", "reason": "..."}
        public string? FinalDecision { get; set; }
        
        // Navigation
        public virtual ApprovalChain ApprovalChain { get; set; } = null!;
    }
}
