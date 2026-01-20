using System.Diagnostics;
using GrcMvc.Configuration;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Models.Entities;
using Volo.Abp.Identity;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Facade that routes between legacy PlatformAdminService and enhanced services
/// </summary>
public class UserManagementFacade : IUserManagementFacade
{
    private readonly IPlatformAdminService _legacyService;
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISecurePasswordGenerator _securePasswordGenerator;
    private readonly IOptions<GrcFeatureOptions> _featureOptions;
    private readonly IMetricsService _metrics;
    private readonly ILogger<UserManagementFacade> _logger;
    
    public UserManagementFacade(
        IPlatformAdminService legacyService,
        IIdentityUserAppService identityUserAppService,
        UserManager<ApplicationUser> userManager,
        ISecurePasswordGenerator securePasswordGenerator,
        IOptions<GrcFeatureOptions> featureOptions,
        IMetricsService metrics,
        ILogger<UserManagementFacade> logger)
    {
        _legacyService = legacyService;
        _identityUserAppService = identityUserAppService;
        _userManager = userManager;
        _securePasswordGenerator = securePasswordGenerator;
        _featureOptions = featureOptions;
        _metrics = metrics;
        _logger = logger;
    }
    
    public async Task<UserDto> GetUserAsync(string userId)
    {
        var sw = Stopwatch.StartNew();
        var useEnhanced = ShouldUseEnhanced("GetUser", userId);
        
        try
        {
            UserDto result;
            
            if (useEnhanced)
            {
                // Use enhanced UserManager approach
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new UserNotFoundException(userId);
                
                var roles = await _userManager.GetRolesAsync(user);
                
                result = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    IsActive = user.IsActive,
                    Roles = roles.ToList()
                };
            }
            else
            {
                // Use legacy service
                var legacyUser = await _legacyService.GetByUserIdAsync(userId);
                if (legacyUser == null)
                    throw new UserNotFoundException(userId);
                
                var identityUser = await _userManager.FindByIdAsync(userId);
                var roles = identityUser != null ? await _userManager.GetRolesAsync(identityUser) : new List<string>();
                
                result = new UserDto
                {
                    Id = legacyUser.UserId,
                    Email = legacyUser.ContactEmail ?? string.Empty,
                    FullName = legacyUser.DisplayName ?? string.Empty,
                    IsActive = legacyUser.Status == "Active",
                    Roles = roles.ToList()
                };
            }
            
            sw.Stop();
            _metrics.TrackMethodCall("GetUser", useEnhanced ? "Enhanced" : "Legacy", true, sw.ElapsedMilliseconds);
            
            // Verify consistency if enabled
            if (_featureOptions.Value.VerifyConsistency && useEnhanced)
            {
                await VerifyUserConsistency(userId, result);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _metrics.TrackMethodCall("GetUser", useEnhanced ? "Enhanced" : "Legacy", false, sw.ElapsedMilliseconds);
            _logger.LogError(ex, "Error in GetUserAsync via {Implementation}", useEnhanced ? "Enhanced" : "Legacy");
            throw;
        }
    }
    
    public async Task<bool> ResetPasswordAsync(string adminUserId, string targetUserId, string newPassword)
    {
        var sw = Stopwatch.StartNew();
        var useEnhanced = ShouldUseEnhanced("ResetPassword", adminUserId);
        
        try
        {
            bool success;
            
            if (useEnhanced)
            {
                // Use enhanced secure password generation
                var user = await _userManager.FindByIdAsync(targetUserId);
                if (user == null)
                    return false;
                
                // Use crypto-safe password if feature enabled
                var passwordToUse = _featureOptions.Value.UseSecurePasswordGeneration
                    ? _securePasswordGenerator.GeneratePassword()
                    : newPassword;
                
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, passwordToUse);
                
                success = result.Succeeded;
                
                if (success)
                {
                    user.MustChangePassword = true;
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }
            else
            {
                // Use legacy service
                success = await _legacyService.ResetPasswordAsync(adminUserId, targetUserId, newPassword);
            }
            
            sw.Stop();
            _metrics.TrackMethodCall("ResetPassword", useEnhanced ? "Enhanced" : "Legacy", success, sw.ElapsedMilliseconds);
            
            return success;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _metrics.TrackMethodCall("ResetPassword", useEnhanced ? "Enhanced" : "Legacy", false, sw.ElapsedMilliseconds);
            _logger.LogError(ex, "Error in ResetPasswordAsync via {Implementation}", useEnhanced ? "Enhanced" : "Legacy");
            throw;
        }
    }
    
    public async Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var sw = Stopwatch.StartNew();
        var useEnhanced = ShouldUseEnhanced("GetUsers", "SYSTEM");
        
        try
        {
            List<UserDto> users;
            
            if (useEnhanced)
            {
                // Use enhanced UserManager
                var skip = (pageNumber - 1) * pageSize;
                var allUsers = _userManager.Users
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();
                
                users = new List<UserDto>();
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    users.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = $"{user.FirstName} {user.LastName}".Trim(),
                        IsActive = user.IsActive,
                        Roles = roles.ToList()
                    });
                }
            }
            else
            {
                // Use legacy service
                var legacyAdmins = await _legacyService.GetAllAsync();
                users = legacyAdmins.Select(admin => new UserDto
                {
                    Id = admin.UserId,
                    Email = admin.ContactEmail ?? string.Empty,
                    FullName = admin.DisplayName ?? string.Empty,
                    IsActive = admin.Status == "Active",
                    Roles = new List<string>()
                }).ToList();
            }
            
            sw.Stop();
            _metrics.TrackMethodCall("GetUsers", useEnhanced ? "Enhanced" : "Legacy", true, sw.ElapsedMilliseconds);
            
            return users;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _metrics.TrackMethodCall("GetUsers", useEnhanced ? "Enhanced" : "Legacy", false, sw.ElapsedMilliseconds);
            _logger.LogError(ex, "Error in GetUsersAsync via {Implementation}", useEnhanced ? "Enhanced" : "Legacy");
            throw;
        }
    }
    
    // Helper methods
    
    private bool ShouldUseEnhanced(string feature, string userId)
    {
        var options = _featureOptions.Value;
        
        // Check feature-specific flags
        var useEnhanced = feature switch
        {
            "GetUser" or "GetUsers" or "ResetPassword" => options.UseSecurePasswordGeneration,
            _ => false
        };
        
        // Apply canary percentage
        if (!useEnhanced && options.CanaryPercentage > 0)
        {
            var hash = Math.Abs(userId.GetHashCode());
            useEnhanced = (hash % 100) < options.CanaryPercentage;
        }
        
        // Log decision if enabled
        if (options.LogFeatureFlagDecisions)
        {
            _metrics.TrackFeatureFlagDecision(feature, useEnhanced, userId);
        }
        
        return useEnhanced;
    }
    
    private async Task VerifyUserConsistency(string userId, UserDto enhancedResult)
    {
        try
        {
            var legacyUser = await _legacyService.GetByUserIdAsync(userId);
            if (legacyUser == null)
            {
                _metrics.TrackConsistencyCheck("GetUser", false, "User not found in legacy system");
                return;
            }
            
            var consistent = enhancedResult.Email == legacyUser.ContactEmail &&
                             enhancedResult.IsActive == (legacyUser.Status == "Active");
            
            _metrics.TrackConsistencyCheck("GetUser", consistent, 
                consistent ? "Data matches" : $"Mismatch: Enhanced({enhancedResult.Email}, {enhancedResult.IsActive}) vs Legacy({legacyUser.ContactEmail}, {legacyUser.Status})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying user consistency");
        }
    }
    
}
