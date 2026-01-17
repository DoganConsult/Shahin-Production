using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GrcMvc.Permissions;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds default GRC roles and permissions
/// </summary>
public class GrcRoleDataSeedContributor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrcRoleDataSeedContributor> _logger;

    public GrcRoleDataSeedContributor(
        IServiceProvider serviceProvider,
        ILogger<GrcRoleDataSeedContributor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await CreateRolesAsync(roleManager);
        await AssignPermissionsAsync(roleManager);
    }

    private async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "SuperAdmin",
            "Admin", 
            "ComplianceManager",
            "RiskManager",
            "AuditManager",
            "PolicyManager",
            "EvidenceOfficer",
            "AssessmentAnalyst",
            "Viewer",
            "Guest"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task AssignPermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        // Define role-permission mappings
        var rolePermissions = new Dictionary<string, List<string>>
        {
            // SuperAdmin - all permissions
            ["SuperAdmin"] = GetAllPermissions(),

            // Admin - all except super admin tasks
            ["Admin"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Admin.Access,
                GrcPermissions.Admin.ManageUsers,
                GrcPermissions.Admin.ManageRoles,
                GrcPermissions.Admin.ManageSettings,
                GrcPermissions.Subscriptions.View,
                GrcPermissions.Subscriptions.Create,
                GrcPermissions.Subscriptions.Update,
                GrcPermissions.Frameworks.View,
                GrcPermissions.Frameworks.Create,
                GrcPermissions.Frameworks.Update,
                GrcPermissions.Regulators.View,
                GrcPermissions.Regulators.Create,
                GrcPermissions.Regulators.Update,
                GrcPermissions.Assessments.View,
                GrcPermissions.Assessments.Create,
                GrcPermissions.Assessments.Update,
                GrcPermissions.Assessments.Submit,
                GrcPermissions.Assessments.Approve,
                GrcPermissions.Evidence.View,
                GrcPermissions.Evidence.Create,
                GrcPermissions.Evidence.Update,
                GrcPermissions.Evidence.Submit,
                GrcPermissions.Evidence.Approve,
                GrcPermissions.Risks.View,
                GrcPermissions.Risks.Create,
                GrcPermissions.Risks.Update,
                GrcPermissions.Risks.Accept,
                GrcPermissions.Audits.View,
                GrcPermissions.Audits.Create,
                GrcPermissions.Audits.Update,
                GrcPermissions.Audits.Close,
                GrcPermissions.Policies.View,
                GrcPermissions.Policies.Create,
                GrcPermissions.Policies.Update,
                GrcPermissions.Policies.Approve,
                GrcPermissions.Policies.Publish,
                GrcPermissions.Reports.View,
                GrcPermissions.Reports.Create,
                GrcPermissions.Reports.Export,
                GrcPermissions.Integrations.View,
                GrcPermissions.Integrations.Manage
            },

            // ComplianceManager
            ["ComplianceManager"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Frameworks.View,
                GrcPermissions.Frameworks.Create,
                GrcPermissions.Frameworks.Update,
                GrcPermissions.Regulators.View,
                GrcPermissions.Assessments.View,
                GrcPermissions.Assessments.Create,
                GrcPermissions.Assessments.Update,
                GrcPermissions.Assessments.Submit,
                GrcPermissions.Assessments.Approve,
                GrcPermissions.ControlAssessments.View,
                GrcPermissions.ControlAssessments.Create,
                GrcPermissions.ControlAssessments.Update,
                GrcPermissions.Evidence.View,
                GrcPermissions.Evidence.Create,
                GrcPermissions.Evidence.Update,
                GrcPermissions.Evidence.Submit,
                GrcPermissions.Evidence.Approve,
                GrcPermissions.ComplianceCalendar.View,
                GrcPermissions.ComplianceCalendar.Manage,
                GrcPermissions.Reports.View,
                GrcPermissions.Reports.Create,
                GrcPermissions.Reports.Export
            },

            // RiskManager
            ["RiskManager"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Risks.View,
                GrcPermissions.Risks.Create,
                GrcPermissions.Risks.Update,
                GrcPermissions.Risks.Accept,
                GrcPermissions.ActionPlans.View,
                GrcPermissions.ActionPlans.Create,
                GrcPermissions.ActionPlans.Update,
                GrcPermissions.Reports.View,
                GrcPermissions.Reports.Create,
                GrcPermissions.Reports.Export
            },

            // AuditManager
            ["AuditManager"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Audits.View,
                GrcPermissions.Audits.Create,
                GrcPermissions.Audits.Update,
                GrcPermissions.Audits.Close,
                GrcPermissions.Evidence.View,
                GrcPermissions.Evidence.Create,
                GrcPermissions.ActionPlans.View,
                GrcPermissions.Reports.View,
                GrcPermissions.Reports.Create,
                GrcPermissions.Reports.Export
            },

            // PolicyManager
            ["PolicyManager"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Policies.View,
                GrcPermissions.Policies.Create,
                GrcPermissions.Policies.Update,
                GrcPermissions.Policies.Approve,
                GrcPermissions.Policies.Publish,
                GrcPermissions.Workflow.View,
                GrcPermissions.Workflow.Execute,
                GrcPermissions.Reports.View,
                GrcPermissions.Reports.Create
            },

            // EvidenceOfficer
            ["EvidenceOfficer"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Evidence.View,
                GrcPermissions.Evidence.Create,
                GrcPermissions.Evidence.Update,
                GrcPermissions.Evidence.Submit,
                GrcPermissions.Assessments.View,
                GrcPermissions.ControlAssessments.View
            },

            // AssessmentAnalyst
            ["AssessmentAnalyst"] = new List<string>
            {
                GrcPermissions.Home.Default,
                GrcPermissions.Dashboard.Default,
                GrcPermissions.Assessments.View,
                GrcPermissions.Assessments.Create,
                GrcPermissions.Assessments.Update,
                GrcPermissions.ControlAssessments.View,
                GrcPermissions.ControlAssessments.Create,
                GrcPermissions.ControlAssessments.Update,
                GrcPermissions.Evidence.View,
                GrcPermissions.Reports.View
            },

            // Viewer - read-only access
            ["Viewer"] = new List<string>
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
                GrcPermissions.Vendors.View,
                GrcPermissions.Reports.View
            },

            // Guest - minimal access
            ["Guest"] = new List<string>
            {
                GrcPermissions.Home.Default
            }
        };

        // Store permissions as role claims
        foreach (var (roleName, permissions) in rolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(role);
                
                foreach (var permission in permissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
                        _logger.LogDebug("Added permission {Permission} to role {Role}", permission, roleName);
                    }
                }
            }
        }
    }

    private List<string> GetAllPermissions()
    {
        var permissions = new List<string>
        {
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            GrcPermissions.Subscriptions.View,
            GrcPermissions.Subscriptions.Create,
            GrcPermissions.Subscriptions.Update,
            GrcPermissions.Subscriptions.Delete,
            GrcPermissions.Admin.Access,
            GrcPermissions.Admin.ManageUsers,
            GrcPermissions.Admin.ManageRoles,
            GrcPermissions.Admin.ManageSettings,
            GrcPermissions.Frameworks.View,
            GrcPermissions.Frameworks.Create,
            GrcPermissions.Frameworks.Update,
            GrcPermissions.Frameworks.Delete,
            GrcPermissions.Regulators.View,
            GrcPermissions.Regulators.Create,
            GrcPermissions.Regulators.Update,
            GrcPermissions.Regulators.Delete,
            GrcPermissions.Assessments.View,
            GrcPermissions.Assessments.Create,
            GrcPermissions.Assessments.Update,
            GrcPermissions.Assessments.Submit,
            GrcPermissions.Assessments.Approve,
            GrcPermissions.Assessments.Delete,
            GrcPermissions.ControlAssessments.View,
            GrcPermissions.ControlAssessments.Create,
            GrcPermissions.ControlAssessments.Update,
            GrcPermissions.ControlAssessments.Delete,
            GrcPermissions.Evidence.View,
            GrcPermissions.Evidence.Create,
            GrcPermissions.Evidence.Update,
            GrcPermissions.Evidence.Submit,
            GrcPermissions.Evidence.Approve,
            GrcPermissions.Evidence.Delete,
            GrcPermissions.Risks.View,
            GrcPermissions.Risks.Create,
            GrcPermissions.Risks.Update,
            GrcPermissions.Risks.Accept,
            GrcPermissions.Risks.Delete,
            GrcPermissions.Audits.View,
            GrcPermissions.Audits.Create,
            GrcPermissions.Audits.Update,
            GrcPermissions.Audits.Close,
            GrcPermissions.Audits.Delete,
            GrcPermissions.ActionPlans.View,
            GrcPermissions.ActionPlans.Create,
            GrcPermissions.ActionPlans.Update,
            GrcPermissions.ActionPlans.Delete,
            GrcPermissions.Policies.View,
            GrcPermissions.Policies.Create,
            GrcPermissions.Policies.Update,
            GrcPermissions.Policies.Approve,
            GrcPermissions.Policies.Publish,
            GrcPermissions.Policies.Delete,
            GrcPermissions.ComplianceCalendar.View,
            GrcPermissions.ComplianceCalendar.Manage,
            GrcPermissions.Workflow.View,
            GrcPermissions.Workflow.Design,
            GrcPermissions.Workflow.Execute,
            GrcPermissions.Notifications.View,
            GrcPermissions.Notifications.Manage,
            GrcPermissions.Vendors.View,
            GrcPermissions.Vendors.Create,
            GrcPermissions.Vendors.Update,
            GrcPermissions.Vendors.Delete,
            GrcPermissions.Reports.View,
            GrcPermissions.Reports.Create,
            GrcPermissions.Reports.Export,
            GrcPermissions.Integrations.View,
            GrcPermissions.Integrations.Manage
        };

        return permissions;
    }
}
