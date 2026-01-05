namespace GrcMvc.Application.Policy;

/// <summary>
/// Exception thrown when a policy violation is detected
/// </summary>
public class PolicyViolationException : Exception
{
    public string RuleId { get; }
    public string RemediationHint { get; }

    public PolicyViolationException(string message, string ruleId, string remediationHint)
        : base(message)
    {
        RuleId = ruleId;
        RemediationHint = remediationHint;
    }

    public PolicyViolationException(string message, string ruleId, string remediationHint, Exception innerException)
        : base(message, innerException)
    {
        RuleId = ruleId;
        RemediationHint = remediationHint;
    }
}
