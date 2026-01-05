using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Individual applicability rule: IF (condition) THEN (derive Baselines/Packages/Templates)
    /// Example: IF sector=Banking AND country=SA THEN apply ECC+SAMA_CSF+PDPL
    /// </summary>
    public class Rule : BaseEntity
    {
        public Guid RulesetId { get; set; }
        
        public string RuleCode { get; set; } = string.Empty; // e.g., RULE_BANKING_SAUDI_APPLICABILITY
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Condition as JSON: {"sector": ["Banking"], "country": ["SA"]}
        /// Evaluated against OrganizationProfile fields
        /// </summary>
        public string ConditionJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Actions as JSON: 
        /// {
        ///   "baselines": ["BL_SECURITY_BANKING"],
        ///   "packages": ["PKG_BANKING_COMPLIANCE_01", "PKG_PRIVACY_DATA_01"],
        ///   "templates": ["TPL_SAMA_CSF_ASSESSMENT"]
        /// }
        /// </summary>
        public string ActionsJson { get; set; } = string.Empty;
        
        /// <summary>
        /// Priority for rule evaluation (lower = higher priority)
        /// Allows more specific rules to match before generic ones
        /// </summary>
        public int Priority { get; set; } = 100;
        
        /// <summary>
        /// Status: DRAFT, ACTIVE, RETIRED
        /// </summary>
        public string Status { get; set; } = "ACTIVE";
        
        /// <summary>
        /// Reason why this rule exists (for audit trail)
        /// E.g., "KSA banking sector requires SAMA CSF compliance"
        /// </summary>
        public string BusinessReason { get; set; } = string.Empty;
        
        /// <summary>
        /// When this rule was last modified (for versioning)
        /// </summary>
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Ruleset Ruleset { get; set; } = null!;
    }
}
