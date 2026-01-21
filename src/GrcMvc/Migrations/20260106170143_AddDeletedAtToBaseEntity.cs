using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkspaceTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Workspaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkspaceMemberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkspaceControls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkspaceApprovalGates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkspaceApprovalGateApprovers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Workflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowInstances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowEscalations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowDefinitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WorkflowAuditEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WebhookSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WebhookDeliveryLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ValidationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ValidationResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserWorkspaceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserWorkspaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserProfileAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserNotificationPreferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserConsents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UniversalEvidencePacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UniversalEvidencePackItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UITextEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TriggerRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TriggerExecutionLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TitleCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ThirdPartyConcentrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TestProcedures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TenantWorkflowConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TenantUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TenantTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TenantPackages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TenantBaselines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TemplateCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TeamsNotificationConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Teams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TeamMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TaskDelegations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TaskComments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SystemOfRecordDefinitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SyncJobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SyncExecutionLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupportMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SuiteEvidenceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SuiteControlEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SubscriptionPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StrategicRoadmapMilestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StandardEvidenceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SoDRuleDefinitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SoDConflicts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SlaRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SiteMapEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ShahinAIModules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ShahinAIBrandConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Rulesets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Rules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RuleExecutionLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RoleTransitionPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RoleProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RoleCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Risks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RiskResiliences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RiskIndicators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RiskIndicatorMeasurements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RiskIndicatorAlerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Resiliences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RequirementMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RegulatoryRequirements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RegulatorCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RACIAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PolicyViolations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PolicyDecisions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Plans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PlanPhases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PlainLanguageControls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PendingApprovals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PackageCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OwnerTenantCreations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformAdminId",
                table: "OwnerTenantCreations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OverlayParameterOverrides",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OverlayControlMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OverlayCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrganizationEntities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OnePageGuides",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OnboardingWizards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MappingWorkflowTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MappingWorkflowSteps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MappingQualityGates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MAPFrameworkConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LlmConfigurations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LegalDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "IntegrationHealthMetrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "IntegrationConnectors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ImportantBusinessServices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HumanRetainedResponsibilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GovernanceRhythmTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GovernanceRhythmItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GovernanceCadences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GeneratedControlSuites",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FrameworkControls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FrameworkCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EvidenceTypeCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EvidenceSourceIntegrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EvidenceScores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EvidencePacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EvidencePackFamilies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EventSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EventSchemaRegistries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EventDeliveryLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EscalationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ERPSystemConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ERPExtractExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ERPExtractConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DomainEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DelegationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DelegationLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DeadLetterEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DataQualityScores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CryptographicAssets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CrossReferenceMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlTestProcedures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Controls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlObjectives",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlExceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlEvidencePacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlDomains",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlChangeHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ControlCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ComplianceGuardrails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CCMTestExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CCMExceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CCMControlTests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CapturedEvidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CanonicalControls",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CadenceExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BaselineControlSets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BaselineControlMappings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BaselineCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AutoTaggedEvidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Audits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditFindings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AssessmentScopes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Assessments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AssessmentRequirements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApprovalRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApprovalInstances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApprovalChains",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApplicabilityRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApplicabilityRuleCatalogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApplicabilityEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentSoDViolations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentSoDRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentDefinitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentConfidenceScores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentCapabilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentApprovalGates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AgentActions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformAdmins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    AdminLevel = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CanCreateTenants = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageTenants = table.Column<bool>(type: "boolean", nullable: false),
                    CanDeleteTenants = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageBilling = table.Column<bool>(type: "boolean", nullable: false),
                    CanAccessTenantData = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageCatalogs = table.Column<bool>(type: "boolean", nullable: false),
                    CanManagePlatformAdmins = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageConfiguration = table.Column<bool>(type: "boolean", nullable: false),
                    CanImpersonateUsers = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedRegions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AllowedTenantIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MaxTenantsAllowed = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalTenantsCreated = table.Column<int>(type: "integer", nullable: false),
                    LastTenantCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MfaRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    CanResetOwnPassword = table.Column<bool>(type: "boolean", nullable: false),
                    CanResetTenantAdminPasswords = table.Column<bool>(type: "boolean", nullable: false),
                    CanRestartTenantAdminAccounts = table.Column<bool>(type: "boolean", nullable: false),
                    LastPasswordChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ForcePasswordChange = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerTenantCreations_PlatformAdminId",
                table: "OwnerTenantCreations",
                column: "PlatformAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdmins_AdminLevel",
                table: "PlatformAdmins",
                column: "AdminLevel");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdmins_Status",
                table: "PlatformAdmins",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdmins_UserId",
                table: "PlatformAdmins",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerTenantCreations_PlatformAdmins_PlatformAdminId",
                table: "OwnerTenantCreations",
                column: "PlatformAdminId",
                principalTable: "PlatformAdmins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OwnerTenantCreations_PlatformAdmins_PlatformAdminId",
                table: "OwnerTenantCreations");

            migrationBuilder.DropTable(
                name: "PlatformAdmins");

            migrationBuilder.DropIndex(
                name: "IX_OwnerTenantCreations_PlatformAdminId",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkspaceMemberships");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkspaceControls");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkspaceApprovalGates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "PlatformAdminId",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AgentActions");
        }
    }
}
