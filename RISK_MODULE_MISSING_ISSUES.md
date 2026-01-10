# Risk Module - Missing Issues & Implementation Gaps

**Document Date:** January 10, 2026  
**Last Updated:** During GRC System Audit  
**Status:** Complete List of Identified Gaps

---

## Executive Summary

The Risk Module has the following implementation gaps:

| Category | Count | Priority | Status |
|----------|-------|----------|--------|
| **Missing Views** | 6 | 🔴 CRITICAL | Not Implemented |
| **Missing API Endpoints** | 8 | 🔴 CRITICAL | Not Implemented |
| **Missing Workflows** | 3 | 🔴 CRITICAL | Not Implemented |
| **Missing Features** | 5 | 🟠 HIGH | Not Implemented |
| **Database Isolation** | 1 | 🟠 HIGH | Pending |
| **Validation Rules** | 4 | 🟡 MEDIUM | Partial |
| **Integrations** | 3 | 🟡 MEDIUM | Stub Only |

---

## 1. MISSING VIEWS (6 Views) 🔴 CRITICAL

### Location
Expected in: `/Areas/Risk/Views/Risk/`

### Missing View Files

| View | Purpose | Required Fields | Status |
|------|---------|-----------------|--------|
| **Index.cshtml** | Risk list/grid with filtering | ID, Name, Description, Level, Status, Owner, DueDate | ❌ MISSING |
| **Details.cshtml** | Risk detail view | Full risk data + history + related controls | ❌ MISSING |
| **Create.cshtml** | Risk creation form | Name, Description, Category, Owner, RiskLevel, Probability, Impact | ❌ MISSING |
| **Edit.cshtml** | Risk edit form | Same as Create + ID | ❌ MISSING |
| **Delete.cshtml** | Risk deletion confirmation | Risk details + warning | ❌ MISSING |
| **Statistics.cshtml** | Risk statistics dashboard | Charts, heatmaps, trends | ❌ MISSING |

### Implementation Requirements

```csharp
// Controller Actions Already Exist (in RiskController.cs)
[HttpGet] public IActionResult Index() { }           // Returns NotFound (no view)
[HttpGet] public IActionResult Details(Guid id) { } // Returns NotFound (no view)
[HttpGet] public IActionResult Create() { }         // Returns NotFound (no view)
[HttpPost] public async Task<IActionResult> Create(CreateRiskDto dto) { } // Functional
[HttpGet] public IActionResult Edit(Guid id) { }    // Returns NotFound (no view)
[HttpPost] public async Task<IActionResult> Edit(UpdateRiskDto dto) { }   // Functional
[HttpGet] public IActionResult Delete(Guid id) { }  // Returns NotFound (no view)
[HttpPost] public async Task<IActionResult> DeleteConfirmed(Guid id) { }  // Functional
[HttpGet] public async Task<IActionResult> Statistics() { }               // Returns NotFound (no view)
```

### View Template Requirements

**Index.cshtml** should include:
- Risk table with columns: ID, Name, Level, Status, Owner, Created Date
- Filter options: By Level, By Status, By Category, By Owner
- Search box
- Create Risk button
- Edit/Delete/Details action links
- Pagination

**Details.cshtml** should include:
- Risk information panel
- Risk scoring breakdown (Probability × Impact)
- Related controls (linked controls)
- Risk history/timeline
- Associated assessments
- Edit/Delete buttons

**Create/Edit.cshtml** should include:
- Form fields:
  - Name (required, max 200)
  - Description (text area, required)
  - Category (dropdown)
  - Owner (user selector)
  - Risk Level (High/Medium/Low - auto-calculated)
  - Probability (1-5 scale)
  - Impact (1-5 scale)
  - Target Mitigation Date
  - Status (Active/Mitigated/Accepted/Monitoring)
  - Related Controls (multi-select)
  - Risk Appetite Threshold

**Statistics.cshtml** should include:
- Risk heat map (Probability vs Impact matrix)
- Risk count by level (pie chart)
- Risk count by status (bar chart)
- Risk trend over time (line chart)
- Top 10 risks by score
- Risk metrics: Total, Active, Mitigated, Accepted

---

## 2. MISSING API ENDPOINTS (8 Endpoints) 🔴 CRITICAL

### Location
Expected in: `/api/risks` (use existing RiskApiController or create new one)

### Missing Endpoints

