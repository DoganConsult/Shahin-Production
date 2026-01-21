using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Catalogs
{
    /// <summary>
    /// Global Regulator Catalog - Regulatory authorities (Saudi + International)
    /// Layer 1: Global (Platform)
    /// 92 regulators: 62 Saudi, 20 International, 10 GCC/Regional
    /// </summary>
    public class RegulatorCatalog : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // e.g., "NCA", "SAMA", "ISO"

        [Required]
        [StringLength(200)]
        public string NameAr { get; set; } = string.Empty; // Arabic name

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; } = string.Empty; // English name

        [StringLength(500)]
        public string JurisdictionEn { get; set; } = string.Empty; // e.g., "Cybersecurity & Critical Infrastructure Protection"

        [StringLength(200)]
        public string Website { get; set; } = string.Empty; // e.g., "https://nca.gov.sa"

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // cybersecurity, financial, healthcare, etc.

        [StringLength(50)]
        public string Sector { get; set; } = string.Empty; // all, banking_finance, healthcare, etc.

        public int? Established { get; set; } // Year established

        [StringLength(20)]
        public string RegionType { get; set; } = "saudi"; // saudi, international, regional

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation - Frameworks issued by this regulator
        public virtual ICollection<FrameworkCatalog> Frameworks { get; set; } = new List<FrameworkCatalog>();
    }

    /// <summary>
    /// Global Framework Catalog - Compliance frameworks (e.g., NCA-ECC, SAMA-CSF, ISO-27001)
    /// Layer 1: Global (Platform)
    /// 163+ frameworks with version tracking
    /// </summary>
    public class FrameworkCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // e.g., "NCA-ECC", "SAMA-CSF", "ISO-27001"

        [StringLength(20)]
        public string Version { get; set; } = string.Empty; // e.g., "2.0", "1.1", "2022"

        [Required]
        [StringLength(300)]
        public string TitleEn { get; set; } = string.Empty; // English title

        [Required]
        [StringLength(300)]
        public string TitleAr { get; set; } = string.Empty; // Arabic title

        [StringLength(2000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [StringLength(2000)]
        public string DescriptionAr { get; set; } = string.Empty;

        public Guid? RegulatorId { get; set; } // FK to RegulatorCatalog

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // cybersecurity, privacy, governance, etc.

        public bool IsMandatory { get; set; } = false; // Required by law vs voluntary

        public int ControlCount { get; set; } = 0; // Number of controls in this framework

        [StringLength(50)]
        public string Domains { get; set; } = string.Empty; // Number of domains/sections

        public DateTime? EffectiveDate { get; set; }
        public DateTime? RetiredDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Draft, Retired

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual RegulatorCatalog? Regulator { get; set; }
        public virtual ICollection<ControlCatalog> Controls { get; set; } = new List<ControlCatalog>();
    }

    /// <summary>
    /// Global Control Catalog - Individual compliance controls/requirements
    /// Layer 1: Global (Platform)
    /// 13,500+ controls with bilingual content and cross-mappings
    /// </summary>
    public class ControlCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ControlId { get; set; } = string.Empty; // e.g., "NCA-ECC-1-1-1"

        public Guid FrameworkId { get; set; } // FK to FrameworkCatalog

        [StringLength(20)]
        public string Version { get; set; } = string.Empty; // Framework version this belongs to

        [Required]
        [StringLength(50)]
        public string ControlNumber { get; set; } = string.Empty; // e.g., "1.1.1", "AC-1"

        [StringLength(100)]
        public string Domain { get; set; } = string.Empty; // e.g., "Governance", "Access Control"

        [StringLength(100)]
        public string Subdomain { get; set; } = string.Empty; // e.g., "Policies", "User Access"

        [Required]
        [StringLength(500)]
        public string TitleAr { get; set; } = string.Empty; // Arabic title

        [Required]
        [StringLength(500)]
        public string TitleEn { get; set; } = string.Empty; // English title

        [StringLength(4000)]
        public string RequirementAr { get; set; } = string.Empty; // Arabic requirement text

        [StringLength(4000)]
        public string RequirementEn { get; set; } = string.Empty; // English requirement text

        [StringLength(30)]
        public string ControlType { get; set; } = "preventive"; // preventive, detective, corrective

        public int MaturityLevel { get; set; } = 1; // 1-4 maturity level

        [StringLength(2000)]
        public string ImplementationGuidanceEn { get; set; } = string.Empty;

        [StringLength(1000)]
        public string EvidenceRequirements { get; set; } = string.Empty; // Comma-separated evidence types

        // Cross-framework mappings
        [StringLength(100)]
        public string MappingIso27001 { get; set; } = string.Empty; // e.g., "A.5.1.1"

        [StringLength(100)]
        public string MappingNistCsf { get; set; } = string.Empty; // e.g., "ID.GV-1"

        [StringLength(100)]
        public string MappingOther { get; set; } = string.Empty; // Other mappings JSON

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Deprecated

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual FrameworkCatalog Framework { get; set; } = null!;
    }

    /// <summary>
    /// Global Role Catalog - Fixed roles that workflow steps reference by RoleCode
    /// Layer 1: Global (Platform)
    /// </summary>
    public class RoleCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string RoleCode { get; set; } = string.Empty; // e.g., "COMPLIANCE_OFFICER"

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty; // e.g., "Compliance Officer"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Layer { get; set; } = string.Empty; // Executive, Management, Operational, Support

        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        public int ApprovalLevel { get; set; } = 0; // 0-4

        public bool CanApprove { get; set; } = false;
        public bool CanReject { get; set; } = false;
        public bool CanEscalate { get; set; } = false;
        public bool CanReassign { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual ICollection<TitleCatalog> AllowedTitles { get; set; } = new List<TitleCatalog>();
    }

    /// <summary>
    /// Global Title Catalog - Job titles that can be assigned to users
    /// Dependent on Role (dropdown dependent)
    /// Layer 1: Global (Platform)
    /// </summary>
    public class TitleCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TitleCode { get; set; } = string.Empty; // e.g., "SR_COMPLIANCE_ANALYST"

        [Required]
        [StringLength(100)]
        public string TitleName { get; set; } = string.Empty; // e.g., "Senior Compliance Analyst"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public Guid RoleCatalogId { get; set; } // FK to parent role

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual RoleCatalog RoleCatalog { get; set; } = null!;
    }

    /// <summary>
    /// Global Baseline Catalog - Compliance baselines (e.g., NCA ECC, SAMA CSF)
    /// Layer 1: Global (Platform)
    /// </summary>
    public class BaselineCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string BaselineCode { get; set; } = string.Empty; // e.g., "NCA_ECC_2024"

        [Required]
        [StringLength(200)]
        public string BaselineName { get; set; } = string.Empty; // e.g., "NCA Essential Cybersecurity Controls 2024"

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string RegulatorCode { get; set; } = string.Empty; // e.g., "NCA"

        [StringLength(100)]
        public string Version { get; set; } = string.Empty; // e.g., "2.0"

        public DateTime EffectiveDate { get; set; }
        public DateTime? RetiredDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Draft, Retired

        public int ControlCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual ICollection<PackageCatalog> Packages { get; set; } = new List<PackageCatalog>();
    }

    /// <summary>
    /// Global Package Catalog - Business-friendly groupings of requirements
    /// What clients see in UI (not technical framework details)
    /// Layer 1: Global (Platform)
    /// </summary>
    public class PackageCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string PackageCode { get; set; } = string.Empty; // e.g., "DATA_PROTECTION"

        [Required]
        [StringLength(200)]
        public string PackageName { get; set; } = string.Empty; // e.g., "Data Protection & Privacy"

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Security, Privacy, Governance, Risk

        public Guid? BaselineCatalogId { get; set; } // Optional FK to baseline

        public int RequirementCount { get; set; } = 0;
        public int EstimatedDays { get; set; } = 0; // Estimated completion time

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual BaselineCatalog? BaselineCatalog { get; set; }
        public virtual ICollection<TemplateCatalog> Templates { get; set; } = new List<TemplateCatalog>();
    }

    /// <summary>
    /// Global Template Catalog - Assessment templates
    /// Layer 1: Global (Platform)
    /// </summary>
    public class TemplateCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TemplateCode { get; set; } = string.Empty; // e.g., "NCA_ECC_FULL"

        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty; // e.g., "NCA ECC Full Assessment"

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty; // QuickScan, Full, Remediation

        public Guid? PackageCatalogId { get; set; } // Optional FK to package

        public int RequirementCount { get; set; } = 0;
        public int EstimatedDays { get; set; } = 0;

        public string RequirementsJson { get; set; } = "[]"; // JSON array of requirement IDs

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public virtual PackageCatalog? PackageCatalog { get; set; }
    }

    /// <summary>
    /// Global Evidence Type Catalog
    /// Layer 1: Global (Platform)
    /// </summary>
    public class EvidenceTypeCatalog : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string EvidenceTypeCode { get; set; } = string.Empty; // e.g., "POLICY_DOC"

        [Required]
        [StringLength(200)]
        public string EvidenceTypeName { get; set; } = string.Empty; // e.g., "Policy Document"

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Document, Screenshot, Config, Log, Certificate

        public string AllowedFileTypes { get; set; } = ".pdf,.doc,.docx,.xlsx,.png,.jpg"; // Comma-separated

        public int MaxFileSizeMB { get; set; } = 10;

        public bool RequiresApproval { get; set; } = true;
        public int MinScore { get; set; } = 0; // Minimum score to accept (0-100)

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
