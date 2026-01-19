using System;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Constants;

/// <summary>
/// Centralized role constants following ASP.NET Identity naming conventions.
/// CANONICAL FORMAT: PascalCase (e.g., "TenantAdmin", not "TENANT_ADMIN")
///
/// This is the SINGLE SOURCE OF TRUTH for all role definitions.
/// All role codes must be normalized through NormalizeRoleCode() before use.
/// </summary>
public static class RoleConstants
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PLATFORM-LEVEL ROLES (System-wide, not tenant-specific)
    // Can only be assigned by Platform Admins
    // ═══════════════════════════════════════════════════════════════════════════
    public const string PlatformAdmin = "PlatformAdmin";
    public const string SystemAdministrator = "SystemAdministrator";

    // ═══════════════════════════════════════════════════════════════════════════
    // TENANT-LEVEL ROLES - ADMINISTRATIVE (Per-tenant)
    // TenantOwner can only be assigned by PlatformAdmin or during tenant creation
    // TenantAdmin can be assigned by TenantOwner or PlatformAdmin
    // ═══════════════════════════════════════════════════════════════════════════
    public const string TenantOwner = "TenantOwner";
    public const string TenantAdmin = "TenantAdmin";

    // ═══════════════════════════════════════════════════════════════════════════
    // EXECUTIVE LAYER (Assignable by TenantAdmin/TenantOwner)
    // ═══════════════════════════════════════════════════════════════════════════
    public const string ChiefRiskOfficer = "ChiefRiskOfficer";
    public const string ChiefComplianceOfficer = "ChiefComplianceOfficer";
    public const string ExecutiveDirector = "ExecutiveDirector";

    // ═══════════════════════════════════════════════════════════════════════════
    // MANAGEMENT LAYER (Assignable by TenantAdmin/TenantOwner/Executives)
    // ═══════════════════════════════════════════════════════════════════════════
    public const string RiskManager = "RiskManager";
    public const string ComplianceManager = "ComplianceManager";
    public const string AuditManager = "AuditManager";
    public const string SecurityManager = "SecurityManager";
    public const string LegalManager = "LegalManager";

    // ═══════════════════════════════════════════════════════════════════════════
    // OPERATIONAL LAYER (Assignable by TenantAdmin/Managers)
    // ═══════════════════════════════════════════════════════════════════════════
    public const string ComplianceOfficer = "ComplianceOfficer";
    public const string RiskAnalyst = "RiskAnalyst";
    public const string PrivacyOfficer = "PrivacyOfficer";
    public const string QualityAssuranceManager = "QualityAssuranceManager";
    public const string ProcessOwner = "ProcessOwner";

    // ═══════════════════════════════════════════════════════════════════════════
    // SUPPORT LAYER (Assignable by TenantAdmin/Managers)
    // ═══════════════════════════════════════════════════════════════════════════
    public const string OperationsSupport = "OperationsSupport";
    public const string SystemObserver = "SystemObserver";
    public const string Employee = "Employee";
    public const string Guest = "Guest";

    // ═══════════════════════════════════════════════════════════════════════════
    // ROLE METADATA AND CATEGORIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// All 21 defined roles (2 platform + 19 tenant)
    /// </summary>
    public static readonly string[] AllRoles = new[]
    {
        // Platform
        PlatformAdmin, SystemAdministrator,
        // Tenant Admin
        TenantOwner, TenantAdmin,
        // Executive
        ChiefRiskOfficer, ChiefComplianceOfficer, ExecutiveDirector,
        // Management
        RiskManager, ComplianceManager, AuditManager, SecurityManager, LegalManager,
        // Operational
        ComplianceOfficer, RiskAnalyst, PrivacyOfficer, QualityAssuranceManager, ProcessOwner,
        // Support
        OperationsSupport, SystemObserver, Employee, Guest
    };

    /// <summary>
    /// Platform-level roles (require platform admin to assign)
    /// </summary>
    public static readonly string[] PlatformRoles = new[]
    {
        PlatformAdmin,
        SystemAdministrator
    };

    /// <summary>
    /// Tenant-level roles (can be assigned within tenant)
    /// </summary>
    public static readonly string[] TenantRoles = new[]
    {
        TenantOwner, TenantAdmin,
        ChiefRiskOfficer, ChiefComplianceOfficer, ExecutiveDirector,
        RiskManager, ComplianceManager, AuditManager, SecurityManager, LegalManager,
        ComplianceOfficer, RiskAnalyst, PrivacyOfficer, QualityAssuranceManager, ProcessOwner,
        OperationsSupport, SystemObserver, Employee, Guest
    };

    /// <summary>
    /// Roles with administrative privileges (can manage users)
    /// </summary>
    public static readonly string[] AdminRoles = new[]
    {
        PlatformAdmin,
        SystemAdministrator,
        TenantOwner,
        TenantAdmin
    };

    /// <summary>
    /// Privileged roles requiring MFA (AM-04)
    /// </summary>
    public static readonly string[] PrivilegedRoles = new[]
    {
        PlatformAdmin,
        SystemAdministrator,
        TenantOwner,
        TenantAdmin,
        ChiefRiskOfficer,
        ChiefComplianceOfficer,
        ExecutiveDirector
    };

    /// <summary>
    /// Executive-level roles
    /// </summary>
    public static readonly string[] ExecutiveRoles = new[]
    {
        ChiefRiskOfficer,
        ChiefComplianceOfficer,
        ExecutiveDirector
    };

    /// <summary>
    /// Manager-level roles
    /// </summary>
    public static readonly string[] ManagerRoles = new[]
    {
        RiskManager,
        ComplianceManager,
        AuditManager,
        SecurityManager,
        LegalManager,
        QualityAssuranceManager
    };

    /// <summary>
    /// Compliance-related roles
    /// </summary>
    public static readonly string[] ComplianceRoles = new[]
    {
        ChiefComplianceOfficer,
        ComplianceManager,
        ComplianceOfficer
    };

    /// <summary>
    /// Risk-related roles
    /// </summary>
    public static readonly string[] RiskRoles = new[]
    {
        ChiefRiskOfficer,
        RiskManager,
        RiskAnalyst
    };

    /// <summary>
    /// Default role for new users (self-registration, invites without explicit role)
    /// </summary>
    public const string DefaultRole = Employee;

    /// <summary>
    /// Default role for trial tenant creators (Flow 1 & Flow 2)
    /// </summary>
    public const string TrialCreatorRole = TenantAdmin;

    // ═══════════════════════════════════════════════════════════════════════════
    // ROLE NORMALIZATION (Handle SNAKE_CASE, kebab-case, PascalCase variants)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Master normalization map: lowercase key → canonical PascalCase value
    /// Handles: TENANT_ADMIN, tenant_admin, tenant-admin, TenantAdmin → "TenantAdmin"
    /// </summary>
    private static readonly Dictionary<string, string> NormalizationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Platform roles
        ["platformadmin"] = PlatformAdmin,
        ["platform_admin"] = PlatformAdmin,
        ["platform-admin"] = PlatformAdmin,
        ["sysadmin"] = PlatformAdmin,
        ["superadmin"] = PlatformAdmin,
        ["super_admin"] = PlatformAdmin,

        ["systemadministrator"] = SystemAdministrator,
        ["system_administrator"] = SystemAdministrator,
        ["system-administrator"] = SystemAdministrator,

        // Tenant admin roles
        ["tenantowner"] = TenantOwner,
        ["tenant_owner"] = TenantOwner,
        ["tenant-owner"] = TenantOwner,
        ["owner"] = TenantOwner,

        ["tenantadmin"] = TenantAdmin,
        ["tenant_admin"] = TenantAdmin,
        ["tenant-admin"] = TenantAdmin,
        ["admin"] = TenantAdmin,
        ["administrator"] = TenantAdmin,

        // Executive roles
        ["chiefriskofficer"] = ChiefRiskOfficer,
        ["chief_risk_officer"] = ChiefRiskOfficer,
        ["chief-risk-officer"] = ChiefRiskOfficer,
        ["cro"] = ChiefRiskOfficer,

        ["chiefcomplianceofficer"] = ChiefComplianceOfficer,
        ["chief_compliance_officer"] = ChiefComplianceOfficer,
        ["chief-compliance-officer"] = ChiefComplianceOfficer,
        ["cco"] = ChiefComplianceOfficer,

        ["executivedirector"] = ExecutiveDirector,
        ["executive_director"] = ExecutiveDirector,
        ["executive-director"] = ExecutiveDirector,
        ["director"] = ExecutiveDirector,

        // Management roles
        ["riskmanager"] = RiskManager,
        ["risk_manager"] = RiskManager,
        ["risk-manager"] = RiskManager,

        ["compliancemanager"] = ComplianceManager,
        ["compliance_manager"] = ComplianceManager,
        ["compliance-manager"] = ComplianceManager,

        ["auditmanager"] = AuditManager,
        ["audit_manager"] = AuditManager,
        ["audit-manager"] = AuditManager,

        ["securitymanager"] = SecurityManager,
        ["security_manager"] = SecurityManager,
        ["security-manager"] = SecurityManager,

        ["legalmanager"] = LegalManager,
        ["legal_manager"] = LegalManager,
        ["legal-manager"] = LegalManager,

        // Operational roles
        ["complianceofficer"] = ComplianceOfficer,
        ["compliance_officer"] = ComplianceOfficer,
        ["compliance-officer"] = ComplianceOfficer,

        ["riskanalyst"] = RiskAnalyst,
        ["risk_analyst"] = RiskAnalyst,
        ["risk-analyst"] = RiskAnalyst,

        ["privacyofficer"] = PrivacyOfficer,
        ["privacy_officer"] = PrivacyOfficer,
        ["privacy-officer"] = PrivacyOfficer,
        ["dpo"] = PrivacyOfficer, // Data Protection Officer

        ["qualityassurancemanager"] = QualityAssuranceManager,
        ["quality_assurance_manager"] = QualityAssuranceManager,
        ["quality-assurance-manager"] = QualityAssuranceManager,
        ["qa_manager"] = QualityAssuranceManager,
        ["qamanager"] = QualityAssuranceManager,

        ["processowner"] = ProcessOwner,
        ["process_owner"] = ProcessOwner,
        ["process-owner"] = ProcessOwner,

        // Support roles
        ["operationssupport"] = OperationsSupport,
        ["operations_support"] = OperationsSupport,
        ["operations-support"] = OperationsSupport,
        ["support"] = OperationsSupport,

        ["systemobserver"] = SystemObserver,
        ["system_observer"] = SystemObserver,
        ["system-observer"] = SystemObserver,
        ["observer"] = SystemObserver,
        ["viewer"] = SystemObserver,

        ["employee"] = Employee,
        ["user"] = Employee,
        ["member"] = Employee,
        ["staff"] = Employee,

        ["guest"] = Guest,
        ["external"] = Guest,
        ["readonly"] = Guest,
        ["read_only"] = Guest
    };

    /// <summary>
    /// Normalize any role code variant to canonical PascalCase format.
    /// Returns Employee if no match found.
    /// </summary>
    /// <example>
    /// NormalizeRoleCode("TENANT_ADMIN") → "TenantAdmin"
    /// NormalizeRoleCode("tenant-admin") → "TenantAdmin"
    /// NormalizeRoleCode("TenantAdmin") → "TenantAdmin"
    /// NormalizeRoleCode("admin") → "TenantAdmin"
    /// NormalizeRoleCode("unknown") → "Employee"
    /// </example>
    public static string NormalizeRoleCode(string? roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return DefaultRole;

        var key = roleCode.Trim();

        // First, try exact match (already canonical)
        if (AllRoles.Contains(key, StringComparer.OrdinalIgnoreCase))
            return AllRoles.First(r => r.Equals(key, StringComparison.OrdinalIgnoreCase));

        // Try normalization map
        if (NormalizationMap.TryGetValue(key, out var normalized))
            return normalized;

        // Try without underscores/hyphens
        var stripped = key.Replace("_", "").Replace("-", "").Replace(" ", "");
        if (NormalizationMap.TryGetValue(stripped, out normalized))
            return normalized;

        // Return default if no match
        return DefaultRole;
    }

    /// <summary>
    /// Check if input matches any valid role (after normalization)
    /// </summary>
    public static bool IsValidRole(string? roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return false;

        var normalized = NormalizeRoleCode(roleCode);
        return AllRoles.Contains(normalized);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ROLE HIERARCHY AND ASSIGNMENT RULES (AM-03)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if a role code matches TenantAdmin (handles legacy variations)
    /// </summary>
    public static bool IsTenantAdmin(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized == TenantAdmin;
    }

    /// <summary>
    /// Check if a role code matches TenantOwner
    /// </summary>
    public static bool IsTenantOwner(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized == TenantOwner;
    }

    /// <summary>
    /// Check if a role code matches PlatformAdmin or SystemAdministrator
    /// </summary>
    public static bool IsPlatformAdmin(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized == PlatformAdmin || normalized == SystemAdministrator;
    }

    /// <summary>
    /// Check if a role is privileged (requires MFA, AM-04)
    /// </summary>
    public static bool IsPrivilegedRole(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return PrivilegedRoles.Contains(normalized);
    }

    /// <summary>
    /// Check if a role is platform-level (system-wide, not tenant-specific)
    /// </summary>
    public static bool IsPlatformRole(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return PlatformRoles.Contains(normalized);
    }

    /// <summary>
    /// Check if a role is tenant-level (per-tenant)
    /// </summary>
    public static bool IsTenantRole(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return TenantRoles.Contains(normalized);
    }

    /// <summary>
    /// Get roles that a given role can assign to other users.
    /// Enforces role hierarchy - users cannot assign roles higher than their own.
    /// </summary>
    public static string[] GetAssignableRoles(string? assignerRole)
    {
        var normalized = NormalizeRoleCode(assignerRole);

        return normalized switch
        {
            // Platform admins can assign any role
            "PlatformAdmin" or "SystemAdministrator" => AllRoles,

            // Tenant owner can assign all tenant roles except TenantOwner
            "TenantOwner" => TenantRoles.Where(r => r != TenantOwner).ToArray(),

            // Tenant admin can assign non-owner, non-admin tenant roles
            "TenantAdmin" => TenantRoles
                .Where(r => r != TenantOwner && r != TenantAdmin)
                .ToArray(),

            // Executives can assign manager and below roles
            "ChiefRiskOfficer" or "ChiefComplianceOfficer" or "ExecutiveDirector" =>
                TenantRoles
                    .Where(r => !AdminRoles.Contains(r) && !ExecutiveRoles.Contains(r))
                    .ToArray(),

            // Managers can assign operational and support roles
            "RiskManager" or "ComplianceManager" or "AuditManager" or "SecurityManager" or "LegalManager" or "QualityAssuranceManager" =>
                new[] { ComplianceOfficer, RiskAnalyst, PrivacyOfficer, ProcessOwner, OperationsSupport, SystemObserver, Employee, Guest },

            // Others cannot assign roles
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Check if an assigner can assign a specific role to another user.
    /// </summary>
    public static bool CanAssignRole(string? assignerRole, string? targetRole)
    {
        var assignable = GetAssignableRoles(assignerRole);
        var normalizedTarget = NormalizeRoleCode(targetRole);
        return assignable.Contains(normalizedTarget);
    }

    /// <summary>
    /// Get the role hierarchy level (lower = more privileged)
    /// </summary>
    public static int GetRoleLevel(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized switch
        {
            "PlatformAdmin" => 0,
            "SystemAdministrator" => 1,
            "TenantOwner" => 10,
            "TenantAdmin" => 11,
            "ChiefRiskOfficer" or "ChiefComplianceOfficer" or "ExecutiveDirector" => 20,
            "RiskManager" or "ComplianceManager" or "AuditManager" or "SecurityManager" or "LegalManager" => 30,
            "QualityAssuranceManager" or "ComplianceOfficer" or "RiskAnalyst" or "PrivacyOfficer" or "ProcessOwner" => 40,
            "OperationsSupport" or "SystemObserver" => 50,
            "Employee" => 60,
            "Guest" => 70,
            _ => 100
        };
    }

    /// <summary>
    /// Check if role A is higher than or equal to role B in hierarchy
    /// </summary>
    public static bool IsRoleHigherOrEqual(string? roleA, string? roleB)
    {
        return GetRoleLevel(roleA) <= GetRoleLevel(roleB);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ROLE METADATA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get role display name (for UI)
    /// </summary>
    public static string GetRoleDisplayName(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized switch
        {
            "PlatformAdmin" => "Platform Administrator",
            "SystemAdministrator" => "System Administrator",
            "TenantOwner" => "Tenant Owner",
            "TenantAdmin" => "Tenant Administrator",
            "ChiefRiskOfficer" => "Chief Risk Officer (CRO)",
            "ChiefComplianceOfficer" => "Chief Compliance Officer (CCO)",
            "ExecutiveDirector" => "Executive Director",
            "RiskManager" => "Risk Manager",
            "ComplianceManager" => "Compliance Manager",
            "AuditManager" => "Audit Manager",
            "SecurityManager" => "Security Manager",
            "LegalManager" => "Legal Manager",
            "ComplianceOfficer" => "Compliance Officer",
            "RiskAnalyst" => "Risk Analyst",
            "PrivacyOfficer" => "Privacy Officer (DPO)",
            "QualityAssuranceManager" => "Quality Assurance Manager",
            "ProcessOwner" => "Process Owner",
            "OperationsSupport" => "Operations Support",
            "SystemObserver" => "System Observer",
            "Employee" => "Employee",
            "Guest" => "Guest",
            _ => normalized
        };
    }

    /// <summary>
    /// Get role display name in Arabic
    /// </summary>
    public static string GetRoleDisplayNameAr(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized switch
        {
            "PlatformAdmin" => "مدير المنصة",
            "SystemAdministrator" => "مدير النظام",
            "TenantOwner" => "مالك المستأجر",
            "TenantAdmin" => "مدير المستأجر",
            "ChiefRiskOfficer" => "الرئيس التنفيذي للمخاطر",
            "ChiefComplianceOfficer" => "الرئيس التنفيذي للامتثال",
            "ExecutiveDirector" => "المدير التنفيذي",
            "RiskManager" => "مدير المخاطر",
            "ComplianceManager" => "مدير الامتثال",
            "AuditManager" => "مدير التدقيق",
            "SecurityManager" => "مدير الأمن",
            "LegalManager" => "المدير القانوني",
            "ComplianceOfficer" => "مسؤول الامتثال",
            "RiskAnalyst" => "محلل المخاطر",
            "PrivacyOfficer" => "مسؤول الخصوصية",
            "QualityAssuranceManager" => "مدير ضمان الجودة",
            "ProcessOwner" => "مالك العملية",
            "OperationsSupport" => "دعم العمليات",
            "SystemObserver" => "مراقب النظام",
            "Employee" => "موظف",
            "Guest" => "ضيف",
            _ => normalized
        };
    }

    /// <summary>
    /// Get role description
    /// </summary>
    public static string GetRoleDescription(string? roleCode)
    {
        var normalized = NormalizeRoleCode(roleCode);
        return normalized switch
        {
            "PlatformAdmin" => "Full platform access. Manages all tenants, users, and system configuration.",
            "SystemAdministrator" => "System-level administration. Technical operations and maintenance.",
            "TenantOwner" => "Owns the tenant organization. Can manage all tenant settings and users.",
            "TenantAdmin" => "Administers the tenant. Manages users, roles, and tenant configuration.",
            "ChiefRiskOfficer" => "Executive oversight of all risk management activities.",
            "ChiefComplianceOfficer" => "Executive oversight of all compliance activities.",
            "ExecutiveDirector" => "Executive leadership with broad oversight across GRC functions.",
            "RiskManager" => "Manages risk identification, assessment, and mitigation activities.",
            "ComplianceManager" => "Manages compliance programs, policies, and regulatory requirements.",
            "AuditManager" => "Manages internal and external audit activities.",
            "SecurityManager" => "Manages information security and cybersecurity programs.",
            "LegalManager" => "Manages legal compliance and regulatory affairs.",
            "ComplianceOfficer" => "Executes compliance activities and monitoring.",
            "RiskAnalyst" => "Analyzes and reports on organizational risks.",
            "PrivacyOfficer" => "Manages data privacy and protection compliance.",
            "QualityAssuranceManager" => "Manages quality assurance processes and standards.",
            "ProcessOwner" => "Owns and manages specific business processes.",
            "OperationsSupport" => "Provides operational support for GRC activities.",
            "SystemObserver" => "Read-only access to observe system activities.",
            "Employee" => "Standard employee access to assigned functions.",
            "Guest" => "Limited guest access with read-only permissions.",
            _ => "No description available"
        };
    }

    /// <summary>
    /// Get roles for user invitation dropdown (filtered by assigner's role)
    /// </summary>
    public static IEnumerable<RoleOption> GetRoleOptionsForInvitation(string? assignerRole)
    {
        var assignable = GetAssignableRoles(assignerRole);
        return assignable.Select(r => new RoleOption
        {
            Code = r,
            DisplayName = GetRoleDisplayName(r),
            DisplayNameAr = GetRoleDisplayNameAr(r),
            Description = GetRoleDescription(r),
            Level = GetRoleLevel(r),
            IsPrivileged = IsPrivilegedRole(r)
        }).OrderBy(r => r.Level);
    }
}

/// <summary>
/// Role option for dropdowns and selection UI
/// </summary>
public class RoleOption
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsPrivileged { get; set; }
}