| Endpoint | Method | Purpose | Request Body | Response | Status |
|----------|--------|---------|--------------|----------|--------|
| `/api/risks/heat-map` | GET | Risk heat map data | - | Matrix data | ❌ MISSING |
| `/api/risks/by-status/{status}` | GET | Filter by status | - | Risk list | ❌ MISSING |
| `/api/risks/by-level/{level}` | GET | Filter by level | - | Risk list | ❌ MISSING |
| `/api/risks/by-category/{categoryId}` | GET | Filter by category | - | Risk list | ❌ MISSING |
| `/api/risks/{id}/mitigation-plan` | GET | Get mitigation plan | - | Mitigation details | ❌ MISSING |
| `/api/risks/{id}/controls` | GET | Get linked controls | - | Control list | ❌ MISSING |
| `/api/risks/{id}/accept` | POST | Accept risk | AcceptanceReason | Success message | ❌ MISSING |
| `/api/risks/statistics` | GET | Risk statistics | - | Stats object | ❌ MISSING |

### Implementation Template

```csharp
[ApiController]
[Route("api/risks")]
[Authorize]
public class RiskApiController : ControllerBase
{
    private readonly IRiskService _riskService;
    private readonly GrcDbContext _context;
    private readonly ILogger<RiskApiController> _logger;

    // MISSING ENDPOINTS TO IMPLEMENT:

    /// <summary>GET /api/risks/heat-map - Get risk heat map data (Probability vs Impact)</summary>
    [HttpGet("heat-map")]
    public async Task<IActionResult> GetRiskHeatMap()
    {
        // Return matrix: {probability: [1-5], impact: [1-5], count: N, risks: [...]}
    }

    /// <summary>GET /api/risks/by-status/{status} - Filter risks by status</summary>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetRisksByStatus(string status)
    {
        // Return risks where Status == status
        // Status: Active, Mitigated, Accepted, Monitoring
    }

    /// <summary>GET /api/risks/by-level/{level} - Filter risks by level</summary>
    [HttpGet("by-level/{level}")]
    public async Task<IActionResult> GetRisksByLevel(string level)
    {
        // Return risks where Level == level
        // Level: Critical, High, Medium, Low
    }

    /// <summary>GET /api/risks/by-category/{categoryId} - Filter risks by category</summary>
    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> GetRisksByCategory(Guid categoryId)
    {
        // Return risks where CategoryId == categoryId
    }

    /// <summary>GET /api/risks/{id}/mitigation-plan - Get risk mitigation plan</summary>
    [HttpGet("{id:guid}/mitigation-plan")]
    public async Task<IActionResult> GetMitigationPlan(Guid id)
    {
        // Return mitigation actions, responsible parties, target dates
    }

    /// <summary>GET /api/risks/{id}/controls - Get controls linked to risk</summary>
    [HttpGet("{id:guid}/controls")]
    public async Task<IActionResult> GetLinkedControls(Guid id)
    {
        // Return related controls from RiskControlMapping
    }

    /// <summary>POST /api/risks/{id}/accept - Accept a risk</summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptRisk(Guid id, [FromBody] RiskAcceptanceDto dto)
    {
        // Set Status = "Accepted", record acceptance reason and date
    }

    /// <summary>GET /api/risks/statistics - Get risk statistics</summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetRiskStatistics()
    {
        // Return: total count, by level, by status, average score, trend
    }
}
```

---

## 3. MISSING WORKFLOWS (3 Workflows) 🔴 CRITICAL

### 3.1 Risk Assessment Workflow

**Current State:** Manual CRUD only  
**Missing:** Workflow automation

**Desired Flow:**
```
1. Create Risk (Status: Draft)
   ↓
2. Submit Risk for Review (Status: Pending Review)
   ↓
3. Risk Manager Reviews
   ├─ Approve (Status: Active, assign controls)
   └─ Reject (Status: Rejected, request revision)
   ↓
4. Risk Active & Monitor
   ├─ Update Probability/Impact
   └─ Implement Mitigating Controls
   ↓
5. Risk Mitigation Complete (Status: Mitigated)
```

**Missing Components:**
- Workflow state machine
- Review approval chain
- Stakeholder notifications
- Audit trail for each transition

### 3.2 Risk Acceptance Workflow

**Current State:** Not implemented  
**Missing:** Complete workflow

**Desired Flow:**
```
1. Risk identified as cannot be mitigated
   ↓
2. Document risk acceptance rationale
   ↓
3. Submit for executive approval (Status: Pending Acceptance)
   ↓
4. Executive approval (Status: Accepted)
   ↓
5. Document monitoring plan
   ↓
6. Monitor & Review periodically
```

**Missing Components:**
- Acceptance record entity
- Executive approval workflow
- Monitoring schedule
- Notifications to stakeholders

