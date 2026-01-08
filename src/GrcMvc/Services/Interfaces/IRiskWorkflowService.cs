using GrcMvc.Models.Entities;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Risk Acceptance Workflow Service Interface
    /// Handles full risk lifecycle: Identify → Assess → Accept/Reject/Mitigate → Monitor → Close
    /// </summary>
    public interface IRiskWorkflowService
    {
        /// <summary>
        /// Accept a risk (acknowledge and monitor)
        /// </summary>
        Task<Risk> AcceptAsync(Guid riskId, string acceptedBy, string? comments = null);
        
        /// <summary>
        /// Reject risk acceptance (requires mitigation)
        /// </summary>
        Task<Risk> RejectAcceptanceAsync(Guid riskId, string rejectedBy, string reason);
        
        /// <summary>
        /// Mark risk as mitigated with details
        /// </summary>
        Task<Risk> MarkMitigatedAsync(Guid riskId, string mitigatedBy, string mitigationDetails);
        
        /// <summary>
        /// Start monitoring an accepted or mitigated risk
        /// </summary>
        Task<Risk> StartMonitoringAsync(Guid riskId, string monitoredBy);
        
        /// <summary>
        /// Close a risk (final state)
        /// </summary>
        Task<Risk> CloseAsync(Guid riskId, string closedBy, string? closureNotes = null);
    }
}
