# Shahin AI - Blazor Component Map (GrcMvc Architecture)

## Architecture Note
This system uses **ASP.NET Core MVC + Blazor Server** (not ABP Framework). Components follow the existing `GrcMvc` namespace and structure.

## Component Structure

```
src/GrcMvc/
├── Components/
│   ├── Pages/              # Routed pages (@page directive)
│   │   ├── Dashboard/
│   │   ├── Assessments/
│   │   ├── ControlAssessments/
│   │   ├── Evidence/
│   │   ├── Risks/
│   │   ├── ActionPlans/
│   │   ├── Policies/
│   │   ├── Audits/
│   │   └── Reports/
│   └── Shared/             # Reusable widgets/components
│       ├── Widgets/        # Dashboard widgets
│       ├── Dialogs/        # Modal dialogs
│       └── Forms/          # Form components
```

---

## A. Dashboard Page

**File:** `Components/Pages/Dashboard/Index.razor`

**Route:** `/dashboard`

**Widgets:**

### 1. ComplianceHealthScoreCard.razor
**Location:** `Components/Shared/Widgets/ComplianceHealthScoreCard.razor`

**API Endpoint:**
```csharp
GET /api/dashboard/overview
```

**Service:**
```csharp
// Services/Interfaces/IDashboardService.cs
Task<DashboardOverviewDto> GetOverviewAsync();
```

**DTO:**
```csharp
public class DashboardOverviewDto
{
    public int ComplianceScore { get; set; } // 0-100
    public string Trend { get; set; } // "up", "down", "stable"
    public DateTime LastCalculatedAt { get; set; }
    public int TotalControls { get; set; }
    public int ImplementedControls { get; set; }
}
```

**Component Code:**
```razor
@inject IDashboardService DashboardService

<div class="card">
    <div class="card-body text-center">
        <h5 class="card-title">Compliance Health Score</h5>
        <div class="display-4 fw-bold @GetScoreColor(overview.ComplianceScore)">
            @overview.ComplianceScore%
        </div>
        <p class="text-muted">
            <i class="bi bi-arrow-@(overview.Trend == "up" ? "up" : overview.Trend == "down" ? "down" : "right")"></i>
            @overview.Trend
        </p>
        <small class="text-muted">Last updated: @overview.LastCalculatedAt.ToString("g")</small>
    </div>
</div>

@code {
    private DashboardOverviewDto? overview;

    protected override async Task OnInitializedAsync()
    {
        overview = await DashboardService.GetOverviewAsync();
    }

    private string GetScoreColor(int score) => score >= 80 ? "text-success" : score >= 60 ? "text-warning" : "text-danger";
}
```

### 2. ControlsStatusSummaryWidget.razor
**Location:** `Components/Shared/Widgets/ControlsStatusSummaryWidget.razor`

**API:** `GET /api/dashboard/control-status-summary`

**Shows:** Counts by status (Not Implemented, Partially Implemented, Implemented) with click-through filters

### 3. OverdueActionPlansWidget.razor
**Location:** `Components/Shared/Widgets/OverdueActionPlansWidget.razor`

**API:** `GET /api/action-plans?status=Overdue&take=10`

**Shows:** List with owner, due date, linked control/risk

### 4. HighRiskItemsWidget.razor
**Location:** `Components/Shared/Widgets/HighRiskItemsWidget.razor`

**API:** `GET /api/risks/high?take=10`

**Shows:** Top risks by residual score

### 5. UpcomingAuditsWidget.razor
**Location:** `Components/Shared/Widgets/UpcomingAuditsWidget.razor`

**API:** `GET /api/audits/upcoming?days=60`

**Shows:** Upcoming audits with due artifacts

---

## B. Assessments Module

### Pages

#### 1. AssessmentsList.razor
**File:** `Components/Pages/Assessments/Index.razor`
**Route:** `/assessments`

**Widgets:**
- `AssessmentFiltersBar.razor` - Status, framework/regulator, owner, date range filters
- `AssessmentListGrid.razor` - Paged list with Create, View, Generate from Template actions

**API:** `GET /api/assessments?page=1&pageSize=20&status=...&frameworkId=...`

