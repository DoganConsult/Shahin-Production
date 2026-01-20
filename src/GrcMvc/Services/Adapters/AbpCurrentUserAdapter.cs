using System;
using System.Collections.Generic;
using System.Linq;
using GrcMvc.Services.Interfaces;
using Volo.Abp.Users;

namespace GrcMvc.Services.Adapters
{
    /// <summary>
    /// Adapter to bridge ABP's ICurrentUser with custom ICurrentUserService interface
    /// </summary>
    public class AbpCurrentUserAdapter : ICurrentUserService
    {
        private readonly ICurrentUser _abpCurrentUser;

        public AbpCurrentUserAdapter(ICurrentUser abpCurrentUser)
        {
            _abpCurrentUser = abpCurrentUser ?? throw new ArgumentNullException(nameof(abpCurrentUser));
        }

        /// <summary>
        /// Get the current user's ID
        /// </summary>
        public Guid GetUserId()
        {
            return _abpCurrentUser.Id ?? Guid.Empty;
        }

        /// <summary>
        /// Get the current user's username
        /// </summary>
        public string GetUserName()
        {
            return _abpCurrentUser.UserName ?? "System";
        }

        /// <summary>
        /// Get the current user's email
        /// </summary>
        public string GetUserEmail()
        {
            return _abpCurrentUser.Email ?? "system@grc.local";
        }

        /// <summary>
        /// Get the current tenant ID
        /// </summary>
        public Guid GetTenantId()
        {
            return _abpCurrentUser.TenantId ?? Guid.Empty;
        }

        /// <summary>
        /// Get the current user's roles
        /// </summary>
        public List<string> GetRoles()
        {
            return _abpCurrentUser.Roles?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Check if the current user is in a specific role
        /// </summary>
        public bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            var roles = GetRoles();
            return roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return _abpCurrentUser.IsAuthenticated;
        }
    }
}
