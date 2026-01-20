using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for PolicyStore - YAML loading, validation, caching, and hot-reload
/// </summary>
public class PolicyStoreTests : IDisposable
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<PolicyStore>> _mockLogger;
    private readonly PolicyStore _policyStore;
    private string? _tempPolicyFile;

    public PolicyStoreTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PolicyStore>>();
        _policyStore = new PolicyStore(_mockConfig.Object, _mockLogger.Object);
    }

    #region GetPolicyAsync Tests

    [Fact]
    public async Task GetPolicyAsync_WhenFileNotFound_ReturnsDefaultPolicy()
    {
        // Arrange
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns("nonexistent/path.yml");

        // Act
        var policy = await _policyStore.GetPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("default-policy", policy.Metadata.Name);
        Assert.Equal("1.0.0", policy.Metadata.Version);
        Assert.Equal("allow", policy.Spec.DefaultEffect);
        Assert.Empty(policy.Spec.Rules);
    }

    [Fact]
    public async Task GetPolicyAsync_WhenCached_ReturnsCachedPolicy()
    {
        // Arrange
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns("nonexistent/path.yml");

        // First call loads default
        var policy1 = await _policyStore.GetPolicyAsync();

        // Act - Second call should return cached
        var policy2 = await _policyStore.GetPolicyAsync();

        // Assert
        Assert.Same(policy1, policy2);
    }

    [Fact]
    public async Task GetPolicyAsync_WithValidYamlFile_LoadsPolicy()
    {
        // Arrange
        var yamlContent = @"
apiVersion: policy.doganconsult.io/v1
kind: Policy
metadata:
  name: test-policy
  version: '1.0.0'
spec:
  mode: enforce
  defaultEffect: deny
  rules:
    - id: TEST_RULE
      priority: 10
      effect: deny
      message: Test rule
";
        _tempPolicyFile = CreateTempPolicyFile(yamlContent);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(_tempPolicyFile);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("test-policy", policy.Metadata.Name);
        Assert.Equal("deny", policy.Spec.DefaultEffect);
        Assert.Single(policy.Spec.Rules);
        Assert.Equal("TEST_RULE", policy.Spec.Rules[0].Id);
    }

    #endregion

    #region ReloadPolicyAsync Tests

    [Fact]
    public async Task ReloadPolicyAsync_SetsCreatedAt_WhenMissing()
    {
        // Arrange
        var yamlContent = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: test-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: allow
";
        _tempPolicyFile = CreateTempPolicyFile(yamlContent);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(_tempPolicyFile);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotEqual(default(DateTime), policy.Metadata.CreatedAt);
        Assert.True(policy.Metadata.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ReloadPolicyAsync_FiresPolicyReloadedEvent()
    {
        // Arrange
        var yamlContent = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: event-test-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: allow
";
        _tempPolicyFile = CreateTempPolicyFile(yamlContent);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(_tempPolicyFile);

        PolicyDocument? eventPolicy = null;
        _policyStore.PolicyReloaded += (sender, args) => eventPolicy = args.Policy;

        // Act
        await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(eventPolicy);
        Assert.Equal("event-test-policy", eventPolicy.Metadata.Name);
    }

    [Fact]
    public async Task ReloadPolicyAsync_HandlesRelativePath()
    {
        // Arrange - Use relative path
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns("etc/policies/test.yml");

        // Act & Assert - Should not throw, even if file doesn't exist
        var policy = await _policyStore.GetPolicyAsync();
        Assert.NotNull(policy);
    }

    #endregion

    #region ValidatePolicyAsync Tests

    [Fact]
    public async Task ValidatePolicyAsync_WithNullSpec_ReturnsFalse()
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = null!
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePolicyAsync_WithNoRules_ReturnsFalse()
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec { Rules = new List<PolicyRule>() }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePolicyAsync_WithEmptyRuleId_ReturnsFalse()
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "", Priority = 10, Effect = "deny" }
                }
            }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    [InlineData(99999)]
    public async Task ValidatePolicyAsync_WithInvalidPriority_ReturnsFalse(int priority)
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "RULE1", Priority = priority, Effect = "deny" }
                }
            }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePolicyAsync_WithEmptyEffect_ReturnsFalse()
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "RULE1", Priority = 10, Effect = "" }
                }
            }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(5000)]
    [InlineData(10000)]
    public async Task ValidatePolicyAsync_WithValidPriority_ReturnsTrue(int priority)
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "RULE1", Priority = priority, Effect = "deny" }
                }
            }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidatePolicyAsync_WithMultipleValidRules_ReturnsTrue()
    {
        // Arrange
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata { Name = "test" },
            Spec = new PolicySpec
            {
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "RULE1", Priority = 10, Effect = "deny" },
                    new PolicyRule { Id = "RULE2", Priority = 20, Effect = "allow" },
                    new PolicyRule { Id = "RULE3", Priority = 30, Effect = "audit" }
                }
            }
        };

        // Act
        var result = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region IHostedService Tests

    [Fact]
    public async Task StartAsync_WithNonExistentDirectory_CompletesWithoutError()
    {
        // Arrange
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns("nonexistent/dir/policy.yml");

        // Act & Assert - Should complete without throwing
        await _policyStore.StartAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_CompletesWithoutError()
    {
        // Act & Assert - Should complete without throwing
        await _policyStore.StopAsync(CancellationToken.None);
    }

    #endregion

    #region YAML Parsing Tests

    [Fact]
    public async Task ReloadPolicyAsync_ParsesComplexYaml_WithAllFields()
    {
        // Arrange
        var yamlContent = @"
apiVersion: policy.doganconsult.io/v1
kind: Policy
metadata:
  name: complex-policy
  namespace: default
  version: '2.0.0'
  labels:
    environment: production
spec:
  mode: enforce
  defaultEffect: deny
  execution:
    order: sequential
    shortCircuit: true
    conflictStrategy: denyOverrides
  target:
    resourceTypes:
      - Evidence
      - Risk
    environments:
      - dev
      - prod
  rules:
    - id: REQUIRE_CLASSIFICATION
      priority: 10
      enabled: true
      match:
        resource:
          type: Evidence
        environment: '*'
      when:
        - op: exists
          path: metadata.labels.dataClassification
      effect: deny
      severity: high
      message: Data classification required
      remediation:
        hint: Add dataClassification label
";
        _tempPolicyFile = CreateTempPolicyFile(yamlContent);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(_tempPolicyFile);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.Equal("complex-policy", policy.Metadata.Name);
        Assert.Equal("2.0.0", policy.Metadata.Version);
        Assert.Equal("enforce", policy.Spec.Mode);
        Assert.Equal("deny", policy.Spec.DefaultEffect);
        Assert.Equal("sequential", policy.Spec.Execution.Order);
        Assert.True(policy.Spec.Execution.ShortCircuit);
        Assert.Equal("denyOverrides", policy.Spec.Execution.ConflictStrategy);
        Assert.Contains("Evidence", policy.Spec.Target.ResourceTypes);
        Assert.Contains("Risk", policy.Spec.Target.ResourceTypes);

        var rule = policy.Spec.Rules[0];
        Assert.Equal("REQUIRE_CLASSIFICATION", rule.Id);
        Assert.Equal(10, rule.Priority);
        Assert.True(rule.Enabled);
        Assert.Equal("deny", rule.Effect);
        Assert.Equal("high", rule.Severity);
        Assert.Single(rule.When);
        Assert.Equal("exists", rule.When[0].Op);
    }

    [Fact]
    public async Task ReloadPolicyAsync_ParsesExceptions()
    {
        // Arrange
        var yamlContent = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: exception-policy
  version: '1.0.0'
spec:
  defaultEffect: deny
  rules:
    - id: DENY_ALL
      priority: 10
      effect: deny
  exceptions:
    - id: DEV_EXCEPTION
      ruleIds:
        - DENY_ALL
      reason: Allow in dev environment
      expiresAt: '2030-12-31T23:59:59Z'
      match:
        environment: dev
";
        _tempPolicyFile = CreateTempPolicyFile(yamlContent);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(_tempPolicyFile);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy.Spec.Exceptions);
        Assert.Single(policy.Spec.Exceptions);
        var exception = policy.Spec.Exceptions[0];
        Assert.Equal("DEV_EXCEPTION", exception.Id);
        Assert.Contains("DENY_ALL", exception.RuleIds);
        Assert.Equal("Allow in dev environment", exception.Reason);
    }

    #endregion

    #region Helper Methods

    private string CreateTempPolicyFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    public void Dispose()
    {
        if (_tempPolicyFile != null && File.Exists(_tempPolicyFile))
        {
            File.Delete(_tempPolicyFile);
        }
    }

    #endregion
}
