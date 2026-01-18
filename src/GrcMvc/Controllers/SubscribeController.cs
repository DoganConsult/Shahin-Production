using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Unified Subscription Flow Controller
    /// Flow: Plans → Checkout → Payment → Account Creation → TenantId Display → Onboarding
    /// </summary>
    [Route("subscribe")]
    public class SubscribeController : Controller
    {
        private readonly GrcDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<SubscribeController> _logger;

        public SubscribeController(
            GrcDbContext context,
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SubscribeController> logger)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        #region Step 1: Select Plan

        /// <summary>
        /// Display available subscription plans
        /// </summary>
        [HttpGet("")]
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> Plans()
        {
            var plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var planDtos = plans.Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                AnnualPrice = p.AnnualPrice,
                MaxUsers = p.MaxUsers,
                MaxAssessments = p.MaxAssessments,
                MaxPolicies = p.MaxPolicies,
                HasAdvancedReporting = p.HasAdvancedReporting,
                HasApiAccess = p.HasApiAccess,
                HasPrioritySupport = p.HasPrioritySupport,
                Features = JsonSerializer.Deserialize<List<string>>(p.Features ?? "[]") ?? new(),
                DisplayOrder = p.DisplayOrder
            }).ToList();

            return View(planDtos);
        }

        #endregion

        #region Step 2: Checkout Form

        /// <summary>
        /// Display checkout form for selected plan
        /// </summary>
        [HttpGet("checkout/{planId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout(Guid planId, string? cycle = "Monthly")
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                return NotFound("Plan not found");

            var model = new CheckoutViewModel
            {
                PlanId = planId,
                PlanName = plan.Name,
                PlanCode = plan.Code,
                BillingCycle = cycle ?? "Monthly",
                MonthlyPrice = plan.MonthlyPrice,
                AnnualPrice = plan.AnnualPrice,
                Amount = cycle == "Annual" ? plan.AnnualPrice : plan.MonthlyPrice,
                Currency = "SAR",
                Features = JsonSerializer.Deserialize<List<string>>(plan.Features ?? "[]") ?? new()
            };

            return View(model);
        }

        /// <summary>
        /// Process checkout - create pending subscription
        /// </summary>
        [HttpPost("checkout")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutFormDto form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all required fields";
                return RedirectToAction("Checkout", new { planId = form.PlanId });
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(form.Email);
            if (existingUser != null)
            {
                TempData["Error"] = "An account with this email already exists. Please login.";
                return RedirectToAction("Checkout", new { planId = form.PlanId });
            }

            // Store checkout data in session for payment step
            var checkoutSession = new CheckoutSession
            {
                SessionId = Guid.NewGuid(),
                PlanId = form.PlanId,
                BillingCycle = form.BillingCycle,
                Email = form.Email,
                FirstName = form.FirstName,
                LastName = form.LastName,
                CompanyName = form.CompanyName,
                Phone = form.Phone,
                Password = form.Password, // Will be used after payment
                Amount = form.Amount,
                Currency = form.Currency,
                CreatedAt = DateTime.UtcNow
            };

            // Store in TempData (encrypted session)
            TempData["CheckoutSession"] = JsonSerializer.Serialize(checkoutSession);
            TempData.Keep("CheckoutSession");

            return RedirectToAction("Payment", new { sessionId = checkoutSession.SessionId });
        }

        #endregion

        #region Step 3: Payment

        /// <summary>
        /// Display payment form
        /// </summary>
        [HttpGet("payment/{sessionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Payment(Guid sessionId)
        {
            var sessionJson = TempData["CheckoutSession"]?.ToString();
            TempData.Keep("CheckoutSession");

            if (string.IsNullOrEmpty(sessionJson))
            {
                TempData["Error"] = "Session expired. Please start again.";
                return RedirectToAction("Plans");
            }

            var session = JsonSerializer.Deserialize<CheckoutSession>(sessionJson);
            if (session == null || session.SessionId != sessionId)
            {
                TempData["Error"] = "Invalid session. Please start again.";
                return RedirectToAction("Plans");
            }

            var plan = await _context.SubscriptionPlans.FindAsync(session.PlanId);

            var model = new PaymentViewModel
            {
                SessionId = sessionId,
                PlanName = plan?.Name ?? "Unknown",
                Amount = session.Amount,
                Currency = session.Currency,
                BillingCycle = session.BillingCycle,
                Email = session.Email,
                CompanyName = session.CompanyName
            };

            return View(model);
        }

        /// <summary>
        /// Process payment (simulated for now - replace with real gateway)
        /// </summary>
        [HttpPost("payment/process")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentFormDto form)
        {
            var sessionJson = TempData["CheckoutSession"]?.ToString();
            TempData.Keep("CheckoutSession");

            if (string.IsNullOrEmpty(sessionJson))
            {
                TempData["Error"] = "Session expired. Please start again.";
                return RedirectToAction("Plans");
            }

            var session = JsonSerializer.Deserialize<CheckoutSession>(sessionJson);
            if (session == null)
            {
                TempData["Error"] = "Invalid session.";
                return RedirectToAction("Plans");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. GENERATE TENANT ID (after payment confirmed)
                var tenantId = Guid.NewGuid();

                // 2. Create Tenant
                var tenantSlug = GenerateTenantSlug(session.CompanyName);
                
                // Ensure slug is unique
                var slugExists = await _context.Tenants.AnyAsync(t => t.TenantSlug == tenantSlug);
                if (slugExists)
                {
                    tenantSlug = $"{tenantSlug}-{DateTime.UtcNow:HHmmss}";
                }

                var tenant = new Tenant
                {
                    // Id is auto-generated by ABP - don't set manually
                    OrganizationName = session.CompanyName,
                    TenantSlug = tenantSlug,
                    SubscriptionTier = "Active",
                    AdminEmail = session.Email,
                    Status = "Active",
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow,
                    SubscriptionStartDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Tenants.Add(tenant);

                // 3. Create User Account
                var user = new ApplicationUser
                {
                    FirstName = session.FirstName,
                    LastName = session.LastName,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, session.Password);
                
                if (createResult.Succeeded)
                {
                    // Set UserName, Email, phone and confirm email using UserManager methods
                    await _userManager.SetUserNameAsync(user, session.Email);
                    await _userManager.SetEmailAsync(user, session.Email);
                    if (!string.IsNullOrEmpty(session.Phone))
                    {
                        await _userManager.SetPhoneNumberAsync(user, session.Phone);
                    }
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }
                
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    throw new Exception("Failed to create user account");
                }

                // 4. Link User to Tenant
                var tenantUser = new TenantUser
                {
                    // Id is auto-generated - don't set manually
                    TenantId = tenantId,
                    UserId = user.Id.ToString(),
                    RoleCode = "TENANT_ADMIN",
                    Status = "Active",
                    ActivatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantUsers.Add(tenantUser);

                // 5. Create Subscription (Status: PendingOnboarding)
                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PlanId = session.PlanId,
                    Status = "PendingOnboarding", // NEW STATUS
                    BillingCycle = session.BillingCycle,
                    SubscriptionStartDate = DateTime.UtcNow,
                    NextBillingDate = session.BillingCycle == "Annual"
                        ? DateTime.UtcNow.AddYears(1)
                        : DateTime.UtcNow.AddMonths(1),
                    AutoRenew = true,
                    CurrentUserCount = 1
                };
                _context.Subscriptions.Add(subscription);

                // 6. Record Payment
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    SubscriptionId = subscription.Id,
                    TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Amount = session.Amount,
                    Currency = session.Currency,
                    Status = "Completed",
                    PaymentMethod = form.PaymentMethod ?? "CreditCard",
                    Gateway = "Simulated", // Replace with real gateway
                    PaymentDate = DateTime.UtcNow,
                    PaymentDetails = JsonSerializer.Serialize(new
                    {
                        CardLast4 = GetCardLast4(form.CardNumber),
                        CardType = DetectCardType(form.CardNumber),
                        session.BillingCycle
                    })
                };
                _context.Payments.Add(payment);

                // 7. Create Invoice
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    SubscriptionId = subscription.Id,
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow,
                    PeriodStart = DateTime.UtcNow,
                    PeriodEnd = session.BillingCycle == "Annual"
                        ? DateTime.UtcNow.AddYears(1)
                        : DateTime.UtcNow.AddMonths(1),
                    SubTotal = session.Amount,
                    TaxAmount = session.Amount * 0.15m, // 15% VAT
                    TotalAmount = session.Amount * 1.15m,
                    AmountPaid = session.Amount * 1.15m,
                    Status = "Paid",
                    PaidDate = DateTime.UtcNow
                };
                _context.Invoices.Add(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 8. Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: true);

                _logger.LogInformation(
                    "Payment successful. TenantId: {TenantId}, User: {Email}, Subscription: {SubscriptionId}",
                    tenantId, session.Email, subscription.Id);

                // Clear session
                TempData.Remove("CheckoutSession");

                // Store tenant info for display
                TempData["NewTenantId"] = tenantId.ToString();
                TempData["NewSubscriptionId"] = subscription.Id.ToString();
                TempData["TransactionId"] = payment.TransactionId;
                TempData["CompanyName"] = session.CompanyName;

                return RedirectToAction("Success", new { tenantId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Payment processing failed");
                TempData["Error"] = "Payment processing failed. Please try again.";
                return RedirectToAction("Payment", new { sessionId = session.SessionId });
            }
        }

        #endregion

        #region Step 4: Success - Show TenantId

        /// <summary>
        /// Payment success - Display TenantId and proceed to onboarding
        /// Status: Active subscription, Pending Onboarding
        /// </summary>
        [HttpGet("success/{tenantId}")]
        [Authorize]
        public async Task<IActionResult> Success(Guid tenantId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Tenant)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription == null)
                return NotFound();

            var payment = await _context.Payments
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();

            var model = new SubscriptionSuccessViewModel
            {
                TenantId = tenantId,
                CompanyName = subscription.Tenant?.OrganizationName ?? TempData["CompanyName"]?.ToString() ?? "Your Company",
                PlanName = subscription.Plan?.Name ?? "Professional",
                SubscriptionStatus = subscription.Status,
                TransactionId = payment?.TransactionId ?? TempData["TransactionId"]?.ToString() ?? "",
                Amount = payment?.Amount ?? 0,
                Currency = payment?.Currency ?? "SAR",
                PaymentDate = payment?.PaymentDate ?? DateTime.UtcNow,
                NextBillingDate = subscription.NextBillingDate,
                BillingCycle = subscription.BillingCycle
            };

            return View(model);
        }

        /// <summary>
        /// Start onboarding after viewing TenantId
        /// </summary>
        [HttpPost("start-onboarding")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartOnboarding(Guid tenantId)
        {
            // Update subscription status
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription != null)
            {
                subscription.Status = "Active";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "OnboardingWizard", new { tenantId });
        }

        #endregion

        #region Status Check

        /// <summary>
        /// Check subscription status
        /// </summary>
        [HttpGet("status/{tenantId}")]
        [Authorize]
        public async Task<IActionResult> Status(Guid tenantId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Tenant)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription == null)
                return NotFound();

            var onboarding = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            var model = new SubscriptionStatusViewModel
            {
                TenantId = tenantId,
                SubscriptionStatus = subscription.Status,
                OnboardingStatus = onboarding?.WizardStatus ?? "NotStarted",
                OnboardingStep = onboarding?.CurrentStep ?? 0,
                PlanName = subscription.Plan?.Name ?? "",
                NextBillingDate = subscription.NextBillingDate
            };

            return View(model);
        }

        #endregion

        #region Helpers

        private static string GetCardLast4(string? cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return "****";
            var cleaned = cardNumber.Replace(" ", "").Replace("-", "");
            if (cleaned.Length < 4) return "****";
            return cleaned[^4..];
        }

        private static string DetectCardType(string? cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return "Unknown";
            var cleaned = cardNumber.Replace(" ", "").Replace("-", "");
            if (cleaned.StartsWith("4")) return "Visa";
            if (cleaned.StartsWith("5")) return "Mastercard";
            if (cleaned.StartsWith("3")) return "Amex";
            return "Card";
        }

        private static string GenerateTenantSlug(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return $"tenant-{Guid.NewGuid().ToString()[..8]}";

            // Remove Arabic diacritics and normalize
            var slug = companyName.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("(", "")
                .Replace(")", "");

            // Remove consecutive dashes
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            // Trim dashes from ends
            slug = slug.Trim('-');

            // Limit length
            if (slug.Length > 50)
                slug = slug[..50];

            return string.IsNullOrEmpty(slug) ? $"tenant-{Guid.NewGuid().ToString()[..8]}" : slug;
        }

        #endregion
    }

    #region View Models

    public class CheckoutViewModel
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = "Monthly";
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public List<string> Features { get; set; } = new();
    }

    public class CheckoutFormDto
    {
        public Guid PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
    }

    public class CheckoutSession
    {
        public Guid SessionId { get; set; }
        public Guid PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentViewModel
    {
        public Guid SessionId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string BillingCycle { get; set; } = "Monthly";
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    public class PaymentFormDto
    {
        public Guid SessionId { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvv { get; set; }
        public string? CardName { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class SubscriptionSuccessViewModel
    {
        public Guid TenantId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public DateTime PaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
    }

    public class SubscriptionStatusViewModel
    {
        public Guid TenantId { get; set; }
        public string SubscriptionStatus { get; set; } = string.Empty;
        public string OnboardingStatus { get; set; } = string.Empty;
        public int OnboardingStep { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime? NextBillingDate { get; set; }
    }

    #endregion
}
