using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Comprehensive Onboarding Wizard Entity
    /// Covers all 96 questions across 12 sections (A-L)
    /// Ensures organizations and teams are correctly "recognized" and the experience is fully mapped
    /// (controls, applicability, evidence, workflows, notifications)
    /// </summary>
    public class OnboardingWizard : BaseEntity
    {
        public Guid TenantId { get; set; }

        // ============================================================================
        // SECTION A: Organization Identity and Tenancy (Questions 1-13)
        // ============================================================================

        // A1: Organization legal name (English)
        [Required]
        [MaxLength(255)]
        public string OrganizationLegalNameEn { get; set; } = string.Empty;

        // A1: Organization legal name (Arabic)
        [MaxLength(255)]
        public string OrganizationLegalNameAr { get; set; } = string.Empty;

        // A2: Trade name / brand (if different)
        [MaxLength(255)]
        public string TradeName { get; set; } = string.Empty;

        // A3: Country of incorporation
        [Required]
        [MaxLength(10)]
        public string CountryOfIncorporation { get; set; } = "SA";

        // A4: Operating countries/jurisdictions (JSON array)
        public string OperatingCountriesJson { get; set; } = "[]";

        // A5: Primary HQ location
        [MaxLength(255)]
        public string PrimaryHqLocation { get; set; } = string.Empty;

        // A6: Timezone (default)
        [MaxLength(50)]
        public string DefaultTimezone { get; set; } = "Asia/Riyadh";

        // A7: Primary language (English / Arabic / bilingual)
        [MaxLength(20)]
        public string PrimaryLanguage { get; set; } = "bilingual";

        // A8: Corporate email domain(s) (JSON array)
        public string CorporateEmailDomainsJson { get; set; } = "[]";

        // A9: Domain verification method (DNS TXT / admin email confirmation)
        [MaxLength(50)]
        public string DomainVerificationMethod { get; set; } = "admin_email";

        // A10: Org type: enterprise / SME / government / regulated financial institution / fintech / telecom / other
        [Required]
        [MaxLength(50)]
        public string OrganizationType { get; set; } = string.Empty;

        // A11: Industry/sector (choose one or multi-sector)
        [Required]
        [MaxLength(100)]
        public string IndustrySector { get; set; } = string.Empty;

        // A12: Business lines (JSON array) - retail, corporate, payments, lending, wealth, telecom services, etc.
        public string BusinessLinesJson { get; set; } = "[]";

        // A13: Data residency requirement (yes/no; which countries)
        public bool HasDataResidencyRequirement { get; set; } = false;
        public string DataResidencyCountriesJson { get; set; } = "[]";

        // ============================================================================
        // SECTION B: Assurance Objective (Questions 14-18)
        // ============================================================================

        // B14: Primary driver
        [Required]
        [MaxLength(100)]
        public string PrimaryDriver { get; set; } = string.Empty; // regulator_exam, internal_audit, external_audit, certification, customer_due_diligence, board_reporting

        // B15: Target timeline (go-live date or audit date)
        public DateTime? TargetTimeline { get; set; }

        // B16: Current pain points (JSON array with ranking)
        public string CurrentPainPointsJson { get; set; } = "[]"; // evidence_collection, mapping_frameworks, remediation_delays, outages, vendor_risk, fraud_leakage

        // B17: Desired maturity: Foundation / Assurance Ops / Continuous Assurance
        [MaxLength(50)]
        public string DesiredMaturity { get; set; } = "Foundation";

        // B18: Reporting audience (JSON array of roles)
        public string ReportingAudienceJson { get; set; } = "[]"; // Board, CRO, CIO, CISO, Internal_Audit, Regulators

        // ============================================================================
        // SECTION C: Regulatory and Framework Applicability (Questions 19-25)
        // ============================================================================

        // C19: Primary regulator(s) per jurisdiction (JSON array)
        public string PrimaryRegulatorsJson { get; set; } = "[]";

        // C20: Secondary regulators (JSON array)
        public string SecondaryRegulatorsJson { get; set; } = "[]";

        // C21: Mandatory frameworks (if already defined internally) (JSON array)
        public string MandatoryFrameworksJson { get; set; } = "[]";

        // C22: Optional frameworks used for benchmarking (JSON array)
        public string OptionalFrameworksJson { get; set; } = "[]";

        // C23: Internal policies/standards to align with (JSON array)
        public string InternalPoliciesJson { get; set; } = "[]";

        // C24: Certifications held (JSON array) - ISO, SOC reports, etc.
        public string CertificationsHeldJson { get; set; } = "[]";

        // C25: Audit scope type: enterprise / business unit / system / process / vendor
        [MaxLength(50)]
        public string AuditScopeType { get; set; } = "enterprise";

        // ============================================================================
        // SECTION D: Scope Definition (Questions 26-34)
        // ============================================================================

        // D26: In-scope legal entities (JSON array)
        public string InScopeLegalEntitiesJson { get; set; } = "[]";

        // D27: In-scope business units (JSON array)
        public string InScopeBusinessUnitsJson { get; set; } = "[]";

        // D28: In-scope systems/applications (JSON array)
        public string InScopeSystemsJson { get; set; } = "[]";

        // D29: In-scope processes (JSON array)
        public string InScopeProcessesJson { get; set; } = "[]";

        // D30: In-scope environments: production / non-production / both
        [MaxLength(50)]
        public string InScopeEnvironments { get; set; } = "both";

        // D31: In-scope locations/data centers/cloud regions (JSON array)
        public string InScopeLocationsJson { get; set; } = "[]";

        // D32: System criticality tiers (JSON object with tier definitions)
        public string SystemCriticalityTiersJson { get; set; } = "{}";

        // D33: Important business services (if resilience program) - JSON array with owners
        public string ImportantBusinessServicesJson { get; set; } = "[]";

        // D34: Exclusions (explicitly out of scope) with rationale (JSON array)
        public string ExclusionsJson { get; set; } = "[]";

        // ============================================================================
        // SECTION E: Data and Risk Profile (Questions 35-40)
        // ============================================================================

        // E35: Data types processed (JSON array) - PII / PCI / PHI / confidential / classified
        public string DataTypesProcessedJson { get; set; } = "[]";

        // E36: Payment card data present? (yes/no) If yes: where (systems/processes)
        public bool HasPaymentCardData { get; set; } = false;
        public string PaymentCardDataLocationsJson { get; set; } = "[]";

        // E37: Cross-border data transfers? (yes/no; which countries)
        public bool HasCrossBorderDataTransfers { get; set; } = false;
        public string CrossBorderTransferCountriesJson { get; set; } = "[]";

        // E38: Customer volume / transaction volume tier (optional; used for impact)
        [MaxLength(50)]
        public string CustomerVolumeTier { get; set; } = string.Empty;
        [MaxLength(50)]
        public string TransactionVolumeTier { get; set; } = string.Empty;

        // E39: Internet-facing systems? (yes/no; list)
        public bool HasInternetFacingSystems { get; set; } = false;
        public string InternetFacingSystemsJson { get; set; } = "[]";

        // E40: Third-party processing of sensitive data? (yes/no; list vendors)
        public bool HasThirdPartyDataProcessing { get; set; } = false;
        public string ThirdPartyDataProcessorsJson { get; set; } = "[]";

        // ============================================================================
        // SECTION F: Technology Landscape (Questions 41-53)
        // ============================================================================

        // F41: Identity provider: Azure AD / Okta / other
        [MaxLength(100)]
        public string IdentityProvider { get; set; } = string.Empty;

        // F42: SSO enabled? (yes/no)
        public bool SsoEnabled { get; set; } = false;

        // F43: SCIM provisioning available? (yes/no)
        public bool ScimProvisioningAvailable { get; set; } = false;

        // F44: ITSM/ticketing: ServiceNow / Jira / other
        [MaxLength(100)]
        public string ItsmPlatform { get; set; } = string.Empty;

        // F45: Evidence repository: SharePoint / GRC vault / Google Drive / other
        [MaxLength(100)]
        public string EvidenceRepository { get; set; } = string.Empty;

        // F46: SIEM/SOC: Sentinel / Splunk / QRadar / other
        [MaxLength(100)]
        public string SiemPlatform { get; set; } = string.Empty;

        // F47: Vulnerability management: Tenable / Qualys / Rapid7 / other
        [MaxLength(100)]
        public string VulnerabilityManagementTool { get; set; } = string.Empty;

        // F48: EDR: Defender / CrowdStrike / other
        [MaxLength(100)]
        public string EdrPlatform { get; set; } = string.Empty;

        // F49: Cloud providers: AWS / Azure / GCP / hybrid (JSON array)
        public string CloudProvidersJson { get; set; } = "[]";

        // F50: ERP: SAP / Oracle / Dynamics / ERPNext / other
        [MaxLength(100)]
        public string ErpSystem { get; set; } = string.Empty;

        // F51: CMDB/asset inventory source (ServiceNow CMDB, cloud inventory, etc.)
        [MaxLength(100)]
        public string CmdbSource { get; set; } = string.Empty;

        // F52: CI/CD tooling (if SDLC controls matter): GitHub/GitLab/Azure DevOps/etc.
        [MaxLength(100)]
        public string CiCdTooling { get; set; } = string.Empty;

        // F53: Backup/DR tooling (optional but recommended)
        [MaxLength(100)]
        public string BackupDrTooling { get; set; } = string.Empty;

        // ============================================================================
        // SECTION G: Control Ownership Model (Questions 54-60)
        // ============================================================================

        // G54: Control ownership approach: centralized / federated / hybrid
        [MaxLength(50)]
        public string ControlOwnershipApproach { get; set; } = "hybrid";

        // G55: Default control owner team (if not specified per system)
        [MaxLength(100)]
        public string DefaultControlOwnerTeam { get; set; } = string.Empty;

        // G56: Who approves exceptions (role/title)
        [MaxLength(100)]
        public string ExceptionApproverRole { get; set; } = string.Empty;

        // G57: Who approves regulatory interpretations/mappings (role/title)
        [MaxLength(100)]
        public string RegulatoryInterpretationApproverRole { get; set; } = string.Empty;

        // G58: Who signs off control effectiveness (role/title)
        [MaxLength(100)]
        public string ControlEffectivenessSignoffRole { get; set; } = string.Empty;

        // G59: Internal audit stakeholder (name/role)
        [MaxLength(255)]
        public string InternalAuditStakeholder { get; set; } = string.Empty;

        // G60: Risk committee cadence (monthly/quarterly) and attendees (roles)
        [MaxLength(50)]
        public string RiskCommitteeCadence { get; set; } = "quarterly";
        public string RiskCommitteeAttendeesJson { get; set; } = "[]";

        // ============================================================================
        // SECTION H: Teams, Roles, and Access (Questions 61-70)
        // ============================================================================

        // H61: Org admin(s): JSON array of {name, email}
        public string OrgAdminsJson { get; set; } = "[]";

        // H62: Create teams now? (yes/no)
        public bool CreateTeamsNow { get; set; } = false;

        // H63: Team list (JSON array of {teamName, teamOwnerEmail, backupOwnerEmail})
        public string TeamListJson { get; set; } = "[]";

        // H64: Team members (JSON array of {teamName, members: [{name, email, role}]})
        public string TeamMembersJson { get; set; } = "[]";

        // H65: Role catalog to use (JSON array of selected roles)
        public string SelectedRoleCatalogJson { get; set; } = "[]"; // Control_Owner, Evidence_Custodian, Approver, Assessor_Tester, Remediation_Owner, Viewer_Auditor

        // H66: RACI mapping needed? (yes/no)
        public bool RaciMappingNeeded { get; set; } = false;
        // H66b: RACI mapping (JSON array of {controlFamily/system, team, role})
        public string RaciMappingJson { get; set; } = "[]";

        // H67: Approval gates needed? (yes/no)
        public bool ApprovalGatesNeeded { get; set; } = false;
        // H67b: Approval gates (JSON array of {scope, approverRole})
        public string ApprovalGatesJson { get; set; } = "[]";

        // H68: Delegation rules (JSON array of {role, delegateTo, conditions})
        public string DelegationRulesJson { get; set; } = "[]";

        // H69: Notification preferences: Teams / email / both
        [MaxLength(50)]
        public string NotificationPreference { get; set; } = "email";

        // H70: Escalation path: to manager after X days overdue
        public int EscalationDaysOverdue { get; set; } = 3;
        [MaxLength(100)]
        public string EscalationTarget { get; set; } = "manager";

        // ============================================================================
        // SECTION I: Workflow and Cadence (Questions 71-80)
        // ============================================================================

        // I71: Evidence frequency defaults (JSON object per domain)
        public string EvidenceFrequencyDefaultsJson { get; set; } = "{}"; // {domain: monthly/quarterly/annual}

        // I72: Access reviews frequency (e.g., quarterly)
        [MaxLength(50)]
        public string AccessReviewsFrequency { get; set; } = "quarterly";

        // I73: Vulnerability/patch review frequency (weekly/monthly)
        [MaxLength(50)]
        public string VulnerabilityPatchReviewFrequency { get; set; } = "weekly";

        // I74: Backup review frequency + restore test cadence
        [MaxLength(50)]
        public string BackupReviewFrequency { get; set; } = "monthly";
        [MaxLength(50)]
        public string RestoreTestCadence { get; set; } = "quarterly";

        // I75: DR exercise cadence
        [MaxLength(50)]
        public string DrExerciseCadence { get; set; } = "annual";

        // I76: Incident tabletop cadence
        [MaxLength(50)]
        public string IncidentTabletopCadence { get; set; } = "annual";

        // I77: Evidence SLA: submit within X days of due date
        public int EvidenceSlaSubmitDays { get; set; } = 5;

        // I78: Remediation SLA by severity (JSON object)
        public string RemediationSlaJson { get; set; } = "{\"critical\": 7, \"high\": 14, \"medium\": 30, \"low\": 60}";

        // I79: Exception expiry default (e.g., 90 days)
        public int ExceptionExpiryDays { get; set; } = 90;

        // I80: Audit request handling: single queue vs per domain queue
        [MaxLength(50)]
        public string AuditRequestHandling { get; set; } = "single_queue";

        // ============================================================================
        // SECTION J: Evidence Standards (Questions 81-87)
        // ============================================================================

        // J81: Evidence naming convention required? (yes/no)
        public bool EvidenceNamingConventionRequired { get; set; } = true;
        [MaxLength(255)]
        public string EvidenceNamingPattern { get; set; } = "{TenantId}-{ControlId}-{Date}-{Sequence}";

        // J82: Evidence storage location by domain (JSON object)
        public string EvidenceStorageLocationJson { get; set; } = "{}";

        // J83: Evidence retention period (years)
        public int EvidenceRetentionYears { get; set; } = 7;

        // J84: Evidence access rules (JSON object)
        public string EvidenceAccessRulesJson { get; set; } = "{}";

        // J85: Acceptable evidence types (JSON array)
        public string AcceptableEvidenceTypesJson { get; set; } = "[\"export_reports\", \"logs\", \"screenshots\", \"signed_pdfs\", \"system_attestations\"]";

        // J86: Sampling guidance (if not continuous): sample size rules
        public string SamplingGuidanceJson { get; set; } = "{}";

        // J87: Confidential evidence handling (encryption, restricted access)
        public bool ConfidentialEvidenceEncryption { get; set; } = true;
        public string ConfidentialEvidenceAccessJson { get; set; } = "[]";

        // ============================================================================
        // SECTION K: Baseline + Overlays Selection (Questions 88-90)
        // ============================================================================

        // K88: Adopt default baseline control set? (yes/no)
        public bool AdoptDefaultBaseline { get; set; } = true;

        // K89: Select overlays to enable (JSON array)
        public string SelectedOverlaysJson { get; set; } = "[]"; // jurisdiction, sector, data (PCI/PII), technology (cloud/on-prem/OT)

        // K90: Client-specific control requirements (JSON array or file reference)
        public bool HasClientSpecificControls { get; set; } = false;
        public string ClientSpecificControlsJson { get; set; } = "[]";

        // ============================================================================
        // SECTION L: Go-Live and Success Metrics (Questions 91-96)
        // ============================================================================

        // L91: Success definition in 90 days (JSON array of top 3)
        public string SuccessMetricsTop3Json { get; set; } = "[]"; // fewer_audit_hours, faster_evidence_turnaround, reduced_repeat_findings, improved_sla_compliance, improved_outage_recovery

        // L92: Baseline measurement: current audit prep hours/month
        public decimal? BaselineAuditPrepHoursPerMonth { get; set; }

        // L93: Baseline: average remediation closure time (days)
        public decimal? BaselineRemediationClosureDays { get; set; }

        // L94: Baseline: # overdue controls per month
        public int? BaselineOverdueControlsPerMonth { get; set; }

        // L95: Target improvement % for each metric (JSON object)
        public string TargetImprovementJson { get; set; } = "{}";

        // L96: Pilot scope: top 10–20 controls (JSON array of control IDs or domains)
        public string PilotScopeJson { get; set; } = "[]";

        // ============================================================================
        // WIZARD METADATA
        // ============================================================================

        /// <summary>
        /// Current step in the wizard (1-12 corresponding to sections A-L)
        /// </summary>
        public int CurrentStep { get; set; } = 1;

        /// <summary>
        /// Wizard status: NotStarted, InProgress, Completed, Cancelled
        /// </summary>
        [MaxLength(50)]
        public string WizardStatus { get; set; } = "NotStarted";

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int ProgressPercent { get; set; } = 0;

        /// <summary>
        /// Sections completed (JSON array of section letters)
        /// </summary>
        public string CompletedSectionsJson { get; set; } = "[]";

        /// <summary>
        /// Validation errors (JSON object of {field: error})
        /// </summary>
        public string ValidationErrorsJson { get; set; } = "{}";

        /// <summary>
        /// When the wizard was started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When the wizard was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Who completed the wizard (user ID)
        /// </summary>
        [MaxLength(100)]
        public string CompletedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Last step saved date
        /// </summary>
        public DateTime? LastStepSavedAt { get; set; }

        /// <summary>
        /// Raw JSON of all answers for audit/replay
        /// </summary>
        public string AllAnswersJson { get; set; } = "{}";

        /// <summary>
        /// Organization maturity level (service compatibility)
        /// </summary>
        [MaxLength(50)]
        public string? MaturityLevel { get; set; }

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Wizard Step Definition - Maps to sections A-L
    /// </summary>
    public static class OnboardingWizardSteps
    {
        public const int OrganizationIdentity = 1;       // Section A
        public const int AssuranceObjective = 2;         // Section B
        public const int RegulatoryApplicability = 3;    // Section C
        public const int ScopeDefinition = 4;            // Section D
        public const int DataRiskProfile = 5;            // Section E
        public const int TechnologyLandscape = 6;        // Section F
        public const int ControlOwnershipModel = 7;      // Section G
        public const int TeamsRolesAccess = 8;           // Section H
        public const int WorkflowCadence = 9;            // Section I
        public const int EvidenceStandards = 10;         // Section J
        public const int BaselineOverlays = 11;          // Section K
        public const int GoLiveMetrics = 12;             // Section L

        public static readonly Dictionary<int, string> StepNames = new()
        {
            { 1, "Organization Identity & Tenancy" },
            { 2, "Assurance Objective" },
            { 3, "Regulatory & Framework Applicability" },
            { 4, "Scope Definition" },
            { 5, "Data & Risk Profile" },
            { 6, "Technology Landscape" },
            { 7, "Control Ownership Model" },
            { 8, "Teams, Roles & Access" },
            { 9, "Workflow & Cadence" },
            { 10, "Evidence Standards" },
            { 11, "Baseline & Overlays Selection" },
            { 12, "Go-Live & Success Metrics" }
        };

        public static readonly Dictionary<int, string> StepNamesAr = new()
        {
            { 1, "هوية المنظمة والمستأجر" },
            { 2, "هدف التأكيد" },
            { 3, "الأطر التنظيمية المطبقة" },
            { 4, "تحديد النطاق" },
            { 5, "ملف البيانات والمخاطر" },
            { 6, "المشهد التقني" },
            { 7, "نموذج ملكية الضوابط" },
            { 8, "الفرق والأدوار والوصول" },
            { 9, "سير العمل والتكرار" },
            { 10, "معايير الأدلة" },
            { 11, "اختيار الأساس والطبقات" },
            { 12, "مقاييس الإطلاق والنجاح" }
        };

        public static readonly Dictionary<int, string> StepIcons = new()
        {
            { 1, "fa-building" },
            { 2, "fa-bullseye" },
            { 3, "fa-gavel" },
            { 4, "fa-crosshairs" },
            { 5, "fa-shield-alt" },
            { 6, "fa-server" },
            { 7, "fa-users-cog" },
            { 8, "fa-user-friends" },
            { 9, "fa-project-diagram" },
            { 10, "fa-file-contract" },
            { 11, "fa-layer-group" },
            { 12, "fa-rocket" }
        };

        /// <summary>
        /// Required fields per step (for validation)
        /// </summary>
        public static readonly Dictionary<int, string[]> RequiredFields = new()
        {
            { 1, new string[] { "OrganizationLegalNameEn", "CountryOfIncorporation", "OrganizationType", "IndustrySector" } },
            { 2, new string[] { "PrimaryDriver" } },
            { 3, Array.Empty<string>() }, // No strictly required fields, derived from sector
            { 4, Array.Empty<string>() }, // Optional but recommended
            { 5, new string[] { "DataTypesProcessedJson" } },
            { 6, Array.Empty<string>() }, // Optional integrations
            { 7, new string[] { "ControlOwnershipApproach" } },
            { 8, new string[] { "OrgAdminsJson" } },
            { 9, Array.Empty<string>() }, // Defaults are provided
            { 10, Array.Empty<string>() }, // Defaults are provided
            { 11, Array.Empty<string>() }, // Defaults are provided
            { 12, Array.Empty<string>() } // Optional metrics
        };

        public static int TotalSteps => 12;
    }
}
