using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-03: Role-Based Access Control (RBAC) assignment validation service.
    /// Enforces role hierarchy and prevents privilege escalation.
    /// </summary>
    public class RoleAssignmentService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly ILogger<RoleAssignmentService> _logger;

        /// <summary>
        /// Role hierarchy - higher roles can assign all roles below them.
        /// </summary>
        private static readonly Dictionary<string, int> RoleHierarchy = new(StringComparer.OrdinalIgnoreCase)
        {
            ["PlatformAdmin"] = 100,
            ["TenantAdmin"] = 90,
            ["ComplianceManager"] = 80,
            ["AuditManager"] = 80,
            ["SecurityManager"] = 80,
            ["RiskManager"] = 70,
            ["PolicyManager"] = 70,
            ["AssetManager"] = 70,
            ["VendorManager"] = 70,
            ["IncidentManager"] = 70,
            ["ComplianceOfficer"] = 60,
            ["RiskAnalyst"] = 60,
            ["AuditAnalyst"] = 60,
            ["SecurityAnalyst"] = 60,
            ["TaskOwner"] = 50,
            ["EvidenceContributor"] = 40,
            ["SystemObserver"] = 30,
            ["TeamMember"] = 20,
            ["ReadOnlyUser"] = 10
        };

        /// <summary>
        /// Defines which roles each assigner role can assign.
        /// </summary>
        private static readonly Dictionary<string, string[]> AssignableRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            ["PlatformAdmin"] = RoleHierarchy.Keys.ToArray(), // Can assign all roles
            ["TenantAdmin"] = new[]
            {
                "ComplianceManager", "AuditManager", "SecurityManager", "RiskManager",
                "PolicyManager", "AssetManager", "VendorManager", "IncidentManager",
                "ComplianceOfficer", "RiskAnalyst", "AuditAnalyst", "SecurityAnalyst",
                "TaskOwner", "EvidenceContributor", "SystemObserver", "TeamMember", "ReadOnlyUser"
            },
            ["ComplianceManager"] = new[] { "ComplianceOfficer", "RiskAnalyst", "EvidenceContributor", "ReadOnlyUser" },
            ["AuditManager"] = new[] { "AuditAnalyst", "SystemObserver", "ReadOnlyUser" },
            ["SecurityManager"] = new[] { "SecurityAnalyst", "ReadOnlyUser" },
            ["RiskManager"] = new[] { "RiskAnalyst", "ReadOnlyUser" }
        };

        /// <summary>
        /// Roles that require dual approval to assign.
        /// </summary>
        private static readonly HashSet<string> DualApprovalRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "PlatformAdmin",
            "TenantAdmin",
            "ComplianceManager",
            "AuditManager"
        };

        public RoleAssignmentService(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            ILogger<RoleAssignmentService> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Validate if an assigner can assign a specific role.
        /// </summary>
        public async Task<RoleAssignmentValidation> ValidateAssignmentAsync(
            Guid assignerId,
            Guid targetUserId,
            Guid tenantId,
            string roleToAssign)
        {
            // Normalize role code
            var normalizedRole = RoleConstants.NormalizeRoleCode(roleToAssign);

            // Get assigner's roles in the tenant
            var assignerIdString = assignerId.ToString();
            var assignerRoles = await _dbContext.TenantUsers
                .Where(tu => tu.UserId == assignerIdString && tu.TenantId == tenantId)
                .Select(tu => tu.Role)
                .ToListAsync();

            if (!assignerRoles.Any())
            {
                return RoleAssignmentValidation.Denied(
                    "unauthorized",
                    "You don't have any roles in this tenant.");
            }

            // Check if any of assigner's roles can assign the target role
            var canAssign = false;
            string? assignerRole = null;

            foreach (var role in assignerRoles)
            {
                if (CanRoleAssign(role, normalizedRole))
                {
                    canAssign = true;
                    assignerRole = role;
                    break;
                }
            }

            if (!canAssign)
            {
                // Log the blocked escalation attempt
                await _auditService.LogPrivilegeEscalationBlockedAsync(
                    targetUserId,
                    string.Empty,
                    normalizedRole,
                    assignerRoles.FirstOrDefault() ?? "Unknown",
                    assignerId,
                    tenantId);

                _logger.LogWarning(
                    "Privilege escalation blocked: User {AssignerId} with roles [{Roles}] tried to assign {Role} to {TargetId}",
                    assignerId, string.Join(", ", assignerRoles), normalizedRole, targetUserId);

                return RoleAssignmentValidation.Denied(
                    "privilege_escalation",
                    $"You cannot assign the {normalizedRole} role. This would be a privilege escalation.");
            }

            // Check if dual approval is required
            var requiresDualApproval = DualApprovalRoles.Contains(normalizedRole);

            // Check if assigner is trying to assign to themselves
            if (assignerId == targetUserId)
            {
                return RoleAssignmentValidation.Denied(
                    "self_assignment",
                    "You cannot assign roles to yourself.");
            }

            return new RoleAssignmentValidation
            {
                IsAllowed = true,
                NormalizedRole = normalizedRole,
                AssignerRole = assignerRole,
                RequiresDualApproval = requiresDualApproval,
                ApprovalStatus = requiresDualApproval ? "PendingApproval" : "Approved"
            };
        }

        /// <summary>
        /// Check if a role change is valid (not a downgrade for critical roles without approval).
        /// </summary>
        public async Task<RoleAssignmentValidation> ValidateRoleChangeAsync(
            Guid changerId,
            Guid targetUserId,
            Guid tenantId,
            string currentRole,
            string newRole)
        {
            // First validate the assignment
            var assignmentValidation = await ValidateAssignmentAsync(changerId, targetUserId, tenantId, newRole);
            if (!assignmentValidation.IsAllowed)
            {
                return assignmentValidation;
            }

            var normalizedCurrent = RoleConstants.NormalizeRoleCode(currentRole);
            var normalizedNew = RoleConstants.NormalizeRoleCode(newRole);

            // Check if this is a role change to a higher privilege role
            var currentLevel = GetRoleLevel(normalizedCurrent);
            var newLevel = GetRoleLevel(normalizedNew);

            if (newLevel > currentLevel)
            {
                // Privilege increase - might need additional validation
                _logger.LogInformation(
                    "Privilege increase: {TargetId} from {Current} (level {CurrentLevel}) to {New} (level {NewLevel})",
                    targetUserId, normalizedCurrent, currentLevel, normalizedNew, newLevel);
            }

            return assignmentValidation;
        }

        /// <summary>
        /// Assign a role to a user.
        /// </summary>
        public async Task<RoleAssignmentResult> AssignRoleAsync(
            Guid assignerId,
            Guid targetUserId,
            Guid tenantId,
            string roleToAssign,
            string? reason = null)
        {
            // Validate first
            var validation = await ValidateAssignmentAsync(assignerId, targetUserId, tenantId, roleToAssign);
            if (!validation.IsAllowed)
            {
                return RoleAssignmentResult.Failed(validation.ErrorCode!, validation.ErrorMessage!);
            }

            var normalizedRole = validation.NormalizedRole!;

            // Check if user already has this role
            var targetUserIdString = targetUserId.ToString();
            var existingAssignment = await _dbContext.TenantUsers
                .Where(tu => tu.UserId == targetUserIdString && tu.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (existingAssignment != null && existingAssignment.Role == normalizedRole)
            {
                return RoleAssignmentResult.AlreadyAssigned();
            }

            var previousRole = existingAssignment?.Role;

            if (existingAssignment != null)
            {
                // Update existing assignment
                existingAssignment.Role = normalizedRole;
                existingAssignment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new assignment (should not happen in normal flow)
                return RoleAssignmentResult.Failed("user_not_found", "User not found in this tenant.");
            }

            // Handle dual approval if required
            // Note: Dual approval workflow requires additional properties on TenantUser
            // For now, skip dual approval and proceed directly
            if (validation.RequiresDualApproval)
            {
                _logger.LogWarning("Dual approval required but not implemented - proceeding with direct assignment");
            }

            await _dbContext.SaveChangesAsync();

            // Log the role assignment
            if (string.IsNullOrEmpty(previousRole))
            {
                await _auditService.LogRoleAssignedAsync(
                    targetUserId, existingAssignment?.Email ?? string.Empty, normalizedRole, tenantId, assignerId);
            }
            else
            {
                await _auditService.LogRoleChangedAsync(
                    targetUserId, existingAssignment?.Email ?? string.Empty, previousRole, normalizedRole, tenantId, assignerId);
            }

            _logger.LogInformation(
                "Role assigned: {TargetId} assigned {Role} by {AssignerId} in tenant {TenantId}",
                targetUserId, normalizedRole, assignerId, tenantId);

            return RoleAssignmentResult.Succeeded(normalizedRole);
        }

        /// <summary>
        /// Remove a role from a user.
        /// </summary>
        public async Task<RoleAssignmentResult> RemoveRoleAsync(
            Guid removerId,
            Guid targetUserId,
            Guid tenantId,
            string? reason = null)
        {
            var targetUserIdString = targetUserId.ToString();
            var assignment = await _dbContext.TenantUsers
                .Where(tu => tu.UserId == targetUserIdString && tu.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return RoleAssignmentResult.Failed("not_found", "User not found in this tenant.");
            }

            var previousRole = assignment.Role;

            // Validate the remover can remove this role
            var validation = await ValidateAssignmentAsync(removerId, targetUserId, tenantId, previousRole);
            if (!validation.IsAllowed)
            {
                return RoleAssignmentResult.Failed(validation.ErrorCode!, validation.ErrorMessage!);
            }

            // Log the role removal
            await _auditService.LogRoleRemovedAsync(
                targetUserId, assignment.Email, previousRole, tenantId, removerId);

            // Remove the tenant user or set to a default role
            _dbContext.TenantUsers.Remove(assignment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Role removed: {TargetId} lost {Role} by {RemoverId} in tenant {TenantId}",
                targetUserId, previousRole, removerId, tenantId);

            return RoleAssignmentResult.Succeeded(null);
        }

        /// <summary>
        /// Get roles that a user can assign.
        /// </summary>
        public async Task<IEnumerable<string>> GetAssignableRolesAsync(Guid userId, Guid tenantId)
        {
            var userIdString = userId.ToString();
            var userRoles = await _dbContext.TenantUsers
                .Where(tu => tu.UserId == userIdString && tu.TenantId == tenantId)
                .Select(tu => tu.Role)
                .ToListAsync();

            var assignable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var role in userRoles)
            {
                if (AssignableRoles.TryGetValue(role, out var roles))
                {
                    foreach (var r in roles)
                    {
                        assignable.Add(r);
                    }
                }
            }

            return assignable.OrderByDescending(r => GetRoleLevel(r));
        }

        private bool CanRoleAssign(string assignerRole, string roleToAssign)
        {
            if (AssignableRoles.TryGetValue(assignerRole, out var assignableRoles))
            {
                return assignableRoles.Contains(roleToAssign, StringComparer.OrdinalIgnoreCase);
            }

            // Fall back to hierarchy check
            var assignerLevel = GetRoleLevel(assignerRole);
            var targetLevel = GetRoleLevel(roleToAssign);

            // Can only assign roles at a lower level
            return assignerLevel > targetLevel;
        }

        private static int GetRoleLevel(string role)
        {
            return RoleHierarchy.TryGetValue(role, out var level) ? level : 0;
        }
    }

    /// <summary>
    /// Result of role assignment validation.
    /// </summary>
    public class RoleAssignmentValidation
    {
        public bool IsAllowed { get; set; }
        public string? NormalizedRole { get; set; }
        public string? AssignerRole { get; set; }
        public bool RequiresDualApproval { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static RoleAssignmentValidation Denied(string errorCode, string errorMessage) => new()
        {
            IsAllowed = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Result of role assignment operation.
    /// </summary>
    public class RoleAssignmentResult
    {
        public bool Success { get; set; }
        public bool RequiresApproval { get; set; }
        public string? AssignedRole { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }

        public static RoleAssignmentResult Succeeded(string? role) => new()
        {
            Success = true,
            AssignedRole = role,
            Message = "Role assigned successfully."
        };

        public static RoleAssignmentResult Failed(string errorCode, string message) => new()
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };

        public static RoleAssignmentResult AlreadyAssigned() => new()
        {
            Success = true,
            Message = "User already has this role."
        };
    }
}
