using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    public class PolicyService : IPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public PolicyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PolicyService> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
        }

        public async Task<IEnumerable<PolicyDto>> GetAllAsync()
        {
            try
            {
                var policies = await _unitOfWork.Policies.GetAllAsync();
                return _mapper.Map<IEnumerable<PolicyDto>>(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all policies");
                throw;
            }
        }

        public async Task<PolicyDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(id);
                return policy == null ? null : _mapper.Map<PolicyDto>(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy with ID {PolicyId}", id);
                throw;
            }
        }

        public async Task<PolicyDto> CreateAsync(CreatePolicyDto createPolicyDto)
        {
            try
            {
                var policy = _mapper.Map<Policy>(createPolicyDto);
                policy.Id = Guid.NewGuid();
                policy.CreatedDate = DateTime.UtcNow;
                policy.Status = "Draft";
                policy.PolicyCode = GeneratePolicyCode(createPolicyDto.Category);
                policy.Version = "1.0";
                policy.IsActive = true;

                // Enforce policies before saving
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "PolicyDocument",
                    resource: policy,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: null); // Will be set to current user by helper if null

                await _unitOfWork.Policies.AddAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy created with ID {PolicyId}", policy.Id);
                return _mapper.Map<PolicyDto>(policy);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented policy creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                throw;
            }
        }

        public async Task<PolicyDto?> UpdateAsync(Guid id, UpdatePolicyDto updatePolicyDto)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(id);
                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {PolicyId} not found for update", id);
                    return null;
                }

                // Check if significant changes require version update
                bool requiresVersionUpdate = policy.Content != updatePolicyDto.Content ||
                                            policy.Requirements != updatePolicyDto.Requirements;

                _mapper.Map(updatePolicyDto, policy);
                policy.ModifiedDate = DateTime.UtcNow;

                if (requiresVersionUpdate)
                {
                    policy.Version = IncrementVersion(policy.Version);
                    _logger.LogInformation("Policy {PolicyId} version updated to {Version}",
                        id, policy.Version);
                }

                await _unitOfWork.Policies.UpdateAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy with ID {PolicyId} updated", id);
                return policy == null ? null : _mapper.Map<PolicyDto>(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy with ID {PolicyId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(id);
                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {PolicyId} not found for deletion", id);
                    return;
                }

                await _unitOfWork.Policies.DeleteAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy with ID {PolicyId} deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy with ID {PolicyId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PolicyDto>> GetByStatusAsync(string status)
        {
            try
            {
                var policies = await _unitOfWork.Policies.FindAsync(p => p.Status == status);
                return _mapper.Map<IEnumerable<PolicyDto>>(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<PolicyDto>> GetExpiringPoliciesAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(days);
                var policies = await _unitOfWork.Policies.FindAsync(
                    p => p.ExpiryDate <= cutoffDate && p.IsActive);

                return _mapper.Map<IEnumerable<PolicyDto>>(
                    policies.OrderBy(p => p.ExpiryDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring policies");
                throw;
            }
        }

        public async Task<PolicyStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var policies = await _unitOfWork.Policies.GetAllAsync();
                var policiesList = policies.ToList();
                var violations = await _unitOfWork.PolicyViolations.GetAllAsync();
                var violationsList = violations.ToList();

                return new PolicyStatisticsDto
                {
                    TotalPolicies = policiesList.Count,
                    ActivePolicies = policiesList.Count(p => p.IsActive),
                    DraftPolicies = policiesList.Count(p => p.Status == "Draft"),
                    ApprovedPolicies = policiesList.Count(p => p.Status == "Approved"),
                    ExpiredPolicies = policiesList.Count(p => p.ExpiryDate < DateTime.UtcNow),
                    TotalViolations = violationsList.Count,
                    OpenViolations = violationsList.Count(v => v.Status == "Open"),
                    PoliciesByCategory = policiesList.GroupBy(p => p.Category)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ComplianceRate = CalculateComplianceRate(policiesList, violationsList)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy statistics");
                throw;
            }
        }

        public async Task<PolicyViolationDto?> AddViolationAsync(Guid policyId, CreatePolicyViolationDto createViolationDto)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(policyId);
                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {PolicyId} not found", policyId);
                    return null;
                }

                var violation = _mapper.Map<PolicyViolation>(createViolationDto);
                violation.Id = Guid.NewGuid();
                violation.PolicyId = policyId;
                violation.CreatedDate = DateTime.UtcNow;
                violation.Status = "Open";
                violation.ViolationCode = GenerateViolationCode();

                await _unitOfWork.PolicyViolations.AddAsync(violation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Violation created with ID {ViolationId} for policy {PolicyId}",
                    violation.Id, policyId);
                return _mapper.Map<PolicyViolationDto>(violation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding violation to policy {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<IEnumerable<PolicyViolationDto>> GetViolationsByPolicyIdAsync(Guid policyId)
        {
            try
            {
                var violations = await _unitOfWork.PolicyViolations.FindAsync(v => v.PolicyId == policyId);
                return _mapper.Map<IEnumerable<PolicyViolationDto>>(violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting violations for policy {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<bool> ValidateComplianceAsync(Guid policyId)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(policyId);
                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {PolicyId} not found for compliance validation", policyId);
                    return false;
                }

                // Check for open violations
                var openViolations = await _unitOfWork.PolicyViolations.FindAsync(
                    v => v.PolicyId == policyId && v.Status == "Open");

                bool isCompliant = !openViolations.Any() && policy.IsActive && policy.Status == "Approved";

                _logger.LogInformation("Policy {PolicyId} compliance status: {IsCompliant}",
                    policyId, isCompliant);

                return isCompliant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating compliance for policy {PolicyId}", policyId);
                throw;
            }
        }

        private string GeneratePolicyCode(string category)
        {
            var prefix = category?.ToUpper().Substring(0, Math.Min(3, category.Length)) ?? "POL";
            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private string GenerateViolationCode()
        {
            return $"VIOL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private string IncrementVersion(string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion))
                return "1.0";

            var parts = currentVersion.Split('.');
            if (parts.Length == 2 && int.TryParse(parts[1], out int minorVersion))
            {
                return $"{parts[0]}.{minorVersion + 1}";
            }

            return "1.0";
        }

        public async Task<IEnumerable<PolicyDto>> GetByCategoryAsync(string category)
        {
            try
            {
                var policies = await _unitOfWork.Policies.FindAsync(p => p.Category.Equals(category));
                return _mapper.Map<IEnumerable<PolicyDto>>(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by category {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<PolicyViolationDto>> GetPolicyViolationsAsync(Guid id)
        {
            try
            {
                var violations = await _unitOfWork.PolicyViolations.FindAsync(v => v.PolicyId == id);
                return _mapper.Map<IEnumerable<PolicyViolationDto>>(violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting violations for policy ID {PolicyId}", id);
                throw;
            }
        }

        private double CalculateComplianceRate(List<Policy> policies, List<PolicyViolation> violations)
        {
            if (!policies.Any())
                return 100.0;

            var activePolicies = policies.Where(p => p.IsActive && p.Status == "Approved").ToList();
            if (!activePolicies.Any())
                return 100.0;

            var policiesWithViolations = violations
                .Where(v => v.Status == "Open")
                .Select(v => v.PolicyId)
                .Distinct()
                .Count();

            var compliantPolicies = activePolicies.Count - policiesWithViolations;
            return (double)compliantPolicies / activePolicies.Count * 100;
        }
    }
}