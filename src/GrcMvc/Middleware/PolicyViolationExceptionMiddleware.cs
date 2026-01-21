using GrcMvc.Application.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace GrcMvc.Middleware;

/// <summary>
/// Middleware to handle PolicyViolationException and return user-friendly error responses
/// </summary>
public class PolicyViolationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PolicyViolationExceptionMiddleware> _logger;

    public PolicyViolationExceptionMiddleware(
        RequestDelegate next,
        ILogger<PolicyViolationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (PolicyViolationException pve)
        {
            _logger.LogWarning(
                "Policy violation: {Message}. Rule: {RuleId}. User: {UserId}",
                pve.Message, pve.RuleId, context.User?.Identity?.Name);

            await HandlePolicyViolationAsync(context, pve);
        }
    }

    private static async Task HandlePolicyViolationAsync(HttpContext context, PolicyViolationException pve)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        var response = new
        {
            success = false,
            error = new
            {
                code = "Grc:PolicyViolation",
                message = pve.Message,
                ruleId = pve.RuleId,
                remediation = pve.RemediationHint,
                type = "PolicyViolation"
            }
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
