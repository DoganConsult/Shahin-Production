using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for authorization/role management service
    /// </summary>
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<bool> HasRoleAsync(string userId, string role);
        Task<UserRoleDto?> GetUserRolesAsync(string userId);
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<bool> RevokeRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetPermissionsAsync(string userId);
        Task<IEnumerable<string>> GetRolesAsync(string userId);
    }
}
