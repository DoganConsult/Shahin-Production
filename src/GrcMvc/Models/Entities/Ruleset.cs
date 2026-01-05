using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Defines a set of applicability rules for scope generation.
    /// Layer 2: Tenant Context
    /// Versions immutable; versioning via RulesetVersion
    /// </summary>
    public class Ruleset : BaseEntity
    {
        public Guid TenantId { get; set; }
        
        public string RulesetCode { get; set; } = string.Empty; // e.g., RULESET_KSA_BANKING_V1
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// Status: DRAFT, ACTIVE, RETIRED
        /// Only ACTIVE rulesets are used for scope generation
        /// </summary>
        public string Status { get; set; } = "DRAFT";
        
        /// <summary>
        /// When this ruleset version became active
        /// </summary>
        public DateTime? ActivatedAt { get; set; }
        
        /// <summary>
        /// Reference to previous version (if any)
        /// Enables rollback/version history
        /// </summary>
        public Guid? PreviousVersionId { get; set; }
        
        /// <summary>
        /// Notes on what changed in this version
        /// </summary>
        public string ChangeNotes { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<Rule> Rules { get; set; } = new List<Rule>();
        public virtual ICollection<RuleExecutionLog> ExecutionLogs { get; set; } = new List<RuleExecutionLog>();
    }
}
