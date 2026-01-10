using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration: Add Performance Indexes
    /// Purpose: Add missing database indexes for high-traffic query paths
    /// Impact: Significant performance improvement on Controls, Risks, Evidence, WorkflowTasks, Incidents, EmailMessages tables
    /// Priority: HIGH - Production performance optimization
    /// </summary>
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ══════════════════════════════════════════════════════════════
            // CONTROLS TABLE - Multi-tenant filtering + category queries
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Controls_TenantId_Category",
                table: "Controls",
                columns: new[] { "TenantId", "Category" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_TenantId_WorkspaceId_Status",
                table: "Controls",
                columns: new[] { "TenantId", "WorkspaceId", "Status" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // RISKS TABLE - Multi-tenant filtering + status/level queries
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Risks_TenantId_Status",
                table: "Risks",
                columns: new[] { "TenantId", "Status" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_TenantId_WorkspaceId_RiskScore",
                table: "Risks",
                columns: new[] { "TenantId", "WorkspaceId", "RiskScore" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // EVIDENCE TABLE - Assessment requirement lookups + date sorting
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_AssessmentRequirementId_UploadedDate",
                table: "Evidences",
                columns: new[] { "AssessmentRequirementId", "UploadedDate" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Evidences_TenantId_Status",
                table: "Evidences",
                columns: new[] { "TenantId", "Status" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // WORKFLOW TASKS TABLE - User assignment + status + due date
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedToUserId_Status_DueDate",
                table: "WorkflowTasks",
                columns: new[] { "AssignedToUserId", "Status", "DueDate" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId_Status",
                table: "WorkflowTasks",
                columns: new[] { "WorkflowInstanceId", "Status" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // INCIDENTS TABLE - Multi-tenant + status + severity filtering
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TenantId_Status_Severity",
                table: "Incidents",
                columns: new[] { "TenantId", "Status", "Severity" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_DetectedAt",
                table: "Incidents",
                column: "DetectedAt",
                descending: new[] { true },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // EMAIL MESSAGES TABLE - Thread lookups + date sorting
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ThreadId_ReceivedAt",
                table: "EmailMessages",
                columns: new[] { "ThreadId", "ReceivedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_MailboxId_IsRead_ReceivedAt",
                table: "EmailMessages",
                columns: new[] { "MailboxId", "IsRead", "ReceivedAt" },
                descending: new[] { false, false, true });

            // ══════════════════════════════════════════════════════════════
            // ASSESSMENTS TABLE - Multi-tenant + status filtering
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_TenantId_Status_DueDate",
                table: "Assessments",
                columns: new[] { "TenantId", "Status", "DueDate" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // WORKFLOW INSTANCES TABLE - Tenant + status + date filtering
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_Status_StartedDate",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "Status", "StartedDate" },
                descending: new[] { false, false, true },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // AUDIT EVENTS TABLE - Date-based partitioning support
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_CreatedDate",
                table: "AuditEvents",
                columns: new[] { "TenantId", "CreatedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType_EntityId_CreatedDate",
                table: "AuditEvents",
                columns: new[] { "EntityType", "EntityId", "CreatedDate" },
                descending: new[] { false, false, true });

            // ══════════════════════════════════════════════════════════════
            // RULE EXECUTION LOGS TABLE - Performance for log queries
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_RulesetId_ExecutedAt",
                table: "RuleExecutionLogs",
                columns: new[] { "RulesetId", "ExecutedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_TenantId_ExecutedAt",
                table: "RuleExecutionLogs",
                columns: new[] { "TenantId", "ExecutedAt" },
                descending: new[] { false, true });

            // ══════════════════════════════════════════════════════════════
            // CERTIFICATIONS TABLE - Expiry tracking
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Certifications_ExpiryDate_Status",
                table: "Certifications",
                columns: new[] { "ExpiryDate", "Status" },
                filter: "\"Status\" = 'Active' AND \"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // POLICIES TABLE - Tenant + status filtering
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_Policies_TenantId_Status_EffectiveDate",
                table: "Policies",
                columns: new[] { "TenantId", "Status", "EffectiveDate" },
                descending: new[] { false, false, true },
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes created in Up()
            migrationBuilder.DropIndex(name: "IX_Controls_TenantId_Category", table: "Controls");
            migrationBuilder.DropIndex(name: "IX_Controls_TenantId_WorkspaceId_Status", table: "Controls");

            migrationBuilder.DropIndex(name: "IX_Risks_TenantId_Status", table: "Risks");
            migrationBuilder.DropIndex(name: "IX_Risks_TenantId_WorkspaceId_RiskScore", table: "Risks");

            migrationBuilder.DropIndex(name: "IX_Evidences_AssessmentRequirementId_UploadedDate", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Evidences_TenantId_Status", table: "Evidences");

            migrationBuilder.DropIndex(name: "IX_WorkflowTasks_AssignedToUserId_Status_DueDate", table: "WorkflowTasks");
            migrationBuilder.DropIndex(name: "IX_WorkflowTasks_WorkflowInstanceId_Status", table: "WorkflowTasks");

            migrationBuilder.DropIndex(name: "IX_Incidents_TenantId_Status_Severity", table: "Incidents");
            migrationBuilder.DropIndex(name: "IX_Incidents_DetectedAt", table: "Incidents");

            migrationBuilder.DropIndex(name: "IX_EmailMessages_ThreadId_ReceivedAt", table: "EmailMessages");
            migrationBuilder.DropIndex(name: "IX_EmailMessages_MailboxId_IsRead_ReceivedAt", table: "EmailMessages");

            migrationBuilder.DropIndex(name: "IX_Assessments_TenantId_Status_DueDate", table: "Assessments");

            migrationBuilder.DropIndex(name: "IX_WorkflowInstances_TenantId_Status_StartedDate", table: "WorkflowInstances");

            migrationBuilder.DropIndex(name: "IX_AuditEvents_TenantId_CreatedDate", table: "AuditEvents");
            migrationBuilder.DropIndex(name: "IX_AuditEvents_EntityType_EntityId_CreatedDate", table: "AuditEvents");

            migrationBuilder.DropIndex(name: "IX_RuleExecutionLogs_RulesetId_ExecutedAt", table: "RuleExecutionLogs");
            migrationBuilder.DropIndex(name: "IX_RuleExecutionLogs_TenantId_ExecutedAt", table: "RuleExecutionLogs");

            migrationBuilder.DropIndex(name: "IX_Certifications_ExpiryDate_Status", table: "Certifications");

            migrationBuilder.DropIndex(name: "IX_Policies_TenantId_Status_EffectiveDate", table: "Policies");
        }
    }
}
