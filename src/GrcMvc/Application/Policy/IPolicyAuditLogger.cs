using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IPolicyAuditLogger
{
    Task LogDecisionAsync(PolicyContext ctx, IEnumerable<PolicyDecision> decisions, PolicyDecision finalDecision, CancellationToken ct = default);
}
