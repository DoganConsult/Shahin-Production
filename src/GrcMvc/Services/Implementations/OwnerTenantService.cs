using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Abp;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using AbpTenant = Volo.Abp.TenantManagement.Tenant;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for owner tenant creation and management
    /// </summary>
    public class OwnerTenantService : IOwnerTenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
        private readonly IAppEmailSender _emailSender;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<OwnerTenantService> _logger;
        private readonly GrcDbContext _context;

        public OwnerTenantService(
            IUnitOfWork unitOfWork,
            ITenantService tenantService,
            IIdentityUserAppService identityUserAppService,
            UserManager<ApplicationUser> userManager,
            RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager,
            IAppEmailSender emailSender,
            IAuditEventService auditService,
            ILogger<OwnerTenantService> logger,
            GrcDbContext context)
        {
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
            _identityUserAppService = identityUserAppService;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _auditService = auditService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Create a tenant with full features (bypass payment)
        /// </summary>
        public async Task<AbpTenant> CreateTenantWithFullFeaturesAsync(
            string organizationName,
            string adminEmail,
            string tenantSlug,
            string ownerId,
            int expirationDays = 14)
        {
            try
            {
                // Use existing TenantService to create tenant
                var tenant = await _tenantService.CreateTenantAsync(organizationName, adminEmail, tenantSlug);

                // Update tenant with owner-created flags using ABP ExtraProperties
                tenant.SetIsOwnerCreated(true);
                tenant.SetCreatedByOwnerId(ownerId);
                tenant.SetBypassPayment(true);
                tenant.SetSubscriptionTier("Enterprise");
                tenant.SetCredentialExpiresAt(DateTime.UtcNow.AddDays(expirationDays));
                tenant.SetStatus("Active"); // Auto-activate owner-created tenants
                tenant.SetActivatedAt(DateTime.UtcNow);
                tenant.SetActivatedBy(ownerId.ToString());
                tenant.SetIsActive(true);

                // Log audit event
                await _auditService.LogEventAsync(
                    tenantId: tenant.Id,
                    eventType: "OwnerTenantCreated",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenant.Id.ToString(),
                    action: "Create",
                    actor: ownerId.ToString(),
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        organizationName,
                        tenantSlug,
                        bypassPayment = true,
                        subscriptionTier = "Enterprise",
                        expirationDays
                    }),
                    correlationId: tenant.GetCorrelationId()
                );

                _logger.LogInformation("Owner {OwnerId} created tenant {TenantId} with full features", ownerId, tenant.Id);
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant with full features for owner {OwnerId}", ownerId);
                throw;
            }
        }

        /// <summary>
        /// Generate tenant admin account with credentials
        /// </summary>
        public async Task<TenantAdminCredentialsDto> GenerateTenantAdminAccountAsync(
            Guid tenantId,
            string ownerId,
            int expirationDays = 14)
        {
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantId);
                }

                // Check if admin account already generated
                if (tenant.AdminAccountGenerated)
                {
                    throw new EntityExistsException("AdminAccount", "TenantId", tenantId.ToString());
                }

                // Generate secure username: admin-{tenant-slug}
                var username = $"admin-{tenant.TenantSlug}";

                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    // Append random suffix if exists
                    username = $"admin-{tenant.TenantSlug}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                }

                // Generate secure password (12+ chars, mixed case, numbers, symbols)
                var password = GenerateSecurePassword(16);

                // Create ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = tenant.AdminEmail,
                    EmailConfirmed = true, // Auto-confirm for owner-generated accounts
                    FirstName = "Admin",
                    LastName = tenant.OrganizationName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new GrcException($"Failed to create user: {errors}", GrcErrorCodes.ValidationFailed);
                }

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
                }

                // Assign Admin role
                await _userManager.AddToRoleAsync(user, "Admin");

                // Create TenantUser linking to tenant
                var expirationDate = DateTime.UtcNow.AddDays(expirationDays);
                var tenantUser = new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = user.Id,
                    RoleCode = "Admin",
                    TitleCode = "TENANT_ADMIN",
                    Status = "Active",
                    IsOwnerGenerated = true,
                    GeneratedByOwnerId = ownerId,
                    CredentialExpiresAt = expirationDate,
                    MustChangePasswordOnFirstLogin = true,
                    ActivatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.TenantUsers.AddAsync(tenantUser);

                // Create OwnerTenantCreation record for audit
                var ownerTenantCreation = new OwnerTenantCreation
                {
                    Id = Guid.NewGuid(),
                    OwnerId = ownerId,
                    TenantId = tenantId,
                    AdminUsername = username,
                    CredentialsExpiresAt = expirationDate,
                    DeliveryMethod = "Manual", // Will be updated when delivered
                    CredentialsDelivered = false,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.OwnerTenantCreations.AddAsync(ownerTenantCreation);

                // Update tenant
                tenant.AdminAccountGenerated = true;
                tenant.AdminAccountGeneratedAt = DateTime.UtcNow;
                tenant.CredentialExpiresAt = expirationDate;

                await _unitOfWork.Tenants.UpdateAsync(tenant);
                await _unitOfWork.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "TenantAdminAccountGenerated",
                    affectedEntityType: "TenantUser",
                    affectedEntityId: tenantUser.Id.ToString(),
                    action: "Generate",
                    actor: ownerId.ToString(),
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        username,
                        expirationDate,
                        tenantId
                    }),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation("Generated admin account {Username} for tenant {TenantId} by owner {OwnerId}", 
                    username, tenantId, ownerId);

                // Build login URL
                var loginUrl = $"/Account/TenantAdminLogin?tenantId={tenantId}";

                return new TenantAdminCredentialsDto
                {
                    TenantId = tenantId,
                    TenantSlug = tenant.TenantSlug,
                    OrganizationName = tenant.OrganizationName,
                    Username = username,
                    Password = password, // Only shown once
                    ExpiresAt = expirationDate,
                    LoginUrl = loginUrl,
                    MustChangePasswordOnFirstLogin = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating admin account for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Validate tenant admin credentials (Tenant ID + Username + Password)
        /// </summary>
        public async Task<bool> ValidateTenantAdminCredentialsAsync(
            Guid tenantId,
            string username,
            string password)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return false;
                }

                // Check TenantUser exists and is Admin for this tenant
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == user.Id);

                if (tenantUser == null || tenantUser.RoleCode != "Admin" || tenantUser.Status != "Active")
                {
                    return false;
                }

                // Check credential expiration if owner-generated
                if (tenantUser.IsOwnerGenerated && tenantUser.CredentialExpiresAt.HasValue)
                {
                    if (tenantUser.CredentialExpiresAt.Value < DateTime.UtcNow)
                    {
                        return false; // Credentials expired
                    }
                }

                // Verify password
                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials for tenant {TenantId}, username {Username}", tenantId, username);
                return false;
            }
        }

        /// <summary>
        /// Check if credentials are expired
        /// </summary>
        public async Task<bool> CheckCredentialExpirationAsync(Guid tenantId)
        {
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null || !tenant.CredentialExpiresAt.HasValue)
                {
                    return false; // No expiration set
                }

                return tenant.CredentialExpiresAt.Value < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking credential expiration for tenant {TenantId}", tenantId);
                return false;
            }
        }

        /// <summary>
        /// Extend credential expiration
        /// </summary>
        public async Task<bool> ExtendCredentialExpirationAsync(Guid tenantId, int additionalDays)
        {
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    return false;
                }

                // Update tenant expiration
                if (tenant.CredentialExpiresAt.HasValue)
                {
                    tenant.CredentialExpiresAt = tenant.CredentialExpiresAt.Value.AddDays(additionalDays);
                }
                else
                {
                    tenant.CredentialExpiresAt = DateTime.UtcNow.AddDays(additionalDays);
                }

                // Update TenantUser expiration
                var tenantUsers = await _context.TenantUsers
                    .Where(tu => tu.TenantId == tenantId && tu.IsOwnerGenerated)
                    .ToListAsync();

                foreach (var tenantUser in tenantUsers)
                {
                    if (tenantUser.CredentialExpiresAt.HasValue)
                    {
                        tenantUser.CredentialExpiresAt = tenantUser.CredentialExpiresAt.Value.AddDays(additionalDays);
                    }
                    else
                    {
                        tenantUser.CredentialExpiresAt = DateTime.UtcNow.AddDays(additionalDays);
                    }
                }

                await _unitOfWork.Tenants.UpdateAsync(tenant);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Extended credential expiration for tenant {TenantId} by {Days} days", 
                    tenantId, additionalDays);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending expiration for tenant {TenantId}", tenantId);
                return false;
            }
        }

        /// <summary>
        /// Deliver credentials via specified method
        /// </summary>
        public async Task<bool> DeliverCredentialsAsync(Guid tenantId, string deliveryMethod, string? recipientEmail = null)
        {
            try
            {
                var ownerTenantCreation = await _context.OwnerTenantCreations
                    .FirstOrDefaultAsync(otc => otc.TenantId == tenantId && !otc.CredentialsDelivered);

                if (ownerTenantCreation == null)
                {
                    _logger.LogWarning("No pending credential delivery found for tenant {TenantId}", tenantId);
                    return false;
                }

                ownerTenantCreation.DeliveryMethod = deliveryMethod;
                ownerTenantCreation.DeliveredAt = DateTime.UtcNow;
                ownerTenantCreation.CredentialsDelivered = true;

                await _unitOfWork.OwnerTenantCreations.UpdateAsync(ownerTenantCreation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked credentials as delivered for tenant {TenantId} via {Method}", 
                    tenantId, deliveryMethod);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering credentials for tenant {TenantId}", tenantId);
                return false;
            }
        }

        /// <summary>
        /// Generate secure password (12+ chars, mixed case, numbers, symbols)
        /// </summary>
        private string GenerateSecurePassword(int length = 16)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            const string allChars = lowercase + uppercase + numbers + symbols;

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var password = new StringBuilder(length);
            
            // Ensure at least one of each type
            password.Append(lowercase[bytes[0] % lowercase.Length]);
            password.Append(uppercase[bytes[1] % uppercase.Length]);
            password.Append(numbers[bytes[2] % numbers.Length]);
            password.Append(symbols[bytes[3] % symbols.Length]);

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[bytes[i] % allChars.Length]);
            }

            // Shuffle the password
            var chars = password.ToString().ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = bytes[i % bytes.Length] % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}
