using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceIndexesAndUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // WORKSPACEID INDEXES - Performance Optimization
            // =====================================================
            // Add indexes for WorkspaceId foreign key columns to improve query performance
            // on workspace-scoped entities

            // Core GRC Entities
            migrationBuilder.CreateIndex(
                name: "IX_Risks_WorkspaceId",
                table: "Risks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_WorkspaceId",
                table: "Assessments",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_WorkspaceId",
                table: "Evidence",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_WorkspaceId",
                table: "Controls",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_WorkspaceId",
                table: "Audits",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_WorkspaceId",
                table: "Policies",
                column: "WorkspaceId");

            // Team Entities
            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_WorkspaceId",
                table: "TeamMembers",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RACIAssignments_WorkspaceId",
                table: "RACIAssignments",
                column: "WorkspaceId");

            // Planning and Reporting
            migrationBuilder.CreateIndex(
                name: "IX_Plans_WorkspaceId",
                table: "Plans",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_WorkspaceId",
                table: "Reports",
                column: "WorkspaceId");

            // =====================================================
            // COMPOSITE UNIQUE CONSTRAINTS - Data Integrity
            // =====================================================
            // Add composite unique constraints to prevent duplicate records per tenant
            // This ensures data integrity at the database level

            // Risk: Unique risk name per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Risks_TenantId_Name_Unique",
                table: "Risks",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [Name] IS NOT NULL AND [IsDeleted] = 0");

            // Control: Unique control ID per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Controls_TenantId_ControlId_Unique",
                table: "Controls",
                columns: new[] { "TenantId", "ControlId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [ControlId] IS NOT NULL AND [IsDeleted] = 0");

            // Assessment: Unique assessment number per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_TenantId_AssessmentNumber_Unique",
                table: "Assessments",
                columns: new[] { "TenantId", "AssessmentNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [AssessmentNumber] IS NOT NULL AND [IsDeleted] = 0");

            // Policy: Unique policy number per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Policies_TenantId_PolicyNumber_Unique",
                table: "Policies",
                columns: new[] { "TenantId", "PolicyNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [PolicyNumber] IS NOT NULL AND [IsDeleted] = 0");

            // Audit: Unique audit number per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Audits_TenantId_AuditNumber_Unique",
                table: "Audits",
                columns: new[] { "TenantId", "AuditNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [AuditNumber] IS NOT NULL AND [IsDeleted] = 0");

            // Vendor: Unique vendor code per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TenantId_VendorCode_Unique",
                table: "Vendors",
                columns: new[] { "TenantId", "VendorCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [VendorCode] IS NOT NULL AND [IsDeleted] = 0");

            // Incident: Unique incident number per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TenantId_IncidentNumber_Unique",
                table: "Incidents",
                columns: new[] { "TenantId", "IncidentNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [IncidentNumber] IS NOT NULL AND [IsDeleted] = 0");

            // ActionPlan: Unique action plan number per tenant
            migrationBuilder.CreateIndex(
                name: "IX_ActionPlans_TenantId_ActionPlanNumber_Unique",
                table: "ActionPlans",
                columns: new[] { "TenantId", "ActionPlanNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [ActionPlanNumber] IS NOT NULL AND [IsDeleted] = 0");

            // Asset: Unique asset code per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId_AssetCode_Unique",
                table: "Assets",
                columns: new[] { "TenantId", "AssetCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [AssetCode] IS NOT NULL AND [IsDeleted] = 0");

            // Workspace: Unique workspace name per tenant
            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_TenantId_Name_Unique",
                table: "Workspaces",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [Name] IS NOT NULL AND [IsDeleted] = 0");

            // EmailMailbox: Unique email address per tenant
            migrationBuilder.CreateIndex(
                name: "IX_EmailMailboxes_TenantId_EmailAddress_Unique",
                table: "EmailMailboxes",
                columns: new[] { "TenantId", "EmailAddress" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [EmailAddress] IS NOT NULL AND [IsDeleted] = 0");

            // SerialCodeRegistry: Unique code (already has unique constraint, but adding for completeness)
            // Note: This table already has a unique constraint on Code column from previous migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // DROP COMPOSITE UNIQUE CONSTRAINTS
            // =====================================================
            migrationBuilder.DropIndex(name: "IX_EmailMailboxes_TenantId_EmailAddress_Unique", table: "EmailMailboxes");
            migrationBuilder.DropIndex(name: "IX_Workspaces_TenantId_Name_Unique", table: "Workspaces");
            migrationBuilder.DropIndex(name: "IX_Assets_TenantId_AssetCode_Unique", table: "Assets");
            migrationBuilder.DropIndex(name: "IX_ActionPlans_TenantId_ActionPlanNumber_Unique", table: "ActionPlans");
            migrationBuilder.DropIndex(name: "IX_Incidents_TenantId_IncidentNumber_Unique", table: "Incidents");
            migrationBuilder.DropIndex(name: "IX_Vendors_TenantId_VendorCode_Unique", table: "Vendors");
            migrationBuilder.DropIndex(name: "IX_Audits_TenantId_AuditNumber_Unique", table: "Audits");
            migrationBuilder.DropIndex(name: "IX_Policies_TenantId_PolicyNumber_Unique", table: "Policies");
            migrationBuilder.DropIndex(name: "IX_Assessments_TenantId_AssessmentNumber_Unique", table: "Assessments");
            migrationBuilder.DropIndex(name: "IX_Controls_TenantId_ControlId_Unique", table: "Controls");
            migrationBuilder.DropIndex(name: "IX_Risks_TenantId_Name_Unique", table: "Risks");

            // =====================================================
            // DROP WORKSPACEID INDEXES
            // =====================================================
            migrationBuilder.DropIndex(name: "IX_Reports_WorkspaceId", table: "Reports");
            migrationBuilder.DropIndex(name: "IX_Plans_WorkspaceId", table: "Plans");
            migrationBuilder.DropIndex(name: "IX_RACIAssignments_WorkspaceId", table: "RACIAssignments");
            migrationBuilder.DropIndex(name: "IX_TeamMembers_WorkspaceId", table: "TeamMembers");
            migrationBuilder.DropIndex(name: "IX_Teams_WorkspaceId", table: "Teams");
            migrationBuilder.DropIndex(name: "IX_Policies_WorkspaceId", table: "Policies");
            migrationBuilder.DropIndex(name: "IX_Audits_WorkspaceId", table: "Audits");
            migrationBuilder.DropIndex(name: "IX_Controls_WorkspaceId", table: "Controls");
            migrationBuilder.DropIndex(name: "IX_Evidence_WorkspaceId", table: "Evidence");
            migrationBuilder.DropIndex(name: "IX_Assessments_WorkspaceId", table: "Assessments");
            migrationBuilder.DropIndex(name: "IX_Risks_WorkspaceId", table: "Risks");
        }
    }
}
