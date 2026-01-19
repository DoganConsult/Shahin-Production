namespace GrcMvc.Configuration;

/// <summary>
/// GRC Feature Management Control Register
/// Defines FM controls for feature flag governance.
/// Reference: FM-01
/// </summary>
public static class FeatureManagementControls
{
    /// <summary>
    /// FM-01: Feature Flag Governance
    /// Ensures feature flags are managed with proper approval and audit trails.
    /// </summary>
    public static class FM01_FeatureFlagGovernance
    {
        public const string ControlId = "FM-01";
        public const string Name = "Feature Flag Governance";
        public const string Objective = "Ensure feature flags are managed with proper approval and audit trails";
        public const string Owner = "Product Engineering + Platform Engineering";
        public const string Frequency = "Per change; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Uncontrolled feature enablement could expose incomplete features or create security risks";

        // Control statement
        public const string Control = "All feature flag changes (tenant or global) must be logged with actor, timestamp, old/new value. Production flag changes require approval.";

        // Test procedure
        public const string TestProcedure = "Review feature flag change log for last quarter; verify each production change has approval and audit trail.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Feature flag change audit log",
            "Approval tickets for production flag changes",
            "ABP FeatureManagement configuration export"
        };

        // Systems
        public static readonly string[] Systems = { "ABP FeatureManagement", "Azure App Configuration", "GrcMvc AuditEvent" };

        // Event types for this control
        public static class Events
        {
            public const string FeatureEnabled = "FM01_FEATURE_ENABLED";
            public const string FeatureDisabled = "FM01_FEATURE_DISABLED";
            public const string FeatureValueChanged = "FM01_FEATURE_VALUE_CHANGED";
            public const string TenantFeatureOverride = "FM01_TENANT_FEATURE_OVERRIDE";
            public const string FeatureApprovalRequested = "FM01_FEATURE_APPROVAL_REQUESTED";
            public const string FeatureApproved = "FM01_FEATURE_APPROVED";
            public const string FeatureDenied = "FM01_FEATURE_DENIED";
            public const string FeatureRolledBack = "FM01_FEATURE_ROLLED_BACK";
        }

        // Feature categories requiring approval
        public static readonly string[] ApprovalRequiredCategories =
        {
            "Security",
            "Integration",
            "AI",
            "Billing",
            "DataProcessing"
        };
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        FM01_FeatureFlagGovernance.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "FM-01" => (FM01_FeatureFlagGovernance.Name, FM01_FeatureFlagGovernance.Objective, FM01_FeatureFlagGovernance.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };
}

/// <summary>
/// GRC Feature definitions for ABP Feature Management
/// </summary>
public static class GrcFeatures
{
    /// <summary>
    /// Core GRC Features
    /// </summary>
    public static class Core
    {
        public const string RiskManagement = "GRC.Core.RiskManagement";
        public const string ComplianceManagement = "GRC.Core.ComplianceManagement";
        public const string PolicyManagement = "GRC.Core.PolicyManagement";
        public const string AuditManagement = "GRC.Core.AuditManagement";
        public const string IncidentManagement = "GRC.Core.IncidentManagement";
        public const string VendorManagement = "GRC.Core.VendorManagement";
    }

    /// <summary>
    /// AI Features (controlled by AI-01, AI-02)
    /// </summary>
    public static class AI
    {
        public const string AIAssistant = "GRC.AI.Assistant";
        public const string RiskClassification = "GRC.AI.RiskClassification";
        public const string ComplianceAnalysis = "GRC.AI.ComplianceAnalysis";
        public const string DocumentAnalysis = "GRC.AI.DocumentAnalysis";
        public const string AutomaticRecommendations = "GRC.AI.AutomaticRecommendations";
    }

    /// <summary>
    /// Integration Features (controlled by IN-01, IN-02)
    /// </summary>
    public static class Integration
    {
        public const string KafkaIntegration = "GRC.Integration.Kafka";
        public const string CamundaIntegration = "GRC.Integration.Camunda";
        public const string ClickHouseAnalytics = "GRC.Integration.ClickHouse";
        public const string RedisCache = "GRC.Integration.Redis";
        public const string AzureServiceBus = "GRC.Integration.AzureServiceBus";
        public const string SlackNotifications = "GRC.Integration.Slack";
        public const string TeamsNotifications = "GRC.Integration.Teams";
    }

    /// <summary>
    /// Advanced Features
    /// </summary>
    public static class Advanced
    {
        public const string WorkflowAutomation = "GRC.Advanced.WorkflowAutomation";
        public const string CustomReporting = "GRC.Advanced.CustomReporting";
        public const string APIAccess = "GRC.Advanced.APIAccess";
        public const string BulkOperations = "GRC.Advanced.BulkOperations";
        public const string DataExport = "GRC.Advanced.DataExport";
    }

    /// <summary>
    /// Trial/Billing Features
    /// </summary>
    public static class Billing
    {
        public const string TrialMode = "GRC.Billing.TrialMode";
        public const string PaidFeatures = "GRC.Billing.PaidFeatures";
        public const string EnterpriseFeatures = "GRC.Billing.EnterpriseFeatures";
        public const string UnlimitedUsers = "GRC.Billing.UnlimitedUsers";
    }

    /// <summary>
    /// Security Features
    /// </summary>
    public static class Security
    {
        public const string MfaEnforcement = "GRC.Security.MfaEnforcement";
        public const string IpWhitelisting = "GRC.Security.IpWhitelisting";
        public const string SessionManagement = "GRC.Security.SessionManagement";
        public const string AuditLogExport = "GRC.Security.AuditLogExport";
    }

    /// <summary>
    /// Get all feature names
    /// </summary>
    public static string[] GetAllFeatures() => new[]
    {
        // Core
        Core.RiskManagement, Core.ComplianceManagement, Core.PolicyManagement,
        Core.AuditManagement, Core.IncidentManagement, Core.VendorManagement,
        // AI
        AI.AIAssistant, AI.RiskClassification, AI.ComplianceAnalysis,
        AI.DocumentAnalysis, AI.AutomaticRecommendations,
        // Integration
        Integration.KafkaIntegration, Integration.CamundaIntegration, Integration.ClickHouseAnalytics,
        Integration.RedisCache, Integration.AzureServiceBus, Integration.SlackNotifications, Integration.TeamsNotifications,
        // Advanced
        Advanced.WorkflowAutomation, Advanced.CustomReporting, Advanced.APIAccess,
        Advanced.BulkOperations, Advanced.DataExport,
        // Billing
        Billing.TrialMode, Billing.PaidFeatures, Billing.EnterpriseFeatures, Billing.UnlimitedUsers,
        // Security
        Security.MfaEnforcement, Security.IpWhitelisting, Security.SessionManagement, Security.AuditLogExport
    };

    /// <summary>
    /// Get features requiring approval for production changes
    /// </summary>
    public static string[] GetApprovalRequiredFeatures() => new[]
    {
        // AI features always require approval
        AI.AIAssistant, AI.RiskClassification, AI.ComplianceAnalysis,
        AI.DocumentAnalysis, AI.AutomaticRecommendations,
        // Integration features require approval
        Integration.KafkaIntegration, Integration.CamundaIntegration, Integration.ClickHouseAnalytics,
        // Security features require approval
        Security.MfaEnforcement, Security.IpWhitelisting, Security.SessionManagement,
        // Billing features require approval
        Billing.PaidFeatures, Billing.EnterpriseFeatures, Billing.UnlimitedUsers
    };
}
