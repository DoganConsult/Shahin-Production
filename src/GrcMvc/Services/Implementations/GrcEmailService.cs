using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using RazorLight;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// GRC-specific email service with templated emails
    /// </summary>
    public class GrcEmailService : IGrcEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<GrcEmailService> _logger;
        private readonly RazorLightEngine _razorEngine;
        private readonly string _templatePath;

        public GrcEmailService(
            IEmailService emailService,
            ILogger<GrcEmailService> logger,
            IWebHostEnvironment environment)
        {
            _emailService = emailService;
            _logger = logger;
            _templatePath = Path.Combine(environment.ContentRootPath, "Views", "EmailTemplates");

            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_templatePath)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, bool isArabic = true)
        {
            try
            {
                var model = new
                {
                    UserName = userName,
                    ResetLink = resetLink,
                    ExpiryHours = 24,
                    IsArabic = isArabic
                };

                var htmlContent = await RenderTemplateAsync("PasswordReset.cshtml", model);
                var subject = isArabic ? "🔐 إعادة تعيين كلمة المرور - شاهين" : "🔐 Password Reset - Shahin AI";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName, string loginUrl, string organizationName, bool isArabic = true)
        {
            try
            {
                var model = new
                {
                    UserName = userName,
                    UserEmail = toEmail,
                    LoginUrl = loginUrl,
                    OrganizationName = organizationName,
                    IsArabic = isArabic
                };

                var htmlContent = await RenderTemplateAsync("Welcome.cshtml", model);
                var subject = isArabic ? $"🎉 مرحباً بك في {organizationName}" : $"🎉 Welcome to {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Welcome email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendMfaCodeEmailAsync(string toEmail, string userName, string verificationCode, int expiryMinutes = 10, bool isArabic = true)
        {
            try
            {
                var model = new
                {
                    UserName = userName,
                    VerificationCode = verificationCode,
                    ExpiryMinutes = expiryMinutes,
                    IsArabic = isArabic
                };

                var htmlContent = await RenderTemplateAsync("MfaCode.cshtml", model);
                var subject = isArabic ? $"🔒 رمز التحقق: {verificationCode}" : $"🔒 Verification Code: {verificationCode}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("MFA code email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send MFA code email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink, bool isArabic = true)
        {
            try
            {
                var model = new
                {
                    UserName = userName,
                    ConfirmationLink = confirmationLink,
                    ExpiryHours = 48,
                    IsArabic = isArabic
                };

                var htmlContent = await RenderTemplateAsync("EmailConfirmation.cshtml", model);
                var subject = isArabic ? "✉️ تأكيد البريد الإلكتروني - شاهين" : "✉️ Email Confirmation - Shahin AI";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Email confirmation sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email confirmation to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendInvitationEmailAsync(string toEmail, string userName, string inviterName, string organizationName, string inviteLink, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateInvitationHtml(userName, inviterName, organizationName, inviteLink, isArabic);
                var subject = isArabic 
                    ? $"📨 دعوة للانضمام إلى {organizationName}" 
                    : $"📨 Invitation to join {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Invitation email sent to {Email} from {Inviter}", toEmail, inviterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invitation email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendPasswordChangedNotificationAsync(string toEmail, string userName, bool isArabic = true)
        {
            try
            {
                var htmlContent = GeneratePasswordChangedHtml(userName, isArabic);
                var subject = isArabic ? "🔑 تم تغيير كلمة المرور" : "🔑 Password Changed";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Password changed notification sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password changed notification to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendAccountLockedNotificationAsync(string toEmail, string userName, string unlockTime, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateAccountLockedHtml(userName, unlockTime, isArabic);
                var subject = isArabic ? "⚠️ تم قفل حسابك مؤقتاً" : "⚠️ Account Temporarily Locked";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Account locked notification sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send account locked notification to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendNewLoginAlertAsync(string toEmail, string userName, string ipAddress, string location, string deviceInfo, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateNewLoginAlertHtml(userName, ipAddress, location, deviceInfo, isArabic);
                var subject = isArabic ? "🔔 تسجيل دخول جديد إلى حسابك" : "🔔 New Login to Your Account";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("New login alert sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new login alert to {Email}", toEmail);
                throw;
            }
        }

        private async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            try
            {
                return await _razorEngine.CompileRenderAsync(templateName, model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to render template {Template}, using fallback", templateName);
                return GenerateFallbackHtml(model);
            }
        }

        private string GenerateFallbackHtml(object model)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Shahin AI GRC System</h2>
                    <p>Email notification</p>
                    <hr/>
                    <p style='color: #666; font-size: 12px;'>Powered by Dogan Consult</p>
                </div>";
        }

        private string GenerateInvitationHtml(string userName, string inviterName, string organizationName, string inviteLink, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            
            return $@"
            <!DOCTYPE html>
            <html dir='{dir}'>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 40px 0;'>
                <table style='width: 600px; margin: 0 auto; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background: linear-gradient(135deg, #fd7e14, #e55300); padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h1 style='color: white; margin: 0;'>📨 {(isArabic ? "دعوة للانضمام" : "You're Invited!")}</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: {align};'>
                            <p style='color: #333; font-size: 16px;'>{(isArabic ? $"مرحباً {userName}،" : $"Hello {userName},")}</p>
                            <p style='color: #555; font-size: 14px; line-height: 1.8;'>
                                {(isArabic 
                                    ? $"قام <strong>{inviterName}</strong> بدعوتك للانضمام إلى <strong>{organizationName}</strong> في نظام شاهين للحوكمة والمخاطر والامتثال."
                                    : $"<strong>{inviterName}</strong> has invited you to join <strong>{organizationName}</strong> on Shahin AI GRC System.")}
                            </p>
                            <table style='width: 100%; margin: 30px 0;'>
                                <tr>
                                    <td style='text-align: center;'>
                                        <a href='{inviteLink}' style='background: linear-gradient(135deg, #fd7e14, #e55300); color: white; text-decoration: none; padding: 15px 40px; border-radius: 50px; font-weight: bold;'>
                                            {(isArabic ? "قبول الدعوة" : "Accept Invitation")}
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #888; font-size: 12px; margin: 0;'>Powered by <a href='https://www.doganconsult.com'>Dogan Consult</a></p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        private string GeneratePasswordChangedHtml(string userName, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            
            return $@"
            <!DOCTYPE html>
            <html dir='{dir}'>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 40px 0;'>
                <table style='width: 600px; margin: 0 auto; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background: linear-gradient(135deg, #28a745, #20c997); padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h1 style='color: white; margin: 0;'>🔑 {(isArabic ? "تم تغيير كلمة المرور" : "Password Changed")}</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: {align};'>
                            <p style='color: #333; font-size: 16px;'>{(isArabic ? $"مرحباً {userName}،" : $"Hello {userName},")}</p>
                            <p style='color: #555; font-size: 14px; line-height: 1.8;'>
                                {(isArabic 
                                    ? "تم تغيير كلمة المرور الخاصة بحسابك بنجاح. إذا لم تقم بهذا التغيير، يرجى التواصل معنا فوراً."
                                    : "Your account password has been successfully changed. If you didn't make this change, please contact us immediately.")}
                            </p>
                            <p style='color: #888; font-size: 12px; margin-top: 20px;'>
                                📅 {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #888; font-size: 12px; margin: 0;'>Powered by <a href='https://www.doganconsult.com'>Dogan Consult</a></p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        private string GenerateAccountLockedHtml(string userName, string unlockTime, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            
            return $@"
            <!DOCTYPE html>
            <html dir='{dir}'>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 40px 0;'>
                <table style='width: 600px; margin: 0 auto; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background: linear-gradient(135deg, #dc3545, #c82333); padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h1 style='color: white; margin: 0;'>⚠️ {(isArabic ? "تم قفل الحساب" : "Account Locked")}</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: {align};'>
                            <p style='color: #333; font-size: 16px;'>{(isArabic ? $"مرحباً {userName}،" : $"Hello {userName},")}</p>
                            <p style='color: #555; font-size: 14px; line-height: 1.8;'>
                                {(isArabic 
                                    ? "تم قفل حسابك مؤقتاً بسبب محاولات تسجيل دخول فاشلة متعددة."
                                    : "Your account has been temporarily locked due to multiple failed login attempts.")}
                            </p>
                            <p style='color: #dc3545; font-size: 14px; font-weight: bold;'>
                                {(isArabic ? $"سيتم فتح القفل في: {unlockTime}" : $"Account will be unlocked at: {unlockTime}")}
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #888; font-size: 12px; margin: 0;'>Powered by <a href='https://www.doganconsult.com'>Dogan Consult</a></p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        private string GenerateNewLoginAlertHtml(string userName, string ipAddress, string location, string deviceInfo, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            
            return $@"
            <!DOCTYPE html>
            <html dir='{dir}'>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 40px 0;'>
                <table style='width: 600px; margin: 0 auto; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background: linear-gradient(135deg, #6f42c1, #5a32a3); padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h1 style='color: white; margin: 0;'>🔔 {(isArabic ? "تسجيل دخول جديد" : "New Login Detected")}</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; text-align: {align};'>
                            <p style='color: #333; font-size: 16px;'>{(isArabic ? $"مرحباً {userName}،" : $"Hello {userName},")}</p>
                            <p style='color: #555; font-size: 14px; line-height: 1.8;'>
                                {(isArabic 
                                    ? "تم تسجيل دخول جديد إلى حسابك:"
                                    : "A new login was detected on your account:")}
                            </p>
                            <div style='background: #f8f9fa; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                                <p style='margin: 5px 0; font-size: 13px;'><strong>{(isArabic ? "عنوان IP:" : "IP Address:")}</strong> {ipAddress}</p>
                                <p style='margin: 5px 0; font-size: 13px;'><strong>{(isArabic ? "الموقع:" : "Location:")}</strong> {location}</p>
                                <p style='margin: 5px 0; font-size: 13px;'><strong>{(isArabic ? "الجهاز:" : "Device:")}</strong> {deviceInfo}</p>
                                <p style='margin: 5px 0; font-size: 13px;'><strong>{(isArabic ? "الوقت:" : "Time:")}</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                            </div>
                            <p style='color: #888; font-size: 12px;'>
                                {(isArabic 
                                    ? "إذا لم تكن أنت، قم بتغيير كلمة المرور فوراً."
                                    : "If this wasn't you, change your password immediately.")}
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #888; font-size: 12px; margin: 0;'>Powered by <a href='https://www.doganconsult.com'>Dogan Consult</a></p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        // Trial email methods
        public async Task SendTrialActivationEmailAsync(string toEmail, string userName, string activationToken, bool isArabic = true)
        {
            try
            {
                var activationLink = $"https://portal.shahin-ai.com/auth/activate?token={activationToken}";
                var subject = isArabic ? "تفعيل حسابك التجريبي - شاهين AI" : "Activate Your Trial Account - Shahin AI";
                var body = await RenderTemplateAsync("TenantActivation", new { Name = userName, ActivationLink = activationLink });
                await _emailService.SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation("Trial activation email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send trial activation email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendTrialNurtureEmailAsync(string toEmail, string templateName, string companyName, int daysRemaining, bool isArabic = true)
        {
            try
            {
                var subject = templateName switch
                {
                    "TrialWelcome" => isArabic ? "مرحباً بك في شاهين AI" : "Welcome to Shahin AI",
                    "TrialNudge24h" => isArabic ? "هل تحتاج مساعدة؟" : "Need help getting started?",
                    "TrialValuePush" => isArabic ? "اكتشف المزيد من المميزات" : "Discover more features",
                    "TrialMidpoint" => isArabic ? $"متبقي {daysRemaining} أيام في تجربتك" : $"{daysRemaining} days left in your trial",
                    "TrialEscalation" => isArabic ? "هل نحتفظ بتجربتك؟" : "Should we keep your trial active?",
                    "TrialExpired" => isArabic ? "انتهت تجربتك" : "Your trial has ended",
                    "TrialWinback" => isArabic ? "لقد أجرينا تحسينات - عد وشاهد" : "We've made improvements - come back",
                    _ => isArabic ? "شاهين AI GRC" : "Shahin AI GRC"
                };

                var model = new { Name = companyName, DaysRemaining = daysRemaining, AccessLink = "https://portal.shahin-ai.com", UpgradeLink = "https://portal.shahin-ai.com/pricing" };
                var body = await RenderTemplateAsync(templateName, model);
                await _emailService.SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation("Trial nurture email {Template} sent to {Email}", templateName, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send trial nurture email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendTemplatedEmailAsync(string toEmail, string templateName, object model, bool isArabic = true)
        {
            try
            {
                var body = await RenderTemplateAsync(templateName, model);
                var subject = isArabic ? "شاهين AI GRC" : "Shahin AI GRC";
                await _emailService.SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation("Templated email {Template} sent to {Email}", templateName, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send templated email to {Email}", toEmail);
                throw;
            }
        }
    }
}
