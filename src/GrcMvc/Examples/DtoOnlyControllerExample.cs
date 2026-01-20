using Microsoft.AspNetCore.Mvc;
using GrcMvc.Authorization;
using GrcMvc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Examples
{
    /// <summary>
    /// EXAMPLE: DTO-only controller pattern
    /// 
    /// Pattern:
    /// - Controller accepts RequestDto and returns ResponseDto only
    /// - No EF entities cross controller/service boundary
    /// - [RequireTenant] enforces tenant context
    /// - Service owns business logic
    /// </summary>
    [RequireTenant]
    [ApiController]
    [Route("api/example/widgets")]
    public sealed class WidgetController : ControllerBase
    {
        private readonly IWidgetAppService _service;

        public WidgetController(IWidgetAppService service)
        {
            _service = service;
        }

        [HttpPost("search")]
        public Task<WidgetSearchResponseDto> SearchAsync(WidgetSearchRequestDto dto, CancellationToken ct)
            => _service.SearchAsync(dto, ct);

        [HttpGet("{id}")]
        public Task<WidgetDto> GetAsync(Guid id, CancellationToken ct)
            => _service.GetAsync(id, ct);

        [HttpPost]
        public Task<WidgetDto> CreateAsync(CreateWidgetRequestDto dto, CancellationToken ct)
            => _service.CreateAsync(dto, ct);
    }

    // Request DTOs
    public sealed record WidgetSearchRequestDto(string Query, int Page, int PageSize);
    public sealed record CreateWidgetRequestDto(string Name, string Description);

    // Response DTOs
    public sealed record WidgetSearchResponseDto(IReadOnlyList<WidgetDto> Items, int Total);
    public sealed record WidgetDto(Guid Id, string Name, string Description, DateTime CreatedAt);
}
