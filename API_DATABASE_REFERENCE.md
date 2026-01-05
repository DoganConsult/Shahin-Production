# 📡 API DOCUMENTATION & DATABASE TABLES

## Complete REST API & Schema Reference

---

## 🔌 WORKFLOW API ENDPOINTS

### Control Implementation Workflow

#### 1. Initiate Control Implementation
```http
POST /api/workflows/control-implementation/initiate/{controlId}
Authorization: Bearer {token}
Content-Type: application/json

Response (200 OK):
{
  "message": "Workflow initiated",
  "workflowId": 123,
  "status": "Initiated",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### 2. Move to Planning
```http
POST /api/workflows/control-implementation/{workflowId}/move-to-planning
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "notes": "Planning phase initiated"
}

Response (200 OK):
{
  "message": "Moved to planning",
  "status": "InPlanning",
  "updatedAt": "2024-01-15T10:35:00Z"
}
```

#### 3. Move to Implementation
```http
POST /api/workflows/control-implementation/{workflowId}/move-to-implementation
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "details": "Implementation details and plan"
}

Response (200 OK):
{
  "message": "Moved to implementation",
  "status": "InImplementation"
}
```

#### 4. Submit for Review
```http
POST /api/workflows/control-implementation/{workflowId}/submit-for-review
Authorization: Bearer {token}
Content-Type: application/json

Response (200 OK):
{
  "message": "Submitted for review",
  "status": "UnderReview"
}
```

#### 5. Approve Control Implementation
```http
POST /api/workflows/control-implementation/{workflowId}/approve
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin, ComplianceOfficer

Body:
{
  "comments": "Approved - meets all requirements"
}

Response (200 OK):
{
  "message": "Control approved",
  "status": "Approved",
  "approvedBy": "user-id",
  "approvedAt": "2024-01-15T11:00:00Z"
}
```

#### 6. Deploy Control
```http
POST /api/workflows/control-implementation/{workflowId}/deploy
Authorization: Bearer {token}
Content-Type: application/json

Response (200 OK):
{
  "message": "Control deployed",
  "status": "Deployed"
}
```

#### 7. Get Control Workflow
```http
GET /api/workflows/control-implementation/{workflowId}
Authorization: Bearer {token}

Response (200 OK):
{
  "id": 123,
  "controlId": 1,
  "currentState": "Deployed",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:00:00Z",
  "pendingTasks": [
    {
      "id": 1,
      "description": "Monitor control effectiveness",
      "assignedTo": "user-id",
      "dueDate": "2024-02-15"
    }
  ]
}
```

---

### Approval Workflow

#### 1. Submit for Approval
```http
POST /api/workflows/approval/submit
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "entityId": 1,
  "entityType": "Policy"
}

Response (200 OK):
{
  "message": "Submitted for approval",
  "workflowId": 124,
  "status": "Submitted",
  "nextApprover": "manager-user-id"
}
```

#### 2. Manager Approve
```http
POST /api/workflows/approval/{workflowId}/manager-approve
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin, ComplianceOfficer

Body:
{
  "comments": "Looks good, approved"
}

Response (200 OK):
{
  "message": "Approved by manager",
  "status": "ManagerApproved",
  "nextApprover": "compliance-user-id"
}
```

#### 3. Manager Reject
```http
POST /api/workflows/approval/{workflowId}/manager-reject
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "comments": "Needs revision in section 3"
}

Response (200 OK):
{
  "message": "Rejected by manager",
  "status": "Rejected"
}
```

#### 4. Compliance Approve
```http
POST /api/workflows/approval/{workflowId}/compliance-approve
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin, ComplianceOfficer

Body:
{
  "comments": "Approved for compliance"
}

Response (200 OK):
{
  "message": "Approved by compliance",
  "status": "ComplianceApproved",
  "nextApprover": "executive-user-id"
}
```

#### 5. Request Revision
```http
POST /api/workflows/approval/{workflowId}/request-revision
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "comments": "Please revise and resubmit"
}

