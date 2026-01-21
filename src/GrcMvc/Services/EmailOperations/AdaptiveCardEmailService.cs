using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrcMvc.Models.Entities.EmailOperations;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Service for generating Adaptive Cards for email notifications
/// Reference: https://adaptivecards.io/
/// </summary>
public class AdaptiveCardEmailService
{
    private readonly ILogger<AdaptiveCardEmailService> _logger;

    public AdaptiveCardEmailService(ILogger<AdaptiveCardEmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate Adaptive Card for new email notification
    /// </summary>
    public string GenerateEmailNotificationCard(EmailMessage email, EmailThread thread)
    {
        var card = new
        {
            type = "AdaptiveCard",
            body = new object[]
            {
                new
                {
                    type = "TextBlock",
                    size = "Medium",
                    weight = "Bolder",
                    text = $"📧 New Email: {email.Subject}"
                },
                new
                {
                    type = "ColumnSet",
                    columns = new object[]
                    {
                        new
                        {
                            type = "Column",
                            items = new object[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    weight = "Bolder",
                                    text = "From:",
                                    wrap = true
                                },
                                new
                                {
                                    type = "TextBlock",
                                    text = email.FromName ?? email.FromEmail,
                                    wrap = true
                                }
                            },
                            width = "stretch"
                        },
                        new
                        {
                            type = "Column",
                            items = new object[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    weight = "Bolder",
                                    text = "Received:",
                                    wrap = true
                                },
                                new
                                {
                                    type = "TextBlock",
                                    text = email.ReceivedAt.ToString("yyyy-MM-dd HH:mm"),
                                    wrap = true
                                }
                            },
                            width = "auto"
                        }
                    }
                },
                new
                {
                    type = "TextBlock",
                    text = email.BodyPreview ?? "No preview available",
                    wrap = true,
                    maxLines = 3
                },
                new
                {
                    type = "FactSet",
                    facts = new object[]
                    {
                        new { title = "Thread ID:", value = thread.Id.ToString() },
                        new { title = "Status:", value = thread.Status.ToString() },
                        new { title = "Priority:", value = thread.Priority.ToString() },
                        new { title = "Classification:", value = thread.Classification.ToString() }
                    }
                }
            },
            actions = new object[]
            {
                new
                {
                    type = "Action.OpenUrl",
                    title = "View in Portal",
                    url = $"https://portal.shahin-ai.com/email/threads/{thread.Id}"
                },
                new
                {
                    type = "Action.Submit",
                    title = "Mark as Read",
                    data = new
                    {
                        action = "markAsRead",
                        threadId = thread.Id.ToString(),
                        messageId = email.Id.ToString()
                    }
                }
            },
            version = "1.6"
        };

        // SECURITY: Add $schema property using dictionary to avoid C# syntax error
        var cardDict = new Dictionary<string, object>
        {
            ["type"] = card.type,
            ["body"] = card.body,
            ["actions"] = card.actions,
            ["version"] = card.version,
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json"
        };

        return JsonSerializer.Serialize(cardDict, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Generate Adaptive Card for auto-reply notification
    /// </summary>
    public string GenerateAutoReplyCard(string recipientEmail, string subject, string replyContent, string ruleName)
    {
        // Clean HTML from reply content for display
        var cleanContent = replyContent?
            .Replace("<html>", "")
            .Replace("</html>", "")
            .Replace("<body>", "")
            .Replace("</body>", "")
            .Replace("<p>", "")
            .Replace("</p>", "\n")
            .Replace("<br>", "\n")
            .Replace("&nbsp;", " ")
            .Trim() ?? "Auto-reply sent";

        var card = new
        {
            type = "AdaptiveCard",
            body = new object[]
            {
                new
                {
                    type = "TextBlock",
                    size = "Medium",
                    weight = "Bolder",
                    text = "✅ Auto-Reply Sent",
                    color = "Good"
                },
                new
                {
                    type = "TextBlock",
                    text = $"An automatic reply was sent for: **{subject}**",
                    wrap = true,
                    spacing = "Medium"
                },
                new
                {
                    type = "FactSet",
                    facts = new object[]
                    {
                        new { title = "To:", value = recipientEmail },
                        new { title = "Rule:", value = ruleName },
                        new { title = "Sent At:", value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") }
                    }
                },
                new
                {
                    type = "Container",
                    items = new object[]
                    {
                        new
                        {
                            type = "TextBlock",
                            text = "Reply Content:",
                            weight = "Bolder",
                            spacing = "Medium"
                        },
                        new
                        {
                            type = "TextBlock",
                            text = cleanContent,
                            wrap = true,
                            maxLines = 10
                        }
                    },
                    style = "emphasis"
                }
            },
            version = "1.6"
        };

        // SECURITY: Add $schema property using dictionary to avoid C# syntax error
        var cardDict = new Dictionary<string, object>
        {
            ["type"] = card.type,
            ["body"] = card.body,
            ["version"] = card.version,
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json"
        };

        return JsonSerializer.Serialize(cardDict, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Generate Adaptive Card for unmatched email (needs review)
    /// </summary>
    public string GenerateUnmatchedEmailCard(EmailMessage email, EmailThread thread)
    {
        var card = new
        {
            type = "AdaptiveCard",
            body = new object[]
            {
                new
                {
                    type = "TextBlock",
                    size = "Medium",
                    weight = "Bolder",
                    text = "⚠️ Email Needs Review",
                    color = "Warning"
                },
                new
                {
                    type = "TextBlock",
                    text = $"Email received but no auto-reply rule matched: **{email.Subject}**",
                    wrap = true,
                    spacing = "Medium"
                },
                new
                {
                    type = "FactSet",
                    facts = new object[]
                    {
                        new { title = "From:", value = email.FromEmail },
                        new { title = "Received:", value = email.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss UTC") },
                        new { title = "Classification:", value = thread.Classification.ToString() },
                        new { title = "Priority:", value = thread.Priority.ToString() }
                    }
                },
                new
                {
                    type = "TextBlock",
                    text = email.BodyPreview ?? "No preview available",
                    wrap = true,
                    maxLines = 5,
                    spacing = "Medium"
                }
            },
            actions = new object[]
            {
                new
                {
                    type = "Action.OpenUrl",
                    title = "Review in Portal",
                    url = $"https://portal.shahin-ai.com/email/threads/{thread.Id}"
                },
                new
                {
                    type = "Action.Submit",
                    title = "Create Reply Rule",
                    data = new
                    {
                        action = "createRule",
                        threadId = thread.Id.ToString(),
                        classification = thread.Classification.ToString()
                    }
                }
            },
            version = "1.6"
        };

        // SECURITY: Add $schema property using dictionary to avoid C# syntax error
        var cardDict = new Dictionary<string, object>
        {
            ["type"] = card.type,
            ["body"] = card.body,
            ["actions"] = card.actions,
            ["version"] = card.version,
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json"
        };

        return JsonSerializer.Serialize(cardDict, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        });
    }
}
