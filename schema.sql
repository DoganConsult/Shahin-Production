CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetRoles" (
        "Id" text NOT NULL,
        "Name" character varying(256),
        "NormalizedName" character varying(256),
        "ConcurrencyStamp" text,
        CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetUsers" (
        "Id" text NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "Department" character varying(100) NOT NULL,
        "JobTitle" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "LastLoginDate" timestamp with time zone,
        "RefreshToken" text,
        "RefreshTokenExpiry" timestamp with time zone,
        "UserName" character varying(256),
        "NormalizedUserName" character varying(256),
        "Email" character varying(256),
        "NormalizedEmail" character varying(256),
        "EmailConfirmed" boolean NOT NULL,
        "PasswordHash" text,
        "SecurityStamp" text,
        "ConcurrencyStamp" text,
        "PhoneNumber" text,
        "PhoneNumberConfirmed" boolean NOT NULL,
        "TwoFactorEnabled" boolean NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "LockoutEnabled" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Audits" (
        "Id" uuid NOT NULL,
        "AuditNumber" character varying(50) NOT NULL,
        "AuditCode" text NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Scope" text NOT NULL,
        "Objectives" text NOT NULL,
        "PlannedStartDate" timestamp with time zone NOT NULL,
        "PlannedEndDate" timestamp with time zone NOT NULL,
        "ActualStartDate" timestamp with time zone,
        "ActualEndDate" timestamp with time zone,
        "Status" text NOT NULL,
        "LeadAuditor" text NOT NULL,
        "AuditTeam" text NOT NULL,
        "RiskRating" text NOT NULL,
        "ExecutiveSummary" text NOT NULL,
        "KeyFindings" text NOT NULL,
        "ManagementResponse" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Audits" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Policies" (
        "Id" uuid NOT NULL,
        "PolicyNumber" character varying(50) NOT NULL,
        "PolicyCode" text NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Version" character varying(20) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "Status" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Owner" text NOT NULL,
        "ApprovedBy" text NOT NULL,
        "ApprovalDate" timestamp with time zone,
        "NextReviewDate" timestamp with time zone NOT NULL,
        "Content" text NOT NULL,
        "Requirements" text NOT NULL,
        "DocumentPath" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Policies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Risks" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Likelihood" integer NOT NULL,
        "Impact" integer NOT NULL,
        "InherentRisk" integer NOT NULL,
        "ResidualRisk" integer NOT NULL,
        "Status" text NOT NULL,
        "Owner" character varying(100) NOT NULL,
        "ReviewDate" timestamp with time zone,
        "DueDate" timestamp with time zone,
        "MitigationStrategy" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Risks" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Workflows" (
        "Id" uuid NOT NULL,
        "WorkflowNumber" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Type" character varying(50) NOT NULL,
        "EntityType" text NOT NULL,
        "Category" character varying(100) NOT NULL,
        "TriggerType" text NOT NULL,
        "Status" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Definition" text NOT NULL,
        "Steps" text NOT NULL,
        "Version" integer NOT NULL,
        "IsTemplate" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Workflows" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetRoleClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" text NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetUserClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "UserId" text NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetUserLogins" (
        "LoginProvider" text NOT NULL,
        "ProviderKey" text NOT NULL,
        "ProviderDisplayName" text,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
        CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetUserRoles" (
        "UserId" text NOT NULL,
        "RoleId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AspNetUserTokens" (
        "UserId" text NOT NULL,
        "LoginProvider" text NOT NULL,
        "Name" text NOT NULL,
        "Value" text,
        CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "AuditFindings" (
        "Id" uuid NOT NULL,
        "FindingNumber" character varying(50) NOT NULL,
        "FindingCode" text NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Severity" character varying(20) NOT NULL,
        "Category" text NOT NULL,
        "RootCause" text NOT NULL,
        "Impact" text NOT NULL,
        "Recommendation" text NOT NULL,
        "ManagementResponse" text NOT NULL,
        "ResponsibleParty" text NOT NULL,
        "TargetDate" timestamp with time zone,
        "Status" text NOT NULL,
        "AuditId" uuid NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AuditFindings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditFindings_Audits_AuditId" FOREIGN KEY ("AuditId") REFERENCES "Audits" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "PolicyViolations" (
        "Id" uuid NOT NULL,
        "ViolationNumber" character varying(50) NOT NULL,
        "ViolationCode" text NOT NULL,
        "Description" text NOT NULL,
        "ViolationDate" timestamp with time zone NOT NULL,
        "Severity" character varying(20) NOT NULL,
        "DetectedBy" text NOT NULL,
        "ViolatorName" text NOT NULL,
        "Department" text NOT NULL,
        "Resolution" text NOT NULL,
        "Status" text NOT NULL,
        "ResolutionDate" timestamp with time zone,
        "PolicyId" uuid NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PolicyViolations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PolicyViolations_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Controls" (
        "Id" uuid NOT NULL,
        "ControlId" character varying(50) NOT NULL,
        "ControlCode" text NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Frequency" character varying(50) NOT NULL,
        "Owner" text NOT NULL,
        "Status" text NOT NULL,
        "EffectivenessScore" integer NOT NULL,
        "Effectiveness" integer NOT NULL,
        "LastTestDate" timestamp with time zone,
        "NextTestDate" timestamp with time zone,
        "RiskId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Controls" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Controls_Risks_RiskId" FOREIGN KEY ("RiskId") REFERENCES "Risks" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "WorkflowExecutions" (
        "Id" uuid NOT NULL,
        "ExecutionNumber" character varying(50) NOT NULL,
        "StartTime" timestamp with time zone NOT NULL,
        "EndTime" timestamp with time zone,
        "Status" character varying(50) NOT NULL,
        "CurrentStep" text NOT NULL,
        "InitiatedBy" text NOT NULL,
        "TriggeredBy" text NOT NULL,
        "Context" text NOT NULL,
        "Result" text NOT NULL,
        "ExecutionHistory" text NOT NULL,
        "ErrorMessage" text NOT NULL,
        "WorkflowId" uuid NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowExecutions_Workflows_WorkflowId" FOREIGN KEY ("WorkflowId") REFERENCES "Workflows" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Assessments" (
        "Id" uuid NOT NULL,
        "AssessmentNumber" character varying(50) NOT NULL,
        "AssessmentCode" text NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "ScheduledDate" timestamp with time zone,
        "EndDate" timestamp with time zone,
        "Status" text NOT NULL,
        "AssignedTo" text NOT NULL,
        "ReviewedBy" text NOT NULL,
        "ComplianceScore" integer,
        "Score" integer NOT NULL,
        "Findings" text NOT NULL,
        "Recommendations" text NOT NULL,
        "RiskId" uuid,
        "ControlId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Assessments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Assessments_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Assessments_Risks_RiskId" FOREIGN KEY ("RiskId") REFERENCES "Risks" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE TABLE "Evidences" (
        "Id" uuid NOT NULL,
        "EvidenceNumber" character varying(50) NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" text NOT NULL,
        "Type" text NOT NULL,
        "FilePath" text NOT NULL,
        "FileName" character varying(255) NOT NULL,
        "FileSize" bigint NOT NULL,
        "MimeType" text NOT NULL,
        "CollectionDate" timestamp with time zone NOT NULL,
        "CollectedBy" text NOT NULL,
        "VerificationStatus" text NOT NULL,
        "VerifiedBy" text NOT NULL,
        "VerificationDate" timestamp with time zone,
        "Comments" text NOT NULL,
        "AssessmentId" uuid,
        "AuditId" uuid,
        "ControlId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Evidences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Evidences_Assessments_AssessmentId" FOREIGN KEY ("AssessmentId") REFERENCES "Assessments" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Evidences_Audits_AuditId" FOREIGN KEY ("AuditId") REFERENCES "Audits" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Evidences_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Assessments_AssessmentNumber" ON "Assessments" ("AssessmentNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Assessments_ControlId" ON "Assessments" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Assessments_RiskId" ON "Assessments" ("RiskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_AuditFindings_AuditId" ON "AuditFindings" ("AuditId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_AuditFindings_FindingNumber" ON "AuditFindings" ("FindingNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Audits_AuditNumber" ON "Audits" ("AuditNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Controls_ControlId" ON "Controls" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Controls_RiskId" ON "Controls" ("RiskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Evidences_AssessmentId" ON "Evidences" ("AssessmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Evidences_AuditId" ON "Evidences" ("AuditId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Evidences_ControlId" ON "Evidences" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Evidences_EvidenceNumber" ON "Evidences" ("EvidenceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Policies_PolicyNumber" ON "Policies" ("PolicyNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_PolicyViolations_PolicyId" ON "PolicyViolations" ("PolicyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_PolicyViolations_ViolationNumber" ON "PolicyViolations" ("ViolationNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_Risks_Name" ON "Risks" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_WorkflowExecutions_ExecutionNumber" ON "WorkflowExecutions" ("ExecutionNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE INDEX "IX_WorkflowExecutions_WorkflowId" ON "WorkflowExecutions" ("WorkflowId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Workflows_WorkflowNumber" ON "Workflows" ("WorkflowNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260103214843_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260103214843_InitialCreate', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    ALTER TABLE "Assessments" ADD "PlanId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    ALTER TABLE "Assessments" ADD "PlanPhaseId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "Tenants" (
        "Id" uuid NOT NULL,
        "TenantSlug" character varying(100) NOT NULL,
        "OrganizationName" character varying(255) NOT NULL,
        "AdminEmail" character varying(255) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "ActivationToken" text NOT NULL,
        "ActivatedAt" timestamp with time zone,
        "ActivatedBy" text NOT NULL,
        "SubscriptionStartDate" timestamp with time zone NOT NULL,
        "SubscriptionEndDate" timestamp with time zone,
        "SubscriptionTier" character varying(50) NOT NULL,
        "CorrelationId" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "AuditEvents" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EventId" character varying(100) NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "CorrelationId" character varying(100) NOT NULL,
        "AffectedEntityType" character varying(100) NOT NULL,
        "AffectedEntityId" character varying(100) NOT NULL,
        "Actor" character varying(255) NOT NULL,
        "Action" character varying(100) NOT NULL,
        "PayloadJson" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "ErrorMessage" text NOT NULL,
        "EventTimestamp" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AuditEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditEvents_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "OrganizationProfiles" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "OrganizationType" character varying(100) NOT NULL,
        "Sector" character varying(100) NOT NULL,
        "Country" character varying(10) NOT NULL,
        "DataTypes" text NOT NULL,
        "HostingModel" character varying(100) NOT NULL,
        "OrganizationSize" character varying(50) NOT NULL,
        "ComplianceMaturity" character varying(50) NOT NULL,
        "Vendors" text NOT NULL,
        "OnboardingQuestionsJson" text NOT NULL,
        "LastScopeDerivedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OrganizationProfiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganizationProfiles_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "Plans" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PlanCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "PlanType" character varying(50) NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "TargetEndDate" timestamp with time zone NOT NULL,
        "ActualEndDate" timestamp with time zone,
        "RulesetVersion" integer NOT NULL,
        "ScopeSnapshotJson" text NOT NULL,
        "CorrelationId" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Plans" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Plans_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "Rulesets" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "RulesetCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "Version" integer NOT NULL,
        "Status" character varying(50) NOT NULL,
        "ActivatedAt" timestamp with time zone,
        "PreviousVersionId" uuid,
        "ChangeNotes" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Rulesets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Rulesets_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "TenantBaselines" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "BaselineCode" character varying(100) NOT NULL,
        "BaselineName" character varying(255) NOT NULL,
        "Applicability" character varying(50) NOT NULL,
        "DerivedAt" timestamp with time zone NOT NULL,
        "ReasonJson" text NOT NULL,
        "RuleExecutionLogId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TenantBaselines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantBaselines_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "TenantPackages" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PackageCode" character varying(100) NOT NULL,
        "PackageName" character varying(255) NOT NULL,
        "Applicability" character varying(50) NOT NULL,
        "DerivedAt" timestamp with time zone NOT NULL,
        "ReasonJson" text NOT NULL,
        "RuleExecutionLogId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TenantPackages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantPackages_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "TenantTemplates" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TemplateCode" character varying(100) NOT NULL,
        "TemplateName" character varying(255) NOT NULL,
        "Applicability" character varying(50) NOT NULL,
        "DerivedAt" timestamp with time zone NOT NULL,
        "ReasonJson" text NOT NULL,
        "RuleExecutionLogId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TenantTemplates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantTemplates_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "TenantUsers" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "RoleCode" character varying(100) NOT NULL,
        "TitleCode" character varying(100) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "InvitationToken" text NOT NULL,
        "InvitedAt" timestamp with time zone,
        "ActivatedAt" timestamp with time zone,
        "InvitedBy" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TenantUsers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantUsers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TenantUsers_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "PlanPhases" (
        "Id" uuid NOT NULL,
        "PlanId" uuid NOT NULL,
        "PhaseCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "Sequence" integer NOT NULL,
        "Status" character varying(50) NOT NULL,
        "PlannedStartDate" timestamp with time zone NOT NULL,
        "PlannedEndDate" timestamp with time zone NOT NULL,
        "ActualStartDate" timestamp with time zone,
        "ActualEndDate" timestamp with time zone,
        "Owner" text NOT NULL,
        "ProgressPercentage" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PlanPhases" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlanPhases_Plans_PlanId" FOREIGN KEY ("PlanId") REFERENCES "Plans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "RuleExecutionLogs" (
        "Id" uuid NOT NULL,
        "RulesetId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ExecutedAt" timestamp with time zone NOT NULL,
        "ExecutedBy" text NOT NULL,
        "MatchedRulesJson" text NOT NULL,
        "OrgProfileSnapshotJson" text NOT NULL,
        "DerivedScopeJson" text NOT NULL,
        "CorrelationId" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "ErrorMessage" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RuleExecutionLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RuleExecutionLogs_Rulesets_RulesetId" FOREIGN KEY ("RulesetId") REFERENCES "Rulesets" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE TABLE "Rules" (
        "Id" uuid NOT NULL,
        "RulesetId" uuid NOT NULL,
        "RuleCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "ConditionJson" text NOT NULL,
        "ActionsJson" text NOT NULL,
        "Priority" integer NOT NULL,
        "Status" character varying(50) NOT NULL,
        "BusinessReason" text NOT NULL,
        "LastModifiedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Rules" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Rules_Rulesets_RulesetId" FOREIGN KEY ("RulesetId") REFERENCES "Rulesets" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Assessments_PlanId" ON "Assessments" ("PlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Assessments_PlanPhaseId" ON "Assessments" ("PlanPhaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_AuditEvents_CorrelationId" ON "AuditEvents" ("CorrelationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_AuditEvents_EventId" ON "AuditEvents" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_AuditEvents_TenantId_EventTimestamp" ON "AuditEvents" ("TenantId", "EventTimestamp");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_OrganizationProfiles_TenantId" ON "OrganizationProfiles" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_PlanPhases_PlanId_Sequence" ON "PlanPhases" ("PlanId", "Sequence");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Plans_CorrelationId" ON "Plans" ("CorrelationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_Plans_TenantId_PlanCode" ON "Plans" ("TenantId", "PlanCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_RuleExecutionLogs_RulesetId_TenantId_ExecutedAt" ON "RuleExecutionLogs" ("RulesetId", "TenantId", "ExecutedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Rules_RulesetId_Priority" ON "Rules" ("RulesetId", "Priority");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Rulesets_TenantId_RulesetCode" ON "Rulesets" ("TenantId", "RulesetCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_TenantBaselines_TenantId_BaselineCode" ON "TenantBaselines" ("TenantId", "BaselineCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_TenantPackages_TenantId_PackageCode" ON "TenantPackages" ("TenantId", "PackageCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_Tenants_AdminEmail" ON "Tenants" ("AdminEmail");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_Tenants_TenantSlug" ON "Tenants" ("TenantSlug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_TenantTemplates_TenantId_TemplateCode" ON "TenantTemplates" ("TenantId", "TemplateCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE UNIQUE INDEX "IX_TenantUsers_TenantId_UserId" ON "TenantUsers" ("TenantId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    CREATE INDEX "IX_TenantUsers_UserId" ON "TenantUsers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    ALTER TABLE "Assessments" ADD CONSTRAINT "FK_Assessments_PlanPhases_PlanPhaseId" FOREIGN KEY ("PlanPhaseId") REFERENCES "PlanPhases" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    ALTER TABLE "Assessments" ADD CONSTRAINT "FK_Assessments_Plans_PlanId" FOREIGN KEY ("PlanId") REFERENCES "Plans" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104032550_AddMultiTenantOnboardingAndRulesEngine') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104032550_AddMultiTenantOnboardingAndRulesEngine', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104034700_AddActualStartDateToPlan') THEN
    ALTER TABLE "Plans" ADD "ActualStartDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104034700_AddActualStartDateToPlan') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104034700_AddActualStartDateToPlan', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "ApprovalChains" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "Name" character varying(255) NOT NULL,
        "EntityType" character varying(100) NOT NULL,
        "Category" text NOT NULL,
        "ApprovalMode" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL,
        "ApprovalSteps" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApprovalChains" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "EscalationRules" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "Name" character varying(255) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DaysOverdueTrigger" integer NOT NULL,
        "Action" character varying(100) NOT NULL,
        "NotificationConfig" text NOT NULL,
        "ShouldReassign" boolean NOT NULL,
        "WorkflowCategory" character varying(100) NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EscalationRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "WorkflowDefinitions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "WorkflowNumber" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Framework" character varying(50) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "TriggerType" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "IsTemplate" boolean NOT NULL,
        "Version" integer NOT NULL,
        "DefaultAssignee" text NOT NULL,
        "BpmnXml" text,
        "Steps" text NOT NULL,
        "VariablesSchema" text NOT NULL,
        "RequiredPermission" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowDefinitions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "ApprovalInstances" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "ApprovalChainId" uuid NOT NULL,
        "InstanceNumber" character varying(50) NOT NULL,
        "EntityId" uuid NOT NULL,
        "EntityType" character varying(100) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "CurrentApproverRole" text NOT NULL,
        "CurrentStepIndex" integer NOT NULL,
        "InitiatedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "InitiatedByUserId" uuid NOT NULL,
        "InitiatedByUserName" text,
        "FinalDecision" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApprovalInstances" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApprovalInstances_ApprovalChains_ApprovalChainId" FOREIGN KEY ("ApprovalChainId") REFERENCES "ApprovalChains" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "WorkflowInstances" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "InstanceNumber" character varying(50) NOT NULL,
        "WorkflowDefinitionId" uuid NOT NULL,
        "Status" character varying(50) NOT NULL,
        "FailureReason" text,
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "InitiatedByUserId" uuid NOT NULL,
        "InitiatedByUserName" text,
        "Variables" text NOT NULL,
        "Result" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowInstances" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId" FOREIGN KEY ("WorkflowDefinitionId") REFERENCES "WorkflowDefinitions" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "WorkflowAuditEntries" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "WorkflowInstanceId" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "SourceEntity" character varying(100) NOT NULL,
        "SourceEntityId" uuid NOT NULL,
        "OldStatus" character varying(50),
        "NewStatus" character varying(50),
        "ActingUserId" uuid NOT NULL,
        "ActingUserName" text,
        "Description" text,
        "AdditionalData" text,
        "EventTime" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowAuditEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowAuditEntries_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE TABLE "WorkflowTasks" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "WorkflowInstanceId" uuid NOT NULL,
        "TaskName" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "AssignedToUserId" uuid NOT NULL,
        "AssignedToUserName" text,
        "DueDate" timestamp with time zone,
        "Priority" integer NOT NULL,
        "Status" character varying(50) NOT NULL,
        "StartedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "CompletedByUserId" uuid,
        "CompletedByUserName" text,
        "TaskData" text NOT NULL,
        "Comments" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowTasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowTasks_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_ApprovalChains_TenantId_EntityType" ON "ApprovalChains" ("TenantId", "EntityType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_ApprovalInstances_ApprovalChainId" ON "ApprovalInstances" ("ApprovalChainId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_ApprovalInstances_EntityType_EntityId" ON "ApprovalInstances" ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE UNIQUE INDEX "IX_ApprovalInstances_InstanceNumber" ON "ApprovalInstances" ("InstanceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_ApprovalInstances_TenantId_Status" ON "ApprovalInstances" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_EscalationRules_TenantId_DaysOverdueTrigger" ON "EscalationRules" ("TenantId", "DaysOverdueTrigger");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowAuditEntries_EventType_EventTime" ON "WorkflowAuditEntries" ("EventType", "EventTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowAuditEntries_TenantId_WorkflowInstanceId_EventTime" ON "WorkflowAuditEntries" ("TenantId", "WorkflowInstanceId", "EventTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowAuditEntries_WorkflowInstanceId" ON "WorkflowAuditEntries" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowDefinitions_Category" ON "WorkflowDefinitions" ("Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE UNIQUE INDEX "IX_WorkflowDefinitions_WorkflowNumber" ON "WorkflowDefinitions" ("WorkflowNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE UNIQUE INDEX "IX_WorkflowInstances_InstanceNumber" ON "WorkflowInstances" ("InstanceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowInstances_StartedAt" ON "WorkflowInstances" ("StartedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowInstances_TenantId_Status" ON "WorkflowInstances" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowInstances_WorkflowDefinitionId" ON "WorkflowInstances" ("WorkflowDefinitionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowTasks_AssignedToUserId_Status" ON "WorkflowTasks" ("AssignedToUserId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowTasks_Status_DueDate" ON "WorkflowTasks" ("Status", "DueDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    CREATE INDEX "IX_WorkflowTasks_WorkflowInstanceId" ON "WorkflowTasks" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104040529_AddWorkflowInfrastructureForStage2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104040529_AddWorkflowInfrastructureForStage2', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "Abilities" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "AssignedScope" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "KnowledgeAreas" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "KsaCompetencyLevel" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "RoleProfileId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD "Skills" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    CREATE TABLE "RoleProfiles" (
        "Id" uuid NOT NULL,
        "RoleCode" character varying(50) NOT NULL,
        "RoleName" character varying(200) NOT NULL,
        "Layer" character varying(100) NOT NULL,
        "Department" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Scope" text NOT NULL,
        "Responsibilities" text NOT NULL,
        "ApprovalLevel" integer NOT NULL,
        "ApprovalAuthority" numeric,
        "CanEscalate" boolean NOT NULL,
        "CanApprove" boolean NOT NULL,
        "CanReject" boolean NOT NULL,
        "CanReassign" boolean NOT NULL,
        "ParticipatingWorkflows" character varying(1000),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "DisplayOrder" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RoleProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    CREATE INDEX "IX_AspNetUsers_RoleProfileId" ON "AspNetUsers" ("RoleProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    CREATE INDEX "IX_RoleProfiles_IsActive" ON "RoleProfiles" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    CREATE INDEX "IX_RoleProfiles_Layer" ON "RoleProfiles" ("Layer");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    CREATE UNIQUE INDEX "IX_RoleProfiles_RoleCode" ON "RoleProfiles" ("RoleCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    ALTER TABLE "AspNetUsers" ADD CONSTRAINT "FK_AspNetUsers_RoleProfiles_RoleProfileId" FOREIGN KEY ("RoleProfileId") REFERENCES "RoleProfiles" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043010_AddRoleProfileAndKsa') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104043010_AddRoleProfileAndKsa', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043620_AddInboxAndTaskComments') THEN
    CREATE TABLE "TaskComments" (
        "Id" uuid NOT NULL,
        "WorkflowTaskId" uuid NOT NULL,
        "TenantId" uuid,
        "CommentedByUserId" text NOT NULL,
        "CommentedByUserName" character varying(255) NOT NULL,
        "Comment" text NOT NULL,
        "AttachmentUrl" text,
        "CommentedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TaskComments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaskComments_WorkflowTasks_WorkflowTaskId" FOREIGN KEY ("WorkflowTaskId") REFERENCES "WorkflowTasks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043620_AddInboxAndTaskComments') THEN
    CREATE INDEX "IX_TaskComments_WorkflowTaskId" ON "TaskComments" ("WorkflowTaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043620_AddInboxAndTaskComments') THEN
    CREATE INDEX "IX_TaskComments_WorkflowTaskId_CommentedAt" ON "TaskComments" ("WorkflowTaskId", "CommentedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104043620_AddInboxAndTaskComments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104043620_AddInboxAndTaskComments', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Workflows" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "WorkflowExecutions" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Tenants" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Rules" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Risks" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "PolicyViolations" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Policies" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "PlanPhases" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Evidences" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Controls" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Audits" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "AuditFindings" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    ALTER TABLE "Assessments" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    CREATE TABLE "LlmConfigurations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "Provider" text NOT NULL,
        "ApiEndpoint" text NOT NULL,
        "ApiKey" text NOT NULL,
        "ModelName" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "MaxTokens" integer NOT NULL,
        "Temperature" numeric NOT NULL,
        "EnabledForTenant" boolean NOT NULL,
        "MonthlyUsageLimit" integer NOT NULL,
        "CurrentMonthUsage" integer NOT NULL,
        "LastUsageResetDate" timestamp with time zone,
        "ConfiguredDate" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LlmConfigurations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LlmConfigurations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    CREATE INDEX "IX_LlmConfigurations_TenantId" ON "LlmConfigurations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104044103_AddLlmConfiguration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104044103_AddLlmConfiguration', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE TABLE "ApprovalRecords" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkflowId" uuid NOT NULL,
        "WorkflowNumber" text NOT NULL,
        "SubmittedBy" text NOT NULL,
        "SubmittedAt" timestamp with time zone NOT NULL,
        "CurrentApprovalLevel" integer NOT NULL,
        "Status" text NOT NULL,
        "AssignedTo" text NOT NULL,
        "DueDate" timestamp with time zone NOT NULL,
        "Priority" text NOT NULL,
        "ApprovedBy" text NOT NULL,
        "ApprovedAt" timestamp with time zone,
        "RejectedBy" text NOT NULL,
        "RejectedAt" timestamp with time zone,
        "RejectionReason" text NOT NULL,
        "DelegatedBy" text NOT NULL,
        "DelegatedAt" timestamp with time zone,
        "DelegationReason" text NOT NULL,
        "Comments" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApprovalRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApprovalRecords_WorkflowInstances_WorkflowId" FOREIGN KEY ("WorkflowId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE TABLE "SubscriptionPlans" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Code" text NOT NULL,
        "Description" text NOT NULL,
        "MonthlyPrice" numeric NOT NULL,
        "AnnualPrice" numeric NOT NULL,
        "MaxUsers" integer NOT NULL,
        "MaxAssessments" integer NOT NULL,
        "MaxPolicies" integer NOT NULL,
        "HasAdvancedReporting" boolean NOT NULL,
        "HasApiAccess" boolean NOT NULL,
        "HasPrioritySupport" boolean NOT NULL,
        "Features" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SubscriptionPlans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE TABLE "Subscriptions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PlanId" uuid NOT NULL,
        "Status" text NOT NULL,
        "TrialEndDate" timestamp with time zone,
        "SubscriptionStartDate" timestamp with time zone NOT NULL,
        "SubscriptionEndDate" timestamp with time zone,
        "NextBillingDate" timestamp with time zone,
        "BillingCycle" text NOT NULL,
        "AutoRenew" boolean NOT NULL,
        "CurrentUserCount" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Subscriptions_SubscriptionPlans_PlanId" FOREIGN KEY ("PlanId") REFERENCES "SubscriptionPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Subscriptions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE TABLE "Invoices" (
        "Id" uuid NOT NULL,
        "SubscriptionId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "InvoiceNumber" text NOT NULL,
        "InvoiceDate" timestamp with time zone NOT NULL,
        "DueDate" timestamp with time zone NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "SubTotal" numeric NOT NULL,
        "TaxAmount" numeric NOT NULL,
        "TotalAmount" numeric NOT NULL,
        "AmountPaid" numeric NOT NULL,
        "Status" text NOT NULL,
        "Notes" text,
        "SentDate" timestamp with time zone,
        "PaidDate" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Invoices_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Invoices_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE TABLE "Payments" (
        "Id" uuid NOT NULL,
        "SubscriptionId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TransactionId" text NOT NULL,
        "Amount" numeric NOT NULL,
        "Currency" text NOT NULL,
        "Status" text NOT NULL,
        "PaymentMethod" text NOT NULL,
        "Gateway" text NOT NULL,
        "PaymentDate" timestamp with time zone NOT NULL,
        "InvoiceId" uuid,
        "ErrorMessage" text,
        "PaymentDetails" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Payments_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id"),
        CONSTRAINT "FK_Payments_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Payments_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_ApprovalRecords_WorkflowId" ON "ApprovalRecords" ("WorkflowId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Invoices_SubscriptionId" ON "Invoices" ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Invoices_TenantId" ON "Invoices" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Payments_InvoiceId" ON "Payments" ("InvoiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Payments_SubscriptionId" ON "Payments" ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Payments_TenantId" ON "Payments" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Subscriptions_PlanId" ON "Subscriptions" ("PlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    CREATE INDEX "IX_Subscriptions_TenantId" ON "Subscriptions" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104132216_AddSubscriptionTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104132216_AddSubscriptionTables', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" DROP CONSTRAINT "FK_ApprovalRecords_WorkflowInstances_WorkflowId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" DROP CONSTRAINT "FK_Invoices_Subscriptions_SubscriptionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" DROP CONSTRAINT "FK_Invoices_Tenants_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" DROP CONSTRAINT "FK_LlmConfigurations_Tenants_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" DROP CONSTRAINT "FK_Payments_Subscriptions_SubscriptionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" DROP CONSTRAINT "FK_Payments_Tenants_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Subscriptions" DROP CONSTRAINT "FK_Subscriptions_Tenants_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    DROP INDEX "IX_Subscriptions_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    DROP INDEX "IX_Payments_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    DROP INDEX "IX_LlmConfigurations_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    DROP INDEX "IX_Invoices_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    DROP INDEX "IX_ApprovalRecords_WorkflowId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Subscriptions" ALTER COLUMN "TenantId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Subscriptions" ALTER COLUMN "Status" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Subscriptions" ALTER COLUMN "BillingCycle" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "TransactionId" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "TenantId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "SubscriptionId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "Status" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "PaymentMethod" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "Gateway" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ALTER COLUMN "Currency" TYPE character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" ALTER COLUMN "TenantId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" ALTER COLUMN "Provider" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" ALTER COLUMN "ModelName" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" ALTER COLUMN "ApiEndpoint" TYPE character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ALTER COLUMN "TenantId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ALTER COLUMN "SubscriptionId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ALTER COLUMN "Status" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ALTER COLUMN "InvoiceNumber" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "WorkflowNumber" TYPE character varying(50);
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "WorkflowNumber" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "SubmittedBy" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "Status" TYPE character varying(50);
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "Status" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "RejectionReason" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "RejectedBy" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "Priority" TYPE character varying(50);
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "Priority" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "DelegationReason" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "DelegatedBy" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "Comments" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "AssignedTo" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ALTER COLUMN "ApprovedBy" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ADD "WorkflowInstanceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Subscriptions_NextBillingDate" ON "Subscriptions" ("NextBillingDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Subscriptions_TenantId_Status" ON "Subscriptions" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Payments_PaymentDate" ON "Payments" ("PaymentDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Payments_TenantId_Status" ON "Payments" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE UNIQUE INDEX "IX_Payments_TransactionId" ON "Payments" ("TransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_LlmConfigurations_TenantId_IsActive" ON "LlmConfigurations" ("TenantId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Invoices_InvoiceDate" ON "Invoices" ("InvoiceDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE UNIQUE INDEX "IX_Invoices_InvoiceNumber" ON "Invoices" ("InvoiceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_Invoices_TenantId_Status" ON "Invoices" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_ApprovalRecords_TenantId_Status" ON "ApprovalRecords" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_ApprovalRecords_WorkflowInstanceId" ON "ApprovalRecords" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    CREATE INDEX "IX_ApprovalRecords_WorkflowNumber" ON "ApprovalRecords" ("WorkflowNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "ApprovalRecords" ADD CONSTRAINT "FK_ApprovalRecords_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ADD CONSTRAINT "FK_Invoices_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Invoices" ADD CONSTRAINT "FK_Invoices_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "LlmConfigurations" ADD CONSTRAINT "FK_LlmConfigurations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ADD CONSTRAINT "FK_Payments_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Payments" ADD CONSTRAINT "FK_Payments_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    ALTER TABLE "Subscriptions" ADD CONSTRAINT "FK_Subscriptions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104155008_FixEntityRelationshipWarnings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104155008_FixEntityRelationshipWarnings', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" DROP COLUMN "Comments";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" DROP COLUMN "TaskData";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" RENAME COLUMN "CompletedByUserName" TO "CompletionNotes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" RENAME COLUMN "FailureReason" TO "Metadata";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    UPDATE "WorkflowTasks" SET "TenantId" = '00000000-0000-0000-0000-000000000000' WHERE "TenantId" IS NULL;
    ALTER TABLE "WorkflowTasks" ALTER COLUMN "TenantId" SET NOT NULL;
    ALTER TABLE "WorkflowTasks" ALTER COLUMN "TenantId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" ALTER COLUMN "AssignedToUserId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" ADD "EscalatedToUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" ADD "EscalationLevel" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" ADD "IsEscalated" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowTasks" ADD "LastEscalatedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ALTER COLUMN "WorkflowDefinitionId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ALTER COLUMN "Variables" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    UPDATE "WorkflowInstances" SET "TenantId" = '00000000-0000-0000-0000-000000000000' WHERE "TenantId" IS NULL;
    ALTER TABLE "WorkflowInstances" ALTER COLUMN "TenantId" SET NOT NULL;
    ALTER TABLE "WorkflowInstances" ALTER COLUMN "TenantId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ALTER COLUMN "InitiatedByUserId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "CompletedByUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "CurrentState" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "EntityId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "EntityType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "SlaBreached" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "SlaBreachedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "SlaDueDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "WorkflowInstances" ADD "WorkflowType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    ALTER TABLE "Tenants" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "BaselineCatalogs" (
        "Id" uuid NOT NULL,
        "BaselineCode" character varying(50) NOT NULL,
        "BaselineName" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "RegulatorCode" character varying(50) NOT NULL,
        "Version" character varying(100) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "RetiredDate" timestamp with time zone,
        "Status" character varying(50) NOT NULL,
        "ControlCount" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_BaselineCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "EvidenceTypeCatalogs" (
        "Id" uuid NOT NULL,
        "EvidenceTypeCode" character varying(50) NOT NULL,
        "EvidenceTypeName" character varying(200) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Category" character varying(50) NOT NULL,
        "AllowedFileTypes" text NOT NULL,
        "MaxFileSizeMB" integer NOT NULL,
        "RequiresApproval" boolean NOT NULL,
        "MinScore" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EvidenceTypeCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "Features" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Code" character varying(255) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Features" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "Permissions" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Code" character varying(255) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "RoleCatalogs" (
        "Id" uuid NOT NULL,
        "RoleCode" character varying(50) NOT NULL,
        "RoleName" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Layer" character varying(50) NOT NULL,
        "Department" character varying(50) NOT NULL,
        "ApprovalLevel" integer NOT NULL,
        "CanApprove" boolean NOT NULL,
        "CanReject" boolean NOT NULL,
        "CanEscalate" boolean NOT NULL,
        "CanReassign" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RoleCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "TenantRoleConfigurations" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "TenantId" uuid NOT NULL,
        "RoleId" text NOT NULL,
        "Description" text NOT NULL,
        "MaxUsersWithRole" integer,
        "CanBeModified" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_TenantRoleConfigurations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantRoleConfigurations_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TenantRoleConfigurations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "UserRoleAssignments" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "UserId" text NOT NULL,
        "TenantId" uuid NOT NULL,
        "RoleId" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "AssignedAt" timestamp with time zone NOT NULL,
        "AssignedBy" text NOT NULL,
        CONSTRAINT "PK_UserRoleAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserRoleAssignments_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoleAssignments_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoleAssignments_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "WorkflowApprovals" (
        "Id" uuid NOT NULL,
        "WorkflowInstanceId" uuid NOT NULL,
        "ApprovalLevel" text NOT NULL,
        "ApprovedByUserId" text NOT NULL,
        "Decision" text NOT NULL,
        "Comments" text,
        "ApprovedAt" timestamp with time zone NOT NULL,
        "ApproversRole" text NOT NULL,
        CONSTRAINT "PK_WorkflowApprovals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowApprovals_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "WorkflowEscalations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkflowInstanceId" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "EscalationLevel" integer NOT NULL,
        "EscalationReason" text NOT NULL,
        "EscalatedAt" timestamp with time zone NOT NULL,
        "OriginalAssignee" uuid NOT NULL,
        "EscalatedToUserId" uuid,
        "Status" text NOT NULL,
        "AcknowledgedAt" timestamp with time zone,
        "AcknowledgedBy" text,
        "WorkflowTaskId" uuid NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowEscalations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowEscalations_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_WorkflowEscalations_WorkflowTasks_WorkflowTaskId" FOREIGN KEY ("WorkflowTaskId") REFERENCES "WorkflowTasks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "WorkflowNotifications" (
        "Id" uuid NOT NULL,
        "WorkflowInstanceId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "NotificationType" text NOT NULL,
        "RecipientUserId" text NOT NULL,
        "Recipient" text NOT NULL,
        "Message" text NOT NULL,
        "Body" text NOT NULL,
        "Subject" text NOT NULL,
        "Priority" text NOT NULL,
        "IsSent" boolean NOT NULL,
        "IsRead" boolean NOT NULL,
        "IsDelivered" boolean NOT NULL,
        "RequiresEmail" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "SentAt" timestamp with time zone,
        "DeliveredAt" timestamp with time zone,
        "LastAttemptAt" timestamp with time zone,
        "DeliveryAttempts" integer NOT NULL,
        "DeliveryNote" text,
        "DeliveryError" text,
        CONSTRAINT "PK_WorkflowNotifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowNotifications_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "PackageCatalogs" (
        "Id" uuid NOT NULL,
        "PackageCode" character varying(50) NOT NULL,
        "PackageName" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Category" character varying(50) NOT NULL,
        "BaselineCatalogId" uuid,
        "RequirementCount" integer NOT NULL,
        "EstimatedDays" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PackageCatalogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PackageCatalogs_BaselineCatalogs_BaselineCatalogId" FOREIGN KEY ("BaselineCatalogId") REFERENCES "BaselineCatalogs" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "FeaturePermissions" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "FeatureId" integer NOT NULL,
        "PermissionId" integer NOT NULL,
        "IsRequired" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FeaturePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FeaturePermissions_Features_FeatureId" FOREIGN KEY ("FeatureId") REFERENCES "Features" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_FeaturePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "TitleCatalogs" (
        "Id" uuid NOT NULL,
        "TitleCode" character varying(50) NOT NULL,
        "TitleName" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "RoleCatalogId" uuid NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TitleCatalogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TitleCatalogs_RoleCatalogs_RoleCatalogId" FOREIGN KEY ("RoleCatalogId") REFERENCES "RoleCatalogs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "RoleFeatures" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" text NOT NULL,
        "FeatureId" integer NOT NULL,
        "TenantId" uuid NOT NULL,
        "IsVisible" boolean NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL,
        "AssignedBy" text NOT NULL,
        "TenantRoleConfigurationId" integer,
        CONSTRAINT "PK_RoleFeatures" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RoleFeatures_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RoleFeatures_Features_FeatureId" FOREIGN KEY ("FeatureId") REFERENCES "Features" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RoleFeatures_TenantRoleConfigurations_TenantRoleConfigurati~" FOREIGN KEY ("TenantRoleConfigurationId") REFERENCES "TenantRoleConfigurations" ("Id"),
        CONSTRAINT "FK_RoleFeatures_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "RolePermissions" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" text NOT NULL,
        "PermissionId" integer NOT NULL,
        "TenantId" uuid NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL,
        "AssignedBy" text NOT NULL,
        "TenantRoleConfigurationId" integer,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RolePermissions_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_TenantRoleConfigurations_TenantRoleConfigur~" FOREIGN KEY ("TenantRoleConfigurationId") REFERENCES "TenantRoleConfigurations" ("Id"),
        CONSTRAINT "FK_RolePermissions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE TABLE "TemplateCatalogs" (
        "Id" uuid NOT NULL,
        "TemplateCode" character varying(50) NOT NULL,
        "TemplateName" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "TemplateType" character varying(50) NOT NULL,
        "PackageCatalogId" uuid,
        "RequirementCount" integer NOT NULL,
        "EstimatedDays" integer NOT NULL,
        "RequirementsJson" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TemplateCatalogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateCatalogs_PackageCatalogs_PackageCatalogId" FOREIGN KEY ("PackageCatalogId") REFERENCES "PackageCatalogs" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_FeaturePermissions_FeatureId" ON "FeaturePermissions" ("FeatureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_FeaturePermissions_PermissionId" ON "FeaturePermissions" ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_PackageCatalogs_BaselineCatalogId" ON "PackageCatalogs" ("BaselineCatalogId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RoleFeatures_FeatureId" ON "RoleFeatures" ("FeatureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RoleFeatures_RoleId" ON "RoleFeatures" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RoleFeatures_TenantId" ON "RoleFeatures" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RoleFeatures_TenantRoleConfigurationId" ON "RoleFeatures" ("TenantRoleConfigurationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RolePermissions_RoleId" ON "RolePermissions" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RolePermissions_TenantId" ON "RolePermissions" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_RolePermissions_TenantRoleConfigurationId" ON "RolePermissions" ("TenantRoleConfigurationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_TemplateCatalogs_PackageCatalogId" ON "TemplateCatalogs" ("PackageCatalogId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_TenantRoleConfigurations_RoleId" ON "TenantRoleConfigurations" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_TenantRoleConfigurations_TenantId" ON "TenantRoleConfigurations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_TitleCatalogs_RoleCatalogId" ON "TitleCatalogs" ("RoleCatalogId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_UserRoleAssignments_RoleId" ON "UserRoleAssignments" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_UserRoleAssignments_TenantId" ON "UserRoleAssignments" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_UserRoleAssignments_UserId" ON "UserRoleAssignments" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_WorkflowApprovals_WorkflowInstanceId" ON "WorkflowApprovals" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_WorkflowEscalations_WorkflowInstanceId" ON "WorkflowEscalations" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_WorkflowEscalations_WorkflowTaskId" ON "WorkflowEscalations" ("WorkflowTaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    CREATE INDEX "IX_WorkflowNotifications_WorkflowInstanceId" ON "WorkflowNotifications" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105004641_AddCatalogTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105004641_AddCatalogTables', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ALTER COLUMN "Version" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "DefaultAssigneeRoleCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "EstimatedDays" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "StepsJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "TotalSteps" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "WorkflowType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105005626_AddWorkflowDefinitionFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105005626_AddWorkflowDefinitionFields', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE TABLE "FrameworkControls" (
        "Id" uuid NOT NULL,
        "FrameworkCode" character varying(50) NOT NULL,
        "Version" character varying(20) NOT NULL,
        "ControlNumber" character varying(50) NOT NULL,
        "Domain" character varying(100) NOT NULL,
        "TitleAr" character varying(500) NOT NULL,
        "TitleEn" character varying(500) NOT NULL,
        "RequirementAr" character varying(4000) NOT NULL,
        "RequirementEn" text NOT NULL,
        "ControlType" character varying(50) NOT NULL,
        "MaturityLevel" integer NOT NULL,
        "ImplementationGuidanceEn" text NOT NULL,
        "EvidenceRequirements" character varying(1000) NOT NULL,
        "MappingIso27001" character varying(50) NOT NULL,
        "MappingNist" character varying(50) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "SearchKeywords" character varying(200) NOT NULL,
        "DefaultWeight" integer NOT NULL,
        "MinEvidenceScore" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_FrameworkControls" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE TABLE "RegulatorCatalogs" (
        "Id" uuid NOT NULL,
        "Code" character varying(20) NOT NULL,
        "NameAr" character varying(200) NOT NULL,
        "NameEn" character varying(200) NOT NULL,
        "JurisdictionEn" character varying(500) NOT NULL,
        "Website" character varying(200) NOT NULL,
        "Category" character varying(50) NOT NULL,
        "Sector" character varying(50) NOT NULL,
        "Established" integer,
        "RegionType" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RegulatorCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE TABLE "FrameworkCatalogs" (
        "Id" uuid NOT NULL,
        "Code" character varying(50) NOT NULL,
        "Version" character varying(20) NOT NULL,
        "TitleEn" character varying(300) NOT NULL,
        "TitleAr" character varying(300) NOT NULL,
        "DescriptionEn" character varying(2000) NOT NULL,
        "DescriptionAr" character varying(2000) NOT NULL,
        "RegulatorId" uuid,
        "Category" character varying(50) NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "ControlCount" integer NOT NULL,
        "Domains" character varying(50) NOT NULL,
        "EffectiveDate" timestamp with time zone,
        "RetiredDate" timestamp with time zone,
        "Status" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_FrameworkCatalogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FrameworkCatalogs_RegulatorCatalogs_RegulatorId" FOREIGN KEY ("RegulatorId") REFERENCES "RegulatorCatalogs" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE TABLE "ControlCatalogs" (
        "Id" uuid NOT NULL,
        "ControlId" character varying(50) NOT NULL,
        "FrameworkId" uuid NOT NULL,
        "Version" character varying(20) NOT NULL,
        "ControlNumber" character varying(50) NOT NULL,
        "Domain" character varying(100) NOT NULL,
        "Subdomain" character varying(100) NOT NULL,
        "TitleAr" character varying(500) NOT NULL,
        "TitleEn" character varying(500) NOT NULL,
        "RequirementAr" character varying(4000) NOT NULL,
        "RequirementEn" character varying(4000) NOT NULL,
        "ControlType" character varying(30) NOT NULL,
        "MaturityLevel" integer NOT NULL,
        "ImplementationGuidanceEn" character varying(2000) NOT NULL,
        "EvidenceRequirements" character varying(1000) NOT NULL,
        "MappingIso27001" character varying(100) NOT NULL,
        "MappingNistCsf" character varying(100) NOT NULL,
        "MappingOther" character varying(100) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlCatalogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlCatalogs_FrameworkCatalogs_FrameworkId" FOREIGN KEY ("FrameworkId") REFERENCES "FrameworkCatalogs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE INDEX "IX_ControlCatalogs_FrameworkId" ON "ControlCatalogs" ("FrameworkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE INDEX "IX_FrameworkCatalogs_RegulatorId" ON "FrameworkCatalogs" ("RegulatorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE INDEX "IX_FrameworkControls_Domain" ON "FrameworkControls" ("Domain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE INDEX "IX_FrameworkControls_FrameworkCode" ON "FrameworkControls" ("FrameworkCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE UNIQUE INDEX "IX_FrameworkControls_FrameworkCode_ControlNumber_Version" ON "FrameworkControls" ("FrameworkCode", "ControlNumber", "Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    CREATE INDEX "IX_FrameworkControls_Status" ON "FrameworkControls" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105020440_AddFrameworkControlsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105020440_AddFrameworkControlsTable', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "CurrentStageIndex" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "FlowDiagramJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "MermaidDiagram" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "StagesJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "StatusFormat" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "AnnualRevenueRange" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "BankAccountType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "BankName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "BranchCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CeoEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CeoName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CfoEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CfoName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CisoEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CisoName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CloudProviders" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CommercialRegistrationNumber" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ComplianceOfficerEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ComplianceOfficerName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CriticalVendorCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "Currency" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DataSubjectCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DpoEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DpoName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "EmployeeCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ExternalAuditorName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "FiscalYearEnd" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasDataCenterInKSA" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasExternalAuditor" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasThirdPartyDataProcessing" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HeadquartersLocation" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IncorporationDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IndustryLicenses" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IsCriticalInfrastructure" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IsPubliclyTraded" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IsRegulatedEntity" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IsSubsidiary" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ItSystemsJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalEntityName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalEntityNameAr" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalEntityType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalRepresentativeEmail" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalRepresentativeName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalRepresentativePhone" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LegalRepresentativeTitle" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OnboardingCompletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OnboardingCompletedBy" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OnboardingProgressPercent" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OnboardingStartedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OnboardingStatus" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OperatingCountries" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "OrganizationStructureJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ParentCompanyName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "PostalCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "PrimaryRegulator" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ProcessesPersonalData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ProcessesSensitiveData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RegisteredAddress" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RegisteredCity" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RegisteredRegion" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RegulatoryCertifications" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "SecondaryRegulators" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "StockExchange" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "StockSymbol" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "SubsidiaryCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "TaxIdentificationNumber" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ThirdPartyRiskLevel" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "OrganizationProfiles" ADD "VendorCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "Description" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "EmailTemplateCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "EscalateToRoleCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "EscalateToUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "HoursOverdueTrigger" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "Priority" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "RuleCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "ShouldNotifyManager" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "ShouldNotifyOriginalAssignee" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "SmsTemplateCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "TaskType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "TriggerConditionJson" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "EscalationRules" ADD "WorkflowDefinitionId" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "Assessments" ADD "DueDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "Assessments" ADD "FrameworkCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    ALTER TABLE "Assessments" ADD "TemplateCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "AssessmentRequirements" (
        "Id" uuid NOT NULL,
        "AssessmentId" uuid NOT NULL,
        "ControlNumber" text NOT NULL,
        "ControlTitle" text NOT NULL,
        "ControlTitleAr" text NOT NULL,
        "RequirementText" text NOT NULL,
        "RequirementTextAr" text NOT NULL,
        "Domain" text NOT NULL,
        "ControlType" text NOT NULL,
        "MaturityLevel" text NOT NULL,
        "Status" text NOT NULL,
        "EvidenceStatus" text NOT NULL,
        "ImplementationGuidance" text NOT NULL,
        "ImplementationGuidanceAr" text NOT NULL,
        "ToolkitReference" text NOT NULL,
        "SampleEvidenceDescription" text NOT NULL,
        "BestPractices" text NOT NULL,
        "CommonGaps" text NOT NULL,
        "ScoringGuideJson" text NOT NULL,
        "WeightPercentage" integer NOT NULL,
        "IsAutoScorable" boolean NOT NULL,
        "AutoScoreRuleJson" text NOT NULL,
        "Score" integer,
        "MaxScore" integer,
        "ScoreRationale" text NOT NULL,
        "IsAutoScored" boolean NOT NULL,
        "ScoredAt" timestamp with time zone,
        "ScoredBy" text NOT NULL,
        "OwnerRoleCode" text NOT NULL,
        "ReviewerRoleCode" text NOT NULL,
        "AssignedToUserId" uuid,
        "ReviewedByUserId" uuid,
        "DueDate" timestamp with time zone,
        "CompletedDate" timestamp with time zone,
        "ReviewedDate" timestamp with time zone,
        "Findings" text NOT NULL,
        "Recommendations" text NOT NULL,
        "RemediationPlan" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AssessmentRequirements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AssessmentRequirements_Assessments_AssessmentId" FOREIGN KEY ("AssessmentId") REFERENCES "Assessments" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "DataQualityScores" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "EntityId" uuid NOT NULL,
        "EntityType" text NOT NULL,
        "CompletenessScore" integer NOT NULL,
        "AccuracyScore" integer NOT NULL,
        "ConsistencyScore" integer NOT NULL,
        "TimelinessScore" integer NOT NULL,
        "OverallScore" integer NOT NULL,
        "QualificationLevel" text NOT NULL,
        "QualificationPoints" integer NOT NULL,
        "IsQualified" boolean NOT NULL,
        "TotalFields" integer NOT NULL,
        "ValidFields" integer NOT NULL,
        "InvalidFields" integer NOT NULL,
        "MissingFields" integer NOT NULL,
        "IssuesJson" text NOT NULL,
        "CalculatedAt" timestamp with time zone NOT NULL,
        "LastImprovedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DataQualityScores" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "DelegationRules" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "RuleCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "DelegationType" text NOT NULL,
        "DelegatorUserId" uuid,
        "DelegatorRoleCode" text NOT NULL,
        "DelegateUserId" uuid,
        "DelegateRoleCode" text NOT NULL,
        "DelegateSelectionRule" text NOT NULL,
        "EffectiveFrom" timestamp with time zone,
        "EffectiveTo" timestamp with time zone,
        "IsIndefinite" boolean NOT NULL,
        "TaskTypesJson" text NOT NULL,
        "WorkflowCategoriesJson" text NOT NULL,
        "ApprovalAmountLimit" numeric,
        "ApprovalLevelLimit" text NOT NULL,
        "CanSubDelegate" boolean NOT NULL,
        "CanApprove" boolean NOT NULL,
        "CanReject" boolean NOT NULL,
        "CanReassign" boolean NOT NULL,
        "CanEscalate" boolean NOT NULL,
        "CanViewConfidential" boolean NOT NULL,
        "NotifyDelegatorOnAction" boolean NOT NULL,
        "NotifyDelegateOnAssignment" boolean NOT NULL,
        "RequireDelegatorConfirmation" boolean NOT NULL,
        "CreatedReason" text NOT NULL,
        "ApprovedByUserId" uuid,
        "ApprovedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DelegationRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "SlaRules" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "RuleCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "ResponseTimeHours" integer NOT NULL,
        "ResolutionTimeHours" integer NOT NULL,
        "WarningThresholdPercent" integer NOT NULL,
        "CriticalThresholdPercent" integer NOT NULL,
        "UseBusinessHoursOnly" boolean NOT NULL,
        "BusinessHoursJson" text NOT NULL,
        "ExcludedDaysJson" text NOT NULL,
        "ExcludeHolidays" boolean NOT NULL,
        "WorkflowCategory" text NOT NULL,
        "TaskPriority" text NOT NULL,
        "TaskType" text NOT NULL,
        "ApplicableRolesJson" text NOT NULL,
        "OnWarningAction" text NOT NULL,
        "OnBreachAction" text NOT NULL,
        "BreachEscalationRuleCode" text NOT NULL,
        "AutoExtendOnHoliday" boolean NOT NULL,
        "TrackFirstResponseTime" boolean NOT NULL,
        "TrackResolutionTime" boolean NOT NULL,
        "IncludeInReporting" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SlaRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "TriggerRules" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "RuleCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "TriggerEvent" text NOT NULL,
        "EntityType" text NOT NULL,
        "EventConditionJson" text NOT NULL,
        "ConditionType" text NOT NULL,
        "ConditionExpression" text NOT NULL,
        "AgentPrompt" text NOT NULL,
        "RequiresAgentEvaluation" boolean NOT NULL,
        "ActionType" text NOT NULL,
        "ActionConfigJson" text NOT NULL,
        "WorkflowDefinitionId" text NOT NULL,
        "NotificationTemplateCode" text NOT NULL,
        "WebhookUrl" text NOT NULL,
        "CronExpression" text NOT NULL,
        "NextRunAt" timestamp with time zone,
        "LastRunAt" timestamp with time zone,
        "RunOnce" boolean NOT NULL,
        "MaxExecutionsPerDay" integer NOT NULL,
        "CooldownMinutes" integer NOT NULL,
        "IsAsync" boolean NOT NULL,
        "ExecutionCount" integer NOT NULL,
        "SuccessCount" integer NOT NULL,
        "FailureCount" integer NOT NULL,
        "LastSuccessAt" timestamp with time zone,
        "LastErrorMessage" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TriggerRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "ValidationRules" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "RuleCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "EntityType" text NOT NULL,
        "FieldName" text NOT NULL,
        "FieldPath" text NOT NULL,
        "ValidationType" text NOT NULL,
        "DataType" text NOT NULL,
        "IsRequired" boolean NOT NULL,
        "MinLength" integer,
        "MaxLength" integer,
        "RegexPattern" text NOT NULL,
        "AllowedValuesJson" text NOT NULL,
        "MinValue" numeric,
        "MaxValue" numeric,
        "DateFormat" text NOT NULL,
        "MinDate" timestamp with time zone,
        "MaxDate" timestamp with time zone,
        "AllowedFileTypes" text NOT NULL,
        "MaxFileSizeMB" integer,
        "RequireDigitalSignature" boolean NOT NULL,
        "DependentFieldName" text NOT NULL,
        "DependentConditionJson" text NOT NULL,
        "ExternalApiUrl" text NOT NULL,
        "ExternalApiMethod" text NOT NULL,
        "ExternalApiHeaders" text NOT NULL,
        "ExternalApiTimeoutMs" integer NOT NULL,
        "IsQualificationRule" boolean NOT NULL,
        "QualificationLevel" text NOT NULL,
        "QualificationScore" integer NOT NULL,
        "QualificationCriteria" text NOT NULL,
        "ErrorMessageEn" text NOT NULL,
        "ErrorMessageAr" text NOT NULL,
        "Severity" text NOT NULL,
        "BlockOnFailure" boolean NOT NULL,
        "ExecutionCount" integer NOT NULL,
        "PassCount" integer NOT NULL,
        "FailCount" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ValidationRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "DelegationLogs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "DelegationRuleId" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "DelegatorUserId" uuid NOT NULL,
        "DelegateUserId" uuid NOT NULL,
        "Action" text NOT NULL,
        "ActionAt" timestamp with time zone NOT NULL,
        "ActionBy" text NOT NULL,
        "Notes" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DelegationLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DelegationLogs_DelegationRules_DelegationRuleId" FOREIGN KEY ("DelegationRuleId") REFERENCES "DelegationRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "TriggerExecutionLogs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "TriggerRuleId" uuid NOT NULL,
        "TriggerEvent" text NOT NULL,
        "EntityId" uuid,
        "EntityType" text NOT NULL,
        "ExecutedAt" timestamp with time zone NOT NULL,
        "Status" text NOT NULL,
        "ResultJson" text NOT NULL,
        "ErrorMessage" text NOT NULL,
        "WasAgentEvaluated" boolean NOT NULL,
        "AgentResponseJson" text NOT NULL,
        "AgentConfidenceScore" double precision,
        "ExecutionTimeMs" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TriggerExecutionLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TriggerExecutionLogs_TriggerRules_TriggerRuleId" FOREIGN KEY ("TriggerRuleId") REFERENCES "TriggerRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE TABLE "ValidationResults" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "ValidationRuleId" uuid NOT NULL,
        "EntityId" uuid NOT NULL,
        "EntityType" text NOT NULL,
        "FieldName" text NOT NULL,
        "FieldValue" text NOT NULL,
        "IsValid" boolean NOT NULL,
        "Status" text NOT NULL,
        "ErrorMessage" text NOT NULL,
        "ValidatedAt" timestamp with time zone NOT NULL,
        "ValidatedBy" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ValidationResults" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ValidationResults_ValidationRules_ValidationRuleId" FOREIGN KEY ("ValidationRuleId") REFERENCES "ValidationRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE INDEX "IX_AssessmentRequirements_AssessmentId" ON "AssessmentRequirements" ("AssessmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE INDEX "IX_DelegationLogs_DelegationRuleId" ON "DelegationLogs" ("DelegationRuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE INDEX "IX_TriggerExecutionLogs_TriggerRuleId" ON "TriggerExecutionLogs" ("TriggerRuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    CREATE INDEX "IX_ValidationResults_ValidationRuleId" ON "ValidationResults" ("ValidationRuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105025510_AddNewGrcEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105025510_AddNewGrcEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    ALTER TABLE "Risks" ADD "ConsequenceArea" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    ALTER TABLE "Risks" ADD "IdentifiedDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    ALTER TABLE "Risks" ADD "ResponsibleParty" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    ALTER TABLE "Risks" ADD "RiskNumber" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    ALTER TABLE "Risks" ADD "Title" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "EvidenceScores" (
        "Id" uuid NOT NULL,
        "EvidenceId" uuid NOT NULL,
        "Score" integer NOT NULL,
        "ScoringCriteria" text NOT NULL,
        "Comments" text NOT NULL,
        "ScoredBy" text NOT NULL,
        "ScoredAt" timestamp with time zone NOT NULL,
        "IsFinal" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EvidenceScores" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EvidenceScores_Evidences_EvidenceId" FOREIGN KEY ("EvidenceId") REFERENCES "Evidences" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "Reports" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ReportNumber" character varying(50) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Scope" character varying(500) NOT NULL,
        "ReportPeriodStart" timestamp with time zone NOT NULL,
        "ReportPeriodEnd" timestamp with time zone NOT NULL,
        "ExecutiveSummary" text NOT NULL,
        "KeyFindings" text NOT NULL,
        "Recommendations" text NOT NULL,
        "TotalFindingsCount" integer NOT NULL,
        "CriticalFindingsCount" integer NOT NULL,
        "GeneratedBy" text NOT NULL,
        "GeneratedDate" timestamp with time zone,
        "DeliveredTo" text,
        "DeliveryDate" timestamp with time zone,
        "FileUrl" text,
        "FilePath" text,
        "FileName" text,
        "ContentType" text,
        "FileSize" bigint,
        "FileHash" text,
        "PageCount" integer NOT NULL,
        "IncludedEntitiesJson" text NOT NULL,
        "MetadataJson" text NOT NULL,
        "CorrelationId" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Reports" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Reports_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "Resiliences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "AssessmentNumber" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "AssessmentType" character varying(100) NOT NULL,
        "Framework" character varying(100) NOT NULL,
        "Scope" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "AssessmentDate" timestamp with time zone,
        "DueDate" timestamp with time zone,
        "CompletedDate" timestamp with time zone,
        "AssessedByUserId" uuid,
        "AssessedByUserName" text,
        "ReviewedByUserId" uuid,
        "ReviewedByUserName" text,
        "ApprovedByUserId" uuid,
        "ApprovedByUserName" text,
        "ResilienceScore" numeric,
        "BusinessContinuityScore" numeric,
        "DisasterRecoveryScore" numeric,
        "CyberResilienceScore" numeric,
        "OverallRating" text,
        "AssessmentDetails" text,
        "Findings" text,
        "ActionItems" text,
        "RelatedAssessmentId" uuid,
        "RelatedRiskId" uuid,
        "RelatedWorkflowInstanceId" uuid,
        "EvidenceUrls" text,
        "ReportUrl" text,
        "Tags" text,
        "Notes" text,
        "IsDeleted" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        CONSTRAINT "PK_Resiliences" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "RiskResiliences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "AssessmentNumber" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" text NOT NULL,
        "RiskCategory" character varying(100) NOT NULL,
        "RiskType" character varying(100) NOT NULL,
        "RelatedRiskId" uuid,
        "RiskToleranceLevel" numeric,
        "RecoveryCapabilityScore" numeric,
        "ImpactMitigationScore" numeric,
        "ResilienceRating" text,
        "RiskScenario" text,
        "ResilienceMeasures" text,
        "RecoveryPlan" text,
        "Status" character varying(50) NOT NULL,
        "AssessmentDate" timestamp with time zone,
        "DueDate" timestamp with time zone,
        "CompletedDate" timestamp with time zone,
        "AssessedByUserId" uuid,
        "AssessedByUserName" text,
        "ReviewedByUserId" uuid,
        "ReviewedByUserName" text,
        "RelatedWorkflowInstanceId" uuid,
        "RelatedAssessmentId" uuid,
        "Notes" text,
        "IsDeleted" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        CONSTRAINT "PK_RiskResiliences" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "UserNotificationPreferences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "EmailEnabled" boolean NOT NULL,
        "SmsEnabled" boolean NOT NULL,
        "InAppEnabled" boolean NOT NULL,
        "PushEnabled" boolean NOT NULL,
        "EnabledTypesJson" text NOT NULL,
        "DigestFrequency" text NOT NULL,
        "PreferredTime" text NOT NULL,
        "Timezone" text NOT NULL,
        "QuietHoursEnabled" boolean NOT NULL,
        "QuietHoursStart" text NOT NULL,
        "QuietHoursEnd" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserNotificationPreferences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserNotificationPreferences_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "UserProfiles" (
        "Id" uuid NOT NULL,
        "ProfileCode" text NOT NULL,
        "ProfileName" text NOT NULL,
        "Description" text NOT NULL,
        "Category" text NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsSystemProfile" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "PermissionsJson" text NOT NULL,
        "WorkflowRolesJson" text NOT NULL,
        "UiAccessJson" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "WorkflowTransitions" (
        "Id" uuid NOT NULL,
        "WorkflowInstanceId" uuid NOT NULL,
        "FromState" character varying(100) NOT NULL,
        "ToState" character varying(100) NOT NULL,
        "TriggeredBy" character varying(255) NOT NULL,
        "TransitionDate" timestamp with time zone NOT NULL,
        "Reason" character varying(500),
        "ContextData" jsonb,
        CONSTRAINT "PK_WorkflowTransitions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowTransitions_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE TABLE "UserProfileAssignments" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "UserProfileId" uuid NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL,
        "AssignedBy" text NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserProfileAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserProfileAssignments_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserProfileAssignments_UserProfiles_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES "UserProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_EvidenceScores_EvidenceId" ON "EvidenceScores" ("EvidenceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_Reports_CorrelationId" ON "Reports" ("CorrelationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE UNIQUE INDEX "IX_Reports_ReportNumber" ON "Reports" ("ReportNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_Reports_TenantId_Type_Status" ON "Reports" ("TenantId", "Type", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_Resiliences_AssessmentDate" ON "Resiliences" ("AssessmentDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_Resiliences_TenantId" ON "Resiliences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_Resiliences_TenantId_Status" ON "Resiliences" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_RiskResiliences_AssessmentDate" ON "RiskResiliences" ("AssessmentDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_RiskResiliences_RelatedRiskId" ON "RiskResiliences" ("RelatedRiskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_RiskResiliences_TenantId" ON "RiskResiliences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_RiskResiliences_TenantId_Status" ON "RiskResiliences" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_UserNotificationPreferences_TenantId" ON "UserNotificationPreferences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_UserProfileAssignments_TenantId" ON "UserProfileAssignments" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_UserProfileAssignments_UserProfileId" ON "UserProfileAssignments" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_WorkflowTransitions_TransitionDate" ON "WorkflowTransitions" ("TransitionDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    CREATE INDEX "IX_WorkflowTransitions_WorkflowInstanceId" ON "WorkflowTransitions" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105150655_AddReportFileFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105150655_AddReportFileFields', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105165429_AddTaskDelegationEntity') THEN
    ALTER TABLE "WorkflowTasks" ADD "Metadata" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105165429_AddTaskDelegationEntity') THEN
    CREATE TABLE "TaskDelegations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "WorkflowInstanceId" uuid NOT NULL,
        "FromType" text NOT NULL,
        "FromUserId" uuid,
        "FromUserName" text,
        "FromAgentType" text,
        "ToType" text NOT NULL,
        "ToUserId" uuid,
        "ToUserName" text,
        "ToAgentType" text,
        "ToAgentTypesJson" text,
        "Action" text NOT NULL,
        "Reason" text,
        "DelegatedAt" timestamp with time zone NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "IsRevoked" boolean NOT NULL,
        "RevokedAt" timestamp with time zone,
        "RevokedByUserId" uuid,
        "DelegationStrategy" text NOT NULL,
        "SelectedAgentType" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TaskDelegations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaskDelegations_WorkflowInstances_WorkflowInstanceId" FOREIGN KEY ("WorkflowInstanceId") REFERENCES "WorkflowInstances" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TaskDelegations_WorkflowTasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "WorkflowTasks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105165429_AddTaskDelegationEntity') THEN
    CREATE INDEX "IX_TaskDelegations_TaskId" ON "TaskDelegations" ("TaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105165429_AddTaskDelegationEntity') THEN
    CREATE INDEX "IX_TaskDelegations_WorkflowInstanceId" ON "TaskDelegations" ("WorkflowInstanceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105165429_AddTaskDelegationEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105165429_AddTaskDelegationEntity', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "WorkflowTasks" ALTER COLUMN "Metadata" TYPE character varying(4000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "ToUserName" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "ToType" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "ToAgentTypesJson" TYPE character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "ToAgentType" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "SelectedAgentType" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "Reason" TYPE character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "FromUserName" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "FromType" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "FromAgentType" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "DelegationStrategy" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    ALTER TABLE "TaskDelegations" ALTER COLUMN "Action" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentDefinitions" (
        "Id" uuid NOT NULL,
        "AgentCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "AgentType" character varying(30) NOT NULL,
        "CapabilitiesJson" text,
        "DataSourcesJson" text,
        "AllowedActionsJson" text,
        "ApprovalRequiredActionsJson" text,
        "AutoApprovalConfidenceThreshold" integer NOT NULL,
        "OversightRoleCode" character varying(50),
        "EscalationRoleCode" character varying(50),
        "IsActive" boolean NOT NULL,
        "Version" character varying(20) NOT NULL,
        "ActivatedAt" timestamp with time zone NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentDefinitions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentSoDRules" (
        "Id" uuid NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "Action1" character varying(100) NOT NULL,
        "Action1AgentTypes" character varying(255),
        "Action2" character varying(100) NOT NULL,
        "Action2AgentTypes" character varying(255),
        "RiskDescription" character varying(1000),
        "Enforcement" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentSoDRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ApplicabilityRuleCatalogs" (
        "Id" uuid NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "RuleCategory" character varying(30) NOT NULL,
        "ConditionExpression" text NOT NULL,
        "ActionType" character varying(30) NOT NULL,
        "ActionTarget" character varying(100) NOT NULL,
        "ActionParametersJson" text,
        "Priority" integer NOT NULL,
        "IsBlocking" boolean NOT NULL,
        "Version" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApplicabilityRuleCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AssessmentScopes" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AssessmentId" uuid,
        "ScopeCode" character varying(50) NOT NULL,
        "ScopeName" character varying(255) NOT NULL,
        "ScopeType" character varying(30) NOT NULL,
        "ScopeDescription" character varying(2000),
        "Jurisdictions" character varying(200),
        "BusinessLines" character varying(500),
        "SystemsInScope" text,
        "DataTypes" character varying(200),
        "HostingModels" character varying(100),
        "ThirdPartiesInScope" text,
        "OutOfScopeDescription" character varying(2000),
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "IsApproved" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AssessmentScopes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AssessmentScopes_Assessments_AssessmentId" FOREIGN KEY ("AssessmentId") REFERENCES "Assessments" ("Id"),
        CONSTRAINT "FK_AssessmentScopes_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "BaselineControlSets" (
        "Id" uuid NOT NULL,
        "BaselineCode" character varying(30) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "BaselineType" character varying(20) NOT NULL,
        "Version" character varying(20) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_BaselineControlSets" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlChangeHistories" (
        "Id" uuid NOT NULL,
        "EntityType" character varying(50) NOT NULL,
        "EntityId" uuid NOT NULL,
        "ChangeType" character varying(20) NOT NULL,
        "PreviousValueJson" text,
        "NewValueJson" text,
        "ChangeReason" character varying(1000),
        "ChangeRequestId" character varying(100),
        "ChangedBy" character varying(100) NOT NULL,
        "ChangedAt" timestamp with time zone NOT NULL,
        "ApprovalStatus" character varying(20) NOT NULL,
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlChangeHistories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlDomains" (
        "Id" uuid NOT NULL,
        "DomainCode" character varying(20) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "DescriptionAr" character varying(1000),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlDomains" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CrossReferenceMappings" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ObjectType" character varying(50) NOT NULL,
        "InternalId" uuid NOT NULL,
        "InternalCode" character varying(100) NOT NULL,
        "ExternalSystemCode" character varying(50) NOT NULL,
        "ExternalId" character varying(255) NOT NULL,
        "ExternalUrl" character varying(500),
        "LastSyncAt" timestamp with time zone,
        "SyncStatus" character varying(20) NOT NULL,
        "LastSyncError" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CrossReferenceMappings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CryptographicAssets" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AssetCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "AssetType" character varying(30) NOT NULL,
        "SystemName" character varying(255),
        "CurrentAlgorithm" character varying(50),
        "KeySizeBits" integer,
        "IsQuantumVulnerable" boolean NOT NULL,
        "PQCMigrationStatus" character varying(20) NOT NULL,
        "TargetPQCAlgorithm" character varying(50),
        "MigrationPriority" character varying(20) NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "OwnerId" character varying(100),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CryptographicAssets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CryptographicAssets_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "DomainEvents" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "CorrelationId" character varying(50) NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "SchemaVersion" character varying(10) NOT NULL,
        "SourceSystem" character varying(50) NOT NULL,
        "ObjectType" character varying(50) NOT NULL,
        "ObjectId" uuid NOT NULL,
        "ObjectCode" character varying(100),
        "PayloadJson" text NOT NULL,
        "Status" character varying(20) NOT NULL,
        "ProcessingAttempts" integer NOT NULL,
        "LastError" character varying(2000),
        "OccurredAt" timestamp with time zone NOT NULL,
        "PublishedAt" timestamp with time zone,
        "ProcessedAt" timestamp with time zone,
        "TriggeredBy" character varying(100),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DomainEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ERPSystemConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "SystemCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "ERPType" character varying(30) NOT NULL,
        "Environment" character varying(20) NOT NULL,
        "ConnectionMethod" character varying(30) NOT NULL,
        "ConnectionConfigJson" text,
        "ServiceAccountId" character varying(100),
        "IsReadOnlyReplica" boolean NOT NULL,
        "ConnectionStatus" character varying(20) NOT NULL,
        "LastHealthCheck" timestamp with time zone,
        "AvailableModules" character varying(255),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ERPSystemConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ERPSystemConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EventSchemaRegistries" (
        "Id" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "SchemaVersion" character varying(10) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "JsonSchema" text NOT NULL,
        "ExamplePayloadJson" text,
        "RequiredFields" character varying(500),
        "IsCurrent" boolean NOT NULL,
        "EffectiveFrom" timestamp with time zone NOT NULL,
        "DeprecatedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EventSchemaRegistries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EventSubscriptions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "SubscriptionCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "EventTypePattern" character varying(100) NOT NULL,
        "SubscriberSystem" character varying(50) NOT NULL,
        "DeliveryMethod" character varying(20) NOT NULL,
        "DeliveryEndpoint" character varying(500),
        "RetryPolicy" character varying(20) NOT NULL,
        "MaxRetries" integer NOT NULL,
        "FilterExpression" text,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EventSubscriptions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EvidencePackFamilies" (
        "Id" uuid NOT NULL,
        "FamilyCode" character varying(30) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "IconClass" character varying(50),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EvidencePackFamilies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EvidencePacks" (
        "Id" uuid NOT NULL,
        "PackCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "EvidenceItemsJson" text,
        "RequiredFrequency" character varying(20) NOT NULL,
        "RetentionMonths" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EvidencePacks" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EvidenceSourceIntegrations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "SourceCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "SourceType" character varying(30) NOT NULL,
        "ConnectionStatus" character varying(20) NOT NULL,
        "ConnectionConfigJson" text,
        "LastSyncDate" timestamp with time zone,
        "SyncFrequency" character varying(20) NOT NULL,
        "EvidenceTypesProvided" character varying(500),
        "ControlsCoveredJson" text,
        "KRIsFed" character varying(500),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EvidenceSourceIntegrations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EvidenceSourceIntegrations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "GovernanceCadences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "CadenceCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "CadenceType" character varying(30) NOT NULL,
        "Frequency" character varying(20) NOT NULL,
        "DayOfWeek" integer,
        "DayOfMonth" integer,
        "WeekOfMonth" integer,
        "TimeOfDay" character varying(5),
        "Timezone" character varying(50) NOT NULL,
        "OwnerRoleCode" character varying(50),
        "ParticipantRoleCodes" character varying(500),
        "ActivitiesJson" text,
        "DeliverablesJson" text,
        "TeamsChannelId" character varying(255),
        "ReminderHoursBefore" integer NOT NULL,
        "LastExecutionDate" timestamp with time zone,
        "NextScheduledDate" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_GovernanceCadences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GovernanceCadences_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "GovernanceRhythmTemplates" (
        "Id" uuid NOT NULL,
        "TemplateCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsDefault" boolean NOT NULL,
        "RhythmItemsJson" text,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_GovernanceRhythmTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "HumanRetainedResponsibilities" (
        "Id" uuid NOT NULL,
        "ResponsibilityCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "Category" character varying(30) NOT NULL,
        "RoleCode" character varying(50) NOT NULL,
        "NonDelegableReason" character varying(1000),
        "RegulatoryReference" character varying(255),
        "AgentSupportDescription" character varying(1000),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_HumanRetainedResponsibilities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ImportantBusinessServices" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ServiceCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "Category" character varying(30) NOT NULL,
        "CriticalityTier" character varying(10) NOT NULL,
        "RTO_Hours" integer NOT NULL,
        "RPO_Hours" integer NOT NULL,
        "MTD_Hours" integer NOT NULL,
        "ServiceOwnerId" character varying(100),
        "ServiceOwnerName" character varying(255),
        "SupportingSystemsJson" text,
        "Dependencies" character varying(500),
        "LastDRTestDate" timestamp with time zone,
        "LastDRTestResult" character varying(20),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ImportantBusinessServices" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ImportantBusinessServices_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "IntegrationConnectors" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "ConnectorCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "ConnectorType" character varying(30) NOT NULL,
        "TargetSystem" character varying(50) NOT NULL,
        "ConnectionConfigJson" text,
        "AuthType" character varying(20) NOT NULL,
        "ConnectionStatus" character varying(20) NOT NULL,
        "LastHealthCheck" timestamp with time zone,
        "LastSuccessfulSync" timestamp with time zone,
        "ErrorCount" integer NOT NULL,
        "SupportedOperationsJson" text,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_IntegrationConnectors" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "LegalDocuments" (
        "Id" uuid NOT NULL,
        "DocumentType" character varying(100) NOT NULL,
        "Version" character varying(50) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "TitleAr" character varying(255),
        "ContentEn" text NOT NULL,
        "ContentAr" text,
        "Summary" character varying(1000),
        "EffectiveDate" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "ContentHash" character varying(256),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LegalDocuments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "MappingWorkflowTemplates" (
        "Id" uuid NOT NULL,
        "TemplateCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "StepsJson" text,
        "IsDefault" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MappingWorkflowTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "OnePageGuides" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "GuideCode" character varying(50) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "TitleAr" character varying(255),
        "TargetAudience" character varying(50) NOT NULL,
        "WhatIsInScope" character varying(2000),
        "HowToDecideApplicability" character varying(2000),
        "WhereToStoreEvidence" character varying(2000),
        "WhoApprovesExceptions" character varying(2000),
        "HowAuditsAreServed" character varying(2000),
        "QuickLinksJson" text,
        "ContactInfo" character varying(500),
        "Version" character varying(20) NOT NULL,
        "LastUpdated" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OnePageGuides" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OnePageGuides_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "OverlayCatalogs" (
        "Id" uuid NOT NULL,
        "OverlayCode" character varying(30) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "OverlayType" character varying(30) NOT NULL,
        "AppliesTo" character varying(100) NOT NULL,
        "Priority" integer NOT NULL,
        "Version" character varying(20) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OverlayCatalogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RegulatoryRequirements" (
        "Id" uuid NOT NULL,
        "RequirementCode" character varying(50) NOT NULL,
        "RegulatorCode" character varying(50) NOT NULL,
        "FrameworkCode" character varying(50) NOT NULL,
        "FrameworkVersion" character varying(255),
        "Section" character varying(100),
        "RequirementText" character varying(2000) NOT NULL,
        "RequirementTextAr" character varying(2000),
        "RequirementType" character varying(20) NOT NULL,
        "Jurisdictions" character varying(100),
        "Industries" character varying(255),
        "DataTypes" character varying(255),
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RegulatoryRequirements" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RoleTransitionPlans" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PlanCode" character varying(50) NOT NULL,
        "CurrentRoleName" character varying(255) NOT NULL,
        "CurrentAutomationPercent" integer NOT NULL,
        "TargetAutomationPercent" integer NOT NULL,
        "TasksToAutomateJson" text,
        "TasksToRetainJson" text,
        "AssignedAgentCodes" character varying(255),
        "NewHumanRole" character varying(255),
        "Phase" character varying(20) NOT NULL,
        "TargetCompletionDate" timestamp with time zone,
        "RiskMitigationNotes" character varying(2000),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RoleTransitionPlans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ShahinAIBrandConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "BrandCode" character varying(50) NOT NULL,
        "BrandName" character varying(255) NOT NULL,
        "BrandNameAr" character varying(255) NOT NULL,
        "Tagline" character varying(500) NOT NULL,
        "TaglineAr" character varying(500) NOT NULL,
        "OperatingSentence" character varying(500) NOT NULL,
        "OperatingSentenceAr" character varying(500) NOT NULL,
        "PrimaryColor" character varying(10) NOT NULL,
        "SecondaryColor" character varying(10) NOT NULL,
        "AccentColor" character varying(10) NOT NULL,
        "LogoUrl" character varying(500),
        "LogoDarkUrl" character varying(500),
        "FaviconUrl" character varying(500),
        "PublicWebsiteUrl" character varying(255) NOT NULL,
        "AppUrl" character varying(255) NOT NULL,
        "SupportEmail" character varying(255) NOT NULL,
        "DefaultLanguage" character varying(5) NOT NULL,
        "RTLEnabled" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ShahinAIBrandConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ShahinAIBrandConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ShahinAIModules" (
        "Id" uuid NOT NULL,
        "ModuleCode" character varying(20) NOT NULL,
        "Name" character varying(100) NOT NULL,
        "NameAr" character varying(100) NOT NULL,
        "ShortDescription" character varying(255) NOT NULL,
        "ShortDescriptionAr" character varying(255) NOT NULL,
        "FullDescription" character varying(1000),
        "FullDescriptionAr" character varying(1000),
        "Outcome" character varying(500),
        "OutcomeAr" character varying(500),
        "IconClass" character varying(50),
        "ModuleColor" character varying(10),
        "RoutePath" character varying(100),
        "DisplayOrder" integer NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "RequiresLicense" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ShahinAIModules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SiteMapEntries" (
        "Id" uuid NOT NULL,
        "PageCode" character varying(50) NOT NULL,
        "SiteType" character varying(10) NOT NULL,
        "TitleEn" character varying(255) NOT NULL,
        "TitleAr" character varying(255) NOT NULL,
        "UrlPath" character varying(255) NOT NULL,
        "ParentPageCode" character varying(50),
        "MetaDescriptionEn" character varying(500),
        "MetaDescriptionAr" character varying(500),
        "ShowInNav" boolean NOT NULL,
        "RequiresAuth" boolean NOT NULL,
        "IconClass" character varying(50),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SiteMapEntries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "StrategicRoadmapMilestones" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "MilestoneCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "CapabilityArea" character varying(30) NOT NULL,
        "Phase" character varying(20) NOT NULL,
        "TargetDate" timestamp with time zone,
        "Status" character varying(20) NOT NULL,
        "CompletionPercent" integer NOT NULL,
        "OwnerId" character varying(100),
        "Dependencies" character varying(500),
        "SuccessCriteria" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_StrategicRoadmapMilestones" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StrategicRoadmapMilestones_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SupportConversations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "UserId" text,
        "SessionId" character varying(100),
        "Status" character varying(50) NOT NULL,
        "Category" character varying(100),
        "Subject" character varying(255),
        "Priority" character varying(20) NOT NULL,
        "IsAgentHandled" boolean NOT NULL,
        "AssignedAgentId" text,
        "StartedAt" timestamp with time zone NOT NULL,
        "ResolvedAt" timestamp with time zone,
        "SatisfactionRating" integer,
        "Feedback" character varying(1000),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SupportConversations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SystemOfRecordDefinitions" (
        "Id" uuid NOT NULL,
        "ObjectType" character varying(50) NOT NULL,
        "SystemCode" character varying(50) NOT NULL,
        "SystemName" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsAuthoritative" boolean NOT NULL,
        "AllowExternalCreate" boolean NOT NULL,
        "AllowExternalUpdate" boolean NOT NULL,
        "ApiEndpoint" character varying(500),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SystemOfRecordDefinitions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "TeamsNotificationConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ConfigCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NotificationType" character varying(30) NOT NULL,
        "WebhookUrl" character varying(500),
        "ChannelId" character varying(255),
        "TriggerConditionsJson" text,
        "MessageTemplateJson" text,
        "UseAdaptiveCard" boolean NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TeamsNotificationConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TeamsNotificationConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "TestProcedures" (
        "Id" uuid NOT NULL,
        "TestCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "TestType" character varying(30) NOT NULL,
        "TestStepsJson" text,
        "ExpectedResults" character varying(2000),
        "SampleSizeGuidance" character varying(500),
        "Frequency" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TestProcedures" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ThirdPartyConcentrations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "VendorCode" character varying(50) NOT NULL,
        "VendorName" character varying(255) NOT NULL,
        "VendorType" character varying(30) NOT NULL,
        "ServicesProvidedJson" text,
        "CriticalityTier" character varying(10) NOT NULL,
        "Substitutability" character varying(20) NOT NULL,
        "ConcentrationRiskScore" integer NOT NULL,
        "HasTestedExitPlan" boolean NOT NULL,
        "ExitPlanLastTested" timestamp with time zone,
        "ExitTimeMonths" integer,
        "HasContinuousAssurance" boolean NOT NULL,
        "HasEvidenceAPI" boolean NOT NULL,
        "ContractEndDate" timestamp with time zone,
        "OwnerId" character varying(100),
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ThirdPartyConcentrations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ThirdPartyConcentrations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UITextEntries" (
        "Id" uuid NOT NULL,
        "TextKey" character varying(100) NOT NULL,
        "Category" character varying(30) NOT NULL,
        "TextEn" character varying(1000) NOT NULL,
        "TextAr" character varying(1000) NOT NULL,
        "UsageNotes" character varying(500),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UITextEntries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UniversalEvidencePacks" (
        "Id" uuid NOT NULL,
        "PackCode" character varying(30) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "ControlFamily" character varying(30) NOT NULL,
        "IconClass" character varying(50),
        "EvidenceItemsJson" text,
        "NamingStandard" character varying(255),
        "StorageLocationPattern" character varying(255),
        "MinimalTestStepsJson" text,
        "SatisfiesFrameworks" character varying(500),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UniversalEvidencePacks" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UserConsents" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "ConsentType" character varying(100) NOT NULL,
        "DocumentVersion" character varying(50) NOT NULL,
        "IsGranted" boolean NOT NULL,
        "ConsentedAt" timestamp with time zone NOT NULL,
        "IpAddress" character varying(50),
        "UserAgent" character varying(500),
        "WithdrawnAt" timestamp with time zone,
        "WithdrawalReason" character varying(500),
        "DocumentHash" character varying(256),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserConsents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserConsents_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserConsents_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UserWorkspaces" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "RoleCode" character varying(100) NOT NULL,
        "RoleName" character varying(255) NOT NULL,
        "RoleNameAr" character varying(255),
        "WorkspaceConfigJson" text,
        "AssignedFrameworks" character varying(500),
        "AssignedAssessmentIds" text,
        "DefaultLandingPage" character varying(255) NOT NULL,
        "QuickActionsJson" text,
        "DashboardWidgetsJson" text,
        "IsConfigured" boolean NOT NULL,
        "LastAccessedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserWorkspaces" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserWorkspaces_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserWorkspaces_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "WorkspaceTemplates" (
        "Id" uuid NOT NULL,
        "RoleCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "DefaultLandingPage" character varying(255) NOT NULL,
        "DashboardWidgetsJson" text,
        "QuickActionsJson" text,
        "MenuItemsJson" text,
        "AssignableTaskTypes" character varying(500),
        "IsDefault" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkspaceTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentActions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AgentId" uuid NOT NULL,
        "ActionCorrelationId" character varying(50) NOT NULL,
        "ActionType" character varying(50) NOT NULL,
        "ActionDescription" character varying(500) NOT NULL,
        "TargetObjectType" character varying(50),
        "TargetObjectId" uuid,
        "InputDataJson" text,
        "OutputDataJson" text,
        "ConfidenceScore" integer,
        "Reasoning" character varying(2000),
        "Status" character varying(20) NOT NULL,
        "RequiredApproval" boolean NOT NULL,
        "ApprovalGateId" uuid,
        "WasApproved" boolean,
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "ApprovalNotes" character varying(1000),
        "ErrorMessage" character varying(2000),
        "DurationMs" integer,
        "TriggeredByActionId" uuid,
        "ExecutedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentActions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentActions_AgentDefinitions_AgentId" FOREIGN KEY ("AgentId") REFERENCES "AgentDefinitions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentApprovalGates" (
        "Id" uuid NOT NULL,
        "AgentId" uuid NOT NULL,
        "GateCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "TriggerActionTypes" character varying(500) NOT NULL,
        "TriggerConditionJson" text,
        "ApproverRoleCode" character varying(50) NOT NULL,
        "ApprovalSLAHours" integer NOT NULL,
        "EscalationRoleCode" character varying(50),
        "AutoRejectHours" integer NOT NULL,
        "BypassConfidenceThreshold" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentApprovalGates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentApprovalGates_AgentDefinitions_AgentId" FOREIGN KEY ("AgentId") REFERENCES "AgentDefinitions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentCapabilities" (
        "Id" uuid NOT NULL,
        "AgentId" uuid NOT NULL,
        "CapabilityCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "Category" character varying(20) NOT NULL,
        "RiskLevel" character varying(20) NOT NULL,
        "RequiresApproval" boolean NOT NULL,
        "MaxUsesPerHour" integer,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentCapabilities" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentCapabilities_AgentDefinitions_AgentId" FOREIGN KEY ("AgentId") REFERENCES "AgentDefinitions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "GeneratedControlSuites" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "SuiteCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "BaselineSetId" uuid NOT NULL,
        "AppliedOverlaysJson" text,
        "TotalControls" integer NOT NULL,
        "MandatoryControls" integer NOT NULL,
        "OptionalControls" integer NOT NULL,
        "GeneratedAt" timestamp with time zone NOT NULL,
        "GeneratedBy" character varying(100),
        "Version" character varying(20) NOT NULL,
        "ProfileSnapshotJson" text,
        "RulesExecutionLogJson" text,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_GeneratedControlSuites" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GeneratedControlSuites_BaselineControlSets_BaselineSetId" FOREIGN KEY ("BaselineSetId") REFERENCES "BaselineControlSets" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_GeneratedControlSuites_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "MAPFrameworkConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ConfigCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Tagline" character varying(500) NOT NULL,
        "BaselineSetId" uuid,
        "ActiveOverlays" character varying(500),
        "GovernanceRhythmJson" text,
        "EvidenceNamingStandard" character varying(500) NOT NULL,
        "Version" character varying(20) NOT NULL,
        "ActivatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MAPFrameworkConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MAPFrameworkConfigs_BaselineControlSets_BaselineSetId" FOREIGN KEY ("BaselineSetId") REFERENCES "BaselineControlSets" ("Id"),
        CONSTRAINT "FK_MAPFrameworkConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlObjectives" (
        "Id" uuid NOT NULL,
        "ObjectiveCode" character varying(30) NOT NULL,
        "DomainId" uuid NOT NULL,
        "ObjectiveStatement" character varying(500) NOT NULL,
        "ObjectiveStatementAr" character varying(500),
        "Description" character varying(2000),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlObjectives" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlObjectives_ControlDomains_DomainId" FOREIGN KEY ("DomainId") REFERENCES "ControlDomains" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ERPExtractConfigs" (
        "Id" uuid NOT NULL,
        "ERPSystemId" uuid NOT NULL,
        "ExtractCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "ProcessArea" character varying(30) NOT NULL,
        "DataSource" character varying(500) NOT NULL,
        "QueryExpression" text,
        "FieldMappingsJson" text,
        "Frequency" character varying(20) NOT NULL,
        "CronExpression" character varying(50),
        "LastExtractAt" timestamp with time zone,
        "LastExtractRecordCount" integer,
        "NextExtractAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ERPExtractConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ERPExtractConfigs_ERPSystemConfigs_ERPSystemId" FOREIGN KEY ("ERPSystemId") REFERENCES "ERPSystemConfigs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SoDRuleDefinitions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "ProcessArea" character varying(30) NOT NULL,
        "RiskLevel" character varying(20) NOT NULL,
        "Function1" character varying(255) NOT NULL,
        "Function1Description" character varying(500),
        "Function1AccessPatternsJson" text,
        "Function2" character varying(255) NOT NULL,
        "Function2Description" character varying(500),
        "Function2AccessPatternsJson" text,
        "BusinessRiskDescription" character varying(1000),
        "MitigatingControls" character varying(1000),
        "ERPSystemId" uuid,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SoDRuleDefinitions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SoDRuleDefinitions_ERPSystemConfigs_ERPSystemId" FOREIGN KEY ("ERPSystemId") REFERENCES "ERPSystemConfigs" ("Id"),
        CONSTRAINT "FK_SoDRuleDefinitions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "EventDeliveryLogs" (
        "Id" uuid NOT NULL,
        "EventId" uuid NOT NULL,
        "SubscriptionId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "AttemptNumber" integer NOT NULL,
        "HttpStatusCode" integer,
        "ResponseBody" character varying(2000),
        "ErrorMessage" character varying(2000),
        "LatencyMs" integer,
        "AttemptedAt" timestamp with time zone NOT NULL,
        "NextRetryAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_EventDeliveryLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EventDeliveryLogs_DomainEvents_EventId" FOREIGN KEY ("EventId") REFERENCES "DomainEvents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_EventDeliveryLogs_EventSubscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "EventSubscriptions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "StandardEvidenceItems" (
        "Id" uuid NOT NULL,
        "FamilyId" uuid NOT NULL,
        "ItemCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "EvidenceType" character varying(30) NOT NULL,
        "RequiredFrequency" character varying(20) NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "SampleFileName" character varying(255),
        "CollectionGuidance" character varying(2000),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_StandardEvidenceItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StandardEvidenceItems_EvidencePackFamilies_FamilyId" FOREIGN KEY ("FamilyId") REFERENCES "EvidencePackFamilies" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CadenceExecutions" (
        "Id" uuid NOT NULL,
        "CadenceId" uuid NOT NULL,
        "ScheduledDate" timestamp with time zone NOT NULL,
        "ExecutedDate" timestamp with time zone,
        "Status" character varying(20) NOT NULL,
        "AttendeesJson" text,
        "ActivitiesCompletedJson" text,
        "DeliverablesProducedJson" text,
        "ActionItemsJson" text,
        "Notes" text,
        "ReminderSent" boolean NOT NULL,
        "ReminderSentAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CadenceExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CadenceExecutions_GovernanceCadences_CadenceId" FOREIGN KEY ("CadenceId") REFERENCES "GovernanceCadences" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "GovernanceRhythmItems" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "ItemCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Frequency" character varying(20) NOT NULL,
        "DurationMinutes" integer NOT NULL,
        "OwnerRoleCode" character varying(50),
        "ParticipantRoleCodes" character varying(255),
        "ActivitiesJson" text,
        "DeliverablesJson" text,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_GovernanceRhythmItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GovernanceRhythmItems_GovernanceRhythmTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "GovernanceRhythmTemplates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "IntegrationHealthMetrics" (
        "Id" uuid NOT NULL,
        "ConnectorId" uuid NOT NULL,
        "MetricType" character varying(30) NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "Value" numeric NOT NULL,
        "Unit" character varying(20) NOT NULL,
        "AlertThreshold" numeric,
        "IsBreaching" boolean NOT NULL,
        "RecordedAt" timestamp with time zone NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_IntegrationHealthMetrics" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IntegrationHealthMetrics_IntegrationConnectors_ConnectorId" FOREIGN KEY ("ConnectorId") REFERENCES "IntegrationConnectors" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SyncJobs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "ConnectorId" uuid NOT NULL,
        "JobCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Direction" character varying(20) NOT NULL,
        "ObjectType" character varying(50) NOT NULL,
        "Frequency" character varying(20) NOT NULL,
        "CronExpression" character varying(50),
        "FieldMappingJson" text,
        "FilterExpression" text,
        "UseUpsert" boolean NOT NULL,
        "LastRunAt" timestamp with time zone,
        "LastRunStatus" character varying(20),
        "LastRunRecordCount" integer,
        "NextRunAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SyncJobs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SyncJobs_IntegrationConnectors_ConnectorId" FOREIGN KEY ("ConnectorId") REFERENCES "IntegrationConnectors" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SupportMessages" (
        "Id" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "SenderType" character varying(20) NOT NULL,
        "SenderId" character varying(100),
        "Content" text NOT NULL,
        "MessageType" character varying(50) NOT NULL,
        "MetadataJson" text,
        "SentAt" timestamp with time zone NOT NULL,
        "ReadAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SupportMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SupportMessages_SupportConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "SupportConversations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UniversalEvidencePackItems" (
        "Id" uuid NOT NULL,
        "PackId" uuid NOT NULL,
        "ItemCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "EvidenceType" character varying(30) NOT NULL,
        "Frequency" character varying(20) NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "CollectionGuidance" character varying(1000),
        "SampleFileName" character varying(255),
        "RetentionMonths" integer NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UniversalEvidencePackItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UniversalEvidencePackItems_UniversalEvidencePacks_PackId" FOREIGN KEY ("PackId") REFERENCES "UniversalEvidencePacks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "UserWorkspaceTasks" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "TitleAr" character varying(255),
        "Description" character varying(1000),
        "TaskType" character varying(50) NOT NULL,
        "RelatedEntityId" uuid,
        "RelatedEntityType" character varying(50),
        "ActionUrl" character varying(500),
        "Priority" integer NOT NULL,
        "DueDate" timestamp with time zone,
        "Status" character varying(50) NOT NULL,
        "FrameworkCode" character varying(50),
        "EstimatedHours" integer,
        "DisplayOrder" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_UserWorkspaceTasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserWorkspaceTasks_UserWorkspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "UserWorkspaces" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentConfidenceScores" (
        "Id" uuid NOT NULL,
        "ActionId" uuid NOT NULL,
        "OverallScore" integer NOT NULL,
        "ConfidenceLevel" character varying(20) NOT NULL,
        "ScoreBreakdownJson" text,
        "LowConfidenceFactorsJson" text,
        "RecommendedAction" character varying(20) NOT NULL,
        "HumanReviewTriggered" boolean NOT NULL,
        "HumanReviewOutcome" character varying(20),
        "HumanReviewerFeedback" character varying(1000),
        "ScoredAt" timestamp with time zone NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentConfidenceScores" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentConfidenceScores_AgentActions_ActionId" FOREIGN KEY ("ActionId") REFERENCES "AgentActions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AgentSoDViolations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "RuleId" uuid NOT NULL,
        "Action1Id" uuid NOT NULL,
        "Action2Id" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "WasBlocked" boolean NOT NULL,
        "OverrideApprovedBy" character varying(100),
        "OverrideReason" character varying(1000),
        "DetectedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AgentSoDViolations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentSoDViolations_AgentActions_Action1Id" FOREIGN KEY ("Action1Id") REFERENCES "AgentActions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AgentSoDViolations_AgentSoDRules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "AgentSoDRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "PendingApprovals" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ActionId" uuid NOT NULL,
        "ApprovalGateId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "AssignedApproverId" character varying(100),
        "AssignedApproverName" character varying(255),
        "DueAt" timestamp with time zone NOT NULL,
        "ReminderSent" boolean NOT NULL,
        "IsEscalated" boolean NOT NULL,
        "EscalatedAt" timestamp with time zone,
        "Decision" character varying(20),
        "DecisionNotes" character varying(2000),
        "DecidedBy" character varying(100),
        "DecidedAt" timestamp with time zone,
        "RequestedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PendingApprovals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PendingApprovals_AgentActions_ActionId" FOREIGN KEY ("ActionId") REFERENCES "AgentActions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_PendingApprovals_AgentApprovalGates_ApprovalGateId" FOREIGN KEY ("ApprovalGateId") REFERENCES "AgentApprovalGates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "OrganizationEntities" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EntityCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "EntityType" character varying(30) NOT NULL,
        "ParentEntityId" uuid,
        "Sectors" character varying(255),
        "Jurisdictions" character varying(255),
        "DataTypes" character varying(255),
        "TechnologyProfile" character varying(100),
        "CriticalityTier" character varying(10) NOT NULL,
        "InheritsFromParent" boolean NOT NULL,
        "AppliedOverlays" character varying(500),
        "GeneratedSuiteId" uuid,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OrganizationEntities" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganizationEntities_GeneratedControlSuites_GeneratedSuiteId" FOREIGN KEY ("GeneratedSuiteId") REFERENCES "GeneratedControlSuites" ("Id"),
        CONSTRAINT "FK_OrganizationEntities_OrganizationEntities_ParentEntityId" FOREIGN KEY ("ParentEntityId") REFERENCES "OrganizationEntities" ("Id"),
        CONSTRAINT "FK_OrganizationEntities_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CanonicalControls" (
        "Id" uuid NOT NULL,
        "ControlId" character varying(30) NOT NULL,
        "ObjectiveId" uuid NOT NULL,
        "ControlName" character varying(255) NOT NULL,
        "ControlNameAr" character varying(255),
        "ControlStatement" character varying(2000) NOT NULL,
        "ControlStatementAr" character varying(2000),
        "ControlType" character varying(20) NOT NULL,
        "ControlNature" character varying(20) NOT NULL,
        "Frequency" character varying(20) NOT NULL,
        "RiskRating" character varying(20) NOT NULL,
        "ImplementationGuidance" character varying(4000),
        "Version" character varying(20) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "SunsetDate" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "ApplicabilityJson" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CanonicalControls" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CanonicalControls_ControlObjectives_ObjectiveId" FOREIGN KEY ("ObjectiveId") REFERENCES "ControlObjectives" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ERPExtractExecutions" (
        "Id" uuid NOT NULL,
        "ExtractConfigId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "RecordsExtracted" integer NOT NULL,
        "RecordsPassedToCCM" integer NOT NULL,
        "StorageLocation" character varying(500),
        "FileHash" character varying(128),
        "DurationSeconds" integer,
        "ErrorMessage" character varying(2000),
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ERPExtractExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ERPExtractExecutions_ERPExtractConfigs_ExtractConfigId" FOREIGN KEY ("ExtractConfigId") REFERENCES "ERPExtractConfigs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SoDConflicts" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "RuleId" uuid NOT NULL,
        "ConflictCode" character varying(50) NOT NULL,
        "UserId" character varying(100) NOT NULL,
        "UserName" character varying(255),
        "UserDepartment" character varying(100),
        "Function1AccessJson" text,
        "Function2AccessJson" text,
        "Status" character varying(20) NOT NULL,
        "HasMitigatingControl" boolean NOT NULL,
        "MitigatingControlDescription" character varying(1000),
        "RiskAcceptanceOwnerId" character varying(100),
        "RiskAcceptanceOwnerName" character varying(255),
        "AcceptanceExpiryDate" timestamp with time zone,
        "ITSMTicketId" character varying(100),
        "DetectedAt" timestamp with time zone NOT NULL,
        "ResolvedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SoDConflicts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SoDConflicts_SoDRuleDefinitions_RuleId" FOREIGN KEY ("RuleId") REFERENCES "SoDRuleDefinitions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_SoDConflicts_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "DeadLetterEntries" (
        "Id" uuid NOT NULL,
        "EventId" uuid,
        "SyncJobId" uuid,
        "EntryType" character varying(20) NOT NULL,
        "OriginalPayloadJson" text NOT NULL,
        "ErrorMessage" character varying(2000) NOT NULL,
        "StackTrace" text,
        "FailureCount" integer NOT NULL,
        "Status" character varying(20) NOT NULL,
        "ResolutionNotes" character varying(2000),
        "ResolvedBy" character varying(100),
        "ResolvedAt" timestamp with time zone,
        "FailedAt" timestamp with time zone NOT NULL,
        "LastRetryAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DeadLetterEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DeadLetterEntries_DomainEvents_EventId" FOREIGN KEY ("EventId") REFERENCES "DomainEvents" ("Id"),
        CONSTRAINT "FK_DeadLetterEntries_SyncJobs_SyncJobId" FOREIGN KEY ("SyncJobId") REFERENCES "SyncJobs" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SyncExecutionLogs" (
        "Id" uuid NOT NULL,
        "SyncJobId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "RecordsProcessed" integer NOT NULL,
        "RecordsCreated" integer NOT NULL,
        "RecordsUpdated" integer NOT NULL,
        "RecordsFailed" integer NOT NULL,
        "RecordsSkipped" integer NOT NULL,
        "ErrorsJson" text,
        "DurationSeconds" integer,
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SyncExecutionLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SyncExecutionLogs_SyncJobs_SyncJobId" FOREIGN KEY ("SyncJobId") REFERENCES "SyncJobs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ApplicabilityEntries" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AssessmentId" uuid,
        "ControlId" uuid NOT NULL,
        "RequirementId" uuid,
        "Status" character varying(20) NOT NULL,
        "Reason" character varying(1000),
        "InheritedFrom" character varying(255),
        "ExceptionReference" character varying(100),
        "ExceptionExpiryDate" timestamp with time zone,
        "JurisdictionDriver" character varying(50),
        "BusinessLineDriver" character varying(100),
        "SystemTierDriver" character varying(20),
        "DataTypeDriver" character varying(100),
        "HostingModelDriver" character varying(20),
        "EvidencePackId" uuid,
        "ControlOwnerId" character varying(100),
        "ControlOwnerName" character varying(255),
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "IsApproved" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApplicabilityEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApplicabilityEntries_Assessments_AssessmentId" FOREIGN KEY ("AssessmentId") REFERENCES "Assessments" ("Id"),
        CONSTRAINT "FK_ApplicabilityEntries_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ApplicabilityEntries_EvidencePacks_EvidencePackId" FOREIGN KEY ("EvidencePackId") REFERENCES "EvidencePacks" ("Id"),
        CONSTRAINT "FK_ApplicabilityEntries_RegulatoryRequirements_RequirementId" FOREIGN KEY ("RequirementId") REFERENCES "RegulatoryRequirements" ("Id"),
        CONSTRAINT "FK_ApplicabilityEntries_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ApplicabilityRules" (
        "Id" uuid NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "RuleType" character varying(20) NOT NULL,
        "Attribute" character varying(50) NOT NULL,
        "Operator" character varying(20) NOT NULL,
        "Value" character varying(500) NOT NULL,
        "Priority" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "ControlId" uuid,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ApplicabilityRules" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApplicabilityRules_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "BaselineControlMappings" (
        "Id" uuid NOT NULL,
        "BaselineSetId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "DefaultParametersJson" text,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_BaselineControlMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BaselineControlMappings_BaselineControlSets_BaselineSetId" FOREIGN KEY ("BaselineSetId") REFERENCES "BaselineControlSets" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_BaselineControlMappings_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CapturedEvidences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EvidenceCode" character varying(50) NOT NULL,
        "SourceIntegrationId" uuid,
        "ControlId" uuid,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "EvidenceTypeCode" character varying(50) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "CollectionMethod" character varying(20) NOT NULL,
        "StorageLocation" character varying(500),
        "FileHash" character varying(128),
        "VersionNumber" integer NOT NULL,
        "IsCurrent" boolean NOT NULL,
        "PreviousVersionId" uuid,
        "TagsJson" text,
        "Status" character varying(20) NOT NULL,
        "OwnerId" character varying(100),
        "OwnerName" character varying(255),
        "ReviewerId" character varying(100),
        "ReviewedAt" timestamp with time zone,
        "RetentionUntil" timestamp with time zone,
        "CapturedAt" timestamp with time zone NOT NULL,
        "CapturedBy" character varying(100) NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CapturedEvidences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CapturedEvidences_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id"),
        CONSTRAINT "FK_CapturedEvidences_EvidenceSourceIntegrations_SourceIntegrat~" FOREIGN KEY ("SourceIntegrationId") REFERENCES "EvidenceSourceIntegrations" ("Id"),
        CONSTRAINT "FK_CapturedEvidences_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CCMControlTests" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TestCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "ControlId" uuid,
        "ProcessArea" character varying(30) NOT NULL,
        "TestCategory" character varying(30) NOT NULL,
        "ERPSystemId" uuid,
        "PopulationDefinitionJson" text NOT NULL,
        "RuleDefinitionJson" text NOT NULL,
        "ThresholdSettingsJson" text,
        "Frequency" character varying(20) NOT NULL,
        "RiskLevel" character varying(20) NOT NULL,
        "ExceptionOwnerRoleCode" character varying(50),
        "ExceptionSLADays" integer NOT NULL,
        "AutoCreateTicket" boolean NOT NULL,
        "SendTeamsNotification" boolean NOT NULL,
        "LastExecutionAt" timestamp with time zone,
        "LastPassRate" numeric,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CCMControlTests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CCMControlTests_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id"),
        CONSTRAINT "FK_CCMControlTests_ERPSystemConfigs_ERPSystemId" FOREIGN KEY ("ERPSystemId") REFERENCES "ERPSystemConfigs" ("Id"),
        CONSTRAINT "FK_CCMControlTests_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ComplianceGuardrails" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "GuardrailCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "GuardrailType" character varying(30) NOT NULL,
        "EnforcementPoint" character varying(30) NOT NULL,
        "RuleDefinitionJson" text,
        "EnforcementMode" character varying(20) NOT NULL,
        "ControlId" uuid,
        "LastEvaluatedAt" timestamp with time zone,
        "LastEvaluationResult" character varying(20),
        "ViolationsCount" integer,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ComplianceGuardrails" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ComplianceGuardrails_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id"),
        CONSTRAINT "FK_ComplianceGuardrails_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlEvidencePacks" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "EvidencePackId" uuid NOT NULL,
        "IsPrimary" boolean NOT NULL,
        "Notes" character varying(500),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlEvidencePacks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlEvidencePacks_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ControlEvidencePacks_EvidencePacks_EvidencePackId" FOREIGN KEY ("EvidencePackId") REFERENCES "EvidencePacks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlExceptions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ExceptionCode" character varying(50) NOT NULL,
        "ControlId" uuid NOT NULL,
        "Scope" character varying(1000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "RiskImpact" character varying(10) NOT NULL,
        "CompensatingControls" character varying(2000),
        "RemediationPlan" character varying(2000),
        "TargetRemediationDate" timestamp with time zone,
        "Status" character varying(20) NOT NULL,
        "ExpiryDate" timestamp with time zone NOT NULL,
        "RiskAcceptanceOwnerId" character varying(100),
        "RiskAcceptanceOwnerName" character varying(255),
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "LastReviewDate" timestamp with time zone,
        "NextReviewDate" timestamp with time zone,
        "ReviewFrequency" character varying(20) NOT NULL,
        "ExpiryReminderSent" boolean NOT NULL,
        "RequestedAt" timestamp with time zone NOT NULL,
        "RequestedBy" character varying(100),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlExceptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlExceptions_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ControlExceptions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "ControlTestProcedures" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "TestProcedureId" uuid NOT NULL,
        "IsPrimary" boolean NOT NULL,
        "Notes" character varying(500),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ControlTestProcedures" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlTestProcedures_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ControlTestProcedures_TestProcedures_TestProcedureId" FOREIGN KEY ("TestProcedureId") REFERENCES "TestProcedures" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "OverlayControlMappings" (
        "Id" uuid NOT NULL,
        "OverlayId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "Action" character varying(20) NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "Reason" character varying(500),
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OverlayControlMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OverlayControlMappings_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_OverlayControlMappings_OverlayCatalogs_OverlayId" FOREIGN KEY ("OverlayId") REFERENCES "OverlayCatalogs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "OverlayParameterOverrides" (
        "Id" uuid NOT NULL,
        "OverlayId" uuid NOT NULL,
        "ControlId" uuid,
        "ParameterName" character varying(100) NOT NULL,
        "OriginalValue" character varying(255),
        "OverrideValue" character varying(255) NOT NULL,
        "Reason" character varying(500),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OverlayParameterOverrides" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OverlayParameterOverrides_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id"),
        CONSTRAINT "FK_OverlayParameterOverrides_OverlayCatalogs_OverlayId" FOREIGN KEY ("OverlayId") REFERENCES "OverlayCatalogs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "PlainLanguageControls" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "PlainStatement" character varying(500) NOT NULL,
        "PlainStatementAr" character varying(500),
        "WhoPerforms" character varying(100) NOT NULL,
        "HowOften" character varying(50) NOT NULL,
        "WhatProvesIt" character varying(500) NOT NULL,
        "PassCriteria" character varying(500) NOT NULL,
        "FailCriteria" character varying(500),
        "FullExample" character varying(1000),
        "IsApproved" boolean NOT NULL,
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PlainLanguageControls" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlainLanguageControls_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RequirementMappings" (
        "Id" uuid NOT NULL,
        "RequirementId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "ObjectiveId" uuid,
        "MappingType" character varying(20) NOT NULL,
        "ConfidenceLevel" integer NOT NULL,
        "Rationale" character varying(1000),
        "ApprovedBy" character varying(100),
        "ApprovedAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RequirementMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequirementMappings_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequirementMappings_ControlObjectives_ObjectiveId" FOREIGN KEY ("ObjectiveId") REFERENCES "ControlObjectives" ("Id"),
        CONSTRAINT "FK_RequirementMappings_RegulatoryRequirements_RequirementId" FOREIGN KEY ("RequirementId") REFERENCES "RegulatoryRequirements" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RiskIndicators" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "IndicatorCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "Description" character varying(1000),
        "IndicatorType" character varying(20) NOT NULL,
        "Category" character varying(30) NOT NULL,
        "ControlId" uuid,
        "DataSource" character varying(50),
        "MeasurementFrequency" character varying(20) NOT NULL,
        "UnitOfMeasure" character varying(20) NOT NULL,
        "TargetValue" numeric,
        "WarningThreshold" numeric,
        "CriticalThreshold" numeric,
        "Direction" character varying(20) NOT NULL,
        "OwnerRoleCode" character varying(50),
        "AutoEscalate" boolean NOT NULL,
        "EscalationDays" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RiskIndicators" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskIndicators_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id"),
        CONSTRAINT "FK_RiskIndicators_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SuiteControlEntries" (
        "Id" uuid NOT NULL,
        "SuiteId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "Source" character varying(20) NOT NULL,
        "SourceOverlayCode" character varying(30),
        "IsMandatory" boolean NOT NULL,
        "AppliedParametersJson" text,
        "InclusionReason" character varying(500),
        "AssignedOwnerRoleCode" character varying(50),
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SuiteControlEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SuiteControlEntries_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_SuiteControlEntries_GeneratedControlSuites_SuiteId" FOREIGN KEY ("SuiteId") REFERENCES "GeneratedControlSuites" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "SuiteEvidenceRequests" (
        "Id" uuid NOT NULL,
        "SuiteId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "EvidencePackId" uuid,
        "EvidenceItemCode" character varying(50) NOT NULL,
        "EvidenceItemName" character varying(255) NOT NULL,
        "RequiredFrequency" character varying(20) NOT NULL,
        "RetentionMonths" integer NOT NULL,
        "AssignedOwnerId" character varying(100),
        "AssignedOwnerName" character varying(255),
        "Status" character varying(20) NOT NULL,
        "DueDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SuiteEvidenceRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SuiteEvidenceRequests_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_SuiteEvidenceRequests_EvidencePacks_EvidencePackId" FOREIGN KEY ("EvidencePackId") REFERENCES "EvidencePacks" ("Id"),
        CONSTRAINT "FK_SuiteEvidenceRequests_GeneratedControlSuites_SuiteId" FOREIGN KEY ("SuiteId") REFERENCES "GeneratedControlSuites" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CCMTestExecutions" (
        "Id" uuid NOT NULL,
        "TestId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "PopulationCount" integer NOT NULL,
        "PassedCount" integer NOT NULL,
        "FailedCount" integer NOT NULL,
        "PassRate" numeric NOT NULL,
        "ResultStatus" character varying(20) NOT NULL,
        "EvidenceSnapshotLocation" character varying(500),
        "EvidenceSnapshotHash" character varying(128),
        "DurationSeconds" integer,
        "ErrorMessage" character varying(2000),
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CCMTestExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CCMTestExecutions_CCMControlTests_TestId" FOREIGN KEY ("TestId") REFERENCES "CCMControlTests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "MappingQualityGates" (
        "Id" uuid NOT NULL,
        "MappingId" uuid NOT NULL,
        "CoverageStatement" character varying(2000),
        "HasCoverageStatement" boolean NOT NULL,
        "HasEvidenceLinkage" boolean NOT NULL,
        "EvidenceLinkageNotes" character varying(500),
        "TestMethod" character varying(2000),
        "HasTestMethod" boolean NOT NULL,
        "GapStatus" character varying(20) NOT NULL,
        "RemediationRequired" character varying(2000),
        "ConfidenceRating" character varying(10) NOT NULL,
        "QualityScore" integer NOT NULL,
        "PassedQualityGate" boolean NOT NULL,
        "ReviewedBy" character varying(100),
        "ReviewedAt" timestamp with time zone,
        "ReviewNotes" character varying(2000),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MappingQualityGates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MappingQualityGates_RequirementMappings_MappingId" FOREIGN KEY ("MappingId") REFERENCES "RequirementMappings" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "MappingWorkflowSteps" (
        "Id" uuid NOT NULL,
        "MappingId" uuid NOT NULL,
        "StepNumber" integer NOT NULL,
        "RoleCode" character varying(30) NOT NULL,
        "RaciType" character varying(15) NOT NULL,
        "StepName" character varying(100) NOT NULL,
        "StepDescription" character varying(500),
        "AssignedToUserId" character varying(100),
        "AssignedToUserName" character varying(255),
        "Status" character varying(20) NOT NULL,
        "Decision" character varying(20),
        "Comments" character varying(2000),
        "StartedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "DueDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MappingWorkflowSteps" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MappingWorkflowSteps_RequirementMappings_MappingId" FOREIGN KEY ("MappingId") REFERENCES "RequirementMappings" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RiskIndicatorMeasurements" (
        "Id" uuid NOT NULL,
        "IndicatorId" uuid NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "Value" numeric NOT NULL,
        "Target" numeric,
        "Status" character varying(10) NOT NULL,
        "Trend" character varying(20),
        "Source" character varying(20) NOT NULL,
        "RawDataJson" text,
        "Commentary" character varying(1000),
        "MeasuredAt" timestamp with time zone NOT NULL,
        "MeasuredBy" character varying(100),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RiskIndicatorMeasurements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskIndicatorMeasurements_RiskIndicators_IndicatorId" FOREIGN KEY ("IndicatorId") REFERENCES "RiskIndicators" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "AutoTaggedEvidences" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "Process" character varying(30) NOT NULL,
        "System" character varying(50) NOT NULL,
        "Period" character varying(20) NOT NULL,
        "PeriodStart" timestamp with time zone NOT NULL,
        "PeriodEnd" timestamp with time zone NOT NULL,
        "OwnerId" character varying(100),
        "OwnerName" character varying(255),
        "EvidenceType" character varying(30) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "StorageLocation" character varying(500) NOT NULL,
        "FileHash" character varying(128),
        "FileSizeBytes" bigint,
        "MimeType" character varying(100),
        "VersionNumber" integer NOT NULL,
        "IsCurrent" boolean NOT NULL,
        "Source" character varying(20) NOT NULL,
        "CCMTestExecutionId" uuid,
        "Status" character varying(20) NOT NULL,
        "ReviewedBy" character varying(100),
        "ReviewedAt" timestamp with time zone,
        "RetentionUntil" timestamp with time zone,
        "CapturedAt" timestamp with time zone NOT NULL,
        "CapturedBy" character varying(100) NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AutoTaggedEvidences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AutoTaggedEvidences_CCMTestExecutions_CCMTestExecutionId" FOREIGN KEY ("CCMTestExecutionId") REFERENCES "CCMTestExecutions" ("Id"),
        CONSTRAINT "FK_AutoTaggedEvidences_CanonicalControls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "CanonicalControls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AutoTaggedEvidences_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "CCMExceptions" (
        "Id" uuid NOT NULL,
        "TestExecutionId" uuid NOT NULL,
        "ExceptionCode" character varying(50) NOT NULL,
        "ExceptionType" character varying(30) NOT NULL,
        "Severity" character varying(20) NOT NULL,
        "Summary" character varying(500) NOT NULL,
        "Details" character varying(2000),
        "AffectedEntity" character varying(255),
        "AffectedEntityId" character varying(100),
        "TransactionReference" character varying(100),
        "AmountInvolved" numeric,
        "Currency" character varying(10),
        "RawDataJson" text,
        "Status" character varying(20) NOT NULL,
        "AssignedToId" character varying(100),
        "AssignedToName" character varying(255),
        "ITSMTicketId" character varying(100),
        "ITSMTicketUrl" character varying(500),
        "TeamsNotificationSent" boolean NOT NULL,
        "DueDate" timestamp with time zone,
        "ResolutionNotes" character varying(2000),
        "ResolvedBy" character varying(100),
        "ResolvedAt" timestamp with time zone,
        "DetectedAt" timestamp with time zone NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CCMExceptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CCMExceptions_CCMTestExecutions_TestExecutionId" FOREIGN KEY ("TestExecutionId") REFERENCES "CCMTestExecutions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE TABLE "RiskIndicatorAlerts" (
        "Id" uuid NOT NULL,
        "IndicatorId" uuid NOT NULL,
        "MeasurementId" uuid,
        "Severity" character varying(20) NOT NULL,
        "Message" character varying(500) NOT NULL,
        "ThresholdValue" numeric,
        "ActualValue" numeric,
        "Status" character varying(20) NOT NULL,
        "DaysInBreach" integer NOT NULL,
        "IsEscalated" boolean NOT NULL,
        "EscalatedAt" timestamp with time zone,
        "EscalatedTo" character varying(100),
        "AssignedTo" character varying(100),
        "AcknowledgedAt" timestamp with time zone,
        "AcknowledgedBy" character varying(100),
        "ResolvedAt" timestamp with time zone,
        "ResolvedBy" character varying(100),
        "ResolutionNotes" character varying(1000),
        "TriggeredAt" timestamp with time zone NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RiskIndicatorAlerts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskIndicatorAlerts_RiskIndicatorMeasurements_MeasurementId" FOREIGN KEY ("MeasurementId") REFERENCES "RiskIndicatorMeasurements" ("Id"),
        CONSTRAINT "FK_RiskIndicatorAlerts_RiskIndicators_IndicatorId" FOREIGN KEY ("IndicatorId") REFERENCES "RiskIndicators" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_TaskDelegations_DelegatedAt" ON "TaskDelegations" ("DelegatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_TaskDelegations_TenantId_IsActive_IsRevoked" ON "TaskDelegations" ("TenantId", "IsActive", "IsRevoked");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_TaskDelegations_TenantId_TaskId" ON "TaskDelegations" ("TenantId", "TaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentActions_AgentId" ON "AgentActions" ("AgentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentApprovalGates_AgentId" ON "AgentApprovalGates" ("AgentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentCapabilities_AgentId" ON "AgentCapabilities" ("AgentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentConfidenceScores_ActionId" ON "AgentConfidenceScores" ("ActionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentSoDViolations_Action1Id" ON "AgentSoDViolations" ("Action1Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AgentSoDViolations_RuleId" ON "AgentSoDViolations" ("RuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityEntries_AssessmentId" ON "ApplicabilityEntries" ("AssessmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityEntries_ControlId" ON "ApplicabilityEntries" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityEntries_EvidencePackId" ON "ApplicabilityEntries" ("EvidencePackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityEntries_RequirementId" ON "ApplicabilityEntries" ("RequirementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityEntries_TenantId" ON "ApplicabilityEntries" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ApplicabilityRules_ControlId" ON "ApplicabilityRules" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AssessmentScopes_AssessmentId" ON "AssessmentScopes" ("AssessmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AssessmentScopes_TenantId" ON "AssessmentScopes" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AutoTaggedEvidences_CCMTestExecutionId" ON "AutoTaggedEvidences" ("CCMTestExecutionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AutoTaggedEvidences_ControlId" ON "AutoTaggedEvidences" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_AutoTaggedEvidences_TenantId" ON "AutoTaggedEvidences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_BaselineControlMappings_BaselineSetId" ON "BaselineControlMappings" ("BaselineSetId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_BaselineControlMappings_ControlId" ON "BaselineControlMappings" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CadenceExecutions_CadenceId" ON "CadenceExecutions" ("CadenceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CanonicalControls_ObjectiveId" ON "CanonicalControls" ("ObjectiveId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CapturedEvidences_ControlId" ON "CapturedEvidences" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CapturedEvidences_SourceIntegrationId" ON "CapturedEvidences" ("SourceIntegrationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CapturedEvidences_TenantId" ON "CapturedEvidences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CCMControlTests_ControlId" ON "CCMControlTests" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CCMControlTests_ERPSystemId" ON "CCMControlTests" ("ERPSystemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CCMControlTests_TenantId" ON "CCMControlTests" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CCMExceptions_TestExecutionId" ON "CCMExceptions" ("TestExecutionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CCMTestExecutions_TestId" ON "CCMTestExecutions" ("TestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ComplianceGuardrails_ControlId" ON "ComplianceGuardrails" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ComplianceGuardrails_TenantId" ON "ComplianceGuardrails" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlEvidencePacks_ControlId" ON "ControlEvidencePacks" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlEvidencePacks_EvidencePackId" ON "ControlEvidencePacks" ("EvidencePackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlExceptions_ControlId" ON "ControlExceptions" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlExceptions_TenantId" ON "ControlExceptions" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlObjectives_DomainId" ON "ControlObjectives" ("DomainId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlTestProcedures_ControlId" ON "ControlTestProcedures" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ControlTestProcedures_TestProcedureId" ON "ControlTestProcedures" ("TestProcedureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_CryptographicAssets_TenantId" ON "CryptographicAssets" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_DeadLetterEntries_EventId" ON "DeadLetterEntries" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_DeadLetterEntries_SyncJobId" ON "DeadLetterEntries" ("SyncJobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ERPExtractConfigs_ERPSystemId" ON "ERPExtractConfigs" ("ERPSystemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ERPExtractExecutions_ExtractConfigId" ON "ERPExtractExecutions" ("ExtractConfigId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ERPSystemConfigs_TenantId" ON "ERPSystemConfigs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_EventDeliveryLogs_EventId" ON "EventDeliveryLogs" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_EventDeliveryLogs_SubscriptionId" ON "EventDeliveryLogs" ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_EvidenceSourceIntegrations_TenantId" ON "EvidenceSourceIntegrations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_GeneratedControlSuites_BaselineSetId" ON "GeneratedControlSuites" ("BaselineSetId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_GeneratedControlSuites_TenantId" ON "GeneratedControlSuites" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_GovernanceCadences_TenantId" ON "GovernanceCadences" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_GovernanceRhythmItems_TemplateId" ON "GovernanceRhythmItems" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ImportantBusinessServices_TenantId" ON "ImportantBusinessServices" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_IntegrationHealthMetrics_ConnectorId" ON "IntegrationHealthMetrics" ("ConnectorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_MAPFrameworkConfigs_BaselineSetId" ON "MAPFrameworkConfigs" ("BaselineSetId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_MAPFrameworkConfigs_TenantId" ON "MAPFrameworkConfigs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_MappingQualityGates_MappingId" ON "MappingQualityGates" ("MappingId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_MappingWorkflowSteps_MappingId" ON "MappingWorkflowSteps" ("MappingId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OnePageGuides_TenantId" ON "OnePageGuides" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OrganizationEntities_GeneratedSuiteId" ON "OrganizationEntities" ("GeneratedSuiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OrganizationEntities_ParentEntityId" ON "OrganizationEntities" ("ParentEntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OrganizationEntities_TenantId" ON "OrganizationEntities" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OverlayControlMappings_ControlId" ON "OverlayControlMappings" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OverlayControlMappings_OverlayId" ON "OverlayControlMappings" ("OverlayId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OverlayParameterOverrides_ControlId" ON "OverlayParameterOverrides" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_OverlayParameterOverrides_OverlayId" ON "OverlayParameterOverrides" ("OverlayId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_PendingApprovals_ActionId" ON "PendingApprovals" ("ActionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_PendingApprovals_ApprovalGateId" ON "PendingApprovals" ("ApprovalGateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_PlainLanguageControls_ControlId" ON "PlainLanguageControls" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RequirementMappings_ControlId" ON "RequirementMappings" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RequirementMappings_ObjectiveId" ON "RequirementMappings" ("ObjectiveId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RequirementMappings_RequirementId" ON "RequirementMappings" ("RequirementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RiskIndicatorAlerts_IndicatorId" ON "RiskIndicatorAlerts" ("IndicatorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RiskIndicatorAlerts_MeasurementId" ON "RiskIndicatorAlerts" ("MeasurementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RiskIndicatorMeasurements_IndicatorId" ON "RiskIndicatorMeasurements" ("IndicatorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RiskIndicators_ControlId" ON "RiskIndicators" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_RiskIndicators_TenantId" ON "RiskIndicators" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ShahinAIBrandConfigs_TenantId" ON "ShahinAIBrandConfigs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SoDConflicts_RuleId" ON "SoDConflicts" ("RuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SoDConflicts_TenantId" ON "SoDConflicts" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SoDRuleDefinitions_ERPSystemId" ON "SoDRuleDefinitions" ("ERPSystemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SoDRuleDefinitions_TenantId" ON "SoDRuleDefinitions" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_StandardEvidenceItems_FamilyId" ON "StandardEvidenceItems" ("FamilyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_StrategicRoadmapMilestones_TenantId" ON "StrategicRoadmapMilestones" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SuiteControlEntries_ControlId" ON "SuiteControlEntries" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SuiteControlEntries_SuiteId" ON "SuiteControlEntries" ("SuiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SuiteEvidenceRequests_ControlId" ON "SuiteEvidenceRequests" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SuiteEvidenceRequests_EvidencePackId" ON "SuiteEvidenceRequests" ("EvidencePackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SuiteEvidenceRequests_SuiteId" ON "SuiteEvidenceRequests" ("SuiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SupportMessages_ConversationId" ON "SupportMessages" ("ConversationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SyncExecutionLogs_SyncJobId" ON "SyncExecutionLogs" ("SyncJobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_SyncJobs_ConnectorId" ON "SyncJobs" ("ConnectorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_TeamsNotificationConfigs_TenantId" ON "TeamsNotificationConfigs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_ThirdPartyConcentrations_TenantId" ON "ThirdPartyConcentrations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UniversalEvidencePackItems_PackId" ON "UniversalEvidencePackItems" ("PackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UserConsents_TenantId" ON "UserConsents" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UserConsents_UserId" ON "UserConsents" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UserWorkspaces_TenantId" ON "UserWorkspaces" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UserWorkspaces_UserId" ON "UserWorkspaces" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    CREATE INDEX "IX_UserWorkspaceTasks_WorkspaceId" ON "UserWorkspaceTasks" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260105205647_ShahinAIPlatform') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260105205647_ShahinAIPlatform', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE TABLE "OnboardingWizards" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "OrganizationLegalNameEn" character varying(255) NOT NULL,
        "OrganizationLegalNameAr" character varying(255) NOT NULL,
        "TradeName" character varying(255) NOT NULL,
        "CountryOfIncorporation" character varying(10) NOT NULL,
        "OperatingCountriesJson" text NOT NULL,
        "PrimaryHqLocation" character varying(255) NOT NULL,
        "DefaultTimezone" character varying(50) NOT NULL,
        "PrimaryLanguage" character varying(20) NOT NULL,
        "CorporateEmailDomainsJson" text NOT NULL,
        "DomainVerificationMethod" character varying(50) NOT NULL,
        "OrganizationType" character varying(50) NOT NULL,
        "IndustrySector" character varying(100) NOT NULL,
        "BusinessLinesJson" text NOT NULL,
        "HasDataResidencyRequirement" boolean NOT NULL,
        "DataResidencyCountriesJson" text NOT NULL,
        "PrimaryDriver" character varying(100) NOT NULL,
        "TargetTimeline" timestamp with time zone,
        "CurrentPainPointsJson" text NOT NULL,
        "DesiredMaturity" character varying(50) NOT NULL,
        "ReportingAudienceJson" text NOT NULL,
        "PrimaryRegulatorsJson" text NOT NULL,
        "SecondaryRegulatorsJson" text NOT NULL,
        "MandatoryFrameworksJson" text NOT NULL,
        "OptionalFrameworksJson" text NOT NULL,
        "InternalPoliciesJson" text NOT NULL,
        "CertificationsHeldJson" text NOT NULL,
        "AuditScopeType" character varying(50) NOT NULL,
        "InScopeLegalEntitiesJson" text NOT NULL,
        "InScopeBusinessUnitsJson" text NOT NULL,
        "InScopeSystemsJson" text NOT NULL,
        "InScopeProcessesJson" text NOT NULL,
        "InScopeEnvironments" character varying(50) NOT NULL,
        "InScopeLocationsJson" text NOT NULL,
        "SystemCriticalityTiersJson" text NOT NULL,
        "ImportantBusinessServicesJson" text NOT NULL,
        "ExclusionsJson" text NOT NULL,
        "DataTypesProcessedJson" text NOT NULL,
        "HasPaymentCardData" boolean NOT NULL,
        "PaymentCardDataLocationsJson" text NOT NULL,
        "HasCrossBorderDataTransfers" boolean NOT NULL,
        "CrossBorderTransferCountriesJson" text NOT NULL,
        "CustomerVolumeTier" character varying(50) NOT NULL,
        "TransactionVolumeTier" character varying(50) NOT NULL,
        "HasInternetFacingSystems" boolean NOT NULL,
        "InternetFacingSystemsJson" text NOT NULL,
        "HasThirdPartyDataProcessing" boolean NOT NULL,
        "ThirdPartyDataProcessorsJson" text NOT NULL,
        "IdentityProvider" character varying(100) NOT NULL,
        "SsoEnabled" boolean NOT NULL,
        "ScimProvisioningAvailable" boolean NOT NULL,
        "ItsmPlatform" character varying(100) NOT NULL,
        "EvidenceRepository" character varying(100) NOT NULL,
        "SiemPlatform" character varying(100) NOT NULL,
        "VulnerabilityManagementTool" character varying(100) NOT NULL,
        "EdrPlatform" character varying(100) NOT NULL,
        "CloudProvidersJson" text NOT NULL,
        "ErpSystem" character varying(100) NOT NULL,
        "CmdbSource" character varying(100) NOT NULL,
        "CiCdTooling" character varying(100) NOT NULL,
        "BackupDrTooling" character varying(100) NOT NULL,
        "ControlOwnershipApproach" character varying(50) NOT NULL,
        "DefaultControlOwnerTeam" character varying(100) NOT NULL,
        "ExceptionApproverRole" character varying(100) NOT NULL,
        "RegulatoryInterpretationApproverRole" character varying(100) NOT NULL,
        "ControlEffectivenessSignoffRole" character varying(100) NOT NULL,
        "InternalAuditStakeholder" character varying(255) NOT NULL,
        "RiskCommitteeCadence" character varying(50) NOT NULL,
        "RiskCommitteeAttendeesJson" text NOT NULL,
        "OrgAdminsJson" text NOT NULL,
        "CreateTeamsNow" boolean NOT NULL,
        "TeamListJson" text NOT NULL,
        "TeamMembersJson" text NOT NULL,
        "SelectedRoleCatalogJson" text NOT NULL,
        "RaciMappingNeeded" boolean NOT NULL,
        "RaciMappingJson" text NOT NULL,
        "ApprovalGatesNeeded" boolean NOT NULL,
        "ApprovalGatesJson" text NOT NULL,
        "DelegationRulesJson" text NOT NULL,
        "NotificationPreference" character varying(50) NOT NULL,
        "EscalationDaysOverdue" integer NOT NULL,
        "EscalationTarget" character varying(100) NOT NULL,
        "EvidenceFrequencyDefaultsJson" text NOT NULL,
        "AccessReviewsFrequency" character varying(50) NOT NULL,
        "VulnerabilityPatchReviewFrequency" character varying(50) NOT NULL,
        "BackupReviewFrequency" character varying(50) NOT NULL,
        "RestoreTestCadence" character varying(50) NOT NULL,
        "DrExerciseCadence" character varying(50) NOT NULL,
        "IncidentTabletopCadence" character varying(50) NOT NULL,
        "EvidenceSlaSubmitDays" integer NOT NULL,
        "RemediationSlaJson" text NOT NULL,
        "ExceptionExpiryDays" integer NOT NULL,
        "AuditRequestHandling" character varying(50) NOT NULL,
        "EvidenceNamingConventionRequired" boolean NOT NULL,
        "EvidenceNamingPattern" character varying(255) NOT NULL,
        "EvidenceStorageLocationJson" text NOT NULL,
        "EvidenceRetentionYears" integer NOT NULL,
        "EvidenceAccessRulesJson" text NOT NULL,
        "AcceptableEvidenceTypesJson" text NOT NULL,
        "SamplingGuidanceJson" text NOT NULL,
        "ConfidentialEvidenceEncryption" boolean NOT NULL,
        "ConfidentialEvidenceAccessJson" text NOT NULL,
        "AdoptDefaultBaseline" boolean NOT NULL,
        "SelectedOverlaysJson" text NOT NULL,
        "HasClientSpecificControls" boolean NOT NULL,
        "ClientSpecificControlsJson" text NOT NULL,
        "SuccessMetricsTop3Json" text NOT NULL,
        "BaselineAuditPrepHoursPerMonth" numeric,
        "BaselineRemediationClosureDays" numeric,
        "BaselineOverdueControlsPerMonth" integer,
        "TargetImprovementJson" text NOT NULL,
        "PilotScopeJson" text NOT NULL,
        "CurrentStep" integer NOT NULL,
        "WizardStatus" character varying(50) NOT NULL,
        "ProgressPercent" integer NOT NULL,
        "CompletedSectionsJson" text NOT NULL,
        "ValidationErrorsJson" text NOT NULL,
        "StartedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "CompletedByUserId" character varying(100) NOT NULL,
        "LastStepSavedAt" timestamp with time zone,
        "AllAnswersJson" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OnboardingWizards" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OnboardingWizards_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE TABLE "Teams" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TeamCode" text NOT NULL,
        "Name" text NOT NULL,
        "NameAr" text NOT NULL,
        "Purpose" text NOT NULL,
        "Description" text NOT NULL,
        "TeamType" text NOT NULL,
        "BusinessUnit" text NOT NULL,
        "ManagerUserId" uuid,
        "IsDefaultFallback" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Teams" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Teams_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE TABLE "Assets" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AssetCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "AssetType" text NOT NULL,
        "SubType" text NOT NULL,
        "SystemId" text NOT NULL,
        "SourceSystem" text NOT NULL,
        "Criticality" text NOT NULL,
        "DataClassification" text NOT NULL,
        "DataTypes" text NOT NULL,
        "OwnerUserId" uuid,
        "OwnerTeamId" uuid,
        "BusinessOwner" text NOT NULL,
        "TechnicalOwner" text NOT NULL,
        "HostingModel" text NOT NULL,
        "CloudProvider" text NOT NULL,
        "Environment" text NOT NULL,
        "Location" text NOT NULL,
        "TagsJson" text NOT NULL,
        "AttributesJson" text NOT NULL,
        "IsInScope" boolean NOT NULL,
        "ApplicableFrameworks" text NOT NULL,
        "LastRiskAssessmentDate" timestamp with time zone,
        "RiskScore" integer,
        "CommissionedDate" timestamp with time zone,
        "DecommissionedDate" timestamp with time zone,
        "Status" text NOT NULL,
        "LastSyncDate" timestamp with time zone,
        "LastSyncStatus" text NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Assets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Assets_Teams_OwnerTeamId" FOREIGN KEY ("OwnerTeamId") REFERENCES "Teams" ("Id"),
        CONSTRAINT "FK_Assets_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE TABLE "RACIAssignments" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ScopeType" text NOT NULL,
        "ScopeId" text NOT NULL,
        "TeamId" uuid NOT NULL,
        "RACI" text NOT NULL,
        "RoleCode" text,
        "Priority" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RACIAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RACIAssignments_Teams_TeamId" FOREIGN KEY ("TeamId") REFERENCES "Teams" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE TABLE "TeamMembers" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TeamId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleCode" text NOT NULL,
        "IsPrimaryForRole" boolean NOT NULL,
        "CanApprove" boolean NOT NULL,
        "CanDelegate" boolean NOT NULL,
        "JoinedDate" timestamp with time zone NOT NULL,
        "LeftDate" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TeamMembers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TeamMembers_Teams_TeamId" FOREIGN KEY ("TeamId") REFERENCES "Teams" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TeamMembers_TenantUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "TenantUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_Assets_OwnerTeamId" ON "Assets" ("OwnerTeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_Assets_TenantId" ON "Assets" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_OnboardingWizards_CurrentStep" ON "OnboardingWizards" ("CurrentStep");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE UNIQUE INDEX "IX_OnboardingWizards_TenantId" ON "OnboardingWizards" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_OnboardingWizards_WizardStatus" ON "OnboardingWizards" ("WizardStatus");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_RACIAssignments_TeamId" ON "RACIAssignments" ("TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_TeamMembers_TeamId" ON "TeamMembers" ("TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_TeamMembers_UserId" ON "TeamMembers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    CREATE INDEX "IX_Teams_TenantId" ON "Teams" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106080415_AddMissingOnboardingTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106080415_AddMissingOnboardingTables', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106102234_OnboardingGRCIntegration') THEN
    CREATE TABLE "SerialNumberCounters" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EntityType" character varying(50) NOT NULL,
        "DateKey" character varying(8) NOT NULL,
        "LastSequence" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        CONSTRAINT "PK_SerialNumberCounters" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106102234_OnboardingGRCIntegration') THEN
    CREATE TABLE "TenantWorkflowConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkflowCode" character varying(50) NOT NULL,
        "WorkflowName" character varying(200) NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "ActivatedAt" timestamp with time zone,
        "ActivatedBy" character varying(100),
        "DeactivatedAt" timestamp with time zone,
        "DeactivatedBy" character varying(100),
        "SlaMultiplier" numeric(5,2) NOT NULL,
        "CustomConfigJson" text,
        "NotificationOverridesJson" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TenantWorkflowConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantWorkflowConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106102234_OnboardingGRCIntegration') THEN
    CREATE UNIQUE INDEX "IX_SerialNumberCounters_TenantId_EntityType_DateKey" ON "SerialNumberCounters" ("TenantId", "EntityType", "DateKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106102234_OnboardingGRCIntegration') THEN
    CREATE UNIQUE INDEX "IX_TenantWorkflowConfigs_TenantId_WorkflowCode" ON "TenantWorkflowConfigs" ("TenantId", "WorkflowCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106102234_OnboardingGRCIntegration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106102234_OnboardingGRCIntegration', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106103024_PolicyDecisionAuditTrail') THEN
    CREATE TABLE "PolicyDecisions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PolicyType" character varying(50) NOT NULL,
        "PolicyVersion" character varying(20) NOT NULL,
        "ContextHash" character varying(50) NOT NULL,
        "ContextJson" text NOT NULL,
        "Decision" character varying(50) NOT NULL,
        "Reason" character varying(1000) NOT NULL,
        "RulesEvaluated" integer NOT NULL,
        "RulesMatched" integer NOT NULL,
        "ConfidenceScore" integer NOT NULL,
        "EvaluatedAt" timestamp with time zone NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "IsCached" boolean NOT NULL,
        "RelatedEntityType" character varying(100),
        "RelatedEntityId" uuid,
        "EvaluatedBy" character varying(100) NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_PolicyDecisions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PolicyDecisions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106103024_PolicyDecisionAuditTrail') THEN
    CREATE INDEX "IX_PolicyDecisions_ContextHash" ON "PolicyDecisions" ("ContextHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106103024_PolicyDecisionAuditTrail') THEN
    CREATE INDEX "IX_PolicyDecisions_EvaluatedAt" ON "PolicyDecisions" ("EvaluatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106103024_PolicyDecisionAuditTrail') THEN
    CREATE INDEX "IX_PolicyDecisions_TenantId_PolicyType_EvaluatedAt" ON "PolicyDecisions" ("TenantId", "PolicyType", "EvaluatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106103024_PolicyDecisionAuditTrail') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106103024_PolicyDecisionAuditTrail', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106105748_PocSeederSupport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106105748_PocSeederSupport', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "VulnerabilityManagementTool" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "SiemPlatform" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "ItsmPlatform" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "IdentityProvider" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "EvidenceRepository" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "ErpSystem" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "EdrPlatform" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "CmdbSource" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "CiCdTooling" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    ALTER TABLE "OnboardingWizards" ALTER COLUMN "BackupDrTooling" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106110541_FixOnboardingWizardDefaults') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106110541_FixOnboardingWizardDefaults', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowTasks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Workflows" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowInstances" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowExecutions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowEscalations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ValidationRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ValidationResults" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserWorkspaces" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserProfiles" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserProfileAssignments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UserConsents" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "UITextEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TriggerRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TitleCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TestProcedures" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TenantUsers" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TenantTemplates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Tenants" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TenantPackages" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TenantBaselines" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TemplateCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "TeamType" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "TeamCode" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "Purpose" TYPE character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "NameAr" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "Name" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "Description" TYPE character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ALTER COLUMN "BusinessUnit" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ADD "IsSharedTeam" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TeamMembers" ALTER COLUMN "RoleCode" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TeamMembers" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TeamMembers" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TaskDelegations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TaskComments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SyncJobs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SupportMessages" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SupportConversations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SuiteControlEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Subscriptions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SubscriptionPlans" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SoDConflicts" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SlaRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "SiteMapEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ShahinAIModules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Rulesets" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Rules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RoleProfiles" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RoleCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Risks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RiskResiliences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RiskIndicators" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Resiliences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RequirementMappings" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Reports" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ALTER COLUMN "ScopeType" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ALTER COLUMN "ScopeId" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ALTER COLUMN "RoleCode" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ALTER COLUMN "RACI" TYPE character varying(1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PolicyViolations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PolicyDecisions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Policies" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Plans" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PlanPhases" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PlainLanguageControls" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PendingApprovals" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Payments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "PackageCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OverlayControlMappings" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OverlayCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OrganizationEntities" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OnePageGuides" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "OnboardingWizards" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "MappingQualityGates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "LlmConfigurations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "LegalDocuments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Invoices" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "IntegrationConnectors" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "GovernanceCadences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "FrameworkControls" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EvidenceScores" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Evidences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EvidencePacks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EventSubscriptions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "EscalationRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "DomainEvents" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "DelegationRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "DelegationLogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "DeadLetterEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "DataQualityScores" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CryptographicAssets" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlTestProcedures" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Controls" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlObjectives" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlExceptions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlDomains" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlChangeHistories" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ControlCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CCMTestExecutions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CCMExceptions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CCMControlTests" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CapturedEvidences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CanonicalControls" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "CadenceExecutions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "BaselineControlSets" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "BaselineControlMappings" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "BaselineCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Audits" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AuditFindings" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AuditEvents" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Assets" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AssessmentScopes" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Assessments" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AssessmentRequirements" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApprovalRecords" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApprovalInstances" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApprovalChains" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApplicabilityRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentSoDViolations" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentSoDRules" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentDefinitions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentCapabilities" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentApprovalGates" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "AgentActions" ADD "RowVersion" bytea;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE TABLE "Workspaces" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceCode" character varying(50) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "WorkspaceType" character varying(50) NOT NULL,
        "JurisdictionCode" character varying(10),
        "DefaultLanguage" character varying(10) NOT NULL,
        "Timezone" character varying(50),
        "Description" character varying(1000),
        "IsDefault" boolean NOT NULL,
        "RegulatorsJson" text,
        "OverlaysJson" text,
        "ConfigJson" text,
        "Status" character varying(20) NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_Workspaces" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Workspaces_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE TABLE "WorkspaceApprovalGates" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "GateCode" character varying(100) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NameAr" character varying(255),
        "ScopeType" character varying(50) NOT NULL,
        "ScopeValue" character varying(255),
        "MinApprovals" integer NOT NULL,
        "SlaDays" integer NOT NULL,
        "EscalationDays" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WorkspaceApprovalGates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkspaceApprovalGates_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE TABLE "WorkspaceControls" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "FrequencyOverride" character varying(20),
        "SlaDaysOverride" integer,
        "OverlaySource" character varying(100),
        "OwnerTeamId" uuid,
        "OwnerUserId" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WorkspaceControls" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkspaceControls_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_WorkspaceControls_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE TABLE "WorkspaceMemberships" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "WorkspaceRolesJson" text,
        "IsPrimary" boolean NOT NULL,
        "IsWorkspaceAdmin" boolean NOT NULL,
        "Status" character varying(20) NOT NULL,
        "JoinedDate" timestamp with time zone NOT NULL,
        "LastAccessedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WorkspaceMemberships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkspaceMemberships_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_WorkspaceMemberships_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE TABLE "WorkspaceApprovalGateApprovers" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "GateId" uuid NOT NULL,
        "ApproverType" character varying(20) NOT NULL,
        "ApproverReference" character varying(255) NOT NULL,
        "ApprovalOrder" integer NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WorkspaceApprovalGateApprovers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkspaceApprovalGateApprovers_WorkspaceApprovalGates_GateId" FOREIGN KEY ("GateId") REFERENCES "WorkspaceApprovalGates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_Teams_WorkspaceId" ON "Teams" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_TeamMembers_WorkspaceId" ON "TeamMembers" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_Risks_TenantId" ON "Risks" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_RACIAssignments_WorkspaceId" ON "RACIAssignments" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceApprovalGateApprovers_GateId" ON "WorkspaceApprovalGateApprovers" ("GateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceApprovalGates_WorkspaceId" ON "WorkspaceApprovalGates" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceControls_ControlId" ON "WorkspaceControls" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceControls_WorkspaceId" ON "WorkspaceControls" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceMemberships_UserId" ON "WorkspaceMemberships" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_WorkspaceMemberships_WorkspaceId" ON "WorkspaceMemberships" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    CREATE INDEX "IX_Workspaces_TenantId" ON "Workspaces" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "RACIAssignments" ADD CONSTRAINT "FK_RACIAssignments_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "TeamMembers" ADD CONSTRAINT "FK_TeamMembers_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    ALTER TABLE "Teams" ADD CONSTRAINT "FK_Teams_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106125113_WorkspaceInsideTenantModel') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106125113_WorkspaceInsideTenantModel', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Risks" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Policies" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Plans" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Evidences" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Controls" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Audits" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Assessments" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Risks_WorkspaceId" ON "Risks" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Policies_WorkspaceId" ON "Policies" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Plans_WorkspaceId" ON "Plans" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Evidences_WorkspaceId" ON "Evidences" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Controls_WorkspaceId" ON "Controls" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Audits_WorkspaceId" ON "Audits" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    CREATE INDEX "IX_Assessments_WorkspaceId" ON "Assessments" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Assessments" ADD CONSTRAINT "FK_Assessments_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Audits" ADD CONSTRAINT "FK_Audits_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Controls" ADD CONSTRAINT "FK_Controls_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Evidences" ADD CONSTRAINT "FK_Evidences_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Plans" ADD CONSTRAINT "FK_Plans_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Policies" ADD CONSTRAINT "FK_Policies_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    ALTER TABLE "Risks" ADD CONSTRAINT "FK_Risks_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106131413_AddWorkspaceIdToCoreEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106131413_AddWorkspaceIdToCoreEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106134752_AddWorkspaceIdToReport') THEN
    ALTER TABLE "Reports" ADD "WorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106134752_AddWorkspaceIdToReport') THEN
    CREATE INDEX "IX_Reports_WorkspaceId" ON "Reports" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106134752_AddWorkspaceIdToReport') THEN
    ALTER TABLE "Reports" ADD CONSTRAINT "FK_Reports_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106134752_AddWorkspaceIdToReport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106134752_AddWorkspaceIdToReport', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    ALTER TABLE "AuditEvents" ADD "Severity" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    CREATE TABLE "WebhookSubscriptions" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "TargetUrl" character varying(500) NOT NULL,
        "Secret" character varying(256) NOT NULL,
        "EventTypes" character varying(1000) NOT NULL,
        "IsActive" boolean NOT NULL,
        "Description" character varying(500),
        "CustomHeaders" character varying(2000),
        "TimeoutSeconds" integer NOT NULL,
        "MaxRetries" integer NOT NULL,
        "RetryDelays" character varying(100) NOT NULL,
        "ContentType" character varying(50) NOT NULL,
        "LastSuccessAt" timestamp with time zone,
        "LastFailureAt" timestamp with time zone,
        "LastErrorMessage" character varying(1000),
        "SuccessCount" bigint NOT NULL,
        "FailureCount" bigint NOT NULL,
        "DisableAfterFailures" integer NOT NULL,
        "ConsecutiveFailures" integer NOT NULL,
        "DisabledAt" timestamp with time zone,
        "DisabledReason" character varying(500),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WebhookSubscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WebhookSubscriptions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    CREATE TABLE "WebhookDeliveryLogs" (
        "Id" uuid NOT NULL,
        "WebhookSubscriptionId" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "EventId" character varying(100) NOT NULL,
        "PayloadJson" text NOT NULL,
        "Status" character varying(20) NOT NULL,
        "AttemptCount" integer NOT NULL,
        "ResponseStatusCode" integer,
        "ResponseBody" character varying(2000),
        "ResponseTimeMs" bigint,
        "ErrorMessage" character varying(1000),
        "ErrorStackTrace" character varying(4000),
        "FirstAttemptAt" timestamp with time zone,
        "LastAttemptAt" timestamp with time zone,
        "NextRetryAt" timestamp with time zone,
        "DeliveredAt" timestamp with time zone,
        "Signature" character varying(256),
        "RequestHeaders" character varying(2000),
        "ResponseHeaders" character varying(2000),
        "TargetUrl" character varying(500) NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_WebhookDeliveryLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WebhookDeliveryLogs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id"),
        CONSTRAINT "FK_WebhookDeliveryLogs_WebhookSubscriptions_WebhookSubscriptio~" FOREIGN KEY ("WebhookSubscriptionId") REFERENCES "WebhookSubscriptions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    CREATE INDEX "IX_WebhookDeliveryLogs_TenantId" ON "WebhookDeliveryLogs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    CREATE INDEX "IX_WebhookDeliveryLogs_WebhookSubscriptionId" ON "WebhookDeliveryLogs" ("WebhookSubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    CREATE INDEX "IX_WebhookSubscriptions_TenantId" ON "WebhookSubscriptions" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106141549_WebhooksAndMessagingSupport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106141549_WebhooksAndMessagingSupport', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "TenantUsers" ADD "CredentialExpiresAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "TenantUsers" ADD "GeneratedByOwnerId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "TenantUsers" ADD "IsOwnerGenerated" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "TenantUsers" ADD "MustChangePasswordOnFirstLogin" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "AdminAccountGenerated" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "AdminAccountGeneratedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "BypassPayment" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "CreatedByOwnerId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "CredentialExpiresAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    ALTER TABLE "Tenants" ADD "IsOwnerCreated" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    CREATE TABLE "OwnerTenantCreations" (
        "Id" uuid NOT NULL,
        "OwnerId" text NOT NULL,
        "TenantId" uuid NOT NULL,
        "AdminUsername" character varying(256) NOT NULL,
        "CredentialsExpiresAt" timestamp with time zone NOT NULL,
        "DeliveryMethod" character varying(50) NOT NULL,
        "CredentialsDelivered" boolean NOT NULL,
        "DeliveredAt" timestamp with time zone,
        "DeliveryNotes" character varying(1000),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_OwnerTenantCreations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OwnerTenantCreations_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OwnerTenantCreations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    CREATE INDEX "IX_OwnerTenantCreations_OwnerId" ON "OwnerTenantCreations" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    CREATE INDEX "IX_OwnerTenantCreations_TenantId" ON "OwnerTenantCreations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    CREATE INDEX "IX_OwnerTenantCreations_TenantId_OwnerId" ON "OwnerTenantCreations" ("TenantId", "OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106153003_AddOwnerTenantCreation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106153003_AddOwnerTenantCreation', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Workspaces" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkspaceMemberships" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkspaceControls" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkspaceApprovalGates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkspaceApprovalGateApprovers" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowTasks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Workflows" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowInstances" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowExecutions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowEscalations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WebhookSubscriptions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "WebhookDeliveryLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ValidationRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ValidationResults" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserWorkspaces" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserProfiles" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserProfileAssignments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UserConsents" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "UITextEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TriggerRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TitleCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TestProcedures" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TenantUsers" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TenantTemplates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Tenants" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TenantPackages" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TenantBaselines" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TemplateCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Teams" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TeamMembers" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TaskDelegations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "TaskComments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SyncJobs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SupportMessages" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SupportConversations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SuiteControlEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Subscriptions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SubscriptionPlans" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SoDConflicts" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SlaRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "SiteMapEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ShahinAIModules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Rulesets" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Rules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RoleProfiles" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RoleCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Risks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RiskResiliences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RiskIndicators" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Resiliences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RequirementMappings" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Reports" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "RACIAssignments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PolicyViolations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PolicyDecisions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Policies" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Plans" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PlanPhases" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PlainLanguageControls" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PendingApprovals" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Payments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "PackageCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OwnerTenantCreations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OwnerTenantCreations" ADD "PlatformAdminId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OverlayControlMappings" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OverlayCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OrganizationEntities" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OnePageGuides" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OnboardingWizards" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "MappingQualityGates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "LlmConfigurations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "LegalDocuments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Invoices" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "IntegrationConnectors" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "GovernanceCadences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "FrameworkControls" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EvidenceScores" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Evidences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EvidencePacks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EventSubscriptions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "EscalationRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "DomainEvents" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "DelegationRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "DelegationLogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "DeadLetterEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "DataQualityScores" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CryptographicAssets" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlTestProcedures" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Controls" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlObjectives" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlExceptions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlDomains" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlChangeHistories" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ControlCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CCMTestExecutions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CCMExceptions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CCMControlTests" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CapturedEvidences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CanonicalControls" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "CadenceExecutions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "BaselineControlSets" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "BaselineControlMappings" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "BaselineCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Audits" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AuditFindings" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AuditEvents" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AuditEvents" ADD "Description" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AuditEvents" ADD "IpAddress" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AuditEvents" ADD "UserId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Assets" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AssessmentScopes" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "Assessments" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AssessmentRequirements" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApprovalRecords" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApprovalInstances" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApprovalChains" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApplicabilityRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentSoDViolations" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentSoDRules" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentDefinitions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentCapabilities" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentApprovalGates" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "AgentActions" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    CREATE TABLE "PlatformAdmins" (
        "Id" uuid NOT NULL,
        "UserId" character varying(450) NOT NULL,
        "AdminLevel" integer NOT NULL,
        "DisplayName" character varying(256) NOT NULL,
        "ContactEmail" character varying(256) NOT NULL,
        "ContactPhone" character varying(50),
        "CanCreateTenants" boolean NOT NULL,
        "CanManageTenants" boolean NOT NULL,
        "CanDeleteTenants" boolean NOT NULL,
        "CanManageBilling" boolean NOT NULL,
        "CanAccessTenantData" boolean NOT NULL,
        "CanManageCatalogs" boolean NOT NULL,
        "CanManagePlatformAdmins" boolean NOT NULL,
        "CanViewAnalytics" boolean NOT NULL,
        "CanManageConfiguration" boolean NOT NULL,
        "CanImpersonateUsers" boolean NOT NULL,
        "AllowedRegions" character varying(500),
        "AllowedTenantIds" character varying(2000),
        "MaxTenantsAllowed" integer NOT NULL,
        "LastLoginAt" timestamp with time zone,
        "LastLoginIp" character varying(50),
        "TotalTenantsCreated" integer NOT NULL,
        "LastTenantCreatedAt" timestamp with time zone,
        "Status" character varying(50) NOT NULL,
        "StatusReason" character varying(500),
        "MfaRequired" boolean NOT NULL,
        "SessionTimeoutMinutes" integer NOT NULL,
        "CanResetOwnPassword" boolean NOT NULL,
        "CanResetTenantAdminPasswords" boolean NOT NULL,
        "CanRestartTenantAdminAccounts" boolean NOT NULL,
        "LastPasswordChangedAt" timestamp with time zone,
        "ForcePasswordChange" boolean NOT NULL,
        "CreatedByAdminId" character varying(450),
        "Notes" character varying(2000),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        CONSTRAINT "PK_PlatformAdmins" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlatformAdmins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    CREATE INDEX "IX_OwnerTenantCreations_PlatformAdminId" ON "OwnerTenantCreations" ("PlatformAdminId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    CREATE INDEX "IX_PlatformAdmins_AdminLevel" ON "PlatformAdmins" ("AdminLevel");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    CREATE INDEX "IX_PlatformAdmins_Status" ON "PlatformAdmins" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    CREATE UNIQUE INDEX "IX_PlatformAdmins_UserId" ON "PlatformAdmins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    ALTER TABLE "OwnerTenantCreations" ADD CONSTRAINT "FK_OwnerTenantCreations_PlatformAdmins_PlatformAdminId" FOREIGN KEY ("PlatformAdminId") REFERENCES "PlatformAdmins" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106170143_AddDeletedAtToBaseEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106170143_AddDeletedAtToBaseEntity', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    ALTER TABLE "Tenants" ADD "AssessmentTemplateId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    ALTER TABLE "Tenants" ADD "DefaultWorkspaceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    ALTER TABLE "Tenants" ADD "GrcPlanId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    ALTER TABLE "Tenants" ADD "OnboardingCompletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    ALTER TABLE "Tenants" ADD "OnboardingStatus" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    CREATE TABLE "RoleLandingConfigs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "RoleCode" character varying(100) NOT NULL,
        "LandingDashboardId" uuid,
        "DefaultLandingPage" character varying(255) NOT NULL,
        "WidgetsJson" text,
        "QuickActionsJson" text,
        "NavigationJson" text,
        "DefaultFiltersJson" text,
        "FavoritesJson" text,
        "NotificationPrefsJson" text,
        "AssignableTaskTypesJson" text,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        CONSTRAINT "PK_RoleLandingConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RoleLandingConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RoleLandingConfigs_Workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES "Workspaces" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    CREATE INDEX "IX_RoleLandingConfigs_TenantId" ON "RoleLandingConfigs" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    CREATE INDEX "IX_RoleLandingConfigs_WorkspaceId" ON "RoleLandingConfigs" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106172924_AddOnboardingWorkspaceSupport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106172924_AddOnboardingWorkspaceSupport', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AspNetUsers" DROP CONSTRAINT "FK_AspNetUsers_RoleProfiles_RoleProfileId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OwnerTenantCreations" DROP CONSTRAINT "FK_OwnerTenantCreations_AspNetUsers_OwnerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlatformAdmins" DROP CONSTRAINT "FK_PlatformAdmins_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleFeatures" DROP CONSTRAINT "FK_RoleFeatures_AspNetRoles_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RolePermissions" DROP CONSTRAINT "FK_RolePermissions_AspNetRoles_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantRoleConfigurations" DROP CONSTRAINT "FK_TenantRoleConfigurations_AspNetRoles_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantUsers" DROP CONSTRAINT "FK_TenantUsers_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserConsents" DROP CONSTRAINT "FK_UserConsents_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserRoleAssignments" DROP CONSTRAINT "FK_UserRoleAssignments_AspNetRoles_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserRoleAssignments" DROP CONSTRAINT "FK_UserRoleAssignments_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaces" DROP CONSTRAINT "FK_UserWorkspaces_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceMemberships" DROP CONSTRAINT "FK_WorkspaceMemberships_AspNetUsers_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetRoleClaims";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetUserClaims";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetUserLogins";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetUserRoles";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetUserTokens";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP TABLE "AspNetRoles";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "IX_UserRoleAssignments_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "IX_UserRoleAssignments_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "IX_TenantRoleConfigurations_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "IX_RolePermissions_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "IX_RoleFeatures_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AspNetUsers" DROP CONSTRAINT "PK_AspNetUsers";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "EmailIndex";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    DROP INDEX "UserNameIndex";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AspNetUsers" RENAME TO "ApplicationUser";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER INDEX "IX_AspNetUsers_RoleProfileId" RENAME TO "IX_ApplicationUser_RoleProfileId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workspaces" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workspaces" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workspaces" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceMemberships" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceMemberships" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceMemberships" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceControls" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceControls" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceControls" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGateApprovers" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGateApprovers" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceApprovalGateApprovers" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowTasks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowTasks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowTasks" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workflows" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workflows" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Workflows" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowInstances" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowInstances" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowInstances" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowExecutions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowExecutions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowExecutions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowEscalations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowEscalations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowEscalations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookSubscriptions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookSubscriptions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookSubscriptions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookDeliveryLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookDeliveryLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WebhookDeliveryLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationResults" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationResults" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ValidationResults" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaces" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaces" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaces" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfiles" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfiles" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfiles" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfileAssignments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfileAssignments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserProfileAssignments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserConsents" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserConsents" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserConsents" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UITextEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UITextEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UITextEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TitleCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TitleCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TitleCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TestProcedures" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TestProcedures" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TestProcedures" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantUsers" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantUsers" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantUsers" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantTemplates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantTemplates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantTemplates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Tenants" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Tenants" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Tenants" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantPackages" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantPackages" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantPackages" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantBaselines" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantBaselines" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantBaselines" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TemplateCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TemplateCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TemplateCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Teams" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Teams" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Teams" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamMembers" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamMembers" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TeamMembers" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskDelegations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskDelegations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskDelegations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskComments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskComments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TaskComments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncJobs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncJobs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncJobs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportMessages" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportMessages" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportMessages" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportConversations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportConversations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SupportConversations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteControlEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteControlEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SuiteControlEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Subscriptions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Subscriptions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Subscriptions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SubscriptionPlans" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SubscriptionPlans" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SubscriptionPlans" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDConflicts" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDConflicts" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SoDConflicts" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SlaRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SlaRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SlaRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SiteMapEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SiteMapEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "SiteMapEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIModules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIModules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIModules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rulesets" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rulesets" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rulesets" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Rules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleProfiles" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleProfiles" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleProfiles" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleLandingConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleLandingConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleLandingConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RoleCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Risks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Risks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskResiliences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskResiliences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskResiliences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicators" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicators" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicators" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Resiliences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Resiliences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Resiliences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RequirementMappings" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RequirementMappings" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RequirementMappings" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Reports" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Reports" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Reports" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RACIAssignments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RACIAssignments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "RACIAssignments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyViolations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyViolations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyViolations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyDecisions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyDecisions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PolicyDecisions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Policies" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Policies" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlatformAdmins" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlatformAdmins" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlatformAdmins" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Plans" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Plans" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Plans" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlanPhases" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlanPhases" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlainLanguageControls" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlainLanguageControls" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlainLanguageControls" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PendingApprovals" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PendingApprovals" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PendingApprovals" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Payments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Payments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Payments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PackageCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PackageCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PackageCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OwnerTenantCreations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OwnerTenantCreations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayControlMappings" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayControlMappings" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayControlMappings" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OverlayCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationProfiles" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationProfiles" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationEntities" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationEntities" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OrganizationEntities" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnePageGuides" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnePageGuides" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnePageGuides" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnboardingWizards" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnboardingWizards" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OnboardingWizards" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingQualityGates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingQualityGates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MappingQualityGates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LlmConfigurations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LlmConfigurations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LlmConfigurations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LegalDocuments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LegalDocuments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "LegalDocuments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Invoices" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Invoices" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Invoices" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationConnectors" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationConnectors" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "IntegrationConnectors" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceCadences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceCadences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GovernanceCadences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkControls" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkControls" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkControls" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceScores" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceScores" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidenceScores" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Evidences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Evidences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Evidences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePacks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePacks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePacks" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSubscriptions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSubscriptions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSubscriptions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EscalationRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EscalationRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "EscalationRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DomainEvents" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DomainEvents" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DomainEvents" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationLogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationLogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DelegationLogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DeadLetterEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DeadLetterEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DeadLetterEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DataQualityScores" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DataQualityScores" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "DataQualityScores" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CryptographicAssets" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CryptographicAssets" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CryptographicAssets" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlTestProcedures" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlTestProcedures" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlTestProcedures" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Controls" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Controls" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlObjectives" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlObjectives" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlObjectives" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlExceptions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlExceptions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlExceptions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlDomains" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlDomains" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlDomains" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlChangeHistories" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlChangeHistories" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlChangeHistories" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ControlCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMTestExecutions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMTestExecutions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMTestExecutions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMExceptions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMExceptions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMExceptions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMControlTests" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMControlTests" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CCMControlTests" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CapturedEvidences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CapturedEvidences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CapturedEvidences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CanonicalControls" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CanonicalControls" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CanonicalControls" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CadenceExecutions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CadenceExecutions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "CadenceExecutions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlSets" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlSets" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlSets" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlMappings" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlMappings" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineControlMappings" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "BaselineCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Audits" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Audits" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Audits" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditFindings" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditFindings" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditFindings" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditEvents" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditEvents" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AuditEvents" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Assets" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Assets" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentScopes" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentScopes" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentScopes" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Assessments" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Assessments" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "Assessments" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentRequirements" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentRequirements" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AssessmentRequirements" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalRecords" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalRecords" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalRecords" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalInstances" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalInstances" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalInstances" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalChains" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalChains" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApprovalChains" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDViolations" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDViolations" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDViolations" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDRules" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDRules" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentSoDRules" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentDefinitions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentDefinitions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentDefinitions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentCapabilities" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentCapabilities" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentCapabilities" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentApprovalGates" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentApprovalGates" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentApprovalGates" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentActions" ADD "DataClassification" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentActions" ADD "LabelsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "AgentActions" ADD "Owner" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ALTER COLUMN "UserName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ALTER COLUMN "NormalizedUserName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ALTER COLUMN "NormalizedEmail" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ALTER COLUMN "Email" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ADD "LastPasswordChangedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ADD "MustChangePassword" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ADD CONSTRAINT "PK_ApplicationUser" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "ApplicationUser" ADD CONSTRAINT "FK_ApplicationUser_RoleProfiles_RoleProfileId" FOREIGN KEY ("RoleProfileId") REFERENCES "RoleProfiles" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "OwnerTenantCreations" ADD CONSTRAINT "FK_OwnerTenantCreations_ApplicationUser_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "ApplicationUser" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "PlatformAdmins" ADD CONSTRAINT "FK_PlatformAdmins_ApplicationUser_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUser" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "TenantUsers" ADD CONSTRAINT "FK_TenantUsers_ApplicationUser_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUser" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserConsents" ADD CONSTRAINT "FK_UserConsents_ApplicationUser_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUser" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "UserWorkspaces" ADD CONSTRAINT "FK_UserWorkspaces_ApplicationUser_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUser" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    ALTER TABLE "WorkspaceMemberships" ADD CONSTRAINT "FK_WorkspaceMemberships_ApplicationUser_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUser" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106203101_AddGovernanceMetadata') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106203101_AddGovernanceMetadata', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    CREATE TABLE "ActionPlans" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid,
        "PlanNumber" text NOT NULL,
        "Title" text NOT NULL,
        "Description" text NOT NULL,
        "Category" text NOT NULL,
        "Status" text NOT NULL,
        "Priority" text NOT NULL,
        "AssignedTo" text NOT NULL,
        "StartDate" timestamp with time zone,
        "DueDate" timestamp with time zone,
        "CompletedDate" timestamp with time zone,
        "Notes" text NOT NULL,
        "RelatedRiskId" uuid,
        "RelatedAuditId" uuid,
        "RelatedAssessmentId" uuid,
        "RelatedControlId" uuid,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ActionPlans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    CREATE TABLE "ComplianceEvents" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid,
        "EventNumber" text NOT NULL,
        "Title" text NOT NULL,
        "Description" text NOT NULL,
        "EventType" text NOT NULL,
        "Category" text NOT NULL,
        "EventDate" timestamp with time zone NOT NULL,
        "DueDate" timestamp with time zone,
        "ReminderDate" timestamp with time zone,
        "Status" text NOT NULL,
        "Priority" text NOT NULL,
        "AssignedTo" text NOT NULL,
        "RelatedRegulatorId" uuid,
        "RelatedFrameworkId" uuid,
        "RelatedAssessmentId" uuid,
        "RecurrencePattern" text NOT NULL,
        "Notes" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ComplianceEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    CREATE TABLE "Frameworks" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid,
        "FrameworkCode" text NOT NULL,
        "Name" text NOT NULL,
        "NameAr" text NOT NULL,
        "Description" text NOT NULL,
        "Version" text NOT NULL,
        "Jurisdiction" text NOT NULL,
        "Type" text NOT NULL,
        "Status" text NOT NULL,
        "EffectiveDate" timestamp with time zone,
        "ExpirationDate" timestamp with time zone,
        "Website" text NOT NULL,
        "Notes" text NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Frameworks" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    CREATE TABLE "Regulators" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid,
        "RegulatorCode" text NOT NULL,
        "Name" text NOT NULL,
        "NameAr" text NOT NULL,
        "Description" text NOT NULL,
        "Jurisdiction" text NOT NULL,
        "Type" text NOT NULL,
        "Website" text NOT NULL,
        "ContactEmail" text NOT NULL,
        "ContactPhone" text NOT NULL,
        "Status" text NOT NULL,
        "IsPrimary" boolean NOT NULL,
        "Notes" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Regulators" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    CREATE TABLE "Vendors" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid,
        "VendorCode" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "Category" text NOT NULL,
        "Status" text NOT NULL,
        "ContactName" text NOT NULL,
        "ContactEmail" text NOT NULL,
        "ContactPhone" text NOT NULL,
        "Address" text NOT NULL,
        "Country" text NOT NULL,
        "RiskLevel" text NOT NULL,
        "LastAssessmentDate" timestamp with time zone,
        "NextAssessmentDate" timestamp with time zone,
        "AssessmentStatus" text NOT NULL,
        "Notes" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Vendors" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106223033_AddStubControllerEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260106223033_AddStubControllerEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    ALTER TABLE "Tenants" ADD "BillingStatus" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    ALTER TABLE "Tenants" ADD "IsTrial" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    ALTER TABLE "Tenants" ADD "TrialEndsAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    ALTER TABLE "Tenants" ADD "TrialStartsAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    CREATE TABLE "TrialRequests" (
        "Id" uuid NOT NULL,
        "OrganizationName" character varying(255) NOT NULL,
        "AdminName" character varying(255) NOT NULL,
        "AdminEmail" character varying(255) NOT NULL,
        "Country" character varying(100) NOT NULL,
        "Phone" character varying(50),
        "Status" character varying(50) NOT NULL,
        "RequestedAt" timestamp with time zone NOT NULL,
        "TermsAcceptedAt" timestamp with time zone,
        "ProvisionedTenantId" uuid,
        "PaymentVerificationStatus" character varying(50),
        "ProviderCustomerId" character varying(255),
        "PaymentMethodId" character varying(255),
        "Source" character varying(100) NOT NULL,
        "RequestIp" character varying(50),
        "UserAgent" character varying(500),
        "Notes" character varying(1000),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TrialRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrialRequests_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    CREATE INDEX "IX_TrialRequests_TenantId" ON "TrialRequests" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107153825_AddTrialFeatures') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260107153825_AddTrialFeatures', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailMailboxes" (
        "Id" uuid NOT NULL,
        "EmailAddress" character varying(200) NOT NULL,
        "DisplayName" character varying(100) NOT NULL,
        "Brand" character varying(50) NOT NULL,
        "Purpose" character varying(50) NOT NULL,
        "GraphUserId" character varying(100),
        "ClientId" character varying(100),
        "EncryptedClientSecret" text,
        "TenantId" character varying(100),
        "IsActive" boolean NOT NULL,
        "AutoReplyEnabled" boolean NOT NULL,
        "DraftModeDefault" boolean NOT NULL,
        "SlaFirstResponseHours" integer NOT NULL,
        "SlaResolutionHours" integer NOT NULL,
        "WebhookSubscriptionId" character varying(200),
        "WebhookExpiresAt" timestamp with time zone,
        "LastSyncAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailMailboxes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailTemplates" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Brand" character varying(50) NOT NULL,
        "Language" character varying(10) NOT NULL,
        "ForClassifications" integer[] NOT NULL,
        "SubjectTemplate" character varying(500),
        "BodyTemplate" text NOT NULL,
        "AvailableVariables" character varying(1000),
        "IsActive" boolean NOT NULL,
        "UsageCount" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailAutoReplyRules" (
        "Id" uuid NOT NULL,
        "MailboxId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text,
        "TriggerClassifications" integer[] NOT NULL,
        "SubjectPattern" character varying(500),
        "BodyPattern" character varying(1000),
        "FromPattern" character varying(500),
        "Action" integer NOT NULL,
        "TemplateName" character varying(100),
        "ReplyContent" text,
        "UseAiGeneration" boolean NOT NULL,
        "AiPromptTemplate" text,
        "Priority" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "FollowUpAfterHours" integer,
        "MaxAutoRepliesPerThread" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailAutoReplyRules" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailAutoReplyRules_EmailMailboxes_MailboxId" FOREIGN KEY ("MailboxId") REFERENCES "EmailMailboxes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailThreads" (
        "Id" uuid NOT NULL,
        "ConversationId" character varying(500) NOT NULL,
        "Subject" character varying(500) NOT NULL,
        "FromEmail" character varying(200) NOT NULL,
        "FromName" character varying(200),
        "Classification" integer NOT NULL,
        "ClassificationConfidence" integer NOT NULL,
        "Status" integer NOT NULL,
        "Priority" integer NOT NULL,
        "AssignedToUserId" uuid,
        "AssignedToUserName" character varying(200),
        "MailboxId" uuid NOT NULL,
        "ReceivedAt" timestamp with time zone NOT NULL,
        "FirstResponseAt" timestamp with time zone,
        "ResolvedAt" timestamp with time zone,
        "SlaFirstResponseDeadline" timestamp with time zone,
        "SlaResolutionDeadline" timestamp with time zone,
        "SlaFirstResponseBreached" boolean NOT NULL,
        "SlaResolutionBreached" boolean NOT NULL,
        "NextFollowUpAt" timestamp with time zone,
        "FollowUpCount" integer NOT NULL,
        "ExtractedDataJson" text,
        "InternalNotes" text,
        "Tags" character varying(500),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailThreads" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailThreads_EmailMailboxes_MailboxId" FOREIGN KEY ("MailboxId") REFERENCES "EmailMailboxes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailMessages" (
        "Id" uuid NOT NULL,
        "GraphMessageId" character varying(500) NOT NULL,
        "InternetMessageId" character varying(500),
        "ThreadId" uuid NOT NULL,
        "FromEmail" character varying(200) NOT NULL,
        "FromName" character varying(200),
        "ToRecipients" character varying(1000),
        "CcRecipients" character varying(1000),
        "Subject" character varying(500) NOT NULL,
        "BodyPreview" character varying(500),
        "BodyContent" text,
        "BodyContentType" character varying(20) NOT NULL,
        "Direction" integer NOT NULL,
        "Status" integer NOT NULL,
        "IsAiGenerated" boolean NOT NULL,
        "AiPromptUsed" text,
        "ApprovedByUserId" uuid,
        "ApprovedByUserName" character varying(200),
        "ApprovedAt" timestamp with time zone,
        "SentAt" timestamp with time zone,
        "ReceivedAt" timestamp with time zone NOT NULL,
        "HasAttachments" boolean NOT NULL,
        "AttachmentsJson" text,
        "Importance" character varying(20) NOT NULL,
        "IsRead" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailMessages_EmailThreads_ThreadId" FOREIGN KEY ("ThreadId") REFERENCES "EmailThreads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailTasks" (
        "Id" uuid NOT NULL,
        "ThreadId" uuid NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Description" text,
        "TaskType" integer NOT NULL,
        "Status" integer NOT NULL,
        "Priority" integer NOT NULL,
        "AssignedToUserId" uuid,
        "AssignedToUserName" character varying(200),
        "DueAt" timestamp with time zone,
        "ReminderAt" timestamp with time zone,
        "ScheduledJobId" character varying(100),
        "CompletedAt" timestamp with time zone,
        "CompletedByUserId" uuid,
        "CompletedByUserName" character varying(200),
        "Notes" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailTasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailTasks_EmailThreads_ThreadId" FOREIGN KEY ("ThreadId") REFERENCES "EmailThreads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE TABLE "EmailAttachments" (
        "Id" uuid NOT NULL,
        "MessageId" uuid NOT NULL,
        "GraphAttachmentId" character varying(500) NOT NULL,
        "FileName" character varying(300) NOT NULL,
        "ContentType" character varying(100) NOT NULL,
        "Size" bigint NOT NULL,
        "LocalPath" character varying(500),
        "IsInline" boolean NOT NULL,
        "ContentId" character varying(200),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EmailAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailAttachments_EmailMessages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "EmailMessages" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE INDEX "IX_EmailAttachments_MessageId" ON "EmailAttachments" ("MessageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE INDEX "IX_EmailAutoReplyRules_MailboxId" ON "EmailAutoReplyRules" ("MailboxId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE INDEX "IX_EmailMessages_ThreadId" ON "EmailMessages" ("ThreadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE INDEX "IX_EmailTasks_ThreadId" ON "EmailTasks" ("ThreadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    CREATE INDEX "IX_EmailThreads_MailboxId" ON "EmailThreads" ("MailboxId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108033051_AddEmailOperations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108033051_AddEmailOperations', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108044323_AddDefaultSlaHoursToMailbox') THEN
    ALTER TABLE "EmailMailboxes" ADD "DefaultSlaHours" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108044323_AddDefaultSlaHoursToMailbox') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108044323_AddDefaultSlaHoursToMailbox', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    ALTER INDEX "IX_FrameworkControls_FrameworkCode" RENAME TO "IX_FrameworkControl_Framework";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    ALTER INDEX "IX_FrameworkControls_Domain" RENAME TO "IX_FrameworkControl_Domain";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE TABLE "EvidenceScoringCriteria" (
        "Id" uuid NOT NULL,
        "EvidenceTypeCode" character varying(50) NOT NULL,
        "EvidenceTypeName" character varying(200) NOT NULL,
        "DescriptionEn" character varying(500) NOT NULL,
        "DescriptionAr" character varying(500) NOT NULL,
        "Category" character varying(50) NOT NULL,
        "BaseScore" integer NOT NULL,
        "MaxScore" integer NOT NULL,
        "ScoringRulesJson" text NOT NULL,
        "MinimumScore" integer NOT NULL,
        "RequiresApproval" boolean NOT NULL,
        "RequiresExpiry" boolean NOT NULL,
        "DefaultValidityDays" integer NOT NULL,
        "AllowedFileTypes" text NOT NULL,
        "MaxFileSizeMB" integer NOT NULL,
        "RequiresDigitalSignature" boolean NOT NULL,
        "CollectionFrequency" character varying(50) NOT NULL,
        "ApplicableFrameworks" character varying(500) NOT NULL,
        "ApplicableSectors" character varying(500) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "UpdatedDate" timestamp with time zone,
        "TenantId" uuid,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EvidenceScoringCriteria" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE TABLE "SectorFrameworkIndex" (
        "Id" uuid NOT NULL,
        "SectorCode" character varying(50) NOT NULL,
        "SectorNameEn" character varying(100) NOT NULL,
        "SectorNameAr" character varying(100) NOT NULL,
        "OrgType" character varying(50) NOT NULL,
        "OrgTypeNameEn" character varying(100) NOT NULL,
        "OrgTypeNameAr" character varying(100) NOT NULL,
        "FrameworkCode" character varying(50) NOT NULL,
        "FrameworkNameEn" character varying(200) NOT NULL,
        "Priority" integer NOT NULL,
        "IsMandatory" boolean NOT NULL,
        "ReasonEn" character varying(500) NOT NULL,
        "ReasonAr" character varying(500) NOT NULL,
        "ControlCount" integer NOT NULL,
        "CriticalControlCount" integer NOT NULL,
        "EvidenceTypesJson" text NOT NULL,
        "EvidenceTypeCount" integer NOT NULL,
        "ScoringWeight" double precision NOT NULL,
        "EstimatedImplementationDays" integer NOT NULL,
        "ImplementationGuidanceEn" character varying(1000) NOT NULL,
        "DeadlinesJson" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "ComputedAt" timestamp with time zone NOT NULL,
        "ComputedHash" text NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_SectorFrameworkIndex" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE TABLE "TenantEvidenceRequirements" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EvidenceTypeCode" character varying(50) NOT NULL,
        "EvidenceTypeName" character varying(200) NOT NULL,
        "FrameworkCode" character varying(50) NOT NULL,
        "ControlNumber" character varying(50) NOT NULL,
        "MinimumScore" integer NOT NULL,
        "CollectionFrequency" character varying(50) NOT NULL,
        "DefaultValidityDays" integer NOT NULL,
        "Status" character varying(30) NOT NULL,
        "DueDate" timestamp with time zone,
        "LastSubmittedDate" timestamp with time zone,
        "LastApprovedDate" timestamp with time zone,
        "ExpiryDate" timestamp with time zone,
        "CurrentScore" integer NOT NULL,
        "AssignedToUserId" uuid,
        "WorkspaceId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TenantEvidenceRequirements" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_FrameworkControl_Framework_Version" ON "FrameworkControls" ("FrameworkCode", "Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_FrameworkControl_Type" ON "FrameworkControls" ("ControlType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_EvidenceScoringCriteria_Active" ON "EvidenceScoringCriteria" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_EvidenceScoringCriteria_Category" ON "EvidenceScoringCriteria" ("Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE UNIQUE INDEX "IX_EvidenceScoringCriteria_TypeCode" ON "EvidenceScoringCriteria" ("EvidenceTypeCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_SectorFrameworkIndex_Active" ON "SectorFrameworkIndex" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_SectorFrameworkIndex_Framework" ON "SectorFrameworkIndex" ("FrameworkCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_SectorFrameworkIndex_Sector_Framework" ON "SectorFrameworkIndex" ("SectorCode", "FrameworkCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_SectorFrameworkIndex_Sector_OrgType" ON "SectorFrameworkIndex" ("SectorCode", "OrgType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_AssignedTo" ON "TenantEvidenceRequirements" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_Tenant" ON "TenantEvidenceRequirements" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_Tenant_DueDate" ON "TenantEvidenceRequirements" ("TenantId", "DueDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_Tenant_Framework" ON "TenantEvidenceRequirements" ("TenantId", "FrameworkCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_Tenant_Status" ON "TenantEvidenceRequirements" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE UNIQUE INDEX "IX_TenantEvidenceRequirement_Unique" ON "TenantEvidenceRequirements" ("TenantId", "FrameworkCode", "ControlNumber", "EvidenceTypeCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    CREATE INDEX "IX_TenantEvidenceRequirement_Workspace" ON "TenantEvidenceRequirements" ("WorkspaceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108084019_AddEvidenceScoringAndSectorIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108084019_AddEvidenceScoringAndSectorIndex', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108092156_AddGosiSubSectorMapping') THEN
    CREATE TABLE "GrcSubSectorMappings" (
        "Id" uuid NOT NULL,
        "GosiCode" character varying(10) NOT NULL,
        "IsicSection" character varying(2) NOT NULL,
        "SubSectorNameEn" character varying(300) NOT NULL,
        "SubSectorNameAr" character varying(300) NOT NULL,
        "MainSectorCode" character varying(50) NOT NULL,
        "MainSectorNameEn" character varying(200) NOT NULL,
        "MainSectorNameAr" character varying(200) NOT NULL,
        "RegulatoryNotes" character varying(1000),
        "PrimaryRegulator" character varying(100),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_GrcSubSectorMappings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108092156_AddGosiSubSectorMapping') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108092156_AddGosiSubSectorMapping', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111032_AddMarketingEntities') THEN
    CREATE TABLE "CaseStudies" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "TitleAr" character varying(200),
        "Slug" character varying(500),
        "Summary" character varying(2000) NOT NULL,
        "SummaryAr" character varying(2000),
        "FullContent" text,
        "FullContentAr" text,
        "Industry" character varying(100) NOT NULL,
        "IndustryAr" character varying(100),
        "CompanyName" character varying(200),
        "CompanyNameAr" character varying(200),
        "FrameworkCode" character varying(50),
        "TimeToCompliance" character varying(50),
        "ImprovementMetric" character varying(20),
        "ImprovementLabel" character varying(100),
        "ImprovementLabelAr" character varying(100),
        "ComplianceScore" character varying(20),
        "ImageUrl" character varying(500),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "PublishDate" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CaseStudies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111032_AddMarketingEntities') THEN
    CREATE TABLE "PricingPlans" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "NameAr" character varying(50),
        "Description" character varying(200),
        "DescriptionAr" character varying(200),
        "Price" numeric NOT NULL,
        "Period" character varying(20) NOT NULL,
        "FeaturesJson" text,
        "FeaturesJsonAr" text,
        "MaxUsers" integer NOT NULL,
        "MaxWorkspaces" integer NOT NULL,
        "MaxFrameworks" integer NOT NULL,
        "HasApiAccess" boolean NOT NULL,
        "HasPrioritySupport" boolean NOT NULL,
        "IsPopular" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PricingPlans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111032_AddMarketingEntities') THEN
    CREATE TABLE "Testimonials" (
        "Id" uuid NOT NULL,
        "Quote" character varying(1000) NOT NULL,
        "QuoteAr" character varying(1000),
        "AuthorName" character varying(100) NOT NULL,
        "AuthorNameAr" character varying(100),
        "AuthorTitle" character varying(100) NOT NULL,
        "AuthorTitleAr" character varying(100),
        "CompanyName" character varying(200) NOT NULL,
        "CompanyNameAr" character varying(200),
        "Industry" character varying(100),
        "IndustryAr" character varying(100),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Testimonials" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111032_AddMarketingEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108111032_AddMarketingEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111553_AddDocumentCenterTables') THEN
    CREATE TABLE "DocumentTemplates" (
        "Id" uuid NOT NULL,
        "Code" character varying(50) NOT NULL,
        "TitleEn" character varying(200) NOT NULL,
        "TitleAr" character varying(200),
        "DescriptionEn" character varying(2000),
        "DescriptionAr" character varying(2000),
        "Category" character varying(50) NOT NULL,
        "Domain" character varying(50),
        "FrameworkCodes" character varying(500),
        "Version" character varying(20) NOT NULL,
        "FileFormat" character varying(10) NOT NULL,
        "FilePathEn" character varying(500),
        "FilePathAr" character varying(500),
        "SectionsJson" text,
        "FieldsJson" text,
        "InstructionsEn" text,
        "InstructionsAr" text,
        "Tags" character varying(500),
        "DisplayOrder" integer NOT NULL,
        "DownloadCount" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsBilingual" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "UpdatedDate" timestamp with time zone,
        "CreatedBy" character varying(100),
        "UpdatedBy" character varying(100),
        CONSTRAINT "PK_DocumentTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108111553_AddDocumentCenterTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108111553_AddDocumentCenterTables', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    ALTER TABLE "Evidences" ADD "AssessmentRequirementId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "BlogPosts" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "TitleAr" character varying(200),
        "Slug" character varying(300) NOT NULL,
        "Excerpt" character varying(500),
        "ExcerptAr" character varying(500),
        "Content" text,
        "ContentAr" text,
        "FeaturedImageUrl" character varying(500),
        "Author" character varying(100),
        "AuthorAr" character varying(100),
        "AuthorAvatarUrl" character varying(500),
        "Category" character varying(50) NOT NULL,
        "TagsJson" text,
        "ReadTimeMinutes" integer NOT NULL,
        "Status" character varying(20) NOT NULL,
        "PublishDate" timestamp with time zone,
        "ScheduledPublishDate" timestamp with time zone,
        "ViewCount" integer NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "MetaTitle" character varying(70),
        "MetaDescription" character varying(160),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        CONSTRAINT "PK_BlogPosts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "ClientLogos" (
        "Id" uuid NOT NULL,
        "ClientName" character varying(100) NOT NULL,
        "ClientNameAr" character varying(100),
        "LogoUrl" character varying(500) NOT NULL,
        "WebsiteUrl" character varying(500),
        "Industry" character varying(100),
        "IndustryAr" character varying(100),
        "Category" character varying(50) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClientLogos" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "Faqs" (
        "Id" uuid NOT NULL,
        "Question" character varying(500) NOT NULL,
        "QuestionAr" character varying(500),
        "Answer" character varying(2000) NOT NULL,
        "AnswerAr" character varying(2000),
        "Category" character varying(50) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Faqs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "FeatureHighlights" (
        "Id" uuid NOT NULL,
        "Title" character varying(100) NOT NULL,
        "TitleAr" character varying(100),
        "Description" character varying(300),
        "DescriptionAr" character varying(300),
        "IconClass" character varying(50),
        "ImageUrl" character varying(500),
        "LearnMoreUrl" character varying(300),
        "Category" character varying(50) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FeatureHighlights" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "LandingPageContents" (
        "Id" uuid NOT NULL,
        "PageKey" character varying(50) NOT NULL,
        "SectionKey" character varying(50) NOT NULL,
        "ContentKey" character varying(50) NOT NULL,
        "ContentValue" text,
        "ContentValueAr" text,
        "ContentType" character varying(20) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        CONSTRAINT "PK_LandingPageContents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "LandingStatistics" (
        "Id" uuid NOT NULL,
        "Label" character varying(100) NOT NULL,
        "LabelAr" character varying(100),
        "Value" character varying(50) NOT NULL,
        "Suffix" character varying(10),
        "IconClass" character varying(50),
        "Category" character varying(50) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_LandingStatistics" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "MarketingTeamMembers" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "NameAr" character varying(100),
        "Title" character varying(100) NOT NULL,
        "TitleAr" character varying(100),
        "Bio" character varying(500),
        "BioAr" character varying(500),
        "PhotoUrl" character varying(500),
        "Email" character varying(200),
        "LinkedInUrl" character varying(200),
        "TwitterUrl" character varying(200),
        "Department" character varying(50) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_MarketingTeamMembers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "Partners" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "NameAr" character varying(100),
        "Description" character varying(300),
        "DescriptionAr" character varying(300),
        "LogoUrl" character varying(500) NOT NULL,
        "WebsiteUrl" character varying(500),
        "Type" character varying(50) NOT NULL,
        "Tier" character varying(20) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Partners" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "RequirementNotes" (
        "Id" uuid NOT NULL,
        "AssessmentRequirementId" uuid NOT NULL,
        "Content" text NOT NULL,
        "NoteType" character varying(50) NOT NULL,
        "IsInternal" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RequirementNotes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequirementNotes_AssessmentRequirements_AssessmentRequireme~" FOREIGN KEY ("AssessmentRequirementId") REFERENCES "AssessmentRequirements" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "TrustBadges" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "NameAr" character varying(100),
        "Description" character varying(200),
        "DescriptionAr" character varying(200),
        "ImageUrl" character varying(500) NOT NULL,
        "VerificationUrl" character varying(500),
        "Category" character varying(50) NOT NULL,
        "BadgeCode" character varying(50),
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_TrustBadges" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE TABLE "Webinars" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "TitleAr" character varying(200),
        "Slug" character varying(300),
        "Description" character varying(1000),
        "DescriptionAr" character varying(1000),
        "ThumbnailUrl" character varying(500),
        "VideoUrl" character varying(500),
        "RegistrationUrl" character varying(500),
        "DurationMinutes" integer NOT NULL,
        "SpeakersJson" text,
        "TopicsJson" text,
        "Type" character varying(20) NOT NULL,
        "ScheduledDate" timestamp with time zone,
        "RegistrationCount" integer NOT NULL,
        "ViewCount" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Webinars" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE INDEX "IX_Evidences_AssessmentRequirementId" ON "Evidences" ("AssessmentRequirementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE INDEX "IX_RequirementNotes_AssessmentRequirementId" ON "RequirementNotes" ("AssessmentRequirementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    CREATE INDEX "IX_RequirementNotes_AssessmentRequirementId_CreatedDate" ON "RequirementNotes" ("AssessmentRequirementId", "CreatedDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    ALTER TABLE "Evidences" ADD CONSTRAINT "FK_Evidences_AssessmentRequirements_AssessmentRequirementId" FOREIGN KEY ("AssessmentRequirementId") REFERENCES "AssessmentRequirements" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108123644_AddMarketingCmsEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108123644_AddMarketingCmsEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkspaceTemplates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Workspaces" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkspaceMemberships" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkspaceControls" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkspaceApprovalGates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkspaceApprovalGateApprovers" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowTasks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Workflows" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowInstances" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowExecutions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowEscalations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowDefinitions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WorkflowAuditEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WebhookSubscriptions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "WebhookDeliveryLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Vendors" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ValidationRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ValidationResults" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserWorkspaceTasks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserWorkspaces" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserProfiles" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserProfileAssignments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserNotificationPreferences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UserConsents" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UniversalEvidencePacks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UniversalEvidencePackItems" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "UITextEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TriggerRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TriggerExecutionLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TrialRequests" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TitleCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ThirdPartyConcentrations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TestProcedures" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantWorkflowConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantUsers" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantTemplates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Tenants" ADD "BusinessCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Tenants" ADD "TenantCode" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantPackages" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantEvidenceRequirements" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TenantBaselines" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TemplateCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TeamsNotificationConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Teams" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TeamMembers" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TaskDelegations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "TaskComments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SystemOfRecordDefinitions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SyncJobs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SyncExecutionLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SupportMessages" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SupportConversations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SuiteEvidenceRequests" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SuiteControlEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Subscriptions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SubscriptionPlans" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "StrategicRoadmapMilestones" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "StandardEvidenceItems" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SoDRuleDefinitions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SoDConflicts" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SlaRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SiteMapEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ShahinAIModules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ShahinAIBrandConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "SectorFrameworkIndex" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Rulesets" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Rules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RuleExecutionLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RoleTransitionPlans" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RoleProfiles" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RoleLandingConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RoleCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Risks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RiskResiliences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RiskIndicators" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RiskIndicatorMeasurements" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RiskIndicatorAlerts" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Resiliences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RequirementNotes" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RequirementMappings" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Reports" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RegulatoryRequirements" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Regulators" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RegulatorCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "RACIAssignments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PolicyViolations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PolicyDecisions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Policies" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Policies" ADD "SupersedesDocumentCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Policies" ADD "VersionMajor" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Policies" ADD "VersionMinor" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Policies" ADD "VersionNotes" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PlatformAdmins" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Plans" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PlanPhases" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PlainLanguageControls" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PendingApprovals" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Payments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "PackageCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OwnerTenantCreations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OverlayParameterOverrides" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OverlayControlMappings" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OverlayCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OrganizationProfiles" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OrganizationEntities" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OnePageGuides" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "OnboardingWizards" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "MappingWorkflowTemplates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "MappingWorkflowSteps" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "MappingQualityGates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "MAPFrameworkConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "LlmConfigurations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "LegalDocuments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Invoices" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "IntegrationHealthMetrics" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "IntegrationConnectors" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ImportantBusinessServices" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "HumanRetainedResponsibilities" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "GovernanceRhythmTemplates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "GovernanceRhythmItems" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "GovernanceCadences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "GeneratedControlSuites" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Frameworks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "FrameworkControls" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "FrameworkCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidenceTypeCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidenceSourceIntegrations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidenceScoringCriteria" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidenceScores" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "EvidencePeriodEnd" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "EvidencePeriodStart" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "FileHash" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "FileVersion" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "OriginalUploadDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "OriginalUploader" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "RetentionEndDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Evidences" ADD "SourceSystem" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidencePacks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EvidencePackFamilies" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EventSubscriptions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EventSchemaRegistries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EventDeliveryLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EscalationRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ERPSystemConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ERPExtractExecutions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ERPExtractConfigs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailThreads" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailTemplates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailTasks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailMessages" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailMailboxes" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailAutoReplyRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "EmailAttachments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "DomainEvents" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "DelegationRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "DelegationLogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "DeadLetterEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "DataQualityScores" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CryptographicAssets" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CrossReferenceMappings" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlTestProcedures" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Controls" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Controls" ADD "SourceControlCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Controls" ADD "SourceControlTitle" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Controls" ADD "SourceFrameworkCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlObjectives" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlExceptions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlEvidencePacks" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlDomains" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlChangeHistories" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ControlCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ComplianceGuardrails" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ComplianceEvents" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CCMTestExecutions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CCMExceptions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CCMControlTests" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CapturedEvidences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CanonicalControls" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "CadenceExecutions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "BaselineControlSets" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "BaselineControlMappings" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "BaselineCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AutoTaggedEvidences" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Audits" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AuditFindings" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AuditEvents" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Assets" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AssessmentScopes" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "Assessments" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AssessmentRequirements" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApprovalRecords" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApprovalInstances" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApprovalChains" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApplicabilityRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApplicabilityRuleCatalogs" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ApplicabilityEntries" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentSoDViolations" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentSoDRules" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentDefinitions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentConfidenceScores" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentCapabilities" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentApprovalGates" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "AgentActions" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    ALTER TABLE "ActionPlans" ADD "BusinessCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "RiskCategories" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "NameAr" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "DescriptionAr" character varying(2000) NOT NULL,
        "ParentCategoryId" uuid,
        "Code" character varying(20) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "DefaultRiskAppetite" integer NOT NULL,
        "EscalationThreshold" integer NOT NULL,
        "EscalationRoles" character varying(1000) NOT NULL,
        "IsActive" boolean NOT NULL,
        "IconClass" character varying(100) NOT NULL,
        "ColorCode" character varying(20) NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskCategories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskCategories_RiskCategories_ParentCategoryId" FOREIGN KEY ("ParentCategoryId") REFERENCES "RiskCategories" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "RiskControlMappings" (
        "Id" uuid NOT NULL,
        "RiskId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "MappingStrength" character varying(50) NOT NULL,
        "ExpectedEffectiveness" integer NOT NULL,
        "ActualEffectiveness" integer NOT NULL,
        "Rationale" character varying(2000) NOT NULL,
        "IsActive" boolean NOT NULL,
        "LastAssessedDate" timestamp with time zone,
        "LastAssessedBy" character varying(256),
        "AssessmentNotes" character varying(2000),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskControlMappings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskControlMappings_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RiskControlMappings_Risks_RiskId" FOREIGN KEY ("RiskId") REFERENCES "Risks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "RiskTreatments" (
        "Id" uuid NOT NULL,
        "RiskId" uuid NOT NULL,
        "TreatmentType" character varying(50) NOT NULL,
        "Description" character varying(4000) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Owner" character varying(256) NOT NULL,
        "TargetDate" timestamp with time zone,
        "CompletionDate" timestamp with time zone,
        "ExpectedResidualRisk" integer NOT NULL,
        "ActualResidualRisk" integer,
        "EstimatedCost" numeric NOT NULL,
        "ActualCost" numeric,
        "TransferParty" character varying(500) NOT NULL,
        "AcceptanceJustification" character varying(2000) NOT NULL,
        "ApprovedBy" character varying(256) NOT NULL,
        "ApprovalDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskTreatments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskTreatments_Risks_RiskId" FOREIGN KEY ("RiskId") REFERENCES "Risks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "SerialCounters" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ObjectType" character varying(10) NOT NULL,
        "Year" integer NOT NULL,
        "NextValue" bigint NOT NULL,
        "RowVersion" bytea,
        "LastUpdated" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_SerialCounters" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "RiskTypes" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "NameAr" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "CategoryId" uuid NOT NULL,
        "Code" character varying(20) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskTypes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskTypes_RiskCategories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "RiskCategories" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE TABLE "RiskTreatmentControls" (
        "Id" uuid NOT NULL,
        "TreatmentId" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "ExpectedEffectiveness" integer NOT NULL,
        "Status" character varying(50) NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskTreatmentControls" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskTreatmentControls_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RiskTreatmentControls_RiskTreatments_TreatmentId" FOREIGN KEY ("TreatmentId") REFERENCES "RiskTreatments" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskCategories_ParentCategoryId" ON "RiskCategories" ("ParentCategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskControlMappings_ControlId" ON "RiskControlMappings" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskControlMappings_RiskId" ON "RiskControlMappings" ("RiskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskTreatmentControls_ControlId" ON "RiskTreatmentControls" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskTreatmentControls_TreatmentId" ON "RiskTreatmentControls" ("TreatmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskTreatments_RiskId" ON "RiskTreatments" ("RiskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    CREATE INDEX "IX_RiskTypes_CategoryId" ON "RiskTypes" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183539_AddSerialCodeInfrastructure') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108183539_AddSerialCodeInfrastructure', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ChamberMembership" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "CrExpiryDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "DataTransferCountries" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "GosiNumber" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasCrossBorderTransfer" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasEInvoicingPhase1" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "HasEInvoicingPhase2" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "IsTadawulListed" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "MisaLicenseNumber" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "MisaLicenseType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "MunicipalLicense" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "NitaqatCategory" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ProcessesBiometricData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ProcessesLocationData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ProcessesNationalIdData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RequiresDataLocalization" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RequiresEsgReporting" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "RequiresIfrsCompliance" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "SasoCertification" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "SaudizationPercent" numeric NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "Vision2030Kpis" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "Vision2030Program" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    ALTER TABLE "OrganizationProfiles" ADD "ZakatCertExpiry" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108183841_AddKsaRegulatoryFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108183841_AddKsaRegulatoryFields', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108193529_GrcEnhancements_RiskResilienceSustainability') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108193529_GrcEnhancements_RiskResilienceSustainability', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108195836_AddUnifiedAiProviderConfiguration') THEN
    CREATE TABLE "AiProviderConfigurations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "Name" text NOT NULL,
        "Provider" text NOT NULL,
        "ApiEndpoint" text,
        "ApiKey" text NOT NULL,
        "ModelId" text NOT NULL,
        "ApiVersion" text,
        "MaxTokens" integer NOT NULL,
        "Temperature" numeric NOT NULL,
        "TopP" numeric NOT NULL,
        "TimeoutSeconds" integer NOT NULL,
        "IsDefault" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "MonthlyUsageLimit" integer NOT NULL,
        "CurrentMonthUsage" integer NOT NULL,
        "LastUsageResetDate" timestamp with time zone,
        "CustomHeaders" text,
        "RequestTemplate" text,
        "ResponsePath" text,
        "SystemPrompt" text,
        "AllowedUseCases" text NOT NULL,
        "ConfiguredAt" timestamp with time zone NOT NULL,
        "ConfiguredBy" text,
        "LastUsedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_AiProviderConfigurations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AiProviderConfigurations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108195836_AddUnifiedAiProviderConfiguration') THEN
    CREATE INDEX "IX_AiProviderConfigurations_TenantId" ON "AiProviderConfigurations" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108195836_AddUnifiedAiProviderConfiguration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108195836_AddUnifiedAiProviderConfiguration', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE TABLE "SerialCodeRegistry" (
        "Id" uuid NOT NULL,
        "Code" character varying(35) NOT NULL,
        "Prefix" character varying(10) NOT NULL,
        "TenantCode" character varying(6) NOT NULL,
        "Stage" integer NOT NULL,
        "Year" integer NOT NULL,
        "Sequence" integer NOT NULL,
        "Version" integer NOT NULL,
        "EntityType" character varying(50) NOT NULL,
        "EntityId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "StatusReason" character varying(500),
        "Metadata" text,
        "PreviousVersionCode" character varying(35),
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(256) NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(256),
        "RowVersion" bytea,
        CONSTRAINT "PK_SerialCodeRegistry" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE TABLE "SerialCodeReservations" (
        "Id" uuid NOT NULL,
        "ReservedCode" character varying(35) NOT NULL,
        "Prefix" character varying(10) NOT NULL,
        "TenantCode" character varying(6) NOT NULL,
        "Stage" integer NOT NULL,
        "Year" integer NOT NULL,
        "Sequence" integer NOT NULL,
        "Status" character varying(20) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "ConfirmedAt" timestamp with time zone,
        "CancelledAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(256) NOT NULL,
        CONSTRAINT "PK_SerialCodeReservations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE TABLE "SerialSequenceCounters" (
        "Id" uuid NOT NULL,
        "Prefix" character varying(10) NOT NULL,
        "TenantCode" character varying(6) NOT NULL,
        "Stage" integer NOT NULL,
        "Year" integer NOT NULL,
        "CurrentSequence" integer NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "RowVersion" bytea,
        CONSTRAINT "PK_SerialSequenceCounters" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE UNIQUE INDEX "IX_SerialCodeRegistry_Code" ON "SerialCodeRegistry" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Created" ON "SerialCodeRegistry" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Entity" ON "SerialCodeRegistry" ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Prefix" ON "SerialCodeRegistry" ("Prefix");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE UNIQUE INDEX "IX_SerialCodeRegistry_Sequence" ON "SerialCodeRegistry" ("Prefix", "TenantCode", "Stage", "Year", "Sequence");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Stage" ON "SerialCodeRegistry" ("Stage");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Status" ON "SerialCodeRegistry" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Tenant" ON "SerialCodeRegistry" ("TenantCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeRegistry_Year" ON "SerialCodeRegistry" ("Year");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE UNIQUE INDEX "IX_SerialCodeReservation_Code" ON "SerialCodeReservations" ("ReservedCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeReservation_Expires" ON "SerialCodeReservations" ("ExpiresAt") WHERE "Status" = 'reserved';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE INDEX "IX_SerialCodeReservation_Status" ON "SerialCodeReservations" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    CREATE UNIQUE INDEX "IX_SerialSequenceCounter_Unique" ON "SerialSequenceCounters" ("Prefix", "TenantCode", "Stage", "Year");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108212931_AddSerialCodeRegistry') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108212931_AddSerialCodeRegistry', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "Certifications" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "NameAr" character varying(200),
        "Code" character varying(50) NOT NULL,
        "Description" text,
        "Category" character varying(100) NOT NULL,
        "Type" character varying(100) NOT NULL,
        "IssuingBody" character varying(200) NOT NULL,
        "IssuingBodyAr" character varying(200),
        "Status" character varying(50) NOT NULL,
        "CertificationNumber" character varying(100),
        "Scope" character varying(2000),
        "IssuedDate" timestamp with time zone,
        "ExpiryDate" timestamp with time zone,
        "LastRenewalDate" timestamp with time zone,
        "NextSurveillanceDate" timestamp with time zone,
        "NextRecertificationDate" timestamp with time zone,
        "RenewalLeadDays" integer NOT NULL,
        "Level" text,
        "StandardVersion" text,
        "OwnerId" character varying(100),
        "OwnerName" character varying(200),
        "Department" character varying(100),
        "AuditorName" text,
        "Cost" numeric,
        "CostCurrency" text NOT NULL,
        "CertificateUrl" text,
        "Notes" text,
        "LinkedFrameworkCode" text,
        "IsMandatory" boolean NOT NULL,
        "MandatorySource" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Certifications" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "ControlOwnerAssignments" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "OwnerId" character varying(100) NOT NULL,
        "OwnerName" character varying(200) NOT NULL,
        "OwnerEmail" character varying(200),
        "Department" character varying(100),
        "AssignmentType" character varying(50) NOT NULL,
        "AssignedDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone,
        "AssignedById" text,
        "AssignedByName" text,
        "Reason" character varying(1000),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ControlOwnerAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlOwnerAssignments_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "ControlTests" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "TestType" character varying(50) NOT NULL,
        "TestMethodology" character varying(500),
        "SampleSize" integer,
        "PopulationSize" integer,
        "ExceptionsFound" integer NOT NULL,
        "Score" integer NOT NULL,
        "Result" character varying(50) NOT NULL,
        "Findings" character varying(4000),
        "Recommendations" character varying(4000),
        "TestNotes" character varying(4000),
        "TesterId" text,
        "TesterName" character varying(200) NOT NULL,
        "TestedDate" timestamp with time zone NOT NULL,
        "ReviewedDate" timestamp with time zone,
        "ReviewerId" text,
        "ReviewerName" character varying(200),
        "ReviewStatus" character varying(50) NOT NULL,
        "PreviousEffectiveness" integer NOT NULL,
        "NewEffectiveness" integer NOT NULL,
        "PeriodStart" timestamp with time zone,
        "PeriodEnd" timestamp with time zone,
        "EvidenceIds" text,
        "NextTestDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ControlTests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ControlTests_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "Incidents" (
        "Id" uuid NOT NULL,
        "IncidentNumber" character varying(50) NOT NULL,
        "Title" character varying(500) NOT NULL,
        "TitleAr" character varying(500),
        "Description" text,
        "Category" character varying(100) NOT NULL,
        "Type" character varying(100) NOT NULL,
        "Severity" character varying(50) NOT NULL,
        "Priority" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Phase" character varying(50) NOT NULL,
        "DetectionSource" character varying(200),
        "DetectedAt" timestamp with time zone NOT NULL,
        "OccurredAt" timestamp with time zone,
        "ContainedAt" timestamp with time zone,
        "EradicatedAt" timestamp with time zone,
        "RecoveredAt" timestamp with time zone,
        "ClosedAt" timestamp with time zone,
        "ReportedById" text,
        "ReportedByName" text,
        "HandlerId" character varying(100),
        "HandlerName" character varying(200),
        "AssignedTeam" character varying(100),
        "AffectedSystems" text,
        "AffectedBusinessUnits" text,
        "AffectedUsersCount" integer,
        "AffectedRecordsCount" integer,
        "PersonalDataAffected" boolean NOT NULL,
        "RootCause" character varying(4000),
        "ContainmentActions" text,
        "EradicationActions" text,
        "RecoveryActions" text,
        "LessonsLearned" character varying(4000),
        "Recommendations" text,
        "RequiresNotification" boolean NOT NULL,
        "RegulatorsToNotify" text,
        "NotificationSent" boolean NOT NULL,
        "NotificationDeadline" timestamp with time zone,
        "NotificationSentDate" timestamp with time zone,
        "EstimatedImpact" numeric,
        "ActualImpact" numeric,
        "ImpactCurrency" text NOT NULL,
        "RelatedRiskIds" text,
        "RelatedControlIds" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Incidents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "CertificationAudits" (
        "Id" uuid NOT NULL,
        "CertificationId" uuid NOT NULL,
        "AuditType" character varying(50) NOT NULL,
        "AuditDate" timestamp with time zone NOT NULL,
        "AuditorName" character varying(200),
        "LeadAuditorName" character varying(200),
        "Result" character varying(50) NOT NULL,
        "MajorFindings" integer NOT NULL,
        "MinorFindings" integer NOT NULL,
        "Observations" integer NOT NULL,
        "CorrectiveActionDeadline" timestamp with time zone,
        "CorrectiveActionsCompleted" boolean NOT NULL,
        "ReportReference" character varying(500),
        "Cost" numeric,
        "Notes" character varying(4000),
        "NextAuditDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_CertificationAudits" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CertificationAudits_Certifications_CertificationId" FOREIGN KEY ("CertificationId") REFERENCES "Certifications" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE TABLE "IncidentTimelineEntries" (
        "Id" uuid NOT NULL,
        "IncidentId" uuid NOT NULL,
        "EntryType" character varying(50) NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(4000),
        "Phase" text,
        "StatusBefore" text,
        "StatusAfter" text,
        "PerformedById" text,
        "PerformedByName" character varying(200) NOT NULL,
        "Timestamp" timestamp with time zone NOT NULL,
        "IsInternal" boolean NOT NULL,
        "Attachments" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_IncidentTimelineEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IncidentTimelineEntries_Incidents_IncidentId" FOREIGN KEY ("IncidentId") REFERENCES "Incidents" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_CertificationAudits_AuditDate" ON "CertificationAudits" ("AuditDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_CertificationAudits_CertificationId" ON "CertificationAudits" ("CertificationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_CertificationAudits_Result" ON "CertificationAudits" ("Result");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_Code" ON "Certifications" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_ExpiryDate" ON "Certifications" ("ExpiryDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_Status" ON "Certifications" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_TenantId" ON "Certifications" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_TenantId_Category" ON "Certifications" ("TenantId", "Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Certifications_TenantId_Status" ON "Certifications" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlOwnerAssignments_ControlId" ON "ControlOwnerAssignments" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlOwnerAssignments_ControlId_IsActive" ON "ControlOwnerAssignments" ("ControlId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlOwnerAssignments_OwnerId" ON "ControlOwnerAssignments" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlTests_ControlId" ON "ControlTests" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlTests_TenantId" ON "ControlTests" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlTests_TenantId_ControlId_TestedDate" ON "ControlTests" ("TenantId", "ControlId", "TestedDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_ControlTests_TestedDate" ON "ControlTests" ("TestedDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_DetectedAt" ON "Incidents" ("DetectedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE UNIQUE INDEX "IX_Incidents_IncidentNumber" ON "Incidents" ("IncidentNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_Severity" ON "Incidents" ("Severity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_Status" ON "Incidents" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_TenantId" ON "Incidents" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_TenantId_Severity_Status" ON "Incidents" ("TenantId", "Severity", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_Incidents_TenantId_Status" ON "Incidents" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_IncidentTimelineEntries_IncidentId" ON "IncidentTimelineEntries" ("IncidentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    CREATE INDEX "IX_IncidentTimelineEntries_Timestamp" ON "IncidentTimelineEntries" ("Timestamp");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108232015_AddGapClosureEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108232015_AddGapClosureEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" DROP CONSTRAINT "FK_ControlOwnerAssignments_Controls_ControlId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" DROP CONSTRAINT "FK_ControlTests_Controls_ControlId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_IncidentTimelineEntries_Timestamp";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_DetectedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_IncidentNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_Severity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_TenantId_Severity_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Incidents_TenantId_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_ControlTests_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_ControlTests_TenantId_ControlId_TestedDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_ControlTests_TestedDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_ControlOwnerAssignments_ControlId_IsActive";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_ControlOwnerAssignments_OwnerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_Code";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_ExpiryDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_TenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_TenantId_Category";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_Certifications_TenantId_Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_CertificationAudits_AuditDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    DROP INDEX "IX_CertificationAudits_Result";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Tenants" ADD "Email" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Tenants" ADD "StripeCustomerId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Subscriptions" ADD "StripeSubscriptionId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "IncidentTimelineEntries" ALTER COLUMN "Title" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "IncidentTimelineEntries" ALTER COLUMN "PerformedByName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "IncidentTimelineEntries" ALTER COLUMN "EntryType" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "IncidentTimelineEntries" ALTER COLUMN "Description" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Type" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Status" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Severity" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "RootCause" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Priority" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Phase" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "LessonsLearned" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "IncidentNumber" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "HandlerName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "HandlerId" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "DetectionSource" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "Category" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Incidents" ALTER COLUMN "AssignedTeam" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "TesterName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "TestType" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "TestNotes" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "TestMethodology" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "ReviewerName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "ReviewStatus" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "Result" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "Recommendations" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ALTER COLUMN "Findings" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "Reason" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "OwnerName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "OwnerId" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "OwnerEmail" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "Department" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ALTER COLUMN "AssignmentType" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "Type" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "Status" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "Scope" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "OwnerName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "OwnerId" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "IssuingBodyAr" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "IssuingBody" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "Department" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "CertificationNumber" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "Certifications" ALTER COLUMN "Category" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "Result" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "ReportReference" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "Notes" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "LeadAuditorName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "AuditorName" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CertificationAudits" ALTER COLUMN "AuditType" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "Challenge" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "ChallengeAr" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "CustomerName" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "CustomerQuote" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "CustomerQuoteAr" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "CustomerTitle" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "CustomerTitleAr" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "Results" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "ResultsAr" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "Solution" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "CaseStudies" ADD "SolutionAr" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE TABLE "NewsletterSubscriptions" (
        "Id" uuid NOT NULL,
        "Email" character varying(256) NOT NULL,
        "Name" character varying(100),
        "IsActive" boolean NOT NULL,
        "Locale" character varying(10) NOT NULL,
        "Source" character varying(50) NOT NULL,
        "IpAddress" character varying(50),
        "SubscribedAt" timestamp with time zone NOT NULL,
        "UnsubscribedAt" timestamp with time zone,
        "ResubscribedAt" timestamp with time zone,
        "LastEmailSentAt" timestamp with time zone,
        "EmailsSentCount" integer NOT NULL,
        "EmailsOpenedCount" integer NOT NULL,
        "Interests" character varying(500),
        "UnsubscribeToken" character varying(100),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_NewsletterSubscriptions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE TABLE "OnboardingStepScores" (
        "Id" uuid NOT NULL,
        "OnboardingWizardId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "StepNumber" integer NOT NULL,
        "StepLetter" character varying(1) NOT NULL,
        "StepName" character varying(100) NOT NULL,
        "TotalPointsAvailable" integer NOT NULL,
        "PointsEarned" integer NOT NULL,
        "SpeedBonus" integer NOT NULL,
        "ThoroughnessBonus" integer NOT NULL,
        "QualityBonus" integer NOT NULL,
        "TotalScore" integer NOT NULL,
        "StarRating" integer NOT NULL,
        "AchievementLevel" character varying(20) NOT NULL,
        "TotalQuestions" integer NOT NULL,
        "RequiredQuestions" integer NOT NULL,
        "QuestionsAnswered" integer NOT NULL,
        "RequiredQuestionsAnswered" integer NOT NULL,
        "CompletionPercent" integer NOT NULL,
        "Status" character varying(20) NOT NULL,
        "EstimatedTimeMinutes" integer NOT NULL,
        "ActualTimeMinutes" integer NOT NULL,
        "StartedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "AssessmentTemplateId" uuid,
        "AssessmentTemplateName" character varying(200) NOT NULL,
        "AssessmentInstanceId" uuid,
        "AssessmentStatus" character varying(20) NOT NULL,
        "GrcRequirementIdsJson" text NOT NULL,
        "GrcRequirementsCount" integer NOT NULL,
        "GrcRequirementsSatisfied" integer NOT NULL,
        "ComplianceFrameworksJson" text NOT NULL,
        "WorkflowId" uuid,
        "WorkflowName" character varying(200) NOT NULL,
        "WorkflowInstanceId" uuid,
        "WorkflowStatus" character varying(20) NOT NULL,
        "WorkflowTasksJson" text NOT NULL,
        "ValidationErrorsJson" text NOT NULL,
        "ValidationAttempts" integer NOT NULL,
        "DataQualityScore" integer NOT NULL,
        "CompletenessScore" integer NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_OnboardingStepScores" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OnboardingStepScores_OnboardingWizards_OnboardingWizardId" FOREIGN KEY ("OnboardingWizardId") REFERENCES "OnboardingWizards" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_OnboardingStepScores_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE TABLE "RiskAppetiteSettings" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(2000),
        "MinimumRiskScore" integer NOT NULL,
        "MaximumRiskScore" integer NOT NULL,
        "TargetRiskScore" integer NOT NULL,
        "TolerancePercentage" integer NOT NULL,
        "ImpactThreshold" integer NOT NULL,
        "LikelihoodThreshold" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "ApprovedDate" timestamp with time zone,
        "ApprovedBy" character varying(450),
        "ExpiryDate" timestamp with time zone,
        "ReviewReminderDays" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(450),
        "UpdatedAt" timestamp with time zone,
        "UpdatedBy" character varying(450),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskAppetiteSettings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskAppetiteSettings_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE TABLE "TrialSignups" (
        "Id" uuid NOT NULL,
        "Email" character varying(256) NOT NULL,
        "FullName" character varying(100) NOT NULL,
        "CompanyName" character varying(200) NOT NULL,
        "PhoneNumber" character varying(20),
        "CompanySize" character varying(20),
        "Industry" character varying(100),
        "TrialPlan" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "IpAddress" character varying(50),
        "UserAgent" character varying(500),
        "Locale" character varying(10) NOT NULL,
        "UtmSource" character varying(100),
        "UtmMedium" character varying(100),
        "UtmCampaign" character varying(100),
        "ReferrerUrl" character varying(500),
        "LandingPageUrl" character varying(500),
        "Notes" character varying(1000),
        "ContactedAt" timestamp with time zone,
        "ConvertedAt" timestamp with time zone,
        "TrialRequestId" uuid,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TrialSignups" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrialSignups_TrialRequests_TrialRequestId" FOREIGN KEY ("TrialRequestId") REFERENCES "TrialRequests" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE INDEX "IX_OnboardingStepScores_OnboardingWizardId" ON "OnboardingStepScores" ("OnboardingWizardId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE INDEX "IX_OnboardingStepScores_TenantId" ON "OnboardingStepScores" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE INDEX "IX_RiskAppetiteSettings_TenantId" ON "RiskAppetiteSettings" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    CREATE INDEX "IX_TrialSignups_TrialRequestId" ON "TrialSignups" ("TrialRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlOwnerAssignments" ADD CONSTRAINT "FK_ControlOwnerAssignments_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    ALTER TABLE "ControlTests" ADD CONSTRAINT "FK_ControlTests_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113093418_AddTenantEmailColumn') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260113093418_AddTenantEmailColumn', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    ALTER TABLE "EmailMailboxes" RENAME COLUMN "TenantId" TO "AzureTenantId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    ALTER TABLE "EmailMailboxes" ADD "TenantId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "BaselineProfiles" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(1000),
        "SourceFramework" character varying(50),
        "ImpactLevel" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "EffectiveDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_BaselineProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "CanonicalControlMappings" (
        "Id" uuid NOT NULL,
        "SourceControlId" uuid NOT NULL,
        "TargetControlId" uuid NOT NULL,
        "SourceFramework" character varying(50) NOT NULL,
        "TargetFramework" character varying(50) NOT NULL,
        "MappingStrength" character varying(20) NOT NULL,
        "Notes" character varying(1000),
        "ConfidenceScore" integer NOT NULL,
        "IsVerified" boolean NOT NULL,
        "VerifiedBy" character varying(200),
        "VerifiedAt" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_CanonicalControlMappings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ControlApplicabilityRules" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "RuleType" character varying(20) NOT NULL,
        "ConditionField" character varying(100) NOT NULL,
        "ConditionOperator" character varying(20) NOT NULL,
        "ConditionValue" character varying(500) NOT NULL,
        "TargetControlId" uuid,
        "TargetFramework" character varying(50),
        "Priority" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ControlApplicabilityRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ControlInheritances" (
        "Id" uuid NOT NULL,
        "ChildControlId" uuid NOT NULL,
        "ParentControlId" uuid NOT NULL,
        "InheritanceType" character varying(20) NOT NULL,
        "InheritancePercentage" integer,
        "Notes" character varying(1000),
        "InheritedAspectsJson" text,
        "IsActive" boolean NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ControlInheritances" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ControlOverlays" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(1000),
        "OverlayType" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL,
        "ConfigurationJson" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ControlOverlays" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "MAPProfiles" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(1000),
        "CriticalityLevel" integer NOT NULL,
        "ConfidentialityImpact" character varying(20) NOT NULL,
        "IntegrityImpact" character varying(20) NOT NULL,
        "AvailabilityImpact" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_MAPProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ResilienceCapabilities" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(1000),
        "CapabilityType" character varying(50) NOT NULL,
        "RtoHours" integer,
        "RpoHours" integer,
        "MaturityLevel" integer NOT NULL,
        "LastTestedAt" timestamp with time zone,
        "LastTestResult" character varying(20),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ResilienceCapabilities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "RiskAppetiteStatements" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "RiskCategory" character varying(100),
        "Statement" character varying(2000),
        "AppetiteLevel" character varying(20) NOT NULL,
        "ThresholdValue" numeric,
        "ThresholdUnit" character varying(20),
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "ApprovedBy" character varying(200),
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskAppetiteStatements" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "RiskScenarios" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(2000),
        "ThreatActorType" character varying(50),
        "AttackVector" character varying(100),
        "TargetAssetType" character varying(100),
        "Likelihood" integer NOT NULL,
        "Impact" integer NOT NULL,
        "InherentRiskScore" numeric NOT NULL,
        "ResidualRiskScore" numeric NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskScenarios" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "RiskTaxonomies" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(1000),
        "ParentId" uuid,
        "Level" integer NOT NULL,
        "SortOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RiskTaxonomies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RiskTaxonomies_RiskTaxonomies_ParentId" FOREIGN KEY ("ParentId") REFERENCES "RiskTaxonomies" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ThreatProfiles" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(50),
        "Description" character varying(2000),
        "ThreatType" character varying(50) NOT NULL,
        "ThreatActor" character varying(100),
        "CapabilityLevel" integer NOT NULL,
        "IntentLevel" integer NOT NULL,
        "TargetingLikelihood" integer NOT NULL,
        "MitreAttackTechniquesJson" text,
        "TargetedSectors" character varying(500),
        "IsActive" boolean NOT NULL,
        "LastUpdated" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ThreatProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "VulnerabilityProfiles" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "CveId" character varying(50),
        "Description" character varying(2000),
        "CvssScore" numeric,
        "Severity" character varying(20) NOT NULL,
        "AffectedSystems" character varying(500),
        "ExploitAvailability" character varying(20) NOT NULL,
        "RemediationStatus" character varying(20) NOT NULL,
        "RemediationPlan" character varying(2000),
        "DiscoveredAt" timestamp with time zone,
        "RemediationDeadline" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_VulnerabilityProfiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "TailoringDecisions" (
        "Id" uuid NOT NULL,
        "BaselineProfileId" uuid NOT NULL,
        "ControlId" uuid,
        "Decision" character varying(50) NOT NULL,
        "Justification" character varying(2000),
        "ModifiedRequirement" text,
        "CompensatingControlId" uuid,
        "DecisionDate" timestamp with time zone NOT NULL,
        "ApprovedBy" character varying(200),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TailoringDecisions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TailoringDecisions_BaselineProfiles_BaselineProfileId" FOREIGN KEY ("BaselineProfileId") REFERENCES "BaselineProfiles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TailoringDecisions_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "ApplicabilityDecisions" (
        "Id" uuid NOT NULL,
        "ControlId" uuid NOT NULL,
        "AssetId" uuid,
        "Decision" character varying(50) NOT NULL,
        "Justification" character varying(2000),
        "ApplicabilityRuleId" uuid,
        "IsAutomated" boolean NOT NULL,
        "DecisionDate" timestamp with time zone NOT NULL,
        "DecidedBy" character varying(200),
        "NextReviewDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ApplicabilityDecisions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApplicabilityDecisions_ControlApplicabilityRules_Applicabil~" FOREIGN KEY ("ApplicabilityRuleId") REFERENCES "ControlApplicabilityRules" ("Id"),
        CONSTRAINT "FK_ApplicabilityDecisions_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "MAPControlImplementations" (
        "Id" uuid NOT NULL,
        "MAPProfileId" uuid NOT NULL,
        "ControlId" uuid,
        "Status" character varying(50) NOT NULL,
        "ImplementationDescription" text,
        "ResponsibleParty" character varying(200),
        "TargetDate" timestamp with time zone,
        "CompletionDate" timestamp with time zone,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_MAPControlImplementations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MAPControlImplementations_Controls_ControlId" FOREIGN KEY ("ControlId") REFERENCES "Controls" ("Id"),
        CONSTRAINT "FK_MAPControlImplementations_MAPProfiles_MAPProfileId" FOREIGN KEY ("MAPProfileId") REFERENCES "MAPProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE TABLE "MAPParameters" (
        "Id" uuid NOT NULL,
        "MAPControlImplementationId" uuid NOT NULL,
        "ParameterName" character varying(100) NOT NULL,
        "ParameterValue" character varying(500),
        "Description" character varying(1000),
        "DataType" character varying(20) NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_MAPParameters" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MAPParameters_MAPControlImplementations_MAPControlImplement~" FOREIGN KEY ("MAPControlImplementationId") REFERENCES "MAPControlImplementations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_ApplicabilityDecisions_ApplicabilityRuleId" ON "ApplicabilityDecisions" ("ApplicabilityRuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_ApplicabilityDecisions_ControlId" ON "ApplicabilityDecisions" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_MAPControlImplementations_ControlId" ON "MAPControlImplementations" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_MAPControlImplementations_MAPProfileId" ON "MAPControlImplementations" ("MAPProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_MAPParameters_MAPControlImplementationId" ON "MAPParameters" ("MAPControlImplementationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_RiskTaxonomies_ParentId" ON "RiskTaxonomies" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_TailoringDecisions_BaselineProfileId" ON "TailoringDecisions" ("BaselineProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    CREATE INDEX "IX_TailoringDecisions_ControlId" ON "TailoringDecisions" ("ControlId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260113223313_AddBaselineEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260113223313_AddBaselineEntities', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "WorkflowTasks" ADD "CompletionPercentage" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "OnboardingWizards" ADD "MaturityLevel" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "DueDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "EvidenceTypeCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "FrameworkCode" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "HasDigitalSignature" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "IsAutoCollected" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "IsHybridCollection" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "ReviewerComments" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "SubmittedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    ALTER TABLE "Evidences" ADD "ValidUntil" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "AgentCommunicationContracts" (
        "Id" uuid NOT NULL,
        "ContractCode" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(500),
        "FromAgentCode" character varying(50) NOT NULL,
        "ToAgentCode" character varying(50) NOT NULL,
        "RequestSchemaJson" text NOT NULL,
        "ResponseSchemaJson" text NOT NULL,
        "ExpectedResponse" character varying(1000),
        "ErrorHandlingJson" text,
        "ValidationRulesJson" text,
        "ExampleJson" text,
        "TimeoutSeconds" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "Version" integer NOT NULL,
        "MessageType" character varying(100),
        "RetryPolicy" text,
        "MaxRetries" integer NOT NULL,
        "RequiresAcknowledgment" boolean NOT NULL,
        "PriorityLevel" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_AgentCommunicationContracts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "AgentEventTriggers" (
        "Id" uuid NOT NULL,
        "TriggerCode" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(500),
        "EventType" character varying(100) NOT NULL,
        "AgentCode" character varying(50) NOT NULL,
        "AgentAction" character varying(100) NOT NULL,
        "ConditionJson" text,
        "ParametersJson" text,
        "DelaySeconds" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "Priority" integer NOT NULL,
        "TenantId" uuid,
        "MaxDailyExecutions" integer,
        "CooldownSeconds" integer,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_AgentEventTriggers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "Badges" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IconUrl" text,
        "PointsValue" integer NOT NULL,
        "Category" text NOT NULL,
        "CriteriaJson" text,
        "IsActive" boolean NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_Badges" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "ConditionalLogicRules" (
        "Id" uuid NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "NameAr" character varying(200),
        "Description" character varying(1000),
        "Category" character varying(50) NOT NULL,
        "TriggerEvent" character varying(100),
        "ConditionJson" text NOT NULL,
        "ActionsJson" text NOT NULL,
        "Priority" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "StopOnMatch" boolean NOT NULL,
        "TenantId" uuid,
        "ApplicableFrameworks" character varying(500),
        "ApplicableRegions" character varying(500),
        "Version" integer NOT NULL,
        "EffectiveFrom" timestamp with time zone,
        "EffectiveTo" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ConditionalLogicRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "EngagementMetrics" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" uuid,
        "ConfidenceScore" integer NOT NULL,
        "FatigueScore" integer NOT NULL,
        "MomentumScore" integer NOT NULL,
        "OverallEngagementScore" integer NOT NULL,
        "SessionDurationMinutes" integer NOT NULL,
        "ActionsInSession" integer NOT NULL,
        "TasksCompletedToday" integer NOT NULL,
        "EvidenceSubmittedToday" integer NOT NULL,
        "ConsecutiveActiveDays" integer NOT NULL,
        "ErrorsEncounteredToday" integer NOT NULL,
        "HelpRequestsToday" integer NOT NULL,
        "AverageResponseTimeMinutes" double precision NOT NULL,
        "FactorBreakdownJson" text,
        "EngagementState" character varying(30),
        "RecommendedAction" character varying(500),
        "RecordedAt" timestamp with time zone NOT NULL,
        "TotalPoints" integer NOT NULL,
        "WeeklyPoints" integer NOT NULL,
        "MonthlyPoints" integer NOT NULL,
        "Level" integer NOT NULL,
        "CurrentStreak" integer NOT NULL,
        "LongestStreak" integer NOT NULL,
        "TotalActivities" integer NOT NULL,
        "BadgeCount" integer NOT NULL,
        "LastActivityAt" timestamp with time zone,
        "WeeklyPointsResetAt" timestamp with time zone,
        "MonthlyPointsResetAt" timestamp with time zone,
        "ActivityCountsJson" text,
        "WorkspaceId" uuid,
        "CurrentState" text,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EngagementMetrics" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "EvidenceConfidenceScores" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EvidenceId" uuid NOT NULL,
        "OverallScore" integer NOT NULL,
        "ConfidenceLevel" character varying(20) NOT NULL,
        "SourceCredibilityScore" integer NOT NULL,
        "CompletenessScore" integer NOT NULL,
        "RelevanceScore" integer NOT NULL,
        "TimelinessScore" integer NOT NULL,
        "AutomationCoveragePercent" integer NOT NULL,
        "CrossVerificationScore" integer NOT NULL,
        "FormatComplianceScore" integer NOT NULL,
        "SlaAdherenceDays" integer,
        "SlaMet" boolean,
        "CollectionMethod" character varying(30),
        "LowConfidenceFactorsJson" text,
        "RecommendedAction" character varying(30) NOT NULL,
        "HumanReviewTriggered" boolean NOT NULL,
        "HumanReviewOutcome" character varying(30),
        "ReviewerFeedback" character varying(1000),
        "ScoredAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EvidenceConfidenceScores" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "MotivationScores" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" uuid,
        "Score" integer NOT NULL,
        "InteractionQualityScore" integer NOT NULL,
        "ControlAlignmentScore" integer NOT NULL,
        "TaskImpactScore" integer NOT NULL,
        "ProgressVisibilityScore" integer NOT NULL,
        "AchievementRecognitionScore" integer NOT NULL,
        "AuditTrailJson" text,
        "MotivationLevel" character varying(30),
        "CalculatedAt" timestamp with time zone NOT NULL,
        "PreviousScore" integer,
        "EngagementScore" integer NOT NULL,
        "ConsistencyScore" integer NOT NULL,
        "ProgressScore" integer NOT NULL,
        "QualityScore" integer NOT NULL,
        "CollaborationScore" integer NOT NULL,
        "Trend" character varying(20),
        "RequiresIntervention" boolean NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_MotivationScores" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "NextBestActionRecommendations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "TargetUserId" uuid,
        "TargetRoleCode" character varying(50),
        "ActionId" character varying(100) NOT NULL,
        "ActionType" character varying(50) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "DescriptionAr" character varying(500),
        "ConfidenceScore" integer NOT NULL,
        "Priority" integer NOT NULL,
        "Rationale" character varying(2000),
        "ExpectedImpact" character varying(1000),
        "RelatedEntityType" character varying(50),
        "RelatedEntityId" uuid,
        "ActionParametersJson" text,
        "ContextDataJson" text,
        "TriggerConditionsJson" text,
        "Status" character varying(30) NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "ActedByUserId" uuid,
        "ActedAt" timestamp with time zone,
        "UserFeedback" character varying(500),
        "UserRating" integer,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_NextBestActionRecommendations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "ProgressCertaintyIndexes" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EntityType" character varying(50) NOT NULL,
        "EntityId" uuid,
        "Score" integer NOT NULL,
        "RiskBand" character varying(20) NOT NULL,
        "ConfidenceLevel" integer NOT NULL,
        "TasksCompletedPercent" double precision NOT NULL,
        "TaskVelocity" double precision NOT NULL,
        "VelocityTrend" character varying(20),
        "EvidenceRejectionRate" double precision NOT NULL,
        "SlaBreachCount" integer NOT NULL,
        "SlaAdherencePercent" double precision NOT NULL,
        "AverageOverdueDays" double precision NOT NULL,
        "OrgMaturityLevel" integer,
        "ComplexityScore" integer,
        "TotalTasks" integer NOT NULL,
        "CompletedTasks" integer NOT NULL,
        "OverdueTasks" integer NOT NULL,
        "AtRiskTasks" integer NOT NULL,
        "PrimaryRiskFactorsJson" text,
        "FactorBreakdownJson" text,
        "RecommendedIntervention" character varying(500),
        "PredictedCompletionDate" timestamp with time zone,
        "TargetCompletionDate" timestamp with time zone,
        "DaysFromBaseline" integer,
        "CalculatedAt" timestamp with time zone NOT NULL,
        "PreviousScore" integer,
        "ScoreChange" integer,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_ProgressCertaintyIndexes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "UserActivities" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ActivityType" text NOT NULL,
        "PointsEarned" integer NOT NULL,
        "MetadataJson" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_UserActivities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "AgentMessages" (
        "Id" uuid NOT NULL,
        "SourceAgentCode" text NOT NULL,
        "TargetAgentCode" text NOT NULL,
        "MessageType" text NOT NULL,
        "ContractId" uuid,
        "CorrelationId" uuid NOT NULL,
        "PayloadJson" text NOT NULL,
        "ResponsePayloadJson" text,
        "Status" text NOT NULL,
        "Priority" integer NOT NULL,
        "RequiresResponse" boolean NOT NULL,
        "TimeoutSeconds" integer NOT NULL,
        "RetryCount" integer NOT NULL,
        "QueuedAt" timestamp with time zone,
        "ProcessedAt" timestamp with time zone,
        "LastRetryAt" timestamp with time zone,
        "ProcessingTimeMs" integer,
        "ErrorMessage" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_AgentMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentMessages_AgentCommunicationContracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "AgentCommunicationContracts" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "AgentTriggerExecutions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "TriggerId" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "SourceEntityType" character varying(50),
        "SourceEntityId" uuid,
        "EventPayloadJson" text,
        "AgentInvoked" boolean NOT NULL,
        "AgentActionId" uuid,
        "Status" character varying(30) NOT NULL,
        "ErrorMessage" character varying(2000),
        "DurationMs" integer NOT NULL,
        "ExecutedAt" timestamp with time zone NOT NULL,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_AgentTriggerExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgentTriggerExecutions_AgentEventTriggers_TriggerId" FOREIGN KEY ("TriggerId") REFERENCES "AgentEventTriggers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "UserBadges" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "BadgeId" uuid NOT NULL,
        "AwardedAt" timestamp with time zone NOT NULL,
        "AwardReason" text,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_UserBadges" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserBadges_Badges_BadgeId" FOREIGN KEY ("BadgeId") REFERENCES "Badges" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE TABLE "RuleEvaluations" (
        "Id" uuid NOT NULL,
        "TenantId" uuid,
        "RuleId" uuid NOT NULL,
        "RuleCode" character varying(100),
        "TriggerEvent" character varying(100),
        "ContextJson" text,
        "ConditionResult" boolean NOT NULL,
        "TriggeredActionsJson" text,
        "Status" character varying(50) NOT NULL,
        "ErrorMessage" character varying(2000),
        "DurationMs" integer NOT NULL,
        "EvaluatedAt" timestamp with time zone NOT NULL,
        "EvaluatedBy" character varying(100),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_RuleEvaluations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RuleEvaluations_ConditionalLogicRules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "ConditionalLogicRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_ACC_From_To" ON "AgentCommunicationContracts" ("FromAgentCode", "ToAgentCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_AET_Event_Active" ON "AgentEventTriggers" ("EventType", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_AgentMessages_ContractId" ON "AgentMessages" ("ContractId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_AgentTriggerExecutions_TriggerId" ON "AgentTriggerExecutions" ("TriggerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_ATE_Tenant_Date" ON "AgentTriggerExecutions" ("TenantId", "ExecutedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_CLR_Active_Priority" ON "ConditionalLogicRules" ("IsActive", "Priority");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_CLR_Trigger" ON "ConditionalLogicRules" ("TriggerEvent");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_Engagement_Date" ON "EngagementMetrics" ("RecordedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_Engagement_Tenant_User" ON "EngagementMetrics" ("TenantId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_ECS_Evidence" ON "EvidenceConfidenceScores" ("EvidenceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_Motivation_Tenant_User" ON "MotivationScores" ("TenantId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_NBA_Tenant_Status" ON "NextBestActionRecommendations" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_NBA_User_Date" ON "NextBestActionRecommendations" ("TargetUserId", "CreatedDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_PCI_Entity" ON "ProgressCertaintyIndexes" ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_PCI_Tenant_Date" ON "ProgressCertaintyIndexes" ("TenantId", "CalculatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_RuleEvaluations_RuleId" ON "RuleEvaluations" ("RuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    CREATE INDEX "IX_UserBadges_BadgeId" ON "UserBadges" ("BadgeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114002457_FullplanEngagementSchemaCompatibility') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260114002457_FullplanEngagementSchemaCompatibility', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP CONSTRAINT "FK_TrialSignups_TrialRequests_TrialRequestId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    DROP INDEX "IX_TrialSignups_TrialRequestId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "CompanySize";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "FullName";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "Industry";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "LandingPageUrl";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "Locale";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "PhoneNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "ReferrerUrl";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "TrialRequestId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" DROP COLUMN "UserAgent";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "UtmSource" TO "Sector";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "UtmMedium" TO "LastName";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "UtmCampaign" TO "FirstName";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "TrialPlan" TO "Source";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "IpAddress" TO "ReferralCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "ConvertedAt" TO "ProvisionedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" RENAME COLUMN "ContactedAt" TO "ExpiredAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ALTER COLUMN "Notes" TYPE character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ALTER COLUMN "Email" TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ALTER COLUMN "CompanyName" TYPE character varying(255);
    ALTER TABLE "TrialSignups" ALTER COLUMN "CompanyName" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ADD "ActivatedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ADD "ActivationToken" character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TrialSignups" ADD "Phone" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TenantUsers" ADD "ActionsCompleted" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TenantUsers" ADD "ContributionScore" double precision NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "TenantUsers" ADD "LastActiveAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "FrameworkControls" ADD "EvidencePackCode" character varying(50) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    ALTER TABLE "FrameworkControls" ADD "ImplementationGuidanceAr" character varying(4000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "CollaborationComments" (
        "Id" uuid NOT NULL,
        "ItemId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "AuthorId" uuid NOT NULL,
        "Content" text NOT NULL,
        "MentionsJson" text,
        "AttachmentsJson" text,
        "ParentCommentId" uuid,
        "IsEdited" boolean NOT NULL,
        "EditedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_CollaborationComments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "CollaborationItems" (
        "Id" uuid NOT NULL,
        "WorkspaceId" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "ItemType" character varying(50) NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Content" text,
        "Status" character varying(50),
        "Priority" character varying(50),
        "AssignedTo" uuid,
        "DueDate" timestamp with time zone,
        "TagsJson" text,
        "AttachmentsJson" text,
        "CommentsCount" integer NOT NULL,
        "ReactionsCount" integer NOT NULL,
        "ParentItemId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_CollaborationItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "CollaborationWorkspaces" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "Type" character varying(50) NOT NULL,
        "FrameworkId" uuid,
        "ProjectId" uuid,
        "MembersJson" text,
        "SettingsJson" text,
        "IsActive" boolean NOT NULL,
        "ArchivedAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_CollaborationWorkspaces" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "EcosystemConnections" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "PartnerType" character varying(50),
        "PartnerId" uuid,
        "PartnerEmail" character varying(255),
        "PartnerName" character varying(255),
        "Purpose" character varying(500),
        "SharedDataTypesJson" text,
        "ExpiresAt" timestamp with time zone,
        "Status" character varying(50),
        "RequestedAt" timestamp with time zone,
        "ApprovedAt" timestamp with time zone,
        "RejectedAt" timestamp with time zone,
        "RejectionReason" character varying(500),
        "InteractionsCount" integer NOT NULL,
        "LastInteractionAt" timestamp with time zone,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EcosystemConnections" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "EcosystemPartners" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Sector" character varying(100),
        "Description" character varying(1000),
        "ServicesJson" text,
        "CertificationsJson" text,
        "ContactEmail" character varying(255),
        "Website" character varying(255),
        "LogoUrl" character varying(255),
        "Rating" double precision NOT NULL,
        "ReviewCount" integer NOT NULL,
        "ConnectionsCount" integer NOT NULL,
        "IsVerified" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "Country" character varying(50),
        "City" character varying(100),
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_EcosystemPartners" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "MarketingTrialLeads" (
        "Id" uuid NOT NULL,
        "Email" character varying(256) NOT NULL,
        "FullName" character varying(100) NOT NULL,
        "CompanyName" character varying(200) NOT NULL,
        "PhoneNumber" character varying(20),
        "CompanySize" character varying(20),
        "Industry" character varying(100),
        "TrialPlan" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "IpAddress" character varying(50),
        "UserAgent" character varying(500),
        "Locale" character varying(10) NOT NULL,
        "UtmSource" character varying(100),
        "UtmMedium" character varying(100),
        "UtmCampaign" character varying(100),
        "ReferrerUrl" character varying(500),
        "LandingPageUrl" character varying(500),
        "Notes" character varying(1000),
        "ContactedAt" timestamp with time zone,
        "ConvertedAt" timestamp with time zone,
        "TrialRequestId" uuid,
        "TenantId" uuid,
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_MarketingTrialLeads" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MarketingTrialLeads_TrialRequests_TrialRequestId" FOREIGN KEY ("TrialRequestId") REFERENCES "TrialRequests" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "TeamActivities" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TargetUserId" uuid,
        "ActivityType" character varying(100) NOT NULL,
        "Description" character varying(500),
        "MetadataJson" text,
        "Module" character varying(50),
        "Action" character varying(50),
        "ResourceId" uuid,
        "ResourceType" character varying(100),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TeamActivities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "TrialEmailLogs" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "EmailType" character varying(50) NOT NULL,
        "SentTo" character varying(255) NOT NULL,
        "SentAt" timestamp with time zone NOT NULL,
        "Status" character varying(50),
        "OpenedAt" timestamp with time zone,
        "ClickedAt" timestamp with time zone,
        "ErrorMessage" character varying(500),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TrialEmailLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE TABLE "TrialExtensions" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "DaysAdded" integer NOT NULL,
        "Reason" character varying(500),
        "PreviousEndDate" timestamp with time zone NOT NULL,
        "NewEndDate" timestamp with time zone NOT NULL,
        "ApprovedBy" character varying(100),
        "ApprovalMethod" character varying(100),
        "CreatedDate" timestamp with time zone NOT NULL,
        "ModifiedDate" timestamp with time zone,
        "CreatedBy" text,
        "ModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "BusinessCode" text,
        "RowVersion" bytea,
        "Owner" text,
        "DataClassification" text,
        "LabelsJson" text,
        CONSTRAINT "PK_TrialExtensions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    CREATE INDEX "IX_MarketingTrialLeads_TrialRequestId" ON "MarketingTrialLeads" ("TrialRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114065258_AddTeamEngagementFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260114065258_AddTeamEngagementFields', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114073941_AddArabicContentAndEvidencePackFields') THEN
    ALTER TABLE "EvidencePacks" ADD "DescriptionAr" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114073941_AddArabicContentAndEvidencePackFields') THEN
    ALTER TABLE "EvidencePacks" ADD "EvidenceItemsArJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260114073941_AddArabicContentAndEvidencePackFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260114073941_AddArabicContentAndEvidencePackFields', '8.0.8');
    END IF;
END $EF$;
COMMIT;

