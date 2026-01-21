using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDelegationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "WorkflowTasks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromType = table.Column<string>(type: "text", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromUserName = table.Column<string>(type: "text", nullable: true),
                    FromAgentType = table.Column<string>(type: "text", nullable: true),
                    ToType = table.Column<string>(type: "text", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToUserName = table.Column<string>(type: "text", nullable: true),
                    ToAgentType = table.Column<string>(type: "text", nullable: true),
                    ToAgentTypesJson = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    DelegatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegationStrategy = table.Column<string>(type: "text", nullable: false),
                    SelectedAgentType = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDelegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskDelegations_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskDelegations_WorkflowTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDelegations_TaskId",
                table: "TaskDelegations",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDelegations_WorkflowInstanceId",
                table: "TaskDelegations",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskDelegations");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "WorkflowTasks");
        }
    }
}
