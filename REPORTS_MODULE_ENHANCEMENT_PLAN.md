# Reports Module Enhancement Plan

## Overview
This document outlines the detailed implementation plan for completing the Reports module with PDF/Excel generation, file storage, user context integration, and CRUD operations.

## 1. PDF/Excel File Generation Implementation

### 1.1 Technology Selection

#### Option A: QuestPDF (Recommended for PDF)
- **License**: Community license available for companies with revenue < $1M
- **Pros**:
  - Fluent API, easy to use
  - Great documentation
  - No external dependencies
  - Cross-platform
- **Installation**:
  ```bash
  dotnet add package QuestPDF
  ```

#### Option B: iTextSharp / iText 7
- **License**: AGPL (open source) or Commercial
- **Pros**: Industry standard, feature-rich
- **Cons**: Complex licensing, steeper learning curve

#### For Excel: ClosedXML (Recommended)
- **License**: MIT (free)
- **Pros**:
  - Easy to use
  - No Excel installation required
  - Good performance
- **Installation**:
  ```bash
  dotnet add package ClosedXML
  ```

### 1.2 Implementation Structure

```csharp
// New Interface: IReportGenerator
public interface IReportGenerator
{
    Task<byte[]> GeneratePdfAsync(Report report, ReportData data);
    Task<byte[]> GenerateExcelAsync(Report report, ReportData data);
}

// New Service: ReportGeneratorService
public class ReportGeneratorService : IReportGenerator
{
    // PDF Generation using QuestPDF
    public async Task<byte[]> GeneratePdfAsync(Report report, ReportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Header().Text(report.Title);
                // ... build PDF content
            });
        }).GeneratePdf();
    }

    // Excel Generation using ClosedXML
    public async Task<byte[]> GenerateExcelAsync(Report report, ReportData data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");
        // ... build Excel content

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
```

### 1.3 Report Templates

Create template classes for each report type:

```csharp
// Base template
public abstract class ReportTemplate
{
    public abstract Task<Document> CreateDocument(Report report, object data);
    public abstract Task<IXLWorkbook> CreateWorkbook(Report report, object data);
}

// Specific templates
public class RiskReportTemplate : ReportTemplate { }
public class ComplianceReportTemplate : ReportTemplate { }
public class AuditReportTemplate : ReportTemplate { }
public class ControlReportTemplate : ReportTemplate { }
```

### 1.4 Implementation Tasks

1. **Install NuGet packages**
   ```bash
   cd src/GrcMvc
   dotnet add package QuestPDF
   dotnet add package ClosedXML
   ```

2. **Create Report Generator Service**
   - File: `Services/Implementations/ReportGeneratorService.cs`
   - Interface: `Services/Interfaces/IReportGenerator.cs`

3. **Create Report Templates**
   - Directory: `Services/ReportTemplates/`
   - Files: `RiskReportTemplate.cs`, `ComplianceReportTemplate.cs`, etc.

4. **Update ReportService**
   - Inject IReportGenerator
   - Generate actual files in generation methods

5. **Register Services in Program.cs**
   ```csharp
   builder.Services.AddScoped<IReportGenerator, ReportGeneratorService>();
   ```

## 2. File Storage Implementation

### 2.1 Storage Strategy

#### Local Storage (Phase 1 - Immediate Implementation)
```csharp
public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] content, string fileName, string contentType);
    Task<byte[]> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> GetFileUrlAsync(string filePath);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _basePath = Path.Combine(_environment.WebRootPath, "reports");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string contentType)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, content);

        return $"reports/{uniqueFileName}";
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        return await File.ReadAllBytesAsync(fullPath);
    }
}
```

#### Cloud Storage (Phase 2 - Future Enhancement)
```csharp
// Azure Blob Storage implementation
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(
            configuration["AzureStorage:ConnectionString"]);
    }

    // Implementation for Azure Blob Storage
}
```

### 2.2 File Management

```csharp
// Update Report entity to track file metadata
public class Report
{
    // Existing properties...

    public string? FilePath { get; set; }  // Internal storage path
    public string? FileName { get; set; }  // Original file name
    public string? ContentType { get; set; } // application/pdf or application/vnd.ms-excel
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }  // SHA256 for integrity
}
```

### 2.3 Implementation Tasks

1. **Create Storage Service**
   - File: `Services/Implementations/LocalFileStorageService.cs`
   - Interface: `Services/Interfaces/IFileStorageService.cs`

