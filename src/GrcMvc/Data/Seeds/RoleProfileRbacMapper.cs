using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Maps RoleProfiles (15 predefined organizational roles) to Identity Roles and RBAC permissions/features.
/// This creates the bridge between organizational role profiles and technical RBAC system.
/// </summary>
public static class RoleProfileRbacMapper
{
    /// <summary>
    /// Maps RoleProfiles to Identity Roles and assigns appropriate permissions/features based on role profile characteristics.
    /// </summary>
    public static async Task MapRoleProfilesToRbacAsync(
        GrcDbContext context,
        RoleManager<IdentityRole> roleManager,
        Dictionary<string, IdentityRole> identityRoles,
        Dictionary<string, Permission> permissions,
        Dictionary<string, Feature> features,
        Guid tenantId,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("🔗 Mapping RoleProfiles to RBAC system...");

            var roleProfiles = await context.RoleProfiles
                .Where(rp => rp.IsActive)
                .OrderBy(rp => rp.DisplayOrder)
                .ToListAsync();

            if (!roleProfiles.Any())
            {
                logger.LogWarning("No active role profiles found. Skipping mapping.");
                return;
            }

            foreach (var roleProfile in roleProfiles)
            {
                // Find or create corresponding Identity Role
                var identityRoleName = GetIdentityRoleNameForProfile(roleProfile.RoleCode);
                IdentityRole? identityRole = null;

                if (identityRoles.ContainsKey(identityRoleName))
                {
                    identityRole = identityRoles[identityRoleName];
                }
                else
                {
                    // Create Identity Role if it doesn't exist
                    identityRole = new IdentityRole(identityRoleName)
                    {
                        Id = Guid.NewGuid().ToString()
                    };
                    var result = await roleManager.CreateAsync(identityRole);
                    if (result.Succeeded)
                    {
                        identityRoles[identityRoleName] = identityRole;
                        logger.LogInformation($"✅ Created Identity Role: {identityRoleName} for RoleProfile: {roleProfile.RoleName}");
                    }
                    else
                    {
                        logger.LogError($"Failed to create Identity Role {identityRoleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        continue;
                    }
                }

                // Map permissions based on role profile characteristics
                await MapPermissionsForRoleProfileAsync(
                    context, identityRole, roleProfile, permissions, tenantId, logger);

                // Map features (view authority) based on role profile layer and department
                await MapFeaturesForRoleProfileAsync(
                    context, identityRole, roleProfile, features, tenantId, logger);
            }

            logger.LogInformation($"✅ Successfully mapped {roleProfiles.Count} RoleProfiles to RBAC system");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error mapping RoleProfiles to RBAC system");
            throw;
        }
    }

    /// <summary>
    /// Maps Identity Role name to RoleProfile RoleCode
    /// </summary>
    private static string GetIdentityRoleNameForProfile(string roleCode)
    {
        return roleCode switch
        {
            "CRO" => "RiskManager", // Chief Risk Officer → RiskManager
            "CCO" => "ComplianceManager", // Chief Compliance Officer → ComplianceManager
            "ED" => "SuperAdmin", // Executive Director → SuperAdmin
            "RM" => "RiskManager",
            "CM" => "ComplianceManager",
            "AM" => "Auditor",
            "SM" => "ComplianceManager", // Security Manager → ComplianceManager
            "LM" => "PolicyManager", // Legal Manager → PolicyManager
            "CO" => "ComplianceOfficer",
            "RA" => "RiskAnalyst",
            "PO" => "ComplianceOfficer", // Privacy Officer → ComplianceOfficer
            "QAM" => "ComplianceOfficer", // Quality Assurance Manager → ComplianceOfficer
            "PRO" => "ComplianceOfficer", // Process Owner → ComplianceOfficer
            "DS" => "Viewer", // Documentation Specialist → Viewer
            "RPA" => "Viewer", // Reporting Analyst → Viewer
            _ => "Viewer" // Default fallback
        };
    }

