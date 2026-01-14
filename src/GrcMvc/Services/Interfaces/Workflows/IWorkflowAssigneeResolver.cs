namespace GrcMvc.Services.Interfaces.Workflows
{
    /// <summary>
    /// Interface for resolving workflow task assignees
    /// </summary>
    public interface IWorkflowAssigneeResolver
    {
        /// <summary>
        /// Resolve assignee user ID from role code, role name, user ID, or department
        /// </summary>
        Task<Guid?> ResolveAssigneeAsync(
            Guid tenantId,
            string? assignee,
            Guid? defaultAssigneeUserId = null,
            string? assigneeRule = null,
            string? department = null);

        /// <summary>
        /// Resolve multiple assignees for team-based assignment
        /// </summary>
        Task<List<Guid>> ResolveTeamAssigneesAsync(
            Guid tenantId,
            string? roleCode = null,
            string? department = null);

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        Guid? GetCurrentUserId(string? userIdClaim);
    }
}
