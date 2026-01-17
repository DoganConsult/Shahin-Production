using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GrcMvc.Application.Policy;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Permissions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GrcMvc.AppServices;

/// <summary>
/// Evidence Application Service with integrated policy enforcement
/// Demonstrates deterministic policy evaluation for all CRUD operations
/// </summary>
[Authorize]
public class EvidenceAppService : BaseAppService
{
    private readonly IPolicyEnforcer _policyEnforcer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EvidenceAppService> _logger;

    public EvidenceAppService(
        IPolicyEnforcer policyEnforcer,
        IUnitOfWork unitOfWork,
        ILogger<EvidenceAppService> logger)
    {
        _policyEnforcer = policyEnforcer;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Create new evidence with policy enforcement
    /// </summary>
    [Authorize(GrcPermissions.Evidence.Create)]
    public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
    {
        _logger.LogInformation("Creating evidence with policy enforcement");

        // Enforce policy for create action
        await EnforceAsync("create", "Evidence", input);

        // Create entity
        var evidence = new Evidence
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            Description = input.Description,
            DataClassification = input.DataClassification,
            Owner = input.Owner,
            CollectionDate = input.CollectionDate ?? DateTime.UtcNow,
            VerificationStatus = "Draft", // Using VerificationStatus instead of Status
            CreatedDate = DateTime.UtcNow,
            CreatedBy = CurrentUser.Id?.ToString() ?? "system"
        };

        // Apply any mutations from policy
        var mutationContext = new PolicyContext
        {
            Action = "create",
            ResourceType = "Evidence",
            Resource = evidence,
            Environment = GetEnvironment(),
            TenantId = CurrentTenant?.Id,
            PrincipalId = CurrentUser.Id?.ToString(),
            PrincipalRoles = GetCurrentUserRoles()
        };

        var decision = await _policyEnforcer.EvaluateAsync(mutationContext);
        
        // Save to database using AddAsync instead of InsertAsync
        await _unitOfWork.Evidences.AddAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Evidence created successfully: {EvidenceId}", evidence.Id);

        return MapToDto(evidence);
    }

    /// <summary>
    /// Update existing evidence with policy enforcement
    /// </summary>
    [Authorize(GrcPermissions.Evidence.Update)]
    public async Task<EvidenceDto> UpdateAsync(Guid id, UpdateEvidenceDto input)
    {
        _logger.LogInformation("Updating evidence {EvidenceId} with policy enforcement", id);

        var evidence = await _unitOfWork.Evidences.GetByIdAsync(id);
        if (evidence == null)
        {
            throw new EntityNotFoundException($"Evidence with ID {id} not found");
        }

        // Apply updates to entity
        evidence.Title = input.Title ?? evidence.Title;
        evidence.Description = input.Description ?? evidence.Description;
        evidence.DataClassification = input.DataClassification ?? evidence.DataClassification;
        evidence.Owner = input.Owner ?? evidence.Owner;
        evidence.ModifiedDate = DateTime.UtcNow;
        evidence.ModifiedBy = CurrentUser.Id?.ToString() ?? "system";

        // Enforce policy for update action
        await EnforceAsync("update", "Evidence", evidence);

        // Save changes using UpdateAsync from interface
        await _unitOfWork.Evidences.UpdateAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Evidence updated successfully: {EvidenceId}", evidence.Id);

        return MapToDto(evidence);
    }

    /// <summary>
    /// Submit evidence for approval with policy enforcement
    /// </summary>
    [Authorize(GrcPermissions.Evidence.Submit)]
    public async Task<EvidenceDto> SubmitAsync(Guid id)
    {
        _logger.LogInformation("Submitting evidence {EvidenceId} with policy enforcement", id);

        var evidence = await _unitOfWork.Evidences.GetByIdAsync(id);
        if (evidence == null)
        {
            throw new EntityNotFoundException($"Evidence with ID {id} not found");
        }

        evidence.VerificationStatus = "Submitted";
        evidence.SubmittedAt = DateTime.UtcNow;
        evidence.CollectedBy = CurrentUser.Id?.ToString() ?? evidence.CollectedBy;

        // Enforce policy for submit action
        await EnforceAsync("submit", "Evidence", evidence);

        // Save changes using UpdateAsync
        await _unitOfWork.Evidences.UpdateAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Evidence submitted successfully: {EvidenceId}", evidence.Id);

        return MapToDto(evidence);
    }

    /// <summary>
    /// Approve evidence with policy enforcement
    /// </summary>
    [Authorize(GrcPermissions.Evidence.Approve)]
    public async Task<EvidenceDto> ApproveAsync(Guid id, ApprovalDto approval)
    {
        _logger.LogInformation("Approving evidence {EvidenceId} with policy enforcement", id);

        var evidence = await _unitOfWork.Evidences.GetByIdAsync(id);
        if (evidence == null)
        {
            throw new EntityNotFoundException($"Evidence with ID {id} not found");
        }

        evidence.VerificationStatus = "Verified";
        evidence.VerificationDate = DateTime.UtcNow;
        evidence.VerifiedBy = CurrentUser.Id?.ToString() ?? "system";
        evidence.ReviewerComments = approval.Comments;
        // Store approval flag in Labels for policy enforcement
        var labels = evidence.Labels;
        labels["approvedForProd"] = approval.ApprovedForProd ? "true" : "false";
        evidence.Labels = labels;

        // Enforce policy for approve action
        await EnforceAsync("approve", "Evidence", evidence);

        // Save changes using UpdateAsync
        await _unitOfWork.Evidences.UpdateAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Evidence approved successfully: {EvidenceId}", evidence.Id);

        return MapToDto(evidence);
    }

