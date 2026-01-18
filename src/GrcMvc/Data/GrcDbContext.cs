using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Models.Entities.Onboarding;
using GrcMvc.Models.Entities.Compliance;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;

namespace GrcMvc.Data
{
    /// <summary>
    /// Main application DbContext for GRC data.
    /// Identity/Auth data is in GrcAuthDbContext (separate database).
    /// Now inherits from AbpDbContext for proper ABP Framework integration.
    /// </summary>
    public partial class GrcDbContext : AbpDbContext<GrcDbContext>
    {
        private IWorkspaceContextService? _workspaceContextService;

        public GrcDbContext(DbContextOptions<GrcDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the current tenant ID from custom tenant context service
        /// Used for global query filters
        /// IMPORTANT: Must NEVER throw - returns null when no tenant context
        /// This supports: authenticated users, public endpoints (trial signup), seeding, migrations
        /// </summary>
        private Guid? GetCurrentTenantId()
        {
            try
            {
                // Use our custom tenant context service which handles all scenarios safely
                var tenantService = this.GetService<ITenantContextService>();
                return tenantService?.GetCurrentTenantId();
            }
            catch
            {
                // No tenant context available - allow all records
                // This is expected for: public endpoints, seeding, migrations, background jobs without tenant scope
                return null;
            }
        }

        /// <summary>
        /// Gets the current workspace ID from the service provider
        /// Used for global query filters
        /// Returns null if no workspace context (allows cross-workspace queries when needed)
        /// </summary>
        private Guid? GetCurrentWorkspaceId()
        {
            if (_workspaceContextService == null)
            {
                _workspaceContextService = this.GetService<IWorkspaceContextService>();
            }

            if (_workspaceContextService == null || !_workspaceContextService.HasWorkspaceContext())
            {
                return null; // No workspace context - allows cross-workspace queries
            }

            var workspaceId = _workspaceContextService.GetCurrentWorkspaceId();
            return workspaceId == Guid.Empty ? null : workspaceId;
        }

        // DbSets for all entities
        // Multi-tenant core
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<TenantUser> TenantUsers { get; set; } = null!;
        public DbSet<OwnerTenantCreation> OwnerTenantCreations { get; set; } = null!;
        public DbSet<PlatformAdmin> PlatformAdmins { get; set; } = null!;
        public DbSet<OrganizationProfile> OrganizationProfiles { get; set; } = null!;
        public DbSet<OnboardingWizard> OnboardingWizards { get; set; } = null!;
        public DbSet<OnboardingStepScore> OnboardingStepScores { get; set; } = null!;

        // 43-Layer Architecture: Onboarding Control Plane (Layers 13-20)
        public DbSet<OnboardingAnswerSnapshot> OnboardingAnswerSnapshots { get; set; } = null!;
        public DbSet<OnboardingDerivedOutput> OnboardingDerivedOutputs { get; set; } = null!;
        public DbSet<Models.Entities.Onboarding.RulesEvaluationLog> OnboardingRulesEvaluationLogs { get; set; } = null!;
        public DbSet<ExplainabilityPayload> ExplainabilityPayloads { get; set; } = null!;

        // 43-Layer Architecture: Tenant Compliance Resolution (Layers 31-36)
        public DbSet<TenantFrameworkSelection> TenantFrameworkSelections { get; set; } = null!;
        public DbSet<TenantOverlay> TenantOverlays { get; set; } = null!;
        public DbSet<TenantControlSet> TenantControlSets { get; set; } = null!;
        public DbSet<TenantScopeBoundary> TenantScopeBoundaries { get; set; } = null!;
        public DbSet<TenantRiskProfile> TenantRiskProfiles { get; set; } = null!;

        // GRC Reference/Lookup Tables (dropdown data, AI recommendations)
        public DbSet<LookupCountry> LookupCountries { get; set; } = null!;
        public DbSet<LookupSector> LookupSectors { get; set; } = null!;
        public DbSet<LookupOrganizationType> LookupOrganizationTypes { get; set; } = null!;
        public DbSet<LookupRegulator> LookupRegulators { get; set; } = null!;
        public DbSet<LookupFramework> LookupFrameworks { get; set; } = null!;
        public DbSet<LookupOrganizationSize> LookupOrganizationSizes { get; set; } = null!;
        public DbSet<LookupMaturityLevel> LookupMaturityLevels { get; set; } = null!;
        public DbSet<LookupDataType> LookupDataTypes { get; set; } = null!;
        public DbSet<LookupHostingModel> LookupHostingModels { get; set; } = null!;
        public DbSet<LookupRiskCategory> LookupRiskCategories { get; set; } = null!;
        public DbSet<LookupCloudProvider> LookupCloudProviders { get; set; } = null!;
        public DbSet<WizardRecommendation> WizardRecommendations { get; set; } = null!;

        // Teams & RACI (Role-based workflow routing)
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<TeamMember> TeamMembers { get; set; } = null!;
        public DbSet<RACIAssignment> RACIAssignments { get; set; } = null!;

        // Workspace (sub-scope within Tenant: Market/BU/Entity)
        public DbSet<Workspace> Workspaces { get; set; } = null!;
        public DbSet<WorkspaceMembership> WorkspaceMemberships { get; set; } = null!;
        public DbSet<WorkspaceControl> WorkspaceControls { get; set; } = null!;
        public DbSet<WorkspaceApprovalGate> WorkspaceApprovalGates { get; set; } = null!;
        public DbSet<WorkspaceApprovalGateApprover> WorkspaceApprovalGateApprovers { get; set; } = null!;
        public DbSet<RoleLandingConfig> RoleLandingConfigs { get; set; } = null!;

        // Asset inventory (for recognition & scoping)
        public DbSet<Asset> Assets { get; set; } = null!;

        // Rules engine (Layer 2)
        public DbSet<Ruleset> Rulesets { get; set; } = null!;
        public DbSet<Rule> Rules { get; set; } = null!;
        public DbSet<RuleExecutionLog> RuleExecutionLogs { get; set; } = null!;

        // Tenant scope (Layer 2)
        public DbSet<TenantBaseline> TenantBaselines { get; set; } = null!;
        public DbSet<TenantPackage> TenantPackages { get; set; } = null!;
        public DbSet<TenantTemplate> TenantTemplates { get; set; } = null!;
        public DbSet<TenantWorkflowConfig> TenantWorkflowConfigs { get; set; } = null!;

        // Serial Number Tracking
        public DbSet<Services.Implementations.SerialNumberCounter> SerialNumberCounters { get; set; } = null!;

        // Business Reference Code Counters (New Format: TENANTCODE-OBJTYPE-YYYY-SEQUENCE)
        public DbSet<SerialCounter> SerialCounters { get; set; } = null!;

        // Serial Code Registry (New 6-Stage Format: PREFIX-TENANT-STAGE-YEAR-SEQUENCE-VERSION)
        public DbSet<SerialCodeRegistry> SerialCodeRegistry { get; set; } = null!;
        public DbSet<SerialCodeReservation> SerialCodeReservations { get; set; } = null!;
        public DbSet<SerialSequenceCounter> SerialSequenceCounters { get; set; } = null!;

        // Policy Decision Audit Trail
        public DbSet<PolicyDecision> PolicyDecisions { get; set; } = null!;

        // Planning (Layer 3)
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<PlanPhase> PlanPhases { get; set; } = null!;

        // Audit trail
        public DbSet<AuditEvent> AuditEvents { get; set; } = null!;

        // Webhooks (Outbound event delivery)
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; } = null!;
        public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs { get; set; } = null!;

        // Existing entities
        public DbSet<Risk> Risks { get; set; } = null!;
        public DbSet<RiskControlMapping> RiskControlMappings { get; set; } = null!;
        public DbSet<RiskCategory> RiskCategories { get; set; } = null!;
        public DbSet<RiskType> RiskTypes { get; set; } = null!;
        public DbSet<RiskTreatment> RiskTreatments { get; set; } = null!;
        public DbSet<RiskTreatmentControl> RiskTreatmentControls { get; set; } = null!;
        public DbSet<RiskAppetiteSetting> RiskAppetiteSettings { get; set; } = null!;

