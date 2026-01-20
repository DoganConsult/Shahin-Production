using GrcMvc.Services.Base;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Examples
{
    /// <summary>
    /// EXAMPLE: DTO-only service pattern
    /// 
    /// Pattern:
    /// - Service inherits TenantAwareAppService (automatic tenant validation)
    /// - Methods accept/return DTOs only (no EF entities)
    /// - TenantId is automatically available via base class
    /// - Business logic is isolated from persistence
    /// </summary>
    public interface IWidgetAppService
    {
        Task<WidgetSearchResponseDto> SearchAsync(WidgetSearchRequestDto dto, CancellationToken ct);
        Task<WidgetDto> GetAsync(Guid id, CancellationToken ct);
        Task<WidgetDto> CreateAsync(CreateWidgetRequestDto dto, CancellationToken ct);
    }

    public sealed class WidgetAppService : TenantAwareAppService, IWidgetAppService
    {
        private readonly GrcDbContext _dbContext;

        public WidgetAppService(
            ITenantContextService tenantContext,
            GrcDbContext dbContext,
            ILogger<WidgetAppService> logger)
            : base(tenantContext, logger)
        {
            _dbContext = dbContext;
        }

        public async Task<WidgetSearchResponseDto> SearchAsync(WidgetSearchRequestDto dto, CancellationToken ct)
        {
            // TenantId is automatically available from base class
            // No need to validate - TenantAwareAppService does it at construction

            var query = _dbContext.Set<WidgetEntity>()
                .Where(w => w.TenantId == TenantId); // Use TenantId from base class

            if (!string.IsNullOrEmpty(dto.Query))
            {
                query = query.Where(w => w.Name.Contains(dto.Query) || w.Description.Contains(dto.Query));
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(w => w.Name)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(w => new WidgetDto(w.Id, w.Name, w.Description, w.CreatedAt))
                .ToListAsync(ct);

            return new WidgetSearchResponseDto(items, total);
        }

        public async Task<WidgetDto> GetAsync(Guid id, CancellationToken ct)
        {
            var widget = await _dbContext.Set<WidgetEntity>()
                .Where(w => w.Id == id && w.TenantId == TenantId)
                .Select(w => new WidgetDto(w.Id, w.Name, w.Description, w.CreatedAt))
                .FirstOrDefaultAsync(ct);

            if (widget == null)
                throw new EntityNotFoundException("Widget", id);

            return widget;
        }

        public async Task<WidgetDto> CreateAsync(CreateWidgetRequestDto dto, CancellationToken ct)
        {
            var widget = new WidgetEntity
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<WidgetEntity>().Add(widget);
            await _dbContext.SaveChangesAsync(ct);

            return new WidgetDto(widget.Id, widget.Name, widget.Description, widget.CreatedAt);
        }
    }

    // Example entity (not exposed to controller)
    internal class WidgetEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
