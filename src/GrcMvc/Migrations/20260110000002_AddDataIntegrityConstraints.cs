using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration: Add Data Integrity Check Constraints
    /// Purpose: Enforce business rules at database level to prevent invalid data
    /// Impact: Ensures data quality and prevents common data integrity issues
    /// Priority: MEDIUM - Data quality and integrity
    /// </summary>
    public partial class AddDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ══════════════════════════════════════════════════════════════
            // RISKS TABLE - Likelihood and Impact must be 1-5
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Risks""
                ADD CONSTRAINT ""CK_Risks_Likelihood""
                CHECK (""Likelihood"" >= 1 AND ""Likelihood"" <= 5);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Risks""
                ADD CONSTRAINT ""CK_Risks_Impact""
                CHECK (""Impact"" >= 1 AND ""Impact"" <= 5);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Risks""
                ADD CONSTRAINT ""CK_Risks_RiskScore""
                CHECK (""RiskScore"" >= 0 AND ""RiskScore"" <= 25);
            ");

            // ══════════════════════════════════════════════════════════════
            // CONTROL TESTS TABLE - Score must be 0-100
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""ControlTests""
                ADD CONSTRAINT ""CK_ControlTests_Score""
                CHECK (""Score"" IS NULL OR (""Score"" >= 0 AND ""Score"" <= 100));
            ");

            // ══════════════════════════════════════════════════════════════
            // CERTIFICATIONS TABLE - Expiry date must be after issue date
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Certifications""
                ADD CONSTRAINT ""CK_Certifications_ExpiryAfterIssue""
                CHECK (""ExpiryDate"" IS NULL OR ""IssuedDate"" IS NULL OR ""ExpiryDate"" > ""IssuedDate"");
            ");

            // ══════════════════════════════════════════════════════════════
            // ASSESSMENTS TABLE - Completion percentage 0-100
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Assessments""
                ADD CONSTRAINT ""CK_Assessments_CompletionPercentage""
                CHECK (""CompletionPercentage"" IS NULL OR (""CompletionPercentage"" >= 0 AND ""CompletionPercentage"" <= 100));
            ");

            // ══════════════════════════════════════════════════════════════
            // EVIDENCES TABLE - File size must be positive
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences""
                ADD CONSTRAINT ""CK_Evidences_FileSize""
                CHECK (""FileSizeBytes"" IS NULL OR ""FileSizeBytes"" >= 0);
            ");

            // ══════════════════════════════════════════════════════════════
            // SUBSCRIPTION PLANS TABLE - Price and limits must be non-negative
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""SubscriptionPlans""
                ADD CONSTRAINT ""CK_SubscriptionPlans_MonthlyPrice""
                CHECK (""MonthlyPriceUSD"" IS NULL OR ""MonthlyPriceUSD"" >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""SubscriptionPlans""
                ADD CONSTRAINT ""CK_SubscriptionPlans_MaxUsers""
                CHECK (""MaxUsers"" IS NULL OR ""MaxUsers"" > 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""SubscriptionPlans""
                ADD CONSTRAINT ""CK_SubscriptionPlans_MaxAssessments""
                CHECK (""MaxAssessments"" IS NULL OR ""MaxAssessments"" > 0);
            ");

            // ══════════════════════════════════════════════════════════════
            // TENANTS TABLE - Admin email must be valid format
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tenants""
                ADD CONSTRAINT ""CK_Tenants_AdminEmail""
                CHECK (""AdminEmail"" ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');
            ");

            // ══════════════════════════════════════════════════════════════
            // WORKFLOW TASKS TABLE - Due date must be after created date
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""WorkflowTasks""
                ADD CONSTRAINT ""CK_WorkflowTasks_DueDateAfterCreated""
                CHECK (""DueDate"" IS NULL OR ""DueDate"" >= ""CreatedDate"");
            ");

            // ══════════════════════════════════════════════════════════════
            // INCIDENTS TABLE - Severity must be 1-5 (Critical to Low)
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Incidents""
                ADD CONSTRAINT ""CK_Incidents_Severity""
                CHECK (""Severity"" >= 1 AND ""Severity"" <= 5);
            ");

            // ══════════════════════════════════════════════════════════════
            // AGENT CONFIDENCE SCORES TABLE - Confidence must be 0-100
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""AgentConfidenceScores""
                ADD CONSTRAINT ""CK_AgentConfidenceScores_ConfidenceLevel""
                CHECK (""ConfidenceLevel"" >= 0 AND ""ConfidenceLevel"" <= 100);
            ");

            // ══════════════════════════════════════════════════════════════
            // CONTROL EFFECTIVENESS TABLE - Effectiveness rating 0-100
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Controls""
                ADD CONSTRAINT ""CK_Controls_Effectiveness""
                CHECK (""Effectiveness"" IS NULL OR (""Effectiveness"" >= 0 AND ""Effectiveness"" <= 100));
            ");

            // ══════════════════════════════════════════════════════════════
            // FRAMEWORK CONTROLS TABLE - Ensure FrameworkVersion is not empty
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""FrameworkControls""
                ADD CONSTRAINT ""CK_FrameworkControls_Version""
                CHECK (""FrameworkVersion"" IS NOT NULL AND LENGTH(TRIM(""FrameworkVersion"")) > 0);
            ");

            // ══════════════════════════════════════════════════════════════
            // PAYMENTS TABLE - Amount must be positive
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""Payments""
                ADD CONSTRAINT ""CK_Payments_Amount""
                CHECK (""Amount"" > 0);
            ");

            // ══════════════════════════════════════════════════════════════
            // SLA RULES TABLE - Warning threshold must be less than breach threshold
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                ALTER TABLE ""SlaRules""
                ADD CONSTRAINT ""CK_SlaRules_WarningBeforeBreach""
                CHECK (""WarningThresholdMinutes"" IS NULL OR ""BreachThresholdMinutes"" IS NULL
                    OR ""WarningThresholdMinutes"" < ""BreachThresholdMinutes"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all check constraints created in Up()
            migrationBuilder.Sql(@"ALTER TABLE ""Risks"" DROP CONSTRAINT IF EXISTS ""CK_Risks_Likelihood"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Risks"" DROP CONSTRAINT IF EXISTS ""CK_Risks_Impact"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Risks"" DROP CONSTRAINT IF EXISTS ""CK_Risks_RiskScore"";");

            migrationBuilder.Sql(@"ALTER TABLE ""ControlTests"" DROP CONSTRAINT IF EXISTS ""CK_ControlTests_Score"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Certifications"" DROP CONSTRAINT IF EXISTS ""CK_Certifications_ExpiryAfterIssue"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Assessments"" DROP CONSTRAINT IF EXISTS ""CK_Assessments_CompletionPercentage"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Evidences"" DROP CONSTRAINT IF EXISTS ""CK_Evidences_FileSize"";");

            migrationBuilder.Sql(@"ALTER TABLE ""SubscriptionPlans"" DROP CONSTRAINT IF EXISTS ""CK_SubscriptionPlans_MonthlyPrice"";");
            migrationBuilder.Sql(@"ALTER TABLE ""SubscriptionPlans"" DROP CONSTRAINT IF EXISTS ""CK_SubscriptionPlans_MaxUsers"";");
            migrationBuilder.Sql(@"ALTER TABLE ""SubscriptionPlans"" DROP CONSTRAINT IF EXISTS ""CK_SubscriptionPlans_MaxAssessments"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Tenants"" DROP CONSTRAINT IF EXISTS ""CK_Tenants_AdminEmail"";");

            migrationBuilder.Sql(@"ALTER TABLE ""WorkflowTasks"" DROP CONSTRAINT IF EXISTS ""CK_WorkflowTasks_DueDateAfterCreated"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Incidents"" DROP CONSTRAINT IF EXISTS ""CK_Incidents_Severity"";");

            migrationBuilder.Sql(@"ALTER TABLE ""AgentConfidenceScores"" DROP CONSTRAINT IF EXISTS ""CK_AgentConfidenceScores_ConfidenceLevel"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Controls"" DROP CONSTRAINT IF EXISTS ""CK_Controls_Effectiveness"";");

            migrationBuilder.Sql(@"ALTER TABLE ""FrameworkControls"" DROP CONSTRAINT IF EXISTS ""CK_FrameworkControls_Version"";");

            migrationBuilder.Sql(@"ALTER TABLE ""Payments"" DROP CONSTRAINT IF EXISTS ""CK_Payments_Amount"";");

            migrationBuilder.Sql(@"ALTER TABLE ""SlaRules"" DROP CONSTRAINT IF EXISTS ""CK_SlaRules_WarningBeforeBreach"";");
        }
    }
}
