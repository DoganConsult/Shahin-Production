# Risk Module - Glossary & Technical Reference

**Document Date:** January 10, 2026
**Purpose:** Technical definitions and reference guide
**Audience:** Developers, architects, and technical stakeholders

---

## 📖 Key Terms & Definitions

### A

**API Endpoint**
- A specific URL path and HTTP method combination that provides access to a resource
- Example: `GET /api/risks/{id}`

**Assessment Integration**
- Automatic creation of risks from compliance assessment findings
- Bidirectional linking between assessments and risks

**Async Validation**
- Asynchronous validation checks that query external services (e.g., user directory)
- Example: `MustAsync(BeValidOwnerAsync)` in FluentValidation

**Audit Trail**
- Complete history of changes to risk records
- Includes: CreatedBy, CreatedDate, ModifiedBy, ModifiedDate

---

### C

**Control Effectiveness**
- Weighted average of how well linked controls mitigate a risk
- Formula: `Σ(ActualEffectiveness × Weight) / TotalWeight`

**Control Linkage**
- Association between risks and mitigating controls
- Stored in: `RiskControlMapping` entity

**CRUD**
- Create, Read, Update, Delete operations
- Basic data management operations

---

### D

**Database-per-Tenant**
- Multi-tenant isolation pattern using separate databases
- Alternative to shared database with query filters

**DTO (Data Transfer Object)**
- Object used to transfer data between layers
- Examples: `CreateRiskDto`, `UpdateRiskDto`, `RiskDto`

---

### F

**FluentValidation**
- Library for building strongly-typed validation rules
- Used in: `RiskValidators.cs` (389 lines)

---

### G

**Global Query Filter**
- EF Core feature that automatically filters queries
- Current implementation: Filters by TenantId + WorkspaceId + IsDeleted
- Location: `GrcDbContext.cs`

**GRC**
- Governance, Risk, and Compliance
- Integrated framework for managing organizational risk

---

### H

**Heat Map**
- 5x5 matrix visualization of risks by Probability × Impact
- Color-coded: Red (critical), Yellow (medium), Green (low)

**HTTP Methods**
- GET: Retrieve data
- POST: Create new resource
- PUT: Update entire resource
- PATCH: Partial update
- DELETE: Remove resource

---

### I

**Inherent Risk**
- Risk level before applying any controls
- Formula: `Likelihood × Impact`

**IUnitOfWork Pattern**
- Design pattern that coordinates multiple repository operations
- Ensures transactional consistency

---

### L

**Localization**
- Process of adapting software for different languages/regions
- Missing: `.resx` resource files for Arabic/English

**Likelihood**
- Probability that a risk event will occur
- Scale: 1-5 (1=rare, 5=almost certain)

---

### M

**Multi-Tenant Isolation**
- Ensuring data from one tenant cannot be accessed by another
- Method: Global query filters in `GrcDbContext`

**Mitigation Strategy**
- Plan for reducing risk to acceptable levels
- Stored in: `Risk.MitigationStrategy` field

---

### P

**Policy Enforcement**
- Authorization checks using `PolicyEnforcementHelper`
- Validates: Create, Update, Read, Delete permissions

**Probability**
- Same as Likelihood (1-5 scale)
- Used in risk score calculation

---

### R

**RBAC (Role-Based Access Control)**
- Access control based on user roles
- Implemented via: `GrcPermissions.Risks.*`

**Residual Risk**
- Risk level after applying controls
- Formula: `InherentRisk × (1 - ControlEffectiveness/100)`

**Risk Appetite**
- Organization's willingness to accept risk
- Configuration in: `RiskAppetiteSetting` entity

**Risk Level**
- Categorical assessment: Critical, High, Medium, Low
- Auto-calculated from risk score

**Risk Score**
- Numeric value: `Probability × Impact` (1-25)

---

### S

**Soft Delete**
- Marking records as deleted without physically removing them
- Field: `IsDeleted = true`
- Filtered by: Global query filter

**State Machine**
- Pattern for managing valid state transitions
- Implementation: `RiskStateMachine` class

**Stakeholder Notification**
- Automated alerts sent when risk status changes
- Routing based on risk level

---

### T

**Tenant**
- Isolated customer/organization in multi-tenant system
- Identified by: `TenantId` (Guid)

**Trend Analysis**
- Historical tracking of risk scores over time
- Shows: 12-month progression

---

### V

**Validation Rules**
- Business logic checks before saving data
- Implementation: FluentValidation in `RiskValidators.cs`

**Vendor Risk**
- Risks associated with third-party vendors/suppliers
- Entity: `Vendor` with `RiskLevel` property

---

### W

**Workflow**
- Automated business process with defined states
- Examples: Risk Assessment, Acceptance, Escalation

**Workspace**
- Logical grouping within a tenant
- Supports multi-workspace tenants

---

## 🔧 Technical Patterns Used

### 1. Repository Pattern
```
Controller → Service → IUnitOfWork → Repository → DbContext
```

