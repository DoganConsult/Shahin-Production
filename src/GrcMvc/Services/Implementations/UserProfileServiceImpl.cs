using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of User Profile Service
    /// Manages the 14 predefined GRC profiles and user assignments
    /// </summary>
    public class UserProfileServiceImpl : IUserProfileService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<UserProfileServiceImpl> _logger;

        public UserProfileServiceImpl(GrcDbContext context, ILogger<UserProfileServiceImpl> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Profile CRUD

        public async Task<List<UserProfile>> GetAllProfilesAsync()
        {
            return await _context.UserProfiles
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<UserProfile?> GetProfileByIdAsync(Guid profileId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && !p.IsDeleted);
        }

        public async Task<UserProfile?> GetProfileByCodeAsync(string profileCode)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.ProfileCode == profileCode && !p.IsDeleted);
        }

        public async Task<UserProfile> CreateProfileAsync(UserProfile profile)
        {
            profile.Id = Guid.NewGuid();
            profile.CreatedDate = DateTime.UtcNow;
            profile.CreatedBy = "System";

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created user profile: {ProfileCode}", profile.ProfileCode);
            return profile;
        }

        public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
        {
            profile.ModifiedDate = DateTime.UtcNow;
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user profile: {ProfileCode}", profile.ProfileCode);
            return profile;
        }

        public async Task<bool> DeleteProfileAsync(Guid profileId)
        {
            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null || profile.IsSystemProfile)
                return false;

            profile.IsDeleted = true;
            profile.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted user profile: {ProfileId}", profileId);
            return true;
        }

        #endregion

        #region Profile Assignments

        public async Task<List<UserProfileAssignment>> GetUserAssignmentsAsync(string userId, Guid tenantId)
        {
            return await _context.UserProfileAssignments
                .Include(a => a.UserProfile)
                .Where(a => a.UserId == userId && a.TenantId == tenantId && a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<UserProfileAssignment>> GetProfileAssignmentsAsync(Guid profileId, Guid tenantId)
        {
            return await _context.UserProfileAssignments
                .Where(a => a.UserProfileId == profileId && a.TenantId == tenantId && a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<UserProfileAssignment> AssignProfileToUserAsync(string userId, Guid profileId, Guid tenantId, string assignedBy)
        {
            // Check if already assigned
            var existing = await _context.UserProfileAssignments
                .FirstOrDefaultAsync(a => a.UserId == userId && a.UserProfileId == profileId && a.TenantId == tenantId && !a.IsDeleted);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var assignment = new UserProfileAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                UserProfileId = profileId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = assignedBy
            };

            _context.UserProfileAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned profile {ProfileId} to user {UserId}", profileId, userId);
            return assignment;
        }

        public async Task<bool> RemoveProfileFromUserAsync(string userId, Guid profileId, Guid tenantId)
        {
            var assignment = await _context.UserProfileAssignments
                .FirstOrDefaultAsync(a => a.UserId == userId && a.UserProfileId == profileId && a.TenantId == tenantId && !a.IsDeleted);

            if (assignment == null)
                return false;

            assignment.IsActive = false;
            assignment.IsDeleted = true;
            assignment.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed profile {ProfileId} from user {UserId}", profileId, userId);
            return true;
        }

        #endregion

        #region Permission Checks

        public async Task<bool> HasPermissionAsync(string userId, Guid tenantId, string permissionCode)
        {
            var permissions = await GetUserPermissionsAsync(userId, tenantId);
            return permissions.Contains(permissionCode) || permissions.Contains("*");
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, Guid tenantId)
        {
            var assignments = await GetUserAssignmentsAsync(userId, tenantId);
            var permissions = new HashSet<string>();

            foreach (var assignment in assignments)
            {
                if (assignment.UserProfile != null && !string.IsNullOrEmpty(assignment.UserProfile.PermissionsJson))
                {
                    try
                    {
                        var profilePerms = JsonSerializer.Deserialize<List<string>>(assignment.UserProfile.PermissionsJson);
                        if (profilePerms != null)
                        {
                            foreach (var perm in profilePerms)
                                permissions.Add(perm);
                        }
                    }
                    catch (JsonException)
                    {
                        // Malformed JSON in PermissionsJson - skip this assignment's permissions
                    }
                }
            }

            return permissions.ToList();
        }

        public async Task<List<string>> GetUserWorkflowRolesAsync(string userId, Guid tenantId)
        {
            var assignments = await GetUserAssignmentsAsync(userId, tenantId);
            var roles = new HashSet<string>();

            foreach (var assignment in assignments)
            {
                if (assignment.UserProfile != null && !string.IsNullOrEmpty(assignment.UserProfile.WorkflowRolesJson))
                {
                    try
                    {
                        var profileRoles = JsonSerializer.Deserialize<List<string>>(assignment.UserProfile.WorkflowRolesJson);
                        if (profileRoles != null)
                        {
                            foreach (var role in profileRoles)
                                roles.Add(role);
                        }
                    }
                    catch (JsonException)
                    {
                        // Malformed JSON in WorkflowRolesJson - skip this assignment's roles
                    }
                }
            }

            return roles.ToList();
        }

        #endregion

        #region Seed Default Profiles

        public async Task SeedDefaultProfilesAsync()
        {
            var existingCount = await _context.UserProfiles.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("User profiles already seeded, skipping");
                return;
            }

            var profiles = GetDefaultProfiles();
            _context.UserProfiles.AddRange(profiles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} default user profiles", profiles.Count);
        }

        private List<UserProfile> GetDefaultProfiles()
        {
            return new List<UserProfile>
            {
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "PLATFORM_ADMIN",
                    ProfileName = "Platform Administrator",
                    Description = "Full platform access, manages tenants and global settings",
                    Category = "Admin",
                    DisplayOrder = 1,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"*\"]",
                    WorkflowRolesJson = "[\"ADMIN\",\"APPROVER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "TENANT_ADMIN",
                    ProfileName = "Tenant Administrator",
                    Description = "Full tenant access, manages users and tenant settings",
                    Category = "Admin",
                    DisplayOrder = 2,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"tenant.*\",\"users.*\",\"settings.*\"]",
                    WorkflowRolesJson = "[\"TENANT_ADMIN\",\"APPROVER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "GRC_MANAGER",
                    ProfileName = "GRC Manager",
                    Description = "Manages GRC program, oversees compliance and risk",
                    Category = "Compliance",
                    DisplayOrder = 3,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"risks.*\",\"controls.*\",\"assessments.*\",\"audits.*\",\"policies.*\",\"evidence.*\",\"reports.*\",\"workflows.*\"]",
                    WorkflowRolesJson = "[\"GRC_MANAGER\",\"APPROVER\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "COMPLIANCE_OFFICER",
                    ProfileName = "Compliance Officer",
                    Description = "Manages compliance requirements and assessments",
                    Category = "Compliance",
                    DisplayOrder = 4,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"assessments.*\",\"controls.read\",\"controls.update\",\"evidence.*\",\"policies.read\"]",
                    WorkflowRolesJson = "[\"COMPLIANCE_OFFICER\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "RISK_MANAGER",
                    ProfileName = "Risk Manager",
                    Description = "Manages risk identification, assessment, and mitigation",
                    Category = "Risk",
                    DisplayOrder = 5,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"risks.*\",\"controls.read\",\"assessments.read\",\"reports.risk.*\"]",
                    WorkflowRolesJson = "[\"RISK_MANAGER\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "CONTROL_OWNER",
                    ProfileName = "Control Owner",
                    Description = "Owns and maintains specific controls",
                    Category = "Risk",
                    DisplayOrder = 6,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"controls.read\",\"controls.update\",\"evidence.create\",\"evidence.read\"]",
                    WorkflowRolesJson = "[\"CONTROL_OWNER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "AUDITOR",
                    ProfileName = "Auditor",
                    Description = "Conducts audits and reviews evidence",
                    Category = "Audit",
                    DisplayOrder = 7,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"audits.*\",\"evidence.read\",\"controls.read\",\"assessments.read\",\"reports.audit.*\"]",
                    WorkflowRolesJson = "[\"AUDITOR\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "POLICY_OWNER",
                    ProfileName = "Policy Owner",
                    Description = "Creates and maintains policies",
                    Category = "Compliance",
                    DisplayOrder = 8,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"policies.*\"]",
                    WorkflowRolesJson = "[\"POLICY_OWNER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "DPO",
                    ProfileName = "Data Protection Officer",
                    Description = "Manages data privacy and PDPL compliance",
                    Category = "Privacy",
                    DisplayOrder = 9,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"privacy.*\",\"assessments.read\",\"evidence.read\",\"reports.privacy.*\"]",
                    WorkflowRolesJson = "[\"DPO\",\"REVIEWER\",\"APPROVER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "SECURITY_OFFICER",
                    ProfileName = "Security Officer",
                    Description = "Manages security controls and incidents",
                    Category = "Security",
                    DisplayOrder = 10,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"security.*\",\"controls.*\",\"risks.read\",\"incidents.*\"]",
                    WorkflowRolesJson = "[\"SECURITY_OFFICER\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "LEGAL_COUNSEL",
                    ProfileName = "Legal Counsel",
                    Description = "Reviews legal and regulatory matters",
                    Category = "Legal",
                    DisplayOrder = 11,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"policies.read\",\"contracts.*\",\"legal.*\"]",
                    WorkflowRolesJson = "[\"LEGAL_COUNSEL\",\"REVIEWER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "EXECUTIVE",
                    ProfileName = "Executive",
                    Description = "Executive dashboard and reporting access",
                    Category = "Executive",
                    DisplayOrder = 12,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"dashboard.executive\",\"reports.read\"]",
                    WorkflowRolesJson = "[\"EXECUTIVE\",\"FINAL_APPROVER\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "CONTRIBUTOR",
                    ProfileName = "Contributor",
                    Description = "Can submit evidence and complete assigned tasks",
                    Category = "General",
                    DisplayOrder = 13,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"evidence.create\",\"tasks.own\"]",
                    WorkflowRolesJson = "[\"CONTRIBUTOR\"]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = "VIEWER",
                    ProfileName = "Viewer",
                    Description = "Read-only access to assigned areas",
                    Category = "General",
                    DisplayOrder = 14,
                    IsSystemProfile = true,
                    PermissionsJson = "[\"*.read\"]",
                    WorkflowRolesJson = "[]",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };
        }

        #endregion
    }
}
