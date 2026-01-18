using GrcMvc.Common.Security;

namespace GrcMvc.Services.Security
{
    /// <summary>
    /// Service that audits security configuration and reports issues at startup.
    /// Implements IHostedService to run during application startup.
    /// </summary>
    public class SecurityAuditService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SecurityAuditService> _logger;

        public SecurityAuditService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<SecurityAuditService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔐 Starting Security Audit...");

            var issues = new List<SecurityIssue>();

            // Check connection strings
            CheckConnectionStrings(issues);

            // Check JWT settings
            CheckJwtSettings(issues);

            // Check SMTP settings
            CheckSmtpSettings(issues);

            // Check API keys
            CheckApiKeys(issues);

            // Check CORS settings
            CheckCorsSettings(issues);

            // Check security settings
            CheckSecuritySettings(issues);

            // Report findings
            ReportFindings(issues);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void CheckConnectionStrings(List<SecurityIssue> issues)
        {
            var connectionStrings = new[] { "DefaultConnection", "GrcAuthDb" };

            foreach (var name in connectionStrings)
            {
                var connectionString = _configuration.GetConnectionString(name);
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    issues.Add(new SecurityIssue
                    {
                        Severity = SecuritySeverity.Critical,
                        Category = "Configuration",
                        Message = $"Connection string '{name}' is not configured",
                        Recommendation = $"Set {name} in appsettings.json or use environment variable"
                    });
                    continue;
                }

                // Check for hardcoded passwords
                if (connectionString.Contains("Password=") && 
                    !connectionString.Contains("${") &&
                    !connectionString.Contains("Password=;") &&
                    !connectionString.Contains("Password=''"))
                {
                    if (_environment.IsProduction())
                    {
                        issues.Add(new SecurityIssue
                        {
                            Severity = SecuritySeverity.Critical,
                            Category = "Credentials",
                            Message = $"Connection string '{name}' contains hardcoded password in production",
                            Recommendation = "Use environment variables or Azure Key Vault for database credentials"
                        });
                    }
                    else
                    {
                        issues.Add(new SecurityIssue
                        {
                            Severity = SecuritySeverity.Warning,
                            Category = "Credentials",
                            Message = $"Connection string '{name}' contains hardcoded password",
                            Recommendation = "Consider using user secrets for development"
                        });
                    }
                }
            }
        }

        private void CheckJwtSettings(List<SecurityIssue> issues)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.Critical,
                    Category = "Authentication",
                    Message = "JWT secret is not configured",
                    Recommendation = "Set JwtSettings:Secret in configuration"
                });
                return;
            }

            if (SecureConfigurationHelper.IsPlaceholder(jwtSecret))
            {
                if (_environment.IsProduction())
                {
                    issues.Add(new SecurityIssue
                    {
                        Severity = SecuritySeverity.Critical,
                        Category = "Authentication",
                        Message = "JWT secret is using a placeholder value in production",
                        Recommendation = "Set a strong, unique JWT secret using environment variables"
                    });
                }
                else
                {
                    issues.Add(new SecurityIssue
                    {
                        Severity = SecuritySeverity.Warning,
                        Category = "Authentication",
                        Message = "JWT secret is using a placeholder value",
                        Recommendation = "Set a proper JWT secret for testing"
                    });
                }
            }

            if (jwtSecret.Length < 32)
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.High,
                    Category = "Authentication",
                    Message = "JWT secret is too short (less than 32 characters)",
                    Recommendation = "Use a JWT secret of at least 32 characters"
                });
            }
        }

        private void CheckSmtpSettings(List<SecurityIssue> issues)
        {
            var smtpPassword = _configuration["SmtpSettings:Password"];
            var emailPassword = _configuration["EmailSettings:Password"];

            if (!string.IsNullOrEmpty(smtpPassword) && 
                !SecureConfigurationHelper.IsPlaceholder(smtpPassword) &&
                _environment.IsProduction())
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.Warning,
                    Category = "Credentials",
                    Message = "SMTP password is configured in settings file",
                    Recommendation = "Use environment variables or OAuth2 for email authentication"
                });
            }
        }

        private void CheckApiKeys(List<SecurityIssue> issues)
        {
            var apiKeySettings = new Dictionary<string, string>
            {
                { "ClaudeAgents:ApiKey", "Claude AI" },
                { "CopilotAgent:ClientSecret", "Microsoft Copilot" },
                { "Security:Captcha:SecretKey", "reCAPTCHA" },
                { "EmailOperations:MicrosoftGraph:ClientSecret", "Microsoft Graph" }
            };

            foreach (var (key, name) in apiKeySettings)
            {
                var value = _configuration[key];
                
                if (!string.IsNullOrEmpty(value) && 
                    !SecureConfigurationHelper.IsPlaceholder(value) &&
                    value.Length > 10)
                {
                    if (_environment.IsProduction())
                    {
                        issues.Add(new SecurityIssue
                        {
                            Severity = SecuritySeverity.Warning,
                            Category = "API Keys",
                            Message = $"{name} API key/secret may be hardcoded in configuration",
                            Recommendation = $"Use environment variables for {name} credentials"
                        });
                    }
                }
            }
        }

        private void CheckCorsSettings(List<SecurityIssue> issues)
        {
            var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            if (allowedOrigins != null)
            {
                if (allowedOrigins.Any(o => o == "*"))
                {
                    issues.Add(new SecurityIssue
                    {
                        Severity = SecuritySeverity.High,
                        Category = "CORS",
                        Message = "CORS is configured to allow all origins (*)",
                        Recommendation = "Specify explicit allowed origins in production"
                    });
                }

                if (_environment.IsProduction() && 
                    allowedOrigins.Any(o => o.Contains("localhost")))
                {
                    issues.Add(new SecurityIssue
                    {
                        Severity = SecuritySeverity.Warning,
                        Category = "CORS",
                        Message = "Localhost is allowed in CORS settings in production",
                        Recommendation = "Remove localhost from CORS allowed origins in production"
                    });
                }
            }
        }

        private void CheckSecuritySettings(List<SecurityIssue> issues)
        {
            // Check if CAPTCHA is enabled in production
            var captchaEnabled = _configuration.GetValue<bool>("Security:Captcha:Enabled");
            if (_environment.IsProduction() && !captchaEnabled)
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.Warning,
                    Category = "Security",
                    Message = "CAPTCHA is disabled in production",
                    Recommendation = "Enable CAPTCHA for public-facing forms"
                });
            }

            // Check rate limiting
            var rateLimitingEnabled = _configuration.GetValue<bool>("RateLimiting:Enabled");
            if (_environment.IsProduction() && !rateLimitingEnabled)
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.High,
                    Category = "Security",
                    Message = "Rate limiting is disabled in production",
                    Recommendation = "Enable rate limiting to prevent abuse"
                });
            }

            // Check fraud detection
            var fraudDetectionEnabled = _configuration.GetValue<bool>("FraudDetection:Enabled");
            if (_environment.IsProduction() && !fraudDetectionEnabled)
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.Warning,
                    Category = "Security",
                    Message = "Fraud detection is disabled in production",
                    Recommendation = "Enable fraud detection for enhanced security"
                });
            }

            // Check trial signup security
            var trialSignupEnabled = _configuration.GetValue<bool>("GrcFeatureFlags:EnableTrialSignup");
            var requirePaymentForTrial = _configuration.GetValue<bool>("GrcFeatureFlags:RequirePaymentVerificationForTrial");
            if (_environment.IsProduction() && trialSignupEnabled && !requirePaymentForTrial)
            {
                issues.Add(new SecurityIssue
                {
                    Severity = SecuritySeverity.Warning,
                    Category = "Security",
                    Message = "Trial signup enabled without payment verification",
                    Recommendation = "Consider enabling RequirePaymentVerificationForTrial for production"
                });
            }
        }

        private void ReportFindings(List<SecurityIssue> issues)
        {
            if (!issues.Any())
            {
                _logger.LogInformation("✅ Security Audit Complete - No issues found");
                return;
            }

            var criticalCount = issues.Count(i => i.Severity == SecuritySeverity.Critical);
            var highCount = issues.Count(i => i.Severity == SecuritySeverity.High);
            var warningCount = issues.Count(i => i.Severity == SecuritySeverity.Warning);

            _logger.LogWarning(
                "🔐 Security Audit Complete - Found {Total} issues: {Critical} Critical, {High} High, {Warning} Warning",
                issues.Count, criticalCount, highCount, warningCount);

            foreach (var issue in issues.OrderByDescending(i => i.Severity))
            {
                var logLevel = issue.Severity switch
                {
                    SecuritySeverity.Critical => LogLevel.Critical,
                    SecuritySeverity.High => LogLevel.Error,
                    SecuritySeverity.Warning => LogLevel.Warning,
                    _ => LogLevel.Information
                };

                var emoji = issue.Severity switch
                {
                    SecuritySeverity.Critical => "🚨",
                    SecuritySeverity.High => "⚠️",
                    SecuritySeverity.Warning => "⚡",
                    _ => "ℹ️"
                };

                _logger.Log(logLevel,
                    "{Emoji} [{Severity}] {Category}: {Message} → {Recommendation}",
                    emoji, issue.Severity, issue.Category, issue.Message, issue.Recommendation);
            }

            // In production, throw if critical issues found
            if (_environment.IsProduction() && criticalCount > 0)
            {
                _logger.LogCritical(
                    "🛑 STARTUP BLOCKED: {Count} critical security issues must be resolved before production deployment",
                    criticalCount);
                
                // Uncomment to block startup on critical issues:
                // throw new InvalidOperationException($"Critical security issues detected: {criticalCount}");
            }
        }
    }

    public class SecurityIssue
    {
        public SecuritySeverity Severity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public enum SecuritySeverity
    {
        Info,
        Warning,
        High,
        Critical
    }
}
