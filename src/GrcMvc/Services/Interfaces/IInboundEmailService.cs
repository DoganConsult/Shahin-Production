using GrcMvc.Controllers.Api;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for processing inbound emails from various providers
/// Handles email routing, parsing, and triggering automation workflows
/// </summary>
public interface IInboundEmailService
{
    /// <summary>
    /// Process an inbound email from any provider
    /// </summary>
    Task ProcessInboundEmailAsync(InboundEmailDto email);

    /// <summary>
    /// Process Microsoft Graph email notification
    /// </summary>
    Task ProcessGraphNotificationAsync(GraphChangeNotification notification);

    /// <summary>
    /// Route email to appropriate handler based on recipient/rules
    /// </summary>
    Task<EmailRoutingResult> RouteEmailAsync(InboundEmailDto email);

    /// <summary>
    /// Parse email for ticket/case creation
    /// </summary>
    Task<ParsedEmailContent> ParseEmailContentAsync(InboundEmailDto email);
}

public class EmailRoutingResult
{
    public bool Routed { get; set; }
    public string? RoutedTo { get; set; }
    public string? RouteType { get; set; } // Ticket, Case, Workflow, User, Queue
    public Guid? EntityId { get; set; }
    public string? Message { get; set; }
}

public class ParsedEmailContent
{
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Priority { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? ExtractedData { get; set; }
}
