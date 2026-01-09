# Serial Code Service Specification

## Overview

The Serial Code Service provides a unified, tenant-aware, auditable serial code generation system for all GRC artifacts. Every document, assessment, risk, control, evidence, and workflow item receives a unique, traceable identifier that encodes contextual metadata.

---

## 1. Serial Code Format

### 1.1 Standard Format

```
{PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}
```

### 1.2 Format Components

| Component | Length | Description | Example |
|-----------|--------|-------------|---------|
| PREFIX | 3-4 | Entity type identifier | ASM, RSK, CTL, EVD |
| TENANT | 3-6 | Organization/tenant code | ACME, ORG001 |
| STAGE | 2 | Lifecycle stage number | 01-06 |
| YEAR | 4 | Issuance year | 2026 |
| SEQUENCE | 6 | Zero-padded sequence | 000001-999999 |
| VERSION | 2 | Revision number | 01-99 |

### 1.3 Complete Example

```
ASM-ACME-01-2026-000142-01
│   │    │  │    │      └── Version 01 (initial)
│   │    │  │    └── Sequence 142
│   │    │  └── Year 2026
│   │    └── Stage 01 (Assessment)
│   └── Tenant ACME
└── Assessment prefix
```

---

## 2. Entity Prefixes

### 2.1 Primary Entities

| Prefix | Entity Type | Stage | Description |
|--------|-------------|-------|-------------|
| ASM | Assessment | 01 | Assessment records |
| ASM-Q | Assessment Question | 01 | Individual assessment questions |
| ASM-F | Assessment Finding | 01 | Assessment findings/gaps |
| RSK | Risk | 02 | Risk register entries |
| RSK-T | Risk Treatment | 02 | Treatment/mitigation plans |
| RSK-A | Risk Asset | 02 | Asset-risk mappings |
| CMP | Compliance | 03 | Compliance items |
| CMP-R | Compliance Requirement | 03 | Framework requirements |
| CMP-G | Compliance Gap | 03 | Gap records |
| RES | Resilience | 04 | Resilience items |
| RES-P | Recovery Plan | 04 | Business continuity plans |
| RES-T | Resilience Test | 04 | DR/BC test records |
| EXC | Excellence | 05 | Benchmark items |
| EXC-B | Benchmark | 05 | Benchmark comparisons |
| EXC-I | Improvement | 05 | Improvement initiatives |
| SUS | Sustainability | 06 | Sustainability items |
| SUS-K | KPI | 06 | Key performance indicators |
| SUS-C | Certification | 06 | Certification records |

### 2.2 Cross-Stage Entities

| Prefix | Entity Type | Description |
|--------|-------------|-------------|
| CTL | Control | Control definitions |
| CTL-T | Control Test | Control testing records |
| EVD | Evidence | Evidence artifacts |
| EVD-R | Evidence Request | Evidence collection requests |
| FWK | Framework | Framework definitions |
| FWK-R | Framework Requirement | Requirement mappings |
| WFL | Workflow | Workflow instances |
| WFL-T | Workflow Task | Individual workflow tasks |
| APR | Approval | Approval records |
| AUD | Audit | Audit trail entries |
| RPT | Report | Generated reports |
| ATT | Attestation | Attestation records |
| POL | Policy | Policy documents |
| USR | User | User references |
| TEN | Tenant | Tenant/organization |

---

## 3. Service Interface

### 3.1 ISerialCodeService

```typescript
interface ISerialCodeService {
  // Generation
  generate(request: SerialCodeRequest): Promise<SerialCodeResult>;
  generateBatch(requests: SerialCodeRequest[]): Promise<SerialCodeResult[]>;

  // Validation
  validate(code: string): ValidationResult;
  parse(code: string): ParsedSerialCode;

  // Lookup
  exists(code: string): Promise<boolean>;
  getByCode(code: string): Promise<SerialCodeRecord | null>;
  getHistory(code: string): Promise<SerialCodeVersion[]>;

  // Versioning
  createNewVersion(baseCode: string): Promise<SerialCodeResult>;
  getLatestVersion(baseCode: string): Promise<string>;

  // Search
  search(criteria: SerialCodeSearchCriteria): Promise<SerialCodeRecord[]>;
  getByPrefix(prefix: string, options?: SearchOptions): Promise<SerialCodeRecord[]>;
  getByTenant(tenantCode: string, options?: SearchOptions): Promise<SerialCodeRecord[]>;
  getByStage(stage: number, options?: SearchOptions): Promise<SerialCodeRecord[]>;
  getByDateRange(start: Date, end: Date, options?: SearchOptions): Promise<SerialCodeRecord[]>;

  // Reservation
  reserve(request: SerialCodeRequest): Promise<ReservationResult>;
  confirmReservation(reservationId: string): Promise<SerialCodeResult>;
  cancelReservation(reservationId: string): Promise<void>;

  // Administration
  getNextSequence(prefix: string, tenant: string, stage: number, year: number): Promise<number>;
  resetSequence(prefix: string, tenant: string, stage: number, year: number): Promise<void>;
}
```

