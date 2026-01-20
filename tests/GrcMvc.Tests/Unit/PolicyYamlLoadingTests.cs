using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for YAML policy loading - schema validation, edge cases, and error handling
/// Tests the PolicyStore's YAML deserialization and validation logic
/// </summary>
public class PolicyYamlLoadingTests : IDisposable
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<PolicyStore>> _mockLogger;
    private readonly PolicyStore _policyStore;
    private readonly List<string> _tempFiles = new();
    private readonly IDeserializer _yamlDeserializer;

    public PolicyYamlLoadingTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PolicyStore>>();
        _policyStore = new PolicyStore(_mockConfig.Object, _mockLogger.Object);

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    #region Schema Validation Tests

    [Fact]
    public async Task LoadPolicy_WithMinimalValidYaml_Succeeds()
    {
        // Arrange - Minimal valid YAML
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
kind: Policy
metadata:
  name: minimal-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: allow
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("minimal-policy", policy.Metadata.Name);
        Assert.Single(policy.Spec.Rules);
    }

    [Fact]
    public async Task LoadPolicy_WithAllOptionalFields_Succeeds()
    {
        // Arrange - Full YAML with all optional fields
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
kind: Policy
metadata:
  name: full-policy
  namespace: production
  version: '2.0.0'
  createdAt: '2026-01-01T00:00:00Z'
  labels:
    environment: prod
    team: security
  annotations:
    description: Full policy with all fields
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
      - Control
    environments:
      - dev
      - staging
      - prod
  rules:
    - id: FULL_RULE
      priority: 10
      description: A fully specified rule
      enabled: true
      match:
        resource:
          type: Evidence
          name: '*'
        principal:
          roles:
            - Admin
            - Auditor
        environment: prod
      when:
        - op: exists
          path: metadata.labels.owner
        - op: matches
          path: metadata.labels.dataClassification
          value: '^(public|internal|confidential|restricted)$'
      effect: allow
      message: Resource meets all requirements
      severity: medium
      remediation:
        hint: Ensure all fields are properly set
        docs: https://docs.example.com/policy
      metadata:
        customField: customValue
  exceptions:
    - id: DEV_EXCEPTION
      ruleIds:
        - FULL_RULE
      reason: Dev environment bypass
      expiresAt: '2030-12-31T23:59:59Z'
      match:
        environment: dev
  audit:
    logDecisions: true
    retentionDays: 365
    sinks:
      - type: stdout
      - type: file
        path: /var/log/policy.log
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("full-policy", policy.Metadata.Name);
        Assert.Equal("production", policy.Metadata.Namespace);
        Assert.Equal("2.0.0", policy.Metadata.Version);
        Assert.Equal("prod", policy.Metadata.Labels["environment"]);
        Assert.Equal("enforce", policy.Spec.Mode);
        Assert.Equal("deny", policy.Spec.DefaultEffect);
        Assert.True(policy.Spec.Execution.ShortCircuit);
        Assert.Contains("Evidence", policy.Spec.Target.ResourceTypes);
        Assert.Single(policy.Spec.Exceptions);
    }

    [Fact]
    public async Task LoadPolicy_WithMutationRules_ParsesMutations()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: mutation-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: NORMALIZE_EMPTY
      priority: 9000
      effect: mutate
      message: Normalizing empty values
      mutations:
        - op: set
          path: metadata.labels.owner
          value: null
        - op: remove
          path: metadata.labels.temporary
        - op: set
          path: metadata.labels.processed
          value: 'true'
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        var rule = policy.Spec.Rules[0];
        Assert.Equal("mutate", rule.Effect);
        Assert.NotNull(rule.Mutations);
        Assert.Equal(3, rule.Mutations.Count);
        Assert.Equal("set", rule.Mutations[0].Op);
        Assert.Equal("remove", rule.Mutations[1].Op);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task LoadPolicy_WithEmptyLabels_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: empty-labels-policy
  version: '1.0.0'
  labels: {}
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: allow
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Empty(policy.Metadata.Labels ?? new Dictionary<string, string>());
    }

    [Fact]
    public async Task LoadPolicy_WithSpecialCharactersInStrings_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: special-chars-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE_WITH_SPECIAL_CHARS
      priority: 10
      effect: deny
      message: 'Message with special chars: <>&""''
      when:
        - op: matches
          path: metadata.labels.pattern
          value: '^[a-z]+\.[a-z]+@[a-z]+\.(com|org)$'
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains("<>&", policy.Spec.Rules[0].Message);
    }

    [Fact]
    public async Task LoadPolicy_WithUnicodeCharacters_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: unicode-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: UNICODE_RULE
      priority: 10
      effect: deny
      message: 'سياسة الامتثال - Compliance Policy - 合规政策'
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains("سياسة", policy.Spec.Rules[0].Message);
        Assert.Contains("合规", policy.Spec.Rules[0].Message);
    }

    [Fact]
    public async Task LoadPolicy_WithMultilineStrings_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: multiline-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: MULTILINE_RULE
      priority: 10
      effect: deny
      description: |
        This is a multiline description
        that spans multiple lines
        and should be preserved
      message: Single line message
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Contains("multiline description", policy.Spec.Rules[0].Description);
        Assert.Contains("\n", policy.Spec.Rules[0].Description);
    }

    [Fact]
    public async Task LoadPolicy_WithConditionArrayValues_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: array-condition-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: IN_CONDITION_RULE
      priority: 10
      effect: allow
      when:
        - op: in
          path: metadata.labels.dataClassification
          value:
            - public
            - internal
            - confidential
        - op: notIn
          path: metadata.labels.status
          value:
            - deleted
            - archived
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        var rule = policy.Spec.Rules[0];
        Assert.Equal(2, rule.When.Count);
        Assert.Equal("in", rule.When[0].Op);
        Assert.Equal("notIn", rule.When[1].Op);
    }

    [Fact]
    public async Task LoadPolicy_WithBooleanValues_ParsesCorrectly()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: boolean-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  execution:
    shortCircuit: true
  rules:
    - id: ENABLED_RULE
      priority: 10
      enabled: true
      effect: allow
    - id: DISABLED_RULE
      priority: 20
      enabled: false
      effect: deny
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.True(policy.Spec.Execution.ShortCircuit);
        Assert.True(policy.Spec.Rules[0].Enabled);
        Assert.False(policy.Spec.Rules[1].Enabled);
    }

    [Fact]
    public async Task LoadPolicy_WithDateTimeValues_ParsesCorrectly()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: datetime-policy
  version: '1.0.0'
  createdAt: '2026-01-15T10:30:00Z'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: allow
  exceptions:
    - id: TEMP_EXCEPTION
      ruleIds:
        - RULE1
      reason: Temporary
      expiresAt: '2026-06-30T23:59:59+03:00'
      match:
        environment: dev
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(2026, policy.Metadata.CreatedAt.Year);
        Assert.NotNull(policy.Spec.Exceptions[0].ExpiresAt);
    }

    [Fact]
    public async Task LoadPolicy_WithNullExpiresAt_Succeeds()
    {
        // Arrange - Exception without expiry (permanent)
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: permanent-exception-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: 10
      effect: deny
  exceptions:
    - id: PERMANENT_EXCEPTION
      ruleIds:
        - RULE1
      reason: Permanent bypass for dev
      match:
        environment: dev
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Null(policy.Spec.Exceptions[0].ExpiresAt);
    }

    #endregion

    #region Priority Edge Cases

    [Fact]
    public async Task LoadPolicy_WithBoundaryPriorities_Succeeds()
    {
        // Arrange - Test priority boundaries
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: priority-boundary-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: MIN_PRIORITY
      priority: 1
      effect: allow
    - id: MAX_PRIORITY
      priority: 10000
      effect: deny
    - id: MID_PRIORITY
      priority: 5000
      effect: audit
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();
        var isValid = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.NotNull(policy);
        Assert.True(isValid);
        Assert.Equal(1, policy.Spec.Rules[0].Priority);
        Assert.Equal(10000, policy.Spec.Rules[1].Priority);
    }

    [Fact]
    public async Task LoadPolicy_WithDuplicatePriorities_SucceedsButMayNeedDeterministicOrdering()
    {
        // Arrange - Same priority rules (order depends on Id for determinism)
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: duplicate-priority-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE_A
      priority: 100
      effect: allow
    - id: RULE_B
      priority: 100
      effect: deny
    - id: RULE_C
      priority: 100
      effect: audit
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(3, policy.Spec.Rules.Count);
        Assert.All(policy.Spec.Rules, r => Assert.Equal(100, r.Priority));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task LoadPolicy_WithMalformedYaml_ReturnsDefaultPolicy()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: malformed
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: RULE1
      priority: invalid_number
      effect: allow
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.GetPolicyAsync();

        // Assert - Should fall back to default policy
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task LoadPolicy_WithEmptyFile_ReturnsDefaultPolicy()
    {
        // Arrange
        var path = CreateTempFile("");
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.GetPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("default-policy", policy.Metadata.Name);
    }

    [Fact]
    public async Task LoadPolicy_WithWhitespaceOnlyFile_ReturnsDefaultPolicy()
    {
        // Arrange
        var path = CreateTempFile("   \n\t\n   ");
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.GetPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("default-policy", policy.Metadata.Name);
    }

    #endregion

    #region All Effect Types Tests

    [Fact]
    public async Task LoadPolicy_WithAllEffectTypes_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: all-effects-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: ALLOW_RULE
      priority: 10
      effect: allow
      message: Allowed
    - id: DENY_RULE
      priority: 20
      effect: deny
      message: Denied
    - id: AUDIT_RULE
      priority: 30
      effect: audit
      message: Audited only
    - id: MUTATE_RULE
      priority: 40
      effect: mutate
      message: Mutated
      mutations:
        - op: set
          path: metadata.processed
          value: 'true'
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(4, policy.Spec.Rules.Count);
        Assert.Equal("allow", policy.Spec.Rules[0].Effect);
        Assert.Equal("deny", policy.Spec.Rules[1].Effect);
        Assert.Equal("audit", policy.Spec.Rules[2].Effect);
        Assert.Equal("mutate", policy.Spec.Rules[3].Effect);
    }

    #endregion

    #region All Condition Operators Tests

    [Fact]
    public async Task LoadPolicy_WithAllConditionOperators_Succeeds()
    {
        // Arrange
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
metadata:
  name: all-operators-policy
  version: '1.0.0'
spec:
  defaultEffect: allow
  rules:
    - id: EXISTS_RULE
      priority: 10
      when:
        - op: exists
          path: metadata.labels.owner
      effect: allow
    - id: EQUALS_RULE
      priority: 20
      when:
        - op: equals
          path: metadata.labels.status
          value: active
      effect: allow
    - id: NOT_EQUALS_RULE
      priority: 30
      when:
        - op: notEquals
          path: metadata.labels.status
          value: deleted
      effect: allow
    - id: IN_RULE
      priority: 40
      when:
        - op: in
          path: metadata.labels.env
          value: [dev, staging, prod]
      effect: allow
    - id: NOT_IN_RULE
      priority: 50
      when:
        - op: notIn
          path: metadata.labels.env
          value: [test, sandbox]
      effect: allow
    - id: MATCHES_RULE
      priority: 60
      when:
        - op: matches
          path: metadata.labels.email
          value: '^[a-z]+@company\.com$'
      effect: allow
    - id: NOT_MATCHES_RULE
      priority: 70
      when:
        - op: notMatches
          path: metadata.labels.name
          value: '^test-.*'
      effect: allow
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(7, policy.Spec.Rules.Count);

        var operators = new[] { "exists", "equals", "notEquals", "in", "notIn", "matches", "notMatches" };
        for (int i = 0; i < operators.Length; i++)
        {
            Assert.Equal(operators[i], policy.Spec.Rules[i].When[0].Op);
        }
    }

    #endregion

    #region Real-World Policy Patterns Tests

    [Fact]
    public async Task LoadPolicy_DataClassificationPattern_Succeeds()
    {
        // Arrange - Real-world data classification policy pattern
        var yaml = @"
apiVersion: policy.doganconsult.io/v1
kind: Policy
metadata:
  name: data-governance-policy
  version: '1.0.0'
  labels:
    domain: governance
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
      - Control
    environments:
      - dev
      - staging
      - prod
  rules:
    - id: REQUIRE_DATA_CLASSIFICATION
      priority: 10
      description: Every resource must have a valid data classification label
      enabled: true
      match:
        resource:
          type: Any
      when:
        - op: notMatches
          path: metadata.labels.dataClassification
          value: '^(public|internal|confidential|restricted)$'
      effect: deny
      severity: high
      message: Missing or invalid data classification label
      remediation:
        hint: Set metadata.labels.dataClassification to one of: public, internal, confidential, restricted
    - id: RESTRICTED_REQUIRES_APPROVAL
      priority: 20
      description: Restricted data requires executive approval
      enabled: true
      match:
        resource:
          type: Any
        environment: prod
      when:
        - op: equals
          path: metadata.labels.dataClassification
          value: restricted
        - op: notEquals
          path: metadata.labels.executiveApproval
          value: 'true'
      effect: deny
      severity: critical
      message: Restricted data in production requires executive approval
      remediation:
        hint: Obtain executive approval and set metadata.labels.executiveApproval to true
  exceptions:
    - id: DEV_CLASSIFICATION_BYPASS
      ruleIds:
        - RESTRICTED_REQUIRES_APPROVAL
      reason: Dev environment does not require executive approval for restricted data
      expiresAt: '2030-12-31T23:59:59Z'
      match:
        environment: dev
";
        var path = CreateTempFile(yaml);
        _mockConfig.Setup(c => c["Policy:FilePath"]).Returns(path);

        // Act
        var policy = await _policyStore.ReloadPolicyAsync();
        var isValid = await _policyStore.ValidatePolicyAsync(policy);

        // Assert
        Assert.NotNull(policy);
        Assert.True(isValid);
        Assert.Equal("data-governance-policy", policy.Metadata.Name);
        Assert.Equal(2, policy.Spec.Rules.Count);
        Assert.Single(policy.Spec.Exceptions);
        Assert.Equal("denyOverrides", policy.Spec.Execution.ConflictStrategy);
    }

    #endregion

    #region Helper Methods

    private string CreateTempFile(string content)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"policy-test-{Guid.NewGuid()}.yml");
        File.WriteAllText(tempFile, content);
        _tempFiles.Add(tempFile);
        return tempFile;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    #endregion
}