#### 2. AssessmentDetails.razor
**File:** `Components/Pages/Assessments/Detail.razor` (create if missing)
**Route:** `/assessments/{id}`

**Widgets:**
- `AssessmentHeader.razor` - Status badge + primary actions (Submit, Approve)
- `AssessmentTabs.razor` - Tabs: Summary, Control Assessments, Evidence, Action Plans, History

### Critical Widget: GenerateFromTemplateDialog.razor

**Location:** `Components/Shared/Dialogs/GenerateFromTemplateDialog.razor`

**API:**
```csharp
POST /api/assessments/generate
Request: {
    "templateId": "guid",
    "scope": "regulator|framework",
    "regulatorId": "guid?",
    "frameworkId": "guid?",
    "ownerId": "string"
}
Response: {
    "assessmentId": "guid",
    "controlAssessmentsCreated": 42
}
```

**Component:**
```razor
@inject IAssessmentService AssessmentService
@inject ILogger<GenerateFromTemplateDialog> Logger

<Modal Title="Generate Assessment from Template" @ref="modal">
    <EditForm Model="@request" OnValidSubmit="HandleGenerate">
        <DataAnnotationsValidator />
        
        <div class="mb-3">
            <label class="form-label">Template</label>
            <InputSelect @bind-Value="request.TemplateId" class="form-select">
                <option value="">Select template...</option>
                @foreach (var template in templates)
                {
                    <option value="@template.Id">@template.Name</option>
                }
            </InputSelect>
        </div>

        <div class="mb-3">
            <label class="form-label">Scope</label>
            <InputSelect @bind-Value="request.Scope" class="form-select">
                <option value="regulator">Regulator</option>
                <option value="framework">Framework</option>
            </InputSelect>
        </div>

        @if (request.Scope == "regulator")
        {
            <div class="mb-3">
                <label class="form-label">Regulator</label>
                <InputSelect @bind-Value="request.RegulatorId" class="form-select">
                    <!-- Populate from regulators list -->
                </InputSelect>
            </div>
        }

        <div class="d-flex justify-content-end gap-2">
            <button type="button" class="btn btn-secondary" @onclick="modal.Hide">Cancel</button>
            <button type="submit" class="btn btn-primary" disabled="@isGenerating">
                @if (isGenerating)
                {
                    <span class="spinner-border spinner-border-sm me-2"></span>
                }
                Generate Assessment
            </button>
        </div>
    </EditForm>
</Modal>

@code {
    private Modal? modal;
    private bool isGenerating = false;
    private GenerateAssessmentRequest request = new();
    private List<AssessmentTemplateDto> templates = new();

    protected override async Task OnInitializedAsync()
    {
        templates = await AssessmentService.GetTemplatesAsync();
    }

    private async Task HandleGenerate()
    {
        isGenerating = true;
        try
        {
            var result = await AssessmentService.GenerateFromTemplateAsync(request);
            // Show success message, navigate to assessment detail
            await modal!.Hide();
            NavigationManager.NavigateTo($"/assessments/{result.AssessmentId}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate assessment");
            // Show error message
        }
        finally
        {
            isGenerating = false;
        }
    }
}
```

---

## C. Control Assessments Module

### Page: ControlAssessments.razor
**File:** `Components/Pages/ControlAssessments/Index.razor` (create)
**Route:** `/control-assessments`

**Widgets:**

#### 1. ControlAssessmentGrid.razor
**API:** `GET /api/control-assessments?page=1&pageSize=20&assessmentId=...&status=...`

#### 2. StatusUpdateDialog.razor
**API:** `PUT /api/control-assessments/{id}/status`
**Validation:** Implemented status requires evidence

```razor
@inject IControlAssessmentService Service

<Modal Title="Update Control Status" @ref="modal">
    <EditForm Model="@updateRequest" OnValidSubmit="HandleUpdate">
        <div class="mb-3">
            <label class="form-label">Status</label>
            <InputSelect @bind-Value="updateRequest.Status" class="form-select">
                <option value="NotStarted">Not Started</option>
                <option value="PartiallyImplemented">Partially Implemented</option>
                <option value="Implemented">Implemented</option>
                <option value="NotImplemented">Not Implemented</option>
            </InputSelect>
        </div>

        @if (updateRequest.Status == "Implemented")
        {
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle"></i>
                Evidence is required to mark a control as Implemented.
            </div>
            <div class="mb-3">
                <label class="form-label">Evidence</label>
                <EvidenceSelector @bind-SelectedEvidenceIds="updateRequest.EvidenceIds" />
            </div>
        }

        <button type="submit" class="btn btn-primary">Update Status</button>
    </EditForm>
</Modal>
```

