using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Delegation Rule - Define rules for task delegation and out-of-office handling
    /// Supports automatic delegation, approval authority limits, and temporary assignments
    /// </summary>
    public class DelegationRule : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant

        public string RuleCode { get; set; } = string.Empty; // DEL_OOO_01
        public string Name { get; set; } = string.Empty; // "Out of Office Delegation"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100;

        // ===== DELEGATION TYPE =====
        public string DelegationType { get; set; } = string.Empty; // OutOfOffice, Permanent, Temporary, Conditional

        // ===== DELEGATOR (FROM) =====
        public Guid? DelegatorUserId { get; set; } // Specific user delegating
        public string DelegatorRoleCode { get; set; } = string.Empty; // Or role-based delegation

        // ===== DELEGATE (TO) =====
        public Guid? DelegateUserId { get; set; } // Specific delegate
        public string DelegateRoleCode { get; set; } = string.Empty; // Or role-based
        public string DelegateSelectionRule { get; set; } = string.Empty; // Manager, Peer, Backup, Custom

        // ===== TIME PERIOD =====
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsIndefinite { get; set; } = false;

        // ===== SCOPE & LIMITS =====
        public string TaskTypesJson { get; set; } = "[]"; // Task types that can be delegated
        public string WorkflowCategoriesJson { get; set; } = "[]"; // Workflow categories
        public decimal? ApprovalAmountLimit { get; set; } // Max amount delegate can approve
        public string ApprovalLevelLimit { get; set; } = string.Empty; // Max approval level
        public bool CanSubDelegate { get; set; } = false; // Can delegate further delegate?

        // ===== PERMISSIONS =====
        public bool CanApprove { get; set; } = true;
        public bool CanReject { get; set; } = true;
        public bool CanReassign { get; set; } = false;
        public bool CanEscalate { get; set; } = true;
        public bool CanViewConfidential { get; set; } = false;

        // ===== NOTIFICATIONS =====
        public bool NotifyDelegatorOnAction { get; set; } = true;
        public bool NotifyDelegateOnAssignment { get; set; } = true;
        public bool RequireDelegatorConfirmation { get; set; } = false;

        // ===== AUDIT =====
        public string CreatedReason { get; set; } = string.Empty; // Why delegation was created
        public Guid? ApprovedByUserId { get; set; } // Manager who approved delegation
        public DateTime? ApprovedAt { get; set; }
    }

    /// <summary>
    /// Delegation Log - Track all delegation actions for audit
    /// </summary>
    public class DelegationLog : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public Guid DelegationRuleId { get; set; }
        public Guid TaskId { get; set; }

        public Guid DelegatorUserId { get; set; }
        public Guid DelegateUserId { get; set; }

        public string Action { get; set; } = string.Empty; // Delegated, Approved, Rejected, Returned
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
        public string ActionBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation
        public virtual DelegationRule DelegationRule { get; set; } = null!;
    }
}
