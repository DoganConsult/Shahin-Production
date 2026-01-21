using System;

namespace GrcMvc.Models.DTOs;

/// <summary>
/// Excellence Initiative DTO
/// </summary>
public class ExcellenceInitiativeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public decimal? Progress { get; set; }
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
}
