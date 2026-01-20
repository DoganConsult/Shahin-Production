using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Test controller for verifying sector tables and relationships
/// </summary>
[ApiController]
[Route("api/test/sectors")]
[Authorize(Roles = "PlatformAdmin")]
public class SectorTestController : ControllerBase
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SectorTestController> _logger;

    public SectorTestController(GrcDbContext context, ILogger<SectorTestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Test and seed sector data - verifies full capacity
    /// </summary>
    [HttpPost("seed-and-verify")]
    public async Task<IActionResult> SeedAndVerify()
    {
        try
        {
            var errors = new List<string>();
            var mainSectorsSeeded = false;
            var subSectorsSeeded = false;
            var foreignKeysLinked = false;
            var mainSectorCount = 0;
            var subSectorCount = 0;
            var linkedSubSectorCount = 0;

            // Step 1: Seed main sectors
            try
            {
                await GrcMainSectorSeeds.SeedMainSectorsAsync(_context, _logger);
                mainSectorsSeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding main sectors");
                errors.Add($"Main sectors: {ex.Message}");
            }

            // Step 2: Seed sub-sectors
            try
            {
                await GosiSectorSeeds.SeedSubSectorMappingsAsync(_context, _logger);
                subSectorsSeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding sub-sectors");
                errors.Add($"Sub-sectors: {ex.Message}");
            }

            // Step 3: Verify data
            mainSectorCount = await _context.GrcMainSectors.CountAsync();
            subSectorCount = await _context.GrcSubSectorMappings.CountAsync();
            linkedSubSectorCount = await _context.GrcSubSectorMappings
                .Where(s => s.MainSectorId != null)
                .CountAsync();

            foreignKeysLinked = linkedSubSectorCount == subSectorCount;

            return Ok(new
            {
                success = true,
                message = "Sector seeding and verification completed",
                results = new
                {
                    MainSectorsSeeded = mainSectorsSeeded,
                    SubSectorsSeeded = subSectorsSeeded,
                    ForeignKeysLinked = foreignKeysLinked,
                    MainSectorCount = mainSectorCount,
                    SubSectorCount = subSectorCount,
                    LinkedSubSectorCount = linkedSubSectorCount,
                    Errors = errors
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in seed and verify");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get verification report of sector data
    /// </summary>
    [HttpGet("verify")]
    public async Task<IActionResult> Verify()
    {
        try
        {
            var mainSectors = await _context.GrcMainSectors
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var subSectors = await _context.GrcSubSectorMappings
                .Include(s => s.MainSector)
                .ToListAsync();

            var sectorStats = mainSectors.Select(ms => new
            {
                SectorCode = ms.SectorCode,
                SectorNameEn = ms.SectorNameEn,
                SubSectorCount = subSectors.Count(s => s.MainSectorCode == ms.SectorCode),
                LinkedCount = subSectors.Count(s => s.MainSectorId == ms.Id),
                IsLinked = subSectors.Any(s => s.MainSectorId == ms.Id)
            }).ToList();

            var unlinkedSubSectors = subSectors
                .Where(s => s.MainSectorId == null)
                .Select(s => new { s.GosiCode, s.MainSectorCode })
                .ToList();

            return Ok(new
            {
                success = true,
                summary = new
                {
                    MainSectorCount = mainSectors.Count,
                    SubSectorCount = subSectors.Count,
                    LinkedSubSectorCount = subSectors.Count(s => s.MainSectorId != null),
                    UnlinkedSubSectorCount = unlinkedSubSectors.Count,
                    AllLinked = unlinkedSubSectors.Count == 0
                },
                sectorStats,
                unlinkedSubSectors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying sectors");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get all main sectors with their sub-sectors
    /// </summary>
    [HttpGet("main-sectors")]
    public async Task<IActionResult> GetMainSectors()
    {
        try
        {
            var sectors = await _context.GrcMainSectors
                .Include(s => s.SubSectors)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new
                {
                    s.Id,
                    s.SectorCode,
                    s.SectorNameEn,
                    s.SectorNameAr,
                    s.PrimaryRegulator,
                    s.FrameworkCount,
                    s.TotalControlCount,
                    s.Icon,
                    s.ColorCode,
                    SubSectorCount = s.SubSectors.Count,
                    SubSectors = s.SubSectors.Select(ss => new
                    {
                        ss.GosiCode,
                        ss.SubSectorNameEn,
                        ss.SubSectorNameAr,
                        ss.IsActive
                    })
                })
                .ToListAsync();

            return Ok(new { success = true, data = sectors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting main sectors");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get sub-sectors for a specific main sector
    /// </summary>
    [HttpGet("sub-sectors/{sectorCode}")]
    public async Task<IActionResult> GetSubSectors(string sectorCode)
    {
        try
        {
            var mainSector = await _context.GrcMainSectors
                .FirstOrDefaultAsync(s => s.SectorCode == sectorCode);

            if (mainSector == null)
            {
                return NotFound(new { success = false, error = $"Main sector '{sectorCode}' not found" });
            }

            var subSectors = await _context.GrcSubSectorMappings
                .Where(s => s.MainSectorCode == sectorCode || s.MainSectorId == mainSector.Id)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new
                {
                    s.GosiCode,
                    s.IsicSection,
                    s.SubSectorNameEn,
                    s.SubSectorNameAr,
                    s.PrimaryRegulator,
                    s.IsActive,
                    IsLinked = s.MainSectorId != null
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                mainSector = new
                {
                    mainSector.SectorCode,
                    mainSector.SectorNameEn,
                    mainSector.SectorNameAr
                },
                subSectors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-sectors");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Fix unlinked foreign keys
    /// </summary>
    [HttpPost("fix-foreign-keys")]
    public async Task<IActionResult> FixForeignKeys()
    {
        try
        {
            var unlinked = await _context.GrcSubSectorMappings
                .Where(s => s.MainSectorId == null)
                .ToListAsync();

            var mainSectors = await _context.GrcMainSectors.ToListAsync();
            var sectorCodeToId = mainSectors.ToDictionary(s => s.SectorCode, s => s.Id);

            int fixedCount = 0;
            foreach (var subSector in unlinked)
            {
                if (sectorCodeToId.TryGetValue(subSector.MainSectorCode, out var mainSectorId))
                {
                    subSector.MainSectorId = mainSectorId;
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = $"Fixed {fixedCount} foreign key relationships",
                fixedCount = fixedCount,
                remaining = unlinked.Count - fixedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing foreign keys");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}
