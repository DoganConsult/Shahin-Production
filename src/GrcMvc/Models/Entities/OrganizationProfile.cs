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

        // ===== KSA-SPECIFIC REGULATORY COMPLIANCE =====
        /// <summary>
        /// GOSI registration number - General Organization for Social Insurance
        /// Required for all Saudi employers
        /// </summary>
        public string GosiNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nitaqat category (Saudization program)
        /// Values: Platinum, Green, Yellow, Red, NotApplicable
        /// </summary>
        public string NitaqatCategory { get; set; } = string.Empty;

        /// <summary>
        /// Current Saudization percentage (0-100)
        /// Required by MHRSD (Ministry of Human Resources)
        /// </summary>
        public decimal SaudizationPercent { get; set; } = 0;

        /// <summary>
        /// Commercial Registration (CR) expiry date
        /// MOC (Ministry of Commerce) requirement
        /// </summary>
        public DateTime? CrExpiryDate { get; set; }

        /// <summary>
        /// Zakat certificate expiry date
        /// ZATCA (Zakat, Tax and Customs Authority) requirement
        /// </summary>
        public DateTime? ZakatCertExpiry { get; set; }

        /// <summary>
        /// E-Invoicing Phase 1 (Generation) compliance
        /// ZATCA Fatoorah requirement
        /// </summary>
        public bool HasEInvoicingPhase1 { get; set; } = false;

        /// <summary>
        /// E-Invoicing Phase 2 (Integration) compliance
        /// ZATCA Fatoorah requirement
        /// </summary>
        public bool HasEInvoicingPhase2 { get; set; } = false;

        /// <summary>
        /// SASO (Saudi Standards Organization) certification
        /// Product compliance for manufacturing/import
        /// </summary>
        public string SasoCertification { get; set; } = string.Empty;

        /// <summary>
        /// Municipal license number (Balady)
        /// Required for physical business locations
        /// </summary>
        public string MunicipalLicense { get; set; } = string.Empty;

        /// <summary>
        /// Chamber of Commerce membership number
        /// Business requirement for trade activities
        /// </summary>
        public string ChamberMembership { get; set; } = string.Empty;

        // ===== KSA MARKET REQUIREMENTS =====
        /// <summary>
        /// Listed on Tadawul (Saudi Stock Exchange)
        /// Triggers ESG and IFRS requirements
        /// </summary>
        public bool IsTadawulListed { get; set; } = false;

        /// <summary>
        /// Requires ESG (Environmental, Social, Governance) reporting
        /// Mandatory for Tadawul-listed companies
        /// </summary>
        public bool RequiresEsgReporting { get; set; } = false;

        /// <summary>
        /// IFRS (International Financial Reporting Standards) compliance required
        /// Required for listed and large companies
        /// </summary>
        public bool RequiresIfrsCompliance { get; set; } = false;

        /// <summary>
        /// MISA (Ministry of Investment) license type
        /// For foreign investment entities
        /// Values: Industrial, Service, Trading, Regional HQ, etc.
        /// </summary>
        public string MisaLicenseType { get; set; } = string.Empty;

        /// <summary>
        /// MISA license number for foreign investment
        /// </summary>
        public string MisaLicenseNumber { get; set; } = string.Empty;

        // ===== DATA RESIDENCY & SOVEREIGNTY =====
        /// <summary>
        /// Data must remain within KSA borders
        /// Required for government and critical infrastructure data
        /// </summary>
        public bool RequiresDataLocalization { get; set; } = false;

        /// <summary>
        /// Organization transfers data across borders
        /// Triggers PDPL cross-border transfer requirements
        /// </summary>
        public bool HasCrossBorderTransfer { get; set; } = false;

        /// <summary>
        /// Countries where data is transferred (JSON array)
        /// Required for PDPL compliance reporting
        /// </summary>
        public string DataTransferCountries { get; set; } = string.Empty;

        /// <summary>
        /// National ID (Iqama) data processing
        /// Special PDPL Article 5 requirements
        /// </summary>
        public bool ProcessesNationalIdData { get; set; } = false;

        /// <summary>
        /// Biometric data processing
        /// Special PDPL Article 6 requirements
        /// </summary>
        public bool ProcessesBiometricData { get; set; } = false;

        /// <summary>
        /// Location/GPS data processing
        /// Special PDPL Article 7 requirements
        /// </summary>
        public bool ProcessesLocationData { get; set; } = false;

        // ===== VISION 2030 ALIGNMENT =====
        /// <summary>
        /// Primary Vision 2030 program alignment
        /// e.g., "Quality of Life", "Financial Sector Development", etc.
        /// </summary>
        public string Vision2030Program { get; set; } = string.Empty;

        /// <summary>
        /// Vision 2030 KPIs being tracked (JSON array)
        /// For government reporting requirements
        /// </summary>
        public string Vision2030Kpis { get; set; } = string.Empty;

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
        public string OnboardingStatus { get; set; } = "NOT_STARTED"; // NOT_STARTED, IN_PROGRESS, COMPLETED
        public DateTime? OnboardingStartedAt { get; set; }
        public DateTime? OnboardingCompletedAt { get; set; }
        public string OnboardingCompletedBy { get; set; } = string.Empty;
        public DateTime? LastScopeDerivedAt { get; set; }
        public int OnboardingProgressPercent { get; set; } = 0;

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
