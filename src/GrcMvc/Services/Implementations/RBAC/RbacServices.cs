using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.EntityFrameworkCore;
using RbacIPermissionService = GrcMvc.Services.Interfaces.RBAC.IPermissionService;

namespace GrcMvc.Services.Implementations.RBAC
{
    public class PermissionService : RbacIPermissionService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(GrcDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Permission> CreatePermissionAsync(string code, string name, string description, string category)
        {
            var permission = new Permission
            {
                Code = code,
                Name = name,
                Description = description,
                Category = category,
                IsActive = true
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Permission created: {code}");
            return permission;
        }

        public async Task<Permission> GetPermissionAsync(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.Where(p => p.IsActive).OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
        }

        public async Task<List<Permission>> GetPermissionsByCategoryAsync(string category)
        {
            return await _context.Permissions.Where(p => p.Category == category && p.IsActive).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<bool> UpdatePermissionAsync(int id, string name, string description, bool isActive)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return false;

            permission.Name = name;
            permission.Description = description;
            permission.IsActive = isActive;

            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return false;

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId, Guid tenantId, string assignedBy)
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId);

            if (existing != null) return true; // Already assigned

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                TenantId = tenantId,
                AssignedBy = assignedBy
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Permission {permissionId} assigned to role {roleId} in tenant {tenantId}");
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId, Guid tenantId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId);

            if (rolePermission == null) return false;

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Permission>> GetRolePermissionsAsync(string roleId, Guid tenantId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionCode, Guid tenantId)
        {
            var userRole = await _context.UserRoleAssignments
                .FirstOrDefaultAsync(ura => ura.UserId == userId && ura.TenantId == tenantId && ura.IsActive);

            if (userRole == null) return false;

            var hasPermission = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == userRole.RoleId &&
                                rp.TenantId == tenantId &&
                                rp.Permission.Code == permissionCode &&
                                rp.Permission.IsActive);

