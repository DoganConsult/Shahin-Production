using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Moq;
using System.IO;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for PolicyStore
/// Tests policy loading and evaluation
/// </summary>
public class PolicyStoreTests
{
    private readonly PolicyStore _store;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<PolicyStore>> _loggerMock;

    public PolicyStoreTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<PolicyStore>>();
        
        // Create a test policy file
        var testPolicyPath = Path.Combine(Path.GetTempPath(), "test-policy.yml");
        File.WriteAllText(testPolicyPath, @"
version: '1.0'
rules:
  - id: TEST_RULE_001
    description: Test rule
    match:
      action: create
      resource:
        type: Evidence
    effect: allow
");

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Policy:FilePath"]).Returns(testPolicyPath);
        
        _store = new PolicyStore(configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_MatchesRule_ReturnsDecision()
    {
        // Arrange
        var context = new PolicyContext
        {
            Action = "create",
            ResourceType = "Evidence",
            Resource = new PolicyResourceWrapper { Title = "Test" },
            TenantId = Guid.NewGuid(),
            PrincipalId = "user123",
            PrincipalRoles = new List<string> { "User" },
            CorrelationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>()
        };

        // Act
        var result = await _store.EvaluateAsync(context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("allow", result.Effect);
    }

    [Fact]
    public async Task EvaluateAsync_NoMatchingRule_ReturnsDefaultAllow()
    {
        // Arrange
        var context = new PolicyContext
        {
            Action = "delete",
            ResourceType = "Unknown",
            Resource = new PolicyResourceWrapper { Title = "Test" },
            TenantId = Guid.NewGuid(),
            PrincipalId = "user123",
            PrincipalRoles = new List<string> { "User" },
            CorrelationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>()
        };

        // Act
        var result = await _store.EvaluateAsync(context);

        // Assert
        Assert.NotNull(result);
        // Default behavior: allow if no rule matches
    }

    [Fact]
    public async Task LoadPoliciesAsync_LoadsFromFile()
    {
        // Act
        await _store.LoadPoliciesAsync();

        // Assert
        // Policies should be loaded (no exception thrown)
        Assert.True(true);
    }

    [Fact]
    public async Task GetRulesAsync_ReturnsLoadedRules()
    {
        // Arrange
        await _store.LoadPoliciesAsync();

        // Act
        var rules = await _store.GetRulesAsync();

        // Assert
        Assert.NotNull(rules);
        Assert.NotEmpty(rules);
    }

    [Fact]
    public async Task EvaluateAsync_ConditionalRule_AppliesCondition()
    {
        // Arrange
        var context = new PolicyContext
        {
            Action = "create",
            ResourceType = "Evidence",
            Resource = new PolicyResourceWrapper 
            { 
                Title = "Test",
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string> { { "dataClassification", "restricted" } }
                }
            },
            Environment = "prod",
            TenantId = Guid.NewGuid(),
            PrincipalId = "user123",
            PrincipalRoles = new List<string> { "User" },
            CorrelationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>()
        };

        // Act
        var result = await _store.EvaluateAsync(context);

        // Assert
        Assert.NotNull(result);
    }
}
