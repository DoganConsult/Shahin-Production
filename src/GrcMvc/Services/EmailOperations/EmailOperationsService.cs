using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Main service for email operations management
/// </summary>
public class EmailOperationsService : IEmailOperationsService
{
    private readonly GrcDbContext _db;
    private readonly IMicrosoftGraphEmailService _graphService;
    private readonly IEmailAiService _aiService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailOperationsService> _logger;

    public EmailOperationsService(
        GrcDbContext db,
        IMicrosoftGraphEmailService graphService,
        IEmailAiService aiService,
        IConfiguration configuration,
        ILogger<EmailOperationsService> logger)
    {
        _db = db;
        _graphService = graphService;
        _aiService = aiService;
        _configuration = configuration;
        _logger = logger;
    }

    #region Mailbox Management

    public async Task<List<EmailMailbox>> GetMailboxesAsync(string? brand = null)
    {
        var query = _db.EmailMailboxes.AsQueryable();
        if (!string.IsNullOrEmpty(brand))
            query = query.Where(m => m.Brand == brand);
        return await query.ToListAsync();
    }

    public async Task<EmailMailbox?> GetMailboxAsync(Guid mailboxId)
    {
        return await _db.EmailMailboxes.FindAsync(mailboxId);
    }

    public async Task<EmailMailbox> CreateMailboxAsync(EmailMailbox mailbox)
    {
        mailbox.Id = Guid.NewGuid();
        mailbox.CreatedAt = DateTime.UtcNow;
        _db.EmailMailboxes.Add(mailbox);
        await _db.SaveChangesAsync();
        return mailbox;
    }

    public async Task<EmailMailbox> UpdateMailboxAsync(EmailMailbox mailbox)
    {
        mailbox.UpdatedAt = DateTime.UtcNow;
        _db.EmailMailboxes.Update(mailbox);
        await _db.SaveChangesAsync();
        return mailbox;
    }

    public async Task SyncMailboxAsync(Guid mailboxId, CancellationToken ct = default)
    {
        var mailbox = await _db.EmailMailboxes.FindAsync(new object[] { mailboxId }, ct);
        if (mailbox == null) return;

        try
        {
            var token = await GetAccessTokenAsync(mailbox);
            var messages = await _graphService.GetMessagesAsync(
                token,
                mailbox.GraphUserId ?? mailbox.EmailAddress,
                since: mailbox.LastSyncAt,
                top: 50);

            foreach (var message in messages)
            {
                await ProcessIncomingMessageAsync(mailbox, message, ct);
            }

            mailbox.LastSyncAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync mailbox {MailboxId}", mailboxId);
            throw;
        }
    }

    #endregion

    #region Thread Management

    public async Task<List<EmailThread>> GetThreadsAsync(Guid mailboxId, EmailThreadStatus? status = null, int skip = 0, int take = 50)
    {
        var query = _db.EmailThreads.Where(t => t.MailboxId == mailboxId);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        
        return await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<EmailThread?> GetThreadAsync(Guid threadId)
    {
        return await _db.EmailThreads
            .Include(t => t.Messages)
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == threadId);
    }