Response (200 OK):
{
  "message": "Revision requested",
  "status": "Submitted"
}
```

#### 6. Executive Approve
```http
POST /api/workflows/approval/{workflowId}/executive-approve
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin

Body:
{
  "comments": "Executive sign-off approved"
}

Response (200 OK):
{
  "message": "Approved by executive",
  "status": "ExecutiveApproved"
}
```

#### 7. Get Approval History
```http
GET /api/workflows/approval/{workflowId}/history
Authorization: Bearer {token}

Response (200 OK):
{
  "workflowId": 124,
  "history": [
    {
      "stage": "Manager Review",
      "timestamp": "2024-01-15T10:30:00Z",
      "approverName": "John Manager",
      "action": "Approved",
      "comments": "Looks good"
    },
    {
      "stage": "Compliance Review",
      "timestamp": "2024-01-15T11:00:00Z",
      "approverName": "Jane Compliance",
      "action": "Approved",
      "comments": "Approved for compliance"
    }
  ]
}
```

---

### Evidence Collection Workflow

#### 1. Initiate Evidence Collection
```http
POST /api/workflows/evidence/initiate/{controlId}
Authorization: Bearer {token}
Content-Type: application/json

Response (200 OK):
{
  "message": "Evidence collection initiated",
  "workflowId": 125
}
```

#### 2. Submit Evidence
```http
POST /api/workflows/evidence/{workflowId}/submit
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "description": "Q1 2024 control evidence",
  "fileUrls": [
    "https://storage.example.com/evidence1.pdf",
    "https://storage.example.com/evidence2.xlsx"
  ]
}

Response (200 OK):
{
  "message": "Evidence submitted",
  "status": "Submitted",
  "submittedAt": "2024-01-15T12:00:00Z"
}
```

#### 3. Approve Evidence
```http
POST /api/workflows/evidence/{workflowId}/approve
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "comments": "Evidence reviewed and approved"
}

Response (200 OK):
{
  "message": "Evidence approved",
  "status": "Approved",
  "approvedAt": "2024-01-15T12:30:00Z"
}
```

---

### Audit Workflow

#### 1. Initiate Audit
```http
POST /api/workflows/audit/initiate
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin, Auditor

Body:
{
  "auditId": 1
}

Response (200 OK):
{
  "message": "Audit initiated",
  "workflowId": 126
}
```

#### 2. Create Audit Plan
```http
POST /api/workflows/audit/{workflowId}/create-plan
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "auditPlan": "Detailed audit plan and schedule"
}

Response (200 OK):
{
  "message": "Plan created",
  "status": "PlanningPhase"
}
```

#### 3. Start Fieldwork
```http
POST /api/workflows/audit/{workflowId}/start-fieldwork
Authorization: Bearer {token}
Content-Type: application/json

Response (200 OK):
{
  "message": "Fieldwork started",
  "status": "FieldworkInProgress"
}
```

#### 4. Submit Draft Report
```http
POST /api/workflows/audit/{workflowId}/submit-draft-report
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "report": "Draft audit report content"
}

Response (200 OK):
{
  "message": "Draft report submitted",
  "status": "DraftReportIssued"
}
```

#### 5. Get Audit Status
```http
GET /api/workflows/audit/{workflowId}/status
Authorization: Bearer {token}

Response (200 OK):
{
  "id": 126,
  "auditType": "Internal",
  "status": "FieldworkInProgress",
  "scope": "Financial Controls",
  "auditorName": "John Auditor",
  "startDate": "2024-01-15",
  "endDate": "2024-01-30",
  "findings": [
    {
      "id": 1,
      "title": "Control Gap",
      "description": "Missing approval step",
      "severity": "High"
    }
  ]
}
```

---

### Exception Handling Workflow

#### 1. Submit Exception
```http
POST /api/workflows/exception/submit
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "description": "Exception request for policy waiver",
  "justification": "Business reason for exception"
}

