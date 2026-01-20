using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// AM-04: Add Access Review management tables for periodic access certification.
    /// Creates AccessReviews and AccessReviewItems tables with proper relationships.
    /// </summary>
    public partial class AddAccessReviewManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReviewType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InitiatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalItems = table.Column<int>(type: "integer", nullable: false),
                    ReviewedItems = table.Column<int>(type: "integer", nullable: false),
                    CertifiedItems = table.Column<int>(type: "integer", nullable: false),
                    RevokedItems = table.Column<int>(type: "integer", nullable: false),
                    ModifiedItems = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessReviewItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrentRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InactiveDays = table.Column<int>(type: "integer", nullable: true),
                    InclusionReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Decision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsExecuted = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessReviewItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessReviewItems_AccessReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "AccessReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_AccessReviews_TenantId_Status_DueDate",
                table: "AccessReviews",
                columns: new[] { "TenantId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessReviews_Status_CreatedAt",
                table: "AccessReviews",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessReviewItems_ReviewId_Decision",
                table: "AccessReviewItems",
                columns: new[] { "ReviewId", "Decision" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessReviewItems_UserId_ReviewId",
                table: "AccessReviewItems",
                columns: new[] { "UserId", "ReviewId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessReviewItems_IsExecuted_ExecutedAt",
                table: "AccessReviewItems",
                columns: new[] { "IsExecuted", "ExecutedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessReviewItems");

            migrationBuilder.DropTable(
                name: "AccessReviews");
        }
    }
}
