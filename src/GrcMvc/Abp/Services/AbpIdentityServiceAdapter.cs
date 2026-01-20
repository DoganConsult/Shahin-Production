using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace GrcMvc.Abp.Services;

/// <summary>
/// Adapter that wraps ABP Identity services while maintaining backward compatibility
/// with existing code that uses UserManager directly.
/// 
/// ABP Services Used:
/// - IIdentityUserAppService: User CRUD operations
/// - IIdentityRoleAppService: Role management
/// - IProfileAppService: Profile management (self-service)
/// - IIdentityUserRepository: Direct data access when needed
/// </summary>
public class AbpIdentityServiceAdapter : IAbpIdentityServiceAdapter
{
    private readonly IIdentityUserAppService _userAppService;
    private readonly IIdentityRoleAppService _roleAppService;
    private readonly IIdentityUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AbpIdentityServiceAdapter> _logger;

    public AbpIdentityServiceAdapter(
        IIdentityUserAppService userAppService,
        IIdentityRoleAppService roleAppService,
        IIdentityUserRepository userRepository,
        ICurrentUser currentUser,
        ILogger<AbpIdentityServiceAdapter> logger)
    {
        _userAppService = userAppService;
        _roleAppService = roleAppService;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user using ABP's IIdentityUserAppService
    /// </summary>
    public async Task<IdentityUserDto> CreateUserAsync(
        string email,
        string password,
        string? firstName = null,
        string? lastName = null,
        string[]? roleNames = null)
    {
        try
        {
            var createDto = new IdentityUserCreateDto
            {
                UserName = email,
                Email = email,
                Password = password,
                Name = firstName,
                Surname = lastName,
                IsActive = true,
                LockoutEnabled = true,
                RoleNames = roleNames ?? Array.Empty<string>()
            };

            var user = await _userAppService.CreateAsync(createDto);
            _logger.LogInformation("User {Email} created via ABP Identity service", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Email} via ABP Identity service", email);
            throw;
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<IdentityUserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            return await _userAppService.GetAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return null;
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<IdentityUserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _userAppService.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User with email {Email} not found", email);
            return null;
        }
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<IdentityUserDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _userAppService.FindByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User with username {Username} not found", username);
            return null;
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    public async Task<IdentityUserDto> UpdateUserAsync(Guid userId, IdentityUserUpdateDto updateDto)
    {
        return await _userAppService.UpdateAsync(userId, updateDto);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    public async Task DeleteUserAsync(Guid userId)
    {
        await _userAppService.DeleteAsync(userId);
        _logger.LogInformation("User {UserId} deleted via ABP Identity service", userId);
    }

    /// <summary>
    /// Get user's roles
    /// </summary>
    public async Task<IList<IdentityRoleDto>> GetUserRolesAsync(Guid userId)
    {
        var result = await _userAppService.GetRolesAsync(userId);
        return result.Items.ToList();
    }

    /// <summary>
    /// Update user's roles
    /// </summary>
    public async Task UpdateUserRolesAsync(Guid userId, string[] roleNames)
    {
        var updateDto = new IdentityUserUpdateRolesDto
        {
            RoleNames = roleNames
        };
        await _userAppService.UpdateRolesAsync(userId, updateDto);
        _logger.LogInformation("User {UserId} roles updated to: {Roles}", userId, string.Join(", ", roleNames));
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    public async Task<List<IdentityRoleDto>> GetAllRolesAsync()
    {
        var result = await _roleAppService.GetAllListAsync();
        return result.Items.ToList();
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    public async Task<IdentityRoleDto> CreateRoleAsync(string roleName, bool isDefault = false, bool isPublic = true)
    {
        var createDto = new IdentityRoleCreateDto
        {
            Name = roleName,
            IsDefault = isDefault,
            IsPublic = isPublic
        };
        return await _roleAppService.CreateAsync(createDto);
    }

    /// <summary>
    /// Get current user info from ABP
    /// </summary>
    public CurrentUserInfo GetCurrentUserInfo()
    {
        return new CurrentUserInfo
        {
            Id = _currentUser.Id,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            Name = _currentUser.Name,
            SurName = _currentUser.SurName,
            TenantId = _currentUser.TenantId,
            IsAuthenticated = _currentUser.IsAuthenticated,
            Roles = _currentUser.Roles?.ToArray() ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Check if current user is in role
    /// </summary>
    public bool IsInRole(string roleName)
    {
        return _currentUser.IsInRole(roleName);
    }

    /// <summary>
    /// Get paged list of users
    /// </summary>
    public async Task<PagedUserResult> GetUsersAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string? sorting = null)
    {
        var input = new GetIdentityUsersInput
        {
            Filter = filter,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount,
            Sorting = sorting
        };

        var result = await _userAppService.GetListAsync(input);
        return new PagedUserResult
        {
            TotalCount = result.TotalCount,
            Items = result.Items.ToList()
        };
    }
}

/// <summary>
/// Interface for ABP Identity service adapter
/// </summary>
public interface IAbpIdentityServiceAdapter
{
    Task<IdentityUserDto> CreateUserAsync(string email, string password, string? firstName = null, string? lastName = null, string[]? roleNames = null);
    Task<IdentityUserDto?> GetUserByIdAsync(Guid userId);
    Task<IdentityUserDto?> GetUserByEmailAsync(string email);
    Task<IdentityUserDto?> GetUserByUsernameAsync(string username);
    Task<IdentityUserDto> UpdateUserAsync(Guid userId, IdentityUserUpdateDto updateDto);
    Task DeleteUserAsync(Guid userId);
    Task<IList<IdentityRoleDto>> GetUserRolesAsync(Guid userId);
    Task UpdateUserRolesAsync(Guid userId, string[] roleNames);
    Task<List<IdentityRoleDto>> GetAllRolesAsync();
    Task<IdentityRoleDto> CreateRoleAsync(string roleName, bool isDefault = false, bool isPublic = true);
    CurrentUserInfo GetCurrentUserInfo();
    bool IsInRole(string roleName);
    Task<PagedUserResult> GetUsersAsync(string? filter = null, int skipCount = 0, int maxResultCount = 10, string? sorting = null);
}

/// <summary>
/// Current user information DTO
/// </summary>
public class CurrentUserInfo
{
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? SurName { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsAuthenticated { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Paged user result DTO
/// </summary>
public class PagedUserResult
{
    public long TotalCount { get; set; }
    public List<IdentityUserDto> Items { get; set; } = new();
}
