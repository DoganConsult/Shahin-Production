using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Evidence Scoring Criteria - Defines how evidence is scored for each control type
/// Layer 1: Global (Platform) - Used by all tenants
/// </summary>
public class EvidenceScoringCriteria : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string EvidenceTypeCode { get; set; } = string.Empty; // e.g., "POLICY_DOC", "PENETRATION_TEST"

    [Required]
    [StringLength(200)]
    public string EvidenceTypeName { get; set; } = string.Empty; // e.g., "Policy Document"

    [StringLength(500)]
    public string DescriptionEn { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionAr { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty; // Document, Screenshot, Config, Log, Certificate, Report

    // Scoring Configuration
    public int BaseScore { get; set; } = 50; // Base score when evidence is uploaded (0-100)
    public int MaxScore { get; set; } = 100; // Maximum achievable score

    // Scoring Criteria (JSON for flexibility)
    public string ScoringRulesJson { get; set; } = "[]"; // Array of scoring rules

    // Quality Criteria
    public int MinimumScore { get; set; } = 70; // Minimum score to be considered "acceptable"
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresExpiry { get; set; } = false; // Some evidence needs validity period
    public int DefaultValidityDays { get; set; } = 365; // Default validity period

    // File Requirements
    public string AllowedFileTypes { get; set; } = ".pdf,.doc,.docx,.xlsx,.png,.jpg,.jpeg"; // Comma-separated
    public int MaxFileSizeMB { get; set; } = 25;
    public bool RequiresDigitalSignature { get; set; } = false;

    // Frequency Requirements
    [StringLength(50)]
    public string CollectionFrequency { get; set; } = "Annual"; // Annual, Quarterly, Monthly, Continuous, OnDemand

    // Framework Applicability (null = all frameworks)
    [StringLength(500)]
    public string ApplicableFrameworks { get; set; } = string.Empty; // Pipe-separated: "NCA-ECC|SAMA-CSF|PDPL"

    // Sector Applicability (null = all sectors)
    [StringLength(500)]
    public string ApplicableSectors { get; set; } = string.Empty; // Pipe-separated: "Banking|Healthcare|Government"

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}

/// <summary>
/// Sector-Framework Index - Fast lookup table for sector → frameworks → controls → evidence
/// Layer 1: Global (Platform) - Pre-computed for fast queries
/// </summary>
[Table("SectorFrameworkIndex")]
[Microsoft.EntityFrameworkCore.Index(nameof(SectorCode), nameof(OrgType), Name = "IX_SectorFrameworkIndex_Sector_OrgType")]
[Microsoft.EntityFrameworkCore.Index(nameof(SectorCode), nameof(FrameworkCode), Name = "IX_SectorFrameworkIndex_Sector_Framework")]
[Microsoft.EntityFrameworkCore.Index(nameof(FrameworkCode), Name = "IX_SectorFrameworkIndex_Framework")]
public class SectorFrameworkIndex : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string SectorCode { get; set; } = string.Empty; // e.g., "BANKING", "HEALTHCARE", "GOVERNMENT"

    [Required]
    [StringLength(100)]
    public string SectorNameEn { get; set; } = string.Empty;

    [StringLength(100)]
    public string SectorNameAr { get; set; } = string.Empty;

    [StringLength(50)]
    public string OrgType { get; set; } = string.Empty; // e.g., "BANK", "HOSPITAL", "MINISTRY"

    [StringLength(100)]
    public string OrgTypeNameEn { get; set; } = string.Empty;

    [StringLength(100)]
    public string OrgTypeNameAr { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FrameworkCode { get; set; } = string.Empty; // e.g., "NCA-ECC", "SAMA-CSF"

    [StringLength(200)]
    public string FrameworkNameEn { get; set; } = string.Empty;

    public int Priority { get; set; } = 1; // 1 = highest priority
    public bool IsMandatory { get; set; } = true;

    [StringLength(500)]
    public string ReasonEn { get; set; } = string.Empty; // Why this framework applies
    
    [StringLength(500)]
    public string ReasonAr { get; set; } = string.Empty;

    // Control counts (pre-computed for fast display)
    public int ControlCount { get; set; } = 0;
    public int CriticalControlCount { get; set; } = 0;

    // Evidence requirements summary (pre-computed)
    public string EvidenceTypesJson { get; set; } = "[]"; // Pre-computed list of required evidence types
    public int EvidenceTypeCount { get; set; } = 0;

    // Scoring weights
    public double ScoringWeight { get; set; } = 1.0; // Weight in overall compliance score

    // Implementation guidance
    public int EstimatedImplementationDays { get; set; } = 90;
    
    [StringLength(1000)]
    public string ImplementationGuidanceEn { get; set; } = string.Empty;

    // Regulatory deadlines (for display)
    public string DeadlinesJson { get; set; } = "[]"; // Array of deadline objects

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Cache control
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    public string ComputedHash { get; set; } = string.Empty; // For cache invalidation
}

/// <summary>
/// Tenant Evidence Requirements - Auto-generated for each tenant based on their sector/org type
/// Layer 2: Tenant (Customer-specific)
/// </summary>
public class TenantEvidenceRequirement : BaseEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string EvidenceTypeCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string EvidenceTypeName { get; set; } = string.Empty;

    [StringLength(50)]
    public string FrameworkCode { get; set; } = string.Empty;

    [StringLength(50)]
    public string ControlNumber { get; set; } = string.Empty; // Which control needs this evidence

    // Copied from EvidenceScoringCriteria for fast access
    public int MinimumScore { get; set; } = 70;
    
    [StringLength(50)]
    public string CollectionFrequency { get; set; } = "Annual";
    
    public int DefaultValidityDays { get; set; } = 365;

    // Status tracking
    [StringLength(30)]
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Submitted, Approved, Rejected, Expired

    public DateTime? DueDate { get; set; }
    public DateTime? LastSubmittedDate { get; set; }
    public DateTime? LastApprovedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int CurrentScore { get; set; } = 0;

    // Ownership
    public Guid? AssignedToUserId { get; set; }
    public Guid? WorkspaceId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
