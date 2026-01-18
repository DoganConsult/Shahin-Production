using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Settings;
using Volo.Abp.SettingManagement;
using Volo.Abp.UI.Navigation;
using System.ComponentModel.DataAnnotations;
using GrcMvc.Settings;

namespace GrcMvc.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class SettingsController : Controller
    {
        private readonly ISettingManager _settingManager;
        private readonly IConfiguration _configuration;

        public SettingsController(ISettingManager settingManager, IConfiguration configuration)
        {
            _settingManager = settingManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SettingsViewModel
            {
                ConnectionStrings = await GetConnectionStringsAsync(),
                SecuritySettings = await GetSecuritySettingsAsync(),
                AppSettings = await GetAppSettingsAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConnectionStrings(SettingsViewModel model)
        {
            try
            {
                // Update connection strings in configuration
                await UpdateConnectionString(AppSettings.DefaultConnection, model.ConnectionStrings.DefaultConnection);
                await UpdateConnectionString(AppSettings.GrcAuthDb, model.ConnectionStrings.GrcAuthDb);
                await UpdateConnectionString(AppSettings.HangfireConnection, model.ConnectionStrings.HangfireConnection);
                await UpdateConnectionString(AppSettings.Redis, model.ConnectionStrings.Redis);

                TempData["Success"] = "Connection strings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update connection strings: {ex.Message}";
                return await Index();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSecuritySettings(SettingsViewModel model)
        {
            try
            {
                await _settingManager.SetGlobalAsync(AppSettings.AllowPublicRegistration, model.SecuritySettings.AllowPublicRegistration.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.RequireEmailConfirmation, model.SecuritySettings.RequireEmailConfirmation.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.PasswordMinLength, model.SecuritySettings.PasswordMinLength.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.RequireUppercase, model.SecuritySettings.RequireUppercase.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.RequireLowercase, model.SecuritySettings.RequireLowercase.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.RequireDigit, model.SecuritySettings.RequireDigit.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.RequireSpecialChar, model.SecuritySettings.RequireSpecialChar.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.MaxLoginAttempts, model.SecuritySettings.MaxLoginAttempts.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.LockoutDuration, model.SecuritySettings.LockoutDuration.ToString());

                TempData["Success"] = "Security settings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update security settings: {ex.Message}";
                return await Index();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppSettings(SettingsViewModel model)
        {
            try
            {
                await _settingManager.SetGlobalAsync(AppSettings.ApplicationName, model.AppSettings.ApplicationName);
                await _settingManager.SetGlobalAsync(AppSettings.SupportEmail, model.AppSettings.SupportEmail);
                await _settingManager.SetGlobalAsync(AppSettings.Version, model.AppSettings.Version);
                await _settingManager.SetGlobalAsync(AppSettings.EnableLogging, model.AppSettings.EnableLogging.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.LogLevel, model.AppSettings.LogLevel);
                await _settingManager.SetGlobalAsync(AppSettings.EnableMetrics, model.AppSettings.EnableMetrics.ToString());
                await _settingManager.SetGlobalAsync(AppSettings.MaintenanceMode, model.AppSettings.MaintenanceMode.ToString());

                TempData["Success"] = "Application settings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update application settings: {ex.Message}";
                return await Index();
            }
        }

        private async Task<ConnectionStringSettings> GetConnectionStringsAsync()
        {
            return new ConnectionStringSettings
            {
                DefaultConnection = await _settingManager.GetOrNullGlobalAsync(AppSettings.DefaultConnection) ?? "",
                GrcAuthDb = await _settingManager.GetOrNullGlobalAsync(AppSettings.GrcAuthDb) ?? "",
                HangfireConnection = await _settingManager.GetOrNullGlobalAsync(AppSettings.HangfireConnection) ?? "",
                Redis = await _settingManager.GetOrNullGlobalAsync(AppSettings.Redis) ?? ""
            };
        }

        private async Task<SecuritySettings> GetSecuritySettingsAsync()
        {
            return new SecuritySettings
            {
                AllowPublicRegistration = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.AllowPublicRegistration) ?? "false"),
                RequireEmailConfirmation = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.RequireEmailConfirmation) ?? "true"),
                PasswordMinLength = int.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.PasswordMinLength) ?? "8"),
                RequireUppercase = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.RequireUppercase) ?? "true"),
                RequireLowercase = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.RequireLowercase) ?? "true"),
                RequireDigit = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.RequireDigit) ?? "true"),
                RequireSpecialChar = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.RequireSpecialChar) ?? "true"),
                MaxLoginAttempts = int.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.MaxLoginAttempts) ?? "5"),
                LockoutDuration = int.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.LockoutDuration) ?? "15")
            };
        }

        private async Task<ApplicationSettings> GetAppSettingsAsync()
        {
            return new ApplicationSettings
            {
                ApplicationName = await _settingManager.GetOrNullGlobalAsync(AppSettings.ApplicationName) ?? "Shahin GRC Platform",
                SupportEmail = await _settingManager.GetOrNullGlobalAsync(AppSettings.SupportEmail) ?? "support@shahin.com",
                Version = await _settingManager.GetOrNullGlobalAsync(AppSettings.Version) ?? "1.0.0",
                EnableLogging = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.EnableLogging) ?? "true"),
                LogLevel = await _settingManager.GetOrNullGlobalAsync(AppSettings.LogLevel) ?? "Information",
                EnableMetrics = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.EnableMetrics) ?? "true"),
                MaintenanceMode = bool.Parse(await _settingManager.GetOrNullGlobalAsync(AppSettings.MaintenanceMode) ?? "false")
            };
        }

        private async Task UpdateConnectionString(string settingName, string value)
        {
            // For connection strings, we need to update the actual configuration
            // This is a simplified approach - in production, you might want to use
            // a more sophisticated configuration management system
            if (!string.IsNullOrEmpty(value))
            {
                await _settingManager.SetGlobalAsync(settingName, value);
            }
        }
    }

    public class SettingsViewModel
    {
        public ConnectionStringSettings ConnectionStrings { get; set; } = new();
        public SecuritySettings SecuritySettings { get; set; } = new();
        public ApplicationSettings AppSettings { get; set; } = new();
    }

    public class ConnectionStringSettings
    {
        [Required]
        [Display(Name = "Default Connection")]
        public string DefaultConnection { get; set; } = "";

        [Required]
        [Display(Name = "Auth Database")]
        public string GrcAuthDb { get; set; } = "";

        [Required]
        [Display(Name = "Hangfire Connection")]
        public string HangfireConnection { get; set; } = "";

        [Required]
        [Display(Name = "Redis Connection")]
        public string Redis { get; set; } = "";
    }

    public class SecuritySettings
    {
        [Display(Name = "Allow Public Registration")]
        public bool AllowPublicRegistration { get; set; }

        [Display(Name = "Require Email Confirmation")]
        public bool RequireEmailConfirmation { get; set; }

        [Range(6, 128)]
        [Display(Name = "Minimum Password Length")]
        public int PasswordMinLength { get; set; } = 8;

        [Display(Name = "Require Uppercase")]
        public bool RequireUppercase { get; set; }

        [Display(Name = "Require Lowercase")]
        public bool RequireLowercase { get; set; }

        [Display(Name = "Require Digit")]
        public bool RequireDigit { get; set; }

        [Display(Name = "Require Special Character")]
        public bool RequireSpecialChar { get; set; }

        [Range(3, 10)]
        [Display(Name = "Max Login Attempts")]
        public int MaxLoginAttempts { get; set; } = 5;

        [Range(5, 60)]
        [Display(Name = "Lockout Duration (minutes)")]
        public int LockoutDuration { get; set; } = 15;
    }

    public class ApplicationSettings
    {
        [Required]
        [Display(Name = "Application Name")]
        public string ApplicationName { get; set; } = "Shahin GRC Platform";

        [Required]
        [EmailAddress]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; } = "support@shahin.com";

        [Required]
        [Display(Name = "Version")]
        public string Version { get; set; } = "1.0.0";

        [Display(Name = "Enable Logging")]
        public bool EnableLogging { get; set; }

        [Display(Name = "Log Level")]
        public string LogLevel { get; set; } = "Information";

        [Display(Name = "Enable Metrics")]
        public bool EnableMetrics { get; set; }

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; }
    }
}
