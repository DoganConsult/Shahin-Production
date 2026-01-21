using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowDefinitionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAssigneeRoleCode",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDays",
                table: "WorkflowDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkflowDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StepsJson",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalSteps",
                table: "WorkflowDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowType",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAssigneeRoleCode",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "EstimatedDays",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "StepsJson",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "TotalSteps",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "WorkflowType",
                table: "WorkflowDefinitions");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "WorkflowDefinitions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
