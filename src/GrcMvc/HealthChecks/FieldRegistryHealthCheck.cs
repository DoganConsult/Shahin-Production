using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace GrcMvc.HealthChecks
{
    /// <summary>
    /// Health check for Field Registry Service
    /// Validates that field registry can be loaded and is valid
    /// </summary>
    public class FieldRegistryHealthCheck : IHealthCheck
    {
        private readonly IFieldRegistryService _registryService;

        public FieldRegistryHealthCheck(IFieldRegistryService registryService)
        {
            _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to load registry
                var registry = await _registryService.LoadRegistryAsync(cancellationToken);

                if (registry == null)
                {
                    return HealthCheckResult.Unhealthy(
                        "Field registry is null",
                        data: new Dictionary<string, object>
                        {
                            ["Service"] = "FieldRegistryService",
                            ["Check"] = "RegistryLoad"
                        });
                }

                // Validate registry has fields
                var allFields = await _registryService.GetAllFieldsAsync(cancellationToken);
                if (allFields == null || allFields.Count == 0)
                {
                    return HealthCheckResult.Degraded(
                        "Field registry is empty",
                        data: new Dictionary<string, object>
                        {
                            ["Service"] = "FieldRegistryService",
                            ["Check"] = "RegistryValidation"
                        });
                }

                // Test validation with a known field
                var testFieldId = "SF.S1.organization_name";
                var isValid = await _registryService.ValidateFieldIdAsync(testFieldId, cancellationToken);

                return HealthCheckResult.Healthy(
                    "Field registry loaded and validated successfully",
                    data: new Dictionary<string, object>
                    {
                        ["Service"] = "FieldRegistryService",
                        ["FieldsCount"] = allFields.Count,
                        ["TestFieldValidated"] = isValid,
                        ["TestFieldId"] = testFieldId
                    });
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Failed to load or validate field registry",
                    ex,
                    data: new Dictionary<string, object>
                    {
                        ["Service"] = "FieldRegistryService",
                        ["Error"] = ex.Message,
                        ["ExceptionType"] = ex.GetType().Name
                    });
            }
        }
    }
}
