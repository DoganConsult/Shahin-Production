using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddReportFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsequenceArea",
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
                name: "RiskNumber",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Risks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvidenceScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ScoringCriteria = table.Column<string>(type: "text", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    ScoredBy = table.Column<string>(type: "text", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceScores_Evidences_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "Evidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReportPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "text", nullable: false),
                    KeyFindings = table.Column<string>(type: "text", nullable: false),
                    Recommendations = table.Column<string>(type: "text", nullable: false),
                    TotalFindingsCount = table.Column<int>(type: "integer", nullable: false),
                    CriticalFindingsCount = table.Column<int>(type: "integer", nullable: false),
                    GeneratedBy = table.Column<string>(type: "text", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredTo = table.Column<string>(type: "text", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "text", nullable: true),
                    PageCount = table.Column<int>(type: "integer", nullable: false),
                    IncludedEntitiesJson = table.Column<string>(type: "text", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resiliences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Framework = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessedByUserName = table.Column<string>(type: "text", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserName = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserName = table.Column<string>(type: "text", nullable: true),
                    ResilienceScore = table.Column<decimal>(type: "numeric", nullable: true),
                    BusinessContinuityScore = table.Column<decimal>(type: "numeric", nullable: true),
                    DisasterRecoveryScore = table.Column<decimal>(type: "numeric", nullable: true),
                    CyberResilienceScore = table.Column<decimal>(type: "numeric", nullable: true),
                    OverallRating = table.Column<string>(type: "text", nullable: true),
                    AssessmentDetails = table.Column<string>(type: "text", nullable: true),
                    Findings = table.Column<string>(type: "text", nullable: true),
                    ActionItems = table.Column<string>(type: "text", nullable: true),
                    RelatedAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedRiskId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedWorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceUrls = table.Column<string>(type: "text", nullable: true),
                    ReportUrl = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resiliences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskResiliences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RiskCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RiskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RelatedRiskId = table.Column<Guid>(type: "uuid", nullable: true),
                    RiskToleranceLevel = table.Column<decimal>(type: "numeric", nullable: true),
                    RecoveryCapabilityScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ImpactMitigationScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ResilienceRating = table.Column<string>(type: "text", nullable: true),
                    RiskScenario = table.Column<string>(type: "text", nullable: true),
                    ResilienceMeasures = table.Column<string>(type: "text", nullable: true),
                    RecoveryPlan = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessedByUserName = table.Column<string>(type: "text", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserName = table.Column<string>(type: "text", nullable: true),
                    RelatedWorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskResiliences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PushEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledTypesJson = table.Column<string>(type: "text", nullable: false),
                    DigestFrequency = table.Column<string>(type: "text", nullable: false),
                    PreferredTime = table.Column<string>(type: "text", nullable: false),
                    Timezone = table.Column<string>(type: "text", nullable: false),
                    QuietHoursEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    QuietHoursStart = table.Column<string>(type: "text", nullable: false),
                    QuietHoursEnd = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileCode = table.Column<string>(type: "text", nullable: false),
                    ProfileName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSystemProfile = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PermissionsJson = table.Column<string>(type: "text", nullable: false),
                    WorkflowRolesJson = table.Column<string>(type: "text", nullable: false),
                    UiAccessJson = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TriggeredBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TransitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "UserProfileAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfileAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfileAssignments_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceScores_EvidenceId",
                table: "EvidenceScores",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CorrelationId",
                table: "Reports",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportNumber",
                table: "Reports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TenantId_Type_Status",
                table: "Reports",
                columns: new[] { "TenantId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Resiliences_AssessmentDate",
                table: "Resiliences",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Resiliences_TenantId",
                table: "Resiliences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Resiliences_TenantId_Status",
                table: "Resiliences",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskResiliences_AssessmentDate",
                table: "RiskResiliences",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_RiskResiliences_RelatedRiskId",
                table: "RiskResiliences",
                column: "RelatedRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskResiliences_TenantId",
                table: "RiskResiliences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskResiliences_TenantId_Status",
                table: "RiskResiliences",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_TenantId",
                table: "UserNotificationPreferences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileAssignments_TenantId",
                table: "UserProfileAssignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileAssignments_UserProfileId",
                table: "UserProfileAssignments",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_TransitionDate",
                table: "WorkflowTransitions",
                column: "TransitionDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowInstanceId",
                table: "WorkflowTransitions",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceScores");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Resiliences");

            migrationBuilder.DropTable(
                name: "RiskResiliences");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropTable(
                name: "UserProfileAssignments");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ConsequenceArea",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "IdentifiedDate",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "ResponsibleParty",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskNumber",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Risks");
        }
    }
}
