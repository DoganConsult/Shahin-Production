using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration: Add Professional Onboarding Achievement System
    /// Purpose: Track configuration quality, professional achievement levels, and link to GRC system
    /// Impact: Enables enterprise-grade motivation system (NOT childish gaming)
    /// Priority: HIGH - Critical for onboarding UX improvement
    /// </summary>
    public partial class OnboardingGamificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ══════════════════════════════════════════════════════════════
            // CREATE OnboardingStepScores TABLE
            // Tracks professional achievement metrics per configuration section
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateTable(
                name: "OnboardingStepScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingWizardId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    StepLetter = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    StepName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),

                    // Professional Scoring
                    TotalPointsAvailable = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SpeedBonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ThoroughnessBonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    QualityBonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StarRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AchievementLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),

                    // Progress Tracking
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RequiredQuestions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    QuestionsAnswered = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RequiredQuestionsAnswered = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletionPercent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotStarted"),

                    // Time Tracking
                    EstimatedTimeMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    ActualTimeMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),

                    // Assessment Template Linkage
                    AssessmentTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentTemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssessmentInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotCreated"),

                    // GRC Requirements Linkage
                    GrcRequirementIdsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    GrcRequirementsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GrcRequirementsSatisfied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ComplianceFrameworksJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),

                    // Workflow Linkage
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotTriggered"),
                    WorkflowTasksJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),

                    // Validation & Quality
                    ValidationErrorsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    ValidationAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DataQualityScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),

                    // Audit Fields (BaseEntity pattern)
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingStepScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingStepScores_OnboardingWizards_OnboardingWizardId",
                        column: x => x.OnboardingWizardId,
                        principalTable: "OnboardingWizards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnboardingStepScores_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_OnboardingStepScores_StepNumber", "\"StepNumber\" >= 1 AND \"StepNumber\" <= 12");
                    table.CheckConstraint("CK_OnboardingStepScores_StarRating", "\"StarRating\" >= 0 AND \"StarRating\" <= 5");
                    table.CheckConstraint("CK_OnboardingStepScores_CompletionPercent", "\"CompletionPercent\" >= 0 AND \"CompletionPercent\" <= 100");
                    table.CheckConstraint("CK_OnboardingStepScores_DataQualityScore", "\"DataQualityScore\" >= 0 AND \"DataQualityScore\" <= 100");
                    table.CheckConstraint("CK_OnboardingStepScores_CompletenessScore", "\"CompletenessScore\" >= 0 AND \"CompletenessScore\" <= 100");
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_OnboardingWizardId",
                table: "OnboardingStepScores",
                column: "OnboardingWizardId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_TenantId",
                table: "OnboardingStepScores",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_StepNumber",
                table: "OnboardingStepScores",
                column: "StepNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_Status",
                table: "OnboardingStepScores",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_AssessmentTemplateId",
                table: "OnboardingStepScores",
                column: "AssessmentTemplateId",
                filter: "\"AssessmentTemplateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_WorkflowId",
                table: "OnboardingStepScores",
                column: "WorkflowId",
                filter: "\"WorkflowId\" IS NOT NULL");

            // ══════════════════════════════════════════════════════════════
            // UPDATE OnboardingWizards TABLE - Add Gamification Summary Fields
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.AddColumn<int>(
                name: "TotalPointsEarned",
                table: "OnboardingWizards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPointsAvailable",
                table: "OnboardingWizards",
                type: "integer",
                nullable: false,
                defaultValue: 1300); // Sum of all 12 steps

            migrationBuilder.AddColumn<int>(
                name: "OverallScore",
                table: "OnboardingWizards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AchievementLevel",
                table: "OnboardingWizards",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedStepsJson",
                table: "OnboardingWizards",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "LeaderboardRank",
                table: "OnboardingWizards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeToComplete",
                table: "OnboardingWizards",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop OnboardingStepScores table
            migrationBuilder.DropTable(
                name: "OnboardingStepScores");

            // Drop added columns from OnboardingWizards
            migrationBuilder.DropColumn(
                name: "TotalPointsEarned",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "TotalPointsAvailable",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "AchievementLevel",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "CompletedStepsJson",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "LeaderboardRank",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "TimeToComplete",
                table: "OnboardingWizards");
        }
    }
}