Response (200 OK):
{
  "message": "Exception submitted",
  "workflowId": 127
}
```

#### 2. Approve Exception
```http
POST /api/workflows/exception/{workflowId}/approve
Authorization: Bearer {token}
Content-Type: application/json
Roles: Admin, ComplianceOfficer

Body:
{
  "approvalConditions": "Approved with conditions"
}

Response (200 OK):
{
  "message": "Exception approved",
  "status": "Approved"
}
```

#### 3. Reject Exception
```http
POST /api/workflows/exception/{workflowId}/reject
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "approvalConditions": "Rejection reason"
}

Response (200 OK):
{
  "message": "Exception rejected",
  "status": "RejectedWithExplanation"
}
```

---

## 🔐 PERMISSION & RBAC ENDPOINTS

### Permission Management

#### Get All Permissions
```http
GET /api/permissions
Authorization: Bearer {token}
Roles: Admin

Response (200 OK):
{
  "permissions": [
    {
      "id": 1,
      "code": "Workflow.View",
      "name": "View Workflows",
      "category": "Workflow",
      "isActive": true
    }
  ],
  "total": 40
}
```

#### Get Permissions by Category
```http
GET /api/permissions?category=Workflow
Authorization: Bearer {token}

Response (200 OK):
{
  "permissions": [
    {
      "code": "Workflow.View",
      "name": "View Workflows"
    },
    {
      "code": "Workflow.Create",
      "name": "Create Workflows"
    }
  ]
}
```

### Feature Management

#### Get All Features
```http
GET /api/features
Authorization: Bearer {token}

Response (200 OK):
{
  "features": [
    {
      "id": 1,
      "code": "Workflows",
      "name": "Workflow Management",
      "displayOrder": 1,
      "isActive": true
    }
  ],
  "total": 12
}
```

#### Get User Features
```http
GET /api/features/user/{userId}
Authorization: Bearer {token}

Response (200 OK):
{
  "features": [
    {
      "code": "Workflows",
      "name": "Workflow Management"
    },
    {
      "code": "Controls",
      "name": "Control Management"
    }
  ]
}
```

### Role Management

#### Assign Role to User
```http
POST /api/roles/assign
Authorization: Bearer {token}
Roles: Admin, TenantAdmin

Body:
{
  "userId": "user-id",
  "roleId": "ComplianceOfficer",
  "tenantId": 1,
  "expiresAt": "2024-12-31T23:59:59Z"
}

Response (200 OK):
{
  "message": "Role assigned",
  "assignmentId": 1,
  "userId": "user-id",
  "roleId": "ComplianceOfficer",
  "assignedAt": "2024-01-15T10:30:00Z"
}
```

#### Get User Permissions
```http
GET /api/users/{userId}/permissions
Authorization: Bearer {token}

Response (200 OK):
{
  "userId": "user-id",
  "permissions": [
    "Workflow.View",
    "Workflow.Create",
    "Control.View",
    "Evidence.Submit"
  ]
}
```

#### Check Permission
```http
GET /api/permissions/check?permissionCode=Workflow.Create
Authorization: Bearer {token}

