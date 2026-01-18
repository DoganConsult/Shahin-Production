using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.StartupValidators
{
    /// <summary>
    /// Validates onboarding services configuration at startup
    /// Ensures all required files and configurations are present before production use
    /// </summary>
    public class OnboardingServicesStartupValidator : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingServicesStartupValidator> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OnboardingServicesStartupValidator(
            IConfiguration configuration,
            ILogger<OnboardingServicesStartupValidator> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔍 Starting Onboarding Services validation...");

            // #region agent log - H3: Onboarding validator entry
            var logPath = @"c:\Shahin-ai\.cursor\debug.log";
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H3",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "OnboardingServicesStartupValidator.cs:34",
                    message = "Onboarding Services validation starting",
                    data = new { validating = true }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            var validationErrors = new System.Collections.Generic.List<string>();
            var validationWarnings = new System.Collections.Generic.List<string>();

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var coverageService = scope.ServiceProvider.GetService<IOnboardingCoverageService>();
                var registryService = scope.ServiceProvider.GetService<IFieldRegistryService>();
                
                // #region agent log - H3: Check DI registration
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "startup-run-1",
                        hypothesisId = "H3",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "OnboardingServicesStartupValidator.cs:44",
                        message = "Checked DI service registration",
                        data = new
                        {
                            coverageServiceRegistered = coverageService != null,
                            registryServiceRegistered = registryService != null
                        }
                    });
                    System.IO.File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion
                
                // 1. Validate coverage manifest path exists
                await ValidateCoverageManifestAsync(validationErrors, validationWarnings, cancellationToken);

                // 2. Validate coverage service can load manifest
                await ValidateCoverageServiceAsync(coverageService, validationErrors, validationWarnings, cancellationToken);

                // 3. Validate field registry service can load registry
                await ValidateFieldRegistryServiceAsync(registryService, validationErrors, validationWarnings, cancellationToken);

                // 4. Validate configuration settings
                ValidateConfigurationSettings(validationErrors, validationWarnings);

                // Report results
                if (validationErrors.Count > 0)
                {
                    _logger.LogError("❌ Onboarding Services validation FAILED with {ErrorCount} errors:", validationErrors.Count);
                    foreach (var error in validationErrors)
                    {
                        _logger.LogError("  - {Error}", error);
                    }
                    
                    // In production, you might want to throw to prevent startup
                    // throw new InvalidOperationException($"Onboarding Services validation failed: {string.Join("; ", validationErrors)}");
                }
                else
                {
                    _logger.LogInformation("✅ Onboarding Services validation PASSED");
                }

                if (validationWarnings.Count > 0)
                {
                    _logger.LogWarning("⚠️ Onboarding Services validation has {WarningCount} warnings:", validationWarnings.Count);
                    foreach (var warning in validationWarnings)
                    {
                        _logger.LogWarning("  - {Warning}", warning);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Onboarding Services validation threw an exception");
                // Don't throw to allow application to start, but log the issue
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ValidateCoverageManifestAsync(
            System.Collections.Generic.List<string> errors,
            System.Collections.Generic.List<string> warnings,
            CancellationToken ct)
        {
            var manifestPath = _configuration["Onboarding:CoverageManifestPath"] 
                ?? "etc/onboarding/coverage-manifest.yml";

            // Resolve full path
            if (!Path.IsPathRooted(manifestPath))
            {
                var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (basePath != null)
                {
                    var projectRoot = Path.GetFullPath(Path.Combine(basePath, "../../../.."));
                    manifestPath = Path.Combine(projectRoot, manifestPath);
                }
            }

            // #region agent log - H7: Manifest file check
            var logPath = @"c:\Shahin-ai\.cursor\debug.log";
            try
            {
                var fileExists = File.Exists(manifestPath);
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H7",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "OnboardingServicesStartupValidator.cs:115",
                    message = "Coverage manifest file existence check",
                    data = new
                    {
                        expectedPath = manifestPath,
                        fileExists,
                        resolvedPath = Path.IsPathRooted(manifestPath)
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            if (!File.Exists(manifestPath))
            {
                errors.Add($"Coverage manifest file not found at: {manifestPath}");
            }
            else
            {
                _logger.LogInformation("✓ Coverage manifest file found at: {Path}", manifestPath);

                // Check file size (should not be empty)
                var fileInfo = new FileInfo(manifestPath);
                if (fileInfo.Length == 0)
                {
                    errors.Add($"Coverage manifest file is empty: {manifestPath}");
                }
            }
        }

        private async Task ValidateCoverageServiceAsync(
            IOnboardingCoverageService? coverageService,
            System.Collections.Generic.List<string> errors,
            System.Collections.Generic.List<string> warnings,
            CancellationToken ct)
        {
            if (coverageService == null)
            {
                errors.Add("IOnboardingCoverageService is not registered in DI container");
                return;
            }
            
            try
            {
                var manifest = await coverageService.LoadManifestAsync(ct);

                if (manifest == null)
                {
                    errors.Add("Coverage service returned null manifest");
                    return;
                }

                if (string.IsNullOrEmpty(manifest.Version))
                {
                    warnings.Add("Coverage manifest version is empty");
                }

                if (manifest.RequiredIdsByNode == null || manifest.RequiredIdsByNode.Count == 0)
                {
                    errors.Add("Coverage manifest has no required IDs by node");
                }
                else
                {
                    _logger.LogInformation("✓ Coverage manifest loaded with {NodeCount} nodes", manifest.RequiredIdsByNode.Count);
                }

                if (manifest.RequiredIdsByMission == null || manifest.RequiredIdsByMission.Count == 0)
                {
                    warnings.Add("Coverage manifest has no required IDs by mission");
                }
                else
                {
                    _logger.LogInformation("✓ Coverage manifest loaded with {MissionCount} missions", manifest.RequiredIdsByMission.Count);
                }

                if (manifest.ConditionalRequired == null || manifest.ConditionalRequired.Count == 0)
                {
                    warnings.Add("Coverage manifest has no conditional required rules");
                }

                if (manifest.IntegrityChecks == null || manifest.IntegrityChecks.Count == 0)
                {
                    warnings.Add("Coverage manifest has no integrity checks defined");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load coverage manifest: {ex.Message}");
                _logger.LogError(ex, "Error loading coverage manifest during validation");
            }
        }

        private async Task ValidateFieldRegistryServiceAsync(
            IFieldRegistryService? registryService,
            System.Collections.Generic.List<string> errors,
            System.Collections.Generic.List<string> warnings,
            CancellationToken ct)
        {
            if (registryService == null)
            {
                errors.Add("IFieldRegistryService is not registered in DI container");
                return;
            }
            
            try
            {
                var registry = await registryService.LoadRegistryAsync(ct);

                if (registry == null)
                {
                    errors.Add("Field registry service returned null registry");
                    return;
                }

                var allFields = await registryService.GetAllFieldsAsync(ct);
                if (allFields == null || allFields.Count == 0)
                {
                    warnings.Add("Field registry is empty - no fields registered");
                }
                else
                {
                    _logger.LogInformation("✓ Field registry loaded with {FieldCount} fields", allFields.Count);
                }

                // Test validation with a known field
                var testFieldId = "SF.S1.organization_name";
                var isValid = await registryService.ValidateFieldIdAsync(testFieldId, ct);
                if (!isValid)
                {
                    warnings.Add($"Test field '{testFieldId}' not found in registry - registry may be incomplete");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load field registry: {ex.Message}");
                _logger.LogError(ex, "Error loading field registry during validation");
            }
        }

        private void ValidateConfigurationSettings(
            System.Collections.Generic.List<string> errors,
            System.Collections.Generic.List<string> warnings)
        {
            var enableCoverageValidation = _configuration.GetValue<bool>("Onboarding:EnableCoverageValidation", true);
            var enableFieldRegistryValidation = _configuration.GetValue<bool>("Onboarding:EnableFieldRegistryValidation", true);
            var cacheManifestMinutes = _configuration.GetValue<int>("Onboarding:CacheManifestMinutes", 60);

            if (cacheManifestMinutes < 0)
            {
                warnings.Add("Onboarding:CacheManifestMinutes is negative - using default value");
            }

            _logger.LogInformation("✓ Onboarding configuration validated (CoverageValidation: {Coverage}, RegistryValidation: {Registry}, CacheMinutes: {Cache})",
                enableCoverageValidation, enableFieldRegistryValidation, cacheManifestMinutes);
        }
    }
}
