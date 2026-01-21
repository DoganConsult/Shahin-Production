using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Enhanced authentication service with session-based claims
/// </summary>
public interface IEnhancedAuthService
{
    /// <summary>
    /// Sign in user with tenant context in session (not DB)
    /// </summary>
    Task SignInWithTenantContextAsync(
        ApplicationUser user, 
        Guid tenantId, 
        string tenantRole,
        bool isPersistent = false);
    
    /// <summary>
    /// Get current tenant ID from session claims
    /// </summary>
    Task<Guid?> GetCurrentTenantIdAsync();
    
    /// <summary>
    /// Switch tenant context (session-only)
    /// </summary>
    Task SwitchTenantContextAsync(Guid newTenantId, string newTenantRole);
}
