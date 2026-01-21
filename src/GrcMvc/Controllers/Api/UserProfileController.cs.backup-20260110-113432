using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _profileService;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserProfileService profileService,
            ITenantContextService tenantContext,
            ILogger<UserProfileController> logger)
        {
            _profileService = profileService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        /// <summary>
        /// Get all available profiles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<UserProfile>>> GetAllProfiles()
        {
            var profiles = await _profileService.GetAllProfilesAsync();
            return Ok(profiles);
        }

        /// <summary>
        /// Get profile by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfile>> GetProfile(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            if (profile == null)
                return NotFound();
            return Ok(profile);
        }

        /// <summary>
        /// Get profile by code
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<UserProfile>> GetProfileByCode(string code)
        {
            var profile = await _profileService.GetProfileByCodeAsync(code);
            if (profile == null)
                return NotFound();
            return Ok(profile);
        }

        /// <summary>
        /// Get current user's profile assignments
        /// </summary>
        [HttpGet("my-profiles")]
        public async Task<ActionResult<List<UserProfileAssignment>>> GetMyProfiles()
        {
            var userId = _tenantContext.GetCurrentUserId();
            var tenantId = _tenantContext.GetCurrentTenantId();
            var assignments = await _profileService.GetUserAssignmentsAsync(userId, tenantId);
            return Ok(assignments);
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        [HttpGet("my-permissions")]
        public async Task<ActionResult<List<string>>> GetMyPermissions()
        {
            var userId = _tenantContext.GetCurrentUserId();
            var tenantId = _tenantContext.GetCurrentTenantId();
            var permissions = await _profileService.GetUserPermissionsAsync(userId, tenantId);
            return Ok(permissions);
        }

        /// <summary>
        /// Get current user's workflow roles
        /// </summary>
        [HttpGet("my-workflow-roles")]
        public async Task<ActionResult<List<string>>> GetMyWorkflowRoles()
        {
            var userId = _tenantContext.GetCurrentUserId();
            var tenantId = _tenantContext.GetCurrentTenantId();
            var roles = await _profileService.GetUserWorkflowRolesAsync(userId, tenantId);
            return Ok(roles);
        }

        /// <summary>
        /// Assign profile to user (admin only)
        /// </summary>
        [HttpPost("assign")]
        public async Task<ActionResult<UserProfileAssignment>> AssignProfile([FromBody] AssignProfileRequest request)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var assignedBy = _tenantContext.GetCurrentUserName();

            var assignment = await _profileService.AssignProfileToUserAsync(
                request.UserId, request.ProfileId, tenantId, assignedBy);

            return Ok(assignment);
        }

        /// <summary>
        /// Remove profile from user (admin only)
        /// </summary>
        [HttpDelete("assign/{userId}/{profileId}")]
        public async Task<ActionResult> RemoveProfile(string userId, Guid profileId)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var result = await _profileService.RemoveProfileFromUserAsync(userId, profileId, tenantId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Check if current user has permission
        /// </summary>
        [HttpGet("has-permission/{permissionCode}")]
        public async Task<ActionResult<bool>> HasPermission(string permissionCode)
        {
            var userId = _tenantContext.GetCurrentUserId();
            var tenantId = _tenantContext.GetCurrentTenantId();
            var hasPermission = await _profileService.HasPermissionAsync(userId, tenantId, permissionCode);
            return Ok(hasPermission);
        }

        /// <summary>
        /// Seed default profiles (admin only)
        /// </summary>
        [HttpPost("seed")]
        public async Task<ActionResult> SeedProfiles()
        {
            await _profileService.SeedDefaultProfilesAsync();
            return Ok(new { message = "Default profiles seeded successfully" });
        }
    }

    public class AssignProfileRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProfileId { get; set; }
    }
}
