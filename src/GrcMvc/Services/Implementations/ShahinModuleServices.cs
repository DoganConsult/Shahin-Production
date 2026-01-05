using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

#region Interfaces

public interface IMAPService
{
    Task<List<CanonicalControl>> GetControlsAsync(Guid tenantId, int page = 1, int pageSize = 50);
    Task<CanonicalControl?> GetControlAsync(Guid id);
    Task<int> GetControlCountAsync();
}

public interface IAPPLYService
{
    Task<List<TenantBaseline>> GetBaselinesAsync(Guid tenantId);
    Task<List<TenantPackage>> GetPackagesAsync(Guid tenantId);
    Task<List<ApplicabilityEntry>> GetApplicabilityEntriesAsync(Guid tenantId);
    Task<ApplicabilityEntry> SetApplicabilityAsync(Guid tenantId, Guid controlId, string status, string reason);
}

public interface IPROVEService
{
    Task<List<AutoTaggedEvidence>> GetEvidenceAsync(Guid tenantId, int page = 1, int pageSize = 50);
    Task<AutoTaggedEvidence?> GetEvidenceByIdAsync(Guid id);
    Task<AutoTaggedEvidence> UploadEvidenceAsync(Guid tenantId, AutoTaggedEvidence evidence);
    Task<AutoTaggedEvidence> ApproveEvidenceAsync(Guid id, string userId, string? notes);
    Task<AutoTaggedEvidence> RejectEvidenceAsync(Guid id, string userId, string reason);
    Task<EvidenceStats> GetStatsAsync(Guid tenantId);
}

public interface IWATCHService
{
    Task<List<RiskIndicator>> GetIndicatorsAsync(Guid tenantId);
    Task<List<RiskIndicatorAlert>> GetAlertsAsync(Guid tenantId, bool openOnly = true);
    Task<RiskIndicator> CreateIndicatorAsync(Guid tenantId, RiskIndicator indicator);
    Task<RiskIndicatorAlert> CreateAlertAsync(Guid indicatorId, RiskIndicatorAlert alert);
    Task<RiskIndicatorAlert> AcknowledgeAlertAsync(Guid alertId, string userId);
    Task<WatchStats> GetStatsAsync(Guid tenantId);
}

public interface IFIXService
{
    Task<List<ControlException>> GetExceptionsAsync(Guid tenantId);
    Task<ControlException> CreateExceptionAsync(Guid tenantId, ControlException exception);
    Task<ControlException> ApproveExceptionAsync(Guid id, string userId);
    Task<ControlException> ExtendExceptionAsync(Guid id, DateTime newExpiry, string reason);
    Task<FixStats> GetStatsAsync(Guid tenantId);
}

public interface IVAULTService
{
    Task<List<CapturedEvidence>> GetStoredEvidenceAsync(Guid tenantId, int page = 1, int pageSize = 50);
    Task<CapturedEvidence?> GetEvidenceByIdAsync(Guid id);
    Task<VaultStats> GetStatsAsync(Guid tenantId);
}

#endregion

#region Stats DTOs

public class EvidenceStats
{
    public int Total { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public decimal ApprovalRate => Total > 0 ? (decimal)Approved / Total * 100 : 0;
}

public class WatchStats
{
    public int TotalIndicators { get; set; }
    public int ActiveIndicators { get; set; }
    public int OpenAlerts { get; set; }
    public int CriticalAlerts { get; set; }
}

public class FixStats
{
    public int TotalExceptions { get; set; }
    public int OpenExceptions { get; set; }
    public int ExpiringIn30Days { get; set; }
    public int Expired { get; set; }
}

public class VaultStats
{
    public int TotalItems { get; set; }
    public long TotalSizeBytes { get; set; }
    public int ByType_Document { get; set; }
    public int ByType_Screenshot { get; set; }
    public int ByType_Config { get; set; }
}

#endregion

#region MAP Service

public class MAPService : IMAPService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<MAPService> _logger;

