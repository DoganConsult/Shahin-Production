using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Base class for all GRC lookup/reference tables
    /// </summary>
    public abstract class GrcLookupBase
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string NameEn { get; set; } = string.Empty;

        [MaxLength(255)]
        public string NameAr { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Countries - ISO 3166-1 standard
    /// </summary>
    [Table("LookupCountries")]
    public class LookupCountry : GrcLookupBase
    {
        [MaxLength(2)]
        public string Iso2Code { get; set; } = string.Empty; // SA, AE, US

        [MaxLength(3)]
        public string Iso3Code { get; set; } = string.Empty; // SAU, ARE, USA

        [MaxLength(10)]
        public string PhoneCode { get; set; } = string.Empty; // +966, +971

        [MaxLength(10)]
        public string Currency { get; set; } = string.Empty; // SAR, AED, USD

        [MaxLength(50)]
        public string Region { get; set; } = string.Empty; // GCC, MENA, Europe

        public bool IsGccCountry { get; set; } = false;

        public bool RequiresDataLocalization { get; set; } = false;
    }

    /// <summary>
    /// Industry Sectors - aligned with GOSI, ISIC, NAICS
    /// </summary>
    [Table("LookupSectors")]
    public class LookupSector : GrcLookupBase
    {
        [MaxLength(10)]
        public string GosiCode { get; set; } = string.Empty; // GOSI sector code

        [MaxLength(10)]
        public string IsicCode { get; set; } = string.Empty; // International Standard Industrial Classification

        [MaxLength(10)]
        public string NaicsCode { get; set; } = string.Empty; // North American Industry Classification

        public int? ParentSectorId { get; set; }

        [ForeignKey("ParentSectorId")]
        public LookupSector? ParentSector { get; set; }

        [MaxLength(100)]
        public string PrimaryRegulatorCode { get; set; } = string.Empty; // SAMA, NCA, etc.

        public bool IsCriticalInfrastructure { get; set; } = false;

        public bool RequiresStricterCompliance { get; set; } = false;

        // AI recommendation hints
        [MaxLength(500)]
        public string RecommendedFrameworks { get; set; } = string.Empty; // JSON array of framework codes

        [MaxLength(500)]
        public string TypicalRisks { get; set; } = string.Empty; // JSON array of risk categories
    }

    /// <summary>
    /// Organization Types
    /// </summary>
    [Table("LookupOrganizationTypes")]
    public class LookupOrganizationType : GrcLookupBase
    {
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Public, Private, NonProfit, Government

        public bool IsRegulatedEntity { get; set; } = false;

        public bool RequiresAudit { get; set; } = false;

        [MaxLength(500)]
        public string ApplicableRegulations { get; set; } = string.Empty; // JSON array
    }

    /// <summary>
    /// Regulators - government bodies that issue regulations
    /// </summary>
    [Table("LookupRegulators")]
    public class LookupRegulator : GrcLookupBase
    {
        [MaxLength(2)]
        public string CountryCode { get; set; } = string.Empty; // SA, AE

        [MaxLength(255)]
        public string FullNameEn { get; set; } = string.Empty;

        [MaxLength(255)]
        public string FullNameAr { get; set; } = string.Empty;

        [MaxLength(500)]
        public string WebsiteUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string RegulatedSectors { get; set; } = string.Empty; // JSON array of sector codes

        [MaxLength(500)]
        public string IssuedFrameworks { get; set; } = string.Empty; // JSON array of framework codes
    }

    /// <summary>
    /// Compliance Frameworks - standards, regulations, best practices
    /// </summary>
    [Table("LookupFrameworks")]
    public class LookupFramework : GrcLookupBase
    {
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Regulation, Standard, BestPractice, Certification

        [MaxLength(100)]
        public string IssuingBody { get; set; } = string.Empty; // SAMA, NCA, ISO, NIST

        [MaxLength(2)]
        public string CountryCode { get; set; } = string.Empty; // SA for local, empty for international

        [MaxLength(50)]
        public string Version { get; set; } = string.Empty; // 2.0, 2023

        public DateTime? EffectiveDate { get; set; }

        public DateTime? SunsetDate { get; set; }

        public bool IsMandatory { get; set; } = false;

        public int Priority { get; set; } = 5; // 1=highest, 10=lowest

        [MaxLength(500)]
        public string ApplicableSectors { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string Prerequisites { get; set; } = string.Empty; // JSON array of framework codes

        // For AI recommendations
        public int EstimatedControlCount { get; set; } = 0;

        public int EstimatedImplementationMonths { get; set; } = 0;

        [MaxLength(50)]
        public string ComplexityLevel { get; set; } = "Medium"; // Low, Medium, High, VeryHigh
    }

    /// <summary>
    /// Organization Size categories
    /// </summary>
    [Table("LookupOrganizationSizes")]
    public class LookupOrganizationSize : GrcLookupBase
    {
        public int MinEmployees { get; set; } = 0;

        public int MaxEmployees { get; set; } = 0;

        public decimal? MinRevenue { get; set; }

        public decimal? MaxRevenue { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "SAR";

        // Recommended GRC approach
        [MaxLength(50)]
        public string RecommendedApproach { get; set; } = string.Empty; // Simplified, Standard, Comprehensive

        public int RecommendedTeamSize { get; set; } = 1;
    }

    /// <summary>
    /// Compliance Maturity Levels - CMMI-based
    /// </summary>
    [Table("LookupMaturityLevels")]
    public class LookupMaturityLevel : GrcLookupBase
    {
        public int Level { get; set; } = 1; // 1-5 CMMI scale

        [MaxLength(500)]
        public string Characteristics { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string RecommendedActions { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string NextLevelRequirements { get; set; } = string.Empty; // JSON array
    }

    /// <summary>
    /// Data Classification Types
    /// </summary>
    [Table("LookupDataTypes")]
    public class LookupDataType : GrcLookupBase
    {
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Personal, Financial, Health, Biometric

        [MaxLength(50)]
        public string SensitivityLevel { get; set; } = "Medium"; // Public, Internal, Confidential, Restricted

        public bool RequiresEncryption { get; set; } = false;

        public bool RequiresConsent { get; set; } = false;

        public bool SubjectToRetention { get; set; } = false;

        public int DefaultRetentionYears { get; set; } = 7;

        [MaxLength(500)]
        public string ApplicableRegulations { get; set; } = string.Empty; // JSON array (PDPL, GDPR, etc.)
    }

    /// <summary>
    /// Data Hosting Models
    /// </summary>
    [Table("LookupHostingModels")]
    public class LookupHostingModel : GrcLookupBase
    {
        [MaxLength(500)]
        public string SecurityConsiderations { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string ComplianceImplications { get; set; } = string.Empty; // JSON array

        public bool RequiresDataLocalization { get; set; } = false;

        [MaxLength(500)]
        public string RecommendedControls { get; set; } = string.Empty; // JSON array of control codes
    }

    /// <summary>
    /// Risk Categories for quick assessment
    /// </summary>
    [Table("LookupRiskCategories")]
    public class LookupRiskCategory : GrcLookupBase
    {
        [MaxLength(50)]
        public string Domain { get; set; } = string.Empty; // Cyber, Operational, Financial, Legal, Reputational

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public LookupRiskCategory? ParentCategory { get; set; }

        [MaxLength(500)]
        public string TypicalCauses { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string TypicalImpacts { get; set; } = string.Empty; // JSON array

        [MaxLength(500)]
        public string MitigationStrategies { get; set; } = string.Empty; // JSON array
    }

    /// <summary>
    /// Cloud Service Providers
    /// </summary>
    [Table("LookupCloudProviders")]
    public class LookupCloudProvider : GrcLookupBase
    {
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // IaaS, PaaS, SaaS

        public bool HasKsaRegion { get; set; } = false;

        public bool HasGccRegion { get; set; } = false;

        [MaxLength(500)]
        public string Certifications { get; set; } = string.Empty; // JSON array (ISO27001, SOC2, etc.)

        [MaxLength(500)]
        public string DataResidencyOptions { get; set; } = string.Empty; // JSON array of regions
    }

    /// <summary>
    /// Wizard AI Recommendations - stores recommendations for onboarding
    /// </summary>
    [Table("WizardRecommendations")]
    public class WizardRecommendation
    {
        [Key]
        public int Id { get; set; }

        public Guid TenantId { get; set; }

        [MaxLength(50)]
        public string StepCode { get; set; } = string.Empty; // StepA, StepB, etc.

        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string RecommendationType { get; set; } = string.Empty; // Value, Warning, Info, Action

        public string RecommendedValue { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string ReasonEn { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string ReasonAr { get; set; } = string.Empty;

        public int Confidence { get; set; } = 80; // 0-100%

        [MaxLength(50)]
        public string Source { get; set; } = string.Empty; // RulesEngine, AI, UserHistory

        public bool IsAccepted { get; set; } = false;

        public bool IsDismissed { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
