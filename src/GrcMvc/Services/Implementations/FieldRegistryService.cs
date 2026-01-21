using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for managing the canonical field registry
    /// Generates field registry from OnboardingWizard entity and coverage manifest
    /// </summary>
    public class FieldRegistryService : IFieldRegistryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FieldRegistryService> _logger;
        private readonly IOnboardingCoverageService _coverageService;
        private FieldRegistry? _cachedRegistry;
        private readonly object _lock = new();

        public FieldRegistryService(
            IConfiguration configuration,
            ILogger<FieldRegistryService> logger,
            IOnboardingCoverageService coverageService)
        {
            _configuration = configuration;
            _logger = logger;
            _coverageService = coverageService;
        }

        public async Task<FieldRegistry> LoadRegistryAsync(CancellationToken ct = default)
        {
            if (_cachedRegistry != null)
                return _cachedRegistry;

            // Use async-compatible locking
            lock (_lock)
            {
                if (_cachedRegistry != null)
                    return _cachedRegistry;
            }

            try
            {
                // Generate registry from OnboardingWizard entity properties and coverage manifest
                var registry = new FieldRegistry();
                var fields = new Dictionary<string, FieldRegistryEntry>();

                // Load coverage manifest to get all field IDs
                var manifest = await _coverageService.LoadManifestAsync(ct);

                    // Collect all field IDs from manifest
                    var allFieldIds = new HashSet<string>();

                    // From required_ids_by_node
                    foreach (var nodeFields in manifest.RequiredIdsByNode.Values)
                    {
                        foreach (var fieldId in nodeFields)
                        {
                            allFieldIds.Add(fieldId);
                        }
                    }

                    // From optional_ids_by_node
                    foreach (var nodeFields in manifest.OptionalIdsByNode.Values)
                    {
                        foreach (var fieldId in nodeFields)
                        {
                            allFieldIds.Add(fieldId);
                        }
                    }

                    // From required_ids_by_mission
                    foreach (var missionFields in manifest.RequiredIdsByMission.Values)
                    {
                        foreach (var fieldId in missionFields)
                        {
                            allFieldIds.Add(fieldId);
                        }
                    }

                    // From conditional_required
                    foreach (var conditionalRule in manifest.ConditionalRequired)
                    {
                        allFieldIds.Add(conditionalRule.If.Field);
                        foreach (var fieldId in conditionalRule.ThenRequire)
                        {
                            allFieldIds.Add(fieldId);
                        }
                    }

                    // Generate field entries from OnboardingWizard entity properties
                    var wizardType = typeof(OnboardingWizard);
                    var properties = wizardType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    // Create a mapping of field IDs to property names (from OnboardingFieldValueProvider logic)
                    var fieldIdToPropertyMap = BuildFieldIdToPropertyMap();

                    // Create registry entries for all field IDs
                    foreach (var fieldId in allFieldIds)
                    {
                        var entry = CreateFieldEntry(fieldId, wizardType, properties, fieldIdToPropertyMap);
                        if (entry != null)
                        {
                            fields[fieldId] = entry;
                        }
                    }

                registry.Fields = fields;

                lock (_lock)
                {
                    _cachedRegistry = registry;
                }

                _logger.LogInformation("Field registry loaded successfully with {Count} fields", fields.Count);
                return registry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading field registry");
                return new FieldRegistry(); // Return empty registry on error
            }
        }

        public async Task<bool> ValidateFieldIdAsync(string fieldId, CancellationToken ct = default)
        {
            var registry = await LoadRegistryAsync(ct);
            return registry.ContainsField(fieldId);
        }

        public async Task<List<string>> ValidateFieldIdsAsync(IEnumerable<string> fieldIds, CancellationToken ct = default)
        {
            var registry = await LoadRegistryAsync(ct);
            var missingIds = new List<string>();

            foreach (var fieldId in fieldIds)
            {
                if (!registry.ContainsField(fieldId))
                {
                    missingIds.Add(fieldId);
                }
            }

            return missingIds;
        }

        public async Task<FieldRegistryEntry?> GetFieldEntryAsync(string fieldId, CancellationToken ct = default)
        {
            var registry = await LoadRegistryAsync(ct);
            return registry.GetField(fieldId);
        }

        public async Task<Dictionary<string, FieldRegistryEntry>> GetAllFieldsAsync(CancellationToken ct = default)
        {
            var registry = await LoadRegistryAsync(ct);
            return registry.Fields;
        }

        #region Private Helper Methods

        private FieldRegistryEntry? CreateFieldEntry(
            string fieldId,
            Type wizardType,
            PropertyInfo[] properties,
            Dictionary<string, string> fieldIdToPropertyMap)
        {
            try
            {
                var entry = new FieldRegistryEntry
                {
                    FieldId = fieldId,
                    FieldName = GetHumanReadableName(fieldId),
                    FieldType = InferFieldType(fieldId),
                    IsRequired = false, // Will be determined by manifest
                    Description = GetFieldDescription(fieldId)
                };

                // Try to find corresponding property
                if (fieldIdToPropertyMap.TryGetValue(fieldId, out var propertyName))
                {
                    var property = properties.FirstOrDefault(p => p.Name == propertyName);
                    if (property != null)
                    {
                        entry.FieldType = GetCSharpTypeName(property.PropertyType);
                        entry.AllowedValues = GetAllowedValues(property, fieldId);
                    }
                }

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating field entry for {FieldId}", fieldId);
                return null;
            }
        }

        private Dictionary<string, string> BuildFieldIdToPropertyMap()
        {
            // This matches the mapping logic in OnboardingFieldValueProvider
            // For brevity, we'll generate entries for all known field IDs
            // In a production system, this would be stored in a configuration file or database
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Simple Flow fields
                ["SF.S1.organization_name"] = nameof(OnboardingWizard.OrganizationLegalNameEn),
                ["SF.S1.admin_email"] = "AdminEmail", // Extracted from Tenant
                ["SF.S1.subscription_tier"] = "SubscriptionTier", // From Tenant
                ["SF.S1.country_of_operation"] = nameof(OnboardingWizard.CountryOfIncorporation),
                
                // Wizard Section A
                ["W.A.1.legal_name_en"] = nameof(OnboardingWizard.OrganizationLegalNameEn),
                ["W.A.2.legal_name_ar"] = nameof(OnboardingWizard.OrganizationLegalNameAr),
                ["W.A.3.trade_name"] = nameof(OnboardingWizard.TradeName),
                ["W.A.4.country_of_incorporation"] = nameof(OnboardingWizard.CountryOfIncorporation),
                ["W.A.5.operating_countries"] = nameof(OnboardingWizard.OperatingCountriesJson),
                ["W.A.6.primary_hq_location"] = nameof(OnboardingWizard.PrimaryHqLocation),
                ["W.A.7.timezone"] = nameof(OnboardingWizard.DefaultTimezone),
                ["W.A.8.primary_language"] = nameof(OnboardingWizard.PrimaryLanguage),
                ["W.A.9.corporate_email_domains"] = nameof(OnboardingWizard.CorporateEmailDomainsJson),
                ["W.A.10.domain_verification_method"] = nameof(OnboardingWizard.DomainVerificationMethod),
                ["W.A.11.organization_type"] = nameof(OnboardingWizard.OrganizationType),
                ["W.A.12.industry_sector"] = nameof(OnboardingWizard.IndustrySector),
                ["W.A.13.data_residency_requirements"] = nameof(OnboardingWizard.HasDataResidencyRequirement),
                
                // Wizard Section B
                ["W.B.1.primary_driver"] = nameof(OnboardingWizard.PrimaryDriver),
                
                // Wizard Section C
                ["W.C.1.primary_regulators"] = nameof(OnboardingWizard.PrimaryRegulatorsJson),
                
                // Wizard Section D
                ["W.D.5.in_scope_environments"] = nameof(OnboardingWizard.InScopeEnvironments),
                
                // Wizard Section E
                ["W.E.1.data_types_processed"] = nameof(OnboardingWizard.DataTypesProcessedJson),
                ["W.E.2.payment_card_data"] = nameof(OnboardingWizard.HasPaymentCardData),
                
                // Wizard Section F
                ["W.F.1.identity_provider"] = nameof(OnboardingWizard.IdentityProvider),
                ["W.F.2.sso_enabled"] = nameof(OnboardingWizard.SsoEnabled),
                ["W.F.3.scim_provisioning"] = nameof(OnboardingWizard.ScimProvisioningAvailable),
                
                // Wizard Section G
                ["W.G.1.ownership_approach"] = nameof(OnboardingWizard.ControlOwnershipApproach),
                
                // Wizard Section H
                ["W.H.1.organization_admins"] = nameof(OnboardingWizard.OrgAdminsJson),
                ["W.H.2.create_teams_now"] = nameof(OnboardingWizard.CreateTeamsNow),
                
                // Wizard Section I
                ["W.I.8.evidence_sla_submit_days"] = nameof(OnboardingWizard.EvidenceSlaSubmitDays),
                
                // Wizard Section K
                ["W.K.1.adopt_default_baseline"] = nameof(OnboardingWizard.AdoptDefaultBaseline),
            };
        }

        private string GetHumanReadableName(string fieldId)
        {
            // Convert field ID to human-readable name
            // e.g., "SF.S1.organization_name" -> "Organization Name"
            var parts = fieldId.Split('.');
            if (parts.Length > 0)
            {
                var lastPart = parts[^1];
                return lastPart.Replace('_', ' ').Split(' ')
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                    .Aggregate((a, b) => a + " " + b);
            }
            return fieldId;
        }

        private string InferFieldType(string fieldId)
        {
            // Infer field type from field ID
            if (fieldId.Contains("_date") || fieldId.Contains("_at"))
                return "DateTime";
            if (fieldId.Contains("_enabled") || fieldId.Contains("_accepted") || fieldId.Contains("_consent") || fieldId.Contains("_required"))
                return "bool";
            if (fieldId.Contains("_tier") || fieldId.Contains("_days") || fieldId.Contains("_hours") || fieldId.Contains("_count"))
                return "int";
            if (fieldId.Contains("_json") || fieldId.Contains("_matrix") || fieldId.Contains("_details") || fieldId.Contains("_config"))
                return "object";
            if (fieldId.Contains("_email"))
                return "string (email)";
            return "string";
        }

        private string GetCSharpTypeName(Type type)
        {
            if (type == typeof(string))
                return "string";
            if (type == typeof(int) || type == typeof(int?))
                return "int";
            if (type == typeof(bool) || type == typeof(bool?))
                return "bool";
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return "DateTime";
            if (type == typeof(decimal) || type == typeof(decimal?))
                return "decimal";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return $"List<{GetCSharpTypeName(type.GetGenericArguments()[0])}>";
            return type.Name;
        }

        private List<string>? GetAllowedValues(PropertyInfo property, string fieldId)
        {
            // Return allowed values if this is an enum or has specific constraints
            if (property.PropertyType.IsEnum)
            {
                return Enum.GetNames(property.PropertyType).ToList();
            }

            // Add common allowed values based on field ID
            return fieldId switch
            {
                "SF.S1.subscription_tier" => new List<string> { "MVP", "Professional", "Enterprise" },
                "W.B.1.primary_driver" => new List<string> { "RegulatorExam", "InternalAudit", "Certification", "CustomerRequirement", "BoardMandate", "RiskReduction", "OperationalImprovement" },
                "W.G.1.ownership_approach" => new List<string> { "Centralized", "Federated", "Hybrid" },
                _ => null
            };
        }

        private string? GetFieldDescription(string fieldId)
        {
            // Return human-readable description for field
            return fieldId switch
            {
                "SF.S1.organization_name" => "Official legal name of the organization in English",
                "SF.S1.admin_email" => "Primary administrator email address for the organization",
                "SF.S1.subscription_tier" => "Selected subscription tier (MVP, Professional, or Enterprise)",
                "SF.S1.country_of_operation" => "Primary country where the organization operates",
                "W.B.1.primary_driver" => "Main business driver for compliance program",
                "W.C.1.primary_regulators" => "List of primary regulatory bodies overseeing the organization",
                "W.D.5.in_scope_environments" => "IT environments in scope for compliance assessment",
                "W.E.1.data_types_processed" => "Types of data the organization processes",
                "W.E.2.payment_card_data" => "Whether the organization processes payment card data (PCI-DSS scope)",
                "W.F.2.sso_enabled" => "Whether single sign-on (SSO) is enabled",
                "W.G.1.ownership_approach" => "Governance model for control ownership (Centralized/Federated/Hybrid)",
                "W.H.1.organization_admins" => "List of organization administrators",
                "W.I.8.evidence_sla_submit_days" => "SLA deadline in days for evidence submission",
                "W.K.1.adopt_default_baseline" => "Whether to adopt the default compliance baseline",
                _ => null
            };
        }

        #endregion
    }
}