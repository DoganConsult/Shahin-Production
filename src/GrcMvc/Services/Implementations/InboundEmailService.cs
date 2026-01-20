using System.Text.RegularExpressions;
using GrcMvc.Controllers.Api;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for processing inbound emails and triggering automation
/// </summary>
public class InboundEmailService : IInboundEmailService
{
    private readonly GrcDbContext _context;
    private readonly IWebhookService _webhookService;
    private readonly IUserNotificationDispatcher _notificationDispatcher;
    private readonly ILogger<InboundEmailService> _logger;

    public InboundEmailService(
        GrcDbContext context,
        IWebhookService webhookService,
        IUserNotificationDispatcher notificationDispatcher,
        ILogger<InboundEmailService> logger)
    {
        _context = context;
        _webhookService = webhookService;
        _notificationDispatcher = notificationDispatcher;
        _logger = logger;
    }

    public async Task ProcessInboundEmailAsync(InboundEmailDto email)
    {
        _logger.LogInformation("Processing inbound email from {Provider}: {From} -> {To}", 
            email.Provider, email.From, email.To);

        try
        {
            // 1. Parse email content
            var parsedContent = await ParseEmailContentAsync(email);

            // 2. Store the inbound email record
            var inboundRecord = new InboundEmailRecord
            {
                Id = Guid.NewGuid(),
                Provider = email.Provider ?? "Unknown",
                FromAddress = email.From ?? "",
                ToAddress = email.To ?? "",
                Subject = email.Subject ?? "",
                TextBody = email.TextBody,
                HtmlBody = email.HtmlBody,
                MessageId = email.MessageId,
                ReceivedAt = email.ReceivedAt,
                Status = "Received",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<InboundEmailRecord>().Add(inboundRecord);
            await _context.SaveChangesAsync();

            // 3. Route the email
            var routingResult = await RouteEmailAsync(email);

            // 4. Update record with routing info
            inboundRecord.RoutedTo = routingResult.RoutedTo;
            inboundRecord.RouteType = routingResult.RouteType;
            inboundRecord.EntityId = routingResult.EntityId;
            inboundRecord.Status = routingResult.Routed ? "Routed" : "Unrouted";
            inboundRecord.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 5. Trigger automation events
            await TriggerEmailAutomationAsync(email, parsedContent, routingResult);

            _logger.LogInformation("Inbound email processed successfully: {RecordId}, RouteType={RouteType}", 
                inboundRecord.Id, routingResult.RouteType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound email from {From}", email.From);
            throw;
        }
    }

    public async Task ProcessGraphNotificationAsync(GraphChangeNotification notification)
    {
        _logger.LogInformation("Processing Graph notification: {ChangeType} on {Resource}", 
            notification.ChangeType, notification.Resource);

        // Extract email ID from resource path (e.g., "users/{id}/messages/{messageId}")
        var resourceParts = notification.Resource?.Split('/') ?? Array.Empty<string>();
        if (resourceParts.Length >= 4 && resourceParts[2] == "messages")
        {
            var messageId = resourceParts[3];
            
            // Trigger notification for new email
            await _notificationDispatcher.DispatchAsync(new UserNotification
            {
                Type = "NewEmail",
                Title = "New Email Received",
                Message = $"New email via Microsoft Graph: {messageId}",
                Priority = "Normal",
                Data = new Dictionary<string, object>
                {
                    ["messageId"] = messageId,
                    ["changeType"] = notification.ChangeType ?? "unknown",
                    ["resource"] = notification.Resource ?? ""
                }
            });
        }

        await Task.CompletedTask;
    }

    public async Task<EmailRoutingResult> RouteEmailAsync(InboundEmailDto email)
    {
        var result = new EmailRoutingResult { Routed = false };

        try
        {
            // 1. Check for ticket reference in subject (e.g., [TICKET-12345])
            var ticketMatch = Regex.Match(email.Subject ?? "", @"\[TICKET-(\d+)\]", RegexOptions.IgnoreCase);
            if (ticketMatch.Success)
            {
                var ticketNumber = ticketMatch.Groups[1].Value;
                result.Routed = true;
                result.RouteType = "Ticket";
                result.RoutedTo = $"Ticket #{ticketNumber}";
                result.Message = "Routed to existing ticket";
                return result;
            }

            // 2. Check for case reference (e.g., [CASE-ABC123])
            var caseMatch = Regex.Match(email.Subject ?? "", @"\[CASE-([A-Z0-9]+)\]", RegexOptions.IgnoreCase);
            if (caseMatch.Success)
            {
                var caseRef = caseMatch.Groups[1].Value;
                result.Routed = true;
                result.RouteType = "Case";
                result.RoutedTo = $"Case {caseRef}";
                result.Message = "Routed to existing case";
                return result;
            }

            // 3. Check routing rules based on recipient
            var routingRule = await _context.Set<EmailRoutingRule>()
                .Where(r => r.IsActive && !r.IsDeleted)
                .Where(r => email.To != null && email.To.Contains(r.MatchPattern))
                .OrderBy(r => r.Priority)
                .FirstOrDefaultAsync();

            if (routingRule != null)
            {
                result.Routed = true;
                result.RouteType = routingRule.RouteType;
                result.RoutedTo = routingRule.RouteDestination;
                result.Message = $"Matched rule: {routingRule.Name}";
                return result;
            }

            // 4. Check if sender is a known user
            var senderEmail = ExtractEmailAddress(email.From);
            if (!string.IsNullOrEmpty(senderEmail))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == senderEmail.ToLower());

                if (user != null)
                {
                    result.Routed = true;
                    result.RouteType = "User";
                    result.RoutedTo = user.Id.ToString();
                    result.Message = "Routed to known user";
                    return result;
                }
            }

            // 5. Route to default queue
            result.Routed = true;
            result.RouteType = "Queue";
            result.RoutedTo = "default-inbox";
            result.Message = "Routed to default queue";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing email from {From}", email.From);
            result.Message = ex.Message;
            return result;
        }
    }

