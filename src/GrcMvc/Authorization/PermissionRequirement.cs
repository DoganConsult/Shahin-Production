using Microsoft.AspNetCore.Authorization;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization requirement that checks if user has a specific permission claim.
/// Used by PermissionPolicyProvider to dynamically create policies for [Authorize("Grc.*")] attributes.
/// </summary>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
