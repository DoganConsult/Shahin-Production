using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Complete onboarding wizard state containing all sections A-L.
    /// Supports progressive save with section-by-section completion.
    /// </summary>
    public class OnboardingWizardStateDto
    {
        public Guid TenantId { get; set; }
        public string WizardStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 12;
        public int ProgressPercent { get; set; } = 0;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; } = string.Empty;

        // Sections A-L
        public SectionA_OrganizationIdentity OrganizationIdentity { get; set; } = new();
        public SectionB_AssuranceObjective AssuranceObjective { get; set; } = new();
        public SectionC_RegulatoryApplicability RegulatoryApplicability { get; set; } = new();
        public SectionD_ScopeDefinition ScopeDefinition { get; set; } = new();
        public SectionE_DataRiskProfile DataRiskProfile { get; set; } = new();
        public SectionF_TechnologyLandscape TechnologyLandscape { get; set; } = new();
        public SectionG_ControlOwnership ControlOwnership { get; set; } = new();
        public SectionH_TeamsRolesAccess TeamsRolesAccess { get; set; } = new();
        public SectionI_WorkflowCadence WorkflowCadence { get; set; } = new();
        public SectionJ_EvidenceStandards EvidenceStandards { get; set; } = new();
        public SectionK_BaselineOverlays BaselineOverlays { get; set; } = new();
        public SectionL_GoLiveMetrics GoLiveMetrics { get; set; } = new();

        // Section completion tracking
        public Dictionary<string, bool> SectionCompleted { get; set; } = new()
        {
            ["A"] = false, ["B"] = false, ["C"] = false, ["D"] = false,
            ["E"] = false, ["F"] = false, ["G"] = false, ["H"] = false,
            ["I"] = false, ["J"] = false, ["K"] = false, ["L"] = false
        };
    }

    #region Section A: Organization Identity and Tenancy (Questions 1-13)

    /// <summary>
    /// Section A: Organization Identity and Tenancy
    /// Questions 1-13 - Core organization identification
    /// </summary>
    public class SectionA_OrganizationIdentity
    {
        // Q1: Organization legal name (English/Arabic) - REQUIRED
        [Required(ErrorMessage = "Organization legal name is required")]
        public string LegalNameEn { get; set; } = string.Empty;
        public string LegalNameAr { get; set; } = string.Empty;

        // Q2: Trade name / brand (if different)
        public string TradeName { get; set; } = string.Empty;
        public string TradeNameAr { get; set; } = string.Empty;

        // Q3: Country of incorporation - REQUIRED
        [Required(ErrorMessage = "Country of incorporation is required")]
        public string CountryOfIncorporation { get; set; } = "SA";

        // Q4: Operating countries/jurisdictions (list)
        public List<string> OperatingCountries { get; set; } = new();

        // Q5: Primary HQ location - REQUIRED
        [Required(ErrorMessage = "Primary HQ location is required")]
        public string PrimaryHqLocation { get; set; } = string.Empty;

        // Q6: Timezone (default) - REQUIRED
        [Required(ErrorMessage = "Timezone is required")]
        public string Timezone { get; set; } = "Asia/Riyadh";

        // Q7: Primary language (English / Arabic / bilingual) - REQUIRED
        [Required(ErrorMessage = "Primary language is required")]
        public string PrimaryLanguage { get; set; } = "English"; // English, Arabic, Bilingual

        // Q8: Corporate email domain(s) - REQUIRED
        [Required(ErrorMessage = "At least one corporate email domain is required")]
        public List<string> CorporateEmailDomains { get; set; } = new();

        // Q9: Domain verification method
        public string DomainVerificationMethod { get; set; } = "DnsTxt"; // DnsTxt, AdminEmailConfirmation
        public bool DomainVerified { get; set; } = false;

        // Q10: Org type - REQUIRED
        [Required(ErrorMessage = "Organization type is required")]
        public string OrgType { get; set; } = string.Empty; // Enterprise, SME, Government, RegulatedFinancialInstitution, Fintech, Telecom, Other

        // Q11: Industry/sector - REQUIRED
        [Required(ErrorMessage = "Industry/sector is required")]
        public List<string> Sectors { get; set; } = new(); // Banking, Insurance, Healthcare, Technology, Energy, Telecom, Retail, Manufacturing, Government, Other

        // Q12: Business lines (optional)
        public List<string> BusinessLines { get; set; } = new(); // Retail, Corporate, Payments, Lending, Wealth, TelecomServices, etc.

        // Q13: Data residency requirement - REQUIRED
        public bool HasDataResidencyRequirement { get; set; } = false;
        public List<string> DataResidencyCountries { get; set; } = new();

        public bool IsComplete => !string.IsNullOrEmpty(LegalNameEn) &&
                                   !string.IsNullOrEmpty(CountryOfIncorporation) &&
                                   !string.IsNullOrEmpty(PrimaryHqLocation) &&
                                   !string.IsNullOrEmpty(Timezone) &&
                                   !string.IsNullOrEmpty(PrimaryLanguage) &&
                                   CorporateEmailDomains.Count > 0 &&
                                   !string.IsNullOrEmpty(OrgType) &&
                                   Sectors.Count > 0;
    }

    #endregion

    #region Section B: Assurance Objective (Questions 14-18)

    /// <summary>
    /// Section B: Assurance Objective (Why are we doing this?)
    /// Questions 14-18 - Business drivers and goals
    /// </summary>
    public class SectionB_AssuranceObjective
    {
        // Q14: Primary driver
        public string PrimaryDriver { get; set; } = string.Empty; // RegulatorExam, InternalAudit, ExternalAudit, Certification, CustomerDueDiligence, BoardReporting

        // Q15: Target timeline (e.g., go-live date or audit date)
        public DateTime? TargetDate { get; set; }
        public string TargetMilestone { get; set; } = string.Empty; // GoLive, AuditDate, CertificationDate, RegulatoryExam

        // Q16: Current pain (rank top 3)
        public List<PainPointRanking> CurrentPainPoints { get; set; } = new();

        // Q17: Desired maturity
        public string DesiredMaturity { get; set; } = "Foundation"; // Foundation, AssuranceOps, ContinuousAssurance

        // Q18: Reporting audience
        public List<string> ReportingAudience { get; set; } = new(); // Board, CRO, CIO, CISO, InternalAudit, Regulators

        public bool IsComplete => !string.IsNullOrEmpty(PrimaryDriver);
    }

    public class PainPointRanking
    {
        public string PainPoint { get; set; } = string.Empty; // EvidenceCollection, MappingFrameworks, RemediationDelays, Outages, VendorRisk, FraudLeakage
        public int Rank { get; set; } // 1, 2, or 3
    }

    #endregion

    #region Section C: Regulatory and Framework Applicability (Questions 19-25)

    /// <summary>
    /// Section C: Regulatory and Framework Applicability (What must apply?)
    /// Questions 19-25 - Regulatory context
    /// </summary>
    public class SectionC_RegulatoryApplicability
    {
        // Q19: Primary regulator(s) (per jurisdiction)
        public List<RegulatorByJurisdiction> PrimaryRegulators { get; set; } = new();

        // Q20: Secondary regulators (if any)
        public List<string> SecondaryRegulators { get; set; } = new();

        // Q21: Mandatory frameworks (if already defined internally)
        public List<string> MandatoryFrameworks { get; set; } = new(); // NCA-ECC, SAMA-CSF, PDPL, ISO27001, PCI-DSS, etc.

        // Q22: Optional frameworks used for benchmarking (if any)
        public List<string> BenchmarkingFrameworks { get; set; } = new();

        // Q23: Internal policies/standards to align with
        public List<string> InternalPolicies { get; set; } = new(); // ISMS, IT Policies, Risk Policies

        // Q24: Certifications held
        public List<CertificationHeld> CertificationsHeld { get; set; } = new();

        // Q25: Audit scope type
        public string AuditScopeType { get; set; } = "Enterprise"; // Enterprise, BusinessUnit, System, Process, Vendor

        public bool IsComplete => PrimaryRegulators.Count > 0 || MandatoryFrameworks.Count > 0;
    }

    public class RegulatorByJurisdiction
    {
        public string Jurisdiction { get; set; } = string.Empty; // SA, AE, BH, etc.
        public string RegulatorCode { get; set; } = string.Empty; // NCA, SAMA, CITC, CMA, etc.
        public string RegulatorName { get; set; } = string.Empty;
    }

    public class CertificationHeld
    {
        public string CertificationType { get; set; } = string.Empty; // ISO27001, ISO22301, SOC2Type1, SOC2Type2, PCIDSS, etc.
        public DateTime? CertificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CertifyingBody { get; set; } = string.Empty;
    }

    #endregion

    #region Section D: Scope Definition (Questions 26-34)

    /// <summary>
    /// Section D: Scope Definition (What is "in scope"?)
    /// Questions 26-34 - Define assessment boundaries
    /// </summary>
    public class SectionD_ScopeDefinition
    {
        // Q26: In-scope legal entities (list) - REQUIRED
        [Required(ErrorMessage = "At least one in-scope legal entity is required")]
        public List<LegalEntityScope> InScopeLegalEntities { get; set; } = new();

        // Q27: In-scope business units (list) - REQUIRED
        public List<BusinessUnitScope> InScopeBusinessUnits { get; set; } = new();

        // Q28: In-scope systems/applications (list or import) - REQUIRED
        public List<SystemScope> InScopeSystems { get; set; } = new();

        // Q29: In-scope processes - REQUIRED
        public List<string> InScopeProcesses { get; set; } = new(); // Onboarding, Payments, P2P, ChangeMgmt, IncidentResponse, etc.

        // Q30: In-scope environments
        public List<string> InScopeEnvironments { get; set; } = new() { "Production" }; // Production, NonProduction, Both

        // Q31: In-scope locations/data centers/cloud regions - REQUIRED
        public List<LocationScope> InScopeLocations { get; set; } = new();

        // Q32: System criticality tiers - REQUIRED
        public List<CriticalityTierDefinition> CriticalityTiers { get; set; } = new()
        {
            new() { TierLevel = 1, TierName = "Critical", Description = "Mission-critical systems with immediate business impact", RtoHours = 4, RpoHours = 1 },
            new() { TierLevel = 2, TierName = "High", Description = "Important systems with significant business impact", RtoHours = 24, RpoHours = 4 },
            new() { TierLevel = 3, TierName = "Medium", Description = "Standard business systems", RtoHours = 72, RpoHours = 24 }
        };

        // Q33: "Important business services" (if resilience program)
        public List<ImportantBusinessService> ImportantBusinessServices { get; set; } = new();

        // Q34: Exclusions (explicitly out of scope) with rationale - REQUIRED
        public List<ScopeExclusion> Exclusions { get; set; } = new();

        public bool IsComplete => InScopeLegalEntities.Count > 0 &&
                                   InScopeBusinessUnits.Count > 0 &&
                                   InScopeSystems.Count > 0 &&
                                   InScopeProcesses.Count > 0 &&
                                   InScopeLocations.Count > 0;
    }

    public class LegalEntityScope
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityNameAr { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
    }

    public class BusinessUnitScope
    {
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string ParentUnit { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }

    public class SystemScope
    {
        public string SystemCode { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty; // Application, Database, Infrastructure, Network, Cloud
        public int CriticalityTier { get; set; } = 2;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string HostingLocation { get; set; } = string.Empty;
        public bool ProcessesPersonalData { get; set; } = false;
        public bool ProcessesPaymentData { get; set; } = false;
    }

    public class LocationScope
    {
        public string LocationType { get; set; } = string.Empty; // DataCenter, Office, CloudRegion
        public string LocationName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CloudProvider { get; set; } = string.Empty;
        public string CloudRegion { get; set; } = string.Empty;
    }

    public class CriticalityTierDefinition
    {
        public int TierLevel { get; set; }
        public string TierName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RtoHours { get; set; }
        public int RpoHours { get; set; }
    }

    public class ImportantBusinessService
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public int CriticalityTier { get; set; } = 1;
    }

    public class ScopeExclusion
    {
        public string ExclusionType { get; set; } = string.Empty; // LegalEntity, BusinessUnit, System, Process, Location
        public string ExcludedItem { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }
    }

    #endregion

    #region Section E: Data and Risk Profile (Questions 35-40)

    /// <summary>
    /// Section E: Data and Risk Profile (Drives overlays and evidence depth)
    /// Questions 35-40 - Data classification and risk context
    /// </summary>
    public class SectionE_DataRiskProfile
    {
        // Q35: Data types processed - REQUIRED
        [Required(ErrorMessage = "Data types processed is required")]
        public List<string> DataTypesProcessed { get; set; } = new(); // PII, PCI, PHI, Confidential, Classified

        // Q36: Payment card data present? - REQUIRED
        public bool HasPaymentCardData { get; set; } = false;
        public List<string> PaymentCardDataSystems { get; set; } = new();
        public List<string> PaymentCardDataProcesses { get; set; } = new();

        // Q37: Cross-border data transfers? - REQUIRED
        public bool HasCrossBorderTransfers { get; set; } = false;
        public List<CrossBorderTransfer> CrossBorderTransfers { get; set; } = new();

        // Q38: Customer volume / transaction volume tier (optional)
        public string CustomerVolumeTier { get; set; } = string.Empty; // Small(<10K), Medium(10K-100K), Large(100K-1M), Enterprise(1M+)
        public string TransactionVolumeTier { get; set; } = string.Empty; // Low, Medium, High, VeryHigh

        // Q39: Internet-facing systems? - REQUIRED
        public bool HasInternetFacingSystems { get; set; } = false;
        public List<string> InternetFacingSystems { get; set; } = new();

        // Q40: Third-party processing of sensitive data?
        public bool HasThirdPartyDataProcessing { get; set; } = false;
        public List<ThirdPartyDataProcessor> ThirdPartyProcessors { get; set; } = new();

        public bool IsComplete => DataTypesProcessed.Count > 0;
    }

    public class CrossBorderTransfer
    {
        public string SourceCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public string TransferMechanism { get; set; } = string.Empty; // Adequacy, SCCs, BCRs, Consent
    }

    public class ThirdPartyDataProcessor
    {
        public string VendorName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string ProcessingLocation { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
    }

    #endregion

    #region Section F: Technology Landscape (Questions 41-53)

    /// <summary>
    /// Section F: Technology Landscape (Integration readiness)
    /// Questions 41-53 - Technology stack and integrations
    /// </summary>
    public class SectionF_TechnologyLandscape
    {
        // Q41: Identity provider - REQUIRED
        [Required(ErrorMessage = "Identity provider is required")]
        public string IdentityProvider { get; set; } = string.Empty; // AzureAD, Okta, PingIdentity, ADFS, Other

        // Q42: SSO enabled? - REQUIRED
        public bool SsoEnabled { get; set; } = false;
        public string SsoProtocol { get; set; } = string.Empty; // SAML, OIDC

        // Q43: SCIM provisioning available?
        public bool ScimEnabled { get; set; } = false;

        // Q44: ITSM/ticketing - REQUIRED
        [Required(ErrorMessage = "ITSM/ticketing system is required")]
        public string ItsmPlatform { get; set; } = string.Empty; // ServiceNow, Jira, Freshservice, Other

        // Q45: Evidence repository - REQUIRED
        public string EvidenceRepository { get; set; } = string.Empty; // SharePoint, GrcVault, GoogleDrive, Confluence, Other

        // Q46: SIEM/SOC - REQUIRED
        public string SiemPlatform { get; set; } = string.Empty; // Sentinel, Splunk, QRadar, Elastic, Other

        // Q47: Vulnerability management - REQUIRED
        public string VulnerabilityManagement { get; set; } = string.Empty; // Tenable, Qualys, Rapid7, Other

        // Q48: EDR - REQUIRED
        public string EdrPlatform { get; set; } = string.Empty; // Defender, CrowdStrike, SentinelOne, CarbonBlack, Other

        // Q49: Cloud providers - REQUIRED
        [Required(ErrorMessage = "Cloud provider information is required")]
        public List<string> CloudProviders { get; set; } = new(); // AWS, Azure, GCP, AliCloud, OCI, OnPremise, Hybrid

        // Q50: ERP - REQUIRED
        public string ErpPlatform { get; set; } = string.Empty; // SAP, Oracle, Dynamics, ERPNext, Other, None

        // Q51: CMDB/asset inventory source
        public string CmdbSource { get; set; } = string.Empty; // ServiceNowCMDB, CloudInventory, Custom, None

        // Q52: CI/CD tooling (if SDLC controls matter)
        public List<string> CiCdTools { get; set; } = new(); // GitHub, GitLab, AzureDevOps, Jenkins, Other

        // Q53: Backup/DR tooling (optional but recommended)
        public string BackupDrTooling { get; set; } = string.Empty;

        // Additional integration details
        public List<IntegrationConfig> IntegrationConfigs { get; set; } = new();

        public bool IsComplete => !string.IsNullOrEmpty(IdentityProvider) &&
                                   !string.IsNullOrEmpty(ItsmPlatform) &&
                                   CloudProviders.Count > 0;
    }

    public class IntegrationConfig
    {
        public string IntegrationType { get; set; } = string.Empty; // SSO, SCIM, ITSM, Evidence, SIEM, VulnMgmt, EDR, CMDB
        public string Platform { get; set; } = string.Empty;
        public string ConnectionStatus { get; set; } = "NotConfigured"; // NotConfigured, Pending, Connected, Failed
        public string ApiEndpoint { get; set; } = string.Empty;
        public bool AutomatedEvidenceCollection { get; set; } = false;
    }

    #endregion

    #region Section G: Control Ownership Model (Questions 54-60)

    /// <summary>
    /// Section G: Control Ownership Model (Who owns what?)
    /// Questions 54-60 - Ownership and accountability
    /// </summary>
    public class SectionG_ControlOwnership
    {
        // Q54: Control ownership approach
        public string OwnershipApproach { get; set; } = "Federated"; // Centralized, Federated, Hybrid

        // Q55: Default control owner team (if not specified per system)
        public string DefaultOwnerTeam { get; set; } = string.Empty;
        public string DefaultOwnerEmail { get; set; } = string.Empty;

        // Q56: Who approves exceptions (role/title)
        public string ExceptionApproverRole { get; set; } = string.Empty;
        public string ExceptionApproverTitle { get; set; } = string.Empty;

        // Q57: Who approves regulatory interpretations/mappings (role/title)
        public string RegulatoryApproverRole { get; set; } = string.Empty;
        public string RegulatoryApproverTitle { get; set; } = string.Empty;

        // Q58: Who signs off control effectiveness (role/title)
        public string EffectivenessSignoffRole { get; set; } = string.Empty;
        public string EffectivenessSignoffTitle { get; set; } = string.Empty;

        // Q59: Internal audit stakeholder (name/role)
        public string InternalAuditContact { get; set; } = string.Empty;
        public string InternalAuditEmail { get; set; } = string.Empty;
        public string InternalAuditRole { get; set; } = string.Empty;

        // Q60: Risk committee cadence and attendees
        public string RiskCommitteeCadence { get; set; } = "Quarterly"; // Weekly, Monthly, Quarterly, Annually
        public List<string> RiskCommitteeRoles { get; set; } = new(); // CEO, CFO, CRO, CISO, CIO, etc.

        public bool IsComplete => !string.IsNullOrEmpty(OwnershipApproach);
    }

    #endregion

    #region Section H: Teams, Roles, and Access (Questions 61-70)

    /// <summary>
    /// Section H: Teams, Roles, and Access (So workflow "recognizes" people)
    /// Questions 61-70 - User and team setup
    /// </summary>
    public class SectionH_TeamsRolesAccess
    {
        // Q61: Org admin(s): name + email - REQUIRED
        [Required(ErrorMessage = "At least one org admin is required")]
        public List<OrgAdmin> OrgAdmins { get; set; } = new();

        // Q62: Create teams now? (yes/no) If yes: team list
        public bool CreateTeamsNow { get; set; } = false;

        // Q63: For each team: team owner + backup owner - REQUIRED if CreateTeamsNow
        public List<WizardTeamDefinition> Teams { get; set; } = new();

        // Q64: Add team members: name + email + role
        public List<TeamMemberDefinition> TeamMembers { get; set; } = new();

        // Q65: Role catalog to use - REQUIRED
        public List<string> RoleCatalog { get; set; } = new()
        {
            "ControlOwner", "EvidenceCustodian", "Approver", "Assessor", "RemediationOwner", "Viewer"
        };

        // Q66: RACI mapping needed? - REQUIRED
        public bool RaciMappingNeeded { get; set; } = false;
        public List<RaciMapping> RaciMappings { get; set; } = new();

        // Q67: Approval gates needed?
        public bool ApprovalGatesNeeded { get; set; } = false;
        public List<ApprovalGate> ApprovalGates { get; set; } = new();

        // Q68: Delegation rules (who acts when someone is OOO)
        public List<DelegationRule> DelegationRules { get; set; } = new();

        // Q69: Notification preferences - REQUIRED
        public List<string> NotificationChannels { get; set; } = new() { "Email" }; // Teams, Email, Both, Slack

        // Q70: Escalation path - REQUIRED
        public int EscalationAfterDays { get; set; } = 5;
        public string EscalationTarget { get; set; } = "Manager"; // Manager, Director, Executive

        public bool IsComplete => OrgAdmins.Count > 0 && NotificationChannels.Count > 0;
    }

    public class OrgAdmin
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
    }

    public class WizardTeamDefinition
    {
        public string TeamCode { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty; // RiskOps, Security, ITOps, AppOwners, Procurement, Legal, Compliance
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string BackupOwnerName { get; set; } = string.Empty;
        public string BackupOwnerEmail { get; set; } = string.Empty;
    }

    public class TeamMemberDefinition
    {
        public string TeamCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty; // From RoleCatalog
        public string Title { get; set; } = string.Empty;
    }

    public class RaciMapping
    {
        public string ControlFamily { get; set; } = string.Empty; // Or SystemCode
        public string Responsible { get; set; } = string.Empty; // RoleCode
        public string Accountable { get; set; } = string.Empty;
        public string Consulted { get; set; } = string.Empty;
        public string Informed { get; set; } = string.Empty;
    }

    public class ApprovalGate
    {
        public string GateName { get; set; } = string.Empty;
        public string ScopeType { get; set; } = string.Empty; // ControlFamily, System, BusinessUnit
        public string Scope { get; set; } = string.Empty;
        public List<string> ApproverRoles { get; set; } = new();
        public int RequiredApprovals { get; set; } = 1;
    }

    public class DelegationRule
    {
        public string FromRoleCode { get; set; } = string.Empty;
        public string ToRoleCode { get; set; } = string.Empty;
        public int MaxDelegationDays { get; set; } = 14;
        public bool RequiresApproval { get; set; } = false;
    }

    #endregion

    #region Section I: Workflow and Cadence (Questions 71-80)

    /// <summary>
    /// Section I: Workflow and Cadence (Make it "non-forgettable")
    /// Questions 71-80 - Timing and SLAs
    /// </summary>
    public class SectionI_WorkflowCadence
    {
        // Q71: Evidence frequency defaults - REQUIRED
        public Dictionary<string, string> EvidenceFrequencyByDomain { get; set; } = new()
        {
            ["AccessControl"] = "Quarterly",
            ["ChangeManagement"] = "Monthly",
            ["IncidentResponse"] = "Quarterly",
            ["VulnerabilityManagement"] = "Monthly",
            ["BackupRecovery"] = "Quarterly",
            ["PolicyReview"] = "Annual"
        };

        // Q72: Access reviews frequency
        public string AccessReviewFrequency { get; set; } = "Quarterly"; // Weekly, Monthly, Quarterly, SemiAnnual, Annual

        // Q73: Vulnerability/patch review frequency
        public string VulnerabilityReviewFrequency { get; set; } = "Monthly";

        // Q74: Backup review frequency + restore test cadence
        public string BackupReviewFrequency { get; set; } = "Monthly";
        public string RestoreTestCadence { get; set; } = "Quarterly";

        // Q75: DR exercise cadence
        public string DrExerciseCadence { get; set; } = "Annual";

        // Q76: Incident tabletop cadence
        public string IncidentTabletopCadence { get; set; } = "SemiAnnual";

        // Q77: Evidence SLA: submit within X days of due date - REQUIRED
        public int EvidenceSubmitSlaDays { get; set; } = 5;

        // Q78: Remediation SLA by severity - REQUIRED
        public Dictionary<string, int> RemediationSlaDays { get; set; } = new()
        {
            ["Critical"] = 7,
            ["High"] = 14,
            ["Medium"] = 30,
            ["Low"] = 90
        };

        // Q79: Exception expiry default - REQUIRED
        public int ExceptionExpiryDays { get; set; } = 90;

        // Q80: Audit request handling
        public string AuditRequestHandling { get; set; } = "SingleQueue"; // SingleQueue, PerDomainQueue

        public bool IsComplete => EvidenceSubmitSlaDays > 0 && RemediationSlaDays.Count > 0;
    }

    #endregion

    #region Section J: Evidence Standards (Questions 81-87)

    /// <summary>
    /// Section J: Evidence Standards (So "PROVE" works automatically)
    /// Questions 81-87 - Evidence requirements
    /// </summary>
    public class SectionJ_EvidenceStandards
    {
        // Q81: Evidence naming convention required?
        public bool NamingConventionRequired { get; set; } = false;
        public string NamingConventionPattern { get; set; } = string.Empty; // e.g., "{ControlId}_{System}_{Date}_{Type}"

        // Q82: Evidence storage location by domain
        public Dictionary<string, string> StorageLocationByDomain { get; set; } = new();

        // Q83: Evidence retention period (years)
        public int RetentionPeriodYears { get; set; } = 7;

        // Q84: Evidence access rules
        public EvidenceAccessRules AccessRules { get; set; } = new();

        // Q85: What evidence is acceptable
        public List<string> AcceptableEvidenceTypes { get; set; } = new()
        {
            "ExportReports", "Logs", "Screenshots", "SignedPDFs", "SystemAttestations", "Configurations"
        };

        // Q86: Sampling guidance (if not continuous)
        public SamplingGuidance Sampling { get; set; } = new();

        // Q87: Confidential evidence handling
        public ConfidentialEvidenceHandling ConfidentialHandling { get; set; } = new();

        public bool IsComplete => RetentionPeriodYears > 0;
    }

    public class EvidenceAccessRules
    {
        public List<string> ViewerRoles { get; set; } = new() { "ControlOwner", "EvidenceCustodian", "Approver", "Assessor", "Viewer" };
        public List<string> ApproverRoles { get; set; } = new() { "Approver", "Assessor" };
        public List<string> UploadRoles { get; set; } = new() { "ControlOwner", "EvidenceCustodian" };
    }

    public class SamplingGuidance
    {
        public bool UseSampling { get; set; } = false;
        public string SamplingMethod { get; set; } = string.Empty; // Statistical, Judgmental, Hybrid
        public int MinimumSampleSize { get; set; } = 25;
        public string SampleSizeFormula { get; set; } = string.Empty; // e.g., "sqrt(population)"
    }

    public class ConfidentialEvidenceHandling
    {
        public bool RequireEncryption { get; set; } = true;
        public bool RestrictedAccess { get; set; } = true;
        public List<string> RestrictedAccessRoles { get; set; } = new() { "Approver", "InternalAudit" };
        public bool RequireWatermark { get; set; } = false;
    }

    #endregion

    #region Section K: Baseline + Overlays Selection (Questions 88-90)

    /// <summary>
    /// Section K: Baseline + Overlays Selection (Make applicability automatic)
    /// Questions 88-90 - Control baseline configuration
    /// </summary>
    public class SectionK_BaselineOverlays
    {
        // Q88: Adopt default baseline control set?
        public bool AdoptDefaultBaseline { get; set; } = true;
        public string DefaultBaselineCode { get; set; } = string.Empty;

        // Q89: Select overlays to enable (auto-derived, but confirm)
        public OverlaySelections Overlays { get; set; } = new();

        // Q90: Any client-specific control requirements to import?
        public bool HasCustomRequirements { get; set; } = false;
        public string CustomRequirementsFile { get; set; } = string.Empty;
        public List<CustomControlRequirement> CustomRequirements { get; set; } = new();

        public bool IsComplete => AdoptDefaultBaseline || HasCustomRequirements;
    }

    public class OverlaySelections
    {
        // Jurisdiction overlays
        public List<string> JurisdictionOverlays { get; set; } = new(); // SA, AE, BH, etc.

        // Sector overlays
        public List<string> SectorOverlays { get; set; } = new(); // Financial, Healthcare, Telecom, Government

        // Data overlays
        public List<string> DataOverlays { get; set; } = new(); // PCI, PII, PHI, Classified

        // Technology overlays
        public List<string> TechnologyOverlays { get; set; } = new(); // Cloud, OnPrem, OT, IoT, Hybrid
    }

    public class CustomControlRequirement
    {
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
        public string ControlFamily { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Source { get; set; } = "Internal";
    }

    #endregion

    #region Section L: Go-Live and Success Metrics (Questions 91-96)

    /// <summary>
    /// Section L: Go-Live and Success Metrics (Sales + operational proof)
    /// Questions 91-96 - Success criteria and baselines
    /// </summary>
    public class SectionL_GoLiveMetrics
    {
        // Q91: What does "success" mean in 90 days? (pick 3)
        public List<string> SuccessMetrics { get; set; } = new(); // FewerAuditHours, FasterEvidenceTurnaround, ReducedRepeatFindings, ImprovedSlaCompliance, ImprovedOutageRecovery

        // Q92: Baseline measurement: current audit prep hours/month
        public decimal? CurrentAuditPrepHoursPerMonth { get; set; }

        // Q93: Baseline: average remediation closure time
        public int? CurrentRemediationClosureDays { get; set; }

        // Q94: Baseline: # overdue controls per month
        public int? CurrentOverdueControlsPerMonth { get; set; }

        // Q95: Target improvement % for each metric
        public Dictionary<string, int> TargetImprovementPercent { get; set; } = new()
        {
            ["AuditPrepHours"] = 30,
            ["RemediationClosure"] = 25,
            ["OverdueControls"] = 50
        };

        // Q96: Pilot scope: top 10–20 controls (which domains)
        public List<string> PilotDomains { get; set; } = new(); // AccessControl, ChangeManagement, IncidentResponse, etc.
        public int PilotControlCount { get; set; } = 20;

        public bool IsComplete => SuccessMetrics.Count >= 3;
    }

    #endregion

    #region Wizard Navigation and Validation

    /// <summary>
    /// Response for saving a wizard section
    /// </summary>
    public class WizardSectionSaveResponse
    {
        public bool Success { get; set; }
        public string Section { get; set; } = string.Empty;
        public bool SectionComplete { get; set; }
        public int OverallProgress { get; set; }
        public string? NextSection { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Coverage validation result for this section
        /// </summary>
        public CoverageValidationResult? CoverageValidation { get; set; }
        
        /// <summary>
        /// Missing required fields for this section
        /// </summary>
        public List<string> MissingRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Missing conditional required fields (required based on answers)
        /// </summary>
        public List<string> MissingConditionalFields { get; set; } = new();
        
        /// <summary>
        /// Coverage completion percentage for this section
        /// </summary>
        public int CoverageCompletionPercent { get; set; }
        
        /// <summary>
        /// Whether this section has all required fields (coverage validation)
        /// </summary>
        public bool CoverageValid { get; set; }
    }

    /// <summary>
    /// Request to validate a specific section
    /// </summary>
    public class WizardValidationRequest
    {
        public Guid TenantId { get; set; }
        public string Section { get; set; } = string.Empty;
        public bool ValidateRequired { get; set; } = true;
        public bool ValidateMinimal { get; set; } = false; // Use minimal required set
    }

    /// <summary>
    /// Minimal required fields for short onboarding (as specified)
    /// A1–A9, D26–D33, E35–E38, F41–F50, H61–H66, I71–I79
    /// </summary>
    public class MinimalOnboardingDto
    {
        // Section A: A1-A9 (identity + domain)
        [Required] public string LegalNameEn { get; set; } = string.Empty;
        public string LegalNameAr { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        [Required] public string CountryOfIncorporation { get; set; } = "SA";
        public List<string> OperatingCountries { get; set; } = new();
        [Required] public string PrimaryHqLocation { get; set; } = string.Empty;
        [Required] public string Timezone { get; set; } = "Asia/Riyadh";
        [Required] public string PrimaryLanguage { get; set; } = "English";
        [Required] public List<string> CorporateEmailDomains { get; set; } = new();
        public string DomainVerificationMethod { get; set; } = "DnsTxt";

        // Section D: D26-D33 (scope)
        [Required] public List<LegalEntityScope> InScopeLegalEntities { get; set; } = new();
        [Required] public List<BusinessUnitScope> InScopeBusinessUnits { get; set; } = new();
        [Required] public List<SystemScope> InScopeSystems { get; set; } = new();
        [Required] public List<string> InScopeProcesses { get; set; } = new();
        public List<string> InScopeEnvironments { get; set; } = new() { "Production" };
        [Required] public List<LocationScope> InScopeLocations { get; set; } = new();
        public List<CriticalityTierDefinition> CriticalityTiers { get; set; } = new();
        public List<ImportantBusinessService> ImportantBusinessServices { get; set; } = new();

        // Section E: E35-E38 (data profile)
        [Required] public List<string> DataTypesProcessed { get; set; } = new();
        public bool HasPaymentCardData { get; set; } = false;
        public bool HasCrossBorderTransfers { get; set; } = false;
        public string CustomerVolumeTier { get; set; } = string.Empty;

        // Section F: F41-F50 (core tooling)
        [Required] public string IdentityProvider { get; set; } = string.Empty;
        public bool SsoEnabled { get; set; } = false;
        public bool ScimEnabled { get; set; } = false;
        [Required] public string ItsmPlatform { get; set; } = string.Empty;
        public string EvidenceRepository { get; set; } = string.Empty;
        public string SiemPlatform { get; set; } = string.Empty;
        public string VulnerabilityManagement { get; set; } = string.Empty;
        public string EdrPlatform { get; set; } = string.Empty;
        [Required] public List<string> CloudProviders { get; set; } = new();
        public string ErpPlatform { get; set; } = string.Empty;

        // Section H: H61-H66 (teams + roles)
        [Required] public List<OrgAdmin> OrgAdmins { get; set; } = new();
        public bool CreateTeamsNow { get; set; } = false;
        public List<WizardTeamDefinition> Teams { get; set; } = new();
        public List<TeamMemberDefinition> TeamMembers { get; set; } = new();
        public List<string> RoleCatalog { get; set; } = new();
        public bool RaciMappingNeeded { get; set; } = false;

        // Section I: I71-I79 (cadence + SLAs)
        public Dictionary<string, string> EvidenceFrequencyByDomain { get; set; } = new();
        public string AccessReviewFrequency { get; set; } = "Quarterly";
        public string VulnerabilityReviewFrequency { get; set; } = "Monthly";
        public string BackupReviewFrequency { get; set; } = "Monthly";
        public string RestoreTestCadence { get; set; } = "Quarterly";
        public string DrExerciseCadence { get; set; } = "Annual";
        public string IncidentTabletopCadence { get; set; } = "SemiAnnual";
        [Required] public int EvidenceSubmitSlaDays { get; set; } = 5;
        [Required] public Dictionary<string, int> RemediationSlaDays { get; set; } = new();
        public int ExceptionExpiryDays { get; set; } = 90;
    }

    /// <summary>
    /// Wizard progress summary
    /// </summary>
    public class WizardProgressSummary
    {
        public Guid TenantId { get; set; }
        public string WizardStatus { get; set; } = "NotStarted";
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; } = 12;
        public int ProgressPercent { get; set; }
        public Dictionary<string, SectionProgress> Sections { get; set; } = new();
        public List<string> PendingRequiredFields { get; set; } = new();
        public bool CanComplete { get; set; }
        public DateTime? LastUpdated { get; set; }
        
        /// <summary>
        /// Coverage validation results for all sections
        /// </summary>
        public Dictionary<string, CoverageValidationResult>? SectionCoverage { get; set; }
        
        /// <summary>
        /// Overall coverage completion percentage
        /// </summary>
        public int OverallCoveragePercent { get; set; }
        
        /// <summary>
        /// Whether all required sections have complete coverage
        /// </summary>
        public bool CoverageComplete { get; set; }
    }

    public class SectionProgress
    {
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public bool IsRequired { get; set; }
        public int QuestionsTotal { get; set; }
        public int QuestionsAnswered { get; set; }
        
        /// <summary>
        /// Missing required fields for this section (from validation)
        /// </summary>
        public List<string> MissingRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Coverage validation for this section
        /// </summary>
        public CoverageValidationResult? CoverageValidation { get; set; }
        
        /// <summary>
        /// Coverage completion percentage for this section
        /// </summary>
        public int CoverageCompletionPercent { get; set; }
        
        /// <summary>
        /// Whether this section has complete coverage (all required fields present)
        /// </summary>
        public bool CoverageValid { get; set; }
    }

    #endregion
}
