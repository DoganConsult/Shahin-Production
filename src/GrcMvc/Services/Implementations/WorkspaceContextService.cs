using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for resolving workspace context in multi-tenant operations.
    /// Caches workspace context per request to avoid repeated DB queries.
    /// </summary>
    public class WorkspaceContextService : IWorkspaceContextService
    {
        private readonly GrcDbContext _context;
        private readonly ITenantContextService _tenantContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WorkspaceContextService> _logger;

        private const string WorkspaceIdSessionKey = "CurrentWorkspaceId";
        private const string WorkspaceRolesCacheKey = "WorkspaceRoles";

        // Request-scoped cache
        private Guid? _cachedWorkspaceId;
        private IReadOnlyList<Guid>? _cachedWorkspaceIds;
        private IReadOnlyList<string>? _cachedWorkspaceRoles;

        public WorkspaceContextService(
            GrcDbContext context,
            ITenantContextService tenantContext,
            IHttpContextAccessor httpContextAccessor,
            ILogger<WorkspaceContextService> logger)
        {
            _context = context;
            _tenantContext = tenantContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid GetCurrentTenantId()
        {
            return _tenantContext.GetCurrentTenantId();
        }

        public Guid GetCurrentWorkspaceId()
        {
            if (_cachedWorkspaceId.HasValue)
                return _cachedWorkspaceId.Value;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session == null)
            {
                _logger.LogWarning("No HTTP context or session available for workspace resolution");
                return Guid.Empty;
            }

            // Try to get from session
            var workspaceIdString = httpContext.Session.GetString(WorkspaceIdSessionKey);
            if (!string.IsNullOrEmpty(workspaceIdString) && Guid.TryParse(workspaceIdString, out var workspaceId))
            {
                _cachedWorkspaceId = workspaceId;
                return workspaceId;
            }

            // No workspace in session - get default synchronously to avoid deadlock
            // Use synchronous query instead of async to prevent deadlock in sync context
            var tenantId = GetCurrentTenantId();
            var userId = _tenantContext.GetCurrentUserId();

            if (tenantId != Guid.Empty && !string.IsNullOrEmpty(userId))
            {
                var defaultWorkspaceId = GetDefaultWorkspaceIdSync(tenantId, userId);
                if (defaultWorkspaceId.HasValue)
                {
                    _cachedWorkspaceId = defaultWorkspaceId.Value;
                    httpContext.Session.SetString(WorkspaceIdSessionKey, defaultWorkspaceId.Value.ToString());
                    return defaultWorkspaceId.Value;
                }
            }

            _logger.LogWarning("No workspace context available for user");
            return Guid.Empty;
        }

        /// <summary>
        /// Synchronous version to avoid deadlock in sync context
        /// </summary>
        private Guid? GetDefaultWorkspaceIdSync(Guid tenantId, string userId)
        {
            try
            {
                // First try: user's primary workspace
                var primaryWorkspaceId = _context.Set<WorkspaceMembership>()
                    .Where(wm => wm.TenantId == tenantId
                              && wm.UserId == userId
                              && wm.IsPrimary
                              && wm.Status == "Active"
                              && !wm.IsDeleted)
                    .Select(wm => wm.WorkspaceId)
                    .FirstOrDefault();

                if (primaryWorkspaceId != Guid.Empty)
                    return primaryWorkspaceId;

                // Second try: tenant's default workspace
                var defaultWorkspaceId = _context.Workspaces
                    .Where(w => w.TenantId == tenantId && w.IsDefault && !w.IsDeleted)
                    .Select(w => w.Id)
                    .FirstOrDefault();

                if (defaultWorkspaceId != Guid.Empty)
                    return defaultWorkspaceId;

                // Third try: any workspace user belongs to
                var anyWorkspaceId = _context.Set<WorkspaceMembership>()
                    .Where(wm => wm.TenantId == tenantId
                              && wm.UserId == userId
                              && wm.Status == "Active"
                              && !wm.IsDeleted)
                    .Select(wm => wm.WorkspaceId)
                    .FirstOrDefault();

                return anyWorkspaceId != Guid.Empty ? anyWorkspaceId : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default workspace synchronously");
                return null;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetUserWorkspaceIdsAsync()
        {
            if (_cachedWorkspaceIds != null)
                return _cachedWorkspaceIds;

            var tenantId = GetCurrentTenantId();
            var userId = _tenantContext.GetCurrentUserId();

            if (tenantId == Guid.Empty || string.IsNullOrEmpty(userId))
            {
                _cachedWorkspaceIds = Array.Empty<Guid>();
                return _cachedWorkspaceIds;
            }

            var workspaceIds = await _context.Set<WorkspaceMembership>()
                .Where(wm => wm.TenantId == tenantId
                          && wm.UserId == userId
                          && wm.Status == "Active"
                          && !wm.IsDeleted)
                .Select(wm => wm.WorkspaceId)
                .ToListAsync();

            _cachedWorkspaceIds = workspaceIds.AsReadOnly();
            return _cachedWorkspaceIds;
        }

        public async Task SetCurrentWorkspaceAsync(Guid workspaceId)
        {
            // Validate access first
            if (!await ValidateWorkspaceAccessAsync(workspaceId))
            {
                throw new UnauthorizedAccessException($"User does not have access to workspace {workspaceId}");
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session == null)
            {
                throw new InvalidOperationException("No HTTP session available");
            }

            httpContext.Session.SetString(WorkspaceIdSessionKey, workspaceId.ToString());

            // Clear cached values
            _cachedWorkspaceId = workspaceId;
            _cachedWorkspaceRoles = null;

            _logger.LogInformation("Switched to workspace {WorkspaceId}", workspaceId);
        }

        public async Task<Guid?> GetDefaultWorkspaceIdAsync()
        {
            var tenantId = GetCurrentTenantId();
            var userId = _tenantContext.GetCurrentUserId();

            if (tenantId == Guid.Empty || string.IsNullOrEmpty(userId))
                return null;

            // First try: user's primary workspace
            var primaryMembership = await _context.Set<WorkspaceMembership>()
                .Where(wm => wm.TenantId == tenantId
                          && wm.UserId == userId
                          && wm.IsPrimary
                          && wm.Status == "Active"
                          && !wm.IsDeleted)
                .Select(wm => wm.WorkspaceId)
                .FirstOrDefaultAsync();

            if (primaryMembership != Guid.Empty)
                return primaryMembership;

            // Second try: tenant's default workspace
            var defaultWorkspace = await _context.Set<Workspace>()
                .Where(w => w.TenantId == tenantId
                         && w.IsDefault
                         && w.Status == "Active"
                         && !w.IsDeleted)
                .Select(w => w.Id)
                .FirstOrDefaultAsync();

            if (defaultWorkspace != Guid.Empty)
                return defaultWorkspace;

            // Third try: any workspace user has access to
            var anyWorkspace = await _context.Set<WorkspaceMembership>()
                .Where(wm => wm.TenantId == tenantId
                          && wm.UserId == userId
                          && wm.Status == "Active"
                          && !wm.IsDeleted)
                .Select(wm => wm.WorkspaceId)
                .FirstOrDefaultAsync();

            return anyWorkspace != Guid.Empty ? anyWorkspace : null;
        }

        public async Task<bool> ValidateWorkspaceAccessAsync(Guid workspaceId)
        {
            var userWorkspaces = await GetUserWorkspaceIdsAsync();
            return userWorkspaces.Contains(workspaceId);
        }

        public async Task<IReadOnlyList<string>> GetCurrentWorkspaceRolesAsync()
        {
            if (_cachedWorkspaceRoles != null)
                return _cachedWorkspaceRoles;

            var tenantId = GetCurrentTenantId();
            var userId = _tenantContext.GetCurrentUserId();
            var workspaceId = GetCurrentWorkspaceId();

            if (tenantId == Guid.Empty || string.IsNullOrEmpty(userId) || workspaceId == Guid.Empty)
            {
                _cachedWorkspaceRoles = Array.Empty<string>();
                return _cachedWorkspaceRoles;
            }

            var membership = await _context.Set<WorkspaceMembership>()
                .Where(wm => wm.TenantId == tenantId
                          && wm.WorkspaceId == workspaceId
                          && wm.UserId == userId
                          && wm.Status == "Active"
                          && !wm.IsDeleted)
                .FirstOrDefaultAsync();

            if (membership?.WorkspaceRolesJson == null)
            {
                _cachedWorkspaceRoles = Array.Empty<string>();
                return _cachedWorkspaceRoles;
            }

            try
            {
                var roles = JsonSerializer.Deserialize<List<string>>(membership.WorkspaceRolesJson);
                _cachedWorkspaceRoles = roles?.AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to deserialize workspace roles for user {UserId}", userId);
                _cachedWorkspaceRoles = Array.Empty<string>();
            }

            return _cachedWorkspaceRoles;
        }

        public bool HasWorkspaceContext()
        {
            return GetCurrentWorkspaceId() != Guid.Empty;
        }
    }
}