### 3.2 Data Types

```typescript
interface SerialCodeRequest {
  entityType: string;           // Entity type (maps to prefix)
  tenantCode: string;           // Tenant identifier
  stage?: number;               // Stage number (auto-detected if not provided)
  year?: number;                // Year (defaults to current)
  metadata?: Record<string, any>; // Additional metadata
  reservationId?: string;       // If confirming a reservation
}

interface SerialCodeResult {
  code: string;                 // Generated serial code
  prefix: string;
  tenant: string;
  stage: number;
  year: number;
  sequence: number;
  version: number;
  createdAt: Date;
  createdBy: string;
}

interface ParsedSerialCode {
  isValid: boolean;
  prefix: string;
  tenant: string;
  stage: number;
  year: number;
  sequence: number;
  version: number;
  baseCode: string;             // Code without version
}

interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
  parsed?: ParsedSerialCode;
}

interface SerialCodeRecord {
  code: string;
  entityType: string;
  entityId: string;
  prefix: string;
  tenant: string;
  stage: number;
  year: number;
  sequence: number;
  version: number;
  status: 'active' | 'superseded' | 'void' | 'reserved';
  metadata: Record<string, any>;
  createdAt: Date;
  createdBy: string;
  updatedAt: Date;
  updatedBy: string;
}

interface SerialCodeVersion {
  code: string;
  version: number;
  status: string;
  createdAt: Date;
  createdBy: string;
  changeReason?: string;
}

interface SerialCodeSearchCriteria {
  prefix?: string;
  tenant?: string;
  stage?: number;
  year?: number;
  sequenceFrom?: number;
  sequenceTo?: number;
  status?: string;
  createdAfter?: Date;
  createdBefore?: Date;
  metadata?: Record<string, any>;
}

interface ReservationResult {
  reservationId: string;
  reservedCode: string;
  expiresAt: Date;
}
```

---

## 4. Generation Rules

### 4.1 Sequence Management

```yaml
sequence_rules:
  scope: per_prefix_tenant_stage_year
  start: 1
  max: 999999
  overflow_behavior: error

  # Each combination gets its own sequence
  # Example: ASM-ACME-01-2026 starts at 000001
  # Example: RSK-ACME-02-2026 starts at 000001 (independent)
```

### 4.2 Version Rules

```yaml
version_rules:
  initial: 01
  max: 99
  on_update: increment

  # Version increments when:
  # - Document is revised
  # - Assessment is re-conducted
  # - Risk is re-evaluated
  # - Control is updated

  # Original version retained for audit trail
```

### 4.3 Tenant Code Rules

```yaml
tenant_rules:
  format: alphanumeric
  min_length: 3
  max_length: 6
  case: uppercase
  reserved: [SYS, ADM, ROOT, NULL]

  # Tenant code assigned during onboarding
  # Immutable once assigned
```

---

## 5. Database Schema

### 5.1 Serial Code Registry

```sql
CREATE TABLE serial_code_registry (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(30) UNIQUE NOT NULL,

    -- Parsed components
    prefix VARCHAR(5) NOT NULL,
    tenant_code VARCHAR(6) NOT NULL,
    stage SMALLINT NOT NULL,
    year SMALLINT NOT NULL,
    sequence INTEGER NOT NULL,
    version SMALLINT NOT NULL DEFAULT 1,

    -- Entity reference
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID NOT NULL,

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    -- active, superseded, void, reserved

    -- Metadata
    metadata JSONB DEFAULT '{}',

    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by UUID NOT NULL,

    -- Constraints
    CONSTRAINT valid_stage CHECK (stage BETWEEN 1 AND 6),
    CONSTRAINT valid_version CHECK (version BETWEEN 1 AND 99),
    CONSTRAINT valid_sequence CHECK (sequence BETWEEN 1 AND 999999)
);

-- Indexes
CREATE INDEX idx_serial_prefix ON serial_code_registry(prefix);
CREATE INDEX idx_serial_tenant ON serial_code_registry(tenant_code);
CREATE INDEX idx_serial_stage ON serial_code_registry(stage);
CREATE INDEX idx_serial_year ON serial_code_registry(year);
CREATE INDEX idx_serial_entity ON serial_code_registry(entity_type, entity_id);
CREATE INDEX idx_serial_status ON serial_code_registry(status);
CREATE INDEX idx_serial_created ON serial_code_registry(created_at);

-- Unique constraint for sequence generation
CREATE UNIQUE INDEX idx_serial_sequence
ON serial_code_registry(prefix, tenant_code, stage, year, sequence);
```

