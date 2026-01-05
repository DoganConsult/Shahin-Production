using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for sending emails (activations, invitations, notifications).
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send an email.
        /// </summary>
        Task SendEmailAsync(string to, string subject, string htmlBody);

        /// <summary>
        /// Send an email to multiple recipients.
        /// </summary>
        Task SendEmailBatchAsync(string[] recipients, string subject, string htmlBody);

        /// <summary>
        /// Send templated email.
        /// </summary>
        Task SendTemplatedEmailAsync(string to, string templateId, Dictionary<string, string> templateData);
    }
}