    public Task<ParsedEmailContent> ParseEmailContentAsync(InboundEmailDto email)
    {
        var parsed = new ParsedEmailContent
        {
            Subject = email.Subject,
            Body = email.TextBody ?? StripHtml(email.HtmlBody),
            Tags = new List<string>(),
            ExtractedData = new Dictionary<string, string>()
        };

        // Extract sender info
        if (!string.IsNullOrEmpty(email.From))
        {
            var match = Regex.Match(email.From, @"^(?:""?([^""<]+)""?\s*)?<?([^>]+@[^>]+)>?$");
            if (match.Success)
            {
                parsed.SenderName = match.Groups[1].Value.Trim();
                parsed.SenderEmail = match.Groups[2].Value.Trim();
            }
            else
            {
                parsed.SenderEmail = email.From;
            }
        }

        // Detect priority from subject
        if (email.Subject?.Contains("[URGENT]", StringComparison.OrdinalIgnoreCase) == true ||
            email.Subject?.Contains("URGENT:", StringComparison.OrdinalIgnoreCase) == true)
        {
            parsed.Priority = "High";
            parsed.Tags.Add("urgent");
        }
        else if (email.Subject?.Contains("[LOW]", StringComparison.OrdinalIgnoreCase) == true)
        {
            parsed.Priority = "Low";
        }
        else
        {
            parsed.Priority = "Normal";
        }

        // Extract common patterns
        ExtractPatterns(parsed, email.Subject + " " + parsed.Body);

        return Task.FromResult(parsed);
    }

    private async Task TriggerEmailAutomationAsync(InboundEmailDto email, ParsedEmailContent parsed, EmailRoutingResult routing)
    {
        // Get tenant ID from routing or default
        Guid? tenantId = null;

        // Trigger webhook event for email automation
        if (tenantId.HasValue)
        {
            await _webhookService.TriggerEventAsync(
                tenantId.Value,
                "email.inbound",
                Guid.NewGuid().ToString(),
                new
                {
                    from = email.From,
                    to = email.To,
                    subject = email.Subject,
                    routing = new
                    {
                        routed = routing.Routed,
                        routeType = routing.RouteType,
                        routedTo = routing.RoutedTo
                    },
                    parsed = new
                    {
                        senderEmail = parsed.SenderEmail,
                        senderName = parsed.SenderName,
                        priority = parsed.Priority,
                        tags = parsed.Tags
                    },
                    receivedAt = email.ReceivedAt
                });
        }

        // Dispatch user notifications based on routing
        if (routing.RouteType == "User" && !string.IsNullOrEmpty(routing.RoutedTo))
        {
            await _notificationDispatcher.DispatchToUserAsync(routing.RoutedTo, new UserNotification
            {
                Type = "InboundEmail",
                Title = $"New email from {parsed.SenderName ?? parsed.SenderEmail}",
                Message = email.Subject ?? "No subject",
                Priority = parsed.Priority ?? "Normal",
                Data = new Dictionary<string, object>
                {
                    ["from"] = email.From ?? "",
                    ["subject"] = email.Subject ?? ""
                }
            });
        }
    }

    private string? ExtractEmailAddress(string? fromHeader)
    {
        if (string.IsNullOrEmpty(fromHeader)) return null;
        var match = Regex.Match(fromHeader, @"<([^>]+@[^>]+)>|([^\s<>]+@[^\s<>]+)");
        return match.Success ? (match.Groups[1].Value ?? match.Groups[2].Value) : null;
    }

    private string? StripHtml(string? html)
    {
        if (string.IsNullOrEmpty(html)) return null;
        return Regex.Replace(html, "<[^>]*>", " ").Trim();
    }

    private void ExtractPatterns(ParsedEmailContent parsed, string? text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Extract phone numbers
        var phoneMatch = Regex.Match(text, @"\b(\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b");
        if (phoneMatch.Success)
        {
            parsed.ExtractedData["phone"] = phoneMatch.Value;
        }

        // Extract order/reference numbers
        var refMatch = Regex.Match(text, @"\b(?:order|ref|reference|ticket|case)[:\s#]*([A-Z0-9-]+)\b", RegexOptions.IgnoreCase);
        if (refMatch.Success)
        {
            parsed.ExtractedData["reference"] = refMatch.Groups[1].Value;
        }
    }
}

// Entity for storing inbound emails
public class InboundEmailRecord
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string ToAddress { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public string? MessageId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Status { get; set; } = "Received";
    public string? RoutedTo { get; set; }
    public string? RouteType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

// Entity for email routing rules
public class EmailRoutingRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string MatchPattern { get; set; } = "";
    public string MatchType { get; set; } = "Contains"; // Contains, Regex, Exact
    public string RouteType { get; set; } = "Queue"; // Queue, User, Workflow, Ticket
    public string RouteDestination { get; set; } = "";
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