    public async Task<EmailThread> ClassifyThreadAsync(Guid threadId)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Messages.OrderBy(m => m.ReceivedAt).Take(1))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            throw new Exception("Thread not found");

        var firstMessage = thread.Messages.FirstOrDefault();
        if (firstMessage == null)
            return thread;

        var mailbox = await _db.EmailMailboxes.FindAsync(thread.MailboxId);
        var result = await _aiService.ClassifyEmailAsync(
            firstMessage.Subject,
            firstMessage.BodyContent ?? firstMessage.BodyPreview ?? "",
            mailbox?.Brand);

        thread.Classification = result.Classification;
        thread.ClassificationConfidence = result.Confidence;
        thread.Priority = result.SuggestedPriority;
        thread.Status = result.RequiresHumanReview 
            ? EmailThreadStatus.AwaitingAssignment 
            : EmailThreadStatus.AwaitingClassification;

        await _db.SaveChangesAsync();
        return thread;
    }

    public async Task AssignThreadAsync(Guid threadId, Guid userId, string userName)
    {
        var thread = await _db.EmailThreads.FindAsync(threadId);
        if (thread == null) return;

        thread.AssignedToUserId = userId;
        thread.AssignedToUserName = userName;
        thread.Status = EmailThreadStatus.Assigned;
        thread.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateThreadStatusAsync(Guid threadId, EmailThreadStatus status)
    {
        var thread = await _db.EmailThreads.FindAsync(threadId);
        if (thread == null) return;

        thread.Status = status;
        thread.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<EmailThread> ResolveThreadAsync(Guid threadId, string? resolution = null)
    {
        var thread = await _db.EmailThreads.FindAsync(threadId);
        if (thread == null)
            throw new Exception("Thread not found");

        thread.Status = EmailThreadStatus.Resolved;
        thread.ResolvedAt = DateTime.UtcNow;
        if (resolution != null)
            thread.InternalNotes = resolution;
        thread.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return thread;
    }

    #endregion

    #region Message Operations

    public async Task<List<EmailMessage>> GetMessagesAsync(Guid threadId)
    {
        return await _db.EmailMessages
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.ReceivedAt)
            .ToListAsync();
    }

    public async Task<EmailMessage?> GetMessageAsync(Guid messageId)
    {
        return await _db.EmailMessages.FindAsync(messageId);
    }

    public async Task<EmailMessage> CreateDraftReplyAsync(Guid threadId, string content, bool useAi = false)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Mailbox)
            .Include(t => t.Messages.OrderByDescending(m => m.ReceivedAt).Take(1))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread?.Mailbox == null)
            throw new Exception("Thread or mailbox not found");

        var lastMessage = thread.Messages.FirstOrDefault();
        if (lastMessage == null)
            throw new Exception("No messages in thread");

        string replyContent = content;
        if (useAi)
        {
            replyContent = await _aiService.GenerateReplyAsync(new EmailReplyContext
            {
                Brand = thread.Mailbox.Brand,
                Language = "ar",
                OriginalSubject = thread.Subject,
                OriginalBody = lastMessage.BodyContent ?? "",
                SenderName = thread.FromName ?? thread.FromEmail,
                Classification = thread.Classification
            });
        }

        var token = await GetAccessTokenAsync(thread.Mailbox);
        var draft = await _graphService.CreateReplyDraftAsync(
            token,
            thread.Mailbox.GraphUserId ?? thread.Mailbox.EmailAddress,
            lastMessage.GraphMessageId,
            replyContent);

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            GraphMessageId = draft.Id,
            ThreadId = threadId,
            FromEmail = thread.Mailbox.EmailAddress,
            ToRecipients = thread.FromEmail,
            Subject = $"Re: {thread.Subject}",
            BodyContent = replyContent,
            BodyContentType = "html",
            Direction = EmailDirection.Outbound,
            Status = EmailMessageStatus.DraftCreated,
            IsAiGenerated = useAi,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmailMessages.Add(message);
        thread.Status = EmailThreadStatus.DraftPending;
        await _db.SaveChangesAsync();

        return message;
    }

    public async Task<EmailMessage> SendMessageAsync(Guid messageId, Guid approvedByUserId, string approvedByUserName)
    {
        var message = await _db.EmailMessages
            .Include(m => m.Thread)
            .ThenInclude(t => t!.Mailbox)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message?.Thread?.Mailbox == null)
            throw new Exception("Message, thread, or mailbox not found");

        var token = await GetAccessTokenAsync(message.Thread.Mailbox);
        await _graphService.SendDraftAsync(token, 
            message.Thread.Mailbox.GraphUserId ?? message.Thread.Mailbox.EmailAddress, 
            message.GraphMessageId);

        message.Status = EmailMessageStatus.Sent;
        message.SentAt = DateTime.UtcNow;
        message.ApprovedByUserId = approvedByUserId;
        message.ApprovedByUserName = approvedByUserName;
        message.ApprovedAt = DateTime.UtcNow;

        message.Thread.Status = EmailThreadStatus.AwaitingCustomerReply;
        if (message.Thread.FirstResponseAt == null)
            message.Thread.FirstResponseAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return message;
    }

    public async Task<EmailMessage> SendDirectReplyAsync(Guid threadId, string content)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Mailbox)
            .Include(t => t.Messages.OrderByDescending(m => m.ReceivedAt).Take(1))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread?.Mailbox == null)
            throw new Exception("Thread or mailbox not found");

        var lastMessage = thread.Messages.FirstOrDefault();
        if (lastMessage == null)
            throw new Exception("No messages in thread");

        var token = await GetAccessTokenAsync(thread.Mailbox);
        await _graphService.SendReplyAsync(
            token,
            thread.Mailbox.GraphUserId ?? thread.Mailbox.EmailAddress,
            lastMessage.GraphMessageId,
            content);

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            GraphMessageId = $"sent-{Guid.NewGuid()}",
            ThreadId = threadId,
            FromEmail = thread.Mailbox.EmailAddress,
            ToRecipients = thread.FromEmail,
            Subject = $"Re: {thread.Subject}",
            BodyContent = content,
            BodyContentType = "html",
            Direction = EmailDirection.Outbound,
            Status = EmailMessageStatus.Sent,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmailMessages.Add(message);
        thread.Status = EmailThreadStatus.AwaitingCustomerReply;
        if (thread.FirstResponseAt == null)
            thread.FirstResponseAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return message;
    }

    #endregion

    #region Task Management

    public async Task<List<EmailTask>> GetTasksAsync(Guid? threadId = null, EmailTaskStatus? status = null)
    {
        var query = _db.EmailTasks.AsQueryable();
        if (threadId.HasValue)
            query = query.Where(t => t.ThreadId == threadId.Value);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        
        return await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueAt)
            .ToListAsync();
    }

    public async Task<EmailTask> CreateTaskAsync(EmailTask task)
    {
        task.Id = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;
        _db.EmailTasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<EmailTask> CompleteTaskAsync(Guid taskId, Guid userId, string userName, string? notes = null)
    {
        var task = await _db.EmailTasks.FindAsync(taskId);
        if (task == null)
            throw new Exception("Task not found");

        task.Status = EmailTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.CompletedByUserId = userId;
        task.CompletedByUserName = userName;
        if (notes != null)
            task.Notes = notes;

        await _db.SaveChangesAsync();
        return task;
    }

    public async Task ScheduleFollowUpAsync(Guid threadId, DateTime followUpAt, string? message = null)
    {
        var thread = await _db.EmailThreads.FindAsync(threadId);
        if (thread == null) return;

        thread.NextFollowUpAt = followUpAt;
        thread.Status = EmailThreadStatus.FollowUpScheduled;

        var task = new EmailTask
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            Title = $"متابعة: {thread.Subject}",
            Description = message ?? "متابعة مجدولة",
            TaskType = EmailTaskType.FollowUp,
            Status = EmailTaskStatus.Pending,
            Priority = thread.Priority,
            DueAt = followUpAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmailTasks.Add(task);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region AI Operations

    public async Task<string> GenerateAiReplyAsync(Guid threadId, string? additionalContext = null)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Mailbox)
            .Include(t => t.Messages.OrderByDescending(m => m.ReceivedAt).Take(1))
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread?.Mailbox == null)
            throw new Exception("Thread not found");

        var lastMessage = thread.Messages.FirstOrDefault();

        return await _aiService.GenerateReplyAsync(new EmailReplyContext
        {
            Brand = thread.Mailbox.Brand,
            Language = "ar",
            OriginalSubject = thread.Subject,
            OriginalBody = lastMessage?.BodyContent ?? "",
            SenderName = thread.FromName ?? thread.FromEmail,
            Classification = thread.Classification,
            AdditionalInstructions = additionalContext
        });
    }

    public async Task<EmailClassification> ClassifyEmailWithAiAsync(string subject, string body)
    {
        var result = await _aiService.ClassifyEmailAsync(subject, body);
        return result.Classification;
    }

    public async Task<Dictionary<string, object>> ExtractEntitiesAsync(string subject, string body)
    {
        return await _aiService.ExtractEntitiesAsync(subject, body);
    }

    #endregion

    #region Dashboard & Analytics

    public async Task<EmailOperationsDashboard> GetDashboardAsync(string? brand = null)
    {
        var query = _db.EmailThreads.AsQueryable();
        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);

        var now = DateTime.UtcNow;
        var today = now.Date;

        return new EmailOperationsDashboard
        {
            TotalThreads = await query.CountAsync(),
            NewThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.New),
            InProgressThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.InProgress),
            AwaitingReplyThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.AwaitingCustomerReply),
            ResolvedToday = await query.CountAsync(t => t.ResolvedAt != null && t.ResolvedAt.Value.Date == today),
            SlaBreachedCount = await query.CountAsync(t => t.SlaFirstResponseBreached || t.SlaResolutionBreached),
            PendingDrafts = await query.CountAsync(t => t.Status == EmailThreadStatus.DraftPending),
            PendingTasks = await _db.EmailTasks.CountAsync(t => t.Status == EmailTaskStatus.Pending)
        };
    }

    public async Task<List<EmailThread>> GetSlaBreachedThreadsAsync(string? brand = null)
    {
        var query = _db.EmailThreads
            .Where(t => (t.SlaFirstResponseBreached || t.SlaResolutionBreached) &&
                       t.Status != EmailThreadStatus.Resolved &&
                       t.Status != EmailThreadStatus.Closed);

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);

        return await query.ToListAsync();
    }

    public async Task<List<EmailThread>> GetPendingFollowUpsAsync(string? brand = null)
    {
        var query = _db.EmailThreads
            .Where(t => t.NextFollowUpAt != null && t.NextFollowUpAt <= DateTime.UtcNow);

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);

        return await query.ToListAsync();
    }

    #endregion

    #region Private Helpers

    private async Task<string> GetAccessTokenAsync(EmailMailbox mailbox)
    {
        var tenantId = mailbox.AzureTenantId ?? _configuration["EmailOperations:MicrosoftGraph:TenantId"];
        var clientId = mailbox.ClientId ?? _configuration["EmailOperations:MicrosoftGraph:ClientId"];
        var clientSecret = mailbox.EncryptedClientSecret ?? _configuration["EmailOperations:MicrosoftGraph:ClientSecret"];

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new Exception("Microsoft Graph credentials not configured");

        return await _graphService.GetAccessTokenAsync(tenantId, clientId, clientSecret);
    }

    private async Task ProcessIncomingMessageAsync(EmailMailbox mailbox, GraphEmailMessage graphMessage, CancellationToken ct)
    {
        // Check if already processed
        var existing = await _db.EmailMessages
            .FirstOrDefaultAsync(m => m.GraphMessageId == graphMessage.Id, ct);

        if (existing != null) return;

        // Find or create thread
        var thread = await _db.EmailThreads
            .FirstOrDefaultAsync(t => t.ConversationId == graphMessage.ConversationId && t.MailboxId == mailbox.Id, ct);

        if (thread == null)
        {
            thread = new EmailThread
            {
                Id = Guid.NewGuid(),
                ConversationId = graphMessage.ConversationId ?? graphMessage.Id,
                Subject = graphMessage.Subject,
                FromEmail = graphMessage.From?.Address ?? "",
                FromName = graphMessage.From?.Name,
                MailboxId = mailbox.Id,
                Status = EmailThreadStatus.New,
                Priority = graphMessage.Importance == "high" ? EmailPriority.High : EmailPriority.Normal,
                ReceivedAt = graphMessage.ReceivedDateTime,
                SlaFirstResponseDeadline = DateTime.UtcNow.AddHours(mailbox.SlaFirstResponseHours),
                SlaResolutionDeadline = DateTime.UtcNow.AddHours(mailbox.SlaResolutionHours),
                CreatedAt = DateTime.UtcNow
            };
            _db.EmailThreads.Add(thread);
        }

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            GraphMessageId = graphMessage.Id,
            InternetMessageId = graphMessage.InternetMessageId,
            ThreadId = thread.Id,
            FromEmail = graphMessage.From?.Address ?? "",
            FromName = graphMessage.From?.Name,
            ToRecipients = string.Join(";", graphMessage.ToRecipients.Select(r => r.Address)),
            Subject = graphMessage.Subject,
            BodyPreview = graphMessage.BodyPreview?[..Math.Min(500, graphMessage.BodyPreview?.Length ?? 0)],
            BodyContent = graphMessage.Body?.Content,
            BodyContentType = graphMessage.Body?.ContentType ?? "text",
            Direction = EmailDirection.Inbound,
            Status = EmailMessageStatus.Received,
            ReceivedAt = graphMessage.ReceivedDateTime,
            HasAttachments = graphMessage.HasAttachments,
            Importance = graphMessage.Importance,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmailMessages.Add(message);
        await _db.SaveChangesAsync(ct);
    }

    #endregion
}
