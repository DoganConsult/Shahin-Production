using GrcMvc.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Email-based MFA service interface
    /// </summary>
    public interface IEmailMfaService
    {
        /// <summary>
        /// Generate and send MFA code via email
        /// </summary>
        Task<bool> SendMfaCodeAsync(string userId, string email, string userName);

        /// <summary>
        /// Verify the MFA code entered by user
        /// </summary>
        Task<MfaVerificationResult> VerifyCodeAsync(string userId, string enteredCode);

        /// <summary>
        /// Check if user has pending MFA verification
        /// </summary>
        bool HasPendingMfa(string userId);

        /// <summary>
        /// Cancel pending MFA for user
        /// </summary>
        void CancelMfa(string userId);

        /// <summary>
        /// Get remaining time for MFA code
        /// </summary>
        TimeSpan? GetRemainingTime(string userId);
    }
}
