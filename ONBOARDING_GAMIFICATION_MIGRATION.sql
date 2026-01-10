-- ============================================================================
-- Onboarding Gamification Schema Migration
-- Adds scoring, achievements, and linkages to assessments/workflows
-- ============================================================================

-- Create OnboardingStepScores table
CREATE TABLE [dbo].[OnboardingStepScores] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [OnboardingWizardId] UNIQUEIDENTIFIER NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [StepNumber] INT NOT NULL CHECK ([StepNumber] BETWEEN 1 AND 12),
    [StepLetter] NVARCHAR(1) NOT NULL,
    [StepName] NVARCHAR(100) NOT NULL,

    -- Gamification Scoring
    [TotalPointsAvailable] INT NOT NULL DEFAULT 100,
    [PointsEarned] INT NOT NULL DEFAULT 0,
    [SpeedBonus] INT NOT NULL DEFAULT 0,
    [ThoroughnessBonus] INT NOT NULL DEFAULT 0,
    [QualityBonus] INT NOT NULL DEFAULT 0,
    [TotalScore] INT NOT NULL DEFAULT 0,
    [StarRating] INT NOT NULL DEFAULT 0 CHECK ([StarRating] BETWEEN 0 AND 5),
    [AchievementLevel] NVARCHAR(20) NULL,

    -- Progress Tracking
    [TotalQuestions] INT NOT NULL DEFAULT 0,
    [RequiredQuestions] INT NOT NULL DEFAULT 0,
    [QuestionsAnswered] INT NOT NULL DEFAULT 0,
    [RequiredQuestionsAnswered] INT NOT NULL DEFAULT 0,
    [CompletionPercent] INT NOT NULL DEFAULT 0 CHECK ([CompletionPercent] BETWEEN 0 AND 100),
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'NotStarted',

    -- Time Tracking
    [EstimatedTimeMinutes] INT NOT NULL DEFAULT 5,
    [ActualTimeMinutes] INT NOT NULL DEFAULT 0,
    [StartedAt] DATETIME2 NULL,
    [CompletedAt] DATETIME2 NULL,

    -- Assessment Template Linkage
    [AssessmentTemplateId] UNIQUEIDENTIFIER NULL,
    [AssessmentTemplateName] NVARCHAR(200) NULL,
    [AssessmentInstanceId] UNIQUEIDENTIFIER NULL,
    [AssessmentStatus] NVARCHAR(20) NOT NULL DEFAULT 'NotCreated',

    -- GRC Requirements Linkage
    [GrcRequirementIdsJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    [GrcRequirementsCount] INT NOT NULL DEFAULT 0,
    [GrcRequirementsSatisfied] INT NOT NULL DEFAULT 0,
    [ComplianceFrameworksJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]',

    -- Workflow Linkage
    [WorkflowId] UNIQUEIDENTIFIER NULL,
    [WorkflowName] NVARCHAR(200) NULL,
    [WorkflowInstanceId] UNIQUEIDENTIFIER NULL,
    [WorkflowStatus] NVARCHAR(20) NOT NULL DEFAULT 'NotTriggered',
    [WorkflowTasksJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]',

    -- Validation & Quality
    [ValidationErrorsJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
    [ValidationAttempts] INT NOT NULL DEFAULT 0,
    [DataQualityScore] INT NOT NULL DEFAULT 0 CHECK ([DataQualityScore] BETWEEN 0 AND 100),
    [CompletenessScore] INT NOT NULL DEFAULT 0 CHECK ([CompletenessScore] BETWEEN 0 AND 100),

    -- Audit Fields
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,

    -- Foreign Keys
    CONSTRAINT [FK_OnboardingStepScores_OnboardingWizards]
        FOREIGN KEY ([OnboardingWizardId])
        REFERENCES [dbo].[OnboardingWizards]([Id])
        ON DELETE CASCADE,

    CONSTRAINT [FK_OnboardingStepScores_Tenants]
        FOREIGN KEY ([TenantId])
        REFERENCES [dbo].[Tenants]([Id])
);

-- Create Indexes
CREATE INDEX [IX_OnboardingStepScores_OnboardingWizardId]
    ON [dbo].[OnboardingStepScores] ([OnboardingWizardId]);

CREATE INDEX [IX_OnboardingStepScores_TenantId]
    ON [dbo].[OnboardingStepScores] ([TenantId]);

CREATE INDEX [IX_OnboardingStepScores_StepNumber]
    ON [dbo].[OnboardingStepScores] ([StepNumber]);

CREATE INDEX [IX_OnboardingStepScores_Status]
    ON [dbo].[OnboardingStepScores] ([Status]);

CREATE INDEX [IX_OnboardingStepScores_AssessmentTemplateId]
    ON [dbo].[OnboardingStepScores] ([AssessmentTemplateId])
    WHERE [AssessmentTemplateId] IS NOT NULL;

CREATE INDEX [IX_OnboardingStepScores_WorkflowId]
    ON [dbo].[OnboardingStepScores] ([WorkflowId])
    WHERE [WorkflowId] IS NOT NULL;

-- ============================================================================
-- Update OnboardingWizards table to add gamification summary fields
-- ============================================================================

ALTER TABLE [dbo].[OnboardingWizards]
ADD [TotalPointsEarned] INT NOT NULL DEFAULT 0,
    [TotalPointsAvailable] INT NOT NULL DEFAULT 1300, -- Sum of all step points
    [OverallScore] INT NOT NULL DEFAULT 0,
    [AchievementLevel] NVARCHAR(20) NULL,
    [CompletedStepsJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]', -- JSON array of completed step letters
    [LeaderboardRank] INT NULL,
    [TimeToComplete] INT NULL; -- Total minutes

-- ============================================================================
-- Seed initial step scores for existing onboarding wizards
-- ============================================================================

INSERT INTO [dbo].[OnboardingStepScores] (
    [OnboardingWizardId],
    [TenantId],
    [StepNumber],
    [StepLetter],
    [StepName],
    [TotalPointsAvailable],
    [EstimatedTimeMinutes],
    [TotalQuestions],
    [RequiredQuestions]
)
SELECT
    ow.[Id] AS [OnboardingWizardId],
    ow.[TenantId],
    steps.[StepNumber],
    steps.[StepLetter],
    steps.[StepName],
    steps.[BasePoints],
    steps.[EstimatedMinutes],
    steps.[TotalQuestions],
    steps.[RequiredQuestions]
FROM [dbo].[OnboardingWizards] ow
CROSS JOIN (
    VALUES
        (1, 'A', 'Organization Identity & Tenancy', 150, 5, 13, 4),
        (2, 'B', 'Assurance Objective', 100, 4, 5, 1),
        (3, 'C', 'Regulatory & Framework Applicability', 120, 6, 7, 0),
        (4, 'D', 'Scope Definition', 110, 7, 9, 0),
        (5, 'E', 'Data & Risk Profile', 130, 6, 6, 1),
        (6, 'F', 'Technology Landscape', 80, 5, 13, 0),
        (7, 'G', 'Control Ownership Model', 100, 8, 7, 1),
        (8, 'H', 'Teams Roles & Access', 120, 5, 10, 1),
        (9, 'I', 'Workflow & Cadence', 90, 6, 10, 0),
        (10, 'J', 'Evidence Standards', 85, 5, 7, 0),
        (11, 'K', 'Baseline & Overlays Selection', 100, 7, 3, 0),
        (12, 'L', 'Go-Live & Success Metrics', 115, 4, 6, 0)
) AS steps([StepNumber], [StepLetter], [StepName], [BasePoints], [EstimatedMinutes], [TotalQuestions], [RequiredQuestions])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[OnboardingStepScores] oss
    WHERE oss.[OnboardingWizardId] = ow.[Id]
      AND oss.[StepNumber] = steps.[StepNumber]
);

