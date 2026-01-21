using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for validating onboarding coverage against manifest specification
    /// Implements deterministic validation based on YAML coverage manifest
    /// </summary>
    public class OnboardingCoverageService : IOnboardingCoverageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingCoverageService> _logger;
        private readonly IDeserializer _yamlDeserializer;
        private CoverageManifest? _cachedManifest;
        private readonly object _lock = new();

        public OnboardingCoverageService(
            IConfiguration configuration,
            ILogger<OnboardingCoverageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public async Task<CoverageManifest> LoadManifestAsync(CancellationToken ct = default)
        {
            if (_cachedManifest != null)
                return _cachedManifest;

            var manifestPath = _configuration["Onboarding:CoverageManifestPath"] 
                ?? "etc/onboarding/coverage-manifest.yml";

            // Handle relative paths
            if (!Path.IsPathRooted(manifestPath))
            {
                var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (basePath != null)
                {
                    // Go up from bin/Debug/net8.0 to project root
                    var projectRoot = Path.GetFullPath(Path.Combine(basePath, "../../../.."));
                    manifestPath = Path.Combine(projectRoot, manifestPath);
                }
            }

            if (!File.Exists(manifestPath))
            {
                _logger.LogWarning("Coverage manifest file not found at {Path}, returning empty manifest", manifestPath);
                return new CoverageManifest();
            }

            try
            {
                var yamlContent = await File.ReadAllTextAsync(manifestPath, ct);
                
                // Parse YAML structure manually for better control
                var yamlDict = _yamlDeserializer.Deserialize<Dictionary<object, object>>(yamlContent);
                
                var manifest = ParseFromDictionary(yamlDict);

                lock (_lock)
                {
                    _cachedManifest = manifest;
                }

                _logger.LogInformation("Coverage manifest loaded successfully from {Path}", manifestPath);
                return manifest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coverage manifest from {Path}", manifestPath);
                
                // Return empty manifest instead of throwing to allow application to start
                // In production, you may want to throw to prevent startup if manifest is critical
                _logger.LogWarning("Returning empty manifest due to load error. Application will continue but coverage validation may be limited.");
                
                var emptyManifest = new CoverageManifest
                {
                    Version = "1.0",
                    Namespace = "grc.onboarding.coverage",
                    GeneratedAt = DateTime.UtcNow
                };
                
                lock (_lock)
                {
                    _cachedManifest = emptyManifest;
                }
                
                return emptyManifest;
            }
        }

        public async Task<NodeCoverageResult> ValidateNodeCoverageAsync(
            string nodeId,
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default)
        {
            var manifest = await LoadManifestAsync(ct);
            var result = new NodeCoverageResult { NodeId = nodeId };

            // Get required fields for this node
            var requiredFields = GetRequiredFieldsForNode(nodeId, manifest);
            var optionalFields = manifest.OptionalIdsByNode.GetValueOrDefault(nodeId, new List<string>());

            // Check required fields
            foreach (var fieldId in requiredFields)
            {
                if (fieldProvider.HasFieldValue(fieldId))
                {
                    result.PresentRequiredFields.Add(fieldId);
                }
                else
                {
                    result.MissingRequiredFields.Add(fieldId);
                }
            }

            // Check optional fields (for telemetry)
            foreach (var fieldId in optionalFields)
            {
                if (fieldProvider.HasFieldValue(fieldId))
                {
                    result.PresentOptionalFields.Add(fieldId);
                }
            }

            // Evaluate conditional required fields
            var conditionalFields = EvaluateConditionalRequired(fieldProvider, manifest);
            foreach (var fieldId in conditionalFields)
            {
                if (fieldProvider.HasFieldValue(fieldId))
                {
                    result.ConditionalRequiredFields.Add(fieldId);
                }
                else if (requiredFields.Contains(fieldId) || conditionalFields.Contains(fieldId))
                {
                    result.MissingRequiredFields.Add(fieldId);
                    result.ValidationErrors[fieldId] = "Conditionally required field is missing";
                }
            }

            result.IsValid = result.MissingRequiredFields.Count == 0;
            result.RequiredFields = requiredFields;
            return result;
        }

        public async Task<MissionCoverageResult> ValidateMissionCoverageAsync(
            string missionId,
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default)
        {
            var manifest = await LoadManifestAsync(ct);
            var result = new MissionCoverageResult { MissionId = missionId };

            // Get required fields for this mission
            var requiredFields = GetRequiredFieldsForMission(missionId, manifest);

            // Check all required fields
            foreach (var fieldId in requiredFields)
            {
                if (fieldProvider.HasFieldValue(fieldId))
                {
                    result.PresentRequiredFields.Add(fieldId);
                }
                else
                {
                    result.MissingRequiredFields.Add(fieldId);
                }
            }

            // Find all nodes that belong to this mission
            var missionNodes = manifest.RequiredIdsByNode
                .Where(kvp => kvp.Key.StartsWith(missionId.Replace("MISSION_", "M"), StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            // Validate each node
            foreach (var nodeId in missionNodes)
            {
                var nodeResult = await ValidateNodeCoverageAsync(nodeId, fieldProvider, ct);
                result.NodeResults.Add(nodeResult);
            }

            result.IsValid = result.MissingRequiredFields.Count == 0;
            result.CompletionPercentage = requiredFields.Count > 0
                ? (int)((double)result.PresentRequiredFields.Count / requiredFields.Count * 100)
                : 100;

            return result;
        }

        public async Task<CoverageValidationResult> ValidateCompleteCoverageAsync(
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default)
        {
            var manifest = await LoadManifestAsync(ct);
            var result = new CoverageValidationResult();

            // Validate all nodes
            foreach (var nodeId in manifest.RequiredIdsByNode.Keys)
            {
                var nodeResult = await ValidateNodeCoverageAsync(nodeId, fieldProvider, ct);
                result.NodeResults[nodeId] = nodeResult;

                if (!nodeResult.IsValid)
                {
                    result.Errors.AddRange(nodeResult.MissingRequiredFields
                        .Select(f => $"{nodeId}: Missing required field '{f}'"));
                }
            }

            // Validate all missions
            foreach (var missionId in manifest.RequiredIdsByMission.Keys)
            {
                var missionResult = await ValidateMissionCoverageAsync(missionId, fieldProvider, ct);
                result.MissionResults[missionId] = missionResult;

                if (!missionResult.IsValid)
                {
                    result.Warnings.Add($"{missionId}: {missionResult.MissingRequiredFields.Count} required fields missing");
                }
            }

            // Calculate overall completion
            var allRequiredFields = manifest.RequiredIdsByMission.Values
                .SelectMany(x => x)
                .Distinct()
                .ToList();
            var presentFields = allRequiredFields
                .Where(fieldProvider.HasFieldValue)
                .ToList();
            
            result.OverallCompletionPercentage = allRequiredFields.Count > 0
                ? (int)((double)presentFields.Count / allRequiredFields.Count * 100)
                : 100;

            result.IsValid = result.Errors.Count == 0 && result.MissionResults.Values.All(m => m.IsValid);
            return result;
        }

        public async Task<Dictionary<string, bool>> RunIntegrityChecksAsync(
            FieldRegistry fieldRegistry,
            CancellationToken ct = default)
        {
            var manifest = await LoadManifestAsync(ct);
            var results = new Dictionary<string, bool>();

            foreach (var check in manifest.IntegrityChecks)
            {
                bool passed = false;
                try
                {
                    switch (check.Name)
                    {
                        case "all_required_ids_exist_in_registry":
                            passed = ValidateAllRequiredIdsExist(fieldRegistry, manifest);
                            break;

                        case "mission_union_equals_nodes_union":
                            passed = ValidateMissionUnionEqualsNodesUnion(manifest);
                            break;

                        case "conditional_required_ids_exist_in_registry":
                            passed = ValidateConditionalRequiredIdsExist(fieldRegistry, manifest);
                            break;

                        default:
                            _logger.LogWarning("Unknown integrity check: {CheckName}", check.Name);
                            passed = false;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running integrity check: {CheckName}", check.Name);
                    passed = false;
                }

                results[check.Name] = passed;
            }

            return results;
        }

        public List<string> EvaluateConditionalRequired(
            IFieldValueProvider fieldProvider,
            CoverageManifest manifest)
        {
            var requiredFields = new List<string>();

            foreach (var rule in manifest.ConditionalRequired)
            {
                if (EvaluateCondition(rule.If, fieldProvider))
                {
                    requiredFields.AddRange(rule.ThenRequire);
                }
            }

            return requiredFields.Distinct().ToList();
        }

        public List<string> GetRequiredFieldsForNode(string nodeId, CoverageManifest manifest)
        {
            return manifest.RequiredIdsByNode.GetValueOrDefault(nodeId, new List<string>());
        }

        public List<string> GetRequiredFieldsForMission(string missionId, CoverageManifest manifest)
        {
            return manifest.RequiredIdsByMission.GetValueOrDefault(missionId, new List<string>());
        }

        #region Private Helper Methods

        private bool EvaluateCondition(ConditionalRuleCondition condition, IFieldValueProvider fieldProvider)
        {
            var fieldValue = fieldProvider.GetFieldValue(condition.Field);
            
            return condition.Op switch
            {
                "==" => Equals(fieldValue, condition.Value),
                "!=" => !Equals(fieldValue, condition.Value),
                "contains" => fieldValue is IEnumerable<object> enumerable && enumerable.Cast<object>().Contains(condition.Value),
                "notContains" => fieldValue is not IEnumerable<object> enumerable || !enumerable.Cast<object>().Contains(condition.Value),
                "notEmpty" => fieldValue != null && fieldValue.ToString() != string.Empty,
                _ => false
            };
        }

        private bool ValidateAllRequiredIdsExist(FieldRegistry fieldRegistry, CoverageManifest manifest)
        {
            var allRequiredIds = manifest.RequiredIdsByNode.Values
                .SelectMany(x => x)
                .Union(manifest.RequiredIdsByMission.Values.SelectMany(x => x))
                .Distinct()
                .ToList();

            var missingIds = allRequiredIds.Where(id => !fieldRegistry.ContainsField(id)).ToList();

            if (missingIds.Any())
            {
                _logger.LogWarning("Missing field IDs in registry: {MissingIds}", string.Join(", ", missingIds));
                return false;
            }

            return true;
        }

        private bool ValidateMissionUnionEqualsNodesUnion(CoverageManifest manifest)
        {
            foreach (var mission in manifest.RequiredIdsByMission.Keys)
            {
                // Find nodes that belong to this mission
                var missionPrefix = mission.Replace("MISSION_", "M");
                var missionNodes = manifest.RequiredIdsByNode.Keys
                    .Where(nodeId => nodeId.StartsWith(missionPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Get union of node required IDs
                var nodesUnion = missionNodes
                    .SelectMany(nodeId => manifest.RequiredIdsByNode[nodeId])
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // Get mission required IDs
                var missionRequired = manifest.RequiredIdsByMission[mission]
                    .OrderBy(x => x)
                    .ToList();

                if (!nodesUnion.SequenceEqual(missionRequired))
                {
                    _logger.LogWarning(
                        "Mission union mismatch for {Mission}. Nodes union: {NodesUnion}, Mission required: {MissionRequired}",
                        mission, string.Join(", ", nodesUnion), string.Join(", ", missionRequired));
                    return false;
                }
            }

            return true;
        }

        private bool ValidateConditionalRequiredIdsExist(FieldRegistry fieldRegistry, CoverageManifest manifest)
        {
            var conditionalRequiredIds = manifest.ConditionalRequired
                .SelectMany(rule => rule.ThenRequire)
                .Distinct()
                .ToList();

            var missingIds = conditionalRequiredIds.Where(id => !fieldRegistry.ContainsField(id)).ToList();

            if (missingIds.Any())
            {
                _logger.LogWarning("Missing conditional required IDs in registry: {MissingIds}", string.Join(", ", missingIds));
                return false;
            }

            return true;
        }

        private static T? GetValue<T>(Dictionary<object, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return default;
        }

        private static DateTime? ParseDateTime(string? dateStr)
        {
            return DateTimeConverter.Parse(dateStr);
        }

        private static Dictionary<string, List<string>> ParseDictionaryOfLists(
            Dictionary<object, object> yamlDict, 
            string key)
        {
            var result = new Dictionary<string, List<string>>();

            if (yamlDict.TryGetValue(key, out var value) && value is Dictionary<object, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var nodeId = kvp.Key.ToString() ?? string.Empty;
                    var list = new List<string>();

                    if (kvp.Value is List<object> yamlList)
                    {
                        list.AddRange(yamlList.Select(item => item?.ToString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s)));
                    }
                    else if (kvp.Value is object[] arr)
                    {
                        list.AddRange(arr.Select(item => item?.ToString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s)));
                    }

                    result[nodeId] = list;
                }
            }

            return result;
        }

        private static List<ConditionalRequiredRule> ParseConditionalRequired(Dictionary<object, object> yamlDict)
        {
            var rules = new List<ConditionalRequiredRule>();

            if (yamlDict.TryGetValue("conditional_required", out var value) && value is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<object, object> ruleDict)
                    {
                        var rule = new ConditionalRequiredRule
                        {
                            Id = GetValue<string>(ruleDict, "id") ?? string.Empty
                        };

                        // Parse "if" condition
                        if (ruleDict.TryGetValue("if", out var ifValue) && ifValue is Dictionary<object, object> ifDict)
                        {
                            rule.If = new ConditionalRuleCondition
                            {
                                Field = GetValue<string>(ifDict, "field") ?? string.Empty,
                                Op = GetValue<string>(ifDict, "op") ?? "==",
                                Value = ifDict.TryGetValue("value", out var valValue) ? valValue : null
                            };
                        }

                        // Parse "then_require" list
                        if (ruleDict.TryGetValue("then_require", out var thenValue))
                        {
                            if (thenValue is List<object> thenList)
                            {
                                rule.ThenRequire = thenList.Select(item => item?.ToString() ?? string.Empty)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();
                            }
                        }

                        if (!string.IsNullOrEmpty(rule.Id))
                        {
                            rules.Add(rule);
                        }
                    }
                }
            }

            return rules;
        }

        private static List<IntegrityCheck> ParseIntegrityChecks(Dictionary<object, object> yamlDict)
        {
            var checks = new List<IntegrityCheck>();

            if (yamlDict.TryGetValue("integrity_checks", out var value) && value is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<object, object> checkDict)
                    {
                        checks.Add(new IntegrityCheck
                        {
                            Name = GetValue<string>(checkDict, "name") ?? string.Empty,
                            Description = GetValue<string>(checkDict, "description") ?? string.Empty
                        });
                    }
                }
            }

            return checks;
        }

        private CoverageManifest ParseFromDictionary(Dictionary<object, object> yamlDict)
        {
            var manifest = new CoverageManifest
            {
                Version = GetValue<string>(yamlDict, "version") ?? "1.0",
                Namespace = GetValue<string>(yamlDict, "namespace") ?? "grc.onboarding.coverage",
                GeneratedAt = ParseDateTime(GetValue<string>(yamlDict, "generated_at")) ?? DateTime.UtcNow,
                RequiredIdsByNode = ParseDictionaryOfLists(yamlDict, "required_ids_by_node"),
                OptionalIdsByNode = ParseDictionaryOfLists(yamlDict, "optional_ids_by_node"),
                RequiredIdsByMission = ParseDictionaryOfLists(yamlDict, "required_ids_by_mission"),
                ConditionalRequired = ParseConditionalRequired(yamlDict),
                IntegrityChecks = ParseIntegrityChecks(yamlDict)
            };

            return manifest;
        }

        #endregion
    }

    /// <summary>
    /// DateTime converter helper for YAML parsing
    /// </summary>
    internal static class DateTimeConverter
    {
        public static DateTime? Parse(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            if (DateTime.TryParse(dateStr, out var date))
                return date;

            return null;
        }
    }
}
