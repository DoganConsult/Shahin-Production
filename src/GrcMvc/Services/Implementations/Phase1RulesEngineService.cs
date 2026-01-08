using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 1: Rules Engine Service Implementation
    /// Evaluates compliance rules for scope derivation
    /// </summary>
    public class Phase1RulesEngineService : IRulesEngineService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<Phase1RulesEngineService> _logger;

        public Phase1RulesEngineService(GrcDbContext context, ILogger<Phase1RulesEngineService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Derive applicable controls for a tenant based on frameworks
        /// </summary>
        public async Task<List<Models.Entities.Control>> DeriveApplicableControlsAsync(Guid tenantId, List<Guid> frameworkIds)
        {
            try
            {
                var controls = await _context.Set<Models.Entities.Control>()
                    .AsNoTracking()
                    .Where(c => c.TenantId == tenantId)
                    .OrderBy(c => c.ControlCode)
                    .ToListAsync();

                _logger.LogInformation($"Derived {controls.Count} applicable controls for tenant {tenantId}");
                return controls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deriving applicable controls for tenant {tenantId}");
                throw;
            }
        }

        /// <summary>
        /// Evaluate a compliance rule expression
        /// </summary>
        public async Task<bool> EvaluateRuleAsync(string ruleExpression, Dictionary<string, object> context)
        {
            try
            {
                if (string.IsNullOrEmpty(ruleExpression))
                    return false;

                // Parse condition JSON and evaluate against context
                var condition = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(ruleExpression);
                if (condition == null) return false;

                // Simple AND evaluation - all conditions must match
                foreach (var kvp in condition)
                {
                    if (!context.TryGetValue(kvp.Key, out var contextValue))
                        return false;

                    var conditionValue = kvp.Value?.ToString()?.ToLower();
                    var actualValue = contextValue?.ToString()?.ToLower();

                    if (conditionValue != actualValue)
                        return false;
                }

                _logger.LogInformation($"Rule evaluated: {ruleExpression} = true");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluating rule: {ruleExpression}");
                return false;
            }
        }

        /// <summary>
        /// Evaluate all active rules for a tenant and derive scope
        /// </summary>
        public async Task<RuleExecutionLog> EvaluateRulesAsync(
            Guid tenantId,
            OrganizationProfile profile,
            Ruleset ruleset,
            string userId)
        {
            var executionLog = new RuleExecutionLog
            {
                Id = Guid.NewGuid(),
                RulesetId = ruleset.Id,
                TenantId = tenantId,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = userId,
                Status = "InProgress",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            try
            {
                // Build context from org profile - Extended with all relevant fields for rule evaluation
                var context = new Dictionary<string, object>
                {
                    // Basic profile
                    { "country", profile.Country ?? "SA" },
                    { "sector", profile.Sector ?? "" },
                    { "orgType", profile.OrganizationType ?? "" },
                    { "dataTypes", profile.DataTypes ?? "" },
                    { "hostingModel", profile.HostingModel ?? "" },
                    { "organizationSize", profile.OrganizationSize ?? "" },
                    { "complianceMaturity", profile.ComplianceMaturity ?? "" },

                    // Regulatory & compliance fields
                    { "isRegulatedEntity", profile.IsRegulatedEntity.ToString().ToLower() },
                    { "isCriticalInfrastructure", profile.IsCriticalInfrastructure.ToString().ToLower() },
                    { "primaryRegulator", profile.PrimaryRegulator ?? "" },
                    { "secondaryRegulators", profile.SecondaryRegulators ?? "" },
                    { "regulatoryCertifications", profile.RegulatoryCertifications ?? "" },
                    { "industryLicenses", profile.IndustryLicenses ?? "" },

                    // Data & technology fields
                    { "processesPersonalData", profile.ProcessesPersonalData.ToString().ToLower() },
                    { "processesSensitiveData", profile.ProcessesSensitiveData.ToString().ToLower() },
                    { "hasDataCenterInKSA", profile.HasDataCenterInKSA.ToString().ToLower() },
                    { "cloudProviders", profile.CloudProviders ?? "" },
                    { "dataSubjectCount", profile.DataSubjectCount.ToString() },

                    // Third parties & vendors
                    { "hasThirdPartyDataProcessing", profile.HasThirdPartyDataProcessing.ToString().ToLower() },
                    { "vendorCount", profile.VendorCount.ToString() },
                    { "criticalVendorCount", profile.CriticalVendorCount.ToString() },
                    { "thirdPartyRiskLevel", profile.ThirdPartyRiskLevel ?? "" },

                    // Organization structure
                    { "isPubliclyTraded", profile.IsPubliclyTraded.ToString().ToLower() },
                    { "isSubsidiary", profile.IsSubsidiary.ToString().ToLower() },
                    { "employeeCount", profile.EmployeeCount.ToString() },
                    { "branchCount", profile.BranchCount.ToString() }
                };

                // Get active rules ordered by priority
                var rules = await _context.Rules
                    .Where(r => r.RulesetId == ruleset.Id && r.Status == "ACTIVE")
                    .OrderBy(r => r.Priority)
                    .ToListAsync();

                var appliedBaselines = new List<string>();
                var appliedPackages = new List<string>();
                var appliedTemplates = new List<string>();
                var matchedRules = new List<object>();

                foreach (var rule in rules)
                {
                    bool matches = await EvaluateRuleCondition(rule.ConditionJson, context);

                    if (matches)
                    {
                        var actions = ParseActions(rule.ActionsJson);
                        appliedBaselines.AddRange(actions.Baselines);
                        appliedPackages.AddRange(actions.Packages);
                        appliedTemplates.AddRange(actions.Templates);

                        matchedRules.Add(new
                        {
                            RuleCode = rule.RuleCode,
                            Name = rule.Name,
                            Reason = rule.BusinessReason
                        });

                        _logger.LogInformation($"Rule matched: {rule.RuleCode} - {rule.Name}");
                    }
                }

                // Store results in execution log
                executionLog.DerivedScopeJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    MatchedRules = matchedRules,
                    DerivedBaselines = appliedBaselines.Distinct().ToList(),
                    DerivedPackages = appliedPackages.Distinct().ToList(),
                    DerivedTemplates = appliedTemplates.Distinct().ToList()
                });
                executionLog.Status = "Completed";
                executionLog.MatchedRulesJson = System.Text.Json.JsonSerializer.Serialize(matchedRules);

                _context.RuleExecutionLogs.Add(executionLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Rules evaluation completed: {matchedRules.Count}/{rules.Count} rules matched for tenant {tenantId}");
                return executionLog;
            }
            catch (Exception ex)
            {
                executionLog.Status = "Failed";
                executionLog.ErrorMessage = ex.Message;
                _context.RuleExecutionLogs.Add(executionLog);
                await _context.SaveChangesAsync();

                _logger.LogError(ex, $"Error evaluating rules for tenant {tenantId}");
                throw;
            }
        }

        /// <summary>
        /// Derive and persist scope (TenantBaselines, TenantPackages, TenantTemplates)
        /// Now includes Asset-based recognition for data classification overlays
        /// </summary>
        public async Task<RuleExecutionLog> DeriveAndPersistScopeAsync(Guid tenantId, string userId)
        {
            // Get org profile
            var profile = await _context.OrganizationProfiles
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);

            if (profile == null)
                throw new EntityNotFoundException("OrganizationProfile", "Complete onboarding questionnaire first.");

            // Get active ruleset (global or tenant-specific)
            var ruleset = await _context.Rulesets
                .FirstOrDefaultAsync(r => r.Status == "Active");

            if (ruleset == null)
                throw new EntityNotFoundException("Ruleset", "No active ruleset found.");

            // ===== ASSET-BASED RECOGNITION (NEW) =====
            // Aggregate asset characteristics to enrich rule context
            var assetSummary = await AggregateAssetCharacteristicsAsync(tenantId);
            
            // Merge asset-derived data into profile for rule evaluation
            EnrichProfileFromAssets(profile, assetSummary);

            // Evaluate rules
            var executionLog = await EvaluateRulesAsync(tenantId, profile, ruleset, userId);

            // Parse results and persist scope
            var result = System.Text.Json.JsonSerializer.Deserialize<RuleResultDto>(executionLog.DerivedScopeJson ?? "{}");

            // Clear existing scope
            var existingBaselines = await _context.TenantBaselines.Where(b => b.TenantId == tenantId).ToListAsync();
            var existingPackages = await _context.TenantPackages.Where(p => p.TenantId == tenantId).ToListAsync();
            var existingTemplates = await _context.TenantTemplates.Where(t => t.TenantId == tenantId).ToListAsync();

            _context.TenantBaselines.RemoveRange(existingBaselines);
            _context.TenantPackages.RemoveRange(existingPackages);
            _context.TenantTemplates.RemoveRange(existingTemplates);

            // Add derived baselines
            foreach (var baselineCode in result?.DerivedBaselines ?? new List<string>())
            {
                _context.TenantBaselines.Add(new TenantBaseline
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    BaselineCode = baselineCode,
                    BaselineName = baselineCode,
                    Applicability = "Required",
                    ReasonJson = System.Text.Json.JsonSerializer.Serialize(new { Source = "RulesEngine", ExecutionLogId = executionLog.Id }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            // Add derived packages
            foreach (var packageCode in result?.DerivedPackages ?? new List<string>())
            {
                _context.TenantPackages.Add(new TenantPackage
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PackageCode = packageCode,
                    PackageName = packageCode,
                    Applicability = "Required",
                    ReasonJson = System.Text.Json.JsonSerializer.Serialize(new { Source = "RulesEngine", ExecutionLogId = executionLog.Id }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            // Add derived templates
            foreach (var templateCode in result?.DerivedTemplates ?? new List<string>())
            {
                _context.TenantTemplates.Add(new TenantTemplate
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TemplateCode = templateCode,
                    TemplateName = templateCode,
                    Applicability = "Required",
                    ReasonJson = System.Text.Json.JsonSerializer.Serialize(new { Source = "RulesEngine", ExecutionLogId = executionLog.Id }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            // Update profile
            profile.LastScopeDerivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Scope persisted for tenant {tenantId}: {result?.DerivedBaselines?.Count ?? 0} baselines, {result?.DerivedPackages?.Count ?? 0} packages, {result?.DerivedTemplates?.Count ?? 0} templates");

            return executionLog;
        }

        private async Task<bool> EvaluateRuleCondition(string conditionJson, Dictionary<string, object> context)
        {
            if (string.IsNullOrEmpty(conditionJson)) return true; // No condition = always match

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var condition = System.Text.Json.JsonSerializer.Deserialize<RuleCondition>(conditionJson, options);
                if (condition == null) return false;

                // Evaluate based on condition type
                if (condition.Type == "and")
                {
                    foreach (var cond in condition.Conditions ?? new List<ConditionItem>())
                    {
                        if (!EvaluateConditionItem(cond, context))
                            return false;
                    }
                    return true;
                }
                else if (condition.Type == "or")
                {
                    foreach (var cond in condition.Conditions ?? new List<ConditionItem>())
                    {
                        if (EvaluateConditionItem(cond, context))
                            return true;
                    }
                    return false;
                }

                return await Task.FromResult(true);
            }
            catch
            {
                return true; // Default to match on parse error
            }
        }

        private bool EvaluateConditionItem(ConditionItem item, Dictionary<string, object> context)
        {
            if (!context.TryGetValue(item.Field ?? "", out var contextValue))
                return false;

            var actualValue = contextValue?.ToString()?.ToLower() ?? "";
            var expectedValue = item.Value?.ToString()?.ToLower() ?? "";

            return item.Operator switch
            {
                "equals" => actualValue == expectedValue,
                "contains" => actualValue.Contains(expectedValue),
                "in" => item.Values?.Any(v => v.ToLower() == actualValue) ?? false,
                "not_equals" => actualValue != expectedValue,
                _ => actualValue == expectedValue
            };
        }

        private RuleActions ParseActions(string actionsJson)
        {
            if (string.IsNullOrEmpty(actionsJson))
                return new RuleActions();

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var actions = System.Text.Json.JsonSerializer.Deserialize<List<ActionItem>>(actionsJson, options);
                var result = new RuleActions();

                foreach (var action in actions ?? new List<ActionItem>())
                {
                    switch (action.Action)
                    {
                        case "apply_baseline":
                            if (!string.IsNullOrEmpty(action.Code))
                                result.Baselines.Add(action.Code);
                            break;
                        case "apply_package":
                            if (!string.IsNullOrEmpty(action.Code))
                                result.Packages.Add(action.Code);
                            break;
                        case "apply_template":
                            if (!string.IsNullOrEmpty(action.Code))
                                result.Templates.Add(action.Code);
                            break;
                    }
                }

                return result;
            }
            catch
            {
                return new RuleActions();
            }
        }

        // Helper classes for JSON parsing
        private class RuleCondition
        {
            public string Type { get; set; } = "and";
            public List<ConditionItem> Conditions { get; set; } = new();
        }

        private class ConditionItem
        {
            public string Field { get; set; } = "";
            public string Operator { get; set; } = "equals";
            public object? Value { get; set; }
            public List<string> Values { get; set; } = new();
        }

        private class ActionItem
        {
            public string Action { get; set; } = "";
            public string Code { get; set; } = "";
            public string Key { get; set; } = "";
            public string Value { get; set; } = "";
        }

        private class RuleActions
        {
            public List<string> Baselines { get; set; } = new();
            public List<string> Packages { get; set; } = new();
            public List<string> Templates { get; set; } = new();
        }

        private class RuleResultDto
        {
            public List<object>? MatchedRules { get; set; }
            public List<string>? DerivedBaselines { get; set; }
            public List<string>? DerivedPackages { get; set; }
            public List<string>? DerivedTemplates { get; set; }
        }

        // ===== ASSET-BASED RECOGNITION METHODS =====

        /// <summary>
        /// Aggregate asset characteristics across all tenant assets
        /// Used to derive data classification overlays (PCI, PHI, etc.)
        /// </summary>
        private async Task<AssetCharacteristicsSummary> AggregateAssetCharacteristicsAsync(Guid tenantId)
        {
            var assets = await _context.Assets
                .Where(a => a.TenantId == tenantId && a.Status == "Active" && !a.IsDeleted)
                .ToListAsync();

            var summary = new AssetCharacteristicsSummary
            {
                TotalAssets = assets.Count,
                CriticalTier1Count = assets.Count(a => a.Criticality == "T1"),
                CriticalTier2Count = assets.Count(a => a.Criticality == "T2"),
                
                // Data classification detection
                HasPCIData = assets.Any(a => a.DataClassification.Contains("PCI") || a.DataTypes.Contains("PCI")),
                HasPHIData = assets.Any(a => a.DataClassification.Contains("PHI") || a.DataTypes.Contains("PHI")),
                HasPIIData = assets.Any(a => a.DataClassification.Contains("PII") || a.DataTypes.Contains("PII")),
                HasRestrictedData = assets.Any(a => a.DataClassification == "Restricted"),
                HasConfidentialData = assets.Any(a => a.DataClassification == "Confidential"),
                
                // Hosting model detection
                HasCloudAssets = assets.Any(a => a.HostingModel == "Cloud" || a.HostingModel == "SaaS"),
                HasOnPremAssets = assets.Any(a => a.HostingModel == "OnPremise"),
                HasHybridAssets = assets.Any(a => a.HostingModel == "Hybrid"),
                
                // Cloud providers used
                CloudProviders = assets
                    .Where(a => !string.IsNullOrEmpty(a.CloudProvider))
                    .Select(a => a.CloudProvider)
                    .Distinct()
                    .ToList(),
                
                // Production vs non-production
                ProductionAssetCount = assets.Count(a => a.Environment == "Production"),
                NonProductionAssetCount = assets.Count(a => a.Environment != "Production"),
                
                // Asset types present
                AssetTypes = assets.Select(a => a.AssetType).Distinct().ToList(),
                
                // Data types aggregate
                AllDataTypes = assets
                    .SelectMany(a => (a.DataTypes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(t => t.Trim())
                    .Distinct()
                    .ToList()
            };

            _logger.LogInformation("Asset aggregation for tenant {TenantId}: {Count} assets, PCI={HasPCI}, PHI={HasPHI}, PII={HasPII}",
                tenantId, summary.TotalAssets, summary.HasPCIData, summary.HasPHIData, summary.HasPIIData);

            return summary;
        }

        /// <summary>
        /// Enrich organization profile with asset-derived characteristics
        /// This allows rules to trigger based on actual asset data, not just profile
        /// </summary>
        private void EnrichProfileFromAssets(OrganizationProfile profile, AssetCharacteristicsSummary assetSummary)
        {
            // Build enriched data types list
            var dataTypesSet = new HashSet<string>(
                (profile.DataTypes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
            );

            // Add asset-derived data types
            if (assetSummary.HasPCIData) dataTypesSet.Add("PCI");
            if (assetSummary.HasPHIData) dataTypesSet.Add("PHI");
            if (assetSummary.HasPIIData) dataTypesSet.Add("PII");

            foreach (var dt in assetSummary.AllDataTypes)
            {
                dataTypesSet.Add(dt);
            }

            profile.DataTypes = string.Join(",", dataTypesSet);

            // Enrich cloud providers
            if (assetSummary.CloudProviders.Any())
            {
                var cloudSet = new HashSet<string>(
                    (profile.CloudProviders ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
                );
                foreach (var cp in assetSummary.CloudProviders)
                {
                    cloudSet.Add(cp);
                }
                profile.CloudProviders = string.Join(",", cloudSet);
            }

            // Auto-derive hosting model from assets if not set
            if (string.IsNullOrEmpty(profile.HostingModel))
            {
                if (assetSummary.HasHybridAssets || (assetSummary.HasCloudAssets && assetSummary.HasOnPremAssets))
                    profile.HostingModel = "Hybrid";
                else if (assetSummary.HasCloudAssets)
                    profile.HostingModel = "Cloud";
                else if (assetSummary.HasOnPremAssets)
                    profile.HostingModel = "OnPremise";
            }

            // Mark as critical infrastructure if T1 critical assets exist
            if (assetSummary.CriticalTier1Count > 0)
            {
                profile.IsCriticalInfrastructure = true;
            }

            // Mark as processing sensitive data if restricted/confidential data present
            if (assetSummary.HasRestrictedData || assetSummary.HasConfidentialData || 
                assetSummary.HasPCIData || assetSummary.HasPHIData)
            {
                profile.ProcessesSensitiveData = true;
            }

            // Mark as processing personal data if PII present
            if (assetSummary.HasPIIData)
            {
                profile.ProcessesPersonalData = true;
            }
        }

        /// <summary>
        /// Asset characteristics summary for rule context enrichment
        /// </summary>
        private class AssetCharacteristicsSummary
        {
            public int TotalAssets { get; set; }
            public int CriticalTier1Count { get; set; }
            public int CriticalTier2Count { get; set; }
            
            // Data classification flags
            public bool HasPCIData { get; set; }
            public bool HasPHIData { get; set; }
            public bool HasPIIData { get; set; }
            public bool HasRestrictedData { get; set; }
            public bool HasConfidentialData { get; set; }
            
            // Hosting model
            public bool HasCloudAssets { get; set; }
            public bool HasOnPremAssets { get; set; }
            public bool HasHybridAssets { get; set; }
            
            // Cloud providers
            public List<string> CloudProviders { get; set; } = new();
            
            // Environment
            public int ProductionAssetCount { get; set; }
            public int NonProductionAssetCount { get; set; }
            
            // Asset types
            public List<string> AssetTypes { get; set; } = new();
            public List<string> AllDataTypes { get; set; } = new();
        }
    }
}
