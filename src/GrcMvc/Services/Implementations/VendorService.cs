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

        public async Task<Result<VendorDto>> CreateAsync(CreateVendorDto dto)
        {
            if (dto == null)
            {
                return Result<VendorDto>.Failure(Error.Validation("Vendor data is required"));
            }

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
                return Result<VendorDto>.Success(_mapper.Map<VendorDto>(vendor));
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating vendor");
                return Result<VendorDto>.Failure(Error.Forbidden(pve.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vendor");
                return Result<VendorDto>.Failure(Error.Internal("Failed to create vendor", ex.Message));
            }
        }

        public async Task<Result<VendorDto>> UpdateAsync(Guid id, UpdateVendorDto dto)
        {
            if (dto == null)
            {
                return Result<VendorDto>.Failure(Error.Validation("Vendor update data is required"));
            }

            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for update", id);
                    return Result<VendorDto>.Failure(Error.NotFound("Vendor", id));
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
                return Result<VendorDto>.Success(_mapper.Map<VendorDto>(vendor));
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating vendor");
                return Result<VendorDto>.Failure(Error.Forbidden(pve.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor with ID {Id}", id);
                return Result<VendorDto>.Failure(Error.Internal("Failed to update vendor", ex.Message));
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for deletion", id);
                    return Result.Failure(Error.NotFound("Vendor", id));
                }

                vendor.IsDeleted = true;
                vendor.DeletedAt = DateTime.UtcNow;
                vendor.ModifiedBy = GetCurrentUser();
                vendor.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.UpdateAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor deleted with ID {Id}", id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vendor with ID {Id}", id);
                return Result.Failure(Error.Internal("Failed to delete vendor", ex.Message));
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

        public async Task<Result> AssessAsync(Guid id)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
                if (vendor == null)
                {
                    _logger.LogWarning("Vendor with ID {Id} not found for assessment", id);
                    return Result.Failure(Error.NotFound("Vendor", id));
                }

                vendor.LastAssessmentDate = DateTime.UtcNow;
                vendor.AssessmentStatus = "InProgress";
                vendor.ModifiedBy = GetCurrentUser();
                vendor.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Vendors.UpdateAsync(vendor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vendor assessment started for ID {Id}", id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing vendor with ID {Id}", id);
                return Result.Failure(Error.Internal("Failed to start vendor assessment", ex.Message));
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
