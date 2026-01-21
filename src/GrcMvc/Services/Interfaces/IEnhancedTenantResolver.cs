namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Enhanced tenant resolver with deterministic tenant selection
/// </summary>
public interface IEnhancedTenantResolver
{
    /// <summary>
    /// Resolve current tenant ID using deterministic priority:
    /// 1. Route parameter (/t/{tenantSlug}/...)
    /// 2. Session claim
    /// 3. Most recently accessed tenant (deterministic ordering)
    /// </summary>
    Task<Guid?> ResolveCurrentTenantAsync(string userId);
    
    /// <summary>
    /// Get all tenants for a user (ordered by last access, deterministic)
    /// </summary>
    Task<List<TenantAccessInfo>> GetUserTenantsAsync(string userId);
    
    /// <summary>
    /// Record tenant access (for "most recently used" tracking)
    /// </summary>
    Task RecordTenantAccessAsync(string userId, Guid tenantId);
}

public class TenantAccessInfo
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public DateTime? LastAccessedAt { get; set; }
    public string Role { get; set; } = string.Empty;
}
