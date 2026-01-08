using System;
using System.Threading.Tasks;
using GrcMvc.Services.Implementations;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for credential expiration checking and enforcement.
    /// HIGH FIX: Implements credential expiration enforcement that was previously unused.
    /// </summary>
    public interface ICredentialExpirationService
    {
        /// <summary>
        /// Check if user's credentials have expired for a specific tenant.
        /// </summary>
        Task<CredentialStatus> CheckCredentialStatusAsync(string userId, Guid tenantId);

        /// <summary>
        /// Mark that user has changed their password (clears MustChangePasswordOnFirstLogin).
        /// </summary>
        Task<bool> MarkPasswordChangedAsync(string userId, Guid tenantId);

        /// <summary>
        /// Extend credential expiration for a user.
        /// </summary>
        Task<bool> ExtendCredentialExpirationAsync(string userId, Guid tenantId, int additionalDays, string extendedBy);
    }
}
