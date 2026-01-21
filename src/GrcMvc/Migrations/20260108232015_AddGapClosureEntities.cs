using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddGapClosureEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuingBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuingBodyAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Scope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRenewalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextSurveillanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRecertificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RenewalLeadDays = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: true),
                    StandardVersion = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AuditorName = table.Column<string>(type: "text", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric", nullable: true),
                    CostCurrency = table.Column<string>(type: "text", nullable: false),
                    CertificateUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    LinkedFrameworkCode = table.Column<string>(type: "text", nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    MandatorySource = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlOwnerAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedById = table.Column<string>(type: "text", nullable: true),
                    AssignedByName = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ControlOwnerAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlOwnerAssignments_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TestMethodology = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SampleSize = table.Column<int>(type: "integer", nullable: true),
                    PopulationSize = table.Column<int>(type: "integer", nullable: true),
                    ExceptionsFound = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Findings = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TestNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TesterId = table.Column<string>(type: "text", nullable: true),
                    TesterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewerId = table.Column<string>(type: "text", nullable: true),
                    ReviewerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousEffectiveness = table.Column<int>(type: "integer", nullable: false),
                    NewEffectiveness = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvidenceIds = table.Column<string>(type: "text", nullable: true),
                    NextTestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ControlTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlTests_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phase = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DetectionSource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContainedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EradicatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecoveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportedById = table.Column<string>(type: "text", nullable: true),
                    ReportedByName = table.Column<string>(type: "text", nullable: true),
                    HandlerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HandlerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedTeam = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AffectedSystems = table.Column<string>(type: "text", nullable: true),
                    AffectedBusinessUnits = table.Column<string>(type: "text", nullable: true),
                    AffectedUsersCount = table.Column<int>(type: "integer", nullable: true),
                    AffectedRecordsCount = table.Column<int>(type: "integer", nullable: true),
                    PersonalDataAffected = table.Column<bool>(type: "boolean", nullable: false),
                    RootCause = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ContainmentActions = table.Column<string>(type: "text", nullable: true),
                    EradicationActions = table.Column<string>(type: "text", nullable: true),
                    RecoveryActions = table.Column<string>(type: "text", nullable: true),
                    LessonsLearned = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    RequiresNotification = table.Column<bool>(type: "boolean", nullable: false),
                    RegulatorsToNotify = table.Column<string>(type: "text", nullable: true),
                    NotificationSent = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotificationSentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedImpact = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualImpact = table.Column<decimal>(type: "numeric", nullable: true),
                    ImpactCurrency = table.Column<string>(type: "text", nullable: false),
                    RelatedRiskIds = table.Column<string>(type: "text", nullable: true),
                    RelatedControlIds = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificationAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeadAuditorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MajorFindings = table.Column<int>(type: "integer", nullable: false),
                    MinorFindings = table.Column<int>(type: "integer", nullable: false),
                    Observations = table.Column<int>(type: "integer", nullable: false),
                    CorrectiveActionDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrectiveActionsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReportReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NextAuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_CertificationAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificationAudits_Certifications_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Phase = table.Column<string>(type: "text", nullable: true),
                    StatusBefore = table.Column<string>(type: "text", nullable: true),
                    StatusAfter = table.Column<string>(type: "text", nullable: true),
                    PerformedById = table.Column<string>(type: "text", nullable: true),
                    PerformedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    Attachments = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_IncidentTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentTimelineEntries_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAudits_AuditDate",
                table: "CertificationAudits",
                column: "AuditDate");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAudits_CertificationId",
                table: "CertificationAudits",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAudits_Result",
                table: "CertificationAudits",
                column: "Result");

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
                name: "IX_ControlOwnerAssignments_ControlId",
                table: "ControlOwnerAssignments",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerAssignments_ControlId_IsActive",
                table: "ControlOwnerAssignments",
                columns: new[] { "ControlId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlOwnerAssignments_OwnerId",
                table: "ControlOwnerAssignments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlTests_ControlId",
                table: "ControlTests",
                column: "ControlId");

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
                name: "IX_IncidentTimelineEntries_IncidentId",
                table: "IncidentTimelineEntries",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTimelineEntries_Timestamp",
                table: "IncidentTimelineEntries",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificationAudits");

            migrationBuilder.DropTable(
                name: "ControlOwnerAssignments");

            migrationBuilder.DropTable(
                name: "ControlTests");

            migrationBuilder.DropTable(
                name: "IncidentTimelineEntries");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Incidents");
        }
    }
}
