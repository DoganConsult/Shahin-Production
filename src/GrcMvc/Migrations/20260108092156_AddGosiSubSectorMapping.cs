using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddGosiSubSectorMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrcSubSectorMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GosiCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsicSection = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    SubSectorNameEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SubSectorNameAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MainSectorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MainSectorNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MainSectorNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegulatoryNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrimaryRegulator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrcSubSectorMappings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrcSubSectorMappings");
        }
    }
}
