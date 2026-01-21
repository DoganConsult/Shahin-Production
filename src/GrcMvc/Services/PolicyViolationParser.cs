using System.Text.Json;
using GrcMvc.Application.Policy;

namespace GrcMvc.Services;

/// <summary>
/// Helper service to parse policy violation errors from HTTP responses
/// </summary>
public class PolicyViolationParser
{
    /// <summary>
    /// Parse policy violation from HTTP response
    /// </summary>
    public static async Task<PolicyViolationInfo?> ParseFromHttpResponseAsync(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Try to parse as JSON error response
                if (!string.IsNullOrEmpty(content) && (content.TrimStart().StartsWith("{") || content.Contains("Policy Violation")))
                {
                    // Check for policy violation in error message
                    if (content.Contains("Policy Violation") || content.Contains("Grc:PolicyViolation"))
                    {
                        try
                        {
                            var jsonDoc = JsonDocument.Parse(content);
                            var root = jsonDoc.RootElement;
                            
                            // Try to extract violation info from various possible response formats
                            var message = GetJsonProperty(root, "message") ?? GetJsonProperty(root, "error") ?? content;
                            var ruleId = GetJsonProperty(root, "ruleId") ?? GetJsonProperty(root, "rule") ?? "";
                            var remediation = GetJsonProperty(root, "remediation") ?? GetJsonProperty(root, "remediationHint") ?? "";
                            var severity = GetJsonProperty(root, "severity") ?? "medium";
                            
                            // Check if message contains policy violation details
                            if (message.Contains("Policy Violation"))
                            {
                                return new PolicyViolationInfo
                                {
                                    Message = message,
                                    RuleId = ruleId,
                                    RemediationHint = remediation,
                                    Severity = severity,
                                    FailedConditions = ExtractFailedConditions(root)
                                };
                            }
                        }
                        catch (JsonException)
                        {
                            // If JSON parsing fails, try to extract from plain text
                            if (content.Contains("Policy Violation"))
                            {
                                return new PolicyViolationInfo
                                {
                                    Message = content,
                                    RuleId = ExtractRuleIdFromText(content),
                                    RemediationHint = ExtractRemediationFromText(content),
                                    Severity = "medium"
                                };
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Return null if parsing fails - exception details not needed at this level
            // Caller should handle logging if required
        }
        
        return null;
    }

    /// <summary>
    /// Parse policy violation from exception message
    /// </summary>
    public static PolicyViolationInfo? ParseFromException(Exception ex)
    {
        if (ex is PolicyViolationException pvEx)
        {
            return new PolicyViolationInfo
            {
                Message = pvEx.Message,
                RuleId = pvEx.RuleId,
                RemediationHint = pvEx.RemediationHint,
                Severity = "high"
            };
        }

        // Check exception message for policy violation keywords
        if (ex.Message.Contains("Policy Violation") || ex.Message.Contains("Grc:PolicyViolation"))
        {
            return new PolicyViolationInfo
            {
                Message = ex.Message,
                RuleId = ExtractRuleIdFromText(ex.Message),
                RemediationHint = ExtractRemediationFromText(ex.Message),
                Severity = "medium"
            };
        }

        return null;
    }

    private static string? GetJsonProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            return prop.GetString();
        }
        
        // Try case-insensitive match
        foreach (var jsonProp in element.EnumerateObject())
        {
            if (string.Equals(jsonProp.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return jsonProp.Value.GetString();
            }
        }
        
        return null;
    }

    private static List<string>? ExtractFailedConditions(JsonElement root)
    {
        try
        {
            if (root.TryGetProperty("failedConditions", out var conditions) && conditions.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var condition in conditions.EnumerateArray())
                {
                    if (condition.ValueKind == JsonValueKind.String)
                    {
                        list.Add(condition.GetString() ?? "");
                    }
                }
                return list.Count > 0 ? list : null;
            }
        }
        catch (JsonException)
        {
            // Invalid JSON structure - return null gracefully
        }
        
        return null;
    }

    private static string ExtractRuleIdFromText(string text)
    {
        // Try to find rule ID pattern like "REQUIRE_DATA_CLASSIFICATION" or "RuleId: XYZ"
        var ruleIdMatch = System.Text.RegularExpressions.Regex.Match(
            text, 
            @"(?:RuleId|Rule ID|ruleId)[\s:]+([A-Z0-9_]+)|([A-Z0-9_]{3,})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return ruleIdMatch.Success ? (ruleIdMatch.Groups[1].Value ?? ruleIdMatch.Groups[2].Value) : "";
    }

    private static string ExtractRemediationFromText(string text)
    {
        // Try to find remediation hint
        var remediationMatch = System.Text.RegularExpressions.Regex.Match(
            text,
            @"(?:Remediation|How to fix|Solution)[\s:]+(.+?)(?:\n|$)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return remediationMatch.Success ? remediationMatch.Groups[1].Value.Trim() : "";
    }
}