-- ============================================================================
-- Create view for leaderboard
-- ============================================================================

CREATE OR ALTER VIEW [dbo].[vw_OnboardingLeaderboard] AS
SELECT
    ow.[TenantId],
    t.[OrganizationName],
    ow.[CompletedByUserId],
    ow.[TotalPointsEarned],
    ow.[TotalPointsAvailable],
    CAST(ow.[TotalPointsEarned] AS FLOAT) / NULLIF(ow.[TotalPointsAvailable], 0) * 100 AS [ScorePercent],
    ow.[ProgressPercent],
    ow.[AchievementLevel],
    ow.[TimeToComplete],
    ow.[CompletedAt],
    COUNT(oss.[Id]) AS [TotalSteps],
    SUM(CASE WHEN oss.[Status] = 'Completed' THEN 1 ELSE 0 END) AS [CompletedSteps],
    AVG(oss.[StarRating]) AS [AverageStarRating],
    SUM(oss.[SpeedBonus]) AS [TotalSpeedBonus],
    SUM(oss.[ThoroughnessBonus]) AS [TotalThoroughnessBonus],
    SUM(oss.[QualityBonus]) AS [TotalQualityBonus],
    ROW_NUMBER() OVER (ORDER BY ow.[TotalPointsEarned] DESC, ow.[CompletedAt] ASC) AS [Rank]
FROM [dbo].[OnboardingWizards] ow
INNER JOIN [dbo].[Tenants] t ON ow.[TenantId] = t.[Id]
LEFT JOIN [dbo].[OnboardingStepScores] oss ON ow.[Id] = oss.[OnboardingWizardId]
WHERE ow.[WizardStatus] IN ('Completed', 'InProgress')
  AND ow.[IsDeleted] = 0
GROUP BY
    ow.[TenantId],
    t.[OrganizationName],
    ow.[CompletedByUserId],
    ow.[TotalPointsEarned],
    ow.[TotalPointsAvailable],
    ow.[ProgressPercent],
    ow.[AchievementLevel],
    ow.[TimeToComplete],
    ow.[CompletedAt];

GO

-- ============================================================================
-- Create stored procedure to calculate step score
-- ============================================================================

