using System;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Coverage manifest model representing the YAML specification structure
    /// </summary>
    public class CoverageManifest
    {
        public string Version { get; set; } = "1.0";
        public string Namespace { get; set; } = "grc.onboarding.coverage";
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Required field IDs by node (FS.1, FS.2, M1.C, etc.)
        /// </summary>
        public Dictionary<string, List<string>> RequiredIdsByNode { get; set; } = new();

        /// <summary>
        /// Optional field IDs by node (for completeness/telemetry)
        /// </summary>
        public Dictionary<string, List<string>> OptionalIdsByNode { get; set; } = new();

        /// <summary>
        /// Required field IDs by mission (union of nodes in that mission)
        /// </summary>
        public Dictionary<string, List<string>> RequiredIdsByMission { get; set; } = new();

        /// <summary>
        /// Conditional required rules (field becomes required when condition holds)
        /// </summary>
        public List<ConditionalRequiredRule> ConditionalRequired { get; set; } = new();

        /// <summary>
        /// Integrity check definitions
        /// </summary>
        public List<IntegrityCheck> IntegrityChecks { get; set; } = new();
    }

    /// <summary>
    /// Conditional required rule: field becomes required when condition holds
    /// </summary>
    public class ConditionalRequiredRule
    {
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Condition that triggers the requirement
        /// </summary>
        public ConditionalRuleCondition If { get; set; } = new();
        
        /// <summary>
        /// Field IDs that become required when condition is true
        /// </summary>
        public List<string> ThenRequire { get; set; } = new();
    }

    /// <summary>
    /// Condition structure for conditional required rules
    /// </summary>
    public class ConditionalRuleCondition
    {
        public string Field { get; set; } = string.Empty;
        public string Op { get; set; } = "=="; // ==, !=, contains, notContains, etc.
        public object? Value { get; set; }
    }

    /// <summary>
    /// Integrity check definition
    /// </summary>
    public class IntegrityCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of coverage validation for a specific node
    /// </summary>
    public class NodeCoverageResult
    {
        public string NodeId { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this node has all required fields (coverage is complete)
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Deprecated: Use IsValid instead. Kept for backward compatibility.
        /// </summary>
        [Obsolete("Use IsValid instead")]
        public bool IsComplete { get => IsValid; set => IsValid = value; }
        
        /// <summary>
        /// List of all required field IDs for this node
        /// </summary>
        public List<string> RequiredFields { get; set; } = new();
        
        /// <summary>
        /// Missing required fields
        /// </summary>
        public List<string> MissingRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Present required fields
        /// </summary>
        public List<string> PresentRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Present optional fields (for telemetry)
        /// </summary>
        public List<string> PresentOptionalFields { get; set; } = new();
        
        /// <summary>
        /// Conditional required fields that should be required based on answers
        /// </summary>
        public List<string> ConditionalRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Validation errors (if any)
        /// </summary>
        public Dictionary<string, string> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Result of mission coverage validation
    /// </summary>
    public class MissionCoverageResult
    {
        public string MissionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this mission has all required fields (coverage is complete)
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Deprecated: Use IsValid instead. Kept for backward compatibility.
        /// </summary>
        [Obsolete("Use IsValid instead")]
        public bool IsComplete { get => IsValid; set => IsValid = value; }
        
        /// <summary>
        /// Missing required fields
        /// </summary>
        public List<string> MissingRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Present required fields
        /// </summary>
        public List<string> PresentRequiredFields { get; set; } = new();
        
        /// <summary>
        /// Completion percentage (0-100)
        /// </summary>
        public int CompletionPercentage { get; set; }
        
        /// <summary>
        /// Node-level results for this mission
        /// </summary>
        public List<NodeCoverageResult> NodeResults { get; set; } = new();
    }

    /// <summary>
    /// Complete coverage validation result
    /// </summary>
    public class CoverageValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, NodeCoverageResult> NodeResults { get; set; } = new();
        public Dictionary<string, MissionCoverageResult> MissionResults { get; set; } = new();
        
        /// <summary>
        /// Overall completion percentage across all missions
        /// </summary>
        public int OverallCompletionPercentage { get; set; }
        
        /// <summary>
        /// Integrity check results
        /// </summary>
        public Dictionary<string, bool> IntegrityCheckResults { get; set; } = new();
    }

    /// <summary>
    /// Field value provider interface for extracting field values from onboarding data
    /// </summary>
    public interface IFieldValueProvider
    {
        /// <summary>
        /// Get field value by canonical field ID (e.g., "SF.S1.organization_name")
        /// </summary>
        object? GetFieldValue(string fieldId);
        
        /// <summary>
        /// Check if field has a value (not null, not empty, not default)
        /// </summary>
        bool HasFieldValue(string fieldId);
        
        /// <summary>
        /// Get all collected field IDs
        /// </summary>
        HashSet<string> GetCollectedFieldIds();
    }

    /// <summary>
    /// Field registry entry
    /// </summary>
    public class FieldRegistryEntry
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
        public List<string>? AllowedValues { get; set; }
    }

    /// <summary>
    /// Field registry containing all known field definitions
    /// </summary>
    public class FieldRegistry
    {
        public Dictionary<string, FieldRegistryEntry> Fields { get; set; } = new();

        public bool ContainsField(string fieldId) => Fields.ContainsKey(fieldId);
        
        public FieldRegistryEntry? GetField(string fieldId) => 
            Fields.TryGetValue(fieldId, out var entry) ? entry : null;
    }
}
