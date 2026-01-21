using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingOnboardingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingWizards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationLegalNameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OrganizationLegalNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CountryOfIncorporation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OperatingCountriesJson = table.Column<string>(type: "text", nullable: false),
                    PrimaryHqLocation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DefaultTimezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CorporateEmailDomainsJson = table.Column<string>(type: "text", nullable: false),
                    DomainVerificationMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrganizationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndustrySector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BusinessLinesJson = table.Column<string>(type: "text", nullable: false),
                    HasDataResidencyRequirement = table.Column<bool>(type: "boolean", nullable: false),
                    DataResidencyCountriesJson = table.Column<string>(type: "text", nullable: false),
                    PrimaryDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetTimeline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPainPointsJson = table.Column<string>(type: "text", nullable: false),
                    DesiredMaturity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportingAudienceJson = table.Column<string>(type: "text", nullable: false),
                    PrimaryRegulatorsJson = table.Column<string>(type: "text", nullable: false),
                    SecondaryRegulatorsJson = table.Column<string>(type: "text", nullable: false),
                    MandatoryFrameworksJson = table.Column<string>(type: "text", nullable: false),
                    OptionalFrameworksJson = table.Column<string>(type: "text", nullable: false),
                    InternalPoliciesJson = table.Column<string>(type: "text", nullable: false),
                    CertificationsHeldJson = table.Column<string>(type: "text", nullable: false),
                    AuditScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InScopeLegalEntitiesJson = table.Column<string>(type: "text", nullable: false),
                    InScopeBusinessUnitsJson = table.Column<string>(type: "text", nullable: false),
                    InScopeSystemsJson = table.Column<string>(type: "text", nullable: false),
                    InScopeProcessesJson = table.Column<string>(type: "text", nullable: false),
                    InScopeEnvironments = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InScopeLocationsJson = table.Column<string>(type: "text", nullable: false),
                    SystemCriticalityTiersJson = table.Column<string>(type: "text", nullable: false),
                    ImportantBusinessServicesJson = table.Column<string>(type: "text", nullable: false),
                    ExclusionsJson = table.Column<string>(type: "text", nullable: false),
                    DataTypesProcessedJson = table.Column<string>(type: "text", nullable: false),
                    HasPaymentCardData = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentCardDataLocationsJson = table.Column<string>(type: "text", nullable: false),
                    HasCrossBorderDataTransfers = table.Column<bool>(type: "boolean", nullable: false),
                    CrossBorderTransferCountriesJson = table.Column<string>(type: "text", nullable: false),
                    CustomerVolumeTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionVolumeTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HasInternetFacingSystems = table.Column<bool>(type: "boolean", nullable: false),
                    InternetFacingSystemsJson = table.Column<string>(type: "text", nullable: false),
                    HasThirdPartyDataProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    ThirdPartyDataProcessorsJson = table.Column<string>(type: "text", nullable: false),
                    IdentityProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SsoEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScimProvisioningAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ItsmPlatform = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvidenceRepository = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SiemPlatform = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VulnerabilityManagementTool = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EdrPlatform = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CloudProvidersJson = table.Column<string>(type: "text", nullable: false),
                    ErpSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CmdbSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CiCdTooling = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BackupDrTooling = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ControlOwnershipApproach = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultControlOwnerTeam = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExceptionApproverRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegulatoryInterpretationApproverRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ControlEffectivenessSignoffRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InternalAuditStakeholder = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RiskCommitteeCadence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskCommitteeAttendeesJson = table.Column<string>(type: "text", nullable: false),
                    OrgAdminsJson = table.Column<string>(type: "text", nullable: false),
                    CreateTeamsNow = table.Column<bool>(type: "boolean", nullable: false),
                    TeamListJson = table.Column<string>(type: "text", nullable: false),
                    TeamMembersJson = table.Column<string>(type: "text", nullable: false),
                    SelectedRoleCatalogJson = table.Column<string>(type: "text", nullable: false),
                    RaciMappingNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    RaciMappingJson = table.Column<string>(type: "text", nullable: false),
                    ApprovalGatesNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalGatesJson = table.Column<string>(type: "text", nullable: false),
                    DelegationRulesJson = table.Column<string>(type: "text", nullable: false),
                    NotificationPreference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EscalationDaysOverdue = table.Column<int>(type: "integer", nullable: false),
                    EscalationTarget = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvidenceFrequencyDefaultsJson = table.Column<string>(type: "text", nullable: false),
                    AccessReviewsFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VulnerabilityPatchReviewFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BackupReviewFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RestoreTestCadence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrExerciseCadence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IncidentTabletopCadence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceSlaSubmitDays = table.Column<int>(type: "integer", nullable: false),
                    RemediationSlaJson = table.Column<string>(type: "text", nullable: false),
                    ExceptionExpiryDays = table.Column<int>(type: "integer", nullable: false),
                    AuditRequestHandling = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceNamingConventionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    EvidenceNamingPattern = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EvidenceStorageLocationJson = table.Column<string>(type: "text", nullable: false),
                    EvidenceRetentionYears = table.Column<int>(type: "integer", nullable: false),
                    EvidenceAccessRulesJson = table.Column<string>(type: "text", nullable: false),
                    AcceptableEvidenceTypesJson = table.Column<string>(type: "text", nullable: false),
                    SamplingGuidanceJson = table.Column<string>(type: "text", nullable: false),
                    ConfidentialEvidenceEncryption = table.Column<bool>(type: "boolean", nullable: false),
                    ConfidentialEvidenceAccessJson = table.Column<string>(type: "text", nullable: false),
                    AdoptDefaultBaseline = table.Column<bool>(type: "boolean", nullable: false),
                    SelectedOverlaysJson = table.Column<string>(type: "text", nullable: false),
                    HasClientSpecificControls = table.Column<bool>(type: "boolean", nullable: false),
                    ClientSpecificControlsJson = table.Column<string>(type: "text", nullable: false),
                    SuccessMetricsTop3Json = table.Column<string>(type: "text", nullable: false),
                    BaselineAuditPrepHoursPerMonth = table.Column<decimal>(type: "numeric", nullable: true),
                    BaselineRemediationClosureDays = table.Column<decimal>(type: "numeric", nullable: true),
                    BaselineOverdueControlsPerMonth = table.Column<int>(type: "integer", nullable: true),
                    TargetImprovementJson = table.Column<string>(type: "text", nullable: false),
                    PilotScopeJson = table.Column<string>(type: "text", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    WizardStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    CompletedSectionsJson = table.Column<string>(type: "text", nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastStepSavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllAnswersJson = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingWizards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingWizards_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NameAr = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TeamType = table.Column<string>(type: "text", nullable: false),
                    BusinessUnit = table.Column<string>(type: "text", nullable: false),
                    ManagerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDefaultFallback = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AssetType = table.Column<string>(type: "text", nullable: false),
                    SubType = table.Column<string>(type: "text", nullable: false),
                    SystemId = table.Column<string>(type: "text", nullable: false),
                    SourceSystem = table.Column<string>(type: "text", nullable: false),
                    Criticality = table.Column<string>(type: "text", nullable: false),
                    DataClassification = table.Column<string>(type: "text", nullable: false),
                    DataTypes = table.Column<string>(type: "text", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessOwner = table.Column<string>(type: "text", nullable: false),
                    TechnicalOwner = table.Column<string>(type: "text", nullable: false),
                    HostingModel = table.Column<string>(type: "text", nullable: false),
                    CloudProvider = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    TagsJson = table.Column<string>(type: "text", nullable: false),
                    AttributesJson = table.Column<string>(type: "text", nullable: false),
                    IsInScope = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicableFrameworks = table.Column<string>(type: "text", nullable: false),
                    LastRiskAssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskScore = table.Column<int>(type: "integer", nullable: true),
                    CommissionedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecommissionedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RACIAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "text", nullable: false),
                    ScopeId = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    RACI = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RACIAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RACIAssignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    IsPrimaryForRole = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelegate = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_TenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_OwnerTeamId",
                table: "Assets",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId",
                table: "Assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingWizards_CurrentStep",
                table: "OnboardingWizards",
                column: "CurrentStep");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingWizards_TenantId",
                table: "OnboardingWizards",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingWizards_WizardStatus",
                table: "OnboardingWizards",
                column: "WizardStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RACIAssignments_TeamId",
                table: "RACIAssignments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId",
                table: "TeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TenantId",
                table: "Teams",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "OnboardingWizards");

            migrationBuilder.DropTable(
                name: "RACIAssignments");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
