using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantEmailColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlOwnerAssignments_Controls_ControlId",
                table: "ControlOwnerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ControlTests_Controls_ControlId",
                table: "ControlTests");

            migrationBuilder.DropIndex(
                name: "IX_IncidentTimelineEntries_Timestamp",
                table: "IncidentTimelineEntries");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_DetectedAt",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentNumber",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_Severity",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_Status",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TenantId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TenantId_Severity_Status",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TenantId_Status",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_ControlTests_TenantId",
                table: "ControlTests");

            migrationBuilder.DropIndex(
                name: "IX_ControlTests_TenantId_ControlId_TestedDate",
                table: "ControlTests");

            migrationBuilder.DropIndex(
                name: "IX_ControlTests_TestedDate",
                table: "ControlTests");

            migrationBuilder.DropIndex(
                name: "IX_ControlOwnerAssignments_ControlId_IsActive",
                table: "ControlOwnerAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ControlOwnerAssignments_OwnerId",
                table: "ControlOwnerAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_Code",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_ExpiryDate",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_Status",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_TenantId",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_TenantId_Category",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_TenantId_Status",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_CertificationAudits_AuditDate",
                table: "CertificationAudits");

            migrationBuilder.DropIndex(
                name: "IX_CertificationAudits_Result",
                table: "CertificationAudits");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "IncidentTimelineEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByName",
                table: "IncidentTimelineEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "EntryType",
                table: "IncidentTimelineEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IncidentTimelineEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RootCause",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Phase",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LessonsLearned",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentNumber",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HandlerName",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HandlerId",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetectionSource",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTeam",
                table: "Incidents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TesterName",
                table: "ControlTests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "TestType",
                table: "ControlTests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TestNotes",
                table: "ControlTests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TestMethodology",
                table: "ControlTests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewerName",
                table: "ControlTests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "ControlTests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "ControlTests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Recommendations",
                table: "ControlTests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Findings",
                table: "ControlTests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignmentType",
                table: "ControlOwnerAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Certifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Certifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBodyAr",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBody",
                table: "Certifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificationNumber",
                table: "Certifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Certifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "CertificationAudits",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ReportReference",
                table: "CertificationAudits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CertificationAudits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeadAuditorName",
                table: "CertificationAudits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditorName",
                table: "CertificationAudits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditType",
                table: "CertificationAudits",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Challenge",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallengeAr",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "CaseStudies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerQuote",
                table: "CaseStudies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerQuoteAr",
                table: "CaseStudies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTitle",
                table: "CaseStudies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTitleAr",
                table: "CaseStudies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Results",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultsAr",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutionAr",
                table: "CaseStudies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailsSentCount = table.Column<int>(type: "integer", nullable: false),
                    EmailsOpenedCount = table.Column<int>(type: "integer", nullable: false),
                    Interests = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnsubscribeToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptions", x => x.Id);
                });

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
                    TotalPointsAvailable = table.Column<int>(type: "integer", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    SpeedBonus = table.Column<int>(type: "integer", nullable: false),
                    ThoroughnessBonus = table.Column<int>(type: "integer", nullable: false),
                    QualityBonus = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    StarRating = table.Column<int>(type: "integer", nullable: false),
                    AchievementLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    RequiredQuestions = table.Column<int>(type: "integer", nullable: false),
                    QuestionsAnswered = table.Column<int>(type: "integer", nullable: false),
                    RequiredQuestionsAnswered = table.Column<int>(type: "integer", nullable: false),
                    CompletionPercent = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstimatedTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    ActualTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentTemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssessmentInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GrcRequirementIdsJson = table.Column<string>(type: "text", nullable: false),
                    GrcRequirementsCount = table.Column<int>(type: "integer", nullable: false),
                    GrcRequirementsSatisfied = table.Column<int>(type: "integer", nullable: false),
                    ComplianceFrameworksJson = table.Column<string>(type: "text", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkflowTasksJson = table.Column<string>(type: "text", nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "text", nullable: false),
                    ValidationAttempts = table.Column<int>(type: "integer", nullable: false),
                    DataQualityScore = table.Column<int>(type: "integer", nullable: false),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAppetiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinimumRiskScore = table.Column<int>(type: "integer", nullable: false),
                    MaximumRiskScore = table.Column<int>(type: "integer", nullable: false),
                    TargetRiskScore = table.Column<int>(type: "integer", nullable: false),
                    TolerancePercentage = table.Column<int>(type: "integer", nullable: false),
                    ImpactThreshold = table.Column<int>(type: "integer", nullable: false),
                    LikelihoodThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewReminderDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAppetiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAppetiteSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrialSignups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanySize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrialPlan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    UtmSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmMedium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UtmCampaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LandingPageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessCode = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DataClassification = table.Column<string>(type: "text", nullable: true),
                    LabelsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialSignups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrialSignups_TrialRequests_TrialRequestId",
                        column: x => x.TrialRequestId,
                        principalTable: "TrialRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_OnboardingWizardId",
                table: "OnboardingStepScores",
                column: "OnboardingWizardId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStepScores_TenantId",
                table: "OnboardingStepScores",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAppetiteSettings_TenantId",
                table: "RiskAppetiteSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialSignups_TrialRequestId",
                table: "TrialSignups",
                column: "TrialRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlOwnerAssignments_Controls_ControlId",
                table: "ControlOwnerAssignments",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ControlTests_Controls_ControlId",
                table: "ControlTests",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlOwnerAssignments_Controls_ControlId",
                table: "ControlOwnerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ControlTests_Controls_ControlId",
                table: "ControlTests");

            migrationBuilder.DropTable(
                name: "NewsletterSubscriptions");

            migrationBuilder.DropTable(
                name: "OnboardingStepScores");

            migrationBuilder.DropTable(
                name: "RiskAppetiteSettings");

            migrationBuilder.DropTable(
                name: "TrialSignups");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Challenge",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "ChallengeAr",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "CustomerQuote",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "CustomerQuoteAr",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "CustomerTitle",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "CustomerTitleAr",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "Results",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "ResultsAr",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "CaseStudies");

            migrationBuilder.DropColumn(
                name: "SolutionAr",
                table: "CaseStudies");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "IncidentTimelineEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByName",
                table: "IncidentTimelineEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EntryType",
                table: "IncidentTimelineEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IncidentTimelineEntries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Incidents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RootCause",
                table: "Incidents",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Phase",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LessonsLearned",
                table: "Incidents",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentNumber",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "HandlerName",
                table: "Incidents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HandlerId",
                table: "Incidents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetectionSource",
                table: "Incidents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Incidents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTeam",
                table: "Incidents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TesterName",
                table: "ControlTests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TestType",
                table: "ControlTests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TestNotes",
                table: "ControlTests",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TestMethodology",
                table: "ControlTests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewerName",
                table: "ControlTests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "ControlTests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "ControlTests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Recommendations",
                table: "ControlTests",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Findings",
                table: "ControlTests",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "ControlOwnerAssignments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "ControlOwnerAssignments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ControlOwnerAssignments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "ControlOwnerAssignments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "ControlOwnerAssignments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignmentType",
                table: "ControlOwnerAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Certifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Certifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "Certifications",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "Certifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Certifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBodyAr",
                table: "Certifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBody",
                table: "Certifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "Certifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificationNumber",
                table: "Certifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Certifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "CertificationAudits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ReportReference",
                table: "CertificationAudits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CertificationAudits",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeadAuditorName",
                table: "CertificationAudits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditorName",
                table: "CertificationAudits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditType",
                table: "CertificationAudits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTimelineEntries_Timestamp",
                table: "IncidentTimelineEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_DetectedAt",
                table: "Incidents",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentNumber",
                table: "Incidents",
                column: "IncidentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Severity",
                table: "Incidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Status",
                table: "Incidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TenantId",
                table: "Incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TenantId_Severity_Status",
                table: "Incidents",
                columns: new[] { "TenantId", "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TenantId_Status",
                table: "Incidents",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlTests_TenantId",
                table: "ControlTests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlTests_TenantId_ControlId_TestedDate",
                table: "ControlTests",
                columns: new[] { "TenantId", "ControlId", "TestedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlTests_TestedDate",
                table: "ControlTests",
                column: "TestedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerAssignments_ControlId_IsActive",
                table: "ControlOwnerAssignments",
                columns: new[] { "ControlId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerAssignments_OwnerId",
                table: "ControlOwnerAssignments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_Code",
                table: "Certifications",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_ExpiryDate",
                table: "Certifications",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_Status",
                table: "Certifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_TenantId",
                table: "Certifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_TenantId_Category",
                table: "Certifications",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_TenantId_Status",
                table: "Certifications",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAudits_AuditDate",
                table: "CertificationAudits",
                column: "AuditDate");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAudits_Result",
                table: "CertificationAudits",
                column: "Result");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlOwnerAssignments_Controls_ControlId",
                table: "ControlOwnerAssignments",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ControlTests_Controls_ControlId",
                table: "ControlTests",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
