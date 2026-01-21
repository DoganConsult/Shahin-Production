using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Data.Migrations
{
    /// <summary>
    /// Migration: Add Data Integrity Constraints
    /// Purpose: Add missing RowVersion concurrency tokens and unique BusinessCode constraints
    /// Generated: 2026-01-10
    ///
    /// Changes:
    /// 1. Add RowVersion (concurrency token) to frequently-updated entities
    /// 2. Add unique constraints on BusinessCode for all core entities
    /// 3. Add indexes on foreign keys for orphaned record detection
    ///
    /// Related Files:
    /// - GrcDbContext.cs (entity configurations)
    /// - DATA_INTEGRITY_AUDIT_REPORT.sql (orphaned records detection)
    /// </summary>
    public partial class AddDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========================================================================================================
            // PART 1: ADD ROWVERSION CONCURRENCY TOKENS
            // ========================================================================================================
            // These entities are frequently updated and need optimistic concurrency control
            // Currently only Risk has RowVersion (GrcDbContext.cs:426-428)
            // ========================================================================================================

            // Add RowVersion to Assessment (frequently updated during assessment workflow)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Assessments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Control (frequently updated during control implementation)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Controls",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Evidence (frequently updated during evidence review/approval)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Evidences",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Audit (frequently updated during audit execution)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Audits",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to AuditFinding (frequently updated during finding remediation)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AuditFindings",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Policy (frequently updated during policy review cycles)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Policies",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to PolicyViolation (frequently updated during violation resolution)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PolicyViolations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Workflow (frequently updated during workflow execution)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Workflows",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to WorkflowExecution (frequently updated during instance execution)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkflowExecutions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to AssessmentRequirement (frequently updated during requirement testing)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AssessmentRequirements",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to ControlImplementation (frequently updated during implementation tracking)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ControlImplementations",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to GapClosurePlan (frequently updated during gap remediation)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GapClosurePlans",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Tenant (frequently updated during tenant configuration changes)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tenants",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            // Add RowVersion to Workspace (frequently updated during workspace configuration changes)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Workspaces",
                type: "bytea",
                rowVersion: true,
                nullable: true);


            // ========================================================================================================
            // PART 2: ADD UNIQUE CONSTRAINTS ON BUSINESSCODE
            // ========================================================================================================
            // BusinessCode is an immutable, human-readable identifier that should be unique
            // Currently no unique constraint exists, allowing duplicates
            // Constraint name format: UK_{TableName}_BusinessCode
            // ========================================================================================================

            // Create unique constraint on Assessment.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Assessments_BusinessCode",
                table: "Assessments",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Control.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Controls_BusinessCode",
                table: "Controls",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Risk.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Risks_BusinessCode",
                table: "Risks",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Audit.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Audits_BusinessCode",
                table: "Audits",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Evidence.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Evidences_BusinessCode",
                table: "Evidences",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on AuditFinding.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_AuditFindings_BusinessCode",
                table: "AuditFindings",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Policy.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Policies_BusinessCode",
                table: "Policies",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on PolicyViolation.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_PolicyViolations_BusinessCode",
                table: "PolicyViolations",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Workflow.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Workflows_BusinessCode",
                table: "Workflows",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on WorkflowExecution.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_WorkflowExecutions_BusinessCode",
                table: "WorkflowExecutions",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on AssessmentRequirement.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_AssessmentRequirements_BusinessCode",
                table: "AssessmentRequirements",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on ControlImplementation.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_ControlImplementations_BusinessCode",
                table: "ControlImplementations",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on GapClosurePlan.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_GapClosurePlans_BusinessCode",
                table: "GapClosurePlans",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Baseline.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Baselines_BusinessCode",
                table: "Baselines",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");

            // Create unique constraint on Framework.BusinessCode
            migrationBuilder.CreateIndex(
                name: "UK_Frameworks_BusinessCode",
                table: "Frameworks",
                column: "BusinessCode",
                unique: true,
                filter: "\"BusinessCode\" IS NOT NULL AND \"IsDeleted\" = FALSE");


            // ========================================================================================================
            // PART 3: ADD INDEXES FOR ORPHANED RECORD DETECTION
            // ========================================================================================================
            // These indexes improve performance of orphaned record detection queries
            // They target foreign keys that use DeleteBehavior.SetNull
            // ========================================================================================================

            // Index for detecting Assessments with NULL RiskId (orphaned from Risk)
            // Supports query: SELECT * FROM Assessments WHERE RiskId IS NULL AND IsDeleted = FALSE
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_RiskId_IsDeleted",
                table: "Assessments",
                columns: new[] { "RiskId", "IsDeleted" },
                filter: "\"RiskId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Assessments with NULL ControlId (orphaned from Control)
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ControlId_IsDeleted",
                table: "Assessments",
                columns: new[] { "ControlId", "IsDeleted" },
                filter: "\"ControlId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Evidences with NULL AssessmentId (orphaned from Assessment)
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_AssessmentId_IsDeleted",
                table: "Evidences",
                columns: new[] { "AssessmentId", "IsDeleted" },
                filter: "\"AssessmentId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Evidences with NULL AuditId (orphaned from Audit)
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_AuditId_IsDeleted",
                table: "Evidences",
                columns: new[] { "AuditId", "IsDeleted" },
                filter: "\"AuditId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Evidences with NULL ControlId (orphaned from Control)
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_ControlId_IsDeleted",
                table: "Evidences",
                columns: new[] { "ControlId", "IsDeleted" },
                filter: "\"ControlId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Evidences with NULL AssessmentRequirementId (orphaned from AssessmentRequirement)
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_AssessmentRequirementId_IsDeleted",
                table: "Evidences",
                columns: new[] { "AssessmentRequirementId", "IsDeleted" },
                filter: "\"AssessmentRequirementId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Controls with NULL RiskId (orphaned from Risk)
            migrationBuilder.CreateIndex(
                name: "IX_Controls_RiskId_IsDeleted",
                table: "Controls",
                columns: new[] { "RiskId", "IsDeleted" },
                filter: "\"RiskId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting ControlImplementations with NULL ControlId (orphaned from Control)
            migrationBuilder.CreateIndex(
                name: "IX_ControlImplementations_ControlId_IsDeleted",
                table: "ControlImplementations",
                columns: new[] { "ControlId", "IsDeleted" },
                filter: "\"ControlId\" IS NULL AND \"IsDeleted\" = FALSE");

            // Index for detecting Baselines with NULL FrameworkId (orphaned from Framework)
            migrationBuilder.CreateIndex(
                name: "IX_Baselines_FrameworkId_IsDeleted",
                table: "Baselines",
                columns: new[] { "FrameworkId", "IsDeleted" },
                filter: "\"FrameworkId\" IS NULL AND \"IsDeleted\" = FALSE");


            // ========================================================================================================
            // PART 4: ADD INDEXES FOR TENANT/WORKSPACE FILTERING PERFORMANCE
            // ========================================================================================================
            // Composite indexes to improve multi-tenant query performance
            // ========================================================================================================

            // Composite index for tenant-scoped queries on Assessments
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_TenantId_WorkspaceId_IsDeleted",
                table: "Assessments",
                columns: new[] { "TenantId", "WorkspaceId", "IsDeleted" });

            // Composite index for tenant-scoped queries on Controls
            migrationBuilder.CreateIndex(
                name: "IX_Controls_TenantId_WorkspaceId_IsDeleted",
                table: "Controls",
                columns: new[] { "TenantId", "WorkspaceId", "IsDeleted" });

            // Composite index for tenant-scoped queries on Evidences
            migrationBuilder.CreateIndex(
                name: "IX_Evidences_TenantId_WorkspaceId_IsDeleted",
                table: "Evidences",
                columns: new[] { "TenantId", "WorkspaceId", "IsDeleted" });

            // Composite index for tenant-scoped queries on Audits
            migrationBuilder.CreateIndex(
                name: "IX_Audits_TenantId_WorkspaceId_IsDeleted",
                table: "Audits",
                columns: new[] { "TenantId", "WorkspaceId", "IsDeleted" });

            // Composite index for tenant-scoped queries on Risks
            migrationBuilder.CreateIndex(
                name: "IX_Risks_TenantId_WorkspaceId_IsDeleted",
                table: "Risks",
                columns: new[] { "TenantId", "WorkspaceId", "IsDeleted" });


            // ========================================================================================================
            // PART 5: ADD CHECK CONSTRAINTS FOR DATA VALIDATION
            // ========================================================================================================
            // These constraints enforce business rules at the database level
            // ========================================================================================================

            // Ensure CreatedDate is not in the future (prevents data entry errors)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Assessments""
                ADD CONSTRAINT ""CK_Assessments_CreatedDate_NotFuture""
                CHECK (""CreatedDate"" <= NOW());
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Controls""
                ADD CONSTRAINT ""CK_Controls_CreatedDate_NotFuture""
                CHECK (""CreatedDate"" <= NOW());
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences""
                ADD CONSTRAINT ""CK_Evidences_CreatedDate_NotFuture""
                CHECK (""CreatedDate"" <= NOW());
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Risks""
                ADD CONSTRAINT ""CK_Risks_CreatedDate_NotFuture""
                CHECK (""CreatedDate"" <= NOW());
            ");

            // Ensure ModifiedDate is not before CreatedDate (logical consistency)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Assessments""
                ADD CONSTRAINT ""CK_Assessments_ModifiedDate_AfterCreated""
                CHECK (""ModifiedDate"" IS NULL OR ""ModifiedDate"" >= ""CreatedDate"");
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Controls""
                ADD CONSTRAINT ""CK_Controls_ModifiedDate_AfterCreated""
                CHECK (""ModifiedDate"" IS NULL OR ""ModifiedDate"" >= ""CreatedDate"");
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences""
                ADD CONSTRAINT ""CK_Evidences_ModifiedDate_AfterCreated""
                CHECK (""ModifiedDate"" IS NULL OR ""ModifiedDate"" >= ""CreatedDate"");
            ");

            // Ensure DeletedAt is set when IsDeleted is TRUE (soft delete consistency)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Assessments""
                ADD CONSTRAINT ""CK_Assessments_DeletedAt_Consistency""
                CHECK ((""IsDeleted"" = TRUE AND ""DeletedAt"" IS NOT NULL) OR (""IsDeleted"" = FALSE AND ""DeletedAt"" IS NULL));
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Controls""
                ADD CONSTRAINT ""CK_Controls_DeletedAt_Consistency""
                CHECK ((""IsDeleted"" = TRUE AND ""DeletedAt"" IS NOT NULL) OR (""IsDeleted"" = FALSE AND ""DeletedAt"" IS NULL));
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences""
                ADD CONSTRAINT ""CK_Evidences_DeletedAt_Consistency""
                CHECK ((""IsDeleted"" = TRUE AND ""DeletedAt"" IS NOT NULL) OR (""IsDeleted"" = FALSE AND ""DeletedAt"" IS NULL));
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Risks""
                ADD CONSTRAINT ""CK_Risks_DeletedAt_Consistency""
                CHECK ((""IsDeleted"" = TRUE AND ""DeletedAt"" IS NOT NULL) OR (""IsDeleted"" = FALSE AND ""DeletedAt"" IS NULL));
            ");

            // Ensure Evidence FileSize is positive (prevent invalid file metadata)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences""
                ADD CONSTRAINT ""CK_Evidences_FileSize_Positive""
                CHECK (""FileSize"" IS NULL OR ""FileSize"" > 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ========================================================================================================
            // ROLLBACK: REMOVE ALL CONSTRAINTS AND COLUMNS ADDED IN UP MIGRATION
            // ========================================================================================================

            // Drop check constraints
            migrationBuilder.Sql(@"ALTER TABLE ""Assessments"" DROP CONSTRAINT IF EXISTS ""CK_Assessments_CreatedDate_NotFuture"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Controls"" DROP CONSTRAINT IF EXISTS ""CK_Controls_CreatedDate_NotFuture"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Evidences"" DROP CONSTRAINT IF EXISTS ""CK_Evidences_CreatedDate_NotFuture"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Risks"" DROP CONSTRAINT IF EXISTS ""CK_Risks_CreatedDate_NotFuture"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Assessments"" DROP CONSTRAINT IF EXISTS ""CK_Assessments_ModifiedDate_AfterCreated"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Controls"" DROP CONSTRAINT IF EXISTS ""CK_Controls_ModifiedDate_AfterCreated"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Evidences"" DROP CONSTRAINT IF EXISTS ""CK_Evidences_ModifiedDate_AfterCreated"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Assessments"" DROP CONSTRAINT IF EXISTS ""CK_Assessments_DeletedAt_Consistency"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Controls"" DROP CONSTRAINT IF EXISTS ""CK_Controls_DeletedAt_Consistency"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Evidences"" DROP CONSTRAINT IF EXISTS ""CK_Evidences_DeletedAt_Consistency"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Risks"" DROP CONSTRAINT IF EXISTS ""CK_Risks_DeletedAt_Consistency"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Evidences"" DROP CONSTRAINT IF EXISTS ""CK_Evidences_FileSize_Positive"";");

            // Drop tenant/workspace filtering indexes
            migrationBuilder.DropIndex(name: "IX_Risks_TenantId_WorkspaceId_IsDeleted", table: "Risks");
            migrationBuilder.DropIndex(name: "IX_Audits_TenantId_WorkspaceId_IsDeleted", table: "Audits");
            migrationBuilder.DropIndex(name: "IX_Evidences_TenantId_WorkspaceId_IsDeleted", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Controls_TenantId_WorkspaceId_IsDeleted", table: "Controls");
            migrationBuilder.DropIndex(name: "IX_Assessments_TenantId_WorkspaceId_IsDeleted", table: "Assessments");

            // Drop orphaned record detection indexes
            migrationBuilder.DropIndex(name: "IX_Baselines_FrameworkId_IsDeleted", table: "Baselines");
            migrationBuilder.DropIndex(name: "IX_ControlImplementations_ControlId_IsDeleted", table: "ControlImplementations");
            migrationBuilder.DropIndex(name: "IX_Controls_RiskId_IsDeleted", table: "Controls");
            migrationBuilder.DropIndex(name: "IX_Evidences_AssessmentRequirementId_IsDeleted", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Evidences_ControlId_IsDeleted", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Evidences_AuditId_IsDeleted", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Evidences_AssessmentId_IsDeleted", table: "Evidences");
            migrationBuilder.DropIndex(name: "IX_Assessments_ControlId_IsDeleted", table: "Assessments");
            migrationBuilder.DropIndex(name: "IX_Assessments_RiskId_IsDeleted", table: "Assessments");

            // Drop unique BusinessCode constraints
            migrationBuilder.DropIndex(name: "UK_Frameworks_BusinessCode", table: "Frameworks");
            migrationBuilder.DropIndex(name: "UK_Baselines_BusinessCode", table: "Baselines");
            migrationBuilder.DropIndex(name: "UK_GapClosurePlans_BusinessCode", table: "GapClosurePlans");
            migrationBuilder.DropIndex(name: "UK_ControlImplementations_BusinessCode", table: "ControlImplementations");
            migrationBuilder.DropIndex(name: "UK_AssessmentRequirements_BusinessCode", table: "AssessmentRequirements");
            migrationBuilder.DropIndex(name: "UK_WorkflowExecutions_BusinessCode", table: "WorkflowExecutions");
            migrationBuilder.DropIndex(name: "UK_Workflows_BusinessCode", table: "Workflows");
            migrationBuilder.DropIndex(name: "UK_PolicyViolations_BusinessCode", table: "PolicyViolations");
            migrationBuilder.DropIndex(name: "UK_Policies_BusinessCode", table: "Policies");
            migrationBuilder.DropIndex(name: "UK_AuditFindings_BusinessCode", table: "AuditFindings");
            migrationBuilder.DropIndex(name: "UK_Evidences_BusinessCode", table: "Evidences");
            migrationBuilder.DropIndex(name: "UK_Audits_BusinessCode", table: "Audits");
            migrationBuilder.DropIndex(name: "UK_Risks_BusinessCode", table: "Risks");
            migrationBuilder.DropIndex(name: "UK_Controls_BusinessCode", table: "Controls");
            migrationBuilder.DropIndex(name: "UK_Assessments_BusinessCode", table: "Assessments");

            // Drop RowVersion columns
            migrationBuilder.DropColumn(name: "RowVersion", table: "Workspaces");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Tenants");
            migrationBuilder.DropColumn(name: "RowVersion", table: "GapClosurePlans");
            migrationBuilder.DropColumn(name: "RowVersion", table: "ControlImplementations");
            migrationBuilder.DropColumn(name: "RowVersion", table: "AssessmentRequirements");
            migrationBuilder.DropColumn(name: "RowVersion", table: "WorkflowExecutions");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Workflows");
            migrationBuilder.DropColumn(name: "RowVersion", table: "PolicyViolations");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Policies");
            migrationBuilder.DropColumn(name: "RowVersion", table: "AuditFindings");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Audits");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Evidences");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Controls");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Assessments");
        }
    }
}
