using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationLastSentAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationResendCount",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiresAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationTokenHash",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationLastSentAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmailVerificationResendCount",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiresAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenHash",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Tenants");
        }
    }
}