    /// <summary>
    /// Delete evidence with policy enforcement
    /// </summary>
    [Authorize(GrcPermissions.Evidence.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting evidence {EvidenceId} with policy enforcement", id);

        var evidence = await _unitOfWork.Evidences.GetByIdAsync(id);
        if (evidence == null)
        {
            throw new EntityNotFoundException($"Evidence with ID {id} not found");
        }

        // Enforce policy for delete action
        await EnforceAsync("delete", "Evidence", evidence);

        // Soft delete
        evidence.IsDeleted = true;
        evidence.DeletedAt = DateTime.UtcNow;
        evidence.ModifiedBy = CurrentUser.Id?.ToString();

        await _unitOfWork.Evidences.UpdateAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Evidence deleted successfully: {EvidenceId}", evidence.Id);
    }

    /// <summary>
    /// Get evidence by ID
    /// </summary>
    [Authorize(GrcPermissions.Evidence.View)]
    public async Task<EvidenceDto> GetAsync(Guid id)
    {
        var evidence = await _unitOfWork.Evidences.GetByIdAsync(id);
        if (evidence == null)
        {
            throw new EntityNotFoundException($"Evidence with ID {id} not found");
        }

        return MapToDto(evidence);
    }

    /// <summary>
    /// Get list of evidence with filtering
    /// </summary>
    [Authorize(GrcPermissions.Evidence.View)]
    public async Task<List<EvidenceDto>> GetListAsync(EvidenceFilterDto filter)
    {
        var query = _unitOfWork.Evidences.Query()
            .Where(e => !e.IsDeleted);

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(e => e.VerificationStatus == filter.Status);
        }

        if (!string.IsNullOrEmpty(filter.DataClassification))
        {
            query = query.Where(e => e.DataClassification == filter.DataClassification);
        }

        if (!string.IsNullOrEmpty(filter.Owner))
        {
            query = query.Where(e => e.Owner == filter.Owner);
        }

        var evidences = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(filter.SkipCount)
            .Take(filter.MaxResultCount)
            .ToListAsync();

        return evidences.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Helper method to enforce policy (from base class pattern)
    /// </summary>
    protected async Task EnforceAsync(string action, string resourceType, object resource)
    {
        var context = new PolicyContext
        {
            Action = action,
            ResourceType = resourceType,
            Resource = resource,
            Environment = GetEnvironment(),
            TenantId = CurrentTenant?.Id,
            PrincipalId = CurrentUser.Id?.ToString(),
            PrincipalRoles = GetCurrentUserRoles()
        };

        await _policyEnforcer.EnforceAsync(context);
    }

    private string GetEnvironment()
    {
        // In production, get from configuration
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "dev";
    }

    private IReadOnlyList<string> GetCurrentUserRoles()
    {
        // In production, get from current user's claims/roles
        return CurrentUser.Roles ?? new string[0];
    }

    private EvidenceDto MapToDto(Evidence evidence)
    {
        return new EvidenceDto
        {
            Id = evidence.Id,
            Title = evidence.Title,
            Description = evidence.Description,
            DataClassification = evidence.DataClassification,
            Owner = evidence.Owner,
            CollectionDate = evidence.CollectionDate,
            Status = evidence.VerificationStatus,
            ApprovedForProd = evidence.Labels.ContainsKey("approvedForProd") && evidence.Labels["approvedForProd"] == "true",
            CreatedAt = evidence.CreatedDate,
            CreatedBy = evidence.CreatedBy,
            UpdatedAt = evidence.ModifiedDate,
            UpdatedBy = evidence.ModifiedBy
        };
    }
}

/// <summary>
/// Base class for AppServices with common functionality
/// </summary>
public abstract class BaseAppService
{
    // These would be injected in a real implementation
    protected virtual ICurrentUser CurrentUser => new CurrentUserStub();
    protected virtual ICurrentTenant CurrentTenant => new CurrentTenantStub();
}

// Stub implementations for demonstration
public interface ICurrentUser
{
    Guid? Id { get; }
    string? UserName { get; }
    string[] Roles { get; }
}

public interface ICurrentTenant
{
    Guid? Id { get; }
}

public class CurrentUserStub : ICurrentUser
{
    public Guid? Id => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string? UserName => "admin";
    public string[] Roles => new[] { "Admin", "ComplianceManager" };
}

public class CurrentTenantStub : ICurrentTenant
{
    public Guid? Id => Guid.Parse("00000000-0000-0000-0000-000000000001");
}

// DTOs for Evidence operations
public class CreateEvidenceDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public DateTime? CollectionDate { get; set; }
}

public class UpdateEvidenceDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
}

public class ApprovalDto
{
    public string? Comments { get; set; }
    public bool ApprovedForProd { get; set; }
}

public class EvidenceDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public DateTime? CollectionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool ApprovedForProd { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class EvidenceFilterDto
{
    public string? Status { get; set; }
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 20;
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}
