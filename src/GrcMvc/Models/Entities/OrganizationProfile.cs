using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Stores comprehensive organization profile data collected during onboarding.
    /// Layer 2: Tenant Context
    /// Input to Rules Engine for scope derivation
    /// Includes: Legal Entity, Financial, Organization Structure, Compliance data
    /// </summary>
    public class OrganizationProfile : BaseEntity
    {
        public Guid TenantId { get; set; }

        // ===== BASIC ORGANIZATION INFO =====
        public string OrganizationType { get; set; } = string.Empty; // Startup, SMB, Enterprise, Government, etc.
        public string Sector { get; set; } = string.Empty; // Banking, Healthcare, Technology, Telecom, Energy
        public string Country { get; set; } = "SA";
        public string OrganizationSize { get; set; } = string.Empty; // 1-10, 11-50, 51-250, 251-1000, 1000+
        public int EmployeeCount { get; set; } = 0;

        // ===== LEGAL ENTITY INFORMATION =====
        public string LegalEntityName { get; set; } = string.Empty;
        public string LegalEntityNameAr { get; set; } = string.Empty;
        public string CommercialRegistrationNumber { get; set; } = string.Empty; // CR Number (Saudi)
        public string TaxIdentificationNumber { get; set; } = string.Empty; // VAT/Tax ID
        public string LegalEntityType { get; set; } = string.Empty; // LLC, JSC, Branch, Partnership, Sole Proprietorship
        public DateTime? IncorporationDate { get; set; }
        public string RegisteredAddress { get; set; } = string.Empty;
        public string RegisteredCity { get; set; } = string.Empty;
        public string RegisteredRegion { get; set; } = string.Empty; // Province/State
        public string PostalCode { get; set; } = string.Empty;
        public string LegalRepresentativeName { get; set; } = string.Empty;
        public string LegalRepresentativeTitle { get; set; } = string.Empty;
        public string LegalRepresentativeEmail { get; set; } = string.Empty;
        public string LegalRepresentativePhone { get; set; } = string.Empty;

        // ===== FINANCIAL INFORMATION =====
        public string AnnualRevenueRange { get; set; } = string.Empty; // <1M, 1-10M, 10-50M, 50-100M, 100M+
        public string Currency { get; set; } = "SAR";
        public string FiscalYearEnd { get; set; } = string.Empty; // e.g., "December"
        public bool IsPubliclyTraded { get; set; } = false;
        public string StockExchange { get; set; } = string.Empty; // Tadawul, etc.
        public string StockSymbol { get; set; } = string.Empty;
        public bool HasExternalAuditor { get; set; } = false;
        public string ExternalAuditorName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountType { get; set; } = string.Empty;

        // ===== ORGANIZATION STRUCTURE =====
        public string ParentCompanyName { get; set; } = string.Empty;
        public bool IsSubsidiary { get; set; } = false;
        public int SubsidiaryCount { get; set; } = 0;
        public int BranchCount { get; set; } = 0;
        public string OperatingCountries { get; set; } = string.Empty; // JSON array or comma-separated
        public string HeadquartersLocation { get; set; } = string.Empty;
        public string OrganizationStructureJson { get; set; } = "{}"; // Org chart as JSON

        // ===== KEY CONTACTS =====
        public string CeoName { get; set; } = string.Empty;
        public string CeoEmail { get; set; } = string.Empty;
        public string CfoName { get; set; } = string.Empty;
        public string CfoEmail { get; set; } = string.Empty;
        public string CisoName { get; set; } = string.Empty;
        public string CisoEmail { get; set; } = string.Empty;
        public string DpoName { get; set; } = string.Empty; // Data Protection Officer
        public string DpoEmail { get; set; } = string.Empty;
        public string ComplianceOfficerName { get; set; } = string.Empty;
        public string ComplianceOfficerEmail { get; set; } = string.Empty;

        // ===== REGULATORY & COMPLIANCE =====
        public string RegulatoryCertifications { get; set; } = string.Empty; // ISO27001, SOC2, etc.
        public string IndustryLicenses { get; set; } = string.Empty; // SAMA, CMA, CITC licenses
        public string PrimaryRegulator { get; set; } = string.Empty; // NCA, SAMA, CITC, etc.
        public string SecondaryRegulators { get; set; } = string.Empty; // JSON array
        public bool IsRegulatedEntity { get; set; } = false;
        public bool IsCriticalInfrastructure { get; set; } = false;
        public string ComplianceMaturity { get; set; } = "Initial"; // Initial, Repeatable, Defined, Managed, Optimized

        // ===== DATA & TECHNOLOGY =====
        public string DataTypes { get; set; } = string.Empty; // PersonalData, FinancialData, HealthData, etc.
        public string HostingModel { get; set; } = string.Empty; // OnPremise, PublicCloud, HybridCloud, Private
        public string CloudProviders { get; set; } = string.Empty; // AWS, Azure, GCP, Alibaba, etc.
        public bool ProcessesPersonalData { get; set; } = false;
        public bool ProcessesSensitiveData { get; set; } = false;
        public bool HasDataCenterInKSA { get; set; } = false;
        public int DataSubjectCount { get; set; } = 0; // Approximate number of data subjects
        public string ItSystemsJson { get; set; } = "[]"; // Key IT systems

        // ===== THIRD PARTIES & VENDORS =====
        public string Vendors { get; set; } = string.Empty;
        public int VendorCount { get; set; } = 0;
        public int CriticalVendorCount { get; set; } = 0;
        public bool HasThirdPartyDataProcessing { get; set; } = false;
        public string ThirdPartyRiskLevel { get; set; } = string.Empty; // Low, Medium, High

        // ===== ONBOARDING METADATA =====
        public string OnboardingQuestionsJson { get; set; } = string.Empty; // Complete answers for audit
        public string OnboardingStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public DateTime? OnboardingStartedAt { get; set; }
        public DateTime? OnboardingCompletedAt { get; set; }
        public string OnboardingCompletedBy { get; set; } = string.Empty;
        public DateTime? LastScopeDerivedAt { get; set; }
        public int OnboardingProgressPercent { get; set; } = 0;

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
