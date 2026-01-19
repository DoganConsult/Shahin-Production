namespace GrcMvc.Configuration;

/// <summary>
/// GRC AI Governance Control Register
/// Defines AI controls for AI feature governance and responsible AI use.
/// Reference: AI-01, AI-02
/// </summary>
public static class AIGovernanceControls
{
    /// <summary>
    /// AI-01: AI Feature Enablement Governance
    /// Ensures AI features are enabled only with explicit tenant opt-in and documented approval.
    /// </summary>
    public static class AI01_FeatureEnablement
    {
        public const string ControlId = "AI-01";
        public const string Name = "AI Feature Enablement Governance";
        public const string Objective = "Ensure AI features are enabled only with explicit tenant opt-in and documented approval";
        public const string Owner = "Product Engineering + Security + Legal";
        public const string Frequency = "Per enablement; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "AI features processing tenant data without consent could violate privacy/regulatory requirements";

        // Control statement
        public const string Control = "AI features (Assistant, Classification, Risk Assessment, Compliance Analysis, Document Analysis) require explicit tenant opt-in. Enablement is logged and reversible.";

        // Test procedure
        public const string TestProcedure = "For each tenant with AI enabled, verify opt-in record exists. Verify AI features are disabled by default for new tenants.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Tenant AI opt-in records",
            "AI feature enablement audit log",
            "Default configuration showing AI disabled"
        };

        // Systems
        public static readonly string[] Systems = { "ABP FeatureManagement", "Tenant Settings", "Claude API" };

        // Event types for this control
        public static class Events
        {
            public const string AIOptInRequested = "AI01_OPTIN_REQUESTED";
            public const string AIOptInApproved = "AI01_OPTIN_APPROVED";
            public const string AIOptInDenied = "AI01_OPTIN_DENIED";
            public const string AIFeatureEnabled = "AI01_FEATURE_ENABLED";
            public const string AIFeatureDisabled = "AI01_FEATURE_DISABLED";
            public const string AIOptOutRequested = "AI01_OPTOUT_REQUESTED";
            public const string AIOptOutCompleted = "AI01_OPTOUT_COMPLETED";
            public const string AIUsageStarted = "AI01_USAGE_STARTED";
        }

