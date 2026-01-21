using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    public class EscalationService : IEscalationService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(GrcDbContext context, ILogger<EscalationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Check for overdue approvals and trigger escalations
        /// </summary>
        public async Task<int> ProcessEscalationsAsync(Guid tenantId)
        {
            try
            {
                int escalationCount = 0;
                var pendingApprovals = await _context.ApprovalRecords
                    .Where(a => a.TenantId == tenantId && a.Status == "Pending" && a.DueDate < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var approval in pendingApprovals)
                {
                    var hoursOverdue = (int)(DateTime.UtcNow - approval.DueDate).TotalHours;
                    var escalationLevel = (hoursOverdue / 24) + 1; // Escalate every 24 hours

                    var existingEscalation = await _context.EscalationRules
                        .Where(e => e.WorkflowCategory == "Workflow" && e.IsActive)
                        .FirstOrDefaultAsync();

                    if (existingEscalation != null && escalationLevel > 1)
                    {
                        await EscalateApprovalAsync(tenantId, approval.Id, "System", 
                            $"Automatic escalation: {hoursOverdue} hours overdue");
                        escalationCount++;
                    }
                }

                _logger.LogInformation($"✅ Processed {escalationCount} escalations for tenant {tenantId}");
                return escalationCount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing escalations: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get escalations for a specific approval
        /// </summary>
        public async Task<List<EscalationDto>> GetEscalationsAsync(Guid tenantId, Guid approvalId)
        {
            try
            {
                var approval = await _context.ApprovalRecords
                    .FirstOrDefaultAsync(a => a.Id == approvalId && a.TenantId == tenantId);

                if (approval == null)
                {
                    return new List<EscalationDto>();
                }

                var hoursOverdue = approval.DueDate < DateTime.UtcNow 
                    ? (int)(DateTime.UtcNow - approval.DueDate).TotalHours 
                    : 0;

                var escalations = new List<EscalationDto>();
                if (hoursOverdue > 0)
                {
                    escalations.Add(new EscalationDto
                    {
                        Id = Guid.NewGuid(),
                        ApprovalId = approvalId,
                        WorkflowId = approval.WorkflowId,
                        Status = "Active",
                        EscalationLevel = (hoursOverdue / 24) + 1,
                        EscalatedTo = approval.AssignedTo,
                        EscalatedAt = DateTime.UtcNow.AddHours(-hoursOverdue),
                        Reason = "SLA breach - approval overdue",
                        HoursOverdue = hoursOverdue
                    });
                }

                return escalations;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting escalations: {ex.Message}");
                return new List<EscalationDto>();
            }
        }

        /// <summary>
        /// Manually trigger escalation for an approval
        /// </summary>
        public async Task<bool> EscalateApprovalAsync(Guid tenantId, Guid approvalId, string escalatedBy, string reason)
        {
            try
            {
                var approval = await _context.ApprovalRecords
                    .FirstOrDefaultAsync(a => a.Id == approvalId && a.TenantId == tenantId);

                if (approval == null)
                {
                    _logger.LogWarning($"❌ Approval {approvalId} not found");
                    return false;
                }

                // Escalate to next level
                approval.CurrentApprovalLevel++;

                // Create audit entry
                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = approval.WorkflowId,
                    EventType = "ApprovalEscalated",
                    ActingUserName = escalatedBy,
                    Description = $"Approval escalated to level {approval.CurrentApprovalLevel}. Reason: {reason}",
                    EventTime = DateTime.UtcNow
                };
                _context.WorkflowAuditEntries.Add(auditEntry);

                _context.ApprovalRecords.Update(approval);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Approval {approvalId} escalated to level {approval.CurrentApprovalLevel}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error escalating approval: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get escalation configuration for a workflow
        /// </summary>
        public async Task<EscalationConfigDto> GetEscalationConfigAsync(Guid tenantId, Guid workflowDefinitionId)
        {
            try
            {
                var definition = await _context.WorkflowDefinitions
                    .FirstOrDefaultAsync(w => w.Id == workflowDefinitionId && w.TenantId == tenantId);

                if (definition == null)
                {
                    return null;
                }

                var rules = await _context.EscalationRules
                    .Where(r => r.WorkflowCategory == "Workflow" && r.TenantId == tenantId && r.IsActive)
                    .ToListAsync();

                return new EscalationConfigDto
                {
                    WorkflowDefinitionId = workflowDefinitionId,
                    WorkflowName = definition.Name,
                    IsActive = definition.Status == "Active",
                    Rules = rules.Select(r => new EscalationRuleDto
                    {
                        Level = int.Parse(r.Name.Split('-').Last()),
                        HoursOverdue = r.DaysOverdueTrigger * 24,
                        EscalateTo = r.Action.Replace("EscalateTo", ""),
                        NotificationTemplate = r.NotificationConfig,
                        RequireAcknowledgment = true
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting escalation config: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update escalation rules
        /// </summary>
        public async Task<bool> UpdateEscalationRulesAsync(Guid tenantId, Guid workflowDefinitionId, List<EscalationRuleDto> rules)
        {
            try
            {
                // Remove existing rules for this workflow
                var existingRules = await _context.EscalationRules
                    .Where(r => r.WorkflowCategory == "Workflow" && r.TenantId == tenantId)
                    .ToListAsync();

                _context.EscalationRules.RemoveRange(existingRules);

                // Add new rules
                foreach (var rule in rules)
                {
                    var newRule = new EscalationRule
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Name = $"Rule-Level-{rule.Level}",
                        DaysOverdueTrigger = rule.HoursOverdue / 24,
                        Action = $"EscalateTo{rule.EscalateTo}",
                        NotificationConfig = rule.NotificationTemplate ?? "{}",
                        ShouldReassign = true,
                        WorkflowCategory = "Workflow",
                        IsActive = true
                    };
                    _context.EscalationRules.Add(newRule);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Updated escalation rules for workflow {workflowDefinitionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating escalation rules: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get escalation statistics for dashboard
        /// </summary>
        public async Task<EscalationStatsDto> GetEscalationStatsAsync(Guid tenantId)
        {
            try
            {
                var approvals = await _context.ApprovalRecords
                    .Where(a => a.TenantId == tenantId)
                    .ToListAsync();

                var escalationCount = approvals.Count(a => a.DueDate < DateTime.UtcNow && a.Status == "Pending");
                var escalatedApprovals = approvals.Where(a => a.DueDate < DateTime.UtcNow).ToList();

                return new EscalationStatsDto
                {
                    TotalEscalations = escalationCount,
                    ActiveEscalations = escalationCount,
                    ResolvedEscalations = approvals.Count(a => a.Status == "Approved"),
                    AverageHoursToResolve = escalatedApprovals.Any() 
                        ? escalatedApprovals
                            .Where(a => a.ApprovedAt.HasValue)
                            .Average(a => (a.ApprovedAt.Value - a.DueDate).TotalHours)
                        : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting escalation stats: {ex.Message}");
                return new EscalationStatsDto();
            }
        }
    }
}
