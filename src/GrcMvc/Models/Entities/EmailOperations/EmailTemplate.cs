using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Email reply templates
/// </summary>
public class EmailTemplate : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Brand this template belongs to
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = EmailBrands.Shahin;
    
    /// <summary>
    /// Language code (ar, en)
    /// </summary>
    [MaxLength(10)]
    public string Language { get; set; } = "ar";
    
    /// <summary>
    /// Which classifications this template is for
    /// </summary>
    public EmailClassification[] ForClassifications { get; set; } = Array.Empty<EmailClassification>();
    
    /// <summary>
    /// Subject template (supports {{variables}})
    /// </summary>
    [MaxLength(500)]
    public string? SubjectTemplate { get; set; }
    
    /// <summary>
    /// Body template (HTML, supports {{variables}})
    /// </summary>
    [Required]
    public string BodyTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Available variables: {{customerName}}, {{ticketNumber}}, {{agentName}}, etc.
    /// </summary>
    [MaxLength(1000)]
    public string? AvailableVariables { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Usage count for analytics
    /// </summary>
    public int UsageCount { get; set; }
}
