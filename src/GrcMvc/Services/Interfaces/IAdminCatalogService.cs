using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// PHASE 10: Admin Catalog Management Service Interface
    /// Provides CRUD operations for global catalogs (Platform Governance)
    /// </summary>
    public interface IAdminCatalogService
    {
        #region Regulators

        Task<RegulatorCatalog> CreateRegulatorAsync(RegulatorCatalog regulator, string createdBy);
        Task<RegulatorCatalog> UpdateRegulatorAsync(Guid id, RegulatorCatalog regulator, string updatedBy);
        Task<bool> DeleteRegulatorAsync(Guid id, string deletedBy);
        Task<bool> ActivateRegulatorAsync(Guid id, string activatedBy);
        Task<bool> DeactivateRegulatorAsync(Guid id, string deactivatedBy);

        #endregion

        #region Frameworks

        Task<FrameworkCatalog> CreateFrameworkAsync(FrameworkCatalog framework, string createdBy);
        Task<FrameworkCatalog> UpdateFrameworkAsync(Guid id, FrameworkCatalog framework, string updatedBy);
        Task<bool> DeleteFrameworkAsync(Guid id, string deletedBy);
        Task<FrameworkCatalog> PublishNewVersionAsync(Guid id, string newVersion, string publishedBy);
        Task<bool> RetireFrameworkAsync(Guid id, string retiredBy);

        #endregion

        #region Controls

        Task<ControlCatalog> CreateControlAsync(ControlCatalog control, string createdBy);
        Task<ControlCatalog> UpdateControlAsync(Guid id, ControlCatalog control, string updatedBy);
        Task<bool> DeleteControlAsync(Guid id, string deletedBy);
        Task<int> BulkImportControlsAsync(List<ControlCatalog> controls, string importedBy);

        #endregion

        #region Roles

        Task<RoleCatalog> CreateRoleAsync(RoleCatalog role, string createdBy);
        Task<RoleCatalog> UpdateRoleAsync(Guid id, RoleCatalog role, string updatedBy);
        Task<bool> DeleteRoleAsync(Guid id, string deletedBy);
        Task<List<RoleCatalog>> GetAllRolesAsync();

        #endregion

        #region Titles

        Task<TitleCatalog> CreateTitleAsync(TitleCatalog title, string createdBy);
        Task<TitleCatalog> UpdateTitleAsync(Guid id, TitleCatalog title, string updatedBy);
        Task<bool> DeleteTitleAsync(Guid id, string deletedBy);
        Task<List<TitleCatalog>> GetTitlesByRoleAsync(Guid roleId);

        #endregion

        #region Baselines

        Task<BaselineCatalog> CreateBaselineAsync(BaselineCatalog baseline, string createdBy);
        Task<BaselineCatalog> UpdateBaselineAsync(Guid id, BaselineCatalog baseline, string updatedBy);
        Task<bool> DeleteBaselineAsync(Guid id, string deletedBy);
        Task<List<BaselineCatalog>> GetAllBaselinesAsync();

        #endregion

        #region Packages

        Task<PackageCatalog> CreatePackageAsync(PackageCatalog package, string createdBy);
        Task<PackageCatalog> UpdatePackageAsync(Guid id, PackageCatalog package, string updatedBy);
        Task<bool> DeletePackageAsync(Guid id, string deletedBy);
        Task<List<PackageCatalog>> GetAllPackagesAsync();

        #endregion

        #region Templates

        Task<TemplateCatalog> CreateTemplateAsync(TemplateCatalog template, string createdBy);
        Task<TemplateCatalog> UpdateTemplateAsync(Guid id, TemplateCatalog template, string updatedBy);
        Task<bool> DeleteTemplateAsync(Guid id, string deletedBy);
        Task<List<TemplateCatalog>> GetAllTemplatesAsync();

        #endregion

        #region Evidence Types

        Task<EvidenceTypeCatalog> CreateEvidenceTypeAsync(EvidenceTypeCatalog evidenceType, string createdBy);
        Task<EvidenceTypeCatalog> UpdateEvidenceTypeAsync(Guid id, EvidenceTypeCatalog evidenceType, string updatedBy);
        Task<bool> DeleteEvidenceTypeAsync(Guid id, string deletedBy);
        Task<List<EvidenceTypeCatalog>> GetAllEvidenceTypesAsync();

        #endregion

        #region Rulesets

        Task<List<Ruleset>> GetAllRulesetsAsync();
        Task<Ruleset> GetRulesetAsync(Guid id);
        Task<Ruleset> CreateRulesetAsync(Ruleset ruleset, string createdBy);
        Task<Ruleset> UpdateRulesetAsync(Guid id, Ruleset ruleset, string updatedBy);
        Task<bool> ActivateRulesetAsync(Guid id, string activatedBy);
        Task<bool> RetireRulesetAsync(Guid id, string retiredBy);
        Task<Ruleset> CloneRulesetAsync(Guid id, string newName, string clonedBy);

        #endregion

        #region Rules

        Task<List<Rule>> GetRulesByRulesetAsync(Guid rulesetId);
        Task<Rule> CreateRuleAsync(Rule rule, string createdBy);
        Task<Rule> UpdateRuleAsync(Guid id, Rule rule, string updatedBy);
        Task<bool> DeleteRuleAsync(Guid id, string deletedBy);
        Task<bool> ReorderRulesAsync(Guid rulesetId, List<Guid> ruleIdsInOrder, string updatedBy);

        #endregion

        #region Audit & Versioning

        Task<List<CatalogChangeLog>> GetChangeLogAsync(string entityType, Guid? entityId = null, int limit = 100);

        #endregion
    }

    /// <summary>
    /// Catalog change log for versioning and audit
    /// </summary>
    public class CatalogChangeLog
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string OldValueJson { get; set; } = string.Empty;
        public string NewValueJson { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
