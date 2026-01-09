using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Control Test Record - Documents testing of control effectiveness
/// Tracks: Design effectiveness, operating effectiveness, and overall control health
/// </summary>
public class ControlTest : BaseEntity
{
    /// <summary>
    /// Control being tested
    /// </summary>
    public Guid ControlId { get; set; }
    
    [ForeignKey("ControlId")]
    public virtual Control? Control { get; set; }
    
    /// <summary>
    /// Type of test performed
    /// </summary>
    [Required]
    public string TestType { get; set; } = "Effectiveness"; // Design, Operating, Effectiveness, Walkthrough
    
    /// <summary>
    /// Test methodology used
    /// </summary>
    public string? TestMethodology { get; set; } // Inquiry, Observation, Inspection, Reperformance
    
    /// <summary>
    /// Sample size tested (for statistical testing)
    /// </summary>
    public int? SampleSize { get; set; }
    
    /// <summary>
    /// Population size (total items that could be tested)
    /// </summary>
    public int? PopulationSize { get; set; }
    
    /// <summary>
    /// Number of exceptions found
    /// </summary>
    public int ExceptionsFound { get; set; }
    
    /// <summary>
    /// Test score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int Score { get; set; }
    
    /// <summary>
    /// Test result
    /// </summary>
    [Required]
    public string Result { get; set; } = "Pass"; // Pass, Fail, PartialPass, NotTested, NeedsReview
    
    /// <summary>
    /// Detailed test findings
    /// </summary>
    public string? Findings { get; set; }
    
    /// <summary>
    /// Recommendations from the test
    /// </summary>
    public string? Recommendations { get; set; }
    
    /// <summary>
    /// Test notes and observations
    /// </summary>
    public string? TestNotes { get; set; }
    
    /// <summary>
    /// Tester user ID
    /// </summary>
    public string? TesterId { get; set; }
    
    /// <summary>
    /// Tester name
    /// </summary>
    public string TesterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date test was performed
    /// </summary>
    public DateTime TestedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date test was reviewed/approved
    /// </summary>
    public DateTime? ReviewedDate { get; set; }
    
    /// <summary>
    /// Reviewer user ID
    /// </summary>
    public string? ReviewerId { get; set; }
    
    /// <summary>
    /// Reviewer name
    /// </summary>
    public string? ReviewerName { get; set; }
    
    /// <summary>
    /// Review status
    /// </summary>
    public string ReviewStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, NeedsRevision
    
    /// <summary>
    /// Effectiveness score before this test
    /// </summary>
    public int PreviousEffectiveness { get; set; }
    
    /// <summary>
    /// New effectiveness score after this test
    /// </summary>
    public int NewEffectiveness { get; set; }
    
    /// <summary>
    /// Test period start date
    /// </summary>
    public DateTime? PeriodStart { get; set; }
    
    /// <summary>
    /// Test period end date
    /// </summary>
    public DateTime? PeriodEnd { get; set; }
    
    /// <summary>
    /// Evidence IDs linked to this test (JSON array)
    /// </summary>
    public string? EvidenceIds { get; set; }
    
    /// <summary>
    /// Next scheduled test date (calculated based on frequency)
    /// </summary>
    public DateTime? NextTestDate { get; set; }
}

/// <summary>
/// Control Owner Assignment - Tracks ownership history
/// </summary>
public class ControlOwnerAssignment : BaseEntity
{
    public Guid ControlId { get; set; }
    
    [ForeignKey("ControlId")]
    public virtual Control? Control { get; set; }
    
    /// <summary>
    /// Owner user ID
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Owner name
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Owner email
    /// </summary>
    public string? OwnerEmail { get; set; }
    
    /// <summary>
    /// Owner department
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Assignment type
    /// </summary>
    public string AssignmentType { get; set; } = "Primary"; // Primary, Backup, Delegate
    
    /// <summary>
    /// Assignment start date
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Assignment end date (null = still active)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Assigned by user ID
    /// </summary>
    public string? AssignedById { get; set; }
    
    /// <summary>
    /// Assigned by name
    /// </summary>
    public string? AssignedByName { get; set; }
    
    /// <summary>
    /// Reason for assignment
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Is this the current active owner?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
