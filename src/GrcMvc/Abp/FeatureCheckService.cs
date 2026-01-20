using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;

namespace GrcMvc.Abp;

/// <summary>
/// Implementation of feature checking for Shahin GRC
/// Uses ABP's IFeatureChecker internally while maintaining backward compatibility
/// with custom methods for subscription/edition integration
/// </summary>
public class FeatureCheckService : IFeatureCheckService, ITransientDependency
{
    private readonly IFeatureChecker _abpFeatureChecker;
    private readonly ITenantContextService _tenantContext;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<FeatureCheckService> _logger;

    public FeatureCheckService(
        IFeatureChecker abpFeatureChecker,
        ITenantContextService tenantContext,
        ISubscriptionService subscriptionService,
        ILogger<FeatureCheckService> logger)
    {
        _abpFeatureChecker = abpFeatureChecker;
        _tenantContext = tenantContext;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        try
        {
            // First try ABP's feature checker
            try
            {
                var abpResult = await _abpFeatureChecker.IsEnabledAsync(featureName);
                return abpResult;
            }
            catch
            {
                // Fall back to edition-based features if ABP feature not defined
            }

            // Fall back to edition-based features
            var edition = await GetCurrentEditionAsync();
            var features = GetFeaturesForEdition(edition);

            if (features.TryGetValue(featureName, out var value))
            {
                return value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            return GetDefaultBoolValue(featureName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature {FeatureName}", featureName);
            return GetDefaultBoolValue(featureName);
        }
    }

    public async Task<string?> GetValueAsync(string featureName)
    {
        var edition = await GetCurrentEditionAsync();
        var features = GetFeaturesForEdition(edition);
        return features.TryGetValue(featureName, out var value) ? value : null;
    }

    public async Task<int> GetIntValueAsync(string featureName, int defaultValue = 0)
    {
        var value = await GetValueAsync(featureName);
        return int.TryParse(value, out var intValue) ? intValue : defaultValue;
    }

    public async Task<bool> IsLimitReachedAsync(string featureName, int currentCount)
    {
        var limit = await GetIntValueAsync(featureName, int.MaxValue);
        return currentCount >= limit;
    }

    public async Task<Dictionary<string, string>> GetAllFeaturesAsync()
    {
        var edition = await GetCurrentEditionAsync();
        return GetFeaturesForEdition(edition);
    }

    public async Task<string> GetCurrentEditionAsync()
    {
        try
        {
            if (!_tenantContext.HasTenantContext()) return "Trial";

            var tenantId = _tenantContext.GetCurrentTenantId();
            var subscription = await _subscriptionService.GetSubscriptionByTenantAsync(tenantId);
            return subscription?.Plan?.Name ?? "Trial";
        }
        catch
        {
            return "Trial";
        }
    }

    private Dictionary<string, string> GetFeaturesForEdition(string editionName)
    {
        var editions = GrcEditionDataSeeder.GetEditionDefinitions();
        var edition = editions.FirstOrDefault(e =>
            e.Name.Equals(editionName, StringComparison.OrdinalIgnoreCase));
        return edition?.Features ?? GetDefaultFeatures();
    }

    private Dictionary<string, string> GetDefaultFeatures() => new()
    {
        [GrcFeatures.MaxUsers] = "5",
        [GrcFeatures.MaxWorkspaces] = "1",
        [GrcFeatures.RiskManagement.Enabled] = "true",
        [GrcFeatures.Compliance.Enabled] = "true",
        [GrcFeatures.Audit.Enabled] = "true",
        [GrcFeatures.Reporting.Enabled] = "true",
        [GrcFeatures.AI.Enabled] = "false",
        [GrcFeatures.Workflow.Enabled] = "true"
    };

    private bool GetDefaultBoolValue(string featureName)
    {
        var defaults = GetDefaultFeatures();
        if (defaults.TryGetValue(featureName, out var value))
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        return featureName.Contains(".Enabled");
    }
}
