using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceScoringAndSectorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_FrameworkControls_FrameworkCode",
                table: "FrameworkControls",
                newName: "IX_FrameworkControl_Framework");

            migrationBuilder.RenameIndex(
                name: "IX_FrameworkControls_Domain",
                table: "FrameworkControls",
                newName: "IX_FrameworkControl_Domain");

            migrationBuilder.CreateTable(
                name: "EvidenceScoringCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseScore = table.Column<int>(type: "integer", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    ScoringRulesJson = table.Column<string>(type: "text", nullable: false),
                    MinimumScore = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresExpiry = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValidityDays = table.Column<int>(type: "integer", nullable: false),
                    AllowedFileTypes = table.Column<string>(type: "text", nullable: false),
                    MaxFileSizeMB = table.Column<int>(type: "integer", nullable: false),
                    RequiresDigitalSignature = table.Column<bool>(type: "boolean", nullable: false),
                    CollectionFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicableFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApplicableSectors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceScoringCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SectorFrameworkIndex",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SectorNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SectorNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrgType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrgTypeNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrgTypeNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    ReasonEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReasonAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ControlCount = table.Column<int>(type: "integer", nullable: false),
                    CriticalControlCount = table.Column<int>(type: "integer", nullable: false),
                    EvidenceTypesJson = table.Column<string>(type: "text", nullable: false),
                    EvidenceTypeCount = table.Column<int>(type: "integer", nullable: false),
                    ScoringWeight = table.Column<double>(type: "double precision", nullable: false),
                    EstimatedImplementationDays = table.Column<int>(type: "integer", nullable: false),
                    ImplementationGuidanceEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DeadlinesJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComputedHash = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorFrameworkIndex", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantEvidenceRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ControlNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinimumScore = table.Column<int>(type: "integer", nullable: false),
                    CollectionFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultValidityDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentScore = table.Column<int>(type: "integer", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantEvidenceRequirements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControl_Framework_Version",
                table: "FrameworkControls",
                columns: new[] { "FrameworkCode", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControl_Type",
                table: "FrameworkControls",
                column: "ControlType");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceScoringCriteria_Active",
                table: "EvidenceScoringCriteria",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceScoringCriteria_Category",
                table: "EvidenceScoringCriteria",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceScoringCriteria_TypeCode",
                table: "EvidenceScoringCriteria",
                column: "EvidenceTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectorFrameworkIndex_Active",
                table: "SectorFrameworkIndex",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SectorFrameworkIndex_Framework",
                table: "SectorFrameworkIndex",
                column: "FrameworkCode");

            migrationBuilder.CreateIndex(
                name: "IX_SectorFrameworkIndex_Sector_Framework",
                table: "SectorFrameworkIndex",
                columns: new[] { "SectorCode", "FrameworkCode" });

            migrationBuilder.CreateIndex(
                name: "IX_SectorFrameworkIndex_Sector_OrgType",
                table: "SectorFrameworkIndex",
                columns: new[] { "SectorCode", "OrgType" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_AssignedTo",
                table: "TenantEvidenceRequirements",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Tenant",
                table: "TenantEvidenceRequirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Tenant_DueDate",
                table: "TenantEvidenceRequirements",
                columns: new[] { "TenantId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Tenant_Framework",
                table: "TenantEvidenceRequirements",
                columns: new[] { "TenantId", "FrameworkCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Tenant_Status",
                table: "TenantEvidenceRequirements",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Unique",
                table: "TenantEvidenceRequirements",
                columns: new[] { "TenantId", "FrameworkCode", "ControlNumber", "EvidenceTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantEvidenceRequirement_Workspace",
                table: "TenantEvidenceRequirements",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceScoringCriteria");

            migrationBuilder.DropTable(
                name: "SectorFrameworkIndex");

            migrationBuilder.DropTable(
                name: "TenantEvidenceRequirements");

            migrationBuilder.DropIndex(
                name: "IX_FrameworkControl_Framework_Version",
                table: "FrameworkControls");

            migrationBuilder.DropIndex(
                name: "IX_FrameworkControl_Type",
                table: "FrameworkControls");

            migrationBuilder.RenameIndex(
                name: "IX_FrameworkControl_Framework",
                table: "FrameworkControls",
                newName: "IX_FrameworkControls_FrameworkCode");

            migrationBuilder.RenameIndex(
                name: "IX_FrameworkControl_Domain",
                table: "FrameworkControls",
                newName: "IX_FrameworkControls_Domain");
        }
    }
}
