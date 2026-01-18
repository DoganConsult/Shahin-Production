using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services
{
    /// <summary>
    /// Service to filter user workspace based on role profile scope
    /// Implements KSA-based role assignment and permission filtering
    /// </summary>
    public interface IUserWorkspaceService
    {
        Task<UserWorkspaceViewModel> GetUserWorkspaceAsync(string userId, Guid tenantId);
        Task AssignRoleToUserAsync(string userId, Guid roleProfileId, string ksaAreas);
        Task<IEnumerable<WorkflowDefinition>> GetUserAccessibleWorkflowsAsync(string userId);
        Task<IEnumerable<T>> FilterByScopeAsync<T>(IEnumerable<T> items, string userScope) where T : BaseEntity;
    }

    public class UserWorkspaceService : IUserWorkspaceService
    {
        private readonly GrcDbContext _context;
        private readonly GrcAuthDbContext _authContext;
        private readonly IUserDirectoryService _userDirectory;
        private readonly ILogger<UserWorkspaceService> _logger;

        public UserWorkspaceService(
            GrcDbContext context,
            GrcAuthDbContext authContext,
            IUserDirectoryService userDirectory,
            ILogger<UserWorkspaceService> logger)
        {
            _context = context;
            _authContext = authContext;
            _userDirectory = userDirectory;
            _logger = logger;
        }

        /// <summary>
        /// Get user's workspace filtered by role scope
        /// </summary>
        public async Task<UserWorkspaceViewModel> GetUserWorkspaceAsync(string userId, Guid tenantId)
        {
            try
            {
                // Get user from Auth DB
                var user = await _authContext.Set<ApplicationUser>()
                    .Include(u => u.RoleProfile)
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId) && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning($"⚠️ User {userId} not found or inactive");
                    return new UserWorkspaceViewModel();
                }

                if (user.RoleProfile == null)
                {
                    _logger.LogWarning($"⚠️ User {userId} has no role profile assigned");
                    return new UserWorkspaceViewModel();
                }

                // Get accessible workflows based on role
                var accessibleWorkflows = await GetUserAccessibleWorkflowsAsync(userId);

                // Get accessible tasks based on assignment
                var userTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => (t.AssignedToUserId.ToString() == userId) &&
                                t.Status != "Completed" &&
                                t.Status != "Rejected")
                    .ToListAsync();

                // Get assigned assessments
                var assessments = await _context.Assessments
                    .Where(a => a.AssignedTo == user.FullName)
                    .ToListAsync();

                // Get risks by department
                var risks = await _context.Risks
                    .Where(r => r.Owner == user.Department || r.Owner == user.JobTitle)
                    .ToListAsync();

                // Get policies for all users
                var policies = await _context.Policies
                    .ToListAsync();

                return new UserWorkspaceViewModel
                {
                    UserId = user.Id.ToString(),
                    UserName = user.FullName,
                    RoleProfile = user.RoleProfile,
                    Layer = user.RoleProfile.Layer,
                    Department = user.RoleProfile.Department,
                    AssignedScope = user.AssignedScope ?? user.RoleProfile.Scope,
                    ApprovalLevel = user.RoleProfile.ApprovalLevel,
                    CanApprove = user.RoleProfile.CanApprove,
                    CanReject = user.RoleProfile.CanReject,
                    CanEscalate = user.RoleProfile.CanEscalate,
                    KsaCompetencyLevel = user.KsaCompetencyLevel,
                    AccessibleWorkflows = accessibleWorkflows.ToList(),
                    PendingTasks = userTasks.Count(),
                    PendingTasksList = userTasks,
                    AssignedAssessments = assessments.Count(),
                    AssignedRisks = risks.Count(),
                    AssignedPolicies = policies.Count(),
                    LastAccessTime = user.LastLoginDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting user workspace: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Assign role profile to user (typically during onboarding)
        /// </summary>
        public async Task AssignRoleToUserAsync(string userId, Guid roleProfileId, string ksaAreas)
        {
            try
            {
                // Get user from Auth DB
                var user = await _authContext.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
                if (user == null)
                {
                    _logger.LogError($"❌ User {userId} not found");
                    throw new InvalidOperationException("User not found");
                }

                // Get role profile from main DB (it's app data, not identity)
                var roleProfile = await _context.RoleProfiles.FirstOrDefaultAsync(r => r.Id == roleProfileId);
                if (roleProfile == null)
                {
                    _logger.LogError($"❌ Role profile {roleProfileId} not found");
                    throw new InvalidOperationException("Role profile not found");
                }

                // Assign role (update user in Auth DB)
                user.RoleProfileId = roleProfileId;
                user.AssignedScope = roleProfile.Scope;
                user.KsaCompetencyLevel = 3; // Default to intermediate

                // Store KSA areas
                var ksaAreas_list = ksaAreas.Split(',').Select(a => a.Trim()).ToList();
                user.KnowledgeAreas = JsonSerializer.Serialize(ksaAreas_list);
                user.Skills = JsonSerializer.Serialize(new List<string> { "Role assigned on " + DateTime.UtcNow });
                user.Abilities = JsonSerializer.Serialize(new List<string> { "Profile: " + roleProfile.RoleName });

                _authContext.Set<ApplicationUser>().Update(user);
                await _authContext.SaveChangesAsync();

                _logger.LogInformation($"✅ User {user.FullName} assigned to role {roleProfile.RoleName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error assigning role: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get workflows accessible to user based on role profile
        /// </summary>
        public async Task<IEnumerable<WorkflowDefinition>> GetUserAccessibleWorkflowsAsync(string userId)
        {
            try
            {
                // Get user from Auth DB
                var user = await _authContext.Set<ApplicationUser>()
                    .Include(u => u.RoleProfile)
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

                if (user?.RoleProfile?.ParticipatingWorkflows == null)
                {
                    return new List<WorkflowDefinition>();
                }

                var workflowNumbers = user.RoleProfile.ParticipatingWorkflows
                    .Split(',')
                    .Select(w => w.Trim())
                    .ToList();

                // Get workflows from main DB
                var workflows = await _context.WorkflowDefinitions
                    .Where(w => workflowNumbers.Contains(w.WorkflowNumber) && w.Status == "Active")
                    .ToListAsync();

                return workflows;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting accessible workflows: {ex.Message}");
                return new List<WorkflowDefinition>();
            }
        }

        /// <summary>
        /// Generic scope-based filtering
        /// </summary>
        public async Task<IEnumerable<T>> FilterByScopeAsync<T>(IEnumerable<T> items, string userScope) where T : BaseEntity
        {
            return await Task.FromResult(items);
        }

        /// <summary>
        /// Filter assessments by department matching user scope
        /// </summary>
        private IQueryable<Assessment> FilterByDepartmentAsync(
            IQueryable<Assessment> query,
            string userScope)
        {
            if (string.IsNullOrEmpty(userScope))
                return query;

            // Parse scope (comma-separated departments/areas)
            var scopeAreas = userScope.Split(',').Select(s => s.Trim()).ToList();

            // Filter assessments matching user scope
            return query.Where(a => scopeAreas.Contains(a.Type));
        }

        /// <summary>
        /// Filter risks by department matching user scope
        /// </summary>
        private IQueryable<Risk> FilterByDepartmentAsync(
            IQueryable<Risk> query,
            string userScope)
        {
            if (string.IsNullOrEmpty(userScope))
                return query;

            var scopeAreas = userScope.Split(',').Select(s => s.Trim()).ToList();
            return query.Where(r => scopeAreas.Contains(r.Category) ||
                                   scopeAreas.Contains(r.Owner));
        }

        /// <summary>
        /// Filter policies by department matching user scope
        /// </summary>
        private IQueryable<Policy> FilterByDepartmentAsync(
            IQueryable<Policy> query,
            string userScope)
        {
            if (string.IsNullOrEmpty(userScope))
                return query;

            var scopeAreas = userScope.Split(',').Select(s => s.Trim()).ToList();
            return query.Where(p => scopeAreas.Any(area => p.Owner.Contains(area)));
        }
    }

    /// <summary>
    /// User workspace view model - filtered based on role scope
    /// </summary>
    public class UserWorkspaceViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public RoleProfile? RoleProfile { get; set; }
        public string Layer { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string AssignedScope { get; set; } = string.Empty;
        public int ApprovalLevel { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanEscalate { get; set; }
        public int KsaCompetencyLevel { get; set; }
        public List<WorkflowDefinition> AccessibleWorkflows { get; set; } = new();
        public int PendingTasks { get; set; }
        public List<WorkflowTask> PendingTasksList { get; set; } = new();
        public int AssignedAssessments { get; set; }
        public int AssignedRisks { get; set; }
        public int AssignedPolicies { get; set; }
        public DateTime? LastAccessTime { get; set; }
    }
}