    public MAPService(GrcDbContext db, ILogger<MAPService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CanonicalControl>> GetControlsAsync(Guid tenantId, int page = 1, int pageSize = 50)
    {
        return await _db.CanonicalControls
            .Where(c => c.IsActive)
            .OrderBy(c => c.ControlName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<CanonicalControl?> GetControlAsync(Guid id)
    {
        return await _db.CanonicalControls.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> GetControlCountAsync()
    {
        return await _db.CanonicalControls.CountAsync(c => c.IsActive);
    }
}

#endregion

#region APPLY Service

public class APPLYService : IAPPLYService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<APPLYService> _logger;

    public APPLYService(GrcDbContext db, ILogger<APPLYService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<TenantBaseline>> GetBaselinesAsync(Guid tenantId)
    {
        return await _db.TenantBaselines.Where(b => b.TenantId == tenantId).ToListAsync();
    }

    public async Task<List<TenantPackage>> GetPackagesAsync(Guid tenantId)
    {
        return await _db.TenantPackages.Where(p => p.TenantId == tenantId).ToListAsync();
    }

    public async Task<List<ApplicabilityEntry>> GetApplicabilityEntriesAsync(Guid tenantId)
    {
        return await _db.ApplicabilityEntries.Where(a => a.TenantId == tenantId).ToListAsync();
    }

    public async Task<ApplicabilityEntry> SetApplicabilityAsync(Guid tenantId, Guid controlId, string status, string reason)
    {
        var entry = await _db.ApplicabilityEntries
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.ControlId == controlId);

        if (entry == null)
        {
            entry = new ApplicabilityEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ControlId = controlId,
                Status = status,
                Reason = reason
            };
            _db.ApplicabilityEntries.Add(entry);
        }
        else
        {
            entry.Status = status;
            entry.Reason = reason;
        }

        await _db.SaveChangesAsync();
        return entry;
    }
}

#endregion

#region PROVE Service

public class PROVEService : IPROVEService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<PROVEService> _logger;

    public PROVEService(GrcDbContext db, ILogger<PROVEService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<AutoTaggedEvidence>> GetEvidenceAsync(Guid tenantId, int page = 1, int pageSize = 50)
    {
        return await _db.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.CapturedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AutoTaggedEvidence?> GetEvidenceByIdAsync(Guid id)
    {
        return await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<AutoTaggedEvidence> UploadEvidenceAsync(Guid tenantId, AutoTaggedEvidence evidence)
    {
        evidence.Id = Guid.NewGuid();
        evidence.TenantId = tenantId;
        evidence.Status = "Collected";
        evidence.CapturedAt = DateTime.UtcNow;

        _db.AutoTaggedEvidences.Add(evidence);
        await _db.SaveChangesAsync();
        return evidence;
    }

    public async Task<AutoTaggedEvidence> ApproveEvidenceAsync(Guid id, string userId, string? notes)
    {
        var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
        if (evidence == null) throw new Exception("Evidence not found");

        evidence.Status = "Approved";
        evidence.ReviewedBy = userId;
        evidence.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return evidence;
    }

    public async Task<AutoTaggedEvidence> RejectEvidenceAsync(Guid id, string userId, string reason)
    {
        var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
        if (evidence == null) throw new Exception("Evidence not found");

        evidence.Status = "Rejected";
        evidence.ReviewedBy = userId;
        evidence.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return evidence;
    }

    public async Task<EvidenceStats> GetStatsAsync(Guid tenantId)
    {
        var all = await _db.AutoTaggedEvidences.Where(e => e.TenantId == tenantId).ToListAsync();
        return new EvidenceStats
        {
            Total = all.Count,
            Approved = all.Count(e => e.Status == "Approved"),
            Pending = all.Count(e => e.Status == "Collected"),
            Rejected = all.Count(e => e.Status == "Rejected")
        };
    }
}

#endregion

#region WATCH Service

public class WATCHService : IWATCHService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<WATCHService> _logger;

    public WATCHService(GrcDbContext db, ILogger<WATCHService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<RiskIndicator>> GetIndicatorsAsync(Guid tenantId)
    {
        return await _db.RiskIndicators.Where(r => r.TenantId == tenantId && r.IsActive).ToListAsync();
    }

    public async Task<List<RiskIndicatorAlert>> GetAlertsAsync(Guid tenantId, bool openOnly = true)
    {
        var query = _db.RiskIndicatorAlerts.Include(a => a.Indicator).Where(a => a.Indicator.TenantId == tenantId);
        if (openOnly) query = query.Where(a => a.Status == "Open");
        return await query.OrderByDescending(a => a.TriggeredAt).ToListAsync();
    }

    public async Task<RiskIndicator> CreateIndicatorAsync(Guid tenantId, RiskIndicator indicator)
    {
        indicator.Id = Guid.NewGuid();
        indicator.TenantId = tenantId;
        indicator.IsActive = true;

        _db.RiskIndicators.Add(indicator);
        await _db.SaveChangesAsync();
        return indicator;
    }

    public async Task<RiskIndicatorAlert> CreateAlertAsync(Guid indicatorId, RiskIndicatorAlert alert)
    {
        alert.Id = Guid.NewGuid();
        alert.IndicatorId = indicatorId;
        alert.Status = "Open";
        alert.TriggeredAt = DateTime.UtcNow;

        _db.RiskIndicatorAlerts.Add(alert);
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task<RiskIndicatorAlert> AcknowledgeAlertAsync(Guid alertId, string userId)
    {
        var alert = await _db.RiskIndicatorAlerts.FirstOrDefaultAsync(a => a.Id == alertId);
        if (alert == null) throw new Exception("Alert not found");

        alert.Status = "Acknowledged";
        alert.AcknowledgedBy = userId;
        alert.AcknowledgedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task<WatchStats> GetStatsAsync(Guid tenantId)
    {
        var indicators = await _db.RiskIndicators.Where(r => r.TenantId == tenantId).ToListAsync();
        var alerts = await _db.RiskIndicatorAlerts.Include(a => a.Indicator)
            .Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open").ToListAsync();

        return new WatchStats
        {
            TotalIndicators = indicators.Count,
            ActiveIndicators = indicators.Count(i => i.IsActive),
            OpenAlerts = alerts.Count,
            CriticalAlerts = alerts.Count(a => a.Severity == "Critical")
        };
    }
}

#endregion

#region FIX Service

public class FIXService : IFIXService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<FIXService> _logger;

    public FIXService(GrcDbContext db, ILogger<FIXService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ControlException>> GetExceptionsAsync(Guid tenantId)
    {
        return await _db.ControlExceptions.Where(e => e.TenantId == tenantId).OrderByDescending(e => e.Id).ToListAsync();
    }

    public async Task<ControlException> CreateExceptionAsync(Guid tenantId, ControlException exception)
    {
        exception.Id = Guid.NewGuid();
        exception.TenantId = tenantId;
        exception.Status = "Pending";

        _db.ControlExceptions.Add(exception);
        await _db.SaveChangesAsync();
        return exception;
    }

    public async Task<ControlException> ApproveExceptionAsync(Guid id, string userId)
    {
        var exception = await _db.ControlExceptions.FirstOrDefaultAsync(e => e.Id == id);
        if (exception == null) throw new Exception("Exception not found");

        exception.Status = "Approved";
        exception.ApprovedBy = userId;

        await _db.SaveChangesAsync();
        return exception;
    }

    public async Task<ControlException> ExtendExceptionAsync(Guid id, DateTime newExpiry, string reason)
    {
        var exception = await _db.ControlExceptions.FirstOrDefaultAsync(e => e.Id == id);
        if (exception == null) throw new Exception("Exception not found");

        exception.ExpiryDate = newExpiry;

        await _db.SaveChangesAsync();
        return exception;
    }

    public async Task<FixStats> GetStatsAsync(Guid tenantId)
    {
        var all = await _db.ControlExceptions.Where(e => e.TenantId == tenantId).ToListAsync();
        var now = DateTime.UtcNow;

        return new FixStats
        {
            TotalExceptions = all.Count,
            OpenExceptions = all.Count(e => e.Status == "Approved" && e.ExpiryDate > now),
            ExpiringIn30Days = all.Count(e => e.Status == "Approved" && e.ExpiryDate > now && e.ExpiryDate <= now.AddDays(30)),
            Expired = all.Count(e => e.ExpiryDate <= now)
        };
    }
}

#endregion

#region VAULT Service

public class VAULTService : IVAULTService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<VAULTService> _logger;

    public VAULTService(GrcDbContext db, ILogger<VAULTService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CapturedEvidence>> GetStoredEvidenceAsync(Guid tenantId, int page = 1, int pageSize = 50)
    {
        return await _db.CapturedEvidences
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.CapturedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<CapturedEvidence?> GetEvidenceByIdAsync(Guid id)
    {
        return await _db.CapturedEvidences.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<VaultStats> GetStatsAsync(Guid tenantId)
    {
        var count = await _db.CapturedEvidences.Where(e => e.TenantId == tenantId).CountAsync();

        return new VaultStats
        {
            TotalItems = count,
            TotalSizeBytes = 0,
            ByType_Document = 0,
            ByType_Screenshot = 0,
            ByType_Config = 0
        };
    }
}

#endregion

#region Assessment Execution Service

public interface IAssessmentExecutionService
{
    Task<Assessment> CreateAssessmentAsync(Guid tenantId, string name, Guid? templateId, string createdBy);
    Task<List<Assessment>> GetAssessmentsAsync(Guid tenantId);
    Task<Assessment?> GetAssessmentAsync(Guid id);
    Task<Assessment> StartAssessmentAsync(Guid id, string userId);
    Task<Assessment> CompleteAssessmentAsync(Guid id, string userId);
    Task<List<AssessmentRequirement>> GetRequirementsAsync(Guid assessmentId);
    Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid requirementId, string status, string? notes);
    Task<AssessmentRequirement> AttachEvidenceAsync(Guid requirementId, Guid evidenceId);
}

public class AssessmentExecutionService : IAssessmentExecutionService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<AssessmentExecutionService> _logger;

    public AssessmentExecutionService(GrcDbContext db, ILogger<AssessmentExecutionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Assessment> CreateAssessmentAsync(Guid tenantId, string name, Guid? templateId, string createdBy)
    {
        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Status = "Draft",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Assessment {Id} created for tenant {TenantId}", assessment.Id, tenantId);
        return assessment;
    }

    public async Task<List<Assessment>> GetAssessmentsAsync(Guid tenantId)
    {
        return await _db.Assessments.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedDate).ToListAsync();
    }

    public async Task<Assessment?> GetAssessmentAsync(Guid id)
    {
        return await _db.Assessments.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Assessment> StartAssessmentAsync(Guid id, string userId)
    {
        var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.Id == id);
        if (assessment == null) throw new Exception("Assessment not found");

        assessment.Status = "InProgress";
        assessment.StartDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return assessment;
    }

    public async Task<Assessment> CompleteAssessmentAsync(Guid id, string userId)
    {
        var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.Id == id);
        if (assessment == null) throw new Exception("Assessment not found");

        assessment.Status = "Completed";

        await _db.SaveChangesAsync();
        return assessment;
    }

    public async Task<List<AssessmentRequirement>> GetRequirementsAsync(Guid assessmentId)
    {
        return await _db.AssessmentRequirements.Where(r => r.AssessmentId == assessmentId).ToListAsync();
    }

    public async Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid requirementId, string status, string? notes)
    {
        var requirement = await _db.AssessmentRequirements.FirstOrDefaultAsync(r => r.Id == requirementId);
        if (requirement == null) throw new Exception("Requirement not found");

        requirement.Status = status;

        await _db.SaveChangesAsync();
        return requirement;
    }

    public async Task<AssessmentRequirement> AttachEvidenceAsync(Guid requirementId, Guid evidenceId)
    {
        var requirement = await _db.AssessmentRequirements.FirstOrDefaultAsync(r => r.Id == requirementId);
        if (requirement == null) throw new Exception("Requirement not found");

        await _db.SaveChangesAsync();
        return requirement;
    }
}

#endregion

#region Workflow Integration Service

public interface IWorkflowIntegrationService
{
    Task<WorkflowInstance> StartWorkflowAsync(Guid tenantId, Guid definitionId, string initiatorId, Dictionary<string, object>? variables = null);
    Task<List<WorkflowInstance>> GetWorkflowsAsync(Guid tenantId);
    Task<List<WorkflowTask>> GetMyTasksAsync(string userId);
    Task<WorkflowTask> CompleteTaskAsync(Guid taskId, string userId, string action, string? notes = null);
    Task<WorkflowTask> ApproveTaskAsync(Guid taskId, string userId, string? notes = null);
    Task<WorkflowTask> RejectTaskAsync(Guid taskId, string userId, string reason);
}

public class WorkflowIntegrationService : IWorkflowIntegrationService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<WorkflowIntegrationService> _logger;

    public WorkflowIntegrationService(GrcDbContext db, ILogger<WorkflowIntegrationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<WorkflowInstance> StartWorkflowAsync(Guid tenantId, Guid definitionId, string initiatorId, Dictionary<string, object>? variables = null)
    {
        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Status = "InProgress"
        };

        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Workflow {Id} started for tenant {TenantId}", instance.Id, tenantId);
        return instance;
    }

    public async Task<List<WorkflowInstance>> GetWorkflowsAsync(Guid tenantId)
    {
        return await _db.WorkflowInstances.Where(w => w.TenantId == tenantId).OrderByDescending(w => w.Id).ToListAsync();
    }

    public async Task<List<WorkflowTask>> GetMyTasksAsync(string userId)
    {
        return await _db.WorkflowTasks.Where(t => t.Status == "Pending").ToListAsync();
    }

    public async Task<WorkflowTask> CompleteTaskAsync(Guid taskId, string userId, string action, string? notes = null)
    {
        var task = await _db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) throw new Exception("Task not found");

        task.Status = action;
        task.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<WorkflowTask> ApproveTaskAsync(Guid taskId, string userId, string? notes = null)
    {
        return await CompleteTaskAsync(taskId, userId, "Approved", notes);
    }

    public async Task<WorkflowTask> RejectTaskAsync(Guid taskId, string userId, string reason)
    {
        return await CompleteTaskAsync(taskId, userId, "Rejected", reason);
    }
}

#endregion
