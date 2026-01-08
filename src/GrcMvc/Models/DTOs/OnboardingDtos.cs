using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models.DTOs
{
    // ============================================================================
    // ONBOARDING WIZARD DTOs - Comprehensive 12-Step Wizard (Sections A-L)
    // ============================================================================

    /// <summary>
    /// Main Wizard DTO - Contains all step data
    /// </summary>
    public class OnboardingWizardDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public int CurrentStep { get; set; } = 1;
        public string WizardStatus { get; set; } = "NotStarted";
        public int ProgressPercent { get; set; } = 0;
        public List<string> CompletedSections { get; set; } = new();
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Step data containers
        public StepAOrganizationIdentityDto StepA { get; set; } = new();
        public StepBAssuranceObjectiveDto StepB { get; set; } = new();
        public StepCRegulatoryApplicabilityDto StepC { get; set; } = new();
        public StepDScopeDefinitionDto StepD { get; set; } = new();
        public StepEDataRiskProfileDto StepE { get; set; } = new();
        public StepFTechnologyLandscapeDto StepF { get; set; } = new();
        public StepGControlOwnershipDto StepG { get; set; } = new();
        public StepHTeamsRolesDto StepH { get; set; } = new();
        public StepIWorkflowCadenceDto StepI { get; set; } = new();
        public StepJEvidenceStandardsDto StepJ { get; set; } = new();
        public StepKBaselineOverlaysDto StepK { get; set; } = new();
        public StepLGoLiveMetricsDto StepL { get; set; } = new();
    }

    /// <summary>
    /// Step A: Organization Identity and Tenancy (Questions 1-13)
    /// </summary>
    public class StepAOrganizationIdentityDto
    {
        [Required(ErrorMessage = "Organization legal name (English) is required")]
        [MaxLength(255)]
        public string OrganizationLegalNameEn { get; set; } = string.Empty;

        [MaxLength(255)]
        public string OrganizationLegalNameAr { get; set; } = string.Empty;

        [MaxLength(255)]
        public string TradeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country of incorporation is required")]
        public string CountryOfIncorporation { get; set; } = "SA";

        public List<string> OperatingCountries { get; set; } = new();

        [MaxLength(255)]
        public string PrimaryHqLocation { get; set; } = string.Empty;

        public string DefaultTimezone { get; set; } = "Asia/Riyadh";

        public string PrimaryLanguage { get; set; } = "bilingual"; // english, arabic, bilingual

        public List<string> CorporateEmailDomains { get; set; } = new();

        public string DomainVerificationMethod { get; set; } = "admin_email"; // dns_txt, admin_email

        [Required(ErrorMessage = "Organization type is required")]
        public string OrganizationType { get; set; } = string.Empty; // enterprise, sme, government, regulated_fi, fintech, telecom, other

        [Required(ErrorMessage = "Industry/sector is required")]
        public string IndustrySector { get; set; } = string.Empty;

        public List<string> BusinessLines { get; set; } = new(); // retail, corporate, payments, lending, wealth, telecom_services

        public bool HasDataResidencyRequirement { get; set; } = false;
        public List<string> DataResidencyCountries { get; set; } = new();
    }

    /// <summary>
    /// Step B: Assurance Objective (Questions 14-18)
    /// </summary>
    public class StepBAssuranceObjectiveDto
    {
        [Required(ErrorMessage = "Primary driver is required")]
        public string PrimaryDriver { get; set; } = string.Empty; // regulator_exam, internal_audit, external_audit, certification, customer_due_diligence, board_reporting

        public DateTime? TargetTimeline { get; set; }

        public List<RankedItem> CurrentPainPoints { get; set; } = new(); // evidence_collection, mapping_frameworks, remediation_delays, outages, vendor_risk, fraud_leakage

        public string DesiredMaturity { get; set; } = "Foundation"; // Foundation, AssuranceOps, ContinuousAssurance

        public List<string> ReportingAudience { get; set; } = new(); // Board, CRO, CIO, CISO, Internal_Audit, Regulators
    }

    /// <summary>
    /// Step C: Regulatory and Framework Applicability (Questions 19-25)
    /// </summary>
    public class StepCRegulatoryApplicabilityDto
    {
        public List<RegulatorEntry> PrimaryRegulators { get; set; } = new();
        public List<RegulatorEntry> SecondaryRegulators { get; set; } = new();
        public List<string> MandatoryFrameworks { get; set; } = new();
        public List<string> OptionalFrameworks { get; set; } = new();
        public List<string> InternalPolicies { get; set; } = new(); // ISMS, IT policies, risk policies
        public List<CertificationEntry> CertificationsHeld { get; set; } = new(); // ISO, SOC reports
        public string AuditScopeType { get; set; } = "enterprise"; // enterprise, business_unit, system, process, vendor
    }

    /// <summary>
    /// Step D: Scope Definition (Questions 26-34)
    /// </summary>
    public class StepDScopeDefinitionDto
    {
        public List<LegalEntityEntry> InScopeLegalEntities { get; set; } = new();
        public List<string> InScopeBusinessUnits { get; set; } = new();
        public List<SystemEntry> InScopeSystems { get; set; } = new();
        public List<string> InScopeProcesses { get; set; } = new(); // onboarding, payments, p2p, change_mgmt, incident_response
        public string InScopeEnvironments { get; set; } = "both"; // production, non_production, both
        public List<string> InScopeLocations { get; set; } = new(); // data centers, cloud regions
        public Dictionary<string, string> SystemCriticalityTiers { get; set; } = new(); // Tier1, Tier2, Tier3 definitions
        public List<BusinessServiceEntry> ImportantBusinessServices { get; set; } = new();
        public List<ExclusionEntry> Exclusions { get; set; } = new();
    }

    /// <summary>
    /// Step E: Data and Risk Profile (Questions 35-40)
    /// </summary>
    public class StepEDataRiskProfileDto
    {
        [Required(ErrorMessage = "At least one data type must be selected")]
        public List<string> DataTypesProcessed { get; set; } = new(); // PII, PCI, PHI, confidential, classified

        public bool HasPaymentCardData { get; set; } = false;
        public List<string> PaymentCardDataLocations { get; set; } = new();

        public bool HasCrossBorderDataTransfers { get; set; } = false;
        public List<string> CrossBorderTransferCountries { get; set; } = new();

        public string CustomerVolumeTier { get; set; } = string.Empty; // <1K, 1K-10K, 10K-100K, 100K-1M, 1M+
        public string TransactionVolumeTier { get; set; } = string.Empty; // <1K/day, 1K-10K/day, 10K-100K/day, 100K+/day

        public bool HasInternetFacingSystems { get; set; } = false;
        public List<string> InternetFacingSystems { get; set; } = new();

        public bool HasThirdPartyDataProcessing { get; set; } = false;
        public List<VendorEntry> ThirdPartyDataProcessors { get; set; } = new();
    }

    /// <summary>
    /// Step F: Technology Landscape (Questions 41-53)
    /// </summary>
    public class StepFTechnologyLandscapeDto
    {
        public string IdentityProvider { get; set; } = string.Empty; // AzureAD, Okta, PingIdentity, Other
        public bool SsoEnabled { get; set; } = false;
        public bool ScimProvisioningAvailable { get; set; } = false;
        public string ItsmPlatform { get; set; } = string.Empty; // ServiceNow, Jira, Other
        public string EvidenceRepository { get; set; } = string.Empty; // SharePoint, GrcVault, GoogleDrive, Other
        public string SiemPlatform { get; set; } = string.Empty; // Sentinel, Splunk, QRadar, Other
        public string VulnerabilityManagementTool { get; set; } = string.Empty; // Tenable, Qualys, Rapid7, Other
        public string EdrPlatform { get; set; } = string.Empty; // Defender, CrowdStrike, Other
        public List<string> CloudProviders { get; set; } = new(); // AWS, Azure, GCP
        public string ErpSystem { get; set; } = string.Empty; // SAP, Oracle, Dynamics, ERPNext, Other
        public string CmdbSource { get; set; } = string.Empty; // ServiceNow CMDB, cloud inventory
        public string CiCdTooling { get; set; } = string.Empty; // GitHub, GitLab, AzureDevOps
        public string BackupDrTooling { get; set; } = string.Empty;
    }

    /// <summary>
    /// Step G: Control Ownership Model (Questions 54-60)
    /// </summary>
    public class StepGControlOwnershipDto
    {
        [Required(ErrorMessage = "Control ownership approach is required")]
        public string ControlOwnershipApproach { get; set; } = "hybrid"; // centralized, federated, hybrid

        public string DefaultControlOwnerTeam { get; set; } = string.Empty;
        public string ExceptionApproverRole { get; set; } = string.Empty;
        public string RegulatoryInterpretationApproverRole { get; set; } = string.Empty;
        public string ControlEffectivenessSignoffRole { get; set; } = string.Empty;
        public string InternalAuditStakeholder { get; set; } = string.Empty; // name/role
        public string RiskCommitteeCadence { get; set; } = "quarterly"; // monthly, quarterly
        public List<string> RiskCommitteeAttendees { get; set; } = new(); // roles
    }

    /// <summary>
    /// Step H: Teams, Roles, and Access (Questions 61-70)
    /// </summary>
    public class StepHTeamsRolesDto
    {
        [Required(ErrorMessage = "At least one org admin is required")]
        public List<AdminEntry> OrgAdmins { get; set; } = new();

        public bool CreateTeamsNow { get; set; } = false;
        public List<TeamDefinition> TeamList { get; set; } = new();
        public List<string> SelectedRoleCatalog { get; set; } = new(); // Control_Owner, Evidence_Custodian, Approver, Assessor_Tester, Remediation_Owner, Viewer_Auditor

        public bool RaciMappingNeeded { get; set; } = false;
        public List<RaciEntry> RaciMapping { get; set; } = new();

        public bool ApprovalGatesNeeded { get; set; } = false;
        public List<ApprovalGateEntry> ApprovalGates { get; set; } = new();

        public List<DelegationRuleEntry> DelegationRules { get; set; } = new();

        public string NotificationPreference { get; set; } = "email"; // teams, email, both

        public int EscalationDaysOverdue { get; set; } = 3;
        public string EscalationTarget { get; set; } = "manager";
    }

    /// <summary>
    /// Step I: Workflow and Cadence (Questions 71-80)
    /// </summary>
    public class StepIWorkflowCadenceDto
    {
        public Dictionary<string, string> EvidenceFrequencyDefaults { get; set; } = new(); // {domain: frequency}
        public string AccessReviewsFrequency { get; set; } = "quarterly";
        public string VulnerabilityPatchReviewFrequency { get; set; } = "weekly"; // weekly, monthly
        public string BackupReviewFrequency { get; set; } = "monthly";
        public string RestoreTestCadence { get; set; } = "quarterly";
        public string DrExerciseCadence { get; set; } = "annual";
        public string IncidentTabletopCadence { get; set; } = "annual";
        public int EvidenceSlaSubmitDays { get; set; } = 5;
        public Dictionary<string, int> RemediationSla { get; set; } = new() { { "critical", 7 }, { "high", 14 }, { "medium", 30 }, { "low", 60 } };
        public int ExceptionExpiryDays { get; set; } = 90;
        public string AuditRequestHandling { get; set; } = "single_queue"; // single_queue, per_domain_queue
    }

    /// <summary>
    /// Step J: Evidence Standards (Questions 81-87)
    /// </summary>
    public class StepJEvidenceStandardsDto
    {
        public bool EvidenceNamingConventionRequired { get; set; } = true;
        public string EvidenceNamingPattern { get; set; } = "{TenantId}-{ControlId}-{Date}-{Sequence}";
        public Dictionary<string, string> EvidenceStorageLocation { get; set; } = new();
        public int EvidenceRetentionYears { get; set; } = 7;
        public Dictionary<string, List<string>> EvidenceAccessRules { get; set; } = new(); // {role: [view, approve]}
        public List<string> AcceptableEvidenceTypes { get; set; } = new() { "export_reports", "logs", "screenshots", "signed_pdfs", "system_attestations" };
        public Dictionary<string, string> SamplingGuidance { get; set; } = new();
        public bool ConfidentialEvidenceEncryption { get; set; } = true;
        public List<string> ConfidentialEvidenceAccess { get; set; } = new(); // roles with access
    }

    /// <summary>
    /// Step K: Baseline + Overlays Selection (Questions 88-90)
    /// </summary>
    public class StepKBaselineOverlaysDto
    {
        public bool AdoptDefaultBaseline { get; set; } = true;
        public List<OverlaySelection> SelectedOverlays { get; set; } = new();
        public bool HasClientSpecificControls { get; set; } = false;
        public List<ClientControlEntry> ClientSpecificControls { get; set; } = new();
    }

    /// <summary>
    /// Step L: Go-Live and Success Metrics (Questions 91-96)
    /// </summary>
    public class StepLGoLiveMetricsDto
    {
        public List<string> SuccessMetricsTop3 { get; set; } = new(); // fewer_audit_hours, faster_evidence_turnaround, reduced_repeat_findings, improved_sla_compliance, improved_outage_recovery
        public decimal? BaselineAuditPrepHoursPerMonth { get; set; }
        public decimal? BaselineRemediationClosureDays { get; set; }
        public int? BaselineOverdueControlsPerMonth { get; set; }
        public Dictionary<string, decimal> TargetImprovement { get; set; } = new(); // {metric: percentage}
        public List<string> PilotScope { get; set; } = new(); // control IDs or domains
    }

    // ============================================================================
    // SUPPORTING DTOs
    // ============================================================================

    public class RankedItem
    {
        public string Item { get; set; } = string.Empty;
        public int Rank { get; set; } = 0;
    }

    public class RegulatorEntry
    {
        public string Jurisdiction { get; set; } = string.Empty;
        public string RegulatorCode { get; set; } = string.Empty;
        public string RegulatorName { get; set; } = string.Empty;
    }

    public class CertificationEntry
    {
        public string CertificationType { get; set; } = string.Empty; // ISO27001, SOC2, etc.
        public DateTime? CertificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CertifyingBody { get; set; } = string.Empty;
    }

    public class LegalEntityEntry
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityNameAr { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class SystemEntry
    {
        public string SystemName { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty; // application, database, infrastructure
        public string CriticalityTier { get; set; } = "Tier2"; // Tier1, Tier2, Tier3
        public string Owner { get; set; } = string.Empty;
    }

    public class BusinessServiceEntry
    {
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceOwner { get; set; } = string.Empty;
        public string CriticalityLevel { get; set; } = "High";
    }

    public class ExclusionEntry
    {
        public string ExclusionType { get; set; } = string.Empty; // system, process, entity, location
        public string ExclusionName { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
    }

    public class VendorEntry
    {
        public string VendorName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string DataTypes { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Medium";
    }

    public class AdminEntry
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class TeamDefinition
    {
        public string TeamName { get; set; } = string.Empty;
        public string TeamNameAr { get; set; } = string.Empty;
        public string TeamType { get; set; } = "Operational"; // Operational, Governance, Project, External
        public string TeamOwnerEmail { get; set; } = string.Empty;
        public string BackupOwnerEmail { get; set; } = string.Empty;
        public List<TeamMemberEntry> Members { get; set; } = new();
    }

    public class TeamMemberEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public bool CanApprove { get; set; } = false;
        public bool CanDelegate { get; set; } = false;
    }

    public class RaciEntry
    {
        public string ScopeType { get; set; } = string.Empty; // ControlFamily, System, BusinessUnit
        public string ScopeId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string RaciType { get; set; } = "R"; // R, A, C, I
        public string RoleCode { get; set; } = string.Empty;
    }

    public class ApprovalGateEntry
    {
        public string Scope { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty;
    }

    public class DelegationRuleEntry
    {
        public string FromRole { get; set; } = string.Empty;
        public string ToRole { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty; // ooo, workload, escalation
    }

    public class OverlaySelection
    {
        public string OverlayType { get; set; } = string.Empty; // jurisdiction, sector, data, technology
        public string OverlayCode { get; set; } = string.Empty;
        public string OverlayName { get; set; } = string.Empty;
        public bool IsAutoSelected { get; set; } = false;
        public string Reason { get; set; } = string.Empty;
    }

    public class ClientControlEntry
    {
        public string ControlId { get; set; } = string.Empty;
        public string ControlName { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // client_policy, contract, custom
    }

    /// <summary>
    /// Wizard step navigation response
    /// </summary>
    public class WizardStepResponse
    {
        public bool Success { get; set; }
        public int CurrentStep { get; set; }
        public int NextStep { get; set; }
        public int ProgressPercent { get; set; }
        public List<string> CompletedSections { get; set; } = new();
        public Dictionary<string, string> ValidationErrors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Wizard summary for display
    /// </summary>
    public class WizardSummaryDto
    {
        public Guid TenantId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; } = 12;
        public int ProgressPercent { get; set; }
        public string WizardStatus { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public List<WizardStepSummary> Steps { get; set; } = new();
    }

    public class WizardStepSummary
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepNameAr { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsLocked { get; set; }
    }


    /// <summary>
    /// DTO for tenant onboarding scope (applicable baselines, packages, templates).
    /// </summary>
    public class OnboardingScopeDto
    {
        public Guid TenantId { get; set; }
        public List<BaselineDto> ApplicableBaselines { get; set; } = new List<BaselineDto>();
        public List<PackageDto> ApplicablePackages { get; set; } = new List<PackageDto>();
        public List<TemplateDto> ApplicableTemplates { get; set; } = new List<TemplateDto>();
        public List<string> ApplicableFrameworks { get; set; } = new List<string>();
        public List<string> RecommendedPackages { get; set; } = new List<string>();
        public List<string> ApplicableTemplatesList { get; set; } = new List<string>();
        public List<string> AuditPackages { get; set; } = new List<string>();
        public DateTime RetrievedAt { get; set; }
    }

    /// <summary>
    /// DTO for baseline in scope.
    /// </summary>
    public class BaselineDto
    {
        public string BaselineCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for package in scope.
    /// </summary>
    public class PackageDto
    {
        public string PackageCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for template in scope.
    /// </summary>
    public class TemplateDto
    {
        public string TemplateCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new tenant during signup.
    /// </summary>
    public class CreateTenantDto
    {
        [Required(ErrorMessage = "Organization name is required")]
        [MinLength(2, ErrorMessage = "Organization name must be at least 2 characters")]
        [MaxLength(255, ErrorMessage = "Organization name cannot exceed 255 characters")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string AdminEmail { get; set; } = string.Empty;

        [MaxLength(63, ErrorMessage = "Tenant slug cannot exceed 63 characters")]
        [RegularExpression(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", ErrorMessage = "Tenant slug must be lowercase alphanumeric with optional hyphens")]
        public string TenantSlug { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SubscriptionTier { get; set; } = "Professional";

        [MaxLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string Country { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for activating a tenant.
    /// </summary>
    public class ActivateTenantDto
    {
        [Required(ErrorMessage = "Tenant slug is required")]
        [MaxLength(63)]
        public string TenantSlug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activation token is required")]
        [MaxLength(255)]
        public string ActivationToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for organization profile setup during onboarding.
    /// </summary>
    public class OrganizationProfileDto
    {
        [Required(ErrorMessage = "Tenant ID is required")]
        public Guid TenantId { get; set; }

        [MaxLength(255, ErrorMessage = "Organization name cannot exceed 255 characters")]
        public string OrganizationName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string OrgType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization type is required")]
        [MaxLength(100, ErrorMessage = "Organization type cannot exceed 100 characters")]
        public string OrganizationType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sector is required")]
        [MaxLength(100, ErrorMessage = "Sector cannot exceed 100 characters")]
        public string Sector { get; set; } = string.Empty;

        [MaxLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string Country { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Data types cannot exceed 500 characters")]
        public string DataTypes { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Hosting model cannot exceed 100 characters")]
        public string HostingModel { get; set; } = string.Empty;

        [MaxLength(50)]
        public string OrganizationSize { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization size is required")]
        [MaxLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string Size { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ComplianceMaturity { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Maturity level cannot exceed 50 characters")]
        public string Maturity { get; set; } = string.Empty;

        public bool IsCriticalInfrastructure { get; set; }

        [MaxLength(500, ErrorMessage = "Vendors cannot exceed 500 characters")]
        public string Vendors { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for plan creation during scope delivery.
    /// </summary>
    public class CreatePlanDto
    {
        public Guid TenantId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty; // QuickScan, Full, Remediation
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public Guid? RulesetVersionId { get; set; }
    }
}
