using System;
using System.Collections.Generic;

namespace GrcMvc.Models.ViewModels.Certification;

/// <summary>
/// Certification Preparation Plan ViewModel
/// </summary>
public class PreparationPlanViewModel
{
    public Guid? CertificationId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CurrentPhase { get; set; } // 1-5
    public int Phase1Progress { get; set; } // 0-100
    public int Phase2Progress { get; set; } // 0-100
    public int Phase3Progress { get; set; } // 0-100
    public int Phase4Progress { get; set; } // 0-100
    public int Phase5Progress { get; set; } // 0-100
    public List<PreparationPhaseViewModel> Phases { get; set; } = new();
    public List<PreparationTaskViewModel> Tasks { get; set; } = new();
    public DateTime? TargetDate { get; set; }
    public int DaysUntilTarget { get; set; }
}

/// <summary>
/// Preparation Phase ViewModel
/// </summary>
public class PreparationPhaseViewModel
{
    public int PhaseNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Progress { get; set; } // 0-100
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed
    public List<string> Tasks { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TargetDate { get; set; }
}

/// <summary>
/// Preparation Task ViewModel
/// </summary>
public class PreparationTaskViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty; // Alias for Name for View compatibility
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Overdue
    public int Phase { get; set; } // 1-5
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public int Priority { get; set; } // 1=High, 2=Medium, 3=Low
}

/// <summary>
/// Certification Audit ViewModel
/// </summary>
public class CertificationAuditViewModel
{
    public Guid Id { get; set; }
    public Guid CertificationId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty; // Initial, Surveillance, Recertification
    public DateTime AuditDate { get; set; }
    public string? AuditorName { get; set; }
    public string? LeadAuditorName { get; set; }
    public string Result { get; set; } = string.Empty; // Passed, Failed, Conditional
    public int FindingsCount { get; set; }
    public int MajorFindings { get; set; }
    public int MinorFindings { get; set; }
    public int Observations { get; set; }
    public bool CorrectiveActionsCompleted { get; set; }
    public DateTime? CorrectiveActionDeadline { get; set; }
    public DateTime? NextAuditDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public string? ReportReference { get; set; }
    public string? ReportUrl { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed
    public string? Scope { get; set; } // Scope of the audit
    public List<string> Findings { get; set; } = new(); // List of finding descriptions for display
    public List<AuditFindingViewModel> FindingDetails { get; set; } = new(); // Full finding details
}

/// <summary>
/// Audit finding detail
/// </summary>
public class AuditFindingViewModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Major, Minor, Observation
    public string Description { get; set; } = string.Empty;
    public string? CorrectiveAction { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsResolved { get; set; }
}

/// <summary>
/// Certification Portfolio ViewModel - comprehensive view of all certifications
/// </summary>
public class CertificationPortfolioViewModel
{
    public int TotalCertifications { get; set; }
    public int ActiveCertifications { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int ExpiredCount { get; set; }
    public decimal CoveragePercentage { get; set; }
    public int SecurityCertifications { get; set; }
    public int QualityCertifications { get; set; }
    public int PrivacyCertifications { get; set; }
    public DateTime? LastAuditDate { get; set; }
    public List<PortfolioCertItemViewModel> SecurityCerts { get; set; } = new();
    public List<PortfolioCertItemViewModel> QualityCerts { get; set; } = new();
    public List<PortfolioCertItemViewModel> PrivacyCerts { get; set; } = new();
    public List<PortfolioCertItemViewModel> ComplianceCerts { get; set; } = new();
    public List<PortfolioCertItemViewModel> IndustryCerts { get; set; } = new();
    public List<GrcMvc.Services.Interfaces.CertificationDto> AllCertifications { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}

/// <summary>
/// Individual certification item for Portfolio view
/// </summary>
public class PortfolioCertItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Security, Quality, Privacy, Industry
    public bool IsActive { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MonthsRemaining { get; set; }
    public string NextAction { get; set; } = string.Empty;
}
