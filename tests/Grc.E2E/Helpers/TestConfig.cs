using Microsoft.Extensions.Configuration;

namespace Grc.E2E.Helpers
{
    public class TestConfig
    {
        private static readonly Lazy<IConfiguration> _configuration = new(() =>
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            
            return builder.Build();
        });

        public static IConfiguration Configuration => _configuration.Value;

        public static string BaseUrl => Configuration["TestSettings:BaseUrl"] ?? "http://localhost:5000";
        public static string BrowserType => Configuration["TestSettings:BrowserType"] ?? "Chrome";
        public static bool Headless => bool.Parse(Configuration["TestSettings:Headless"] ?? "false");
        public static int ImplicitWaitSeconds => int.Parse(Configuration["TestSettings:ImplicitWaitSeconds"] ?? "10");
        public static int PageLoadTimeoutSeconds => int.Parse(Configuration["TestSettings:PageLoadTimeoutSeconds"] ?? "30");
        public static bool TakeScreenshotOnFailure => bool.Parse(Configuration["TestSettings:TakeScreenshotOnFailure"] ?? "true");
        public static string ScreenshotPath => Configuration["TestSettings:ScreenshotPath"] ?? "./TestResults/Screenshots";

        public static string ApiBaseUrl => Configuration["ApiSettings:BaseApiUrl"] ?? "http://localhost:5000/api";
        public static int ApiMaxRetries => int.Parse(Configuration["ApiSettings:MaxRetries"] ?? "3");
        public static int ApiTimeoutSeconds => int.Parse(Configuration["ApiSettings:TimeoutSeconds"] ?? "30");

        public static string MailhogBaseUrl => Configuration["MailhogSettings:BaseUrl"] ?? "http://localhost:8025";
        public static string MailhogApiUrl => Configuration["MailhogSettings:ApiUrl"] ?? "http://localhost:8025/api/v2";

        public static class TestAccounts
        {
            public static (string Email, string Password) PlatformAdmin =>
                (Configuration["TestAccounts:PlatformAdmin:Email"] ?? "admin@platform.local",
                 Configuration["TestAccounts:PlatformAdmin:Password"] ?? "Admin@Platform2026!");

            public static (string Name, string Slug, string AdminEmail) DefaultTenant =>
                (Configuration["TestAccounts:DefaultTenant:Name"] ?? "TestCompany",
                 Configuration["TestAccounts:DefaultTenant:Slug"] ?? "test-company",
                 Configuration["TestAccounts:DefaultTenant:AdminEmail"] ?? "admin@testcompany.com");
        }
    }
}
