using System.Security.Claims;
using System.Threading.Tasks;
using GrcMvc.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Transforms claims to add TenantId claim automatically if missing
    /// This ensures tenant context is available even if claim wasn't added during login
    /// </summary>
    public class ClaimsTransformationService : IClaimsTransformation
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<ClaimsTransformationService> _logger;

        public ClaimsTransformationService(
            GrcDbContext context,
            ILogger<ClaimsTransformationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // If TenantId claim already exists, no transformation needed
            if (principal.FindFirst("TenantId") != null)
                return principal;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return principal;

            // Get tenant from database (only if claim missing)
            var tenantUser = await _context.TenantUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(tu => tu.UserId == userId && !tu.IsDeleted);

            if (tenantUser?.TenantId != null)
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim("TenantId", tenantUser.TenantId.ToString()));
                principal.AddIdentity(identity);
                
                _logger.LogDebug("Added TenantId claim for user {UserId}", userId);
            }

            return principal;
        }
    }
}
