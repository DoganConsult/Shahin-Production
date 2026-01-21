using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentCenterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DescriptionEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrameworkCodes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FilePathEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilePathAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SectionsJson = table.Column<string>(type: "text", nullable: true),
                    FieldsJson = table.Column<string>(type: "text", nullable: true),
                    InstructionsEn = table.Column<string>(type: "text", nullable: true),
                    InstructionsAr = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBilingual = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentTemplates");
        }
    }
}
