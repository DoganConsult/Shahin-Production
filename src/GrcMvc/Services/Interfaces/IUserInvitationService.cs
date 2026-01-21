using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// PHASE 6: User Invitation Service Interface
    /// Handles user invitations, role assignments, and activation
    /// </summary>
    public interface IUserInvitationService
    {
        /// <summary>
        /// Invite a user to a tenant with a specific role and title
        /// </summary>
        Task<TenantUser> InviteUserAsync(
            Guid tenantId,
            string email,
            string firstName,
            string lastName,
            string roleCode,
            string titleCode,
            string invitedBy);

        /// <summary>
        /// Accept invitation and activate user
        /// </summary>
        Task<TenantUser> AcceptInvitationAsync(
            string invitationToken,
            string password);

        /// <summary>
        /// Resend invitation email
        /// </summary>
        Task<bool> ResendInvitationAsync(Guid tenantUserId, string requestedBy);

        /// <summary>
        /// Cancel pending invitation
        /// </summary>
        Task<bool> CancelInvitationAsync(Guid tenantUserId, string cancelledBy);

        /// <summary>
        /// Get all users for a tenant
        /// </summary>
        Task<List<TenantUser>> GetTenantUsersAsync(Guid tenantId);

        /// <summary>
        /// Get pending invitations for a tenant
        /// </summary>
        Task<List<TenantUser>> GetPendingInvitationsAsync(Guid tenantId);

        /// <summary>
        /// Update user role and title
        /// </summary>
        Task<TenantUser> UpdateUserRoleAsync(
            Guid tenantUserId,
            string roleCode,
            string titleCode,
            string updatedBy);

        /// <summary>
        /// Suspend user access
        /// </summary>
        Task<bool> SuspendUserAsync(Guid tenantUserId, string suspendedBy);

        /// <summary>
        /// Reactivate suspended user
        /// </summary>
        Task<bool> ReactivateUserAsync(Guid tenantUserId, string reactivatedBy);

        /// <summary>
        /// Get user's tasks based on their role
        /// </summary>
        Task<List<WorkflowTask>> GetUserTasksAsync(Guid tenantId, string userId);
    }
}
