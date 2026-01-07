namespace GrcMvc.Configuration;

/// <summary>
/// Feature flags for gradual architecture enhancement
/// </summary>
public class GrcFeatureOptions
{
    public const string SectionName = "GrcFeatureFlags";
    
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
}