    /// <summary>
    /// Maps permissions to Identity Role based on RoleProfile characteristics
    /// </summary>
    private static async Task MapPermissionsForRoleProfileAsync(
        GrcDbContext context,
        IdentityRole identityRole,
        RoleProfile roleProfile,
        Dictionary<string, Permission> permissions,
        Guid tenantId,
        ILogger logger)
    {
        // Check if already mapped
        var existingMappings = await context.RolePermissions
            .Where(rp => rp.RoleId == identityRole.Id && rp.TenantId == tenantId)
            .AnyAsync();

        if (existingMappings)
        {
            logger.LogInformation($"Permissions already mapped for {identityRole.Name}. Skipping.");
            return;
        }

        var permissionCodes = GetPermissionsForRoleProfile(roleProfile);
        var rolePermissions = new List<RolePermission>();

        foreach (var permCode in permissionCodes)
        {
            if (permissions.ContainsKey(permCode))
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = identityRole.Id,
                    PermissionId = permissions[permCode].Id,
                    TenantId = tenantId,
                    AssignedBy = "System"
                });
            }
        }

        if (rolePermissions.Any())
        {
            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
            logger.LogInformation($"✅ Mapped {rolePermissions.Count} permissions to {identityRole.Name} (RoleProfile: {roleProfile.RoleName})");
        }
    }

    /// <summary>
    /// Maps features (view authority) to Identity Role based on RoleProfile
    /// </summary>
    private static async Task MapFeaturesForRoleProfileAsync(
        GrcDbContext context,
        IdentityRole identityRole,
        RoleProfile roleProfile,
        Dictionary<string, Feature> features,
        Guid tenantId,
        ILogger logger)
    {
        // Check if already mapped
        var existingMappings = await context.RoleFeatures
            .Where(rf => rf.RoleId == identityRole.Id && rf.TenantId == tenantId)
            .AnyAsync();

        if (existingMappings)
        {
            logger.LogInformation($"Features already mapped for {identityRole.Name}. Skipping.");
            return;
        }

        var featureCodes = GetFeaturesForRoleProfile(roleProfile);
        var roleFeatures = new List<RoleFeature>();

        foreach (var featureCode in featureCodes)
        {
            if (features.ContainsKey(featureCode))
            {
                roleFeatures.Add(new RoleFeature
                {
                    RoleId = identityRole.Id,
                    FeatureId = features[featureCode].Id,
                    TenantId = tenantId,
                    IsVisible = true,
                    AssignedBy = "System"
                });
            }
        }

        if (roleFeatures.Any())
        {
            await context.RoleFeatures.AddRangeAsync(roleFeatures);
            await context.SaveChangesAsync();
            logger.LogInformation($"✅ Mapped {roleFeatures.Count} features to {identityRole.Name} (RoleProfile: {roleProfile.RoleName})");
        }
    }

    /// <summary>
    /// Determines permissions for a RoleProfile based on layer, department, and approval level
    /// </summary>
    private static List<string> GetPermissionsForRoleProfile(RoleProfile profile)
    {
        var permissions = new List<string> { "Grc.Home", "Grc.Dashboard" };

        // Executive Layer - Full access
        if (profile.Layer == "Executive")
        {
            permissions.AddRange(new[]
            {
                "Grc.Risks.View", "Grc.Risks.Manage", "Grc.Risks.Accept",
                "Grc.Assessments.View", "Grc.Assessments.Create", "Grc.Assessments.Update",
                "Grc.Assessments.Submit", "Grc.Assessments.Approve",
                "Grc.Policies.View", "Grc.Policies.Manage", "Grc.Policies.Approve", "Grc.Policies.Publish",
                "Grc.Reports.View", "Grc.Reports.Export"
            });

            if (profile.RoleCode == "CRO")
            {
                permissions.AddRange(new[]
                {
                    "Grc.Risks.Accept",
                    "Grc.ActionPlans.View", "Grc.ActionPlans.Manage", "Grc.ActionPlans.Close"
                });
            }

            if (profile.RoleCode == "CCO")
            {
                permissions.AddRange(new[]
                {
                    "Grc.Frameworks.View", "Grc.Frameworks.Create", "Grc.Frameworks.Update",
                    "Grc.Regulators.View", "Grc.Regulators.Manage",
                    "Grc.Assessments.Approve",
                    "Grc.Evidence.View", "Grc.Evidence.Approve",
                    "Grc.Policies.Approve", "Grc.Policies.Publish"
                });
            }
        }

        // Management Layer
        if (profile.Layer == "Management")
        {
            if (profile.Department.Contains("Risk"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Risks.View", "Grc.Risks.Manage", "Grc.Risks.Accept",
                    "Grc.ActionPlans.View", "Grc.ActionPlans.Manage", "Grc.ActionPlans.Assign", "Grc.ActionPlans.Close",
                    "Grc.Reports.View", "Grc.Reports.Export"
                });
            }

            if (profile.Department.Contains("Compliance"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Frameworks.View", "Grc.Frameworks.Create", "Grc.Frameworks.Update",
                    "Grc.Regulators.View", "Grc.Regulators.Manage",
                    "Grc.Assessments.View", "Grc.Assessments.Create", "Grc.Assessments.Update",
                    "Grc.Assessments.Submit", "Grc.Assessments.Approve",
                    "Grc.ControlAssessments.View", "Grc.ControlAssessments.Manage",
                    "Grc.Evidence.View", "Grc.Evidence.Upload", "Grc.Evidence.Update", "Grc.Evidence.Approve",
                    "Grc.Policies.View", "Grc.Policies.Manage", "Grc.Policies.Approve",
                    "Grc.ComplianceCalendar.View", "Grc.ComplianceCalendar.Manage"
                });
            }

            if (profile.Department.Contains("Audit"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Audits.View", "Grc.Audits.Manage", "Grc.Audits.Close",
                    "Grc.Evidence.View",
                    "Grc.Assessments.View",
                    "Grc.Reports.View", "Grc.Reports.Export"
                });
            }

            if (profile.Department.Contains("Security"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Risks.View", "Grc.Risks.Manage",
                    "Grc.Policies.View", "Grc.Policies.Manage",
                    "Grc.Evidence.View", "Grc.Evidence.Upload"
                });
            }

            if (profile.Department.Contains("Legal"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Policies.View", "Grc.Policies.Manage", "Grc.Policies.Approve", "Grc.Policies.Publish"
                });
            }
        }

        // Operational Layer
        if (profile.Layer == "Operational")
        {
            permissions.AddRange(new[]
            {
                "Grc.Assessments.View", "Grc.Assessments.Create", "Grc.Assessments.Update", "Grc.Assessments.Submit",
                "Grc.ControlAssessments.View", "Grc.ControlAssessments.Manage",
                "Grc.Evidence.View", "Grc.Evidence.Upload", "Grc.Evidence.Update",
                "Grc.Risks.View", "Grc.Risks.Manage"
            });

            if (profile.Department.Contains("Privacy"))
            {
                permissions.AddRange(new[]
                {
                    "Grc.Policies.View",
                    "Grc.Evidence.View", "Grc.Evidence.Upload"
                });
            }
        }

        // Support Layer - Read-only
        if (profile.Layer == "Support")
        {
            permissions.AddRange(new[]
            {
                "Grc.Reports.View", "Grc.Reports.Export",
                "Grc.Assessments.View",
                "Grc.Evidence.View",
                "Grc.Risks.View",
                "Grc.Policies.View"
            });
        }

        // Approval-based permissions
        if (profile.CanApprove)
        {
            permissions.AddRange(new[]
            {
                "Grc.Assessments.Approve",
                "Grc.Policies.Approve",
                "Grc.Evidence.Approve"
            });
        }

        return permissions.Distinct().ToList();
    }

    /// <summary>
    /// Determines features (view authority) for a RoleProfile
    /// </summary>
    private static List<string> GetFeaturesForRoleProfile(RoleProfile profile)
    {
        var features = new List<string> { "Home", "Dashboard" };

        // Executive Layer - All features
        if (profile.Layer == "Executive")
        {
            features.AddRange(new[]
            {
                "Risks", "ActionPlans", "Assessments", "ControlAssessments",
                "Evidence", "Policies", "Audits", "Reports"
            });
        }

        // Management Layer
        if (profile.Layer == "Management")
        {
            if (profile.Department.Contains("Risk"))
            {
                features.AddRange(new[] { "Risks", "ActionPlans", "Reports" });
            }

            if (profile.Department.Contains("Compliance"))
            {
                features.AddRange(new[]
                {
                    "Frameworks", "Regulators", "Assessments", "ControlAssessments",
                    "Evidence", "Policies", "ComplianceCalendar", "Reports"
                });
            }

            if (profile.Department.Contains("Audit"))
            {
                features.AddRange(new[] { "Audits", "Evidence", "Assessments", "Reports" });
            }

            if (profile.Department.Contains("Security") || profile.Department.Contains("Legal"))
            {
                features.AddRange(new[] { "Risks", "Policies", "Evidence" });
            }
        }

        // Operational Layer
        if (profile.Layer == "Operational")
        {
            features.AddRange(new[]
            {
                "Assessments", "ControlAssessments", "Evidence", "Risks"
            });

            if (profile.Department.Contains("Privacy"))
            {
                features.Add("Policies");
            }
        }

        // Support Layer
        if (profile.Layer == "Support")
        {
            features.AddRange(new[]
            {
                "Reports", "Assessments", "Evidence", "Risks", "Policies"
            });
        }

        return features.Distinct().ToList();
    }
}
