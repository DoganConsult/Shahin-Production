using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedAiProviderConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: true),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    ModelId = table.Column<string>(type: "text", nullable: false),
                    ApiVersion = table.Column<string>(type: "text", nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    TopP = table.Column<decimal>(type: "numeric", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    MonthlyUsageLimit = table.Column<int>(type: "integer", nullable: false),
                    CurrentMonthUsage = table.Column<int>(type: "integer", nullable: false),
                    LastUsageResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomHeaders = table.Column<string>(type: "text", nullable: true),
                    RequestTemplate = table.Column<string>(type: "text", nullable: true),
                    ResponsePath = table.Column<string>(type: "text", nullable: true),
                    SystemPrompt = table.Column<string>(type: "text", nullable: true),
                    AllowedUseCases = table.Column<string>(type: "text", nullable: false),
                    ConfiguredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfiguredBy = table.Column<string>(type: "text", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProviderConfigurations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderConfigurations_TenantId",
                table: "AiProviderConfigurations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiProviderConfigurations");
        }
    }
}
