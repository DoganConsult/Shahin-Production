using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// REST API for workspace management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceContextService _workspaceContext;
        private readonly IWorkspaceManagementService _workspaceService;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<WorkspaceController> _logger;

        public WorkspaceController(
            IWorkspaceContextService workspaceContext,
            IWorkspaceManagementService workspaceService,
            ITenantContextService tenantContext,
            ILogger<WorkspaceController> logger)
        {
            _workspaceContext = workspaceContext ?? throw new ArgumentNullException(nameof(workspaceContext));
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Switch current workspace context
        /// POST /api/workspace/switch
        /// </summary>
        [HttpPost("switch")]
        public async Task<IActionResult> SwitchWorkspace([FromBody] SwitchWorkspaceRequest request)
        {
            if (request == null || request.WorkspaceId == Guid.Empty)
            {
                return BadRequest(new { error = "WorkspaceId is required" });
            }

            try
            {
                // Validate user has access to this workspace
                var hasAccess = await _workspaceContext.ValidateWorkspaceAccessAsync(request.WorkspaceId);
                if (!hasAccess)
                {
                    return Forbid("User does not have access to this workspace");
                }

                // Switch workspace
                await _workspaceContext.SetCurrentWorkspaceAsync(request.WorkspaceId);

                _logger.LogInformation("User switched to workspace {WorkspaceId}", request.WorkspaceId);

                return Ok(new { 
                    success = true, 
                    workspaceId = request.WorkspaceId,
                    message = "Workspace switched successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized workspace switch attempt");
                return Forbid("Access denied.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching workspace");
                return StatusCode(500, new { error = "Failed to switch workspace" });
            }
        }

        /// <summary>
        /// Get current workspace
        /// GET /api/workspace/current
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentWorkspace()
        {
            try
            {
                var workspaceId = _workspaceContext.GetCurrentWorkspaceId();
                if (workspaceId == Guid.Empty)
                {
                    return Ok(new { workspaceId = (Guid?)null, message = "No workspace context" });
                }

                var workspace = await _workspaceService.GetWorkspaceAsync(workspaceId);
                if (workspace == null)
                {
                    return NotFound(new { error = "Workspace not found" });
                }

                return Ok(new
                {
                    workspaceId = workspace.Id,
                    workspaceCode = workspace.WorkspaceCode,
                    name = workspace.Name,
                    nameAr = workspace.NameAr,
                    workspaceType = workspace.WorkspaceType,
                    isDefault = workspace.IsDefault
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current workspace");
                return StatusCode(500, new { error = "Failed to get current workspace" });
            }
        }

        /// <summary>
        /// Get all workspaces for current tenant
        /// GET /api/workspace/list
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> ListWorkspaces()
        {
            try
            {
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId == Guid.Empty)
                {
                    return BadRequest(new { error = "No tenant context" });
                }

                var workspaces = await _workspaceService.GetTenantWorkspacesAsync(tenantId);
                var currentWorkspaceId = _workspaceContext.GetCurrentWorkspaceId();

                return Ok(workspaces.Select(w => new
                {
                    workspaceId = w.Id,
                    workspaceCode = w.WorkspaceCode,
                    name = w.Name,
                    nameAr = w.NameAr,
                    workspaceType = w.WorkspaceType,
                    isDefault = w.IsDefault,
                    isCurrent = w.Id == currentWorkspaceId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing workspaces");
                return StatusCode(500, new { error = "Failed to list workspaces" });
            }
        }

        /// <summary>
        /// Create a new workspace
        /// POST /api/workspace
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceApiRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            // Validate request
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
            {
                return BadRequest(new { errors = validationResults.Select(r => r.ErrorMessage) });
            }

            try
            {
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId == Guid.Empty)
                {
                    return BadRequest(new { error = "No tenant context" });
                }

                var createRequest = new CreateWorkspaceRequest
                {
                    TenantId = tenantId,
                    WorkspaceCode = request.WorkspaceCode,
                    Name = request.Name,
                    NameAr = request.NameAr,
                    WorkspaceType = request.WorkspaceType ?? "BusinessUnit",
                    JurisdictionCode = request.JurisdictionCode,
                    DefaultLanguage = request.DefaultLanguage ?? "ar",
                    Timezone = request.Timezone ?? "Asia/Riyadh",
                    Description = request.Description,
                    IsDefault = request.IsDefault,
                    Regulators = request.Regulators,
                    Overlays = request.Overlays,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                var workspace = await _workspaceService.CreateWorkspaceAsync(createRequest);

                _logger.LogInformation("Created workspace {WorkspaceCode} for tenant {TenantId}", 
                    workspace.WorkspaceCode, tenantId);

                return CreatedAtAction(nameof(GetWorkspace), new { id = workspace.Id }, new
                {
                    workspaceId = workspace.Id,
                    workspaceCode = workspace.WorkspaceCode,
                    name = workspace.Name,
                    nameAr = workspace.NameAr,
                    workspaceType = workspace.WorkspaceType,
                    isDefault = workspace.IsDefault,
                    status = workspace.Status,
                    createdDate = workspace.CreatedDate
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Workspace creation failed: duplicate code");
                return Conflict(new { error = "A conflict occurred with the current state." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Workspace creation failed: validation");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workspace");
                return StatusCode(500, new { error = "Failed to create workspace" });
            }
        }

        /// <summary>
        /// Get a specific workspace by ID
        /// GET /api/workspace/{id}
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetWorkspace(Guid id)
        {
            try
            {
                var workspace = await _workspaceService.GetWorkspaceAsync(id);
                if (workspace == null)
                {
                    return NotFound(new { error = "Workspace not found" });
                }

                // Verify tenant access
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId != Guid.Empty && workspace.TenantId != tenantId)
                {
                    return Forbid("Access denied to this workspace");
                }

                return Ok(new
                {
                    workspaceId = workspace.Id,
                    workspaceCode = workspace.WorkspaceCode,
                    name = workspace.Name,
                    nameAr = workspace.NameAr,
                    workspaceType = workspace.WorkspaceType,
                    jurisdictionCode = workspace.JurisdictionCode,
                    defaultLanguage = workspace.DefaultLanguage,
                    timezone = workspace.Timezone,
                    description = workspace.Description,
                    isDefault = workspace.IsDefault,
                    status = workspace.Status,
                    createdDate = workspace.CreatedDate,
                    memberCount = workspace.Memberships?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace {WorkspaceId}", id);
                return StatusCode(500, new { error = "Failed to get workspace" });
            }
        }

        /// <summary>
        /// Update a workspace
        /// PUT /api/workspace/{id}
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceApiRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            try
            {
                var workspace = await _workspaceService.GetWorkspaceAsync(id);
                if (workspace == null)
                {
                    return NotFound(new { error = "Workspace not found" });
                }

                // Verify tenant access
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId != Guid.Empty && workspace.TenantId != tenantId)
                {
                    return Forbid("Access denied to this workspace");
                }

                var updateRequest = new UpdateWorkspaceRequest
                {
                    Name = request.Name,
                    NameAr = request.NameAr,
                    Description = request.Description,
                    DefaultLanguage = request.DefaultLanguage,
                    Timezone = request.Timezone,
                    Status = request.Status,
                    Regulators = request.Regulators,
                    Overlays = request.Overlays,
                    ModifiedBy = User.Identity?.Name ?? "System"
                };

                var updated = await _workspaceService.UpdateWorkspaceAsync(id, updateRequest);

                _logger.LogInformation("Updated workspace {WorkspaceId}", id);

                return Ok(new
                {
                    workspaceId = updated.Id,
                    workspaceCode = updated.WorkspaceCode,
                    name = updated.Name,
                    nameAr = updated.NameAr,
                    status = updated.Status,
                    message = "Workspace updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace {WorkspaceId}", id);
                return StatusCode(500, new { error = "Failed to update workspace" });
            }
        }

        /// <summary>
        /// Set a workspace as default
        /// POST /api/workspace/{id}/set-default
        /// </summary>
        [HttpPost("{id:guid}/set-default")]
        public async Task<IActionResult> SetDefaultWorkspace(Guid id)
        {
            try
            {
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId == Guid.Empty)
                {
                    return BadRequest(new { error = "No tenant context" });
                }

                await _workspaceService.SetDefaultWorkspaceAsync(tenantId, id);

                _logger.LogInformation("Set workspace {WorkspaceId} as default for tenant {TenantId}", id, tenantId);

                return Ok(new { success = true, message = "Workspace set as default" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default workspace");
                return StatusCode(500, new { error = "Failed to set default workspace" });
            }
        }

        /// <summary>
        /// Add a member to a workspace
        /// POST /api/workspace/{id}/members
        /// </summary>
        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberApiRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { error = "UserId is required" });
            }

            try
            {
                var workspace = await _workspaceService.GetWorkspaceAsync(id);
                if (workspace == null)
                {
                    return NotFound(new { error = "Workspace not found" });
                }

                var addRequest = new AddWorkspaceMemberRequest
                {
                    TenantId = workspace.TenantId,
                    UserId = request.UserId,
                    WorkspaceRoles = request.WorkspaceRoles ?? new List<string> { "Member" },
                    IsPrimary = request.IsPrimary,
                    IsWorkspaceAdmin = request.IsWorkspaceAdmin,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                var membership = await _workspaceService.AddMemberAsync(id, addRequest);

                return Ok(new
                {
                    membershipId = membership.Id,
                    workspaceId = id,
                    userId = membership.UserId,
                    roles = request.WorkspaceRoles,
                    message = "Member added successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to workspace {WorkspaceId}", id);
                return StatusCode(500, new { error = "Failed to add member" });
            }
        }

        /// <summary>
        /// Get members of a workspace
        /// GET /api/workspace/{id}/members
        /// </summary>
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            try
            {
                var members = await _workspaceService.GetMembersAsync(id);

                return Ok(members.Select(m => new
                {
                    membershipId = m.Id,
                    userId = m.UserId,
                    userName = m.User?.UserName,
                    email = m.User?.Email,
                    isPrimary = m.IsPrimary,
                    isWorkspaceAdmin = m.IsWorkspaceAdmin,
                    status = m.Status,
                    joinedDate = m.JoinedDate
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members for workspace {WorkspaceId}", id);
                return StatusCode(500, new { error = "Failed to get members" });
            }
        }

        /// <summary>
        /// Remove a member from a workspace
        /// DELETE /api/workspace/{id}/members/{userId}
        /// </summary>
        [HttpDelete("{id:guid}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, string userId)
        {
            try
            {
                await _workspaceService.RemoveMemberAsync(id, userId);

                _logger.LogInformation("Removed member {UserId} from workspace {WorkspaceId}", userId, id);

                return Ok(new { success = true, message = "Member removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member from workspace {WorkspaceId}", id);
                return StatusCode(500, new { error = "Failed to remove member" });
            }
        }
    }

    #region Request DTOs

    public class SwitchWorkspaceRequest
    {
        public Guid WorkspaceId { get; set; }
    }

    public class CreateWorkspaceApiRequest
    {
        [Required(ErrorMessage = "WorkspaceCode is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "WorkspaceCode must be 2-50 characters")]
        [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "WorkspaceCode must be uppercase alphanumeric with _ or -")]
        public string WorkspaceCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be 2-200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NameAr { get; set; }

        [StringLength(50)]
        public string? WorkspaceType { get; set; }

        [StringLength(10)]
        public string? JurisdictionCode { get; set; }

        [StringLength(10)]
        public string? DefaultLanguage { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }

        public List<string>? Regulators { get; set; }
        public List<string>? Overlays { get; set; }
    }

    public class UpdateWorkspaceApiRequest
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? NameAr { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? DefaultLanguage { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public List<string>? Regulators { get; set; }
        public List<string>? Overlays { get; set; }
    }

    public class AddMemberApiRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        public List<string>? WorkspaceRoles { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsWorkspaceAdmin { get; set; }
    }

    #endregion
}
