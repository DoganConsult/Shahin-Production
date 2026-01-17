using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddGrcLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: FirstAdminUserId and OnboardingStartedAt columns already exist in database
            // They were added in a previous migration that wasn't properly tracked

            migrationBuilder.CreateTable(
                name: "LookupCloudProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HasKsaRegion = table.Column<bool>(type: "boolean", nullable: false),
                    HasGccRegion = table.Column<bool>(type: "boolean", nullable: false),
                    Certifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataResidencyOptions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCloudProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Iso2Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Iso3Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsGccCountry = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresDataLocalization = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCountries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SensitivityLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiresEncryption = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresConsent = table.Column<bool>(type: "boolean", nullable: false),
                    SubjectToRetention = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultRetentionYears = table.Column<int>(type: "integer", nullable: false),
                    ApplicableRegulations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupDataTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupFrameworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuingBody = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SunsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ApplicableSectors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Prerequisites = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EstimatedControlCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedImplementationMonths = table.Column<int>(type: "integer", nullable: false),
                    ComplexityLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupFrameworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupHostingModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityConsiderations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ComplianceImplications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequiresDataLocalization = table.Column<bool>(type: "boolean", nullable: false),
                    RecommendedControls = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupHostingModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupMaturityLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Characteristics = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecommendedActions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NextLevelRequirements = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupMaturityLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupOrganizationSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinEmployees = table.Column<int>(type: "integer", nullable: false),
                    MaxEmployees = table.Column<int>(type: "integer", nullable: false),
                    MinRevenue = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxRevenue = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RecommendedApproach = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecommendedTeamSize = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupOrganizationSizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupOrganizationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRegulatedEntity = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAudit = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicableRegulations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupOrganizationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupRegulators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    FullNameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RegulatedSectors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IssuedFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupRegulators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupRiskCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true),
                    TypicalCauses = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TypicalImpacts = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MitigationStrategies = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupRiskCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupRiskCategories_LookupRiskCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "LookupRiskCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LookupSectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GosiCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsicCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NaicsCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ParentSectorId = table.Column<int>(type: "integer", nullable: true),
                    PrimaryRegulatorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsCriticalInfrastructure = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresStricterCompliance = table.Column<bool>(type: "boolean", nullable: false),
                    RecommendedFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TypicalRisks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupSectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupSectors_LookupSectors_ParentSectorId",
                        column: x => x.ParentSectorId,
                        principalTable: "LookupSectors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WizardRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecommendationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecommendedValue = table.Column<string>(type: "text", nullable: false),
                    ReasonEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReasonAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WizardRecommendations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupRiskCategories_ParentCategoryId",
                table: "LookupRiskCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupSectors_ParentSectorId",
                table: "LookupSectors",
                column: "ParentSectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupCloudProviders");

            migrationBuilder.DropTable(
                name: "LookupCountries");

            migrationBuilder.DropTable(
                name: "LookupDataTypes");

            migrationBuilder.DropTable(
                name: "LookupFrameworks");

            migrationBuilder.DropTable(
                name: "LookupHostingModels");

            migrationBuilder.DropTable(
                name: "LookupMaturityLevels");

            migrationBuilder.DropTable(
                name: "LookupOrganizationSizes");

            migrationBuilder.DropTable(
                name: "LookupOrganizationTypes");

            migrationBuilder.DropTable(
                name: "LookupRegulators");

            migrationBuilder.DropTable(
                name: "LookupRiskCategories");

            migrationBuilder.DropTable(
                name: "LookupSectors");

            migrationBuilder.DropTable(
                name: "WizardRecommendations");

            // Note: FirstAdminUserId and OnboardingStartedAt columns already existed
            // Don't drop them in rollback
        }
    }
}
