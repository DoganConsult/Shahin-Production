using GrcMvc.Models.Entities;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Evidence Approval Workflow Service Interface
    /// Handles: Submit → Review → Approve → Archive workflow
    /// </summary>
    public interface IEvidenceWorkflowService
    {
        Task<Evidence> SubmitForReviewAsync(Guid evidenceId, string submittedBy);
        Task<Evidence> ApproveAsync(Guid evidenceId, string approvedBy, string? comments = null);
        Task<Evidence> RejectAsync(Guid evidenceId, string rejectedBy, string reason);
        Task<Evidence> ArchiveAsync(Guid evidenceId, string archivedBy);
    }
}
