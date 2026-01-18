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
        // Priority order for connection strings:
        // 1. ABP Settings (database - encrypted) - checked via IConfiguration after ABP is configured
        // 2. Environment variables (Railway DB, Docker, etc.)
        // 3. Configuration files (appsettings.json)
        
        Console.WriteLine("[CONFIG] ========================================");
        Console.WriteLine("[CONFIG] Resolving Connection Strings");
        Console.WriteLine("[CONFIG] ========================================");
        
        // Debug: Log all environment variables related to connections
        LogEnvironmentVariables(new[] { 
            "ConnectionStrings__DefaultConnection", 
            "CONNECTION_STRING",
            "ConnectionStrings__GrcAuthDb",
            "AUTH_CONNECTION_STRING"
        });

        // Read connection string with fallback chain
        var connectionString = GetConnectionStringWithFallback(
            builder, 
            "DefaultConnection",
            new[] { "ConnectionStrings__DefaultConnection", "CONNECTION_STRING" }
        );

        // Support Railway DB format (DATABASE_URL)
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var railwayUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(railwayUrl))
            {
                // Convert Railway format: postgresql://user:pass@host:port/dbname
                // To: Host=host;Database=dbname;Username=user;Password=pass;Port=port
                try
                {
                    var uri = new Uri(railwayUrl);
                    var userInfo = uri.UserInfo.Split(':');
                    if (userInfo.Length == 2)
                    {
                        connectionString = 
                            $"Host={uri.Host};Database={uri.LocalPath.TrimStart('/')};" +
                            $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                            $"Password={Uri.UnescapeDataString(userInfo[1])};Port={uri.Port}";
                        
                        Console.WriteLine("[CONFIG] ✅ Converted Railway DATABASE_URL to connection string");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CONFIG] ⚠️  Failed to parse DATABASE_URL: {ex.Message}");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var errorMessage = 
                "Database connection string 'DefaultConnection' is required.\n" +
                "Set one of the following:\n" +
                "  - Environment variable: ConnectionStrings__DefaultConnection\n" +
                "  - Environment variable: CONNECTION_STRING\n" +
                "  - Environment variable: DATABASE_URL (Railway format)\n" +
                "  - Configuration file: appsettings.json (ConnectionStrings.DefaultConnection)\n" +
                "  - ABP Setting: GrcMvc.DefaultConnection (after ABP initialization)";
            
            Console.WriteLine($"[CONFIG] ❌ FATAL: {errorMessage}");
            throw new InvalidOperationException(errorMessage);
        }

        // Validate connection string format
        try
        {
            var csb = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(csb.Host))
            {
                throw new InvalidOperationException("Connection string missing Host");
            }
            if (string.IsNullOrWhiteSpace(csb.Database))
            {
                throw new InvalidOperationException("Connection string missing Database");
            }
            Console.WriteLine("[CONFIG] ✅ Connection string format validated");
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Invalid connection string format: {ex.Message}", ex);
        }

        var source = GetConnectionStringSource(builder, connectionString);
        Console.WriteLine($"[CONFIG] ✅ Using database connection from: {source}");
        Console.WriteLine($"[CONFIG] 📊 Database: {GetDatabaseInfo(connectionString)}");
        Console.WriteLine("[CONFIG] 🔄 Setting in IConfiguration for all layers to use");

        // Set all possible connection string keys for compatibility
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        builder.Configuration["ConnectionStrings:Default"] = connectionString;
        
        Console.WriteLine("[CONFIG] ✅ Connection string set in IConfiguration");
        Console.WriteLine("[CONFIG] 📍 All layers will read from: ConnectionStrings:DefaultConnection");

        // Resolve auth database connection string
        Console.WriteLine("[CONFIG] 🔍 Resolving auth database connection...");
        var authConnectionString = GetConnectionStringWithFallback(
            builder,
            "GrcAuthDb",
            new[] { "ConnectionStrings__GrcAuthDb", "AUTH_CONNECTION_STRING" }
        );

        // If auth connection string not explicitly set, derive it from main connection string
        if (string.IsNullOrWhiteSpace(authConnectionString) && !string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                var csb = new NpgsqlConnectionStringBuilder(connectionString);
                csb.Database = $"{csb.Database}_auth";
                authConnectionString = csb.ConnectionString;
                Console.WriteLine("[CONFIG] ✅ Derived auth database connection from main connection string");
                Console.WriteLine($"[CONFIG] 📊 Auth Database: {GetDatabaseInfo(authConnectionString)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONFIG] ⚠️  WARNING: Failed to derive auth connection string: {ex.Message}");
                authConnectionString = connectionString;
            }
        }

        if (!string.IsNullOrWhiteSpace(authConnectionString))
        {
            builder.Configuration["ConnectionStrings:GrcAuthDb"] = authConnectionString;
            Console.WriteLine("[CONFIG] ✅ Auth connection string set in IConfiguration");
        }
        
        Console.WriteLine("[CONFIG] ========================================");
        Console.WriteLine("[CONFIG] Connection String Resolution Complete");
        Console.WriteLine("[CONFIG] ========================================");
    }

    private static void LogEnvironmentVariables(string[] variableNames)
    {
        Console.WriteLine("[CONFIG] 🔍 Checking environment variables:");
        foreach (var varName in variableNames)
        {
            var value = Environment.GetEnvironmentVariable(varName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Mask sensitive parts
                var masked = MaskSensitiveValue(varName, value);
                Console.WriteLine($"[CONFIG]   ✅ {varName} = {masked}");
            }
            else
            {
                Console.WriteLine($"[CONFIG]   ❌ {varName} = (not set)");
            }
        }
    }

    private static string MaskSensitiveValue(string varName, string value)
    {
        // Mask connection strings and secrets
        if (varName.Contains("Connection") || varName.Contains("Secret") || varName.Contains("Password") || varName.Contains("Key"))
        {
            try
            {
                if (value.Contains("Host="))
                {
                    // Connection string format
                    var parts = value.Split(';');
                    var masked = new List<string>();
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                            part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                        {
                            masked.Add(part.Split('=')[0] + "=***");
                        }
                        else
                        {
                            masked.Add(part);
                        }
                    }
                    return string.Join(";", masked);
                }
                else
                {
                    // Simple secret value
                    return value.Length > 10 ? $"{value.Substring(0, 4)}...{value.Substring(value.Length - 4)}" : "***";
                }
            }
            catch
            {
                return "***";
            }
        }
        return value;
    }

    private static string? GetConnectionStringWithFallback(
        WebApplicationBuilder builder, 
        string configKey, 
        string[] envVarNames)
    {
        Console.WriteLine($"[CONFIG] 🔍 Resolving connection string: {configKey}");
        
        // 1. Try environment variables first (Railway DB, Docker, etc.)
        foreach (var envVarName in envVarNames)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                Console.WriteLine($"[CONFIG]   ✅ Found in environment variable: {envVarName}");
                return envValue;
            }
            else
            {
                Console.WriteLine($"[CONFIG]   ❌ Not found: {envVarName}");
            }
        }

        // 2. Try configuration (may include ABP Settings after ABP is initialized)
        var configValue = builder.Configuration.GetConnectionString(configKey);
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            Console.WriteLine($"[CONFIG]   ✅ Found in IConfiguration: ConnectionStrings:{configKey}");
            return configValue;
        }
        
        configValue = builder.Configuration[$"ConnectionStrings:{configKey}"];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            Console.WriteLine($"[CONFIG]   ✅ Found in IConfiguration: ConnectionStrings:{configKey} (alternative path)");
            return configValue;
        }

        Console.WriteLine($"[CONFIG]   ❌ Not found in IConfiguration: ConnectionStrings:{configKey}");
        return null;
    }

    private static string GetConnectionStringSource(WebApplicationBuilder builder, string connectionString)
    {
        // Check which source provided the connection string
        if (Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") == connectionString ||
            Environment.GetEnvironmentVariable("CONNECTION_STRING") == connectionString)
        {
            return "Environment Variable (Railway/Docker)";
        }

        if (builder.Configuration.GetConnectionString("DefaultConnection") == connectionString)
        {
            // Could be from ABP Settings or appsettings.json
            // ABP Settings are loaded into IConfiguration after ABP initialization
            return "Configuration (ABP Settings or appsettings.json)";
        }

        return "Unknown Source";
    }

    private static string GetDatabaseInfo(string connectionString)
    {
        try
        {
            var csb = new NpgsqlConnectionStringBuilder(connectionString);
            return $"{csb.Host}:{csb.Port} / {csb.Username}@{csb.Database}";
        }
        catch
        {
            return "[connection string format]";
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
        Console.WriteLine("[CONFIG] ========================================");
        Console.WriteLine("[CONFIG] Configuring Cloud Services");
        Console.WriteLine("[CONFIG] ========================================");
        
        // Azure Tenant ID (shared across services)
        var azureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
                         ?? Environment.GetEnvironmentVariable("Azure__TenantId")
                         ?? builder.Configuration["EmailOperations:MicrosoftGraph:TenantId"];

        if (!string.IsNullOrWhiteSpace(azureTenantId) && !azureTenantId.Contains("CHANGE_") && azureTenantId.Length > 10)
        {
            builder.Configuration["SmtpSettings:TenantId"] = azureTenantId;
            builder.Configuration["EmailOperations:MicrosoftGraph:TenantId"] = azureTenantId;
            builder.Configuration["CopilotAgent:TenantId"] = azureTenantId;
            Console.WriteLine("[CONFIG] ✅ Azure Tenant ID configured");
        }
        else
        {
            Console.WriteLine("[CONFIG] ⚠️  Azure Tenant ID not set or invalid");
        }

        // SMTP Settings
        var smtpClientId = Environment.GetEnvironmentVariable("SMTP_CLIENT_ID") ?? "";
        var smtpClientSecret = Environment.GetEnvironmentVariable("SMTP_CLIENT_SECRET") ?? "";
        var smtpFromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";
        
        builder.Configuration["SmtpSettings:ClientId"] = smtpClientId;
        builder.Configuration["SmtpSettings:ClientSecret"] = smtpClientSecret;
        builder.Configuration["SmtpSettings:FromEmail"] = smtpFromEmail;
        
        Console.WriteLine($"[CONFIG] 📧 SMTP: ClientId={(!string.IsNullOrEmpty(smtpClientId) ? "SET" : "NOT SET")}, FromEmail={smtpFromEmail}");

        // Microsoft Graph API
        var msGraphClientId = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_ID") ?? "";
        var msGraphClientSecret = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_SECRET") ?? "";
        
        builder.Configuration["EmailOperations:MicrosoftGraph:ClientId"] = msGraphClientId;
        builder.Configuration["EmailOperations:MicrosoftGraph:ClientSecret"] = msGraphClientSecret;
        
        Console.WriteLine($"[CONFIG] 📊 MS Graph: ClientId={(!string.IsNullOrEmpty(msGraphClientId) ? "SET" : "NOT SET")}");

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
        
        Console.WriteLine($"[CONFIG] 🤖 Claude AI: ApiKey={(!string.IsNullOrEmpty(claudeApiKey) ? "SET" : "NOT SET")}, Model={builder.Configuration["ClaudeAgents:Model"]}, Enabled={builder.Configuration["ClaudeAgents:Enabled"]}");

        // Camunda BPM
        var camundaBaseUrl = Environment.GetEnvironmentVariable("CAMUNDA_BASE_URL") ?? "http://localhost:8085/camunda";
        var camundaEnabled = Environment.GetEnvironmentVariable("CAMUNDA_ENABLED") ?? "false";
        builder.Configuration["Camunda:BaseUrl"] = camundaBaseUrl;
        builder.Configuration["Camunda:Enabled"] = camundaEnabled;
        
        Console.WriteLine($"[CONFIG] 🔄 Camunda: BaseUrl={camundaBaseUrl}, Enabled={camundaEnabled}");

        // Kafka
        var kafkaBootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
        var kafkaEnabled = Environment.GetEnvironmentVariable("KAFKA_ENABLED") ?? "false";
        builder.Configuration["Kafka:BootstrapServers"] = kafkaBootstrapServers;
        builder.Configuration["Kafka:Enabled"] = kafkaEnabled;
        
        Console.WriteLine($"[CONFIG] 📨 Kafka: BootstrapServers={kafkaBootstrapServers}, Enabled={kafkaEnabled}");
        Console.WriteLine("[CONFIG] ========================================");
    }

    #endregion
}
