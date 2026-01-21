using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;

namespace GrcMvc.Data;

/// <summary>
/// Partial class extension for GrcDbContext - Gap Closure entities
/// Adds: Control Testing, Incident Response, Certification Tracking
/// </summary>
public partial class GrcDbContext
{
    // Control Testing and Owner Management
    public DbSet<ControlTest> ControlTests { get; set; } = null!;
    public DbSet<ControlOwnerAssignment> ControlOwnerAssignments { get; set; } = null!;

    // Incident Response (Resilience Stage)
    public DbSet<Incident> Incidents { get; set; } = null!;
    public DbSet<IncidentTimelineEntry> IncidentTimelineEntries { get; set; } = null!;

    // Certification Tracking
    public DbSet<Certification> Certifications { get; set; } = null!;
    public DbSet<CertificationAudit> CertificationAudits { get; set; } = null!;
}
