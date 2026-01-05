using GrcMvc.Models.Dtos;
using GrcMvc.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface ISubscriptionService
    {
        // Subscription Plans
        Task<List<SubscriptionPlanDto>> GetAllPlansAsync();
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId);
        Task<SubscriptionPlanDto?> GetPlanByCodeAsync(string code);

        // Subscriptions
        Task<SubscriptionDto?> GetSubscriptionByTenantAsync(Guid tenantId);
        Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId);
        Task<SubscriptionDto> CreateSubscriptionAsync(Guid tenantId, Guid planId, string billingCycle = "Monthly");
        Task<SubscriptionDto> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus);
        Task<bool> ActivateTrialAsync(Guid subscriptionId, int trialDays = 14);
        Task<bool> ActivateSubscriptionAsync(Guid subscriptionId);
        Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string reason = "");
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string reason = "");
        Task<bool> RenewSubscriptionAsync(Guid subscriptionId);
        Task<int> CheckSubscriptionStatusAsync(); // Check for expired subscriptions

        // Payments
        Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId);
        Task<List<PaymentDto>> GetPaymentsBySubscriptionAsync(Guid subscriptionId);
        Task<PaymentDto> RecordPaymentAsync(Guid subscriptionId, decimal amount, string transactionId, string status = "Completed");
        Task<PaymentConfirmationDto> ProcessPaymentAsync(ProcessPaymentDto paymentDto);
        Task<bool> RefundPaymentAsync(Guid paymentId, string reason = "");

        // Invoices
        Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId);
        Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<List<InvoiceDto>> GetInvoicesBySubscriptionAsync(Guid subscriptionId);
        Task<InvoiceDto?> GetInvoicesByTenantAsync(Guid tenantId);
        Task<InvoiceDto> CreateInvoiceAsync(Guid subscriptionId);
        Task<bool> SendInvoiceAsync(Guid invoiceId, string recipientEmail);
        Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, Guid paymentId);

        // Notifications
        Task SendWelcomeEmailAsync(Guid subscriptionId);
        Task SendPaymentConfirmationEmailAsync(Guid paymentId);
        Task SendInvoiceEmailAsync(Guid invoiceId);
        Task SendSubscriptionRenewalReminderAsync(Guid subscriptionId);
        Task SendSubscriptionExpiringReminderAsync(Guid subscriptionId);

        // Access Control
        Task<bool> IsUserLimitReachedAsync(Guid tenantId);
        Task<bool> IsFeatureAvailableAsync(Guid tenantId, string featureName);
    }
}