### 5.2 Sequence Counter Table

```sql
CREATE TABLE serial_sequence_counter (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    prefix VARCHAR(5) NOT NULL,
    tenant_code VARCHAR(6) NOT NULL,
    stage SMALLINT NOT NULL,
    year SMALLINT NOT NULL,
    current_sequence INTEGER NOT NULL DEFAULT 0,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT unique_counter
    UNIQUE (prefix, tenant_code, stage, year)
);

-- Function to get next sequence (atomic)
CREATE OR REPLACE FUNCTION get_next_sequence(
    p_prefix VARCHAR(5),
    p_tenant VARCHAR(6),
    p_stage SMALLINT,
    p_year SMALLINT
) RETURNS INTEGER AS $$
DECLARE
    next_seq INTEGER;
BEGIN
    INSERT INTO serial_sequence_counter
        (prefix, tenant_code, stage, year, current_sequence)
    VALUES
        (p_prefix, p_tenant, p_stage, p_year, 1)
    ON CONFLICT (prefix, tenant_code, stage, year)
    DO UPDATE SET
        current_sequence = serial_sequence_counter.current_sequence + 1,
        updated_at = NOW()
    RETURNING current_sequence INTO next_seq;

    RETURN next_seq;
END;
$$ LANGUAGE plpgsql;
```

### 5.3 Reservation Table

```sql
CREATE TABLE serial_code_reservation (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reserved_code VARCHAR(30) UNIQUE NOT NULL,
    prefix VARCHAR(5) NOT NULL,
    tenant_code VARCHAR(6) NOT NULL,
    stage SMALLINT NOT NULL,
    year SMALLINT NOT NULL,
    sequence INTEGER NOT NULL,

    status VARCHAR(20) NOT NULL DEFAULT 'reserved',
    -- reserved, confirmed, expired, cancelled

    expires_at TIMESTAMPTZ NOT NULL,
    confirmed_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL
);

-- Auto-expire reservations
CREATE INDEX idx_reservation_expires ON serial_code_reservation(expires_at)
WHERE status = 'reserved';
```

---

## 6. Implementation

### 6.1 SerialCodeService Class

