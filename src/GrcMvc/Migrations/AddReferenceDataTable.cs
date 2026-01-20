using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration to add ReferenceData table for dropdown options
    /// All onboarding wizard fields will use database-driven dropdowns
    /// </summary>
    public partial class AddReferenceDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LabelEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCommon = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IndustryContext = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrganizationTypeContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceData_Category",
                table: "ReferenceData",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceData_Category_Value",
                table: "ReferenceData",
                columns: new[] { "Category", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceData_IsActive",
                table: "ReferenceData",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceData_IsCommon",
                table: "ReferenceData",
                column: "IsCommon");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceData");
        }
    }
}
