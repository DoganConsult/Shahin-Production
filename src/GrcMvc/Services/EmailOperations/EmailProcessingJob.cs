using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Background job for processing emails
/// Used by Hangfire for async email operations
/// </summary>
public class EmailProcessingJob
{
    private readonly GrcDbContext _db;
    private readonly IMicrosoftGraphEmailService _graphService;
    private readonly IEmailAiService _aiService;
    private readonly ILogger<EmailProcessingJob> _logger;

    public EmailProcessingJob(
        GrcDbContext db,
        IMicrosoftGraphEmailService graphService,
        IEmailAiService aiService,
        ILogger<EmailProcessingJob> logger)
    {
        _db = db;
        _graphService = graphService;
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Process a new incoming email
    /// Called from webhook notification
    /// </summary>
    public async Task ProcessNewEmailAsync(string graphMailboxId, string? messageId, string? subscriptionId)
    {
        _logger.LogInformation("Processing new email from mailbox {Mailbox}, message {MessageId}", 
            graphMailboxId, messageId);

        try
        {
            // Find the mailbox in our database
            var mailbox = await _db.Set<EmailMailbox>()
                .FirstOrDefaultAsync(m => m.GraphUserId == graphMailboxId && m.IsActive);

            if (mailbox == null)
            {
                _logger.LogWarning("Mailbox {Mailbox} not found in database", graphMailboxId);
                return;
            }

            // Get access token
            var token = await _graphService.GetAccessTokenAsync(
                mailbox.AzureTenantId!,
                mailbox.ClientId!,
                DecryptSecret(mailbox.EncryptedClientSecret!));

            // Fetch the message
            GraphEmailMessage? message;
            if (messageId != null)
            {
                message = await _graphService.GetMessageAsync(token, graphMailboxId, messageId);
            }
            else
            {
                // If no specific message ID, get latest messages
                var messages = await _graphService.GetMessagesAsync(token, graphMailboxId, since: mailbox.LastSyncAt, top: 10);
                message = messages.FirstOrDefault();
            }

            if (message == null)
            {
                _logger.LogWarning("Message {MessageId} not found", messageId);
                return;
            }

            // Check if we already have this message
            var existingMessage = await _db.Set<EmailMessage>()
                .FirstOrDefaultAsync(m => m.GraphMessageId == message.Id);

            if (existingMessage != null)
            {
                _logger.LogInformation("Message {MessageId} already processed", message.Id);
                return;
            }

            // Find or create thread
            var thread = await FindOrCreateThreadAsync(mailbox, message);

            // Create message record
            var emailMessage = new EmailMessage
            {
                Id = Guid.NewGuid(),
                GraphMessageId = message.Id,
                InternetMessageId = message.InternetMessageId,
                ThreadId = thread.Id,
                FromEmail = message.From?.Address ?? "",
                FromName = message.From?.Name,
                ToRecipients = string.Join(";", message.ToRecipients.Select(r => r.Address)),
                CcRecipients = string.Join(";", message.CcRecipients.Select(r => r.Address)),
                Subject = message.Subject,
                BodyPreview = message.BodyPreview?[..Math.Min(500, message.BodyPreview?.Length ?? 0)],
                BodyContent = message.Body?.Content,
                BodyContentType = message.Body?.ContentType ?? "text",
                Direction = EmailDirection.Inbound,
                Status = EmailMessageStatus.Received,
                ReceivedAt = message.ReceivedDateTime,
                HasAttachments = message.HasAttachments,
                Importance = message.Importance,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Set<EmailMessage>().Add(emailMessage);

            // Classify the email using AI
            await ClassifyThreadAsync(thread, message);

            // Update mailbox last sync
            mailbox.LastSyncAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Mark as read in Graph
            await _graphService.MarkAsReadAsync(token, graphMailboxId, message.Id);

            // Process auto-reply rules
            await ProcessAutoReplyRulesAsync(mailbox, thread, emailMessage);

            _logger.LogInformation("Successfully processed email {MessageId} in thread {ThreadId}", 
                message.Id, thread.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email from {Mailbox}", graphMailboxId);
            throw; // Hangfire will retry
        }
    }

    private async Task<EmailThread> FindOrCreateThreadAsync(EmailMailbox mailbox, GraphEmailMessage message)
    {
        // Try to find existing thread by conversation ID
        var existingThread = await _db.Set<EmailThread>()
            .FirstOrDefaultAsync(t => t.ConversationId == message.ConversationId && t.MailboxId == mailbox.Id);

        if (existingThread != null)
        {
            // Update thread status if needed
            if (existingThread.Status == EmailThreadStatus.AwaitingCustomerReply ||
                existingThread.Status == EmailThreadStatus.FollowUpScheduled)
            {
                existingThread.Status = EmailThreadStatus.InProgress;
            }
            return existingThread;
        }

        // Create new thread
        var thread = new EmailThread
        {
            Id = Guid.NewGuid(),
            ConversationId = message.ConversationId ?? message.Id,
            Subject = message.Subject,
            FromEmail = message.From?.Address ?? "",
            FromName = message.From?.Name,
            MailboxId = mailbox.Id,
            Status = EmailThreadStatus.New,
            Priority = message.Importance == "high" ? EmailPriority.High : EmailPriority.Normal,
            ReceivedAt = message.ReceivedDateTime,
            SlaFirstResponseDeadline = DateTime.UtcNow.AddHours(mailbox.SlaFirstResponseHours),
            SlaResolutionDeadline = DateTime.UtcNow.AddHours(mailbox.SlaResolutionHours),
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<EmailThread>().Add(thread);
        return thread;
    }

    private async Task ClassifyThreadAsync(EmailThread thread, GraphEmailMessage message)
    {
        try
        {
            var mailbox = await _db.Set<EmailMailbox>().FindAsync(thread.MailboxId);
            var result = await _aiService.ClassifyEmailAsync(
                message.Subject, 
                message.Body?.Content ?? message.BodyPreview ?? "",
                mailbox?.Brand);

            thread.Classification = result.Classification;
            thread.ClassificationConfidence = result.Confidence;
            thread.Priority = result.SuggestedPriority;

            // Set status based on classification
            if (result.RequiresHumanReview || 
                result.Classification == EmailClassification.Complaint ||
                result.Classification == EmailClassification.Legal ||
                result.Classification == EmailClassification.Escalation)
            {
                thread.Status = EmailThreadStatus.AwaitingAssignment;
            }
            else if (result.Classification == EmailClassification.Spam ||
                     result.Classification == EmailClassification.AutoReply ||
                     result.Classification == EmailClassification.OutOfOffice)
            {
                thread.Status = EmailThreadStatus.Closed;
            }
            else
            {
                thread.Status = EmailThreadStatus.AwaitingClassification;
            }

            // Extract entities
            var entities = await _aiService.ExtractEntitiesAsync(
                message.Subject, 
                message.Body?.Content ?? "");
            
            if (entities.Any())
            {
                thread.ExtractedDataJson = System.Text.Json.JsonSerializer.Serialize(entities);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to classify thread {ThreadId}", thread.Id);
            thread.Status = EmailThreadStatus.AwaitingAssignment;
        }
    }

    private async Task ProcessAutoReplyRulesAsync(EmailMailbox mailbox, EmailThread thread, EmailMessage message)
    {
        if (!mailbox.AutoReplyEnabled)
            return;

        // Get matching auto-reply rules
        var rules = await _db.Set<EmailAutoReplyRule>()
            .Where(r => r.MailboxId == mailbox.Id && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (RuleMatches(rule, thread, message))
            {
                await ApplyAutoReplyRuleAsync(mailbox, thread, message, rule);
                break; // Apply only first matching rule
            }
        }
    }

    private bool RuleMatches(EmailAutoReplyRule rule, EmailThread thread, EmailMessage message)
    {
        // Check classification match
        if (rule.TriggerClassifications?.Length > 0)
        {
            if (!rule.TriggerClassifications.Contains(thread.Classification))
                return false;
        }

        // Check subject pattern
        if (!string.IsNullOrEmpty(rule.SubjectPattern))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(message.Subject, rule.SubjectPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;
        }

        // Check from pattern
        if (!string.IsNullOrEmpty(rule.FromPattern))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(message.FromEmail, rule.FromPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;
        }

        return true;
    }

    private async Task ApplyAutoReplyRuleAsync(EmailMailbox mailbox, EmailThread thread, EmailMessage message, EmailAutoReplyRule rule)
    {
        _logger.LogInformation("Applying auto-reply rule {RuleName} to thread {ThreadId}", rule.Name, thread.Id);

        switch (rule.Action)
        {
            case AutoReplyAction.CreateDraft:
            case AutoReplyAction.SendImmediately:
                await CreateOrSendReplyAsync(mailbox, thread, message, rule);
                break;

            case AutoReplyAction.CreateTask:
                await CreateTaskFromRuleAsync(thread, rule);
                break;

            case AutoReplyAction.MarkAsHandled:
                thread.Status = EmailThreadStatus.Closed;
                break;

            case AutoReplyAction.Ignore:
                thread.Status = EmailThreadStatus.Spam;
                break;
        }

        // Schedule follow-up if specified
        if (rule.FollowUpAfterHours.HasValue && rule.FollowUpAfterHours > 0)
        {
            var followUpTime = DateTime.UtcNow.AddHours(rule.FollowUpAfterHours.Value);
            thread.NextFollowUpAt = followUpTime;
            thread.Status = EmailThreadStatus.FollowUpScheduled;

            BackgroundJob.Schedule<EmailProcessingJob>(
                job => job.SendFollowUpAsync(thread.Id),
                followUpTime);
        }

        await _db.SaveChangesAsync();
    }

    private async Task CreateOrSendReplyAsync(EmailMailbox mailbox, EmailThread thread, EmailMessage message, EmailAutoReplyRule rule)
    {
        string replyContent;

        if (rule.UseAiGeneration)
        {
            replyContent = await _aiService.GenerateReplyAsync(new EmailReplyContext
            {
                Brand = mailbox.Brand,
                Language = "ar",
                OriginalSubject = message.Subject,
                OriginalBody = message.BodyContent ?? message.BodyPreview ?? "",
                SenderName = message.FromName ?? message.FromEmail,
                Classification = thread.Classification,
                AdditionalInstructions = rule.AiPromptTemplate
            });
        }
        else
        {
            replyContent = rule.ReplyContent ?? "شكراً لتواصلك معنا. سيتم الرد عليك قريباً.";
        }

        // Get token
        var token = await _graphService.GetAccessTokenAsync(
            mailbox.AzureTenantId!,
            mailbox.ClientId!,
            DecryptSecret(mailbox.EncryptedClientSecret!));

        if (rule.Action == AutoReplyAction.SendImmediately && !mailbox.DraftModeDefault)
        {
            // Send directly
            await _graphService.SendReplyAsync(token, mailbox.GraphUserId!, message.GraphMessageId, replyContent);
            
            thread.Status = EmailThreadStatus.AwaitingCustomerReply;
            thread.FirstResponseAt = DateTime.UtcNow;
        }
        else
        {
            // Create draft
            var draft = await _graphService.CreateReplyDraftAsync(
                token, mailbox.GraphUserId!, message.GraphMessageId, replyContent);

            // Save draft info
            var draftMessage = new EmailMessage
            {
                Id = Guid.NewGuid(),
                GraphMessageId = draft.Id,
                ThreadId = thread.Id,
                FromEmail = mailbox.EmailAddress,
                ToRecipients = message.FromEmail,
                Subject = $"Re: {message.Subject}",
                BodyContent = replyContent,
                BodyContentType = "html",
                Direction = EmailDirection.Outbound,
                Status = EmailMessageStatus.DraftCreated,
                IsAiGenerated = rule.UseAiGeneration,
                CreatedAt = DateTime.UtcNow
            };

            _db.Set<EmailMessage>().Add(draftMessage);
            thread.Status = EmailThreadStatus.DraftPending;
        }
    }

    private async Task CreateTaskFromRuleAsync(EmailThread thread, EmailAutoReplyRule rule)
    {
        var task = new EmailTask
        {
            Id = Guid.NewGuid(),
            ThreadId = thread.Id,
            Title = $"متابعة: {thread.Subject}",
            Description = rule.Description,
            TaskType = EmailTaskType.FollowUp,
            Status = EmailTaskStatus.Pending,
            Priority = thread.Priority,
            DueAt = DateTime.UtcNow.AddHours(rule.FollowUpAfterHours ?? 24),
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<EmailTask>().Add(task);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Send a scheduled follow-up email
    /// </summary>
    public async Task SendFollowUpAsync(Guid threadId)
    {
        _logger.LogInformation("Sending follow-up for thread {ThreadId}", threadId);

        var thread = await _db.Set<EmailThread>()
            .Include(t => t.Mailbox)
            .Include(t => t.Messages.OrderByDescending(m => m.ReceivedAt).Take(1))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null || thread.Mailbox == null)
        {
            _logger.LogWarning("Thread {ThreadId} not found for follow-up", threadId);
            return;
        }

        if (thread.Status == EmailThreadStatus.Resolved || thread.Status == EmailThreadStatus.Closed)
        {
            _logger.LogInformation("Thread {ThreadId} already resolved, skipping follow-up", threadId);
            return;
        }

        var mailbox = thread.Mailbox;
        var lastMessage = thread.Messages.FirstOrDefault();

        if (lastMessage == null)
            return;

        // Generate follow-up message
        var followUpContent = await _aiService.GenerateReplyAsync(new EmailReplyContext
        {
            Brand = mailbox.Brand,
            Language = "ar",
            OriginalSubject = thread.Subject,
            OriginalBody = lastMessage.BodyContent ?? "",
            SenderName = thread.FromName ?? thread.FromEmail,
            Classification = thread.Classification,
            AdditionalInstructions = "هذه رسالة متابعة. اسأل العميل إذا يحتاج مساعدة إضافية."
        });

        // Create draft (not send directly for follow-ups)
        var token = await _graphService.GetAccessTokenAsync(
            mailbox.AzureTenantId!,
            mailbox.ClientId!,
            DecryptSecret(mailbox.EncryptedClientSecret!));

        var draft = await _graphService.CreateReplyDraftAsync(
            token, mailbox.GraphUserId!, lastMessage.GraphMessageId, followUpContent);

        var draftMessage = new EmailMessage
        {
            Id = Guid.NewGuid(),
            GraphMessageId = draft.Id,
            ThreadId = thread.Id,
            FromEmail = mailbox.EmailAddress,
            ToRecipients = thread.FromEmail,
            Subject = $"Re: {thread.Subject}",
            BodyContent = followUpContent,
            BodyContentType = "html",
            Direction = EmailDirection.Outbound,
            Status = EmailMessageStatus.DraftCreated,
            IsAiGenerated = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<EmailMessage>().Add(draftMessage);
        
        thread.FollowUpCount++;
        thread.Status = EmailThreadStatus.DraftPending;
        thread.NextFollowUpAt = null;

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Renew a Graph subscription
    /// </summary>
    public async Task RenewSubscriptionAsync(string subscriptionId)
    {
        _logger.LogInformation("Renewing subscription {SubscriptionId}", subscriptionId);

        var mailbox = await _db.Set<EmailMailbox>()
            .FirstOrDefaultAsync(m => m.WebhookSubscriptionId == subscriptionId);

        if (mailbox == null)
        {
            _logger.LogWarning("Mailbox for subscription {SubscriptionId} not found", subscriptionId);
            return;
        }

        try
        {
            var token = await _graphService.GetAccessTokenAsync(
                mailbox.AzureTenantId!,
                mailbox.ClientId!,
                DecryptSecret(mailbox.EncryptedClientSecret!));

            var renewed = await _graphService.RenewSubscriptionAsync(token, subscriptionId);
            
            mailbox.WebhookExpiresAt = renewed.ExpirationDateTime;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Subscription renewed until {Expiry}", renewed.ExpirationDateTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to renew subscription {SubscriptionId}", subscriptionId);
        }
    }

    /// <summary>
    /// Check and process SLA breaches
    /// Scheduled to run every hour
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public async Task CheckSlaBreachesAsync()
    {
        _logger.LogInformation("Checking SLA breaches");

        var now = DateTime.UtcNow;

        // Check first response SLA
        var firstResponseBreaches = await _db.Set<EmailThread>()
            .Where(t => t.Status != EmailThreadStatus.Resolved && 
                       t.Status != EmailThreadStatus.Closed &&
                       t.FirstResponseAt == null &&
                       t.SlaFirstResponseDeadline < now &&
                       !t.SlaFirstResponseBreached)
            .ToListAsync();

        foreach (var thread in firstResponseBreaches)
        {
            thread.SlaFirstResponseBreached = true;
            thread.Priority = EmailPriority.Urgent;
            
            // Create escalation task
            var task = new EmailTask
            {
                Id = Guid.NewGuid(),
                ThreadId = thread.Id,
                Title = $"⚠️ SLA انتهى: {thread.Subject}",
                Description = "تجاوز وقت الرد الأول المحدد. يرجى الرد فوراً.",
                TaskType = EmailTaskType.Escalate,
                Status = EmailTaskStatus.Pending,
                Priority = EmailPriority.Urgent,
                DueAt = DateTime.UtcNow.AddHours(2),
                CreatedAt = DateTime.UtcNow
            };

            _db.Set<EmailTask>().Add(task);
        }

        // Check resolution SLA
        var resolutionBreaches = await _db.Set<EmailThread>()
            .Where(t => t.Status != EmailThreadStatus.Resolved && 
                       t.Status != EmailThreadStatus.Closed &&
                       t.SlaResolutionDeadline < now &&
                       !t.SlaResolutionBreached)
            .ToListAsync();

        foreach (var thread in resolutionBreaches)
        {
            thread.SlaResolutionBreached = true;
            thread.Priority = EmailPriority.Critical;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("SLA check complete. First response breaches: {First}, Resolution breaches: {Res}", 
            firstResponseBreaches.Count, resolutionBreaches.Count);
    }

    /// <summary>
    /// Sync all active mailboxes - check for new emails and process them
    /// Called by Hangfire recurring job every 5 minutes
    /// </summary>
    [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 300 })]
    public async Task SyncAllMailboxesAsync()
    {
        _logger.LogInformation("Starting email polling sync for all mailboxes");

        var mailboxes = await _db.Set<EmailMailbox>()
            .Where(m => m.IsActive && m.GraphUserId != null && m.AutoReplyEnabled)
            .ToListAsync();

        if (!mailboxes.Any())
        {
            _logger.LogInformation("No active mailboxes with auto-reply enabled for polling");
            return;
        }

        foreach (var mailbox in mailboxes)
        {
            try
            {
                _logger.LogInformation("Syncing mailbox: {Email}", mailbox.EmailAddress);

                // Get access token
                var token = await _graphService.GetAccessTokenAsync(
                    mailbox.AzureTenantId!,
                    mailbox.ClientId!,
                    DecryptSecret(mailbox.EncryptedClientSecret!));

                // Get messages since last sync (or last hour if never synced)
                var since = mailbox.LastSyncAt ?? DateTime.UtcNow.AddHours(-1);
                var messages = await _graphService.GetMessagesAsync(
                    token,
                    mailbox.GraphUserId!,
                    since: since,
                    top: 50);

                _logger.LogInformation("Found {Count} new messages in {Mailbox}", 
                    messages.Count, mailbox.EmailAddress);

                // Process each new message
                foreach (var message in messages)
                {
                    try
                    {
                        // Check if already processed
                        var existing = await _db.Set<EmailMessage>()
                            .FirstOrDefaultAsync(m => m.GraphMessageId == message.Id);

                        if (existing == null)
                        {
                            // Process new email
                            await ProcessNewEmailAsync(mailbox.GraphUserId!, message.Id, null);
                            _logger.LogInformation("Processed new email: {MessageId} from {From}", 
                                message.Id, message.From?.Address);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message {MessageId} in mailbox {Mailbox}", 
                            message.Id, mailbox.EmailAddress);
                        // Continue with next message
                    }
                }

                // Update last sync time
                mailbox.LastSyncAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Completed sync for mailbox: {Email}, processed {Count} messages", 
                    mailbox.EmailAddress, messages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync mailbox {Mailbox}", mailbox.EmailAddress);
                // Continue with next mailbox
            }
        }

        _logger.LogInformation("Email polling sync completed for {Count} mailboxes", mailboxes.Count);
    }

    private string DecryptSecret(string encryptedSecret)
    {
        // In production, use proper encryption/decryption
        // For now, assume it's stored as-is (should use Azure Key Vault or similar)
        return encryptedSecret;
    }
}
