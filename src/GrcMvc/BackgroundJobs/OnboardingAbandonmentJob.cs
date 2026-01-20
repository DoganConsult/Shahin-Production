using GrcMvc.Abp;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for detecting and recovering abandoned onboarding processes.
    /// Runs daily to identify incomplete onboarding wizards and send recovery emails.
    /// </summary>
    public class OnboardingAbandonmentJob
    {
        private readonly GrcDbContext _context;
        private readonly IGrcEmailService _grcEmailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingAbandonmentJob> _logger;

        public OnboardingAbandonmentJob(
            GrcDbContext context,
            IGrcEmailService grcEmailService,
            IConfiguration configuration,
            ILogger<OnboardingAbandonmentJob> logger)
        {
            _context = context;
            _grcEmailService = grcEmailService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Main job execution - detects abandoned onboarding and sends recovery emails.
        /// Called by Hangfire on schedule (daily).
        /// </summary>
        [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting onboarding abandonment detection job at {Time}", DateTime.UtcNow);

            try
            {
                var stats = new AbandonmentStats();

                // Find incomplete onboarding wizards
                var incompleteWizards = await _context.OnboardingWizards
                    .Where(w => w.WizardStatus != "Completed" && w.WizardStatus != "Cancelled")
                    .Where(w => !w.IsDeleted)
                    .Include(w => w.Tenant)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} incomplete onboarding wizards", incompleteWizards.Count);

                foreach (var wizard in incompleteWizards)
                {
                    try
                    {
                        var daysIncomplete = CalculateDaysIncomplete(wizard);
                        var daysSinceLastActivity = CalculateDaysSinceLastActivity(wizard);

                        // Skip if too recent (less than 2 days)
                        if (daysIncomplete < 2)
                        {
                            stats.SkippedRecent++;
                            continue;
                        }

                        // Determine email type based on days incomplete
                        if (daysIncomplete >= 7 && daysSinceLastActivity >= 7)
                        {
                            // Abandonment recovery email (7+ days)
                            await SendAbandonmentRecoveryEmailAsync(wizard, daysIncomplete);
                            stats.AbandonmentEmailsSent++;
                        }
                        else if (daysSinceLastActivity >= 3)
                        {
                            // Progress reminder email (3+ days since last activity)
                            var currentStep = wizard.CurrentStep; // Already an int (1-12)
                            var totalSteps = 12; // 12-step wizard
                            await SendProgressReminderEmailAsync(wizard, currentStep, totalSteps, daysSinceLastActivity);
                            stats.ReminderEmailsSent++;
                        }

                        stats.Processed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing wizard {WizardId} for tenant {TenantId}", 
                            wizard.Id, wizard.TenantId);
                        stats.Errors++;
                    }
                }

                _logger.LogInformation(
                    "Abandonment job completed: Processed={Processed}, AbandonmentEmails={Abandonment}, ReminderEmails={Reminder}, Skipped={Skipped}, Errors={Errors}",
                    stats.Processed, stats.AbandonmentEmailsSent, stats.ReminderEmailsSent, stats.SkippedRecent, stats.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in onboarding abandonment detection job");
                throw; // Rethrow to trigger Hangfire retry
            }
        }

        /// <summary>
        /// Calculate days since onboarding started but not completed
        /// </summary>
        private int CalculateDaysIncomplete(Models.Entities.OnboardingWizard wizard)
        {
            var startDate = wizard.CreatedDate;
            return (int)(DateTime.UtcNow - startDate).TotalDays;
        }

        /// <summary>
        /// Calculate days since last activity (last step saved)
        /// </summary>
        private int CalculateDaysSinceLastActivity(Models.Entities.OnboardingWizard wizard)
        {
            var lastActivity = wizard.LastStepSavedAt ?? wizard.CreatedDate;
            return (int)(DateTime.UtcNow - lastActivity).TotalDays;
        }

        /// <summary>
        /// Get current step number from step name
        /// </summary>
        private int GetCurrentStepNumber(string? stepName)
        {
            if (string.IsNullOrWhiteSpace(stepName))
                return 1;

            // Extract step number from step name (e.g., "SectionA" -> 1, "SectionB" -> 2)
            var stepMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "SectionA", 1 }, { "SectionB", 2 }, { "SectionC", 3 }, { "SectionD", 4 },
                { "SectionE", 5 }, { "SectionF", 6 }, { "SectionG", 7 }, { "SectionH", 8 },
                { "SectionI", 9 }, { "SectionJ", 10 }, { "SectionK", 11 }, { "SectionL", 12 }
            };

            return stepMap.TryGetValue(stepName, out var step) ? step : 1;
        }

        /// <summary>
        /// Send abandonment recovery email
        /// </summary>
        private async Task SendAbandonmentRecoveryEmailAsync(Models.Entities.OnboardingWizard wizard, int daysIncomplete)
        {
            try
            {
                var tenant = wizard.Tenant;
                if (tenant == null || tenant.IsDeleted)
                {
                    _logger.LogWarning("Tenant {TenantId} not found or deleted for wizard {WizardId}", 
                        wizard.TenantId, wizard.Id);
                    return;
                }

                var adminEmail = tenant.AdminEmail;
                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    _logger.LogWarning("No admin email found for tenant {TenantId}", wizard.TenantId);
                    return;
                }

                var firstName = "Administrator"; // TODO: Get from user profile
                var organizationName = wizard.OrganizationLegalNameEn ?? 
                                     wizard.OrganizationLegalNameAr ?? 
                                     tenant.OrganizationName ?? 
                                     "Your Organization";

                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var resumeLink = $"{baseUrl}/OnboardingWizard/Index?tenantId={wizard.TenantId}";

                await _grcEmailService.SendOnboardingAbandonmentRecoveryEmailAsync(
                    toEmail: adminEmail,
                    firstName: firstName,
                    organizationName: organizationName,
                    resumeLink: resumeLink,
                    daysIncomplete: daysIncomplete,
                    isArabic: false // TODO: Detect from tenant preferences
                );

                _logger.LogInformation(
                    "Abandonment recovery email sent to {Email} for tenant {TenantId} (incomplete for {Days} days)",
                    adminEmail, wizard.TenantId, daysIncomplete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send abandonment recovery email for wizard {WizardId}", wizard.Id);
                throw;
            }
        }

        /// <summary>
        /// Send progress reminder email
        /// </summary>
        private async Task SendProgressReminderEmailAsync(
            Models.Entities.OnboardingWizard wizard, 
            int currentStep, 
            int totalSteps, 
            int daysSinceLastActivity)
        {
            try
            {
                var tenant = wizard.Tenant;
                if (tenant == null || tenant.IsDeleted)
                {
                    _logger.LogWarning("Tenant {TenantId} not found or deleted for wizard {WizardId}", 
                        wizard.TenantId, wizard.Id);
                    return;
                }

                var adminEmail = tenant.AdminEmail;
                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    _logger.LogWarning("No admin email found for tenant {TenantId}", wizard.TenantId);
                    return;
                }

                var firstName = "Administrator"; // TODO: Get from user profile
                var organizationName = wizard.OrganizationLegalNameEn ?? 
                                     wizard.OrganizationLegalNameAr ?? 
                                     tenant.OrganizationName ?? 
                                     "Your Organization";

                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var resumeLink = $"{baseUrl}/OnboardingWizard/Index?tenantId={wizard.TenantId}";

                await _grcEmailService.SendOnboardingProgressReminderEmailAsync(
                    toEmail: adminEmail,
                    firstName: firstName,
                    organizationName: organizationName,
                    resumeLink: resumeLink,
                    currentStep: currentStep,
                    totalSteps: totalSteps,
                    daysSinceLastActivity: daysSinceLastActivity,
                    isArabic: false // TODO: Detect from tenant preferences
                );

                _logger.LogInformation(
                    "Progress reminder email sent to {Email} for tenant {TenantId} (step {Step}/{Total})",
                    adminEmail, wizard.TenantId, currentStep, totalSteps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send progress reminder email for wizard {WizardId}", wizard.Id);
                throw;
            }
        }
    }

    /// <summary>
    /// Statistics for abandonment detection monitoring
    /// </summary>
    public class AbandonmentStats
    {
        public int Processed { get; set; }
        public int AbandonmentEmailsSent { get; set; }
        public int ReminderEmailsSent { get; set; }
        public int SkippedRecent { get; set; }
        public int Errors { get; set; }
    }
}
