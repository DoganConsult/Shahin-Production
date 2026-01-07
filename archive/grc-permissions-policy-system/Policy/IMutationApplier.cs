using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IMutationApplier
{
    Task ApplyAsync(IEnumerable<PolicyMutation> mutations, object resource, CancellationToken ct = default);
}
