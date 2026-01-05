using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Validation Rule - Define data validation and qualification rules
    /// Ensures data cleanliness, completeness, and quality
    /// Can be applied to: Evidence, Documents, Forms, Onboarding data
    /// </summary>
    public class ValidationRule : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant (null = global)

        public string RuleCode { get; set; } = string.Empty; // VAL_CR_NUMBER_01
        public string Name { get; set; } = string.Empty; // "CR Number Validation"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100;

        // ===== TARGET =====
        public string EntityType { get; set; } = string.Empty; // OrganizationProfile, Evidence, Document, Assessment
        public string FieldName { get; set; } = string.Empty; // Field to validate
        public string FieldPath { get; set; } = string.Empty; // JSON path for nested fields

        // ===== VALIDATION TYPE =====
        public string ValidationType { get; set; } = string.Empty; // Required, Format, Range, Lookup, Custom, CrossField, External
        public string DataType { get; set; } = string.Empty; // String, Number, Date, Email, Phone, File, Json

        // ===== VALIDATION RULES =====
        public bool IsRequired { get; set; } = false;
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string RegexPattern { get; set; } = string.Empty; // Regex for format validation
        public string AllowedValuesJson { get; set; } = "[]"; // ["Value1", "Value2"]
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string DateFormat { get; set; } = string.Empty; // yyyy-MM-dd
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        // ===== FILE VALIDATION =====
        public string AllowedFileTypes { get; set; } = string.Empty; // .pdf,.doc,.xlsx
        public int? MaxFileSizeMB { get; set; }
        public bool RequireDigitalSignature { get; set; } = false;

        // ===== CROSS-FIELD VALIDATION =====
        public string DependentFieldName { get; set; } = string.Empty; // Field this depends on
        public string DependentConditionJson { get; set; } = "{}"; // {"field": "Country", "equals": "SA"}

        // ===== EXTERNAL VALIDATION =====
        public string ExternalApiUrl { get; set; } = string.Empty; // API to call for validation
        public string ExternalApiMethod { get; set; } = "GET"; // GET, POST
        public string ExternalApiHeaders { get; set; } = "{}"; // Headers JSON
        public int ExternalApiTimeoutMs { get; set; } = 5000;

        // ===== QUALIFICATION RULES =====
        public bool IsQualificationRule { get; set; } = false; // True = qualification, False = validation
        public string QualificationLevel { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
        public int QualificationScore { get; set; } = 0; // Points for meeting this rule
        public string QualificationCriteria { get; set; } = string.Empty; // Description of qualification criteria

        // ===== ERROR HANDLING =====
        public string ErrorMessageEn { get; set; } = string.Empty;
        public string ErrorMessageAr { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning, Info
        public bool BlockOnFailure { get; set; } = true; // Block submission on failure

        // ===== AUDIT =====
        public int ExecutionCount { get; set; } = 0;
        public int PassCount { get; set; } = 0;
        public int FailCount { get; set; } = 0;
    }

    /// <summary>
    /// Validation Result - Track validation execution results
    /// </summary>
    public class ValidationResult : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public Guid ValidationRuleId { get; set; }

        public Guid EntityId { get; set; } // Entity being validated
        public string EntityType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty; // Value that was validated

        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty; // Passed, Failed, Warning, Skipped
        public string ErrorMessage { get; set; } = string.Empty;

        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        public string ValidatedBy { get; set; } = string.Empty; // User or "System"

        // Navigation
        public virtual ValidationRule ValidationRule { get; set; } = null!;
    }

    /// <summary>
    /// Data Quality Score - Track overall data quality for entities
    /// </summary>
    public class DataQualityScore : BaseEntity
    {
        public Guid? TenantId { get; set; }

        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty; // OrganizationProfile, Evidence, etc.

        // ===== QUALITY METRICS =====
        public int CompletenessScore { get; set; } = 0; // % of required fields filled
        public int AccuracyScore { get; set; } = 0; // % of fields passing validation
        public int ConsistencyScore { get; set; } = 0; // % of cross-field validations passing
        public int TimelinessScore { get; set; } = 0; // % of data updated within SLA
        public int OverallScore { get; set; } = 0; // Weighted average

        // ===== QUALIFICATION =====
        public string QualificationLevel { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
        public int QualificationPoints { get; set; } = 0;
        public bool IsQualified { get; set; } = false;

        // ===== DETAILS =====
        public int TotalFields { get; set; } = 0;
        public int ValidFields { get; set; } = 0;
        public int InvalidFields { get; set; } = 0;
        public int MissingFields { get; set; } = 0;
        public string IssuesJson { get; set; } = "[]"; // List of issues found

        // ===== TIMESTAMPS =====
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastImprovedAt { get; set; }
    }
}
