using Microsoft.Extensions.Configuration;
using Volo.Abp.Settings;
using Volo.Abp.DependencyInjection;

namespace GrcMvc.Settings;

/// <summary>
/// Value provider that reads from environment variables and IConfiguration
/// Provides fallback for ABP Settings when not set in database
/// </summary>
public class EnvironmentVariableSettingValueProvider : ISettingValueProvider, ITransientDependency
{
    public const string ProviderName = "Environment";

    private readonly IConfiguration _configuration;

    public EnvironmentVariableSettingValueProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Name => ProviderName;

    public async Task<string?> GetOrNullAsync(SettingDefinition setting)
    {
        // Map ABP setting names to environment variable names
        var envVarName = GetEnvironmentVariableName(setting.Name);
        
        if (envVarName != null)
        {
            // Try environment variable first (most dynamic - reads from current environment)
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrEmpty(envValue))
            {
                Console.WriteLine("[ABP-SETTINGS] ✅ EnvironmentVariable provider: {Setting} = {Value} (from env var: {EnvVar})", 
                    setting.Name, MaskSensitiveValue(setting.Name, envValue), envVarName);
                return envValue;
            }

            // Fallback to configuration (appsettings.json - reloaded dynamically)
            var configValue = _configuration[envVarName];
            if (!string.IsNullOrEmpty(configValue))
            {
                Console.WriteLine("[ABP-SETTINGS] ✅ EnvironmentVariable provider: {Setting} = {Value} (from IConfiguration: {EnvVar})", 
                    setting.Name, MaskSensitiveValue(setting.Name, configValue), envVarName);
                return configValue;
            }

            // Try alternative formats
            var altFormats = GetAlternativeFormats(envVarName);
            foreach (var altFormat in altFormats)
            {
                var altValue = Environment.GetEnvironmentVariable(altFormat)
                            ?? _configuration[altFormat];
                if (!string.IsNullOrEmpty(altValue))
                {
                    Console.WriteLine("[ABP-SETTINGS] ✅ EnvironmentVariable provider: {Setting} = {Value} (from alt format: {AltFormat})", 
                        setting.Name, MaskSensitiveValue(setting.Name, altValue), altFormat);
                    return altValue;
                }
            }
            
            Console.WriteLine("[ABP-SETTINGS] ❌ EnvironmentVariable provider: {Setting} = (not found in env vars or config)", setting.Name);
        }

        return await Task.FromResult((string?)null);
    }

    private static string MaskSensitiveValue(string settingName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "[empty]";
        
        if (settingName.Contains("Secret") || settingName.Contains("Password") || settingName.Contains("Key") || settingName.Contains("Connection"))
        {
            if (value.Length > 10)
            {
                return $"{value.Substring(0, 4)}...{value.Substring(value.Length - 4)}";
            }
            return "***";
        }
        return value;
    }

    public Task<List<SettingValue>> GetAllAsync(SettingDefinition[] settings)
    {
        var values = new List<SettingValue>();

        foreach (var setting in settings)
        {
            var value = GetOrNullAsync(setting).Result;
            if (value != null)
            {
                values.Add(new SettingValue(setting.Name, value));
            }
        }

        return Task.FromResult(values);
    }

    private string? GetEnvironmentVariableName(string settingName)
    {
        return settingName switch
        {
            // Connection Strings
            AppSettings.DefaultConnection => "ConnectionStrings__DefaultConnection",
            AppSettings.GrcAuthDb => "ConnectionStrings__GrcAuthDb",
            AppSettings.HangfireConnection => "ConnectionStrings__HangfireConnection",
            AppSettings.Redis => "ConnectionStrings__Redis",

            // Secrets
            AppSettings.JwtSecret => "JWT_SECRET",
            AppSettings.ClaudeApiKey => "CLAUDE_API_KEY",
            AppSettings.ClaudeModel => "CLAUDE_MODEL",
            AppSettings.ClaudeEnabled => "CLAUDE_ENABLED",

            // Azure/Microsoft
            AppSettings.AzureTenantId => "AZURE_TENANT_ID",
            AppSettings.SmtpClientId => "SMTP_CLIENT_ID",
            AppSettings.SmtpClientSecret => "SMTP_CLIENT_SECRET",
            AppSettings.SmtpFromEmail => "SMTP_FROM_EMAIL",
            AppSettings.MsGraphClientId => "MSGRAPH_CLIENT_ID",
            AppSettings.MsGraphClientSecret => "MSGRAPH_CLIENT_SECRET",
            AppSettings.MsGraphAppIdUri => "MSGRAPH_APP_ID_URI",

            // Copilot
            AppSettings.CopilotClientId => "COPILOT_CLIENT_ID",
            AppSettings.CopilotClientSecret => "COPILOT_CLIENT_SECRET",
            AppSettings.CopilotAppIdUri => "COPILOT_APP_ID_URI",

            // Integration Services
            AppSettings.KafkaBootstrapServers => "KAFKA_BOOTSTRAP_SERVERS",
            AppSettings.KafkaEnabled => "KAFKA_ENABLED",
            AppSettings.CamundaBaseUrl => "CAMUNDA_BASE_URL",
            AppSettings.CamundaEnabled => "CAMUNDA_ENABLED",
            AppSettings.CamundaUsername => "CAMUNDA_USERNAME",
            AppSettings.CamundaPassword => "CAMUNDA_PASSWORD",

            _ => null
        };
    }

    private string[] GetAlternativeFormats(string envVarName)
    {
        var alternatives = new List<string>();

        // Add alternative formats
        if (envVarName.Contains("__"))
        {
            // ConnectionStrings__DefaultConnection -> ConnectionStrings:DefaultConnection
            alternatives.Add(envVarName.Replace("__", ":"));
        }

        // Add underscore format
        alternatives.Add(envVarName.Replace("__", "_"));

        return alternatives.ToArray();
    }
}
