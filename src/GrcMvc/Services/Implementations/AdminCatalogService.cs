using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 10: Admin Catalog Management Service Implementation
    /// Provides CRUD operations for global catalogs (Platform Governance)
    /// </summary>
    public class AdminCatalogService : IAdminCatalogService
    {
        private readonly GrcDbContext _context;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<AdminCatalogService> _logger;

        public AdminCatalogService(
            GrcDbContext context,
            IAuditEventService auditService,
            ILogger<AdminCatalogService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        #region Regulators

        public async Task<RegulatorCatalog> CreateRegulatorAsync(RegulatorCatalog regulator, string createdBy)
        {
            regulator.Id = Guid.NewGuid();
            regulator.CreatedDate = DateTime.UtcNow;
            regulator.CreatedBy = createdBy;
            regulator.IsActive = true;

            _context.RegulatorCatalogs.Add(regulator);
            await _context.SaveChangesAsync();

            await LogChangeAsync("RegulatorCatalog", regulator.Id, "Create", null, regulator, createdBy);
            _logger.LogInformation($"Regulator created: {regulator.Code}");
            return regulator;
        }

        public async Task<RegulatorCatalog> UpdateRegulatorAsync(Guid id, RegulatorCatalog regulator, string updatedBy)
        {
            var existing = await _context.RegulatorCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Regulator not found.");

            var oldValue = JsonSerializer.Serialize(existing);

            existing.Code = regulator.Code;
            existing.NameAr = regulator.NameAr;
            existing.NameEn = regulator.NameEn;
            existing.JurisdictionEn = regulator.JurisdictionEn;
            existing.Website = regulator.Website;
            existing.Category = regulator.Category;
            existing.Sector = regulator.Sector;
            existing.Established = regulator.Established;
            existing.RegionType = regulator.RegionType;
            existing.DisplayOrder = regulator.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();

            await LogChangeAsync("RegulatorCatalog", id, "Update", oldValue, existing, updatedBy);
            return existing;
        }

        public async Task<bool> DeleteRegulatorAsync(Guid id, string deletedBy)
        {
            var existing = await _context.RegulatorCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            await LogChangeAsync("RegulatorCatalog", id, "Delete", existing, null, deletedBy);
            return true;
        }

        public async Task<bool> ActivateRegulatorAsync(Guid id, string activatedBy)
        {
            var existing = await _context.RegulatorCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsActive = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = activatedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateRegulatorAsync(Guid id, string deactivatedBy)
        {
            var existing = await _context.RegulatorCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deactivatedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Frameworks

        public async Task<FrameworkCatalog> CreateFrameworkAsync(FrameworkCatalog framework, string createdBy)
        {
            framework.Id = Guid.NewGuid();
            framework.CreatedDate = DateTime.UtcNow;
            framework.CreatedBy = createdBy;
            framework.IsActive = true;
            framework.Status = "Active";

            _context.FrameworkCatalogs.Add(framework);
            await _context.SaveChangesAsync();

            await LogChangeAsync("FrameworkCatalog", framework.Id, "Create", null, framework, createdBy);
            _logger.LogInformation($"Framework created: {framework.Code} v{framework.Version}");
            return framework;
        }

        public async Task<FrameworkCatalog> UpdateFrameworkAsync(Guid id, FrameworkCatalog framework, string updatedBy)
        {
            var existing = await _context.FrameworkCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Framework not found.");

            var oldValue = JsonSerializer.Serialize(existing);

            existing.Code = framework.Code;
            existing.Version = framework.Version;
            existing.TitleEn = framework.TitleEn;
            existing.TitleAr = framework.TitleAr;
            existing.DescriptionEn = framework.DescriptionEn;
            existing.DescriptionAr = framework.DescriptionAr;
            existing.RegulatorId = framework.RegulatorId;
            existing.Category = framework.Category;
            existing.IsMandatory = framework.IsMandatory;
            existing.ControlCount = framework.ControlCount;
            existing.Domains = framework.Domains;
            existing.EffectiveDate = framework.EffectiveDate;
            existing.DisplayOrder = framework.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();

            await LogChangeAsync("FrameworkCatalog", id, "Update", oldValue, existing, updatedBy);
            return existing;
        }

        public async Task<bool> DeleteFrameworkAsync(Guid id, string deletedBy)
        {
            var existing = await _context.FrameworkCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            await LogChangeAsync("FrameworkCatalog", id, "Delete", existing, null, deletedBy);
            return true;
        }

        public async Task<FrameworkCatalog> PublishNewVersionAsync(Guid id, string newVersion, string publishedBy)
        {
            var existing = await _context.FrameworkCatalogs
                .Include(f => f.Controls)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existing == null)
                throw new InvalidOperationException("Framework not found.");

            // Retire old version
            existing.Status = "Retired";
            existing.RetiredDate = DateTime.UtcNow;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = publishedBy;

            // Create new version
            var newFramework = new FrameworkCatalog
            {
                Id = Guid.NewGuid(),
                Code = existing.Code,
                Version = newVersion,
                TitleEn = existing.TitleEn,
                TitleAr = existing.TitleAr,
                DescriptionEn = existing.DescriptionEn,
                DescriptionAr = existing.DescriptionAr,
                RegulatorId = existing.RegulatorId,
                Category = existing.Category,
                IsMandatory = existing.IsMandatory,
                ControlCount = existing.ControlCount,
                Domains = existing.Domains,
                EffectiveDate = DateTime.UtcNow,
                Status = "Active",
                IsActive = true,
                DisplayOrder = existing.DisplayOrder,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = publishedBy
            };

            _context.FrameworkCatalogs.Add(newFramework);
            await _context.SaveChangesAsync();

            await LogChangeAsync("FrameworkCatalog", newFramework.Id, "PublishNewVersion", existing, newFramework, publishedBy);
            _logger.LogInformation($"Framework new version published: {newFramework.Code} v{newVersion}");
            return newFramework;
        }

        public async Task<bool> RetireFrameworkAsync(Guid id, string retiredBy)
        {
            var existing = await _context.FrameworkCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.Status = "Retired";
            existing.RetiredDate = DateTime.UtcNow;
            existing.IsActive = false;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = retiredBy;

            await _context.SaveChangesAsync();
            await LogChangeAsync("FrameworkCatalog", id, "Retire", null, existing, retiredBy);
            return true;
        }

        #endregion

        #region Controls

        public async Task<ControlCatalog> CreateControlAsync(ControlCatalog control, string createdBy)
        {
            control.Id = Guid.NewGuid();
            control.CreatedDate = DateTime.UtcNow;
            control.CreatedBy = createdBy;
            control.IsActive = true;

            _context.ControlCatalogs.Add(control);
            await _context.SaveChangesAsync();

            await LogChangeAsync("ControlCatalog", control.Id, "Create", null, control, createdBy);
            return control;
        }

        public async Task<ControlCatalog> UpdateControlAsync(Guid id, ControlCatalog control, string updatedBy)
        {
            var existing = await _context.ControlCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Control not found.");

            var oldValue = JsonSerializer.Serialize(existing);

            existing.ControlId = control.ControlId;
            existing.ControlNumber = control.ControlNumber;
            existing.Domain = control.Domain;
            existing.Subdomain = control.Subdomain;
            existing.TitleAr = control.TitleAr;
            existing.TitleEn = control.TitleEn;
            existing.RequirementAr = control.RequirementAr;
            existing.RequirementEn = control.RequirementEn;
            existing.ControlType = control.ControlType;
            existing.MaturityLevel = control.MaturityLevel;
            existing.ImplementationGuidanceEn = control.ImplementationGuidanceEn;
            existing.EvidenceRequirements = control.EvidenceRequirements;
            existing.MappingIso27001 = control.MappingIso27001;
            existing.MappingNistCsf = control.MappingNistCsf;
            existing.MappingOther = control.MappingOther;
            existing.DisplayOrder = control.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();

            await LogChangeAsync("ControlCatalog", id, "Update", oldValue, existing, updatedBy);
            return existing;
        }

        public async Task<bool> DeleteControlAsync(Guid id, string deletedBy)
        {
            var existing = await _context.ControlCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> BulkImportControlsAsync(List<ControlCatalog> controls, string importedBy)
        {
            var count = 0;
            foreach (var control in controls)
            {
                control.Id = Guid.NewGuid();
                control.CreatedDate = DateTime.UtcNow;
                control.CreatedBy = importedBy;
                control.IsActive = true;
                _context.ControlCatalogs.Add(control);
                count++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Bulk imported {count} controls");
            return count;
        }

        #endregion

        #region Roles

        public async Task<RoleCatalog> CreateRoleAsync(RoleCatalog role, string createdBy)
        {
            role.Id = Guid.NewGuid();
            role.CreatedDate = DateTime.UtcNow;
            role.CreatedBy = createdBy;
            role.IsActive = true;

            _context.RoleCatalogs.Add(role);
            await _context.SaveChangesAsync();

            await LogChangeAsync("RoleCatalog", role.Id, "Create", null, role, createdBy);
            return role;
        }

        public async Task<RoleCatalog> UpdateRoleAsync(Guid id, RoleCatalog role, string updatedBy)
        {
            var existing = await _context.RoleCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Role not found.");

            existing.RoleCode = role.RoleCode;
            existing.RoleName = role.RoleName;
            existing.Description = role.Description;
            existing.Layer = role.Layer;
            existing.Department = role.Department;
            existing.ApprovalLevel = role.ApprovalLevel;
            existing.CanApprove = role.CanApprove;
            existing.CanReject = role.CanReject;
            existing.CanEscalate = role.CanEscalate;
            existing.CanReassign = role.CanReassign;
            existing.DisplayOrder = role.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(Guid id, string deletedBy)
        {
            var existing = await _context.RoleCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoleCatalog>> GetAllRolesAsync()
        {
            return await _context.RoleCatalogs
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Titles

        public async Task<TitleCatalog> CreateTitleAsync(TitleCatalog title, string createdBy)
        {
            title.Id = Guid.NewGuid();
            title.CreatedDate = DateTime.UtcNow;
            title.CreatedBy = createdBy;
            title.IsActive = true;

            _context.TitleCatalogs.Add(title);
            await _context.SaveChangesAsync();
            return title;
        }

        public async Task<TitleCatalog> UpdateTitleAsync(Guid id, TitleCatalog title, string updatedBy)
        {
            var existing = await _context.TitleCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Title not found.");

            existing.TitleCode = title.TitleCode;
            existing.TitleName = title.TitleName;
            existing.Description = title.Description;
            existing.RoleCatalogId = title.RoleCatalogId;
            existing.DisplayOrder = title.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTitleAsync(Guid id, string deletedBy)
        {
            var existing = await _context.TitleCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TitleCatalog>> GetTitlesByRoleAsync(Guid roleId)
        {
            return await _context.TitleCatalogs
                .Where(t => t.RoleCatalogId == roleId && !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Baselines

        public async Task<BaselineCatalog> CreateBaselineAsync(BaselineCatalog baseline, string createdBy)
        {
            baseline.Id = Guid.NewGuid();
            baseline.CreatedDate = DateTime.UtcNow;
            baseline.CreatedBy = createdBy;
            baseline.IsActive = true;

            _context.BaselineCatalogs.Add(baseline);
            await _context.SaveChangesAsync();
            return baseline;
        }

        public async Task<BaselineCatalog> UpdateBaselineAsync(Guid id, BaselineCatalog baseline, string updatedBy)
        {
            var existing = await _context.BaselineCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Baseline not found.");

            existing.BaselineCode = baseline.BaselineCode;
            existing.BaselineName = baseline.BaselineName;
            existing.Description = baseline.Description;
            existing.RegulatorCode = baseline.RegulatorCode;
            existing.Version = baseline.Version;
            existing.EffectiveDate = baseline.EffectiveDate;
            existing.ControlCount = baseline.ControlCount;
            existing.Status = baseline.Status;
            existing.DisplayOrder = baseline.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteBaselineAsync(Guid id, string deletedBy)
        {
            var existing = await _context.BaselineCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BaselineCatalog>> GetAllBaselinesAsync()
        {
            return await _context.BaselineCatalogs
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Packages

        public async Task<PackageCatalog> CreatePackageAsync(PackageCatalog package, string createdBy)
        {
            package.Id = Guid.NewGuid();
            package.CreatedDate = DateTime.UtcNow;
            package.CreatedBy = createdBy;
            package.IsActive = true;

            _context.PackageCatalogs.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<PackageCatalog> UpdatePackageAsync(Guid id, PackageCatalog package, string updatedBy)
        {
            var existing = await _context.PackageCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Package not found.");

            existing.PackageCode = package.PackageCode;
            existing.PackageName = package.PackageName;
            existing.Description = package.Description;
            existing.Category = package.Category;
            existing.BaselineCatalogId = package.BaselineCatalogId;
            existing.RequirementCount = package.RequirementCount;
            existing.EstimatedDays = package.EstimatedDays;
            existing.DisplayOrder = package.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePackageAsync(Guid id, string deletedBy)
        {
            var existing = await _context.PackageCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PackageCatalog>> GetAllPackagesAsync()
        {
            return await _context.PackageCatalogs
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Templates

        public async Task<TemplateCatalog> CreateTemplateAsync(TemplateCatalog template, string createdBy)
        {
            template.Id = Guid.NewGuid();
            template.CreatedDate = DateTime.UtcNow;
            template.CreatedBy = createdBy;
            template.IsActive = true;

            _context.TemplateCatalogs.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<TemplateCatalog> UpdateTemplateAsync(Guid id, TemplateCatalog template, string updatedBy)
        {
            var existing = await _context.TemplateCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Template not found.");

            existing.TemplateCode = template.TemplateCode;
            existing.TemplateName = template.TemplateName;
            existing.Description = template.Description;
            existing.TemplateType = template.TemplateType;
            existing.PackageCatalogId = template.PackageCatalogId;
            existing.RequirementCount = template.RequirementCount;
            existing.EstimatedDays = template.EstimatedDays;
            existing.RequirementsJson = template.RequirementsJson;
            existing.DisplayOrder = template.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTemplateAsync(Guid id, string deletedBy)
        {
            var existing = await _context.TemplateCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TemplateCatalog>> GetAllTemplatesAsync()
        {
            return await _context.TemplateCatalogs
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Evidence Types

        public async Task<EvidenceTypeCatalog> CreateEvidenceTypeAsync(EvidenceTypeCatalog evidenceType, string createdBy)
        {
            evidenceType.Id = Guid.NewGuid();
            evidenceType.CreatedDate = DateTime.UtcNow;
            evidenceType.CreatedBy = createdBy;
            evidenceType.IsActive = true;

            _context.EvidenceTypeCatalogs.Add(evidenceType);
            await _context.SaveChangesAsync();
            return evidenceType;
        }

        public async Task<EvidenceTypeCatalog> UpdateEvidenceTypeAsync(Guid id, EvidenceTypeCatalog evidenceType, string updatedBy)
        {
            var existing = await _context.EvidenceTypeCatalogs.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Evidence type not found.");

            existing.EvidenceTypeCode = evidenceType.EvidenceTypeCode;
            existing.EvidenceTypeName = evidenceType.EvidenceTypeName;
            existing.Description = evidenceType.Description;
            existing.Category = evidenceType.Category;
            existing.AllowedFileTypes = evidenceType.AllowedFileTypes;
            existing.MaxFileSizeMB = evidenceType.MaxFileSizeMB;
            existing.RequiresApproval = evidenceType.RequiresApproval;
            existing.MinScore = evidenceType.MinScore;
            existing.DisplayOrder = evidenceType.DisplayOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteEvidenceTypeAsync(Guid id, string deletedBy)
        {
            var existing = await _context.EvidenceTypeCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EvidenceTypeCatalog>> GetAllEvidenceTypesAsync()
        {
            return await _context.EvidenceTypeCatalogs
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Rulesets

        public async Task<List<Ruleset>> GetAllRulesetsAsync()
        {
            return await _context.Rulesets
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<Ruleset> GetRulesetAsync(Guid id)
        {
            var ruleset = await _context.Rulesets
                .Include(r => r.Rules)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (ruleset == null)
                throw new InvalidOperationException("Ruleset not found.");

            return ruleset;
        }

        public async Task<Ruleset> CreateRulesetAsync(Ruleset ruleset, string createdBy)
        {
            ruleset.Id = Guid.NewGuid();
            ruleset.Status = "Draft";
            ruleset.CreatedDate = DateTime.UtcNow;
            ruleset.CreatedBy = createdBy;

            _context.Rulesets.Add(ruleset);
            await _context.SaveChangesAsync();

            await LogChangeAsync("Ruleset", ruleset.Id, "Create", null, ruleset, createdBy);
            return ruleset;
        }

        public async Task<Ruleset> UpdateRulesetAsync(Guid id, Ruleset ruleset, string updatedBy)
        {
            var existing = await _context.Rulesets.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Ruleset not found.");

            existing.RulesetCode = ruleset.RulesetCode;
            existing.Name = ruleset.Name;
            existing.Description = ruleset.Description;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> ActivateRulesetAsync(Guid id, string activatedBy)
        {
            // Deactivate all other rulesets first
            var activeRulesets = await _context.Rulesets
                .Where(r => r.Status == "Active" && !r.IsDeleted)
                .ToListAsync();

            foreach (var rs in activeRulesets)
            {
                rs.Status = "Retired";
                rs.ModifiedDate = DateTime.UtcNow;
                rs.ModifiedBy = activatedBy;
            }

            var existing = await _context.Rulesets.FindAsync(id);
            if (existing == null) return false;

            existing.Status = "Active";
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = activatedBy;

            await _context.SaveChangesAsync();
            await LogChangeAsync("Ruleset", id, "Activate", null, existing, activatedBy);
            return true;
        }

        public async Task<bool> RetireRulesetAsync(Guid id, string retiredBy)
        {
            var existing = await _context.Rulesets.FindAsync(id);
            if (existing == null) return false;

            existing.Status = "Retired";
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = retiredBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Ruleset> CloneRulesetAsync(Guid id, string newName, string clonedBy)
        {
            var existing = await _context.Rulesets
                .Include(r => r.Rules)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existing == null)
                throw new InvalidOperationException("Ruleset not found.");

            var newRuleset = new Ruleset
            {
                Id = Guid.NewGuid(),
                RulesetCode = $"{existing.RulesetCode}_CLONE_{DateTime.UtcNow:yyyyMMdd}",
                Name = newName,
                Description = $"Cloned from {existing.Name}",
                Status = "Draft",
                TenantId = existing.TenantId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = clonedBy
            };

            _context.Rulesets.Add(newRuleset);

            // Clone rules
            foreach (var rule in existing.Rules.Where(r => !r.IsDeleted))
            {
                var newRule = new Rule
                {
                    Id = Guid.NewGuid(),
                    RulesetId = newRuleset.Id,
                    RuleCode = rule.RuleCode,
                    Name = rule.Name,
                    Description = rule.Description,
                    Priority = rule.Priority,
                    ConditionJson = rule.ConditionJson,
                    ActionsJson = rule.ActionsJson,
                    BusinessReason = rule.BusinessReason,
                    Status = "ACTIVE",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = clonedBy
                };
                _context.Rules.Add(newRule);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Ruleset cloned: {existing.Name} -> {newName}");
            return newRuleset;
        }

        #endregion

        #region Rules

        public async Task<List<Rule>> GetRulesByRulesetAsync(Guid rulesetId)
        {
            return await _context.Rules
                .Where(r => r.RulesetId == rulesetId && !r.IsDeleted)
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        public async Task<Rule> CreateRuleAsync(Rule rule, string createdBy)
        {
            rule.Id = Guid.NewGuid();
            rule.Status = "ACTIVE";
            rule.CreatedDate = DateTime.UtcNow;
            rule.CreatedBy = createdBy;

            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<Rule> UpdateRuleAsync(Guid id, Rule rule, string updatedBy)
        {
            var existing = await _context.Rules.FindAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Rule not found.");

            existing.RuleCode = rule.RuleCode;
            existing.Name = rule.Name;
            existing.Description = rule.Description;
            existing.Priority = rule.Priority;
            existing.ConditionJson = rule.ConditionJson;
            existing.ActionsJson = rule.ActionsJson;
            existing.BusinessReason = rule.BusinessReason;
            existing.Status = rule.Status;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = updatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRuleAsync(Guid id, string deletedBy)
        {
            var existing = await _context.Rules.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderRulesAsync(Guid rulesetId, List<Guid> ruleIdsInOrder, string updatedBy)
        {
            var rules = await _context.Rules
                .Where(r => r.RulesetId == rulesetId && !r.IsDeleted)
                .ToListAsync();

            for (int i = 0; i < ruleIdsInOrder.Count; i++)
            {
                var rule = rules.FirstOrDefault(r => r.Id == ruleIdsInOrder[i]);
                if (rule != null)
                {
                    rule.Priority = i + 1;
                    rule.ModifiedDate = DateTime.UtcNow;
                    rule.ModifiedBy = updatedBy;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Audit & Versioning

        public async Task<List<CatalogChangeLog>> GetChangeLogAsync(string entityType, Guid? entityId = null, int limit = 100)
        {
            var query = _context.AuditEvents
                .Where(e => e.AffectedEntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(e => e.AffectedEntityId == entityId.Value.ToString());

            var events = await query
                .OrderByDescending(e => e.EventTimestamp)
                .Take(limit)
                .ToListAsync();

            return events.Select(e => new CatalogChangeLog
            {
                Id = e.Id,
                EntityType = e.AffectedEntityType ?? "",
                EntityId = Guid.TryParse(e.AffectedEntityId, out var id) ? id : Guid.Empty,
                Action = e.Action ?? "",
                OldValueJson = "", // Would need to parse from PayloadJson
                NewValueJson = e.PayloadJson ?? "",
                ChangedBy = e.Actor ?? "",
                ChangedAt = e.EventTimestamp
            }).ToList();
        }

        private async Task LogChangeAsync(string entityType, Guid entityId, string action, object? oldValue, object? newValue, string changedBy)
        {
            await _auditService.LogEventAsync(
                tenantId: Guid.Empty,
                eventType: $"Catalog{action}",
                affectedEntityType: entityType,
                affectedEntityId: entityId.ToString(),
                action: action,
                actor: changedBy,
                payloadJson: JsonSerializer.Serialize(new
                {
                    OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                    NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null
                }),
                correlationId: null
            );
        }

        #endregion
    }
}