2. **Create Reports Directory**
   - Path: `wwwroot/reports/`
   - Add to `.gitignore`: `/wwwroot/reports/*`

3. **Update Database Schema**
   ```csharp
   // Add migration for new file fields
   dotnet ef migrations add AddFileFieldsToReport -s . -p .
   ```

4. **Implement File Cleanup**
   - Background job to remove orphaned files
   - Archive old reports

## 3. User/Tenant Context Integration

### 3.1 Current User Service

```csharp
public interface ICurrentUserService
{
    Guid GetUserId();
    string GetUserName();
    string GetUserEmail();
    Guid GetTenantId();
    List<string> GetRoles();
    bool IsInRole(string role);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : Guid.Empty;
    }

    public Guid GetTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst("TenantId")?.Value;

        return Guid.TryParse(tenantIdClaim, out var tenantId)
            ? tenantId
            : Guid.Empty;
    }

    public string GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name
            ?? "System";
    }
}
```

### 3.2 Update ReportService

```csharp
public class ReportService : IReportService
{
    private readonly ICurrentUserService _currentUser;

    public ReportService(
        IUnitOfWork unitOfWork,
        IAuditEventService auditService,
        ICurrentUserService currentUser,
        ILogger<ReportService> logger)
    {
        _currentUser = currentUser;
        // ... other dependencies
    }

    public async Task<(string reportId, string filePath)> GenerateComplianceReportAsync(
        DateTime startDate, DateTime endDate)
    {
        var tenantId = _currentUser.GetTenantId();
        var userName = _currentUser.GetUserName();

        var report = new Report
        {
            TenantId = tenantId,
            GeneratedBy = userName,
            // ... rest of report creation
        };
    }
}
```

### 3.3 Tenant Filtering

```csharp
// Add tenant filtering to all queries
public class TenantFilterService
{
    private readonly ICurrentUserService _currentUser;

    public IQueryable<T> ApplyTenantFilter<T>(IQueryable<T> query)
        where T : BaseEntity
    {
        var tenantId = _currentUser.GetTenantId();
        return query.Where(e => e.TenantId == tenantId);
    }
}
```

### 3.4 Implementation Tasks

1. **Create Current User Service**
   - File: `Services/Implementations/CurrentUserService.cs`
   - Interface: `Services/Interfaces/ICurrentUserService.cs`

2. **Update All Services**
   - Inject ICurrentUserService
   - Replace hardcoded "System" and Guid.Empty

3. **Add Authorization**
   ```csharp
   [Authorize]
   [ApiController]
   public class ReportController : ControllerBase
   {
       // Existing code...
   }
   ```

4. **Register Services**
   ```csharp
   builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
   ```

## 4. Complete Update/Delete Operations

### 4.1 Update Operation

```csharp
// In ReportService.cs
public async Task<Report> UpdateReportAsync(Guid reportId, ReportEditDto dto)
{
    var report = await _unitOfWork.Reports.GetByIdAsync(reportId);

    if (report == null || report.IsDeleted)
        throw new EntityNotFoundException($"Report {reportId} not found");

    // Verify tenant access
    if (report.TenantId != _currentUser.GetTenantId())
        throw new UnauthorizedAccessException();

    // Update properties
    report.Title = dto.Title;
    report.Description = dto.Description;
    report.ExecutiveSummary = dto.ExecutiveSummary;
    report.KeyFindings = dto.KeyFindings;
    report.Recommendations = dto.Recommendations;
    report.DeliveredTo = dto.DeliveredTo;
    report.DeliveryDate = dto.DeliveryDate;
    report.Status = dto.Status;
    report.ModifiedDate = DateTime.UtcNow;
    report.ModifiedBy = _currentUser.GetUserName();

    await _unitOfWork.Reports.UpdateAsync(report);
    await _unitOfWork.SaveChangesAsync();

    // Audit log
    await _auditService.LogEventAsync(
        tenantId: report.TenantId,
        eventType: "ReportUpdated",
        affectedEntityType: "Report",
        affectedEntityId: report.Id.ToString(),
        action: "Update",
        actor: _currentUser.GetUserName(),
        payloadJson: JsonSerializer.Serialize(dto),
        correlationId: report.CorrelationId
    );

    return report;
}
```

### 4.2 Delete Operation (Soft Delete)

