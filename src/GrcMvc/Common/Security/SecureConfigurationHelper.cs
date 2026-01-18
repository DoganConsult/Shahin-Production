using Microsoft.Extensions.Configuration;

namespace GrcMvc.Common.Security
{
    /// <summary>
    /// Helper class for secure configuration management.
    /// Supports environment variables, user secrets, and Azure Key Vault.
    /// </summary>
    public static class SecureConfigurationHelper
    {
        /// <summary>
        /// Gets a configuration value with environment variable fallback.
        /// Format: ${ENV_VAR_NAME:default_value}
        /// </summary>
        public static string GetSecureValue(this IConfiguration configuration, string key, string? defaultValue = null)
        {
            var value = configuration[key];
            
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue ?? string.Empty;
            }

            // Check for environment variable placeholder format: ${ENV_VAR:default}
            if (value.StartsWith("${") && value.Contains("}"))
            {
                return ResolveEnvironmentVariable(value, defaultValue);
            }

            return value;
        }

        /// <summary>
        /// Gets a connection string with environment variable support.
        /// </summary>
        public static string GetSecureConnectionString(this IConfiguration configuration, string name)
        {
            var connectionString = configuration.GetConnectionString(name);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{name}' not found in configuration.");
            }

            // Check for environment variable placeholder
            if (connectionString.StartsWith("${") && connectionString.Contains("}"))
            {
                return ResolveEnvironmentVariable(connectionString, null) 
                    ?? throw new InvalidOperationException($"Environment variable for connection string '{name}' not set.");
            }

            return connectionString;
        }

        /// <summary>
        /// Resolves environment variable placeholder format: ${ENV_VAR:default_value}
        /// </summary>
        private static string ResolveEnvironmentVariable(string placeholder, string? fallbackDefault)
        {
            // Extract content between ${ and }
            var content = placeholder.TrimStart('$', '{').TrimEnd('}');
            
            // Split by : to get env var name and default value
            var parts = content.Split(':', 2);
            var envVarName = parts[0];
            var defaultValue = parts.Length > 1 ? parts[1] : fallbackDefault;

            // Get environment variable value
            var envValue = Environment.GetEnvironmentVariable(envVarName);

            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Return default value if environment variable not set
            return defaultValue ?? string.Empty;
        }

        /// <summary>
        /// Validates that required configuration keys are present.
        /// </summary>
        public static void ValidateRequiredConfiguration(this IConfiguration configuration, params string[] requiredKeys)
        {
            var missingKeys = new List<string>();

            foreach (var key in requiredKeys)
            {
                var value = configuration.GetSecureValue(key);
                if (string.IsNullOrEmpty(value) || value.Contains("CHANGE_THIS"))
                {
                    missingKeys.Add(key);
                }
            }

            if (missingKeys.Any())
            {
                throw new InvalidOperationException(
                    $"Required configuration keys are missing or not properly set: {string.Join(", ", missingKeys)}");
            }
        }

        /// <summary>
        /// Checks if a configuration value contains a placeholder that needs to be set.
        /// </summary>
        public static bool IsPlaceholder(string? value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            var placeholderIndicators = new[]
            {
                "CHANGE_THIS",
                "CHANGE_ME",
                "YOUR_",
                "TODO",
                "PLACEHOLDER",
                "${",
                "<YOUR_"
            };

            return placeholderIndicators.Any(p => value.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets sensitive configuration and logs a warning if it's a placeholder.
        /// </summary>
        public static string GetSensitiveValue(
            this IConfiguration configuration, 
            string key, 
            ILogger? logger = null,
            string? defaultValue = null)
        {
            var value = configuration.GetSecureValue(key, defaultValue);

            if (IsPlaceholder(value))
            {
                logger?.LogWarning(
                    "Configuration key '{Key}' appears to be a placeholder. " +
                    "Please set this value using environment variables or user secrets in production.",
                    key);
            }

            return value;
        }
    }

    /// <summary>
    /// Extension methods for IServiceCollection to add secure configuration.
    /// </summary>
    public static class SecureConfigurationExtensions
    {
        /// <summary>
        /// Adds environment variable configuration with prefix support.
        /// </summary>
        public static IConfigurationBuilder AddSecureEnvironmentVariables(
            this IConfigurationBuilder builder, 
            string prefix = "SHAHIN_")
        {
            return builder.AddEnvironmentVariables(prefix);
        }

        /// <summary>
        /// Validates production configuration on startup.
        /// </summary>
        public static void ValidateProductionConfiguration(
            this IConfiguration configuration, 
            IWebHostEnvironment environment,
            ILogger logger)
        {
            if (!environment.IsProduction()) return;

            var criticalKeys = new[]
            {
                "ConnectionStrings:DefaultConnection",
                "JwtSettings:Secret"
            };

            foreach (var key in criticalKeys)
            {
                var value = configuration[key];
                if (SecureConfigurationHelper.IsPlaceholder(value))
                {
                    logger.LogCritical(
                        "SECURITY WARNING: Production configuration key '{Key}' is not properly configured!",
                        key);
                }
            }
        }
    }
}
