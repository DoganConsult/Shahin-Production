//=============================================================================
// SHAHIN GRC PLATFORM - COMPLETE DTO COLLECTION
// ABP Framework Multi-Tenant GRC with Autonomous Onboarding
// Author: Ahmet (PhD IoT Healthcare Cybersecurity)
// Version: 1.0
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace Shahin.Grc.Dtos
{
    //=============================================================================
    // 1. TRIAL & TENANT BOOTSTRAP DTOs
    //=============================================================================
    
    public class TrialRegistrationDto
    {
        [Required]
        [StringLength(100)]
        public string OrganizationName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string AdminEmail { get; set; }
        
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(100)]
        public string JobTitle { get; set; }
        
        // Fraud Detection Fields
        public string DeviceFingerprint { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string GeoLocation { get; set; }
        
        // CAPTCHA
        [Required]
        public string CaptchaToken { get; set; }
        
        // Legal Agreements
        [Required]
        public bool AcceptTerms { get; set; }
        
        [Required]
        public bool AcceptPrivacy { get; set; }
        
        public bool AcceptMarketing { get; set; }
    }
    
    public class TrialCreationResultDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public Guid AdminUserId { get; set; }
        public DateTime TrialExpiryDate { get; set; }
        public OnboardingStatus OnboardingStatus { get; set; }
        public string RedirectUrl { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public bool IsQuarantined { get; set; }
        public string QuarantineReason { get; set; }
    }
    
    public class TenantBootstrapDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public TrialTier Tier { get; set; }
        public Dictionary<string, bool> FeatureFlags { get; set; }
        public List<string> AllowedModules { get; set; }
        public SecurityProfile SecurityProfile { get; set; }
    }

    //=============================================================================
    // 2. ONBOARDING STATE & WIZARD DTOs
    //=============================================================================
    
    public class OnboardingStateDto
    {
        public Guid TenantId { get; set; }
        public OnboardingStatus Status { get; set; }
        public string CurrentStep { get; set; }
        public string NextStep { get; set; }
        public string ResumeUrl { get; set; }
        public DateTime LastActivityAt { get; set; }
        public float CompletionPercentage { get; set; }
        public List<OnboardingStepDto> Steps { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
    
    public class OnboardingStepDto
    {
        public string StepId { get; set; }
        public string StepName { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> Dependencies { get; set; }
        public Dictionary<string, object> Outputs { get; set; }
    }
    
    public class FastStartDto
    {
        // Basic Organization Info
        [Required]
        [StringLength(100)]
        public string OrganizationName { get; set; }
        
        [Required]
        public string IncorporationCountry { get; set; }
        
        [Required]
        public List<string> OperatingCountries { get; set; }
        
        [Required]
        public OrganizationSector Sector { get; set; }
        
        [StringLength(50)]
        public string SubSector { get; set; }
        
        [Required]
        public List<DataType> DataTypes { get; set; }
        
        [Required]
        public HostingModel HostingModel { get; set; }
        
        public List<string> CloudProviders { get; set; }
        
        [Required]
        public ComplianceDriver PrimaryDriver { get; set; }
        
        [Required]
        public string Timeline { get; set; }
        
        // Size Indicators
        public int? EmployeeCount { get; set; }
        public decimal? AnnualRevenue { get; set; }
        public string RevenueCurrency { get; set; } = "SAR";
    }
    
    public class FastStartResultDto
    {
        public List<ApplicableFrameworkDto> BaselineFrameworks { get; set; }
        public int EstimatedControlCount { get; set; }
        public string NextRecommendedStep { get; set; }
        public List<string> RequiredMissions { get; set; }
        public ComplianceComplexityLevel ComplexityLevel { get; set; }
        public List<RuleEvaluationDto> RuleEvaluations { get; set; }
    }

    //=============================================================================
    // 3. MISSION DTOs (Onboarding Stages)
    //=============================================================================
    
    public class Mission1ScopeDto
    {
        // Scope Definition
        [Required]
        public List<EntityScopeDto> Entities { get; set; }
        
        [Required]
        public List<SystemScopeDto> Systems { get; set; }
        
        [Required]
        public List<ProcessScopeDto> Processes { get; set; }
        
        public List<LocationScopeDto> Locations { get; set; }
        
        // Exclusions with Rationale
        public List<ScopeExclusionDto> Exclusions { get; set; }
        
        // Data Classification
        public List<DataClassificationDto> DataClassifications { get; set; }
    }
    
    public class Mission2OwnershipDto
    {
        // Team Structure
        [Required]
        public List<TeamDefinitionDto> Teams { get; set; }
        
        // Role Assignments
        [Required]
        public List<RoleAssignmentDto> RoleAssignments { get; set; }
        
        // RACI Matrix
        [Required]
        public RACIMatrixDto RaciMatrix { get; set; }
        
        // SLA Definitions
        [Required]
        public List<SLADefinitionDto> SlaDefinitions { get; set; }
        
        // Approval Workflows
        public List<ApprovalWorkflowDto> ApprovalWorkflows { get; set; }
    }
    
    public class Mission3EvidenceDto
    {
        // Evidence Standards
        [Required]
        public List<EvidenceStandardDto> EvidenceStandards { get; set; }
        
        // KPI Baselines
        [Required]
        public List<KpiBaselineDto> KpiBaselines { get; set; }
        
        // Success Metrics
        [Required]
        public List<SuccessMetricDto> SuccessMetrics { get; set; }
        
        // Retention Policies
        public List<RetentionPolicyDto> RetentionPolicies { get; set; }
        
        // Dashboard Configuration
        public DashboardConfigurationDto DashboardConfiguration { get; set; }
    }

    //=============================================================================
    // 4. COMPLIANCE CATALOG DTOs
    //=============================================================================
    
    public class RegulatorDto : EntityDto<Guid>
    {
        public string Code { get; set; } // NCA, SAMA, CBB
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Jurisdiction { get; set; }
        public string Website { get; set; }
        public ContactInfoDto ContactInfo { get; set; }
        public List<string> MandatorySectors { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class FrameworkDto : EntityDto<Guid>
    {
        public Guid RegulatorId { get; set; }
        public string RegulatorName { get; set; }
        public string Code { get; set; } // NCA-ECC, SAMA-CSF
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public List<string> MandatorySectors { get; set; }
        public List<string> ApplicableCountries { get; set; }
        public FrameworkType Type { get; set; }
        public int ControlCount { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string ImplementationGuidance { get; set; }
    }
    
    public class ControlDto : EntityDto<Guid>
    {
        public Guid FrameworkId { get; set; }
        public string FrameworkCode { get; set; }
        public string ControlId { get; set; } // AC-1, SC-7
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImplementationGuidance { get; set; }
        public List<string> Domains { get; set; } // Access, Security, Risk
        public ControlCriticality Criticality { get; set; }
        public List<EvidenceRequirementDto> EvidenceRequirements { get; set; }
        public List<string> Tags { get; set; }
        public bool IsOptional { get; set; }
        public string Rationale { get; set; }
    }
    
    public class ApplicableFrameworkDto
    {
        public Guid FrameworkId { get; set; }
        public string FrameworkCode { get; set; }
        public string FrameworkName { get; set; }
        public string ApplicabilityReason { get; set; }
        public bool IsMandatory { get; set; }
        public List<string> TriggerConditions { get; set; }
        public List<OverlayRuleDto> OverlayRules { get; set; }
        public int ControlCount { get; set; }
        public ComplianceComplexityLevel ComplexityLevel { get; set; }
    }

    //=============================================================================
    // 5. RULES ENGINE DTOs
    //=============================================================================
    
    public class RuleEvaluationDto
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleVersion { get; set; }
        public DateTime EvaluatedAt { get; set; }
        public string InputsHash { get; set; }
        public List<RuleConditionDto> MatchedConditions { get; set; }
        public Dictionary<string, object> Outputs { get; set; }
        public float ConfidenceScore { get; set; }
        public string ExplainabilityPayload { get; set; }
        public string HumanReadableRationale { get; set; }
    }
    
    public class RuleConditionDto
    {
        public string ConditionId { get; set; }
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public bool IsMatched { get; set; }
        public string MatchReason { get; set; }
    }
    
    public class OverlayRuleDto
    {
        public string OverlayId { get; set; }
        public string OverlayName { get; set; }
        public string TriggerCondition { get; set; }
        public List<string> AdditionalFrameworks { get; set; }
        public List<string> AdditionalControls { get; set; }
        public Dictionary<string, object> ModificationRules { get; set; }
    }

    //=============================================================================
    // 6. SCOPE & BOUNDARY DTOs
    //=============================================================================
    
    public class EntityScopeDto
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public EntityType EntityType { get; set; }
        public CriticalityLevel CriticalityLevel { get; set; }
        public List<string> Tags { get; set; }
        public bool IsInScope { get; set; }
        public string ScopeRationale { get; set; }
    }
    
    public class SystemScopeDto
    {
        public string SystemId { get; set; }
        public string SystemName { get; set; }
        public SystemType SystemType { get; set; }
        public HostingLocation HostingLocation { get; set; }
        public List<string> DataTypes { get; set; }
        public CriticalityLevel CriticalityLevel { get; set; }
        public bool IsInScope { get; set; }
        public string ScopeRationale { get; set; }
        public List<string> ConnectedSystems { get; set; }
    }
    
    public class ProcessScopeDto
    {
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string ProcessOwner { get; set; }
        public List<string> InvolvedSystems { get; set; }
        public List<string> DataTypes { get; set; }
        public CriticalityLevel CriticalityLevel { get; set; }
        public bool IsInScope { get; set; }
        public string ScopeRationale { get; set; }
    }
    
    public class ScopeExclusionDto
    {
        public string ExclusionId { get; set; }
        public string EntityName { get; set; }
        public EntityType EntityType { get; set; }
        public string ExclusionReason { get; set; }
        public string ApprovalAuthority { get; set; }
        public DateTime ExclusionDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool RequiresPeriodicReview { get; set; }
    }

    //=============================================================================
    // 7. TEAM & OWNERSHIP DTOs
    //=============================================================================
    
    public class TeamDefinitionDto
    {
        public string TeamId { get; set; }
        public string TeamName { get; set; }
        public string Description { get; set; }
        public string TeamType { get; set; } // Governance, IT, Legal, Business
        public List<TeamMemberDto> Members { get; set; }
        public List<string> Responsibilities { get; set; }
        public string EscalationPath { get; set; }
    }
    
    public class TeamMemberDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public List<string> Roles { get; set; }
        public bool IsPrimary { get; set; }
        public bool ReceivesNotifications { get; set; }
    }
    
    public class RoleAssignmentDto
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public List<string> Permissions { get; set; }
        public List<string> ControlDomains { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class RACIMatrixDto
    {
        public Dictionary<string, Dictionary<string, RACIRole>> Matrix { get; set; }
        // Key1: Control/Process, Key2: Role/User, Value: RACI Assignment
        public DateTime LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }

    //=============================================================================
    // 8. ASSESSMENT & PLAN DTOs
    //=============================================================================
    
    public class GrcPlanDto : EntityDto<Guid>
    {
        public Guid TenantId { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; } // QuickScan, Comprehensive, Continuous
        public List<Guid> FrameworkIds { get; set; }
        public List<string> FrameworkNames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetCompletionDate { get; set; }
        public PlanStatus Status { get; set; }
        public List<AssessmentTemplateDto> AssessmentTemplates { get; set; }
        public List<string> Milestones { get; set; }
        public float CompletionPercentage { get; set; }
    }
    
    public class AssessmentTemplateDto : EntityDto<Guid>
    {
        public Guid PlanId { get; set; }
        public Guid FrameworkId { get; set; }
        public string FrameworkName { get; set; }
        public string TemplateName { get; set; }
        public List<ControlAssignmentDto> ControlAssignments { get; set; }
        public string Schedule { get; set; } // Quarterly, Annual, Continuous
        public DateTime NextDueDate { get; set; }
        public string AssessmentType { get; set; } // Self, External, Automated
        public bool IsActive { get; set; }
    }
    
    public class ControlAssignmentDto
    {
        public Guid ControlId { get; set; }
        public string ControlCode { get; set; }
        public string ControlTitle { get; set; }
        public Guid AssignedToUserId { get; set; }
        public string AssignedToEmail { get; set; }
        public DateTime DueDate { get; set; }
        public ControlCriticality Criticality { get; set; }
        public AssessmentStatus Status { get; set; }
        public List<EvidenceRequestDto> EvidenceRequests { get; set; }
    }

    //=============================================================================
    // 9. EVIDENCE & WORKFLOW DTOs
    //=============================================================================
    
    public class EvidenceRequestDto : EntityDto<Guid>
    {
        public Guid ControlId { get; set; }
        public Guid AssessmentId { get; set; }
        public string EvidenceType { get; set; } // Document, Screenshot, Configuration
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime DueDate { get; set; }
        public Guid RequestedFromUserId { get; set; }
        public string RequestedFromEmail { get; set; }
        public EvidenceStatus Status { get; set; }
        public List<EvidenceSubmissionDto> Submissions { get; set; }
        public string ValidationCriteria { get; set; }
    }
    
    public class EvidenceSubmissionDto : EntityDto<Guid>
    {
        public Guid EvidenceRequestId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string FileHash { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Guid SubmittedByUserId { get; set; }
        public string SubmittedByEmail { get; set; }
        public string Comments { get; set; }
        public EvidenceValidationStatus ValidationStatus { get; set; }
        public string ValidationComments { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }
    }
    
    public class EvidenceStandardDto
    {
        public string StandardId { get; set; }
        public string StandardName { get; set; }
        public string EvidenceType { get; set; }
        public List<string> AcceptableFormats { get; set; }
        public string NamingConvention { get; set; }
        public int MaxFileSize { get; set; } // in MB
        public string RetentionPeriod { get; set; }
        public bool RequiresApproval { get; set; }
        public List<string> ValidationRules { get; set; }
    }

    //=============================================================================
    // 10. FINDINGS & REMEDIATION DTOs
    //=============================================================================
    
    public class FindingDto : EntityDto<Guid>
    {
        public Guid ControlId { get; set; }
        public Guid AssessmentId { get; set; }
        public string ControlCode { get; set; }
        public string FindingTitle { get; set; }
        public string FindingDescription { get; set; }
        public FindingSeverity Severity { get; set; }
        public FindingType FindingType { get; set; }
        public DateTime IdentifiedAt { get; set; }
        public Guid IdentifiedByUserId { get; set; }
        public FindingStatus Status { get; set; }
        public DateTime? TargetRemediationDate { get; set; }
        public List<RemediationActionDto> RemediationActions { get; set; }
        public string RiskStatement { get; set; }
        public string BusinessImpact { get; set; }
    }
    
    public class RemediationActionDto : EntityDto<Guid>
    {
        public Guid FindingId { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public Guid AssignedToUserId { get; set; }
        public string AssignedToEmail { get; set; }
        public DateTime DueDate { get; set; }
        public ActionStatus Status { get; set; }
        public string Priority { get; set; }
        public decimal? EstimatedCost { get; set; }
        public int? EstimatedHours { get; set; }
        public List<string> Dependencies { get; set; }
        public string ProgressNotes { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
    
    public class ExceptionRequestDto : EntityDto<Guid>
    {
        public Guid ControlId { get; set; }
        public Guid? FindingId { get; set; }
        public string ControlCode { get; set; }
        public string ExceptionTitle { get; set; }
        public string ExceptionRationale { get; set; }
        public string CompensatingControls { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid RequestedByUserId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ExceptionStatus Status { get; set; }
        public List<ExceptionApprovalDto> Approvals { get; set; }
        public string RiskAcceptanceStatement { get; set; }
    }

    //=============================================================================
    // 11. DASHBOARD & REPORTING DTOs
    //=============================================================================
    
    public class DashboardConfigurationDto
    {
        public List<DashboardWidgetDto> Widgets { get; set; }
        public string Layout { get; set; } // Grid, Tabbed, Accordion
        public List<string> UserRoles { get; set; }
        public string RefreshInterval { get; set; }
        public bool IsDefault { get; set; }
    }
    
    public class DashboardWidgetDto
    {
        public string WidgetId { get; set; }
        public string WidgetType { get; set; } // Chart, Table, Metric, Progress
        public string Title { get; set; }
        public string DataSource { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
        public Position Position { get; set; }
        public Size Size { get; set; }
    }
    
    public class KpiBaselineDto
    {
        public string KpiId { get; set; }
        public string KpiName { get; set; }
        public string Category { get; set; } // Compliance, Risk, Efficiency
        public decimal BaselineValue { get; set; }
        public string Unit { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? ThresholdWarning { get; set; }
        public decimal? ThresholdCritical { get; set; }
        public string CalculationMethod { get; set; }
        public string DataSource { get; set; }
        public DateTime BaselinedAt { get; set; }
    }
    
    public class ComplianceScoreDto
    {
        public Guid FrameworkId { get; set; }
        public string FrameworkName { get; set; }
        public float OverallScore { get; set; } // 0-100
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
        public int NonCompliantControls { get; set; }
        public int NotAssessedControls { get; set; }
        public List<DomainScoreDto> DomainScores { get; set; }
        public DateTime LastAssessedAt { get; set; }
        public string TrendDirection { get; set; } // Improving, Declining, Stable
    }

    //=============================================================================
    // 12. AUDIT TRAIL & EXPLAINABILITY DTOs
    //=============================================================================
    
    public class AuditEventDto : EntityDto<Guid>
    {
        public Guid TenantId { get; set; }
        public string EventType { get; set; }
        public string EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string Action { get; set; }
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> BeforeValues { get; set; }
        public Dictionary<string, object> AfterValues { get; set; }
        public string Reason { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
    
    public class ExplainabilityDto
    {
        public string DecisionId { get; set; }
        public string DecisionType { get; set; }
        public DateTime DecisionTimestamp { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public List<RuleEvaluationDto> RulesEvaluated { get; set; }
        public Dictionary<string, object> Outputs { get; set; }
        public string HumanReadableExplanation { get; set; }
        public float ConfidenceLevel { get; set; }
        public List<string> AlternativeOptions { get; set; }
        public string RecommendationRationale { get; set; }
    }
    
    public class AuditReplayRequestDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? UserId { get; set; }
        public List<string> EventTypes { get; set; }
        public List<string> EntityTypes { get; set; }
        public string Query { get; set; } // "Why was PDPL applied?"
        public bool IncludeExplanations { get; set; }
        public string OutputFormat { get; set; } // JSON, PDF, Excel
    }

    //=============================================================================
    // 13. ENUMERATIONS & VALUE OBJECTS
    //=============================================================================
    
    public enum OnboardingStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Expired = 3,
        Quarantined = 4
    }
    
    public enum TrialTier
    {
        Basic = 0,
        Standard = 1,
        Premium = 2,
        Enterprise = 3
    }
    
    public enum OrganizationSector
    {
        Government = 1,
        Banking = 2,
        Healthcare = 3,
        Education = 4,
        Energy = 5,
        Technology = 6,
        Manufacturing = 7,
        Retail = 8,
        Telecommunications = 9,
        Transportation = 10,
        Other = 99
    }
    
    public enum DataType
    {
        PII = 1,
        FinancialData = 2,
        HealthData = 3,
        PaymentCardData = 4,
        IntellectualProperty = 5,
        GovernmentData = 6,
        BiometricData = 7,
        LocationData = 8
    }
    
    public enum HostingModel
    {
        OnPremise = 1,
        Cloud = 2,
        Hybrid = 3,
        MultiCloud = 4
    }
    
    public enum ComplianceDriver
    {
        RegulatoryRequirement = 1,
        CustomerRequirement = 2,
        BusinessStrategy = 3,
        RiskManagement = 4,
        CertificationGoal = 5,
        InvestorRequirement = 6
    }
    
    public enum ControlCriticality
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    public enum CriticalityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    public enum RACIRole
    {
        Responsible = 1, // Does the work
        Accountable = 2, // Signs off on the work
        Consulted = 3,   // Provides input
        Informed = 4     // Kept informed
    }
    
    public enum AssessmentStatus
    {
        NotStarted = 0,
        InProgress = 1,
        UnderReview = 2,
        Completed = 3,
        RequiresAttention = 4
    }
    
    public enum EvidenceStatus
    {
        Requested = 0,
        InProgress = 1,
        Submitted = 2,
        UnderReview = 3,
        Approved = 4,
        Rejected = 5,
        Expired = 6
    }
    
    public enum FindingSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    public enum FindingStatus
    {
        Open = 1,
        InProgress = 2,
        Resolved = 3,
        Accepted = 4, // Risk Accepted
        Closed = 5
    }

    //=============================================================================
    // 14. SUPPORTING VALUE OBJECTS
    //=============================================================================
    
    public class ContactInfoDto
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public AddressDto Address { get; set; }
    }
    
    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
    
    public class SecurityProfile
    {
        public int MaxUsers { get; set; }
        public int MaxFrameworks { get; set; }
        public int MaxAssessments { get; set; }
        public bool AllowApiAccess { get; set; }
        public bool AllowIntegrations { get; set; }
        public bool AllowExports { get; set; }
        public List<string> RestrictedFeatures { get; set; }
    }
    
    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
    
    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    
    public class DomainScoreDto
    {
        public string Domain { get; set; }
        public float Score { get; set; }
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
    }
    
    public class EvidenceRequirementDto
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public string Frequency { get; set; }
        public List<string> AcceptableFormats { get; set; }
    }
    
    public class SLADefinitionDto
    {
        public string SlaId { get; set; }
        public string SlaName { get; set; }
        public string EntityType { get; set; } // Control, Finding, Evidence
        public ControlCriticality ApplicableCriticality { get; set; }
        public int ResponseTimeHours { get; set; }
        public int ResolutionTimeDays { get; set; }
        public List<string> EscalationPath { get; set; }
    }
    
    public class ApprovalWorkflowDto
    {
        public string WorkflowId { get; set; }
        public string WorkflowName { get; set; }
        public string TriggerType { get; set; }
        public List<ApprovalStepDto> Steps { get; set; }
    }
    
    public class ApprovalStepDto
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public List<Guid> ApproverUserIds { get; set; }
        public bool RequiresAllApprovers { get; set; }
        public int TimeoutDays { get; set; }
    }
    
    public class ExceptionApprovalDto
    {
        public Guid ApproverUserId { get; set; }
        public string ApproverEmail { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsApproved { get; set; }
        public string Comments { get; set; }
    }
    
    public class RetentionPolicyDto
    {
        public string PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string EvidenceType { get; set; }
        public int RetentionYears { get; set; }
        public string DisposalMethod { get; set; }
        public bool RequiresApprovalForDisposal { get; set; }
    }
    
    public class SuccessMetricDto
    {
        public string MetricId { get; set; }
        public string MetricName { get; set; }
        public string Category { get; set; }
        public decimal TargetValue { get; set; }
        public string Unit { get; set; }
        public string MeasurementMethod { get; set; }
        public string ReportingFrequency { get; set; }
    }
    
    public class DataClassificationDto
    {
        public string DataCategory { get; set; }
        public string ClassificationLevel { get; set; } // Public, Internal, Confidential, Restricted
        public List<string> HandlingRequirements { get; set; }
        public List<string> ApplicableControls { get; set; }
    }
    
    public class LocationScopeDto
    {
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationType { get; set; } // Office, DataCenter, Remote
        public string Country { get; set; }
        public string City { get; set; }
        public bool IsInScope { get; set; }
        public string ScopeRationale { get; set; }
    }

    //=============================================================================
    // 15. RESPONSE & RESULT DTOs
    //=============================================================================
    
    public class OnboardingCompletionResultDto
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public GrcPlanDto GeneratedPlan { get; set; }
        public List<AssessmentTemplateDto> GeneratedAssessments { get; set; }
        public List<string> GeneratedWorkflows { get; set; }
        public DashboardConfigurationDto GeneratedDashboard { get; set; }
        public string WorkspaceUrl { get; set; }
        public List<string> NextRecommendedActions { get; set; }
    }
    
    public class RuleEngineResultDto
    {
        public bool IsSuccessful { get; set; }
        public List<ApplicableFrameworkDto> ApplicableFrameworks { get; set; }
        public List<ControlDto> ResolvedControls { get; set; }
        public List<RuleEvaluationDto> RuleEvaluations { get; set; }
        public Dictionary<string, object> ComputedProperties { get; set; }
        public string Summary { get; set; }
        public List<string> Recommendations { get; set; }
    }
    
    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<ValidationErrorDto> Errors { get; set; }
        public List<ValidationWarningDto> Warnings { get; set; }
        public Dictionary<string, object> ComputedValues { get; set; }
    }
    
    public class ValidationErrorDto
    {
        public string Field { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public object AttemptedValue { get; set; }
    }
    
    public class ValidationWarningDto
    {
        public string Field { get; set; }
        public string WarningCode { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
    }
}

//=============================================================================
// END OF DTO COLLECTION
//=============================================================================
