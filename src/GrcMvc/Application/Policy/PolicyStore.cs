using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using System.IO;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Policy store with hot-reload capability - automatically reloads policy when YAML file changes
/// </summary>
public class PolicyStore : IPolicyStore, IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolicyStore> _logger;
    private readonly IDeserializer _yamlDeserializer;
    private PolicyDocument? _cachedPolicy;
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;

    public event EventHandler<PolicyReloadedEventArgs>? PolicyReloaded;

    public PolicyStore(
        IConfiguration configuration,
        ILogger<PolicyStore> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<PolicyDocument> GetPolicyAsync(CancellationToken ct = default)
    {
        if (_cachedPolicy != null)
            return _cachedPolicy;

        return await ReloadPolicyAsync(ct);
    }

    public async Task<PolicyDocument> ReloadPolicyAsync(CancellationToken ct = default)
    {
        var policyPath = _configuration["Policy:FilePath"] ?? "etc/policies/grc-baseline.yml";
        
        // Handle relative paths
        if (!Path.IsPathRooted(policyPath))
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (basePath != null)
            {
                // Go up from bin/Debug/net8.0 to project root
                var projectRoot = Path.GetFullPath(Path.Combine(basePath, "../../../.."));
                policyPath = Path.Combine(projectRoot, policyPath);
            }
        }

        if (!File.Exists(policyPath))
        {
            _logger.LogWarning("Policy file not found at {Path}, using default policy", policyPath);
            return GetDefaultPolicy();
        }

        try
        {
            var yamlContent = await File.ReadAllTextAsync(policyPath, ct);
            
            // Deserialize policy (YamlDotNet handles DateTime automatically)
            var policy = _yamlDeserializer.Deserialize<PolicyDocument>(yamlContent);
            
            // Ensure CreatedAt is set if missing
            if (policy.Metadata.CreatedAt == default)
            {
                policy.Metadata.CreatedAt = DateTime.UtcNow;
            }
            
            lock (_lock)
            {
                _cachedPolicy = policy;
            }

            _logger.LogInformation("Policy reloaded successfully from {Path}", policyPath);
            PolicyReloaded?.Invoke(this, new PolicyReloadedEventArgs { Policy = policy });
            
            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading policy from {Path}", policyPath);
            throw;
        }
    }

    public async Task<bool> ValidatePolicyAsync(PolicyDocument policy, CancellationToken ct = default)
    {
        if (policy.Spec == null)
        {
            _logger.LogWarning("Policy spec is null");
            return false;
        }

        if (!policy.Spec.Rules.Any())
        {
            _logger.LogWarning("Policy has no rules");
            return false;
        }

        // Validate rules
        foreach (var rule in policy.Spec.Rules)
        {
            if (string.IsNullOrEmpty(rule.Id))
            {
                _logger.LogWarning("Rule has empty ID");
                return false;
            }
            if (rule.Priority < 1 || rule.Priority > 10000)
            {
                _logger.LogWarning("Rule {RuleId} has invalid priority {Priority}", rule.Id, rule.Priority);
                return false;
            }
            if (string.IsNullOrEmpty(rule.Effect))
            {
                _logger.LogWarning("Rule {RuleId} has empty effect", rule.Id);
                return false;
            }
        }

        return await Task.FromResult(true);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var policyPath = _configuration["Policy:FilePath"] ?? "etc/policies/grc-baseline.yml";
        
        // Handle relative paths
        if (!Path.IsPathRooted(policyPath))
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (basePath != null)
            {
                var projectRoot = Path.GetFullPath(Path.Combine(basePath, "../../../.."));
                policyPath = Path.Combine(projectRoot, policyPath);
            }
        }

        var directory = Path.GetDirectoryName(policyPath);
        var fileName = Path.GetFileName(policyPath);

        if (directory != null && Directory.Exists(directory))
        {
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _watcher.Changed += async (sender, e) =>
            {
                _logger.LogInformation("Policy file changed, reloading...");
                await Task.Delay(500); // Wait for file to be fully written
                try
                {
                    await ReloadPolicyAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading policy after file change");
                }
            };
            _watcher.EnableRaisingEvents = true;
            _logger.LogInformation("Policy file watcher started for {Path}", policyPath);
        }
        else
        {
            _logger.LogWarning("Policy directory not found: {Directory}", directory);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    private PolicyDocument GetDefaultPolicy()
    {
        return new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "default-policy",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                DefaultEffect = "allow",
                Rules = new List<PolicyRule>()
            }
        };
    }
}
