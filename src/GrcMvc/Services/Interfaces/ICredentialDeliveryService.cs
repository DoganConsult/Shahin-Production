using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for credential delivery service
    /// </summary>
    public interface ICredentialDeliveryService
    {
        /// <summary>
        /// Send credentials via email
        /// </summary>
        Task<bool> SendCredentialsViaEmailAsync(
            Guid tenantId, 
            string recipientEmail, 
            TenantAdminCredentialsDto credentials);
            
        /// <summary>
        /// Generate credentials PDF for download
        /// </summary>
        Task<byte[]> GenerateCredentialsPdfAsync(
            Guid tenantId, 
            TenantAdminCredentialsDto credentials);
            
        /// <summary>
        /// Get credentials formatted for manual sharing (copy/paste)
        /// </summary>
        Task<string> GetCredentialsForManualShareAsync(
            Guid tenantId, 
            TenantAdminCredentialsDto credentials);
    }
}
