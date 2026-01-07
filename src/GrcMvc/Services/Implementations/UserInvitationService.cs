using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 6: User Invitation Service Implementation
    /// Handles user invitations, role assignments, and activation
    /// </summary>
    public class UserInvitationService : IUserInvitationService
    {
        private readonly GrcDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<UserInvitationService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public UserInvitationService(
            GrcDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IAuditEventService auditService,
            ILogger<UserInvitationService> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        public async Task<TenantUser> InviteUserAsync(
            Guid tenantId,
            string email,
            string firstName,
            string lastName,
            string roleCode,
            string titleCode,
            string invitedBy)
        {
            // Validate tenant exists
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new InvalidOperationException($"Tenant '{tenantId}' not found.");

            // Check if user already exists in this tenant
            var existingTenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId &&
                    tu.User.Email == email && !tu.IsDeleted);

            if (existingTenantUser != null)
                throw new InvalidOperationException($"User '{email}' is already a member of this tenant.");

            // Check if user exists in system
            var existingUser = await _userManager.FindByEmailAsync(email);
            string userId;

            if (existingUser != null)
            {
                userId = existingUser.Id;
            }
            else
            {
                // Create new user (inactive until invitation accepted)
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = false,
                    IsActive = false
                };

                var result = await _userManager.CreateAsync(newUser);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                userId = newUser.Id;
            }

            // Create tenant user with invitation
            var tenantUser = new TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                RoleCode = roleCode,
                TitleCode = titleCode,
                Status = "Pending",
                InvitationToken = GenerateInvitationToken(),
                InvitedAt = DateTime.UtcNow,
                InvitedBy = invitedBy,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = invitedBy
            };

            // Enforce policy before creating user invitation
            await _policyHelper.EnforceCreateAsync(
                resourceType: "TenantUser",
                resource: tenantUser,
                dataClassification: "confidential",
                owner: invitedBy);

            _context.TenantUsers.Add(tenantUser);
            await _context.SaveChangesAsync();

            // Send invitation email
            await SendInvitationEmailAsync(tenant, tenantUser, email, firstName);

            // Log audit event
            await _auditService.LogEventAsync(
                tenantId: tenantId,
                eventType: "UserInvited",
                affectedEntityType: "TenantUser",
                affectedEntityId: tenantUser.Id.ToString(),
                action: "Invite",
                actor: invitedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                {
                    Email = email,
                    RoleCode = roleCode,
                    TitleCode = titleCode
                }),
                correlationId: tenant.CorrelationId
            );

            _logger.LogInformation($"User invited: {email} to tenant {tenantId} with role {roleCode}");
            return tenantUser;
        }

        public async Task<TenantUser> AcceptInvitationAsync(string invitationToken, string password)
        {
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .Include(tu => tu.User)
                .FirstOrDefaultAsync(tu => tu.InvitationToken == invitationToken &&
                    tu.Status == "Pending" && !tu.IsDeleted);

            if (tenantUser == null)
                throw new InvalidOperationException("Invalid or expired invitation token.");

            // Activate user account
            var user = tenantUser.User;
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                user.IsActive = true;
                await _userManager.UpdateAsync(user);

                // Set password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to set password: {errors}");
                }
            }

            // Activate tenant user
            tenantUser.Status = "Active";
            tenantUser.ActivatedAt = DateTime.UtcNow;
            tenantUser.InvitationToken = string.Empty;
            tenantUser.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ROLE-BASED ACCESS: Assign user to existing tenant workspace (NOT create new)
            // Workspace is created ONCE during onboarding finalization - users get role-based access
            try
            {
                // Get the tenant's shared workspace (created during onboarding)
                var tenantWorkspace = await _context.Workspaces
                    .FirstOrDefaultAsync(w => w.TenantId == tenantUser.TenantId &&
                                              w.IsDefault && !w.IsDeleted);

                if (tenantWorkspace != null)
                {
                    // Check for existing membership
                    var existingMembership = await _context.WorkspaceMemberships
                        .FirstOrDefaultAsync(wm => wm.WorkspaceId == tenantWorkspace.Id &&
                                                   wm.UserId == user.Id && !wm.IsDeleted);

                    if (existingMembership == null)
                    {
                        // Create workspace membership with role-based permissions
                        var membership = new WorkspaceMembership
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantUser.TenantId,
                            WorkspaceId = tenantWorkspace.Id,
                            UserId = user.Id,
                            WorkspaceRolesJson = System.Text.Json.JsonSerializer.Serialize(new[] { tenantUser.RoleCode }),
                            IsPrimary = true,
                            IsWorkspaceAdmin = tenantUser.RoleCode == "TENANT_ADMIN" || tenantUser.RoleCode == "Admin",
                            Status = "Active",
                            JoinedDate = DateTime.UtcNow,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = tenantUser.InvitedBy ?? "system"
                        };
                        _context.WorkspaceMemberships.Add(membership);
                        await _context.SaveChangesAsync();
                    }

                    // Auto-map tasks from active assessments based on RACI
                    await AutoMapTasksForUserAsync(tenantUser.TenantId, user.Id, tenantUser.RoleCode);

                    _logger.LogInformation("Assigned user {UserId} to tenant workspace with role {RoleCode}",
                        user.Id, tenantUser.RoleCode);
                }
                else
                {
                    _logger.LogWarning("No default workspace found for tenant {TenantId}", tenantUser.TenantId);
                }
            }
            catch (Exception wsEx)
            {
                _logger.LogWarning(wsEx, "Failed to assign workspace access for user {UserId}", user.Id);
            }

            // Log audit event
            await _auditService.LogEventAsync(
                tenantId: tenantUser.TenantId,
                eventType: "UserActivated",
                affectedEntityType: "TenantUser",
                affectedEntityId: tenantUser.Id.ToString(),
                action: "Activate",
                actor: user.Id,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                {
                    UserId = user.Id,
                    Email = user.Email,
                    RoleCode = tenantUser.RoleCode,
                    WorkspaceAssigned = true
                }),
                correlationId: tenantUser.Tenant.CorrelationId
            );

            _logger.LogInformation($"User activated: {user.Email} in tenant {tenantUser.TenantId}");
            return tenantUser;
        }

        public async Task<bool> ResendInvitationAsync(Guid tenantUserId, string requestedBy)
        {
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .Include(tu => tu.User)
                .FirstOrDefaultAsync(tu => tu.Id == tenantUserId && !tu.IsDeleted);

            if (tenantUser == null || tenantUser.Status != "Pending")
                return false;

            // Generate new token
            tenantUser.InvitationToken = GenerateInvitationToken();
            tenantUser.InvitedAt = DateTime.UtcNow;
            tenantUser.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Resend email
            await SendInvitationEmailAsync(
                tenantUser.Tenant,
                tenantUser,
                tenantUser.User.Email!,
                tenantUser.User.FirstName);

            _logger.LogInformation($"Invitation resent for user {tenantUser.User.Email}");
            return true;
        }

        public async Task<bool> CancelInvitationAsync(Guid tenantUserId, string cancelledBy)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.Id == tenantUserId && !tu.IsDeleted);

            if (tenantUser == null || tenantUser.Status != "Pending")
                return false;

            tenantUser.Status = "Cancelled";
            tenantUser.InvitationToken = string.Empty;
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = cancelledBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Invitation cancelled for tenant user {tenantUserId}");
            return true;
        }

        public async Task<List<TenantUser>> GetTenantUsersAsync(Guid tenantId)
        {
            return await _context.TenantUsers
                .Include(tu => tu.User)
                .Where(tu => tu.TenantId == tenantId && !tu.IsDeleted)
                .OrderBy(tu => tu.User.LastName)
                .ThenBy(tu => tu.User.FirstName)
                .ToListAsync();
        }

        public async Task<List<TenantUser>> GetPendingInvitationsAsync(Guid tenantId)
        {
            return await _context.TenantUsers
                .Include(tu => tu.User)
                .Where(tu => tu.TenantId == tenantId &&
                    tu.Status == "Pending" && !tu.IsDeleted)
                .OrderByDescending(tu => tu.InvitedAt)
                .ToListAsync();
        }

        public async Task<TenantUser> UpdateUserRoleAsync(
            Guid tenantUserId,
            string roleCode,
            string titleCode,
            string updatedBy)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.Id == tenantUserId && !tu.IsDeleted);

            if (tenantUser == null)
                throw new InvalidOperationException("Tenant user not found.");

            var oldRoleCode = tenantUser.RoleCode;
            var oldTitleCode = tenantUser.TitleCode;

            tenantUser.RoleCode = roleCode;
            tenantUser.TitleCode = titleCode;
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();

            // Log audit event
            await _auditService.LogEventAsync(
                tenantId: tenantUser.TenantId,
                eventType: "UserRoleUpdated",
                affectedEntityType: "TenantUser",
                affectedEntityId: tenantUser.Id.ToString(),
                action: "Update",
                actor: updatedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                {
                    OldRoleCode = oldRoleCode,
                    NewRoleCode = roleCode,
                    OldTitleCode = oldTitleCode,
                    NewTitleCode = titleCode
                }),
                correlationId: null
            );

            _logger.LogInformation($"User role updated: {tenantUserId} from {oldRoleCode} to {roleCode}");
            return tenantUser;
        }

        public async Task<bool> SuspendUserAsync(Guid tenantUserId, string suspendedBy)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.Id == tenantUserId && !tu.IsDeleted);

            if (tenantUser == null || tenantUser.Status == "Suspended")
                return false;

            tenantUser.Status = "Suspended";
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = suspendedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User suspended: {tenantUserId}");
            return true;
        }

        public async Task<bool> ReactivateUserAsync(Guid tenantUserId, string reactivatedBy)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.Id == tenantUserId && !tu.IsDeleted);

            if (tenantUser == null || tenantUser.Status != "Suspended")
                return false;

            tenantUser.Status = "Active";
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = reactivatedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User reactivated: {tenantUserId}");
            return true;
        }

        public async Task<List<WorkflowTask>> GetUserTasksAsync(Guid tenantId, string userId)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId &&
                    tu.UserId == userId && !tu.IsDeleted);

            if (tenantUser == null)
                return new List<WorkflowTask>();

            // Get tasks assigned to user
            var userGuid = Guid.TryParse(userId, out var parsedId) ? parsedId : (Guid?)null;
            return await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                    t.AssignedToUserId == userGuid &&
                    t.Status != "Completed" && t.Status != "Cancelled" &&
                    !t.IsDeleted)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        private static string GenerateInvitationToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private async Task SendInvitationEmailAsync(
            Tenant tenant,
            TenantUser tenantUser,
            string email,
            string firstName)
        {
            var invitationUrl = $"https://grc.example.com/invitation/accept?token={tenantUser.InvitationToken}";

            await _emailService.SendEmailAsync(
                email,
                $"You've been invited to join {tenant.OrganizationName}",
                $@"Hello {firstName},

You have been invited to join {tenant.OrganizationName} on the GRC Platform.

Your Role: {tenantUser.RoleCode}
Your Title: {tenantUser.TitleCode}

Click the link below to accept your invitation and set up your account:
{invitationUrl}

This invitation will expire in 7 days.

If you did not expect this invitation, please ignore this email.

Best regards,
The GRC Platform Team"
            );
        }

        /// <summary>
        /// Auto-map tasks from active assessments based on RACI rules for user's role
        /// </summary>
        private async Task AutoMapTasksForUserAsync(Guid tenantId, string userId, string roleCode)
        {
            try
            {
                // Get RACI assignments where this role is Responsible (R)
                var raciAssignments = await _context.RACIAssignments
                    .Where(r => r.TenantId == tenantId &&
                                r.RoleCode == roleCode &&
                                r.RACI == "R" &&
                                r.IsActive && !r.IsDeleted)
                    .ToListAsync();

                if (!raciAssignments.Any())
                {
                    _logger.LogDebug("No RACI assignments found for role {RoleCode}", roleCode);
                    return;
                }

                // Get pending unassigned workflow tasks for this tenant
                var userGuid = Guid.TryParse(userId, out var parsed) ? parsed : (Guid?)null;

                var unassignedTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.WorkflowInstance.TenantId == tenantId &&
                                t.AssignedToUserId == null &&
                                t.Status == "Pending" && !t.IsDeleted)
                    .Take(20)
                    .ToListAsync();

                // Match tasks to RACI scope
                var scopeTypes = raciAssignments.Select(r => r.ScopeType).Distinct().ToList();
                var tasksToAssign = unassignedTasks
                    .Where(t => scopeTypes.Any(st => t.TaskName.Contains(st, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var task in tasksToAssign)
                {
                    task.AssignedToUserId = userGuid;
                    task.ModifiedDate = DateTime.UtcNow;
                }

                if (tasksToAssign.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Auto-mapped {Count} tasks to user {UserId} with role {RoleCode}",
                        tasksToAssign.Count, userId, roleCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-map tasks for user {UserId}", userId);
            }
        }
    }
}
