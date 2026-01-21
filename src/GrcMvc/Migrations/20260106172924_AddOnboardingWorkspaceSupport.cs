using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingWorkspaceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentTemplateId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultWorkspaceId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GrcPlanId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingStatus",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RoleLandingConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LandingDashboardId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultLandingPage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WidgetsJson = table.Column<string>(type: "text", nullable: true),
                    QuickActionsJson = table.Column<string>(type: "text", nullable: true),
                    NavigationJson = table.Column<string>(type: "text", nullable: true),
                    DefaultFiltersJson = table.Column<string>(type: "text", nullable: true),
                    FavoritesJson = table.Column<string>(type: "text", nullable: true),
                    NotificationPrefsJson = table.Column<string>(type: "text", nullable: true),
                    AssignableTaskTypesJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleLandingConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleLandingConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleLandingConfigs_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleLandingConfigs_TenantId",
                table: "RoleLandingConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLandingConfigs_WorkspaceId",
                table: "RoleLandingConfigs",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleLandingConfigs");

            migrationBuilder.DropColumn(
                name: "AssessmentTemplateId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DefaultWorkspaceId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "GrcPlanId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OnboardingStatus",
                table: "Tenants");
        }
    }
}
