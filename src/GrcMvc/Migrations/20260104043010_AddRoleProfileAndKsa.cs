using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleProfileAndKsa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Abilities",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedScope",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeAreas",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KsaCompetencyLevel",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleProfileId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Layer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    Responsibilities = table.Column<string>(type: "text", nullable: false),
                    ApprovalLevel = table.Column<int>(type: "integer", nullable: false),
                    ApprovalAuthority = table.Column<decimal>(type: "numeric", nullable: true),
                    CanEscalate = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false),
                    CanReject = table.Column<bool>(type: "boolean", nullable: false),
                    CanReassign = table.Column<bool>(type: "boolean", nullable: false),
                    ParticipatingWorkflows = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleProfileId",
                table: "AspNetUsers",
                column: "RoleProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleProfiles_IsActive",
                table: "RoleProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RoleProfiles_Layer",
                table: "RoleProfiles",
                column: "Layer");

            migrationBuilder.CreateIndex(
                name: "IX_RoleProfiles_RoleCode",
                table: "RoleProfiles",
                column: "RoleCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_RoleProfiles_RoleProfileId",
                table: "AspNetUsers",
                column: "RoleProfileId",
                principalTable: "RoleProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_RoleProfiles_RoleProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "RoleProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Abilities",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssignedScope",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "KnowledgeAreas",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "KsaCompetencyLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "AspNetUsers");
        }
    }
}
