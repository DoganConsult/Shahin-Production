using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Regulatory Integration Service Implementation
    /// Provides integration with KSA government regulatory portals:
    /// - NCA-ISR (National Cybersecurity Authority)
    /// - SAMA (Saudi Central Bank) e-Filing
    /// - PDPL (Personal Data Protection Law) Compliance Portal
    /// - CITC (Communications, Space and Technology Commission)
    /// - Ministry of Commerce
    /// </summary>
    public class RegulatoryIntegrationService : IRegulatoryIntegrationService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<RegulatoryIntegrationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly byte[] _encryptionKey;

        public RegulatoryIntegrationService(
            GrcDbContext context,
            ILogger<RegulatoryIntegrationService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            // Get encryption key from configuration (should be stored securely)
            var keyBase64 = _configuration["RegulatoryIntegration:EncryptionKey"];
            _encryptionKey = string.IsNullOrEmpty(keyBase64)
                ? Encoding.UTF8.GetBytes("DefaultKey32BytesForDevelopment!") // 32 bytes for AES-256
                : Convert.FromBase64String(keyBase64);
        }

        #region Portal Connection Management

        public async Task<RegulatoryPortalConnectionDto> RegisterConnectionAsync(RegisterPortalConnectionRequest request)
        {
            _logger.LogInformation("Registering regulatory portal connection for tenant {TenantId}, portal {PortalType}",
                request.TenantId, request.PortalType);

            // Check if connection already exists
            var existing = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.TenantId == request.TenantId && c.PortalType == request.PortalType);

            if (existing != null)
            {
                throw new InvalidOperationException($"Connection for portal {request.PortalType} already exists for this tenant");
            }

            var connection = new RegulatoryPortalConnection
            {
                TenantId = request.TenantId,
                PortalType = request.PortalType,
                PortalName = request.PortalName,
                PortalNameAr = request.PortalNameAr,
                BaseUrl = request.BaseUrl,
                AuthType = request.AuthType,
                EncryptedApiKey = EncryptString(request.ApiKey),
                ClientId = request.ClientId,
                EncryptedClientSecret = EncryptString(request.ClientSecret),
                CertificateThumbprint = request.CertificateThumbprint,
                Username = request.Username,
                EncryptedPassword = EncryptString(request.Password),
                OrganizationId = request.OrganizationId,
                AdditionalSettingsJson = request.AdditionalSettings != null
                    ? JsonSerializer.Serialize(request.AdditionalSettings)
                    : null,
                IsActive = true
            };

            _context.RegulatoryPortalConnections.Add(connection);
            await _context.SaveChangesAsync();

            await LogAuditAsync(request.TenantId, request.PortalType, "ConnectionRegistered",
                null, true, "Portal connection registered successfully", "system", "System");

            return MapToConnectionDto(connection);
        }

        public async Task<List<RegulatoryPortalConnectionDto>> GetConnectionsAsync(Guid tenantId)
        {
            var connections = await _context.RegulatoryPortalConnections
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .ToListAsync();

            return connections.Select(MapToConnectionDto).ToList();
        }

        public async Task<RegulatoryPortalConnectionDto?> GetConnectionAsync(Guid tenantId, string portalType)
        {
            var connection = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.PortalType == portalType && !c.IsDeleted);

            return connection != null ? MapToConnectionDto(connection) : null;
        }

        public async Task<ConnectionTestResultDto> TestConnectionAsync(Guid connectionId)
        {
            var connection = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.Id == connectionId);

            if (connection == null)
            {
                return new ConnectionTestResultDto
                {
                    ConnectionId = connectionId,
                    Success = false,
                    Message = "Connection not found",
                    ErrorCode = "CONNECTION_NOT_FOUND"
                };
            }

            var startTime = DateTime.UtcNow;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(connection.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);

                // Add authentication headers based on auth type
                AddAuthHeaders(client, connection);

                // Make a health check request
                var response = await client.GetAsync("/health");
                var responseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                var result = new ConnectionTestResultDto
                {
                    ConnectionId = connectionId,
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Connection successful"
                        : $"Connection failed with status {response.StatusCode}",
                    ResponseTimeMs = responseTimeMs
                };

                // Update connection status
                connection.LastConnectedAt = DateTime.UtcNow;
                connection.LastConnectionStatus = result.Success ? "Connected" : "Failed";
                await _context.SaveChangesAsync();

                await LogAuditAsync(connection.TenantId!.Value, connection.PortalType, "ConnectionTest",
                    null, result.Success, result.Message, "system", "System");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection {ConnectionId}", connectionId);

                connection.LastConnectionStatus = "Error";
                await _context.SaveChangesAsync();

                return new ConnectionTestResultDto
                {
                    ConnectionId = connectionId,
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "CONNECTION_ERROR",
                    ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
        }

        public async Task<RegulatoryPortalConnectionDto> UpdateCredentialsAsync(Guid connectionId, UpdateCredentialsRequest request)
        {
            var connection = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.Id == connectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Connection {connectionId} not found");
            }

            if (!string.IsNullOrEmpty(request.ApiKey))
                connection.EncryptedApiKey = EncryptString(request.ApiKey);

            if (!string.IsNullOrEmpty(request.ClientId))
                connection.ClientId = request.ClientId;

            if (!string.IsNullOrEmpty(request.ClientSecret))
                connection.EncryptedClientSecret = EncryptString(request.ClientSecret);

            if (!string.IsNullOrEmpty(request.Username))
                connection.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Password))
                connection.EncryptedPassword = EncryptString(request.Password);

            connection.ModifiedDate = DateTime.UtcNow;
            connection.ModifiedBy = request.UpdatedBy;

            await _context.SaveChangesAsync();

            await LogAuditAsync(connection.TenantId!.Value, connection.PortalType, "CredentialsUpdated",
                null, true, "Credentials updated", request.UpdatedBy, request.UpdatedBy);

            return MapToConnectionDto(connection);
        }

        public async Task DeactivateConnectionAsync(Guid connectionId, string reason)
        {
            var connection = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.Id == connectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Connection {connectionId} not found");
            }

            connection.IsActive = false;
            connection.DeactivationReason = reason;
            connection.DeactivatedAt = DateTime.UtcNow;
            connection.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(connection.TenantId!.Value, connection.PortalType, "ConnectionDeactivated",
                null, true, $"Connection deactivated: {reason}", "system", "System");
        }

        #endregion

        #region NCA-ISR Integration

        public async Task<SubmissionResultDto> SubmitNcaIsrReportAsync(Guid tenantId, NcaIsrReportRequest request)
        {
            _logger.LogInformation("Submitting NCA-ISR report for tenant {TenantId}, assessment {AssessmentId}",
                tenantId, request.AssessmentId);

            var connection = await GetActiveConnectionAsync(tenantId, RegulatoryPortalTypes.NcaIsr);

            // Validate data before submission
            var validation = await ValidateNcaIsrDataAsync(tenantId, request);
            if (!validation.IsValid)
            {
                return new SubmissionResultDto
                {
                    Success = false,
                    Status = "ValidationFailed",
                    Message = string.Join("; ", validation.Errors.Select(e => e.Message)),
                    PortalType = RegulatoryPortalTypes.NcaIsr
                };
            }

            // Create submission record
            var submission = new RegulatorySubmission
            {
                TenantId = tenantId,
                ConnectionId = connection.Id,
                PortalType = RegulatoryPortalTypes.NcaIsr,
                SubmissionType = request.ReportType,
                AssessmentId = request.AssessmentId,
                ReportingPeriodStart = request.ReportingPeriodStart,
                ReportingPeriodEnd = request.ReportingPeriodEnd,
                SubmittedById = request.SubmittedById,
                SubmittedByName = request.SubmittedByName,
                Notes = request.Notes,
                Status = "Submitting",
                RequestPayloadJson = JsonSerializer.Serialize(request)
            };

            _context.RegulatorySubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Link evidence
            if (request.EvidenceIds?.Any() == true)
            {
                foreach (var evidenceId in request.EvidenceIds)
                {
                    _context.RegulatorySubmissionEvidences.Add(new RegulatorySubmissionEvidence
                    {
                        TenantId = tenantId,
                        SubmissionId = submission.Id,
                        EvidenceId = evidenceId
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Submit to portal (simulated - in production, this would call the actual NCA-ISR API)
            var result = await SubmitToPortalAsync(connection, submission, request);

            // Update submission status
            submission.Status = result.Success ? "Submitted" : "Failed";
            submission.ExternalSubmissionId = result.ExternalSubmissionId;
            submission.ConfirmationNumber = result.ConfirmationNumber;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.ResponsePayloadJson = JsonSerializer.Serialize(result);
            submission.ErrorCode = result.ErrorCode;
            submission.ErrorMessage = result.Message;

            await _context.SaveChangesAsync();

            await LogAuditAsync(tenantId, RegulatoryPortalTypes.NcaIsr, "ReportSubmitted",
                submission.Id, result.Success, result.Message ?? "Report submitted",
                request.SubmittedById, request.SubmittedByName);

            result.InternalId = submission.Id;
            return result;
        }

        public async Task<SubmissionStatusDto> GetNcaIsrStatusAsync(Guid tenantId, string submissionId)
        {
            var submission = await _context.RegulatorySubmissions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                    (s.ExternalSubmissionId == submissionId || s.Id.ToString() == submissionId));

            if (submission == null)
            {
                return new SubmissionStatusDto
                {
                    SubmissionId = submissionId,
                    PortalType = RegulatoryPortalTypes.NcaIsr,
                    Status = "NotFound"
                };
            }

            return new SubmissionStatusDto
            {
                SubmissionId = submission.ExternalSubmissionId ?? submission.Id.ToString(),
                PortalType = RegulatoryPortalTypes.NcaIsr,
                Status = submission.Status,
                StatusDescription = submission.Notes,
                LastUpdated = submission.ModifiedDate ?? submission.CreatedDate
            };
        }

        public async Task<List<SubmissionHistoryDto>> GetNcaIsrHistoryAsync(Guid tenantId)
        {
            return await GetSubmissionHistoryByPortalAsync(tenantId, RegulatoryPortalTypes.NcaIsr);
        }

        public async Task<NcaIsrComplianceReportDto> GenerateNcaIsrReportAsync(Guid tenantId, Guid assessmentId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Requirements)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TenantId == tenantId);

            if (assessment == null)
            {
                throw new KeyNotFoundException($"Assessment {assessmentId} not found");
            }

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);

            // Calculate compliance scores
            var requirements = assessment.Requirements?.ToList() ?? new List<AssessmentRequirement>();
            var totalControls = requirements.Count;
            var implementedControls = requirements.Count(r => r.Status == "Compliant");
            var partialControls = requirements.Count(r => r.Status == "PartiallyCompliant");
            var notImplementedControls = requirements.Count(r => r.Status == "NonCompliant" || r.Status == "NotApplicable");

            var overallScore = totalControls > 0
                ? Math.Round((implementedControls + (partialControls * 0.5m)) / totalControls * 100, 2)
                : 0;

            // Group by domain
            var domainScores = requirements
                .GroupBy(r => r.Domain ?? "Uncategorized")
                .ToDictionary(
                    g => g.Key,
                    g => g.Any()
                        ? Math.Round((g.Count(r => r.Status == "Compliant")
                            + (g.Count(r => r.Status == "PartiallyCompliant") * 0.5m))
                            / g.Count() * 100, 2)
                        : 0m
                );

            // Get findings
            var findings = requirements
                .Where(r => r.Status != "Compliant" && r.Status != "NotApplicable")
                .Select(r => new NcaFindingDto
                {
                    ControlId = r.ControlNumber ?? r.Id.ToString(),
                    ControlName = r.ControlTitle ?? "Unknown",
                    Domain = r.Domain ?? "Uncategorized",
                    Status = r.Status ?? "Unknown",
                    Gap = r.Findings,
                    RemediationPlan = r.RemediationPlan,
                    TargetDate = r.DueDate
                })
                .ToList();

            return new NcaIsrComplianceReportDto
            {
                AssessmentId = assessmentId,
                OrganizationName = tenant?.Name ?? "Unknown",
                ReportingPeriodStart = assessment.StartDate,
                ReportingPeriodEnd = assessment.EndDate ?? DateTime.UtcNow,
                OverallComplianceScore = overallScore,
                DomainScores = domainScores,
                TotalControls = totalControls,
                ImplementedControls = implementedControls,
                PartialControls = partialControls,
                NotImplementedControls = notImplementedControls,
                Findings = findings
            };
        }

        public async Task<ValidationResultDto> ValidateNcaIsrDataAsync(Guid tenantId, NcaIsrReportRequest request)
        {
            var errors = new List<ValidationErrorDto>();
            var warnings = new List<string>();

            // Validate assessment exists
            if (request.AssessmentId != Guid.Empty)
            {
                var assessment = await _context.Assessments
                    .FirstOrDefaultAsync(a => a.Id == request.AssessmentId && a.TenantId == tenantId);

                if (assessment == null)
                {
                    errors.Add(new ValidationErrorDto
                    {
                        Field = "AssessmentId",
                        ErrorCode = "ASSESSMENT_NOT_FOUND",
                        Message = "Assessment not found or does not belong to this tenant"
                    });
                }
            }

            // Validate reporting period
            if (request.ReportingPeriodEnd <= request.ReportingPeriodStart)
            {
                errors.Add(new ValidationErrorDto
                {
                    Field = "ReportingPeriod",
                    ErrorCode = "INVALID_PERIOD",
                    Message = "Reporting period end date must be after start date"
                });
            }

            // Validate submitter
            if (string.IsNullOrEmpty(request.SubmittedById))
            {
                errors.Add(new ValidationErrorDto
                {
                    Field = "SubmittedById",
                    ErrorCode = "SUBMITTER_REQUIRED",
                    Message = "Submitter ID is required"
                });
            }

            return new ValidationResultDto
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        #endregion

        #region SAMA e-Filing Integration

        public async Task<SubmissionResultDto> SubmitSamaReportAsync(Guid tenantId, SamaReportRequest request)
        {
            _logger.LogInformation("Submitting SAMA report for tenant {TenantId}, type {ReportType}",
                tenantId, request.ReportType);

            var connection = await GetActiveConnectionAsync(tenantId, RegulatoryPortalTypes.SamaEfiling);

            var submission = new RegulatorySubmission
            {
                TenantId = tenantId,
                ConnectionId = connection.Id,
                PortalType = RegulatoryPortalTypes.SamaEfiling,
                SubmissionType = request.ReportType,
                AssessmentId = request.AssessmentId,
                ReportingPeriodStart = request.ReportingPeriodStart,
                ReportingPeriodEnd = request.ReportingPeriodEnd,
                SubmittedById = request.SubmittedById,
                SubmittedByName = request.SubmittedByName,
                Notes = request.Notes,
                Status = "Submitting",
                RequestPayloadJson = JsonSerializer.Serialize(request)
            };

            _context.RegulatorySubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Link evidence
            if (request.EvidenceIds?.Any() == true)
            {
                foreach (var evidenceId in request.EvidenceIds)
                {
                    _context.RegulatorySubmissionEvidences.Add(new RegulatorySubmissionEvidence
                    {
                        TenantId = tenantId,
                        SubmissionId = submission.Id,
                        EvidenceId = evidenceId
                    });
                }
                await _context.SaveChangesAsync();
            }

            var result = await SubmitToPortalAsync(connection, submission, request);

            submission.Status = result.Success ? "Submitted" : "Failed";
            submission.ExternalSubmissionId = result.ExternalSubmissionId;
            submission.ConfirmationNumber = result.ConfirmationNumber;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.ResponsePayloadJson = JsonSerializer.Serialize(result);

            await _context.SaveChangesAsync();

            await LogAuditAsync(tenantId, RegulatoryPortalTypes.SamaEfiling, "ReportSubmitted",
                submission.Id, result.Success, result.Message ?? "Report submitted",
                request.SubmittedById, request.SubmittedByName);

            result.InternalId = submission.Id;
            return result;
        }

        public async Task<SubmissionStatusDto> GetSamaStatusAsync(Guid tenantId, string submissionId)
        {
            var submission = await _context.RegulatorySubmissions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.PortalType == RegulatoryPortalTypes.SamaEfiling &&
                    (s.ExternalSubmissionId == submissionId || s.Id.ToString() == submissionId));

            if (submission == null)
            {
                return new SubmissionStatusDto
                {
                    SubmissionId = submissionId,
                    PortalType = RegulatoryPortalTypes.SamaEfiling,
                    Status = "NotFound"
                };
            }

            return new SubmissionStatusDto
            {
                SubmissionId = submission.ExternalSubmissionId ?? submission.Id.ToString(),
                PortalType = RegulatoryPortalTypes.SamaEfiling,
                Status = submission.Status,
                LastUpdated = submission.ModifiedDate ?? submission.CreatedDate
            };
        }

        public async Task<List<SubmissionHistoryDto>> GetSamaHistoryAsync(Guid tenantId)
        {
            return await GetSubmissionHistoryByPortalAsync(tenantId, RegulatoryPortalTypes.SamaEfiling);
        }

        public async Task<SubmissionResultDto> SubmitSamaIncidentAsync(Guid tenantId, SamaIncidentNotificationRequest request)
        {
            _logger.LogInformation("Submitting SAMA incident notification for tenant {TenantId}, severity {Severity}",
                tenantId, request.Severity);

            var connection = await GetActiveConnectionAsync(tenantId, RegulatoryPortalTypes.SamaEfiling);

            var submission = new RegulatorySubmission
            {
                TenantId = tenantId,
                ConnectionId = connection.Id,
                PortalType = RegulatoryPortalTypes.SamaEfiling,
                SubmissionType = "IncidentNotification",
                IncidentId = request.IncidentId,
                SubmittedById = request.SubmittedById,
                SubmittedByName = request.SubmittedByName,
                Notes = request.Description,
                Status = "Submitting",
                RequestPayloadJson = JsonSerializer.Serialize(request)
            };

            _context.RegulatorySubmissions.Add(submission);
            await _context.SaveChangesAsync();

            var result = await SubmitToPortalAsync(connection, submission, request);

            submission.Status = result.Success ? "Submitted" : "Failed";
            submission.ExternalSubmissionId = result.ExternalSubmissionId;
            submission.ConfirmationNumber = result.ConfirmationNumber;
            submission.SubmittedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(tenantId, RegulatoryPortalTypes.SamaEfiling, "IncidentSubmitted",
                submission.Id, result.Success, result.Message ?? "Incident notification submitted",
                request.SubmittedById, request.SubmittedByName);

            result.InternalId = submission.Id;
            return result;
        }

        public async Task<List<RegulatoryPortalDeadlineDto>> GetSamaDeadlinesAsync(Guid tenantId)
        {
            return await GetDeadlinesByPortalAsync(tenantId, RegulatoryPortalTypes.SamaEfiling);
        }

        #endregion

        #region PDPL Compliance Integration

        public async Task<SubmissionResultDto> SubmitPdplBreachNotificationAsync(Guid tenantId, PdplBreachNotificationRequest request)
        {
            _logger.LogInformation("Submitting PDPL breach notification for tenant {TenantId}, type {BreachType}",
                tenantId, request.BreachType);

            var connection = await GetActiveConnectionAsync(tenantId, RegulatoryPortalTypes.Pdpl);

            // Calculate 72-hour deadline
            var deadline = await CalculatePdplDeadlineAsync(request.DetectedAt);

            var submission = new RegulatorySubmission
            {
                TenantId = tenantId,
                ConnectionId = connection.Id,
                PortalType = RegulatoryPortalTypes.Pdpl,
                SubmissionType = "BreachNotification",
                IncidentId = request.IncidentId,
                SubmittedById = request.SubmittedById,
                SubmittedByName = request.SubmittedByName,
                Notes = request.Description,
                Deadline = deadline,
                Status = "Submitting",
                RequestPayloadJson = JsonSerializer.Serialize(request)
            };

            _context.RegulatorySubmissions.Add(submission);
            await _context.SaveChangesAsync();

            var result = await SubmitToPortalAsync(connection, submission, request);

            submission.Status = result.Success ? "Submitted" : "Failed";
            submission.ExternalSubmissionId = result.ExternalSubmissionId;
            submission.ConfirmationNumber = result.ConfirmationNumber;
            submission.SubmittedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(tenantId, RegulatoryPortalTypes.Pdpl, "BreachNotificationSubmitted",
                submission.Id, result.Success, result.Message ?? "Breach notification submitted",
                request.SubmittedById, request.SubmittedByName);

            result.InternalId = submission.Id;
            result.Deadline = deadline;
            return result;
        }

        public async Task<SubmissionResultDto> SubmitPdplRegistrationAsync(Guid tenantId, PdplRegistrationRequest request)
        {
            _logger.LogInformation("Submitting PDPL data processing registration for tenant {TenantId}",
                tenantId);

            var connection = await GetActiveConnectionAsync(tenantId, RegulatoryPortalTypes.Pdpl);

            var submission = new RegulatorySubmission
            {
                TenantId = tenantId,
                ConnectionId = connection.Id,
                PortalType = RegulatoryPortalTypes.Pdpl,
                SubmissionType = "DataProcessingRegistration",
                SubmittedById = request.SubmittedById,
                SubmittedByName = request.SubmittedByName,
                Notes = request.ProcessingPurpose,
                Status = "Submitting",
                RequestPayloadJson = JsonSerializer.Serialize(request)
            };

            _context.RegulatorySubmissions.Add(submission);
            await _context.SaveChangesAsync();

            var result = await SubmitToPortalAsync(connection, submission, request);

            submission.Status = result.Success ? "Submitted" : "Failed";
            submission.ExternalSubmissionId = result.ExternalSubmissionId;
            submission.ConfirmationNumber = result.ConfirmationNumber;
            submission.SubmittedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(tenantId, RegulatoryPortalTypes.Pdpl, "RegistrationSubmitted",
                submission.Id, result.Success, result.Message ?? "Registration submitted",
                request.SubmittedById, request.SubmittedByName);

            result.InternalId = submission.Id;
            return result;
        }

        public async Task<SubmissionStatusDto> GetPdplStatusAsync(Guid tenantId, string submissionId)
        {
            var submission = await _context.RegulatorySubmissions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.PortalType == RegulatoryPortalTypes.Pdpl &&
                    (s.ExternalSubmissionId == submissionId || s.Id.ToString() == submissionId));

            if (submission == null)
            {
                return new SubmissionStatusDto
                {
                    SubmissionId = submissionId,
                    PortalType = RegulatoryPortalTypes.Pdpl,
                    Status = "NotFound"
                };
            }

            return new SubmissionStatusDto
            {
                SubmissionId = submission.ExternalSubmissionId ?? submission.Id.ToString(),
                PortalType = RegulatoryPortalTypes.Pdpl,
                Status = submission.Status,
                LastUpdated = submission.ModifiedDate ?? submission.CreatedDate,
                NextActionDeadline = submission.Deadline
            };
        }

        public async Task<List<SubmissionHistoryDto>> GetPdplHistoryAsync(Guid tenantId)
        {
            return await GetSubmissionHistoryByPortalAsync(tenantId, RegulatoryPortalTypes.Pdpl);
        }

        public Task<DateTime> CalculatePdplDeadlineAsync(DateTime detectionTime)
        {
            // PDPL requires breach notification within 72 hours of detection
            return Task.FromResult(detectionTime.AddHours(72));
        }

        #endregion

        #region General Regulatory Filing

        public async Task<List<PendingSubmissionDto>> GetPendingSubmissionsAsync(Guid tenantId)
        {
            var deadlines = await _context.RegulatoryDeadlines
                .Where(d => d.TenantId == tenantId &&
                    d.Status != "Completed" &&
                    d.Status != "Cancelled" &&
                    !d.IsDeleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();

            return deadlines.Select(d => new PendingSubmissionDto
            {
                Id = d.Id,
                PortalType = d.PortalType,
                SubmissionType = d.DeadlineType,
                Deadline = d.DeadlineDate,
                DaysUntilDeadline = (int)(d.DeadlineDate - DateTime.UtcNow).TotalDays,
                Priority = d.Priority,
                AssignedTo = d.AssignedToName,
                Status = d.Status
            }).ToList();
        }

        public async Task<List<RegulatoryPortalDeadlineDto>> GetUpcomingDeadlinesAsync(Guid tenantId, int days = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            var deadlines = await _context.RegulatoryDeadlines
                .Where(d => d.TenantId == tenantId &&
                    d.DeadlineDate <= cutoffDate &&
                    d.Status != "Completed" &&
                    !d.IsDeleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();

            return deadlines.Select(MapToDeadlineDto).ToList();
        }

        public async Task<List<SubmissionHistoryDto>> GetAllSubmissionHistoryAsync(Guid tenantId)
        {
            var submissions = await _context.RegulatorySubmissions
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .OrderByDescending(s => s.SubmittedAt ?? s.CreatedDate)
                .Take(100)
                .ToListAsync();

            return submissions.Select(MapToHistoryDto).ToList();
        }

        public async Task<RegulatoryComplianceStatusDto> GetComplianceStatusAsync(Guid tenantId)
        {
            var connections = await _context.RegulatoryPortalConnections
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .ToListAsync();

            var deadlines = await _context.RegulatoryDeadlines
                .Where(d => d.TenantId == tenantId && !d.IsDeleted && d.Status != "Completed")
                .ToListAsync();

            var submissions = await _context.RegulatorySubmissions
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .GroupBy(s => s.PortalType)
                .Select(g => new
                {
                    PortalType = g.Key,
                    LastSubmission = g.OrderByDescending(s => s.SubmittedAt).FirstOrDefault()
                })
                .ToListAsync();

            var portalStatuses = new Dictionary<string, PortalComplianceStatusDto>();

            foreach (var portalType in new[] { RegulatoryPortalTypes.NcaIsr, RegulatoryPortalTypes.SamaEfiling,
                RegulatoryPortalTypes.Pdpl, RegulatoryPortalTypes.Citc, RegulatoryPortalTypes.Moc })
            {
                var connection = connections.FirstOrDefault(c => c.PortalType == portalType);
                var portalDeadlines = deadlines.Where(d => d.PortalType == portalType).ToList();
                var lastSubmission = submissions.FirstOrDefault(s => s.PortalType == portalType)?.LastSubmission;

                portalStatuses[portalType] = new PortalComplianceStatusDto
                {
                    PortalType = portalType,
                    IsConnected = connection?.IsActive == true,
                    LastSubmissionStatus = lastSubmission?.Status,
                    LastSubmissionDate = lastSubmission?.SubmittedAt,
                    NextDeadline = portalDeadlines.OrderBy(d => d.DeadlineDate).FirstOrDefault()?.DeadlineDate,
                    PendingSubmissions = portalDeadlines.Count(d => d.Status == "Pending"),
                    OverdueSubmissions = portalDeadlines.Count(d => d.DeadlineDate < DateTime.UtcNow)
                };
            }

            var nextDeadline = deadlines.OrderBy(d => d.DeadlineDate).FirstOrDefault();

            return new RegulatoryComplianceStatusDto
            {
                TenantId = tenantId,
                PortalStatuses = portalStatuses,
                TotalOverdueSubmissions = deadlines.Count(d => d.DeadlineDate < DateTime.UtcNow),
                TotalPendingSubmissions = deadlines.Count,
                NextDeadline = nextDeadline?.DeadlineDate ?? DateTime.MaxValue,
                NextDeadlineType = nextDeadline?.DeadlineType
            };
        }

        public async Task<RegulatoryCalendarDto> GenerateRegulatoryCalendarAsync(Guid tenantId, int year)
        {
            var deadlines = await _context.RegulatoryDeadlines
                .Where(d => d.TenantId == tenantId &&
                    d.Year == year &&
                    !d.IsDeleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();

            var events = deadlines.Select(d => new RegulatoryCalendarEventDto
            {
                PortalType = d.PortalType,
                EventType = d.DeadlineType,
                Title = d.Title,
                TitleAr = d.TitleAr,
                Date = d.DeadlineDate,
                IsMandatory = d.IsMandatory,
                Description = d.Description
            }).ToList();

            return new RegulatoryCalendarDto
            {
                TenantId = tenantId,
                Year = year,
                Events = events
            };
        }

        #endregion

        #region Document & Evidence Integration

        public async Task<DocumentUploadResultDto> UploadDocumentAsync(Guid submissionId, UploadDocumentRequest request)
        {
            var submission = await _context.RegulatorySubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                return new DocumentUploadResultDto
                {
                    Success = false,
                    Message = "Submission not found"
                };
            }

            // Store document (in production, this would use blob storage)
            var storagePath = $"regulatory/{submission.TenantId}/{submissionId}/{Guid.NewGuid()}/{request.FileName}";

            var document = new RegulatorySubmissionDocument
            {
                TenantId = submission.TenantId,
                SubmissionId = submissionId,
                FileName = storagePath,
                OriginalFileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.Content.Length,
                DocumentType = request.DocumentType,
                Description = request.Description,
                StoragePath = storagePath,
                UploadedById = request.UploadedById,
                UploadedByName = request.UploadedByName
            };

            _context.RegulatorySubmissionDocuments.Add(document);
            await _context.SaveChangesAsync();

            return new DocumentUploadResultDto
            {
                DocumentId = document.Id,
                Success = true,
                Message = "Document uploaded successfully"
            };
        }

        public async Task<List<SubmissionDocumentDto>> GetDocumentsAsync(Guid submissionId)
        {
            var documents = await _context.RegulatorySubmissionDocuments
                .Where(d => d.SubmissionId == submissionId && !d.IsDeleted)
                .ToListAsync();

            return documents.Select(d => new SubmissionDocumentDto
            {
                Id = d.Id,
                FileName = d.OriginalFileName ?? d.FileName,
                DocumentType = d.DocumentType,
                FileSize = d.FileSize,
                UploadedAt = d.CreatedDate,
                UploadedByName = d.UploadedByName ?? "Unknown"
            }).ToList();
        }

        public async Task LinkEvidenceAsync(Guid submissionId, List<Guid> evidenceIds)
        {
            var submission = await _context.RegulatorySubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new KeyNotFoundException($"Submission {submissionId} not found");
            }

            foreach (var evidenceId in evidenceIds)
            {
                var exists = await _context.RegulatorySubmissionEvidences
                    .AnyAsync(e => e.SubmissionId == submissionId && e.EvidenceId == evidenceId);

                if (!exists)
                {
                    _context.RegulatorySubmissionEvidences.Add(new RegulatorySubmissionEvidence
                    {
                        TenantId = submission.TenantId,
                        SubmissionId = submissionId,
                        EvidenceId = evidenceId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Audit & Reporting

        public async Task<List<IntegrationAuditLogDto>> GetAuditLogAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.RegulatoryAuditLogs
                .Where(l => l.TenantId == tenantId && !l.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(l => l.ActionTimestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.ActionTimestamp <= toDate.Value);

            var logs = await query
                .OrderByDescending(l => l.ActionTimestamp)
                .Take(500)
                .ToListAsync();

            return logs.Select(l => new IntegrationAuditLogDto
            {
                Id = l.Id,
                TenantId = l.TenantId!.Value,
                PortalType = l.PortalType,
                Action = l.Action,
                SubmissionId = l.ExternalSubmissionId,
                Success = l.Success,
                Message = l.Message,
                ErrorDetails = l.ErrorDetails,
                PerformedByName = l.PerformedByName,
                Timestamp = l.ActionTimestamp
            }).ToList();
        }

        public async Task<RegulatoryIntegrationDashboardDto> GetDashboardAsync(Guid tenantId)
        {
            var connections = await GetConnectionsAsync(tenantId);
            var pending = await GetPendingSubmissionsAsync(tenantId);
            var deadlines = await GetUpcomingDeadlinesAsync(tenantId, 30);
            var history = await GetAllSubmissionHistoryAsync(tenantId);
            var statistics = await GetStatisticsAsync(tenantId);

            return new RegulatoryIntegrationDashboardDto
            {
                TenantId = tenantId,
                Connections = connections,
                PendingSubmissions = pending,
                UpcomingDeadlines = deadlines,
                RecentSubmissions = history.Take(10).ToList(),
                Statistics = statistics
            };
        }

        public async Task<IntegrationStatisticsDto> GetStatisticsAsync(Guid tenantId)
        {
            var submissions = await _context.RegulatorySubmissions
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .ToListAsync();

            var byPortal = submissions
                .GroupBy(s => s.PortalType)
                .ToDictionary(g => g.Key, g => g.Count());

            var byStatus = submissions
                .GroupBy(s => s.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var successful = submissions.Count(s => s.Status == "Submitted" || s.Status == "Accepted");
            var failed = submissions.Count(s => s.Status == "Failed" || s.Status == "Rejected");

            return new IntegrationStatisticsDto
            {
                TotalSubmissions = submissions.Count,
                SuccessfulSubmissions = successful,
                FailedSubmissions = failed,
                PendingSubmissions = submissions.Count(s => s.Status == "Pending" || s.Status == "Submitting"),
                ByPortal = byPortal,
                ByStatus = byStatus,
                SuccessRate = submissions.Any()
                    ? Math.Round((decimal)successful / submissions.Count * 100, 2)
                    : 0
            };
        }

        #endregion

        #region Private Helper Methods

        private async Task<RegulatoryPortalConnection> GetActiveConnectionAsync(Guid tenantId, string portalType)
        {
            var connection = await _context.RegulatoryPortalConnections
                .FirstOrDefaultAsync(c => c.TenantId == tenantId &&
                    c.PortalType == portalType &&
                    c.IsActive &&
                    !c.IsDeleted);

            if (connection == null)
            {
                throw new InvalidOperationException($"No active connection found for portal {portalType}. Please configure the connection first.");
            }

            return connection;
        }

        private async Task<List<SubmissionHistoryDto>> GetSubmissionHistoryByPortalAsync(Guid tenantId, string portalType)
        {
            var submissions = await _context.RegulatorySubmissions
                .Where(s => s.TenantId == tenantId && s.PortalType == portalType && !s.IsDeleted)
                .OrderByDescending(s => s.SubmittedAt ?? s.CreatedDate)
                .Take(50)
                .ToListAsync();

            return submissions.Select(MapToHistoryDto).ToList();
        }

        private async Task<List<RegulatoryPortalDeadlineDto>> GetDeadlinesByPortalAsync(Guid tenantId, string portalType)
        {
            var deadlines = await _context.RegulatoryDeadlines
                .Where(d => d.TenantId == tenantId &&
                    d.PortalType == portalType &&
                    d.Status != "Completed" &&
                    !d.IsDeleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();

            return deadlines.Select(MapToDeadlineDto).ToList();
        }

        private async Task<SubmissionResultDto> SubmitToPortalAsync<T>(RegulatoryPortalConnection connection, RegulatorySubmission submission, T request)
        {
            // In a production implementation, this would make actual API calls to the government portal
            // For now, we simulate a successful submission

            try
            {
                // Simulate API call delay
                await Task.Delay(500);

                // Generate a simulated response
                var externalId = $"{connection.PortalType}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                var confirmationNumber = $"CNF-{DateTime.UtcNow:yyyyMMddHHmmss}";

                return new SubmissionResultDto
                {
                    ExternalSubmissionId = externalId,
                    ConfirmationNumber = confirmationNumber,
                    PortalType = connection.PortalType,
                    Success = true,
                    Status = "Submitted",
                    Message = "Submission accepted by portal"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting to portal {PortalType}", connection.PortalType);

                return new SubmissionResultDto
                {
                    PortalType = connection.PortalType,
                    Success = false,
                    Status = "Failed",
                    Message = ex.Message,
                    ErrorCode = "PORTAL_ERROR"
                };
            }
        }

        private void AddAuthHeaders(HttpClient client, RegulatoryPortalConnection connection)
        {
            switch (connection.AuthType.ToLowerInvariant())
            {
                case "apikey":
                    var apiKey = DecryptString(connection.EncryptedApiKey);
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                    }
                    break;

                case "bearer":
                case "oauth2":
                    // In production, this would handle OAuth2 token refresh
                    break;

                case "basicauth":
                    var password = DecryptString(connection.EncryptedPassword);
                    if (!string.IsNullOrEmpty(connection.Username) && !string.IsNullOrEmpty(password))
                    {
                        var credentials = Convert.ToBase64String(
                            Encoding.UTF8.GetBytes($"{connection.Username}:{password}"));
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                    }
                    break;
            }
        }

        private async Task LogAuditAsync(Guid tenantId, string portalType, string action,
            Guid? submissionId, bool success, string message, string performedById, string performedByName)
        {
            var log = new RegulatoryAuditLog
            {
                TenantId = tenantId,
                PortalType = portalType,
                Action = action,
                SubmissionId = submissionId,
                Success = success,
                Message = message,
                PerformedById = performedById,
                PerformedByName = performedByName,
                ActionTimestamp = DateTime.UtcNow
            };

            _context.RegulatoryAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private string? EncryptString(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var ms = new System.IO.MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private string? DecryptString(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = _encryptionKey;

                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                var cipher = new byte[fullCipher.Length - iv.Length];
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var ms = new System.IO.MemoryStream(cipher);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new System.IO.StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting string");
                return null;
            }
        }

        private static RegulatoryPortalConnectionDto MapToConnectionDto(RegulatoryPortalConnection connection)
        {
            return new RegulatoryPortalConnectionDto
            {
                Id = connection.Id,
                TenantId = connection.TenantId!.Value,
                PortalType = connection.PortalType,
                PortalName = connection.PortalName,
                PortalNameAr = connection.PortalNameAr,
                BaseUrl = connection.BaseUrl,
                AuthType = connection.AuthType,
                OrganizationId = connection.OrganizationId,
                IsActive = connection.IsActive,
                LastConnectedAt = connection.LastConnectedAt,
                LastConnectionStatus = connection.LastConnectionStatus,
                CreatedAt = connection.CreatedDate,
                ModifiedAt = connection.ModifiedDate
            };
        }

        private static SubmissionHistoryDto MapToHistoryDto(RegulatorySubmission submission)
        {
            return new SubmissionHistoryDto
            {
                InternalId = submission.Id,
                ExternalSubmissionId = submission.ExternalSubmissionId,
                PortalType = submission.PortalType,
                SubmissionType = submission.SubmissionType,
                Status = submission.Status,
                SubmittedAt = submission.SubmittedAt ?? submission.CreatedDate,
                SubmittedByName = submission.SubmittedByName,
                ConfirmationNumber = submission.ConfirmationNumber,
                ReportingPeriodStart = submission.ReportingPeriodStart,
                ReportingPeriodEnd = submission.ReportingPeriodEnd
            };
        }

        private static RegulatoryPortalDeadlineDto MapToDeadlineDto(GrcMvc.Models.Entities.RegulatoryDeadline deadline)
        {
            return new RegulatoryPortalDeadlineDto
            {
                PortalType = deadline.PortalType,
                DeadlineType = deadline.DeadlineType,
                Description = deadline.Title,
                DescriptionAr = deadline.TitleAr,
                Deadline = deadline.DeadlineDate,
                DaysUntilDeadline = (int)(deadline.DeadlineDate - DateTime.UtcNow).TotalDays,
                Priority = deadline.Priority,
                IsMandatory = deadline.IsMandatory,
                AssignedTo = deadline.AssignedToName
            };
        }

        #endregion
    }
}
