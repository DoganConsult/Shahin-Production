using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Resolves workflow task assignees from roles, user IDs, or role codes
    /// </summary>
    public class WorkflowAssigneeResolver : IWorkflowAssigneeResolver
    {
        private readonly GrcDbContext _context;
        private readonly IUserDirectoryService _userDirectory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WorkflowAssigneeResolver> _logger;

        public WorkflowAssigneeResolver(
            GrcDbContext context,
            IUserDirectoryService userDirectory,
            UserManager<ApplicationUser> userManager,
            ILogger<WorkflowAssigneeResolver> logger)
        {
            _context = context;
            _userDirectory = userDirectory;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Resolve assignee user ID from role code, role name, user ID, or department
        /// Supports multi-team assignment via department/role
        /// </summary>
        public async Task<Guid?> ResolveAssigneeAsync(
            Guid tenantId,
            string? assignee,
            Guid? defaultAssigneeUserId = null,
            string? assigneeRule = null,
            string? department = null)
        {
            if (string.IsNullOrWhiteSpace(assignee))
            {
                return defaultAssigneeUserId;
            }

            // Handle ByDepartment rule for multi-team support
            if (assigneeRule == "ByDepartment")
            {
                // Use role code as department identifier for now
                return await ResolveByDepartmentAsync(tenantId, assignee ?? "Default", assignee);
            }

            // Legacy signature support (backward compatibility)
            if (defaultAssigneeUserId.HasValue && string.IsNullOrEmpty(assigneeRule))
            {
                // Original signature: (tenantId, assignee, defaultAssigneeUserId)
                // Continue with original logic
            }

            // If assignee is already a GUID, return it
            if (Guid.TryParse(assignee, out var userId))
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    var tenantUser = await _context.TenantUsers
                        .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString() && tu.TenantId == tenantId && !tu.IsDeleted);
                    if (tenantUser != null)
                    {
                        _logger.LogDebug("Resolved assignee from GUID: {UserId}", userId);
                        return userId;
                    }
                }
            }

            // Try to resolve from role code (RoleCatalog)
            var roleCatalog = await _context.RoleCatalogs
                .FirstOrDefaultAsync(r => r.RoleCode == assignee && !r.IsDeleted);

            if (roleCatalog != null)
            {
                // Find first user with this role in the tenant via TenantUser
                var tenantUser = await _context.TenantUsers
                    .Where(tu => tu.TenantId == tenantId && tu.RoleCode == assignee && !tu.IsDeleted)
                    .FirstOrDefaultAsync();

                if (tenantUser != null)
                {
                    _logger.LogDebug("Resolved assignee from role code {RoleCode}: {UserId}", assignee, tenantUser.UserId);
                    return Guid.TryParse(tenantUser.UserId, out var uid) ? uid : null;
                }
            }

            // Try to resolve from Identity role name
            var identityRole = await _userDirectory.GetRoleByNameAsync(assignee);

            if (identityRole != null)
            {
                // Find first user with this role in tenant
                var usersInRole = await _userManager.GetUsersInRoleAsync(assignee);
                foreach (var u in usersInRole)
                {
                    var tenantUser = await _context.TenantUsers
                        .FirstOrDefaultAsync(tu => tu.UserId == u.Id.ToString() && tu.TenantId == tenantId && !tu.IsDeleted);
                    if (tenantUser != null)
                    {
                        _logger.LogDebug("Resolved assignee from Identity role {RoleName}: {UserId}", assignee, u.Id);
                        return u.Id;
                    }
                }
            }

            // Try to find user by username/email
            var userByEmail = await _userManager.FindByEmailAsync(assignee);
            if (userByEmail != null)
            {
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.UserId == userByEmail.Id.ToString() && tu.TenantId == tenantId && !tu.IsDeleted);
                if (tenantUser != null)
                {
                    _logger.LogDebug("Resolved assignee from email {Email}: {UserId}", assignee, userByEmail.Id);
                    return userByEmail.Id;
                }
            }

            _logger.LogWarning("Could not resolve assignee: {Assignee}, using default", assignee);
            return defaultAssigneeUserId;
        }

        /// <summary>
        /// Resolve assignee by department (multi-team support)
        /// Returns first user in the department with the specified role
        /// Uses ApplicationUser.Department property for true department-based assignment
        /// </summary>
        private async Task<Guid?> ResolveByDepartmentAsync(
            Guid tenantId,
            string department,
            string? roleCode = null)
        {
            try
            {
                // Get tenant users
                var tenantUsers = await _context.TenantUsers
                    .Where(tu => tu.TenantId == tenantId && !tu.IsDeleted)
                    .ToListAsync();

                // Try to find user by department and role
                foreach (var tenantUser in tenantUsers)
                {
                    if (!Guid.TryParse(tenantUser.UserId, out var userId))
                        continue;

                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                        continue;

                    // Check if user's department matches (ApplicationUser has Department property)
                    var appUser = user as ApplicationUser;
                    if (appUser != null && !string.IsNullOrEmpty(appUser.Department))
                    {
                        // If role code specified, also check role
                        if (!string.IsNullOrEmpty(roleCode))
                        {
                            if (tenantUser.RoleCode == roleCode &&
                                appUser.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Resolved assignee by department {Department} with role {RoleCode}: {UserId}",
                                    department, roleCode, userId);
                                return userId;
                            }
                        }
                        else if (appUser.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug("Resolved assignee by department {Department}: {UserId}", department, userId);
                            return userId;
                        }
                    }
                }

                // Fallback: use role-based if department not found
                if (!string.IsNullOrEmpty(roleCode))
                {
                    var userWithRole = tenantUsers.FirstOrDefault(tu => tu.RoleCode == roleCode);
                    if (userWithRole != null && Guid.TryParse(userWithRole.UserId, out var fallbackUserId))
                    {
                        _logger.LogWarning("Department {Department} not found, using role {RoleCode} fallback: {UserId}",
                            department, roleCode, fallbackUserId);
                        return fallbackUserId;
                    }
                }

                _logger.LogWarning("Could not resolve assignee by department {Department}", department);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving assignee by department {Department}", department);
                return null;
            }
        }

        /// <summary>
        /// Resolve multiple assignees for team-based assignment
        /// Returns all users matching the criteria (for team assignment)
        /// </summary>
        public async Task<List<Guid>> ResolveTeamAssigneesAsync(
            Guid tenantId,
            string? roleCode = null,
            string? department = null)
        {
            var assignees = new List<Guid>();

            try
            {
                // If department is specified, use TeamMember with Team.BusinessUnit for filtering
                if (!string.IsNullOrEmpty(department))
                {
                    var teamMembers = await _context.TeamMembers
                        .Include(tm => tm.Team)
                        .Where(tm => tm.TenantId == tenantId &&
                                    tm.IsActive && !tm.IsDeleted &&
                                    tm.Team != null &&
                                    tm.Team.BusinessUnit == department)
                        .ToListAsync();

                    // Further filter by role if specified
                    if (!string.IsNullOrEmpty(roleCode))
                    {
                        teamMembers = teamMembers
                            .Where(tm => tm.RoleCode == roleCode)
                            .ToList();
                    }

                    assignees = teamMembers.Select(tm => tm.UserId).Distinct().ToList();
                }
                else
                {
                    // No department filter - use TenantUsers with role filter
                    var tenantUsers = await _context.TenantUsers
                        .Where(tu => tu.TenantId == tenantId &&
                                    tu.Status == "Active" && !tu.IsDeleted)
                        .ToListAsync();

                    // Filter by role if specified
                    if (!string.IsNullOrEmpty(roleCode))
                    {
                        tenantUsers = tenantUsers
                            .Where(tu => tu.RoleCode == roleCode)
                            .ToList();
                    }

                    foreach (var tenantUser in tenantUsers)
                    {
                        if (Guid.TryParse(tenantUser.UserId, out var userId))
                        {
                            assignees.Add(userId);
                        }
                    }
                }

                _logger.LogDebug("Resolved {Count} team assignees for tenant {TenantId}, role {RoleCode}, department {Department}",
                    assignees.Count, tenantId, roleCode, department);

                return assignees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving team assignees");
                return assignees;
            }
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        public Guid? GetCurrentUserId(string? userIdClaim)
        {
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return null;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }
}
