using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Implementations.RBAC
{
    public class RbacSeederService : IRbacSeederService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<RbacSeederService> _logger;

        public RbacSeederService(GrcDbContext context, ILogger<RbacSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedDefaultPermissionsAsync()
        {
            // Workflow Permissions
            var workflowPermissions = new[]
            {
                ("Workflow.View", "View Workflows", "View workflow instances"),
                ("Workflow.Create", "Create Workflows", "Create new workflows"),
                ("Workflow.Edit", "Edit Workflows", "Edit workflow configurations"),
                ("Workflow.Delete", "Delete Workflows", "Delete workflows"),
                ("Workflow.Approve", "Approve Workflows", "Approve pending workflows"),
                ("Workflow.Reject", "Reject Workflows", "Reject workflows"),
                ("Workflow.AssignTask", "Assign Tasks", "Assign workflow tasks to users"),
                ("Workflow.Escalate", "Escalate Tasks", "Escalate overdue tasks"),
                ("Workflow.Monitor", "Monitor Workflows", "Monitor workflow status and metrics"),
            };

            // Control Permissions
            var controlPermissions = new[]
            {
                ("Control.View", "View Controls", "View control details"),
                ("Control.Create", "Create Controls", "Create new controls"),
                ("Control.Edit", "Edit Controls", "Edit control configurations"),
                ("Control.Delete", "Delete Controls", "Delete controls"),
                ("Control.Implement", "Implement Controls", "Implement controls"),
                ("Control.Test", "Test Controls", "Test compliance of controls"),
            };

            // Evidence Permissions
            var evidencePermissions = new[]
            {
                ("Evidence.View", "View Evidence", "View evidence submissions"),
                ("Evidence.Submit", "Submit Evidence", "Submit control evidence"),
                ("Evidence.Review", "Review Evidence", "Review submitted evidence"),
                ("Evidence.Approve", "Approve Evidence", "Approve evidence"),
                ("Evidence.Archive", "Archive Evidence", "Archive evidence"),
            };

            // Risk Permissions
            var riskPermissions = new[]
            {
                ("Risk.View", "View Risks", "View risk assessments"),
                ("Risk.Create", "Create Risks", "Identify new risks"),
                ("Risk.Edit", "Edit Risks", "Edit risk assessments"),
                ("Risk.Approve", "Approve Risks", "Approve risk assessments"),
                ("Risk.Monitor", "Monitor Risks", "Monitor ongoing risks"),
            };

            // Audit Permissions
            var auditPermissions = new[]
            {
                ("Audit.View", "View Audits", "View audit information"),
                ("Audit.Create", "Create Audits", "Create new audits"),
                ("Audit.Fieldwork", "Conduct Fieldwork", "Perform audit fieldwork"),
                ("Audit.Report", "Issue Reports", "Issue audit reports"),
            };

            // Policy Permissions
            var policyPermissions = new[]
            {
                ("Policy.View", "View Policies", "View policy documents"),
                ("Policy.Create", "Create Policies", "Create new policies"),
                ("Policy.Review", "Review Policies", "Review policies for updates"),
                ("Policy.Approve", "Approve Policies", "Approve policy changes"),
                ("Policy.Publish", "Publish Policies", "Publish policies to users"),
            };

            // User Management Permissions (Admin/TenantAdmin)
            var adminPermissions = new[]
            {
                ("User.View", "View Users", "View user accounts"),
                ("User.Create", "Create Users", "Create new users"),
                ("User.Edit", "Edit Users", "Edit user details"),
                ("User.Delete", "Delete Users", "Delete users"),
                ("User.AssignRole", "Assign Roles", "Assign roles to users"),
                ("Role.View", "View Roles", "View role configurations"),
                ("Role.Edit", "Edit Roles", "Edit role permissions"),
                ("Permission.Manage", "Manage Permissions", "Manage system permissions"),
                ("Feature.Manage", "Manage Features", "Manage feature visibility"),
            };

            // Reporting Permissions
            var reportingPermissions = new[]
            {
                ("Report.View", "View Reports", "View compliance reports"),
                ("Report.Generate", "Generate Reports", "Generate custom reports"),
                ("Report.Export", "Export Reports", "Export reports to files"),
            };

            var allPermissions = new[] { workflowPermissions, controlPermissions, evidencePermissions,
                riskPermissions, auditPermissions, policyPermissions, adminPermissions, reportingPermissions };

            var categories = new[] { "Workflow", "Control", "Evidence", "Risk", "Audit", "Policy", "Admin", "Reporting" };

            int categoryIndex = 0;
            foreach (var permissionGroup in allPermissions)
            {
                foreach (var (code, name, description) in permissionGroup)
                {
                    var exists = await _context.Permissions.AnyAsync(p => p.Code == code);
                    if (!exists)
                    {
                        var permission = new Permission
                        {
                            Code = code,
                            Name = name,
                            Description = description,
                            Category = categories[categoryIndex],
                            IsActive = true
                        };
                        _context.Permissions.Add(permission);
                    }
                }
                categoryIndex++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default permissions seeded");
        }

        public async Task SeedDefaultFeaturesAsync()
        {
            var features = new[]
            {
                ("Workflows", "Workflow Management", "Manage compliance workflows", "GRC", 1),
                ("Controls", "Control Management", "Manage security controls", "GRC", 2),
                ("Evidence", "Evidence Management", "Collect and manage control evidence", "Compliance", 3),
                ("Risks", "Risk Management", "Assess and manage organizational risks", "GRC", 4),
                ("Audits", "Audit Management", "Plan and execute compliance audits", "Audit", 5),
                ("Policies", "Policy Management", "Create and manage compliance policies", "Compliance", 6),
                ("Users", "User Management", "Manage user accounts and assignments", "Admin", 7),
                ("Roles", "Role Management", "Configure roles and permissions", "Admin", 8),
                ("Reports", "Reporting", "Generate compliance and audit reports", "Reporting", 9),
                ("Dashboard", "Compliance Dashboard", "View compliance metrics and KPIs", "Dashboard", 10),
                ("Training", "Training Management", "Manage compliance training", "Compliance", 11),
                ("Exceptions", "Exception Management", "Handle policy exceptions and waivers", "Compliance", 12),
            };

            foreach (var (code, name, description, category, displayOrder) in features)
            {
                var exists = await _context.Features.AnyAsync(f => f.Code == code);
                if (!exists)
                {
                    var feature = new Feature
                    {
                        Code = code,
                        Name = name,
                        Description = description,
                        Category = category,
                        DisplayOrder = displayOrder,
                        IsActive = true
                    };
                    _context.Features.Add(feature);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default features seeded");
        }

        public async Task SeedDefaultFeaturePermissionMappingsAsync()
        {
            // Workflow Feature requires these permissions
            var workflows = await _context.Features.FirstOrDefaultAsync(f => f.Code == "Workflows");
            var workflowPermissions = await _context.Permissions
                .Where(p => p.Category == "Workflow")
                .ToListAsync();

            foreach (var permission in workflowPermissions)
            {
                var exists = await _context.FeaturePermissions
                    .AnyAsync(fp => fp.FeatureId == workflows.Id && fp.PermissionId == permission.Id);

                if (!exists)
                {
                    _context.FeaturePermissions.Add(new FeaturePermission
                    {
                        FeatureId = workflows.Id,
                        PermissionId = permission.Id,
                        IsRequired = permission.Code == "Workflow.View" // View is required
                    });
                }
            }

            // Controls Feature
            var controls = await _context.Features.FirstOrDefaultAsync(f => f.Code == "Controls");
            var controlPermissions = await _context.Permissions
                .Where(p => p.Category == "Control")
                .ToListAsync();

            foreach (var permission in controlPermissions)
            {
                var exists = await _context.FeaturePermissions
                    .AnyAsync(fp => fp.FeatureId == controls.Id && fp.PermissionId == permission.Id);

                if (!exists)
                {
                    _context.FeaturePermissions.Add(new FeaturePermission
                    {
                        FeatureId = controls.Id,
                        PermissionId = permission.Id,
                        IsRequired = permission.Code == "Control.View"
                    });
                }
            }

            // Evidence Feature
            var evidence = await _context.Features.FirstOrDefaultAsync(f => f.Code == "Evidence");
            var evidencePermissions = await _context.Permissions
                .Where(p => p.Category == "Evidence")
                .ToListAsync();

            foreach (var permission in evidencePermissions)
            {
                var exists = await _context.FeaturePermissions
                    .AnyAsync(fp => fp.FeatureId == evidence.Id && fp.PermissionId == permission.Id);

                if (!exists)
                {
                    _context.FeaturePermissions.Add(new FeaturePermission
                    {
                        FeatureId = evidence.Id,
                        PermissionId = permission.Id,
                        IsRequired = permission.Code == "Evidence.View"
                    });
                }
            }

            // Similar for other features...
            await _context.SaveChangesAsync();
            _logger.LogInformation("Feature-permission mappings seeded");
        }

        public async Task ConfigureRolePermissionsAsync(string roleId, List<string> permissionCodes, Guid tenantId)
        {
            var permissions = await _context.Permissions
                .Where(p => permissionCodes.Contains(p.Code))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                var exists = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id && rp.TenantId == tenantId);

                if (!exists)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permission.Id,
                        TenantId = tenantId,
                        AssignedBy = "system"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ConfigureRoleFeaturesAsync(string roleId, List<string> featureCodes, Guid tenantId)
        {
            var features = await _context.Features
                .Where(f => featureCodes.Contains(f.Code))
                .ToListAsync();

            foreach (var feature in features)
            {
                var exists = await _context.RoleFeatures
                    .AnyAsync(rf => rf.RoleId == roleId && rf.FeatureId == feature.Id && rf.TenantId == tenantId);

                if (!exists)
                {
                    _context.RoleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        FeatureId = feature.Id,
                        TenantId = tenantId,
                        AssignedBy = "system",
                        IsVisible = true
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task SeedTenantRoleConfigurationsAsync(Guid tenantId)
        {
            var roles = new[] { "Admin", "ComplianceOfficer", "RiskManager", "Auditor", "User" };

            foreach (var roleId in roles)
            {
                var exists = await _context.TenantRoleConfigurations
                    .AnyAsync(trc => trc.TenantId == tenantId && trc.RoleId == roleId);

                if (!exists)
                {
                    var config = new TenantRoleConfiguration
                    {
                        TenantId = tenantId,
                        RoleId = roleId,
                        Description = $"{roleId} role for tenant",
                        MaxUsersWithRole = roleId == "Admin" ? 5 : null,
                        CanBeModified = roleId != "Admin"
                    };

                    _context.TenantRoleConfigurations.Add(config);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
