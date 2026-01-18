using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API controller for GRC lookup/reference data
    /// Used to populate dropdowns and provide AI recommendations
    /// </summary>
    [ApiController]
    [Route("api/lookups")]
    [AllowAnonymous] // Lookups are public for onboarding flows
    public class LookupsController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<LookupsController> _logger;

        public LookupsController(GrcDbContext context, ILogger<LookupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all countries (with GCC prioritized)
        /// </summary>
        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries([FromQuery] string? region = null, [FromQuery] bool gccOnly = false)
        {
            var query = _context.LookupCountries.Where(c => c.IsActive);

            if (gccOnly)
                query = query.Where(c => c.IsGccCountry);
            else if (!string.IsNullOrEmpty(region))
                query = query.Where(c => c.Region == region);

            var countries = await query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameEn)
                .ToListAsync();

            return Ok(countries.Select(c => new
            {
                c.Code,
                c.Iso2Code,
                c.Iso3Code,
                c.NameEn,
                c.NameAr,
                c.PhoneCode,
                c.Currency,
                c.Region,
                c.IsGccCountry,
                c.RequiresDataLocalization
            }));
        }

        /// <summary>
        /// Get all sectors with regulator and framework recommendations
        /// </summary>
        [HttpGet("sectors")]
        public async Task<IActionResult> GetSectors([FromQuery] bool criticalOnly = false)
        {
            var query = _context.LookupSectors.Where(s => s.IsActive);

            if (criticalOnly)
                query = query.Where(s => s.IsCriticalInfrastructure);

            var sectors = await query
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return Ok(sectors.Select(s => new
            {
                s.Code,
                s.NameEn,
                s.NameAr,
                s.DescriptionEn,
                s.DescriptionAr,
                s.GosiCode,
                s.PrimaryRegulatorCode,
                s.IsCriticalInfrastructure,
                s.RequiresStricterCompliance,
                RecommendedFrameworks = ParseJsonArray(s.RecommendedFrameworks)
            }));
        }

        /// <summary>
        /// Get organization types
        /// </summary>
        [HttpGet("organization-types")]
        public async Task<IActionResult> GetOrganizationTypes([FromQuery] string? category = null)
        {
            var query = _context.LookupOrganizationTypes.Where(o => o.IsActive);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(o => o.Category == category);

            var types = await query
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            return Ok(types.Select(o => new
            {
                o.Code,
                o.NameEn,
                o.NameAr,
                o.DescriptionEn,
                o.Category,
                o.IsRegulatedEntity,
                o.RequiresAudit
            }));
        }

        /// <summary>
        /// Get regulators (by country)
        /// </summary>
        [HttpGet("regulators")]
        public async Task<IActionResult> GetRegulators([FromQuery] string? countryCode = null)
        {
            var query = _context.LookupRegulators.Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(countryCode))
                query = query.Where(r => r.CountryCode == countryCode || string.IsNullOrEmpty(r.CountryCode));

            var regulators = await query
                .OrderBy(r => r.SortOrder)
                .ToListAsync();

            return Ok(regulators.Select(r => new
            {
                r.Code,
                r.NameEn,
                r.NameAr,
                r.FullNameEn,
                r.FullNameAr,
                r.CountryCode,
                r.WebsiteUrl,
                RegulatedSectors = ParseJsonArray(r.RegulatedSectors),
                IssuedFrameworks = ParseJsonArray(r.IssuedFrameworks)
            }));
        }

        /// <summary>
        /// Get compliance frameworks (with recommendations based on sector)
        /// </summary>
        [HttpGet("frameworks")]
        public async Task<IActionResult> GetFrameworks(
            [FromQuery] string? sector = null,
            [FromQuery] string? countryCode = null,
            [FromQuery] bool mandatoryOnly = false)
        {
            var query = _context.LookupFrameworks.Where(f => f.IsActive);

            if (mandatoryOnly)
                query = query.Where(f => f.IsMandatory);

            if (!string.IsNullOrEmpty(countryCode))
                query = query.Where(f => f.CountryCode == countryCode || string.IsNullOrEmpty(f.CountryCode));

            var frameworksList = await query
                .OrderBy(f => f.Priority)
                .ThenBy(f => f.SortOrder)
                .ToListAsync();

            var frameworks = frameworksList.Select(f => new
            {
                f.Code,
                f.NameEn,
                f.NameAr,
                f.DescriptionEn,
                f.Type,
                f.IssuingBody,
                f.CountryCode,
                f.Version,
                f.IsMandatory,
                f.Priority,
                f.EstimatedControlCount,
                f.EstimatedImplementationMonths,
                f.ComplexityLevel,
                ApplicableSectors = ParseJsonArray(f.ApplicableSectors)
            }).ToList();

            // Filter by sector if provided
            if (!string.IsNullOrEmpty(sector))
            {
                frameworks = frameworks
                    .Where(f => !f.ApplicableSectors.Any() || f.ApplicableSectors.Contains(sector))
                    .ToList();
            }

            return Ok(frameworks);
        }

        /// <summary>
        /// Get organization sizes
        /// </summary>
        [HttpGet("organization-sizes")]
        public async Task<IActionResult> GetOrganizationSizes()
        {
            var sizes = await _context.LookupOrganizationSizes
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return Ok(sizes.Select(s => new
            {
                s.Code,
                s.NameEn,
                s.NameAr,
                s.DescriptionEn,
                s.MinEmployees,
                s.MaxEmployees,
                s.RecommendedApproach,
                s.RecommendedTeamSize
            }));
        }

        /// <summary>
        /// Get compliance maturity levels (CMMI-based)
        /// </summary>
        [HttpGet("maturity-levels")]
        public async Task<IActionResult> GetMaturityLevels()
        {
            var levels = await _context.LookupMaturityLevels
                .Where(m => m.IsActive)
                .OrderBy(m => m.Level)
                .ToListAsync();

            return Ok(levels.Select(m => new
            {
                m.Code,
                m.NameEn,
                m.NameAr,
                m.DescriptionEn,
                m.DescriptionAr,
                m.Level,
                Characteristics = ParseJsonArray(m.Characteristics),
                RecommendedActions = ParseJsonArray(m.RecommendedActions)
            }));
        }

        /// <summary>
        /// Get data types for data classification
        /// </summary>
        [HttpGet("data-types")]
        public async Task<IActionResult> GetDataTypes([FromQuery] string? category = null)
        {
            var query = _context.LookupDataTypes.Where(d => d.IsActive);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(d => d.Category == category);

            var dataTypes = await query
                .OrderBy(d => d.SortOrder)
                .ToListAsync();

            return Ok(dataTypes.Select(d => new
            {
                d.Code,
                d.NameEn,
                d.NameAr,
                d.DescriptionEn,
                d.Category,
                d.SensitivityLevel,
                d.RequiresEncryption,
                d.RequiresConsent,
                d.DefaultRetentionYears,
                ApplicableRegulations = ParseJsonArray(d.ApplicableRegulations)
            }));
        }

        /// <summary>
        /// Get hosting models
        /// </summary>
        [HttpGet("hosting-models")]
        public async Task<IActionResult> GetHostingModels()
        {
            var models = await _context.LookupHostingModels
                .Where(h => h.IsActive)
                .OrderBy(h => h.SortOrder)
                .ToListAsync();

            return Ok(models.Select(h => new
            {
                h.Code,
                h.NameEn,
                h.NameAr,
                h.DescriptionEn,
                h.DescriptionAr,
                h.RequiresDataLocalization,
                SecurityConsiderations = ParseJsonArray(h.SecurityConsiderations),
                ComplianceImplications = ParseJsonArray(h.ComplianceImplications)
            }));
        }

        /// <summary>
        /// Get cloud providers
        /// </summary>
        [HttpGet("cloud-providers")]
        public async Task<IActionResult> GetCloudProviders([FromQuery] bool ksaRegionOnly = false)
        {
            var query = _context.LookupCloudProviders.Where(c => c.IsActive);

            if (ksaRegionOnly)
                query = query.Where(c => c.HasKsaRegion);

            var providers = await query
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return Ok(providers.Select(c => new
            {
                c.Code,
                c.NameEn,
                c.NameAr,
                c.DescriptionEn,
                c.Type,
                c.HasKsaRegion,
                c.HasGccRegion,
                Certifications = ParseJsonArray(c.Certifications),
                DataResidencyOptions = ParseJsonArray(c.DataResidencyOptions)
            }));
        }

        /// <summary>
        /// Get all lookups in one call (for initial page load)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLookups()
        {
            var countries = await _context.LookupCountries.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();
            var sectors = await _context.LookupSectors.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync();
            var orgTypes = await _context.LookupOrganizationTypes.Where(o => o.IsActive).OrderBy(o => o.SortOrder).ToListAsync();
            var regulators = await _context.LookupRegulators.Where(r => r.IsActive).OrderBy(r => r.SortOrder).ToListAsync();
            var frameworks = await _context.LookupFrameworks.Where(f => f.IsActive).OrderBy(f => f.Priority).ToListAsync();
            var sizes = await _context.LookupOrganizationSizes.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync();
            var maturity = await _context.LookupMaturityLevels.Where(m => m.IsActive).OrderBy(m => m.Level).ToListAsync();
            var dataTypes = await _context.LookupDataTypes.Where(d => d.IsActive).OrderBy(d => d.SortOrder).ToListAsync();
            var hosting = await _context.LookupHostingModels.Where(h => h.IsActive).OrderBy(h => h.SortOrder).ToListAsync();
            var cloud = await _context.LookupCloudProviders.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();

            var result = new
            {
                countries = countries.Select(c => new { c.Code, c.NameEn, c.NameAr, c.Iso2Code, c.IsGccCountry, c.RequiresDataLocalization }),
                sectors = sectors.Select(s => new { s.Code, s.NameEn, s.NameAr, s.PrimaryRegulatorCode, s.IsCriticalInfrastructure, RecommendedFrameworks = ParseJsonArray(s.RecommendedFrameworks) }),
                organizationTypes = orgTypes.Select(o => new { o.Code, o.NameEn, o.NameAr, o.Category, o.IsRegulatedEntity }),
                regulators = regulators.Select(r => new { r.Code, r.NameEn, r.NameAr, r.FullNameEn, r.CountryCode }),
                frameworks = frameworks.Select(f => new { f.Code, f.NameEn, f.NameAr, f.Type, f.IsMandatory, f.Priority, ApplicableSectors = ParseJsonArray(f.ApplicableSectors) }),
                organizationSizes = sizes.Select(s => new { s.Code, s.NameEn, s.NameAr, s.MinEmployees, s.MaxEmployees, s.RecommendedApproach }),
                maturityLevels = maturity.Select(m => new { m.Code, m.NameEn, m.NameAr, m.Level, m.DescriptionEn }),
                dataTypes = dataTypes.Select(d => new { d.Code, d.NameEn, d.NameAr, d.Category, d.SensitivityLevel }),
                hostingModels = hosting.Select(h => new { h.Code, h.NameEn, h.NameAr, h.DescriptionEn }),
                cloudProviders = cloud.Select(c => new { c.Code, c.NameEn, c.NameAr, c.HasKsaRegion })
            };

            return Ok(result);
        }

        /// <summary>
        /// Get AI-powered recommendations for wizard based on organization profile
        /// </summary>
        [HttpPost("recommend")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetRecommendations([FromBody] RecommendationRequest request)
        {
            var recommendations = new List<object>();

            // Get sector info
            var sector = await _context.LookupSectors
                .FirstOrDefaultAsync(s => s.Code == request.SectorCode && s.IsActive);

            if (sector != null)
            {
                // Recommend frameworks based on sector
                if (!string.IsNullOrEmpty(sector.RecommendedFrameworks))
                {
                    var frameworkCodes = ParseJsonArray(sector.RecommendedFrameworks);
                    var frameworksList = await _context.LookupFrameworks
                        .Where(f => frameworkCodes.Contains(f.Code) && f.IsActive)
                        .OrderBy(f => f.Priority)
                        .ToListAsync();

                    recommendations.Add(new
                    {
                        type = "frameworks",
                        title = "Recommended Compliance Frameworks",
                        titleAr = "أطر الامتثال الموصى بها",
                        reason = $"Based on your sector ({sector.NameEn}), regulated by {sector.PrimaryRegulatorCode}",
                        reasonAr = $"بناءً على قطاعك ({sector.NameAr})، المنظم من قبل {sector.PrimaryRegulatorCode}",
                        items = frameworksList.Select(f => new
                        {
                            f.Code,
                            f.NameEn,
                            f.NameAr,
                            f.IsMandatory,
                            f.Priority,
                            f.EstimatedImplementationMonths
                        })
                    });
                }

                // Critical infrastructure warning
                if (sector.IsCriticalInfrastructure)
                {
                    recommendations.Add(new
                    {
                        type = "warning",
                        title = "Critical Infrastructure Classification",
                        titleAr = "تصنيف البنية التحتية الحرجة",
                        message = "Your sector is classified as critical infrastructure. Additional NCA controls (CSCC, OT) may apply.",
                        messageAr = "قطاعك مصنف كبنية تحتية حرجة. قد تنطبق ضوابط NCA إضافية (CSCC، OT).",
                        severity = "high"
                    });
                }
            }

            // Size-based recommendations
            if (!string.IsNullOrEmpty(request.OrganizationSizeCode))
            {
                var size = await _context.LookupOrganizationSizes
                    .FirstOrDefaultAsync(s => s.Code == request.OrganizationSizeCode);

                if (size != null)
                {
                    recommendations.Add(new
                    {
                        type = "approach",
                        title = "Recommended Implementation Approach",
                        titleAr = "نهج التنفيذ الموصى به",
                        approach = size.RecommendedApproach,
                        recommendedTeamSize = size.RecommendedTeamSize,
                        message = $"Based on your organization size ({size.NameEn}), we recommend a {size.RecommendedApproach} compliance approach with a team of {size.RecommendedTeamSize}+ members.",
                        messageAr = $"بناءً على حجم منظمتك ({size.NameAr})، نوصي بنهج امتثال {size.RecommendedApproach} مع فريق من {size.RecommendedTeamSize}+ أعضاء."
                    });
                }
            }

            // Data localization warnings
            if (!string.IsNullOrEmpty(request.CountryCode))
            {
                var country = await _context.LookupCountries
                    .FirstOrDefaultAsync(c => c.Iso2Code == request.CountryCode || c.Code == request.CountryCode);

                if (country?.RequiresDataLocalization == true)
                {
                    recommendations.Add(new
                    {
                        type = "info",
                        title = "Data Localization Requirement",
                        titleAr = "متطلبات توطين البيانات",
                        message = $"{country.NameEn} requires certain data to be stored within the country. Consider cloud providers with local regions.",
                        messageAr = $"{country.NameAr} تتطلب تخزين بعض البيانات داخل البلد. فكر في مزودي السحابة الذين لديهم مناطق محلية.",
                        severity = "medium"
                    });
                }
            }

            return Ok(new { recommendations });
        }

        /// <summary>
        /// Helper to parse JSON array string to List
        /// </summary>
        private static List<string> ParseJsonArray(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }

    public class RecommendationRequest
    {
        public string? SectorCode { get; set; }
        public string? CountryCode { get; set; }
        public string? OrganizationSizeCode { get; set; }
        public string? OrganizationTypeCode { get; set; }
        public List<string>? DataTypes { get; set; }
        public string? HostingModel { get; set; }
    }
}
