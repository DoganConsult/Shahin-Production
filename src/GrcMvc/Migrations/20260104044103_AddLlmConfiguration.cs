using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Workflows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "WorkflowExecutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Risks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PolicyViolations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PlanPhases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Evidences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Controls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Audits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AuditFindings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Assessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LlmConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    EnabledForTenant = table.Column<bool>(type: "boolean", nullable: false),
                    MonthlyUsageLimit = table.Column<int>(type: "integer", nullable: false),
                    CurrentMonthUsage = table.Column<int>(type: "integer", nullable: false),
                    LastUsageResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfiguredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LlmConfigurations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LlmConfigurations_TenantId",
                table: "LlmConfigurations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmConfigurations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PolicyViolations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PlanPhases");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditFindings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Assessments");
        }
    }
}
