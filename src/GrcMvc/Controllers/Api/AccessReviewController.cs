using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Configuration;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// AM-04/AM-11: Access Review API Controller
/// Provides endpoints for managing access review campaigns, items, and decisions.
/// Control Reference: AM-04 (Access Reviews), AM-11 (Periodic Access Reviews)
/// </summary>
[Route("api/access-reviews")]
[Authorize]
public class AccessReviewController : GrcApiControllerBase
{
    private readonly IAccessReviewService _accessReviewService;
    private readonly ILogger<AccessReviewController> _logger;

    public AccessReviewController(
        IAccessReviewService accessReviewService,
        ILogger<AccessReviewController> logger)
    {
        _accessReviewService = accessReviewService;
        _logger = logger;
    }

    #region Review Campaign Endpoints

    /// <summary>
    /// Create a new access review campaign
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> CreateReview([FromBody] CreateAccessReviewDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var review = await _accessReviewService.CreateReviewAsync(dto, userId);

        _logger.LogInformation("Access review {ReviewCode} created by {UserId}", review.ReviewCode, userId);

        return Created(review, nameof(GetReview), new { id = review.Id }, "Access review created successfully");
    }

    /// <summary>
    /// Get a specific access review by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReview(Guid id)
    {
        var review = await _accessReviewService.GetReviewByIdAsync(id);
        if (review == null)
            return NotFoundError("Access review not found");

        return Success(review);
    }

    /// <summary>
    /// Get access review with all items
    /// </summary>
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetReviewDetails(Guid id)
    {
        var review = await _accessReviewService.GetReviewWithItemsAsync(id);
        if (review == null)
            return NotFoundError("Access review not found");

        return Success(review);
    }

