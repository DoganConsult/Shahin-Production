using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for recording workflow audit events
    /// </summary>
    public interface IWorkflowAuditService
    {
        Task RecordInstanceEventAsync(
            WorkflowInstance instance,
            string eventType,
            string? oldStatus,
            string description);

        Task RecordTaskEventAsync(
            WorkflowTask task,
            string eventType,
            string? oldStatus,
            string description);
    }
}