### 3.3 Risk Escalation Workflow

**Current State:** Not implemented  
**Missing:** Complete workflow

**Desired Flow:**
```
1. Risk Score exceeds threshold
   ↓
2. Auto-escalate to Risk Committee
   ↓
3. Committee reviews risk
   ↓
4. Determine escalation action (Mitigate/Accept/Transfer/Avoid)
   ↓
5. Execute action
   ↓
6. Monitor effectiveness
```

**Missing Components:**
- Threshold configuration
- Escalation routing rules
- Committee notification
- Action tracking

---

## 4. MISSING FEATURES (5 Features) 🟠 HIGH

### 4.1 Risk Heat Map Visualization ❌

**Status:** Data model exists, UI missing  
**Required For:** Risk Management Dashboard

**Implementation:**
- Create Probability vs Impact matrix (5x5 grid)
- Color-code cells: Red (high), Yellow (medium), Green (low)
- Show risk count in each cell
- Tooltip on hover showing risk names
- Interactive: click cell to filter risks

**UI Component Location:** `/Components/Risk/RiskHeatMap.razor` (Blazor)

### 4.2 Risk Trend Analysis ❌

**Status:** Not implemented  
**Required For:** Risk Monitoring Dashboard

**Implementation:**
- Chart showing risk score trends over time
- Separate lines per risk level (High/Medium/Low)
- Date range selector
- Export trend data

**UI Component Location:** `/Components/Risk/RiskTrendChart.razor` (Blazor)

### 4.3 Risk Appetite Settings ❌

**Status:** Not implemented  
**Required For:** Risk Governance

**Implementation:**
- Allow Risk Committee to set risk appetite thresholds by category
- Compare actual risks against appetite
- Alert when exceeding appetite
- Report on appetite compliance

**Database Entity:** `RiskAppetiteSetting` (new)  
**UI Location:** Risk Administration Panel

### 4.4 Vendor Risk Scoring ❌

**Status:** Not implemented  
**Required For:** Third-Party Risk Management

**Implementation:**
- Auto-score vendor risks based on:
  - Security assessment
  - Compliance certifications
  - Historical incidents
  - Financial health
- Risk questionnaire for vendors
- Scoring algorithm

**Database Entities:** `VendorRisk`, `VendorQuestionnaire` (new)

### 4.5 Risk-Control Linkage Management ❌

**Status:** Entity exists (`RiskControlMapping`), UI missing  
**Required For:** Control Effectiveness

**Implementation:**
- Link controls to risks
- Show control effectiveness in mitigating risk
- Unlinked risks report
- Control coverage analysis

**UI Location:** Risk Details page, Control Links tab

---

## 5. DATABASE ISOLATION (1 Issue) 🟠 HIGH

### 5.1 RiskService Tenant Isolation

**Status:** Not migrated to database-per-tenant  
**Current:** Uses direct `GrcDbContext` injection  
**Required:** Migrate to `IDbContextFactory<GrcDbContext>`

**File:** `/src/GrcMvc/Services/Implementations/RiskService.cs`

**Impact:** Multi-tenant isolation incomplete

---

## 6. VALIDATION RULES (4 Issues) 🟡 MEDIUM

### 6.1 Risk Level Auto-Calculation Validation ❌

**Current:** Risk level must be manually set  
**Required:** Auto-calculate from Probability × Impact

**Validation Rule:**
```csharp
// Risk Score = Probability (1-5) × Impact (1-5)
// Score 1-5: Low
// Score 6-15: Medium
// Score 16-25: High
// Score 25: Critical
```

**Files to Update:**
- `RiskValidators.cs` - Add validation
- `RiskController.cs` - Auto-calculate in Create/Edit

### 6.2 Related Controls Validation ❌

**Current:** No validation that linked controls exist  
**Required:** Validate control IDs exist and user has access

**File:** `RiskValidators.cs` - Add `CreateRiskDtoValidator`

### 6.3 Owner Assignment Validation ❌

**Current:** No validation that owner user exists  
**Required:** Validate user exists and is active

**File:** `RiskValidators.cs` - Add owner existence check

### 6.4 Status Transition Validation ❌

**Current:** Status can transition to any value  
**Required:** Enforce valid state transitions

**Valid Transitions:**
```
Draft → Pending Review
Pending Review → Active or Rejected
Active → Mitigated or Accepted or Monitoring
Monitoring → Mitigated or Escalated
Escalated → Active or Accepted
Mitigated → [Final]
Accepted → [Final]
Rejected → Draft
```

---

## 7. INTEGRATIONS (3 Integration Gaps) 🟡 MEDIUM

