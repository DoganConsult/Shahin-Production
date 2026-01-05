using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrcMvc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase2WorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WorkflowInstance table
            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    WorkflowType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkflowInstanceId = table.Column<string>(type: "character varying(100)", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(100)", nullable: false),
                    InitiatedBy = table.Column<string>(type: "character varying(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // WorkflowTask table
            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<int>(type: "integer", nullable: false),
                    TaskName = table.Column<string>(type: "character varying(255)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "character varying(450)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionNotes = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: true),
                    IsEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedToUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // WorkflowApproval table
            migrationBuilder.CreateTable(
                name: "WorkflowApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<int>(type: "integer", nullable: false),
                    ApprovalLevel = table.Column<string>(type: "character varying(100)", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "character varying(450)", nullable: false),
                    Decision = table.Column<string>(type: "character varying(50)", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApproversRole = table.Column<string>(type: "character varying(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // WorkflowTransition table
            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<int>(type: "integer", nullable: false),
                    FromState = table.Column<string>(type: "character varying(100)", nullable: false),
                    ToState = table.Column<string>(type: "character varying(100)", nullable: false),
                    TriggeredBy = table.Column<string>(type: "character varying(450)", nullable: false),
                    TransitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ContextData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // WorkflowNotification table
            migrationBuilder.CreateTable(
                name: "WorkflowNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<int>(type: "integer", nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(100)", nullable: false),
                    RecipientUserId = table.Column<string>(type: "character varying(450)", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNotifications_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_WorkflowType_Status",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "WorkflowType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_EntityId_EntityType",
                table: "WorkflowInstances",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId_Status",
                table: "WorkflowTasks",
                columns: new[] { "WorkflowInstanceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedToUserId_Status",
                table: "WorkflowTasks",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApprovals_WorkflowInstanceId",
                table: "WorkflowApprovals",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowInstanceId",
                table: "WorkflowTransitions",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNotifications_WorkflowInstanceId_IsSent",
                table: "WorkflowNotifications",
                columns: new[] { "WorkflowInstanceId", "IsSent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowNotifications");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "WorkflowApprovals");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");
        }
    }
}
