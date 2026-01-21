using System;
using System.Threading.Tasks;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for multi-tenant provisioning and management.
    /// Handles tenant creation, activation, and organizational setup.
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TenantService> _logger;
        private readonly IEmailService _emailService;
        private readonly IAuditEventService _auditService;
        private readonly ITenantProvisioningService _provisioningService;
        private readonly IConfiguration _configuration;

        public TenantService(
            IUnitOfWork unitOfWork,
            ILogger<TenantService> logger,
            IEmailService emailService,
            IAuditEventService auditService,
            ITenantProvisioningService provisioningService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _auditService = auditService;
            _provisioningService = provisioningService;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a new tenant (organization).
        /// Sends activation email to admin.
        /// </summary>
        public async Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug)
        {
            try
            {
                // Validate tenant slug is unique (case-insensitive)
                // HIGH FIX: Normalize slug to lowercase for consistent comparison
                tenantSlug = tenantSlug.ToLower().Trim();
                var existingTenant = await _unitOfWork.Tenants
                    .Query()
                    .FirstOrDefaultAsync(t => t.TenantSlug.ToLower() == tenantSlug);

                if (existingTenant != null)
                {
                    throw new EntityExistsException("Tenant", "Slug", tenantSlug);
                }

                var tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    OrganizationName = organizationName,
                    AdminEmail = adminEmail,
                    TenantSlug = tenantSlug,
                    Status = "Pending",
                    ActivationToken = GenerateActivationToken(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM"
                };

                await _unitOfWork.Tenants.AddAsync(tenant);
                await _unitOfWork.SaveChangesAsync();

                // CRITICAL: Provision tenant database immediately
                // This creates the isolated database for the tenant
                try
                {
                    var provisioned = await _provisioningService.ProvisionTenantAsync(tenant.Id);
                    if (!provisioned)
                    {
                        _logger.LogError("Failed to provision database for tenant {TenantId}. Tenant record created but database not provisioned.", tenant.Id);
                        // Don't fail tenant creation - database can be provisioned later
                    }
                    else
                    {
                        _logger.LogInformation("Successfully provisioned database for tenant {TenantId}", tenant.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error provisioning database for tenant {TenantId}. Tenant record created but database provisioning failed.", tenant.Id);
                    // Continue - database can be provisioned manually later
                }

                // Send activation email
                await SendActivationEmailAsync(tenant);

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenant.Id,
                    eventType: "TenantCreated",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenant.Id.ToString(),
                    action: "Create",
                    actor: "SYSTEM",
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(tenant),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation($"Tenant created: {tenant.Id}, slug: {tenant.TenantSlug}");
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                throw;
            }
        }

        /// <summary>
        /// Activate tenant after admin confirms email.
        /// </summary>
        public async Task<Tenant> ActivateTenantAsync(string tenantSlug, string activationToken, string activatedBy)
        {
            try
            {
                var tenant = await _unitOfWork.Tenants
                    .Query()
                    .FirstOrDefaultAsync(t => t.TenantSlug == tenantSlug);

                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantSlug);
                }

                if (tenant.ActivationToken != activationToken)
                {
                    throw new ValidationException("ActivationToken", "Invalid activation token");
                }

                if (tenant.Status != "Pending")
                {
                    throw new TenantStateException(tenant.Status, "Pending");
                }

                tenant.Status = "Active";
                tenant.ActivatedAt = DateTime.UtcNow;
                tenant.ActivatedBy = activatedBy;
                tenant.ActivationToken = string.Empty; // Clear token after use

                await _unitOfWork.Tenants.UpdateAsync(tenant);
                await _unitOfWork.SaveChangesAsync();

                // Ensure tenant database is provisioned before activation
                if (!await _provisioningService.IsTenantProvisionedAsync(tenant.Id))
                {
                    _logger.LogWarning("Tenant {TenantId} activated but database not provisioned. Provisioning now...", tenant.Id);
                    var provisioned = await _provisioningService.ProvisionTenantAsync(tenant.Id);
                    if (!provisioned)
                    {
                        throw new IntegrationException("TenantProvisioning", $"Failed to provision database for tenant {tenant.Id}");
                    }
                }

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenant.Id,
                    eventType: "TenantActivated",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenant.Id.ToString(),
                    action: "Activate",
                    actor: activatedBy,
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(
                        new { tenant.Id, tenant.Status, tenant.ActivatedAt }
                    ),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation($"Tenant activated: {tenant.Id}");
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant");
                throw;
            }
        }

        /// <summary>
        /// Get tenant by slug (used in multi-tenant routing).
        /// </summary>
        public async Task<Tenant?> GetTenantBySlugAsync(string tenantSlug)
        {
            return await _unitOfWork.Tenants
                .Query()
                .FirstOrDefaultAsync(t => t.TenantSlug == tenantSlug && t.Status == "Active");
        }

        /// <summary>
        /// Get tenant by ID.
        /// </summary>
        public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
        {
            return await _unitOfWork.Tenants.GetByIdAsync(tenantId);
        }

        /// <summary>
        /// Send activation email with activation link.
        /// ASP.NET Core Best Practice: Use configuration for external URLs
        /// ABP Best Practice: Use email templates for consistent branding
        /// </summary>
        private async Task SendActivationEmailAsync(Tenant tenant)
        {
            try
            {
                // ABP Best Practice: Use IConfiguration for environment-specific URLs
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://app.shahin-ai.com";
                var activationUrl = $"{baseUrl}/auth/activate?slug={tenant.TenantSlug}&token={tenant.ActivationToken}";
                var expiryHours = _configuration.GetValue<int>("App:ActivationTokenExpiryHours", 48);

                // Professional HTML email template
                var emailBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
    <div style=""background-color: #ffffff; border-radius: 8px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
        <div style=""text-align: center; padding-bottom: 20px; border-bottom: 2px solid #10b981;"">
            <h1 style=""color: #10b981; margin: 0; font-size: 24px;"">Welcome to Shahin AI GRC Platform!</h1>
        </div>
        
        <div style=""padding: 30px 0;"">
            <p>Dear <strong>Administrator</strong>,</p>
            
            <p>Thank you for registering with Shahin AI GRC Platform. Your organization has been successfully created.</p>
            
            <div style=""background-color: #f0fdf4; padding: 15px; border-radius: 6px; border-left: 4px solid #10b981; margin: 20px 0;"">
                <strong>Organization:</strong> {tenant.OrganizationName}<br>
                <strong>Tenant ID:</strong> {tenant.TenantSlug}
            </div>
            
            <p>To complete your registration and access the platform, please activate your account:</p>
            
            <div style=""text-align: center; padding: 20px 0;"">
                <a href=""{activationUrl}"" style=""display: inline-block; background-color: #10b981; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                    ✅ Activate Your Account
                </a>
            </div>
            
            <p style=""font-size: 14px; color: #6b7280;"">If the button doesn't work, copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #6b7280; font-size: 12px; background-color: #f3f4f6; padding: 10px; border-radius: 4px;"">
                {activationUrl}
            </p>
            
            <div style=""background-color: #fef3c7; padding: 12px; border-radius: 6px; font-size: 14px; margin-top: 20px;"">
                ⚠️ <strong>Important:</strong> This activation link will expire in {expiryHours} hours.
            </div>
        </div>
        
        <div style=""text-align: center; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 12px;"">
            <p>If you did not create this account, please ignore this email.</p>
            <p>&copy; 2026 Shahin AI. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                await _emailService.SendEmailAsync(
                    to: tenant.AdminEmail,
                    subject: $"✅ Activate Your Shahin AI Account - {tenant.OrganizationName}",
                    htmlBody: emailBody
                );

                _logger.LogInformation("Activation email sent to {AdminEmail} for tenant {TenantSlug}", 
                    tenant.AdminEmail, tenant.TenantSlug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send activation email to {AdminEmail}", tenant.AdminEmail);
                // Don't throw; allow tenant creation even if email fails
            }
        }

        /// <summary>
        /// Generate a secure activation token.
        /// </summary>
        private string GenerateActivationToken()
        {
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }

        /// <summary>
        /// Suspend a tenant (temporary deactivation).
        /// HIGH FIX: Added missing lifecycle operation.
        /// </summary>
        public async Task<Tenant> SuspendTenantAsync(Guid tenantId, string suspendedBy, string? reason = null)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.Status == "Suspended")
                throw new TenantStateException("Suspended", "Active");

            tenant.Status = "Suspended";
            tenant.IsActive = false;
            tenant.ModifiedDate = DateTime.UtcNow;
            tenant.ModifiedBy = suspendedBy;

            await _unitOfWork.Tenants.UpdateAsync(tenant);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantSuspended",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Suspend",
                actor: suspendedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, tenant.Status, Reason = reason }),
                correlationId: tenant.CorrelationId
            );

            _logger.LogInformation("Tenant {TenantId} suspended by {SuspendedBy}. Reason: {Reason}", tenantId, suspendedBy, reason ?? "Not specified");
            return tenant;
        }

        /// <summary>
        /// Reactivate a suspended tenant.
        /// </summary>
        public async Task<Tenant> ReactivateTenantAsync(Guid tenantId, string reactivatedBy)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.Status != "Suspended")
                throw new TenantStateException(tenant.Status, "Suspended");

            tenant.Status = "Active";
            tenant.IsActive = true;
            tenant.ModifiedDate = DateTime.UtcNow;
            tenant.ModifiedBy = reactivatedBy;

            await _unitOfWork.Tenants.UpdateAsync(tenant);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantReactivated",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Reactivate",
                actor: reactivatedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, tenant.Status }),
                correlationId: tenant.CorrelationId
            );

            _logger.LogInformation("Tenant {TenantId} reactivated by {ReactivatedBy}", tenantId, reactivatedBy);
            return tenant;
        }

        /// <summary>
        /// Archive a tenant (soft delete with data retention).
        /// </summary>
        public async Task<Tenant> ArchiveTenantAsync(Guid tenantId, string archivedBy, string? reason = null)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.Status == "Archived")
                throw new TenantStateException("Archived", "Active");

            tenant.Status = "Archived";
            tenant.IsActive = false;
            tenant.IsDeleted = true; // Soft delete flag
            tenant.DeletedAt = DateTime.UtcNow;
            tenant.ModifiedDate = DateTime.UtcNow;
            tenant.ModifiedBy = archivedBy;

            await _unitOfWork.Tenants.UpdateAsync(tenant);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantArchived",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Archive",
                actor: archivedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, tenant.Status, Reason = reason }),
                correlationId: tenant.CorrelationId
            );

            _logger.LogInformation("Tenant {TenantId} archived by {ArchivedBy}. Reason: {Reason}", tenantId, archivedBy, reason ?? "Not specified");
            return tenant;
        }

        /// <summary>
        /// Permanently delete a tenant (soft delete by default).
        /// </summary>
        public async Task DeleteTenantAsync(Guid tenantId, string deletedBy)
        {
            await DeleteTenantAsync(tenantId, deletedBy, hardDelete: false);
        }

        /// <summary>
        /// Permanently delete a tenant with option for hard delete.
        /// </summary>
        public async Task<bool> DeleteTenantAsync(Guid tenantId, string deletedBy, bool hardDelete = false)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Attempt to delete non-existent tenant {TenantId}", tenantId);
                return false;
            }

            // Log before deletion
            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: hardDelete ? "TenantHardDeleted" : "TenantSoftDeleted",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: hardDelete ? "HardDelete" : "SoftDelete",
                actor: deletedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, tenant.OrganizationName, HardDelete = hardDelete }),
                correlationId: tenant.CorrelationId
            );

            if (hardDelete)
            {
                // WARNING: This permanently removes data - should require additional confirmation
                _logger.LogWarning("HARD DELETE requested for tenant {TenantId} by {DeletedBy}", tenantId, deletedBy);
                await _unitOfWork.Tenants.DeleteAsync(tenant);
            }
            else
            {
                // Soft delete - mark as deleted but retain data
                tenant.Status = "Deleted";
                tenant.IsActive = false;
                tenant.IsDeleted = true;
                tenant.DeletedAt = DateTime.UtcNow;
                tenant.ModifiedDate = DateTime.UtcNow;
                tenant.ModifiedBy = deletedBy;
                await _unitOfWork.Tenants.UpdateAsync(tenant);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Tenant {TenantId} {DeleteType} by {DeletedBy}", tenantId, hardDelete ? "hard deleted" : "soft deleted", deletedBy);
            return true;
        }

        /// <summary>
        /// Resend activation email for a pending tenant.
        /// ASP.NET Core Best Practice: Rate limiting should be applied at controller level
        /// </summary>
        public async Task<bool> ResendActivationEmailAsync(string adminEmail)
        {
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                _logger.LogWarning("ResendActivationEmail called with empty email");
                return false;
            }

            try
            {
                var tenant = await _unitOfWork.Tenants
                    .Query()
                    .FirstOrDefaultAsync(t => t.AdminEmail.ToLower() == adminEmail.ToLower() && t.Status == "Pending");

                if (tenant == null)
                {
                    _logger.LogWarning("Resend activation requested for non-existent or non-pending tenant: {Email}", adminEmail);
                    // Return true to prevent email enumeration attacks
                    return true;
                }

                // Generate new activation token if expired or near expiry
                var expiryHours = _configuration.GetValue<int>("App:ActivationTokenExpiryHours", 48);
                if (string.IsNullOrEmpty(tenant.ActivationToken) || 
                    (tenant.CreatedDate != default && tenant.CreatedDate.AddHours(expiryHours) < DateTime.UtcNow))
                {
                    tenant.ActivationToken = GenerateActivationToken();
                    tenant.ModifiedDate = DateTime.UtcNow;
                    await _unitOfWork.Tenants.UpdateAsync(tenant);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Send activation email
                await SendActivationEmailAsync(tenant);

                _logger.LogInformation("Activation email resent to {Email} for tenant {TenantSlug}", adminEmail, tenant.TenantSlug);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending activation email to {Email}", adminEmail);
                return false;
            }
        }
    }
}
