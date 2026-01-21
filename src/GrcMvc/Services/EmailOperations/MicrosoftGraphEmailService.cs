using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Implementation of Microsoft Graph API for email operations
/// Uses Client Credentials flow for daemon/worker scenarios
/// </summary>
public class MicrosoftGraphEmailService : IMicrosoftGraphEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MicrosoftGraphEmailService> _logger;
    private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";
    private const string TokenUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";

    public MicrosoftGraphEmailService(
        HttpClient httpClient,
        ILogger<MicrosoftGraphEmailService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get access token using Client Credentials flow (Application permissions)
    /// </summary>
    public async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret)
    {
        var url = string.Format(TokenUrl, tenantId);
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });

        var response = await _httpClient.PostAsync(url, content);
        var json = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get access token: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to get access token: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString() ?? 
            throw new Exception("No access token in response");
    }

    /// <summary>
    /// Get messages from a mailbox
    /// </summary>
    public async Task<List<GraphEmailMessage>> GetMessagesAsync(
        string accessToken,
        string mailboxId,
        DateTime? since = null,
        string? folderId = null,
        int top = 50)
    {
        var folder = folderId ?? "inbox";
        var url = $"{GraphBaseUrl}/users/{mailboxId}/mailFolders/{folder}/messages?$top={top}&$orderby=receivedDateTime desc";
        
        if (since.HasValue)
        {
            url += $"&$filter=receivedDateTime ge {since.Value:yyyy-MM-ddTHH:mm:ssZ}";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get messages: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to get messages: {response.StatusCode}");
        }

        var messages = new List<GraphEmailMessage>();
        using var doc = JsonDocument.Parse(json);
        
        if (doc.RootElement.TryGetProperty("value", out var valueArray))
        {
            foreach (var item in valueArray.EnumerateArray())
            {
                messages.Add(ParseMessage(item));
            }
        }

        return messages;
    }

    /// <summary>
    /// Get a single message with full body
    /// </summary>
    public async Task<GraphEmailMessage?> GetMessageAsync(string accessToken, string mailboxId, string messageId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get message: {StatusCode}", response.StatusCode);
            throw new Exception($"Failed to get message: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        return ParseMessage(doc.RootElement);
    }

    /// <summary>
    /// Get attachments for a message
    /// </summary>
    public async Task<List<GraphAttachment>> GetAttachmentsAsync(string accessToken, string mailboxId, string messageId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}/attachments";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get attachments: {StatusCode}", response.StatusCode);
            return new List<GraphAttachment>();
        }

        var attachments = new List<GraphAttachment>();
        using var doc = JsonDocument.Parse(json);
        
        if (doc.RootElement.TryGetProperty("value", out var valueArray))
        {
            foreach (var item in valueArray.EnumerateArray())
            {
                attachments.Add(new GraphAttachment
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    ContentType = item.TryGetProperty("contentType", out var ct) ? ct.GetString() ?? "" : "",
                    Size = item.TryGetProperty("size", out var size) ? size.GetInt32() : 0,
                    IsInline = item.TryGetProperty("isInline", out var inline) && inline.GetBoolean(),
                    ContentId = item.TryGetProperty("contentId", out var cid) ? cid.GetString() : null
                });
            }
        }

        return attachments;
    }

    /// <summary>
    /// Download attachment content
    /// </summary>
    public async Task<byte[]> DownloadAttachmentAsync(string accessToken, string mailboxId, string messageId, string attachmentId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}/attachments/{attachmentId}/$value";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to download attachment: {StatusCode}", response.StatusCode);
            throw new Exception($"Failed to download attachment: {response.StatusCode}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Create a draft reply
    /// </summary>
    public async Task<GraphEmailMessage> CreateReplyDraftAsync(
        string accessToken,
        string mailboxId,
        string messageId,
        string replyBody,
        bool replyAll = false)
    {
        var action = replyAll ? "createReplyAll" : "createReply";
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}/{action}";
        
        var payload = new
        {
            message = new
            {
                body = new
                {
                    contentType = "html",
                    content = replyBody
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create reply draft: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to create reply draft: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        return ParseMessage(doc.RootElement);
    }

    /// <summary>
    /// Send a draft message
    /// </summary>
    public async Task SendDraftAsync(string accessToken, string mailboxId, string draftId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{draftId}/send";
        
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send draft: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to send draft: {response.StatusCode}");
        }

        _logger.LogInformation("Draft {DraftId} sent successfully", draftId);
    }

    /// <summary>
    /// Send a new message directly
    /// </summary>
    public async Task SendMessageAsync(
        string accessToken,
        string mailboxId,
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        List<string>? ccEmails = null)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/sendMail";
        
        var toRecipients = new List<object> { new { emailAddress = new { address = toEmail } } };
        var ccRecipients = ccEmails?.Select(e => new { emailAddress = new { address = e } }).ToList();

        var payload = new
        {
            message = new
            {
                subject = subject,
                body = new
                {
                    contentType = isHtml ? "html" : "text",
                    content = body
                },
                toRecipients = toRecipients,
                ccRecipients = ccRecipients
            },
            saveToSentItems = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send message: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to send message: {response.StatusCode}");
        }

        _logger.LogInformation("Message sent to {ToEmail}", toEmail);
    }

    /// <summary>
    /// Send reply directly without creating draft
    /// </summary>
    public async Task SendReplyAsync(
        string accessToken,
        string mailboxId,
        string messageId,
        string replyBody,
        bool replyAll = false)
    {
        var action = replyAll ? "replyAll" : "reply";
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}/{action}";
        
        var payload = new
        {
            message = new
            {
                body = new
                {
                    contentType = "html",
                    content = replyBody
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send reply: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to send reply: {response.StatusCode}");
        }

        _logger.LogInformation("Reply sent to message {MessageId}", messageId);
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    public async Task MarkAsReadAsync(string accessToken, string mailboxId, string messageId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}";
        
        var payload = new { isRead = true };

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to mark message as read: {StatusCode}", response.StatusCode);
        }
    }

    /// <summary>
    /// Move message to folder
    /// </summary>
    public async Task MoveMessageAsync(string accessToken, string mailboxId, string messageId, string destinationFolderId)
    {
        var url = $"{GraphBaseUrl}/users/{mailboxId}/messages/{messageId}/move";
        
        var payload = new { destinationId = destinationFolderId };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to move message: {StatusCode}", response.StatusCode);
        }
    }

    /// <summary>
    /// Create webhook subscription for new messages
    /// </summary>
    public async Task<GraphSubscription> CreateSubscriptionAsync(
        string accessToken,
        string mailboxId,
        string webhookUrl,
        int expirationMinutes = 4230)
    {
        var url = $"{GraphBaseUrl}/subscriptions";
        
        var payload = new
        {
            changeType = "created",
            notificationUrl = webhookUrl,
            resource = $"/users/{mailboxId}/mailFolders/inbox/messages",
            expirationDateTime = DateTime.UtcNow.AddMinutes(expirationMinutes).ToString("o"),
            clientState = Guid.NewGuid().ToString()
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create subscription: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to create subscription: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        return new GraphSubscription
        {
            Id = doc.RootElement.GetProperty("id").GetString() ?? "",
            Resource = doc.RootElement.GetProperty("resource").GetString() ?? "",
            ExpirationDateTime = doc.RootElement.GetProperty("expirationDateTime").GetDateTime()
        };
    }

    /// <summary>
    /// Renew webhook subscription
    /// </summary>
    public async Task<GraphSubscription> RenewSubscriptionAsync(string accessToken, string subscriptionId, int expirationMinutes = 4230)
    {
        var url = $"{GraphBaseUrl}/subscriptions/{subscriptionId}";
        
        var payload = new
        {
            expirationDateTime = DateTime.UtcNow.AddMinutes(expirationMinutes).ToString("o")
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to renew subscription: {StatusCode}", response.StatusCode);
            throw new Exception($"Failed to renew subscription: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(json);
        return new GraphSubscription
        {
            Id = doc.RootElement.GetProperty("id").GetString() ?? "",
            Resource = doc.RootElement.GetProperty("resource").GetString() ?? "",
            ExpirationDateTime = doc.RootElement.GetProperty("expirationDateTime").GetDateTime()
        };
    }

    /// <summary>
    /// Delete webhook subscription
    /// </summary>
    public async Task DeleteSubscriptionAsync(string accessToken, string subscriptionId)
    {
        var url = $"{GraphBaseUrl}/subscriptions/{subscriptionId}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Failed to delete subscription: {StatusCode}", response.StatusCode);
        }
    }

    private GraphEmailMessage ParseMessage(JsonElement element)
    {
        var message = new GraphEmailMessage
        {
            Id = element.GetProperty("id").GetString() ?? "",
            Subject = element.TryGetProperty("subject", out var subj) ? subj.GetString() ?? "" : "",
            BodyPreview = element.TryGetProperty("bodyPreview", out var bp) ? bp.GetString() : null,
            IsRead = element.TryGetProperty("isRead", out var ir) && ir.GetBoolean(),
            HasAttachments = element.TryGetProperty("hasAttachments", out var ha) && ha.GetBoolean(),
            Importance = element.TryGetProperty("importance", out var imp) ? imp.GetString() ?? "normal" : "normal",
            IsDraft = element.TryGetProperty("isDraft", out var draft) && draft.GetBoolean()
        };

        if (element.TryGetProperty("internetMessageId", out var imi))
            message.InternetMessageId = imi.GetString();

        if (element.TryGetProperty("conversationId", out var ci))
            message.ConversationId = ci.GetString();

        if (element.TryGetProperty("receivedDateTime", out var rd))
            message.ReceivedDateTime = rd.GetDateTime();

        if (element.TryGetProperty("sentDateTime", out var sd))
            message.SentDateTime = sd.GetDateTime();

        if (element.TryGetProperty("body", out var body))
        {
            message.Body = new GraphEmailBody
            {
                ContentType = body.TryGetProperty("contentType", out var bct) ? bct.GetString() ?? "text" : "text",
                Content = body.TryGetProperty("content", out var bc) ? bc.GetString() ?? "" : ""
            };
        }

        if (element.TryGetProperty("from", out var from) && from.TryGetProperty("emailAddress", out var fromAddr))
        {
            message.From = new GraphEmailAddress
            {
                Address = fromAddr.GetProperty("address").GetString() ?? "",
                Name = fromAddr.TryGetProperty("name", out var fn) ? fn.GetString() : null
            };
        }

        if (element.TryGetProperty("toRecipients", out var toArray))
        {
            foreach (var to in toArray.EnumerateArray())
            {
                if (to.TryGetProperty("emailAddress", out var toAddr))
                {
                    message.ToRecipients.Add(new GraphEmailAddress
                    {
                        Address = toAddr.GetProperty("address").GetString() ?? "",
                        Name = toAddr.TryGetProperty("name", out var tn) ? tn.GetString() : null
                    });
                }
            }
        }

        if (element.TryGetProperty("ccRecipients", out var ccArray))
        {
            foreach (var cc in ccArray.EnumerateArray())
            {
                if (cc.TryGetProperty("emailAddress", out var ccAddr))
                {
                    message.CcRecipients.Add(new GraphEmailAddress
                    {
                        Address = ccAddr.GetProperty("address").GetString() ?? "",
                        Name = ccAddr.TryGetProperty("name", out var cn) ? cn.GetString() : null
                    });
                }
            }
        }

        return message;
    }

    /// <summary>
    /// Get users from Microsoft 365 tenant
    /// </summary>
    public async Task<List<GraphUser>> GetUsersAsync(string? emailFilter = null)
    {
        // Get access token using configuration
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        var tenantId = config["EmailOperations:MicrosoftGraph:TenantId"] 
            ?? throw new Exception("TenantId not configured");
        var clientId = config["EmailOperations:MicrosoftGraph:ClientId"] 
            ?? throw new Exception("ClientId not configured");
        var clientSecret = config["EmailOperations:MicrosoftGraph:ClientSecret"] 
            ?? throw new Exception("ClientSecret not configured");

        var accessToken = await GetAccessTokenAsync(tenantId, clientId, clientSecret);
        
        var url = $"{GraphBaseUrl}/users?$select=id,mail,displayName,userPrincipalName&$top=100";
        
        if (!string.IsNullOrEmpty(emailFilter))
        {
            url += $"&$filter=mail eq '{emailFilter}' or userPrincipalName eq '{emailFilter}'";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get users: {StatusCode} - {Response}", response.StatusCode, json);
            throw new Exception($"Failed to get users: {response.StatusCode}");
        }

        var users = new List<GraphUser>();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("value", out var valueArray))
        {
            foreach (var element in valueArray.EnumerateArray())
            {
                var user = new GraphUser
                {
                    Id = element.GetProperty("id").GetString() ?? "",
                    DisplayName = element.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
                    UserPrincipalName = element.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null
                };
                
                // Prefer mail over userPrincipalName
                if (element.TryGetProperty("mail", out var mail) && !string.IsNullOrEmpty(mail.GetString()))
                {
                    user.EmailAddress = mail.GetString();
                }
                else
                {
                    user.EmailAddress = user.UserPrincipalName;
                }

                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
                    users.Add(user);
                }
            }
        }

        _logger.LogInformation("Retrieved {Count} users from Microsoft 365", users.Count);
        return users;
    }
}
