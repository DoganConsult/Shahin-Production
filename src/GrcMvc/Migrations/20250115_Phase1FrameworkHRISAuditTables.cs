using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class Phase1FrameworkHRISAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Framework table
            migrationBuilder.CreateTable(
                name: "Frameworks",
                columns: table => new
                {
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkName = table.Column<string>(type: "text", nullable: false),
                    FrameworkCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    TotalControls = table.Column<int>(type: "integer", nullable: false),
                    LatestVersion = table.Column<string>(type: "text", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeprecatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frameworks", x => x.FrameworkId);
                    table.ForeignKey(
                        name: "FK_Frameworks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Control table
            migrationBuilder.CreateTable(
                name: "Controls",
                columns: table => new
                {
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlCode = table.Column<string>(type: "text", nullable: false),
                    ControlName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Criticality = table.Column<string>(type: "text", nullable: true),
                    TestingFrequency = table.Column<string>(type: "text", nullable: true),
                    MaturityLevel = table.Column<string>(type: "text", nullable: true),
                    RiskIfNotImplemented = table.Column<string>(type: "text", nullable: true),
                    ApplicableSectors = table.Column<string>(type: "text", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerDepartment = table.Column<string>(type: "text", nullable: true),
                    ComplianceStatus = table.Column<string>(type: "text", nullable: true),
                    EffectivenessScore = table.Column<double>(type: "double precision", nullable: true),
                    LastTestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextTestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controls", x => x.ControlId);
                    table.ForeignKey(
                        name: "FK_Controls_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "Frameworks",
                        principalColumn: "FrameworkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Controls_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            // ControlOwnership table
            migrationBuilder.CreateTable(
                name: "ControlOwnerships",
                columns: table => new
                {
                    OwnershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlternateOwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TestingResponsibility = table.Column<string>(type: "text", nullable: true),
                    ApprovalAuthority = table.Column<string>(type: "text", nullable: true),
                    AssignmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlOwnerships", x => x.OwnershipId);
                    table.ForeignKey(
                        name: "FK_ControlOwnerships_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "ControlId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ControlEvidence table
            migrationBuilder.CreateTable(
                name: "ControlEvidences",
                columns: table => new
                {
                    EvidenceRequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AcceptableFormats = table.Column<string>(type: "text", nullable: true),
                    FrequencyDays = table.Column<int>(type: "integer", nullable: false),
                    MaxAgeDays = table.Column<int>(type: "integer", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlEvidences", x => x.EvidenceRequirementId);
                    table.ForeignKey(
                        name: "FK_ControlEvidences_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "ControlId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Baseline table
            migrationBuilder.CreateTable(
                name: "Baselines",
                columns: table => new
                {
                    BaselineId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineName = table.Column<string>(type: "text", nullable: false),
                    Sector = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TotalControls = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baselines", x => x.BaselineId);
                    table.ForeignKey(
                        name: "FK_Baselines_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "Frameworks",
                        principalColumn: "FrameworkId",
                        onDelete: ReferentialAction.Cascade);
                });

            // BaselineControl table
            migrationBuilder.CreateTable(
                name: "BaselineControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaselineControls_Baselines_BaselineId",
                        column: x => x.BaselineId,
                        principalTable: "Baselines",
                        principalColumn: "BaselineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaselineControls_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "ControlId",
                        onDelete: ReferentialAction.Cascade);
                });

            // HRIS Integration table
            migrationBuilder.CreateTable(
                name: "HRISIntegrations",
                columns: table => new
                {
                    IntegrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "text", nullable: false),
                    APIEndpoint = table.Column<string>(type: "text", nullable: true),
                    AuthType = table.Column<string>(type: "text", nullable: true),
                    EncryptedCredentials = table.Column<string>(type: "text", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextSyncDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyncStatus = table.Column<string>(type: "text", nullable: true),
                    LastSyncError = table.Column<string>(type: "text", nullable: true),
                    SyncIntervalHours = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRISIntegrations", x => x.IntegrationId);
                    table.ForeignKey(
                        name: "FK_HRISIntegrations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            // HRIS Employee table
            migrationBuilder.CreateTable(
                name: "HRISEmployees",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    HRISEmployeeId = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    ReportsToEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyncedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRISEmployees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_HRISEmployees_HRISIntegrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "HRISIntegrations",
                        principalColumn: "IntegrationId",
                        onDelete: ReferentialAction.Cascade);
                });

            // AuditLog table
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogId);
                });

            // ComplianceSnapshot table
            migrationBuilder.CreateTable(
                name: "ComplianceSnapshots",
                columns: table => new
                {
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompliancePercentage = table.Column<double>(type: "double precision", nullable: false),
                    TotalControls = table.Column<int>(type: "integer", nullable: false),
                    ImplementedControls = table.Column<int>(type: "integer", nullable: false),
                    InProgressControls = table.Column<int>(type: "integer", nullable: false),
                    PlannedControls = table.Column<int>(type: "integer", nullable: false),
                    AverageEffectivenessScore = table.Column<double>(type: "double precision", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceSnapshots", x => x.SnapshotId);
                    table.ForeignKey(
                        name: "FK_ComplianceSnapshots_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "Frameworks",
                        principalColumn: "FrameworkId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ControlTestResult table
            migrationBuilder.CreateTable(
                name: "ControlTestResults",
                columns: table => new
                {
                    TestResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TestResult = table.Column<string>(type: "text", nullable: false),
                    TestMethod = table.Column<string>(type: "text", nullable: true),
                    Findings = table.Column<string>(type: "text", nullable: true),
                    EffectivenessScore = table.Column<double>(type: "double precision", nullable: false),
                    Evidence = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlTestResults", x => x.TestResultId);
                    table.ForeignKey(
                        name: "FK_ControlTestResults_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "ControlId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Frameworks_TenantId",
                table: "Frameworks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_FrameworkId",
                table: "Controls",
                column: "FrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_TenantId",
                table: "Controls",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerships_TenantId",
                table: "ControlOwnerships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerships_OwnerId",
                table: "ControlOwnerships",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_Action",
                table: "AuditLogs",
                columns: new[] { "EntityType", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_HRISIntegrations_TenantId",
                table: "HRISIntegrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HRISEmployees_TenantId",
                table: "HRISEmployees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HRISEmployees_IntegrationId",
                table: "HRISEmployees",
                column: "IntegrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "BaselineControls");
            migrationBuilder.DropTable(name: "ComplianceSnapshots");
            migrationBuilder.DropTable(name: "ControlEvidences");
            migrationBuilder.DropTable(name: "ControlOwnerships");
            migrationBuilder.DropTable(name: "ControlTestResults");
            migrationBuilder.DropTable(name: "HRISEmployees");
            migrationBuilder.DropTable(name: "Baselines");
            migrationBuilder.DropTable(name: "Controls");
            migrationBuilder.DropTable(name: "HRISIntegrations");
            migrationBuilder.DropTable(name: "Frameworks");
        }
    }
}
