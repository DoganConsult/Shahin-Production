using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration to add RiskAppetiteSettings table.
    /// Supports organizational risk appetite definition by category.
    /// </summary>
    public partial class AddRiskAppetiteSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskAppetiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinimumRiskScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaximumRiskScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    TargetRiskScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    TolerancePercentage = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    ImpactThreshold = table.Column<int>(type: "integer", nullable: false),
                    LikelihoodThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAppetiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAppetiteSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for common query patterns
            migrationBuilder.CreateIndex(
                name: "IX_RiskAppetiteSettings_TenantId",
                table: "RiskAppetiteSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAppetiteSettings_TenantId_Category",
                table: "RiskAppetiteSettings",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAppetiteSettings_TenantId_IsActive",
                table: "RiskAppetiteSettings",
                columns: new[] { "TenantId", "IsActive" });

            // Unique constraint on tenant + category + name
            migrationBuilder.CreateIndex(
                name: "IX_RiskAppetiteSettings_TenantId_Category_Name",
                table: "RiskAppetiteSettings",
                columns: new[] { "TenantId", "Category", "Name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskAppetiteSettings");
        }
    }
}
