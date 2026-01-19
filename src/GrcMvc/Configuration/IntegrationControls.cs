namespace GrcMvc.Configuration;

/// <summary>
/// GRC Integration Control Register
/// Defines IN controls for external integration governance.
/// Reference: IN-01, IN-02
/// </summary>
public static class IntegrationControls
{
    /// <summary>
    /// IN-01: Integration Enablement Approval
    /// Ensures external integrations are enabled only with proper approval and security review.
    /// </summary>
    public static class IN01_IntegrationApproval
    {
        public const string ControlId = "IN-01";
        public const string Name = "Integration Enablement Approval";
        public const string Objective = "Ensure external integrations are enabled only with proper approval and security review";
        public const string Owner = "Security Engineering + Platform Engineering";
        public const string Frequency = "Per integration change; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Uncontrolled external integrations could expose data or introduce vulnerabilities";

        // Control statement
        public const string Control = "Enabling any external integration (Kafka, Camunda, ClickHouse, Redis, Slack, Teams, etc.) requires documented approval with security review. Integration configs are audited.";

        // Test procedure
        public const string TestProcedure = "Review enabled integrations; verify each has approval ticket with security sign-off. Check for unauthorized integrations.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Integration enablement approval tickets",
            "Security review documentation",
            "Integration configuration audit log"
        };

        // Systems
        public static readonly string[] Systems = { "Azure DevOps", "Azure Key Vault", "ABP FeatureManagement" };

        // Event types for this control
        public static class Events
        {
            public const string IntegrationRequested = "IN01_INTEGRATION_REQUESTED";
            public const string IntegrationApproved = "IN01_INTEGRATION_APPROVED";
            public const string IntegrationDenied = "IN01_INTEGRATION_DENIED";
            public const string IntegrationEnabled = "IN01_INTEGRATION_ENABLED";
            public const string IntegrationDisabled = "IN01_INTEGRATION_DISABLED";
            public const string IntegrationConfigChanged = "IN01_INTEGRATION_CONFIG_CHANGED";
            public const string SecurityReviewCompleted = "IN01_SECURITY_REVIEW_COMPLETED";
        }

        // Controlled integrations requiring approval
        public static readonly IntegrationDefinition[] ControlledIntegrations = new[]
        {
            new IntegrationDefinition { Name = "Kafka", Category = "Messaging", RiskLevel = "High", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "Camunda", Category = "Workflow", RiskLevel = "Medium", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "ClickHouse", Category = "Analytics", RiskLevel = "Medium", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "Redis", Category = "Caching", RiskLevel = "Low", RequiresSecurityReview = false },
            new IntegrationDefinition { Name = "AzureServiceBus", Category = "Messaging", RiskLevel = "Medium", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "Slack", Category = "Notification", RiskLevel = "Low", RequiresSecurityReview = false },
            new IntegrationDefinition { Name = "Teams", Category = "Notification", RiskLevel = "Low", RequiresSecurityReview = false },
            new IntegrationDefinition { Name = "Twilio", Category = "Communication", RiskLevel = "Medium", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "Stripe", Category = "Payment", RiskLevel = "High", RequiresSecurityReview = true },
            new IntegrationDefinition { Name = "SendGrid", Category = "Email", RiskLevel = "Low", RequiresSecurityReview = false }
        };
    }

    /// <summary>
    /// IN-02: Integration Credential Management
    /// Ensures integration credentials are securely stored, rotated, and audited.
    /// </summary>
    public static class IN02_CredentialManagement
    {
        public const string ControlId = "IN-02";
        public const string Name = "Integration Credential Management";
        public const string Objective = "Ensure integration credentials are securely stored, rotated, and audited";
        public const string Owner = "Security Engineering";
        public const string Frequency = "Continuous; rotation per policy; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Compromised or stale credentials could enable unauthorized access";

        // Control statement
        public const string Control = "All integration credentials (API keys, connection strings, secrets) are stored in Azure Key Vault, rotated per schedule, and access is logged.";

        // Test procedure
        public const string TestProcedure = "Verify all integration secrets are in Key Vault (not in code/config). Check rotation dates meet policy. Review access logs.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Key Vault secret inventory",
            "Secret rotation schedule and execution logs",
            "Key Vault access audit logs"
        };

        // Systems
        public static readonly string[] Systems = { "Azure Key Vault", "Azure AD", "Application Insights" };

        // Event types for this control
        public static class Events
        {
            public const string CredentialCreated = "IN02_CREDENTIAL_CREATED";
            public const string CredentialRotated = "IN02_CREDENTIAL_ROTATED";
            public const string CredentialExpired = "IN02_CREDENTIAL_EXPIRED";
            public const string CredentialAccessed = "IN02_CREDENTIAL_ACCESSED";
            public const string CredentialRevoked = "IN02_CREDENTIAL_REVOKED";
            public const string RotationDue = "IN02_ROTATION_DUE";
            public const string RotationOverdue = "IN02_ROTATION_OVERDUE";
        }

        // Credential types and rotation policy
        public static readonly CredentialPolicy[] CredentialPolicies = new[]
        {
            new CredentialPolicy { Type = "APIKey", RotationDays = 90, RequiresMFA = true },
            new CredentialPolicy { Type = "ConnectionString", RotationDays = 180, RequiresMFA = true },
            new CredentialPolicy { Type = "ClientSecret", RotationDays = 90, RequiresMFA = true },
            new CredentialPolicy { Type = "Certificate", RotationDays = 365, RequiresMFA = true },
            new CredentialPolicy { Type = "WebhookSecret", RotationDays = 90, RequiresMFA = false },
            new CredentialPolicy { Type = "EncryptionKey", RotationDays = 365, RequiresMFA = true }
        };
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        IN01_IntegrationApproval.ControlId,
        IN02_CredentialManagement.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "IN-01" => (IN01_IntegrationApproval.Name, IN01_IntegrationApproval.Objective, IN01_IntegrationApproval.Owner),
        "IN-02" => (IN02_CredentialManagement.Name, IN02_CredentialManagement.Objective, IN02_CredentialManagement.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };
}

/// <summary>
/// Integration definition for IN-01
/// </summary>
public class IntegrationDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
    public bool RequiresSecurityReview { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledDate { get; set; }
    public string? ApprovalTicket { get; set; }
}

/// <summary>
/// Credential rotation policy for IN-02
/// </summary>
public class CredentialPolicy
{
    public string Type { get; set; } = string.Empty;
    public int RotationDays { get; set; }
    public bool RequiresMFA { get; set; }
}

/// <summary>
/// Integration status tracker
/// </summary>
public static class IntegrationStatus
{
    /// <summary>
    /// Current integration enablement status
    /// </summary>
    public static readonly Dictionary<string, bool> EnabledIntegrations = new()
    {
        { "Kafka", false },
        { "Camunda", false },
        { "ClickHouse", false },
        { "Redis", true },
        { "AzureServiceBus", false },
        { "Slack", true },
        { "Teams", true },
        { "Twilio", true },
        { "Stripe", true },
        { "SendGrid", true }
    };

    /// <summary>
    /// Get integrations requiring security review that are enabled
    /// </summary>
    public static string[] GetHighRiskEnabledIntegrations() =>
        IntegrationControls.IN01_IntegrationApproval.ControlledIntegrations
            .Where(i => i.RequiresSecurityReview && EnabledIntegrations.GetValueOrDefault(i.Name, false))
            .Select(i => i.Name)
            .ToArray();
}
