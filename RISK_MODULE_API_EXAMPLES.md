# Risk Module - API Endpoint Examples

**Document Date:** January 10, 2026
**Purpose:** Complete API reference with cURL, JavaScript, and C# examples
**Base URL:** `https://your-domain.com` or `http://localhost:5000`

---

## 🔐 Authentication

All endpoints require authentication. Include JWT token in Authorization header:

```bash
Authorization: Bearer {your-jwt-token}
```

---

## 📋 Table of Contents

1. [CRUD Operations](#crud-operations)
2. [Filtering & Search](#filtering--search)
3. [Workflow Operations](#workflow-operations)
4. [Analytics & Reporting](#analytics--reporting)
5. [Control Linkage](#control-linkage)
6. [Assessment Integration](#assessment-integration)

---

## 1. CRUD Operations

### 1.1 Get All Risks

**Endpoint:** `GET /api/risks`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks?page=1&size=10" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json"
```

**JavaScript (Fetch):**
```javascript
const response = await fetch('https://your-domain.com/api/risks?page=1&size=10', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
const data = await response.json();
```

**C# (HttpClient):**
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync(
    "https://your-domain.com/api/risks?page=1&size=10");
var risks = await response.Content.ReadFromJsonAsync<ApiResponse<List<RiskDto>>>();
```

**Response:**
```json
{
  "success": true,
  "message": "Risks retrieved successfully",
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Data Breach Risk",
        "description": "Risk of unauthorized data access",
        "category": "Security",
        "probability": 3,
        "impact": 4,
        "riskScore": 12,
        "riskLevel": "High",
        "status": "Active",
        "owner": "john.doe@example.com",
        "createdDate": "2026-01-10T10:00:00Z"
      }
    ],
    "page": 1,
    "size": 10,
    "totalItems": 45
  }
}
```

---

### 1.2 Get Risk by ID

**Endpoint:** `GET /api/risks/{id}`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {token}"
```

**JavaScript:**
```javascript
const riskId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
const response = await fetch(`https://your-domain.com/api/risks/${riskId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
});
const risk = await response.json();
```

**C#:**
```csharp
var riskId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var response = await client.GetAsync($"https://your-domain.com/api/risks/{riskId}");
var risk = await response.Content.ReadFromJsonAsync<ApiResponse<RiskDto>>();
```

---

### 1.3 Create Risk

**Endpoint:** `POST /api/risks`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Vendor Security Risk",
    "description": "Third-party vendor lacks SOC 2 certification",
    "category": "Vendor Risk",
    "probability": 3,
    "impact": 4,
    "status": "Identified",
    "owner": "risk.manager@example.com",
    "mitigationStrategy": "Require SOC 2 Type II certification before renewal",
    "dueDate": "2026-06-30T00:00:00Z"
  }'
```

**JavaScript:**
```javascript
const newRisk = {
  name: 'Vendor Security Risk',
  description: 'Third-party vendor lacks SOC 2 certification',
  category: 'Vendor Risk',
  probability: 3,
  impact: 4,
  status: 'Identified',
  owner: 'risk.manager@example.com',
  mitigationStrategy: 'Require SOC 2 Type II certification',
  dueDate: '2026-06-30T00:00:00Z'
};

const response = await fetch('https://your-domain.com/api/risks', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(newRisk)
});
const result = await response.json();
```

**C#:**
```csharp
var createDto = new CreateRiskDto
{
    Name = "Vendor Security Risk",
    Description = "Third-party vendor lacks SOC 2 certification",
    Category = "Vendor Risk",
    Probability = 3,
    Impact = 4,
    Status = "Identified",
    Owner = "risk.manager@example.com",
    MitigationStrategy = "Require SOC 2 Type II certification",
    DueDate = new DateTime(2026, 6, 30)
};

var response = await client.PostAsJsonAsync("https://your-domain.com/api/risks", createDto);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<RiskDto>>();
```

**Response:**
```json
{
  "success": true,
  "message": "Risk created successfully",
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Vendor Security Risk",
    "riskScore": 12,
    "riskLevel": "High",
    "status": "Identified",
    "createdDate": "2026-01-10T15:30:00Z"
  }
}
```

---

### 1.4 Update Risk

**Endpoint:** `PUT /api/risks/{id}`

**cURL:**
```bash
curl -X PUT "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Vendor Security Risk - Updated",
    "description": "Vendor obtained SOC 2 Type II",
    "probability": 2,
    "impact": 3,
    "status": "Mitigated"
  }'
```

**JavaScript:**
```javascript
const riskId = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
const updates = {
  name: 'Vendor Security Risk - Updated',
  probability: 2,
  impact: 3,
  status: 'Mitigated'
};

await fetch(`https://your-domain.com/api/risks/${riskId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(updates)
});
```

---

### 1.5 Delete Risk

**Endpoint:** `DELETE /api/risks/{id}`

**cURL:**
```bash
curl -X DELETE "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7" \
  -H "Authorization: Bearer {token}"
```

---

## 2. Filtering & Search

### 2.1 Get Risks by Status

**Endpoint:** `GET /api/risks/by-status/{status}`

**cURL:**
```bash
# Get all active risks
curl -X GET "https://your-domain.com/api/risks/by-status/Active" \
  -H "Authorization: Bearer {token}"
```

**Valid Status Values:** `Identified`, `Active`, `Under Review`, `Accepted`, `Mitigated`, `Closed`

---

### 2.2 Get Risks by Level

**Endpoint:** `GET /api/risks/by-level/{level}`

**cURL:**
```bash
# Get all critical and high risks
curl -X GET "https://your-domain.com/api/risks/by-level/Critical" \
  -H "Authorization: Bearer {token}"
```

**Valid Level Values:** `Critical`, `High`, `Medium`, `Low`

**JavaScript:**
```javascript
const level = 'High';
const response = await fetch(`https://your-domain.com/api/risks/by-level/${level}`, {
  headers: { 'Authorization': `Bearer ${token}` }
});
const highRisks = await response.json();
```

---

### 2.3 Get Risks by Category

**Endpoint:** `GET /api/risks/by-category/{categoryId}`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/by-category/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {token}"
```

---

### 2.4 Search Risks

**Endpoint:** `GET /api/risks?q={searchTerm}`

**cURL:**
```bash
# Search for risks containing "security"
curl -X GET "https://your-domain.com/api/risks?q=security&page=1&size=20" \
  -H "Authorization: Bearer {token}"
```

**JavaScript:**
```javascript
const searchTerm = 'security';
const response = await fetch(
  `https://your-domain.com/api/risks?q=${encodeURIComponent(searchTerm)}`,
  { headers: { 'Authorization': `Bearer ${token}` } }
);
```

---

## 3. Workflow Operations

### 3.1 Accept Risk

**Endpoint:** `POST /api/risks/{id}/accept`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/accept" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "comments": "Risk accepted after executive review. Monitoring plan in place."
  }'
```

**JavaScript:**
```javascript
const riskId = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
await fetch(`https://your-domain.com/api/risks/${riskId}/accept`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    comments: 'Risk accepted after executive review'
  })
});
```

---

### 3.2 Mitigate Risk

**Endpoint:** `POST /api/risks/{id}/mitigate`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/mitigate" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "mitigationDetails": "Implemented MFA, updated firewall rules, conducted security training"
  }'
```

