namespace GrcMvc.Configuration;

/// <summary>
/// GRC Module Governance Control Register
/// Defines all MG controls for ABP module lifecycle management.
/// Reference: MG-01 through MG-03
/// </summary>
public static class ModuleGovernanceControls
{
    /// <summary>
    /// MG-01: Module Inventory and Traceability
    /// Ensures every module (active or disabled) is catalogued with a known purpose, owner, and status.
    /// </summary>
    public static class MG01_ModuleInventory
    {
        public const string ControlId = "MG-01";
        public const string Name = "Module Inventory and Traceability";
        public const string Objective = "Ensure every module (active or disabled) is catalogued with purpose, owner, and status";
        public const string Owner = "Platform Engineering";
        public const string Frequency = "Continuous inventory; reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Untracked/unlicensed code could introduce vulnerabilities or licensing risk";

        // Control statement
        public const string Control = "Maintain a Module Register listing all 41 modules (32 active, 9 disabled) with status, owner, and last-changed date. Any module addition/removal must pass change approval.";

        // Test procedure
        public const string TestProcedure = "Export Module Register; verify completeness against code (appsettings/module registration). Check for missing entries.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Module Register export (Excel/Markdown)",
            "Change tickets referencing MG-01 check"
        };

        // Systems
        public static readonly string[] Systems = { "ABP Framework", "Git", "Azure DevOps" };

        // Event types for this control
        public static class Events
        {
            public const string ModuleAdded = "MG01_MODULE_ADDED";
            public const string ModuleRemoved = "MG01_MODULE_REMOVED";
            public const string ModuleStatusChanged = "MG01_MODULE_STATUS_CHANGED";
            public const string InventoryExported = "MG01_INVENTORY_EXPORTED";
            public const string InventoryReviewed = "MG01_INVENTORY_REVIEWED";
            public const string ModuleOwnerChanged = "MG01_MODULE_OWNER_CHANGED";
        }

