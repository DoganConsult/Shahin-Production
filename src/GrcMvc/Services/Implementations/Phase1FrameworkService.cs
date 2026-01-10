using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 1: Framework Service Implementation
    /// Manages controls and framework-related operations
    /// </summary>
    public class Phase1FrameworkService : IFrameworkService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<Phase1FrameworkService> _logger;
        private readonly IAuditTrailService _auditTrail;

        public Phase1FrameworkService(GrcDbContext context, ILogger<Phase1FrameworkService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;
        }

        /// <summary>
        /// Get a single control by ID
        /// </summary>
        public async Task<Models.Entities.Control> GetControlAsync(Guid controlId)
        {
            try
            {
                var controlIdStr = controlId.ToString();
                return await _context.Set<Models.Entities.Control>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ControlId == controlIdStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving control {controlId}");
                throw;
            }
        }

        /// <summary>
        /// Get all controls for a framework
        /// </summary>
        public async Task<List<Models.Entities.Control>> GetControlsByFrameworkAsync(Guid frameworkId)
        {
            try
            {
                return await _context.Set<Models.Entities.Control>()
                    .AsNoTracking()
                    .Where(c => c.TenantId != Guid.Empty && !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving controls for framework {frameworkId}");
                throw;
            }
        }

        /// <summary>
        /// Create a new control
        /// </summary>
        public async Task<Models.Entities.Control> CreateControlAsync(Guid tenantId, Guid frameworkId, string controlCode, string controlName, string description)
        {
            try
            {
                var control = new Models.Entities.Control
                {
                    ControlId = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    ControlCode = controlCode,
                    Name = controlName,
                    Description = description
                };

                _context.Set<Models.Entities.Control>().Add(control);
                await _context.SaveChangesAsync();

                await _auditTrail.LogCreatedAsync(tenantId, "Control", Guid.Parse(control.ControlId));
                _logger.LogInformation($"Control created: {controlCode} for tenant {tenantId}");

                return control;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating control {controlCode}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing control
        /// </summary>
        public async Task UpdateControlAsync(Models.Entities.Control control)
        {
            try
            {
                if (control == null) return;

                // Control entity doesn't have LastModifiedDate - using BaseEntity's UpdatedAt
                _context.Set<Models.Entities.Control>().Update(control);
                await _context.SaveChangesAsync();

                await _auditTrail.LogUpdatedAsync(control.TenantId ?? Guid.Empty, "Control", Guid.Parse(control.ControlId), "Updated", "", "");
                _logger.LogInformation($"Control updated: {control.ControlId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating control {control?.ControlId}");
                throw;
            }
        }

        /// <summary>
        /// Delete a control
        /// </summary>
        public async Task DeleteControlAsync(Guid controlId)
        {
            try
            {
                var control = await GetControlAsync(controlId);
                if (control == null) return;

                _context.Set<Models.Entities.Control>().Remove(control);
                await _context.SaveChangesAsync();

                await _auditTrail.LogDeletedAsync(control.TenantId ?? Guid.Empty, "Control", controlId);
                _logger.LogInformation($"Control deleted: {controlId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting control {controlId}");
                throw;
            }
        }

        /// <summary>
        /// Search controls by search term
        /// </summary>
        public async Task<List<Models.Entities.Control>> SearchControlsAsync(Guid tenantId, string searchTerm)
        {
            try
            {
                return await _context.Set<Models.Entities.Control>()
                    .AsNoTracking()
                    .Where(c => c.TenantId == tenantId &&
                               (c.ControlCode.Contains(searchTerm) ||
                                c.Name.Contains(searchTerm) ||
                                c.Description.Contains(searchTerm)))
                    .OrderBy(c => c.ControlCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching controls for tenant {tenantId}");
                throw;
            }
        }
    }
}
