using GrcMvc.Models.Dtos;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Controllers
{
    [Route("subscriptions")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionService subscriptionService, ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        #region Plans

        /// <summary>
        /// Get all available subscription plans
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
        {
            try
            {
                var plans = await _subscriptionService.GetAllPlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting plans: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving plans" });
            }
        }

        /// <summary>
        /// Get plan by ID
        /// </summary>
        [HttpGet("plans/{planId}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubscriptionPlanDto>> GetPlanById(Guid planId)
        {
            try
            {
                var plan = await _subscriptionService.GetPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound();

                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting plan: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving plan" });
            }
        }

        /// <summary>
        /// Get plan by code (MVP, PRO, ENT)
        /// </summary>
        [HttpGet("plans/code/{code}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubscriptionPlanDto>> GetPlanByCode(string code)
        {
            try
            {
                var plan = await _subscriptionService.GetPlanByCodeAsync(code);
                if (plan == null)
                    return NotFound();

                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting plan: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving plan" });
            }
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Get subscription for current tenant
        /// </summary>
        [HttpGet("{tenantId}")]
        [Authorize]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(Guid tenantId)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionByTenantAsync(tenantId);
                if (subscription == null)
                    return NotFound();

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving subscription" });
            }
        }

        /// <summary>
        /// Create a new subscription and initialize trial period
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionDto request)
        {
            try
            {
                var subscription = await _subscriptionService.CreateSubscriptionAsync(
                    request.TenantId,
                    request.PlanId,
                    request.BillingCycle ?? "Monthly");

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error creating subscription" });
            }
        }

        #endregion

        #region Checkout & Payment

        /// <summary>
        /// Initialize checkout process
        /// </summary>
        [HttpPost("checkout")]
        [AllowAnonymous]
        public async Task<ActionResult<SubscriptionDto>> Checkout([FromBody] CheckoutDto request)
        {
            try
            {
                // Validate plan
                var plan = await _subscriptionService.GetPlanByIdAsync(request.PlanId);
                if (plan == null)
                    return BadRequest(new { message = "Invalid plan" });

                // Create subscription
                var subscription = await _subscriptionService.CreateSubscriptionAsync(
                    Guid.NewGuid(), // This would be the actual tenant ID
                    request.PlanId,
                    request.BillingCycle);

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout error: {ex.Message}");
                return StatusCode(500, new { message = "Checkout failed" });
            }
        }

        /// <summary>
        /// Process payment
        /// </summary>
        [HttpPost("payment")]
        [AllowAnonymous]
        public async Task<ActionResult<PaymentConfirmationDto>> ProcessPayment([FromBody] ProcessPaymentDto request)
        {
            try
            {
                var result = await _subscriptionService.ProcessPaymentAsync(request);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Payment processing error: {ex.Message}");
                return StatusCode(500, new { message = "Payment processing failed" });
            }
        }

        /// <summary>
        /// Get payment history
        /// </summary>
        [HttpGet("payments/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult<List<PaymentDto>>> GetPayments(Guid subscriptionId)
        {
            try
            {
                var payments = await _subscriptionService.GetPaymentsBySubscriptionAsync(subscriptionId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving payments" });
            }
        }

        #endregion

        #region Invoices

        /// <summary>
        /// Get invoices for subscription
        /// </summary>
        [HttpGet("invoices/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult<List<InvoiceDto>>> GetInvoices(Guid subscriptionId)
        {
            try
            {
                var invoices = await _subscriptionService.GetInvoicesBySubscriptionAsync(subscriptionId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting invoices: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving invoices" });
            }
        }

        /// <summary>
        /// Get specific invoice
        /// </summary>
        [HttpGet("invoice/{invoiceId}")]
        [Authorize]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = await _subscriptionService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                    return NotFound();

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting invoice: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving invoice" });
            }
        }

        #endregion

        #region Status & Management

        /// <summary>
        /// Activate trial for subscription
        /// </summary>
        [HttpPost("trial/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult> ActivateTrial(Guid subscriptionId, [FromQuery] int? trialDays = null)
        {
            try
            {
                var success = await _subscriptionService.ActivateTrialAsync(subscriptionId, trialDays ?? 14);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Trial activated" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error activating trial: {ex.Message}");
                return StatusCode(500, new { message = "Error activating trial" });
            }
        }

        /// <summary>
        /// Activate subscription (after payment)
        /// </summary>
        [HttpPost("activate/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult> ActivateSubscription(Guid subscriptionId)
        {
            try
            {
                var success = await _subscriptionService.ActivateSubscriptionAsync(subscriptionId);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Subscription activated" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error activating subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error activating subscription" });
            }
        }

        /// <summary>
        /// Suspend subscription
        /// </summary>
        [HttpPost("suspend/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult> SuspendSubscription(Guid subscriptionId, [FromBody] SuspendReasonDto request)
        {
            try
            {
                var success = await _subscriptionService.SuspendSubscriptionAsync(subscriptionId, request?.Reason ?? "");
                if (!success)
                    return NotFound();

                return Ok(new { message = "Subscription suspended" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error suspending subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error suspending subscription" });
            }
        }

        /// <summary>
        /// Cancel subscription
        /// </summary>
        [HttpPost("cancel/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult> CancelSubscription(Guid subscriptionId, [FromBody] CancelReasonDto request)
        {
            try
            {
                var success = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, request?.Reason ?? "");
                if (!success)
                    return NotFound();

                return Ok(new { message = "Subscription cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error cancelling subscription" });
            }
        }

        /// <summary>
        /// Renew subscription
        /// </summary>
        [HttpPost("renew/{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult> RenewSubscription(Guid subscriptionId)
        {
            try
            {
                var success = await _subscriptionService.RenewSubscriptionAsync(subscriptionId);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Subscription renewed" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error renewing subscription: {ex.Message}");
                return StatusCode(500, new { message = "Error renewing subscription" });
            }
        }

        #endregion

        #region MVC Views

        /// <summary>
        /// Display subscription status page
        /// </summary>
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Return user's current subscription or null if none exists
            return View((GrcMvc.Models.Dtos.SubscriptionDto)null);
        }

        /// <summary>
        /// Display checkout page for a specific plan
        /// </summary>
        [HttpGet("checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout(Guid? planId)
        {
            if (!planId.HasValue)
                return BadRequest("Plan ID is required");
            
            var plan = await _subscriptionService.GetPlanByIdAsync(planId.Value);
            if (plan == null)
                return NotFound("Plan not found");
            
            return View(plan);
        }

        /// <summary>
        /// Display payment receipt page
        /// </summary>
        [HttpGet("receipt")]
        [AllowAnonymous]
        public IActionResult Receipt()
        {
            // Create a dummy receipt model for display
            var receipt = new PaymentDto
            {
                Id = Guid.NewGuid(),
                Amount = 0,
                Currency = "USD",
                Status = "success",
                PaymentDate = DateTime.UtcNow
            };
            return View(receipt);
        }

        #endregion

        #region Helper Models

        public class CreateSubscriptionDto
        {
            public Guid TenantId { get; set; }
            public Guid PlanId { get; set; }
            public string? BillingCycle { get; set; }
        }

        public class SuspendReasonDto
        {
            public string? Reason { get; set; }
        }

        public class CancelReasonDto
        {
            public string? Reason { get; set; }
        }

        #endregion
    }
}
