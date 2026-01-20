using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Reference data for dropdown options in onboarding wizard
    /// Stores all possible values for fields like organization types, industries, countries, etc.
    /// </summary>
    public class ReferenceData : BaseEntity
    {
        /// <summary>
        /// Category/field name (e.g., "OrganizationType", "IndustrySector", "CountryOfIncorporation")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The actual value (e.g., "enterprise", "SME", "government")
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display label in English
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string LabelEn { get; set; } = string.Empty;

        /// <summary>
        /// Display label in Arabic
        /// </summary>
        [MaxLength(255)]
        public string? LabelAr { get; set; }

        /// <summary>
        /// Description/help text in English
        /// </summary>
        [MaxLength(1000)]
        public string? DescriptionEn { get; set; }

        /// <summary>
        /// Description/help text in Arabic
        /// </summary>
        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Whether this option is active/available
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is a common/recommended option for AI suggestions
        /// </summary>
        public bool IsCommon { get; set; } = false;

        /// <summary>
        /// Industry/sector context (for conditional recommendations)
        /// </summary>
        [MaxLength(100)]
        public string? IndustryContext { get; set; }

        /// <summary>
        /// Organization type context (for conditional recommendations)
        /// </summary>
        [MaxLength(50)]
        public string? OrganizationTypeContext { get; set; }

        /// <summary>
        /// Metadata JSON for additional context (e.g., country codes, framework codes)
        /// </summary>
        public string? MetadataJson { get; set; }
    }
}