        // Current module statistics
        public const int TotalModules = 41;
        public const int ActiveModules = 32;
        public const int DisabledModules = 9;
    }

    /// <summary>
    /// MG-02: Module Change Approval
    /// Ensures no module is enabled/disabled in production without authorized approval.
    /// </summary>
    public static class MG02_ChangeApproval
    {
        public const string ControlId = "MG-02";
        public const string Name = "Module Change Approval";
        public const string Objective = "Ensure no module is enabled/disabled in production without authorized approval";
        public const string Owner = "Platform Engineering + Change Advisory Board";
        public const string Frequency = "Per change event; audit quarterly";

        // Risk addressed
        public const string RiskAddressed = "Unauthorized feature changes could disrupt service or introduce risk";

        // Control statement
        public const string Control = "Any module enablement/disablement must be logged in a change ticket and approved by platform owner before deployment.";

        // Test procedure
        public const string TestProcedure = "Sample 5 recent module changes; verify each has an approved change ticket prior to merge/deploy.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Change tickets with approval",
            "Git commit messages referencing tickets",
            "Deployment audit logs"
        };

        // Systems
        public static readonly string[] Systems = { "Azure DevOps", "Git", "CI/CD Pipeline" };

        // Event types for this control
        public static class Events
        {
            public const string ChangeRequested = "MG02_CHANGE_REQUESTED";
            public const string ChangeApproved = "MG02_CHANGE_APPROVED";
            public const string ChangeDenied = "MG02_CHANGE_DENIED";
            public const string ChangeDeployed = "MG02_CHANGE_DEPLOYED";
            public const string ChangeRolledBack = "MG02_CHANGE_ROLLED_BACK";
            public const string EmergencyChange = "MG02_EMERGENCY_CHANGE";
        }

        // Configuration
        public const int MinApproversRequired = 1;
        public const bool RequireCABForProduction = true;
    }

    /// <summary>
    /// MG-03: Environment Parity
    /// Ensures dev/staging/prod have consistent module configurations to prevent surprises in production.
    /// </summary>
    public static class MG03_EnvironmentParity
    {
        public const string ControlId = "MG-03";
        public const string Name = "Environment Parity";
        public const string Objective = "Ensure dev/staging/prod have consistent module configurations";
        public const string Owner = "DevOps + Platform Engineering";
        public const string Frequency = "Continuous (automated); manual review monthly";

        // Risk addressed
        public const string RiskAddressed = "Feature drift between environments could cause production incidents";

        // Control statement
        public const string Control = "Module config must be identical across Dev, Staging, and Prod unless documented and approved. Automated checks validate parity on each deploy.";

        // Test procedure
        public const string TestProcedure = "Run environment comparison script; verify no undocumented differences between Prod and Staging module configs.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Environment parity report",
            "CI/CD job output showing check passed",
            "Exception documentation for intentional differences"
        };

        // Systems
        public static readonly string[] Systems = { "CI/CD Pipeline", "Infrastructure as Code", "Azure App Configuration" };

        // Event types for this control
        public static class Events
        {
            public const string ParityCheckPassed = "MG03_PARITY_CHECK_PASSED";
            public const string ParityCheckFailed = "MG03_PARITY_CHECK_FAILED";
            public const string DriftDetected = "MG03_DRIFT_DETECTED";
            public const string DriftResolved = "MG03_DRIFT_RESOLVED";
            public const string ExceptionApproved = "MG03_EXCEPTION_APPROVED";
            public const string EnvironmentSynced = "MG03_ENVIRONMENT_SYNCED";
        }

        // Environments tracked
        public static readonly string[] TrackedEnvironments = { "Development", "Staging", "Production" };
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        MG01_ModuleInventory.ControlId,
        MG02_ChangeApproval.ControlId,
        MG03_EnvironmentParity.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "MG-01" => (MG01_ModuleInventory.Name, MG01_ModuleInventory.Objective, MG01_ModuleInventory.Owner),
        "MG-02" => (MG02_ChangeApproval.Name, MG02_ChangeApproval.Objective, MG02_ChangeApproval.Owner),
        "MG-03" => (MG03_EnvironmentParity.Name, MG03_EnvironmentParity.Objective, MG03_EnvironmentParity.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };
}