#### 3. EvidenceLinkDrawer.razor
**API:** `POST /api/evidence/link`
**Links evidence to control assessments**

---

## D. Evidence Module

### Page: Evidence.razor
**File:** `Components/Pages/Evidence/Index.razor`
**Route:** `/evidence`

**Widgets:**

#### 1. EvidenceUploadPanel.razor
**Location:** `Components/Shared/Widgets/EvidenceUploadPanel.razor`

**API:** `POST /api/evidence/upload` (multipart/form-data)

**Stores to:** Azure Blob Storage / S3 / Local file system + metadata in database

**Component:**
```razor
@inject IEvidenceService EvidenceService
@inject ILogger<EvidenceUploadPanel> Logger

<div class="card">
    <div class="card-body">
        <h5 class="card-title">Upload Evidence</h5>
        
        <InputFile OnChange="HandleFileSelected" class="form-control" />
        
        <div class="mt-3">
            <label class="form-label">Link to</label>
            <InputSelect @bind-Value="linkToEntityType" class="form-select">
                <option value="ControlAssessment">Control Assessment</option>
                <option value="ActionPlan">Action Plan</option>
                <option value="Audit">Audit</option>
            </InputSelect>
        </div>

        <div class="mt-3">
            <label class="form-label">Entity ID</label>
            <InputText @bind-Value="linkToEntityId" class="form-control" />
        </div>

        <button class="btn btn-primary mt-3" @onclick="HandleUpload" disabled="@isUploading">
            @if (isUploading)
            {
                <span class="spinner-border spinner-border-sm me-2"></span>
            }
            Upload Evidence
        </button>
    </div>
</div>

@code {
    private IBrowserFile? selectedFile;
    private bool isUploading = false;
    private string linkToEntityType = "ControlAssessment";
    private string linkToEntityId = string.Empty;

    private void HandleFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
    }

    private async Task HandleUpload()
    {
        if (selectedFile == null) return;

        isUploading = true;
        try
        {
            var request = new UploadEvidenceRequest
            {
                File = selectedFile,
                EntityType = linkToEntityType,
                EntityId = Guid.Parse(linkToEntityId),
                Description = $"Uploaded via UI on {DateTime.UtcNow}"
            };

            var result = await EvidenceService.UploadAsync(request);
            // Show success, refresh list
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to upload evidence");
        }
        finally
        {
            isUploading = false;
        }
    }
}
```

#### 2. EvidenceListGrid.razor
**API:** `GET /api/evidence?page=1&pageSize=20&entityType=...&entityId=...`

#### 3. EvidencePreviewModal.razor
**API:** `GET /api/evidence/{id}/download`

---

## E. Risks Module

### Page: Risks.razor
**File:** `Components/Pages/Risks/Index.razor`
**Route:** `/risks`

**Widgets:**

#### 1. RiskRegisterGrid.razor
**API:** `GET /api/risks?page=1&pageSize=20`

#### 2. RiskMatrixWidget.razor
**API:** `GET /api/risks/matrix`
**Shows:** Heatmap with risk distribution

#### 3. TreatmentSelector.razor
**Options:** Mitigate, Accept, Transfer, Avoid

#### 4. ResidualRiskCalculatorPanel.razor
**API:** `POST /api/risks/{id}/calculate-residual`
**Calculates:** Residual risk after treatment

---

## F. Action Plans Module

### Page: ActionPlans.razor
**File:** `Components/Pages/ActionPlans/Index.razor` (create)
**Route:** `/action-plans`

**Widgets:**

#### 1. ActionPlanGrid.razor
**API:** `GET /api/action-plans?page=1&pageSize=20&status=...&ownerId=...`

