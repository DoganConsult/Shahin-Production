using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Filters;

/// <summary>
/// Detects duplicate property binding (canonical + alias pairs) by inspecting incoming request keys.
/// - MVC requests: logs a warning and proceeds (backward compatible)
/// - API requests (controllers with [ApiController]): returns 400 BadRequest (strict)
///
/// Only runs on mutating HTTP methods: POST, PUT, PATCH.
///
/// Duplicate detection is "path-aware":
/// - Flags duplicates only when canonical + alias occur under the same key path prefix
///   e.g. Controls[0].ControlId + Controls[0].ControlNumber (duplicate)
///        dto.ControlId + other.ControlNumber (not treated as duplicate)
///
/// Performance note:
/// - Accessing request.Form.Keys forces form parsing for form-urlencoded and multipart/form-data.
/// - For large file uploads (multipart/form-data), this filter will parse the entire form.
/// - If this causes performance issues, consider adding endpoint-specific skip logic.
/// </summary>
public sealed class DuplicatePropertyBindingFilter : IActionFilter
{
    private readonly ILogger<DuplicatePropertyBindingFilter> _logger;

    // Fast-path: Skip form parsing for multipart requests that are likely file uploads
    // This prevents expensive form parsing on large file uploads where duplicate detection is less critical.
    // Set to false if you need duplicate detection on file upload endpoints.
    private const bool SkipMultipartFileUploads = true;

