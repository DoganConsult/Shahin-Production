using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public TenantService(
            IUnitOfWork unitOfWork,
            ILogger<TenantService> logger,
            IEmailService emailService,
            IAuditEventService auditService,
            ITenantProvisioningService provisioningService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _auditService = auditService;
            _provisioningService = provisioningService;
        }

        /// <summary>
        /// Create a new tenant (organization).
        /// Sends activation email to admin.
        /// </summary>
        public async Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug)
        {
            try
            {
                // Validate tenant slug is unique
                var existingTenant = await _unitOfWork.Tenants
                    .Query()
                    .FirstOrDefaultAsync(t => t.TenantSlug == tenantSlug);
                
                if (existingTenant != null)
                {
                    throw new InvalidOperationException($"Tenant slug '{tenantSlug}' is already taken.");
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
                    throw new InvalidOperationException($"Tenant '{tenantSlug}' not found.");
                }

                if (tenant.ActivationToken != activationToken)
                {
                    throw new InvalidOperationException("Invalid activation token.");
                }

                if (tenant.Status != "Pending")
                {
                    throw new InvalidOperationException($"Tenant is already {tenant.Status}.");
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
                        throw new InvalidOperationException($"Failed to provision database for tenant {tenant.Id}. Activation cannot complete.");
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
        /// </summary>
        private async Task SendActivationEmailAsync(Tenant tenant)
        {
            try
            {
                var activationUrl = $"https://yourdomain.com/auth/activate?slug={tenant.TenantSlug}&token={tenant.ActivationToken}";
                
                var emailBody = $@"
                    <h2>Welcome to GRC Platform!</h2>
                    <p>Your organization <strong>{tenant.OrganizationName}</strong> has been registered.</p>
                    <p>Please click the link below to activate your account:</p>
                    <p><a href='{activationUrl}'>Activate Your Account</a></p>
                    <p>This link expires in 24 hours.</p>
                    <p>If you did not request this, please ignore this email.</p>
                ";

                await _emailService.SendEmailAsync(
                    to: tenant.AdminEmail,
                    subject: $"Activate Your GRC Platform Account - {tenant.OrganizationName}",
                    htmlBody: emailBody
                );

                _logger.LogInformation($"Activation email sent to {tenant.AdminEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send activation email to {tenant.AdminEmail}");
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
    }
}