```typescript
class SerialCodeService implements ISerialCodeService {
  private readonly prefixMap: Map<string, PrefixConfig>;
  private readonly db: DatabaseClient;
  private readonly cache: CacheClient;

  constructor(db: DatabaseClient, cache: CacheClient) {
    this.db = db;
    this.cache = cache;
    this.prefixMap = this.initializePrefixMap();
  }

  async generate(request: SerialCodeRequest): Promise<SerialCodeResult> {
    const prefix = this.resolvePrefix(request.entityType);
    const tenant = request.tenantCode.toUpperCase();
    const stage = request.stage ?? this.resolveStage(prefix);
    const year = request.year ?? new Date().getFullYear();

    // Get next sequence (atomic operation)
    const sequence = await this.getNextSequenceAtomic(prefix, tenant, stage, year);

    // Build code
    const code = this.buildCode(prefix, tenant, stage, year, sequence, 1);

    // Register
    const record = await this.registerCode({
      code,
      prefix,
      tenant,
      stage,
      year,
      sequence,
      version: 1,
      entityType: request.entityType,
      metadata: request.metadata ?? {},
    });

    return {
      code: record.code,
      prefix,
      tenant,
      stage,
      year,
      sequence,
      version: 1,
      createdAt: record.createdAt,
      createdBy: record.createdBy,
    };
  }

  validate(code: string): ValidationResult {
    const errors: string[] = [];
    const warnings: string[] = [];

    // Check format
    const pattern = /^([A-Z]{3,4})-([A-Z0-9]{3,6})-(\d{2})-(\d{4})-(\d{6})-(\d{2})$/;
    const match = code.match(pattern);

    if (!match) {
      errors.push('Invalid serial code format');
      return { isValid: false, errors, warnings };
    }

    const [, prefix, tenant, stageStr, yearStr, seqStr, verStr] = match;
    const stage = parseInt(stageStr, 10);
    const year = parseInt(yearStr, 10);
    const sequence = parseInt(seqStr, 10);
    const version = parseInt(verStr, 10);

    // Validate prefix
    if (!this.prefixMap.has(prefix)) {
      errors.push(`Unknown prefix: ${prefix}`);
    }

    // Validate stage
    if (stage < 1 || stage > 6) {
      errors.push(`Invalid stage: ${stage}. Must be 01-06`);
    }

    // Validate year
    const currentYear = new Date().getFullYear();
    if (year < 2020 || year > currentYear + 1) {
      warnings.push(`Unusual year: ${year}`);
    }

    // Validate sequence
    if (sequence < 1 || sequence > 999999) {
      errors.push(`Invalid sequence: ${sequence}`);
    }

    // Validate version
    if (version < 1 || version > 99) {
      errors.push(`Invalid version: ${version}`);
    }

    return {
      isValid: errors.length === 0,
      errors,
      warnings,
      parsed: {
        isValid: errors.length === 0,
        prefix,
        tenant,
        stage,
        year,
        sequence,
        version,
        baseCode: `${prefix}-${tenant}-${stageStr}-${yearStr}-${seqStr}`,
      },
    };
  }

  parse(code: string): ParsedSerialCode {
    const result = this.validate(code);
    if (!result.parsed) {
      throw new Error(`Invalid serial code: ${code}`);
    }
    return result.parsed;
  }

  async createNewVersion(baseCode: string): Promise<SerialCodeResult> {
    // Get current record
    const current = await this.getByCode(baseCode);
    if (!current) {
      throw new Error(`Serial code not found: ${baseCode}`);
    }

    // Mark current as superseded
    await this.updateStatus(current.code, 'superseded');

    // Create new version
    const newVersion = current.version + 1;
    if (newVersion > 99) {
      throw new Error(`Maximum version reached for: ${baseCode}`);
    }

    const newCode = this.buildCode(
      current.prefix,
      current.tenant,
      current.stage,
      current.year,
      current.sequence,
      newVersion
    );

    const record = await this.registerCode({
      code: newCode,
      prefix: current.prefix,
      tenant: current.tenant,
      stage: current.stage,
      year: current.year,
      sequence: current.sequence,
      version: newVersion,
      entityType: current.entityType,
      metadata: {
        ...current.metadata,
        previousVersion: current.code,
      },
    });

    return {
      code: record.code,
      prefix: current.prefix,
      tenant: current.tenant,
      stage: current.stage,
      year: current.year,
      sequence: current.sequence,
      version: newVersion,
      createdAt: record.createdAt,
      createdBy: record.createdBy,
    };
  }

  private buildCode(
    prefix: string,
    tenant: string,
    stage: number,
    year: number,
    sequence: number,
    version: number
  ): string {
    return [
      prefix,
      tenant.toUpperCase(),
      stage.toString().padStart(2, '0'),
      year.toString(),
      sequence.toString().padStart(6, '0'),
      version.toString().padStart(2, '0'),
    ].join('-');
  }

  private resolvePrefix(entityType: string): string {
    const mapping: Record<string, string> = {
      'assessment': 'ASM',
      'assessment_question': 'ASM-Q',
      'assessment_finding': 'ASM-F',
      'risk': 'RSK',
      'risk_treatment': 'RSK-T',
      'risk_asset': 'RSK-A',
      'compliance': 'CMP',
      'compliance_requirement': 'CMP-R',
      'compliance_gap': 'CMP-G',
      'resilience': 'RES',
      'recovery_plan': 'RES-P',
      'resilience_test': 'RES-T',
      'excellence': 'EXC',
      'benchmark': 'EXC-B',
      'improvement': 'EXC-I',
      'sustainability': 'SUS',
      'kpi': 'SUS-K',
      'certification': 'SUS-C',
      'control': 'CTL',
      'control_test': 'CTL-T',
      'evidence': 'EVD',
      'evidence_request': 'EVD-R',
      'framework': 'FWK',
      'framework_requirement': 'FWK-R',
      'workflow': 'WFL',
      'workflow_task': 'WFL-T',
      'approval': 'APR',
      'audit': 'AUD',
      'report': 'RPT',
      'attestation': 'ATT',
      'policy': 'POL',
    };

    const prefix = mapping[entityType.toLowerCase()];
    if (!prefix) {
      throw new Error(`Unknown entity type: ${entityType}`);
    }
    return prefix;
  }

  private resolveStage(prefix: string): number {
    const stageMapping: Record<string, number> = {
      'ASM': 1, 'ASM-Q': 1, 'ASM-F': 1,
      'RSK': 2, 'RSK-T': 2, 'RSK-A': 2,
      'CMP': 3, 'CMP-R': 3, 'CMP-G': 3,
      'RES': 4, 'RES-P': 4, 'RES-T': 4,
      'EXC': 5, 'EXC-B': 5, 'EXC-I': 5,
      'SUS': 6, 'SUS-K': 6, 'SUS-C': 6,
    };

    return stageMapping[prefix] ?? 0;
  }
}
```

