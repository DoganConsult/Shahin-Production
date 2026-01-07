using Xunit;
using Moq;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for PolicyEnforcer - validates policy evaluation and conflict resolution
/// </summary>
public class PolicyEnforcerTests
{
    private readonly Mock<IPolicyStore> _mockStore = new();
    private readonly Mock<IDotPathResolver> _mockResolver = new();
    private readonly Mock<IMutationApplier> _mockMutator = new();
    private readonly Mock<IPolicyAuditLogger> _mockAuditLogger = new();
    private readonly Mock<ILogger<PolicyEnforcer>> _mockLogger = new();
    private readonly PolicyEnforcer _enforcer;

    public PolicyEnforcerTests()
    {
        _enforcer = new PolicyEnforcer(
            _mockStore.Object,
            _mockResolver.Object,
            _mockMutator.Object,
            _mockAuditLogger.Object,
            Options.Create(new PolicyOptions { FilePath = "test.yml" }),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task EnforceAsync_NoMatchingRules_AllowsByDefault()
    {
        var policy = new PolicyDocument
        {
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
                Rules = new List<PolicyRule>()
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Test",
            Resource = new { },
            PrincipalId = "user1",
            PrincipalRoles = new List<string>()
        };
        
        // Should not throw
        await _enforcer.EnforceAsync(context);
    }

    [Fact]
    public async Task EnforceAsync_DenyRule_ThrowsPolicyViolationException()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = true, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = "DENY_TEST",
                        Priority = 10,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } },
                        When = new List<PolicyCondition>(),
                        Effect = "deny",
                        Message = "Test denied",
                        Remediation = new PolicyRemediation { Hint = "Fix test" }
                    }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(() => _enforcer.EnforceAsync(context));
        Assert.Equal("Test denied", exception.Message);
        Assert.Equal("DENY_TEST", exception.RuleId);
        Assert.Equal("Fix test", exception.RemediationHint);
    }

    [Fact]
    public async Task EnforceAsync_DenyOverrides_DenyWins()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = false, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "ALLOW_1", Priority = 10, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "allow", Message = "" },
                    new PolicyRule { Id = "DENY_1", Priority = 20, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Denied", Remediation = new PolicyRemediation { Hint = "Fix" } }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        await Assert.ThrowsAsync<PolicyViolationException>(() => _enforcer.EnforceAsync(context));
    }

    [Fact]
    public async Task EnforceAsync_AllowOverrides_AllowWins()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "deny",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = false, ConflictStrategy = "allowOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "DENY_1", Priority = 10, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Denied" },
                    new PolicyRule { Id = "ALLOW_1", Priority = 20, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "allow", Message = "" }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        // Should not throw
        await _enforcer.EnforceAsync(context);
    }

    [Fact]
    public async Task EnforceAsync_HighestPriorityWins_FirstRuleDecides()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = false, ConflictStrategy = "highestPriorityWins" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "DENY_LOW", Priority = 100, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Low priority deny" },
                    new PolicyRule { Id = "ALLOW_HIGH", Priority = 1, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "allow", Message = "" }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        // Should not throw - ALLOW_HIGH (priority 1) wins over DENY_LOW (priority 100)
        await _enforcer.EnforceAsync(context);
    }

    [Fact]
    public async Task EnforceAsync_DisabledRule_IsSkipped()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = true, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "DISABLED_DENY", Priority = 10, Enabled = false, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Should not trigger" }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        // Should not throw - disabled rule is skipped
        await _enforcer.EnforceAsync(context);
    }

    [Fact]
    public async Task EnforceAsync_ShortCircuit_StopsAtFirstDeny()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = true, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "DENY_1", Priority = 10, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "First deny", Remediation = new PolicyRemediation { Hint = "Fix" } },
                    new PolicyRule { Id = "DENY_2", Priority = 20, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Second deny" }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(() => _enforcer.EnforceAsync(context));
        Assert.Equal("DENY_1", exception.RuleId); // Should stop at first deny
    }

    [Fact]
    public async Task EnforceAsync_MutateEffect_AppliesMutation()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = false, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = "NORMALIZE",
                        Priority = 10,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } },
                        When = new List<PolicyCondition>(),
                        Effect = "mutate",
                        Message = "",
                        Mutations = new List<PolicyMutation> { new PolicyMutation { Op = "set", Path = "owner", Value = null } }
                    }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var resource = new Dictionary<string, object> { ["owner"] = "unknown" };
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = resource, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        await _enforcer.EnforceAsync(context);
        
        _mockMutator.Verify(m => m.ApplyMutation(resource, It.IsAny<PolicyMutation>()), Times.Once);
    }

    [Fact]
    public async Task EnforceAsync_AuditMode_DoesNotEnforce()
    {
        var policy = new PolicyDocument
        {
            Spec = new PolicySpec
            {
                Mode = "audit",
                DefaultEffect = "allow",
                Execution = new PolicyExecution { Order = "sequential", ShortCircuit = true, ConflictStrategy = "denyOverrides" },
                Rules = new List<PolicyRule>
                {
                    new PolicyRule { Id = "DENY_TEST", Priority = 10, Enabled = true, Match = new PolicyMatch { Resource = new ResourceMatch { Type = "Test" } }, When = new List<PolicyCondition>(), Effect = "deny", Message = "Should not block" }
                }
            }
        };
        
        _mockStore.Setup(s => s.GetCurrentPolicyAsync()).ReturnsAsync(policy);
        
        var context = new PolicyContext { Action = "create", Environment = "dev", ResourceType = "Test", Resource = new { }, PrincipalId = "user1", PrincipalRoles = new List<string>() };
        
        // Should not throw in audit mode
        await _enforcer.EnforceAsync(context);
    }
}
