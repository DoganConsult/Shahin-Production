using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceIdToCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Risks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Plans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Evidences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Controls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Audits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Assessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Risks_WorkspaceId",
                table: "Risks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_WorkspaceId",
                table: "Policies",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_WorkspaceId",
                table: "Plans",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidences_WorkspaceId",
                table: "Evidences",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_WorkspaceId",
                table: "Controls",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_WorkspaceId",
                table: "Audits",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_WorkspaceId",
                table: "Assessments",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Workspaces_WorkspaceId",
                table: "Assessments",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Workspaces_WorkspaceId",
                table: "Audits",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Controls_Workspaces_WorkspaceId",
                table: "Controls",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Workspaces_WorkspaceId",
                table: "Evidences",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Workspaces_WorkspaceId",
                table: "Plans",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Workspaces_WorkspaceId",
                table: "Policies",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_Workspaces_WorkspaceId",
                table: "Risks",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Workspaces_WorkspaceId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Audits_Workspaces_WorkspaceId",
                table: "Audits");

            migrationBuilder.DropForeignKey(
                name: "FK_Controls_Workspaces_WorkspaceId",
                table: "Controls");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Workspaces_WorkspaceId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Workspaces_WorkspaceId",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Workspaces_WorkspaceId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_Workspaces_WorkspaceId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_WorkspaceId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Policies_WorkspaceId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Plans_WorkspaceId",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Evidences_WorkspaceId",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Controls_WorkspaceId",
                table: "Controls");

            migrationBuilder.DropIndex(
                name: "IX_Audits_WorkspaceId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_WorkspaceId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Assessments");
        }
    }
}