CREATE OR ALTER PROCEDURE [dbo].[sp_CalculateStepScore]
    @StepScoreId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PointsEarned INT;
    DECLARE @SpeedBonus INT = 0;
    DECLARE @ThoroughnessBonus INT = 0;
    DECLARE @QualityBonus INT = 0;
    DECLARE @TotalScore INT;
    DECLARE @StarRating INT;
    DECLARE @AchievementLevel NVARCHAR(20);

    -- Get current step data
    DECLARE @CompletionPercent INT;
    DECLARE @DataQualityScore INT;
    DECLARE @EstimatedTime INT;
    DECLARE @ActualTime INT;
    DECLARE @ValidationAttempts INT;
    DECLARE @TotalPointsAvailable INT;
    DECLARE @RequiredQuestions INT;
    DECLARE @TotalQuestions INT;
    DECLARE @QuestionsAnswered INT;

    SELECT
        @CompletionPercent = [CompletionPercent],
        @DataQualityScore = [DataQualityScore],
        @EstimatedTime = [EstimatedTimeMinutes],
        @ActualTime = [ActualTimeMinutes],
        @ValidationAttempts = [ValidationAttempts],
        @TotalPointsAvailable = [TotalPointsAvailable],
        @RequiredQuestions = [RequiredQuestions],
        @TotalQuestions = [TotalQuestions],
        @QuestionsAnswered = [QuestionsAnswered]
    FROM [dbo].[OnboardingStepScores]
    WHERE [Id] = @StepScoreId;

    -- Calculate base points (based on completion %)
    SET @PointsEarned = (@TotalPointsAvailable * @CompletionPercent) / 100;

    -- Speed Bonus: if completed under estimated time
    IF @ActualTime > 0 AND @ActualTime < @EstimatedTime
        SET @SpeedBonus = (@EstimatedTime - @ActualTime) * 10; -- 10 points per minute saved

    -- Thoroughness Bonus: if all questions answered (including optional)
    IF @QuestionsAnswered >= @TotalQuestions
        SET @ThoroughnessBonus = 25;

    -- Quality Bonus: if validated on first try
    IF @ValidationAttempts = 1
        SET @QualityBonus = 30;

    -- Perfect Score Bonus
    IF @CompletionPercent = 100 AND @DataQualityScore >= 95
        SET @QualityBonus = @QualityBonus + 50;

    -- Calculate total score
    SET @TotalScore = @PointsEarned + @SpeedBonus + @ThoroughnessBonus + @QualityBonus;

    -- Calculate star rating (1-5)
    DECLARE @AverageQuality INT = (@CompletionPercent + @DataQualityScore) / 2;
    SET @StarRating = CASE
        WHEN @AverageQuality >= 95 THEN 5
        WHEN @AverageQuality >= 80 THEN 4
        WHEN @AverageQuality >= 65 THEN 3
        WHEN @AverageQuality >= 50 THEN 2
        WHEN @AverageQuality >= 30 THEN 1
        ELSE 0
    END;

    -- Calculate achievement level
    DECLARE @ScorePercent FLOAT = CAST(@TotalScore AS FLOAT) / @TotalPointsAvailable * 100;
    SET @AchievementLevel = CASE
        WHEN @ScorePercent >= 95 THEN 'Diamond'
        WHEN @ScorePercent >= 85 THEN 'Platinum'
        WHEN @ScorePercent >= 75 THEN 'Gold'
        WHEN @ScorePercent >= 60 THEN 'Silver'
        WHEN @ScorePercent >= 40 THEN 'Bronze'
        ELSE NULL
    END;

    -- Update the step score
    UPDATE [dbo].[OnboardingStepScores]
    SET [PointsEarned] = @PointsEarned,
        [SpeedBonus] = @SpeedBonus,
        [ThoroughnessBonus] = @ThoroughnessBonus,
        [QualityBonus] = @QualityBonus,
        [TotalScore] = @TotalScore,
        [StarRating] = @StarRating,
        [AchievementLevel] = @AchievementLevel,
        [ModifiedDate] = GETUTCDATE()
    WHERE [Id] = @StepScoreId;

    -- Update wizard totals
    UPDATE [dbo].[OnboardingWizards]
    SET [TotalPointsEarned] = (
            SELECT SUM([TotalScore])
            FROM [dbo].[OnboardingStepScores]
            WHERE [OnboardingWizardId] = (SELECT [OnboardingWizardId] FROM [dbo].[OnboardingStepScores] WHERE [Id] = @StepScoreId)
        ),
        [OverallScore] = (
            SELECT AVG([StarRating])
            FROM [dbo].[OnboardingStepScores]
            WHERE [OnboardingWizardId] = (SELECT [OnboardingWizardId] FROM [dbo].[OnboardingStepScores] WHERE [Id] = @StepScoreId)
        )
    WHERE [Id] = (SELECT [OnboardingWizardId] FROM [dbo].[OnboardingStepScores] WHERE [Id] = @StepScoreId);

    RETURN @TotalScore;
END;
GO

PRINT 'Onboarding Gamification Migration Completed Successfully!';
