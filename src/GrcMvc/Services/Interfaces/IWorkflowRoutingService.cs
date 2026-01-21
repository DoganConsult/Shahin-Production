using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Workflow Routing Service - Resolves assignees using roles, not hard-coded users
    /// Guarantees "recognition" even when some fields are missing
    ///
    /// Resolution order:
    /// 1. If record has owner_user_id → assign user
    /// 2. Else if record has owner_team_id → assign team members with matching role
    /// 3. Else use RACI map by scope
    /// 4. Else fallback to org default team
    /// </summary>
    public interface IWorkflowRoutingService
    {
        /// <summary>
        /// Resolve assignees for a workflow task based on role and scope
        /// </summary>
        Task<List<AssigneeResolution>> ResolveAssigneesAsync(
            Guid tenantId,
            string roleCode,
            string? scopeType = null,
            string? scopeId = null,
            Guid? recordOwnerUserId = null,
            Guid? recordOwnerTeamId = null);

        /// <summary>
        /// Resolve single primary assignee (for tasks that need one owner)
        /// </summary>
        Task<AssigneeResolution?> ResolvePrimaryAssigneeAsync(
            Guid tenantId,
            string roleCode,
            string? scopeType = null,
            string? scopeId = null);

        /// <summary>
        /// Get all users with a specific role in a tenant
        /// </summary>
        Task<List<AssigneeResolution>> GetUsersByRoleAsync(Guid tenantId, string roleCode);

        /// <summary>
        /// Get RACI assignments for a scope
        /// </summary>
        Task<List<RACIResolution>> GetRACIForScopeAsync(
            Guid tenantId,
            string scopeType,
            string scopeId);

        /// <summary>
        /// Get default fallback team for tenant
        /// </summary>
        Task<Guid?> GetFallbackTeamAsync(Guid tenantId);

        /// <summary>
        /// Validate that a user can be assigned to a role-based task
        /// </summary>
        Task<bool> CanUserBeAssignedAsync(Guid tenantId, Guid userId, string roleCode);
    }

    /// <summary>
    /// Result of assignee resolution
    /// </summary>
    public class AssigneeResolution
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string ResolutionSource { get; set; } = string.Empty; // "DirectOwner", "TeamRole", "RACI", "Fallback"
        public bool IsPrimary { get; set; } = false;
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// RACI resolution result
    /// </summary>
    public class RACIResolution
    {
        public string RACI { get; set; } = string.Empty; // R, A, C, I
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? RoleCode { get; set; }
        public List<AssigneeResolution> Assignees { get; set; } = new();
    }
}
