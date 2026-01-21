using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for handling authorization and role management
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly Dictionary<string, List<string>> _userRoles = new();
        private readonly Dictionary<string, List<string>> _rolePermissions = new();

        public AuthorizationService()
        {
            InitializeRolePermissions();
        }

        private void InitializeRolePermissions()
        {
            _rolePermissions["Admin"] = new List<string> { "read", "write", "delete", "approve", "audit", "manage_users" };
            _rolePermissions["Auditor"] = new List<string> { "read", "audit", "report" };
            _rolePermissions["Approver"] = new List<string> { "read", "approve", "comment" };
            _rolePermissions["User"] = new List<string> { "read", "comment" };
            _rolePermissions["Viewer"] = new List<string> { "read" };
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            await Task.Delay(10);
            
            if (!_userRoles.ContainsKey(userId))
                return false;

            var userRoles = _userRoles[userId];
            foreach (var role in userRoles)
            {
                if (_rolePermissions.ContainsKey(role) && _rolePermissions[role].Contains(permission))
                    return true;
            }

            return false;
        }

        public async Task<bool> HasRoleAsync(string userId, string role)
        {
            await Task.Delay(10);
            
            return _userRoles.ContainsKey(userId) && _userRoles[userId].Contains(role);
        }

        public async Task<UserRoleDto?> GetUserRolesAsync(string userId)
        {
            await Task.Delay(10);

            if (!_userRoles.ContainsKey(userId))
                return null;

            var roles = _userRoles[userId];
            var permissions = new List<string>();

            foreach (var role in roles)
            {
                if (_rolePermissions.ContainsKey(role))
                {
                    permissions.AddRange(_rolePermissions[role]);
                }
            }

            return new UserRoleDto
            {
                UserId = userId,
                UserEmail = $"user{userId}@grc.com",
                Roles = roles,
                Permissions = permissions.Distinct().ToList(),
                AssignedDate = DateTime.UtcNow
            };
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            await Task.Delay(50);

            if (!_rolePermissions.ContainsKey(role))
                return false;

            if (!_userRoles.ContainsKey(userId))
                _userRoles[userId] = new List<string>();

            if (!_userRoles[userId].Contains(role))
                _userRoles[userId].Add(role);

            return true;
        }

        public async Task<bool> RevokeRoleAsync(string userId, string role)
        {
            await Task.Delay(50);

            if (!_userRoles.ContainsKey(userId))
                return false;

            return _userRoles[userId].Remove(role);
        }

        public async Task<IEnumerable<string>> GetPermissionsAsync(string userId)
        {
            await Task.Delay(10);

            if (!_userRoles.ContainsKey(userId))
                return new List<string>();

            var permissions = new HashSet<string>();
            foreach (var role in _userRoles[userId])
            {
                if (_rolePermissions.ContainsKey(role))
                {
                    foreach (var permission in _rolePermissions[role])
                    {
                        permissions.Add(permission);
                    }
                }
            }

            return permissions.ToList();
        }

        public async Task<IEnumerable<string>> GetRolesAsync(string userId)
        {
            await Task.Delay(10);
            
            return _userRoles.ContainsKey(userId) ? _userRoles[userId] : new List<string>();
        }
    }
}
