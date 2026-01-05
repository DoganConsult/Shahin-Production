using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing user profiles and permissions
    /// </summary>
    public interface IUserProfileService
    {
        // Profile CRUD
        Task<List<UserProfile>> GetAllProfilesAsync();
        Task<UserProfile?> GetProfileByIdAsync(Guid profileId);
        Task<UserProfile?> GetProfileByCodeAsync(string profileCode);
        Task<UserProfile> CreateProfileAsync(UserProfile profile);
        Task<UserProfile> UpdateProfileAsync(UserProfile profile);
        Task<bool> DeleteProfileAsync(Guid profileId);

        // Profile Assignments
        Task<List<UserProfileAssignment>> GetUserAssignmentsAsync(string userId, Guid tenantId);
        Task<List<UserProfileAssignment>> GetProfileAssignmentsAsync(Guid profileId, Guid tenantId);
        Task<UserProfileAssignment> AssignProfileToUserAsync(string userId, Guid profileId, Guid tenantId, string assignedBy);
        Task<bool> RemoveProfileFromUserAsync(string userId, Guid profileId, Guid tenantId);

        // Permission checks
        Task<bool> HasPermissionAsync(string userId, Guid tenantId, string permissionCode);
        Task<List<string>> GetUserPermissionsAsync(string userId, Guid tenantId);
        Task<List<string>> GetUserWorkflowRolesAsync(string userId, Guid tenantId);

        // Seed default profiles
        Task SeedDefaultProfilesAsync();
    }
}
