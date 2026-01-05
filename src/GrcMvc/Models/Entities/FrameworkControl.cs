using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Framework Control - Individual control/requirement from a regulatory framework
    /// Layer 1: Global (Platform) - Immutable regulatory content
    /// Maps to CSV: id,framework_code,version,control_number,domain,title_ar,title_en,requirement_ar,requirement_en,control_type,maturity_level,implementation_guidance_en,evidence_requirements,mapping_iso27001,mapping_nist,status
    /// </summary>
    public class FrameworkControl : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string FrameworkCode { get; set; } = string.Empty; // e.g., "NCA-ECC", "PDPL", "SAMA-CSF"

        [StringLength(20)]
        public string Version { get; set; } = string.Empty; // e.g., "2.0"

        [Required]
        [StringLength(50)]
        public string ControlNumber { get; set; } = string.Empty; // e.g., "1.1", "2.3"

        [StringLength(100)]
        public string Domain { get; set; } = string.Empty; // e.g., "Governance", "Protection", "Detection"

        [StringLength(500)]
        public string TitleAr { get; set; } = string.Empty; // Arabic title

        [Required]
        [StringLength(500)]
        public string TitleEn { get; set; } = string.Empty; // English title

        [StringLength(4000)]
        public string RequirementAr { get; set; } = string.Empty; // Arabic requirement text

        [Required]
        public string RequirementEn { get; set; } = string.Empty; // English requirement text

        [StringLength(50)]
        public string ControlType { get; set; } = string.Empty; // preventive, detective, corrective

        public int MaturityLevel { get; set; } = 1; // 1-5

        public string ImplementationGuidanceEn { get; set; } = string.Empty; // Implementation steps

        [StringLength(1000)]
        public string EvidenceRequirements { get; set; } = string.Empty; // Pipe-separated evidence types

        [StringLength(50)]
        public string MappingIso27001 { get; set; } = string.Empty; // e.g., "A.5.1"

        [StringLength(50)]
        public string MappingNist { get; set; } = string.Empty; // e.g., "ID.GV", "PR.AC"

        [StringLength(20)]
        public string Status { get; set; } = "active"; // active, deprecated, draft

        // Computed/derived fields
        public string FullControlId => $"{FrameworkCode}-{ControlNumber}"; // e.g., "NCA-ECC-1.1"

        // For search and filtering
        [StringLength(200)]
        public string SearchKeywords { get; set; } = string.Empty;

        // Scoring configuration
        public int DefaultWeight { get; set; } = 1; // Weight for scoring calculations
        public int MinEvidenceScore { get; set; } = 70; // Minimum evidence score to pass
    }
}