/// <summary>
/// Module Register entry for MG-01 compliance
/// Aligned with module_register_inventory.xlsx columns
/// </summary>
public class ModuleRegisterEntry
{
    public string ModuleId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Status { get; set; } = "Active"; // Active, Disabled, Deprecated
    public string Purpose { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // ABP Framework, Business Module, Integration, AI Feature

    // Excel-aligned columns
    public string DataSensitivity { get; set; } = "Moderate"; // Low, Moderate, High
    public string? Dependencies { get; set; }
    public string? EnablementCriteria { get; set; }
    public string? MonitoringLink { get; set; }
    public string? DisableReason { get; set; }

    // Environment status (MG-03 parity)
    public string DevStatus { get; set; } = "Active";
    public string StagingStatus { get; set; } = "Active";
    public string ProdStatus { get; set; } = "Active";

    // Change tracking
    public DateTime? LastChangedDate { get; set; }
    public string? LastChangedBy { get; set; }
    public string? ChangeTicket { get; set; }
    public DateTime? LastReviewedDate { get; set; }
}

/// <summary>
/// Pre-populated Module Register (41 modules) - Aligned with module_register_inventory.xlsx
/// Categories: ABP Framework (23), Business Module (8), Integration (6), AI Feature (4)
/// Status: 32 Active, 9 Disabled
/// </summary>
public static class AbpModuleRegister
{
    public static readonly ModuleRegisterEntry[] Modules = new[]
    {
        // ============================================
        // ABP FRAMEWORK MODULES (23 modules: 22 Active, 1 Disabled)
        // ============================================

        // MVC/Core
        new ModuleRegisterEntry { ModuleId = "ABP.AspNetCore.Mvc", ModuleName = "AbpAspNetCoreMvcModule", Namespace = "Volo.Abp.AspNetCore.Mvc", IsEnabled = true, Status = "Active", Purpose = "ASP.NET Core MVC integration", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },

        // Entity Framework Core
        new ModuleRegisterEntry { ModuleId = "ABP.EntityFrameworkCore", ModuleName = "AbpEntityFrameworkCoreModule", Namespace = "Volo.Abp.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Entity Framework Core integration", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.EntityFrameworkCore.PostgreSql", ModuleName = "AbpEntityFrameworkCorePostgreSqlModule", Namespace = "Volo.Abp.EntityFrameworkCore.PostgreSql", IsEnabled = true, Status = "Active", Purpose = "PostgreSQL database provider", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Identity
        new ModuleRegisterEntry { ModuleId = "ABP.Identity.Domain", ModuleName = "AbpIdentityDomainModule", Namespace = "Volo.Abp.Identity.Domain", IsEnabled = true, Status = "Active", Purpose = "Identity domain layer", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.Identity.Application", ModuleName = "AbpIdentityApplicationModule", Namespace = "Volo.Abp.Identity.Application", IsEnabled = true, Status = "Active", Purpose = "Identity application services", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.Identity.EntityFrameworkCore", ModuleName = "AbpIdentityEntityFrameworkCoreModule", Namespace = "Volo.Abp.Identity.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Identity EF Core storage", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Permission Management
        new ModuleRegisterEntry { ModuleId = "ABP.PermissionManagement.Domain", ModuleName = "AbpPermissionManagementDomainModule", Namespace = "Volo.Abp.PermissionManagement.Domain", IsEnabled = true, Status = "Active", Purpose = "Permission management domain", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.PermissionManagement.Application", ModuleName = "AbpPermissionManagementApplicationModule", Namespace = "Volo.Abp.PermissionManagement.Application", IsEnabled = true, Status = "Active", Purpose = "Permission management services", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.PermissionManagement.EntityFrameworkCore", ModuleName = "AbpPermissionManagementEntityFrameworkCoreModule", Namespace = "Volo.Abp.PermissionManagement.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Permission EF Core storage", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Audit Logging
        new ModuleRegisterEntry { ModuleId = "ABP.AuditLogging.Domain", ModuleName = "AbpAuditLoggingDomainModule", Namespace = "Volo.Abp.AuditLogging.Domain", IsEnabled = true, Status = "Active", Purpose = "Audit logging domain", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.AuditLogging.EntityFrameworkCore", ModuleName = "AbpAuditLoggingEntityFrameworkCoreModule", Namespace = "Volo.Abp.AuditLogging.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Audit log EF storage", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Feature Management
        new ModuleRegisterEntry { ModuleId = "ABP.FeatureManagement.Domain", ModuleName = "AbpFeatureManagementDomainModule", Namespace = "Volo.Abp.FeatureManagement.Domain", IsEnabled = true, Status = "Active", Purpose = "Feature flag domain", Owner = "Product Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },
        new ModuleRegisterEntry { ModuleId = "ABP.FeatureManagement.Application", ModuleName = "AbpFeatureManagementApplicationModule", Namespace = "Volo.Abp.FeatureManagement.Application", IsEnabled = true, Status = "Active", Purpose = "Feature flag services", Owner = "Product Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },
        new ModuleRegisterEntry { ModuleId = "ABP.FeatureManagement.EntityFrameworkCore", ModuleName = "AbpFeatureManagementEntityFrameworkCoreModule", Namespace = "Volo.Abp.FeatureManagement.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Feature flag EF storage", Owner = "Product Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },

        // Tenant Management
        new ModuleRegisterEntry { ModuleId = "ABP.TenantManagement.Domain", ModuleName = "AbpTenantManagementDomainModule", Namespace = "Volo.Abp.TenantManagement.Domain", IsEnabled = true, Status = "Active", Purpose = "Multi-tenant domain", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.TenantManagement.Application", ModuleName = "AbpTenantManagementApplicationModule", Namespace = "Volo.Abp.TenantManagement.Application", IsEnabled = true, Status = "Active", Purpose = "Multi-tenant services", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.TenantManagement.EntityFrameworkCore", ModuleName = "AbpTenantManagementEntityFrameworkCoreModule", Namespace = "Volo.Abp.TenantManagement.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Tenant EF storage", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Setting Management
        new ModuleRegisterEntry { ModuleId = "ABP.SettingManagement.Domain", ModuleName = "AbpSettingManagementDomainModule", Namespace = "Volo.Abp.SettingManagement.Domain", IsEnabled = true, Status = "Active", Purpose = "Settings domain", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },
        new ModuleRegisterEntry { ModuleId = "ABP.SettingManagement.Application", ModuleName = "AbpSettingManagementApplicationModule", Namespace = "Volo.Abp.SettingManagement.Application", IsEnabled = true, Status = "Active", Purpose = "Settings services", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },
        new ModuleRegisterEntry { ModuleId = "ABP.SettingManagement.EntityFrameworkCore", ModuleName = "AbpSettingManagementEntityFrameworkCoreModule", Namespace = "Volo.Abp.SettingManagement.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "Settings EF storage", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "Moderate" },

        // OpenIddict (Active - EF modules for token storage)
        new ModuleRegisterEntry { ModuleId = "ABP.OpenIddict.Domain", ModuleName = "AbpOpenIddictDomainModule", Namespace = "Volo.Abp.OpenIddict.Domain", IsEnabled = true, Status = "Active", Purpose = "OpenIddict domain layer", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },
        new ModuleRegisterEntry { ModuleId = "ABP.OpenIddict.EntityFrameworkCore", ModuleName = "AbpOpenIddictEntityFrameworkCoreModule", Namespace = "Volo.Abp.OpenIddict.EntityFrameworkCore", IsEnabled = true, Status = "Active", Purpose = "OpenIddict EF storage", Owner = "Security Engineering", Category = "ABP Framework", DataSensitivity = "High" },

        // Background Workers (Disabled - OpenIddict bug)
        new ModuleRegisterEntry { ModuleId = "ABP.BackgroundWorkers", ModuleName = "AbpBackgroundWorkerOptions", Namespace = "Volo.Abp.BackgroundWorkers", IsEnabled = false, Status = "Disabled", Purpose = "Background job processing", Owner = "Platform Engineering", Category = "ABP Framework", DataSensitivity = "Moderate", DisableReason = "OpenIddict initialization bug - using Hangfire as compensating control", EnablementCriteria = "Resolve OpenIddict null logger issue; regression test background workers; approval per change control" },

        // ============================================
        // BUSINESS MODULES (8 modules: all Active)
        // ============================================
        new ModuleRegisterEntry { ModuleId = "GRC.RiskManagement", ModuleName = "Risk Management", Namespace = "GrcMvc.Modules.Risk", IsEnabled = true, Status = "Active", Purpose = "Risk identification, assessment, and treatment", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database; workflow" },
        new ModuleRegisterEntry { ModuleId = "GRC.AuditManagement", ModuleName = "Audit Management", Namespace = "GrcMvc.Modules.Audit", IsEnabled = true, Status = "Active", Purpose = "Internal/external audit tracking", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database; workflow" },
        new ModuleRegisterEntry { ModuleId = "GRC.EvidenceManagement", ModuleName = "Evidence Management", Namespace = "GrcMvc.Modules.Evidence", IsEnabled = true, Status = "Active", Purpose = "Evidence collection and management", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database; workflow" },
        new ModuleRegisterEntry { ModuleId = "GRC.PolicyManagement", ModuleName = "Policy Management", Namespace = "GrcMvc.Modules.Policy", IsEnabled = true, Status = "Active", Purpose = "Policy lifecycle management", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database" },
        new ModuleRegisterEntry { ModuleId = "GRC.VendorManagement", ModuleName = "Vendor Management", Namespace = "GrcMvc.Modules.Vendor", IsEnabled = true, Status = "Active", Purpose = "Third-party vendor risk management", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database" },
        new ModuleRegisterEntry { ModuleId = "GRC.WorkflowEngine", ModuleName = "Workflow Engine", Namespace = "GrcMvc.Modules.Workflow", IsEnabled = true, Status = "Active", Purpose = "Approval and task workflows", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database; workflow" },
        new ModuleRegisterEntry { ModuleId = "GRC.ComplianceCalendar", ModuleName = "Compliance Calendar", Namespace = "GrcMvc.Modules.Calendar", IsEnabled = true, Status = "Active", Purpose = "Compliance deadline tracking", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "Moderate", Dependencies = "Database; workflow" },
        new ModuleRegisterEntry { ModuleId = "GRC.ActionPlans", ModuleName = "Action Plans", Namespace = "GrcMvc.Modules.ActionPlans", IsEnabled = true, Status = "Active", Purpose = "Remediation action tracking", Owner = "Product Engineering", Category = "Business Module", DataSensitivity = "High", Dependencies = "Database; workflow" },

        // ============================================
        // INTEGRATION MODULES (6 modules: 2 Active, 4 Disabled)
        // ============================================
        new ModuleRegisterEntry { ModuleId = "GRC.EmailNotifications", ModuleName = "Email Notifications", Namespace = "GrcMvc.Services.Email", IsEnabled = true, Status = "Active", Purpose = "Email notification delivery", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "Moderate", Dependencies = "SMTP/Email provider", EnablementCriteria = "Provider configured; SPF/DKIM where applicable; alerts on failures" },
        new ModuleRegisterEntry { ModuleId = "GRC.SignalR", ModuleName = "SignalR Real-time", Namespace = "GrcMvc.Hubs", IsEnabled = true, Status = "Active", Purpose = "Real-time notifications via WebSocket", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "Moderate", Dependencies = "SignalR backplane (as needed)", EnablementCriteria = "AuthZ validated; load test" },
        new ModuleRegisterEntry { ModuleId = "GRC.ClickHouse", ModuleName = "ClickHouse Analytics", Namespace = "GrcMvc.Analytics.ClickHouse", IsEnabled = false, Status = "Disabled", Purpose = "Analytics data warehouse", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "Moderate", DisableReason = "Infrastructure not yet provisioned", Dependencies = "ClickHouse cluster; network egress", EnablementCriteria = "Infra ready; security review; monitoring; data classification" },
        new ModuleRegisterEntry { ModuleId = "GRC.Kafka", ModuleName = "Kafka Integration", Namespace = "GrcMvc.Messaging.Kafka", IsEnabled = false, Status = "Disabled", Purpose = "Event streaming platform", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "Moderate", DisableReason = "Infrastructure not yet provisioned", Dependencies = "Kafka cluster; TLS; topic governance", EnablementCriteria = "Infra ready; security review; monitoring; data egress controls" },
        new ModuleRegisterEntry { ModuleId = "GRC.Camunda", ModuleName = "Camunda Workflows", Namespace = "GrcMvc.Workflow.Camunda", IsEnabled = false, Status = "Disabled", Purpose = "External workflow orchestration", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "High", DisableReason = "Using internal workflow engine", Dependencies = "Camunda; network; SSO", EnablementCriteria = "Infra ready; security review; SoD mapping; monitoring" },
        new ModuleRegisterEntry { ModuleId = "GRC.Redis", ModuleName = "Redis Caching", Namespace = "GrcMvc.Caching.Redis", IsEnabled = false, Status = "Disabled", Purpose = "Distributed caching", Owner = "Platform Engineering", Category = "Integration", DataSensitivity = "Moderate", DisableReason = "Using in-memory cache currently", Dependencies = "Redis; encryption; key mgmt", EnablementCriteria = "Infra ready; security review; monitoring; cache safety review" },

        // ============================================
        // AI FEATURE MODULES (4 modules: all Disabled)
        // ============================================
        new ModuleRegisterEntry { ModuleId = "GRC.AI.Agents", ModuleName = "AI Agents", Namespace = "GrcMvc.AI.Agents", IsEnabled = false, Status = "Disabled", Purpose = "AI-powered GRC assistants", Owner = "Product Engineering", Category = "AI Feature", DataSensitivity = "High", DisableReason = "AI governance policy pending approval", Dependencies = "AI provider/API; data handling policy; tenant opt-in; audit logging", EnablementCriteria = "AI policy approved; tenant opt-in; logging enabled; data minimization/redaction; security review; monitoring" },
        new ModuleRegisterEntry { ModuleId = "GRC.AI.Classification", ModuleName = "AI Classification", Namespace = "GrcMvc.AI.Classification", IsEnabled = false, Status = "Disabled", Purpose = "AI-powered risk/data classification", Owner = "Product Engineering", Category = "AI Feature", DataSensitivity = "High", DisableReason = "AI governance policy pending approval", Dependencies = "AI provider/API; data handling policy; tenant opt-in; audit logging", EnablementCriteria = "AI policy approved; tenant opt-in; logging enabled; data minimization/redaction; security review; monitoring" },
        new ModuleRegisterEntry { ModuleId = "GRC.AI.RiskAssessment", ModuleName = "AI Risk Assessment", Namespace = "GrcMvc.AI.RiskAssessment", IsEnabled = false, Status = "Disabled", Purpose = "AI-assisted risk scoring", Owner = "Product Engineering", Category = "AI Feature", DataSensitivity = "High", DisableReason = "AI governance policy pending approval", Dependencies = "AI provider/API; data handling policy; tenant opt-in; audit logging", EnablementCriteria = "AI policy approved; tenant opt-in; logging enabled; data minimization/redaction; security review; monitoring" },
        new ModuleRegisterEntry { ModuleId = "GRC.AI.ComplianceAnalysis", ModuleName = "AI Compliance Analysis", Namespace = "GrcMvc.AI.ComplianceAnalysis", IsEnabled = false, Status = "Disabled", Purpose = "AI gap analysis and recommendations", Owner = "Product Engineering", Category = "AI Feature", DataSensitivity = "High", DisableReason = "AI governance policy pending approval", Dependencies = "AI provider/API; data handling policy; tenant opt-in; audit logging", EnablementCriteria = "AI policy approved; tenant opt-in; logging enabled; data minimization/redaction; security review; monitoring" }
    };

    /// <summary>
    /// Get modules by status
    /// </summary>
    public static ModuleRegisterEntry[] GetByStatus(string status) =>
        Modules.Where(m => m.Status == status).ToArray();

    /// <summary>
    /// Get modules by category
    /// </summary>
    public static ModuleRegisterEntry[] GetByCategory(string category) =>
        Modules.Where(m => m.Category == category).ToArray();

    /// <summary>
    /// Get disabled modules with reasons
    /// </summary>
    public static ModuleRegisterEntry[] GetDisabledWithReasons() =>
        Modules.Where(m => !m.IsEnabled && !string.IsNullOrEmpty(m.DisableReason)).ToArray();

    /// <summary>
    /// Get module counts by category
    /// </summary>
    public static Dictionary<string, (int Active, int Disabled)> GetCountsByCategory()
    {
        return Modules
            .GroupBy(m => m.Category)
            .ToDictionary(
                g => g.Key,
                g => (Active: g.Count(m => m.IsEnabled), Disabled: g.Count(m => !m.IsEnabled))
            );
    }

    /// <summary>
    /// Validate module against Excel inventory
    /// </summary>
    public static bool ValidateModuleCount() => Modules.Length == 41;
}
