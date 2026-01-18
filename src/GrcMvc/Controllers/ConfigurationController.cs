using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GrcMvc.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class ConfigurationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<ConfigurationController> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new ConfigurationViewModel
            {
                ConnectionStrings = GetConnectionStrings(),
                SecuritySettings = GetSecuritySettings(),
                AppSettings = GetAppSettings(),
                CurrentEnvironment = _environment.EnvironmentName
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateConnectionStrings(ConfigurationViewModel model)
        {
            try
            {
                UpdateConfigurationFile("ConnectionStrings", model.ConnectionStrings);
                TempData["Success"] = "Connection strings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection strings");
                TempData["Error"] = "Failed to update connection strings";
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSecuritySettings(ConfigurationViewModel model)
        {
            try
            {
                UpdateConfigurationFile("Security", model.SecuritySettings);
                TempData["Success"] = "Security settings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update security settings");
                TempData["Error"] = "Failed to update security settings";
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAppSettings(ConfigurationViewModel model)
        {
            try
            {
                UpdateConfigurationFile("AppSettings", model.AppSettings);
                TempData["Success"] = "Application settings updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update application settings");
                TempData["Error"] = "Failed to update application settings";
                return View("Index", model);
            }
        }

        private Dictionary<string, string> GetConnectionStrings()
        {
            return new Dictionary<string, string>
            {
                ["DefaultConnection"] = _configuration.GetConnectionString("DefaultConnection") ?? "",
                ["GrcAuthDb"] = _configuration.GetConnectionString("GrcAuthDb") ?? "",
                ["HangfireConnection"] = _configuration.GetConnectionString("HangfireConnection") ?? "",
                ["Redis"] = _configuration.GetConnectionString("Redis") ?? ""
            };
        }

        private Dictionary<string, object> GetSecuritySettings()
        {
            return new Dictionary<string, object>
            {
                ["AllowPublicRegistration"] = _configuration.GetValue<bool>("Security:AllowPublicRegistration"),
                ["RequireEmailConfirmation"] = _configuration.GetValue<bool>("Security:RequireEmailConfirmation"),
                ["PasswordMinLength"] = _configuration.GetValue<int>("Security:PasswordMinLength"),
                ["RequireUppercase"] = _configuration.GetValue<bool>("Security:RequireUppercase"),
                ["RequireLowercase"] = _configuration.GetValue<bool>("Security:RequireLowercase"),
                ["RequireDigit"] = _configuration.GetValue<bool>("Security:RequireDigit"),
                ["RequireSpecialChar"] = _configuration.GetValue<bool>("Security:RequireSpecialChar"),
                ["MaxLoginAttempts"] = _configuration.GetValue<int>("Security:MaxLoginAttempts"),
                ["LockoutDuration"] = _configuration.GetValue<int>("Security:LockoutDuration")
            };
        }

        private Dictionary<string, object> GetAppSettings()
        {
            return new Dictionary<string, object>
            {
                ["ApplicationName"] = _configuration.GetValue<string>("AppSettings:ApplicationName") ?? "",
                ["SupportEmail"] = _configuration.GetValue<string>("AppSettings:SupportEmail") ?? "",
                ["Version"] = _configuration.GetValue<string>("AppSettings:Version") ?? "",
                ["EnableLogging"] = _configuration.GetValue<bool>("AppSettings:EnableLogging"),
                ["LogLevel"] = _configuration.GetValue<string>("AppSettings:LogLevel") ?? "",
                ["EnableMetrics"] = _configuration.GetValue<bool>("AppSettings:EnableMetrics"),
                ["MaintenanceMode"] = _configuration.GetValue<bool>("AppSettings:MaintenanceMode")
            };
        }

        private void UpdateConfigurationFile(string section, object values)
        {
            string configPath;
            if (_environment.IsDevelopment())
            {
                configPath = Path.Combine(_environment.ContentRootPath, "appsettings.Development.json");
            }
            else
            {
                configPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            }

            // Also check for local settings
            var localConfigPath = Path.Combine(_environment.ContentRootPath, "appsettings.Local.json");
            if (System.IO.File.Exists(localConfigPath))
            {
                configPath = localConfigPath;
            }

            var json = System.IO.File.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var newJson = UpdateJsonSection(root, section, values);
            System.IO.File.WriteAllText(configPath, newJson);

            // Reload configuration
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(_environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            var newConfig = configBuilder.Build();
            
            // Update the injected configuration
            ((IConfigurationRoot)_configuration).Reload();
        }

        private string UpdateJsonSection(JsonElement root, string section, object values)
        {
            var dict = new Dictionary<string, object>();
            
            // Copy existing properties
            foreach (var property in root.EnumerateObject())
            {
                if (property.Name != section)
                {
                    dict[property.Name] = JsonElementToObject(property.Value);
                }
            }

            // Add/update the section
            dict[section] = values;

            return JsonSerializer.Serialize(dict, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        private object JsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
                JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToArray(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
                _ => null
            };
        }
    }

    public class ConfigurationViewModel
    {
        public Dictionary<string, string> ConnectionStrings { get; set; } = new();
        public Dictionary<string, object> SecuritySettings { get; set; } = new();
        public Dictionary<string, object> AppSettings { get; set; } = new();
        public string CurrentEnvironment { get; set; } = "";
    }
}
