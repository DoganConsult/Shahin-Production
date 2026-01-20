using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Volo.Abp.Identity;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Enhanced authentication service - uses session-based claims instead of DB persistence
/// Fixes bug: tenant claims should not be persisted to AspNetUserClaims table
/// </summary>
public class EnhancedAuthService : IEnhancedAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EnhancedAuthService> _logger;
    
    public EnhancedAuthService(
        SignInManager<ApplicationUser> signInManager,
        IIdentityUserAppService identityUserAppService,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EnhancedAuthService> logger)
    {
        _signInManager = signInManager;
        _identityUserAppService = identityUserAppService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public async Task SignInWithTenantContextAsync(
        ApplicationUser user, 
        Guid tenantId, 
        string tenantRole,
        bool isPersistent = false)
    {
        // Create session-only claims (NOT persisted to database)
        var additionalClaims = new List<Claim>
        {
            new Claim("TenantId", tenantId.ToString()),
            new Claim("TenantRole", tenantRole),
            new Claim("SessionStarted", DateTime.UtcNow.ToString("O"))
        };
        
        // Sign in with claims - these will be in the cookie/session, not DB
        await _signInManager.SignInWithClaimsAsync(user, isPersistent, additionalClaims);
        
        _logger.LogInformation(
            "Enhanced auth: User {UserId} signed in with tenant {TenantId} (session-only)",
            user.Id, tenantId);
    }
    
    public async Task<Guid?> GetCurrentTenantIdAsync()
    {
        await Task.CompletedTask; // For async consistency
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        
        // Get from session claims (not from database)
        var tenantIdClaim = httpContext.User.FindFirst("TenantId")?.Value;
        
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }
        
        return null;
    }
    
    public async Task SwitchTenantContextAsync(Guid newTenantId, string newTenantRole)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new AuthenticationException("User is not authenticated");
        }
        
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new AuthenticationException("User ID not found in claims");
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }
        
        // Sign out and re-sign in with new tenant context
        await _signInManager.SignOutAsync();
        await SignInWithTenantContextAsync(user, newTenantId, newTenantRole);
        
        _logger.LogInformation(
            "Enhanced auth: User {UserId} switched to tenant {TenantId}",
            userId, newTenantId);
    }
}
