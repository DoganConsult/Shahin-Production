namespace GrcMvc.Abp;

/// <summary>
/// Service for checking feature availability for current tenant
///
/// Usage in controllers/services:
/// if (await _featureCheck.IsEnabledAsync(GrcFeatures.AI.Enabled))
/// {
///     // AI feature is available for this tenant
/// }
///
/// Usage in Razor views:
/// @if (await FeatureCheck.IsEnabledAsync(GrcFeatures.Reporting.ExecutiveDashboard))
/// {
///     <a href="/dashboard/executive">Executive Dashboard</a>
/// }
/// </summary>
public interface IFeatureCheckService
{
    /// <summary>
    /// Check if a feature is enabled for the current tenant
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    /// Get the value of a feature (for numeric limits like MaxUsers)
    /// </summary>
    Task<string?> GetValueAsync(string featureName);

    /// <summary>
    /// Get the integer value of a feature
    /// </summary>
    Task<int> GetIntValueAsync(string featureName, int defaultValue = 0);

    /// <summary>
    /// Check if a limit has been reached (e.g., MaxUsers)
    /// </summary>
    Task<bool> IsLimitReachedAsync(string featureName, int currentCount);

    /// <summary>
    /// Get all features for the current tenant
    /// </summary>
    Task<Dictionary<string, string>> GetAllFeaturesAsync();

    /// <summary>
    /// Get the current tenant's edition name
    /// </summary>
    Task<string> GetCurrentEditionAsync();
}