            return hasPermission;
        }

        public async Task<List<string>> GetUserPermissionCodesAsync(string userId, Guid tenantId)
        {
            var userRole = await _context.UserRoleAssignments
                .FirstOrDefaultAsync(ura => ura.UserId == userId && ura.TenantId == tenantId && ura.IsActive);

            if (userRole == null) return new List<string>();

            return await _context.RolePermissions
                .Where(rp => rp.RoleId == userRole.RoleId && rp.TenantId == tenantId && rp.Permission.IsActive)
                .Select(rp => rp.Permission.Code)
                .OrderBy(c => c)
                .ToListAsync();
        }
    }

    public class FeatureService : IFeatureService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<FeatureService> _logger;

        public FeatureService(GrcDbContext context, ILogger<FeatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Feature> CreateFeatureAsync(string code, string name, string description, string category, int? displayOrder)
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
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Feature created: {code}");
            return feature;
        }

        public async Task<Feature> GetFeatureAsync(int id)
        {
            return await _context.Features.FindAsync(id);
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await _context.Features.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ThenBy(f => f.Name).ToListAsync();
        }

        public async Task<List<Feature>> GetFeaturesByCategoryAsync(string category)
        {
            return await _context.Features.Where(f => f.Category == category && f.IsActive).OrderBy(f => f.DisplayOrder).ThenBy(f => f.Name).ToListAsync();
        }

        public async Task<bool> UpdateFeatureAsync(int id, string name, string description, bool isActive, int? displayOrder)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null) return false;

            feature.Name = name;
            feature.Description = description;
            feature.IsActive = isActive;
            feature.DisplayOrder = displayOrder;

            _context.Features.Update(feature);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFeatureAsync(int id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null) return false;

            _context.Features.Remove(feature);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LinkFeatureToPermissionAsync(int featureId, int permissionId, bool isRequired)
        {
            var existing = await _context.FeaturePermissions
                .FirstOrDefaultAsync(fp => fp.FeatureId == featureId && fp.PermissionId == permissionId);

            if (existing != null) return true;

            var featurePermission = new FeaturePermission
            {
                FeatureId = featureId,
                PermissionId = permissionId,
                IsRequired = isRequired
            };

            _context.FeaturePermissions.Add(featurePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlinkFeatureFromPermissionAsync(int featureId, int permissionId)
        {
            var featurePermission = await _context.FeaturePermissions
                .FirstOrDefaultAsync(fp => fp.FeatureId == featureId && fp.PermissionId == permissionId);

            if (featurePermission == null) return false;

            _context.FeaturePermissions.Remove(featurePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Permission>> GetFeatureRequiredPermissionsAsync(int featureId)
        {
            return await _context.FeaturePermissions
                .Where(fp => fp.FeatureId == featureId && fp.IsRequired)
                .Include(fp => fp.Permission)
                .Select(fp => fp.Permission)
                .ToListAsync();
        }

        public async Task<bool> AssignFeatureToRoleAsync(string roleId, int featureId, Guid tenantId, string assignedBy)
        {
            var existing = await _context.RoleFeatures
                .FirstOrDefaultAsync(rf => rf.RoleId == roleId && rf.FeatureId == featureId && rf.TenantId == tenantId);

            if (existing != null) return true;

            var roleFeature = new RoleFeature
            {
                RoleId = roleId,
                FeatureId = featureId,
                TenantId = tenantId,
                AssignedBy = assignedBy,
                IsVisible = true
            };

            _context.RoleFeatures.Add(roleFeature);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFeatureFromRoleAsync(string roleId, int featureId, Guid tenantId)
        {
            var roleFeature = await _context.RoleFeatures
                .FirstOrDefaultAsync(rf => rf.RoleId == roleId && rf.FeatureId == featureId && rf.TenantId == tenantId);

            if (roleFeature == null) return false;

            _context.RoleFeatures.Remove(roleFeature);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Feature>> GetRoleFeaturesAsync(string roleId, Guid tenantId)
        {
            return await _context.RoleFeatures
                .Where(rf => rf.RoleId == roleId && rf.TenantId == tenantId && rf.IsVisible)
                .Include(rf => rf.Feature)
                .Select(rf => rf.Feature)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<List<Feature>> GetUserVisibleFeaturesAsync(string userId, Guid tenantId)
        {
            var userRole = await _context.UserRoleAssignments
                .FirstOrDefaultAsync(ura => ura.UserId == userId && ura.TenantId == tenantId && ura.IsActive);

            if (userRole == null) return new List<Feature>();

            return await GetRoleFeaturesAsync(userRole.RoleId, tenantId);
        }
    }

    public class TenantRoleConfigurationService : ITenantRoleConfigurationService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<TenantRoleConfigurationService> _logger;

        public TenantRoleConfigurationService(GrcDbContext context, ILogger<TenantRoleConfigurationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TenantRoleConfiguration> CreateRoleConfigurationAsync(Guid tenantId, string roleId, string description, int? maxUsers)
        {
            var config = new TenantRoleConfiguration
            {
                TenantId = tenantId,
                RoleId = roleId,
                Description = description,
                MaxUsersWithRole = maxUsers,
                CanBeModified = true
            };

            _context.TenantRoleConfigurations.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task<TenantRoleConfiguration> GetRoleConfigurationAsync(Guid tenantId, string roleId)
        {
            return await _context.TenantRoleConfigurations
                .FirstOrDefaultAsync(trc => trc.TenantId == tenantId && trc.RoleId == roleId);
        }

        public async Task<List<TenantRoleConfiguration>> GetTenantRoleConfigurationsAsync(Guid tenantId)
        {
            return await _context.TenantRoleConfigurations
                .Where(trc => trc.TenantId == tenantId)
                .OrderBy(trc => trc.RoleId)
                .ToListAsync();
        }

        public async Task<bool> UpdateRoleConfigurationAsync(int id, string description, int? maxUsers, bool canBeModified)
        {
            var config = await _context.TenantRoleConfigurations.FindAsync(id);
            if (config == null) return false;

            config.Description = description;
            config.MaxUsersWithRole = maxUsers;
            config.CanBeModified = canBeModified;

            _context.TenantRoleConfigurations.Update(config);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoleConfigurationAsync(int id)
        {
            var config = await _context.TenantRoleConfigurations.FindAsync(id);
            if (config == null) return false;

            _context.TenantRoleConfigurations.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanAssignRoleAsync(Guid tenantId, string roleId)
        {
            var config = await GetRoleConfigurationAsync(tenantId, roleId);
            if (config == null) return false; // Role not configured for this tenant

            if (!config.CanBeModified) return false; // System role

            if (config.MaxUsersWithRole.HasValue)
            {
                var userCount = await _context.UserRoleAssignments
                    .CountAsync(ura => ura.TenantId == tenantId && ura.RoleId == roleId && ura.IsActive);

                return userCount < config.MaxUsersWithRole.Value;
            }

            return true;
        }
    }

    public class UserRoleAssignmentService : IUserRoleAssignmentService
    {
        private readonly GrcDbContext _context;
        private readonly IUserDirectoryService _userDirectory;
        private readonly ILogger<UserRoleAssignmentService> _logger;

        public UserRoleAssignmentService(GrcDbContext context, IUserDirectoryService userDirectory, ILogger<UserRoleAssignmentService> logger)
        {
            _context = context;
            _userDirectory = userDirectory;
            _logger = logger;
        }

        public async Task<UserRoleAssignment> AssignRoleToUserAsync(string userId, string roleId, Guid tenantId, string assignedBy, DateTime? expiresAt = null)
        {
            var assignment = new UserRoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                TenantId = tenantId,
                AssignedBy = assignedBy,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            _context.UserRoleAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Role {roleId} assigned to user {userId} in tenant {tenantId}");
            return assignment;
        }

        public async Task<UserRoleAssignment> GetUserRoleAssignmentAsync(string userId, Guid tenantId)
        {
            return await _context.UserRoleAssignments
                .FirstOrDefaultAsync(ura => ura.UserId == userId && ura.TenantId == tenantId && ura.IsActive);
        }

        public async Task<List<UserRoleAssignment>> GetUserRolesAsync(string userId)
        {
            // Note: Role navigation removed - use IUserDirectoryService.GetRolesByIdsAsync for role details
            return await _context.UserRoleAssignments
                .Where(ura => ura.UserId == userId && ura.IsActive)
                .Include(ura => ura.Tenant)
                .ToListAsync();
        }

        public async Task<List<UserRoleAssignment>> GetRoleUsersAsync(string roleId, Guid tenantId)
        {
            // Note: User navigation removed - use IUserDirectoryService.GetUsersByIdsAsync for user details
            return await _context.UserRoleAssignments
                .Where(ura => ura.RoleId == roleId && ura.TenantId == tenantId && ura.IsActive)
                .ToListAsync();
        }

        public async Task<bool> RemoveUserRoleAsync(string userId, Guid tenantId)
        {
            var assignment = await _context.UserRoleAssignments
                .FirstOrDefaultAsync(ura => ura.UserId == userId && ura.TenantId == tenantId && ura.IsActive);

            if (assignment == null) return false;

            assignment.IsActive = false;
            _context.UserRoleAssignments.Update(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int assignmentId, string newRoleId, DateTime? expiresAt)
        {
            var assignment = await _context.UserRoleAssignments.FindAsync(assignmentId);
            if (assignment == null) return false;

            assignment.RoleId = newRoleId;
            assignment.ExpiresAt = expiresAt;

            _context.UserRoleAssignments.Update(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsRoleExpiredAsync(int assignmentId)
        {
            var assignment = await _context.UserRoleAssignments.FindAsync(assignmentId);
            if (assignment == null || !assignment.ExpiresAt.HasValue) return false;

            return assignment.ExpiresAt.Value < DateTime.UtcNow;
        }

        public async Task<List<ApplicationUser>> GetUsersWithRoleAsync(string roleId, Guid tenantId)
        {
            // Get user IDs from role assignments, then batch lookup users from auth DB
            var userIds = await _context.UserRoleAssignments
                .Where(ura => ura.RoleId == roleId && ura.TenantId == tenantId && ura.IsActive)
                .Select(ura => ura.UserId)
                .ToListAsync();

            var usersDict = await _userDirectory.GetUsersByIdsAsync(userIds);
            return usersDict.Values
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();
        }
    }

    public class AccessControlService : IAccessControlService
    {
        private readonly RbacIPermissionService _permissionService;
        private readonly IFeatureService _featureService;
        private readonly GrcDbContext _context;
        private readonly ILogger<AccessControlService> _logger;

        public AccessControlService(RbacIPermissionService permissionService, IFeatureService featureService,
            GrcDbContext context, ILogger<AccessControlService> logger)
        {
            _permissionService = permissionService;
            _featureService = featureService;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CanUserPerformActionAsync(string userId, string permissionCode, Guid tenantId)
        {
            return await _permissionService.HasPermissionAsync(userId, permissionCode, tenantId);
        }

        public async Task<bool> CanUserViewFeatureAsync(string userId, int featureId, Guid tenantId)
        {
            var features = await _featureService.GetUserVisibleFeaturesAsync(userId, tenantId);
            return features.Any(f => f.Id == featureId);
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, Guid tenantId)
        {
            return await _permissionService.GetUserPermissionCodesAsync(userId, tenantId);
        }

        public async Task<List<Feature>> GetUserAccessibleFeaturesAsync(string userId, Guid tenantId)
        {
            return await _featureService.GetUserVisibleFeaturesAsync(userId, tenantId);
        }

        public async Task<bool> CanUserApproveWorkflowAsync(string userId, Guid workflowInstanceId)
        {
            // Check if user has approval permissions
            var userRoles = await _context.UserRoleAssignments
                .Where(ura => ura.UserId == userId && ura.IsActive)
                .Select(ura => ura.RoleId)
                .ToListAsync();

            foreach (var roleId in userRoles)
            {
                var hasApprovalPermission = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Code.Contains("Approve"));

                if (hasApprovalPermission) return true;
            }

            return false;
        }

        public async Task<bool> CanUserAssignTaskAsync(string userId, string targetUserId, Guid tenantId)
        {
            return await _permissionService.HasPermissionAsync(userId, "Workflow.AssignTask", tenantId);
        }

        public async Task<bool> CanUserEscalateAsync(string userId, Guid tenantId)
        {
            return await _permissionService.HasPermissionAsync(userId, "Workflow.Escalate", tenantId);
        }

        public async Task<bool> CanUserReviewEvidenceAsync(string userId, Guid tenantId)
        {
            return await _permissionService.HasPermissionAsync(userId, "Evidence.Review", tenantId);
        }

        public async Task<bool> CanUserAuditAsync(string userId, Guid tenantId)
        {
            return await _permissionService.HasPermissionAsync(userId, "Audit.Create", tenantId);
        }

        public async Task<Dictionary<string, bool>> GetFeatureVisibilityAsync(string userId, Guid tenantId)
        {
            var features = await _featureService.GetUserVisibleFeaturesAsync(userId, tenantId);
            var allFeatures = await _featureService.GetAllFeaturesAsync();

            return allFeatures.ToDictionary(f => f.Code, f => features.Any(uf => uf.Id == f.Id));
        }
    }
}
