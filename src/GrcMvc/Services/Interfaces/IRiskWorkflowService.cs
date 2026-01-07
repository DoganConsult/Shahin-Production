using GrcMvc.Models.Entities;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Risk Acceptance Workflow Service Interface
    /// Handles: Assess → Accept/Reject → Monitor workflow
    /// </summary>
    public interface IRiskWorkflowService
    {
        Task<Risk> AcceptAsync(Guid riskId, string acceptedBy, string? comments = null);
        Task<Risk> RejectAcceptanceAsync(Guid riskId, string rejectedBy, string reason);
        Task<Risk> MarkMitigatedAsync(Guid riskId, string mitigatedBy, string mitigationDetails);
    }
}
