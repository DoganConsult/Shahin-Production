namespace GrcMvc.Constants;

/// <summary>
/// Centralized claim type constants for consistent claim handling.
/// </summary>
public static class ClaimConstants
{
    // ═══════════════════════════════════════════════════════════════════════
    // TENANT CLAIMS
    // ═══════════════════════════════════════════════════════════════════════
    public const string TenantId = "TenantId";
    public const string TenantSlug = "TenantSlug";
    public const string TenantName = "TenantName";
    
    // ═══════════════════════════════════════════════════════════════════════
    // WORKSPACE CLAIMS
    // ═══════════════════════════════════════════════════════════════════════
    public const string WorkspaceId = "WorkspaceId";
    public const string WorkspaceName = "WorkspaceName";
    
    // ═══════════════════════════════════════════════════════════════════════
    // USER CLAIMS
    // ═══════════════════════════════════════════════════════════════════════
    public const string UserId = "sub";
    public const string Email = "email";
    public const string FullName = "name";
    public const string FirstName = "given_name";
    public const string LastName = "family_name";
    
    // ═══════════════════════════════════════════════════════════════════════
    // PERMISSION CLAIMS
    // ═══════════════════════════════════════════════════════════════════════
    public const string Permission = "permission";
    public const string RoleCode = "role_code";
    
    // ═══════════════════════════════════════════════════════════════════════
    // ONBOARDING STATUS
    // ═══════════════════════════════════════════════════════════════════════
    public const string OnboardingCompleted = "onboarding_completed";
}

/// <summary>
/// Standardized onboarding status values.
/// </summary>
public static class OnboardingStatus
{
    public const string NotStarted = "NOT_STARTED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    
    /// <summary>
    /// Check if status indicates completion
    /// </summary>
    public static bool IsCompleted(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var normalized = status.ToUpperInvariant().Replace("_", "").Replace("-", "");
        return normalized == "COMPLETED";
    }
}

/// <summary>
/// Standardized tenant/user status values.
/// </summary>
public static class EntityStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Pending = "Pending";
    public const string Suspended = "Suspended";
    public const string Deleted = "Deleted";
}
