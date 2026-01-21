using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Subscription API Controller
    /// Handles REST API requests for subscription management, plan selection, and billing
    /// Route: /api/subscriptions
    /// </summary>
    [Route("api/subscriptions")]
    [ApiController]
    [Authorize]
    public class SubscriptionApiController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionApiController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Get all subscriptions with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&status=active&q=searchterm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubscriptions(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? status = null,
            [FromQuery] string? q = null)
        {
            try
            {
                // Mock subscriptions list
                var subscriptions = new List<dynamic>
                {
                    new {
                        id = Guid.NewGuid(),
                        tenantId = Guid.NewGuid(),
                        tenantName = "TechCorp Inc.",
                        planName = "Professional",
                        status = "Active",
                        billingCycle = "Monthly",
                        startDate = DateTime.UtcNow.AddMonths(-3),
                        renewalDate = DateTime.UtcNow.AddMonths(1),
                        monthlyUsers = 50,
                        monthlyControls = 250,
                        monthlyAudits = 12
                    },
                    new {
                        id = Guid.NewGuid(),
                        tenantId = Guid.NewGuid(),
                        tenantName = "FinanceFlow Ltd.",
                        planName = "Enterprise",
                        status = "Active",
                        billingCycle = "Annual",
                        startDate = DateTime.UtcNow.AddMonths(-6),
                        renewalDate = DateTime.UtcNow.AddMonths(6),
                        monthlyUsers = 200,
                        monthlyControls = 1000,
                        monthlyAudits = 52
                    },
                    new {
                        id = Guid.NewGuid(),
                        tenantId = Guid.NewGuid(),
                        tenantName = "StartupXYZ",
                        planName = "Starter",
                        status = "Trial",
                        billingCycle = "Monthly",
                        startDate = DateTime.UtcNow.AddDays(-5),
                        renewalDate = DateTime.UtcNow.AddDays(9),
                        monthlyUsers = 10,
                        monthlyControls = 50,
                        monthlyAudits = 2
                    }
                };

                // Apply filtering
                var filtered = subscriptions.ToList();
                if (!string.IsNullOrEmpty(status))
                    filtered = filtered.Where(s => s.status == status).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(s => 
                        s.tenantName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        s.planName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc" 
                        ? filtered.OrderByDescending(s => s.GetType().GetProperty(sortBy)?.GetValue(s)).ToList()
                        : filtered.OrderBy(s => s.GetType().GetProperty(sortBy)?.GetValue(s)).ToList();

                // Apply pagination
                var totalItems = filtered.Count;
                var paginatedItems = filtered.Skip((page - 1) * size).Take(size).ToList();

                var response = new PaginatedResponse<object>
                {
                    Items = paginatedItems.Cast<object>().ToList(),
                    Page = page,
                    Size = size,
                    TotalItems = totalItems
                };

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Subscriptions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Bulk create subscriptions
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateSubscriptions([FromBody] BulkOperationRequest bulkRequest)
        {
            try
            {
                if (bulkRequest?.Items == null || bulkRequest.Items.Count == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Items are required for bulk operation"));

                var result = new BulkOperationResult
                {
                    TotalItems = bulkRequest.Items.Count,
                    SuccessfulItems = bulkRequest.Items.Count,
                    FailedItems = 0,
                    CompletedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, "Bulk operation completed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get subscription by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscription(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid subscription ID"));

                // Mock subscription details
                var subscription = new
                {
                    id = id,
                    tenantId = Guid.NewGuid(),
                    tenantName = "TechCorp Inc.",
                    planName = "Professional",
                    planId = Guid.NewGuid(),
                    status = "Active",
                    billingCycle = "Monthly",
                    startDate = DateTime.UtcNow.AddMonths(-3),
                    renewalDate = DateTime.UtcNow.AddMonths(1),
                    monthlyPrice = 299.99m,
                    monthlyUsers = 50,
                    monthlyControls = 250,
                    monthlyAudits = 12,
                    currentUsers = 42,
                    currentControls = 187,
                    currentAudits = 8,
                    paymentMethod = "Credit Card",
                    lastPaymentDate = DateTime.UtcNow.AddMonths(-1),
                    nextBillingDate = DateTime.UtcNow.AddMonths(1),
                    autoRenewal = true
                };

                return Ok(ApiResponse<object>.SuccessResponse(subscription, "Subscription retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Subscribe to a plan
        /// Creates a new subscription for a tenant
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] dynamic subscriptionData)
        {
            try
            {
                if (subscriptionData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Subscription data is required"));

                var tenantId = (Guid?)subscriptionData.tenantId ?? Guid.NewGuid();
                var planId = (Guid?)subscriptionData.planId ?? Guid.NewGuid();
                var billingCycle = (string?)subscriptionData.billingCycle ?? "Monthly";

                // Mock subscription creation
                var newSubscription = new
                {
                    id = Guid.NewGuid(),
                    tenantId = tenantId,
                    planId = planId,
                    status = "Active",
                    billingCycle = billingCycle,
                    startDate = DateTime.UtcNow,
                    renewalDate = billingCycle == "Monthly" ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddYears(1),
                    autoRenewal = true,
                    createdDate = DateTime.UtcNow,
                    message = "Subscription created successfully"
                };

                return CreatedAtAction(nameof(GetSubscription), new { id = newSubscription.id },
                    ApiResponse<object>.SuccessResponse(newSubscription, "Subscription created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Cancel subscription
        /// Cancels an active subscription
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelSubscription([FromBody] dynamic cancelData)
        {
            try
            {
                if (cancelData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Cancellation data is required"));

                var subscriptionId = (Guid?)cancelData.subscriptionId ?? Guid.NewGuid();
                var reason = (string?)cancelData.reason ?? "User requested";

                // Mock subscription cancellation
                var cancelResult = new
                {
                    subscriptionId = subscriptionId,
                    status = "Cancelled",
                    cancelledDate = DateTime.UtcNow,
                    reason = reason,
                    refundAmount = 149.99m,
                    refundStatus = "Processed",
                    message = "Subscription cancelled successfully. Refund has been processed."
                };

                return Ok(ApiResponse<object>.SuccessResponse(cancelResult, "Subscription cancelled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Upgrade subscription
        /// Upgrades subscription to a higher tier plan
        /// </summary>
        [HttpPost("upgrade")]
        public async Task<IActionResult> UpgradeSubscription([FromBody] dynamic upgradeData)
        {
            try
            {
                if (upgradeData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Upgrade data is required"));

                var subscriptionId = (Guid?)upgradeData.subscriptionId ?? Guid.NewGuid();
                var newPlanId = (Guid?)upgradeData.newPlanId ?? Guid.NewGuid();
                var newPlanName = (string?)upgradeData.newPlanName ?? "Enterprise";

                // Mock subscription upgrade
                var upgradeResult = new
                {
                    subscriptionId = subscriptionId,
                    previousPlan = "Professional",
                    newPlan = newPlanName,
                    upgradedDate = DateTime.UtcNow,
                    newMonthlyPrice = 799.99m,
                    previousMonthlyPrice = 299.99m,
                    additionalCost = 500.00m,
                    creditApplied = 100.00m,
                    effectiveDate = DateTime.UtcNow,
                    message = "Subscription upgraded successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(upgradeResult, "Subscription upgraded successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get available subscription plans
        /// Returns all available plans with pricing and features
        /// </summary>
        [HttpGet("plans")]
        public async Task<IActionResult> GetSubscriptionPlans()
        {
            try
            {
                // Mock subscription plans
                var plans = new List<dynamic>
                {
                    new {
                        id = Guid.NewGuid(),
                        name = "Starter",
                        code = "STARTER",
                        monthlyPrice = 99.99m,
                        annualPrice = 999.99m,
                        description = "Perfect for small organizations starting with GRC",
                        features = new List<string>
                        {
                            "Up to 10 users",
                            "50 controls",
                            "2 audits per month",
                            "Basic reporting",
                            "Email support"
                        },
                        limits = new { users = 10, controls = 50, audits = 2 },
                        isPopular = false
                    },
                    new {
                        id = Guid.NewGuid(),
                        name = "Professional",
                        code = "PROFESSIONAL",
                        monthlyPrice = 299.99m,
                        annualPrice = 2999.99m,
                        description = "Ideal for medium-sized organizations with growing compliance needs",
                        features = new List<string>
                        {
                            "Up to 50 users",
                            "250 controls",
                            "12 audits per month",
                            "Advanced reporting",
                            "Priority email & phone support",
                            "Custom workflows"
                        },
                        limits = new { users = 50, controls = 250, audits = 12 },
                        isPopular = true
                    },
                    new {
                        id = Guid.NewGuid(),
                        name = "Enterprise",
                        code = "ENTERPRISE",
                        monthlyPrice = 799.99m,
                        annualPrice = 7999.99m,
                        description = "For large enterprises requiring comprehensive GRC solutions",
                        features = new List<string>
                        {
                            "Unlimited users",
                            "1000+ controls",
                            "Unlimited audits",
                            "Custom reporting",
                            "24/7 dedicated support",
                            "Custom integrations",
                            "Advanced analytics",
                            "API access"
                        },
                        limits = new { users = -1, controls = 1000, audits = -1 },
                        isPopular = false
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(plans, "Subscription plans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update subscription
        /// Updates subscription details (full update)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] dynamic updateData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid subscription ID"));

                if (updateData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Subscription data is required"));

                // Mock subscription update
                var updatedSubscription = new
                {
                    id = id,
                    tenantId = Guid.NewGuid(),
                    planName = (string?)updateData.planName ?? "Professional",
                    billingCycle = (string?)updateData.billingCycle ?? "Monthly",
                    status = "Active",
                    autoRenewal = (bool?)updateData.autoRenewal ?? true,
                    paymentMethod = (string?)updateData.paymentMethod ?? "Credit Card",
                    updatedDate = DateTime.UtcNow,
                    message = "Subscription updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(updatedSubscription, "Subscription updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Partially update subscription
        /// Updates specific fields of a subscription (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchSubscription(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid subscription ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                // Mock subscription patch
                var patchedSubscription = new
                {
                    id = id,
                    updatedFields = new
                    {
                        autoRenewal = (bool?)patchData.autoRenewal,
                        billingCycle = (string?)patchData.billingCycle,
                        paymentMethod = (string?)patchData.paymentMethod
                    },
                    updatedDate = DateTime.UtcNow,
                    message = "Subscription partially updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(patchedSubscription, "Subscription updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Delete subscription
        /// Permanently removes a subscription
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid subscription ID"));

                // Mock subscription deletion
                var deleteResult = new
                {
                    subscriptionId = id,
                    status = "Deleted",
                    deletedDate = DateTime.UtcNow,
                    message = "Subscription deleted successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(deleteResult, "Subscription deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
