using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds catalog data from CSV files:
/// - RegulatorCatalog (92 regulators)
/// - FrameworkCatalog (163 frameworks)
/// - ControlCatalog (13,528 controls)
/// Layer 1: Global (Platform) - Immutable regulatory content
/// </summary>
public class CatalogCsvSeeder
{
    private readonly GrcDbContext _context;
    private readonly ILogger _logger;
    private readonly string _seedDataPath;

    public CatalogCsvSeeder(GrcDbContext context, ILogger logger, string seedDataPath)
    {
        _context = context;
        _logger = logger;
        _seedDataPath = seedDataPath;
    }

    public async Task SeedAllCatalogsAsync()
    {
        _logger.LogInformation("Starting catalog CSV seeding...");

        await SeedRegulatorsFromCsvAsync();
        await SeedFrameworksFromCsvAsync();
        await SeedControlsFromCsvAsync();

        _logger.LogInformation("✅ Catalog CSV seeding completed.");
    }

    public async Task SeedRegulatorsFromCsvAsync()
    {
        if (await _context.RegulatorCatalogs.AnyAsync())
        {
            _logger.LogInformation("✅ RegulatorCatalogs already seeded. Skipping.");
            return;
        }

        var csvPath = Path.Combine(_seedDataPath, "regulators_catalog_seed.csv");
        if (!File.Exists(csvPath))
        {
            _logger.LogWarning($"⚠️ CSV file not found: {csvPath}");
            return;
        }

        var regulators = new List<RegulatorCatalog>();
        var lines = await File.ReadAllLinesAsync(csvPath);
        var displayOrder = 1;

        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length < 8) continue;

