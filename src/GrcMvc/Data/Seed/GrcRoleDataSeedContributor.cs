using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Application.Permissions;

namespace GrcMvc.Data.Seed;

/// <summary>
/// Seeds default roles and permission grants for the GRC system.
/// Implements the 8 baseline roles defined in the GRC Policy Enforcement Agent plan.
/// </summary>
public class GrcRoleDataSeedContributor
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly GrcDbContext _context;
    private readonly ILogger<GrcRoleDataSeedContributor> _logger;

    public GrcRoleDataSeedContributor(
        RoleManager<IdentityRole> roleManager,
        GrcDbContext context,
        ILogger<GrcRoleDataSeedContributor> logger)
    {
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting GRC role and permission seeding...");

        try
        {
            // Define 8 baseline roles with their permission mappings
            var roleDefinitions = GetRoleDefinitions();

            foreach (var (roleName, permissions) in roleDefinitions)
            {
                // Create role if it doesn't exist
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Created role: {RoleName}", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role {RoleName}: {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue;
                    }
                }

                // Grant permissions to role
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    await GrantPermissionsToRoleAsync(role.Id, permissions);
                }
            }

            _logger.LogInformation("GRC role and permission seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding GRC roles and permissions");
            throw;
        }
    }

    private async Task GrantPermissionsToRoleAsync(string roleId, List<string> permissions)
    {
        foreach (var permission in permissions)
        {
            // Check if permission grant already exists
            var exists = await _context.Set<IdentityRoleClaim<string>>()
                .AnyAsync(rc => rc.RoleId == roleId && rc.ClaimType == "Permission" && rc.ClaimValue == permission);

            if (!exists)
            {
                // Add permission claim to role
                var claim = new IdentityRoleClaim<string>
                {
                    RoleId = roleId,
                    ClaimType = "Permission",
                    ClaimValue = permission
                };
                _context.Set<IdentityRoleClaim<string>>().Add(claim);
                _logger.LogDebug("Granted permission {Permission} to role {RoleId}", permission, roleId);
            }
        }

        await _context.SaveChangesAsync();
    }

    private Dictionary<string, List<string>> GetRoleDefinitions()
    {
        return new Dictionary<string, List<string>>
        {
            // =====================================
            // PlatformAdmin - Full System Access
            // =====================================
            {
                "PlatformAdmin",
                GrcPermissions.GetAllPermissions().ToList()
            },

            // =====================================
            // TenantAdmin - Tenant Management
            // =====================================
            {
                "TenantAdmin",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Subscriptions.View,
                    GrcPermissions.Subscriptions.Manage,
                    GrcPermissions.Admin.Access,
                    GrcPermissions.Admin.Users,
                    GrcPermissions.Admin.Roles,
                    GrcPermissions.Admin.Tenants,
                    GrcPermissions.Integrations.View,
                    GrcPermissions.Integrations.Manage,
                    GrcPermissions.Reports.View,
                    GrcPermissions.Reports.Export
                }
            },

            // =====================================
            // ComplianceManager - Full GRC Operations
            // =====================================
            {
                "ComplianceManager",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Frameworks.View,
                    GrcPermissions.Frameworks.Create,
                    GrcPermissions.Frameworks.Update,
                    GrcPermissions.Frameworks.Delete,
                    GrcPermissions.Frameworks.Import,
                    GrcPermissions.Regulators.View,
                    GrcPermissions.Regulators.Manage,
                    GrcPermissions.Assessments.View,
                    GrcPermissions.Assessments.Create,
                    GrcPermissions.Assessments.Update,
                    GrcPermissions.Assessments.Submit,
                    GrcPermissions.Assessments.Approve,
                    GrcPermissions.ControlAssessments.View,
                    GrcPermissions.ControlAssessments.Manage,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Evidence.Upload,
                    GrcPermissions.Evidence.Update,
                    GrcPermissions.Evidence.Approve,
                    GrcPermissions.Policies.View,
                    GrcPermissions.Policies.Manage,
                    GrcPermissions.Policies.Approve,
                    GrcPermissions.Policies.Publish,
                    GrcPermissions.ComplianceCalendar.View,
                    GrcPermissions.ComplianceCalendar.Manage,
                    GrcPermissions.Workflow.View,
                    GrcPermissions.Workflow.Manage,
                    GrcPermissions.Notifications.View,
                    GrcPermissions.Reports.View,
                    GrcPermissions.Reports.Export
                }
            },

            // =====================================
            // RiskManager - Risk & Action Plans
            // =====================================
            {
                "RiskManager",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Risks.View,
                    GrcPermissions.Risks.Manage,
                    GrcPermissions.Risks.Accept,
                    GrcPermissions.ActionPlans.View,
                    GrcPermissions.ActionPlans.Manage,
                    GrcPermissions.ActionPlans.Assign,
                    GrcPermissions.ActionPlans.Close,
                    GrcPermissions.Assessments.View,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Reports.View,
                    GrcPermissions.Reports.Export,
                    GrcPermissions.Notifications.View
                }
            },

            // =====================================
            // Auditor - Read-Only Audit Access
            // =====================================
            {
                "Auditor",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Audits.View,
                    GrcPermissions.Audits.Manage,
                    GrcPermissions.Audits.Close,
                    GrcPermissions.Assessments.View,
                    GrcPermissions.ControlAssessments.View,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Risks.View,
                    GrcPermissions.Policies.View,
                    GrcPermissions.Reports.View,
                    GrcPermissions.Reports.Export,
                    GrcPermissions.Notifications.View
                }
            },

            // =====================================
            // EvidenceOfficer - Evidence Management
            // =====================================
            {
                "EvidenceOfficer",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Evidence.Upload,
                    GrcPermissions.Evidence.Update,
                    GrcPermissions.Evidence.Delete,
                    GrcPermissions.Assessments.View,
                    GrcPermissions.ControlAssessments.View,
                    GrcPermissions.Notifications.View
                }
            },

            // =====================================
            // VendorManager - Vendor Risk Management
            // =====================================
            {
                "VendorManager",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Vendors.View,
                    GrcPermissions.Vendors.Manage,
                    GrcPermissions.Vendors.Assess,
                    GrcPermissions.Risks.View,
                    GrcPermissions.Risks.Manage,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Reports.View,
                    GrcPermissions.Notifications.View
                }
            },

            // =====================================
            // Viewer - Read-Only Access
            // =====================================
            {
                "Viewer",
                new List<string>
                {
                    GrcPermissions.Home.Default,
                    GrcPermissions.Dashboard.Default,
                    GrcPermissions.Frameworks.View,
                    GrcPermissions.Regulators.View,
                    GrcPermissions.Assessments.View,
                    GrcPermissions.ControlAssessments.View,
                    GrcPermissions.Evidence.View,
                    GrcPermissions.Risks.View,
                    GrcPermissions.Audits.View,
                    GrcPermissions.ActionPlans.View,
                    GrcPermissions.Policies.View,
                    GrcPermissions.ComplianceCalendar.View,
                    GrcPermissions.Workflow.View,
                    GrcPermissions.Notifications.View,
                    GrcPermissions.Vendors.View,
                    GrcPermissions.Reports.View
                    // Note: NO Export permission for Viewer
                }
            }
        };
    }
}
