namespace GrcMvc.Configuration;

/// <summary>
/// Feature flags for gradual architecture enhancement and business module control
/// </summary>
public class GrcFeatureOptions
{
    public const string SectionName = "GrcFeatureFlags";

    #region Infrastructure Feature Flags

    /// <summary>
    /// Use enhanced secure password generation (crypto-safe RNG)
    /// </summary>
    public bool UseSecurePasswordGeneration { get; set; } = false;

    /// <summary>
    /// Use improved session-based claims (instead of DB-persisted)
    /// </summary>
    public bool UseSessionBasedClaims { get; set; } = false;

    /// <summary>
    /// Use enhanced audit logging (structured, no file I/O)
    /// </summary>
    public bool UseEnhancedAuditLogging { get; set; } = false;

    /// <summary>
    /// Use deterministic tenant resolution (instead of FirstOrDefault)
    /// </summary>
    public bool UseDeterministicTenantResolution { get; set; } = false;

    /// <summary>
    /// Remove hard-coded demo credentials
    /// </summary>
    public bool DisableDemoLogin { get; set; } = false;

    /// <summary>
    /// Enable canary deployment (percentage of users using enhanced code)
    /// Range: 0-100. 0 = all legacy, 100 = all enhanced
    /// </summary>
    public int CanaryPercentage { get; set; } = 0;

    /// <summary>
    /// Verify data consistency between legacy and enhanced (dual-read)
    /// </summary>
    public bool VerifyConsistency { get; set; } = false;

    /// <summary>
    /// Log all feature flag decisions for monitoring
    /// </summary>
    public bool LogFeatureFlagDecisions { get; set; } = true;

    #endregion

    #region Business Module Feature Flags

    /// <summary>
    /// Enable the Risk Management module
    /// </summary>
    public bool EnableRiskModule { get; set; } = true;

    /// <summary>
    /// Enable the Audit Management module
    /// </summary>
    public bool EnableAuditModule { get; set; } = true;

    /// <summary>
    /// Enable the Evidence Management module
    /// </summary>
    public bool EnableEvidenceModule { get; set; } = true;

    /// <summary>
    /// Enable the Policy Management module
    /// </summary>
    public bool EnablePolicyModule { get; set; } = true;

    /// <summary>
    /// Enable the Vendor Management module
    /// </summary>
    public bool EnableVendorModule { get; set; } = true;

    /// <summary>
    /// Enable the Workflow Engine module
    /// </summary>
    public bool EnableWorkflowModule { get; set; } = true;

    /// <summary>
    /// Enable the Compliance Calendar module
    /// </summary>
    public bool EnableComplianceCalendarModule { get; set; } = true;

    /// <summary>
    /// Enable the Action Plans module
    /// </summary>
    public bool EnableActionPlansModule { get; set; } = true;

    #endregion

    #region Integration Feature Flags

    /// <summary>
    /// Enable email notifications (requires SMTP/SendGrid/SES configuration)
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;

    /// <summary>
    /// Enable ClickHouse analytics (requires ClickHouse database)
    /// </summary>
    public bool EnableClickHouseAnalytics { get; set; } = false;

    /// <summary>
    /// Enable Kafka message integration (requires Kafka broker)
    /// </summary>
    public bool EnableKafkaIntegration { get; set; } = false;

    /// <summary>
    /// Enable Camunda workflow orchestration (requires Camunda server)
    /// </summary>
    public bool EnableCamundaWorkflows { get; set; } = false;

    /// <summary>
    /// Enable Redis caching (requires Redis server)
    /// </summary>
    public bool EnableRedisCaching { get; set; } = false;

    /// <summary>
    /// Enable SignalR real-time notifications
    /// </summary>
    public bool EnableSignalRNotifications { get; set; } = true;

    #endregion

    #region AI Feature Flags

    /// <summary>
    /// Enable Claude AI agents for GRC automation
    /// </summary>
    public bool EnableAiAgents { get; set; } = false;

    /// <summary>
    /// Enable AI-powered document classification
    /// </summary>
    public bool EnableAiClassification { get; set; } = false;

    /// <summary>
    /// Enable AI-powered risk assessment recommendations
    /// </summary>
    public bool EnableAiRiskAssessment { get; set; } = false;

    /// <summary>
    /// Enable AI-powered compliance gap analysis
    /// </summary>
    public bool EnableAiComplianceAnalysis { get; set; } = false;

    #endregion
}
