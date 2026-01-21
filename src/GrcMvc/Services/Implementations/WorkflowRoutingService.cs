using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Workflow Routing Service Implementation
    /// Resolves assignees using roles, not hard-coded users/emails
    ///
    /// Resolution order (deterministic):
    /// 1. If record has owner_user_id → assign user
    /// 2. Else if record has owner_team_id → assign team members with matching role
    /// 3. Else use RACI map by scope
    /// 4. Else fallback to org default team
    /// </summary>
    public class WorkflowRoutingService : IWorkflowRoutingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkflowRoutingService> _logger;

        public WorkflowRoutingService(IUnitOfWork unitOfWork, ILogger<WorkflowRoutingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AssigneeResolution>> ResolveAssigneesAsync(
            Guid tenantId,
            string roleCode,
            string? scopeType = null,
            string? scopeId = null,
            Guid? recordOwnerUserId = null,
            Guid? recordOwnerTeamId = null)
        {
            var assignees = new List<AssigneeResolution>();

            // Step 1: If record has direct owner user → assign that user
            if (recordOwnerUserId.HasValue)
            {
                var ownerUser = await _unitOfWork.TenantUsers
                    .Query()
                    .FirstOrDefaultAsync(u => u.Id == recordOwnerUserId.Value && u.TenantId == tenantId && !u.IsDeleted);

                if (ownerUser != null)
                {
                    assignees.Add(new AssigneeResolution
                    {
                        UserId = ownerUser.Id,
                        UserName = ownerUser.UserId, // Use UserId as fallback
                        Email = ownerUser.User?.Email ?? "",
                        RoleCode = ownerUser.RoleCode,
                        ResolutionSource = "DirectOwner",
                        IsPrimary = true,
                        Priority = 1
                    });

                    _logger.LogDebug("Resolved assignee via DirectOwner: {UserId}", ownerUser.Id);
                    return assignees;
                }
            }

            // Step 2: If record has owner team → assign team members with matching role
            if (recordOwnerTeamId.HasValue)
            {
                var teamMembers = await GetTeamMembersWithRoleAsync(tenantId, recordOwnerTeamId.Value, roleCode);
                if (teamMembers.Any())
                {
                    foreach (var member in teamMembers)
                    {
                        member.ResolutionSource = "TeamRole";
                    }
                    _logger.LogDebug("Resolved {Count} assignees via TeamRole for team {TeamId}", teamMembers.Count, recordOwnerTeamId);
                    return teamMembers;
                }
            }

            // Step 3: Use RACI map by scope
            if (!string.IsNullOrEmpty(scopeType) && !string.IsNullOrEmpty(scopeId))
            {
                var raciAssignees = await ResolveFromRACIAsync(tenantId, scopeType, scopeId, roleCode);
                if (raciAssignees.Any())
                {
                    _logger.LogDebug("Resolved {Count} assignees via RACI for scope {ScopeType}:{ScopeId}", raciAssignees.Count, scopeType, scopeId);
                    return raciAssignees;
                }
            }

            // Step 4: Fallback to org default team
            var fallbackTeamId = await GetFallbackTeamAsync(tenantId);
            if (fallbackTeamId.HasValue)
            {
                var fallbackMembers = await GetTeamMembersWithRoleAsync(tenantId, fallbackTeamId.Value, roleCode);
                if (fallbackMembers.Any())
                {
                    foreach (var member in fallbackMembers)
                    {
                        member.ResolutionSource = "Fallback";
                    }
                    _logger.LogDebug("Resolved {Count} assignees via Fallback team", fallbackMembers.Count);
                    return fallbackMembers;
                }
            }

            // Step 5: Last resort - any user with the role in the tenant
            var anyWithRole = await GetUsersByRoleAsync(tenantId, roleCode);
            if (anyWithRole.Any())
            {
                foreach (var member in anyWithRole)
                {
                    member.ResolutionSource = "TenantRole";
                }
                _logger.LogDebug("Resolved {Count} assignees via TenantRole fallback", anyWithRole.Count);
                return anyWithRole;
            }

            _logger.LogWarning("No assignees found for tenant {TenantId}, role {RoleCode}", tenantId, roleCode);
            return assignees;
        }

        public async Task<AssigneeResolution?> ResolvePrimaryAssigneeAsync(
            Guid tenantId,
            string roleCode,
            string? scopeType = null,
            string? scopeId = null)
        {
            var assignees = await ResolveAssigneesAsync(tenantId, roleCode, scopeType, scopeId);

            // Return the primary one, or first by priority
            return assignees
                .OrderByDescending(a => a.IsPrimary)
                .ThenBy(a => a.Priority)
                .FirstOrDefault();
        }

        public async Task<List<AssigneeResolution>> GetUsersByRoleAsync(Guid tenantId, string roleCode)
        {
            var users = await _unitOfWork.TenantUsers
                .Query()
                .Where(u => u.TenantId == tenantId && u.RoleCode == roleCode && u.Status == "Active" && !u.IsDeleted)
                .ToListAsync();

            return users.Select(u => new AssigneeResolution
            {
                UserId = u.Id,
                UserName = u.UserId,
                Email = u.User?.Email ?? "",
                RoleCode = u.RoleCode,
                ResolutionSource = "TenantRole",
                Priority = 10
            }).ToList();
        }

        public async Task<List<RACIResolution>> GetRACIForScopeAsync(
            Guid tenantId,
            string scopeType,
            string scopeId)
        {
            var raciAssignments = await _unitOfWork.RACIAssignments
                .Query()
                .Include(r => r.Team)
                .Where(r => r.TenantId == tenantId
                    && r.ScopeType == scopeType
                    && r.ScopeId == scopeId
                    && r.IsActive
                    && !r.IsDeleted)
                .OrderBy(r => r.RACI)
                .ThenBy(r => r.Priority)
                .ToListAsync();

            var results = new List<RACIResolution>();

            foreach (var raci in raciAssignments)
            {
                var resolution = new RACIResolution
                {
                    RACI = raci.RACI,
                    TeamId = raci.TeamId,
                    TeamName = raci.Team?.Name ?? "",
                    RoleCode = raci.RoleCode
                };

                // Get team members
                if (!string.IsNullOrEmpty(raci.RoleCode))
                {
                    resolution.Assignees = await GetTeamMembersWithRoleAsync(tenantId, raci.TeamId, raci.RoleCode);
                }
                else
                {
                    resolution.Assignees = await GetAllTeamMembersAsync(tenantId, raci.TeamId);
                }

                results.Add(resolution);
            }

            return results;
        }

        public async Task<Guid?> GetFallbackTeamAsync(Guid tenantId)
        {
            var fallbackTeam = await _unitOfWork.Teams
                .Query()
                .Where(t => t.TenantId == tenantId && t.IsDefaultFallback && t.IsActive && !t.IsDeleted)
                .FirstOrDefaultAsync();

            return fallbackTeam?.Id;
        }

        public async Task<bool> CanUserBeAssignedAsync(Guid tenantId, Guid userId, string roleCode)
        {
            // Check if user exists in tenant with the required role
            var user = await _unitOfWork.TenantUsers
                .Query()
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && !u.IsDeleted);

            if (user == null) return false;

            // User has the exact role
            if (user.RoleCode == roleCode) return true;

            // User is in a team with that role
            var teamMembership = await _unitOfWork.TeamMembers
                .Query()
                .AnyAsync(tm => tm.UserId == userId
                    && tm.TenantId == tenantId
                    && tm.RoleCode == roleCode
                    && tm.IsActive
                    && !tm.IsDeleted);

            return teamMembership;
        }

        // ===== Private Helper Methods =====

        private async Task<List<AssigneeResolution>> GetTeamMembersWithRoleAsync(
            Guid tenantId,
            Guid teamId,
            string roleCode)
        {
            var members = await _unitOfWork.TeamMembers
                .Query()
                .Include(tm => tm.User)
                .Include(tm => tm.Team)
                .Where(tm => tm.TenantId == tenantId
                    && tm.TeamId == teamId
                    && tm.RoleCode == roleCode
                    && tm.IsActive
                    && !tm.IsDeleted)
                .OrderByDescending(tm => tm.IsPrimaryForRole)
                .ToListAsync();

            return members.Select(tm => new AssigneeResolution
            {
                UserId = tm.UserId,
                UserName = tm.UserId.ToString(),
                Email = "",
                RoleCode = tm.RoleCode,
                TeamId = tm.TeamId,
                TeamName = tm.Team?.Name,
                IsPrimary = tm.IsPrimaryForRole,
                Priority = tm.IsPrimaryForRole ? 1 : 5
            }).ToList();
        }

        private async Task<List<AssigneeResolution>> GetAllTeamMembersAsync(Guid tenantId, Guid teamId)
        {
            var members = await _unitOfWork.TeamMembers
                .Query()
                .Include(tm => tm.Team)
                .Where(tm => tm.TenantId == tenantId
                    && tm.TeamId == teamId
                    && tm.IsActive
                    && !tm.IsDeleted)
                .ToListAsync();

            return members.Select(tm => new AssigneeResolution
            {
                UserId = tm.UserId,
                UserName = tm.UserId.ToString(),
                Email = "",
                RoleCode = tm.RoleCode,
                TeamId = tm.TeamId,
                TeamName = tm.Team?.Name,
                Priority = 5
            }).ToList();
        }

        private async Task<List<AssigneeResolution>> ResolveFromRACIAsync(
            Guid tenantId,
            string scopeType,
            string scopeId,
            string roleCode)
        {
            // Find RACI assignment for this scope with R (Responsible) or A (Accountable)
            var raciAssignment = await _unitOfWork.RACIAssignments
                .Query()
                .Where(r => r.TenantId == tenantId
                    && r.ScopeType == scopeType
                    && r.ScopeId == scopeId
                    && (r.RACI == "R" || r.RACI == "A")
                    && r.IsActive
                    && !r.IsDeleted)
                .OrderBy(r => r.Priority)
                .FirstOrDefaultAsync();

            if (raciAssignment == null) return new List<AssigneeResolution>();

            // Get team members with the required role
            var members = await GetTeamMembersWithRoleAsync(tenantId, raciAssignment.TeamId, roleCode);

            foreach (var member in members)
            {
                member.ResolutionSource = "RACI";
            }

            return members;
        }
    }
}
