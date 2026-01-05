using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a compliance plan created for a tenant.
    /// Contains phases and assessments.
    /// Layer 3: Operational
    /// </summary>
    public class Plan : BaseEntity
    {
        public Guid TenantId { get; set; }
        
        public string PlanCode { get; set; } = string.Empty; // e.g., PLAN_2026_Q1
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Status: Draft, Active, Paused, Completed, Archived
        /// </summary>
        public string Status { get; set; } = "Draft";
        
        /// <summary>
        /// Plan type: QuickScan, Full, Remediation, Custom
        /// Determined by rules/org size
        /// </summary>
        public string PlanType { get; set; } = string.Empty;
        
        /// <summary>
        /// When the plan should start execution
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// When the plan should be completed
        /// </summary>
        public DateTime TargetEndDate { get; set; }
        
        /// <summary>
        /// When the plan actually started execution (null until activated)
        /// </summary>
        public DateTime? ActualStartDate { get; set; }
        
        /// <summary>
        /// When the plan was actually completed (null if not completed)
        /// </summary>
        public DateTime? ActualEndDate { get; set; }
        
        /// <summary>
        /// Rules engine version used to derive scope for this plan
        /// Preserves immutability: if rules change, create new plan
        /// </summary>
        public int RulesetVersion { get; set; } = 1;
        
        /// <summary>
        /// Scope snapshot as JSON: baselines, packages, templates at time of plan creation
        /// Preserves historical applicability
        /// </summary>
        public string ScopeSnapshotJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Correlation ID for audit trail and event tracking
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<PlanPhase> Phases { get; set; } = new List<PlanPhase>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