---

### 3.3 Start Monitoring

**Endpoint:** `POST /api/risks/{id}/monitor`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/monitor" \
  -H "Authorization: Bearer {token}"
```

---

### 3.4 Close Risk

**Endpoint:** `POST /api/risks/{id}/close`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/close" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "comments": "Risk fully mitigated and verified. Closing after 90-day monitoring period."
  }'
```

---

## 4. Analytics & Reporting

### 4.1 Get Risk Statistics

**Endpoint:** `GET /api/risks/statistics`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/statistics" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalRisks": 156,
    "criticalRisks": 8,
    "highRisks": 24,
    "mediumRisks": 67,
    "lowRisks": 57,
    "mitigatedRisks": 45,
    "activeRisks": 89
  }
}
```

---

### 4.2 Get Risk Heat Map

**Endpoint:** `GET /api/risks/heatmap/{tenantId}`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/heatmap/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "generatedAt": "2026-01-10T15:45:00Z",
    "cells": [
      {
        "likelihood": 5,
        "impact": 5,
        "riskCount": 8,
        "riskLevel": "Critical",
        "riskNames": ["Data Breach", "System Outage", "..."]
      },
      {
        "likelihood": 4,
        "impact": 5,
        "riskCount": 12,
        "riskLevel": "High",
        "riskNames": ["..."]
      }
    ]
  }
}
```

---

### 4.3 Get Risk Posture

