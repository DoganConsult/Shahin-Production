using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds comprehensive RBAC system:
/// - All GRC Permissions (View, Create, Update, Delete, Approve for each module)
/// - All Features (Risks, Audits, Evidence, Policies, etc.)
/// - Identity Roles (Admin, Manager, Compliance Officer, etc.)
/// - Role-Permission mappings
/// - Role-Feature mappings (view authority)
/// </summary>
public static class RbacSeeds
{
    public static async Task SeedRbacSystemAsync(
        GrcDbContext context,
        RoleManager<IdentityRole> roleManager,
        Guid defaultTenantId,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("🔐 Starting RBAC system seeding...");

            // Step 1: Seed all Permissions
            var permissions = await SeedPermissionsAsync(context, logger);

            // Step 2: Seed all Features
            var features = await SeedFeaturesAsync(context, logger);

            // Step 3: Link Features to Permissions
            await LinkFeaturesToPermissionsAsync(context, features, permissions, logger);

            // Step 4: Create Identity Roles
            var roles = await SeedIdentityRolesAsync(roleManager, logger);

            // Step 5: Map Roles to Permissions
            await MapRolesToPermissionsAsync(context, roles, permissions, defaultTenantId, logger);

            // Step 6: Map Roles to Features (View Authority)
            await MapRolesToFeaturesAsync(context, roles, features, defaultTenantId, logger);

            // Step 7: Map RoleProfiles to Identity Roles and RBAC
            await RoleProfileRbacMapper.MapRoleProfilesToRbacAsync(
                context, roleManager, roles, permissions, features, defaultTenantId, logger);

            logger.LogInformation("✅ RBAC system seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during RBAC system seeding");
            throw;
        }
    }

    private static async Task<Dictionary<string, Permission>> SeedPermissionsAsync(
        GrcDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Seeding permissions...");

        if (await context.Permissions.AnyAsync())
        {
            logger.LogInformation("Permissions already exist. Skipping seed.");
            return await context.Permissions.ToDictionaryAsync(p => p.Code);
        }

        var permissions = new List<Permission>();

        // Home & Dashboard
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Home", Name = "Access Home", Category = "Home", Description = "Access to home page" },
            new Permission { Code = "Grc.Dashboard", Name = "View Dashboard", Category = "Dashboard", Description = "View dashboard and metrics" },
        });

        // Subscriptions
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Subscriptions.View", Name = "View Subscriptions", Category = "Subscriptions", Description = "View subscription information" },
            new Permission { Code = "Grc.Subscriptions.Manage", Name = "Manage Subscriptions", Category = "Subscriptions", Description = "Manage subscription plans and billing" },
        });

        // Admin
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Admin.Access", Name = "Admin Access", Category = "Admin", Description = "Access admin section" },
            new Permission { Code = "Grc.Admin.Users", Name = "Manage Users", Category = "Admin", Description = "Create, edit, and delete users" },
            new Permission { Code = "Grc.Admin.Roles", Name = "Manage Roles", Category = "Admin", Description = "Create, edit, and delete roles" },
            new Permission { Code = "Grc.Admin.Tenants", Name = "Manage Tenants", Category = "Admin", Description = "Manage tenant configurations" },
        });

        // Frameworks
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Frameworks.View", Name = "View Frameworks", Category = "Frameworks", Description = "View regulatory frameworks" },
            new Permission { Code = "Grc.Frameworks.Create", Name = "Create Frameworks", Category = "Frameworks", Description = "Create new frameworks" },
            new Permission { Code = "Grc.Frameworks.Update", Name = "Update Frameworks", Category = "Frameworks", Description = "Update existing frameworks" },
            new Permission { Code = "Grc.Frameworks.Delete", Name = "Delete Frameworks", Category = "Frameworks", Description = "Delete frameworks" },
            new Permission { Code = "Grc.Frameworks.Import", Name = "Import Frameworks", Category = "Frameworks", Description = "Import frameworks from external sources" },
        });

        // Regulators
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Regulators.View", Name = "View Regulators", Category = "Regulators", Description = "View regulatory authorities" },
            new Permission { Code = "Grc.Regulators.Manage", Name = "Manage Regulators", Category = "Regulators", Description = "Create, update, and delete regulators" },
        });

        // Assessments
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Assessments.View", Name = "View Assessments", Category = "Assessments", Description = "View compliance assessments" },
            new Permission { Code = "Grc.Assessments.Create", Name = "Create Assessments", Category = "Assessments", Description = "Create new assessments" },
            new Permission { Code = "Grc.Assessments.Update", Name = "Update Assessments", Category = "Assessments", Description = "Update existing assessments" },
            new Permission { Code = "Grc.Assessments.Submit", Name = "Submit Assessments", Category = "Assessments", Description = "Submit assessments for review" },
            new Permission { Code = "Grc.Assessments.Approve", Name = "Approve Assessments", Category = "Assessments", Description = "Approve submitted assessments" },
        });

        // Control Assessments
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.ControlAssessments.View", Name = "View Control Assessments", Category = "ControlAssessments", Description = "View control assessments" },
            new Permission { Code = "Grc.ControlAssessments.Manage", Name = "Manage Control Assessments", Category = "ControlAssessments", Description = "Create, update, and delete control assessments" },
        });

        // Evidence
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Evidence.View", Name = "View Evidence", Category = "Evidence", Description = "View evidence documents" },
            new Permission { Code = "Grc.Evidence.Upload", Name = "Upload Evidence", Category = "Evidence", Description = "Upload new evidence" },
            new Permission { Code = "Grc.Evidence.Update", Name = "Update Evidence", Category = "Evidence", Description = "Update existing evidence" },
            new Permission { Code = "Grc.Evidence.Delete", Name = "Delete Evidence", Category = "Evidence", Description = "Delete evidence" },
            new Permission { Code = "Grc.Evidence.Approve", Name = "Approve Evidence", Category = "Evidence", Description = "Approve evidence submissions" },
        });

        // Risks
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Risks.View", Name = "View Risks", Category = "Risks", Description = "View risk register" },
            new Permission { Code = "Grc.Risks.Manage", Name = "Manage Risks", Category = "Risks", Description = "Create, update, and delete risks" },
            new Permission { Code = "Grc.Risks.Accept", Name = "Accept Risks", Category = "Risks", Description = "Accept risk ownership" },
        });

        // Audits
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Audits.View", Name = "View Audits", Category = "Audits", Description = "View audit records" },
            new Permission { Code = "Grc.Audits.Manage", Name = "Manage Audits", Category = "Audits", Description = "Create, update, and manage audits" },
            new Permission { Code = "Grc.Audits.Close", Name = "Close Audits", Category = "Audits", Description = "Close completed audits" },
        });

        // Action Plans
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.ActionPlans.View", Name = "View Action Plans", Category = "ActionPlans", Description = "View action plans" },
            new Permission { Code = "Grc.ActionPlans.Manage", Name = "Manage Action Plans", Category = "ActionPlans", Description = "Create, update, and delete action plans" },
            new Permission { Code = "Grc.ActionPlans.Assign", Name = "Assign Action Plans", Category = "ActionPlans", Description = "Assign action plans to users" },
            new Permission { Code = "Grc.ActionPlans.Close", Name = "Close Action Plans", Category = "ActionPlans", Description = "Close completed action plans" },
        });

        // Policies
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Policies.View", Name = "View Policies", Category = "Policies", Description = "View policy documents" },
            new Permission { Code = "Grc.Policies.Manage", Name = "Manage Policies", Category = "Policies", Description = "Create, update, and delete policies" },
            new Permission { Code = "Grc.Policies.Approve", Name = "Approve Policies", Category = "Policies", Description = "Approve policy documents" },
            new Permission { Code = "Grc.Policies.Publish", Name = "Publish Policies", Category = "Policies", Description = "Publish approved policies" },
        });

        // Compliance Calendar
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.ComplianceCalendar.View", Name = "View Compliance Calendar", Category = "ComplianceCalendar", Description = "View compliance calendar events" },
            new Permission { Code = "Grc.ComplianceCalendar.Manage", Name = "Manage Compliance Calendar", Category = "ComplianceCalendar", Description = "Create and manage compliance events" },
        });

        // Workflow
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Workflow.View", Name = "View Workflows", Category = "Workflow", Description = "View workflow instances" },
            new Permission { Code = "Grc.Workflow.Manage", Name = "Manage Workflows", Category = "Workflow", Description = "Create and manage workflows" },
        });

        // Notifications
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Notifications.View", Name = "View Notifications", Category = "Notifications", Description = "View notifications" },
            new Permission { Code = "Grc.Notifications.Manage", Name = "Manage Notifications", Category = "Notifications", Description = "Manage notification settings" },
        });

        // Vendors
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Vendors.View", Name = "View Vendors", Category = "Vendors", Description = "View vendor information" },
            new Permission { Code = "Grc.Vendors.Manage", Name = "Manage Vendors", Category = "Vendors", Description = "Create, update, and delete vendors" },
            new Permission { Code = "Grc.Vendors.Assess", Name = "Assess Vendors", Category = "Vendors", Description = "Perform vendor assessments" },
        });

        // Reports
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Reports.View", Name = "View Reports", Category = "Reports", Description = "View reports" },
            new Permission { Code = "Grc.Reports.Export", Name = "Export Reports", Category = "Reports", Description = "Export and download reports" },
        });

        // Integrations
        permissions.AddRange(new[]
        {
            new Permission { Code = "Grc.Integrations.View", Name = "View Integrations", Category = "Integrations", Description = "View integration configurations" },
            new Permission { Code = "Grc.Integrations.Manage", Name = "Manage Integrations", Category = "Integrations", Description = "Configure and manage integrations" },
        });

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Seeded {permissions.Count} permissions");
        return permissions.ToDictionary(p => p.Code);
    }

    private static async Task<Dictionary<string, Feature>> SeedFeaturesAsync(
        GrcDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Seeding features...");

        if (await context.Features.AnyAsync())
        {
            logger.LogInformation("Features already exist. Skipping seed.");
            return await context.Features.ToDictionaryAsync(f => f.Code);
        }

        var features = new List<Feature>
        {
            new Feature { Code = "Home", Name = "Home", Category = "GRC", DisplayOrder = 1, Description = "Home page" },
            new Feature { Code = "Dashboard", Name = "Dashboard", Category = "GRC", DisplayOrder = 2, Description = "Dashboard and metrics" },
            new Feature { Code = "Subscriptions", Name = "Subscriptions", Category = "GRC", DisplayOrder = 3, Description = "Subscription management" },
            new Feature { Code = "Admin", Name = "Administration", Category = "GRC", DisplayOrder = 4, Description = "System administration" },
            new Feature { Code = "Frameworks", Name = "Regulatory Frameworks", Category = "Compliance", DisplayOrder = 5, Description = "Regulatory framework library" },
            new Feature { Code = "Regulators", Name = "Regulators", Category = "Compliance", DisplayOrder = 6, Description = "Regulatory authorities" },
            new Feature { Code = "Assessments", Name = "Assessments", Category = "Compliance", DisplayOrder = 7, Description = "Compliance assessments" },
            new Feature { Code = "ControlAssessments", Name = "Control Assessments", Category = "Compliance", DisplayOrder = 8, Description = "Control effectiveness assessments" },
            new Feature { Code = "Evidence", Name = "Evidence", Category = "Compliance", DisplayOrder = 9, Description = "Evidence collection and management" },
            new Feature { Code = "Risks", Name = "Risk Management", Category = "Risk", DisplayOrder = 10, Description = "Risk register and management" },
            new Feature { Code = "Audits", Name = "Audits", Category = "Audit", DisplayOrder = 11, Description = "Audit management" },
            new Feature { Code = "ActionPlans", Name = "Action Plans", Category = "GRC", DisplayOrder = 12, Description = "Remediation action plans" },
            new Feature { Code = "Policies", Name = "Policies", Category = "Compliance", DisplayOrder = 13, Description = "Policy management" },
            new Feature { Code = "ComplianceCalendar", Name = "Compliance Calendar", Category = "Compliance", DisplayOrder = 14, Description = "Compliance event calendar" },
            new Feature { Code = "Workflow", Name = "Workflows", Category = "GRC", DisplayOrder = 15, Description = "Workflow engine" },
            new Feature { Code = "Notifications", Name = "Notifications", Category = "GRC", DisplayOrder = 16, Description = "Notifications center" },
            new Feature { Code = "Vendors", Name = "Vendors", Category = "GRC", DisplayOrder = 17, Description = "Vendor management" },
            new Feature { Code = "Reports", Name = "Reports", Category = "Reporting", DisplayOrder = 18, Description = "Reports and analytics" },
            new Feature { Code = "Integrations", Name = "Integrations", Category = "GRC", DisplayOrder = 19, Description = "System integrations" },
        };

        await context.Features.AddRangeAsync(features);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Seeded {features.Count} features");
        return features.ToDictionary(f => f.Code);
    }

    private static async Task LinkFeaturesToPermissionsAsync(
        GrcDbContext context,
        Dictionary<string, Feature> features,
        Dictionary<string, Permission> permissions,
        ILogger logger)
    {
        logger.LogInformation("Linking features to permissions...");

        if (await context.FeaturePermissions.AnyAsync())
        {
            logger.LogInformation("Feature-Permission links already exist. Skipping.");
            return;
        }

        var featurePermissions = new List<FeaturePermission>();

        // Home requires Home permission
        if (features.ContainsKey("Home") && permissions.ContainsKey("Grc.Home"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Home"].Id,
                PermissionId = permissions["Grc.Home"].Id,
                IsRequired = true
            });
        }

        // Dashboard requires Dashboard permission
        if (features.ContainsKey("Dashboard") && permissions.ContainsKey("Grc.Dashboard"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Dashboard"].Id,
                PermissionId = permissions["Grc.Dashboard"].Id,
                IsRequired = true
            });
        }

        // Subscriptions requires View permission
        if (features.ContainsKey("Subscriptions") && permissions.ContainsKey("Grc.Subscriptions.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Subscriptions"].Id,
                PermissionId = permissions["Grc.Subscriptions.View"].Id,
                IsRequired = true
            });
        }

        // Admin requires Admin.Access
        if (features.ContainsKey("Admin") && permissions.ContainsKey("Grc.Admin.Access"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Admin"].Id,
                PermissionId = permissions["Grc.Admin.Access"].Id,
                IsRequired = true
            });
        }

        // Frameworks requires View permission
        if (features.ContainsKey("Frameworks") && permissions.ContainsKey("Grc.Frameworks.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Frameworks"].Id,
                PermissionId = permissions["Grc.Frameworks.View"].Id,
                IsRequired = true
            });
        }

        // Regulators requires View permission
        if (features.ContainsKey("Regulators") && permissions.ContainsKey("Grc.Regulators.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Regulators"].Id,
                PermissionId = permissions["Grc.Regulators.View"].Id,
                IsRequired = true
            });
        }

        // Assessments requires View permission
        if (features.ContainsKey("Assessments") && permissions.ContainsKey("Grc.Assessments.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Assessments"].Id,
                PermissionId = permissions["Grc.Assessments.View"].Id,
                IsRequired = true
            });
        }

        // ControlAssessments requires View permission
        if (features.ContainsKey("ControlAssessments") && permissions.ContainsKey("Grc.ControlAssessments.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["ControlAssessments"].Id,
                PermissionId = permissions["Grc.ControlAssessments.View"].Id,
                IsRequired = true
            });
        }

        // Evidence requires View permission
        if (features.ContainsKey("Evidence") && permissions.ContainsKey("Grc.Evidence.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Evidence"].Id,
                PermissionId = permissions["Grc.Evidence.View"].Id,
                IsRequired = true
            });
        }

        // Risks requires View permission
        if (features.ContainsKey("Risks") && permissions.ContainsKey("Grc.Risks.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Risks"].Id,
                PermissionId = permissions["Grc.Risks.View"].Id,
                IsRequired = true
            });
        }

        // Audits requires View permission
        if (features.ContainsKey("Audits") && permissions.ContainsKey("Grc.Audits.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Audits"].Id,
                PermissionId = permissions["Grc.Audits.View"].Id,
                IsRequired = true
            });
        }

        // ActionPlans requires View permission
        if (features.ContainsKey("ActionPlans") && permissions.ContainsKey("Grc.ActionPlans.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["ActionPlans"].Id,
                PermissionId = permissions["Grc.ActionPlans.View"].Id,
                IsRequired = true
            });
        }

        // Policies requires View permission
        if (features.ContainsKey("Policies") && permissions.ContainsKey("Grc.Policies.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Policies"].Id,
                PermissionId = permissions["Grc.Policies.View"].Id,
                IsRequired = true
            });
        }

        // ComplianceCalendar requires View permission
        if (features.ContainsKey("ComplianceCalendar") && permissions.ContainsKey("Grc.ComplianceCalendar.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["ComplianceCalendar"].Id,
                PermissionId = permissions["Grc.ComplianceCalendar.View"].Id,
                IsRequired = true
            });
        }

        // Workflow requires View permission
        if (features.ContainsKey("Workflow") && permissions.ContainsKey("Grc.Workflow.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Workflow"].Id,
                PermissionId = permissions["Grc.Workflow.View"].Id,
                IsRequired = true
            });
        }

        // Notifications requires View permission
        if (features.ContainsKey("Notifications") && permissions.ContainsKey("Grc.Notifications.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Notifications"].Id,
                PermissionId = permissions["Grc.Notifications.View"].Id,
                IsRequired = true
            });
        }

        // Vendors requires View permission
        if (features.ContainsKey("Vendors") && permissions.ContainsKey("Grc.Vendors.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Vendors"].Id,
                PermissionId = permissions["Grc.Vendors.View"].Id,
                IsRequired = true
            });
        }

        // Reports requires View permission
        if (features.ContainsKey("Reports") && permissions.ContainsKey("Grc.Reports.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Reports"].Id,
                PermissionId = permissions["Grc.Reports.View"].Id,
                IsRequired = true
            });
        }

        // Integrations requires View permission
        if (features.ContainsKey("Integrations") && permissions.ContainsKey("Grc.Integrations.View"))
        {
            featurePermissions.Add(new FeaturePermission
            {
                FeatureId = features["Integrations"].Id,
                PermissionId = permissions["Grc.Integrations.View"].Id,
                IsRequired = true
            });
        }

        await context.FeaturePermissions.AddRangeAsync(featurePermissions);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Linked {featurePermissions.Count} features to permissions");
    }

    private static async Task<Dictionary<string, IdentityRole>> SeedIdentityRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        logger.LogInformation("Seeding identity roles...");

        var roles = new Dictionary<string, IdentityRole>();

        // Define all GRC roles - 4 Level Hierarchy
        var roleDefinitions = new[]
        {
            // Level 1: Platform Administration (Shahin-AI staff)
            new { Name = "PlatformAdmin", Description = "Platform Administrator - Full system access, manages all tenants" },

            // Level 2: Tenant Administration
            new { Name = "TenantAdmin", Description = "Tenant Administrator - Tenant management, users, roles within tenant" },

            // Level 3: Application User Administration (delegated)
            new { Name = "AppUserAdmin", Description = "Application User Admin - Manages users within tenant (delegated by TenantAdmin)" },

            // Level 4: Basic User
            new { Name = "User", Description = "Basic User - Standard authenticated user with assigned permissions" },

            // Operational Roles (assigned to Users)
            new { Name = "ComplianceManager", Description = "Compliance Manager - Compliance oversight" },
            new { Name = "RiskManager", Description = "Risk Manager - Risk management" },
            new { Name = "Auditor", Description = "Auditor - Audit activities" },
            new { Name = "EvidenceOfficer", Description = "Evidence Officer - Evidence management" },
            new { Name = "VendorManager", Description = "Vendor Manager - Vendor management" },
            new { Name = "Viewer", Description = "Viewer - Read-only access" },
            new { Name = "ComplianceOfficer", Description = "Compliance Officer - Operational compliance" },
            new { Name = "RiskAnalyst", Description = "Risk Analyst - Risk analysis" },
            new { Name = "PolicyManager", Description = "Policy Manager - Policy management" },
            new { Name = "WorkflowManager", Description = "Workflow Manager - Workflow management" },
        };

        foreach (var roleDef in roleDefinitions)
        {
            var existingRole = await roleManager.FindByNameAsync(roleDef.Name);
            if (existingRole == null)
            {
                var role = new IdentityRole(roleDef.Name)
                {
                    Id = Guid.NewGuid().ToString()
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    roles[roleDef.Name] = role;
                    logger.LogInformation($"✅ Created role: {roleDef.Name}");
                }
                else
                {
                    logger.LogError($"Failed to create role {roleDef.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                roles[roleDef.Name] = existingRole;
                logger.LogInformation($"Role {roleDef.Name} already exists");
            }
        }

        logger.LogInformation($"✅ Seeded {roles.Count} identity roles");
        return roles;
    }

    private static async Task MapRolesToPermissionsAsync(
        GrcDbContext context,
        Dictionary<string, IdentityRole> roles,
        Dictionary<string, Permission> permissions,
        Guid tenantId,
        ILogger logger)
    {
        logger.LogInformation("Mapping roles to permissions...");

        if (await context.RolePermissions.AnyAsync(rp => rp.TenantId == tenantId))
        {
            logger.LogInformation("Role-Permission mappings already exist. Skipping.");
            return;
        }

        var rolePermissions = new List<RolePermission>();

        // PlatformAdmin - All permissions
        if (roles.ContainsKey("PlatformAdmin"))
        {
            foreach (var permission in permissions.Values)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = roles["PlatformAdmin"].Id,
                    PermissionId = permission.Id,
                    TenantId = tenantId,
                    AssignedBy = "System"
                });
            }
        }

        // TenantAdmin - Admin + Subscriptions + Integrations + Users/Roles within tenant
        if (roles.ContainsKey("TenantAdmin"))
        {
            var tenantAdminPermissions = new[] {
                "Grc.Home", "Grc.Dashboard", "Grc.Subscriptions.View", "Grc.Subscriptions.Manage",
                "Grc.Admin.Access", "Grc.Admin.Users", "Grc.Admin.Roles",
                "Grc.Integrations.View", "Grc.Integrations.Manage"
            };
            foreach (var permCode in tenantAdminPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["TenantAdmin"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // AppUserAdmin - User management within tenant (delegated by TenantAdmin)
        if (roles.ContainsKey("AppUserAdmin"))
        {
            var appUserAdminPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Admin.Access", "Grc.Admin.Users"
            };
            foreach (var permCode in appUserAdminPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["AppUserAdmin"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // User - Basic authenticated user (Home + Dashboard only, other permissions assigned per user)
        if (roles.ContainsKey("User"))
        {
            var userPermissions = new[] {
                "Grc.Home", "Grc.Dashboard"
            };
            foreach (var permCode in userPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["User"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // ComplianceManager - Frameworks, Regulators, Assessments, Evidence, Policies, Calendar, Workflow, Reports
        if (roles.ContainsKey("ComplianceManager"))
        {
            var compliancePermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Frameworks.View", "Grc.Frameworks.Create", "Grc.Frameworks.Update", "Grc.Frameworks.Import",
                "Grc.Regulators.View", "Grc.Regulators.Manage",
                "Grc.Assessments.View", "Grc.Assessments.Create", "Grc.Assessments.Update", "Grc.Assessments.Submit", "Grc.Assessments.Approve",
                "Grc.ControlAssessments.View", "Grc.ControlAssessments.Manage",
                "Grc.Evidence.View", "Grc.Evidence.Upload", "Grc.Evidence.Update", "Grc.Evidence.Approve",
                "Grc.Policies.View", "Grc.Policies.Manage", "Grc.Policies.Approve", "Grc.Policies.Publish",
                "Grc.ComplianceCalendar.View", "Grc.ComplianceCalendar.Manage",
                "Grc.Workflow.View", "Grc.Workflow.Manage",
                "Grc.Reports.View", "Grc.Reports.Export"
            };
            foreach (var permCode in compliancePermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["ComplianceManager"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // RiskManager - Risks, ActionPlans, Reports
        if (roles.ContainsKey("RiskManager"))
        {
            var riskPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Risks.View", "Grc.Risks.Manage", "Grc.Risks.Accept",
                "Grc.ActionPlans.View", "Grc.ActionPlans.Manage", "Grc.ActionPlans.Assign", "Grc.ActionPlans.Close",
                "Grc.Reports.View", "Grc.Reports.Export"
            };
            foreach (var permCode in riskPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["RiskManager"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // Auditor - Audits + read-only on Evidence/Assessments
        if (roles.ContainsKey("Auditor"))
        {
            var auditorPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Audits.View", "Grc.Audits.Manage", "Grc.Audits.Close",
                "Grc.Evidence.View",
                "Grc.Assessments.View",
                "Grc.Reports.View", "Grc.Reports.Export"
            };
            foreach (var permCode in auditorPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["Auditor"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // EvidenceOfficer - Evidence upload/update + submit
        if (roles.ContainsKey("EvidenceOfficer"))
        {
            var evidencePermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Evidence.View", "Grc.Evidence.Upload", "Grc.Evidence.Update"
            };
            foreach (var permCode in evidencePermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["EvidenceOfficer"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // VendorManager - Vendors + Vendor Assessments
        if (roles.ContainsKey("VendorManager"))
        {
            var vendorPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Vendors.View", "Grc.Vendors.Manage", "Grc.Vendors.Assess"
            };
            foreach (var permCode in vendorPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["VendorManager"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // Viewer - View-only on everything (no Export)
        if (roles.ContainsKey("Viewer"))
        {
            var viewerPermissions = permissions.Values
                .Where(p => p.Code.EndsWith(".View") || p.Code == "Grc.Home" || p.Code == "Grc.Dashboard")
                .Select(p => p.Code);
            foreach (var permCode in viewerPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["Viewer"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // ComplianceOfficer - Operational compliance tasks
        if (roles.ContainsKey("ComplianceOfficer"))
        {
            var complianceOfficerPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Assessments.View", "Grc.Assessments.Create", "Grc.Assessments.Update", "Grc.Assessments.Submit",
                "Grc.Evidence.View", "Grc.Evidence.Upload", "Grc.Evidence.Update",
                "Grc.Policies.View"
            };
            foreach (var permCode in complianceOfficerPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["ComplianceOfficer"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // RiskAnalyst - Risk analysis and reporting
        if (roles.ContainsKey("RiskAnalyst"))
        {
            var riskAnalystPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Risks.View", "Grc.Risks.Manage",
                "Grc.Reports.View", "Grc.Reports.Export"
            };
            foreach (var permCode in riskAnalystPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["RiskAnalyst"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // PolicyManager - Policy management
        if (roles.ContainsKey("PolicyManager"))
        {
            var policyManagerPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Policies.View", "Grc.Policies.Manage", "Grc.Policies.Approve", "Grc.Policies.Publish"
            };
            foreach (var permCode in policyManagerPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["PolicyManager"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // WorkflowManager - Workflow management
        if (roles.ContainsKey("WorkflowManager"))
        {
            var workflowManagerPermissions = new[] {
                "Grc.Home", "Grc.Dashboard",
                "Grc.Workflow.View", "Grc.Workflow.Manage"
            };
            foreach (var permCode in workflowManagerPermissions)
            {
                if (permissions.ContainsKey(permCode))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = roles["WorkflowManager"].Id,
                        PermissionId = permissions[permCode].Id,
                        TenantId = tenantId,
                        AssignedBy = "System"
                    });
                }
            }
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Mapped {rolePermissions.Count} role-permission relationships");
    }

    private static async Task MapRolesToFeaturesAsync(
        GrcDbContext context,
        Dictionary<string, IdentityRole> roles,
        Dictionary<string, Feature> features,
        Guid tenantId,
        ILogger logger)
    {
        logger.LogInformation("Mapping roles to features (view authority)...");

        if (await context.RoleFeatures.AnyAsync(rf => rf.TenantId == tenantId))
        {
            logger.LogInformation("Role-Feature mappings already exist. Skipping.");
            return;
        }

        var roleFeatures = new List<RoleFeature>();

        // PlatformAdmin - All features visible
        if (roles.ContainsKey("PlatformAdmin"))
        {
            foreach (var feature in features.Values)
            {
                roleFeatures.Add(new RoleFeature
                {
                    RoleId = roles["PlatformAdmin"].Id,
                    FeatureId = feature.Id,
                    TenantId = tenantId,
                    IsVisible = true,
                    AssignedBy = "System"
                });
            }
        }

        // TenantAdmin - Admin, Subscriptions, Integrations, Dashboard
        if (roles.ContainsKey("TenantAdmin"))
        {
            var tenantAdminFeatures = new[] { "Home", "Dashboard", "Subscriptions", "Admin", "Integrations" };
            foreach (var featureCode in tenantAdminFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["TenantAdmin"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // AppUserAdmin - Admin section only
        if (roles.ContainsKey("AppUserAdmin"))
        {
            var appUserAdminFeatures = new[] { "Home", "Dashboard", "Admin" };
            foreach (var featureCode in appUserAdminFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["AppUserAdmin"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // User - Home and Dashboard only
        if (roles.ContainsKey("User"))
        {
            var userFeatures = new[] { "Home", "Dashboard" };
            foreach (var featureCode in userFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["User"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // ComplianceManager - Frameworks, Regulators, Assessments, Evidence, Policies, Calendar, Workflow, Reports
        if (roles.ContainsKey("ComplianceManager"))
        {
            var complianceFeatures = new[] {
                "Home", "Dashboard", "Frameworks", "Regulators", "Assessments", "ControlAssessments",
                "Evidence", "Policies", "ComplianceCalendar", "Workflow", "Reports"
            };
            foreach (var featureCode in complianceFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["ComplianceManager"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // RiskManager - Risks, ActionPlans, Reports
        if (roles.ContainsKey("RiskManager"))
        {
            var riskFeatures = new[] { "Home", "Dashboard", "Risks", "ActionPlans", "Reports" };
            foreach (var featureCode in riskFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["RiskManager"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // Auditor - Audits, Evidence (view), Assessments (view), Reports
        if (roles.ContainsKey("Auditor"))
        {
            var auditorFeatures = new[] { "Home", "Dashboard", "Audits", "Evidence", "Assessments", "Reports" };
            foreach (var featureCode in auditorFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["Auditor"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // EvidenceOfficer - Evidence, Dashboard
        if (roles.ContainsKey("EvidenceOfficer"))
        {
            var evidenceFeatures = new[] { "Home", "Dashboard", "Evidence" };
            foreach (var featureCode in evidenceFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["EvidenceOfficer"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // VendorManager - Vendors, Reports
        if (roles.ContainsKey("VendorManager"))
        {
            var vendorFeatures = new[] { "Home", "Dashboard", "Vendors", "Reports" };
            foreach (var featureCode in vendorFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["VendorManager"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // Viewer - All features visible (read-only)
        if (roles.ContainsKey("Viewer"))
        {
            foreach (var feature in features.Values)
            {
                roleFeatures.Add(new RoleFeature
                {
                    RoleId = roles["Viewer"].Id,
                    FeatureId = feature.Id,
                    TenantId = tenantId,
                    IsVisible = true,
                    AssignedBy = "System"
                });
            }
        }

        // ComplianceOfficer - Assessments, Evidence, Policies, Dashboard
        if (roles.ContainsKey("ComplianceOfficer"))
        {
            var complianceOfficerFeatures = new[] { "Home", "Dashboard", "Assessments", "Evidence", "Policies" };
            foreach (var featureCode in complianceOfficerFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["ComplianceOfficer"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // RiskAnalyst - Risks, Reports, Dashboard
        if (roles.ContainsKey("RiskAnalyst"))
        {
            var riskAnalystFeatures = new[] { "Home", "Dashboard", "Risks", "Reports" };
            foreach (var featureCode in riskAnalystFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["RiskAnalyst"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // PolicyManager - Policies, Dashboard
        if (roles.ContainsKey("PolicyManager"))
        {
            var policyManagerFeatures = new[] { "Home", "Dashboard", "Policies" };
            foreach (var featureCode in policyManagerFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["PolicyManager"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        // WorkflowManager - Workflow, Dashboard
        if (roles.ContainsKey("WorkflowManager"))
        {
            var workflowManagerFeatures = new[] { "Home", "Dashboard", "Workflow" };
            foreach (var featureCode in workflowManagerFeatures)
            {
                if (features.ContainsKey(featureCode))
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roles["WorkflowManager"].Id,
                        FeatureId = features[featureCode].Id,
                        TenantId = tenantId,
                        IsVisible = true,
                        AssignedBy = "System"
                    });
                }
            }
        }

        await context.RoleFeatures.AddRangeAsync(roleFeatures);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Mapped {roleFeatures.Count} role-feature relationships (view authority)");
    }
}
