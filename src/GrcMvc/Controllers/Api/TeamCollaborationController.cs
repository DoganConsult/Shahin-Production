using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Team Collaboration API Controller
    /// Handles intra-organization team collaboration scenarios
    /// </summary>
    [Route("api/team")]
    [ApiController]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class TeamCollaborationController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<TeamCollaborationController> _logger;

        public TeamCollaborationController(
            GrcDbContext context,
            ILogger<TeamCollaborationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Workspace Management

        /// <summary>
        /// Create a collaboration workspace
        /// POST /api/team/workspaces
        /// </summary>
        [HttpPost("workspaces")]
        public async Task<IActionResult> CreateWorkspace([FromBody] CollaborationWorkspaceRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var workspace = new CollaborationWorkspace
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = request.Name,
                    Description = request.Description,
                    Type = request.Type ?? "general",
                    FrameworkId = request.FrameworkId,
                    ProjectId = request.ProjectId,
                    MembersJson = JsonSerializer.Serialize(new List<WorkspaceMember>
                    {
                        new() { UserId = userId, Role = "owner", JoinedAt = DateTime.UtcNow }
                    }),
                    SettingsJson = JsonSerializer.Serialize(request.Settings ?? new WorkspaceSettings()),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Set<CollaborationWorkspace>().Add(workspace);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Workspace created: {WorkspaceId} by {UserId}", workspace.Id, userId);

                return Ok(new
                {
                    success = true,
                    workspaceId = workspace.Id,
                    message = "Workspace created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workspace");
                return StatusCode(500, new { error = "Failed to create workspace" });
            }
        }

        /// <summary>
        /// Get workspaces for tenant
        /// GET /api/team/workspaces
        /// </summary>
        [HttpGet("workspaces")]
        public async Task<IActionResult> GetWorkspaces()
        {
            try
            {
                var tenantId = GetTenantId();

                var rawWorkspaces = await _context.Set<CollaborationWorkspace>()
                    .Where(w => w.TenantId == tenantId && w.IsActive)
                    .OrderByDescending(w => w.CreatedDate)
                    .ToListAsync();

                var workspaces = rawWorkspaces.Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Description,
                    w.Type,
                    w.FrameworkId,
                    w.ProjectId,
                    MemberCount = w.MembersJson != null ? JsonSerializer.Deserialize<List<WorkspaceMember>>(w.MembersJson)?.Count ?? 0 : 0,
                    w.CreatedDate,
                    w.CreatedBy
                }).ToList();

                return Ok(new { total = workspaces.Count, workspaces });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces");
                return StatusCode(500, new { error = "Failed to get workspaces" });
            }
        }

        /// <summary>
        /// Add member to workspace
        /// POST /api/team/workspaces/{workspaceId}/members
        /// </summary>
        [HttpPost("workspaces/{workspaceId}/members")]
        public async Task<IActionResult> AddMember(Guid workspaceId, [FromBody] AddMemberRequest request)
        {
            try
            {
                var workspace = await _context.Set<CollaborationWorkspace>().FindAsync(workspaceId);
                if (workspace == null)
                    return NotFound(new { error = "Workspace not found" });

                var members = JsonSerializer.Deserialize<List<WorkspaceMember>>(workspace.MembersJson ?? "[]") ?? new();

                if (members.Any(m => m.UserId == request.UserId))
                    return BadRequest(new { error = "User is already a member" });

                members.Add(new WorkspaceMember
                {
                    UserId = request.UserId,
                    Role = request.Role ?? "member",
                    JoinedAt = DateTime.UtcNow
                });

                workspace.MembersJson = JsonSerializer.Serialize(members);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Member added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to workspace");
                return StatusCode(500, new { error = "Failed to add member" });
            }
        }

        #endregion

        #region Collaboration Items (Tasks, Documents, Discussions)

        /// <summary>
        /// Create collaboration item
        /// POST /api/team/workspaces/{workspaceId}/items
        /// </summary>
        [HttpPost("workspaces/{workspaceId}/items")]
        public async Task<IActionResult> CreateItem(Guid workspaceId, [FromBody] CreateItemRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var item = new CollaborationItem
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    TenantId = tenantId,
                    ItemType = request.ItemType,
                    Title = request.Title,
                    Content = request.Content,
                    Status = request.Status ?? "open",
                    Priority = request.Priority ?? "medium",
                    AssignedTo = request.AssignedTo,
                    DueDate = request.DueDate,
                    TagsJson = JsonSerializer.Serialize(request.Tags ?? new List<string>()),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Set<CollaborationItem>().Add(item);
                await _context.SaveChangesAsync();

                // Log team activity
                await LogTeamActivity(tenantId, userId, "item_created", $"Created {request.ItemType}: {request.Title}", workspaceId);

                return Ok(new
                {
                    success = true,
                    itemId = item.Id,
                    message = "Item created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return StatusCode(500, new { error = "Failed to create item" });
            }
        }

        /// <summary>
        /// Get items in workspace
        /// GET /api/team/workspaces/{workspaceId}/items
        /// </summary>
        [HttpGet("workspaces/{workspaceId}/items")]
        public async Task<IActionResult> GetItems(Guid workspaceId, [FromQuery] string? type = null, [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Set<CollaborationItem>()
                    .Where(i => i.WorkspaceId == workspaceId);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(i => i.ItemType == type);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(i => i.Status == status);

                var items = await query
                    .OrderByDescending(i => i.CreatedDate)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemType,
                        i.Title,
                        i.Status,
                        i.Priority,
                        i.AssignedTo,
                        i.DueDate,
                        i.CommentsCount,
                        i.CreatedDate,
                        i.CreatedBy
                    })
                    .ToListAsync();

                return Ok(new { total = items.Count, items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items");
                return StatusCode(500, new { error = "Failed to get items" });
            }
        }

        /// <summary>
        /// Update item status
        /// PATCH /api/team/items/{itemId}/status
        /// </summary>
        [HttpPatch("items/{itemId}/status")]
        public async Task<IActionResult> UpdateItemStatus(Guid itemId, [FromBody] CollaborationUpdateStatusRequest request)
        {
            try
            {
                var item = await _context.Set<CollaborationItem>().FindAsync(itemId);
                if (item == null)
                    return NotFound(new { error = "Item not found" });

                var oldStatus = item.Status;
                item.Status = request.Status;
                item.ModifiedDate = DateTime.UtcNow;
                item.ModifiedBy = GetUserId();

                await _context.SaveChangesAsync();

                await LogTeamActivity(item.TenantId, GetUserId(), "status_changed",
                    $"Changed status from {oldStatus} to {request.Status}: {item.Title}", item.WorkspaceId);

                return Ok(new { success = true, message = "Status updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item status");
                return StatusCode(500, new { error = "Failed to update status" });
            }
        }

        /// <summary>
        /// Assign item to user
        /// PATCH /api/team/items/{itemId}/assign
        /// </summary>
        [HttpPatch("items/{itemId}/assign")]
        public async Task<IActionResult> AssignItem(Guid itemId, [FromBody] AssignItemRequest request)
        {
            try
            {
                var item = await _context.Set<CollaborationItem>().FindAsync(itemId);
                if (item == null)
                    return NotFound(new { error = "Item not found" });

                item.AssignedTo = request.AssignedTo;
                item.ModifiedDate = DateTime.UtcNow;
                item.ModifiedBy = GetUserId();

                await _context.SaveChangesAsync();

                await LogTeamActivity(item.TenantId, GetUserId(), "item_assigned",
                    $"Assigned {item.Title} to user", item.WorkspaceId);

                return Ok(new { success = true, message = "Item assigned" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning item");
                return StatusCode(500, new { error = "Failed to assign item" });
            }
        }

        #endregion

        #region Comments & Discussions

        /// <summary>
        /// Add comment to item
        /// POST /api/team/items/{itemId}/comments
        /// </summary>
        [HttpPost("items/{itemId}/comments")]
        public async Task<IActionResult> AddComment(Guid itemId, [FromBody] AddCommentRequest request)
        {
            try
            {
                var item = await _context.Set<CollaborationItem>().FindAsync(itemId);
                if (item == null)
                    return NotFound(new { error = "Item not found" });

                var userId = GetUserId();

                var comment = new CollaborationComment
                {
                    Id = Guid.NewGuid(),
                    ItemId = itemId,
                    TenantId = item.TenantId,
                    AuthorId = Guid.TryParse(userId, out var authorGuid) ? authorGuid : Guid.Empty,
                    Content = request.Content,
                    MentionsJson = JsonSerializer.Serialize(request.Mentions ?? new List<string>()),
                    ParentCommentId = request.ParentCommentId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Set<CollaborationComment>().Add(comment);

                // Update comment count
                item.CommentsCount++;

                await _context.SaveChangesAsync();

                await LogTeamActivity(item.TenantId, userId, "comment_added",
                    $"Commented on: {item.Title}", item.WorkspaceId);

                return Ok(new
                {
                    success = true,
                    commentId = comment.Id,
                    message = "Comment added"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, new { error = "Failed to add comment" });
            }
        }

        /// <summary>
        /// Get comments for item
        /// GET /api/team/items/{itemId}/comments
        /// </summary>
        [HttpGet("items/{itemId}/comments")]
        public async Task<IActionResult> GetComments(Guid itemId)
        {
            try
            {
                var comments = await _context.Set<CollaborationComment>()
                    .Where(c => c.ItemId == itemId)
                    .OrderBy(c => c.CreatedDate)
                    .Select(c => new
                    {
                        c.Id,
                        c.AuthorId,
                        c.Content,
                        c.ParentCommentId,
                        c.IsEdited,
                        c.CreatedDate
                    })
                    .ToListAsync();

                return Ok(new { total = comments.Count, comments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments");
                return StatusCode(500, new { error = "Failed to get comments" });
            }
        }

        #endregion

        #region Activity Feed

        /// <summary>
        /// Get team activity feed
        /// GET /api/team/activity
        /// </summary>
        [HttpGet("activity")]
        public async Task<IActionResult> GetActivityFeed([FromQuery] int limit = 50)
        {
            try
            {
                var tenantId = GetTenantId();

                var activities = await _context.Set<TeamActivity>()
                    .Where(a => a.TenantId == tenantId)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(limit)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.ActivityType,
                        a.Description,
                        a.Module,
                        a.Action,
                        a.ResourceType,
                        a.CreatedDate
                    })
                    .ToListAsync();

                return Ok(new { total = activities.Count, activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity feed");
                return StatusCode(500, new { error = "Failed to get activity feed" });
            }
        }

        #endregion

        #region Helpers

        private Guid GetTenantId()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system";
        }

        private async Task LogTeamActivity(Guid tenantId, string userId, string activityType, string description, Guid? workspaceId = null)
        {
            var activity = new TeamActivity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = Guid.TryParse(userId, out var userGuid) ? userGuid : Guid.Empty,
                ActivityType = activityType,
                Description = description,
                MetadataJson = workspaceId.HasValue ? JsonSerializer.Serialize(new { workspaceId }) : null,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Set<TeamActivity>().Add(activity);
            await _context.SaveChangesAsync();
        }

        #endregion
    }

    #region Request/Response DTOs

    public class CollaborationWorkspaceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? ProjectId { get; set; }
        public WorkspaceSettings? Settings { get; set; }
    }

    public class WorkspaceSettings
    {
        public bool AllowGuestAccess { get; set; }
        public bool RequireApproval { get; set; }
        public List<string> NotificationPreferences { get; set; } = new();
    }

    public class WorkspaceMember
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = "member";
        public DateTime JoinedAt { get; set; }
    }

    public class AddMemberRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class CreateItemRequest
    {
        public string ItemType { get; set; } = "task";
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class CollaborationUpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class AssignItemRequest
    {
        public Guid? AssignedTo { get; set; }
    }

    public class AddCommentRequest
    {
        public string Content { get; set; } = string.Empty;
        public List<string>? Mentions { get; set; }
        public Guid? ParentCommentId { get; set; }
    }

    #endregion
}
