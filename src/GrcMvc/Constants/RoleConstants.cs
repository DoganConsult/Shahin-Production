namespace GrcMvc.Constants;

/// <summary>
/// Centralized role constants following ASP.NET Identity naming conventions.
/// Use PascalCase for Identity roles - SINGLE SOURCE OF TRUTH.
/// </summary>
public static class RoleConstants
{
    // ═══════════════════════════════════════════════════════════════════════
    // PLATFORM-LEVEL ROLES (System-wide, not tenant-specific)
    // ═══════════════════════════════════════════════════════════════════════
    public const string PlatformAdmin = "PlatformAdmin";
    public const string SystemAdministrator = "SystemAdministrator";
    
    // ═══════════════════════════════════════════════════════════════════════
    // TENANT-LEVEL ROLES (Per-tenant)
    // ═══════════════════════════════════════════════════════════════════════
    public const string TenantAdmin = "TenantAdmin";
    public const string TenantOwner = "TenantOwner";
    
    // ═══════════════════════════════════════════════════════════════════════
    // EXECUTIVE LAYER
    // ═══════════════════════════════════════════════════════════════════════
    public const string ChiefRiskOfficer = "ChiefRiskOfficer";
    public const string ChiefComplianceOfficer = "ChiefComplianceOfficer";
    public const string ExecutiveDirector = "ExecutiveDirector";
    
    // ═══════════════════════════════════════════════════════════════════════
    // MANAGEMENT LAYER
    // ═══════════════════════════════════════════════════════════════════════
    public const string RiskManager = "RiskManager";
    public const string ComplianceManager = "ComplianceManager";
    public const string AuditManager = "AuditManager";
    public const string SecurityManager = "SecurityManager";
    public const string LegalManager = "LegalManager";
    
    // ═══════════════════════════════════════════════════════════════════════
    // OPERATIONAL LAYER
    // ═══════════════════════════════════════════════════════════════════════
    public const string ComplianceOfficer = "ComplianceOfficer";
    public const string RiskAnalyst = "RiskAnalyst";
    public const string PrivacyOfficer = "PrivacyOfficer";
    public const string QualityAssuranceManager = "QualityAssuranceManager";
    public const string ProcessOwner = "ProcessOwner";
    
    // ═══════════════════════════════════════════════════════════════════════
    // SUPPORT LAYER
    // ═══════════════════════════════════════════════════════════════════════
    public const string OperationsSupport = "OperationsSupport";
    public const string SystemObserver = "SystemObserver";
    public const string Employee = "Employee";
    public const string Guest = "Guest";
    
    /// <summary>
    /// Check if a role code matches TenantAdmin (handles legacy variations)
    /// </summary>
    public static bool IsTenantAdmin(string? roleCode)
    {
        if (string.IsNullOrEmpty(roleCode)) return false;
        var normalized = roleCode.ToUpperInvariant().Replace("_", "").Replace("-", "");
        return normalized == "TENANTADMIN" || 
               normalized == "ADMIN" || 
               normalized == "ADMINISTRATOR";
    }
    
    /// <summary>
    /// Check if a role code matches PlatformAdmin
    /// </summary>
    public static bool IsPlatformAdmin(string? roleCode)
    {
        if (string.IsNullOrEmpty(roleCode)) return false;
        var normalized = roleCode.ToUpperInvariant().Replace("_", "").Replace("-", "");
        return normalized == "PLATFORMADMIN" || 
               normalized == "SYSTEMADMINISTRATOR" ||
               normalized == "SYSADMIN";
    }
    
    /// <summary>
    /// Normalize role code to standard PascalCase format
    /// </summary>
    public static string NormalizeRoleCode(string? roleCode)
    {
        if (string.IsNullOrEmpty(roleCode)) return Employee;
        
        var normalized = roleCode.ToUpperInvariant().Replace("_", "").Replace("-", "");
        return normalized switch
        {
            "TENANTADMIN" or "ADMIN" or "ADMINISTRATOR" => TenantAdmin,
            "TENANTOWNER" or "OWNER" => TenantOwner,
            "PLATFORMADMIN" or "SYSADMIN" or "SYSTEMADMINISTRATOR" => PlatformAdmin,
            "COMPLIANCEOFFICER" => ComplianceOfficer,
            "RISKMANAGER" => RiskManager,
            "COMPLIANCEMANAGER" => ComplianceManager,
            "AUDITMANAGER" => AuditManager,
            "SECURITYMANAGER" => SecurityManager,
            "RISKANALYST" => RiskAnalyst,
            "PRIVACYOFFICER" => PrivacyOfficer,
            "PROCESSOWNER" => ProcessOwner,
            "EMPLOYEE" or "USER" => Employee,
            "GUEST" or "VIEWER" => Guest,
            _ => roleCode // Return original if no match
        };
    }
    
    /// <summary>
    /// Get all roles that have admin-level access
    /// </summary>
    public static string[] AdminRoles => new[] 
    { 
        PlatformAdmin, 
        SystemAdministrator, 
        TenantAdmin, 
        TenantOwner 
    };
    
    /// <summary>
    /// Get all roles that can manage compliance
    /// </summary>
    public static string[] ComplianceRoles => new[] 
    { 
        ChiefComplianceOfficer, 
        ComplianceManager, 
        ComplianceOfficer 
    };
    
    /// <summary>
    /// Get all roles that can manage risk
    /// </summary>
    public static string[] RiskRoles => new[] 
    { 
        ChiefRiskOfficer, 
        RiskManager, 
        RiskAnalyst 
    };
}