```csharp
// In ReportService.cs
public async Task<bool> DeleteReportAsync(Guid reportId)
{
    var report = await _unitOfWork.Reports.GetByIdAsync(reportId);

    if (report == null || report.IsDeleted)
        return false;

    // Verify tenant access
    if (report.TenantId != _currentUser.GetTenantId())
        throw new UnauthorizedAccessException();

    // Soft delete
    report.IsDeleted = true;
    report.DeletedDate = DateTime.UtcNow;
    report.DeletedBy = _currentUser.GetUserName();

    await _unitOfWork.Reports.UpdateAsync(report);
    await _unitOfWork.SaveChangesAsync();

    // Audit log
    await _auditService.LogEventAsync(
        tenantId: report.TenantId,
        eventType: "ReportDeleted",
        affectedEntityType: "Report",
        affectedEntityId: report.Id.ToString(),
        action: "Delete",
        actor: _currentUser.GetUserName(),
        correlationId: report.CorrelationId
    );

    return true;
}

// Optional: Hard delete with file cleanup
public async Task<bool> PermanentlyDeleteReportAsync(Guid reportId)
{
    var report = await _unitOfWork.Reports.GetByIdAsync(reportId);

    if (report == null)
        return false;

    // Delete associated file
    if (!string.IsNullOrEmpty(report.FilePath))
    {
        await _fileStorage.DeleteFileAsync(report.FilePath);
    }

    // Hard delete from database
    await _unitOfWork.Reports.DeleteAsync(report);
    await _unitOfWork.SaveChangesAsync();

    return true;
}
```

### 4.3 Update Controller

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<ReportDetailDto>> UpdateReport(
    string id, [FromBody] ReportEditDto dto)
{
    try
    {
        if (!Guid.TryParse(id, out var reportId))
            return BadRequest(new { error = "Invalid report ID" });

        var report = await _reportService.UpdateReportAsync(reportId, dto);

        var reportDto = _mapper.Map<ReportDetailDto>(report);
        return Ok(reportDto);
    }
    catch (EntityNotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException)
    {
        return Forbid();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating report {ReportId}", id);
        return StatusCode(500, new { error = "Failed to update report" });
    }
}

[HttpDelete("{id}")]
public async Task<ActionResult> DeleteReport(string id)
{
    try
    {
        if (!Guid.TryParse(id, out var reportId))
            return BadRequest(new { error = "Invalid report ID" });

        var deleted = await _reportService.DeleteReportAsync(reportId);

        if (!deleted)
            return NotFound(new { error = "Report not found" });

        return NoContent();
    }
    catch (UnauthorizedAccessException)
    {
        return Forbid();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting report {ReportId}", id);
        return StatusCode(500, new { error = "Failed to delete report" });
    }
}
```

## 5. Implementation Roadmap

### Phase 1: Foundation (Week 1)
**Priority: HIGH**

1. **Day 1-2: User/Tenant Context**
   - [ ] Create ICurrentUserService interface
   - [ ] Implement CurrentUserService
   - [ ] Update ReportService to use current user context
   - [ ] Add authorization to controllers
   - [ ] Test with mock authentication

2. **Day 3-4: Complete CRUD Operations**
   - [ ] Implement UpdateReportAsync in ReportService
   - [ ] Implement DeleteReportAsync (soft delete)
   - [ ] Update ReportController endpoints
   - [ ] Add error handling and logging
   - [ ] Write unit tests

3. **Day 5: Local File Storage**
   - [ ] Create IFileStorageService interface
   - [ ] Implement LocalFileStorageService
   - [ ] Create reports directory structure
   - [ ] Add file management methods
   - [ ] Test file upload/download

### Phase 2: PDF/Excel Generation (Week 2)
**Priority: HIGH**

1. **Day 6-7: Setup and Base Templates**
   - [ ] Install QuestPDF and ClosedXML packages
   - [ ] Create IReportGenerator interface
   - [ ] Implement base ReportGeneratorService
   - [ ] Create ReportTemplate base class
   - [ ] Implement basic PDF structure

2. **Day 8-9: Report Templates**
   - [ ] Create RiskReportTemplate
   - [ ] Create ComplianceReportTemplate
   - [ ] Create AuditReportTemplate
   - [ ] Create ControlReportTemplate
   - [ ] Add charts and formatting

3. **Day 10: Integration**
   - [ ] Update ReportService to generate files
   - [ ] Store generated files
   - [ ] Update download endpoint
   - [ ] Test end-to-end flow
   - [ ] Performance optimization

### Phase 3: Advanced Features (Week 3)
**Priority: MEDIUM**

1. **Day 11-12: Excel Export**
   - [ ] Implement Excel generation
   - [ ] Add data tables and formatting
   - [ ] Create pivot tables for analysis
   - [ ] Test with large datasets

2. **Day 13: Scheduling and Automation**
   - [ ] Create report scheduling service
   - [ ] Add background job for generation
   - [ ] Implement email delivery
   - [ ] Add report subscriptions

3. **Day 14-15: Polish and Testing**
   - [ ] Add report preview functionality
   - [ ] Implement report versioning
   - [ ] Add bulk operations
   - [ ] Comprehensive testing
   - [ ] Documentation update

### Phase 4: Cloud Storage (Future)
**Priority: LOW**

1. **Azure Blob Storage Integration**
   - [ ] Install Azure.Storage.Blobs package
   - [ ] Implement AzureBlobStorageService
   - [ ] Add configuration for storage account
   - [ ] Migrate existing files
   - [ ] Test failover scenarios

## 6. Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class ReportServiceTests
{
    [TestMethod]
    public async Task GenerateComplianceReport_ShouldCreatePdfFile()
    {
        // Arrange
        var reportService = new ReportService(/* mocked dependencies */);

        // Act
        var (reportId, filePath) = await reportService.GenerateComplianceReportAsync(
            DateTime.Now.AddMonths(-1),
            DateTime.Now);

        // Assert
        Assert.IsNotNull(reportId);
        Assert.IsTrue(File.Exists(filePath));
    }
}
```

