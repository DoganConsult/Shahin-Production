using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using System.Security.Claims;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization requirement that ensures user is an active, non-deleted Platform Admin.
/// This closes the security gap where a user could remain in PlatformAdmin role
/// even after their PlatformAdmin record is suspended/deleted.
/// </summary>
public class ActivePlatformAdminRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler that verifies the user has an active PlatformAdmin record.
/// Checks: Role = PlatformAdmin AND PlatformAdmin.Status = "Active" AND !IsDeleted
/// </summary>
public class ActivePlatformAdminHandler : AuthorizationHandler<ActivePlatformAdminRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ActivePlatformAdminHandler> _logger;

    public ActivePlatformAdminHandler(
        IServiceProvider serviceProvider,
        ILogger<ActivePlatformAdminHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActivePlatformAdminRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ActivePlatformAdmin check failed: No user ID in claims");
            return;
        }

        // Check if user has PlatformAdmin role
        if (!context.User.IsInRole("PlatformAdmin"))
        {
            _logger.LogWarning("ActivePlatformAdmin check failed: User {UserId} not in PlatformAdmin role", userId);
            return;
        }

        // Check if user has active PlatformAdmin record
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

        var platformAdmin = await dbContext.PlatformAdmins
            .AsNoTracking()
            .FirstOrDefaultAsync(pa => pa.UserId == Guid.Parse(userId) && !pa.IsDeleted);

        if (platformAdmin == null)
        {
            _logger.LogWarning("ActivePlatformAdmin check failed: No PlatformAdmin record for user {UserId}", userId);
            return;
        }

        if (platformAdmin.Status != "Active")
        {
            _logger.LogWarning("ActivePlatformAdmin check failed: PlatformAdmin {UserId} status is {Status}",
                userId, platformAdmin.Status);
            return;
        }

        // All checks passed
        _logger.LogDebug("ActivePlatformAdmin check passed for user {UserId}", userId);
        context.Succeed(requirement);
    }
}
