using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialCodeInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkspaceTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Workspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkspaceMemberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkspaceControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkspaceApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkspaceApprovalGateApprovers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Workflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowEscalations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WorkflowAuditEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WebhookSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "WebhookDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Vendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ValidationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ValidationResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserWorkspaceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserWorkspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserProfileAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserNotificationPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UserConsents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UniversalEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UniversalEvidencePackItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "UITextEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TriggerRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TriggerExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TrialRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TitleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ThirdPartyConcentrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantWorkflowConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantCode",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantEvidenceRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TenantBaselines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TemplateCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TeamsNotificationConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TaskDelegations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "TaskComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SystemOfRecordDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SyncJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SyncExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SupportMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SupportConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SuiteEvidenceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SuiteControlEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "StrategicRoadmapMilestones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "StandardEvidenceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SoDRuleDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SoDConflicts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SlaRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SiteMapEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ShahinAIModules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ShahinAIBrandConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "SectorFrameworkIndex",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Rulesets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Rules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RuleExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RoleTransitionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RoleProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RoleLandingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RoleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RiskResiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RiskIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RiskIndicatorMeasurements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RiskIndicatorAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Resiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RequirementNotes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RequirementMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RegulatoryRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Regulators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RegulatorCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "RACIAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PolicyViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PolicyDecisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupersedesDocumentCode",
                table: "Policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionMajor",
                table: "Policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionMinor",
                table: "Policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionNotes",
                table: "Policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PlatformAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Plans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PlanPhases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PlainLanguageControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PendingApprovals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "PackageCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OwnerTenantCreations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OverlayParameterOverrides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OverlayControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OverlayCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OrganizationProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OrganizationEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OnePageGuides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "OnboardingWizards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "MappingWorkflowTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "MappingWorkflowSteps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "MappingQualityGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "MAPFrameworkConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "LlmConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "IntegrationHealthMetrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "IntegrationConnectors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ImportantBusinessServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "HumanRetainedResponsibilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "GovernanceRhythmTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "GovernanceRhythmItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "GovernanceCadences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "GeneratedControlSuites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Frameworks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "FrameworkControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "FrameworkCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidenceTypeCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidenceSourceIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidenceScoringCriteria",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EvidencePeriodEnd",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EvidencePeriodStart",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileVersion",
                table: "Evidences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginalUploadDate",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalUploader",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionEndDate",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EvidencePackFamilies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EventSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EventSchemaRegistries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EventDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EscalationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ERPSystemConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ERPExtractExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ERPExtractConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailThreads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailMailboxes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailAutoReplyRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "EmailAttachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "DomainEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "DelegationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "DelegationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "DeadLetterEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "DataQualityScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CryptographicAssets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CrossReferenceMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlTestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceControlCode",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceControlTitle",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceFrameworkCode",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlObjectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlDomains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlChangeHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ControlCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ComplianceGuardrails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ComplianceEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CCMTestExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CCMExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CCMControlTests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CapturedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CanonicalControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "CadenceExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "BaselineControlSets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "BaselineControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "BaselineCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AutoTaggedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AuditFindings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AssessmentScopes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "Assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AssessmentRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApprovalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApprovalInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApprovalChains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApplicabilityRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApplicabilityRuleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ApplicabilityEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentSoDViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentSoDRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentConfidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "AgentActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCode",
                table: "ActionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RiskCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DefaultRiskAppetite = table.Column<int>(type: "integer", nullable: false),
                    EscalationThreshold = table.Column<int>(type: "integer", nullable: false),
                    EscalationRoles = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IconClass = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskCategories_RiskCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "RiskCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RiskControlMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingStrength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedEffectiveness = table.Column<int>(type: "integer", nullable: false),
                    ActualEffectiveness = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastAssessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAssessedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AssessmentNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskControlMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskControlMappings_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskControlMappings_Risks_RiskId",
                        column: x => x.RiskId,
                        principalTable: "Risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskTreatments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskId = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Owner = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedResidualRisk = table.Column<int>(type: "integer", nullable: false),
                    ActualResidualRisk = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric", nullable: true),
                    TransferParty = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AcceptanceJustification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskTreatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskTreatments_Risks_RiskId",
                        column: x => x.RiskId,
                        principalTable: "Risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SerialCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialCounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskTypes_RiskCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "RiskCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskTreatmentControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedEffectiveness = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskTreatmentControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskTreatmentControls_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskTreatmentControls_RiskTreatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "RiskTreatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_ParentCategoryId",
                table: "RiskCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskControlMappings_ControlId",
                table: "RiskControlMappings",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskControlMappings_RiskId",
                table: "RiskControlMappings",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskTreatmentControls_ControlId",
                table: "RiskTreatmentControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskTreatmentControls_TreatmentId",
                table: "RiskTreatmentControls",
                column: "TreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskTreatments_RiskId",
                table: "RiskTreatments",
                column: "RiskId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskTypes_CategoryId",
                table: "RiskTypes",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskControlMappings");

            migrationBuilder.DropTable(
                name: "RiskTreatmentControls");

            migrationBuilder.DropTable(
                name: "RiskTypes");

            migrationBuilder.DropTable(
                name: "SerialCounters");

            migrationBuilder.DropTable(
                name: "RiskTreatments");

            migrationBuilder.DropTable(
                name: "RiskCategories");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkspaceMemberships");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkspaceControls");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkspaceApprovalGates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TrialRequests");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TenantCode",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantEvidenceRequirements");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "SectorFrameworkIndex");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RoleLandingConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RequirementNotes");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Regulators");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "SupersedesDocumentCode",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VersionMajor",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VersionMinor",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VersionNotes",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PlatformAdmins");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Frameworks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidenceScoringCriteria");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "EvidencePeriodEnd",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "EvidencePeriodStart",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "FileVersion",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "OriginalUploadDate",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "OriginalUploader",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "RetentionEndDate",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailThreads");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailTasks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailMailboxes");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailAutoReplyRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "EmailAttachments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "SourceControlCode",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "SourceControlTitle",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "SourceFrameworkCode",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ComplianceEvents");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "AgentActions");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "ActionPlans");
        }
    }
}