### Integration Tests
- Test file storage operations
- Test PDF generation
- Test database operations
- Test API endpoints

### Performance Tests
- Large report generation (>100 pages)
- Concurrent report generation
- File storage limits

## 7. Configuration

### appsettings.json
```json
{
  "FileStorage": {
    "Type": "Local",
    "LocalPath": "wwwroot/reports",
    "MaxFileSizeInMB": 50,
    "AllowedExtensions": [".pdf", ".xlsx", ".xls"],
    "RetentionDays": 90
  },
  "ReportGeneration": {
    "EnablePdfGeneration": true,
    "EnableExcelGeneration": true,
    "DefaultPageSize": "A4",
    "DefaultOrientation": "Portrait",
    "CompanyLogo": "/images/logo.png",
    "WatermarkText": "CONFIDENTIAL"
  }
}
```

## 8. Dependencies to Add

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <!-- PDF Generation -->
    <PackageReference Include="QuestPDF" Version="2024.3.0" />

    <!-- Excel Generation -->
    <PackageReference Include="ClosedXML" Version="0.102.1" />

    <!-- Azure Storage (Future) -->
    <!-- <PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" /> -->

    <!-- Additional Utilities -->
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
  </ItemGroup>
</Project>
```

## 9. Success Criteria

### Functional Requirements
- ✅ Generate PDF reports for all report types
- ✅ Generate Excel reports with data tables
- ✅ Store files securely with proper access control
- ✅ Track user and tenant context
- ✅ Complete CRUD operations with audit trail
- ✅ Download reports in multiple formats

### Non-Functional Requirements
- ✅ Generate 50-page report in < 5 seconds
- ✅ Support concurrent report generation (10+ simultaneous)
- ✅ File size limit enforcement
- ✅ Automatic cleanup of old reports
- ✅ 99.9% availability for report generation

## 10. Risk Mitigation

### Risks and Mitigations

1. **Risk**: Large report generation blocking server
   - **Mitigation**: Use background jobs (Hangfire/Quartz.NET)

2. **Risk**: Storage space exhaustion
   - **Mitigation**: Implement retention policy and archival

3. **Risk**: PDF library licensing issues
   - **Mitigation**: Use QuestPDF with community license

4. **Risk**: Memory issues with large Excel files
   - **Mitigation**: Stream data, use SAX parsing

5. **Risk**: Concurrent access conflicts
   - **Mitigation**: Use unique file names, implement locking

## Conclusion

This plan provides a comprehensive roadmap for enhancing the Reports module with proper file generation, storage, user context, and complete CRUD operations. The phased approach allows for incremental delivery while maintaining system stability.

Estimated Total Time: 3 weeks for core features, additional time for advanced features and cloud storage migration.