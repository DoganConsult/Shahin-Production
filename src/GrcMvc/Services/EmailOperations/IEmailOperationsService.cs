using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Models.Entities.EmailOperations;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Main service for email operations management
/// </summary>
public interface IEmailOperationsService
{
    // Mailbox Management
    Task<List<EmailMailbox>> GetMailboxesAsync(string? brand = null);
    Task<EmailMailbox?> GetMailboxAsync(Guid mailboxId);
    Task<EmailMailbox> CreateMailboxAsync(EmailMailbox mailbox);
    Task<EmailMailbox> UpdateMailboxAsync(EmailMailbox mailbox);
    Task SyncMailboxAsync(Guid mailboxId, CancellationToken ct = default);
    
    // Thread Management
    Task<List<EmailThread>> GetThreadsAsync(Guid mailboxId, EmailThreadStatus? status = null, int skip = 0, int take = 50);
    Task<EmailThread?> GetThreadAsync(Guid threadId);
    Task<EmailThread> ClassifyThreadAsync(Guid threadId);
    Task AssignThreadAsync(Guid threadId, Guid userId, string userName);
    Task UpdateThreadStatusAsync(Guid threadId, EmailThreadStatus status);
    Task<EmailThread> ResolveThreadAsync(Guid threadId, string? resolution = null);
    
    // Message Operations
    Task<List<EmailMessage>> GetMessagesAsync(Guid threadId);
    Task<EmailMessage?> GetMessageAsync(Guid messageId);
    Task<EmailMessage> CreateDraftReplyAsync(Guid threadId, string content, bool useAi = false);
    Task<EmailMessage> SendMessageAsync(Guid messageId, Guid approvedByUserId, string approvedByUserName);
    Task<EmailMessage> SendDirectReplyAsync(Guid threadId, string content);
    
    // Task Management
    Task<List<EmailTask>> GetTasksAsync(Guid? threadId = null, EmailTaskStatus? status = null);
    Task<EmailTask> CreateTaskAsync(EmailTask task);
    Task<EmailTask> CompleteTaskAsync(Guid taskId, Guid userId, string userName, string? notes = null);
    Task ScheduleFollowUpAsync(Guid threadId, DateTime followUpAt, string? message = null);
    
    // AI Operations
    Task<string> GenerateAiReplyAsync(Guid threadId, string? additionalContext = null);
    Task<EmailClassification> ClassifyEmailWithAiAsync(string subject, string body);
    Task<Dictionary<string, object>> ExtractEntitiesAsync(string subject, string body);
    
    // Dashboard & Analytics
    Task<EmailOperationsDashboard> GetDashboardAsync(string? brand = null);
    Task<List<EmailThread>> GetSlaBreachedThreadsAsync(string? brand = null);
    Task<List<EmailThread>> GetPendingFollowUpsAsync(string? brand = null);
}

/// <summary>
/// Dashboard data for email operations
/// </summary>
public class EmailOperationsDashboard
{
    public int TotalThreads { get; set; }
    public int NewThreads { get; set; }
    public int InProgressThreads { get; set; }
    public int AwaitingReplyThreads { get; set; }
    public int ResolvedToday { get; set; }
    public int SlaBreachedCount { get; set; }
    public int PendingDrafts { get; set; }
    public int PendingTasks { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public double AvgResolutionHours { get; set; }
    public Dictionary<EmailClassification, int> ClassificationBreakdown { get; set; } = new();
    public Dictionary<string, int> MailboxBreakdown { get; set; } = new();
}