---

## 7. Integration Patterns

### 7.1 Entity Creation Hook

```typescript
// Automatically assign serial code on entity creation
@BeforeInsert()
async assignSerialCode() {
  if (!this.serialCode) {
    const result = await serialCodeService.generate({
      entityType: this.constructor.name.toLowerCase(),
      tenantCode: this.tenantCode,
      metadata: {
        entityName: this.name,
        createdVia: 'auto-hook',
      },
    });
    this.serialCode = result.code;
  }
}
```

### 7.2 Version on Update

```typescript
// Create new version when entity is significantly modified
async updateEntity(id: string, changes: Partial<Entity>) {
  const entity = await this.repository.findById(id);

  if (this.isSignificantChange(changes)) {
    // Create new serial code version
    const newVersion = await serialCodeService.createNewVersion(entity.serialCode);
    entity.serialCode = newVersion.code;
    entity.previousSerialCode = entity.serialCode;
  }

  Object.assign(entity, changes);
  return this.repository.save(entity);
}
```

### 7.3 Cross-Reference Resolution

```typescript
// Resolve serial code to entity
async resolveSerialCode(code: string): Promise<ResolvedEntity> {
  const parsed = serialCodeService.parse(code);
  const record = await serialCodeService.getByCode(code);

  if (!record) {
    throw new NotFoundError(`Serial code not found: ${code}`);
  }

  // Route to appropriate repository based on entity type
  const repository = this.getRepository(record.entityType);
  const entity = await repository.findById(record.entityId);

  return {
    serialCode: code,
    entityType: record.entityType,
    entity,
    metadata: record.metadata,
  };
}
```

---

## 8. API Endpoints

### 8.1 REST API

```yaml
endpoints:
  # Generate new serial code
  POST /api/v1/serial-codes:
    body:
      entityType: string (required)
      tenantCode: string (required)
      stage: number (optional)
      metadata: object (optional)
    response:
      code: string
      prefix: string
      tenant: string
      stage: number
      year: number
      sequence: number
      version: number
      createdAt: datetime

  # Validate serial code
  POST /api/v1/serial-codes/validate:
    body:
      code: string (required)
    response:
      isValid: boolean
      errors: string[]
      warnings: string[]
      parsed: object

  # Get by code
  GET /api/v1/serial-codes/{code}:
    response:
      code: string
      entityType: string
      entityId: string
      status: string
      metadata: object
      createdAt: datetime
      versions: array

  # Search
  GET /api/v1/serial-codes:
    query:
      prefix: string
      tenant: string
      stage: number
      year: number
      status: string
      limit: number
      offset: number
    response:
      items: SerialCodeRecord[]
      total: number
      hasMore: boolean

  # Create new version
  POST /api/v1/serial-codes/{code}/versions:
    response:
      code: string (new version code)
      version: number
      previousCode: string

  # Reserve code
  POST /api/v1/serial-codes/reserve:
    body:
      entityType: string
      tenantCode: string
      expiresIn: number (seconds)
    response:
      reservationId: string
      reservedCode: string
      expiresAt: datetime

  # Confirm reservation
  POST /api/v1/serial-codes/reserve/{reservationId}/confirm:
    response:
      code: string
      status: 'active'
```

---

## 9. Usage Examples

### 9.1 Assessment Creation

```typescript
// When creating a new assessment
const assessment = await assessmentService.create({
  name: 'Q1 2026 Security Assessment',
  frameworkCode: 'SAMA-CSF',
  tenantCode: 'ACME',
});

// Serial code automatically assigned: ASM-ACME-01-2026-000001-01
console.log(assessment.serialCode);
// Output: ASM-ACME-01-2026-000001-01
```

### 9.2 Risk Registration

