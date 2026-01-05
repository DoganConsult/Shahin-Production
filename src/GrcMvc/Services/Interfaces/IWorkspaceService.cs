using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for managing role-based user workspaces
/// Pre-maps dashboards, tasks, and quick actions based on user's role
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// Create workspace for a user based on their role
    /// </summary>
    Task<UserWorkspace> CreateWorkspaceAsync(
        Guid tenantId,
        string userId,
        string roleCode,
        string createdBy);

    /// <summary>
    /// Get user's workspace
    /// </summary>
    Task<UserWorkspace?> GetUserWorkspaceAsync(string userId);

    /// <summary>
    /// Pre-map tasks to user's workspace based on role and applicable frameworks
    /// </summary>
    Task<List<UserWorkspaceTask>> PreMapTasksAsync(
        Guid workspaceId,
        Guid tenantId,
        string roleCode,
        List<Guid> assessmentIds,
        string createdBy);

    /// <summary>
    /// Get workspace template for a role
    /// </summary>
    Task<WorkspaceTemplate?> GetWorkspaceTemplateAsync(string roleCode);

    /// <summary>
    /// Get user's pending tasks
    /// </summary>
    Task<IEnumerable<UserWorkspaceTask>> GetUserTasksAsync(string userId, string? status = null);

    /// <summary>
    /// Update task status
    /// </summary>
    Task<UserWorkspaceTask> UpdateTaskStatusAsync(Guid taskId, string status, string modifiedBy);

    /// <summary>
    /// Create workspaces for all team members after onboarding
    /// </summary>
    Task<List<UserWorkspace>> CreateTeamWorkspacesAsync(
        Guid tenantId,
        List<TeamMemberDto> teamMembers,
        List<Guid> assessmentIds,
        string createdBy);
}

/// <summary>
/// DTO for team member assignment
/// </summary>
public class TeamMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? TitleCode { get; set; }
    public List<string>? AssignedFrameworks { get; set; }
}
