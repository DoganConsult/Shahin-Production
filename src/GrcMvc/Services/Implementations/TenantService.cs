using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Abp;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for multi-tenant provisioning and management.
    /// Uses ABP Tenant with ExtraProperties for all business fields.
    /// Single tenant entity - no custom Tenant table needed.
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ILogger<TenantService> _logger;
        private readonly IEmailService _emailService;
        private readonly IGrcEmailService _grcEmailService;
        private readonly IAuditEventService _auditService;
        private readonly ITenantProvisioningService _provisioningService;
        private readonly IConfiguration _configuration;

        public TenantService(
            ITenantAppService tenantAppService,
            ITenantRepository tenantRepository,
            ILogger<TenantService> logger,
            IEmailService emailService,
            IGrcEmailService grcEmailService,
            IAuditEventService auditService,
            ITenantProvisioningService provisioningService,
            IConfiguration configuration)
        {
            _tenantAppService = tenantAppService;
            _tenantRepository = tenantRepository;
            _logger = logger;
            _emailService = emailService;
            _grcEmailService = grcEmailService;
            _auditService = auditService;
            _provisioningService = provisioningService;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a new tenant (organization).
        /// Uses ABP Tenant with ExtraProperties for business fields.
        /// </summary>
        public async Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug)
        {
            try
            {
                tenantSlug = tenantSlug.ToLower().Trim();
                
                // Check if tenant exists via ABP
                var existingTenant = await _tenantRepository.FindByNameAsync(tenantSlug);
                if (existingTenant != null)
                {
                    throw new EntityExistsException("Tenant", "Slug", tenantSlug);
                }

                // Create ABP Tenant with admin
                var abpTenantDto = await _tenantAppService.CreateAsync(new TenantCreateDto
                {
                    Name = tenantSlug,
                    AdminEmailAddress = adminEmail,
                    AdminPassword = GenerateActivationToken()
                });

                // Get the created tenant entity to set ExtraProperties
                var tenant = await _tenantRepository.GetAsync(abpTenantDto.Id);
                
                // Set business fields via ExtraProperties
                tenant.SetTenantSlug(tenantSlug);
                tenant.SetOrganizationName(organizationName);
                tenant.SetAdminEmail(adminEmail);
                tenant.SetStatus("Pending");
                tenant.SetActivationToken(GenerateActivationToken());
                tenant.SetCorrelationId(Guid.NewGuid().ToString());
                tenant.SetIsActive(false);
                tenant.SetSubscriptionStartDate(DateTime.UtcNow);
                tenant.SetSubscriptionTier("MVP");
                tenant.SetOnboardingStatus("NOT_STARTED");

                await _tenantRepository.UpdateAsync(tenant, autoSave: true);
                _logger.LogInformation("Created ABP tenant {TenantId} with ExtraProperties", tenant.Id);

                // Provision tenant database
                try
                {
                    var provisioned = await _provisioningService.ProvisionTenantAsync(tenant.Id);
                    if (!provisioned)
                    {
                        _logger.LogError("Failed to provision database for tenant {TenantId}", tenant.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error provisioning database for tenant {TenantId}", tenant.Id);
                }

                // Send activation email
                await SendActivationEmailAsync(tenant);

                // Log audit event
                await _auditService.LogEventAsync(
                    tenantId: tenant.Id,
                    eventType: "TenantCreated",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenant.Id.ToString(),
                    action: "Create",
                    actor: "SYSTEM",
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, tenant.Name, organizationName }),
                    correlationId: tenant.GetCorrelationId()
                );

                _logger.LogInformation("Tenant created: {TenantId}, slug: {Slug}", tenant.Id, tenantSlug);
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
                var tenant = await _tenantRepository.FindByNameAsync(tenantSlug);
                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantSlug);
                }

                if (tenant.GetActivationToken() != activationToken)
                {
                    throw new ValidationException("ActivationToken", "Invalid activation token");
                }

                if (tenant.GetStatus() != "Pending")
                {
                    throw new TenantStateException(tenant.GetStatus(), "Pending");
                }

                tenant.SetStatus("Active");
                tenant.SetActivatedAt(DateTime.UtcNow);
                tenant.SetActivatedBy(activatedBy);
                tenant.SetActivationToken(string.Empty);
                tenant.SetIsActive(true);

                await _tenantRepository.UpdateAsync(tenant, autoSave: true);

                // Ensure tenant database is provisioned
                if (!await _provisioningService.IsTenantProvisionedAsync(tenant.Id))
                {
                    _logger.LogWarning("Tenant {TenantId} activated but database not provisioned", tenant.Id);
                    var provisioned = await _provisioningService.ProvisionTenantAsync(tenant.Id);
                    if (!provisioned)
                    {
                        throw new IntegrationException("TenantProvisioning", $"Failed to provision database for tenant {tenant.Id}");
                    }
                }

                await _auditService.LogEventAsync(
                    tenantId: tenant.Id,
                    eventType: "TenantActivated",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenant.Id.ToString(),
                    action: "Activate",
                    actor: activatedBy,
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, Status = tenant.GetStatus() }),
                    correlationId: tenant.GetCorrelationId()
                );

                _logger.LogInformation("Tenant activated: {TenantId}", tenant.Id);
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
            var tenant = await _tenantRepository.FindByNameAsync(tenantSlug);
            if (tenant != null && tenant.GetStatus() == "Active")
                return tenant;
            return null;
        }

        /// <summary>
        /// Get tenant by ID.
        /// </summary>
        public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
        {
            return await _tenantRepository.FindAsync(tenantId);
        }

        /// <summary>
        /// Send activation email with activation link using enterprise email service.
        /// </summary>
        private async Task SendActivationEmailAsync(Tenant tenant)
        {
            try
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var activationUrl = $"{baseUrl}/auth/activate?slug={tenant.Name}&token={tenant.GetActivationToken()}";
                var organizationName = tenant.GetOrganizationName();
                var adminEmail = tenant.GetAdminEmail();
                
                // Use enterprise email service
                await _grcEmailService.SendOnboardingActivationEmailAsync(
                    toEmail: adminEmail,
                    organizationName: organizationName,
                    activationLink: activationUrl,
                    tenantSlug: tenant.Name,
                    isArabic: false // TODO: Detect from tenant preferences
                );

                _logger.LogInformation("Activation email sent to {AdminEmail} for tenant {TenantName}", 
                    adminEmail, tenant.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send activation email to {AdminEmail}", tenant.GetAdminEmail());
                // Don't throw - email failure shouldn't block tenant creation
            }
        }

        private string GenerateActivationToken()
        {
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }

        public async Task<Tenant> SuspendTenantAsync(Guid tenantId, string suspendedBy, string? reason = null)
        {
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.GetStatus() == "Suspended")
                throw new TenantStateException("Suspended", "Active");

            tenant.SetStatus("Suspended");
            tenant.SetIsActive(false);

            await _tenantRepository.UpdateAsync(tenant, autoSave: true);

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantSuspended",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Suspend",
                actor: suspendedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, Status = tenant.GetStatus(), Reason = reason }),
                correlationId: tenant.GetCorrelationId()
            );

            _logger.LogInformation("Tenant {TenantId} suspended by {SuspendedBy}", tenantId, suspendedBy);
            return tenant;
        }

        public async Task<Tenant> ReactivateTenantAsync(Guid tenantId, string reactivatedBy)
        {
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.GetStatus() != "Suspended")
                throw new TenantStateException(tenant.GetStatus(), "Suspended");

            tenant.SetStatus("Active");
            tenant.SetIsActive(true);

            await _tenantRepository.UpdateAsync(tenant, autoSave: true);

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantReactivated",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Reactivate",
                actor: reactivatedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, Status = tenant.GetStatus() }),
                correlationId: tenant.GetCorrelationId()
            );

            _logger.LogInformation("Tenant {TenantId} reactivated by {ReactivatedBy}", tenantId, reactivatedBy);
            return tenant;
        }

        public async Task<Tenant> ArchiveTenantAsync(Guid tenantId, string archivedBy, string? reason = null)
        {
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            if (tenant.GetStatus() == "Archived")
                throw new TenantStateException("Archived", "Active");

            tenant.SetStatus("Archived");
            tenant.SetIsActive(false);

            await _tenantRepository.UpdateAsync(tenant, autoSave: true);

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: "TenantArchived",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: "Archive",
                actor: archivedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, Status = tenant.GetStatus(), Reason = reason }),
                correlationId: tenant.GetCorrelationId()
            );

            _logger.LogInformation("Tenant {TenantId} archived by {ArchivedBy}", tenantId, archivedBy);
            return tenant;
        }

        public async Task<bool> DeleteTenantAsync(Guid tenantId, string deletedBy, bool hardDelete = false)
        {
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Attempt to delete non-existent tenant {TenantId}", tenantId);
                return false;
            }

            await _auditService.LogEventAsync(
                tenantId: tenant.Id,
                eventType: hardDelete ? "TenantHardDeleted" : "TenantSoftDeleted",
                affectedEntityType: "Tenant",
                affectedEntityId: tenant.Id.ToString(),
                action: hardDelete ? "HardDelete" : "SoftDelete",
                actor: deletedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new { tenant.Id, Name = tenant.Name, HardDelete = hardDelete }),
                correlationId: tenant.GetCorrelationId()
            );

            if (hardDelete)
            {
                _logger.LogWarning("HARD DELETE requested for tenant {TenantId} by {DeletedBy}", tenantId, deletedBy);
                await _tenantRepository.DeleteAsync(tenant, autoSave: true);
            }
            else
            {
                tenant.SetStatus("Deleted");
                tenant.SetIsActive(false);
                await _tenantRepository.UpdateAsync(tenant, autoSave: true);
            }
            _logger.LogInformation("Tenant {TenantId} {DeleteType} by {DeletedBy}", tenantId, hardDelete ? "hard deleted" : "soft deleted", deletedBy);
            return true;
        }

        public async Task<bool> ResendActivationEmailAsync(string adminEmail)
        {
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                _logger.LogWarning("ResendActivationEmail called with empty email");
                return false;
            }

            try
            {
                // Get all tenants and filter by ExtraProperties
                var tenants = await _tenantRepository.GetListAsync();
                var tenant = tenants.FirstOrDefault(t => 
                    t.GetAdminEmail().Equals(adminEmail, StringComparison.OrdinalIgnoreCase) && 
                    t.GetStatus() == "Pending");

                if (tenant == null)
                {
                    _logger.LogWarning("Resend activation requested for non-existent or non-pending tenant: {Email}", adminEmail);
                    return true; // Prevent email enumeration
                }

                var expiryHours = _configuration.GetValue<int>("App:ActivationTokenExpiryHours", 48);
                if (string.IsNullOrEmpty(tenant.GetActivationToken()))
                {
                    tenant.SetActivationToken(GenerateActivationToken());
                    await _tenantRepository.UpdateAsync(tenant, autoSave: true);
                }

                await SendActivationEmailAsync(tenant);

                _logger.LogInformation("Activation email resent to {Email} for tenant {TenantName}", adminEmail, tenant.Name);
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