#### 2. ActionPlanFormDialog.razor
**API:** `POST /api/action-plans` (Create), `PUT /api/action-plans/{id}` (Update)

**Fields:**
- Title, Description
- Due Date
- Owner (User ID)
- Linked Control Assessment / Risk
- Status: Open, In Progress, Completed, Overdue

#### 3. CloseActionPlanDialog.razor
**API:** `PUT /api/action-plans/{id}/close`
**Requires:** Closure evidence (optional but recommended)

---

## G. Policies Module

### Page: Policies.razor
**File:** `Components/Pages/Policies/Index.razor`
**Route:** `/policies`

**Widgets:**

#### 1. PolicyLibraryGrid.razor
**API:** `GET /api/policies?page=1&pageSize=20`

#### 2. PolicyEditor.razor
**Fields:** Metadata, attachment upload, linked controls

#### 3. VersionTimeline.razor
**API:** `GET /api/policies/{id}/versions`
**Shows:** Version history (v1, v2, v3...)

---

## H. Reports Module

### Page: Reports.razor
**File:** `Components/Pages/Reports/Index.razor`
**Route:** `/reports`

**Widgets:**

#### 1. ReportTypeSelector.razor
**Options:** Compliance Report, Audit Pack, Gap Analysis, Evidence Index

#### 2. ReportParametersForm.razor
**Parameters:** Date range, framework/regulator, status filters

#### 3. GenerateReportButton
**API:** `POST /api/reports/generate`
**Request:** Report type + parameters
**Response:** Report ID (for download)

#### 4. ExportButtons
**API:** `GET /api/reports/{id}/export?format=pdf|xlsx`

---

## Service Interfaces Required

```csharp
// Services/Interfaces/IDashboardService.cs
public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync();
    Task<ControlStatusSummaryDto> GetControlStatusSummaryAsync();
}

// Services/Interfaces/IAssessmentService.cs
public interface IAssessmentService
{
    Task<PagedResult<AssessmentDto>> GetAssessmentsAsync(AssessmentFilterDto filter);
    Task<AssessmentDto> GetByIdAsync(Guid id);
    Task<GenerateAssessmentResponse> GenerateFromTemplateAsync(GenerateAssessmentRequest request);
    Task<List<AssessmentTemplateDto>> GetTemplatesAsync();
}

// Services/Interfaces/IControlAssessmentService.cs
public interface IControlAssessmentService
{
    Task<PagedResult<ControlAssessmentDto>> GetControlAssessmentsAsync(ControlAssessmentFilterDto filter);
    Task UpdateStatusAsync(Guid id, UpdateControlStatusRequest request);
    Task LinkEvidenceAsync(Guid controlAssessmentId, Guid evidenceId);
}

// Services/Interfaces/IEvidenceService.cs
public interface IEvidenceService
{
    Task<EvidenceDto> UploadAsync(UploadEvidenceRequest request);
    Task<PagedResult<EvidenceDto>> GetEvidenceAsync(EvidenceFilterDto filter);
    Task<Stream> DownloadAsync(Guid id);
    Task LinkToEntityAsync(Guid evidenceId, string entityType, Guid entityId);
}

// Services/Interfaces/IActionPlanService.cs
public interface IActionPlanService
{
    Task<PagedResult<ActionPlanDto>> GetActionPlansAsync(ActionPlanFilterDto filter);
    Task<ActionPlanDto> CreateAsync(CreateActionPlanDto dto);
    Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto dto);
    Task CloseAsync(Guid id, CloseActionPlanRequest request);
}
```

---

## Implementation Priority

**Phase 1 (Critical):**
1. EvidenceUploadPanel + EvidenceService (file storage)
2. ActionPlanFormDialog + ActionPlanService (CRUD)
3. StatusUpdateDialog + ControlAssessmentService (status + evidence linking)
4. GenerateFromTemplateDialog + AssessmentService (template generation)

**Phase 2:**
5. RiskMatrixWidget (enable matrix)
6. ResidualRiskCalculatorPanel
7. PolicyEditor + versioning
8. Report generation (real PDF/Excel)

**Phase 3:**
9. Replace all setTimeout mocks
10. Add pagination to all grids
11. Bulk operations
12. Notifications & preferences
