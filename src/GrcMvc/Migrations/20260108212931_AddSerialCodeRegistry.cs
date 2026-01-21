using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialCodeRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerialCodeRegistry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    PreviousVersionCode = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialCodeRegistry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialCodeReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservedCode = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialCodeReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialSequenceCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CurrentSequence = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialSequenceCounters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Code",
                table: "SerialCodeRegistry",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Created",
                table: "SerialCodeRegistry",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Entity",
                table: "SerialCodeRegistry",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Prefix",
                table: "SerialCodeRegistry",
                column: "Prefix");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Sequence",
                table: "SerialCodeRegistry",
                columns: new[] { "Prefix", "TenantCode", "Stage", "Year", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Stage",
                table: "SerialCodeRegistry",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Status",
                table: "SerialCodeRegistry",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Tenant",
                table: "SerialCodeRegistry",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeRegistry_Year",
                table: "SerialCodeRegistry",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeReservation_Code",
                table: "SerialCodeReservations",
                column: "ReservedCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeReservation_Expires",
                table: "SerialCodeReservations",
                column: "ExpiresAt",
                filter: "\"Status\" = 'reserved'");

            migrationBuilder.CreateIndex(
                name: "IX_SerialCodeReservation_Status",
                table: "SerialCodeReservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SerialSequenceCounter_Unique",
                table: "SerialSequenceCounters",
                columns: new[] { "Prefix", "TenantCode", "Stage", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialCodeRegistry");

            migrationBuilder.DropTable(
                name: "SerialCodeReservations");

            migrationBuilder.DropTable(
                name: "SerialSequenceCounters");
        }
    }
}
