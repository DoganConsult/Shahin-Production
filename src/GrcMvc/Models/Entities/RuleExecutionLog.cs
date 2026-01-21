using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Audit log for rule execution.
    /// Documents when scope was generated, which rules matched, why.
    /// Enables traceability: "Why does this tenant apply framework X?"
    /// </summary>
    public class RuleExecutionLog : BaseEntity
    {
        public Guid RulesetId { get; set; }
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Timestamp when rules were executed
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// User who triggered scope generation (or "SYSTEM" if automatic)
        /// </summary>
        public string ExecutedBy { get; set; } = "SYSTEM";
        
        /// <summary>
        /// Which rules matched: JSON array of {ruleCode, matched, reason}
        /// </summary>
        public string MatchedRulesJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Organization profile snapshot used for matching (as JSON)
        /// Preserves state at execution time
        /// </summary>
        public string OrgProfileSnapshotJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Derived scope: list of baselines/packages/templates that resulted
        /// </summary>
        public string DerivedScopeJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Correlation ID linking this execution to specific plan/assessment
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Status: SUCCESS, PARTIAL, FAILED
        /// </summary>
        public string Status { get; set; } = "SUCCESS";
        
        /// <summary>
        /// Error message if execution failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Ruleset Ruleset { get; set; } = null!;
    }
}
