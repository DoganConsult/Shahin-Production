using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class Add43LayerArchitectureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingAnswerSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingWizardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CompletedStep = table.Column<int>(type: "integer", nullable: false),
                    SectionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AnswersJson = table.Column<string>(type: "text", nullable: false),
                    AnswersHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SnapshotAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFinalSnapshot = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OnboardingAnswerSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingAnswerSnapshots_OnboardingWizards_OnboardingWizar~",
                        column: x => x.OnboardingWizardId,
                        principalTable: "OnboardingWizards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantControlSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ControlName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ControlNameAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ControlDomain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ControlFamily = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApplicabilityStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApplicabilityReason = table.Column<string>(type: "text", nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    OwnerTeam = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvidenceTypesJson = table.Column<string>(type: "text", nullable: false),
                    ImplementationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComplianceStatus = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    LastAssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAssessmentDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExplainabilityPayloadId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TenantControlSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantControlSets_Controls_CatalogControlId",
                        column: x => x.CatalogControlId,
                        principalTable: "Controls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantControlSets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantFrameworkSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FrameworkNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FrameworkVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SelectionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Applicability = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RulesEvaluationLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExplainabilityPayloadId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectionReason = table.Column<string>(type: "text", nullable: false),
                    RegulatorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EstimatedControlCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedImplementationMonths = table.Column<int>(type: "integer", nullable: false),
                    ComplianceDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComplianceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivationReason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TenantFrameworkSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantFrameworkSelections_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantOverlays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverlayType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OverlayCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OverlayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OverlayNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SelectionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RulesEvaluationLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationReason = table.Column<string>(type: "text", nullable: false),
                    TriggerCondition = table.Column<string>(type: "text", nullable: false),
                    AdditionalControlsJson = table.Column<string>(type: "text", nullable: false),
                    AdditionalControlCount = table.Column<int>(type: "integer", nullable: false),
                    ModifiedControlsJson = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_TenantOverlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantOverlays_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantRiskProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessesPII = table.Column<bool>(type: "boolean", nullable: false),
                    PIIVolume = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProcessesPCI = table.Column<bool>(type: "boolean", nullable: false),
                    PCIScopeLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProcessesPHI = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessesClassifiedData = table.Column<bool>(type: "boolean", nullable: false),
                    ClassificationLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataTypesJson = table.Column<string>(type: "text", nullable: false),
                    HasCrossBorderTransfers = table.Column<bool>(type: "boolean", nullable: false),
                    TransferCountriesJson = table.Column<string>(type: "text", nullable: false),
                    HasThirdPartySharing = table.Column<bool>(type: "boolean", nullable: false),
                    ThirdPartiesJson = table.Column<string>(type: "text", nullable: false),
                    IsCriticalInfrastructure = table.Column<bool>(type: "boolean", nullable: false),
                    CriticalInfrastructureSector = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerVolumeTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionVolumeTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HasInternetFacing = table.Column<bool>(type: "boolean", nullable: false),
                    InternetFacingSystemsJson = table.Column<string>(type: "text", nullable: false),
                    UsesCloud = table.Column<bool>(type: "boolean", nullable: false),
                    CloudProvidersJson = table.Column<string>(type: "text", nullable: false),
                    UsesOT = table.Column<bool>(type: "boolean", nullable: false),
                    OTSystemsJson = table.Column<string>(type: "text", nullable: false),
                    OverallRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RiskScoreBreakdownJson = table.Column<string>(type: "text", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RulesEvaluationLogId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TenantRiskProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRiskProfiles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantScopeBoundaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ScopeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScopeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ScopeNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsInScope = table.Column<bool>(type: "boolean", nullable: false),
                    ExclusionRationale = table.Column<string>(type: "text", nullable: true),
                    ExclusionApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExclusionApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriticalityTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicableControlsJson = table.Column<string>(type: "text", nullable: false),
                    TagsJson = table.Column<string>(type: "text", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    OwnerTeam = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_TenantScopeBoundaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantScopeBoundaries_TenantScopeBoundaries_ParentScopeId",
                        column: x => x.ParentScopeId,
                        principalTable: "TenantScopeBoundaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantScopeBoundaries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingRulesEvaluationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingWizardId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerStep = table.Column<int>(type: "integer", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RuleVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InputContextJson = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    OutputJson = table.Column<string>(type: "text", nullable: false),
                    ReasonText = table.Column<string>(type: "text", nullable: false),
                    ReasonTextAr = table.Column<string>(type: "text", nullable: true),
                    EvaluationDurationMs = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_OnboardingRulesEvaluationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingRulesEvaluationLogs_OnboardingAnswerSnapshots_Ans~",
                        column: x => x.AnswerSnapshotId,
                        principalTable: "OnboardingAnswerSnapshots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OnboardingRulesEvaluationLogs_OnboardingWizards_OnboardingW~",
                        column: x => x.OnboardingWizardId,
                        principalTable: "OnboardingWizards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExplainabilityPayloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubjectCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubjectName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OnboardingWizardId = table.Column<Guid>(type: "uuid", nullable: true),
                    RulesEvaluationLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PrimaryReason = table.Column<string>(type: "text", nullable: false),
                    PrimaryReasonAr = table.Column<string>(type: "text", nullable: true),
                    DetailedExplanationJson = table.Column<string>(type: "text", nullable: false),
                    SupportingReferencesJson = table.Column<string>(type: "text", nullable: false),
                    InputFactorsJson = table.Column<string>(type: "text", nullable: false),
                    IsOverridable = table.Column<bool>(type: "boolean", nullable: false),
                    OverriddenByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OverriddenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverrideDecision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OverrideJustification = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ExplainabilityPayloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExplainabilityPayloads_OnboardingRulesEvaluationLogs_RulesE~",
                        column: x => x.RulesEvaluationLogId,
                        principalTable: "OnboardingRulesEvaluationLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExplainabilityPayloads_OnboardingWizards_OnboardingWizardId",
                        column: x => x.OnboardingWizardId,
                        principalTable: "OnboardingWizards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OnboardingDerivedOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingWizardId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OutputCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OutputName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OutputNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OutputPayloadJson = table.Column<string>(type: "text", nullable: false),
                    Applicability = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RulesEvaluationLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    DerivedAtStep = table.Column<int>(type: "integer", nullable: false),
                    DerivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_OnboardingDerivedOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingDerivedOutputs_OnboardingRulesEvaluationLogs_Rule~",
                        column: x => x.RulesEvaluationLogId,
                        principalTable: "OnboardingRulesEvaluationLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OnboardingDerivedOutputs_OnboardingWizards_OnboardingWizard~",
                        column: x => x.OnboardingWizardId,
                        principalTable: "OnboardingWizards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExplainabilityPayloads_GeneratedAt",
                table: "ExplainabilityPayloads",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExplainabilityPayloads_OnboardingWizardId",
                table: "ExplainabilityPayloads",
                column: "OnboardingWizardId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplainabilityPayloads_RulesEvaluationLogId",
                table: "ExplainabilityPayloads",
                column: "RulesEvaluationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplainabilityPayloads_TenantId_DecisionType_SubjectCode",
                table: "ExplainabilityPayloads",
                columns: new[] { "TenantId", "DecisionType", "SubjectCode" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingAnswerSnapshots_OnboardingWizardId_Version",
                table: "OnboardingAnswerSnapshots",
                columns: new[] { "OnboardingWizardId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingAnswerSnapshots_SnapshotAt",
                table: "OnboardingAnswerSnapshots",
                column: "SnapshotAt");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingDerivedOutputs_IsActive",
                table: "OnboardingDerivedOutputs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingDerivedOutputs_OnboardingWizardId_OutputType_Outp~",
                table: "OnboardingDerivedOutputs",
                columns: new[] { "OnboardingWizardId", "OutputType", "OutputCode" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingDerivedOutputs_RulesEvaluationLogId",
                table: "OnboardingDerivedOutputs",
                column: "RulesEvaluationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingRulesEvaluationLogs_AnswerSnapshotId",
                table: "OnboardingRulesEvaluationLogs",
                column: "AnswerSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingRulesEvaluationLogs_EvaluatedAt",
                table: "OnboardingRulesEvaluationLogs",
                column: "EvaluatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingRulesEvaluationLogs_OnboardingWizardId_TriggerStep",
                table: "OnboardingRulesEvaluationLogs",
                columns: new[] { "OnboardingWizardId", "TriggerStep" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantControlSets_CatalogControlId",
                table: "TenantControlSets",
                column: "CatalogControlId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantControlSets_IsActive",
                table: "TenantControlSets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TenantControlSets_TenantId_FrameworkCode_ControlCode",
                table: "TenantControlSets",
                columns: new[] { "TenantId", "FrameworkCode", "ControlCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantControlSets_TenantId_Source",
                table: "TenantControlSets",
                columns: new[] { "TenantId", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantFrameworkSelections_IsActive",
                table: "TenantFrameworkSelections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TenantFrameworkSelections_TenantId_FrameworkCode",
                table: "TenantFrameworkSelections",
                columns: new[] { "TenantId", "FrameworkCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOverlays_IsActive",
                table: "TenantOverlays",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOverlays_TenantId_OverlayType_OverlayCode",
                table: "TenantOverlays",
                columns: new[] { "TenantId", "OverlayType", "OverlayCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantRiskProfiles_TenantId",
                table: "TenantRiskProfiles",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantScopeBoundaries_IsActive",
                table: "TenantScopeBoundaries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TenantScopeBoundaries_IsInScope",
                table: "TenantScopeBoundaries",
                column: "IsInScope");

            migrationBuilder.CreateIndex(
                name: "IX_TenantScopeBoundaries_ParentScopeId",
                table: "TenantScopeBoundaries",
                column: "ParentScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantScopeBoundaries_TenantId_ScopeType_ScopeCode",
                table: "TenantScopeBoundaries",
                columns: new[] { "TenantId", "ScopeType", "ScopeCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExplainabilityPayloads");

            migrationBuilder.DropTable(
                name: "OnboardingDerivedOutputs");

            migrationBuilder.DropTable(
                name: "TenantControlSets");

            migrationBuilder.DropTable(
                name: "TenantFrameworkSelections");

            migrationBuilder.DropTable(
                name: "TenantOverlays");

            migrationBuilder.DropTable(
                name: "TenantRiskProfiles");

            migrationBuilder.DropTable(
                name: "TenantScopeBoundaries");

            migrationBuilder.DropTable(
                name: "OnboardingRulesEvaluationLogs");

            migrationBuilder.DropTable(
                name: "OnboardingAnswerSnapshots");
        }
    }
}
