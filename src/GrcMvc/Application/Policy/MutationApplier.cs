using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Applies policy mutations to resources (normalization, transformations)
/// </summary>
public class MutationApplier : IMutationApplier
{
    private readonly IDotPathResolver _pathResolver;
    private readonly ILogger<MutationApplier> _logger;

    public MutationApplier(IDotPathResolver pathResolver, ILogger<MutationApplier> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
    }

    public async Task ApplyAsync(IEnumerable<PolicyMutation> mutations, object resource, CancellationToken ct = default)
    {
        foreach (var mutation in mutations)
        {
            try
            {
                switch (mutation.Op.ToLower())
                {
                    case "set":
                        // Try to set on the wrapper first (mutable)
                        if (resource is PolicyResourceWrapper wrapper)
                        {
                            // Handle metadata.labels.* paths
                            if (mutation.Path.StartsWith("metadata.labels."))
                            {
                                var labelKey = mutation.Path.Substring("metadata.labels.".Length);
                                wrapper.Metadata.Labels[labelKey] = mutation.Value?.ToString() ?? string.Empty;
                                _logger.LogDebug("Applied mutation: set {Path} = {Value} on wrapper", mutation.Path, mutation.Value);
                            }
                            else
                            {
                                _pathResolver.Set(resource, mutation.Path, mutation.Value);
                                _logger.LogDebug("Applied mutation: set {Path} = {Value}", mutation.Path, mutation.Value);
                            }
                        }
                        else
                        {
                            _pathResolver.Set(resource, mutation.Path, mutation.Value);
                            _logger.LogDebug("Applied mutation: set {Path} = {Value}", mutation.Path, mutation.Value);
                        }
                        break;
                    case "remove":
                        if (resource is PolicyResourceWrapper wrapper2)
                        {
                            if (mutation.Path.StartsWith("metadata.labels."))
                            {
                                var labelKey = mutation.Path.Substring("metadata.labels.".Length);
                                wrapper2.Metadata.Labels.Remove(labelKey);
                                _logger.LogDebug("Applied mutation: remove {Path} from wrapper", mutation.Path);
                            }
                            else
                            {
                                _pathResolver.Remove(resource, mutation.Path);
                                _logger.LogDebug("Applied mutation: remove {Path}", mutation.Path);
                            }
                        }
                        else
                        {
                            _pathResolver.Remove(resource, mutation.Path);
                            _logger.LogDebug("Applied mutation: remove {Path}", mutation.Path);
                        }
                        break;
                    case "add":
                        // For arrays/lists - implementation depends on resource type
                        _logger.LogWarning("Add mutation not fully implemented for {Path}", mutation.Path);
                        break;
                    default:
                        _logger.LogWarning("Unknown mutation operation: {Op}", mutation.Op);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying mutation {Op} to {Path}", mutation.Op, mutation.Path);
                throw;
            }
        }

        await Task.CompletedTask;
    }
}
