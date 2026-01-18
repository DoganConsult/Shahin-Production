using Volo.Abp.Settings;

namespace GrcMvc.Settings
{
    public static class AppSettings
    {
        // Connection Strings
        public const string DefaultConnection = "GrcMvc.DefaultConnection";
        public const string GrcAuthDb = "GrcMvc.GrcAuthDb";
        public const string HangfireConnection = "GrcMvc.HangfireConnection";
        public const string Redis = "GrcMvc.Redis";

        // Security Settings
        public const string AllowPublicRegistration = "GrcMvc.Security.AllowPublicRegistration";
        public const string RequireEmailConfirmation = "GrcMvc.Security.RequireEmailConfirmation";
        public const string PasswordMinLength = "GrcMvc.Security.PasswordMinLength";
        public const string RequireUppercase = "GrcMvc.Security.RequireUppercase";
        public const string RequireLowercase = "GrcMvc.Security.RequireLowercase";
        public const string RequireDigit = "GrcMvc.Security.RequireDigit";
        public const string RequireSpecialChar = "GrcMvc.Security.RequireSpecialChar";
        public const string MaxLoginAttempts = "GrcMvc.Security.MaxLoginAttempts";
        public const string LockoutDuration = "GrcMvc.Security.LockoutDuration";

        // Application Settings
        public const string ApplicationName = "GrcMvc.App.ApplicationName";
        public const string SupportEmail = "GrcMvc.App.SupportEmail";
        public const string Version = "GrcMvc.App.Version";
        public const string EnableLogging = "GrcMvc.App.EnableLogging";
        public const string LogLevel = "GrcMvc.App.LogLevel";
        public const string EnableMetrics = "GrcMvc.App.EnableMetrics";
        public const string MaintenanceMode = "GrcMvc.App.MaintenanceMode";

        // Secrets (Encrypted in Database)
        public const string JwtSecret = "GrcMvc.Security.JwtSecret";
        public const string ClaudeApiKey = "GrcMvc.Integrations.ClaudeApiKey";
        public const string ClaudeModel = "GrcMvc.Integrations.ClaudeModel";
        public const string ClaudeEnabled = "GrcMvc.Integrations.ClaudeEnabled";
        
        // Azure/Microsoft
        public const string AzureTenantId = "GrcMvc.Azure.TenantId";
        public const string SmtpClientId = "GrcMvc.Email.SmtpClientId";
        public const string SmtpClientSecret = "GrcMvc.Email.SmtpClientSecret";
        public const string SmtpFromEmail = "GrcMvc.Email.SmtpFromEmail";
        public const string MsGraphClientId = "GrcMvc.Email.MsGraphClientId";
        public const string MsGraphClientSecret = "GrcMvc.Email.MsGraphClientSecret";
        public const string MsGraphAppIdUri = "GrcMvc.Email.MsGraphAppIdUri";
        
        // Copilot
        public const string CopilotClientId = "GrcMvc.Integrations.CopilotClientId";
        public const string CopilotClientSecret = "GrcMvc.Integrations.CopilotClientSecret";
        public const string CopilotAppIdUri = "GrcMvc.Integrations.CopilotAppIdUri";
        
        // Integration Services
        public const string KafkaBootstrapServers = "GrcMvc.Integrations.KafkaBootstrapServers";
        public const string KafkaEnabled = "GrcMvc.Integrations.KafkaEnabled";
        public const string CamundaBaseUrl = "GrcMvc.Integrations.CamundaBaseUrl";
        public const string CamundaEnabled = "GrcMvc.Integrations.CamundaEnabled";
        public const string CamundaUsername = "GrcMvc.Integrations.CamundaUsername";
        public const string CamundaPassword = "GrcMvc.Integrations.CamundaPassword";
    }
}
