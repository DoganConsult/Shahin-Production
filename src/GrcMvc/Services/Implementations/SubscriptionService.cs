using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public SubscriptionService(
            GrcDbContext dbContext,
            IEmailService emailService,
            ILogger<SubscriptionService> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        #region Subscription Plans

        public async Task<List<SubscriptionPlanDto>> GetAllPlansAsync()
        {
            var plans = await _dbContext.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return plans.Select(p => MapToDto(p)).ToList();
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId);
            return plan != null ? MapToDto(plan) : null;
        }

        public async Task<SubscriptionPlanDto?> GetPlanByCodeAsync(string code)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == code);
            return plan != null ? MapToDto(plan) : null;
        }

        #endregion

        #region Subscriptions

        public async Task<SubscriptionDto?> GetSubscriptionByTenantAsync(Guid tenantId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status != "Cancelled");

            return subscription != null ? MapToDto(subscription) : null;
        }

        public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            return subscription != null ? MapToDto(subscription) : null;
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(Guid tenantId, Guid planId, string billingCycle = "Monthly")
        {
            try
            {
                var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId);
                if (plan == null)
                    throw new Exception($"Plan {planId} not found");

                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PlanId = planId,
                    Status = "Trial",
                    BillingCycle = billingCycle,
                    SubscriptionStartDate = DateTime.UtcNow,
                    TrialEndDate = DateTime.UtcNow.AddDays(14),
                    NextBillingDate = DateTime.UtcNow.AddDays(14),
                    CreatedDate = DateTime.UtcNow
                };

                // Enforce policy before creating subscription
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Subscription",
                    resource: subscription,
                    dataClassification: "confidential",
                    owner: tenantId.ToString());

                _dbContext.Subscriptions.Add(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Subscription created: {subscription.Id} for tenant {tenantId}");

                return MapToDto(subscription);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning($"Policy violation prevented subscription creation: {pve.Message}. Rule: {pve.RuleId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating subscription: {ex.Message}");
                throw;
            }
        }

        public async Task<SubscriptionDto> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                throw new SubscriptionException($"Subscription {subscriptionId} not found", subscriptionId);

            string oldStatus = subscription.Status;
            subscription.Status = newStatus;
            subscription.ModifiedDate = DateTime.UtcNow;

            if (newStatus == "Active")
                subscription.SubscriptionStartDate = DateTime.UtcNow;

            if (newStatus == "Cancelled" || newStatus == "Expired")
                subscription.SubscriptionEndDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Subscription {subscriptionId} status changed: {oldStatus} → {newStatus}");

            return MapToDto(subscription);
        }

        public async Task<bool> ActivateTrialAsync(Guid subscriptionId, int trialDays = 14)
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            subscription.Status = "Trial";
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(trialDays);
            subscription.NextBillingDate = subscription.TrialEndDate;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Trial activated for subscription {subscriptionId}, ends {subscription.TrialEndDate}");
            return true;
        }

        public async Task<bool> ActivateSubscriptionAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            subscription.Status = "Active";
            subscription.TrialEndDate = null;
            subscription.SubscriptionStartDate = DateTime.UtcNow;
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(subscription.BillingCycle == "Annual" ? 12 : 1);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Subscription {subscriptionId} activated");

            // Send welcome email
            try
            {
                await SendWelcomeEmailAsync(subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to send welcome email: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string reason = "")
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            subscription.Status = "Suspended";
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Subscription {subscriptionId} suspended. Reason: {reason}");
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string reason = "")
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            subscription.Status = "Cancelled";
            subscription.SubscriptionEndDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Subscription {subscriptionId} cancelled. Reason: {reason}");
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                return false;

            subscription.Status = "Active";
            subscription.SubscriptionStartDate = DateTime.UtcNow;
            subscription.SubscriptionEndDate = null;
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(subscription.BillingCycle == "Annual" ? 12 : 1);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Subscription {subscriptionId} renewed until {subscription.NextBillingDate}");
            return true;
        }

        public async Task<int> CheckSubscriptionStatusAsync()
        {
            var now = DateTime.UtcNow;
            var expiredSubscriptions = await _dbContext.Subscriptions
                .Where(s => s.Status == "Trial" && s.TrialEndDate < now)
                .ToListAsync();

            int count = 0;
            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = "Expired";
                subscription.SubscriptionEndDate = subscription.TrialEndDate;
                count++;

                _logger.LogInformation($"Subscription {subscription.Id} marked as expired");
            }

            if (count > 0)
                await _dbContext.SaveChangesAsync();

            return count;
        }

        #endregion

        #region Payments

        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            return payment != null ? MapToDto(payment) : null;
        }

        public async Task<List<PaymentDto>> GetPaymentsBySubscriptionAsync(Guid subscriptionId)
        {
            return await _dbContext.Payments
                .Where(p => p.SubscriptionId == subscriptionId && p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PaymentDto> RecordPaymentAsync(Guid subscriptionId, decimal amount, string transactionId, string status = "Completed")
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                throw new SubscriptionException($"Subscription {subscriptionId} not found", subscriptionId);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                TenantId = subscription.TenantId,
                Amount = amount,
                TransactionId = transactionId,
                Status = status,
                PaymentDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            // Enforce policy before recording payment (financial data)
            await _policyHelper.EnforceCreateAsync(
                resourceType: "Payment",
                resource: payment,
                dataClassification: "restricted",
                owner: subscription.TenantId.ToString());

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Payment recorded: {payment.Id} for subscription {subscriptionId}, amount: {amount}");

            return MapToDto(payment);
        }

        public async Task<PaymentConfirmationDto> ProcessPaymentAsync(ProcessPaymentDto paymentDto)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions
                    .Include(s => s.Tenant)
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.Id == paymentDto.SubscriptionId);

                if (subscription == null)
                    return new PaymentConfirmationDto { Success = false, Message = "Subscription not found" };

                // Record the payment
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = paymentDto.SubscriptionId,
                    TenantId = subscription.TenantId,
                    Amount = paymentDto.Amount,
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = "Completed",
                    PaymentMethod = "CreditCard",
                    Gateway = "Stripe",
                    PaymentDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _dbContext.Payments.Add(payment);

                // Create invoice
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = paymentDto.SubscriptionId,
                    TenantId = subscription.TenantId,
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    PeriodStart = DateTime.UtcNow,
                    PeriodEnd = DateTime.UtcNow.AddMonths(subscription.BillingCycle == "Annual" ? 12 : 1),
                    SubTotal = paymentDto.Amount,
                    TaxAmount = 0,
                    TotalAmount = paymentDto.Amount,
                    AmountPaid = paymentDto.Amount,
                    Status = "Paid",
                    SentDate = DateTime.UtcNow,
                    PaidDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _dbContext.Invoices.Add(invoice);
                payment.InvoiceId = invoice.Id;

                // Activate subscription
                subscription.Status = "Active";
                subscription.TrialEndDate = null;
                subscription.SubscriptionStartDate = DateTime.UtcNow;
                subscription.NextBillingDate = invoice.PeriodEnd;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Payment processed: {payment.TransactionId}, subscription {subscription.Id} activated");

                // Send emails
                try
                {
                    await SendPaymentConfirmationEmailAsync(payment.Id);
                    await SendInvoiceEmailAsync(invoice.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to send confirmation emails: {ex.Message}");
                }

                return new PaymentConfirmationDto
                {
                    Success = true,
                    TransactionId = payment.TransactionId,
                    Message = "Payment processed successfully",
                    Subscription = MapToDto(subscription),
                    Invoice = MapToDto(invoice)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Payment processing failed: {ex.Message}");
                return new PaymentConfirmationDto { Success = false, Message = $"Payment failed: {ex.Message}" };
            }
        }

        public async Task<bool> RefundPaymentAsync(Guid paymentId, string reason = "")
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null)
                return false;

            payment.Status = "Refunded";
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Payment {paymentId} refunded. Reason: {reason}");
            return true;
        }

        #endregion

        #region Invoices

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);
            return invoice != null ? MapToDto(invoice) : null;
        }

        public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
            return invoice != null ? MapToDto(invoice) : null;
        }

        public async Task<List<InvoiceDto>> GetInvoicesBySubscriptionAsync(Guid subscriptionId)
        {
            return await _dbContext.Invoices
                .Where(i => i.SubscriptionId == subscriptionId)
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => MapToDto(i))
                .ToListAsync();
        }

        public async Task<InvoiceDto?> GetInvoicesByTenantAsync(Guid tenantId)
        {
            var invoice = await _dbContext.Invoices
                .Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.InvoiceDate)
                .FirstOrDefaultAsync();

            return invoice != null ? MapToDto(invoice) : null;
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
                throw new SubscriptionException($"Subscription {subscriptionId} not found", subscriptionId);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                TenantId = subscription.TenantId,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                PeriodStart = DateTime.UtcNow,
                PeriodEnd = DateTime.UtcNow.AddMonths(1),
                SubTotal = subscription.BillingCycle == "Annual"
                    ? (await GetPlanByIdAsync(subscription.PlanId))?.AnnualPrice ?? 0
                    : (await GetPlanByIdAsync(subscription.PlanId))?.MonthlyPrice ?? 0,
                TaxAmount = 0,
                Status = "Draft",
                CreatedDate = DateTime.UtcNow
            };

            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Invoice created: {invoice.InvoiceNumber} for subscription {subscriptionId}");

            return MapToDto(invoice);
        }

        public async Task<bool> SendInvoiceAsync(Guid invoiceId, string recipientEmail)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Subscription)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return false;

            try
            {
                invoice.Status = "Sent";
                invoice.SentDate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                await SendInvoiceEmailAsync(invoiceId);

                _logger.LogInformation($"Invoice {invoiceId} sent to {recipientEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send invoice: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, Guid paymentId)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

            if (invoice == null || payment == null)
                return false;

            invoice.Status = "Paid";
            invoice.AmountPaid = payment.Amount;
            invoice.PaidDate = DateTime.UtcNow;
            invoice.Payments.Add(payment);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Invoice {invoiceId} marked as paid");
            return true;
        }

        #endregion

        #region Notifications

        public async Task SendWelcomeEmailAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription?.Tenant == null)
                return;

            var subject = "Welcome to GRC System!";
            var body = $@"
                <h2>Welcome to GRC System, {subscription.Tenant.OrganizationName}!</h2>
                <p>Your subscription has been activated successfully.</p>
                <h3>Subscription Details:</h3>
                <ul>
                    <li>Plan: {subscription.Plan?.Name}</li>
                    <li>Billing Cycle: {subscription.BillingCycle}</li>
                    <li>Start Date: {subscription.SubscriptionStartDate:MMM dd, yyyy}</li>
                    <li>Next Billing Date: {subscription.NextBillingDate:MMM dd, yyyy}</li>
                </ul>
                <p>Thank you for choosing us!</p>
            ";

            await _emailService.SendEmailAsync(subscription.Tenant.AdminEmail, subject, body);
        }

        public async Task SendPaymentConfirmationEmailAsync(Guid paymentId)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Subscription)
                .ThenInclude(s => s.Tenant)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment?.Subscription?.Tenant == null)
                return;

            var subject = $"Payment Confirmation - Transaction {payment.TransactionId}";
            var body = $@"
                <h2>Payment Confirmation</h2>
                <p>Thank you for your payment!</p>
                <h3>Payment Details:</h3>
                <ul>
                    <li>Amount: ${payment.Amount} {payment.Currency}</li>
                    <li>Transaction ID: {payment.TransactionId}</li>
                    <li>Date: {payment.PaymentDate:MMM dd, yyyy}</li>
                    <li>Status: {payment.Status}</li>
                </ul>
                <p>Your subscription is now active.</p>
            ";

            await _emailService.SendEmailAsync(payment.Subscription.Tenant.AdminEmail, subject, body);
        }

        public async Task SendInvoiceEmailAsync(Guid invoiceId)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Subscription)
                .ThenInclude(s => s.Tenant)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice?.Subscription?.Tenant == null)
                return;

            var subject = $"Invoice {invoice.InvoiceNumber}";
            var body = $@"
                <h2>Invoice</h2>
                <h3>Invoice Details:</h3>
                <ul>
                    <li>Invoice Number: {invoice.InvoiceNumber}</li>
                    <li>Date: {invoice.InvoiceDate:MMM dd, yyyy}</li>
                    <li>Due Date: {invoice.DueDate:MMM dd, yyyy}</li>
                    <li>Period: {invoice.PeriodStart:MMM dd, yyyy} - {invoice.PeriodEnd:MMM dd, yyyy}</li>
                </ul>
                <h3>Amount:</h3>
                <ul>
                    <li>Subtotal: ${invoice.SubTotal}</li>
                    <li>Tax: ${invoice.TaxAmount}</li>
                    <li>Total: ${invoice.TotalAmount}</li>
                    <li>Amount Paid: ${invoice.AmountPaid}</li>
                </ul>
            ";

            await _emailService.SendEmailAsync(invoice.Subscription.Tenant.AdminEmail, subject, body);
        }

        public async Task SendSubscriptionRenewalReminderAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Tenant)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription?.Tenant == null)
                return;

            var subject = "Your subscription is expiring soon";
            var body = $@"
                <h2>Subscription Renewal Reminder</h2>
                <p>Your GRC System subscription is expiring on {subscription.NextBillingDate:MMM dd, yyyy}.</p>
                <p>Please renew your subscription to maintain uninterrupted service.</p>
            ";

            await _emailService.SendEmailAsync(subscription.Tenant.AdminEmail, subject, body);
        }

        public async Task SendSubscriptionExpiringReminderAsync(Guid subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Tenant)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription?.Tenant == null)
                return;

            var subject = "Your subscription will expire in 7 days";
            var body = $@"
                <h2>Subscription Expiration Warning</h2>
                <p>Your GRC System subscription will expire on {subscription.SubscriptionEndDate:MMM dd, yyyy}.</p>
                <p>Please renew immediately to avoid service interruption.</p>
            ";

            await _emailService.SendEmailAsync(subscription.Tenant.AdminEmail, subject, body);
        }

        #endregion

        #region Access Control

        public async Task<bool> IsUserLimitReachedAsync(Guid tenantId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == "Active");

            if (subscription?.Plan == null)
                return true;

            var userCount = await _dbContext.TenantUsers.CountAsync(tu => tu.TenantId == tenantId);
            return userCount >= subscription.Plan.MaxUsers;
        }

        public async Task<bool> IsFeatureAvailableAsync(Guid tenantId, string featureName)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == "Active");

            if (subscription?.Plan == null)
                return false;

            return featureName switch
            {
                "AdvancedReporting" => subscription.Plan.HasAdvancedReporting,
                "ApiAccess" => subscription.Plan.HasApiAccess,
                "PrioritySupport" => subscription.Plan.HasPrioritySupport,
                _ => false
            };
        }

        #endregion

        #region Mapping Methods

        private SubscriptionPlanDto MapToDto(SubscriptionPlan plan)
        {
            var features = new List<string>();
            if (!string.IsNullOrEmpty(plan.Features))
            {
                // Parse JSON features (best-effort - malformed JSON returns empty list)
                try
                {
                    features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(plan.Features) ?? new List<string>();
                }
                catch (System.Text.Json.JsonException)
                {
                    // Malformed JSON in Features field - return empty list
                }
            }

            return new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Code = plan.Code,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                AnnualPrice = plan.AnnualPrice,
                MaxUsers = plan.MaxUsers,
                MaxAssessments = plan.MaxAssessments,
                MaxPolicies = plan.MaxPolicies,
                HasAdvancedReporting = plan.HasAdvancedReporting,
                HasApiAccess = plan.HasApiAccess,
                HasPrioritySupport = plan.HasPrioritySupport,
                Features = features,
                DisplayOrder = plan.DisplayOrder
            };
        }

        private SubscriptionDto MapToDto(Subscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                PlanId = subscription.PlanId,
                Status = subscription.Status,
                TrialEndDate = subscription.TrialEndDate,
                SubscriptionStartDate = subscription.SubscriptionStartDate,
                SubscriptionEndDate = subscription.SubscriptionEndDate,
                NextBillingDate = subscription.NextBillingDate,
                BillingCycle = subscription.BillingCycle,
                AutoRenew = subscription.AutoRenew,
                CurrentUserCount = subscription.CurrentUserCount,
                Plan = subscription.Plan != null ? MapToDto(subscription.Plan) : null
            };
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                SubscriptionId = payment.SubscriptionId,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                PaymentDate = payment.PaymentDate,
                ErrorMessage = payment.ErrorMessage
            };
        }

        private InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                PeriodStart = invoice.PeriodStart,
                PeriodEnd = invoice.PeriodEnd,
                SubTotal = invoice.SubTotal,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                AmountPaid = invoice.AmountPaid,
                Status = invoice.Status,
                PaidDate = invoice.PaidDate
            };
        }

        #endregion
    }
}
