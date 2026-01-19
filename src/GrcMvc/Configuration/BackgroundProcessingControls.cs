namespace GrcMvc.Configuration;

/// <summary>
/// GRC Background Processing Control Register
/// Defines BP controls for background job governance.
/// Reference: BP-01, BP-02
/// Note: ABP Background Workers are disabled due to OpenIddict bug - Hangfire is used as compensating control.
/// </summary>
public static class BackgroundProcessingControls
{
    /// <summary>
    /// BP-01: Background Job Governance (Hangfire)
    /// Ensures all compliance-critical jobs are implemented in Hangfire with proper monitoring.
    /// </summary>
    public static class BP01_JobGovernance
    {
        public const string ControlId = "BP-01";
        public const string Name = "Background Job Governance (Hangfire)";
        public const string Objective = "Ensure all compliance-critical jobs are implemented in Hangfire with proper monitoring";
        public const string Owner = "Platform Engineering";
        public const string Frequency = "Continuous monitoring; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Compliance deadlines could be missed if background jobs fail silently";

        // Control statement
        public const string Control = "All scheduled/async tasks required for compliance (trial expiry, access reviews, notifications) are implemented as Hangfire jobs with retry logic and failure alerting.";

        // Test procedure
        public const string TestProcedure = "Review Hangfire dashboard; verify critical jobs are scheduled and have succeeded in last 7 days. Check failure alerts are configured.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Hangfire job list export",
            "Job success/failure metrics",
            "Alert configuration for job failures"
        };

        // Systems
        public static readonly string[] Systems = { "Hangfire", "PostgreSQL (Hangfire storage)", "Application Insights" };

        // Event types for this control
        public static class Events
        {
            public const string JobScheduled = "BP01_JOB_SCHEDULED";
            public const string JobStarted = "BP01_JOB_STARTED";
            public const string JobCompleted = "BP01_JOB_COMPLETED";
            public const string JobFailed = "BP01_JOB_FAILED";
            public const string JobRetried = "BP01_JOB_RETRIED";
            public const string JobCancelled = "BP01_JOB_CANCELLED";
            public const string CriticalJobMissed = "BP01_CRITICAL_JOB_MISSED";
        }

        // Critical compliance jobs
        public static readonly string[] CriticalJobs =
        {
            "TrialExpiryChecker",
            "AccessReviewReminder",
            "AuditLogRetention",
            "InactivitySuspension",
            "CertificationExpiryNotifier",
            "ComplianceDeadlineReminder",
            "EvidenceExpiryChecker"
        };

