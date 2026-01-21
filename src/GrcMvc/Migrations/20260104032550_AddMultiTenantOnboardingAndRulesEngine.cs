using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantOnboardingAndRulesEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "Assessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlanPhaseId",
                table: "Assessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AdminEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActivationToken = table.Column<string>(type: "text", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedBy = table.Column<string>(type: "text", nullable: false),
                    SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AffectedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AffectedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Actor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DataTypes = table.Column<string>(type: "text", nullable: false),
                    HostingModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationSize = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComplianceMaturity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Vendors = table.Column<string>(type: "text", nullable: false),
                    OnboardingQuestionsJson = table.Column<string>(type: "text", nullable: false),
                    LastScopeDerivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationProfiles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RulesetVersion = table.Column<int>(type: "integer", nullable: false),
                    ScopeSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rulesets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulesetCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeNotes = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rulesets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rulesets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantBaselines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaselineName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Applicability = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DerivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReasonJson = table.Column<string>(type: "text", nullable: false),
                    RuleExecutionLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBaselines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantBaselines_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PackageName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Applicability = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DerivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReasonJson = table.Column<string>(type: "text", nullable: false),
                    RuleExecutionLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPackages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Applicability = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DerivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReasonJson = table.Column<string>(type: "text", nullable: false),
                    RuleExecutionLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantTemplates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvitationToken = table.Column<string>(type: "text", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanPhases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanPhases_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RulesetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedBy = table.Column<string>(type: "text", nullable: false),
                    MatchedRulesJson = table.Column<string>(type: "text", nullable: false),
                    OrgProfileSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    DerivedScopeJson = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleExecutionLogs_Rulesets_RulesetId",
                        column: x => x.RulesetId,
                        principalTable: "Rulesets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RulesetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ConditionJson = table.Column<string>(type: "text", nullable: false),
                    ActionsJson = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BusinessReason = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_Rulesets_RulesetId",
                        column: x => x.RulesetId,
                        principalTable: "Rulesets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_PlanId",
                table: "Assessments",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_PlanPhaseId",
                table: "Assessments",
                column: "PlanPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CorrelationId",
                table: "AuditEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EventId",
                table: "AuditEvents",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_EventTimestamp",
                table: "AuditEvents",
                columns: new[] { "TenantId", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_TenantId",
                table: "OrganizationProfiles",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanPhases_PlanId_Sequence",
                table: "PlanPhases",
                columns: new[] { "PlanId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_CorrelationId",
                table: "Plans",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_TenantId_PlanCode",
                table: "Plans",
                columns: new[] { "TenantId", "PlanCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionLogs_RulesetId_TenantId_ExecutedAt",
                table: "RuleExecutionLogs",
                columns: new[] { "RulesetId", "TenantId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Rules_RulesetId_Priority",
                table: "Rules",
                columns: new[] { "RulesetId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Rulesets_TenantId_RulesetCode",
                table: "Rulesets",
                columns: new[] { "TenantId", "RulesetCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantBaselines_TenantId_BaselineCode",
                table: "TenantBaselines",
                columns: new[] { "TenantId", "BaselineCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantPackages_TenantId_PackageCode",
                table: "TenantPackages",
                columns: new[] { "TenantId", "PackageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AdminEmail",
                table: "Tenants",
                column: "AdminEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TenantSlug",
                table: "Tenants",
                column: "TenantSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantTemplates_TenantId_TemplateCode",
                table: "TenantTemplates",
                columns: new[] { "TenantId", "TemplateCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_TenantId_UserId",
                table: "TenantUsers",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_UserId",
                table: "TenantUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_PlanPhases_PlanPhaseId",
                table: "Assessments",
                column: "PlanPhaseId",
                principalTable: "PlanPhases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Plans_PlanId",
                table: "Assessments",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_PlanPhases_PlanPhaseId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Plans_PlanId",
                table: "Assessments");

            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "OrganizationProfiles");

            migrationBuilder.DropTable(
                name: "PlanPhases");

            migrationBuilder.DropTable(
                name: "RuleExecutionLogs");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "TenantBaselines");

            migrationBuilder.DropTable(
                name: "TenantPackages");

            migrationBuilder.DropTable(
                name: "TenantTemplates");

            migrationBuilder.DropTable(
                name: "TenantUsers");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Rulesets");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_PlanId",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_PlanPhaseId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "PlanPhaseId",
                table: "Assessments");
        }
    }
}
