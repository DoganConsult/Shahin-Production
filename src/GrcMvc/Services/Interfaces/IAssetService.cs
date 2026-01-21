using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Asset Service Interface
    /// Manages organization assets for recognition and scoping
    /// Assets drive control applicability (e.g., PCI data → PCI-DSS controls)
    /// </summary>
    public interface IAssetService
    {
        // ===== CRUD Operations =====
        Task<Asset> CreateAssetAsync(Guid tenantId, CreateAssetDto dto, string userId);
        Task<Asset?> GetAssetAsync(Guid assetId);
        Task<Asset?> GetAssetByCodeAsync(Guid tenantId, string assetCode);
        Task<List<Asset>> GetAssetsByTenantAsync(Guid tenantId, AssetFilterDto? filter = null);
        Task<Asset> UpdateAssetAsync(Guid assetId, UpdateAssetDto dto, string userId);
        Task<bool> DeleteAssetAsync(Guid assetId, string userId);

        // ===== Classification Operations =====
        Task<Asset> UpdateClassificationAsync(Guid assetId, string criticality, string dataClassification, string userId);
        Task<List<Asset>> GetAssetsByCriticalityAsync(Guid tenantId, string criticality);
        Task<List<Asset>> GetAssetsByDataClassificationAsync(Guid tenantId, string dataClassification);
        Task<List<Asset>> GetPciAssetsAsync(Guid tenantId);
        Task<List<Asset>> GetPiiAssetsAsync(Guid tenantId);

        // ===== Ownership Operations =====
        Task<Asset> AssignOwnerAsync(Guid assetId, Guid? userId, Guid? teamId, string updatedBy);
        Task<List<Asset>> GetAssetsByOwnerAsync(Guid tenantId, Guid userId);
        Task<List<Asset>> GetAssetsByTeamAsync(Guid tenantId, Guid teamId);
        Task<List<Asset>> GetUnassignedAssetsAsync(Guid tenantId);

        // ===== Scope Operations =====
        Task<List<Asset>> GetInScopeAssetsAsync(Guid tenantId);
        Task<Asset> SetInScopeAsync(Guid assetId, bool inScope, string userId);
        Task<List<Asset>> GetAssetsByEnvironmentAsync(Guid tenantId, string environment);
        Task<List<Asset>> GetAssetsByHostingModelAsync(Guid tenantId, string hostingModel);

        // ===== Aggregation & Analytics =====
        Task<AssetSummaryDto> GetAssetSummaryAsync(Guid tenantId);
        Task<Dictionary<string, int>> GetAssetCountByTypeAsync(Guid tenantId);
        Task<Dictionary<string, int>> GetAssetCountByCriticalityAsync(Guid tenantId);
        Task<Dictionary<string, List<string>>> GetDataTypesAcrossAssetsAsync(Guid tenantId);

        // ===== Sync Operations =====
        Task<int> BulkImportAsync(Guid tenantId, List<CreateAssetDto> assets, string userId);
        Task<Asset> SyncFromExternalAsync(Guid tenantId, string systemId, string sourceSystem, CreateAssetDto dto, string userId);
        Task<List<Asset>> GetAssetsBySourceSystemAsync(Guid tenantId, string sourceSystem);
    }

    // ===== DTOs =====
    public class CreateAssetDto
    {
        public string AssetCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssetType { get; set; } = string.Empty;
        public string? SubType { get; set; }
        public string? SystemId { get; set; }
        public string SourceSystem { get; set; } = "Manual";
        public string Criticality { get; set; } = "T3";
        public string DataClassification { get; set; } = "Internal";
        public string? DataTypes { get; set; }
        public Guid? OwnerUserId { get; set; }
        public Guid? OwnerTeamId { get; set; }
        public string? BusinessOwner { get; set; }
        public string? TechnicalOwner { get; set; }
        public string? HostingModel { get; set; }
        public string? CloudProvider { get; set; }
        public string Environment { get; set; } = "Production";
        public string? Location { get; set; }
        public bool IsInScope { get; set; } = true;
        public Dictionary<string, string>? Tags { get; set; }
        public Dictionary<string, object>? Attributes { get; set; }
    }

    public class UpdateAssetDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? AssetType { get; set; }
        public string? SubType { get; set; }
        public string? Criticality { get; set; }
        public string? DataClassification { get; set; }
        public string? DataTypes { get; set; }
        public Guid? OwnerUserId { get; set; }
        public Guid? OwnerTeamId { get; set; }
        public string? BusinessOwner { get; set; }
        public string? TechnicalOwner { get; set; }
        public string? HostingModel { get; set; }
        public string? CloudProvider { get; set; }
        public string? Environment { get; set; }
        public string? Location { get; set; }
        public bool? IsInScope { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
        public Dictionary<string, object>? Attributes { get; set; }
    }

    public class AssetFilterDto
    {
        public string? AssetType { get; set; }
        public string? Criticality { get; set; }
        public string? DataClassification { get; set; }
        public string? Environment { get; set; }
        public string? HostingModel { get; set; }
        public string? Status { get; set; }
        public bool? IsInScope { get; set; }
        public Guid? OwnerUserId { get; set; }
        public Guid? OwnerTeamId { get; set; }
        public string? SearchTerm { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    public class AssetSummaryDto
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int InScopeAssets { get; set; }
        
        // Criticality breakdown
        public int Tier1Critical { get; set; }
        public int Tier2High { get; set; }
        public int Tier3Medium { get; set; }
        public int Tier4Low { get; set; }
        
        // Data classification breakdown
        public int WithPciData { get; set; }
        public int WithPiiData { get; set; }
        public int WithPhiData { get; set; }
        public int RestrictedData { get; set; }
        public int ConfidentialData { get; set; }
        
        // Ownership
        public int AssignedAssets { get; set; }
        public int UnassignedAssets { get; set; }
        
        // Environment
        public int ProductionAssets { get; set; }
        public int NonProductionAssets { get; set; }
        
        // Hosting
        public int CloudAssets { get; set; }
        public int OnPremAssets { get; set; }
        public int HybridAssets { get; set; }
        
        // Risk
        public decimal AverageRiskScore { get; set; }
        public int HighRiskAssets { get; set; }
    }
}
