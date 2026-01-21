using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseStudies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SummaryAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FullContent = table.Column<string>(type: "text", nullable: true),
                    FullContentAr = table.Column<string>(type: "text", nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IndustryAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompanyNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TimeToCompliance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImprovementMetric = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImprovementLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImprovementLabelAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ComplianceScore = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStudies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FeaturesJson = table.Column<string>(type: "text", nullable: true),
                    FeaturesJsonAr = table.Column<string>(type: "text", nullable: true),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxWorkspaces = table.Column<int>(type: "integer", nullable: false),
                    MaxFrameworks = table.Column<int>(type: "integer", nullable: false),
                    HasApiAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasPrioritySupport = table.Column<bool>(type: "boolean", nullable: false),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuoteAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AuthorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AuthorTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorTitleAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IndustryAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseStudies");

            migrationBuilder.DropTable(
                name: "PricingPlans");

            migrationBuilder.DropTable(
                name: "Testimonials");
        }
    }
}
