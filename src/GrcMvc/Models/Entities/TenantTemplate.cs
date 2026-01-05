using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents an assessment template that applies to a specific tenant.
    /// Derived from rules engine execution.
    /// Layer 2: Tenant Context
    /// Templates are used to create Assessments in Layer 3
    /// </summary>
    public class TenantTemplate : BaseEntity
    {
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Reference to the Template from catalog (e.g., TPL_SAMA_CSF_ASSESSMENT)
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        
        /// <summary>
        /// Applicability: MANDATORY, RECOMMENDED, OPTIONAL
        /// Derived from rules
        /// </summary>
        public string Applicability { get; set; } = "MANDATORY";
        
        /// <summary>
        /// When this template was derived for the tenant
        /// </summary>
        public DateTime DerivedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Which rules caused this template to be derived
        /// JSON: [{ruleCode, reason}]
        /// Answers: "Why should this tenant run this assessment?"
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
