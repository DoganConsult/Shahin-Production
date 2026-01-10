using Microsoft.Extensions.Options;

namespace GrcMvc.Configuration;

/// <summary>
/// Configuration Validator - Validates required configuration at startup
/// Prevents application from starting with invalid or missing critical configuration
/// </summary>
public class ConfigurationValidator : IHostedService
{
    private readonly ILogger<ConfigurationValidator> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOptions<ClaudeApiSettings> _claudeSettings;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public ConfigurationValidator(
        ILogger<ConfigurationValidator> logger,
        IConfiguration configuration,
        IOptions<ClaudeApiSettings> claudeSettings,
        IOptions<JwtSettings> jwtSettings)
    {
        _logger = logger;
        _configuration = configuration;
        _claudeSettings = claudeSettings;
        _jwtSettings = jwtSettings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating application configuration...");

        var errors = new List<string>();

        // Validate Database Connection
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Database connection string (ConnectionStrings:DefaultConnection) is not configured");
        }
        else
        {
            _logger.LogInformation("✓ Database connection string configured");
        }

        // Validate JWT Settings
        var jwtSecret = _jwtSettings.Value.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            errors.Add("JWT Secret (JwtSettings:Secret) is not configured");
        }
        else if (jwtSecret.Length < 32)
        {
            errors.Add($"JWT Secret is too short ({jwtSecret.Length} characters). Minimum 32 characters required for security.");
        }
        else
        {
            _logger.LogInformation("✓ JWT settings configured");
        }

        // Validate Claude API (Optional but recommended)
        var claudeApiKey = _claudeSettings.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(claudeApiKey))
        {
            _logger.LogWarning("⚠ Claude API key is not configured. AI features will be disabled.");
            _logger.LogWarning("  To enable AI features:");
            _logger.LogWarning("  1. Get API key from https://console.anthropic.com/");
            _logger.LogWarning("  2. Set CLAUDE_API_KEY in .env file or environment variables");
        }
        else if (!claudeApiKey.StartsWith("sk-ant-"))
        {
            _logger.LogWarning("⚠ Claude API key format appears incorrect (should start with 'sk-ant-')");
        }
        else
        {
            _logger.LogInformation("✓ Claude API key configured");
        }

        // Validate CORS Origins
        var corsOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (corsOrigins == null || corsOrigins.Length == 0)
        {
            _logger.LogWarning("⚠ No CORS origins configured. API may not be accessible from frontend.");
        }
        else
        {
            _logger.LogInformation($"✓ CORS configured with {corsOrigins.Length} allowed origin(s)");
        }

        // Validate SMTP Settings (Optional)
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("⚠ SMTP settings not configured. Email features will be disabled.");
        }
        else
        {
            _logger.LogInformation("✓ SMTP settings configured");
        }

        // Validate Hangfire Connection (Background Jobs)
        var hangfireConnection = _configuration.GetConnectionString("HangfireConnection");
        if (string.IsNullOrWhiteSpace(hangfireConnection))
        {
            _logger.LogWarning("⚠ Hangfire connection not configured. Using default connection string.");
        }
        else
        {
            _logger.LogInformation("✓ Hangfire connection configured");
        }

        // Validate Data Protection Keys Path (Production)
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
        if (environment == "Production")
        {
            var keysPath = _configuration["DataProtection:KeysPath"];
            if (string.IsNullOrWhiteSpace(keysPath))
            {
                _logger.LogWarning("⚠ Data Protection keys path not configured for production. Keys will be stored in default location.");
                _logger.LogWarning("  Recommended: Set DataProtection:KeysPath for production environments.");
            }
        }

        // If there are critical errors, prevent startup
        if (errors.Count > 0)
        {
            _logger.LogCritical("Configuration validation failed with {Count} error(s):", errors.Count);
            foreach (var error in errors)
            {
                _logger.LogCritical("  ✗ {Error}", error);
            }

            throw new InvalidOperationException(
                $"Application configuration is invalid. {errors.Count} error(s) found. " +
                "Please check appsettings.json and environment variables. See logs for details.");
        }

        _logger.LogInformation("✓ Configuration validation completed successfully");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