        // Configuration
        public const int MaxRetryAttempts = 3;
        public const int RetryDelayMinutes = 5;
        public const int CriticalJobAlertDelayMinutes = 15;
    }

    /// <summary>
    /// BP-02: ABP Worker Disablement Documentation
    /// Ensures the ABP background worker disablement is documented with rationale and exit plan.
    /// </summary>
    public static class BP02_WorkerDisablement
    {
        public const string ControlId = "BP-02";
        public const string Name = "ABP Worker Disablement Documentation";
        public const string Objective = "Ensure ABP background worker disablement is documented with rationale and exit plan";
        public const string Owner = "Platform Engineering + Security";
        public const string Frequency = "Reviewed quarterly; exit plan updated";

        // Risk addressed
        public const string RiskAddressed = "Technical debt and deviation from standard framework could create maintenance risk";

        // Control statement
        public const string Control = "A known-issue record documents why ABP BackgroundWorkers are disabled (OpenIddict initialization bug), the compensating control (Hangfire), and an exit plan for re-enablement.";

        // Test procedure
        public const string TestProcedure = "Verify known-issue record exists in issue tracker; verify it includes root cause, compensating control, and exit criteria.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Known-issue ticket/document",
            "Compensating control documentation",
            "Exit plan with criteria for re-enablement"
        };

        // Systems
        public static readonly string[] Systems = { "Azure DevOps", "Confluence/Wiki", "GitHub Issues" };

        // Event types for this control
        public static class Events
        {
            public const string DisablementDocumented = "BP02_DISABLEMENT_DOCUMENTED";
            public const string ExitPlanUpdated = "BP02_EXIT_PLAN_UPDATED";
            public const string CompensatingControlVerified = "BP02_COMPENSATING_CONTROL_VERIFIED";
            public const string ReenablementAttempted = "BP02_REENABLEMENT_ATTEMPTED";
            public const string ReenablementSuccessful = "BP02_REENABLEMENT_SUCCESSFUL";
            public const string ReenablementFailed = "BP02_REENABLEMENT_FAILED";
        }

        // Known issue details
        public static class KnownIssue
        {
            public const string IssueId = "PLATFORM-001";
            public const string Title = "ABP BackgroundWorkers Disabled Due to OpenIddict Initialization Bug";
            public const string RootCause = "OpenIddict initialization fails when ABP BackgroundWorkers are enabled due to timing/dependency issue during startup";
            public const string Impact = "Cannot use ABP's built-in background worker infrastructure";
            public const string CompensatingControl = "Hangfire is used for all background job processing instead of ABP workers";
            public const string ExitCriteria = "ABP/OpenIddict bug is fixed in upstream, or custom initialization order resolves the conflict";
            public static readonly DateTime DocumentedDate = new DateTime(2025, 6, 1);
            public static readonly DateTime LastReviewDate = new DateTime(2026, 1, 1);
            public static readonly DateTime NextReviewDate = new DateTime(2026, 4, 1);
        }

        // Disabled ABP modules for reference
        public static readonly string[] DisabledModules =
        {
            "Volo.Abp.BackgroundWorkers",
            "Volo.Abp.BackgroundJobs",
            "Volo.Abp.BackgroundJobs.Hangfire"
        };
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        BP01_JobGovernance.ControlId,
        BP02_WorkerDisablement.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "BP-01" => (BP01_JobGovernance.Name, BP01_JobGovernance.Objective, BP01_JobGovernance.Owner),
        "BP-02" => (BP02_WorkerDisablement.Name, BP02_WorkerDisablement.Objective, BP02_WorkerDisablement.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };
}

/// <summary>
/// Hangfire job registry for BP-01 compliance
/// </summary>
public static class HangfireJobRegistry
{
    /// <summary>
    /// Critical compliance jobs that must run on schedule
    /// </summary>
    public static class CriticalJobs
    {
        public const string TrialExpiryChecker = "TrialExpiryChecker";
        public const string AccessReviewReminder = "AccessReviewReminder";
        public const string AccessReviewOverdueChecker = "AccessReviewOverdueChecker";
        public const string InactivitySuspensionChecker = "InactivitySuspensionChecker";
        public const string AuditLogRetention = "AuditLogRetention";
        public const string CertificationExpiryNotifier = "CertificationExpiryNotifier";
        public const string ComplianceDeadlineReminder = "ComplianceDeadlineReminder";
        public const string EvidenceExpiryChecker = "EvidenceExpiryChecker";
        public const string PasswordExpiryNotifier = "PasswordExpiryNotifier";
    }

    /// <summary>
    /// Standard operational jobs
    /// </summary>
    public static class OperationalJobs
    {
        public const string EmailQueueProcessor = "EmailQueueProcessor";
        public const string NotificationProcessor = "NotificationProcessor";
        public const string ReportGenerator = "ReportGenerator";
        public const string DataSyncJob = "DataSyncJob";
        public const string CacheWarmer = "CacheWarmer";
        public const string MetricsAggregator = "MetricsAggregator";
    }

    /// <summary>
    /// Job schedule definitions
    /// </summary>
    public static class Schedules
    {
        // Critical jobs run frequently
        public const string TrialExpiryChecker = "0 */2 * * *";      // Every 2 hours
        public const string AccessReviewReminder = "0 9 * * *";      // Daily at 9 AM
        public const string InactivityChecker = "0 3 * * *";         // Daily at 3 AM
        public const string AuditRetention = "0 2 * * 0";            // Weekly Sunday 2 AM

        // Standard jobs
        public const string EmailProcessor = "*/5 * * * *";          // Every 5 minutes
        public const string NotificationProcessor = "*/1 * * * *";   // Every minute
        public const string MetricsAggregator = "0 * * * *";         // Every hour
    }
}
