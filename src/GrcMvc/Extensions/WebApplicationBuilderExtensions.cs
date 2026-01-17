using GrcMvc.Abp;
using GrcMvc.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Autofac.Extensions.DependencyInjection;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for WebApplicationBuilder configuration
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configure Serilog logging with file and console sinks
    /// </summary>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "GrcMvc")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "/app/logs/grcmvc-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                shared: true)
            .WriteTo.File(
                path: "/app/logs/grcmvc-errors-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Warning,
                retainedFileCountLimit: 60)
        );

        return builder;
    }

    /// <summary>
    /// Load environment variables from .env files and validate critical configuration
    /// </summary>
    public static WebApplicationBuilder LoadEnvironmentConfiguration(this WebApplicationBuilder builder)
    {
        // Load .env.local first (local development)
        var envLocalFile = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (!File.Exists(envLocalFile))
        {
            envLocalFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env.local");
        }

        // Fallback to .env (Docker/production)
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(envFile))
        {
            envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
        }

        // Load .env.local first (highest priority)
        if (File.Exists(envLocalFile))
        {
            LoadEnvFile(envLocalFile);
            Console.WriteLine($"[ENV] Loaded LOCAL environment variables from: {envLocalFile}");
        }
        // Fallback to .env
        else if (File.Exists(envFile))
        {
            LoadEnvFile(envFile);
            Console.WriteLine($"[ENV] Loaded environment variables from: {envFile}");
        }

        // Load appsettings.Local.json ONLY in Development
        if (builder.Environment.IsDevelopment())
        {
            var localSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Local.json");
            if (File.Exists(localSettingsPath))
            {
                builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
                Console.WriteLine($"[CONFIG] Loaded local settings from: {localSettingsPath}");
            }
        }

        // Resolve connection strings
        ResolveConnectionStrings(builder);

        // Validate critical configuration in production
        if (builder.Environment.IsProduction())
        {
            ValidateProductionConfiguration(builder);
        }

        return builder;
    }

    /// <summary>
    /// Configure ABP Framework with Autofac
    /// </summary>
    public static async Task<WebApplicationBuilder> ConfigureAbpFramework(this WebApplicationBuilder builder)
    {
        // Use Autofac as DI container
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        // Register ABP Application Module
        await builder.Services.AddApplicationAsync<GrcMvcAbpModule>();

        return builder;
    }

    /// <summary>
    /// Bind strongly-typed configuration sections with validation
    /// </summary>
    public static WebApplicationBuilder BindConfiguration(this WebApplicationBuilder builder)
    {
        // Bind configuration sections
        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection(JwtSettings.SectionName));
        builder.Services.Configure<ApplicationSettings>(
            builder.Configuration.GetSection(ApplicationSettings.SectionName));
        builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection(EmailSettings.SectionName));
        builder.Services.Configure<SiteSettings>(
            builder.Configuration.GetSection(SiteSettings.SectionName));
        builder.Services.Configure<ClaudeApiSettings>(
            builder.Configuration.GetSection(ClaudeApiSettings.SectionName));

        // Rate Limiting & Fraud Detection
        builder.Services.Configure<GrcMvc.Services.Security.RateLimitingOptions>(
            builder.Configuration.GetSection(GrcMvc.Services.Security.RateLimitingOptions.SectionName));
        builder.Services.Configure<GrcMvc.Services.Security.FraudDetectionOptions>(
            builder.Configuration.GetSection(GrcMvc.Services.Security.FraudDetectionOptions.SectionName));

        // Validate configuration at startup
        builder.Services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<ApplicationSettings>, ApplicationSettingsValidator>();

        return builder;
    }

    #region Private Helper Methods

    private static void LoadEnvFile(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var envKey = parts[0].Trim();
                var envValue = parts[1].Trim();
                Environment.SetEnvironmentVariable(envKey, envValue);
            }
        }
    }

    private static void ResolveConnectionStrings(WebApplicationBuilder builder)
    {
        // FORCE RAILWAY DATABASE CONNECTION - OVERRIDE EVERYTHING
        string connectionString = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require;Trust Server Certificate=true";
        
        Console.WriteLine("[CONFIG] FORCING Railway PostgreSQL connection");
        Console.WriteLine("[CONFIG] Database: caboose.proxy.rlwy.net:11527 / postgres@GrcMvcDb");
        
        // Set all possible connection string keys
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        builder.Configuration["ConnectionStrings:Default"] = connectionString;
        string authConnectionString = connectionString;

        if (string.IsNullOrWhiteSpace(authConnectionString) && !string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                var csb = new NpgsqlConnectionStringBuilder(connectionString);
                csb.Database = $"{csb.Database}_auth";
                authConnectionString = csb.ConnectionString;
            }
            catch
            {
                authConnectionString = connectionString;
            }
        }

        if (!string.IsNullOrWhiteSpace(authConnectionString))
        {
            builder.Configuration["ConnectionStrings:GrcAuthDb"] = authConnectionString;
        }
    }

    private static void ValidateProductionConfiguration(WebApplicationBuilder builder)
    {
        var missingVars = new List<string>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Critical variables
        if (string.IsNullOrWhiteSpace(connectionString))
            missingVars.Add("CONNECTION_STRING or DB_PASSWORD");

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                     ?? Environment.GetEnvironmentVariable("JwtSettings__Secret")
                     ?? builder.Configuration["JwtSettings:Secret"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
            missingVars.Add("JWT_SECRET");

        if (missingVars.Any())
        {
            throw new InvalidOperationException(
                $"Critical environment variables missing in Production: {string.Join(", ", missingVars)}");
        }

        // Validate JWT secret strength
        if (jwtSecret!.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters in Production");
        }

        // Set JWT configuration
        builder.Configuration["JwtSettings:Secret"] = jwtSecret;

        // Validate AI service configuration
        var claudeEnabled = builder.Configuration.GetValue<bool>("ClaudeAgents:Enabled", false);
        var claudeApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
                        ?? Environment.GetEnvironmentVariable("ClaudeAgents__ApiKey");

        if (claudeEnabled && string.IsNullOrWhiteSpace(claudeApiKey))
        {
            throw new InvalidOperationException(
                "CLAUDE_API_KEY required when ClaudeAgents:Enabled=true in Production");
        }

        // Configure Azure/Cloud services
        ConfigureCloudServices(builder);
    }

    private static void ConfigureCloudServices(WebApplicationBuilder builder)
    {
        // Azure Tenant ID (shared across services)
        var azureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
                         ?? Environment.GetEnvironmentVariable("Azure__TenantId")
                         ?? builder.Configuration["EmailOperations:MicrosoftGraph:TenantId"];

        if (!string.IsNullOrWhiteSpace(azureTenantId) && !azureTenantId.Contains("CHANGE_") && azureTenantId.Length > 10)
        {
            builder.Configuration["SmtpSettings:TenantId"] = azureTenantId;
            builder.Configuration["EmailOperations:MicrosoftGraph:TenantId"] = azureTenantId;
            builder.Configuration["CopilotAgent:TenantId"] = azureTenantId;
        }

        // SMTP Settings
        builder.Configuration["SmtpSettings:ClientId"] = Environment.GetEnvironmentVariable("SMTP_CLIENT_ID") ?? "";
        builder.Configuration["SmtpSettings:ClientSecret"] = Environment.GetEnvironmentVariable("SMTP_CLIENT_SECRET") ?? "";
        builder.Configuration["SmtpSettings:FromEmail"] = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";

        // Microsoft Graph API
        builder.Configuration["EmailOperations:MicrosoftGraph:ClientId"] = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_ID") ?? "";
        builder.Configuration["EmailOperations:MicrosoftGraph:ClientSecret"] = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_SECRET") ?? "";

        // Claude AI
        var claudeApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
                        ?? Environment.GetEnvironmentVariable("ClaudeAgents__ApiKey")
                        ?? builder.Configuration["ClaudeAgents:ApiKey"] ?? "";
        builder.Configuration["ClaudeAgents:ApiKey"] = claudeApiKey;
        builder.Configuration["ClaudeAgents:Model"] = Environment.GetEnvironmentVariable("CLAUDE_MODEL")
                                                   ?? Environment.GetEnvironmentVariable("ClaudeAgents__Model")
                                                   ?? "claude-sonnet-4-20250514";
        builder.Configuration["ClaudeAgents:Enabled"] = Environment.GetEnvironmentVariable("CLAUDE_ENABLED")
                                                      ?? Environment.GetEnvironmentVariable("ClaudeAgents__Enabled")
                                                      ?? "true";

        // Camunda BPM
        builder.Configuration["Camunda:BaseUrl"] = Environment.GetEnvironmentVariable("CAMUNDA_BASE_URL") ?? "http://localhost:8085/camunda";
        builder.Configuration["Camunda:Enabled"] = Environment.GetEnvironmentVariable("CAMUNDA_ENABLED") ?? "false";

        // Kafka
        builder.Configuration["Kafka:BootstrapServers"] = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
        builder.Configuration["Kafka:Enabled"] = Environment.GetEnvironmentVariable("KAFKA_ENABLED") ?? "false";
    }

    #endregion
}
