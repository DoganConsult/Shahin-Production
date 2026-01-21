using Xunit;
using Microsoft.Extensions.Logging;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Moq;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for PolicyEnforcer
/// Tests policy evaluation and enforcement logic
/// </summary>
public class PolicyEnforcerTests
{
    private readonly PolicyEnforcer _enforcer;
    private readonly Mock<IPolicyStore> _policyStoreMock;
    private readonly Mock<IDotPathResolver> _pathResolverMock;
    private readonly Mock<IMutationApplier> _mutationApplierMock;
    private readonly Mock<IPolicyAuditLogger> _auditLoggerMock;
    private readonly Mock<ILogger<PolicyEnforcer>> _loggerMock;

    public PolicyEnforcerTests()
    {
        _policyStoreMock = new Mock<IPolicyStore>();
        _pathResolverMock = new Mock<IDotPathResolver>();
        _mutationApplierMock = new Mock<IMutationApplier>();
        _auditLoggerMock = new Mock<IPolicyAuditLogger>();
        _loggerMock = new Mock<ILogger<PolicyEnforcer>>();

        _enforcer = new PolicyEnforcer(
            _policyStoreMock.Object,
            _pathResolverMock.Object,
            _mutationApplierMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EnforceAsync_AllowDecision_NoException()
    {
        // Arrange
        var context = CreateTestContext();
        var decision = new PolicyDecision { Effect = "allow" };

        _policyStoreMock.Setup(x => x.EvaluateAsync(It.IsAny<PolicyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        // Act & Assert
        await _enforcer.EnforceAsync(context);
        // Should not throw
    }

    [Fact]
    public async Task EnforceAsync_DenyDecision_ThrowsPolicyViolationException()
    {
        // Arrange
        var context = CreateTestContext();
        var decision = new PolicyDecision 
        { 
            Effect = "deny",
            Message = "Policy violation",
            MatchedRuleId = "RULE_001",
            RemediationHint = "Fix the issue"
        };

        _policyStoreMock.Setup(x => x.EvaluateAsync(It.IsAny<PolicyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(
            () => _enforcer.EnforceAsync(context));

        Assert.Equal("Policy violation", exception.Message);
        Assert.Equal("RULE_001", exception.RuleId);
        Assert.Equal("Fix the issue", exception.RemediationHint);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsPolicyDecision()
    {
        // Arrange
        var context = CreateTestContext();
        var expectedDecision = new PolicyDecision { Effect = "allow" };

        _policyStoreMock.Setup(x => x.EvaluateAsync(It.IsAny<PolicyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDecision);

        // Act
        var result = await _enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", result.Effect);
    }

    [Fact]
    public async Task EvaluateAsync_LogsAuditEvent()
    {
        // Arrange
        var context = CreateTestContext();
        var decision = new PolicyDecision { Effect = "allow" };

        _policyStoreMock.Setup(x => x.EvaluateAsync(It.IsAny<PolicyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        // Act
        await _enforcer.EvaluateAsync(context);

        // Assert
        _auditLoggerMock.Verify(x => x.LogDecisionAsync(
            It.IsAny<PolicyContext>(),
            It.IsAny<PolicyDecision>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_AppliesMutations_WhenPresent()
    {
        // Arrange
        var context = CreateTestContext();
        var decision = new PolicyDecision 
        { 
            Effect = "allow",
            Mutations = new[]
            {
                new PolicyMutation { Op = "set", Path = "metadata.labels.owner", Value = "admin" }
            }
        };

        _policyStoreMock.Setup(x => x.EvaluateAsync(It.IsAny<PolicyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);

        // Act
        await _enforcer.EvaluateAsync(context);

        // Assert
        _mutationApplierMock.Verify(x => x.ApplyAsync(
            It.IsAny<IEnumerable<PolicyMutation>>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private PolicyContext CreateTestContext()
    {
        return new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Title = "Test Resource",
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string>()
                }
            },
            TenantId = Guid.NewGuid(),
            PrincipalId = "user123",
            PrincipalRoles = new List<string> { "User" },
            CorrelationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>()
        };
    }
}
