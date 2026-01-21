using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerTenantCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CredentialExpiresAt",
                table: "TenantUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedByOwnerId",
                table: "TenantUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOwnerGenerated",
                table: "TenantUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePasswordOnFirstLogin",
                table: "TenantUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AdminAccountGenerated",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdminAccountGeneratedAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BypassPayment",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByOwnerId",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CredentialExpiresAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOwnerCreated",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OwnerTenantCreations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUsername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CredentialsExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CredentialsDelivered = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerTenantCreations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerTenantCreations_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OwnerTenantCreations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerTenantCreations_OwnerId",
                table: "OwnerTenantCreations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerTenantCreations_TenantId",
                table: "OwnerTenantCreations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerTenantCreations_TenantId_OwnerId",
                table: "OwnerTenantCreations",
                columns: new[] { "TenantId", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerTenantCreations");

            migrationBuilder.DropColumn(
                name: "CredentialExpiresAt",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "GeneratedByOwnerId",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "IsOwnerGenerated",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "MustChangePasswordOnFirstLogin",
                table: "TenantUsers");

            migrationBuilder.DropColumn(
                name: "AdminAccountGenerated",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "AdminAccountGeneratedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BypassPayment",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CreatedByOwnerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CredentialExpiresAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsOwnerCreated",
                table: "Tenants");
        }
    }
}
