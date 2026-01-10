using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for Asset Management
    /// Assets drive control applicability through data classification (PCI, PHI, PII)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOnboardingService _onboardingService;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(
            IAssetService assetService,
            ICurrentUserService currentUserService,
            IOnboardingService onboardingService,
            ILogger<AssetsController> logger)
        {
            _assetService = assetService;
            _currentUserService = currentUserService;
            _onboardingService = onboardingService;
            _logger = logger;
        }

        #region CRUD Operations

        /// <summary>
        /// Get all assets for current tenant
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Asset>>> GetAssets([FromQuery] AssetFilterDto? filter = null)
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetAssetsByTenantAsync(tenantId, filter);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assets");
                return StatusCode(500, new { error = "Failed to retrieve assets" });
            }
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Asset>> GetAsset(Guid id)
        {
            try
            {
                var asset = await _assetService.GetAssetAsync(id);
                if (asset == null)
                    return NotFound(new { error = "Asset not found" });

                return Ok(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset {AssetId}", id);
                return StatusCode(500, new { error = "Failed to retrieve asset" });
            }
        }

        /// <summary>
        /// Get asset by code
        /// </summary>
        [HttpGet("by-code/{assetCode}")]
        public async Task<ActionResult<Asset>> GetAssetByCode(string assetCode)
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var asset = await _assetService.GetAssetByCodeAsync(tenantId, assetCode);
                if (asset == null)
                    return NotFound(new { error = "Asset not found" });

                return Ok(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset by code {AssetCode}", assetCode);
                return StatusCode(500, new { error = "Failed to retrieve asset" });
            }
        }

        /// <summary>
        /// Create a new asset
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Asset>> CreateAsset([FromBody] CreateAssetDto dto)
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var userId = _currentUserService.GetUserName();

                var asset = await _assetService.CreateAssetAsync(tenantId, dto, userId);

                // Trigger scope refresh if asset has compliance-relevant data
                if (HasComplianceRelevantData(dto))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _onboardingService.RefreshScopeAsync(tenantId, userId, $"New asset added: {dto.AssetCode}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to refresh scope after asset creation");
                        }
                    });
                }

                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset");
                return StatusCode(500, new { error = "Failed to create asset" });
            }
        }

        /// <summary>
        /// Update an existing asset
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Asset>> UpdateAsset(Guid id, [FromBody] UpdateAssetDto dto)
        {
            try
            {
                var userId = _currentUserService.GetUserName();
                var asset = await _assetService.UpdateAssetAsync(id, dto, userId);

                // Trigger scope refresh if classification changed
                if (dto.DataClassification != null || dto.DataTypes != null)
                {
                    var tenantId = _currentUserService.GetTenantId();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _onboardingService.RefreshScopeAsync(tenantId, userId, $"Asset updated: {asset.AssetCode}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to refresh scope after asset update");
                        }
                    });
                }

                return Ok(asset);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset {AssetId}", id);
                return StatusCode(500, new { error = "Failed to update asset" });
            }
        }

        /// <summary>
        /// Delete (soft) an asset
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteAsset(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserName();
                var result = await _assetService.DeleteAssetAsync(id, userId);

                if (!result)
                    return NotFound(new { error = "Asset not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset {AssetId}", id);
                return StatusCode(500, new { error = "Failed to delete asset" });
            }
        }

        #endregion

        #region Classification Operations

        /// <summary>
        /// Update asset classification
        /// </summary>
        [HttpPut("{id:guid}/classification")]
        public async Task<ActionResult<Asset>> UpdateClassification(
            Guid id,
            [FromBody] UpdateClassificationDto dto)
        {
            try
            {
                var userId = _currentUserService.GetUserName();
                var asset = await _assetService.UpdateClassificationAsync(
                    id, dto.Criticality, dto.DataClassification, userId);

                // Trigger scope refresh
                var tenantId = _currentUserService.GetTenantId();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _onboardingService.RefreshScopeAsync(tenantId, userId,
                            $"Asset classification changed: {asset.AssetCode}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to refresh scope after classification change");
                    }
                });

                return Ok(asset);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset classification {AssetId}", id);
                return StatusCode(500, new { error = "Failed to update classification" });
            }
        }

        /// <summary>
        /// Get assets by criticality level
        /// </summary>
        [HttpGet("by-criticality/{criticality}")]
        public async Task<ActionResult<List<Asset>>> GetAssetsByCriticality(string criticality)
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetAssetsByCriticalityAsync(tenantId, criticality);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assets by criticality");
                return StatusCode(500, new { error = "Failed to retrieve assets" });
            }
        }

        /// <summary>
        /// Get PCI data assets
        /// </summary>
        [HttpGet("pci")]
        public async Task<ActionResult<List<Asset>>> GetPciAssets()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetPciAssetsAsync(tenantId);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PCI assets");
                return StatusCode(500, new { error = "Failed to retrieve PCI assets" });
            }
        }

        /// <summary>
        /// Get PII data assets
        /// </summary>
        [HttpGet("pii")]
        public async Task<ActionResult<List<Asset>>> GetPiiAssets()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetPiiAssetsAsync(tenantId);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PII assets");
                return StatusCode(500, new { error = "Failed to retrieve PII assets" });
            }
        }

        #endregion

        #region Ownership Operations

        /// <summary>
        /// Assign owner to asset
        /// </summary>
        [HttpPut("{id:guid}/owner")]
        public async Task<ActionResult<Asset>> AssignOwner(Guid id, [FromBody] AssignOwnerDto dto)
        {
            try
            {
                var userId = _currentUserService.GetUserName();
                var asset = await _assetService.AssignOwnerAsync(id, dto.UserId, dto.TeamId, userId);
                return Ok(asset);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning owner to asset {AssetId}", id);
                return StatusCode(500, new { error = "Failed to assign owner" });
            }
        }

        /// <summary>
        /// Get unassigned assets
        /// </summary>
        [HttpGet("unassigned")]
        public async Task<ActionResult<List<Asset>>> GetUnassignedAssets()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetUnassignedAssetsAsync(tenantId);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned assets");
                return StatusCode(500, new { error = "Failed to retrieve unassigned assets" });
            }
        }

        #endregion

        #region Scope Operations

        /// <summary>
        /// Get in-scope assets
        /// </summary>
        [HttpGet("in-scope")]
        public async Task<ActionResult<List<Asset>>> GetInScopeAssets()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var assets = await _assetService.GetInScopeAssetsAsync(tenantId);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-scope assets");
                return StatusCode(500, new { error = "Failed to retrieve in-scope assets" });
            }
        }

        /// <summary>
        /// Set asset scope status
        /// </summary>
        [HttpPut("{id:guid}/scope")]
        public async Task<ActionResult<Asset>> SetInScope(Guid id, [FromBody] SetScopeDto dto)
        {
            try
            {
                var userId = _currentUserService.GetUserName();
                var asset = await _assetService.SetInScopeAsync(id, dto.InScope, userId);
                return Ok(asset);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting asset scope {AssetId}", id);
                return StatusCode(500, new { error = "Failed to update scope" });
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get asset summary/statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<AssetSummaryDto>> GetAssetSummary()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var summary = await _assetService.GetAssetSummaryAsync(tenantId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset summary");
                return StatusCode(500, new { error = "Failed to retrieve asset summary" });
            }
        }

        /// <summary>
        /// Get asset count by type
        /// </summary>
        [HttpGet("counts/by-type")]
        public async Task<ActionResult<Dictionary<string, int>>> GetCountByType()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var counts = await _assetService.GetAssetCountByTypeAsync(tenantId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset counts by type");
                return StatusCode(500, new { error = "Failed to retrieve counts" });
            }
        }

        /// <summary>
        /// Get asset count by criticality
        /// </summary>
        [HttpGet("counts/by-criticality")]
        public async Task<ActionResult<Dictionary<string, int>>> GetCountByCriticality()
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var counts = await _assetService.GetAssetCountByCriticalityAsync(tenantId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset counts by criticality");
                return StatusCode(500, new { error = "Failed to retrieve counts" });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Bulk import assets
        /// </summary>
        [HttpPost("bulk-import")]
        public async Task<ActionResult<object>> BulkImport([FromBody] List<CreateAssetDto> assets)
        {
            try
            {
                var tenantId = _currentUserService.GetTenantId();
                var userId = _currentUserService.GetUserName();

                var count = await _assetService.BulkImportAsync(tenantId, assets, userId);

                // Trigger scope refresh after bulk import
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _onboardingService.RefreshScopeAsync(tenantId, userId,
                            $"Bulk import: {count} assets added");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to refresh scope after bulk import");
                    }
                });

                return Ok(new { imported = count, message = $"Successfully imported {count} assets" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk import");
                return StatusCode(500, new { error = "Failed to import assets" });
            }
        }

        #endregion

        #region Helpers

        private static bool HasComplianceRelevantData(CreateAssetDto dto)
        {
            var dataTypes = dto.DataTypes?.ToUpperInvariant() ?? "";
            var classification = dto.DataClassification?.ToUpperInvariant() ?? "";

            return dataTypes.Contains("PCI") ||
                   dataTypes.Contains("PHI") ||
                   dataTypes.Contains("PII") ||
                   classification == "RESTRICTED" ||
                   classification == "CONFIDENTIAL" ||
                   dto.Criticality == "T1" ||
                   dto.Criticality == "T2";
        }

        #endregion
    }

    #region DTOs

    public class UpdateClassificationDto
    {
        public string Criticality { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
    }

    public class AssignOwnerDto
    {
        public Guid? UserId { get; set; }
        public Guid? TeamId { get; set; }
    }

    public class SetScopeDto
    {
        public bool InScope { get; set; }
    }

    #endregion
}
