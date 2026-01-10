using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// REST API Controller for Approval Management
    /// Provides CRUD and workflow operations for approval instances
    /// 
    /// ASP.NET Best Practice: RESTful endpoints with proper authorization
    /// ABP Pattern: Repository-based controller with proper DI
    /// </summary>
    [ApiController]
    [Route("api/approvals")]
    [Authorize]
    public class ApprovalApiController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<ApprovalApiController> _logger;

        public ApprovalApiController(
            GrcDbContext context,
            ILogger<ApprovalApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/approvals - Get all approvals for current user's tenant
        /// Returns paginated list of approval instances
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetApprovals(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] string? entityType = null)
        {
            try
            {
                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                var query = _context.ApprovalInstances
                    .Where(a => a.TenantId == tenantId)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status);

                if (!string.IsNullOrEmpty(entityType))
                    query = query.Where(a => a.EntityType == entityType);

                var totalCount = await query.CountAsync();

                var approvals = await query
                    .OrderByDescending(a => a.InitiatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.InstanceNumber,
                        a.EntityId,
                        a.EntityType,
                        a.Status,
                        a.CurrentApproverRole,
                        a.CurrentStepIndex,
                        a.InitiatedAt,
                        a.CompletedAt,
                        a.InitiatedByUserName,
                        DaysOpen = a.CompletedAt.HasValue 
                            ? (a.CompletedAt.Value - a.InitiatedAt).Days 
                            : (DateTime.UtcNow - a.InitiatedAt).Days
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = approvals,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approvals");
                return StatusCode(500, new { success = false, error = "Error retrieving approvals" });
            }
        }

        /// <summary>
        /// GET /api/approvals/pending - Get pending approvals for current user
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var tenantId = GetUserTenantId();

                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                // Get pending approvals where user is the current approver
                var pendingApprovals = await _context.ApprovalInstances
                    .Where(a => a.TenantId == tenantId && 
                               (a.Status == "Pending" || a.Status == "InProgress"))
                    .Include(a => a.ApprovalChain)
                    .OrderBy(a => a.InitiatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.InstanceNumber,
                        a.EntityId,
                        a.EntityType,
                        a.Status,
                        a.CurrentApproverRole,
                        a.InitiatedAt,
                        a.InitiatedByUserName,
                        ChainName = a.ApprovalChain.Name,
                        DaysWaiting = (DateTime.UtcNow - a.InitiatedAt).Days,
                        IsOverdue = (DateTime.UtcNow - a.InitiatedAt).Days > 3 // Default 3-day SLA
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = pendingApprovals,
                    count = pendingApprovals.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals");
                return StatusCode(500, new { success = false, error = "Error retrieving pending approvals" });
            }
        }

        /// <summary>
        /// GET /api/approvals/escalated - Get escalated approvals
        /// </summary>
        [HttpGet("escalated")]
        public async Task<IActionResult> GetEscalatedApprovals()
        {
            try
            {
                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                // Get approvals that have exceeded SLA
                var escalatedApprovals = await _context.ApprovalInstances
                    .Where(a => a.TenantId == tenantId && 
                               (a.Status == "Pending" || a.Status == "InProgress") &&
                               (DateTime.UtcNow - a.InitiatedAt).Days > 3)
                    .Include(a => a.ApprovalChain)
                    .OrderBy(a => a.InitiatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.InstanceNumber,
                        a.EntityId,
                        a.EntityType,
                        a.Status,
                        a.CurrentApproverRole,
                        a.InitiatedAt,
                        a.InitiatedByUserName,
                        ChainName = a.ApprovalChain.Name,
                        DaysOverdue = (DateTime.UtcNow - a.InitiatedAt).Days - 3,
                        EscalationLevel = (DateTime.UtcNow - a.InitiatedAt).Days > 7 ? "Critical" : "Warning"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = escalatedApprovals,
                    count = escalatedApprovals.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting escalated approvals");
                return StatusCode(500, new { success = false, error = "Error retrieving escalated approvals" });
            }
        }

        /// <summary>
        /// GET /api/approvals/{id} - Get specific approval details
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetApproval(Guid id)
        {
            try
            {
                var tenantId = GetUserTenantId();

                var approval = await _context.ApprovalInstances
                    .Include(a => a.ApprovalChain)
                    .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

                if (approval == null)
                    return NotFound(new { success = false, error = "Approval not found" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        approval.Id,
                        approval.InstanceNumber,
                        approval.EntityId,
                        approval.EntityType,
                        approval.Status,
                        approval.CurrentApproverRole,
                        approval.CurrentStepIndex,
                        approval.InitiatedAt,
                        approval.CompletedAt,
                        approval.InitiatedByUserId,
                        approval.InitiatedByUserName,
                        approval.FinalDecision,
                        Chain = new
                        {
                            approval.ApprovalChain.Id,
                            approval.ApprovalChain.Name
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval {ApprovalId}", id);
                return StatusCode(500, new { success = false, error = "Error retrieving approval" });
            }
        }

        /// <summary>
        /// POST /api/approvals/{id}/approve - Approve an approval instance
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> ApproveApproval(Guid id, [FromBody] ApprovalActionInputDto? input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";
                var tenantId = GetUserTenantId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var approval = await _context.ApprovalInstances
                    .Include(a => a.ApprovalChain)
                    .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

                if (approval == null)
                    return NotFound(new { success = false, error = "Approval not found" });

                if (approval.Status == "Approved" || approval.Status == "Rejected")
                    return BadRequest(new { success = false, error = "Approval has already been completed" });

                // Update approval status
                approval.Status = "Approved";
                approval.CompletedAt = DateTime.UtcNow;
                approval.FinalDecision = System.Text.Json.JsonSerializer.Serialize(new
                {
                    decision = "Approved",
                    approverUserId = userId,
                    approverUserName = userName,
                    reason = input?.Comments,
                    decidedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                _logger.LogInformation("Approval {ApprovalId} approved by {UserId}", id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Approval completed successfully",
                    messageAr = "تمت الموافقة بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving approval {ApprovalId}", id);
                return StatusCode(500, new { success = false, error = "Error processing approval" });
            }
        }

        /// <summary>
        /// POST /api/approvals/{id}/reject - Reject an approval instance
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> RejectApproval(Guid id, [FromBody] ApprovalActionInputDto? input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";
                var tenantId = GetUserTenantId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                if (string.IsNullOrEmpty(input?.Comments))
                    return BadRequest(new { success = false, error = "Rejection reason is required" });

                var approval = await _context.ApprovalInstances
                    .Include(a => a.ApprovalChain)
                    .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

                if (approval == null)
                    return NotFound(new { success = false, error = "Approval not found" });

                if (approval.Status == "Approved" || approval.Status == "Rejected")
                    return BadRequest(new { success = false, error = "Approval has already been completed" });

                // Update approval status
                approval.Status = "Rejected";
                approval.CompletedAt = DateTime.UtcNow;
                approval.FinalDecision = System.Text.Json.JsonSerializer.Serialize(new
                {
                    decision = "Rejected",
                    approverUserId = userId,
                    approverUserName = userName,
                    reason = input.Comments,
                    decidedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                _logger.LogInformation("Approval {ApprovalId} rejected by {UserId}", id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Approval rejected",
                    messageAr = "تم الرفض"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting approval {ApprovalId}", id);
                return StatusCode(500, new { success = false, error = "Error processing rejection" });
            }
        }

        /// <summary>
        /// GET /api/approvals/stats - Get approval statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetApprovalStats()
        {
            try
            {
                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                var allApprovals = await _context.ApprovalInstances
                    .Where(a => a.TenantId == tenantId)
                    .ToListAsync();

                var stats = new ApprovalStatsDto
                {
                    TotalPending = allApprovals.Count(a => a.Status == "Pending" || a.Status == "InProgress"),
                    Overdue = allApprovals.Count(a => 
                        (a.Status == "Pending" || a.Status == "InProgress") && 
                        (DateTime.UtcNow - a.InitiatedAt).Days > 3),
                    AverageTurnaroundHours = (int)allApprovals
                        .Where(a => a.CompletedAt.HasValue)
                        .Select(a => (a.CompletedAt!.Value - a.InitiatedAt).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    ByStatus = allApprovals
                        .GroupBy(a => a.Status)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    CompletionRate = allApprovals.Count > 0 
                        ? Math.Round((double)allApprovals.Count(a => a.Status == "Approved" || a.Status == "Rejected") / allApprovals.Count * 100, 2)
                        : 0
                };

                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval stats");
                return StatusCode(500, new { success = false, error = "Error retrieving statistics" });
            }
        }

        #region Helper Methods

        private Guid GetUserTenantId()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value 
                            ?? User.FindFirst("TenantId")?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId))
                return tenantId;

            return Guid.Empty;
        }

        #endregion
    }

    #region Input DTOs

    /// <summary>
    /// DTO for approval action requests (approve, reject)
    /// </summary>
    public class ApprovalActionInputDto
    {
        /// <summary>Comments or reason for the action</summary>
        public string? Comments { get; set; }
    }

    #endregion
}
