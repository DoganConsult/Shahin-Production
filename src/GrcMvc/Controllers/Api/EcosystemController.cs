using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Ecosystem Collaboration API Controller
    /// Handles cross-organization collaboration with consultants, auditors, vendors, and regulators
    /// </summary>
    [Route("api/ecosystem")]
    [ApiController]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class EcosystemController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<EcosystemController> _logger;

        public EcosystemController(
            GrcDbContext context,
            ILogger<EcosystemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Partner Discovery

        /// <summary>
        /// Search ecosystem partners
        /// GET /api/ecosystem/partners
        /// </summary>
        [HttpGet("partners")]
        public async Task<IActionResult> SearchPartners(
            [FromQuery] string? type = null,
            [FromQuery] string? sector = null,
            [FromQuery] string? service = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Set<EcosystemPartner>()
                    .Where(p => p.IsActive);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(p => p.Type == type);

                if (!string.IsNullOrEmpty(sector))
                    query = query.Where(p => p.Sector == sector || p.Sector == "all");

                if (!string.IsNullOrEmpty(service))
                    query = query.Where(p => p.ServicesJson != null && p.ServicesJson.Contains(service));

                var total = await query.CountAsync();

                var rawPartners = await query
                    .OrderByDescending(p => p.Rating)
                    .ThenByDescending(p => p.ConnectionsCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var partners = rawPartners.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Type,
                    p.Sector,
                    p.Description,
                    Services = p.ServicesJson != null ? JsonSerializer.Deserialize<List<string>>(p.ServicesJson) : new List<string>(),
                    Certifications = p.CertificationsJson != null ? JsonSerializer.Deserialize<List<string>>(p.CertificationsJson) : new List<string>(),
                    p.Rating,
                    p.ReviewCount,
                    p.ConnectionsCount,
                    p.IsVerified,
                    p.LogoUrl,
                    p.Country,
                    p.City
                }).ToList();

                return Ok(new
                {
                    total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    partners
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching partners");
                return StatusCode(500, new { error = "Failed to search partners" });
            }
        }

        /// <summary>
        /// Get partner details
        /// GET /api/ecosystem/partners/{partnerId}
        /// </summary>
        [HttpGet("partners/{partnerId}")]
        public async Task<IActionResult> GetPartner(Guid partnerId)
        {
            try
            {
                var rawPartner = await _context.Set<EcosystemPartner>()
                    .Where(p => p.Id == partnerId)
                    .FirstOrDefaultAsync();

                if (rawPartner == null)
                    return NotFound(new { error = "Partner not found" });

                var partner = new
                {
                    rawPartner.Id,
                    rawPartner.Name,
                    rawPartner.Type,
                    rawPartner.Sector,
                    rawPartner.Description,
                    Services = rawPartner.ServicesJson != null ? JsonSerializer.Deserialize<List<string>>(rawPartner.ServicesJson) : new List<string>(),
                    Certifications = rawPartner.CertificationsJson != null ? JsonSerializer.Deserialize<List<string>>(rawPartner.CertificationsJson) : new List<string>(),
                    rawPartner.ContactEmail,
                    rawPartner.Website,
                    rawPartner.LogoUrl,
                    rawPartner.Rating,
                    rawPartner.ReviewCount,
                    rawPartner.ConnectionsCount,
                    rawPartner.IsVerified,
                    rawPartner.Country,
                    rawPartner.City
                };

                return Ok(partner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partner");
                return StatusCode(500, new { error = "Failed to get partner" });
            }
        }

        /// <summary>
        /// Get recommended partners based on tenant profile
        /// GET /api/ecosystem/partners/recommended
        /// </summary>
        [HttpGet("partners/recommended")]
        public async Task<IActionResult> GetRecommendedPartners()
        {
            try
            {
                var tenantId = GetTenantId();
                var tenant = await _context.Tenants.FindAsync(tenantId);

                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                // Get top rated active and verified partners
                var rawPartners = await _context.Set<EcosystemPartner>()
                    .Where(p => p.IsActive && p.IsVerified)
                    .OrderByDescending(p => p.Rating)
                    .Take(10)
                    .ToListAsync();

                var partners = rawPartners.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Type,
                    p.Description,
                    Services = p.ServicesJson != null ? JsonSerializer.Deserialize<List<string>>(p.ServicesJson) : new List<string>(),
                    p.Rating,
                    p.IsVerified,
                    RecommendationReason = "Top rated partner in your region"
                }).ToList();

                return Ok(new { partners });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended partners");
                return StatusCode(500, new { error = "Failed to get recommendations" });
            }
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Request connection with partner
        /// POST /api/ecosystem/connections
        /// </summary>
        [HttpPost("connections")]
        public async Task<IActionResult> RequestConnection([FromBody] ConnectionRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                // Check for existing connection
                var existing = await _context.Set<EcosystemConnection>()
                    .AnyAsync(c => c.TenantId == tenantId &&
                                   c.PartnerId == request.PartnerId &&
                                   c.Status != "rejected" &&
                                   c.Status != "revoked");

                if (existing)
                    return BadRequest(new { error = "Connection already exists with this partner" });

                var partner = await _context.Set<EcosystemPartner>().FindAsync(request.PartnerId);

                var connection = new EcosystemConnection
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PartnerId = request.PartnerId,
                    PartnerType = partner?.Type ?? request.PartnerType,
                    PartnerEmail = partner?.ContactEmail ?? request.PartnerEmail,
                    PartnerName = partner?.Name,
                    Purpose = request.Purpose,
                    SharedDataTypesJson = JsonSerializer.Serialize(request.SharedDataTypes ?? new List<string>()),
                    ExpiresAt = request.ExpiresAt,
                    Status = "pending",
                    RequestedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Set<EcosystemConnection>().Add(connection);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Connection requested: {ConnectionId} by tenant {TenantId}", connection.Id, tenantId);

                return Ok(new
                {
                    success = true,
                    connectionId = connection.Id,
                    status = "pending",
                    message = "Connection request sent"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting connection");
                return StatusCode(500, new { error = "Failed to request connection" });
            }
        }

        /// <summary>
        /// Get tenant's connections
        /// GET /api/ecosystem/connections
        /// </summary>
        [HttpGet("connections")]
        public async Task<IActionResult> GetConnections([FromQuery] string? status = null)
        {
            try
            {
                var tenantId = GetTenantId();

                var query = _context.Set<EcosystemConnection>()
                    .Where(c => c.TenantId == tenantId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status == status);

                var rawConnections = await query
                    .OrderByDescending(c => c.RequestedAt)
                    .ToListAsync();

                var connections = rawConnections.Select(c => new
                {
                    c.Id,
                    c.PartnerId,
                    c.PartnerName,
                    c.PartnerType,
                    c.Purpose,
                    SharedDataTypes = c.SharedDataTypesJson != null ? JsonSerializer.Deserialize<List<string>>(c.SharedDataTypesJson) : new List<string>(),
                    c.Status,
                    c.RequestedAt,
                    c.ApprovedAt,
                    c.ExpiresAt,
                    c.InteractionsCount,
                    c.LastInteractionAt
                }).ToList();

                return Ok(new { total = connections.Count, connections });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections");
                return StatusCode(500, new { error = "Failed to get connections" });
            }
        }

        /// <summary>
        /// Update connection (approve/reject for partners, revoke for tenants)
        /// PATCH /api/ecosystem/connections/{connectionId}
        /// </summary>
        [HttpPatch("connections/{connectionId}")]
        public async Task<IActionResult> UpdateConnection(Guid connectionId, [FromBody] UpdateConnectionRequest request)
        {
            try
            {
                var connection = await _context.Set<EcosystemConnection>().FindAsync(connectionId);
                if (connection == null)
                    return NotFound(new { error = "Connection not found" });

                switch (request.Action.ToLower())
                {
                    case "approve":
                        connection.Status = "approved";
                        connection.ApprovedAt = DateTime.UtcNow;
                        // Increment partner connection count
                        if (connection.PartnerId.HasValue)
                        {
                            var partner = await _context.Set<EcosystemPartner>().FindAsync(connection.PartnerId);
                            if (partner != null) partner.ConnectionsCount++;
                        }
                        break;

                    case "reject":
                        connection.Status = "rejected";
                        connection.RejectedAt = DateTime.UtcNow;
                        connection.RejectionReason = request.Reason;
                        break;

                    case "revoke":
                        connection.Status = "revoked";
                        connection.ModifiedDate = DateTime.UtcNow;
                        break;

                    default:
                        return BadRequest(new { error = "Invalid action" });
                }

                connection.ModifiedBy = GetUserId();
                await _context.SaveChangesAsync();

                return Ok(new { success = true, status = connection.Status, message = $"Connection {request.Action}d" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connection");
                return StatusCode(500, new { error = "Failed to update connection" });
            }
        }

        #endregion

        #region Shared Data & Collaboration

        /// <summary>
        /// Share data with connected partner
        /// POST /api/ecosystem/connections/{connectionId}/share
        /// </summary>
        [HttpPost("connections/{connectionId}/share")]
        public async Task<IActionResult> ShareData(Guid connectionId, [FromBody] ShareDataRequest request)
        {
            try
            {
                var connection = await _context.Set<EcosystemConnection>().FindAsync(connectionId);
                if (connection == null)
                    return NotFound(new { error = "Connection not found" });

                if (connection.Status != "approved")
                    return BadRequest(new { error = "Connection must be approved to share data" });

                // Log the share activity
                connection.InteractionsCount++;
                connection.LastInteractionAt = DateTime.UtcNow;

                // Create share record (you would implement actual data sharing logic here)
                var shareLog = new
                {
                    connectionId,
                    dataType = request.DataType,
                    resourceIds = request.ResourceIds,
                    sharedAt = DateTime.UtcNow,
                    sharedBy = GetUserId(),
                    expiresAt = request.ExpiresAt
                };

                _logger.LogInformation("Data shared: {ShareLog}", JsonSerializer.Serialize(shareLog));

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Shared {request.ResourceIds?.Count ?? 0} {request.DataType} items with partner"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing data");
                return StatusCode(500, new { error = "Failed to share data" });
            }
        }

        /// <summary>
        /// Get shared data log for connection
        /// GET /api/ecosystem/connections/{connectionId}/shares
        /// </summary>
        [HttpGet("connections/{connectionId}/shares")]
        public async Task<IActionResult> GetSharedDataLog(Guid connectionId)
        {
            try
            {
                var connection = await _context.Set<EcosystemConnection>().FindAsync(connectionId);
                if (connection == null)
                    return NotFound(new { error = "Connection not found" });

                // Return mock data for now - in production, query actual share log table
                var shares = new List<object>
                {
                    new { dataType = "compliance_report", count = 3, sharedAt = DateTime.UtcNow.AddDays(-5) },
                    new { dataType = "evidence", count = 12, sharedAt = DateTime.UtcNow.AddDays(-3) },
                    new { dataType = "risk_register", count = 1, sharedAt = DateTime.UtcNow.AddDays(-1) }
                };

                return Ok(new
                {
                    connectionId,
                    totalInteractions = connection.InteractionsCount,
                    lastInteraction = connection.LastInteractionAt,
                    shares
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared data log");
                return StatusCode(500, new { error = "Failed to get shared data log" });
            }
        }

        #endregion

        #region Partner Types & Categories

        /// <summary>
        /// Get available partner types
        /// GET /api/ecosystem/partner-types
        /// </summary>
        [HttpGet("partner-types")]
        public IActionResult GetPartnerTypes()
        {
            var types = new[]
            {
                new { code = "consultant", name = "Consultant", nameAr = "مستشار", description = "GRC consulting and advisory services" },
                new { code = "auditor", name = "Auditor", nameAr = "مدقق", description = "Internal and external audit services" },
                new { code = "vendor", name = "Technology Vendor", nameAr = "مورد تقني", description = "Security and compliance tools" },
                new { code = "regulator", name = "Regulator", nameAr = "جهة تنظيمية", description = "Regulatory bodies and authorities" },
                new { code = "certifier", name = "Certification Body", nameAr = "جهة اعتماد", description = "ISO and compliance certifications" },
                new { code = "training", name = "Training Provider", nameAr = "مزود تدريب", description = "GRC training and awareness" },
                new { code = "legal", name = "Legal Advisor", nameAr = "مستشار قانوني", description = "Legal and regulatory advice" },
                new { code = "insurance", name = "Cyber Insurance", nameAr = "تأمين سيبراني", description = "Cyber risk insurance providers" }
            };

            return Ok(new { types });
        }

        /// <summary>
        /// Get partner service categories
        /// GET /api/ecosystem/service-categories
        /// </summary>
        [HttpGet("service-categories")]
        public IActionResult GetServiceCategories()
        {
            var categories = new[]
            {
                new { code = "nca_compliance", name = "NCA Compliance", nameAr = "امتثال الهيئة الوطنية" },
                new { code = "sama_framework", name = "SAMA Framework", nameAr = "إطار ساما" },
                new { code = "pdpl", name = "PDPL Compliance", nameAr = "حماية البيانات الشخصية" },
                new { code = "iso_27001", name = "ISO 27001", nameAr = "أيزو 27001" },
                new { code = "iso_22301", name = "ISO 22301 BCM", nameAr = "استمرارية الأعمال" },
                new { code = "soc2", name = "SOC 2", nameAr = "SOC 2" },
                new { code = "pci_dss", name = "PCI DSS", nameAr = "PCI DSS" },
                new { code = "penetration_testing", name = "Penetration Testing", nameAr = "اختبار الاختراق" },
                new { code = "vulnerability_assessment", name = "Vulnerability Assessment", nameAr = "تقييم الثغرات" },
                new { code = "gap_analysis", name = "Gap Analysis", nameAr = "تحليل الفجوات" },
                new { code = "risk_assessment", name = "Risk Assessment", nameAr = "تقييم المخاطر" },
                new { code = "awareness_training", name = "Security Awareness", nameAr = "التوعية الأمنية" }
            };

            return Ok(new { categories });
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get ecosystem analytics for tenant
        /// GET /api/ecosystem/analytics
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var tenantId = GetTenantId();

                var connections = await _context.Set<EcosystemConnection>()
                    .Where(c => c.TenantId == tenantId)
                    .ToListAsync();

                var analytics = new
                {
                    totalConnections = connections.Count,
                    activeConnections = connections.Count(c => c.Status == "approved"),
                    pendingRequests = connections.Count(c => c.Status == "pending"),
                    totalInteractions = connections.Sum(c => c.InteractionsCount),
                    connectionsByType = connections
                        .GroupBy(c => c.PartnerType ?? "other")
                        .Select(g => new { type = g.Key, count = g.Count() })
                        .ToList(),
                    recentActivity = connections
                        .Where(c => c.LastInteractionAt.HasValue)
                        .OrderByDescending(c => c.LastInteractionAt)
                        .Take(5)
                        .Select(c => new
                        {
                            c.PartnerName,
                            c.PartnerType,
                            lastInteraction = c.LastInteractionAt
                        })
                        .ToList()
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { error = "Failed to get analytics" });
            }
        }

        #endregion

        #region Helpers

        private Guid GetTenantId()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system";
        }

        #endregion
    }

    #region Request DTOs

    public class ConnectionRequest
    {
        public Guid? PartnerId { get; set; }
        public string? PartnerType { get; set; }
        public string? PartnerEmail { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public List<string>? SharedDataTypes { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateConnectionRequest
    {
        public string Action { get; set; } = string.Empty; // approve, reject, revoke
        public string? Reason { get; set; }
    }

    public class ShareDataRequest
    {
        public string DataType { get; set; } = string.Empty; // evidence, report, risk, control
        public List<Guid>? ResourceIds { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Message { get; set; }
    }

    #endregion
}
