using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public GrcEmailService(
            IEmailService emailService,
            ILogger<GrcEmailService> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
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

        /// <summary>
        /// Enterprise email template wrapper with professional branding
        /// </summary>
        private string GetEmailTemplateWrapper(string content, string dir, bool isArabic)
        {
            var currentYear = DateTime.UtcNow.Year;
            var companyName = isArabic ? "دوجان للاستشارات" : "Dogan Consult";
            var platformName = isArabic ? "شاهين AI" : "Shahin AI";
            var supportEmail = "support@shahin-ai.com";
            var website = _configuration["App:LandingUrl"] ?? 
                         _configuration["AppInfo:Website"] ?? 
                         "https://www.shahin-ai.com";
            
            return $@"
<!DOCTYPE html>
<html lang='{(isArabic ? "ar" : "en")}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>{platformName} GRC Platform</title>
    <!--[if mso]>
    <style type='text/css'>
        body, table, td {{font-family: Arial, sans-serif !important;}}
    </style>
    <![endif]-->
</head>
<body style='margin: 0; padding: 0; background-color: #f5f7fa; font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, ''Helvetica Neue'', Arial, sans-serif;'>
    <!-- Email Wrapper -->
    <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #f5f7fa; padding: 40px 20px;'>
        <tr>
            <td align='center'>
                <!-- Main Container -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08); overflow: hidden;'>
                    
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); padding: 32px 40px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center' style='padding-bottom: 16px;'>
                                        <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;'>{platformName}</h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center'>
                                        <p style='margin: 0; color: #e0e7ff; font-size: 14px; font-weight: 400; text-transform: uppercase; letter-spacing: 1px;'>{(isArabic ? "منصة الحوكمة والمخاطر والامتثال" : "Governance, Risk & Compliance Platform")}</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            {content}
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 32px 40px;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center' style='padding-bottom: 16px;'>
                                        <p style='margin: 0; color: #64748b; font-size: 12px; line-height: 1.6;'>
                                            {(isArabic 
                                                ? $"© {currentYear} {companyName}. جميع الحقوق محفوظة."
                                                : $"© {currentYear} {companyName}. All rights reserved.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 12px;'>
                                        <p style='margin: 0; color: #64748b; font-size: 12px;'>
                                            {(isArabic ? "للدعم الفني:" : "Support:")} <a href='mailto:{supportEmail}' style='color: #2563eb; text-decoration: none;'>{supportEmail}</a>
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center'>
                                        <p style='margin: 0; color: #94a3b8; font-size: 11px; line-height: 1.5;'>
                                            {(isArabic 
                                                ? "هذا البريد الإلكتروني تم إرساله تلقائياً من نظام شاهين AI. يرجى عدم الرد على هذا البريد."
                                                : "This email was automatically sent from the Shahin AI system. Please do not reply to this email.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-top: 20px; border-top: 1px solid #e2e8f0; margin-top: 20px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='padding: 0 8px;'>
                                                    <a href='{website}' style='color: #64748b; text-decoration: none; font-size: 11px;'>{website}</a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
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

        // Onboarding email methods
        public async Task SendOnboardingActivationEmailAsync(string toEmail, string organizationName, string activationLink, string tenantSlug, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateOnboardingActivationHtml(organizationName, activationLink, tenantSlug, isArabic);
                var subject = isArabic 
                    ? $"🎉 تفعيل حسابك - {organizationName}" 
                    : $"🎉 Activate Your Account - {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Onboarding activation email sent to {Email} for {Organization}", toEmail, organizationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send onboarding activation email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendTeamInvitationEmailAsync(string toEmail, string firstName, string inviterName, string organizationName, string invitationLink, string roleName, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateTeamInvitationHtml(firstName, inviterName, organizationName, invitationLink, roleName, isArabic);
                var subject = isArabic 
                    ? $"📨 دعوة للانضمام إلى {organizationName}" 
                    : $"📨 Invitation to join {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Team invitation email sent to {Email} for role {Role}", toEmail, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send team invitation email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendOnboardingAbandonmentRecoveryEmailAsync(string toEmail, string firstName, string organizationName, string resumeLink, int daysIncomplete, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateAbandonmentRecoveryHtml(firstName, organizationName, resumeLink, daysIncomplete, isArabic);
                var subject = isArabic 
                    ? $"⏰ أكمل إعداد حسابك - {organizationName}" 
                    : $"⏰ Complete Your Setup - {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Abandonment recovery email sent to {Email} (incomplete for {Days} days)", toEmail, daysIncomplete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send abandonment recovery email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendOnboardingProgressReminderEmailAsync(string toEmail, string firstName, string organizationName, string resumeLink, int currentStep, int totalSteps, int daysSinceLastActivity, bool isArabic = true)
        {
            try
            {
                var progressPercent = (int)((currentStep / (double)totalSteps) * 100);
                var htmlContent = GenerateProgressReminderHtml(firstName, organizationName, resumeLink, currentStep, totalSteps, progressPercent, daysSinceLastActivity, isArabic);
                var subject = isArabic 
                    ? $"📊 تذكير: أكمل إعدادك - {organizationName}" 
                    : $"📊 Reminder: Complete Your Setup - {organizationName}";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Progress reminder email sent to {Email} (step {Step}/{Total})", toEmail, currentStep, totalSteps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send progress reminder email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendOnboardingWelcomeEmailAsync(string toEmail, string firstName, string organizationName, string dashboardLink, bool isArabic = true)
        {
            try
            {
                var htmlContent = GenerateOnboardingWelcomeHtml(firstName, organizationName, dashboardLink, isArabic);
                var subject = isArabic 
                    ? $"🎉 مرحباً بك في {organizationName} - شاهين AI" 
                    : $"🎉 Welcome to {organizationName} - Shahin AI";

                await _emailService.SendEmailAsync(toEmail, subject, htmlContent);
                _logger.LogInformation("Onboarding welcome email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send onboarding welcome email to {Email}", toEmail);
                throw;
            }
        }

        // HTML generation helpers for onboarding emails - Enterprise/Government Professional Level
        private string GenerateOnboardingActivationHtml(string organizationName, string activationLink, string tenantSlug, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var align = isArabic ? "right" : "left";
            var textAlign = isArabic ? "right" : "left";
            
            var content = $@"
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <h2 style='margin: 0; color: #1e293b; font-size: 24px; font-weight: 700; line-height: 1.3;'>
                                            {(isArabic ? "تفعيل حسابك المؤسسي" : "Activate Your Enterprise Account")}
                                        </h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #475569; font-size: 16px; line-height: 1.6; font-weight: 400;'>
                                            {(isArabic 
                                                ? $"السيد/السيدة المحترم/ة،"
                                                : "Dear Valued Client,")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? $"نود إعلامكم بأن حساب منظمة <strong style='color: #1e293b;'>{organizationName}</strong> قد تم إنشاؤه بنجاح في منصة شاهين AI للحوكمة والمخاطر والامتثال."
                                                : $"We are pleased to inform you that the account for <strong style='color: #1e293b;'>{organizationName}</strong> has been successfully created on the Shahin AI Governance, Risk & Compliance Platform.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? "لإكمال عملية التسجيل والبدء في استخدام النظام، يرجى تفعيل حسابك من خلال الرابط أدناه:"
                                                : "To complete your registration and begin using the system, please activate your account using the link below:")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); border-radius: 6px;'>
                                                    <a href='{activationLink}' style='display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;'>
                                                        {(isArabic ? "تفعيل الحساب الآن" : "Activate Account Now")}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f1f5f9; border-left: 4px solid #2563eb; padding: 16px 20px; border-radius: 4px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0 0 8px 0; color: #1e293b; font-size: 13px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>
                                                        {(isArabic ? "معلومات مهمة:" : "Important Information:")}
                                                    </p>
                                                    <ul style='margin: 0; padding-{align}: 20px; color: #475569; font-size: 13px; line-height: 1.8;'>
                                                        <li style='margin-bottom: 6px;'>
                                                            {(isArabic 
                                                                ? "صحة الرابط: 48 ساعة من وقت الإرسال"
                                                                : "Link Validity: 48 hours from time of sending")}
                                                        </li>
                                                        <li style='margin-bottom: 6px;'>
                                                            {(isArabic 
                                                                ? "الأمان: الرابط آمن ومشفر"
                                                                : "Security: Link is secure and encrypted")}
                                                        </li>
                                                        <li>
                                                            {(isArabic 
                                                                ? "الدعم: للاستفسارات، يرجى التواصل مع فريق الدعم"
                                                                : "Support: For inquiries, please contact our support team")}
                                                        </li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-top: 24px; border-top: 1px solid #e2e8f0;'>
                                        <p style='margin: 0 0 8px 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic 
                                                ? "نشكركم لاختياركم منصة شاهين AI. نتطلع إلى خدمتكم."
                                                : "Thank you for choosing Shahin AI Platform. We look forward to serving you.")}
                                        </p>
                                        <p style='margin: 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic ? "مع أطيب التحيات،" : "Best regards,")}<br>
                                            <strong style='color: #1e293b;'>{(isArabic ? "فريق شاهين AI" : "Shahin AI Team")}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>";
            
            return GetEmailTemplateWrapper(content, dir, isArabic);
        }

        private string GenerateTeamInvitationHtml(string firstName, string inviterName, string organizationName, string invitationLink, string roleName, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var textAlign = isArabic ? "right" : "left";
            
            var content = $@"
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <h2 style='margin: 0; color: #1e293b; font-size: 24px; font-weight: 700; line-height: 1.3;'>
                                            {(isArabic ? "دعوة للانضمام إلى الفريق" : "Team Collaboration Invitation")}
                                        </h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #475569; font-size: 16px; line-height: 1.6; font-weight: 400;'>
                                            {(isArabic ? $"السيد/السيدة {firstName} المحترم/ة،" : $"Dear {firstName},")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? $"يسرنا إبلاغكم بأن <strong style='color: #1e293b;'>{inviterName}</strong> قد قام بدعوتكم للانضمام إلى فريق العمل في منظمة <strong style='color: #1e293b;'>{organizationName}</strong> على منصة شاهين AI للحوكمة والمخاطر والامتثال."
                                                : $"We are pleased to inform you that <strong style='color: #1e293b;'>{inviterName}</strong> has invited you to join the team at <strong style='color: #1e293b;'>{organizationName}</strong> on the Shahin AI Governance, Risk & Compliance Platform.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 20px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign}; padding-bottom: 12px;'>
                                                    <p style='margin: 0; color: #64748b; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>
                                                        {(isArabic ? "تفاصيل الدعوة:" : "Invitation Details:")}
                                                    </p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='text-align: {textAlign}; padding-bottom: 8px;'>
                                                    <p style='margin: 0; color: #1e293b; font-size: 14px;'>
                                                        <strong>{(isArabic ? "الدور المخصص:" : "Assigned Role:")}</strong> <span style='color: #334155;'>{roleName}</span>
                                                    </p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0; color: #1e293b; font-size: 14px;'>
                                                        <strong>{(isArabic ? "المنظمة:" : "Organization:")}</strong> <span style='color: #334155;'>{organizationName}</span>
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? "للقبول والبدء في استخدام النظام، يرجى النقر على الزر أدناه:"
                                                : "To accept and begin using the system, please click the button below:")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); border-radius: 6px;'>
                                                    <a href='{invitationLink}' style='display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;'>
                                                        {(isArabic ? "قبول الدعوة والبدء" : "Accept & Get Started")}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px 20px; border-radius: 4px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0; color: #92400e; font-size: 13px; line-height: 1.6;'>
                                                        <strong>{(isArabic ? "ملاحظة:" : "Note:")}</strong> {(isArabic 
                                                            ? "هذه الدعوة صالحة لمدة 7 أيام من تاريخ الإرسال. بعد انتهاء الصلاحية، ستحتاج إلى طلب دعوة جديدة."
                                                            : "This invitation is valid for 7 days from the date of sending. After expiration, you will need to request a new invitation.")}
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-top: 24px; border-top: 1px solid #e2e8f0;'>
                                        <p style='margin: 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic ? "مع أطيب التحيات،" : "Best regards,")}<br>
                                            <strong style='color: #1e293b;'>{(isArabic ? "فريق شاهين AI" : "Shahin AI Team")}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>";
            
            return GetEmailTemplateWrapper(content, dir, isArabic);
        }

        private string GenerateAbandonmentRecoveryHtml(string firstName, string organizationName, string resumeLink, int daysIncomplete, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var textAlign = isArabic ? "right" : "left";
            
            var content = $@"
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <h2 style='margin: 0; color: #1e293b; font-size: 24px; font-weight: 700; line-height: 1.3;'>
                                            {(isArabic ? "استكمال إعداد حسابك المؤسسي" : "Complete Your Enterprise Account Setup")}
                                        </h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #475569; font-size: 16px; line-height: 1.6; font-weight: 400;'>
                                            {(isArabic ? $"السيد/السيدة {firstName} المحترم/ة،" : $"Dear {firstName},")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? $"نود تذكيركم بأن عملية إعداد حساب منظمة <strong style='color: #1e293b;'>{organizationName}</strong> على منصة شاهين AI قد بدأت منذ {daysIncomplete} {(daysIncomplete == 1 ? "يوم" : "أيام")} ولم تكتمل بعد."
                                                : $"We would like to remind you that the setup process for <strong style='color: #1e293b;'>{organizationName}</strong> on the Shahin AI Platform was started {daysIncomplete} {(daysIncomplete == 1 ? "day" : "days")} ago and has not yet been completed.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 20px; border-radius: 4px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0 0 12px 0; color: #92400e; font-size: 15px; font-weight: 600;'>
                                                        {(isArabic ? "لماذا من المهم إكمال الإعداد؟" : "Why is it important to complete setup?")}
                                                    </p>
                                                    <ul style='margin: 0; padding-{textAlign}: 24px; color: #78350f; font-size: 14px; line-height: 1.8;'>
                                                        <li style='margin-bottom: 8px;'>
                                                            {(isArabic 
                                                                ? "الوصول الكامل إلى جميع ميزات المنصة"
                                                                : "Full access to all platform features")}
                                                        </li>
                                                        <li style='margin-bottom: 8px;'>
                                                            {(isArabic 
                                                                ? "تكوين إعدادات الحوكمة والمخاطر والامتثال"
                                                                : "Configure governance, risk, and compliance settings")}
                                                        </li>
                                                        <li>
                                                            {(isArabic 
                                                                ? "بدء استخدام النظام بشكل فعال"
                                                                : "Begin using the system effectively")}
                                                        </li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? "يمكنكم متابعة عملية الإعداد من حيث توقفتم. جميع البيانات التي أدخلتموها محفوظة بشكل آمن."
                                                : "You can continue the setup process from where you left off. All data you entered has been securely saved.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); border-radius: 6px;'>
                                                    <a href='{resumeLink}' style='display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;'>
                                                        {(isArabic ? "استكمل الإعداد الآن" : "Resume Setup Now")}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-top: 24px; border-top: 1px solid #e2e8f0;'>
                                        <p style='margin: 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic ? "مع أطيب التحيات،" : "Best regards,")}<br>
                                            <strong style='color: #1e293b;'>{(isArabic ? "فريق شاهين AI" : "Shahin AI Team")}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>";
            
            return GetEmailTemplateWrapper(content, dir, isArabic);
        }

        private string GenerateProgressReminderHtml(string firstName, string organizationName, string resumeLink, int currentStep, int totalSteps, int progressPercent, int daysSinceLastActivity, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var textAlign = isArabic ? "right" : "left";
            var remainingSteps = totalSteps - currentStep;
            
            var content = $@"
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <h2 style='margin: 0; color: #1e293b; font-size: 24px; font-weight: 700; line-height: 1.3;'>
                                            {(isArabic ? "تذكير: استكمال إعداد حسابك" : "Reminder: Complete Your Account Setup")}
                                        </h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #475569; font-size: 16px; line-height: 1.6; font-weight: 400;'>
                                            {(isArabic ? $"السيد/السيدة {firstName} المحترم/ة،" : $"Dear {firstName},")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? $"أنت على بعد {remainingSteps} {(remainingSteps == 1 ? "خطوة واحدة" : "خطوات")} فقط من إكمال إعداد حساب منظمة <strong style='color: #1e293b;'>{organizationName}</strong> على منصة شاهين AI."
                                                : $"You are just {remainingSteps} {(remainingSteps == 1 ? "step" : "steps")} away from completing the setup for <strong style='color: #1e293b;'>{organizationName}</strong> on the Shahin AI Platform.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 24px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign}; padding-bottom: 16px;'>
                                                    <p style='margin: 0; color: #64748b; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>
                                                        {(isArabic ? "حالة التقدم الحالية:" : "Current Progress Status:")}
                                                    </p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding-bottom: 12px;'>
                                                    <p style='margin: 0; color: #1e293b; font-size: 14px; font-weight: 500;'>
                                                        {(isArabic ? "الخطوات المكتملة:" : "Steps Completed:")} <strong style='color: #2563eb;'>{currentStep} / {totalSteps}</strong>
                                                    </p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding-bottom: 8px;'>
                                                    <div style='background-color: #e2e8f0; height: 8px; border-radius: 4px; overflow: hidden;'>
                                                        <div style='background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); height: 100%; width: {progressPercent}%; transition: width 0.3s ease;'></div>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0; color: #64748b; font-size: 13px;'>
                                                        <strong style='color: #6366f1;'>{progressPercent}%</strong> {(isArabic ? "مكتمل" : "Complete")}
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #64748b; font-size: 13px;'>
                                            {(isArabic 
                                                ? $"آخر نشاط: منذ {daysSinceLastActivity} {(daysSinceLastActivity == 1 ? "يوم" : "أيام")}"
                                                : $"Last Activity: {daysSinceLastActivity} {(daysSinceLastActivity == 1 ? "day" : "days")} ago")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); border-radius: 6px;'>
                                                    <a href='{resumeLink}' style='display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;'>
                                                        {(isArabic ? "استكمل الإعداد الآن" : "Continue Setup Now")}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-top: 24px; border-top: 1px solid #e2e8f0;'>
                                        <p style='margin: 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic ? "مع أطيب التحيات،" : "Best regards,")}<br>
                                            <strong style='color: #1e293b;'>{(isArabic ? "فريق شاهين AI" : "Shahin AI Team")}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>";
            
            return GetEmailTemplateWrapper(content, dir, isArabic);
        }

        private string GenerateOnboardingWelcomeHtml(string firstName, string organizationName, string dashboardLink, bool isArabic)
        {
            var dir = isArabic ? "rtl" : "ltr";
            var textAlign = isArabic ? "right" : "left";
            
            var content = $@"
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <div style='width: 80px; height: 80px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); border-radius: 50%; display: inline-flex; align-items: center; justify-content: center;'>
                                            <span style='color: #ffffff; font-size: 40px;'>✓</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <h2 style='margin: 0; color: #1e293b; font-size: 28px; font-weight: 700; line-height: 1.3;'>
                                            {(isArabic ? "تهانينا! تم إكمال الإعداد بنجاح" : "Congratulations! Setup Completed Successfully")}
                                        </h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 20px;'>
                                        <p style='margin: 0; color: #475569; font-size: 16px; line-height: 1.6; font-weight: 400;'>
                                            {(isArabic ? $"السيد/السيدة {firstName} المحترم/ة،" : $"Dear {firstName},")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-bottom: 24px;'>
                                        <p style='margin: 0; color: #334155; font-size: 15px; line-height: 1.7;'>
                                            {(isArabic 
                                                ? $"يسرنا إبلاغكم بأن عملية إعداد حساب منظمة <strong style='color: #1e293b;'>{organizationName}</strong> على منصة شاهين AI للحوكمة والمخاطر والامتثال قد اكتملت بنجاح."
                                                : $"We are pleased to inform you that the setup process for <strong style='color: #1e293b;'>{organizationName}</strong> on the Shahin AI Governance, Risk & Compliance Platform has been completed successfully.")}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f0fdf4; border-left: 4px solid #10b981; padding: 20px; border-radius: 4px; margin-bottom: 24px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='text-align: {textAlign};'>
                                                    <p style='margin: 0 0 12px 0; color: #166534; font-size: 15px; font-weight: 600;'>
                                                        {(isArabic ? "أنت الآن جاهز للبدء:" : "You are now ready to:")}
                                                    </p>
                                                    <ul style='margin: 0; padding-{textAlign}: 24px; color: #15803d; font-size: 14px; line-height: 1.8;'>
                                                        <li style='margin-bottom: 8px;'>
                                                            {(isArabic 
                                                                ? "الوصول إلى لوحة التحكم الرئيسية"
                                                                : "Access the main dashboard")}
                                                        </li>
                                                        <li style='margin-bottom: 8px;'>
                                                            {(isArabic 
                                                                ? "بدء إدارة الحوكمة والمخاطر والامتثال"
                                                                : "Begin managing governance, risk, and compliance")}
                                                        </li>
                                                        <li>
                                                            {(isArabic 
                                                                ? "استخدام جميع ميزات المنصة المتقدمة"
                                                                : "Utilize all advanced platform features")}
                                                        </li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding-bottom: 32px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); border-radius: 6px;'>
                                                    <a href='{dashboardLink}' style='display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;'>
                                                        {(isArabic ? "الذهاب إلى لوحة التحكم" : "Go to Dashboard")}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: {textAlign}; padding-top: 24px; border-top: 1px solid #e2e8f0;'>
                                        <p style='margin: 0 0 8px 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic 
                                                ? "نشكركم لثقتكم بمنصة شاهين AI. نحن هنا لدعمكم في رحلتكم نحو التميز في الحوكمة والمخاطر والامتثال."
                                                : "Thank you for your trust in the Shahin AI Platform. We are here to support you on your journey toward excellence in governance, risk, and compliance.")}
                                        </p>
                                        <p style='margin: 0; color: #64748b; font-size: 14px; line-height: 1.6;'>
                                            {(isArabic ? "مع أطيب التحيات،" : "Best regards,")}<br>
                                            <strong style='color: #1e293b;'>{(isArabic ? "فريق شاهين AI" : "Shahin AI Team")}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>";
            
            return GetEmailTemplateWrapper(content, dir, isArabic);
        }
    }
}
