using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Control Testing Service Implementation
/// Handles the complete control testing lifecycle
/// </summary>
public class ControlTestService : IControlTestService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<ControlTestService> _logger;
    private readonly ITenantContextService _tenantContext;

    public ControlTestService(
        GrcDbContext context,
        ILogger<ControlTestService> logger,
        ITenantContextService tenantContext)
    {
        _context = context;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    #region Test Execution

    public async Task<ControlTestResultDetailDto> ExecuteTestAsync(Guid controlId, ExecuteControlTestRequest request)
    {
        var control = await _context.Controls.FindAsync(controlId)
            ?? throw new InvalidOperationException($"Control {controlId} not found");

        var previousEffectiveness = control.EffectivenessScore;

        // Create test record
        var test = new ControlTest
        {
            TenantId = control.TenantId,
            ControlId = controlId,
            TestType = request.TestType,
            TestMethodology = request.TestMethodology,
            SampleSize = request.SampleSize,
            PopulationSize = request.PopulationSize,
            ExceptionsFound = request.ExceptionsFound,
            Score = request.Score,
            Result = request.Result,
            Findings = request.Findings,
            Recommendations = request.Recommendations,
            TestNotes = request.TestNotes,
            TesterId = request.TesterId,
            TesterName = request.TesterName,
            TestedDate = DateTime.UtcNow,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            EvidenceIds = request.EvidenceIds != null ? JsonSerializer.Serialize(request.EvidenceIds) : null,
            PreviousEffectiveness = previousEffectiveness,
            ReviewStatus = "Pending"
        };

        // Calculate new effectiveness based on test result
        var newEffectiveness = CalculateNewEffectiveness(previousEffectiveness, request.Score, request.Result);
        test.NewEffectiveness = newEffectiveness;

        // Calculate next test date based on control frequency
        test.NextTestDate = CalculateNextTestDate(control.Frequency);

        _context.Set<ControlTest>().Add(test);

        // Update control with test results
        control.EffectivenessScore = newEffectiveness;
        control.Effectiveness = newEffectiveness;
        control.LastTestDate = DateTime.UtcNow;
        control.NextTestDate = test.NextTestDate;
        control.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Executed control test for {ControlId}, Score: {Score}, Result: {Result}", 
            controlId, request.Score, request.Result);

        return MapToDetailDto(test, control);
    }

    public async Task<ControlTestResultDetailDto?> GetTestByIdAsync(Guid testId)
    {
        var test = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .FirstOrDefaultAsync(t => t.Id == testId);

        return test != null ? MapToDetailDto(test, test.Control) : null;
    }

    public async Task<List<ControlTestResultDetailDto>> GetTestsForControlAsync(Guid controlId)
    {
        var tests = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .Where(t => t.ControlId == controlId)
            .OrderByDescending(t => t.TestedDate)
            .ToListAsync();

        return tests.Select(t => MapToDetailDto(t, t.Control)).ToList();
    }

    public async Task<List<ControlTestHistoryDto>> GetTestHistoryAsync(Guid controlId, int limit = 12)
    {
        return await _context.Set<ControlTest>()
            .Where(t => t.ControlId == controlId)
            .OrderByDescending(t => t.TestedDate)
            .Take(limit)
            .Select(t => new ControlTestHistoryDto(
                t.Id,
                t.TestedDate,
                t.TestType,
                t.Score,
                t.Result,
                t.TesterName,
                t.ReviewStatus
            ))
            .ToListAsync();
    }

    #endregion

    #region Test Review

    public async Task<ControlTestResultDetailDto> SubmitForReviewAsync(Guid testId, string submittedBy)
    {
        var test = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .FirstOrDefaultAsync(t => t.Id == testId)
            ?? throw new InvalidOperationException($"Test {testId} not found");

        test.ReviewStatus = "PendingReview";
        test.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDetailDto(test, test.Control);
    }

    public async Task<ControlTestResultDetailDto> ApproveTestAsync(Guid testId, string reviewerId, string reviewerName, string? notes = null)
    {
        var test = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .FirstOrDefaultAsync(t => t.Id == testId)
            ?? throw new InvalidOperationException($"Test {testId} not found");

        test.ReviewStatus = "Approved";
        test.ReviewerId = reviewerId;
        test.ReviewerName = reviewerName;
        test.ReviewedDate = DateTime.UtcNow;
        test.ModifiedDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            test.TestNotes = string.IsNullOrEmpty(test.TestNotes) 
                ? $"Review: {notes}" 
                : $"{test.TestNotes}\n\nReview: {notes}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Test {TestId} approved by {ReviewerName}", testId, reviewerName);

        return MapToDetailDto(test, test.Control);
    }

    public async Task<ControlTestResultDetailDto> RejectTestAsync(Guid testId, string reviewerId, string reviewerName, string reason)
    {
        var test = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .FirstOrDefaultAsync(t => t.Id == testId)
            ?? throw new InvalidOperationException($"Test {testId} not found");

        test.ReviewStatus = "Rejected";
        test.ReviewerId = reviewerId;
        test.ReviewerName = reviewerName;
        test.ReviewedDate = DateTime.UtcNow;
        test.TestNotes = string.IsNullOrEmpty(test.TestNotes) 
            ? $"Rejection reason: {reason}" 
            : $"{test.TestNotes}\n\nRejection reason: {reason}";
        test.ModifiedDate = DateTime.UtcNow;

        // Revert effectiveness score if rejected
        if (test.Control != null)
        {
            test.Control.EffectivenessScore = test.PreviousEffectiveness;
            test.Control.Effectiveness = test.PreviousEffectiveness;
        }

        await _context.SaveChangesAsync();

        return MapToDetailDto(test, test.Control);
    }

    public async Task<List<ControlTestResultDetailDto>> GetPendingReviewsAsync(Guid tenantId)
    {
        var tests = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .Where(t => t.TenantId == tenantId && t.ReviewStatus == "PendingReview")
            .OrderBy(t => t.TestedDate)
            .ToListAsync();

        return tests.Select(t => MapToDetailDto(t, t.Control)).ToList();
    }

    #endregion

    #region Scheduling

    public async Task<ControlTestScheduleDto> ScheduleTestAsync(Guid controlId, ScheduleTestRequest request)
    {
        var control = await _context.Controls.FindAsync(controlId)
            ?? throw new InvalidOperationException($"Control {controlId} not found");

        control.NextTestDate = request.ScheduledDate;
        control.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToScheduleDto(control);
    }

    public async Task<List<ControlTestScheduleDto>> GetUpcomingTestsAsync(Guid tenantId, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && c.NextTestDate != null && c.NextTestDate <= cutoffDate)
            .OrderBy(c => c.NextTestDate)
            .ToListAsync();

        return controls.Select(MapToScheduleDto).ToList();
    }

    public async Task<List<ControlTestScheduleDto>> GetOverdueTestsAsync(Guid tenantId)
    {
        var now = DateTime.UtcNow;

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && c.NextTestDate != null && c.NextTestDate < now)
            .OrderBy(c => c.NextTestDate)
            .ToListAsync();

        return controls.Select(MapToScheduleDto).ToList();
    }

    public async Task<ControlTestScheduleDto> UpdateScheduleAsync(Guid controlId, DateTime newTestDate, string? reason = null)
    {
        var control = await _context.Controls.FindAsync(controlId)
            ?? throw new InvalidOperationException($"Control {controlId} not found");

        control.NextTestDate = newTestDate;
        control.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToScheduleDto(control);
    }

    #endregion

    #region Effectiveness Scoring

    public async Task<ControlTestEffectivenessDto> CalculateEffectivenessAsync(Guid controlId)
    {
        var control = await _context.Controls.FindAsync(controlId)
            ?? throw new InvalidOperationException($"Control {controlId} not found");

        var tests = await _context.Set<ControlTest>()
            .Where(t => t.ControlId == controlId && t.ReviewStatus == "Approved")
            .OrderByDescending(t => t.TestedDate)
            .Take(12)
            .ToListAsync();

        var designTests = tests.Where(t => t.TestType == "Design").ToList();
        var operatingTests = tests.Where(t => t.TestType == "Operating" || t.TestType == "Effectiveness").ToList();

        var designEffectiveness = designTests.Any() ? (int)designTests.Average(t => t.Score) : 0;
        var operatingEffectiveness = operatingTests.Any() ? (int)operatingTests.Average(t => t.Score) : 0;

        var averageScore = tests.Any() ? tests.Average(t => t.Score) : 0;

        // Determine effectiveness level
        var level = control.EffectivenessScore switch
        {
            >= 80 => "Effective",
            >= 50 => "PartiallyEffective",
            > 0 => "Ineffective",
            _ => "NotTested"
        };

        var levelAr = level switch
        {
            "Effective" => "فعّال",
            "PartiallyEffective" => "فعّال جزئياً",
            "Ineffective" => "غير فعّال",
            _ => "لم يُختبر"
        };

        // Calculate trend
        var trend = "Stable";
        if (tests.Count >= 3)
        {
            var recent = tests.Take(3).Average(t => t.Score);
            var older = tests.Skip(3).Take(3).DefaultIfEmpty().Average(t => t?.Score ?? recent);
            
            if (recent > older + 5) trend = "Improving";
            else if (recent < older - 5) trend = "Declining";
        }

        return new ControlTestEffectivenessDto
        {
            ControlId = controlId,
            ControlCode = control.DisplayCode,
            ControlName = control.Name,
            CurrentEffectiveness = control.EffectivenessScore,
            DesignEffectiveness = designEffectiveness,
            OperatingEffectiveness = operatingEffectiveness,
            EffectivenessLevel = level,
            EffectivenessLevelAr = levelAr,
            LastTestDate = control.LastTestDate,
            TestCount = tests.Count,
            AverageScore = (decimal)averageScore,
            Trend = trend
        };
    }

    public async Task<List<EffectivenessTrendPointDto>> GetEffectivenessTrendAsync(Guid controlId, int months = 12)
    {
        var fromDate = DateTime.UtcNow.AddMonths(-months);

        return await _context.Set<ControlTest>()
            .Where(t => t.ControlId == controlId && t.TestedDate >= fromDate)
            .OrderBy(t => t.TestedDate)
            .Select(t => new EffectivenessTrendPointDto(
                t.TestedDate,
                t.Score,
                t.TestType,
                t.Result
            ))
            .ToListAsync();
    }

    public async Task<ControlEffectivenessSummaryDto> GetEffectivenessSummaryAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var quarterStart = new DateTime(DateTime.UtcNow.Year, ((DateTime.UtcNow.Month - 1) / 3) * 3 + 1, 1);

        var testsThisQuarter = await _context.Set<ControlTest>()
            .Where(t => t.TenantId == tenantId && t.TestedDate >= quarterStart)
            .Select(t => t.ControlId)
            .Distinct()
            .CountAsync();

        return new ControlEffectivenessSummaryDto
        {
            TenantId = tenantId,
            TotalControls = controls.Count,
            EffectiveControls = controls.Count(c => c.EffectivenessScore >= 80),
            PartiallyEffectiveControls = controls.Count(c => c.EffectivenessScore >= 50 && c.EffectivenessScore < 80),
            IneffectiveControls = controls.Count(c => c.EffectivenessScore > 0 && c.EffectivenessScore < 50),
            NotTestedControls = controls.Count(c => c.EffectivenessScore == 0 || c.LastTestDate == null),
            OverallEffectivenessRate = controls.Any() 
                ? (decimal)controls.Count(c => c.EffectivenessScore >= 80) / controls.Count * 100 
                : 0,
            AverageEffectivenessScore = controls.Any() 
                ? (decimal)controls.Average(c => c.EffectivenessScore) 
                : 0,
            ControlsTestedThisQuarter = testsThisQuarter,
            ControlsDueForTesting = controls.Count(c => c.NextTestDate != null && c.NextTestDate <= DateTime.UtcNow.AddDays(30)),
            GeneratedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Owner Management

    public async Task<ControlOwnerDto> AssignOwnerAsync(Guid controlId, AssignOwnerRequest request)
    {
        var control = await _context.Controls.FindAsync(controlId)
            ?? throw new InvalidOperationException($"Control {controlId} not found");

        // Deactivate existing active assignments
        var existingAssignments = await _context.Set<ControlOwnerAssignment>()
            .Where(a => a.ControlId == controlId && a.IsActive && a.AssignmentType == request.AssignmentType)
            .ToListAsync();

        foreach (var existing in existingAssignments)
        {
            existing.IsActive = false;
            existing.EndDate = DateTime.UtcNow;
            existing.ModifiedDate = DateTime.UtcNow;
        }

        // Create new assignment
        var assignment = new ControlOwnerAssignment
        {
            TenantId = control.TenantId,
            ControlId = controlId,
            OwnerId = request.OwnerId,
            OwnerName = request.OwnerName,
            OwnerEmail = request.OwnerEmail,
            Department = request.Department,
            AssignmentType = request.AssignmentType,
            AssignedById = request.AssignedById,
            AssignedByName = request.AssignedByName,
            Reason = request.Reason,
            AssignedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Set<ControlOwnerAssignment>().Add(assignment);

        // Update control owner
        control.Owner = request.OwnerName;
        control.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned owner {OwnerName} to control {ControlId}", request.OwnerName, controlId);

        return MapToOwnerDto(assignment, control);
    }

    public async Task<ControlOwnerDto?> GetCurrentOwnerAsync(Guid controlId)
    {
        var assignment = await _context.Set<ControlOwnerAssignment>()
            .Include(a => a.Control)
            .FirstOrDefaultAsync(a => a.ControlId == controlId && a.IsActive && a.AssignmentType == "Primary");

        return assignment != null ? MapToOwnerDto(assignment, assignment.Control) : null;
    }

    public async Task<List<ControlOwnerDto>> GetOwnershipHistoryAsync(Guid controlId)
    {
        var control = await _context.Controls.FindAsync(controlId);

        var assignments = await _context.Set<ControlOwnerAssignment>()
            .Where(a => a.ControlId == controlId)
            .OrderByDescending(a => a.AssignedDate)
            .ToListAsync();

        return assignments.Select(a => MapToOwnerDto(a, control)).ToList();
    }

    public async Task<ControlOwnerDto> TransferOwnershipAsync(Guid controlId, TransferOwnershipRequest request)
    {
        return await AssignOwnerAsync(controlId, new AssignOwnerRequest
        {
            OwnerId = request.NewOwnerId,
            OwnerName = request.NewOwnerName,
            OwnerEmail = request.NewOwnerEmail,
            Department = request.NewDepartment,
            AssignmentType = "Primary",
            Reason = request.TransferReason,
            AssignedById = request.TransferredById,
            AssignedByName = request.TransferredByName
        });
    }

    public async Task<List<ControlWithEffectivenessDto>> GetControlsByOwnerAsync(string ownerId)
    {
        var assignments = await _context.Set<ControlOwnerAssignment>()
            .Include(a => a.Control)
            .Where(a => a.OwnerId == ownerId && a.IsActive)
            .ToListAsync();

        return assignments
            .Where(a => a.Control != null)
            .Select(a => MapToControlWithEffectiveness(a.Control!))
            .ToList();
    }

    #endregion

    #region Reporting

    public async Task<ControlTestingDashboardDto> GetTestingDashboardAsync(Guid tenantId)
    {
        return new ControlTestingDashboardDto
        {
            TenantId = tenantId,
            EffectivenessSummary = await GetEffectivenessSummaryAsync(tenantId),
            UpcomingTests = await GetUpcomingTestsAsync(tenantId, 30),
            OverdueTests = await GetOverdueTestsAsync(tenantId),
            RecentTests = (await GetRecentTestsAsync(tenantId, 10)).ToList(),
            PendingReviews = await GetPendingReviewsAsync(tenantId),
            Coverage = await GetTestingCoverageAsync(tenantId),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<List<ControlWithEffectivenessDto>> GetControlsRequiringTestingAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && 
                       (c.NextTestDate == null || c.NextTestDate <= DateTime.UtcNow.AddDays(30)))
            .OrderBy(c => c.NextTestDate)
            .ToListAsync();

        return controls.Select(MapToControlWithEffectiveness).ToList();
    }

    public async Task<TestingCoverageReportDto> GetTestingCoverageAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var testedControlIds = await _context.Set<ControlTest>()
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.ControlId)
            .Distinct()
            .ToListAsync();

        return new TestingCoverageReportDto
        {
            TenantId = tenantId,
            TotalControls = controls.Count,
            ControlsTested = testedControlIds.Count,
            ControlsNotTested = controls.Count - testedControlIds.Count,
            CoveragePercentage = controls.Any() 
                ? (decimal)testedControlIds.Count / controls.Count * 100 
                : 0,
            ByCategory = controls.GroupBy(c => c.Category).ToDictionary(g => g.Key, g => g.Count()),
            ByFrequency = controls.GroupBy(c => c.Frequency).ToDictionary(g => g.Key, g => g.Count()),
            ByType = controls.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count()),
            LastUpdated = DateTime.UtcNow
        };
    }

    #endregion

    #region Private Helper Methods

    private async Task<IEnumerable<ControlTestResultDetailDto>> GetRecentTestsAsync(Guid tenantId, int limit)
    {
        var tests = await _context.Set<ControlTest>()
            .Include(t => t.Control)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.TestedDate)
            .Take(limit)
            .ToListAsync();

        return tests.Select(t => MapToDetailDto(t, t.Control));
    }

    private static int CalculateNewEffectiveness(int previousEffectiveness, int testScore, string result)
    {
        // Weight: 70% current test, 30% previous
        var baseScore = (int)(testScore * 0.7 + previousEffectiveness * 0.3);

        // Apply result modifier
        return result switch
        {
            "Pass" => Math.Min(100, baseScore + 5),
            "PartialPass" => baseScore,
            "Fail" => Math.Max(0, baseScore - 10),
            _ => baseScore
        };
    }

    private static DateTime? CalculateNextTestDate(string frequency)
    {
        return frequency.ToLowerInvariant() switch
        {
            "daily" => DateTime.UtcNow.AddDays(1),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "monthly" => DateTime.UtcNow.AddMonths(1),
            "quarterly" => DateTime.UtcNow.AddMonths(3),
            "semi-annual" or "semiannual" => DateTime.UtcNow.AddMonths(6),
            "annual" or "annually" => DateTime.UtcNow.AddYears(1),
            _ => DateTime.UtcNow.AddMonths(3)
        };
    }

    private static ControlTestResultDetailDto MapToDetailDto(ControlTest test, Control? control)
    {
        return new ControlTestResultDetailDto
        {
            TestId = test.Id,
            ControlId = test.ControlId,
            ControlCode = control?.DisplayCode ?? "",
            ControlName = control?.Name ?? "",
            TestType = test.TestType,
            TestMethodology = test.TestMethodology,
            SampleSize = test.SampleSize,
            PopulationSize = test.PopulationSize,
            ExceptionsFound = test.ExceptionsFound,
            Score = test.Score,
            Result = test.Result,
            Findings = test.Findings,
            Recommendations = test.Recommendations,
            TestNotes = test.TestNotes,
            TesterName = test.TesterName,
            TestedDate = test.TestedDate,
            ReviewStatus = test.ReviewStatus,
            ReviewerName = test.ReviewerName,
            ReviewedDate = test.ReviewedDate,
            PreviousEffectiveness = test.PreviousEffectiveness,
            NewEffectiveness = test.NewEffectiveness,
            PeriodStart = test.PeriodStart,
            PeriodEnd = test.PeriodEnd,
            EvidenceIds = !string.IsNullOrEmpty(test.EvidenceIds) 
                ? JsonSerializer.Deserialize<List<Guid>>(test.EvidenceIds) ?? new List<Guid>() 
                : new List<Guid>()
        };
    }

    private static ControlTestScheduleDto MapToScheduleDto(Control control)
    {
        var now = DateTime.UtcNow;
        var daysUntilDue = control.NextTestDate.HasValue 
            ? (int)(control.NextTestDate.Value - now).TotalDays 
            : 0;

        return new ControlTestScheduleDto
        {
            ControlId = control.Id,
            ControlCode = control.DisplayCode,
            ControlName = control.Name,
            Owner = control.Owner,
            Frequency = control.Frequency,
            LastTestDate = control.LastTestDate,
            NextTestDate = control.NextTestDate,
            DaysUntilDue = daysUntilDue,
            IsOverdue = daysUntilDue < 0,
            CurrentEffectiveness = control.EffectivenessScore,
            Status = control.Status
        };
    }

    private static ControlOwnerDto MapToOwnerDto(ControlOwnerAssignment assignment, Control? control)
    {
        return new ControlOwnerDto
        {
            AssignmentId = assignment.Id,
            ControlId = assignment.ControlId,
            ControlCode = control?.DisplayCode ?? "",
            OwnerId = assignment.OwnerId,
            OwnerName = assignment.OwnerName,
            OwnerEmail = assignment.OwnerEmail,
            Department = assignment.Department,
            AssignmentType = assignment.AssignmentType,
            AssignedDate = assignment.AssignedDate,
            EndDate = assignment.EndDate,
            AssignedByName = assignment.AssignedByName,
            IsActive = assignment.IsActive
        };
    }

    private static ControlWithEffectivenessDto MapToControlWithEffectiveness(Control control)
    {
        var now = DateTime.UtcNow;
        var isOverdue = control.NextTestDate.HasValue && control.NextTestDate.Value < now;
        var daysOverdue = isOverdue 
            ? (int)(now - control.NextTestDate!.Value).TotalDays 
            : 0;

        return new ControlWithEffectivenessDto
        {
            ControlId = control.Id,
            ControlCode = control.DisplayCode,
            ControlName = control.Name,
            Category = control.Category,
            Type = control.Type,
            Frequency = control.Frequency,
            Owner = control.Owner,
            Status = control.Status,
            EffectivenessScore = control.EffectivenessScore,
            EffectivenessLevel = control.EffectivenessScore switch
            {
                >= 80 => "Effective",
                >= 50 => "PartiallyEffective",
                > 0 => "Ineffective",
                _ => "NotTested"
            },
            LastTestDate = control.LastTestDate,
            NextTestDate = control.NextTestDate,
            IsOverdue = isOverdue,
            DaysOverdue = daysOverdue
        };
    }

    #endregion
}
