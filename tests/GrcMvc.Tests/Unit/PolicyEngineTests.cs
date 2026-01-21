using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for the Policy Engine components:
/// - DotPathResolver
/// - PolicyEnforcer condition evaluation
/// - Conflict strategy resolution
/// - Exception matching
/// </summary>
public class PolicyEngineTests
{
    #region DotPathResolver Tests

    [Fact]
    public void DotPathResolver_ResolvesSimplePath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var obj = new TestResource { Id = Guid.NewGuid(), Title = "Test Title" };

        // Act
        var result = resolver.Resolve(obj, "Title");

        // Assert
        Assert.Equal("Test Title", result);
    }

    [Fact]
    public void DotPathResolver_ResolvesNestedPath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var wrapper = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "confidential",
                    ["owner"] = "test-user"
                }
            }
        };

        // Act
        var result = resolver.Resolve(wrapper, "metadata.labels.dataClassification");

        // Assert
        Assert.Equal("confidential", result);
    }

    [Fact]
    public void DotPathResolver_ReturnsNullForMissingPath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var obj = new TestResource { Id = Guid.NewGuid(), Title = "Test" };

        // Act
        var result = resolver.Resolve(obj, "NonExistentProperty");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DotPathResolver_HandlesNullPath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var obj = new TestResource { Id = Guid.NewGuid(), Title = "Test" };

        // Act
        var result = resolver.Resolve(obj, "");

        // Assert
        Assert.Same(obj, result);
    }

    [Fact]
    public void DotPathResolver_HandlesDictionaryAccess()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var dict = new Dictionary<string, object>
        {
            ["level1"] = new Dictionary<string, object>
            {
                ["level2"] = "deep-value"
            }
        };

        // Act
        var result = resolver.Resolve(dict, "level1.level2");

        // Assert
        Assert.Equal("deep-value", result);
    }

    [Fact]
    public void DotPathResolver_Exists_ReturnsTrueForExistingPath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var obj = new TestResource { Id = Guid.NewGuid(), Title = "Test" };

        // Act
        var exists = resolver.Exists(obj, "Title");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void DotPathResolver_Exists_ReturnsFalseForMissingPath()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DotPathResolver>>();
        var resolver = new DotPathResolver(cache, logger.Object);

        var obj = new TestResource { Id = Guid.NewGuid(), Title = "Test" };

        // Act
        var exists = resolver.Exists(obj, "NonExistent");

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region PolicyEnforcer Condition Tests

    [Fact]
    public async Task PolicyEnforcer_EvaluatesEqualsCondition()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "TEST_EQUALS",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>
            {
                new() { Op = "equals", Path = "metadata.labels.dataClassification", Value = "restricted" }
            },
            Effect = "deny",
            Message = "Restricted data not allowed"
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("restricted");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("TEST_EQUALS", decision.MatchedRuleId);
    }

    [Fact]
    public async Task PolicyEnforcer_EvaluatesNotEqualsCondition()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "TEST_NOT_EQUALS",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>
            {
                new() { Op = "notEquals", Path = "metadata.labels.dataClassification", Value = "public" }
            },
            Effect = "deny",
            Message = "Only public data allowed"
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("confidential");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
    }

    [Fact]
    public async Task PolicyEnforcer_EvaluatesMatchesCondition()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "TEST_MATCHES",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>
            {
                new() { Op = "matches", Path = "metadata.labels.owner", Value = "^team-.*" }
            },
            Effect = "allow",
            Message = "Team ownership verified"
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContextWithOwner("team-security");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect);
    }

    [Fact]
    public async Task PolicyEnforcer_EvaluatesInCondition()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "TEST_IN",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>
            {
                new() { Op = "in", Path = "metadata.labels.dataClassification", Value = new List<object> { "public", "internal" } }
            },
            Effect = "allow",
            Message = "Classification allowed"
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("internal");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect);
    }

    #endregion

    #region Conflict Strategy Tests

    [Fact]
    public async Task PolicyEnforcer_DenyOverrides_DenyWins()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(
            new PolicyRule
            {
                Id = "RULE_ALLOW",
                Priority = 10,
                Enabled = true,
                Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                When = new List<PolicyCondition>(),
                Effect = "allow",
                Message = "Allow rule"
            },
            new PolicyRule
            {
                Id = "RULE_DENY",
                Priority = 20,
                Enabled = true,
                Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                When = new List<PolicyCondition>(),
                Effect = "deny",
                Message = "Deny rule"
            }
        );
        policy.Spec.Execution.ConflictStrategy = "denyOverrides";
        policy.Spec.Execution.ShortCircuit = false;

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("internal");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
    }

    [Fact]
    public async Task PolicyEnforcer_ShortCircuit_StopsOnFirstDeny()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(
            new PolicyRule
            {
                Id = "FIRST_DENY",
                Priority = 10,
                Enabled = true,
                Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                When = new List<PolicyCondition>(),
                Effect = "deny",
                Message = "First deny rule"
            },
            new PolicyRule
            {
                Id = "SECOND_RULE",
                Priority = 20,
                Enabled = true,
                Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                When = new List<PolicyCondition>(),
                Effect = "allow",
                Message = "Second rule should not be reached"
            }
        );
        policy.Spec.Execution.ShortCircuit = true;

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("internal");

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("FIRST_DENY", decision.MatchedRuleId);
    }

    #endregion

    #region Exception Tests

    [Fact]
    public async Task PolicyEnforcer_ExceptionBypassesRule()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "DENY_RULE",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>(),
            Effect = "deny",
            Message = "Should be denied"
        });

        // Add exception for dev environment
        policy.Spec.Exceptions = new List<PolicyException>
        {
            new()
            {
                Id = "DEV_EXCEPTION",
                RuleIds = new List<string> { "DENY_RULE" },
                Reason = "Dev bypass",
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Match = new PolicyMatch
                {
                    Resource = new PolicyResourceMatch { Type = "Any" },
                    Environment = "dev"
                }
            }
        };

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = CreateTestWrapper("internal", "test-owner")
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect); // Default effect since exception bypasses the rule
    }

    [Fact]
    public async Task PolicyEnforcer_ExpiredException_DoesNotApply()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "DENY_RULE",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>(),
            Effect = "deny",
            Message = "Should be denied"
        });

        // Add expired exception
        policy.Spec.Exceptions = new List<PolicyException>
        {
            new()
            {
                Id = "EXPIRED_EXCEPTION",
                RuleIds = new List<string> { "DENY_RULE" },
                Reason = "Expired bypass",
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
                Match = new PolicyMatch
                {
                    Resource = new PolicyResourceMatch { Type = "Any" },
                    Environment = "dev"
                }
            }
        };

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = CreateTestWrapper("internal", "test-owner")
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect); // Exception expired, so rule applies
    }

    #endregion

    #region PolicyViolationException Tests

    [Fact]
    public async Task PolicyEnforcer_EnforceAsync_ThrowsOnDeny()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "DENY_ALL",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>(),
            Effect = "deny",
            Message = "Access denied",
            Remediation = new PolicyRemediation { Hint = "Contact administrator" }
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("internal");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(
            () => enforcer.EnforceAsync(context));

        Assert.Equal("DENY_ALL", exception.RuleId);
        Assert.Contains("Access denied", exception.Message);
        Assert.Equal("Contact administrator", exception.RemediationHint);
    }

    [Fact]
    public async Task PolicyEnforcer_IsAllowedAsync_ReturnsFalseOnDeny()
    {
        // Arrange
        var (enforcer, policyStore) = CreateEnforcerWithMocks();

        var policy = CreateTestPolicy(new PolicyRule
        {
            Id = "DENY_RULE",
            Priority = 10,
            Enabled = true,
            Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
            When = new List<PolicyCondition>(),
            Effect = "deny",
            Message = "Denied"
        });

        policyStore.Setup(x => x.GetPolicyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateTestContext("internal");

        // Act
        var isAllowed = await enforcer.IsAllowedAsync(context);

        // Assert
        Assert.False(isAllowed);
    }

    #endregion

    #region Helper Methods

    private (PolicyEnforcer, Mock<IPolicyStore>) CreateEnforcerWithMocks()
    {
        var policyStore = new Mock<IPolicyStore>();
        var pathResolver = new DotPathResolver(
            new MemoryCache(new MemoryCacheOptions()),
            new Mock<ILogger<DotPathResolver>>().Object);
        var mutationApplier = new Mock<IMutationApplier>();
        var auditLogger = new Mock<IPolicyAuditLogger>();
        var logger = new Mock<ILogger<PolicyEnforcer>>();

        var enforcer = new PolicyEnforcer(
            policyStore.Object,
            pathResolver,
            mutationApplier.Object,
            auditLogger.Object,
            logger.Object);

        return (enforcer, policyStore);
    }

    private PolicyDocument CreateTestPolicy(params PolicyRule[] rules)
    {
        return new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "test-policy",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution
                {
                    Order = "sequential",
                    ShortCircuit = true,
                    ConflictStrategy = "denyOverrides"
                },
                Target = new PolicyTarget
                {
                    ResourceTypes = new List<string> { "Any" },
                    Environments = new List<string> { "dev", "staging", "prod" }
                },
                Rules = new List<PolicyRule>(rules),
                Exceptions = new List<PolicyException>()
            }
        };
    }

    private PolicyContext CreateTestContext(string dataClassification)
    {
        return new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = CreateTestWrapper(dataClassification, "test-owner")
        };
    }

    private PolicyContext CreateTestContextWithOwner(string owner)
    {
        return new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = CreateTestWrapper("internal", owner)
        };
    }

    private PolicyResourceWrapper CreateTestWrapper(string dataClassification, string owner)
    {
        return new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test Resource",
            Type = "Evidence",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = dataClassification,
                    ["owner"] = owner
                }
            }
        };
    }

    #endregion

    #region Test Classes

    private class TestResource
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    #endregion
}
