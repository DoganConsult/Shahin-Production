using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using System.Security.Claims;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// AM-04: Access Review API endpoints for periodic access certification.
    /// </summary>
    [ApiController]
    [Route("api/access-reviews")]
    [Authorize]
    public class AccessReviewController : ControllerBase
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

        /// <summary>
        /// Create a new access review for a tenant.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateAccessReviewRequest request)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var userId = GetCurrentUserId();

                var result = await _accessReviewService.CreateReviewAsync(
                    tenantId,
                    userId,
                    request.ReviewType,
                    request.Name,
                    request.Description,
                    request.DueDays);

                if (result.Success)
                {
                    return Ok(new { 
                        success = true,
                        reviewId = result.ReviewId,
                        itemCount = result.ItemCount,
                        message = "Access review created successfully"
                    });
                }

                return BadRequest(new { 
                    success = false, 
                    error = result.ErrorMessage 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access review");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Start an access review (move from Draft to InProgress).
        /// </summary>
        [HttpPost("{reviewId}/start")]
        public async Task<IActionResult> StartReview(Guid reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _accessReviewService.StartReviewAsync(reviewId, userId);

                if (success)
                {
                    return Ok(new { 
                        success = true,
                        message = "Access review started successfully"
                    });
                }

                return BadRequest(new { 
                    success = false, 
                    error = "Failed to start review" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting access review {ReviewId}", reviewId);
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get access review details with items.
        /// </summary>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReview(Guid reviewId)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var review = await _accessReviewService.GetReviewAsync(reviewId, tenantId);

                if (review == null)
                {
                    return NotFound(new { 
                        success = false, 
                        error = "Review not found" 
                    });
                }

                return Ok(new { 
                    success = true, 
                    review = review 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access review {ReviewId}", reviewId);
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// List all access reviews for the current tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListReviews([FromQuery] AccessReviewListRequest request)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var reviews = await _accessReviewService.ListReviewsAsync(
                    tenantId,
                    request.Status,
                    request.Page,
                    request.PageSize);

                return Ok(new { 
                    success = true, 
                    reviews = reviews.Reviews,
                    totalCount = reviews.TotalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing access reviews");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Submit a decision for a review item.
        /// </summary>
        [HttpPost("{reviewId}/items/{itemId}/decision")]
        public async Task<IActionResult> SubmitDecision(
            Guid reviewId, 
            Guid itemId, 
            [FromBody] SubmitDecisionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _accessReviewService.SubmitDecisionAsync(
                    itemId,
                    userId,
                    request.Decision,
                    request.NewRole,
                    request.Justification);

                if (success)
                {
                    return Ok(new { 
                        success = true,
                        message = "Decision submitted successfully"
                    });
                }

                return BadRequest(new { 
                    success = false, 
                    error = "Failed to submit decision" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting decision for item {ItemId}", itemId);
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Complete an access review.
        /// </summary>
        [HttpPost("{reviewId}/complete")]
        public async Task<IActionResult> CompleteReview(Guid reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _accessReviewService.CompleteReviewAsync(reviewId, userId);

                if (success)
                {
                    return Ok(new { 
                        success = true,
                        message = "Access review completed successfully"
                    });
                }

                return BadRequest(new { 
                    success = false, 
                    error = "Failed to complete review" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing access review {ReviewId}", reviewId);
                return StatusCode(500, new { 
                    success = false, 
                    error = "Internal server error" 
                });
            }
        }

        private Guid GetCurrentTenantId()
        {
            // Implement based on your tenant resolution strategy
            // This could come from claims, headers, or context
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantIdClaim, out var id) ? id : Guid.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
        }
    }

    public class CreateAccessReviewRequest
    {
        public string ReviewType { get; set; } = "Quarterly";
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DueDays { get; set; }
    }

    public class AccessReviewListRequest
    {
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SubmitDecisionRequest
    {
        public string Decision { get; set; } = "Certified";
        public string? NewRole { get; set; }
        public string? Justification { get; set; }
    }
}
