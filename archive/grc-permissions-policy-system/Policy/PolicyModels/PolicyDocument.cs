namespace GrcMvc.Application.Policy.PolicyModels;

public class PolicyDocument
{
    public string ApiVersion { get; set; } = "policy.doganconsult.io/v1";
    public string Kind { get; set; } = "Policy";
    public PolicyMetadata Metadata { get; set; } = new();
    public PolicySpec Spec { get; set; } = new();
}

public class PolicyMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = "default";
    public string Version { get; set; } = "1.0.0";
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
}

public class PolicySpec
{
    public string Mode { get; set; } = "enforce"; // enforce|audit
    public string DefaultEffect { get; set; } = "allow";
    public PolicyExecution Execution { get; set; } = new();
    public PolicyTarget Target { get; set; } = new();
    public List<PolicyRule> Rules { get; set; } = new();
    public List<PolicyException> Exceptions { get; set; } = new();
    public PolicyAuditConfig Audit { get; set; } = new();
}

public class PolicyExecution
{
    public string Order { get; set; } = "sequential";
    public bool ShortCircuit { get; set; } = true;
    public string ConflictStrategy { get; set; } = "denyOverrides"; // denyOverrides|allowOverrides|highestPriorityWins
}

public class PolicyTarget
{
    public List<string> ResourceTypes { get; set; } = new();
    public List<string> Environments { get; set; } = new();
}

public class PolicyRule
{
    public string Id { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public PolicyMatch Match { get; set; } = new();
    public List<PolicyCondition> When { get; set; } = new();
    public string Effect { get; set; } = "allow"; // allow|deny|audit|mutate
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium"; // low|medium|high|critical
    public List<PolicyMutation> Mutations { get; set; } = new();
    public PolicyRemediation Remediation { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class PolicyMatch
{
    public PolicyResourceMatch Resource { get; set; } = new();
    public PolicyPrincipalMatch? Principal { get; set; }
    public string Environment { get; set; } = "*";
}

public class PolicyResourceMatch
{
    public string Type { get; set; } = "*";
    public string Name { get; set; } = "*";
    public Dictionary<string, string> Labels { get; set; } = new();
}

public class PolicyPrincipalMatch
{
    public string? Id { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class PolicyCondition
{
    public string Op { get; set; } = string.Empty; // exists|equals|notEquals|in|notIn|matches|notMatches
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public class PolicyMutation
{
    public string Op { get; set; } = string.Empty; // set|remove|add
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public class PolicyRemediation
{
    public string? Url { get; set; }
    public string? Hint { get; set; }
}

public class PolicyException
{
    public string Id { get; set; } = string.Empty;
    public List<string> RuleIds { get; set; } = new();
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PolicyMatch Match { get; set; } = new();
}

public class PolicyAuditConfig
{
    public bool LogDecisions { get; set; } = true;
    public int RetentionDays { get; set; } = 365;
    public List<PolicyAuditSink> Sinks { get; set; } = new();
}

public class PolicyAuditSink
{
    public string Type { get; set; } = string.Empty; // stdout|file|http
    public string? Path { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class PolicyDecision
{
    public string Effect { get; set; } = string.Empty; // allow|deny|audit|mutate
    public string? MatchedRuleId { get; set; }
    public string? Message { get; set; }
    public string? RemediationHint { get; set; }
    public string Severity { get; set; } = "medium";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
