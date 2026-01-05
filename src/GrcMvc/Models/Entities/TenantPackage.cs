using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a package (collection of requirements) that applies to a specific tenant.
    /// Derived from rules engine execution.
    /// Layer 2: Tenant Context
    /// </summary>
    public class TenantPackage : BaseEntity
    {
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Reference to the Package from catalog (e.g., PKG_SECURITY_GOV_01)
        /// </summary>
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        
        /// <summary>
        /// Applicability: MANDATORY, RECOMMENDED, OPTIONAL
        /// Derived from rules
        /// </summary>
        public string Applicability { get; set; } = "MANDATORY";
        
        /// <summary>
        /// When this package was derived for the tenant
        /// </summary>
        public DateTime DerivedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Which rules caused this package to be derived
        /// JSON: [{ruleCode, reason}]
        /// Answers: "Why does this tenant need to address this package?"
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