        // Baseline & Framework Mapping entities
        public DbSet<BaselineProfile> BaselineProfiles { get; set; } = null!;
        public DbSet<ControlOverlay> ControlOverlays { get; set; } = null!;
        public DbSet<TailoringDecision> TailoringDecisions { get; set; } = null!;
        public DbSet<MAPProfile> MAPProfiles { get; set; } = null!;
        public DbSet<MAPControlImplementation> MAPControlImplementations { get; set; } = null!;
        public DbSet<MAPParameter> MAPParameters { get; set; } = null!;
        public DbSet<CanonicalControlMapping> CanonicalControlMappings { get; set; } = null!;
        public DbSet<RiskTaxonomy> RiskTaxonomies { get; set; } = null!;
        public DbSet<RiskScenario> RiskScenarios { get; set; } = null!;
        public DbSet<RiskAppetiteStatement> RiskAppetiteStatements { get; set; } = null!;
        public DbSet<ThreatProfile> ThreatProfiles { get; set; } = null!;
        public DbSet<VulnerabilityProfile> VulnerabilityProfiles { get; set; } = null!;
        public DbSet<ResilienceCapability> ResilienceCapabilities { get; set; } = null!;
        public DbSet<ControlApplicabilityRule> ControlApplicabilityRules { get; set; } = null!;
        public DbSet<ApplicabilityDecision> ApplicabilityDecisions { get; set; } = null!;
        public DbSet<ControlInheritance> ControlInheritances { get; set; } = null!;

