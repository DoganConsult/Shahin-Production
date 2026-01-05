using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddFrameworkControlsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FrameworkControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ControlNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequirementAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RequirementEn = table.Column<string>(type: "text", nullable: false),
                    ControlType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaturityLevel = table.Column<int>(type: "integer", nullable: false),
                    ImplementationGuidanceEn = table.Column<string>(type: "text", nullable: false),
                    EvidenceRequirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MappingIso27001 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MappingNist = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SearchKeywords = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultWeight = table.Column<int>(type: "integer", nullable: false),
                    MinEvidenceScore = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkControls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatorCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JurisdictionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sector = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Established = table.Column<int>(type: "integer", nullable: true),
                    RegionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatorCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FrameworkCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RegulatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    ControlCount = table.Column<int>(type: "integer", nullable: false),
                    Domains = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameworkCatalogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrameworkCatalogs_RegulatorCatalogs_RegulatorId",
                        column: x => x.RegulatorId,
                        principalTable: "RegulatorCatalogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ControlCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ControlNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subdomain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequirementAr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RequirementEn = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ControlType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MaturityLevel = table.Column<int>(type: "integer", nullable: false),
                    ImplementationGuidanceEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EvidenceRequirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MappingIso27001 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MappingNistCsf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MappingOther = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlCatalogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlCatalogs_FrameworkCatalogs_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "FrameworkCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlCatalogs_FrameworkId",
                table: "ControlCatalogs",
                column: "FrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkCatalogs_RegulatorId",
                table: "FrameworkCatalogs",
                column: "RegulatorId");

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControls_Domain",
                table: "FrameworkControls",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControls_FrameworkCode",
                table: "FrameworkControls",
                column: "FrameworkCode");

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControls_FrameworkCode_ControlNumber_Version",
                table: "FrameworkControls",
                columns: new[] { "FrameworkCode", "ControlNumber", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkControls_Status",
                table: "FrameworkControls",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlCatalogs");

            migrationBuilder.DropTable(
                name: "FrameworkControls");

            migrationBuilder.DropTable(
                name: "FrameworkCatalogs");

            migrationBuilder.DropTable(
                name: "RegulatorCatalogs");
        }
    }
}
