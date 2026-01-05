using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Data
{
    public partial class GrcDbContext : IdentityDbContext<ApplicationUser>
    {
        public GrcDbContext(DbContextOptions<GrcDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        // Multi-tenant core
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<TenantUser> TenantUsers { get; set; } = null!;
        public DbSet<OrganizationProfile> OrganizationProfiles { get; set; } = null!;

        // Rules engine (Layer 2)
        public DbSet<Ruleset> Rulesets { get; set; } = null!;
        public DbSet<Rule> Rules { get; set; } = null!;
        public DbSet<RuleExecutionLog> RuleExecutionLogs { get; set; } = null!;

        // Tenant scope (Layer 2)
        public DbSet<TenantBaseline> TenantBaselines { get; set; } = null!;
        public DbSet<TenantPackage> TenantPackages { get; set; } = null!;
        public DbSet<TenantTemplate> TenantTemplates { get; set; } = null!;

        // Planning (Layer 3)
        public DbSet<Plan> Plans { get; set; } = null!;
        public DbSet<PlanPhase> PlanPhases { get; set; } = null!;

        // Audit trail
        public DbSet<AuditEvent> AuditEvents { get; set; } = null!;

        // Existing entities
        public DbSet<Risk> Risks { get; set; } = null!;
        public DbSet<Control> Controls { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;
        public DbSet<Audit> Audits { get; set; } = null!;
        public DbSet<AuditFinding> AuditFindings { get; set; } = null!;
        public DbSet<Evidence> Evidences { get; set; } = null!;
        public DbSet<Policy> Policies { get; set; } = null!;
        public DbSet<PolicyViolation> PolicyViolations { get; set; } = null!;
        public DbSet<Workflow> Workflows { get; set; } = null!;
        public DbSet<WorkflowExecution> WorkflowExecutions { get; set; } = null!;

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

        // Assessment Requirements (Layer 3: Operational)
        public DbSet<AssessmentRequirement> AssessmentRequirements { get; set; } = null!;

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
        public DbSet<ValidationResult> ValidationResults { get; set; } = null!;
        public DbSet<DataQualityScore> DataQualityScores { get; set; } = null!;

        // Evidence Scoring (Phase 8)
        public DbSet<Services.Interfaces.EvidenceScore> EvidenceScores { get; set; } = null!;

        // User Profiles & Preferences
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<UserProfileAssignment> UserProfileAssignments { get; set; } = null!;
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; } = null!;

        // Workflow Transitions (Audit Trail)
        public DbSet<Models.Workflows.WorkflowTransition> WorkflowTransitions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Risk entity
            modelBuilder.Entity<Risk>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Owner).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasQueryFilter(e => !e.IsDeleted);
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

            // Configure Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantSlug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.SubscriptionTier).HasMaxLength(50);
                entity.HasIndex(e => e.TenantSlug).IsUnique();
                entity.HasIndex(e => e.AdminEmail);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}