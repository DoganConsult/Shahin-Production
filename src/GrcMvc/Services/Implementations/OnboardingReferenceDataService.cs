using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    public class OnboardingReferenceDataService : IOnboardingReferenceDataService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<OnboardingReferenceDataService> _logger;

        public OnboardingReferenceDataService(
            GrcDbContext context,
            ILogger<OnboardingReferenceDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ReferenceDataOptionDto>> GetOptionsAsync(string category, string? language = "en")
        {
            var query = _context.ReferenceData
                .Where(r => r.Category == category && r.IsActive)
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.LabelEn);

            var options = await query.ToListAsync();

            return options.Select(r => new ReferenceDataOptionDto
            {
                Value = r.Value,
                Label = language == "ar" && !string.IsNullOrEmpty(r.LabelAr) ? r.LabelAr : r.LabelEn,
                Description = language == "ar" && !string.IsNullOrEmpty(r.DescriptionAr) 
                    ? r.DescriptionAr 
                    : r.DescriptionEn,
                SortOrder = r.SortOrder,
                IsCommon = r.IsCommon,
                Metadata = string.IsNullOrEmpty(r.MetadataJson) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(r.MetadataJson)
            }).ToList();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.ReferenceData
                .Where(r => r.IsActive)
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<ReferenceDataOptionDto>>> GetOptionsForCategoriesAsync(
            List<string> categories, 
            string? language = "en")
        {
            var result = new Dictionary<string, List<ReferenceDataOptionDto>>();

            foreach (var category in categories)
            {
                result[category] = await GetOptionsAsync(category, language);
            }

            return result;
        }

        public async Task<List<ReferenceDataOptionDto>> GetCommonOptionsAsync(string category, string? language = "en")
        {
            var query = _context.ReferenceData
                .Where(r => r.Category == category && r.IsActive && r.IsCommon)
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.LabelEn);

            var options = await query.ToListAsync();

            return options.Select(r => new ReferenceDataOptionDto
            {
                Value = r.Value,
                Label = language == "ar" && !string.IsNullOrEmpty(r.LabelAr) ? r.LabelAr : r.LabelEn,
                Description = language == "ar" && !string.IsNullOrEmpty(r.DescriptionAr) 
                    ? r.DescriptionAr 
                    : r.DescriptionEn,
                SortOrder = r.SortOrder,
                IsCommon = r.IsCommon,
                Metadata = string.IsNullOrEmpty(r.MetadataJson) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(r.MetadataJson)
            }).ToList();
        }

        public async Task<List<ReferenceDataOptionDto>> GetFilteredOptionsAsync(
            string category,
            Dictionary<string, object>? filterContext = null,
            string? language = "en")
        {
            var query = _context.ReferenceData
                .Where(r => r.Category == category && r.IsActive);

            // Apply filters from context
            if (filterContext != null)
            {
                // Filter by audit scope type
                if (filterContext.ContainsKey("AuditScopeType") && 
                    filterContext["AuditScopeType"] is string auditScopeType)
                {
                    query = query.Where(r => 
                        r.MetadataJson != null && 
                        (r.MetadataJson.Contains($"\"auditScopeType\":\"{auditScopeType}\"") ||
                         r.MetadataJson.Contains($"\"auditScopeType\":[\"{auditScopeType}\"")));
                }

                // Filter by framework codes
                if (filterContext.ContainsKey("FrameworkCodes") && 
                    filterContext["FrameworkCodes"] is List<string> frameworkCodes && 
                    frameworkCodes.Any())
                {
                    var frameworkFilter = string.Join("|", frameworkCodes);
                    query = query.Where(r => 
                        r.MetadataJson != null && 
                        frameworkCodes.Any(fc => r.MetadataJson.Contains($"\"frameworkCode\":\"{fc}\"") ||
                                                 r.MetadataJson.Contains($"\"frameworkCode\":[\"{fc}\"")));
                }

                // Filter by regulator codes
                if (filterContext.ContainsKey("RegulatorCodes") && 
                    filterContext["RegulatorCodes"] is List<string> regulatorCodes && 
                    regulatorCodes.Any())
                {
                    query = query.Where(r => 
                        r.MetadataJson != null && 
                        regulatorCodes.Any(rc => r.MetadataJson.Contains($"\"regulatorCode\":\"{rc}\"") ||
                                                r.MetadataJson.Contains($"\"regulatorCode\":[\"{rc}\"")));
                }

                // Filter by industry context
                if (filterContext.ContainsKey("IndustrySector") && 
                    filterContext["IndustrySector"] is string industrySector)
                {
                    query = query.Where(r => 
                        r.IndustryContext == null || 
                        r.IndustryContext == industrySector ||
                        r.IndustryContext.Contains(industrySector));
                }

                // Filter by organization type context
                if (filterContext.ContainsKey("OrganizationType") && 
                    filterContext["OrganizationType"] is string organizationType)
                {
                    query = query.Where(r => 
                        r.OrganizationTypeContext == null || 
                        r.OrganizationTypeContext == organizationType ||
                        r.OrganizationTypeContext.Contains(organizationType));
                }
            }

            query = query.OrderBy(r => r.SortOrder).ThenBy(r => r.LabelEn);

            var options = await query.ToListAsync();

            return options.Select(r => new ReferenceDataOptionDto
            {
                Value = r.Value,
                Label = language == "ar" && !string.IsNullOrEmpty(r.LabelAr) ? r.LabelAr : r.LabelEn,
                Description = language == "ar" && !string.IsNullOrEmpty(r.DescriptionAr) 
                    ? r.DescriptionAr 
                    : r.DescriptionEn,
                SortOrder = r.SortOrder,
                IsCommon = r.IsCommon,
                Metadata = string.IsNullOrEmpty(r.MetadataJson) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(r.MetadataJson)
            }).ToList();
        }

        public async Task<List<ReferenceDataOptionDto>> GetOptionsByAuditScopeAsync(
            string category,
            string auditScopeType,
            string? language = "en")
        {
            var filterContext = new Dictionary<string, object>
            {
                { "AuditScopeType", auditScopeType }
            };
            return await GetFilteredOptionsAsync(category, filterContext, language);
        }

        public async Task<List<ReferenceDataOptionDto>> GetOptionsByFrameworksAsync(
            string category,
            List<string> frameworkCodes,
            string? language = "en")
        {
            var filterContext = new Dictionary<string, object>
            {
                { "FrameworkCodes", frameworkCodes }
            };
            return await GetFilteredOptionsAsync(category, filterContext, language);
        }
    }
}
