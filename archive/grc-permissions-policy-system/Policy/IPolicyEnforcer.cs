using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IPolicyEnforcer
{
    Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<PolicyDecision> EvaluateAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<bool> IsAllowedAsync(PolicyContext ctx, CancellationToken ct = default);
}
