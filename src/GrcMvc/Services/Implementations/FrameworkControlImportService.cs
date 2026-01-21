using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service to import framework controls from CSV files
    /// Supports the CSV format: id,framework_code,version,control_number,domain,title_ar,title_en,requirement_ar,requirement_en,control_type,maturity_level,implementation_guidance_en,evidence_requirements,mapping_iso27001,mapping_nist,status
    /// </summary>
    public class FrameworkControlImportService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<FrameworkControlImportService> _logger;

        public FrameworkControlImportService(GrcDbContext context, ILogger<FrameworkControlImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Import controls from CSV file path
        /// </summary>
        public async Task<ImportResult> ImportFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ImportResult { Success = false, ErrorMessage = $"File not found: {filePath}" };
            }

            using var reader = new StreamReader(filePath);
            return await ImportFromStreamAsync(reader);
        }

        /// <summary>
        /// Import controls from CSV stream
        /// </summary>
        public async Task<ImportResult> ImportFromStreamAsync(StreamReader reader)
        {
            var result = new ImportResult();
            var controls = new List<FrameworkControl>();

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<FrameworkControlCsvMap>();

                var records = csv.GetRecords<FrameworkControlCsvRecord>().ToList();
                _logger.LogInformation("Read {Count} records from CSV", records.Count);

                // Get existing controls to avoid duplicates
                var existingKeysList = await _context.Set<FrameworkControl>()
                    .Select(c => $"{c.FrameworkCode}|{c.Version}|{c.ControlNumber}")
                    .ToListAsync();
                var existingKeys = existingKeysList.ToHashSet();

                int skipped = 0;
                int added = 0;

                foreach (var record in records)
                {
                    var key = $"{record.FrameworkCode}|{record.Version}|{record.ControlNumber}";

                    if (existingKeys.Contains(key))
                    {
                        skipped++;
                        continue;
                    }

                    var control = new FrameworkControl
                    {
                        Id = Guid.NewGuid(),
                        FrameworkCode = record.FrameworkCode ?? string.Empty,
                        Version = record.Version ?? string.Empty,
                        ControlNumber = record.ControlNumber ?? string.Empty,
                        Domain = record.Domain ?? string.Empty,
                        TitleAr = record.TitleAr ?? string.Empty,
                        TitleEn = record.TitleEn ?? string.Empty,
                        RequirementAr = record.RequirementAr ?? string.Empty,
                        RequirementEn = record.RequirementEn ?? string.Empty,
                        ControlType = record.ControlType ?? "preventive",
                        MaturityLevel = int.TryParse(record.MaturityLevel, out var ml) ? ml : 1,
                        ImplementationGuidanceEn = record.ImplementationGuidanceEn ?? string.Empty,
                        EvidenceRequirements = record.EvidenceRequirements ?? string.Empty,
                        MappingIso27001 = record.MappingIso27001 ?? string.Empty,
                        MappingNist = record.MappingNist ?? string.Empty,
                        Status = record.Status ?? "active",
                        SearchKeywords = $"{record.TitleEn} {record.Domain} {record.ControlType}".ToLower()
                    };

                    controls.Add(control);
                    existingKeys.Add(key);
                    added++;
                }

                if (controls.Any())
                {
                    // Batch insert for performance
                    const int batchSize = 500;
                    for (int i = 0; i < controls.Count; i += batchSize)
                    {
                        var batch = controls.Skip(i).Take(batchSize).ToList();
                        await _context.Set<FrameworkControl>().AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Imported batch {Batch}/{Total}", i / batchSize + 1, (controls.Count / batchSize) + 1);
                    }
                }

                result.Success = true;
                result.TotalRecords = records.Count;
                result.ImportedCount = added;
                result.SkippedCount = skipped;
                result.Message = $"Successfully imported {added} controls, skipped {skipped} duplicates";

                _logger.LogInformation("✅ Import complete: {Added} added, {Skipped} skipped", added, skipped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing controls from CSV");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Get statistics about imported controls
        /// </summary>
        public async Task<ControlStatistics> GetStatisticsAsync()
        {
            var stats = new ControlStatistics();

            stats.TotalControls = await _context.Set<FrameworkControl>().CountAsync();

            stats.ByFramework = await _context.Set<FrameworkControl>()
                .GroupBy(c => c.FrameworkCode)
                .Select(g => new { Framework = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Framework, x => x.Count);

            stats.ByDomain = await _context.Set<FrameworkControl>()
                .GroupBy(c => c.Domain)
                .Select(g => new { Domain = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Domain, x => x.Count);

            stats.ByControlType = await _context.Set<FrameworkControl>()
                .GroupBy(c => c.ControlType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return stats;
        }

        /// <summary>
        /// Get controls for a specific framework
        /// </summary>
        public async Task<List<FrameworkControl>> GetControlsByFrameworkAsync(string frameworkCode, string? version = null)
        {
            var query = _context.Set<FrameworkControl>()
                .Where(c => c.FrameworkCode == frameworkCode && c.Status == "active");

            if (!string.IsNullOrEmpty(version))
            {
                query = query.Where(c => c.Version == version);
            }

            return await query.OrderBy(c => c.ControlNumber).ToListAsync();
        }

        /// <summary>
        /// Search controls by keyword
        /// </summary>
        public async Task<List<FrameworkControl>> SearchControlsAsync(string keyword, int limit = 50)
        {
            var lowerKeyword = keyword.ToLower();
            return await _context.Set<FrameworkControl>()
                .Where(c => c.SearchKeywords.Contains(lowerKeyword) ||
                           c.TitleEn.ToLower().Contains(lowerKeyword) ||
                           c.RequirementEn.ToLower().Contains(lowerKeyword) ||
                           c.ControlNumber.Contains(keyword))
                .Take(limit)
                .ToListAsync();
        }
    }

    /// <summary>
    /// CSV record mapping class
    /// </summary>
    public class FrameworkControlCsvRecord
    {
        public string? Id { get; set; }
        public string? FrameworkCode { get; set; }
        public string? Version { get; set; }
        public string? ControlNumber { get; set; }
        public string? Domain { get; set; }
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public string? RequirementAr { get; set; }
        public string? RequirementEn { get; set; }
        public string? ControlType { get; set; }
        public string? MaturityLevel { get; set; }
        public string? ImplementationGuidanceEn { get; set; }
        public string? EvidenceRequirements { get; set; }
        public string? MappingIso27001 { get; set; }
        public string? MappingNist { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// CSV mapping configuration
    /// </summary>
    public sealed class FrameworkControlCsvMap : ClassMap<FrameworkControlCsvRecord>
    {
        public FrameworkControlCsvMap()
        {
            Map(m => m.Id).Name("id");
            Map(m => m.FrameworkCode).Name("framework_code");
            Map(m => m.Version).Name("version");
            Map(m => m.ControlNumber).Name("control_number");
            Map(m => m.Domain).Name("domain");
            Map(m => m.TitleAr).Name("title_ar");
            Map(m => m.TitleEn).Name("title_en");
            Map(m => m.RequirementAr).Name("requirement_ar");
            Map(m => m.RequirementEn).Name("requirement_en");
            Map(m => m.ControlType).Name("control_type");
            Map(m => m.MaturityLevel).Name("maturity_level");
            Map(m => m.ImplementationGuidanceEn).Name("implementation_guidance_en");
            Map(m => m.EvidenceRequirements).Name("evidence_requirements");
            Map(m => m.MappingIso27001).Name("mapping_iso27001");
            Map(m => m.MappingNist).Name("mapping_nist");
            Map(m => m.Status).Name("status");
        }
    }

    /// <summary>
    /// Import result
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Control statistics
    /// </summary>
    public class ControlStatistics
    {
        public int TotalControls { get; set; }
        public Dictionary<string, int> ByFramework { get; set; } = new();
        public Dictionary<string, int> ByDomain { get; set; } = new();
        public Dictionary<string, int> ByControlType { get; set; } = new();
    }
}
