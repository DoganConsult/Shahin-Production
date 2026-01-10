using AutoMapper;
using GrcMvc.Common.Results;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    public class VendorService : IVendorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VendorService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public VendorService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<VendorService> logger,
            IHttpContextAccessor httpContextAccessor,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _workspaceContext = workspaceContext;
        }

        public async Task<VendorDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found", id);
                    return null;
                }
                return _mapper.Map<VendorDto>(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vendor with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            try
            {
                var vendors = await _unitOfWork.Vendors.GetAllAsync();
                return _mapper.Map<IEnumerable<VendorDto>>(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vendors");
                throw;
            }
        }

        public async Task<VendorDto> CreateAsync(CreateVendorDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var vendor = _mapper.Map<Vendor>(dto);
                
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    vendor.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Vendor",
                    resource: vendor,
                    dataClassification: vendor.DataClassification,
                    owner: vendor.Owner);

                vendor.CreatedBy = GetCurrentUser();
                vendor.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.AddAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor created with ID {Id}", vendor.Id);
                return _mapper.Map<VendorDto>(vendor);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating vendor");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vendor");
                throw;
            }
        }

        public async Task<VendorDto> UpdateAsync(Guid id, UpdateVendorDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for update", id);
                    return null!; // Caller should check for null
                }

                _mapper.Map(dto, vendor);

                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "Vendor",
                    resource: vendor,
                    dataClassification: vendor.DataClassification,
                    owner: vendor.Owner);

                vendor.ModifiedBy = GetCurrentUser();
                vendor.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.UpdateAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor updated with ID {Id}", id);
                return _mapper.Map<VendorDto>(vendor);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating vendor");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for deletion", id);
                    return; // Idempotent delete - already gone
                }

                vendor.IsDeleted = true;
                vendor.DeletedAt = DateTime.UtcNow;
                vendor.ModifiedBy = GetCurrentUser();
                vendor.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.UpdateAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor deleted with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vendor with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<VendorDto>> GetByStatusAsync(string status)
        {
            try
            {
                var vendors = await _unitOfWork.Vendors.GetAllAsync();
                var filtered = vendors.Where(v => v.Status == status && !v.IsDeleted);
                return _mapper.Map<IEnumerable<VendorDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vendors by status {Status}", status);
                throw;
            }
        }

        public async Task AssessAsync(Guid id)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for assessment", id);
                    return; // Cannot assess non-existent vendor
                }

                vendor.LastAssessmentDate = DateTime.UtcNow;
                vendor.AssessmentStatus = "InProgress";
                vendor.ModifiedBy = GetCurrentUser();
                vendor.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.UpdateAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor assessment started for ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing vendor with ID {Id}", id);
                throw;
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
