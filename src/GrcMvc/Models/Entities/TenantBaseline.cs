using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a baseline that applies to a specific tenant.
    /// Derived from rules engine execution.
    /// Layer 2: Tenant Context
    /// </summary>
    public class TenantBaseline : BaseEntity
    {
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Reference to the Baseline from catalog (e.g., BL_SECURITY_BANKING)
        /// </summary>
        public string BaselineCode { get; set; } = string.Empty;
        public string BaselineName { get; set; } = string.Empty;
        
        /// <summary>
        /// Applicability: MANDATORY, RECOMMENDED, OPTIONAL
        /// Derived from rules
        /// </summary>
        public string Applicability { get; set; } = "MANDATORY";
        
        /// <summary>
        /// When this baseline was derived for the tenant
        /// </summary>
        public DateTime DerivedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Which rules caused this baseline to be derived
        /// JSON: [{ruleCode, reason}]
        /// Answers: "Why does this tenant apply this baseline?"
        /// </summary>
        public string ReasonJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Reference to the rule execution that derived this
        /// </summary>
        public Guid? RuleExecutionLogId { get; set; }
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