            // CSV: code,name_ar,name_en,jurisdiction_en,website,category,sector,established
            regulators.Add(new RegulatorCatalog
            {
                Id = Guid.NewGuid(),
                Code = fields[0].Trim(),
                NameAr = fields[1].Trim(),
                NameEn = fields[2].Trim(),
                JurisdictionEn = fields[3].Trim(),
                Website = fields[4].Trim(),
                Category = fields[5].Trim(),
                Sector = fields[6].Trim(),
                Established = int.TryParse(fields[7].Trim(), out var year) ? year : null,
                RegionType = DetermineRegionType(fields[0].Trim()),
                IsActive = true,
                DisplayOrder = displayOrder++,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "CsvSeeder"
            });
        }

        await _context.RegulatorCatalogs.AddRangeAsync(regulators);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Seeded {regulators.Count} regulators from CSV.");
    }

    public async Task SeedFrameworksFromCsvAsync()
    {
        if (await _context.FrameworkCatalogs.AnyAsync())
        {
            _logger.LogInformation("✅ FrameworkCatalogs already seeded. Skipping.");
            return;
        }

        var csvPath = Path.Combine(_seedDataPath, "frameworks_catalog_seed.csv");
        if (!File.Exists(csvPath))
        {
            _logger.LogWarning($"⚠️ CSV file not found: {csvPath}");
            return;
        }

        // Build regulator lookup
        var regulatorLookup = await _context.RegulatorCatalogs
            .ToDictionaryAsync(r => r.Code, r => r.Id);

        var frameworks = new List<FrameworkCatalog>();
        var frameworkCodes = new HashSet<string>(); // Track codes to prevent duplicates
        var lines = await File.ReadAllLinesAsync(csvPath);
        var displayOrder = 1;

        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length < 9) continue;

            // CSV: code,version,title_en,title_ar,regulator,category,mandatory,controls,domains
            var frameworkCode = fields[0].Trim();
            
            // Skip if duplicate code
            if (frameworkCodes.Contains(frameworkCode))
            {
                _logger.LogWarning($"⚠️ Skipping duplicate framework code: {frameworkCode}");
                continue;
            }
            
            frameworkCodes.Add(frameworkCode);
            
            var regulatorCode = fields[4].Trim();
            Guid? regulatorId = regulatorLookup.TryGetValue(regulatorCode, out var id) ? id : null;

            frameworks.Add(new FrameworkCatalog
            {
                Id = Guid.NewGuid(),
                Code = frameworkCode,
                Version = fields[1].Trim(),
                TitleEn = fields[2].Trim(),
                TitleAr = fields[3].Trim(),
                RegulatorId = regulatorId,
                Category = fields[5].Trim(),
                IsMandatory = bool.TryParse(fields[6].Trim(), out var mandatory) && mandatory,
                ControlCount = int.TryParse(fields[7].Trim(), out var controls) ? controls : 0,
                Domains = fields[8].Trim(),
                Status = "Active",
                IsActive = true,
                DisplayOrder = displayOrder++,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "CsvSeeder"
            });
        }

        await _context.FrameworkCatalogs.AddRangeAsync(frameworks);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Seeded {frameworks.Count} frameworks from CSV.");
    }

    public async Task SeedControlsFromCsvAsync()
    {
        if (await _context.ControlCatalogs.AnyAsync())
        {
            _logger.LogInformation("✅ ControlCatalogs already seeded. Skipping.");
            return;
        }

        var csvPath = Path.Combine(_seedDataPath, "controls_catalog_seed.csv");
        if (!File.Exists(csvPath))
        {
            _logger.LogWarning($"⚠️ CSV file not found: {csvPath}");
            return;
        }

        // Build framework lookup - handle duplicates by taking first occurrence
        var frameworkCatalogs = await _context.FrameworkCatalogs.ToListAsync();
        var frameworkLookup = frameworkCatalogs
            .GroupBy(f => f.Code)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var controls = new List<ControlCatalog>();
        var lines = await File.ReadAllLinesAsync(csvPath);
        var displayOrder = 1;
        var batchSize = 1000;
        var totalSeeded = 0;

        _logger.LogInformation($"Processing {lines.Length - 1} control records...");

        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length < 16) continue;

            // CSV: id,framework_code,version,control_number,domain,title_ar,title_en,
            //      requirement_ar,requirement_en,control_type,maturity_level,
            //      implementation_guidance_en,evidence_requirements,mapping_iso27001,mapping_nist,status
            var frameworkCode = fields[1].Trim();
            if (!frameworkLookup.TryGetValue(frameworkCode, out var frameworkId))
            {
                continue; // Skip if framework not found
            }

            controls.Add(new ControlCatalog
            {
                Id = Guid.NewGuid(),
                ControlId = $"{frameworkCode}-{fields[3].Trim()}",
                FrameworkId = frameworkId,
                Version = fields[2].Trim(),
                ControlNumber = fields[3].Trim(),
                Domain = fields[4].Trim(),
                TitleAr = fields[5].Trim(),
                TitleEn = fields[6].Trim(),
                RequirementAr = fields[7].Trim(),
                RequirementEn = fields[8].Trim(),
                ControlType = fields[9].Trim(),
                MaturityLevel = int.TryParse(fields[10].Trim(), out var level) ? level : 1,
                ImplementationGuidanceEn = fields[11].Trim(),
                EvidenceRequirements = fields[12].Trim(),
                MappingIso27001 = fields[13].Trim(),
                MappingNistCsf = fields[14].Trim(),
                Status = fields[15].Trim(),
                IsActive = true,
                DisplayOrder = displayOrder++,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "CsvSeeder"
            });

            // Batch insert for performance
            if (controls.Count >= batchSize)
            {
                await _context.ControlCatalogs.AddRangeAsync(controls);
                await _context.SaveChangesAsync();
                totalSeeded += controls.Count;
                _logger.LogInformation($"  Seeded {totalSeeded} controls...");
                controls.Clear();
            }
        }

        // Insert remaining
        if (controls.Count > 0)
        {
            await _context.ControlCatalogs.AddRangeAsync(controls);
            await _context.SaveChangesAsync();
            totalSeeded += controls.Count;
        }

        _logger.LogInformation($"✅ Seeded {totalSeeded} controls from CSV.");
    }

    private static string DetermineRegionType(string code)
    {
        var internationalCodes = new HashSet<string>
        {
            "ISO", "IEC", "NIST", "PCI-SSC", "AICPA", "ISACA", "EU-GDPR", "HHS-OCR",
            "HITRUST", "CIS", "OWASP", "CSA", "SWIFT", "BASEL", "FATF", "IIA",
            "COSO", "ITIL", "PMI", "TOGAF"
        };

        var regionalCodes = new HashSet<string>
        {
            "CBUAE", "ADGM", "DIFC", "CBB", "CBO", "QCB", "CBK", "NESA", "NCSC-UK", "ENISA"
        };

        if (internationalCodes.Contains(code)) return "international";
        if (regionalCodes.Contains(code)) return "regional";
        return "saudi";
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString());
        return result.ToArray();
    }
}
