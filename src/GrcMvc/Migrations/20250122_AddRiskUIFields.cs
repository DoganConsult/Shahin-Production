using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskUIFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskNumber",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentifiedDate",
                table: "Risks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleParty",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsequenceArea",
                table: "Risks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskNumber",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "IdentifiedDate",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "ResponsibleParty",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "ConsequenceArea",
                table: "Risks");
        }
    }
}
