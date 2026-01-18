using Volo.Abp.Settings;
using Volo.Abp.Localization;
using Microsoft.Extensions.Localization;

namespace GrcMvc.Settings
{
    public class GrcMvcSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            // Connection Strings (Encrypted - contain passwords)
            context.Add(
                new SettingDefinition(
                    AppSettings.DefaultConnection,
                    "",
                    L("DisplayName:GrcMvc.DefaultConnection"),
                    L("Description:GrcMvc.DefaultConnection"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.GrcAuthDb,
                    "",
                    L("DisplayName:GrcMvc.GrcAuthDb"),
                    L("Description:GrcMvc.GrcAuthDb"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.HangfireConnection,
                    "",
                    L("DisplayName:GrcMvc.HangfireConnection"),
                    L("Description:GrcMvc.HangfireConnection"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.Redis,
                    "",
                    L("DisplayName:GrcMvc.Redis"),
                    L("Description:GrcMvc.Redis"),
                    isVisibleToClients: false
                )
            );

            // Security Settings
            context.Add(
                new SettingDefinition(
                    AppSettings.AllowPublicRegistration,
                    "false",
                    L("DisplayName:GrcMvc.Security.AllowPublicRegistration"),
                    L("Description:GrcMvc.Security.AllowPublicRegistration"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.RequireEmailConfirmation,
                    "true",
                    L("DisplayName:GrcMvc.Security.RequireEmailConfirmation"),
                    L("Description:GrcMvc.Security.RequireEmailConfirmation"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.PasswordMinLength,
                    "8",
                    L("DisplayName:GrcMvc.Security.PasswordMinLength"),
                    L("Description:GrcMvc.Security.PasswordMinLength"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.RequireUppercase,
                    "true",
                    L("DisplayName:GrcMvc.Security.RequireUppercase"),
                    L("Description:GrcMvc.Security.RequireUppercase"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.RequireLowercase,
                    "true",
                    L("DisplayName:GrcMvc.Security.RequireLowercase"),
                    L("Description:GrcMvc.Security.RequireLowercase"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.RequireDigit,
                    "true",
                    L("DisplayName:GrcMvc.Security.RequireDigit"),
                    L("Description:GrcMvc.Security.RequireDigit"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.RequireSpecialChar,
                    "true",
                    L("DisplayName:GrcMvc.Security.RequireSpecialChar"),
                    L("Description:GrcMvc.Security.RequireSpecialChar"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.MaxLoginAttempts,
                    "5",
                    L("DisplayName:GrcMvc.Security.MaxLoginAttempts"),
                    L("Description:GrcMvc.Security.MaxLoginAttempts"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.LockoutDuration,
                    "15",
                    L("DisplayName:GrcMvc.Security.LockoutDuration"),
                    L("Description:GrcMvc.Security.LockoutDuration"),
                    isVisibleToClients: true
                )
            );

            // Application Settings
            context.Add(
                new SettingDefinition(
                    AppSettings.ApplicationName,
                    "Shahin GRC Platform",
                    L("DisplayName:GrcMvc.App.ApplicationName"),
                    L("Description:GrcMvc.App.ApplicationName"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.SupportEmail,
                    "support@shahin.com",
                    L("DisplayName:GrcMvc.App.SupportEmail"),
                    L("Description:GrcMvc.App.SupportEmail"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.Version,
                    "1.0.0",
                    L("DisplayName:GrcMvc.App.Version"),
                    L("Description:GrcMvc.App.Version"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.EnableLogging,
                    "true",
                    L("DisplayName:GrcMvc.App.EnableLogging"),
                    L("Description:GrcMvc.App.EnableLogging"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.LogLevel,
                    "Information",
                    L("DisplayName:GrcMvc.App.LogLevel"),
                    L("Description:GrcMvc.App.LogLevel"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.EnableMetrics,
                    "true",
                    L("DisplayName:GrcMvc.App.EnableMetrics"),
                    L("Description:GrcMvc.App.EnableMetrics"),
                    isVisibleToClients: true
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.MaintenanceMode,
                    "false",
                    L("DisplayName:GrcMvc.App.MaintenanceMode"),
                    L("Description:GrcMvc.App.MaintenanceMode"),
                    isVisibleToClients: true
                )
            );

            // Secrets - Encrypted in Database
            context.Add(
                new SettingDefinition(
                    AppSettings.JwtSecret,
                    "",
                    L("DisplayName:GrcMvc.Security.JwtSecret"),
                    L("Description:GrcMvc.Security.JwtSecret"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            // Claude AI
            context.Add(
                new SettingDefinition(
                    AppSettings.ClaudeApiKey,
                    "",
                    L("DisplayName:GrcMvc.Integrations.ClaudeApiKey"),
                    L("Description:GrcMvc.Integrations.ClaudeApiKey"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.ClaudeModel,
                    "claude-sonnet-4-20250514",
                    L("DisplayName:GrcMvc.Integrations.ClaudeModel"),
                    L("Description:GrcMvc.Integrations.ClaudeModel"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.ClaudeEnabled,
                    "true",
                    L("DisplayName:GrcMvc.Integrations.ClaudeEnabled"),
                    L("Description:GrcMvc.Integrations.ClaudeEnabled"),
                    isVisibleToClients: false
                )
            );

            // Azure/Microsoft
            context.Add(
                new SettingDefinition(
                    AppSettings.AzureTenantId,
                    "",
                    L("DisplayName:GrcMvc.Azure.TenantId"),
                    L("Description:GrcMvc.Azure.TenantId"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.SmtpClientId,
                    "",
                    L("DisplayName:GrcMvc.Email.SmtpClientId"),
                    L("Description:GrcMvc.Email.SmtpClientId"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.SmtpClientSecret,
                    "",
                    L("DisplayName:GrcMvc.Email.SmtpClientSecret"),
                    L("Description:GrcMvc.Email.SmtpClientSecret"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.SmtpFromEmail,
                    "",
                    L("DisplayName:GrcMvc.Email.SmtpFromEmail"),
                    L("Description:GrcMvc.Email.SmtpFromEmail"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.MsGraphClientId,
                    "",
                    L("DisplayName:GrcMvc.Email.MsGraphClientId"),
                    L("Description:GrcMvc.Email.MsGraphClientId"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.MsGraphClientSecret,
                    "",
                    L("DisplayName:GrcMvc.Email.MsGraphClientSecret"),
                    L("Description:GrcMvc.Email.MsGraphClientSecret"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.MsGraphAppIdUri,
                    "",
                    L("DisplayName:GrcMvc.Email.MsGraphAppIdUri"),
                    L("Description:GrcMvc.Email.MsGraphAppIdUri"),
                    isVisibleToClients: false
                )
            );

            // Copilot
            context.Add(
                new SettingDefinition(
                    AppSettings.CopilotClientId,
                    "",
                    L("DisplayName:GrcMvc.Integrations.CopilotClientId"),
                    L("Description:GrcMvc.Integrations.CopilotClientId"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CopilotClientSecret,
                    "",
                    L("DisplayName:GrcMvc.Integrations.CopilotClientSecret"),
                    L("Description:GrcMvc.Integrations.CopilotClientSecret"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CopilotAppIdUri,
                    "",
                    L("DisplayName:GrcMvc.Integrations.CopilotAppIdUri"),
                    L("Description:GrcMvc.Integrations.CopilotAppIdUri"),
                    isVisibleToClients: false
                )
            );

            // Integration Services
            context.Add(
                new SettingDefinition(
                    AppSettings.KafkaBootstrapServers,
                    "localhost:9092",
                    L("DisplayName:GrcMvc.Integrations.KafkaBootstrapServers"),
                    L("Description:GrcMvc.Integrations.KafkaBootstrapServers"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.KafkaEnabled,
                    "false",
                    L("DisplayName:GrcMvc.Integrations.KafkaEnabled"),
                    L("Description:GrcMvc.Integrations.KafkaEnabled"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CamundaBaseUrl,
                    "http://localhost:8085/camunda",
                    L("DisplayName:GrcMvc.Integrations.CamundaBaseUrl"),
                    L("Description:GrcMvc.Integrations.CamundaBaseUrl"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CamundaEnabled,
                    "false",
                    L("DisplayName:GrcMvc.Integrations.CamundaEnabled"),
                    L("Description:GrcMvc.Integrations.CamundaEnabled"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CamundaUsername,
                    "admin",
                    L("DisplayName:GrcMvc.Integrations.CamundaUsername"),
                    L("Description:GrcMvc.Integrations.CamundaUsername"),
                    isVisibleToClients: false
                )
            );

            context.Add(
                new SettingDefinition(
                    AppSettings.CamundaPassword,
                    "",
                    L("DisplayName:GrcMvc.Integrations.CamundaPassword"),
                    L("Description:GrcMvc.Integrations.CamundaPassword"),
                    isEncrypted: true,
                    isVisibleToClients: false
                )
            );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<GrcMvcResource>(name);
        }
    }
}