Response (200 OK):
{
  "hasPermission": true,
  "permissionCode": "Workflow.Create"
}
```

---

## 📊 DATABASE TABLES

### Workflow Tables

#### WorkflowInstance
```sql
CREATE TABLE WorkflowInstances (
  id INT PRIMARY KEY IDENTITY(1,1),
  entityType VARCHAR(100) NOT NULL,          -- "Control", "Policy", "Evidence", etc.
  entityId INT NOT NULL,
  workflowType VARCHAR(100) NOT NULL,        -- "ControlImplementation", "Approval", etc.
  currentState VARCHAR(100) NOT NULL,        -- Current workflow state
  tenantId INT NOT NULL FOREIGN KEY,
  createdByUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),
  updatedAt DATETIME2 DEFAULT GETUTCDATE(),
  completedAt DATETIME2 NULL,

  INDEX idx_tenantId_workflowType (tenantId, workflowType),
  INDEX idx_currentState (currentState),
  INDEX idx_createdByUserId (createdByUserId)
);
```

#### WorkflowTask
```sql
CREATE TABLE WorkflowTasks (
  id INT PRIMARY KEY IDENTITY(1,1),
  workflowInstanceId INT NOT NULL FOREIGN KEY,
  taskType VARCHAR(100) NOT NULL,            -- "Approve", "Review", "Submit", etc.
  description NVARCHAR(MAX),
  assignedToUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  dueDate DATETIME2,
  completedAt DATETIME2 NULL,
  status VARCHAR(50) NOT NULL,               -- "Pending", "Completed", "Overdue"
  tenantId INT NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_workflowInstanceId (workflowInstanceId),
  INDEX idx_assignedToUserId (assignedToUserId),
  INDEX idx_status (status)
);
```

#### WorkflowApproval
```sql
CREATE TABLE WorkflowApprovals (
  id INT PRIMARY KEY IDENTITY(1,1),
  workflowInstanceId INT NOT NULL FOREIGN KEY,
  approvalLevel INT NOT NULL,                -- 1=Manager, 2=Compliance, 3=Executive
  approverUserId NVARCHAR(450) FOREIGN KEY,
  approvalStatus VARCHAR(50) NOT NULL,       -- "Pending", "Approved", "Rejected"
  comments NVARCHAR(MAX),
  approvalDate DATETIME2 NULL,
  tenantId INT NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_workflowInstanceId (workflowInstanceId),
  INDEX idx_approverUserId (approverUserId),
  INDEX idx_approvalStatus (approvalStatus)
);
```

#### WorkflowTransition
```sql
CREATE TABLE WorkflowTransitions (
  id INT PRIMARY KEY IDENTITY(1,1),
  workflowInstanceId INT NOT NULL FOREIGN KEY,
  fromState VARCHAR(100) NOT NULL,
  toState VARCHAR(100) NOT NULL,
  transitionReason NVARCHAR(MAX),
  triggeredByUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  transitionData NVARCHAR(MAX),              -- JSON data for the transition
  tenantId INT NOT NULL FOREIGN KEY,
  transitionAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_workflowInstanceId (workflowInstanceId),
  INDEX idx_fromState_toState (fromState, toState)
);
```

#### WorkflowNotification
```sql
CREATE TABLE WorkflowNotifications (
  id INT PRIMARY KEY IDENTITY(1,1),
  workflowInstanceId INT NOT NULL FOREIGN KEY,
  notificationType VARCHAR(100) NOT NULL,    -- "TaskAssigned", "Approved", "Rejected"
  recipientUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  subject NVARCHAR(255),
  body NVARCHAR(MAX),
  isRead BIT DEFAULT 0,
  tenantId INT NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),
  readAt DATETIME2 NULL,

  INDEX idx_recipientUserId (recipientUserId),
  INDEX idx_isRead (isRead),
  INDEX idx_createdAt (createdAt)
);
```

---

### RBAC Tables

#### Permission
```sql
CREATE TABLE Permissions (
  id INT PRIMARY KEY IDENTITY(1,1),
  code VARCHAR(255) NOT NULL UNIQUE,         -- "Workflow.View", "Control.Create"
  name NVARCHAR(255) NOT NULL,
  description NVARCHAR(1000),
  category VARCHAR(100),                     -- "Workflow", "Control", "Evidence", etc.
  isActive BIT DEFAULT 1,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_code (code),
  INDEX idx_category (category)
);
```

#### Feature
```sql
CREATE TABLE Features (
  id INT PRIMARY KEY IDENTITY(1,1),
  code VARCHAR(255) NOT NULL UNIQUE,         -- "Workflows", "Controls", "Evidence"
  name NVARCHAR(255) NOT NULL,
  description NVARCHAR(1000),
  category VARCHAR(100),                     -- "GRC", "Compliance", "Reporting"
  isActive BIT DEFAULT 1,
  displayOrder INT,                          -- Order in UI menu
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_code (code),
  INDEX idx_displayOrder (displayOrder)
);
```

#### RolePermission
```sql
CREATE TABLE RolePermissions (
  id INT PRIMARY KEY IDENTITY(1,1),
  roleId NVARCHAR(450) NOT NULL FOREIGN KEY,
  permissionId INT NOT NULL FOREIGN KEY,
  tenantId INT NOT NULL FOREIGN KEY,
  assignedAt DATETIME2 DEFAULT GETUTCDATE(),
  assignedBy NVARCHAR(450),

  INDEX idx_roleId_tenantId (roleId, tenantId),
  INDEX idx_permissionId (permissionId),
  UNIQUE (roleId, permissionId, tenantId)
);
```

#### RoleFeature
```sql
CREATE TABLE RoleFeatures (
  id INT PRIMARY KEY IDENTITY(1,1),
  roleId NVARCHAR(450) NOT NULL FOREIGN KEY,
  featureId INT NOT NULL FOREIGN KEY,
  tenantId INT NOT NULL FOREIGN KEY,
  isVisible BIT DEFAULT 1,
  assignedAt DATETIME2 DEFAULT GETUTCDATE(),
  assignedBy NVARCHAR(450),

  INDEX idx_roleId_tenantId (roleId, tenantId),
  INDEX idx_featureId (featureId)
);
```

#### FeaturePermission
```sql
CREATE TABLE FeaturePermissions (
  id INT PRIMARY KEY IDENTITY(1,1),
  featureId INT NOT NULL FOREIGN KEY,
  permissionId INT NOT NULL FOREIGN KEY,
  isRequired BIT DEFAULT 1,                  -- Required to access feature
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_featureId (featureId),
  INDEX idx_permissionId (permissionId),
  UNIQUE (featureId, permissionId)
);
```

#### TenantRoleConfiguration
```sql
CREATE TABLE TenantRoleConfigurations (
  id INT PRIMARY KEY IDENTITY(1,1),
  tenantId INT NOT NULL FOREIGN KEY,
  roleId NVARCHAR(450) NOT NULL FOREIGN KEY,
  description NVARCHAR(MAX),
  maxUsersWithRole INT NULL,                 -- NULL = unlimited
  canBeModified BIT DEFAULT 1,               -- System roles cannot be modified
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_tenantId_roleId (tenantId, roleId),
  UNIQUE (tenantId, roleId)
);
```

#### UserRoleAssignment
```sql
CREATE TABLE UserRoleAssignments (
  id INT PRIMARY KEY IDENTITY(1,1),
  userId NVARCHAR(450) NOT NULL FOREIGN KEY,
  tenantId INT NOT NULL FOREIGN KEY,
  roleId NVARCHAR(450) NOT NULL FOREIGN KEY,
  isActive BIT DEFAULT 1,
  expiresAt DATETIME2 NULL,                  -- Temporary assignments
  assignedAt DATETIME2 DEFAULT GETUTCDATE(),
  assignedBy NVARCHAR(450),

  INDEX idx_userId_tenantId (userId, tenantId),
  INDEX idx_roleId_tenantId (roleId, tenantId),
  UNIQUE (userId, tenantId)
);
```

---

### Core Business Tables

#### Control
```sql
CREATE TABLE Controls (
  id INT PRIMARY KEY IDENTITY(1,1),
  controlCode VARCHAR(100) NOT NULL,
  controlName NVARCHAR(255) NOT NULL,
  description NVARCHAR(MAX),
  controlType VARCHAR(100),                  -- "Preventive", "Detective", "Corrective"
  riskArea INT FOREIGN KEY,
  ownerUserId NVARCHAR(450) FOREIGN KEY,
  frequency VARCHAR(50),                     -- "Daily", "Weekly", "Monthly"
  lastTestedDate DATETIME2,
  status VARCHAR(50),                        -- "Active", "Inactive", "Testing"
  tenantId INT NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),
  updatedAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_tenantId (tenantId),
  INDEX idx_status (status),
  INDEX idx_ownerUserId (ownerUserId)
);
```

#### Framework
```sql
CREATE TABLE Frameworks (
  id INT PRIMARY KEY IDENTITY(1,1),
  frameworkCode VARCHAR(100) NOT NULL,
  frameworkName NVARCHAR(255) NOT NULL,
  description NVARCHAR(MAX),
  version VARCHAR(50),
  publishedDate DATETIME2,
  ownerUserId NVARCHAR(450) FOREIGN KEY,
  status VARCHAR(50),                        -- "Draft", "Active", "Archived"
  tenantId INT NOT NULL FOREIGN KEY,
  createdAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_tenantId (tenantId),
  INDEX idx_status (status)
);
```

#### AuditLog
```sql
CREATE TABLE AuditLogs (
  id INT PRIMARY KEY IDENTITY(1,1),
  entityType VARCHAR(100) NOT NULL,          -- "Control", "Evidence", "Workflow"
  entityId INT NOT NULL,
  action VARCHAR(100) NOT NULL,              -- "Create", "Update", "Delete", "Approve"
  oldValues NVARCHAR(MAX),                   -- JSON
  newValues NVARCHAR(MAX),                   -- JSON
  changedByUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  changeReason NVARCHAR(MAX),
  tenantId INT NOT NULL FOREIGN KEY,
  changedAt DATETIME2 DEFAULT GETUTCDATE(),

  INDEX idx_entityType_entityId (entityType, entityId),
  INDEX idx_changedByUserId (changedByUserId),
  INDEX idx_changedAt (changedAt),
  INDEX idx_tenantId (tenantId)
);
```

#### ControlEvidence
```sql
CREATE TABLE ControlEvidences (
  id INT PRIMARY KEY IDENTITY(1,1),
  controlId INT NOT NULL FOREIGN KEY,
  evidenceType VARCHAR(100),                 -- "Documentation", "Screenshot", "SystemLog"
  description NVARCHAR(MAX),
  fileUrl NVARCHAR(MAX),                     -- URL to uploaded file
  submittedByUserId NVARCHAR(450) NOT NULL FOREIGN KEY,
  submittedDate DATETIME2 DEFAULT GETUTCDATE(),
  reviewedByUserId NVARCHAR(450) FOREIGN KEY,
  reviewDate DATETIME2 NULL,
  approvalStatus VARCHAR(50),                -- "Pending", "Approved", "Rejected"
  tenantId INT NOT NULL FOREIGN KEY,

  INDEX idx_controlId (controlId),
  INDEX idx_submittedByUserId (submittedByUserId),
  INDEX idx_approvalStatus (approvalStatus)
);
```

---

## 🔑 API RESPONSE CODES

| Code | Status | Description |
|------|--------|-------------|
| **200** | OK | Request successful |
| **201** | Created | Resource created |
| **204** | No Content | Request successful, no content |
| **400** | Bad Request | Invalid request data |
| **401** | Unauthorized | Missing/invalid authentication |
| **403** | Forbidden | Insufficient permissions |
| **404** | Not Found | Resource not found |
| **409** | Conflict | State conflict |
| **422** | Unprocessable | Validation failed |
| **500** | Server Error | Internal server error |

---

## 📈 API STATISTICS

- **Total Endpoints**: 35+
- **Workflow Endpoints**: 25+
- **RBAC Endpoints**: 10+
- **HTTP Methods**: GET, POST, PUT, DELETE
- **Authentication**: JWT Bearer Token
- **Content-Type**: application/json
- **Pagination**: Supported on list endpoints

---

## 🔒 API SECURITY

- ✅ JWT Bearer Token authentication
- ✅ Role-based authorization
- ✅ Permission validation
- ✅ CSRF protection
- ✅ Rate limiting
- ✅ Input validation
- ✅ SQL injection prevention
- ✅ XSS prevention

---

**Complete API & Database Reference Ready!**