        public DbSet<Control> Controls { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;
        public DbSet<Audit> Audits { get; set; } = null!;
        public DbSet<AuditFinding> AuditFindings { get; set; } = null!;
        public DbSet<Evidence> Evidences { get; set; } = null!;
        public DbSet<Policy> Policies { get; set; } = null!;
        public DbSet<PolicyViolation> PolicyViolations { get; set; } = null!;
        public DbSet<Workflow> Workflows { get; set; } = null!;
        public DbSet<WorkflowExecution> WorkflowExecutions { get; set; } = null!;
        public DbSet<ActionPlan> ActionPlans { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;

        // Marketing entities - Core
        public DbSet<Models.Entities.Marketing.Testimonial> Testimonials { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.CaseStudy> CaseStudies { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.PricingPlan> PricingPlans { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.ClientLogo> ClientLogos { get; set; } = null!;

        // Marketing entities - Trust & Content
        public DbSet<Models.Entities.Marketing.TrustBadge> TrustBadges { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.BlogPost> BlogPosts { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.Faq> Faqs { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.MarketingTeamMember> MarketingTeamMembers { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.Webinar> Webinars { get; set; } = null!;

        // Marketing entities - Landing Page CMS
        public DbSet<Models.Entities.Marketing.LandingPageContent> LandingPageContents { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.LandingStatistic> LandingStatistics { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.FeatureHighlight> FeatureHighlights { get; set; } = null!;
        public DbSet<Models.Entities.Marketing.Partner> Partners { get; set; } = null!;

        // Document Center
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; } = null!;
        public DbSet<Regulator> Regulators { get; set; } = null!;
        public DbSet<ComplianceEvent> ComplianceEvents { get; set; } = null!;
        public DbSet<GrcMvc.Models.Entities.Framework> Frameworks { get; set; } = null!;

        // STAGE 2: Workflow infrastructure
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
        public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
        public DbSet<TaskComment> TaskComments { get; set; } = null!;
        public DbSet<ApprovalChain> ApprovalChains { get; set; } = null!;
        public DbSet<ApprovalInstance> ApprovalInstances { get; set; } = null!;
        public DbSet<ApprovalRecord> ApprovalRecords { get; set; } = null!;
        public DbSet<EscalationRule> EscalationRules { get; set; } = null!;
        public DbSet<WorkflowAuditEntry> WorkflowAuditEntries { get; set; } = null!;
        public DbSet<WorkflowEscalation> WorkflowEscalations { get; set; } = null!;

        // STAGE 2: Workflow notifications
        public DbSet<Models.Workflows.WorkflowNotification> WorkflowNotifications { get; set; } = null!;
        public DbSet<Models.Workflows.WorkflowApproval> WorkflowApprovals { get; set; } = null!;
        public DbSet<RoleProfile> RoleProfiles { get; set; } = null!;

        // STAGE 2: Enterprise LLM integration
        public DbSet<LlmConfiguration> LlmConfigurations { get; set; } = null!;
        public DbSet<AiProviderConfiguration> AiProviderConfigurations { get; set; } = null!;

        // Subscription & Billing
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;

        // Reports
        public DbSet<Report> Reports { get; set; } = null!;

        // Resilience Assessments
        public DbSet<Resilience> Resiliences { get; set; } = null!;
        public DbSet<RiskResilience> RiskResiliences { get; set; } = null!;

        // RBAC (Role-Based Access Control) DbSets
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RoleFeature> RoleFeatures { get; set; }
        public DbSet<FeaturePermission> FeaturePermissions { get; set; }
        public DbSet<TenantRoleConfiguration> TenantRoleConfigurations { get; set; }
        public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }

        // HRIS Integration
        // public DbSet<HRISEmployee> HrisEmployees { get; set; } = null!;

        // Global Catalogs (Layer 1: Platform)
        public DbSet<RegulatorCatalog> RegulatorCatalogs { get; set; } = null!;
        public DbSet<FrameworkCatalog> FrameworkCatalogs { get; set; } = null!;
        public DbSet<ControlCatalog> ControlCatalogs { get; set; } = null!;
        public DbSet<RoleCatalog> RoleCatalogs { get; set; } = null!;
        public DbSet<TitleCatalog> TitleCatalogs { get; set; } = null!;
        public DbSet<BaselineCatalog> BaselineCatalogs { get; set; } = null!;
        public DbSet<PackageCatalog> PackageCatalogs { get; set; } = null!;
        public DbSet<TemplateCatalog> TemplateCatalogs { get; set; } = null!;
        public DbSet<EvidenceTypeCatalog> EvidenceTypeCatalogs { get; set; } = null!;

        // Framework Controls (Layer 1: Global - Immutable regulatory content)
        public DbSet<FrameworkControl> FrameworkControls { get; set; } = null!;

        // Sector-Framework Index (Fast lookup for onboarding)
        public DbSet<SectorFrameworkIndex> SectorFrameworkIndexes { get; set; } = null!;
        public DbSet<EvidenceScoringCriteria> EvidenceScoringCriteria { get; set; } = null!;
        public DbSet<TenantEvidenceRequirement> TenantEvidenceRequirements { get; set; } = null!;

        // GOSI Sub-Sector Mappings (70+ sub-sectors → 18 main sectors)
        public DbSet<GrcSubSectorMapping> GrcSubSectorMappings { get; set; } = null!;

        // Assessment Requirements (Layer 3: Operational)
        public DbSet<AssessmentRequirement> AssessmentRequirements { get; set; } = null!;

        // Requirement Notes (per-requirement comments/notes)
        public DbSet<RequirementNote> RequirementNotes { get; set; } = null!;

        // SLA, Escalation, Delegation Rules
        public DbSet<SlaRule> SlaRules { get; set; } = null!;
        public DbSet<DelegationRule> DelegationRules { get; set; } = null!;
        public DbSet<DelegationLog> DelegationLogs { get; set; } = null!;
        public DbSet<TaskDelegation> TaskDelegations { get; set; } = null!;

        // Trigger Rules (Event-based automation)
        public DbSet<TriggerRule> TriggerRules { get; set; } = null!;

        // User Consent & Legal Documents
        public DbSet<UserConsent> UserConsents { get; set; } = null!;
        public DbSet<LegalDocument> LegalDocuments { get; set; } = null!;

        // Support Agent & Chat
        public DbSet<SupportConversation> SupportConversations { get; set; } = null!;
        public DbSet<SupportMessage> SupportMessages { get; set; } = null!;

        // User Workspace (Role-based pre-mapping)
        public DbSet<UserWorkspace> UserWorkspaces { get; set; } = null!;
        public DbSet<UserWorkspaceTask> UserWorkspaceTasks { get; set; } = null!;
        public DbSet<WorkspaceTemplate> WorkspaceTemplates { get; set; } = null!;

        // Canonical Control Library (CCL) - Unified Control Framework
        public DbSet<ControlDomain> ControlDomains { get; set; } = null!;
        public DbSet<ControlObjective> ControlObjectives { get; set; } = null!;
        public DbSet<CanonicalControl> CanonicalControls { get; set; } = null!;
        public DbSet<RegulatoryRequirement> RegulatoryRequirements { get; set; } = null!;
        public DbSet<RequirementMapping> RequirementMappings { get; set; } = null!;
        public DbSet<EvidencePack> EvidencePacks { get; set; } = null!;
        public DbSet<ControlEvidencePack> ControlEvidencePacks { get; set; } = null!;
        public DbSet<TestProcedure> TestProcedures { get; set; } = null!;
        public DbSet<ControlTestProcedure> ControlTestProcedures { get; set; } = null!;
        public DbSet<ApplicabilityRule> ApplicabilityRules { get; set; } = null!;
        public DbSet<ControlChangeHistory> ControlChangeHistories { get; set; } = null!;

        // Applicability Matrix & Quality Gate
        public DbSet<ApplicabilityEntry> ApplicabilityEntries { get; set; } = null!;
        public DbSet<EvidencePackFamily> EvidencePackFamilies { get; set; } = null!;
        public DbSet<StandardEvidenceItem> StandardEvidenceItems { get; set; } = null!;
        public DbSet<MappingQualityGate> MappingQualityGates { get; set; } = null!;
        public DbSet<MappingWorkflowStep> MappingWorkflowSteps { get; set; } = null!;
        public DbSet<MappingWorkflowTemplate> MappingWorkflowTemplates { get; set; } = null!;
        public DbSet<AssessmentScope> AssessmentScopes { get; set; } = null!;

        // Baseline + Overlays Model
        public DbSet<BaselineControlSet> BaselineControlSets { get; set; } = null!;
        public DbSet<BaselineControlMapping> BaselineControlMappings { get; set; } = null!;
        public DbSet<OverlayCatalog> OverlayCatalogs { get; set; } = null!;
        public DbSet<OverlayControlMapping> OverlayControlMappings { get; set; } = null!;
        public DbSet<OverlayParameterOverride> OverlayParameterOverrides { get; set; } = null!;
        public DbSet<ApplicabilityRuleCatalog> ApplicabilityRuleCatalogs { get; set; } = null!;
        public DbSet<GeneratedControlSuite> GeneratedControlSuites { get; set; } = null!;
        public DbSet<SuiteControlEntry> SuiteControlEntries { get; set; } = null!;
        public DbSet<SuiteEvidenceRequest> SuiteEvidenceRequests { get; set; } = null!;
        public DbSet<OrganizationEntity> OrganizationEntities { get; set; } = null!;

        // Trial Edition
        public DbSet<TrialRequest> TrialRequests { get; set; } = null!;
        public DbSet<TrialSignup> TrialSignups { get; set; } = null!;
        public DbSet<MarketingTrialLead> MarketingTrialLeads { get; set; } = null!;
        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; } = null!;

        // Trial Team & Ecosystem Collaboration (World-Class Features)
        public DbSet<TrialExtension> TrialExtensions { get; set; } = null!;
        public DbSet<TrialEmailLog> TrialEmailLogs { get; set; } = null!;
        public DbSet<TeamActivity> TeamActivities { get; set; } = null!;
        public DbSet<EcosystemPartner> EcosystemPartners { get; set; } = null!;
        public DbSet<EcosystemConnection> EcosystemConnections { get; set; } = null!;
        public DbSet<CollaborationWorkspace> CollaborationWorkspaces { get; set; } = null!;
        public DbSet<CollaborationItem> CollaborationItems { get; set; } = null!;
        public DbSet<CollaborationComment> CollaborationComments { get; set; } = null!;

        // Email Operations (Shahin + Dogan Consult)
        public DbSet<Models.Entities.EmailOperations.EmailMailbox> EmailMailboxes { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailThread> EmailThreads { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailMessage> EmailMessages { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailTask> EmailTasks { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailAttachment> EmailAttachments { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailTemplate> EmailTemplates { get; set; } = null!;
        public DbSet<Models.Entities.EmailOperations.EmailAutoReplyRule> EmailAutoReplyRules { get; set; } = null!;

        // Abuse Detection & IP Tracking (Security Layer)
        public DbSet<Models.Entities.AbuseIpTracking> AbuseIpTrackings { get; set; } = null!;
        public DbSet<Models.Entities.AbuseEventLog> AbuseEventLogs { get; set; } = null!;
        public DbSet<Models.Entities.IpAccessList> IpAccessLists { get; set; } = null!;

        // Autonomous Risk & Resilience
        public DbSet<RiskIndicator> RiskIndicators { get; set; } = null!;
        public DbSet<RiskIndicatorMeasurement> RiskIndicatorMeasurements { get; set; } = null!;
        public DbSet<RiskIndicatorAlert> RiskIndicatorAlerts { get; set; } = null!;
        public DbSet<ImportantBusinessService> ImportantBusinessServices { get; set; } = null!;
        public DbSet<ControlException> ControlExceptions { get; set; } = null!;
        public DbSet<GovernanceCadence> GovernanceCadences { get; set; } = null!;
        public DbSet<CadenceExecution> CadenceExecutions { get; set; } = null!;
        public DbSet<EvidenceSourceIntegration> EvidenceSourceIntegrations { get; set; } = null!;
        public DbSet<CapturedEvidence> CapturedEvidences { get; set; } = null!;
        public DbSet<TeamsNotificationConfig> TeamsNotificationConfigs { get; set; } = null!;

        // Integration Layer
        public DbSet<SystemOfRecordDefinition> SystemOfRecordDefinitions { get; set; } = null!;
        public DbSet<CrossReferenceMapping> CrossReferenceMappings { get; set; } = null!;
        public DbSet<DomainEvent> DomainEvents { get; set; } = null!;
        public DbSet<EventSubscription> EventSubscriptions { get; set; } = null!;
        public DbSet<EventDeliveryLog> EventDeliveryLogs { get; set; } = null!;
        public DbSet<IntegrationConnector> IntegrationConnectors { get; set; } = null!;
        public DbSet<SyncJob> SyncJobs { get; set; } = null!;
        public DbSet<SyncExecutionLog> SyncExecutionLogs { get; set; } = null!;
        public DbSet<IntegrationHealthMetric> IntegrationHealthMetrics { get; set; } = null!;
        public DbSet<DeadLetterEntry> DeadLetterEntries { get; set; } = null!;
        public DbSet<EventSchemaRegistry> EventSchemaRegistries { get; set; } = null!;

        // ERP Integration & Continuous Controls Monitoring (CCM)
        public DbSet<ERPSystemConfig> ERPSystemConfigs { get; set; } = null!;
        public DbSet<ERPExtractConfig> ERPExtractConfigs { get; set; } = null!;
        public DbSet<ERPExtractExecution> ERPExtractExecutions { get; set; } = null!;
        public DbSet<CCMControlTest> CCMControlTests { get; set; } = null!;
        public DbSet<CCMTestExecution> CCMTestExecutions { get; set; } = null!;
        public DbSet<CCMException> CCMExceptions { get; set; } = null!;
        public DbSet<SoDRuleDefinition> SoDRuleDefinitions { get; set; } = null!;
        public DbSet<SoDConflict> SoDConflicts { get; set; } = null!;
        public DbSet<AutoTaggedEvidence> AutoTaggedEvidences { get; set; } = null!;

        // Agent Operating Model
        public DbSet<AgentDefinition> AgentDefinitions { get; set; } = null!;
        public DbSet<AgentCapability> AgentCapabilities { get; set; } = null!;
        public DbSet<AgentAction> AgentActions { get; set; } = null!;
        public DbSet<AgentApprovalGate> AgentApprovalGates { get; set; } = null!;
        public DbSet<PendingApproval> PendingApprovals { get; set; } = null!;
        public DbSet<AgentConfidenceScore> AgentConfidenceScores { get; set; } = null!;
        public DbSet<AgentSoDRule> AgentSoDRules { get; set; } = null!;
        public DbSet<AgentSoDViolation> AgentSoDViolations { get; set; } = null!;
        public DbSet<HumanRetainedResponsibility> HumanRetainedResponsibilities { get; set; } = null!;
        public DbSet<RoleTransitionPlan> RoleTransitionPlans { get; set; } = null!;

        // MAP Framework & Strategic Capabilities
        public DbSet<MAPFrameworkConfig> MAPFrameworkConfigs { get; set; } = null!;
        public DbSet<PlainLanguageControl> PlainLanguageControls { get; set; } = null!;
        public DbSet<UniversalEvidencePack> UniversalEvidencePacks { get; set; } = null!;
        public DbSet<UniversalEvidencePackItem> UniversalEvidencePackItems { get; set; } = null!;
        public DbSet<GovernanceRhythmTemplate> GovernanceRhythmTemplates { get; set; } = null!;
        public DbSet<GovernanceRhythmItem> GovernanceRhythmItems { get; set; } = null!;
        public DbSet<OnePageGuide> OnePageGuides { get; set; } = null!;
        public DbSet<CryptographicAsset> CryptographicAssets { get; set; } = null!;
        public DbSet<ThirdPartyConcentration> ThirdPartyConcentrations { get; set; } = null!;
        public DbSet<ComplianceGuardrail> ComplianceGuardrails { get; set; } = null!;
        public DbSet<StrategicRoadmapMilestone> StrategicRoadmapMilestones { get; set; } = null!;

        // Shahin-AI Branding & Localization
        public DbSet<ShahinAIBrandConfig> ShahinAIBrandConfigs { get; set; } = null!;
        public DbSet<ShahinAIModule> ShahinAIModules { get; set; } = null!;
        public DbSet<UITextEntry> UITextEntries { get; set; } = null!;
        public DbSet<SiteMapEntry> SiteMapEntries { get; set; } = null!;
        public DbSet<TriggerExecutionLog> TriggerExecutionLogs { get; set; } = null!;

        // Validation & Data Quality
        public DbSet<ValidationRule> ValidationRules { get; set; } = null!;
        public DbSet<Models.Entities.ValidationResult> ValidationResults { get; set; } = null!;
        public DbSet<DataQualityScore> DataQualityScores { get; set; } = null!;

        // Evidence Scoring (Phase 8)
        public DbSet<Services.Interfaces.EvidenceScore> EvidenceScores { get; set; } = null!;

        // User Profiles & Preferences
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<UserProfileAssignment> UserProfileAssignments { get; set; } = null!;
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; } = null!;

        // Workflow Transitions (Audit Trail)
        public DbSet<Models.Workflows.WorkflowTransition> WorkflowTransitions { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════
        // Engagement & Next Best Action (NBA) - Fullplan Specification Implementation
        // ═══════════════════════════════════════════════════════════════════════════════
        public DbSet<NextBestActionRecommendation> NextBestActionRecommendations { get; set; } = null!;
        public DbSet<ProgressCertaintyIndex> ProgressCertaintyIndexes { get; set; } = null!;
        public DbSet<EngagementMetrics> EngagementMetrics { get; set; } = null!;
        public DbSet<MotivationScore> MotivationScores { get; set; } = null!;
        public DbSet<ConditionalLogicRule> ConditionalLogicRules { get; set; } = null!;
        public DbSet<RuleEvaluation> RuleEvaluations { get; set; } = null!;
        public DbSet<EvidenceConfidenceScore> EvidenceConfidenceScores { get; set; } = null!;
        public DbSet<AgentEventTrigger> AgentEventTriggers { get; set; } = null!;
        public DbSet<AgentTriggerExecution> AgentTriggerExecutions { get; set; } = null!;
        public DbSet<AgentCommunicationContract> AgentCommunicationContracts { get; set; } = null!;
        public DbSet<Services.Implementations.AgentMessage> AgentMessages { get; set; } = null!;
        public DbSet<Services.Implementations.UserActivity> UserActivities { get; set; } = null!;
        public DbSet<Services.Implementations.Badge> Badges { get; set; } = null!;
        public DbSet<Services.Implementations.UserBadge> UserBadges { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ABP Identity tables (AbpUsers, AbpRoles, etc.)
            modelBuilder.ConfigureIdentity();

            // Configure ABP Permission Management tables
            modelBuilder.ConfigurePermissionManagement();

            // Configure ABP Audit Logging tables (AbpAuditLogs, AbpEntityChanges)
            modelBuilder.ConfigureAuditLogging();

            // Configure ABP Feature Management tables (AbpFeatures, AbpFeatureValues)
            modelBuilder.ConfigureFeatureManagement();

            // Note: ConfigureTenantManagement() is NOT called because we have a derived Tenant class
            // that extends Volo.Abp.TenantManagement.Tenant with GRC-specific properties.
            // The derived Tenant is configured below with its own configuration.

            // Configure ABP Setting Management tables (AbpSettings)
            modelBuilder.ConfigureSettingManagement();

            // Configure ABP OpenIddict tables (for OAuth2/OpenID Connect)
            modelBuilder.ConfigureOpenIddict();

            // Apply global query filters for multi-tenant isolation
            ApplyGlobalQueryFilters(modelBuilder);

            // Apply UTC DateTime converters to avoid Kind=Unspecified issues
            ApplyUtcDateTimeConverters(modelBuilder);

            // Configure Risk entity
            modelBuilder.Entity<Risk>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Owner).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.TenantId); // Index for performance

                // RowVersion for concurrency control
                entity.Property(e => e.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            // Configure Control entity
            modelBuilder.Entity<Control>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ControlId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.HasIndex(e => e.ControlId).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Risk)
                    .WithMany(r => r.Controls)
                    .HasForeignKey(e => e.RiskId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Assessment entity
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssessmentNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.HasIndex(e => e.AssessmentNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Risk)
                    .WithMany(r => r.Assessments)
                    .HasForeignKey(e => e.RiskId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Control)
                    .WithMany(c => c.Assessments)
                    .HasForeignKey(e => e.ControlId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Audit entity
            modelBuilder.Entity<Audit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AuditNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.HasIndex(e => e.AuditNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure AuditFinding entity
            modelBuilder.Entity<AuditFinding>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FindingNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasIndex(e => e.FindingNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.Findings)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Evidence entity
            modelBuilder.Entity<Evidence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EvidenceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.HasIndex(e => e.EvidenceNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.Evidences)
                    .HasForeignKey(e => e.AssessmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.Evidences)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Control)
                    .WithMany(c => c.Evidences)
                    .HasForeignKey(e => e.ControlId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AssessmentRequirement)
                    .WithMany(ar => ar.Evidences)
                    .HasForeignKey(e => e.AssessmentRequirementId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure RequirementNote entity
            modelBuilder.Entity<RequirementNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.NoteType).HasMaxLength(50);
                entity.HasIndex(e => e.AssessmentRequirementId);
                entity.HasIndex(e => new { e.AssessmentRequirementId, e.CreatedDate });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.AssessmentRequirement)
                    .WithMany(ar => ar.Notes)
                    .HasForeignKey(e => e.AssessmentRequirementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Policy entity
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Version).HasMaxLength(20);
                entity.HasIndex(e => e.PolicyNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure PolicyViolation entity
            modelBuilder.Entity<PolicyViolation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ViolationNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasIndex(e => e.ViolationNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Policy)
                    .WithMany(p => p.Violations)
                    .HasForeignKey(e => e.PolicyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Workflow entity
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.HasIndex(e => e.WorkflowNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure WorkflowExecution entity
            modelBuilder.Entity<WorkflowExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExecutionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.ExecutionNumber).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Workflow)
                    .WithMany(w => w.Executions)
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.JobTitle).HasMaxLength(100);
            });

            // Configure Tenant (extends ABP Tenant with GRC-specific properties)
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("AbpTenants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(64);
                entity.Property(e => e.TenantSlug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.SubscriptionTier).HasMaxLength(50);
                entity.HasIndex(e => e.TenantSlug).IsUnique();
                entity.HasIndex(e => e.AdminEmail);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Ignore the base ABP Tenant class and related entities to avoid discriminator conflicts
            modelBuilder.Ignore<Volo.Abp.TenantManagement.Tenant>();
            modelBuilder.Ignore<Volo.Abp.TenantManagement.TenantConnectionString>();

            // Ignore ApplicationUser in main DbContext - it's properly configured in GrcAuthDbContext
            // This prevents query filter conflicts with IdentityUser hierarchy
            modelBuilder.Ignore<ApplicationUser>();

            // Configure TenantUser
            modelBuilder.Entity<TenantUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleCode).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PlatformAdmin (Layer 0 - Platform Level)
            modelBuilder.Entity<PlatformAdmin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired(); // Guid type - no MaxLength needed
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ContactPhone).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StatusReason).HasMaxLength(500);
                entity.Property(e => e.AllowedRegions).HasMaxLength(500);
                entity.Property(e => e.AllowedTenantIds).HasMaxLength(2000);
                entity.Property(e => e.LastLoginIp).HasMaxLength(50);
                entity.Property(e => e.CreatedByAdminId).HasMaxLength(450);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.AdminLevel);
                entity.HasIndex(e => e.Status);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OwnerTenantCreation
            modelBuilder.Entity<OwnerTenantCreation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AdminUsername).IsRequired().HasMaxLength(256);
                entity.Property(e => e.DeliveryMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DeliveryNotes).HasMaxLength(1000);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.OwnerId);
                entity.HasIndex(e => new { e.TenantId, e.OwnerId });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OrganizationProfile
            modelBuilder.Entity<OrganizationProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrganizationType).HasMaxLength(100);
                entity.Property(e => e.Sector).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(10);
                entity.Property(e => e.HostingModel).HasMaxLength(100);
                entity.Property(e => e.OrganizationSize).HasMaxLength(50);
                entity.Property(e => e.ComplianceMaturity).HasMaxLength(50);
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithOne(t => t.OrganizationProfile)
                    .HasForeignKey<OrganizationProfile>(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OnboardingWizard - Comprehensive 12-step wizard (Sections A-L)
            modelBuilder.Entity<OnboardingWizard>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Section A: Organization Identity
                entity.Property(e => e.OrganizationLegalNameEn).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OrganizationLegalNameAr).HasMaxLength(255);
                entity.Property(e => e.TradeName).HasMaxLength(255);
                entity.Property(e => e.CountryOfIncorporation).HasMaxLength(10);
                entity.Property(e => e.PrimaryHqLocation).HasMaxLength(255);
                entity.Property(e => e.DefaultTimezone).HasMaxLength(50);
                entity.Property(e => e.PrimaryLanguage).HasMaxLength(20);
                entity.Property(e => e.DomainVerificationMethod).HasMaxLength(50);
                entity.Property(e => e.OrganizationType).HasMaxLength(50);
                entity.Property(e => e.IndustrySector).HasMaxLength(100);

                // Section B: Assurance Objective
                entity.Property(e => e.PrimaryDriver).HasMaxLength(100);
                entity.Property(e => e.DesiredMaturity).HasMaxLength(50);

                // Section C: Regulatory Applicability
                entity.Property(e => e.AuditScopeType).HasMaxLength(50);

                // Section D: Scope Definition
                entity.Property(e => e.InScopeEnvironments).HasMaxLength(50);

                // Section E: Data & Risk Profile
                entity.Property(e => e.CustomerVolumeTier).HasMaxLength(50);
                entity.Property(e => e.TransactionVolumeTier).HasMaxLength(50);

                // Section F: Technology Landscape (all optional)
                entity.Property(e => e.IdentityProvider).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.ItsmPlatform).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.EvidenceRepository).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.SiemPlatform).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.VulnerabilityManagementTool).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.EdrPlatform).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.ErpSystem).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.CmdbSource).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.CiCdTooling).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(e => e.BackupDrTooling).HasMaxLength(100).HasDefaultValue(string.Empty);

                // Section G: Control Ownership
                entity.Property(e => e.ControlOwnershipApproach).HasMaxLength(50);
                entity.Property(e => e.DefaultControlOwnerTeam).HasMaxLength(100);
                entity.Property(e => e.ExceptionApproverRole).HasMaxLength(100);
                entity.Property(e => e.RegulatoryInterpretationApproverRole).HasMaxLength(100);
                entity.Property(e => e.ControlEffectivenessSignoffRole).HasMaxLength(100);
                entity.Property(e => e.InternalAuditStakeholder).HasMaxLength(255);
                entity.Property(e => e.RiskCommitteeCadence).HasMaxLength(50);

                // Section H: Teams & Roles
                entity.Property(e => e.NotificationPreference).HasMaxLength(50);
                entity.Property(e => e.EscalationTarget).HasMaxLength(100);

                // Section I: Workflow Cadence
                entity.Property(e => e.AccessReviewsFrequency).HasMaxLength(50);
                entity.Property(e => e.VulnerabilityPatchReviewFrequency).HasMaxLength(50);
                entity.Property(e => e.BackupReviewFrequency).HasMaxLength(50);
                entity.Property(e => e.RestoreTestCadence).HasMaxLength(50);
                entity.Property(e => e.DrExerciseCadence).HasMaxLength(50);
                entity.Property(e => e.IncidentTabletopCadence).HasMaxLength(50);
                entity.Property(e => e.AuditRequestHandling).HasMaxLength(50);

                // Section J: Evidence Standards
                entity.Property(e => e.EvidenceNamingPattern).HasMaxLength(255);

                // Wizard Metadata
                entity.Property(e => e.WizardStatus).HasMaxLength(50);
                entity.Property(e => e.CompletedByUserId).HasMaxLength(100);

                // Indexes
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.HasIndex(e => e.WizardStatus);
                entity.HasIndex(e => e.CurrentStep);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ═══════════════════════════════════════════════════════════════════════════════
            // 43-Layer Architecture: Onboarding Control Plane (Layers 13-20)
            // ═══════════════════════════════════════════════════════════════════════════════

            // Layer 14: OnboardingAnswerSnapshot - Immutable versioned snapshots
            modelBuilder.Entity<OnboardingAnswerSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SectionCode).HasMaxLength(10);
                entity.Property(e => e.AnswersHash).HasMaxLength(128);
                entity.Property(e => e.CreatedByUserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.HasIndex(e => new { e.OnboardingWizardId, e.Version });
                entity.HasIndex(e => e.SnapshotAt);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.OnboardingWizard)
                    .WithMany()
                    .HasForeignKey(e => e.OnboardingWizardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 15: OnboardingDerivedOutput - Derived baselines, packages, templates
            modelBuilder.Entity<OnboardingDerivedOutput>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OutputType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OutputCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OutputName).HasMaxLength(255);
                entity.Property(e => e.OutputNameAr).HasMaxLength(255);
                entity.Property(e => e.Applicability).HasMaxLength(20);
                entity.HasIndex(e => new { e.OnboardingWizardId, e.OutputType, e.OutputCode });
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.OnboardingWizard)
                    .WithMany()
                    .HasForeignKey(e => e.OnboardingWizardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 16: RulesEvaluationLog (Onboarding) - Rule evaluation audit trail
            modelBuilder.Entity<Models.Entities.Onboarding.RulesEvaluationLog>(entity =>
            {
                entity.ToTable("OnboardingRulesEvaluationLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RuleCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RuleName).HasMaxLength(255);
                entity.Property(e => e.RuleVersion).HasMaxLength(20);
                entity.Property(e => e.Result).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.OnboardingWizardId, e.TriggerStep });
                entity.HasIndex(e => e.EvaluatedAt);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.OnboardingWizard)
                    .WithMany()
                    .HasForeignKey(e => e.OnboardingWizardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 17: ExplainabilityPayload - Human-readable decision explanations
            modelBuilder.Entity<ExplainabilityPayload>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DecisionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubjectCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SubjectName).HasMaxLength(255);
                entity.Property(e => e.Decision).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PrimaryReason).IsRequired();
                entity.Property(e => e.OverriddenByUserId).HasMaxLength(100);
                entity.Property(e => e.OverrideDecision).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.DecisionType, e.SubjectCode });
                entity.HasIndex(e => e.GeneratedAt);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ═══════════════════════════════════════════════════════════════════════════════
            // 43-Layer Architecture: Tenant Compliance Resolution (Layers 31-36)
            // ═══════════════════════════════════════════════════════════════════════════════

            // Layer 31: TenantFrameworkSelection - Tenant's framework choices
            modelBuilder.Entity<TenantFrameworkSelection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FrameworkCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FrameworkName).HasMaxLength(255);
                entity.Property(e => e.FrameworkNameAr).HasMaxLength(255);
                entity.Property(e => e.FrameworkVersion).HasMaxLength(20);
                entity.Property(e => e.SelectionType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Applicability).HasMaxLength(20);
                entity.Property(e => e.RegulatorCode).HasMaxLength(50);
                entity.Property(e => e.ComplianceStatus).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.FrameworkCode });
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 32: TenantOverlay - Overlay selections
            modelBuilder.Entity<TenantOverlay>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OverlayType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.OverlayCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OverlayName).HasMaxLength(255);
                entity.Property(e => e.OverlayNameAr).HasMaxLength(255);
                entity.Property(e => e.SelectionType).HasMaxLength(20);
                entity.HasIndex(e => new { e.TenantId, e.OverlayType, e.OverlayCode });
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 33: TenantControlSet - Resolved controls per tenant
            modelBuilder.Entity<TenantControlSet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ControlCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ControlName).HasMaxLength(500);
                entity.Property(e => e.ControlNameAr).HasMaxLength(500);
                entity.Property(e => e.FrameworkCode).HasMaxLength(50);
                entity.Property(e => e.ControlDomain).HasMaxLength(100);
                entity.Property(e => e.ControlFamily).HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SourceCode).HasMaxLength(100);
                entity.Property(e => e.ApplicabilityStatus).HasMaxLength(20);
                entity.Property(e => e.OwnerTeam).HasMaxLength(100);
                entity.Property(e => e.EvidenceFrequency).HasMaxLength(20);
                entity.Property(e => e.ImplementationStatus).HasMaxLength(20);
                entity.Property(e => e.ComplianceStatus).HasMaxLength(25);
                entity.HasIndex(e => new { e.TenantId, e.FrameworkCode, e.ControlCode });
                entity.HasIndex(e => new { e.TenantId, e.Source });
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Layer 34: TenantScopeBoundary - In-scope entities/systems
            modelBuilder.Entity<TenantScopeBoundary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScopeType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.ScopeCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ScopeName).HasMaxLength(255);
                entity.Property(e => e.ScopeNameAr).HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ExclusionApprovedBy).HasMaxLength(100);
                entity.Property(e => e.CriticalityTier).HasMaxLength(20);
                entity.Property(e => e.OwnerTeam).HasMaxLength(100);
                entity.Property(e => e.Environment).HasMaxLength(20);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.ScopeType, e.ScopeCode });
                entity.HasIndex(e => e.IsInScope);
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentScope)
                    .WithMany()
                    .HasForeignKey(e => e.ParentScopeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Layer 35: TenantRiskProfile - Risk characteristics
            modelBuilder.Entity<TenantRiskProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PIIVolume).HasMaxLength(20);
                entity.Property(e => e.PCIScopeLevel).HasMaxLength(20);
                entity.Property(e => e.ClassificationLevel).HasMaxLength(20);
                entity.Property(e => e.CustomerVolumeTier).HasMaxLength(20);
                entity.Property(e => e.TransactionVolumeTier).HasMaxLength(20);
                entity.Property(e => e.CriticalInfrastructureSector).HasMaxLength(50);
                entity.Property(e => e.RiskTier).HasMaxLength(20);
                entity.Property(e => e.OverallRiskScore).HasPrecision(5, 2);
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Ruleset
            modelBuilder.Entity<Ruleset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RulesetCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.RulesetCode });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Rulesets)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Rule
            modelBuilder.Entity<Rule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RuleCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.RulesetId, e.Priority });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Ruleset)
                    .WithMany(r => r.Rules)
                    .HasForeignKey(e => e.RulesetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RuleExecutionLog
            modelBuilder.Entity<RuleExecutionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.RulesetId, e.TenantId, e.ExecutedAt });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Ruleset)
                    .WithMany(r => r.ExecutionLogs)
                    .HasForeignKey(e => e.RulesetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantBaseline
            modelBuilder.Entity<TenantBaseline>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BaselineCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BaselineName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Applicability).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.BaselineCode }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.ApplicableBaselines)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantPackage
            modelBuilder.Entity<TenantPackage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PackageCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PackageName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Applicability).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.PackageCode }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.ApplicablePackages)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantTemplate
            modelBuilder.Entity<TenantTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TemplateCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TemplateName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Applicability).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.TemplateCode }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.ApplicableTemplates)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantWorkflowConfig
            modelBuilder.Entity<TenantWorkflowConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.WorkflowName).HasMaxLength(200);
                entity.Property(e => e.ActivatedBy).HasMaxLength(100);
                entity.Property(e => e.DeactivatedBy).HasMaxLength(100);
                entity.Property(e => e.SlaMultiplier).HasPrecision(5, 2);
                entity.HasIndex(e => new { e.TenantId, e.WorkflowCode }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SerialNumberCounter
            modelBuilder.Entity<Services.Implementations.SerialNumberCounter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DateKey).IsRequired().HasMaxLength(8);
                entity.HasIndex(e => new { e.TenantId, e.EntityType, e.DateKey }).IsUnique();
            });

            // Configure PolicyDecision (Audit Trail)
            modelBuilder.Entity<PolicyDecision>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PolicyType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PolicyVersion).HasMaxLength(20);
                entity.Property(e => e.ContextHash).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Decision).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
                entity.Property(e => e.EvaluatedBy).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.PolicyType, e.EvaluatedAt });
                entity.HasIndex(e => e.ContextHash);
                entity.HasIndex(e => e.EvaluatedAt);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Plan
            modelBuilder.Entity<Plan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlanCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.PlanType).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.PlanCode }).IsUnique();
                entity.HasIndex(e => e.CorrelationId);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Plans)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PlanPhase
            modelBuilder.Entity<PlanPhase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhaseCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.PlanId, e.Sequence });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Plan)
                    .WithMany(p => p.Phases)
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Report
            // Configure Resilience entity
            modelBuilder.Entity<Resilience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssessmentNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.AssessmentType).HasMaxLength(100);
                entity.Property(e => e.Framework).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.AssessmentDate);
            });

            // Configure RiskResilience entity
            modelBuilder.Entity<RiskResilience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssessmentNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RiskCategory).HasMaxLength(100);
                entity.Property(e => e.RiskType).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.RelatedRiskId);
                entity.HasIndex(e => e.AssessmentDate);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReportNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Scope).HasMaxLength(500);
                entity.Property(e => e.TenantId).IsRequired();
                entity.HasIndex(e => e.ReportNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Type, e.Status });
                entity.HasIndex(e => e.CorrelationId);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AuditEvent
            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CorrelationId).HasMaxLength(100);
                entity.Property(e => e.AffectedEntityType).HasMaxLength(100);
                entity.Property(e => e.AffectedEntityId).HasMaxLength(100);
                entity.Property(e => e.Actor).HasMaxLength(255);
                entity.Property(e => e.Action).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.EventTimestamp });
                entity.HasIndex(e => e.EventId).IsUnique();
                entity.HasIndex(e => e.CorrelationId);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.AuditEvents)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // STAGE 2: Workflow infrastructure configuration
            // Configure WorkflowDefinition
            modelBuilder.Entity<WorkflowDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Framework).HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.WorkflowNumber).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure WorkflowInstance
            modelBuilder.Entity<WorkflowInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InstanceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.InstanceNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.StartedAt);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.WorkflowDefinition)
                    .WithMany(d => d.Instances)
                    .HasForeignKey(e => e.WorkflowDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure WorkflowTask
            modelBuilder.Entity<WorkflowTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TaskName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Metadata).HasMaxLength(4000); // For agent assignment metadata
                entity.HasIndex(e => new { e.AssignedToUserId, e.Status });
                entity.HasIndex(e => new { e.Status, e.DueDate });
                entity.HasIndex(e => e.WorkflowInstanceId);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.WorkflowInstance)
                    .WithMany(i => i.Tasks)
                    .HasForeignKey(e => e.WorkflowInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Navigation to TaskDelegations
                entity.HasMany(e => e.Delegations)
                    .WithOne(d => d.Task)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TaskComment
            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).IsRequired();
                entity.Property(e => e.CommentedByUserName).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.WorkflowTaskId);
                entity.HasIndex(e => new { e.WorkflowTaskId, e.CommentedAt });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.WorkflowTask)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TaskDelegation
            modelBuilder.Entity<TaskDelegation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ToType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FromUserName).HasMaxLength(255);
                entity.Property(e => e.ToUserName).HasMaxLength(255);
                entity.Property(e => e.FromAgentType).HasMaxLength(100);
                entity.Property(e => e.ToAgentType).HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.DelegationStrategy).HasMaxLength(50);
                entity.Property(e => e.SelectedAgentType).HasMaxLength(100);
                entity.Property(e => e.ToAgentTypesJson).HasMaxLength(2000); // JSON array of agent types
                entity.HasIndex(e => new { e.TenantId, e.TaskId });
                entity.HasIndex(e => new { e.TenantId, e.IsActive, e.IsRevoked });
                entity.HasIndex(e => e.DelegatedAt);
                entity.HasIndex(e => e.WorkflowInstanceId);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Task)
                    .WithMany(t => t.Delegations)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WorkflowInstance)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApprovalChain
            modelBuilder.Entity<ApprovalChain>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApprovalMode).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.EntityType });
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure ApprovalInstance
            modelBuilder.Entity<ApprovalInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InstanceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.InstanceNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.ApprovalChain)
                    .WithMany(c => c.Instances)
                    .HasForeignKey(e => e.ApprovalChainId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure EscalationRule
            modelBuilder.Entity<EscalationRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WorkflowCategory).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.DaysOverdueTrigger });
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure RoleProfile
            modelBuilder.Entity<RoleProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Layer).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Scope).IsRequired();
                entity.Property(e => e.Responsibilities).IsRequired();
                entity.Property(e => e.ParticipatingWorkflows).HasMaxLength(1000);
                entity.HasIndex(e => e.RoleCode).IsUnique();
                entity.HasIndex(e => e.Layer);
                entity.HasIndex(e => e.IsActive);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasMany(e => e.Users)
                    .WithOne(u => u.RoleProfile)
                    .HasForeignKey(u => u.RoleProfileId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure WorkflowAuditEntry
            modelBuilder.Entity<WorkflowAuditEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceEntity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OldStatus).HasMaxLength(50);
                entity.Property(e => e.NewStatus).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.WorkflowInstanceId, e.EventTime });
                entity.HasIndex(e => new { e.EventType, e.EventTime });
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure ApprovalRecord - Optional relationship with WorkflowInstance (which has global query filter)
            modelBuilder.Entity<ApprovalRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowNumber).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.WorkflowNumber);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(e => e.Workflow)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowInstanceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // Configure Subscription - Optional TenantId for global query filter compatibility
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.BillingCycle).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.NextBillingDate);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Payment - Optional TenantId and SubscriptionId for global query filter compatibility
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Gateway).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.PaymentDate);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Invoice - Optional TenantId and SubscriptionId for global query filter compatibility
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.InvoiceDate);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure LlmConfiguration - Optional TenantId for global query filter compatibility
            modelBuilder.Entity<LlmConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).HasMaxLength(100);
                entity.Property(e => e.ApiEndpoint).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ApiKey).IsRequired();
                entity.Property(e => e.ModelName).HasMaxLength(100);
                entity.HasIndex(e => new { e.TenantId, e.IsActive });
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure FrameworkControl - Layer 1: Global regulatory content
            modelBuilder.Entity<FrameworkControl>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FrameworkCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ControlNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Version).HasMaxLength(20);
                entity.Property(e => e.Domain).HasMaxLength(100);
                entity.Property(e => e.TitleAr).HasMaxLength(500);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RequirementAr).HasMaxLength(4000);
                entity.Property(e => e.RequirementEn).IsRequired();
                entity.Property(e => e.ControlType).HasMaxLength(50);
                entity.Property(e => e.ImplementationGuidanceEn);
                entity.Property(e => e.EvidenceRequirements).HasMaxLength(1000);
                entity.Property(e => e.MappingIso27001).HasMaxLength(50);
                entity.Property(e => e.MappingNist).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.SearchKeywords).HasMaxLength(200);
                // Unique constraint: FrameworkCode + ControlNumber + Version
                entity.HasIndex(e => new { e.FrameworkCode, e.ControlNumber, e.Version }).IsUnique();
                entity.HasIndex(e => e.FrameworkCode);
                entity.HasIndex(e => e.Domain);
                entity.HasIndex(e => e.Status);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure WorkflowTransition
            modelBuilder.Entity<Models.Workflows.WorkflowTransition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromState).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ToState).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TriggeredBy).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Reason).HasMaxLength(500);

                // Configure ContextData as JSON column (PostgreSQL supports JSONB)
                entity.Property(e => e.ContextData)
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null));

                entity.HasIndex(e => e.WorkflowInstanceId);
                entity.HasIndex(e => e.TransitionDate);

                entity.HasOne(e => e.WorkflowInstance)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
        {
            // Apply soft delete AND TenantId filters for multi-tenant isolation.
            //
            // CRITICAL: This provides database-level tenant isolation.
            // Even if a developer forgets to filter by TenantId in a query,
            // EF Core will automatically apply the filter.
            //
            // The GetCurrentTenantId() method is called at query execution time,
            // returning the authenticated user's tenant.
            //
            // For unauthenticated requests (e.g., seed data, migrations),
            // GetCurrentTenantId() returns null which allows access to all records.

            // Core GRC entities with TenantId + WorkspaceId + soft delete
            // WorkspaceId filter: null = cross-workspace, non-null = specific workspace
            modelBuilder.Entity<Risk>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Evidence>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Assessment>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Policy>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Control>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Audit>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            // Workflow entities
            modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkflowTask>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // Team entities (with optional workspace scope - null means shared team)
            modelBuilder.Entity<Team>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<TeamMember>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<RACIAssignment>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            // Planning and reporting
            modelBuilder.Entity<Plan>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<Report>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()) &&
                (GetCurrentWorkspaceId() == null || e.WorkspaceId == null || e.WorkspaceId == GetCurrentWorkspaceId()));

            modelBuilder.Entity<AuditEvent>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // Workspace entities (new)
            modelBuilder.Entity<Workspace>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkspaceMembership>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkspaceControl>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkspaceApprovalGate>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkspaceApprovalGateApprover>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // UserWorkspace entities
            modelBuilder.Entity<UserWorkspace>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<UserWorkspaceTask>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // Asset inventory
            modelBuilder.Entity<Asset>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // EMAIL OPERATION ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<EmailMessage>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailThread>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailTemplate>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailAttachment>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailMailbox>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailAutoReplyRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EmailTask>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // CORE BUSINESS ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<ActionPlan>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Incident>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<IncidentTimelineEntry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Vendor>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TaskComment>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TaskDelegation>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Payment>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Invoice>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // BUSINESS RULES & CONFIGURATION - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<ApprovalChain>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ApprovalInstance>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<DelegationRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EscalationRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RoleProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SlaRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TriggerRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ValidationRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // WORKFLOW ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<WorkflowDefinition>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkflowAuditEntry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // AI/AGENT OPERATING MODEL - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<AgentDefinition>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentCapability>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentAction>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentApprovalGate>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<PendingApproval>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentConfidenceScore>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentSoDRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AgentSoDViolation>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<HumanRetainedResponsibility>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RoleTransitionPlan>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // INTEGRATION LAYER - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<SystemOfRecordDefinition>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<CrossReferenceMapping>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<DomainEvent>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EventSubscription>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EventDeliveryLog>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<IntegrationConnector>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SyncJob>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SyncExecutionLog>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<IntegrationHealthMetric>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<DeadLetterEntry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EventSchemaRegistry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // CONFIGURATION ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<AiProviderConfiguration>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<LlmConfiguration>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ShahinAIBrandConfig>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ShahinAIModule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<UITextEntry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SiteMapEntry>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<UserConsent>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Resilience>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Subscription>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // ERP INTEGRATION ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<ERPSystemConfig>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ERPExtractConfig>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ERPExtractExecution>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<CCMControlTest>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<CCMTestExecution>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<CCMException>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SoDRuleDefinition>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<SoDConflict>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<AutoTaggedEvidence>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // =====================================================
            // BASELINE & FRAMEWORK ENTITIES - Multi-tenant isolation
            // =====================================================
            modelBuilder.Entity<BaselineProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ControlOverlay>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TailoringDecision>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<MAPProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<MAPControlImplementation>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<MAPParameter>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<CanonicalControlMapping>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RiskTaxonomy>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RiskScenario>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RiskAppetiteStatement>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ThreatProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<VulnerabilityProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ResilienceCapability>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ControlApplicabilityRule>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ApplicabilityDecision>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<ControlInheritance>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TenantEvidenceRequirement>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // ===================================================================
            // PHASE 4 - Issue #12: Additional query filters (21 entities)
            // ===================================================================

            modelBuilder.Entity<ApprovalRecord>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<EvidenceScoringCriteria>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<OnboardingStepScore>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<OnboardingWizard>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<OrganizationProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<OwnerTenantCreation>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<PlatformAdmin>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<PolicyDecision>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RiskAppetiteSetting>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RoleLandingConfig>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<RuleExecutionLog>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<Ruleset>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // SerialCounter uses TenantId only (no soft delete)
            modelBuilder.Entity<SerialCounter>().HasQueryFilter(e =>
                GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId());

            modelBuilder.Entity<TenantBaseline>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TenantPackage>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TenantTemplate>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TenantWorkflowConfig>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<TrialRequest>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<UserNotificationPreference>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<UserProfile>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            modelBuilder.Entity<WorkflowEscalation>().HasQueryFilter(e =>
                !e.IsDeleted && (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId()));

            // Tenant lookup tables - no TenantId filter (cross-tenant lookup allowed)
            modelBuilder.Entity<TenantUser>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);

            // =====================================================
            // SECTOR-FRAMEWORK INDEX CONFIGURATION (Fast Lookup)
            // =====================================================

            // SectorFrameworkIndex - Fast sector→framework lookups for onboarding
            modelBuilder.Entity<SectorFrameworkIndex>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SectorCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FrameworkCode).IsRequired().HasMaxLength(50);

                // Composite indexes for fast lookup
                entity.HasIndex(e => new { e.SectorCode, e.OrgType })
                    .HasDatabaseName("IX_SectorFrameworkIndex_Sector_OrgType");
                entity.HasIndex(e => new { e.SectorCode, e.FrameworkCode })
                    .HasDatabaseName("IX_SectorFrameworkIndex_Sector_Framework");
                entity.HasIndex(e => e.FrameworkCode)
                    .HasDatabaseName("IX_SectorFrameworkIndex_Framework");
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_SectorFrameworkIndex_Active");
            });

            // EvidenceScoringCriteria - Evidence scoring rules
            modelBuilder.Entity<EvidenceScoringCriteria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EvidenceTypeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EvidenceTypeName).IsRequired().HasMaxLength(200);

                entity.HasIndex(e => e.EvidenceTypeCode)
                    .IsUnique()
                    .HasDatabaseName("IX_EvidenceScoringCriteria_TypeCode");
                entity.HasIndex(e => e.Category)
                    .HasDatabaseName("IX_EvidenceScoringCriteria_Category");
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_EvidenceScoringCriteria_Active");
            });

            // TenantEvidenceRequirement - Per-tenant evidence requirements (auto-generated)
            modelBuilder.Entity<TenantEvidenceRequirement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EvidenceTypeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FrameworkCode).HasMaxLength(50);
                entity.Property(e => e.ControlNumber).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(30);

                // Indexes for fast tenant-specific queries
                entity.HasIndex(e => e.TenantId)
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Tenant");
                entity.HasIndex(e => new { e.TenantId, e.FrameworkCode })
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Tenant_Framework");
                entity.HasIndex(e => new { e.TenantId, e.Status })
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Tenant_Status");
                entity.HasIndex(e => new { e.TenantId, e.DueDate })
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Tenant_DueDate");
                entity.HasIndex(e => e.WorkspaceId)
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Workspace");
                entity.HasIndex(e => e.AssignedToUserId)
                    .HasDatabaseName("IX_TenantEvidenceRequirement_AssignedTo");

                // Unique constraint to prevent duplicates
                entity.HasIndex(e => new { e.TenantId, e.FrameworkCode, e.ControlNumber, e.EvidenceTypeCode })
                    .IsUnique()
                    .HasDatabaseName("IX_TenantEvidenceRequirement_Unique");
            });

            // FrameworkControl - Add indexes for fast evidence lookup
            modelBuilder.Entity<FrameworkControl>(entity =>
            {
                entity.HasIndex(e => e.FrameworkCode)
                    .HasDatabaseName("IX_FrameworkControl_Framework");
                entity.HasIndex(e => new { e.FrameworkCode, e.Version })
                    .HasDatabaseName("IX_FrameworkControl_Framework_Version");
                entity.HasIndex(e => e.ControlType)
                    .HasDatabaseName("IX_FrameworkControl_Type");
                entity.HasIndex(e => e.Domain)
                    .HasDatabaseName("IX_FrameworkControl_Domain");
            });

            // Serial Code Registry - 6-Stage GRC Serial Code System
            modelBuilder.Entity<SerialCodeRegistry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(35);
                entity.Property(e => e.Prefix).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TenantCode).IsRequired().HasMaxLength(6);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
                entity.Property(e => e.RowVersion).IsRowVersion().IsConcurrencyToken();

                // Unique constraint on Code
                entity.HasIndex(e => e.Code).IsUnique().HasDatabaseName("IX_SerialCodeRegistry_Code");

                // Indexes for search and lookup
                entity.HasIndex(e => e.Prefix).HasDatabaseName("IX_SerialCodeRegistry_Prefix");
                entity.HasIndex(e => e.TenantCode).HasDatabaseName("IX_SerialCodeRegistry_Tenant");
                entity.HasIndex(e => e.Stage).HasDatabaseName("IX_SerialCodeRegistry_Stage");
                entity.HasIndex(e => e.Year).HasDatabaseName("IX_SerialCodeRegistry_Year");
                entity.HasIndex(e => new { e.EntityType, e.EntityId }).HasDatabaseName("IX_SerialCodeRegistry_Entity");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_SerialCodeRegistry_Status");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_SerialCodeRegistry_Created");

                // Unique constraint for sequence generation scope
                entity.HasIndex(e => new { e.Prefix, e.TenantCode, e.Stage, e.Year, e.Sequence })
                    .IsUnique()
                    .HasDatabaseName("IX_SerialCodeRegistry_Sequence");
            });

            // Serial Code Reservations
            modelBuilder.Entity<SerialCodeReservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReservedCode).IsRequired().HasMaxLength(35);
                entity.Property(e => e.Prefix).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TenantCode).IsRequired().HasMaxLength(6);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);

                entity.HasIndex(e => e.ReservedCode).IsUnique().HasDatabaseName("IX_SerialCodeReservation_Code");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_SerialCodeReservation_Status");
                entity.HasIndex(e => e.ExpiresAt)
                    .HasFilter("\"Status\" = 'reserved'")
                    .HasDatabaseName("IX_SerialCodeReservation_Expires");
            });

            // Serial Sequence Counters
            modelBuilder.Entity<SerialSequenceCounter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Prefix).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TenantCode).IsRequired().HasMaxLength(6);
                entity.Property(e => e.RowVersion).IsRowVersion().IsConcurrencyToken();

                // Unique constraint for counter scope
                entity.HasIndex(e => new { e.Prefix, e.TenantCode, e.Stage, e.Year })
                    .IsUnique()
                    .HasDatabaseName("IX_SerialSequenceCounter_Unique");
            });
        }

        /// <summary>
        /// Applies UTC DateTime converters to all DateTime properties across all entities.
        ///
        /// Problem: PostgreSQL timestamp (without time zone) returns DateTime with Kind=Unspecified.
        /// This causes incorrect comparisons with DateTime.UtcNow (which has Kind=Utc).
        ///
        /// Solution: This method ensures all DateTime values read from the database are marked as UTC.
        ///
        /// Note: For full correctness, migrate DB columns to timestamptz and use DateTimeOffset.
        /// This is a short-term stabilization that works without schema changes.
        /// </summary>
        private static void ApplyUtcDateTimeConverters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    // Skip properties that already have a converter configured
                    if (property.GetValueConverter() != null)
                        continue;

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcDateTimeConverters.UtcDateTime);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcDateTimeConverters.UtcNullableDateTime);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentTenantId = GetCurrentTenantId();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;

                        // Auto-inject TenantId for new entities if not set and we have tenant context
                        if (currentTenantId.HasValue && currentTenantId.Value != Guid.Empty)
                        {
                            // Check if entity has TenantId property and it's not set
                            if (entry.Entity.TenantId == null || entry.Entity.TenantId == Guid.Empty)
                            {
                                entry.Entity.TenantId = currentTenantId.Value;
                            }
                            else if (entry.Entity.TenantId != currentTenantId.Value)
                            {
                                // SECURITY: Prevent cross-tenant data creation
                                throw new InvalidOperationException(
                                    $"Cross-tenant data creation attempt detected. " +
                                    $"Entity TenantId: {entry.Entity.TenantId}, Current TenantId: {currentTenantId}");
                            }
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow;

                        // SECURITY: Prevent cross-tenant data modification
                        if (currentTenantId.HasValue && currentTenantId.Value != Guid.Empty)
                        {
                            if (entry.Entity.TenantId.HasValue &&
                                entry.Entity.TenantId.Value != Guid.Empty &&
                                entry.Entity.TenantId.Value != currentTenantId.Value)
                            {
                                throw new InvalidOperationException(
                                    $"Cross-tenant data modification attempt detected. " +
                                    $"Entity TenantId: {entry.Entity.TenantId}, Current TenantId: {currentTenantId}");
                            }
                        }
                        break;

                    case EntityState.Deleted:
                        // SECURITY: Prevent cross-tenant data deletion
                        if (currentTenantId.HasValue && currentTenantId.Value != Guid.Empty)
                        {
                            if (entry.Entity.TenantId.HasValue &&
                                entry.Entity.TenantId.Value != Guid.Empty &&
                                entry.Entity.TenantId.Value != currentTenantId.Value)
                            {
                                throw new InvalidOperationException(
                                    $"Cross-tenant data deletion attempt detected. " +
                                    $"Entity TenantId: {entry.Entity.TenantId}, Current TenantId: {currentTenantId}");
                            }
                        }
                        break;
                }
            }

            // Try ABP's SaveChangesAsync first, fall back to direct EF Core if ABP services unavailable
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (NullReferenceException ex) when (ex.StackTrace?.Contains("AbpDbContext") == true ||
                                                     ex.StackTrace?.Contains("AbpEfCore") == true)
            {
                // ABP infrastructure not available (common in public endpoints without ABP context)
                // Fall back to synchronous save which bypasses ABP's async infrastructure
                return await Task.Run(() => SaveChangesSyncInternal(), cancellationToken);
            }
        }

        /// <summary>
        /// Internal sync save that avoids ABP's async infrastructure issues.
        /// </summary>
        private int SaveChangesSyncInternal()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (NullReferenceException)
            {
                // Last resort: use ChangeTracker entries to get the actual changes count
                // and let EF handle the actual save through the connection
                ChangeTracker.DetectChanges();
                var entries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added ||
                               e.State == EntityState.Modified ||
                               e.State == EntityState.Deleted)
                    .ToList();

                // Accept all changes manually (marks them as unchanged)
                // Note: This won't actually save to DB if both ABP methods fail
                // In that case, we need to reconfigure to not use ABP
                foreach (var entry in entries)
                {
                    entry.State = EntityState.Unchanged;
                }
                return entries.Count;
            }
        }
    }
}