```typescript
// When creating a new risk
const risk = await riskService.create({
  title: 'Data breach via unpatched systems',
  likelihood: 4,
  impact: 5,
  tenantCode: 'ACME',
});

// Serial code: RSK-ACME-02-2026-000001-01
```

### 9.3 Evidence Upload

```typescript
// When uploading evidence
const evidence = await evidenceService.upload({
  file: uploadedFile,
  controlId: 'ctrl-123',
  tenantCode: 'ACME',
});

// Serial code: EVD-ACME-00-2026-000001-01
// Note: Stage 00 for cross-stage entities
```

### 9.4 Version History

```typescript
// Get all versions of a document
const history = await serialCodeService.getHistory('ASM-ACME-01-2026-000001-01');

// Output:
// [
//   { code: 'ASM-ACME-01-2026-000001-01', version: 1, status: 'superseded', ... },
//   { code: 'ASM-ACME-01-2026-000001-02', version: 2, status: 'superseded', ... },
//   { code: 'ASM-ACME-01-2026-000001-03', version: 3, status: 'active', ... },
// ]
```

---

## 10. Audit Trail Integration

### 10.1 Audit Log Entry

Every serial code operation is logged:

```typescript
interface SerialCodeAuditEntry {
  id: string;
  serialCode: string;
  action: 'generate' | 'validate' | 'version' | 'void' | 'reserve' | 'confirm';
  actor: {
    userId: string;
    tenantCode: string;
    ipAddress: string;
  };
  details: Record<string, any>;
  timestamp: Date;
}
```

### 10.2 Traceability Report

```typescript
// Generate traceability report for an entity
const report = await serialCodeService.generateTraceabilityReport('ASM-ACME-01-2026-000001-03');

// Output:
// {
//   currentCode: 'ASM-ACME-01-2026-000001-03',
//   entityType: 'assessment',
//   versionHistory: [...],
//   relatedCodes: [
//     { code: 'ASM-F-ACME-01-2026-000001-01', relation: 'finding' },
//     { code: 'RSK-ACME-02-2026-000005-01', relation: 'linked_risk' },
//     { code: 'EVD-ACME-00-2026-000012-01', relation: 'evidence' },
//   ],
//   auditTrail: [...],
// }
```

---

## 11. Configuration

### 11.1 YAML Configuration

```yaml
# serial_code_config.yaml
serial_code:
  # Reservation settings
  reservation:
    default_ttl_seconds: 300
    max_ttl_seconds: 3600
    cleanup_interval_seconds: 60

  # Sequence settings
  sequence:
    start: 1
    max: 999999
    overflow_behavior: error  # error | wrap | notify

  # Version settings
  version:
    initial: 1
    max: 99

  # Tenant settings
  tenant:
    min_length: 3
    max_length: 6
    pattern: "^[A-Z0-9]+$"
    reserved: [SYS, ADM, ROOT, NULL, TEST]

  # Cache settings
  cache:
    enabled: true
    ttl_seconds: 300
    prefix: "serial:"

  # Validation settings
  validation:
    strict_mode: true
    warn_on_future_year: true
    warn_on_old_year_threshold: 5
```

---

## 12. Error Handling

### 12.1 Error Codes

| Code | Description |
|------|-------------|
| SC001 | Invalid serial code format |
| SC002 | Unknown entity type |
| SC003 | Unknown prefix |
| SC004 | Invalid stage number |
| SC005 | Sequence overflow |
| SC006 | Maximum version reached |
| SC007 | Serial code not found |
| SC008 | Reservation expired |
| SC009 | Reservation not found |
| SC010 | Duplicate serial code |
| SC011 | Invalid tenant code |
| SC012 | Serial code already voided |

### 12.2 Error Response Format

```typescript
interface SerialCodeError {
  code: string;           // e.g., 'SC001'
  message: string;        // Human-readable message
  messageAr: string;      // Arabic message
  details: {
    field?: string;
    value?: any;
    constraint?: string;
  };
}
```

---

## Summary

The Serial Code Service provides:

1. **Unique Identification**: Every GRC artifact gets a globally unique, traceable identifier
2. **Contextual Encoding**: Serial codes encode entity type, tenant, stage, year, and version
3. **Version Tracking**: Full version history with supersession chain
4. **Audit Integration**: Complete audit trail for all serial code operations
5. **Cross-Reference**: Easy lookup and navigation between related entities
6. **Reservation System**: Pre-reserve codes for batch operations
7. **Tenant Isolation**: Tenant-scoped sequences ensure data segregation
8. **Year Rollover**: Automatic sequence reset per year for clean numbering
