using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// GRC-specific email service with templated emails
    /// </summary>
    public interface IGrcEmailService
    {
        /// <summary>
        /// Send password reset email
        /// </summary>
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, bool isArabic = true);

        /// <summary>
        /// Send welcome email to new users
        /// </summary>
        Task SendWelcomeEmailAsync(string toEmail, string userName, string loginUrl, string organizationName, bool isArabic = true);

        /// <summary>
        /// Send MFA verification code
        /// </summary>
        Task SendMfaCodeEmailAsync(string toEmail, string userName, string verificationCode, int expiryMinutes = 10, bool isArabic = true);

        /// <summary>
        /// Send email confirmation link
        /// </summary>
        Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink, bool isArabic = true);

        /// <summary>
        /// Send user invitation email
        /// </summary>
        Task SendInvitationEmailAsync(string toEmail, string userName, string inviterName, string organizationName, string inviteLink, bool isArabic = true);

        /// <summary>
        /// Send password changed notification
        /// </summary>
        Task SendPasswordChangedNotificationAsync(string toEmail, string userName, bool isArabic = true);

        /// <summary>
        /// Send account locked notification
        /// </summary>
        Task SendAccountLockedNotificationAsync(string toEmail, string userName, string unlockTime, bool isArabic = true);

        /// <summary>
        /// Send new login alert
        /// </summary>
        Task SendNewLoginAlertAsync(string toEmail, string userName, string ipAddress, string location, string deviceInfo, bool isArabic = true);

        /// <summary>
        /// Send trial activation email
        /// </summary>
        Task SendTrialActivationEmailAsync(string toEmail, string userName, string activationToken, bool isArabic = true);

        /// <summary>
        /// Send trial nurture email (welcome, nudge, midpoint, etc.)
        /// </summary>
        Task SendTrialNurtureEmailAsync(string toEmail, string templateName, string companyName, int daysRemaining, bool isArabic = true);

        /// <summary>
        /// Send generic templated email
        /// </summary>
        Task SendTemplatedEmailAsync(string toEmail, string templateName, object model, bool isArabic = true);

        /// <summary>
        /// Send onboarding activation email after tenant creation
        /// </summary>
        Task SendOnboardingActivationEmailAsync(string toEmail, string organizationName, string activationLink, string tenantSlug, bool isArabic = true);

        /// <summary>
        /// Send team member invitation email from onboarding Section H
        /// </summary>
        Task SendTeamInvitationEmailAsync(string toEmail, string firstName, string inviterName, string organizationName, string invitationLink, string roleName, bool isArabic = true);

        /// <summary>
        /// Send abandonment recovery email for incomplete onboarding
        /// </summary>
        Task SendOnboardingAbandonmentRecoveryEmailAsync(string toEmail, string firstName, string organizationName, string resumeLink, int daysIncomplete, bool isArabic = true);

        /// <summary>
        /// Send progress reminder email for stalled onboarding
        /// </summary>
        Task SendOnboardingProgressReminderEmailAsync(string toEmail, string firstName, string organizationName, string resumeLink, int currentStep, int totalSteps, int daysSinceLastActivity, bool isArabic = true);

        /// <summary>
        /// Send welcome email after onboarding completion
        /// </summary>
        Task SendOnboardingWelcomeEmailAsync(string toEmail, string firstName, string organizationName, string dashboardLink, bool isArabic = true);
    }
}
