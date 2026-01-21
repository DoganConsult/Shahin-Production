using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Task created from an email thread
/// </summary>
public class EmailTask : BaseEntity
{
    public Guid ThreadId { get; set; }
    
    [ForeignKey(nameof(ThreadId))]
    public virtual EmailThread? Thread { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public EmailTaskType TaskType { get; set; } = EmailTaskType.FollowUp;
    
    public EmailTaskStatus Status { get; set; } = EmailTaskStatus.Pending;
    
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    
    /// <summary>
    /// Assigned team member
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    [MaxLength(200)]
    public string? AssignedToUserName { get; set; }
    
    /// <summary>
    /// When this task is due
    /// </summary>
    public DateTime? DueAt { get; set; }
    
    /// <summary>
    /// When to send reminder
    /// </summary>
    public DateTime? ReminderAt { get; set; }
    
    /// <summary>
    /// Hangfire Job ID for scheduled tasks
    /// </summary>
    [MaxLength(100)]
    public string? ScheduledJobId { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public Guid? CompletedByUserId { get; set; }
    
    [MaxLength(200)]
    public string? CompletedByUserName { get; set; }
    
    public string? Notes { get; set; }
}

public enum EmailTaskType
{
    FollowUp = 0,
    SendReply = 1,
    ReviewDraft = 2,
    Escalate = 3,
    AssignToTeam = 4,
    CallCustomer = 5,
    CreateTicket = 6,
    ScheduleMeeting = 7,
    Other = 99
}

public enum EmailTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Overdue = 4
}