    // Known alias pairs: (canonical, aliases)
    private static readonly Dictionary<string, HashSet<string>> KnownAliasPairs = new(StringComparer.OrdinalIgnoreCase)
    {
        // Control aliases
        { "ControlId", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ControlNumber" } },
        { "Type", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ControlType", "AuditType", "AssessmentType" } },
        { "Frequency", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TestingFrequency" } },
        { "LastTestDate", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LastTestedDate" } },

        // Audit aliases
        { "Name", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Title" } },
        { "PlannedStartDate", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ScheduledDate" } },
        { "LeadAuditor", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "AuditorId" } },

        // Risk aliases
        { "ResidualRisk", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "RiskScore" } },
        { "MitigationStrategy", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TreatmentPlan" } },

        // Assessment aliases
        { "AssignedTo", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "AssessorId" } },
        { "EndDate", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CompletedDate" } },
        { "Description", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Notes" } }
    };

    public DuplicatePropertyBindingFilter(ILogger<DuplicatePropertyBindingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Only check mutating HTTP methods
        var method = context.HttpContext.Request.Method;
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsPatch(method))
            return;

        var request = context.HttpContext.Request;

        // Fast-path: Skip expensive form parsing for large multipart file uploads
        if (SkipMultipartFileUploads && request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Optionally check Content-Length to only skip large uploads (e.g., > 10MB)
            // For now, skip all multipart requests to avoid form parsing overhead
            return;
        }

        // Pull incoming keys from form + query
        var incomingKeys = GetIncomingKeys(request);
        if (incomingKeys.Count == 0)
            return;

        // Build a path-aware map: "prefix path" -> set of leaf field names
        // Example:
        //   "Controls[0].ControlId"    => path="Controls[0]", leaf="ControlId"
        //   "ControlId"               => path="",           leaf="ControlId"
        var pathToLeaves = BuildPathToLeafMap(incomingKeys);

        var duplicates = DetectDuplicates(pathToLeaves);
        if (duplicates.Count == 0)
            return;

        // Detect API requests via endpoint metadata ([ApiController] attribute) rather than path prefix.
        // This is more robust than path-based detection and handles non-standard API routes.
        var isApiRequest = IsApiController(context);
        var dtoTypeNames = GetDtoTypeNames(context);

        foreach (var dup in duplicates)
        {
            // Include path prefix in message when present (helps debug nested editors/collections)
            var location = string.IsNullOrWhiteSpace(dup.PathPrefix) ? "" : $" (path: '{dup.PathPrefix}')";

            if (isApiRequest)
            {
                context.ModelState.AddModelError(
                    dup.Canonical,
                    $"Duplicate property binding detected{location}: both '{dup.Canonical}' and '{dup.Alias}' were provided. " +
                    "These properties map to the same backing field. Use only the canonical property name."
                );

                _logger.LogWarning(
                    "Duplicate property binding rejected for API request. DTOs: {DtoTypes}. Canonical='{Canonical}', Alias='{Alias}', Path='{PathPrefix}'",
                    dtoTypeNames,
                    dup.Canonical,
                    dup.Alias,
                    dup.PathPrefix
                );
            }
            else
            {
                _logger.LogWarning(
                    "Duplicate property binding detected for MVC request. DTOs: {DtoTypes}. Canonical='{Canonical}', Alias='{Alias}', Path='{PathPrefix}'. " +
                    "This may cause unexpected behavior because both map to the same backing field. Prefer canonical names.",
                    dtoTypeNames,
                    dup.Canonical,
                    dup.Alias,
                    dup.PathPrefix
                );
            }
        }

        if (isApiRequest && context.ModelState.ErrorCount > 0)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No-op
    }

    private static HashSet<string> GetIncomingKeys(HttpRequest request)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Form keys (MVC form posts + form-encoded API requests)
        if (request.HasFormContentType)
        {
            foreach (var k in request.Form.Keys)
                keys.Add(k);
        }

        // Query keys (included for completeness)
        foreach (var k in request.Query.Keys)
            keys.Add(k);

        return keys;
    }

    private static Dictionary<string, HashSet<string>> BuildPathToLeafMap(HashSet<string> incomingKeys)
    {
        var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawKey in incomingKeys)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                continue;

            var (pathPrefix, leaf) = SplitPathAndLeaf(rawKey);
            if (string.IsNullOrWhiteSpace(leaf))
                continue;

            if (!map.TryGetValue(pathPrefix, out var leaves))
            {
                leaves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                map[pathPrefix] = leaves;
            }

            leaves.Add(leaf);
        }

        return map;
    }

    // Splits "Controls[0].ControlId" into ("Controls[0]", "ControlId")
    // Splits "dto.ControlId" into ("dto", "ControlId")
    // Splits "ControlId" into ("", "ControlId")
    private static (string PathPrefix, string Leaf) SplitPathAndLeaf(string key)
    {
        var lastDot = key.LastIndexOf('.');
        if (lastDot < 0)
            return ("", key.Trim());

        var prefix = key.Substring(0, lastDot).Trim();
        var leaf = key.Substring(lastDot + 1).Trim();
        return (prefix, leaf);
    }

    private List<(string PathPrefix, string Canonical, string Alias)> DetectDuplicates(
        Dictionary<string, HashSet<string>> pathToLeaves)
    {
        var duplicates = new List<(string, string, string)>();

        foreach (var (pathPrefix, leaves) in pathToLeaves)
        {
            foreach (var (canonical, aliases) in KnownAliasPairs)
            {
                if (!leaves.Contains(canonical))
                    continue;

                foreach (var alias in aliases)
                {
                    if (leaves.Contains(alias))
                        duplicates.Add((pathPrefix, canonical, alias));
                }
            }
        }

        return duplicates;
    }

    private static string GetDtoTypeNames(ActionExecutingContext context)
    {
        // Handles multiple action arguments (more than one DTO parameter)
        var types = context.ActionArguments.Values
            .Where(v => v is not null)
            .Select(v => v!.GetType().Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return types.Length == 0 ? "Unknown" : string.Join(", ", types);
    }

    /// <summary>
    /// Determines if the action belongs to an API controller by checking for [ApiController] attribute
    /// in endpoint metadata. This is more robust than path-based detection.
    /// </summary>
    private static bool IsApiController(ActionExecutingContext context)
    {
        var endpointMetadata = context.ActionDescriptor.EndpointMetadata;
        if (endpointMetadata == null)
            return false;

        // Check for [ApiController] attribute in endpoint metadata
        return endpointMetadata.OfType<ApiControllerAttribute>().Any();
    }
}
