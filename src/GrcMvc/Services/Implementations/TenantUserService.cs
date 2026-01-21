using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for tenant user operations
    /// Follows ABP Framework best practices: Application Services handle data access logic
    /// </summary>
    public class TenantUserService : ITenantUserService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<TenantUserService> _logger;

        public TenantUserService(
            GrcDbContext context,
            ILogger<TenantUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get tenant user by user ID (most recently activated tenant)
        /// Uses deterministic ordering for consistent tenant selection
        /// </summary>
        public async Task<TenantUser?> GetTenantUserByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var tenantUser = await _context.TenantUsers
                .AsNoTracking()
                .Include(tu => tu.Tenant)
                .Where(tu => tu.UserId == userId && tu.Status == "Active" && !tu.IsDeleted)
                .OrderByDescending(tu => tu.ActivatedAt ?? tu.CreatedDate) // Most recently activated
                .ThenBy(tu => tu.CreatedDate) // Creation date as tiebreaker
                .FirstOrDefaultAsync();

            if (tenantUser != null)
            {
                _logger.LogDebug("Retrieved tenant user for userId: {UserId}, tenantId: {TenantId}", 
                    userId, tenantUser.TenantId);
            }
            else
            {
                _logger.LogDebug("No tenant user found for userId: {UserId}", userId);
            }

            return tenantUser;
        }

        /// <summary>
        /// Get tenant user by user ID and tenant ID
        /// </summary>
        public async Task<TenantUser?> GetTenantUserAsync(string userId, Guid tenantId)
        {
            if (string.IsNullOrEmpty(userId) || tenantId == Guid.Empty)
                return null;

            return await _context.TenantUsers
                .AsNoTracking()
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => 
                    tu.UserId == userId && 
                    tu.TenantId == tenantId && 
                    tu.Status == "Active" && 
                    !tu.IsDeleted);
        }

        /// <summary>
        /// Check if user belongs to tenant
        /// </summary>
        public async Task<bool> UserBelongsToTenantAsync(string userId, Guid tenantId)
        {
            if (string.IsNullOrEmpty(userId) || tenantId == Guid.Empty)
                return false;

            return await _context.TenantUsers
                .AsNoTracking()
                .AnyAsync(tu => 
                    tu.UserId == userId && 
                    tu.TenantId == tenantId && 
                    tu.Status == "Active" && 
                    !tu.IsDeleted);
        }
    }
}