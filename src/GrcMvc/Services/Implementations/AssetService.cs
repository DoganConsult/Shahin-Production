using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Asset Service Implementation
    /// Manages organization assets for recognition and scoping
    /// Assets drive control applicability through data classification
    /// </summary>
    public class AssetService : IAssetService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<AssetService> _logger;

        public AssetService(GrcDbContext context, ILogger<AssetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== CRUD Operations =====

        public async Task<Asset> CreateAssetAsync(Guid tenantId, CreateAssetDto dto, string userId)
        {
            // Check for duplicate asset code
            var exists = await _context.Assets
                .AnyAsync(a => a.TenantId == tenantId && a.AssetCode == dto.AssetCode && !a.IsDeleted);
            if (exists)
                throw new EntityExistsException("Asset", "AssetCode", dto.AssetCode);

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetCode = dto.AssetCode,
                Name = dto.Name,
                Description = dto.Description ?? "",
                AssetType = dto.AssetType,
                SubType = dto.SubType ?? "",
                SystemId = dto.SystemId ?? "",
                SourceSystem = dto.SourceSystem,
                Criticality = dto.Criticality,
                DataClassification = dto.DataClassification,
                DataTypes = dto.DataTypes ?? "",
                OwnerUserId = dto.OwnerUserId,
                OwnerTeamId = dto.OwnerTeamId,
                BusinessOwner = dto.BusinessOwner ?? "",
                TechnicalOwner = dto.TechnicalOwner ?? "",
                HostingModel = dto.HostingModel ?? "",
                CloudProvider = dto.CloudProvider ?? "",
                Environment = dto.Environment,
                Location = dto.Location ?? "",
                IsInScope = dto.IsInScope,
                TagsJson = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : "{}",
                AttributesJson = dto.Attributes != null ? JsonSerializer.Serialize(dto.Attributes) : "{}",
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created asset {AssetCode} for tenant {TenantId}", dto.AssetCode, tenantId);
            return asset;
        }

        public async Task<Asset?> GetAssetAsync(Guid assetId)
        {
            return await _context.Assets
                .Include(a => a.OwnerTeam)
                .FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);
        }

        public async Task<Asset?> GetAssetByCodeAsync(Guid tenantId, string assetCode)
        {
            return await _context.Assets
                .Include(a => a.OwnerTeam)
                .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AssetCode == assetCode && !a.IsDeleted);
        }

        public async Task<List<Asset>> GetAssetsByTenantAsync(Guid tenantId, AssetFilterDto? filter = null)
        {
            var query = _context.Assets
                .Where(a => a.TenantId == tenantId && !a.IsDeleted);

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.AssetType))
                    query = query.Where(a => a.AssetType == filter.AssetType);
                if (!string.IsNullOrEmpty(filter.Criticality))
                    query = query.Where(a => a.Criticality == filter.Criticality);
                if (!string.IsNullOrEmpty(filter.DataClassification))
                    query = query.Where(a => a.DataClassification == filter.DataClassification);
                if (!string.IsNullOrEmpty(filter.Environment))
                    query = query.Where(a => a.Environment == filter.Environment);
                if (!string.IsNullOrEmpty(filter.HostingModel))
                    query = query.Where(a => a.HostingModel == filter.HostingModel);
                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(a => a.Status == filter.Status);
                if (filter.IsInScope.HasValue)
                    query = query.Where(a => a.IsInScope == filter.IsInScope.Value);
                if (filter.OwnerUserId.HasValue)
                    query = query.Where(a => a.OwnerUserId == filter.OwnerUserId);
                if (filter.OwnerTeamId.HasValue)
                    query = query.Where(a => a.OwnerTeamId == filter.OwnerTeamId);
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                    query = query.Where(a => a.Name.Contains(filter.SearchTerm) ||
                                             a.AssetCode.Contains(filter.SearchTerm) ||
                                             a.Description.Contains(filter.SearchTerm));
            }

            query = query.OrderBy(a => a.AssetCode);

            if (filter?.Skip.HasValue == true)
                query = query.Skip(filter.Skip.Value);
            if (filter?.Take.HasValue == true)
                query = query.Take(filter.Take.Value);

            return await query.Include(a => a.OwnerTeam).ToListAsync();
        }

        public async Task<Asset> UpdateAssetAsync(Guid assetId, UpdateAssetDto dto, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || asset.IsDeleted)
                throw new EntityNotFoundException("Asset", assetId);

            if (!string.IsNullOrEmpty(dto.Name)) asset.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) asset.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.AssetType)) asset.AssetType = dto.AssetType;
            if (!string.IsNullOrEmpty(dto.SubType)) asset.SubType = dto.SubType;
            if (!string.IsNullOrEmpty(dto.Criticality)) asset.Criticality = dto.Criticality;
            if (!string.IsNullOrEmpty(dto.DataClassification)) asset.DataClassification = dto.DataClassification;
            if (!string.IsNullOrEmpty(dto.DataTypes)) asset.DataTypes = dto.DataTypes;
            if (dto.OwnerUserId.HasValue) asset.OwnerUserId = dto.OwnerUserId;
            if (dto.OwnerTeamId.HasValue) asset.OwnerTeamId = dto.OwnerTeamId;
            if (!string.IsNullOrEmpty(dto.BusinessOwner)) asset.BusinessOwner = dto.BusinessOwner;
            if (!string.IsNullOrEmpty(dto.TechnicalOwner)) asset.TechnicalOwner = dto.TechnicalOwner;
            if (!string.IsNullOrEmpty(dto.HostingModel)) asset.HostingModel = dto.HostingModel;
            if (!string.IsNullOrEmpty(dto.CloudProvider)) asset.CloudProvider = dto.CloudProvider;
            if (!string.IsNullOrEmpty(dto.Environment)) asset.Environment = dto.Environment;
            if (!string.IsNullOrEmpty(dto.Location)) asset.Location = dto.Location;
            if (dto.IsInScope.HasValue) asset.IsInScope = dto.IsInScope.Value;
            if (!string.IsNullOrEmpty(dto.Status)) asset.Status = dto.Status;
            if (dto.Tags != null) asset.TagsJson = JsonSerializer.Serialize(dto.Tags);
            if (dto.Attributes != null) asset.AttributesJson = JsonSerializer.Serialize(dto.Attributes);

            asset.ModifiedDate = DateTime.UtcNow;
            asset.ModifiedBy = userId;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated asset {AssetId}", assetId);
            return asset;
        }

        public async Task<bool> DeleteAssetAsync(Guid assetId, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return false;

            asset.IsDeleted = true;
            asset.ModifiedDate = DateTime.UtcNow;
            asset.ModifiedBy = userId;
            asset.Status = "Decommissioned";

            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted asset {AssetId}", assetId);
            return true;
        }

        // ===== Classification Operations =====

        public async Task<Asset> UpdateClassificationAsync(Guid assetId, string criticality, string dataClassification, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || asset.IsDeleted)
                throw new EntityNotFoundException("Asset", assetId);

            asset.Criticality = criticality;
            asset.DataClassification = dataClassification;
            asset.ModifiedDate = DateTime.UtcNow;
            asset.ModifiedBy = userId;

            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task<List<Asset>> GetAssetsByCriticalityAsync(Guid tenantId, string criticality)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.Criticality == criticality && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetAssetsByDataClassificationAsync(Guid tenantId, string dataClassification)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.DataClassification == dataClassification && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetPciAssetsAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId &&
                           (a.DataClassification.Contains("PCI") || a.DataTypes.Contains("PCI")) &&
                           !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetPiiAssetsAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId &&
                           (a.DataClassification.Contains("PII") || a.DataTypes.Contains("PII")) &&
                           !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        // ===== Ownership Operations =====

        public async Task<Asset> AssignOwnerAsync(Guid assetId, Guid? userId, Guid? teamId, string updatedBy)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || asset.IsDeleted)
                throw new EntityNotFoundException("Asset", assetId);

            asset.OwnerUserId = userId;
            asset.OwnerTeamId = teamId;
            asset.ModifiedDate = DateTime.UtcNow;
            asset.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task<List<Asset>> GetAssetsByOwnerAsync(Guid tenantId, Guid userId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.OwnerUserId == userId && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetAssetsByTeamAsync(Guid tenantId, Guid teamId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.OwnerTeamId == teamId && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetUnassignedAssetsAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId &&
                           a.OwnerUserId == null && a.OwnerTeamId == null &&
                           !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        // ===== Scope Operations =====

        public async Task<List<Asset>> GetInScopeAssetsAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.IsInScope && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<Asset> SetInScopeAsync(Guid assetId, bool inScope, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || asset.IsDeleted)
                throw new EntityNotFoundException("Asset", assetId);

            asset.IsInScope = inScope;
            asset.ModifiedDate = DateTime.UtcNow;
            asset.ModifiedBy = userId;

            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task<List<Asset>> GetAssetsByEnvironmentAsync(Guid tenantId, string environment)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.Environment == environment && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        public async Task<List<Asset>> GetAssetsByHostingModelAsync(Guid tenantId, string hostingModel)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.HostingModel == hostingModel && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }

        // ===== Aggregation & Analytics =====

        public async Task<AssetSummaryDto> GetAssetSummaryAsync(Guid tenantId)
        {
            var assets = await _context.Assets
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();

            return new AssetSummaryDto
            {
                TotalAssets = assets.Count,
                ActiveAssets = assets.Count(a => a.Status == "Active"),
                InScopeAssets = assets.Count(a => a.IsInScope),

                Tier1Critical = assets.Count(a => a.Criticality == "T1"),
                Tier2High = assets.Count(a => a.Criticality == "T2"),
                Tier3Medium = assets.Count(a => a.Criticality == "T3"),
                Tier4Low = assets.Count(a => a.Criticality == "T4"),

                WithPciData = assets.Count(a => a.DataClassification.Contains("PCI") || a.DataTypes.Contains("PCI")),
                WithPiiData = assets.Count(a => a.DataClassification.Contains("PII") || a.DataTypes.Contains("PII")),
                WithPhiData = assets.Count(a => a.DataClassification.Contains("PHI") || a.DataTypes.Contains("PHI")),
                RestrictedData = assets.Count(a => a.DataClassification == "Restricted"),
                ConfidentialData = assets.Count(a => a.DataClassification == "Confidential"),

                AssignedAssets = assets.Count(a => a.OwnerUserId.HasValue || a.OwnerTeamId.HasValue),
                UnassignedAssets = assets.Count(a => !a.OwnerUserId.HasValue && !a.OwnerTeamId.HasValue),

                ProductionAssets = assets.Count(a => a.Environment == "Production"),
                NonProductionAssets = assets.Count(a => a.Environment != "Production"),

                CloudAssets = assets.Count(a => a.HostingModel == "Cloud" || a.HostingModel == "SaaS"),
                OnPremAssets = assets.Count(a => a.HostingModel == "OnPremise"),
                HybridAssets = assets.Count(a => a.HostingModel == "Hybrid"),

                AverageRiskScore = assets.Where(a => a.RiskScore.HasValue).Any()
                    ? (decimal)assets.Where(a => a.RiskScore.HasValue).Average(a => a.RiskScore!.Value)
                    : 0,
                HighRiskAssets = assets.Count(a => a.RiskScore >= 70)
            };
        }

        public async Task<Dictionary<string, int>> GetAssetCountByTypeAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .GroupBy(a => a.AssetType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetAssetCountByCriticalityAsync(Guid tenantId)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .GroupBy(a => a.Criticality)
                .Select(g => new { Criticality = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Criticality, x => x.Count);
        }

        public async Task<Dictionary<string, List<string>>> GetDataTypesAcrossAssetsAsync(Guid tenantId)
        {
            var assets = await _context.Assets
                .Where(a => a.TenantId == tenantId && !a.IsDeleted && !string.IsNullOrEmpty(a.DataTypes))
                .Select(a => new { a.AssetCode, a.DataTypes })
                .ToListAsync();

            var result = new Dictionary<string, List<string>>();
            foreach (var asset in assets)
            {
                var types = asset.DataTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
                foreach (var type in types)
                {
                    if (!result.ContainsKey(type))
                        result[type] = new List<string>();
                    result[type].Add(asset.AssetCode);
                }
            }
            return result;
        }

        // ===== Sync Operations =====

        public async Task<int> BulkImportAsync(Guid tenantId, List<CreateAssetDto> assets, string userId)
        {
            var count = 0;
            foreach (var dto in assets)
            {
                try
                {
                    await CreateAssetAsync(tenantId, dto, userId);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import asset {AssetCode}", dto.AssetCode);
                }
            }
            return count;
        }

        public async Task<Asset> SyncFromExternalAsync(Guid tenantId, string systemId, string sourceSystem, CreateAssetDto dto, string userId)
        {
            // Check if asset exists by system ID
            var existing = await _context.Assets
                .FirstOrDefaultAsync(a => a.TenantId == tenantId &&
                                         a.SystemId == systemId &&
                                         a.SourceSystem == sourceSystem &&
                                         !a.IsDeleted);

            if (existing != null)
            {
                // Update existing asset
                existing.Name = dto.Name;
                existing.Description = dto.Description ?? existing.Description;
                existing.AssetType = dto.AssetType;
                existing.SubType = dto.SubType ?? existing.SubType;
                existing.Criticality = dto.Criticality;
                existing.DataClassification = dto.DataClassification;
                existing.DataTypes = dto.DataTypes ?? existing.DataTypes;
                existing.HostingModel = dto.HostingModel ?? existing.HostingModel;
                existing.CloudProvider = dto.CloudProvider ?? existing.CloudProvider;
                existing.Environment = dto.Environment;
                existing.Location = dto.Location ?? existing.Location;
                existing.LastSyncDate = DateTime.UtcNow;
                existing.LastSyncStatus = "Success";
                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = userId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Synced existing asset {SystemId} from {SourceSystem}", systemId, sourceSystem);
                return existing;
            }
            else
            {
                // Create new asset
                dto.SystemId = systemId;
                dto.SourceSystem = sourceSystem;
                var asset = await CreateAssetAsync(tenantId, dto, userId);
                asset.LastSyncDate = DateTime.UtcNow;
                asset.LastSyncStatus = "Success";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created asset {SystemId} from {SourceSystem}", systemId, sourceSystem);
                return asset;
            }
        }

        public async Task<List<Asset>> GetAssetsBySourceSystemAsync(Guid tenantId, string sourceSystem)
        {
            return await _context.Assets
                .Where(a => a.TenantId == tenantId && a.SourceSystem == sourceSystem && !a.IsDeleted)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();
        }
    }
}
