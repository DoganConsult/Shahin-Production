using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for accessing current user context information from HTTP context
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Get the current user's ID from claims
        /// </summary>
        public Guid GetUserId()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    // Try alternative claim types
                    userIdClaim = _httpContextAccessor.HttpContext?.User?
                        .FindFirst("sub")?.Value ??
                        _httpContextAccessor.HttpContext?.User?
                        .FindFirst("UserId")?.Value;
                }

                return Guid.TryParse(userIdClaim, out var userId)
                    ? userId
                    : Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user ID from context");
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Get the current user's username
        /// </summary>
        public string GetUserName()
        {
            try
            {
                var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    // Try alternative claim types
                    userName = _httpContextAccessor.HttpContext?.User?
                        .FindFirst(ClaimTypes.Name)?.Value ??
                        _httpContextAccessor.HttpContext?.User?
                        .FindFirst("name")?.Value ??
                        _httpContextAccessor.HttpContext?.User?
                        .FindFirst("preferred_username")?.Value;
                }

                return !string.IsNullOrEmpty(userName) ? userName : "System";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting username from context");
                return "System";
            }
        }

        /// <summary>
        /// Get the current user's email
        /// </summary>
        public string GetUserEmail()
        {
            try
            {
                var email = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    // Try alternative claim types
                    email = _httpContextAccessor.HttpContext?.User?
                        .FindFirst("email")?.Value ??
                        _httpContextAccessor.HttpContext?.User?
                        .FindFirst("upn")?.Value;
                }

                return !string.IsNullOrEmpty(email) ? email : "system@grc.local";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting email from context");
                return "system@grc.local";
            }
        }

        /// <summary>
        /// Get the current tenant ID
        /// </summary>
        public Guid GetTenantId()
        {
            try
            {
                var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst("TenantId")?.Value;

                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    // Try alternative claim types
                    tenantIdClaim = _httpContextAccessor.HttpContext?.User?
                        .FindFirst("tid")?.Value ??
                        _httpContextAccessor.HttpContext?.User?
                        .FindFirst("tenant_id")?.Value;
                }

                // If still no tenant ID, check headers (for API calls)
                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    tenantIdClaim = _httpContextAccessor.HttpContext?
                        .Request?.Headers["X-Tenant-Id"].FirstOrDefault();
                }

                // Default to empty GUID if no tenant context (single-tenant mode)
                return Guid.TryParse(tenantIdClaim, out var tenantId)
                    ? tenantId
                    : Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting tenant ID from context");
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Get the current user's roles
        /// </summary>
        public List<string> GetRoles()
        {
            try
            {
                var roles = new List<string>();

                // Get roles from standard claim type
                var roleClaims = _httpContextAccessor.HttpContext?.User?
                    .FindAll(ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (roleClaims != null && roleClaims.Any())
                {
                    roles.AddRange(roleClaims);
                }

                // Also check alternative claim types
                var altRoleClaims = _httpContextAccessor.HttpContext?.User?
                    .FindAll("role")
                    .Select(c => c.Value)
                    .ToList();

                if (altRoleClaims != null && altRoleClaims.Any())
                {
                    roles.AddRange(altRoleClaims);
                }

                // Check for roles in a single comma-separated claim
                var rolesClaimValue = _httpContextAccessor.HttpContext?.User?
                    .FindFirst("roles")?.Value;

                if (!string.IsNullOrEmpty(rolesClaimValue))
                {
                    roles.AddRange(rolesClaimValue.Split(',').Select(r => r.Trim()));
                }

                return roles.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting roles from context");
                return new List<string>();
            }
        }

        /// <summary>
        /// Check if the current user is in a specific role
        /// </summary>
        public bool IsInRole(string role)
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                    return false;

                // Use built-in IsInRole first
                if (_httpContextAccessor.HttpContext?.User?.IsInRole(role) == true)
                    return true;

                // Check our custom role retrieval
                var roles = GetRoles();
                return roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking user role");
                return false;
            }
        }

        /// <summary>
        /// Check if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking authentication status");
                return false;
            }
        }
    }
}