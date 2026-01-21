using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceIdToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_WorkspaceId",
                table: "Reports",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Workspaces_WorkspaceId",
                table: "Reports",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Workspaces_WorkspaceId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_WorkspaceId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Reports");
        }
    }
}
