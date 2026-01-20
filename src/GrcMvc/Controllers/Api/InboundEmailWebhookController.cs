using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Webhook controller for receiving inbound emails from email providers
/// Supports: SendGrid, Mailgun, Postmark, AWS SES, Microsoft Graph
/// </summary>
[ApiController]
[Route("api/webhooks/email")]
[AllowAnonymous] // Webhooks need to be accessible without auth
public class InboundEmailWebhookController : ControllerBase
{
    private readonly IInboundEmailService _inboundEmailService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<InboundEmailWebhookController> _logger;
    private readonly IConfiguration _configuration;

    public InboundEmailWebhookController(
        IInboundEmailService inboundEmailService,
        IWebhookService webhookService,
        ILogger<InboundEmailWebhookController> logger,
        IConfiguration configuration)
    {
        _inboundEmailService = inboundEmailService;
        _webhookService = webhookService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// SendGrid Inbound Parse webhook
    /// POST /api/webhooks/email/sendgrid
    /// </summary>
    [HttpPost("sendgrid")]
    public async Task<IActionResult> ReceiveSendGridEmail()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            
            var inboundEmail = new InboundEmailDto
            {
                Provider = "SendGrid",
                From = form["from"].ToString(),
                To = form["to"].ToString(),
                Subject = form["subject"].ToString(),
                TextBody = form["text"].ToString(),
                HtmlBody = form["html"].ToString(),
                Headers = form["headers"].ToString(),
                Envelope = form["envelope"].ToString(),
                ReceivedAt = DateTime.UtcNow
            };

            // Handle attachments
            if (form.Files.Count > 0)
            {
                inboundEmail.Attachments = new List<InboundAttachmentDto>();
                foreach (var file in form.Files)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    inboundEmail.Attachments.Add(new InboundAttachmentDto
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        Size = file.Length,
                        Content = ms.ToArray()
                    });
                }
            }

            await _inboundEmailService.ProcessInboundEmailAsync(inboundEmail);
            
            _logger.LogInformation("SendGrid inbound email processed: From={From}, Subject={Subject}", 
                inboundEmail.From, inboundEmail.Subject);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid inbound email");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Mailgun Inbound webhook
    /// POST /api/webhooks/email/mailgun
    /// </summary>
    [HttpPost("mailgun")]
    public async Task<IActionResult> ReceiveMailgunEmail()
    {
        try
        {
            var form = await Request.ReadFormAsync();

            // Verify Mailgun signature
            var timestamp = form["timestamp"].ToString();
            var token = form["token"].ToString();
            var signature = form["signature"].ToString();

            if (!VerifyMailgunSignature(timestamp, token, signature))
            {
                _logger.LogWarning("Invalid Mailgun signature");
                return Unauthorized();
            }

            var inboundEmail = new InboundEmailDto
            {
                Provider = "Mailgun",
                From = form["sender"].ToString(),
                To = form["recipient"].ToString(),
                Subject = form["subject"].ToString(),
                TextBody = form["body-plain"].ToString(),
                HtmlBody = form["body-html"].ToString(),
                MessageId = form["Message-Id"].ToString(),
                ReceivedAt = DateTime.UtcNow
            };

            await _inboundEmailService.ProcessInboundEmailAsync(inboundEmail);
            
            _logger.LogInformation("Mailgun inbound email processed: From={From}, Subject={Subject}", 
                inboundEmail.From, inboundEmail.Subject);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Mailgun inbound email");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Microsoft Graph webhook for email notifications
    /// POST /api/webhooks/email/graph
    /// </summary>
    [HttpPost("graph")]
    public async Task<IActionResult> ReceiveGraphNotification([FromQuery] string? validationToken = null)
    {
        // Handle subscription validation
        if (!string.IsNullOrEmpty(validationToken))
        {
            _logger.LogInformation("Graph webhook validation request received");
            return Content(validationToken, "text/plain");
        }

        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var notification = JsonSerializer.Deserialize<GraphNotification>(body);

            if (notification?.Value != null)
            {
                foreach (var change in notification.Value)
                {
                    await _inboundEmailService.ProcessGraphNotificationAsync(change);
                    _logger.LogInformation("Graph notification processed: Resource={Resource}", change.Resource);
                }
            }

            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Graph notification");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// AWS SES Inbound webhook (via SNS)
    /// POST /api/webhooks/email/ses
    /// </summary>
    [HttpPost("ses")]
    public async Task<IActionResult> ReceiveSesEmail()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Check if this is an SNS subscription confirmation
            if (Request.Headers.ContainsKey("x-amz-sns-message-type"))
            {
                var messageType = Request.Headers["x-amz-sns-message-type"].ToString();
                if (messageType == "SubscriptionConfirmation")
                {
                    var snsMessage = JsonSerializer.Deserialize<SnsSubscriptionConfirmation>(body);
                    _logger.LogInformation("SES SNS subscription confirmation: {SubscribeUrl}", snsMessage?.SubscribeURL);
                    // In production, automatically confirm by calling the SubscribeURL
                    return Ok();
                }
            }

            var sesNotification = JsonSerializer.Deserialize<SesNotification>(body);
            if (sesNotification?.Message != null)
            {
                var inboundEmail = new InboundEmailDto
                {
                    Provider = "AWS_SES",
                    MessageId = sesNotification.MessageId,
                    RawMessage = sesNotification.Message,
                    ReceivedAt = DateTime.UtcNow
                };

                await _inboundEmailService.ProcessInboundEmailAsync(inboundEmail);
                _logger.LogInformation("SES inbound email processed: MessageId={MessageId}", sesNotification.MessageId);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SES inbound email");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Generic webhook for custom email providers
    /// POST /api/webhooks/email/custom
    /// </summary>
    [HttpPost("custom")]
    public async Task<IActionResult> ReceiveCustomEmail([FromBody] InboundEmailDto inboundEmail)
    {
        try
        {
            // Verify webhook signature if configured
            if (Request.Headers.TryGetValue("X-Webhook-Signature", out var signature))
            {
                var secret = _configuration["Webhooks:InboundEmail:Secret"];
                if (!string.IsNullOrEmpty(secret))
                {
                    using var reader = new StreamReader(Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    if (!_webhookService.VerifySignature(body, signature.ToString(), secret))
                    {
                        _logger.LogWarning("Invalid custom webhook signature");
                        return Unauthorized();
                    }
                }
            }

            inboundEmail.Provider ??= "Custom";
            inboundEmail.ReceivedAt = DateTime.UtcNow;

            await _inboundEmailService.ProcessInboundEmailAsync(inboundEmail);
            
            _logger.LogInformation("Custom inbound email processed: From={From}, Subject={Subject}", 
                inboundEmail.From, inboundEmail.Subject);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing custom inbound email");
            return StatusCode(500);
        }
    }

    private bool VerifyMailgunSignature(string timestamp, string token, string signature)
    {
        var apiKey = _configuration["Mailgun:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return true; // Skip verification if not configured

        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
        var computed = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + token)))
            .Replace("-", "").ToLower();
        return computed == signature;
    }
}

// DTOs for inbound email handling
public class InboundEmailDto
{
    public string? Provider { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public string? Headers { get; set; }
    public string? Envelope { get; set; }
    public string? MessageId { get; set; }
    public string? RawMessage { get; set; }
    public DateTime ReceivedAt { get; set; }
    public List<InboundAttachmentDto>? Attachments { get; set; }
}

public class InboundAttachmentDto
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public byte[]? Content { get; set; }
}

public class GraphNotification
{
    public List<GraphChangeNotification>? Value { get; set; }
}

public class GraphChangeNotification
{
    public string? ChangeType { get; set; }
    public string? ClientState { get; set; }
    public string? Resource { get; set; }
    public string? SubscriptionId { get; set; }
    public DateTime? SubscriptionExpirationDateTime { get; set; }
}

public class SnsSubscriptionConfirmation
{
    public string? Type { get; set; }
    public string? MessageId { get; set; }
    public string? Token { get; set; }
    public string? TopicArn { get; set; }
    public string? SubscribeURL { get; set; }
}

public class SesNotification
{
    public string? Type { get; set; }
    public string? MessageId { get; set; }
    public string? Message { get; set; }
}
