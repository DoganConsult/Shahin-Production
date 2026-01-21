using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IControlService
    {
        Task<IEnumerable<ControlDto>> GetAllAsync();
        Task<ControlDto> GetByIdAsync(Guid id);
        Task<ControlDto> CreateAsync(CreateControlDto createControlDto);
        Task<ControlDto> UpdateAsync(Guid id, UpdateControlDto updateControlDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ControlDto>> GetByRiskIdAsync(Guid riskId);
        Task<ControlStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Test a control and update effectiveness score
        /// </summary>
        Task<ControlTestResultDto> TestControlAsync(Guid controlId, ControlTestRequest request);

        /// <summary>
        /// Get control effectiveness score
        /// </summary>
        Task<int> GetEffectivenessScoreAsync(Guid controlId);

        /// <summary>
        /// Assign control owner
        /// </summary>
        Task<ControlDto> AssignOwnerAsync(Guid controlId, string ownerId, string ownerName);

        /// <summary>
        /// Get controls by framework
        /// </summary>
        Task<IEnumerable<ControlDto>> GetByFrameworkAsync(string frameworkCode);

        /// <summary>
        /// Link control to risk
        /// </summary>
        Task LinkToRiskAsync(Guid controlId, Guid riskId, int expectedEffectiveness);
    }

    /// <summary>
    /// Control test request
    /// </summary>
    public class ControlTestRequest
    {
        public string TestType { get; set; } = "Effectiveness"; // Effectiveness, Design, Operating
        public string TesterId { get; set; } = string.Empty;
        public string TesterName { get; set; } = string.Empty;
        public string TestNotes { get; set; } = string.Empty;
        public int Score { get; set; } // 0-100
        public string Result { get; set; } = "Pass"; // Pass, Fail, Partial
        public List<string>? EvidenceIds { get; set; }
    }

    /// <summary>
    /// Control test result
    /// </summary>
    public class ControlTestResultDto
    {
        public Guid ControlId { get; set; }
        public Guid TestId { get; set; }
        public DateTime TestedDate { get; set; }
        public string TestType { get; set; } = string.Empty;
        public string TesterName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PreviousEffectiveness { get; set; }
        public int NewEffectiveness { get; set; }
    }
}