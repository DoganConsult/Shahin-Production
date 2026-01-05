using System;
using System.Collections.Generic;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for accessing current user context information
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Get the current user's ID
        /// </summary>
        Guid GetUserId();

        /// <summary>
        /// Get the current user's username
        /// </summary>
        string GetUserName();

        /// <summary>
        /// Get the current user's email
        /// </summary>
        string GetUserEmail();

        /// <summary>
        /// Get the current tenant ID
        /// </summary>
        Guid GetTenantId();

        /// <summary>
        /// Get the current user's roles
        /// </summary>
        List<string> GetRoles();

        /// <summary>
        /// Check if the current user is in a specific role
        /// </summary>
        bool IsInRole(string role);

        /// <summary>
        /// Check if the current user is authenticated
        /// </summary>
        bool IsAuthenticated();
    }
}