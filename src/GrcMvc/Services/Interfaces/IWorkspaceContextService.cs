using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for resolving workspace context in multi-tenant operations.
    /// Workspaces are sub-scopes within a tenant (Market/BU/Entity).
    /// </summary>
    public interface IWorkspaceContextService
    {
        /// <summary>
        /// Gets the current workspace ID from session/context
        /// </summary>
        Guid GetCurrentWorkspaceId();

        /// <summary>
        /// Gets the current tenant ID (from TenantContextService)
        /// </summary>
        Guid GetCurrentTenantId();

        /// <summary>
        /// Gets all workspace IDs the current user has access to
        /// </summary>
        Task<IReadOnlyList<Guid>> GetUserWorkspaceIdsAsync();

        /// <summary>
        /// Sets the current workspace context (for workspace switcher)
        /// </summary>
        Task SetCurrentWorkspaceAsync(Guid workspaceId);

        /// <summary>
        /// Gets the default workspace for the current user
        /// </summary>
        Task<Guid?> GetDefaultWorkspaceIdAsync();

        /// <summary>
        /// Validates that the user has access to the specified workspace
        /// </summary>
        Task<bool> ValidateWorkspaceAccessAsync(Guid workspaceId);

        /// <summary>
        /// Gets the user's roles within the current workspace
        /// </summary>
        Task<IReadOnlyList<string>> GetCurrentWorkspaceRolesAsync();

        /// <summary>
        /// Returns true if workspace context is available
        /// </summary>
        bool HasWorkspaceContext();
    }
}