        // AI features requiring opt-in
        public static readonly AIFeatureDefinition[] AIFeatures = new[]
        {
            new AIFeatureDefinition
            {
                FeatureId = "GRC.AI.Assistant",
                Name = "AI Assistant (Copilot)",
                Description = "Conversational AI assistant for GRC tasks",
                DataProcessed = "User queries, context from GRC entities",
                RequiresOptIn = true,
                DefaultEnabled = false
            },
            new AIFeatureDefinition
            {
                FeatureId = "GRC.AI.RiskClassification",
                Name = "Risk Classification",
                Description = "AI-powered risk categorization and scoring",
                DataProcessed = "Risk descriptions, control data",
                RequiresOptIn = true,
                DefaultEnabled = false
            },
            new AIFeatureDefinition
            {
                FeatureId = "GRC.AI.ComplianceAnalysis",
                Name = "Compliance Analysis",
                Description = "AI analysis of compliance gaps and recommendations",
                DataProcessed = "Compliance data, control assessments",
                RequiresOptIn = true,
                DefaultEnabled = false
            },
            new AIFeatureDefinition
            {
                FeatureId = "GRC.AI.DocumentAnalysis",
                Name = "Document Analysis",
                Description = "AI extraction and analysis of uploaded documents",
                DataProcessed = "Uploaded documents, policies, evidence",
                RequiresOptIn = true,
                DefaultEnabled = false
            },
            new AIFeatureDefinition
            {
                FeatureId = "GRC.AI.AutoRecommendations",
                Name = "Automatic Recommendations",
                Description = "AI-generated recommendations for actions",
                DataProcessed = "GRC entity data, historical decisions",
                RequiresOptIn = true,
                DefaultEnabled = false
            }
        };
    }

    /// <summary>
    /// AI-02: AI Usage Logging and Transparency
    /// Ensures all AI invocations are logged for transparency, auditability, and cost tracking.
    /// </summary>
    public static class AI02_UsageLogging
    {
        public const string ControlId = "AI-02";
        public const string Name = "AI Usage Logging and Transparency";
        public const string Objective = "Ensure all AI invocations are logged for transparency, auditability, and cost tracking";
        public const string Owner = "Platform Engineering + Security";
        public const string Frequency = "Continuous; reviewed monthly";

        // Risk addressed
        public const string RiskAddressed = "Lack of visibility into AI usage could mask abuse, errors, or unexpected costs";

        // Control statement
        public const string Control = "Every AI API call logs: timestamp, tenant, user, feature used, input/output size (not content), model, tokens used, latency, and result status. Logs are retained per AU-02.";

        // Test procedure
        public const string TestProcedure = "Sample 10 AI usage logs; verify all mandatory fields are populated. Check usage can be attributed to specific tenants/users.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "AI usage audit log export",
            "Token/cost usage reports by tenant",
            "Model invocation metrics dashboard"
        };

        // Systems
        public static readonly string[] Systems = { "Claude API", "Application Insights", "Cost Management" };

        // Event types for this control
        public static class Events
        {
            public const string AIRequestStarted = "AI02_REQUEST_STARTED";
            public const string AIRequestCompleted = "AI02_REQUEST_COMPLETED";
            public const string AIRequestFailed = "AI02_REQUEST_FAILED";
            public const string AIRateLimited = "AI02_RATE_LIMITED";
            public const string AIQuotaExceeded = "AI02_QUOTA_EXCEEDED";
            public const string AIUsageReportGenerated = "AI02_USAGE_REPORT_GENERATED";
            public const string AIAnomalyDetected = "AI02_ANOMALY_DETECTED";
        }

        // Mandatory log fields for AI usage
        public static readonly string[] MandatoryLogFields =
        {
            "Timestamp",
            "TenantId",
            "UserId",
            "FeatureId",
            "ModelId",
            "InputTokens",
            "OutputTokens",
            "LatencyMs",
            "Status",
            "CorrelationId"
        };

        // AI models in use
        public static readonly AIModelDefinition[] Models = new[]
        {
            new AIModelDefinition
            {
                ModelId = "claude-opus-4-5-20251101",
                Provider = "Anthropic",
                CostPerInputToken = 0.015m,
                CostPerOutputToken = 0.075m,
                MaxTokens = 200000,
                UseCase = "Complex analysis, document processing"
            },
            new AIModelDefinition
            {
                ModelId = "claude-sonnet-4-20250514",
                Provider = "Anthropic",
                CostPerInputToken = 0.003m,
                CostPerOutputToken = 0.015m,
                MaxTokens = 200000,
                UseCase = "General tasks, assistant queries"
            },
            new AIModelDefinition
            {
                ModelId = "claude-3-5-haiku-20241022",
                Provider = "Anthropic",
                CostPerInputToken = 0.001m,
                CostPerOutputToken = 0.005m,
                MaxTokens = 200000,
                UseCase = "Simple classification, quick responses"
            }
        };

        // Usage limits
        public static class UsageLimits
        {
            public const int MaxTokensPerRequestDefault = 10000;
            public const int MaxRequestsPerMinutePerTenant = 60;
            public const int MaxRequestsPerDayPerTenant = 1000;
            public const decimal MaxDailyCostPerTenantUSD = 50.0m;
        }
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        AI01_FeatureEnablement.ControlId,
        AI02_UsageLogging.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "AI-01" => (AI01_FeatureEnablement.Name, AI01_FeatureEnablement.Objective, AI01_FeatureEnablement.Owner),
        "AI-02" => (AI02_UsageLogging.Name, AI02_UsageLogging.Objective, AI02_UsageLogging.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };
}

/// <summary>
/// AI feature definition for AI-01
/// </summary>
public class AIFeatureDefinition
{
    public string FeatureId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataProcessed { get; set; } = string.Empty;
    public bool RequiresOptIn { get; set; } = true;
    public bool DefaultEnabled { get; set; } = false;
}

/// <summary>
/// AI model definition for AI-02
/// </summary>
public class AIModelDefinition
{
    public string ModelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public decimal CostPerInputToken { get; set; }
    public decimal CostPerOutputToken { get; set; }
    public int MaxTokens { get; set; }
    public string UseCase { get; set; } = string.Empty;
}

/// <summary>
/// AI usage tracking record for AI-02 compliance
/// </summary>
public class AIUsageRecord
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FeatureId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int LatencyMs { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failed, RateLimited
    public string? ErrorCode { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public decimal EstimatedCostUSD { get; set; }
}

/// <summary>
/// Tenant AI settings for AI-01 compliance
/// </summary>
public class TenantAISettings
{
    public Guid TenantId { get; set; }
    public bool AIOptedIn { get; set; }
    public DateTime? OptInDate { get; set; }
    public string? OptInApprovedBy { get; set; }
    public Dictionary<string, bool> EnabledFeatures { get; set; } = new();
    public int DailyTokenLimit { get; set; }
    public decimal DailyCostLimitUSD { get; set; }
    public DateTime? LastUsageDate { get; set; }
    public int TotalTokensUsedToday { get; set; }
}
