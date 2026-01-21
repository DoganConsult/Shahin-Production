using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Email attachment metadata
/// </summary>
public class EmailAttachment : BaseEntity
{
    public Guid MessageId { get; set; }
    
    [ForeignKey(nameof(MessageId))]
    public virtual EmailMessage? Message { get; set; }
    
    /// <summary>
    /// Microsoft Graph Attachment ID
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string GraphAttachmentId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ContentType { get; set; } = "application/octet-stream";
    
    /// <summary>
    /// Size in bytes
    /// </summary>
    public long Size { get; set; }
    
    /// <summary>
    /// Local storage path (if downloaded)
    /// </summary>
    [MaxLength(500)]
    public string? LocalPath { get; set; }
    
    /// <summary>
    /// Is this attachment inline (embedded image) or regular
    /// </summary>
    public bool IsInline { get; set; }
    
    /// <summary>
    /// Content ID for inline attachments
    /// </summary>
    [MaxLength(200)]
    public string? ContentId { get; set; }
}
