namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Permission Service Interface
/// Handles permission checking and management
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permission">Permission name</param>
    /// <returns>True if user has permission</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission);

    /// <summary>
    /// Check if user has any of the specified permissions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">Permissions to check</param>
    /// <returns>True if user has any permission</returns>
    Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissions);

    /// <summary>
    /// Check if user has all specified permissions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">Permissions to check</param>
    /// <returns>True if user has all permissions</returns>
    Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissions);

    /// <summary>
    /// Get all permissions for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of permissions</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
}
