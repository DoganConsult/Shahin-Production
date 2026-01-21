using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class OnboardingGRCIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerialNumberCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateKey = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    LastSequence = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberCounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantWorkflowConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkflowName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SlaMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CustomConfigJson = table.Column<string>(type: "text", nullable: true),
                    NotificationOverridesJson = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantWorkflowConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantWorkflowConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberCounters_TenantId_EntityType_DateKey",
                table: "SerialNumberCounters",
                columns: new[] { "TenantId", "EntityType", "DateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantWorkflowConfigs_TenantId_WorkflowCode",
                table: "TenantWorkflowConfigs",
                columns: new[] { "TenantId", "WorkflowCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialNumberCounters");

            migrationBuilder.DropTable(
                name: "TenantWorkflowConfigs");
        }
    }
}
