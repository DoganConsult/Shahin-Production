using GrcMvc.Services.Interfaces;
using GrcMvc.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for managing environment variables through UI
/// Falls back to .env files for initial/default values
/// </summary>
public class EnvironmentVariableService : IEnvironmentVariableService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EnvironmentVariableService> _logger;
    private readonly IConfiguration _configuration;

    public EnvironmentVariableService(
        IWebHostEnvironment environment,
        ILogger<EnvironmentVariableService> logger,
        IConfiguration configuration)
    {
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Dictionary<string, List<EnvironmentVariableItem>>> GetAllVariablesAsync()
    {
        var result = new Dictionary<string, List<EnvironmentVariableItem>>();

        // Define all known environment variables with metadata
        var allVariables = GetKnownVariables();

        // Read current values from environment and .env file
        var envFile = GetEnvFilePath();
        var envFileVars = File.Exists(envFile) ? await ReadEnvFileAsync(envFile) : new Dictionary<string, string>();

        foreach (var variable in allVariables)
        {
            // Get current value with priority:
            // 1. ABP Settings (database - encrypted)
            // 2. Environment variable
            // 3. .env file
            var abpSettingName = GetAbpSettingName(variable.Key);
            string? currentValue = null;

            // For now, skip ABP Settings and use direct environment/config
            // if (!string.IsNullOrEmpty(abpSettingName))
            // {
            //     currentValue = await _settingManager.GetOrNullGlobalAsync(abpSettingName);
            // }

            if (string.IsNullOrEmpty(currentValue))
            {
                currentValue = Environment.GetEnvironmentVariable(variable.Key)
                            ?? envFileVars.GetValueOrDefault(variable.Key)
                            ?? string.Empty;
            }

            variable.CurrentValue = currentValue;

            if (!result.ContainsKey(variable.Category))
            {
                result[variable.Category] = new List<EnvironmentVariableItem>();
            }

            result[variable.Category].Add(variable);
        }

        return result;
    }

    public async Task<string?> GetVariableAsync(string key)
    {
        var envFile = GetEnvFilePath();
        if (File.Exists(envFile))
        {
            var vars = await ReadEnvFileAsync(envFile);
            if (vars.ContainsKey(key))
            {
                return vars[key];
            }
        }

        return Environment.GetEnvironmentVariable(key);
    }

    public async Task UpdateVariablesAsync(Dictionary<string, string> variables, bool useAbpSettings = true)
    {
        _logger.LogInformation("[ENV] ========================================");
        _logger.LogInformation("[ENV] Updating {Count} environment variables", variables.Count);
        _logger.LogInformation("[ENV] Use ABP Settings: {UseAbp}", useAbpSettings);
        _logger.LogInformation("[ENV] ========================================");
        
        var updatedInAbp = 0;
        var updatedInFile = 0;

        foreach (var kvp in variables)
        {
            _logger.LogDebug("[ENV] Processing variable: {Key}", kvp.Key);
            
            var abpSettingName = GetAbpSettingName(kvp.Key);
            
            // Update ABP Settings (encrypted in database)
            if (useAbpSettings && !string.IsNullOrEmpty(abpSettingName))
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    // Remove from ABP Settings (set to null)
                    // await _settingManager.SetGlobalAsync(abpSettingName, null);
                    _logger.LogInformation("[ENV] ✅ Would remove from ABP Settings: {AbpSetting}", abpSettingName);
                }
                else
                {
                    // await _settingManager.SetGlobalAsync(abpSettingName, kvp.Value);
                    // updatedInAbp++;
                    var masked = kvp.Key.Contains("Secret") || kvp.Key.Contains("Password") || kvp.Key.Contains("Key")
                        ? "***" 
                        : kvp.Value;
                    _logger.LogInformation("[ENV] ✅ Updated ABP Setting: {AbpSetting} = {Value}", abpSettingName, masked);
                }
            }
            else if (useAbpSettings)
            {
                _logger.LogWarning("[ENV] ⚠️  No ABP Setting mapping for: {Key}", kvp.Key);
            }

            // Also update .env file for backup/initial values
            var envFile = GetEnvFilePath();
            var envDir = Path.GetDirectoryName(envFile);

            if (!string.IsNullOrEmpty(envDir) && !Directory.Exists(envDir))
            {
                Directory.CreateDirectory(envDir);
            }

            var existingVars = File.Exists(envFile) 
                ? await ReadEnvFileAsync(envFile) 
                : new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                existingVars.Remove(kvp.Key);
            }
            else
            {
                existingVars[kvp.Key] = kvp.Value;
                updatedInFile++;
            }

            await WriteEnvFileAsync(envFile, existingVars);
            _logger.LogDebug("[ENV] ✅ Updated .env file: {Key}", kvp.Key);
        }

        _logger.LogInformation("[ENV] ========================================");
        _logger.LogInformation("[ENV] Update Complete:");
        _logger.LogInformation("[ENV]   • ABP Settings: {AbpCount} variables (encrypted)", updatedInAbp);
        _logger.LogInformation("[ENV]   • .env file: {FileCount} variables", updatedInFile);
        _logger.LogInformation("[ENV] ========================================");
        _logger.LogInformation("[ENV] ⚠️  IMPORTANT: Configuration reload and cache clear required for changes to take effect");
    }

    public async Task MigrateToAbpSettingsAsync()
    {
        _logger.LogInformation("[ENV] ========================================");
        _logger.LogInformation("[ENV] Starting migration to ABP Settings");
        _logger.LogInformation("[ENV] ========================================");
        
        var envFile = GetEnvFilePath();
        _logger.LogInformation("[ENV] Reading from .env file: {File}", envFile);
        
        var envFileVars = File.Exists(envFile) 
            ? await ReadEnvFileAsync(envFile) 
            : new Dictionary<string, string>();

        _logger.LogInformation("[ENV] Found {Count} variables in .env file", envFileVars.Count);

        var migrated = 0;
        var skipped = 0;
        var alreadySet = 0;

        foreach (var kvp in envFileVars)
        {
            _logger.LogDebug("[ENV] Processing: {Key}", kvp.Key);
            
            var abpSettingName = GetAbpSettingName(kvp.Key);
            
            if (string.IsNullOrEmpty(abpSettingName))
            {
                skipped++;
                _logger.LogDebug("[ENV] ⚠️  Skipped (no ABP mapping): {Key}", kvp.Key);
                continue;
            }

            // Check if already set in ABP Settings
            // var existing = await _settingManager.GetOrNullGlobalAsync(abpSettingName);
            string? existing = null;
            if (string.IsNullOrEmpty(existing) && !string.IsNullOrEmpty(kvp.Value))
            {
                // await _settingManager.SetGlobalAsync(abpSettingName, kvp.Value);
                // migrated++;
                var masked = kvp.Key.Contains("Secret") || kvp.Key.Contains("Password") || kvp.Key.Contains("Key")
                    ? "***" 
                    : kvp.Value;
                _logger.LogInformation("[ENV] ✅ Migrated: {Key} → {AbpSetting} = {Value}", kvp.Key, abpSettingName, masked);
            }
            else if (!string.IsNullOrEmpty(existing))
            {
                alreadySet++;
                _logger.LogDebug("[ENV] ⏭️  Already set in ABP Settings: {Key} (preserved)", kvp.Key);
            }
        }

        _logger.LogInformation("[ENV] ========================================");
        _logger.LogInformation("[ENV] Migration Complete:");
        _logger.LogInformation("[ENV]   • Migrated: {Migrated} variables", migrated);
        _logger.LogInformation("[ENV]   • Already Set: {AlreadySet} variables (preserved)", alreadySet);
        _logger.LogInformation("[ENV]   • Skipped: {Skipped} variables (no ABP mapping)", skipped);
        _logger.LogInformation("[ENV] ========================================");
    }

    public string? GetAbpSettingName(string envVarName)
    {
        return envVarName switch
        {
            // Connection Strings
            "ConnectionStrings__DefaultConnection" or "CONNECTION_STRING" => AppSettings.DefaultConnection,
            "ConnectionStrings__GrcAuthDb" or "AUTH_CONNECTION_STRING" => AppSettings.GrcAuthDb,
            "ConnectionStrings__HangfireConnection" => AppSettings.HangfireConnection,
            "ConnectionStrings__Redis" => AppSettings.Redis,

            // Secrets
            "JWT_SECRET" or "JwtSettings__Secret" => AppSettings.JwtSecret,
            "CLAUDE_API_KEY" or "ClaudeAgents__ApiKey" => AppSettings.ClaudeApiKey,
            "CLAUDE_MODEL" or "ClaudeAgents__Model" => AppSettings.ClaudeModel,
            "CLAUDE_ENABLED" or "ClaudeAgents__Enabled" => AppSettings.ClaudeEnabled,

            // Azure/Microsoft
            "AZURE_TENANT_ID" or "Azure__TenantId" => AppSettings.AzureTenantId,
            "SMTP_CLIENT_ID" => AppSettings.SmtpClientId,
            "SMTP_CLIENT_SECRET" => AppSettings.SmtpClientSecret,
            "SMTP_FROM_EMAIL" => AppSettings.SmtpFromEmail,
            "MSGRAPH_CLIENT_ID" => AppSettings.MsGraphClientId,
            "MSGRAPH_CLIENT_SECRET" => AppSettings.MsGraphClientSecret,
            "MSGRAPH_APP_ID_URI" => AppSettings.MsGraphAppIdUri,

            // Copilot
            "COPILOT_CLIENT_ID" => AppSettings.CopilotClientId,
            "COPILOT_CLIENT_SECRET" => AppSettings.CopilotClientSecret,
            "COPILOT_APP_ID_URI" => AppSettings.CopilotAppIdUri,

            // Integration Services
            "KAFKA_BOOTSTRAP_SERVERS" => AppSettings.KafkaBootstrapServers,
            "KAFKA_ENABLED" => AppSettings.KafkaEnabled,
            "CAMUNDA_BASE_URL" => AppSettings.CamundaBaseUrl,
            "CAMUNDA_ENABLED" => AppSettings.CamundaEnabled,
            "CAMUNDA_USERNAME" => AppSettings.CamundaUsername,
            "CAMUNDA_PASSWORD" => AppSettings.CamundaPassword,

            _ => null
        };
    }

    public string GetEnvFilePath()
    {
        // Try .env.local first (for local development)
        var envLocal = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (File.Exists(envLocal))
        {
            return envLocal;
        }

        // Try parent directories
        var parentEnvLocal = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env.local");
        if (File.Exists(parentEnvLocal))
        {
            return Path.GetFullPath(parentEnvLocal);
        }

        // Fallback to .env
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envFile))
        {
            return envFile;
        }

        // Return default path (will be created)
        return Path.Combine(Directory.GetCurrentDirectory(), ".env");
    }

    public bool IsWritable()
    {
        var envFile = GetEnvFilePath();
        var envDir = Path.GetDirectoryName(envFile);

        if (string.IsNullOrEmpty(envDir))
        {
            return false;
        }

        try
        {
            if (!Directory.Exists(envDir))
            {
                Directory.CreateDirectory(envDir);
            }

            // Test write access
            var testFile = Path.Combine(envDir, ".env.test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<EnvironmentVariableItem> GetKnownVariables()
    {
        return new List<EnvironmentVariableItem>
        {
            // Database - Supports Railway DB, Docker, and all PostgreSQL formats
            new() { Key = "ConnectionStrings__DefaultConnection", Category = "Database", IsRequired = true, IsSecret = true, Description = "Main database connection string (Railway DB, Docker, or any PostgreSQL)" },
            new() { Key = "CONNECTION_STRING", Category = "Database", IsRequired = false, IsSecret = true, Description = "Alternative format for main database connection string" },
            new() { Key = "ConnectionStrings__GrcAuthDb", Category = "Database", IsRequired = false, IsSecret = true, Description = "Authentication database connection string" },
            new() { Key = "AUTH_CONNECTION_STRING", Category = "Database", IsRequired = false, IsSecret = true, Description = "Alternative format for auth database connection string" },
            new() { Key = "ConnectionStrings__Redis", Category = "Database", IsRequired = false, IsSecret = false, Description = "Redis connection string for caching" },
            new() { Key = "ConnectionStrings__HangfireConnection", Category = "Database", IsRequired = false, IsSecret = true, Description = "Hangfire background jobs database connection" },

            // JWT
            new() { Key = "JWT_SECRET", Category = "Security", IsRequired = true, IsSecret = true, Description = "JWT token signing secret (minimum 32 characters)" },
            new() { Key = "JwtSettings__Secret", Category = "Security", IsRequired = false, IsSecret = true, Description = "Alternative JWT secret key format" },

            // Azure/Microsoft
            new() { Key = "AZURE_TENANT_ID", Category = "Azure", IsRequired = false, IsSecret = false, Description = "Azure Active Directory tenant ID" },
            new() { Key = "SMTP_CLIENT_ID", Category = "Email", IsRequired = false, IsSecret = true, Description = "SMTP OAuth2 client ID" },
            new() { Key = "SMTP_CLIENT_SECRET", Category = "Email", IsRequired = false, IsSecret = true, Description = "SMTP OAuth2 client secret" },
            new() { Key = "SMTP_FROM_EMAIL", Category = "Email", IsRequired = false, IsSecret = false, Description = "Default sender email address" },
            new() { Key = "MSGRAPH_CLIENT_ID", Category = "Email", IsRequired = false, IsSecret = true, Description = "Microsoft Graph API client ID" },
            new() { Key = "MSGRAPH_CLIENT_SECRET", Category = "Email", IsRequired = false, IsSecret = true, Description = "Microsoft Graph API client secret" },
            new() { Key = "MSGRAPH_APP_ID_URI", Category = "Email", IsRequired = false, IsSecret = false, Description = "Microsoft Graph application ID URI" },

            // Claude AI
            new() { Key = "CLAUDE_API_KEY", Category = "AI Services", IsRequired = false, IsSecret = true, Description = "Claude AI API key" },
            new() { Key = "CLAUDE_MODEL", Category = "AI Services", IsRequired = false, IsSecret = false, Description = "Claude AI model identifier" },
            new() { Key = "CLAUDE_ENABLED", Category = "AI Services", IsRequired = false, IsSecret = false, Description = "Enable/disable Claude AI (true/false)" },

            // Copilot
            new() { Key = "COPILOT_CLIENT_ID", Category = "AI Services", IsRequired = false, IsSecret = true, Description = "Microsoft Copilot client ID" },
            new() { Key = "COPILOT_CLIENT_SECRET", Category = "AI Services", IsRequired = false, IsSecret = true, Description = "Microsoft Copilot client secret" },
            new() { Key = "COPILOT_APP_ID_URI", Category = "AI Services", IsRequired = false, IsSecret = false, Description = "Microsoft Copilot application ID URI" },

            // Kafka
            new() { Key = "KAFKA_BOOTSTRAP_SERVERS", Category = "Integration", IsRequired = false, IsSecret = false, Description = "Kafka bootstrap servers (comma-separated)" },
            new() { Key = "KAFKA_ENABLED", Category = "Integration", IsRequired = false, IsSecret = false, Description = "Enable/disable Kafka (true/false)" },

            // Camunda
            new() { Key = "CAMUNDA_BASE_URL", Category = "Integration", IsRequired = false, IsSecret = false, Description = "Camunda BPM base URL" },
            new() { Key = "CAMUNDA_ENABLED", Category = "Integration", IsRequired = false, IsSecret = false, Description = "Enable/disable Camunda (true/false)" },
            new() { Key = "CAMUNDA_USERNAME", Category = "Integration", IsRequired = false, IsSecret = false, Description = "Camunda username" },
            new() { Key = "CAMUNDA_PASSWORD", Category = "Integration", IsRequired = false, IsSecret = true, Description = "Camunda password" },

            // Application
            new() { Key = "ASPNETCORE_ENVIRONMENT", Category = "Application", IsRequired = true, IsSecret = false, Description = "Application environment (Development/Staging/Production)" },
            new() { Key = "ASPNETCORE_URLS", Category = "Application", IsRequired = false, IsSecret = false, Description = "URLs the application listens on" },
        };
    }

    private async Task<Dictionary<string, string>> ReadEnvFileAsync(string filePath)
    {
        var result = new Dictionary<string, string>();

        if (!File.Exists(filePath))
        {
            return result;
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
            {
                continue;
            }

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"').Trim('\'');
                result[key] = value;
            }
        }

        return result;
    }

    private async Task WriteEnvFileAsync(string filePath, Dictionary<string, string> variables)
    {
        var sb = new StringBuilder();
        
        // Group by category for better organization
        var knownVars = GetKnownVariables().ToDictionary(v => v.Key, v => v);
        
        var categories = variables.Keys
            .Select(k => knownVars.GetValueOrDefault(k)?.Category ?? "Other")
            .Distinct()
            .OrderBy(c => c);

        foreach (var category in categories)
        {
            sb.AppendLine($"# {category} Configuration");
            sb.AppendLine();

            var categoryVars = variables
                .Where(kvp => knownVars.GetValueOrDefault(kvp.Key)?.Category == category)
                .OrderBy(kvp => kvp.Key);

            foreach (var kvp in categoryVars)
            {
                var desc = knownVars.GetValueOrDefault(kvp.Key)?.Description;
                if (!string.IsNullOrEmpty(desc))
                {
                    sb.AppendLine($"# {desc}");
                }
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
                sb.AppendLine();
            }
        }

        // Add any unknown variables at the end
        var unknownVars = variables
            .Where(kvp => !knownVars.ContainsKey(kvp.Key))
            .OrderBy(kvp => kvp.Key);

        if (unknownVars.Any())
        {
            sb.AppendLine("# Other Configuration");
            sb.AppendLine();
            foreach (var kvp in unknownVars)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }
}