### 2. Query Filter Pattern
```csharp
modelBuilder.Entity<Risk>().HasQueryFilter(e =>
    !e.IsDeleted &&
    (GetCurrentTenantId() == null || e.TenantId == GetCurrentTenantId())
);
```

### 3. State Machine Pattern
```
Draft → PendingReview → Active → Mitigated/Accepted
```

### 4. DTO Pattern
```
Request → DTO → Validator → Service → Entity → Database
```

---

## 📊 Risk Scoring Reference

### Probability Scale (1-5)
| Value | Label | Description |
|-------|-------|-------------|
| 1 | Rare | < 5% chance |
| 2 | Unlikely | 5-25% chance |
| 3 | Possible | 25-50% chance |
| 4 | Likely | 50-75% chance |
| 5 | Almost Certain | > 75% chance |

### Impact Scale (1-5)
| Value | Label | Description |
|-------|-------|-------------|
| 1 | Negligible | Minor inconvenience |
| 2 | Minor | Limited impact |
| 3 | Moderate | Noticeable impact |
| 4 | Major | Significant impact |
| 5 | Severe | Critical impact |

### Risk Level Thresholds
| Score Range | Level | Color | Action Required |
|-------------|-------|-------|-----------------|
| 20-25 | Critical | Red | Immediate action |
| 12-19 | High | Orange | Urgent attention |
| 6-11 | Medium | Yellow | Plan mitigation |
| 1-5 | Low | Green | Monitor |

---

## 🔐 Permission Constants

```csharp
// View permissions
Grc.Risks.View         // Read risk data
Grc.Risks.Manage       // Full management

// CRUD permissions
Grc.Risks.Create       // Create new risks
Grc.Risks.Edit         // Edit existing risks
Grc.Risks.Delete       // Delete risks

// Workflow permissions
Grc.Risks.Approve      // Approve risk assessments
Grc.Risks.Accept       // Accept risks
Grc.Risks.Monitor      // Start monitoring
Grc.Risks.Escalate     // Escalate to committee
```

---

## 📁 Entity Relationships

```
Risk
├── TenantId → Tenant
├── WorkspaceId → Workspace
├── Owner → User (string reference)
├── Category → RiskCategory
└── Relationships:
    ├── RiskControlMapping[] → Controls
    ├── Labels → Key-value metadata
    └── AssessmentId → Assessment (via Labels)

RiskControlMapping
├── RiskId → Risk
├── ControlId → Control
├── ExpectedEffectiveness (int)
├── ActualEffectiveness (int)
└── MappingStrength (string)

RiskAppetiteSetting
├── TenantId → Tenant
├── Category (string)
├── MinimumRiskScore (int)
├── MaximumRiskScore (int)
└── TargetRiskScore (int)
```

---

## 🔄 State Transitions

### Valid Transitions Matrix

| From \ To | Draft | PendingReview | Active | Accepted | Mitigated | Closed | Rejected |
|-----------|-------|---------------|--------|----------|-----------|--------|----------|
| **Draft** | ✓ | ✓ | ✓ | ✗ | ✗ | ✓ | ✗ |
| **PendingReview** | ✗ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Active** | ✗ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ |
| **Accepted** | ✗ | ✗ | ✓ | ✓ | ✓ | ✓ | ✗ |
| **Mitigated** | ✗ | ✗ | ✓ | ✗ | ✓ | ✓ | ✗ |
| **Closed** | ✗ | ✗ | ✓ | ✗ | ✗ | ✓ | ✗ |
| **Rejected** | ✓ | ✓ | ✗ | ✗ | ✗ | ✗ | ✓ |

---

## 🌐 API Response Formats

### Success Response
```json
{
  "success": true,
  "message": "Risk retrieved successfully",
  "data": {
    "id": "guid",
    "name": "Risk name",
    "riskScore": 12
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Risk not found",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

### Paginated Response
```json
{
  "items": [...],
  "page": 1,
  "size": 10,
  "totalItems": 150
}
```

---

## 🔍 Common Queries

### Get Risks by Level
```csharp
var highRisks = await _riskService.GetAllAsync()
    .Where(r => r.RiskScore >= 12)
    .ToList();
```

### Calculate Control Effectiveness
```csharp
var effectiveness = await _riskService
    .CalculateControlEffectivenessAsync(riskId);
```

### Get Heat Map Data
```csharp
var heatMap = await _riskService
    .GetHeatMapAsync(tenantId);
```

---

## 📚 Related Documentation

- **Main Report:** [RISK_MODULE_ACTUAL_STATUS.md](./RISK_MODULE_ACTUAL_STATUS.md)
- **Quick Summary:** [RISK_MODULE_VALIDATION_SUMMARY.md](./RISK_MODULE_VALIDATION_SUMMARY.md)
- **Index:** [RISK_MODULE_INDEX.md](./RISK_MODULE_INDEX.md)

---

**Last Updated:** January 10, 2026
**Version:** 1.0
**Maintained By:** Development Team
