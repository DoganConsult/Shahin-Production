using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class FullplanEngagementSchemaCompatibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionPercentage",
                table: "WorkflowTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaturityLevel",
                table: "OnboardingWizards",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceTypeCode",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrameworkCode",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDigitalSignature",
                table: "Evidences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoCollected",
                table: "Evidences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHybridCollection",
                table: "Evidences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComments",
                table: "Evidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgentCommunicationContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FromAgentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToAgentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestSchemaJson = table.Column<string>(type: "text", nullable: false),
                    ResponseSchemaJson = table.Column<string>(type: "text", nullable: false),
                    ExpectedResponse = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorHandlingJson = table.Column<string>(type: "text", nullable: true),
                    ValidationRulesJson = table.Column<string>(type: "text", nullable: true),
                    ExampleJson = table.Column<string>(type: "text", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RetryPolicy = table.Column<string>(type: "text", nullable: true),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    RequiresAcknowledgment = table.Column<bool>(type: "boolean", nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AgentCommunicationContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentEventTriggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AgentAction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionJson = table.Column<string>(type: "text", nullable: true),
                    ParametersJson = table.Column<string>(type: "text", nullable: true),
                    DelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaxDailyExecutions = table.Column<int>(type: "integer", nullable: true),
                    CooldownSeconds = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_AgentEventTriggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    PointsValue = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CriteriaJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalLogicRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggerEvent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConditionJson = table.Column<string>(type: "text", nullable: false),
                    ActionsJson = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StopOnMatch = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicableFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApplicableRegions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ConditionalLogicRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngagementMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    FatigueScore = table.Column<int>(type: "integer", nullable: false),
                    MomentumScore = table.Column<int>(type: "integer", nullable: false),
                    OverallEngagementScore = table.Column<int>(type: "integer", nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ActionsInSession = table.Column<int>(type: "integer", nullable: false),
                    TasksCompletedToday = table.Column<int>(type: "integer", nullable: false),
                    EvidenceSubmittedToday = table.Column<int>(type: "integer", nullable: false),
                    ConsecutiveActiveDays = table.Column<int>(type: "integer", nullable: false),
                    ErrorsEncounteredToday = table.Column<int>(type: "integer", nullable: false),
                    HelpRequestsToday = table.Column<int>(type: "integer", nullable: false),
                    AverageResponseTimeMinutes = table.Column<double>(type: "double precision", nullable: false),
                    FactorBreakdownJson = table.Column<string>(type: "text", nullable: true),
                    EngagementState = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RecommendedAction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    WeeklyPoints = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalActivities = table.Column<int>(type: "integer", nullable: false),
                    BadgeCount = table.Column<int>(type: "integer", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WeeklyPointsResetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MonthlyPointsResetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivityCountsJson = table.Column<string>(type: "text", nullable: true),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentState = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_EngagementMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvidenceConfidenceScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceCredibilityScore = table.Column<int>(type: "integer", nullable: false),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false),
                    RelevanceScore = table.Column<int>(type: "integer", nullable: false),
                    TimelinessScore = table.Column<int>(type: "integer", nullable: false),
                    AutomationCoveragePercent = table.Column<int>(type: "integer", nullable: false),
                    CrossVerificationScore = table.Column<int>(type: "integer", nullable: false),
                    FormatComplianceScore = table.Column<int>(type: "integer", nullable: false),
                    SlaAdherenceDays = table.Column<int>(type: "integer", nullable: true),
                    SlaMet = table.Column<bool>(type: "boolean", nullable: true),
                    CollectionMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LowConfidenceFactorsJson = table.Column<string>(type: "text", nullable: true),
                    RecommendedAction = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HumanReviewTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    HumanReviewOutcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReviewerFeedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_EvidenceConfidenceScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotivationScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    InteractionQualityScore = table.Column<int>(type: "integer", nullable: false),
                    ControlAlignmentScore = table.Column<int>(type: "integer", nullable: false),
                    TaskImpactScore = table.Column<int>(type: "integer", nullable: false),
                    ProgressVisibilityScore = table.Column<int>(type: "integer", nullable: false),
                    AchievementRecognitionScore = table.Column<int>(type: "integer", nullable: false),
                    AuditTrailJson = table.Column<string>(type: "text", nullable: true),
                    MotivationLevel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousScore = table.Column<int>(type: "integer", nullable: true),
                    EngagementScore = table.Column<int>(type: "integer", nullable: false),
                    ConsistencyScore = table.Column<int>(type: "integer", nullable: false),
                    ProgressScore = table.Column<int>(type: "integer", nullable: false),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    CollaborationScore = table.Column<int>(type: "integer", nullable: false),
                    Trend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RequiresIntervention = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MotivationScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NextBestActionRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExpectedImpact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionParametersJson = table.Column<string>(type: "text", nullable: true),
                    ContextDataJson = table.Column<string>(type: "text", nullable: true),
                    TriggerConditionsJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserFeedback = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserRating = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_NextBestActionRecommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgressCertaintyIndexes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    RiskBand = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfidenceLevel = table.Column<int>(type: "integer", nullable: false),
                    TasksCompletedPercent = table.Column<double>(type: "double precision", nullable: false),
                    TaskVelocity = table.Column<double>(type: "double precision", nullable: false),
                    VelocityTrend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EvidenceRejectionRate = table.Column<double>(type: "double precision", nullable: false),
                    SlaBreachCount = table.Column<int>(type: "integer", nullable: false),
                    SlaAdherencePercent = table.Column<double>(type: "double precision", nullable: false),
                    AverageOverdueDays = table.Column<double>(type: "double precision", nullable: false),
                    OrgMaturityLevel = table.Column<int>(type: "integer", nullable: true),
                    ComplexityScore = table.Column<int>(type: "integer", nullable: true),
                    TotalTasks = table.Column<int>(type: "integer", nullable: false),
                    CompletedTasks = table.Column<int>(type: "integer", nullable: false),
                    OverdueTasks = table.Column<int>(type: "integer", nullable: false),
                    AtRiskTasks = table.Column<int>(type: "integer", nullable: false),
                    PrimaryRiskFactorsJson = table.Column<string>(type: "text", nullable: true),
                    FactorBreakdownJson = table.Column<string>(type: "text", nullable: true),
                    RecommendedIntervention = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PredictedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysFromBaseline = table.Column<int>(type: "integer", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousScore = table.Column<int>(type: "integer", nullable: true),
                    ScoreChange = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_ProgressCertaintyIndexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceAgentCode = table.Column<string>(type: "text", nullable: false),
                    TargetAgentCode = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ResponsePayloadJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RequiresResponse = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AgentMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentMessages_AgentCommunicationContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "AgentCommunicationContracts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AgentTriggerExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventPayloadJson = table.Column<string>(type: "text", nullable: true),
                    AgentInvoked = table.Column<bool>(type: "boolean", nullable: false),
                    AgentActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AgentTriggerExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentTriggerExecutions_AgentEventTriggers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "AgentEventTriggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AwardReason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TriggerEvent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContextJson = table.Column<string>(type: "text", nullable: true),
                    ConditionResult = table.Column<bool>(type: "boolean", nullable: false),
                    TriggeredActionsJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EvaluatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_RuleEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleEvaluations_ConditionalLogicRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "ConditionalLogicRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACC_From_To",
                table: "AgentCommunicationContracts",
                columns: new[] { "FromAgentCode", "ToAgentCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AET_Event_Active",
                table: "AgentEventTriggers",
                columns: new[] { "EventType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessages_ContractId",
                table: "AgentMessages",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentTriggerExecutions_TriggerId",
                table: "AgentTriggerExecutions",
                column: "TriggerId");

            migrationBuilder.CreateIndex(
                name: "IX_ATE_Tenant_Date",
                table: "AgentTriggerExecutions",
                columns: new[] { "TenantId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CLR_Active_Priority",
                table: "ConditionalLogicRules",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_CLR_Trigger",
                table: "ConditionalLogicRules",
                column: "TriggerEvent");

            migrationBuilder.CreateIndex(
                name: "IX_Engagement_Date",
                table: "EngagementMetrics",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Engagement_Tenant_User",
                table: "EngagementMetrics",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ECS_Evidence",
                table: "EvidenceConfidenceScores",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Motivation_Tenant_User",
                table: "MotivationScores",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_NBA_Tenant_Status",
                table: "NextBestActionRecommendations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NBA_User_Date",
                table: "NextBestActionRecommendations",
                columns: new[] { "TargetUserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PCI_Entity",
                table: "ProgressCertaintyIndexes",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_PCI_Tenant_Date",
                table: "ProgressCertaintyIndexes",
                columns: new[] { "TenantId", "CalculatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RuleEvaluations_RuleId",
                table: "RuleEvaluations",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentMessages");

            migrationBuilder.DropTable(
                name: "AgentTriggerExecutions");

            migrationBuilder.DropTable(
                name: "EngagementMetrics");

            migrationBuilder.DropTable(
                name: "EvidenceConfidenceScores");

            migrationBuilder.DropTable(
                name: "MotivationScores");

            migrationBuilder.DropTable(
                name: "NextBestActionRecommendations");

            migrationBuilder.DropTable(
                name: "ProgressCertaintyIndexes");

            migrationBuilder.DropTable(
                name: "RuleEvaluations");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "AgentCommunicationContracts");

            migrationBuilder.DropTable(
                name: "AgentEventTriggers");

            migrationBuilder.DropTable(
                name: "ConditionalLogicRules");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "WorkflowTasks");

            migrationBuilder.DropColumn(
                name: "MaturityLevel",
                table: "OnboardingWizards");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "EvidenceTypeCode",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "FrameworkCode",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "HasDigitalSignature",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "IsAutoCollected",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "IsHybridCollection",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "ReviewerComments",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                table: "Evidences");
        }
    }
}
