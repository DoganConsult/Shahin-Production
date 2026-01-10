-- ============================================================================
-- Onboarding Professional Achievement System - PostgreSQL Migration
-- Purpose: Add professional scoring, assessment linkage, and GRC integration
-- Priority: HIGH - Critical for onboarding UX improvement
-- ============================================================================

BEGIN;

-- ══════════════════════════════════════════════════════════════
-- CREATE OnboardingStepScores TABLE
-- ══════════════════════════════════════════════════════════════

CREATE TABLE "OnboardingStepScores" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "OnboardingWizardId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "StepNumber" integer NOT NULL CHECK ("StepNumber" >= 1 AND "StepNumber" <= 12),
    "StepLetter" character varying(1) NOT NULL,
    "StepName" character varying(100) NOT NULL,

    -- Professional Scoring
    "TotalPointsAvailable" integer NOT NULL DEFAULT 100,
    "PointsEarned" integer NOT NULL DEFAULT 0,
    "SpeedBonus" integer NOT NULL DEFAULT 0,
    "ThoroughnessBonus" integer NOT NULL DEFAULT 0,
    "QualityBonus" integer NOT NULL DEFAULT 0,
    "TotalScore" integer NOT NULL DEFAULT 0,
    "StarRating" integer NOT NULL DEFAULT 0 CHECK ("StarRating" >= 0 AND "StarRating" <= 5),
    "AchievementLevel" character varying(20),

    -- Progress Tracking
    "TotalQuestions" integer NOT NULL DEFAULT 0,
    "RequiredQuestions" integer NOT NULL DEFAULT 0,
    "QuestionsAnswered" integer NOT NULL DEFAULT 0,
    "RequiredQuestionsAnswered" integer NOT NULL DEFAULT 0,
    "CompletionPercent" integer NOT NULL DEFAULT 0 CHECK ("CompletionPercent" >= 0 AND "CompletionPercent" <= 100),
    "Status" character varying(20) NOT NULL DEFAULT 'NotStarted',

    -- Time Tracking
    "EstimatedTimeMinutes" integer NOT NULL DEFAULT 5,
    "ActualTimeMinutes" integer NOT NULL DEFAULT 0,
    "StartedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone,

    -- Assessment Template Linkage
    "AssessmentTemplateId" uuid,
    "AssessmentTemplateName" character varying(200),
    "AssessmentInstanceId" uuid,
    "AssessmentStatus" character varying(20) NOT NULL DEFAULT 'NotCreated',

    -- GRC Requirements Linkage
    "GrcRequirementIdsJson" text NOT NULL DEFAULT '[]',
    "GrcRequirementsCount" integer NOT NULL DEFAULT 0,
    "GrcRequirementsSatisfied" integer NOT NULL DEFAULT 0,
    "ComplianceFrameworksJson" text NOT NULL DEFAULT '[]',

    -- Workflow Linkage
    "WorkflowId" uuid,
    "WorkflowName" character varying(200),
    "WorkflowInstanceId" uuid,
    "WorkflowStatus" character varying(20) NOT NULL DEFAULT 'NotTriggered',
    "WorkflowTasksJson" text NOT NULL DEFAULT '[]',

    -- Validation & Quality
    "ValidationErrorsJson" text NOT NULL DEFAULT '{}',
    "ValidationAttempts" integer NOT NULL DEFAULT 0,
    "DataQualityScore" integer NOT NULL DEFAULT 0 CHECK ("DataQualityScore" >= 0 AND "DataQualityScore" <= 100),
    "CompletenessScore" integer NOT NULL DEFAULT 0 CHECK ("CompletenessScore" >= 0 AND "CompletenessScore" <= 100),

    -- Audit Fields (BaseEntity pattern)
    "CreatedDate" timestamp with time zone NOT NULL DEFAULT NOW(),
    "CreatedBy" character varying(100),
    "ModifiedDate" timestamp with time zone,
    "ModifiedBy" character varying(100),
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "DeletedAt" timestamp with time zone,

    CONSTRAINT "FK_OnboardingStepScores_OnboardingWizards_OnboardingWizardId"
        FOREIGN KEY ("OnboardingWizardId")
        REFERENCES "OnboardingWizards"("Id")
        ON DELETE CASCADE,

    CONSTRAINT "FK_OnboardingStepScores_Tenants_TenantId"
        FOREIGN KEY ("TenantId")
        REFERENCES "Tenants"("Id")
        ON DELETE RESTRICT
);

-- Create indexes for performance
CREATE INDEX "IX_OnboardingStepScores_OnboardingWizardId"
    ON "OnboardingStepScores" ("OnboardingWizardId");

CREATE INDEX "IX_OnboardingStepScores_TenantId"
    ON "OnboardingStepScores" ("TenantId");

CREATE INDEX "IX_OnboardingStepScores_StepNumber"
    ON "OnboardingStepScores" ("StepNumber");

CREATE INDEX "IX_OnboardingStepScores_Status"
    ON "OnboardingStepScores" ("Status");

CREATE INDEX "IX_OnboardingStepScores_AssessmentTemplateId"
    ON "OnboardingStepScores" ("AssessmentTemplateId")
    WHERE "AssessmentTemplateId" IS NOT NULL;

CREATE INDEX "IX_OnboardingStepScores_WorkflowId"
    ON "OnboardingStepScores" ("WorkflowId")
    WHERE "WorkflowId" IS NOT NULL;

-- ══════════════════════════════════════════════════════════════
-- UPDATE OnboardingWizards TABLE - Add Professional Achievement Fields
-- ══════════════════════════════════════════════════════════════

ALTER TABLE "OnboardingWizards"
ADD COLUMN "TotalPointsEarned" integer NOT NULL DEFAULT 0,
ADD COLUMN "TotalPointsAvailable" integer NOT NULL DEFAULT 1300,
ADD COLUMN "OverallScore" integer NOT NULL DEFAULT 0,
ADD COLUMN "AchievementLevel" character varying(20),
ADD COLUMN "CompletedStepsJson" text NOT NULL DEFAULT '[]',
ADD COLUMN "LeaderboardRank" integer,
ADD COLUMN "TimeToComplete" integer;

-- ══════════════════════════════════════════════════════════════
-- INSERT __EFMigrationsHistory RECORD
-- ══════════════════════════════════════════════════════════════

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260110000003_OnboardingGamificationSystem', '8.0.8');

COMMIT;

-- ============================================================================
-- MIGRATION COMPLETED SUCCESSFULLY
-- ============================================================================

COMMENT ON TABLE "OnboardingStepScores" IS 'Professional achievement tracking for onboarding sections - ENTERPRISE-LEVEL motivation (not childish gaming)';
COMMENT ON COLUMN "OnboardingStepScores"."AchievementLevel" IS 'Professional levels: Executive, Expert, Advanced, Proficient, Developing (NOT gaming levels!)';
