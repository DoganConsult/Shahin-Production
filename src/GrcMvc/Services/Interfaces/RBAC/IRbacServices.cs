using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces.RBAC
{
    /// <summary>
    /// Service for managing permissions and roles
    /// </summary>
    public interface IPermissionService
    {
        // Permission CRUD
        Task<Permission> CreatePermissionAsync(string code, string name, string description, string category);
        Task<Permission> GetPermissionAsync(int id);
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<Permission>> GetPermissionsByCategoryAsync(string category);
        Task<bool> UpdatePermissionAsync(int id, string name, string description, bool isActive);
        Task<bool> DeletePermissionAsync(int id);

        // Role-Permission Management
        Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId, Guid tenantId, string assignedBy);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId, Guid tenantId);
        Task<List<Permission>> GetRolePermissionsAsync(string roleId, Guid tenantId);
        Task<bool> HasPermissionAsync(string userId, string permissionCode, Guid tenantId);
        Task<List<string>> GetUserPermissionCodesAsync(string userId, Guid tenantId);
    }

    /// <summary>
    /// Service for managing features and feature visibility
    /// </summary>
    public interface IFeatureService
    {
        // Feature CRUD
        Task<Feature> CreateFeatureAsync(string code, string name, string description, string category, int? displayOrder);
        Task<Feature> GetFeatureAsync(int id);
        Task<List<Feature>> GetAllFeaturesAsync();
        Task<List<Feature>> GetFeaturesByCategoryAsync(string category);
        Task<bool> UpdateFeatureAsync(int id, string name, string description, bool isActive, int? displayOrder);
        Task<bool> DeleteFeatureAsync(int id);

        // Feature-Permission Linking
        Task<bool> LinkFeatureToPermissionAsync(int featureId, int permissionId, bool isRequired);
        Task<bool> UnlinkFeatureFromPermissionAsync(int featureId, int permissionId);
        Task<List<Permission>> GetFeatureRequiredPermissionsAsync(int featureId);

        // Role-Feature Management
        Task<bool> AssignFeatureToRoleAsync(string roleId, int featureId, Guid tenantId, string assignedBy);
        Task<bool> RemoveFeatureFromRoleAsync(string roleId, int featureId, Guid tenantId);
        Task<List<Feature>> GetRoleFeaturesAsync(string roleId, Guid tenantId);
        Task<List<Feature>> GetUserVisibleFeaturesAsync(string userId, Guid tenantId);
    }

    /// <summary>
    /// Service for managing role configurations per tenant
    /// </summary>
    public interface ITenantRoleConfigurationService
    {
        Task<TenantRoleConfiguration> CreateRoleConfigurationAsync(Guid tenantId, string roleId, string description, int? maxUsers);
        Task<TenantRoleConfiguration> GetRoleConfigurationAsync(Guid tenantId, string roleId);
        Task<List<TenantRoleConfiguration>> GetTenantRoleConfigurationsAsync(Guid tenantId);
        Task<bool> UpdateRoleConfigurationAsync(int id, string description, int? maxUsers, bool canBeModified);
        Task<bool> DeleteRoleConfigurationAsync(int id);
        Task<bool> CanAssignRoleAsync(Guid tenantId, string roleId); // Check if tenant can assign this role
    }

    /// <summary>
    /// Service for user role assignments per tenant
    /// </summary>
    public interface IUserRoleAssignmentService
    {
        Task<UserRoleAssignment> AssignRoleToUserAsync(string userId, string roleId, Guid tenantId, string assignedBy, DateTime? expiresAt = null);
        Task<UserRoleAssignment> GetUserRoleAssignmentAsync(string userId, Guid tenantId);
        Task<List<UserRoleAssignment>> GetUserRolesAsync(string userId);
        Task<List<UserRoleAssignment>> GetRoleUsersAsync(string roleId, Guid tenantId);
        Task<bool> RemoveUserRoleAsync(string userId, Guid tenantId);
        Task<bool> UpdateUserRoleAsync(int assignmentId, string newRoleId, DateTime? expiresAt);
        Task<bool> IsRoleExpiredAsync(int assignmentId);
        Task<List<ApplicationUser>> GetUsersWithRoleAsync(string roleId, Guid tenantId);
    }

    /// <summary>
    /// Service for checking access and authorizing actions
    /// </summary>
    public interface IAccessControlService
    {
        // Permission checks
        Task<bool> CanUserPerformActionAsync(string userId, string permissionCode, Guid tenantId);
        Task<bool> CanUserViewFeatureAsync(string userId, int featureId, Guid tenantId);
        Task<List<string>> GetUserPermissionsAsync(string userId, Guid tenantId);
        Task<List<Feature>> GetUserAccessibleFeaturesAsync(string userId, Guid tenantId);

        // Workflow-specific checks
        Task<bool> CanUserApproveWorkflowAsync(string userId, Guid workflowInstanceId);
        Task<bool> CanUserAssignTaskAsync(string userId, string targetUserId, Guid tenantId);
        Task<bool> CanUserEscalateAsync(string userId, Guid tenantId);
        Task<bool> CanUserReviewEvidenceAsync(string userId, Guid tenantId);
        Task<bool> CanUserAuditAsync(string userId, Guid tenantId);

        // Feature visibility
        Task<Dictionary<string, bool>> GetFeatureVisibilityAsync(string userId, Guid tenantId);
    }

    /// <summary>
    /// Service for bulk operations and seeding
    /// </summary>
    public interface IRbacSeederService
    {
        Task SeedDefaultPermissionsAsync();
        Task SeedDefaultFeaturesAsync();
        Task SeedDefaultFeaturePermissionMappingsAsync();
        Task ConfigureRolePermissionsAsync(string roleId, List<string> permissionCodes, Guid tenantId);
        Task ConfigureRoleFeaturesAsync(string roleId, List<string> featureCodes, Guid tenantId);
        Task SeedTenantRoleConfigurationsAsync(Guid tenantId);
    }
}
