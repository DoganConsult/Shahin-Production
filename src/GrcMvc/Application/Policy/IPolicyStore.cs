using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IPolicyStore
{
    Task<PolicyDocument> GetPolicyAsync(CancellationToken ct = default);
    Task<PolicyDocument> ReloadPolicyAsync(CancellationToken ct = default);
    Task<bool> ValidatePolicyAsync(PolicyDocument policy, CancellationToken ct = default);
    event EventHandler<PolicyReloadedEventArgs>? PolicyReloaded;
}

public class PolicyReloadedEventArgs : EventArgs
{
    public PolicyDocument Policy { get; set; } = null!;
}
