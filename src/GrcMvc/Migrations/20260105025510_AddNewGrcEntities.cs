using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddNewGrcEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStageIndex",
                table: "WorkflowDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FlowDiagramJson",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MermaidDiagram",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StagesJson",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusFormat",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AnnualRevenueRange",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountType",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BranchCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CeoEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CeoName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CfoEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CfoName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CisoEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CisoName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudProviders",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegistrationNumber",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComplianceOfficerEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComplianceOfficerName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CriticalVendorCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DataSubjectCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DpoEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DpoName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalAuditorName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FiscalYearEnd",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasDataCenterInKSA",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasExternalAuditor",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThirdPartyDataProcessing",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeadquartersLocation",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "IncorporationDate",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndustryLicenses",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCriticalInfrastructure",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPubliclyTraded",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegulatedEntity",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubsidiary",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItSystemsJson",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalEntityName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalEntityNameAr",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalEntityType",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativeEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativeName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativePhone",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativeTitle",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingCompletedBy",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OnboardingProgressPercent",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingStartedAt",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingStatus",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingCountries",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationStructureJson",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParentCompanyName",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryRegulator",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ProcessesPersonalData",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessesSensitiveData",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddress",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCity",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegisteredRegion",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegulatoryCertifications",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryRegulators",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockExchange",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockSymbol",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubsidiaryCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaxIdentificationNumber",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyRiskLevel",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VendorCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailTemplateCode",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EscalateToRoleCode",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EscalateToUserId",
                table: "EscalationRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoursOverdueTrigger",
                table: "EscalationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "EscalationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RuleCode",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldNotifyManager",
                table: "EscalationRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldNotifyOriginalAssignee",
                table: "EscalationRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmsTemplateCode",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaskType",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TriggerConditionJson",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefinitionId",
                table: "EscalationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Assessments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrameworkCode",
                table: "Assessments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "Assessments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AssessmentRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlNumber = table.Column<string>(type: "text", nullable: false),
                    ControlTitle = table.Column<string>(type: "text", nullable: false),
                    ControlTitleAr = table.Column<string>(type: "text", nullable: false),
                    RequirementText = table.Column<string>(type: "text", nullable: false),
                    RequirementTextAr = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    ControlType = table.Column<string>(type: "text", nullable: false),
                    MaturityLevel = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EvidenceStatus = table.Column<string>(type: "text", nullable: false),
                    ImplementationGuidance = table.Column<string>(type: "text", nullable: false),
                    ImplementationGuidanceAr = table.Column<string>(type: "text", nullable: false),
                    ToolkitReference = table.Column<string>(type: "text", nullable: false),
                    SampleEvidenceDescription = table.Column<string>(type: "text", nullable: false),
                    BestPractices = table.Column<string>(type: "text", nullable: false),
                    CommonGaps = table.Column<string>(type: "text", nullable: false),
                    ScoringGuideJson = table.Column<string>(type: "text", nullable: false),
                    WeightPercentage = table.Column<int>(type: "integer", nullable: false),
                    IsAutoScorable = table.Column<bool>(type: "boolean", nullable: false),
                    AutoScoreRuleJson = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    MaxScore = table.Column<int>(type: "integer", nullable: true),
                    ScoreRationale = table.Column<string>(type: "text", nullable: false),
                    IsAutoScored = table.Column<bool>(type: "boolean", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScoredBy = table.Column<string>(type: "text", nullable: false),
                    OwnerRoleCode = table.Column<string>(type: "text", nullable: false),
                    ReviewerRoleCode = table.Column<string>(type: "text", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Findings = table.Column<string>(type: "text", nullable: false),
                    Recommendations = table.Column<string>(type: "text", nullable: false),
                    RemediationPlan = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentRequirements_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataQualityScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false),
                    AccuracyScore = table.Column<int>(type: "integer", nullable: false),
                    ConsistencyScore = table.Column<int>(type: "integer", nullable: false),
                    TimelinessScore = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    QualificationLevel = table.Column<string>(type: "text", nullable: false),
                    QualificationPoints = table.Column<int>(type: "integer", nullable: false),
                    IsQualified = table.Column<bool>(type: "boolean", nullable: false),
                    TotalFields = table.Column<int>(type: "integer", nullable: false),
                    ValidFields = table.Column<int>(type: "integer", nullable: false),
                    InvalidFields = table.Column<int>(type: "integer", nullable: false),
                    MissingFields = table.Column<int>(type: "integer", nullable: false),
                    IssuesJson = table.Column<string>(type: "text", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastImprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataQualityScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DelegationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DelegationType = table.Column<string>(type: "text", nullable: false),
                    DelegatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegatorRoleCode = table.Column<string>(type: "text", nullable: false),
                    DelegateUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegateRoleCode = table.Column<string>(type: "text", nullable: false),
                    DelegateSelectionRule = table.Column<string>(type: "text", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsIndefinite = table.Column<bool>(type: "boolean", nullable: false),
                    TaskTypesJson = table.Column<string>(type: "text", nullable: false),
                    WorkflowCategoriesJson = table.Column<string>(type: "text", nullable: false),
                    ApprovalAmountLimit = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovalLevelLimit = table.Column<string>(type: "text", nullable: false),
                    CanSubDelegate = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false),
                    CanReject = table.Column<bool>(type: "boolean", nullable: false),
                    CanReassign = table.Column<bool>(type: "boolean", nullable: false),
                    CanEscalate = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyDelegatorOnAction = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyDelegateOnAssignment = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDelegatorConfirmation = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedReason = table.Column<string>(type: "text", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlaRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ResponseTimeHours = table.Column<int>(type: "integer", nullable: false),
                    ResolutionTimeHours = table.Column<int>(type: "integer", nullable: false),
                    WarningThresholdPercent = table.Column<int>(type: "integer", nullable: false),
                    CriticalThresholdPercent = table.Column<int>(type: "integer", nullable: false),
                    UseBusinessHoursOnly = table.Column<bool>(type: "boolean", nullable: false),
                    BusinessHoursJson = table.Column<string>(type: "text", nullable: false),
                    ExcludedDaysJson = table.Column<string>(type: "text", nullable: false),
                    ExcludeHolidays = table.Column<bool>(type: "boolean", nullable: false),
                    WorkflowCategory = table.Column<string>(type: "text", nullable: false),
                    TaskPriority = table.Column<string>(type: "text", nullable: false),
                    TaskType = table.Column<string>(type: "text", nullable: false),
                    ApplicableRolesJson = table.Column<string>(type: "text", nullable: false),
                    OnWarningAction = table.Column<string>(type: "text", nullable: false),
                    OnBreachAction = table.Column<string>(type: "text", nullable: false),
                    BreachEscalationRuleCode = table.Column<string>(type: "text", nullable: false),
                    AutoExtendOnHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    TrackFirstResponseTime = table.Column<bool>(type: "boolean", nullable: false),
                    TrackResolutionTime = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeInReporting = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TriggerRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TriggerEvent = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EventConditionJson = table.Column<string>(type: "text", nullable: false),
                    ConditionType = table.Column<string>(type: "text", nullable: false),
                    ConditionExpression = table.Column<string>(type: "text", nullable: false),
                    AgentPrompt = table.Column<string>(type: "text", nullable: false),
                    RequiresAgentEvaluation = table.Column<bool>(type: "boolean", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    ActionConfigJson = table.Column<string>(type: "text", nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "text", nullable: false),
                    NotificationTemplateCode = table.Column<string>(type: "text", nullable: false),
                    WebhookUrl = table.Column<string>(type: "text", nullable: false),
                    CronExpression = table.Column<string>(type: "text", nullable: false),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RunOnce = table.Column<bool>(type: "boolean", nullable: false),
                    MaxExecutionsPerDay = table.Column<int>(type: "integer", nullable: false),
                    CooldownMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsAsync = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    LastSuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    FieldPath = table.Column<string>(type: "text", nullable: false),
                    ValidationType = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MinLength = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    RegexPattern = table.Column<string>(type: "text", nullable: false),
                    AllowedValuesJson = table.Column<string>(type: "text", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DateFormat = table.Column<string>(type: "text", nullable: false),
                    MinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowedFileTypes = table.Column<string>(type: "text", nullable: false),
                    MaxFileSizeMB = table.Column<int>(type: "integer", nullable: true),
                    RequireDigitalSignature = table.Column<bool>(type: "boolean", nullable: false),
                    DependentFieldName = table.Column<string>(type: "text", nullable: false),
                    DependentConditionJson = table.Column<string>(type: "text", nullable: false),
                    ExternalApiUrl = table.Column<string>(type: "text", nullable: false),
                    ExternalApiMethod = table.Column<string>(type: "text", nullable: false),
                    ExternalApiHeaders = table.Column<string>(type: "text", nullable: false),
                    ExternalApiTimeoutMs = table.Column<int>(type: "integer", nullable: false),
                    IsQualificationRule = table.Column<bool>(type: "boolean", nullable: false),
                    QualificationLevel = table.Column<string>(type: "text", nullable: false),
                    QualificationScore = table.Column<int>(type: "integer", nullable: false),
                    QualificationCriteria = table.Column<string>(type: "text", nullable: false),
                    ErrorMessageEn = table.Column<string>(type: "text", nullable: false),
                    ErrorMessageAr = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    BlockOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    PassCount = table.Column<int>(type: "integer", nullable: false),
                    FailCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DelegationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegateUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionBy = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelegationLogs_DelegationRules_DelegationRuleId",
                        column: x => x.DelegationRuleId,
                        principalTable: "DelegationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TriggerExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerEvent = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    WasAgentEvaluated = table.Column<bool>(type: "boolean", nullable: false),
                    AgentResponseJson = table.Column<string>(type: "text", nullable: false),
                    AgentConfidenceScore = table.Column<double>(type: "double precision", nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggerExecutionLogs_TriggerRules_TriggerRuleId",
                        column: x => x.TriggerRuleId,
                        principalTable: "TriggerRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    FieldValue = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationResults_ValidationRules_ValidationRuleId",
                        column: x => x.ValidationRuleId,
                        principalTable: "ValidationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRequirements_AssessmentId",
                table: "AssessmentRequirements",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationLogs_DelegationRuleId",
                table: "DelegationLogs",
                column: "DelegationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TriggerExecutionLogs_TriggerRuleId",
                table: "TriggerExecutionLogs",
                column: "TriggerRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationResults_ValidationRuleId",
                table: "ValidationResults",
                column: "ValidationRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentRequirements");

            migrationBuilder.DropTable(
                name: "DataQualityScores");

            migrationBuilder.DropTable(
                name: "DelegationLogs");

            migrationBuilder.DropTable(
                name: "SlaRules");

            migrationBuilder.DropTable(
                name: "TriggerExecutionLogs");

            migrationBuilder.DropTable(
                name: "ValidationResults");

            migrationBuilder.DropTable(
                name: "DelegationRules");

            migrationBuilder.DropTable(
                name: "TriggerRules");

            migrationBuilder.DropTable(
                name: "ValidationRules");

            migrationBuilder.DropColumn(
                name: "CurrentStageIndex",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "FlowDiagramJson",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "MermaidDiagram",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "StagesJson",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "StatusFormat",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "AnnualRevenueRange",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "BankAccountType",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "BranchCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CeoEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CeoName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CfoEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CfoName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CisoEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CisoName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CloudProviders",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CommercialRegistrationNumber",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ComplianceOfficerEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ComplianceOfficerName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CriticalVendorCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DataSubjectCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DpoEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DpoName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ExternalAuditorName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "FiscalYearEnd",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasDataCenterInKSA",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasExternalAuditor",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasThirdPartyDataProcessing",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HeadquartersLocation",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IncorporationDate",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IndustryLicenses",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IsCriticalInfrastructure",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IsPubliclyTraded",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IsRegulatedEntity",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IsSubsidiary",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ItSystemsJson",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalEntityName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalEntityNameAr",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalEntityType",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativeEmail",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativeName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativePhone",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativeTitle",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedBy",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingProgressPercent",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingStartedAt",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingStatus",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OperatingCountries",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "OrganizationStructureJson",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ParentCompanyName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "PrimaryRegulator",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ProcessesPersonalData",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ProcessesSensitiveData",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RegisteredAddress",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RegisteredCity",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RegisteredRegion",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RegulatoryCertifications",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "SecondaryRegulators",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "StockExchange",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "StockSymbol",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "SubsidiaryCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "TaxIdentificationNumber",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ThirdPartyRiskLevel",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "VendorCount",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "EmailTemplateCode",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "EscalateToRoleCode",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "EscalateToUserId",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "HoursOverdueTrigger",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "RuleCode",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "ShouldNotifyManager",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "ShouldNotifyOriginalAssignee",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "SmsTemplateCode",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "TriggerConditionJson",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "FrameworkCode",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "Assessments");
        }
    }
}
