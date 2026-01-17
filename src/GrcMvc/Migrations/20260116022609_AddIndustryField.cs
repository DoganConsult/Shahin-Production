using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataIsolationLevel",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StorageUsedBytes",
                table: "Tenants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlan",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "abuse_event_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    ActionTaken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abuse_event_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "abuse_ip_tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    FailedRegistrationAttempts = table.Column<int>(type: "integer", nullable: false),
                    RateLimitViolations = table.Column<int>(type: "integer", nullable: false),
                    CaptchaFailures = table.Column<int>(type: "integer", nullable: false),
                    TotalSuspiciousActivities = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlockExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlockReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CountersResetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abuse_ip_tracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ip_access_list",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddressOrRange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ListType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ip_access_list", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abuse_event_logs");

            migrationBuilder.DropTable(
                name: "abuse_ip_tracking");

            migrationBuilder.DropTable(
                name: "ip_access_list");

            migrationBuilder.DropColumn(
                name: "DataIsolationLevel",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StorageUsedBytes",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlan",
                table: "Tenants");
        }
    }
}