**Endpoint:** `GET /api/risks/posture/{tenantId}`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/posture/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {token}"
```

---

### 4.4 Get Risk Score History

**Endpoint:** `GET /api/risks/{id}/history?months=12`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/history?months=6" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "date": "2025-07-10T00:00:00Z",
      "inherentRisk": 20,
      "residualRisk": 12,
      "riskLevel": "High"
    },
    {
      "date": "2025-08-10T00:00:00Z",
      "inherentRisk": 20,
      "residualRisk": 8,
      "riskLevel": "Medium"
    }
  ]
}
```

---

## 5. Control Linkage

### 5.1 Link Control to Risk

**Endpoint:** `POST /api/risks/{riskId}/controls/{controlId}`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/controls/a1b2c3d4-5678-90ab-cdef-1234567890ab" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "expectedEffectiveness": 80
  }'
```

**JavaScript:**
```javascript
const riskId = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
const controlId = 'a1b2c3d4-5678-90ab-cdef-1234567890ab';

await fetch(`https://your-domain.com/api/risks/${riskId}/controls/${controlId}`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ expectedEffectiveness: 80 })
});
```

---

### 5.2 Get Linked Controls

**Endpoint:** `GET /api/risks/{id}/controls`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/controls" \
  -H "Authorization: Bearer {token}"
```

---

### 5.3 Get Control Effectiveness

**Endpoint:** `GET /api/risks/{id}/control-effectiveness`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/control-effectiveness" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "effectiveness": 75.5
  }
}
```

---

## 6. Assessment Integration

### 6.1 Link Risk to Assessment

**Endpoint:** `POST /api/risks/{riskId}/assessments/{assessmentId}`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/assessments/b2c3d4e5-6789-01bc-def2-345678901bcd" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "findingReference": "FINDING-2026-001"
  }'
```

---

### 6.2 Get Risks from Assessment

**Endpoint:** `GET /api/risks/by-assessment/{assessmentId}`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/by-assessment/b2c3d4e5-6789-01bc-def2-345678901bcd" \
  -H "Authorization: Bearer {token}"
```

---

### 6.3 Auto-Generate Risks from Assessment

**Endpoint:** `POST /api/risks/generate-from-assessment/{assessmentId}?tenantId={tenantId}`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/generate-from-assessment/b2c3d4e5-6789-01bc-def2-345678901bcd?tenantId=3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "message": "5 risks generated from assessment",
  "data": [
    {
      "id": "...",
      "name": "Gap: Access Control",
      "description": "Risk identified from assessment gap. Control AC-2 scored 35%"
    }
  ]
}
```

---

## 7. Bulk Operations

### 7.1 Bulk Create Risks

**Endpoint:** `POST /api/risks/bulk`

**cURL:**
```bash
curl -X POST "https://your-domain.com/api/risks/bulk" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "name": "Risk 1",
      "category": "Security",
      "probability": 3,
      "impact": 4
    },
    {
      "name": "Risk 2",
      "category": "Compliance",
      "probability": 2,
      "impact": 3
    }
  ]'
```

---

## 8. Mitigation Plan

### 8.1 Get Mitigation Plan

**Endpoint:** `GET /api/risks/{id}/mitigation-plan`

**cURL:**
```bash
curl -X GET "https://your-domain.com/api/risks/7c9e6679-7425-40de-944b-e07fc1f90ae7/mitigation-plan" \
  -H "Authorization: Bearer {token}"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "riskId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "riskName": "Vendor Security Risk",
    "mitigationStrategy": "Require SOC 2 certification",
    "owner": "risk.manager@example.com",
    "targetDate": "2026-06-30T00:00:00Z",
    "status": "Active",
    "linkedControls": [],
    "mitigationActions": [
      {
        "action": "Require SOC 2 certification",
        "responsible": "risk.manager@example.com",
        "status": "Active",
        "dueDate": "2026-06-30T00:00:00Z"
      }
    ]
  }
}
```

---

## 📚 Related Documentation

- **Glossary:** [RISK_MODULE_GLOSSARY.md](./RISK_MODULE_GLOSSARY.md)
- **Troubleshooting:** [RISK_MODULE_TROUBLESHOOTING.md](./RISK_MODULE_TROUBLESHOOTING.md)
- **Full Status:** [RISK_MODULE_ACTUAL_STATUS.md](./RISK_MODULE_ACTUAL_STATUS.md)

---

**Last Updated:** January 10, 2026
**API Version:** 1.0
**Base Path:** `/api/risks`