    /// <summary>
    /// List access reviews for a tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListReviews(
        [FromQuery] Guid tenantId,
        [FromQuery] string? status = null,
        [FromQuery] string? reviewType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool includeCompleted = true)
    {
        var filter = new AccessReviewFilterDto
        {
            Status = status,
            ReviewType = reviewType,
            From = from,
            To = to,
            IncludeCompleted = includeCompleted
        };

        var reviews = await _accessReviewService.GetReviewsByTenantAsync(tenantId, filter);
        return Success(reviews);
    }

    /// <summary>
    /// Update an access review (only Draft/Scheduled status)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateAccessReviewDto dto)
    {
        var userId = GetCurrentUserId();
        var review = await _accessReviewService.UpdateReviewAsync(id, dto, userId ?? "system");

        if (review == null)
            return NotFoundError("Access review not found or cannot be updated in current status");

        return Success(review, "Access review updated successfully");
    }

    /// <summary>
    /// Cancel an access review
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin")]
    public async Task<IActionResult> CancelReview(Guid id, [FromBody] CancelReviewRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _accessReviewService.CancelReviewAsync(id, userId ?? "system", request.Reason);

        if (!result)
            return BadRequestError("Cannot cancel this review");

        return Success("Access review cancelled successfully");
    }

    /// <summary>
    /// Delete a draft access review
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _accessReviewService.DeleteReviewAsync(id, userId ?? "system");

        if (!result)
            return BadRequestError("Cannot delete this review (only draft reviews can be deleted)");

        return Success("Access review deleted successfully");
    }

    #endregion

    #region Review Execution Endpoints

    /// <summary>
    /// Start an access review (generates items and moves to InProgress)
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> StartReview(Guid id)
    {
        var userId = GetCurrentUserId();
        var review = await _accessReviewService.StartReviewAsync(id, userId ?? "system");

        if (review == null)
            return BadRequestError("Cannot start this review");

        return Success(review, $"Access review started with {review.TotalItems} items to review");
    }

    /// <summary>
    /// Schedule a review for future start
    /// </summary>
    [HttpPost("{id:guid}/schedule")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> ScheduleReview(Guid id, [FromBody] ScheduleReviewRequest request)
    {
        var userId = GetCurrentUserId();
        var review = await _accessReviewService.ScheduleReviewAsync(id, request.StartDate, userId ?? "system");

        if (review == null)
            return BadRequestError("Cannot schedule this review");

        return Success(review, "Access review scheduled successfully");
    }

    /// <summary>
    /// Get review items with pagination
    /// </summary>
    [HttpGet("{id:guid}/items")]
    public async Task<IActionResult> GetReviewItems(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null)
    {
        var result = await _accessReviewService.GetReviewItemsAsync(id, page, pageSize, status);
        return Success(result);
    }

    /// <summary>
    /// Make a decision on a review item
    /// </summary>
    [HttpPost("items/{itemId:guid}/decision")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer,Reviewer")]
    public async Task<IActionResult> MakeDecision(Guid itemId, [FromBody] MakeDecisionRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _accessReviewService.MakeDecisionAsync(
            itemId,
            request.Decision,
            userId ?? "system",
            request.Justification);

        if (item == null)
            return NotFoundError("Review item not found");

        return Success(item, "Decision recorded successfully");
    }

    /// <summary>
    /// Make bulk decisions on multiple items
    /// </summary>
    [HttpPost("{id:guid}/items/bulk-decision")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> BulkDecision(Guid id, [FromBody] BulkDecisionRequest request)
    {
        var userId = GetCurrentUserId();
        var count = await _accessReviewService.BulkDecisionAsync(
            request.ItemIds,
            request.Decision,
            userId ?? "system",
            request.Justification);

        return Success(new { UpdatedCount = count }, $"{count} items updated");
    }

    /// <summary>
    /// Flag a review item for attention
    /// </summary>
    [HttpPost("items/{itemId:guid}/flag")]
    public async Task<IActionResult> FlagItem(Guid itemId, [FromBody] FlagItemRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _accessReviewService.FlagItemAsync(itemId, userId ?? "system", request.Reason);

        if (item == null)
            return NotFoundError("Review item not found");

        return Success(item, "Item flagged successfully");
    }

    /// <summary>
    /// Complete a review (all items must be decided)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin,ComplianceOfficer")]
    public async Task<IActionResult> CompleteReview(Guid id, [FromBody] CompleteReviewRequest? request = null)
    {
        var userId = GetCurrentUserId();
        var review = await _accessReviewService.CompleteReviewAsync(id, userId ?? "system", request?.Notes);

        if (review == null)
            return BadRequestError("Cannot complete review - ensure all items have decisions");

        return Success(review, "Access review completed successfully");
    }

    /// <summary>
    /// Approve a completed review
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin")]
    public async Task<IActionResult> ApproveReview(Guid id)
    {
        var userId = GetCurrentUserId();
        var review = await _accessReviewService.ApproveReviewAsync(id, userId ?? "system");

        if (review == null)
            return BadRequestError("Cannot approve this review");

        return Success(review, "Access review approved successfully");
    }

    #endregion

    #region Remediation Endpoints

    /// <summary>
    /// Execute remediation for revoked access items
    /// </summary>
    [HttpPost("{id:guid}/remediate")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin")]
    public async Task<IActionResult> ExecuteRemediation(Guid id)
    {
        var userId = GetCurrentUserId();
        var count = await _accessReviewService.ExecuteRemediationAsync(id, userId ?? "system");

        return Success(new { RemediatedCount = count }, $"{count} access items remediated");
    }

    /// <summary>
    /// Complete remediation for a specific item
    /// </summary>
    [HttpPost("items/{itemId:guid}/remediation/complete")]
    [Authorize(Roles = "TenantAdmin,SecurityAdmin")]
    public async Task<IActionResult> CompleteRemediation(Guid itemId, [FromBody] CompleteRemediationRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _accessReviewService.CompleteRemediationAsync(itemId, userId ?? "system", request.Notes);

        if (item == null)
            return NotFoundError("Review item not found");

        return Success(item, "Remediation completed");
    }

    /// <summary>
    /// Get items pending remediation for a tenant
    /// </summary>
    [HttpGet("remediation/pending")]
    public async Task<IActionResult> GetPendingRemediation([FromQuery] Guid tenantId)
    {
        var items = await _accessReviewService.GetPendingRemediationItemsAsync(tenantId);
        return Success(items);
    }

    #endregion

    #region Reporting Endpoints

    /// <summary>
    /// Get access review statistics for a tenant
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var stats = await _accessReviewService.GetReviewStatisticsAsync(tenantId, from, to);
        return Success(stats);
    }

    /// <summary>
    /// Get compliance summary for AM-11 control
    /// </summary>
    [HttpGet("compliance-summary")]
    public async Task<IActionResult> GetComplianceSummary([FromQuery] Guid tenantId)
    {
        var summary = await _accessReviewService.GetComplianceSummaryAsync(tenantId);
        return Success(summary);
    }

    /// <summary>
    /// Export review results to CSV
    /// </summary>
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> ExportResults(Guid id, [FromQuery] string format = "csv")
    {
        var data = await _accessReviewService.ExportReviewResultsAsync(id, format);

        if (data.Length == 0)
            return NotFoundError("Review not found");

        var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";
        var fileName = $"access-review-{id:N}.{format}";

        return File(data, contentType, fileName);
    }

    /// <summary>
    /// Generate a detailed report for a review
    /// </summary>
    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> GenerateReport(Guid id)
    {
        var report = await _accessReviewService.GenerateReportAsync(id);
        return Success(report);
    }

    /// <summary>
    /// Get overdue reviews
    /// </summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueReviews([FromQuery] Guid? tenantId = null)
    {
        var reviews = await _accessReviewService.GetOverdueReviewsAsync(tenantId);
        return Success(reviews);
    }

    #endregion
}

#region DTOs

public class CancelReviewRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ScheduleReviewRequest
{
    public DateTime StartDate { get; set; }
}

public class MakeDecisionRequest
{
    public string Decision { get; set; } = string.Empty; // Keep, Revoke, Modify, Escalate
    public string? Justification { get; set; }
}

public class BulkDecisionRequest
{
    public List<Guid> ItemIds { get; set; } = new();
    public string Decision { get; set; } = string.Empty;
    public string? Justification { get; set; }
}

public class FlagItemRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CompleteReviewRequest
{
    public string? Notes { get; set; }
}

public class CompleteRemediationRequest
{
    public string Notes { get; set; } = string.Empty;
}

#endregion
