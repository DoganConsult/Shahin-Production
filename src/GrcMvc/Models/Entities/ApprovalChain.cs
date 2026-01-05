using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Approval Chain - Defines approval routing (e.g., Evidence → Compliance Officer → Auditor → Audit Manager)
    /// Used for Evidence Review, Finding Remediation, Policy Review workflows
    /// </summary>
    public class ApprovalChain : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant
        
        public string Name { get; set; } = string.Empty; // "Evidence Review Chain", "Policy Approval Chain"
        public string EntityType { get; set; } = string.Empty; // "Evidence", "Finding", "Policy", "ActionPlan"
        public string Category { get; set; } = string.Empty; // "Compliance", "Audit", "Security", "Operations"
        
        public string ApprovalMode { get; set; } = "Sequential"; // Sequential, Parallel, Hybrid
        public bool IsActive { get; set; } = true;
        
        // JSON array of approvers: [{"order": 1, "role": "ComplianceOfficer", "slaHours": 48}, {"order": 2, "role": "AuditManager", "slaHours": 24}]
        public string ApprovalSteps { get; set; } = "[]";
        
        // Navigation
        public virtual ICollection<ApprovalInstance> Instances { get; set; } = new List<ApprovalInstance>();
    }
}
