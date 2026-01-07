using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceInsideTenantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkspaceTemplates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowTasks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Workflows",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowInstances",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowExecutions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowEscalations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowDefinitions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowAuditEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ValidationRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ValidationResults",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserWorkspaceTasks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserWorkspaces",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserProfiles",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserProfileAssignments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserNotificationPreferences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserConsents",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UniversalEvidencePacks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UniversalEvidencePackItems",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UITextEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TriggerRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TriggerExecutionLogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TitleCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ThirdPartyConcentrations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TestProcedures",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TenantWorkflowConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TenantUsers",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TenantTemplates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tenants",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TenantPackages",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TenantBaselines",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TemplateCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TeamsNotificationConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TeamType",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TeamCode",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Teams",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Teams",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Teams",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessUnit",
                table: "Teams",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsSharedTeam",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Teams",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Teams",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "TeamMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TeamMembers",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "TeamMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TaskDelegations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TaskComments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SystemOfRecordDefinitions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SyncJobs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SyncExecutionLogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SupportMessages",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SupportConversations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SuiteEvidenceRequests",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SuiteControlEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Subscriptions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SubscriptionPlans",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StrategicRoadmapMilestones",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StandardEvidenceItems",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SoDRuleDefinitions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SoDConflicts",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SlaRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SiteMapEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ShahinAIModules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ShahinAIBrandConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rulesets",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RuleExecutionLogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RoleTransitionPlans",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RoleProfiles",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RoleCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Risks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RiskResiliences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RiskIndicators",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RiskIndicatorMeasurements",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RiskIndicatorAlerts",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Resiliences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RequirementMappings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Reports",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RegulatoryRequirements",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RegulatorCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ScopeType",
                table: "RACIAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ScopeId",
                table: "RACIAssignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "RACIAssignments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RACI",
                table: "RACIAssignments",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RACIAssignments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "RACIAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PolicyViolations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PolicyDecisions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Policies",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Plans",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PlanPhases",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PlainLanguageControls",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PendingApprovals",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PackageCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OverlayParameterOverrides",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OverlayControlMappings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OverlayCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OrganizationProfiles",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OrganizationEntities",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OnePageGuides",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OnboardingWizards",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MappingWorkflowTemplates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MappingWorkflowSteps",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MappingQualityGates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MAPFrameworkConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LlmConfigurations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LegalDocuments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoices",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "IntegrationHealthMetrics",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "IntegrationConnectors",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ImportantBusinessServices",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HumanRetainedResponsibilities",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GovernanceRhythmTemplates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GovernanceRhythmItems",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GovernanceCadences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GeneratedControlSuites",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FrameworkControls",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FrameworkCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EvidenceTypeCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EvidenceSourceIntegrations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EvidenceScores",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Evidences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EvidencePacks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EvidencePackFamilies",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EventSubscriptions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EventSchemaRegistries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EventDeliveryLogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EscalationRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ERPSystemConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ERPExtractExecutions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ERPExtractConfigs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DomainEvents",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DelegationRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DelegationLogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DeadLetterEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DataQualityScores",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CryptographicAssets",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CrossReferenceMappings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlTestProcedures",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Controls",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlObjectives",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlExceptions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlEvidencePacks",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlDomains",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlChangeHistories",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ComplianceGuardrails",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CCMTestExecutions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CCMExceptions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CCMControlTests",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CapturedEvidences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CanonicalControls",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CadenceExecutions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BaselineControlSets",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BaselineControlMappings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BaselineCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AutoTaggedEvidences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Audits",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AuditFindings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AuditEvents",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Assets",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AssessmentScopes",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Assessments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AssessmentRequirements",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApprovalRecords",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApprovalInstances",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApprovalChains",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApplicabilityRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApplicabilityRuleCatalogs",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApplicabilityEntries",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentSoDViolations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentSoDRules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentDefinitions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentConfidenceScores",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentCapabilities",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentApprovalGates",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AgentActions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    WorkspaceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JurisdictionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RegulatorsJson = table.Column<string>(type: "text", nullable: true),
                    OverlaysJson = table.Column<string>(type: "text", nullable: true),
                    ConfigJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspaces_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceApprovalGates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GateCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MinApprovals = table.Column<int>(type: "integer", nullable: false),
                    SlaDays = table.Column<int>(type: "integer", nullable: false),
                    EscalationDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceApprovalGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceApprovalGates_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FrequencyOverride = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SlaDaysOverride = table.Column<int>(type: "integer", nullable: true),
                    OverlaySource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceControls_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceControls_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    WorkspaceRolesJson = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsWorkspaceAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceMemberships_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceApprovalGateApprovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApproverReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ApprovalOrder = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceApprovalGateApprovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceApprovalGateApprovers_WorkspaceApprovalGates_GateId",
                        column: x => x.GateId,
                        principalTable: "WorkspaceApprovalGates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_WorkspaceId",
                table: "TeamMembers",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_TenantId",
                table: "Risks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RACIAssignments_WorkspaceId",
                table: "RACIAssignments",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceApprovalGateApprovers_GateId",
                table: "WorkspaceApprovalGateApprovers",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceApprovalGates_WorkspaceId",
                table: "WorkspaceApprovalGates",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceControls_ControlId",
                table: "WorkspaceControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceControls_WorkspaceId",
                table: "WorkspaceControls",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMemberships_UserId",
                table: "WorkspaceMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMemberships_WorkspaceId",
                table: "WorkspaceMemberships",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_TenantId",
                table: "Workspaces",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_RACIAssignments_Workspaces_WorkspaceId",
                table: "RACIAssignments",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Workspaces_WorkspaceId",
                table: "TeamMembers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RACIAssignments_Workspaces_WorkspaceId",
                table: "RACIAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Workspaces_WorkspaceId",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropTable(
                name: "WorkspaceControls");

            migrationBuilder.DropTable(
                name: "WorkspaceMemberships");

            migrationBuilder.DropTable(
                name: "WorkspaceApprovalGates");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_WorkspaceId",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Risks_TenantId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_RACIAssignments_WorkspaceId",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "IsSharedTeam",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AgentActions");

            migrationBuilder.AlterColumn<string>(
                name: "TeamType",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TeamCode",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "BusinessUnit",
                table: "Teams",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "TeamMembers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ScopeType",
                table: "RACIAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ScopeId",
                table: "RACIAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "RACIAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RACI",
                table: "RACIAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);
        }
    }
}
