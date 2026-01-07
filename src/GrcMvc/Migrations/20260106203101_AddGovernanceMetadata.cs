using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_RoleProfiles_RoleProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerTenantCreations_AspNetUsers_OwnerId",
                table: "OwnerTenantCreations");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformAdmins_AspNetUsers_UserId",
                table: "PlatformAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleFeatures_AspNetRoles_RoleId",
                table: "RoleFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_AspNetRoles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantRoleConfigurations_AspNetRoles_RoleId",
                table: "TenantRoleConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantUsers_AspNetUsers_UserId",
                table: "TenantUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_AspNetUsers_UserId",
                table: "UserConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_AspNetRoles_RoleId",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_AspNetUsers_UserId",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkspaces_AspNetUsers_UserId",
                table: "UserWorkspaces");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMemberships_AspNetUsers_UserId",
                table: "WorkspaceMemberships");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_RoleId",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_UserId",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TenantRoleConfigurations_RoleId",
                table: "TenantRoleConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RoleFeatures_RoleId",
                table: "RoleFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "ApplicationUser");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RoleProfileId",
                table: "ApplicationUser",
                newName: "IX_ApplicationUser_RoleProfileId");

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkspaceTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkspaceTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkspaceTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Workspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Workspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Workspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkspaceMemberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkspaceMemberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkspaceMemberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkspaceControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkspaceControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkspaceControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkspaceApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkspaceApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkspaceApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkspaceApprovalGateApprovers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkspaceApprovalGateApprovers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkspaceApprovalGateApprovers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Workflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Workflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Workflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowEscalations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowEscalations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowEscalations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WorkflowAuditEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WorkflowAuditEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WorkflowAuditEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WebhookSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WebhookSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WebhookSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "WebhookDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "WebhookDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WebhookDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ValidationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ValidationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ValidationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ValidationResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ValidationResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ValidationResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserWorkspaceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserWorkspaceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserWorkspaceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserWorkspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserWorkspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserWorkspaces",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserProfileAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserProfileAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserProfileAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserNotificationPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserNotificationPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserNotificationPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UserConsents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UserConsents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UserConsents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UniversalEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UniversalEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UniversalEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UniversalEvidencePackItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UniversalEvidencePackItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UniversalEvidencePackItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "UITextEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "UITextEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "UITextEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TriggerRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TriggerRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TriggerRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TriggerExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TriggerExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TriggerExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TitleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TitleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TitleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ThirdPartyConcentrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ThirdPartyConcentrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ThirdPartyConcentrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TenantWorkflowConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TenantWorkflowConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TenantWorkflowConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TenantUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TenantUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TenantUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TenantTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TenantTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TenantTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TenantPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TenantPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TenantPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TenantBaselines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TenantBaselines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TenantBaselines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TemplateCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TemplateCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TemplateCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TeamsNotificationConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TeamsNotificationConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TeamsNotificationConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TeamMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TaskDelegations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TaskDelegations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TaskDelegations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "TaskComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "TaskComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "TaskComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SystemOfRecordDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SystemOfRecordDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SystemOfRecordDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SyncJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SyncJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SyncJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SyncExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SyncExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SyncExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SupportMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SupportMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SupportMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SupportConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SupportConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SupportConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SuiteEvidenceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SuiteEvidenceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SuiteEvidenceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SuiteControlEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SuiteControlEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SuiteControlEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "StrategicRoadmapMilestones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "StrategicRoadmapMilestones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "StrategicRoadmapMilestones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "StandardEvidenceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "StandardEvidenceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "StandardEvidenceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SoDRuleDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SoDRuleDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SoDRuleDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SoDConflicts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SoDConflicts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SoDConflicts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SlaRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SlaRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SlaRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "SiteMapEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "SiteMapEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "SiteMapEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ShahinAIModules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ShahinAIModules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ShahinAIModules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ShahinAIBrandConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ShahinAIBrandConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ShahinAIBrandConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Rulesets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Rulesets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Rulesets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Rules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Rules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Rules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RuleExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RuleExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RuleExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RoleTransitionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RoleTransitionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RoleTransitionPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RoleProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RoleProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RoleProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RoleLandingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RoleLandingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RoleLandingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RoleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RoleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RoleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RiskResiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RiskResiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RiskResiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RiskIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RiskIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RiskIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RiskIndicatorMeasurements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RiskIndicatorMeasurements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RiskIndicatorMeasurements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RiskIndicatorAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RiskIndicatorAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RiskIndicatorAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Resiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Resiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Resiliences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RequirementMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RequirementMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RequirementMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RegulatoryRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RegulatoryRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RegulatoryRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RegulatorCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RegulatorCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RegulatorCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "RACIAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "RACIAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "RACIAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PolicyViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PolicyViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PolicyViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PolicyDecisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PolicyDecisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PolicyDecisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PlatformAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PlatformAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PlatformAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Plans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Plans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Plans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PlanPhases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PlanPhases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PlainLanguageControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PlainLanguageControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PlainLanguageControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PendingApprovals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PendingApprovals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PendingApprovals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "PackageCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "PackageCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PackageCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OwnerTenantCreations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OwnerTenantCreations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OverlayParameterOverrides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OverlayParameterOverrides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OverlayParameterOverrides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OverlayControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OverlayControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OverlayControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OverlayCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OverlayCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OverlayCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OrganizationProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OrganizationProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OrganizationProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OrganizationEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OrganizationEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OrganizationEntities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OnePageGuides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OnePageGuides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OnePageGuides",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "OnboardingWizards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "OnboardingWizards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "OnboardingWizards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "MappingWorkflowTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "MappingWorkflowTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "MappingWorkflowTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "MappingWorkflowSteps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "MappingWorkflowSteps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "MappingWorkflowSteps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "MappingQualityGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "MappingQualityGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "MappingQualityGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "MAPFrameworkConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "MAPFrameworkConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "MAPFrameworkConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "LlmConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "LlmConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "LlmConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "IntegrationHealthMetrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "IntegrationHealthMetrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "IntegrationHealthMetrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "IntegrationConnectors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "IntegrationConnectors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "IntegrationConnectors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ImportantBusinessServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ImportantBusinessServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ImportantBusinessServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "HumanRetainedResponsibilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "HumanRetainedResponsibilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "HumanRetainedResponsibilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "GovernanceRhythmTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "GovernanceRhythmTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "GovernanceRhythmTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "GovernanceRhythmItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "GovernanceRhythmItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "GovernanceRhythmItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "GovernanceCadences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "GovernanceCadences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "GovernanceCadences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "GeneratedControlSuites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "GeneratedControlSuites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "GeneratedControlSuites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "FrameworkControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "FrameworkControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "FrameworkControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "FrameworkCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "FrameworkCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "FrameworkCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EvidenceTypeCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EvidenceTypeCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EvidenceTypeCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EvidenceSourceIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EvidenceSourceIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EvidenceSourceIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EvidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EvidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EvidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EvidencePackFamilies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EvidencePackFamilies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EvidencePackFamilies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EventSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EventSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EventSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EventSchemaRegistries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EventSchemaRegistries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EventSchemaRegistries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EventDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EventDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EventDeliveryLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "EscalationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "EscalationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "EscalationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ERPSystemConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ERPSystemConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ERPSystemConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ERPExtractExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ERPExtractExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ERPExtractExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ERPExtractConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ERPExtractConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ERPExtractConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "DomainEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "DomainEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DomainEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "DelegationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "DelegationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DelegationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "DelegationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "DelegationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DelegationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "DeadLetterEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "DeadLetterEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DeadLetterEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "DataQualityScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "DataQualityScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DataQualityScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CryptographicAssets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CryptographicAssets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CryptographicAssets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CrossReferenceMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CrossReferenceMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CrossReferenceMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlTestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlTestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlTestProcedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Controls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlObjectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlObjectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlObjectives",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlEvidencePacks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlDomains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlDomains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlDomains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlChangeHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlChangeHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlChangeHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ControlCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ControlCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ControlCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ComplianceGuardrails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ComplianceGuardrails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ComplianceGuardrails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CCMTestExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CCMTestExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CCMTestExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CCMExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CCMExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CCMExceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CCMControlTests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CCMControlTests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CCMControlTests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CapturedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CapturedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CapturedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CanonicalControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CanonicalControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CanonicalControls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "CadenceExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "CadenceExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "CadenceExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "BaselineControlSets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "BaselineControlSets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "BaselineControlSets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "BaselineControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "BaselineControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "BaselineControlMappings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "BaselineCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "BaselineCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "BaselineCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AutoTaggedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AutoTaggedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AutoTaggedEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AuditFindings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AuditFindings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AuditFindings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AssessmentScopes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AssessmentScopes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AssessmentScopes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "Assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "Assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AssessmentRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AssessmentRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AssessmentRequirements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApprovalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApprovalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApprovalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApprovalInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApprovalInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApprovalInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApprovalChains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApprovalChains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApprovalChains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApplicabilityRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApplicabilityRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApplicabilityRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApplicabilityRuleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApplicabilityRuleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApplicabilityRuleCatalogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "ApplicabilityEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "ApplicabilityEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ApplicabilityEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentSoDViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentSoDViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentSoDViolations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentSoDRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentSoDRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentSoDRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentConfidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentConfidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentConfidenceScores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentCapabilities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentApprovalGates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataClassification",
                table: "AgentActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelsJson",
                table: "AgentActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgentActions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ApplicationUser",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "ApplicationUser",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "ApplicationUser",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ApplicationUser",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangedAt",
                table: "ApplicationUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "ApplicationUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUser",
                table: "ApplicationUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUser_RoleProfiles_RoleProfileId",
                table: "ApplicationUser",
                column: "RoleProfileId",
                principalTable: "RoleProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerTenantCreations_ApplicationUser_OwnerId",
                table: "OwnerTenantCreations",
                column: "OwnerId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformAdmins_ApplicationUser_UserId",
                table: "PlatformAdmins",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantUsers_ApplicationUser_UserId",
                table: "TenantUsers",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_ApplicationUser_UserId",
                table: "UserConsents",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkspaces_ApplicationUser_UserId",
                table: "UserWorkspaces",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMemberships_ApplicationUser_UserId",
                table: "WorkspaceMemberships",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUser_RoleProfiles_RoleProfileId",
                table: "ApplicationUser");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerTenantCreations_ApplicationUser_OwnerId",
                table: "OwnerTenantCreations");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformAdmins_ApplicationUser_UserId",
                table: "PlatformAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantUsers_ApplicationUser_UserId",
                table: "TenantUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_ApplicationUser_UserId",
                table: "UserConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkspaces_ApplicationUser_UserId",
                table: "UserWorkspaces");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMemberships_ApplicationUser_UserId",
                table: "WorkspaceMemberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUser",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkspaceTemplates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkspaceMemberships");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkspaceMemberships");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkspaceMemberships");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkspaceControls");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkspaceControls");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkspaceControls");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkspaceApprovalGates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkspaceApprovalGates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkspaceApprovalGates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkspaceApprovalGateApprovers");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowEscalations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WorkflowAuditEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WebhookDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserWorkspaceTasks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserWorkspaces");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserProfileAssignments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UserConsents");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UniversalEvidencePacks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UniversalEvidencePackItems");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "UITextEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TriggerRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TriggerExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TitleCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ThirdPartyConcentrations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TestProcedures");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TenantWorkflowConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TenantTemplates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TenantPackages");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TenantBaselines");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TemplateCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TeamsNotificationConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SystemOfRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SyncExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SuiteEvidenceRequests");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SuiteControlEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "StrategicRoadmapMilestones");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "StandardEvidenceItems");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SoDRuleDefinitions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SoDConflicts");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SlaRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "SiteMapEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ShahinAIModules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ShahinAIBrandConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Rulesets");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RuleExecutionLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RoleTransitionPlans");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RoleProfiles");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RoleLandingConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RoleLandingConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RoleLandingConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RoleCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RiskResiliences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RiskIndicators");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RiskIndicatorMeasurements");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RiskIndicatorAlerts");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Resiliences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RequirementMappings");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RegulatoryRequirements");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RegulatorCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "RACIAssignments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PolicyDecisions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PlatformAdmins");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PlatformAdmins");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PlatformAdmins");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PlainLanguageControls");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PendingApprovals");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PackageCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OverlayParameterOverrides");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OverlayControlMappings");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OverlayCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OrganizationEntities");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OnePageGuides");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "MappingWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "MappingWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "MappingQualityGates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "MAPFrameworkConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "IntegrationHealthMetrics");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "IntegrationConnectors");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ImportantBusinessServices");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "HumanRetainedResponsibilities");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "GovernanceRhythmTemplates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "GovernanceRhythmItems");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "GovernanceCadences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "GeneratedControlSuites");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "FrameworkControls");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "FrameworkCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EvidenceTypeCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EvidenceSourceIntegrations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EvidenceScores");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EvidencePacks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EvidencePackFamilies");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EventSubscriptions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EventSchemaRegistries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EventDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ERPSystemConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ERPExtractExecutions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ERPExtractConfigs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DomainEvents");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DelegationRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DelegationLogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DeadLetterEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DataQualityScores");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CryptographicAssets");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CrossReferenceMappings");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlTestProcedures");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlObjectives");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlExceptions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlEvidencePacks");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlDomains");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlChangeHistories");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ControlCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ComplianceGuardrails");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CCMTestExecutions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CCMExceptions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CCMControlTests");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CapturedEvidences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CanonicalControls");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "CadenceExecutions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "BaselineControlSets");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "BaselineControlMappings");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "BaselineCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AutoTaggedEvidences");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AssessmentScopes");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AssessmentRequirements");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApprovalRecords");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApprovalChains");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApplicabilityRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ApplicabilityEntries");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentSoDViolations");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentSoDRules");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentDefinitions");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentConfidenceScores");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentCapabilities");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentApprovalGates");

            migrationBuilder.DropColumn(
                name: "DataClassification",
                table: "AgentActions");

            migrationBuilder.DropColumn(
                name: "LabelsJson",
                table: "AgentActions");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgentActions");

            migrationBuilder.DropColumn(
                name: "LastPasswordChangedAt",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "ApplicationUser");

            migrationBuilder.RenameTable(
                name: "ApplicationUser",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUser_RoleProfileId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RoleProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_RoleId",
                table: "UserRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId",
                table: "UserRoleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRoleConfigurations_RoleId",
                table: "TenantRoleConfigurations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleFeatures_RoleId",
                table: "RoleFeatures",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_RoleProfiles_RoleProfileId",
                table: "AspNetUsers",
                column: "RoleProfileId",
                principalTable: "RoleProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerTenantCreations_AspNetUsers_OwnerId",
                table: "OwnerTenantCreations",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformAdmins_AspNetUsers_UserId",
                table: "PlatformAdmins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleFeatures_AspNetRoles_RoleId",
                table: "RoleFeatures",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_AspNetRoles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRoleConfigurations_AspNetRoles_RoleId",
                table: "TenantRoleConfigurations",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantUsers_AspNetUsers_UserId",
                table: "TenantUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_AspNetUsers_UserId",
                table: "UserConsents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_AspNetRoles_RoleId",
                table: "UserRoleAssignments",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_AspNetUsers_UserId",
                table: "UserRoleAssignments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkspaces_AspNetUsers_UserId",
                table: "UserWorkspaces",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMemberships_AspNetUsers_UserId",
                table: "WorkspaceMemberships",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
