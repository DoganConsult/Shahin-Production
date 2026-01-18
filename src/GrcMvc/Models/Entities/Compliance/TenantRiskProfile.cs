using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Compliance
{
    /// <summary>
    /// Layer 35: Tenant's risk profile.
    /// Captures data types, transfer patterns, and risk characteristics.
    /// Used to determine overlay applicability and control prioritization.
    /// </summary>
    public class TenantRiskProfile : BaseEntity
    {
        // ============================================================================
        // DATA TYPE RISK FACTORS
        // ============================================================================

        /// <summary>
        /// Does tenant process personal data (PII)?
        /// </summary>
        public bool ProcessesPII { get; set; } = false;

        /// <summary>
        /// Volume of PII records: LOW (< 10K), MEDIUM (10K-100K), HIGH (> 100K)
        /// </summary>
        [MaxLength(20)]
        public string? PIIVolume { get; set; }

        /// <summary>
        /// Does tenant process payment card data (PCI)?
        /// </summary>
        public bool ProcessesPCI { get; set; } = false;

        /// <summary>
        /// PCI scope level: SAQ-A, SAQ-A-EP, SAQ-D, ROC
        /// </summary>
        [MaxLength(20)]
        public string? PCIScopeLevel { get; set; }

        /// <summary>
        /// Does tenant process protected health information (PHI)?
        /// </summary>
        public bool ProcessesPHI { get; set; } = false;

        /// <summary>
        /// Does tenant process government classified data?
        /// </summary>
        public bool ProcessesClassifiedData { get; set; } = false;

        /// <summary>
        /// Classification level: CONFIDENTIAL, SECRET, TOP_SECRET
        /// </summary>
        [MaxLength(20)]
        public string? ClassificationLevel { get; set; }

        /// <summary>
        /// Data types processed (JSON array)
        /// </summary>
        public string DataTypesJson { get; set; } = "[]";

        // ============================================================================
        // DATA TRANSFER RISK FACTORS
        // ============================================================================

        /// <summary>
        /// Does tenant transfer data across borders?
        /// </summary>
        public bool HasCrossBorderTransfers { get; set; } = false;

        /// <summary>
        /// Countries data is transferred to (JSON array)
        /// </summary>
        public string TransferCountriesJson { get; set; } = "[]";

        /// <summary>
        /// Does tenant share data with third parties?
        /// </summary>
        public bool HasThirdPartySharing { get; set; } = false;

        /// <summary>
        /// Third parties data is shared with (JSON array)
        /// </summary>
        public string ThirdPartiesJson { get; set; } = "[]";

        // ============================================================================
        // OPERATIONAL RISK FACTORS
        // ============================================================================

        /// <summary>
        /// Is tenant critical infrastructure?
        /// </summary>
        public bool IsCriticalInfrastructure { get; set; } = false;

        /// <summary>
        /// Critical infrastructure sector (if applicable)
        /// </summary>
        [MaxLength(50)]
        public string? CriticalInfrastructureSector { get; set; }

        /// <summary>
        /// Customer volume tier: LOW, MEDIUM, HIGH, VERY_HIGH
        /// </summary>
        [MaxLength(20)]
        public string? CustomerVolumeTier { get; set; }

        /// <summary>
        /// Transaction volume tier: LOW, MEDIUM, HIGH, VERY_HIGH
        /// </summary>
        [MaxLength(20)]
        public string? TransactionVolumeTier { get; set; }

        /// <summary>
        /// Has internet-facing systems?
        /// </summary>
        public bool HasInternetFacing { get; set; } = false;

        /// <summary>
        /// Internet-facing systems (JSON array)
        /// </summary>
        public string InternetFacingSystemsJson { get; set; } = "[]";

        /// <summary>
        /// Uses cloud services?
        /// </summary>
        public bool UsesCloud { get; set; } = false;

        /// <summary>
        /// Cloud providers used (JSON array)
        /// </summary>
        public string CloudProvidersJson { get; set; } = "[]";

        /// <summary>
        /// Uses operational technology (OT)?
        /// </summary>
        public bool UsesOT { get; set; } = false;

        /// <summary>
        /// OT systems (JSON array)
        /// </summary>
        public string OTSystemsJson { get; set; } = "[]";

        // ============================================================================
        // RISK SCORING
        // ============================================================================

        /// <summary>
        /// Overall risk score (0-100)
        /// </summary>
        public decimal OverallRiskScore { get; set; } = 0;

        /// <summary>
        /// Risk tier: LOW, MEDIUM, HIGH, CRITICAL
        /// </summary>
        [MaxLength(20)]
        public string RiskTier { get; set; } = "MEDIUM";

        /// <summary>
        /// Risk score breakdown (JSON object)
        /// </summary>
        public string RiskScoreBreakdownJson { get; set; } = "{}";

        /// <summary>
        /// When risk profile was last calculated
        /// </summary>
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reference to rules evaluation that calculated the score
        /// </summary>
        public Guid? RulesEvaluationLogId { get; set; }

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Risk tiers
    /// </summary>
    public static class RiskTiers
    {
        public const string Low = "LOW";
        public const string Medium = "MEDIUM";
        public const string High = "HIGH";
        public const string Critical = "CRITICAL";
    }
}
