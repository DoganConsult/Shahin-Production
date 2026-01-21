using System;
using System.Collections.Generic;

namespace GrcMvc.Models.ViewModels.Excellence;

/// <summary>
/// Excellence Dashboard ViewModel
/// </summary>
public class ExcellenceDashboardViewModel
{
    public decimal OverallScore { get; set; }
    public decimal PreviousQuarterScore { get; set; }
    public decimal TargetScore { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public int ActiveInitiativesCount { get; set; }
    public decimal MaturityScore { get; set; }
    public decimal CertificationCoverage { get; set; }
    public decimal PerformanceScore { get; set; }
    public decimal? InnovationScore { get; set; }
    public decimal? CultureScore { get; set; }
    public List<ExcellenceInitiativeViewModel>? ActiveInitiatives { get; set; }
    public List<decimal>? MonthlyScores { get; set; }
}

/// <summary>
/// Excellence Initiative ViewModel
/// </summary>
public class ExcellenceInitiativeViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Maturity, Certification, Performance, Process
    public string Owner { get; set; } = string.Empty;
    public decimal Progress { get; set; }
    public DateTime? TargetDate { get; set; }
    public string Category { get; set; } = string.Empty;
}