### 7.1 Risk Notifications (Stub Implementation) ⚠

**Status:** Mock only - doesn't send real notifications  
**Location:** `RiskWorkflowService.cs`

**Missing:**
- Email notifications when risk status changes
- Slack/Teams notifications for escalations
- SMS alerts for critical risks
- In-app notifications

### 7.2 Risk Export (Stub Implementation) ⚠

**Status:** Not implemented  
**Required:** Export risks to Excel, PDF

**Implementation:**
- Export current risk list
- Export with charts and analysis
- Scheduled exports via Hangfire

### 7.3 Risk Assessment Integration (Stub) ⚠

**Status:** Link between Assessment and Risk not fully implemented  
**Required:** Assessments should create/update risks automatically

**Missing:**
- Assessment score → Risk identification
- Non-conformity → Risk creation
- Assessment approval → Risk activation

---

## 8. LOCALIZATION (Arabic Language Support) 🟡 MEDIUM

### Missing Translation Keys

| Key | English | Arabic | Status |
|-----|---------|--------|--------|
| `Risk_Title` | Risk Management | إدارة المخاطر | ⏳ Pending |
| `Risk_List` | Risk List | قائمة المخاطر | ⏳ Pending |
| `Risk_Create` | Create Risk | إنشاء مخاطرة | ⏳ Pending |
| `Risk_Edit` | Edit Risk | تحرير مخاطرة | ⏳ Pending |
| `Risk_Delete` | Delete Risk | حذف مخاطرة | ⏳ Pending |
| `Risk_Level_High` | High | عالي | ⏳ Pending |
| `Risk_Level_Medium` | Medium | متوسط | ⏳ Pending |
| `Risk_Level_Low` | Low | منخفض | ⏳ Pending |
| `Risk_Status_Active` | Active | نشط | ⏳ Pending |
| `Risk_Status_Mitigated` | Mitigated | تم تخفيفه | ⏳ Pending |

**File to Update:** `Resources/Risk.{culture}.resx`

---

## 9. POLICY ENFORCEMENT (Missing Policies) 🟡 MEDIUM

### Missing Risk Policies

| Policy | Purpose | Current | Status |
|--------|---------|---------|--------|
| `CreateRisk` | Can create new risks | ✅ Exists | ✅ Complete |
| `UpdateRisk` | Can edit risks | ❌ Missing | ❌ Missing |
| `DeleteRisk` | Can delete risks | ❌ Missing | ❌ Missing |
| `ApproveRisk` | Can approve risk reviews | ❌ Missing | ❌ Missing |
| `AcceptRisk` | Can accept risks | ❌ Missing | ❌ Missing |
| `EscalateRisk` | Can escalate risks | ❌ Missing | ❌ Missing |

**File to Update:** `Configuration/GrcPermissions.cs`

---

## 10. UNIT TEST COVERAGE (Missing Tests) 🟡 MEDIUM

### Missing Test Files

| Test Class | Purpose | Status |
|-----------|---------|--------|
| `RiskServiceTests.cs` | RiskService unit tests | ❌ MISSING |
| `RiskControllerTests.cs` | RiskController integration tests | ❌ MISSING |
| `RiskValidatorTests.cs` | RiskValidator tests | ❌ MISSING |
| `RiskWorkflowTests.cs` | Workflow state machine tests | ❌ MISSING |

---

## Implementation Priority & Timeline

### Phase 1: Critical (Week 1) 🔴
- [ ] Create 6 missing Razor views
- [ ] Implement 3 risk workflows
- [ ] Add 8 API endpoints
- **Effort:** 40 hours

### Phase 2: High Priority (Week 2-3) 🟠
- [ ] Implement 5 missing features
- [ ] Migrate RiskService to database-per-tenant
- **Effort:** 30 hours

### Phase 3: Medium Priority (Week 4) 🟡
- [ ] Add validation rules
- [ ] Implement integrations
- [ ] Add localization
- [ ] Add policies
- **Effort:** 25 hours

### Phase 4: Low Priority (Week 5) 
- [ ] Add unit tests
- [ ] Performance optimization
- **Effort:** 20 hours

---

## Success Criteria

- ✅ All 6 views rendering correctly
- ✅ All 8 API endpoints functional
- ✅ All 3 workflows executable
- ✅ All 5 features operational
- ✅ All validation rules enforced
- ✅ All tests passing (>80% coverage)
- ✅ Production ready deployment

---

**Created:** January 10, 2026  
**Last Reviewed:** January 10, 2026  
**Next Review:** After Phase 1 completion
