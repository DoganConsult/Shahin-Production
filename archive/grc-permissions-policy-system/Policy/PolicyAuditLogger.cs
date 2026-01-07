using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Logs policy decisions for audit and compliance purposes
/// </summary>
public class PolicyAuditLogger : IPolicyAuditLogger
{
    private readonly ILogger<PolicyAuditLogger> _logger;

    public PolicyAuditLogger(ILogger<PolicyAuditLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogDecisionAsync(
        PolicyContext ctx, 
        IEnumerable<PolicyDecision> decisions, 
        PolicyDecision finalDecision, 
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Policy Decision: {Effect} for {ResourceType} {Action} by {PrincipalId}. " +
            "Matched Rules: {RuleCount}, Final: {FinalRuleId}",
            finalDecision.Effect,
            ctx.ResourceType,
            ctx.Action,
            ctx.PrincipalId,
            decisions.Count(),
            finalDecision.MatchedRuleId);

        if (finalDecision.Effect == "deny")
        {
            _logger.LogWarning(
                "Policy Violation: {Message}. Rule: {RuleId}. Remediation: {Remediation}",
                finalDecision.Message,
                finalDecision.MatchedRuleId,
                finalDecision.RemediationHint);
        }

        await Task.CompletedTask;
    }
}
