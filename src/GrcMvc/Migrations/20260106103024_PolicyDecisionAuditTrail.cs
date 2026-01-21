using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class PolicyDecisionAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContextHash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContextJson = table.Column<string>(type: "text", nullable: false),
                    Decision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RulesEvaluated = table.Column<int>(type: "integer", nullable: false),
                    RulesMatched = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCached = table.Column<bool>(type: "boolean", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyDecisions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDecisions_ContextHash",
                table: "PolicyDecisions",
                column: "ContextHash");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDecisions_EvaluatedAt",
                table: "PolicyDecisions",
                column: "EvaluatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDecisions_TenantId_PolicyType_EvaluatedAt",
                table: "PolicyDecisions",
                columns: new[] { "TenantId", "PolicyType", "EvaluatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyDecisions");
        }
    }
}
