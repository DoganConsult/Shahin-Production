using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MsHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace GrcMvc.HealthChecks
{
    /// <summary>
    /// Health check for Onboarding Coverage Service
    /// Validates that coverage manifest can be loaded and is valid
    /// </summary>
    public class OnboardingCoverageHealthCheck : IHealthCheck
    {
        private readonly IOnboardingCoverageService _coverageService;

        public OnboardingCoverageHealthCheck(IOnboardingCoverageService coverageService)
        {
            _coverageService = coverageService ?? throw new ArgumentNullException(nameof(coverageService));
        }

        public async Task<MsHealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to load manifest
                var manifest = await _coverageService.LoadManifestAsync(cancellationToken);

                if (manifest == null)
                {
                    return MsHealthCheckResult.Unhealthy(
                        "Onboarding coverage manifest is null",
                        data: new Dictionary<string, object>
                        {
                            ["Service"] = "OnboardingCoverageService",
                            ["Check"] = "ManifestLoad"
                        });
                }

                // Validate manifest has required structure
                if (manifest.RequiredIdsByNode == null || manifest.RequiredIdsByNode.Count == 0)
                {
                    return MsHealthCheckResult.Degraded(
                        "Onboarding coverage manifest is empty or invalid",
                        data: new Dictionary<string, object>
                        {
                            ["Service"] = "OnboardingCoverageService",
                            ["Check"] = "ManifestValidation",
                            ["Version"] = manifest.Version,
                            ["Namespace"] = manifest.Namespace
                        });
                }

                return MsHealthCheckResult.Healthy(
                    "Onboarding coverage manifest loaded successfully",
                    data: new Dictionary<string, object>
                    {
                        ["Service"] = "OnboardingCoverageService",
                        ["Version"] = manifest.Version,
                        ["Namespace"] = manifest.Namespace,
                        ["NodesCount"] = manifest.RequiredIdsByNode.Count,
                        ["MissionsCount"] = manifest.RequiredIdsByMission?.Count ?? 0,
                        ["ConditionalRulesCount"] = manifest.ConditionalRequired?.Count ?? 0
                    });
            }
            catch (Exception ex)
            {
                return MsHealthCheckResult.Unhealthy(
                    "Failed to load onboarding coverage manifest",
                    ex,
                    data: new Dictionary<string, object>
                    {
                        ["Service"] = "OnboardingCoverageService",
                        ["Error"] = ex.Message,
                        ["ExceptionType"] = ex.GetType().Name
                    });
            }
        }
    }
}
