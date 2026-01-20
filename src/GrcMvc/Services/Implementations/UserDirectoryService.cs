using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Volo.Abp.Identity;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of IUserDirectoryService that queries GrcAuthDbContext.
    /// Provides batch lookups to avoid N+1 queries when accessing user data.
    /// </summary>
    public class UserDirectoryService : IUserDirectoryService
    {
        private readonly GrcAuthDbContext _authContext;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserDirectoryService> _logger;

        private const string UserCachePrefix = "user_";
        private const string RoleCachePrefix = "role_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public UserDirectoryService(
            GrcAuthDbContext authContext,
            IIdentityUserAppService identityUserAppService,
            UserManager<ApplicationUser> userManager,
            RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager,
            IMemoryCache cache,
            ILogger<UserDirectoryService> logger)
        {
            _authContext = authContext;
            _identityUserAppService = identityUserAppService;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
            _logger = logger;
        }

        // ===== USER LOOKUPS =====

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var cacheKey = $"{UserCachePrefix}{userId}";
            if (_cache.TryGetValue(cacheKey, out ApplicationUser? cached))
                return cached;

            var user = await _authContext.Users.FindAsync(userId);
            if (user != null)
            {
                _cache.Set(cacheKey, user, CacheDuration);
            }
            return user;
        }

        public async Task<Dictionary<string, ApplicationUser>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            var ids = userIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids == null || !ids.Any())
                return new Dictionary<string, ApplicationUser>();

            // Check cache first
            var result = new Dictionary<string, ApplicationUser>();
            var uncachedIds = new List<string>();

            foreach (var id in ids)
            {
                var cacheKey = $"{UserCachePrefix}{id}";
                if (_cache.TryGetValue(cacheKey, out ApplicationUser? cached) && cached != null)
                {
                    result[id] = cached;
                }
                else
                {
                    uncachedIds.Add(id);
                }
            }

            // Fetch uncached users in batch
            if (uncachedIds.Any())
            {
                var users = await _authContext.Users
                    .Where(u => uncachedIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var user in users)
                {
                    result[user.Id] = user;
                    _cache.Set($"{UserCachePrefix}{user.Id}", user, CacheDuration);
                }
            }

            return result;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<List<ApplicationUser>> SearchUsersAsync(string searchTerm, int maxResults = 20)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<ApplicationUser>();

            var normalizedSearch = searchTerm.ToUpperInvariant();
            return await _authContext.Users
                .Where(u => u.NormalizedEmail!.Contains(normalizedSearch) ||
                           u.FirstName.ToUpper().Contains(normalizedSearch) ||
                           u.LastName.ToUpper().Contains(normalizedSearch))
                .Take(maxResults)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetAllActiveUsersAsync()
        {
            return await _authContext.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _authContext.Users.CountAsync();
        }

        // ===== ROLE LOOKUPS =====

        public async Task<Microsoft.AspNetCore.Identity.IdentityRole?> GetRoleByIdAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId)) return null;

            var cacheKey = $"{RoleCachePrefix}{roleId}";
            if (_cache.TryGetValue(cacheKey, out Microsoft.AspNetCore.Identity.IdentityRole? cached))
                return cached;

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                _cache.Set(cacheKey, role, CacheDuration);
            }
            return role;
        }

        public async Task<Dictionary<string, Microsoft.AspNetCore.Identity.IdentityRole>> GetRolesByIdsAsync(IEnumerable<string> roleIds)
        {
            var ids = roleIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids == null || !ids.Any())
                return new Dictionary<string, Microsoft.AspNetCore.Identity.IdentityRole>();

            var result = new Dictionary<string, Microsoft.AspNetCore.Identity.IdentityRole>();
            var uncachedIds = new List<string>();

            foreach (var id in ids)
            {
                var cacheKey = $"{RoleCachePrefix}{id}";
                if (_cache.TryGetValue(cacheKey, out Microsoft.AspNetCore.Identity.IdentityRole? cached) && cached != null)
                {
                    result[id] = cached;
                }
                else
                {
                    uncachedIds.Add(id);
                }
            }

            if (uncachedIds.Any())
            {
                var roles = await _authContext.Roles
                    .Where(r => uncachedIds.Contains(r.Id))
                    .ToListAsync();

                foreach (var role in roles)
                {
                    result[role.Id] = role;
                    _cache.Set($"{RoleCachePrefix}{role.Id}", role, CacheDuration);
                }
            }

            return result;
        }

        public async Task<Microsoft.AspNetCore.Identity.IdentityRole?> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return null;
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<List<Microsoft.AspNetCore.Identity.IdentityRole>> GetAllRolesAsync()
        {
            return await _authContext.Roles.OrderBy(r => r.Name).ToListAsync();
        }

        // ===== USER-ROLE LOOKUPS =====

        public async Task<List<string>> GetUserRoleNamesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<string>();

            var user = await GetUserByIdAsync(userId);
            if (user == null) return new List<string>();

            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return new List<ApplicationUser>();
            return (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
                return false;

            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        // ===== DISPLAY HELPERS =====

        public async Task<string> GetUserDisplayNameAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return userId ?? "Unknown";

            if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                return $"{user.FirstName} {user.LastName}".Trim();

            return user.Email ?? userId;
        }

        public async Task<Dictionary<string, string>> GetUserDisplayNamesAsync(IEnumerable<string> userIds)
        {
            var users = await GetUsersByIdsAsync(userIds);
            var result = new Dictionary<string, string>();

            foreach (var id in userIds.Distinct())
            {
                if (users.TryGetValue(id, out var user))
                {
                    if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                        result[id] = $"{user.FirstName} {user.LastName}".Trim();
                    else
                        result[id] = user.Email ?? id;
                }
                else
                {
                    result[id] = id;
                }
            }

            return result;
        }
    }
}
