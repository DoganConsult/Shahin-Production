# Technology Vendor Integration Guide - Shahin Modules as Embeddable Platform

**Date:** 2025-01-06  
**Status:** 📋 **INTEGRATION GUIDE - READY FOR VENDOR ONBOARDING**

---

## 🎯 Vision

**Shahin GRC modules as embeddable components** that technology vendors (Microsoft, IBM, Dell, SAP, Oracle, etc.) can integrate into their solutions to add GRC capabilities.

---

## 🧩 Available Shahin Modules

### Module Overview

| Module | Code | Purpose | Key Features |
|--------|------|---------|--------------|
| **🗺️ MAP** | Control Library | Framework mapping, control catalog | NCA ECC, SAMA CSF, ISO 27001 controls |
| **📋 APPLY** | Applicability Matrix | Scope definition, gap analysis | Baseline selection, applicability scoring |
| **✅ PROVE** | Evidence & Testing | Evidence collection, CCM testing | Evidence upload, test execution |
| **👁️ WATCH** | Risk Monitoring | KRI dashboard, risk alerts | Real-time monitoring, threshold alerts |
| **🔧 FIX** | Remediation | Action plans, exception management | Remediation tracking, exception workflow |
| **🔒 VAULT** | Evidence Repository | Document storage, retention | Secure storage, versioning, access control |
| **📊 REPORT** | Reporting Engine | Compliance reports, dashboards | PDF export, board reports, analytics |
| **🤖 AI ENGINE** | AI Analysis | Auto-scoring, gap detection, NLP | Document analysis, risk prediction |

---

## 🔌 Integration Options

### Option 1: REST API Integration (Lightweight)

**Best For:** Quick integrations, proof-of-concept, small vendors

**Features:**
- RESTful API access to all Shahin modules
- JSON request/response format
- OAuth2 authentication
- Rate limiting (tier-based)
- Per-API-call pricing

**API Base URL:**
```
https://api.shahin.ai/v1
```

**Authentication:**
```http
Authorization: Bearer {vendor_api_key}
X-Tenant-ID: {customer_tenant_id}
```

**Example API Calls:**

```http
# Get Control Library (MAP Module)
GET /api/v1/map/controls?framework=NCA-ECC
Authorization: Bearer {api_key}
X-Tenant-ID: {tenant_id}

# Submit Evidence (PROVE Module)
POST /api/v1/prove/evidence
Authorization: Bearer {api_key}
X-Tenant-ID: {tenant_id}
Content-Type: application/json

{
  "control_id": "NCA-ECC-1-1-1",
  "evidence_url": "https://vendor.com/evidence/123",
  "evidence_type": "Screenshot",
  "description": "Microsoft Defender alert showing control implementation"
}

# Get Compliance Score (REPORT Module)
GET /api/v1/report/compliance-score?framework=NCA-ECC
Authorization: Bearer {api_key}
X-Tenant-ID: {tenant_id}

# AI Analysis (AI ENGINE Module)
POST /api/v1/ai/analyze-document
Authorization: Bearer {api_key}
X-Tenant-ID: {tenant_id}
Content-Type: application/json

{
  "document_url": "https://vendor.com/policy.pdf",
  "analysis_type": "control_extraction"
}
```

**Pricing Model:**
- Tier 1: $0.01 per control query
- Tier 2: $0.10 per AI analysis
- Tier 3: $0.05 per evidence submission
- Volume discounts available

---

### Option 2: SDK Integration (Embedded)

**Best For:** Product integrations, ISV solutions, enterprise vendors

**Available SDKs:**
- **.NET SDK** - `Shahin.GRC.SDK` (NuGet)
- **Java SDK** - `com.shahin.grc.sdk` (Maven)
- **Python SDK** - `shahin-grc-sdk` (PyPI)
- **Node.js SDK** - `@shahin/grc-sdk` (npm)

**Installation (.NET):**
```bash
dotnet add package Shahin.GRC.SDK
```

**Usage Example (.NET):**
```csharp
using Shahin.GRC;

// Initialize client
var shahin = new ShahinClient(
    apiKey: "sk-shahin-xxx",
    tenantId: "customer-tenant-id",
    baseUrl: "https://api.shahin.ai"
);

// MAP Module - Get Control Library
var controls = await shahin.MAP.GetControlsAsync(
    framework: "NCA-ECC",
    status: ControlStatus.Active
);

// APPLY Module - Check Applicability
var applicability = await shahin.APPLY.CheckApplicabilityAsync(
    controlId: "NCA-ECC-1-1-1",
    tenantId: "customer-tenant-id"
);

// PROVE Module - Submit Evidence
var evidence = await shahin.PROVE.SubmitEvidenceAsync(new EvidenceRequest {
    ControlId = "NCA-ECC-1-1-1",
    Type = EvidenceType.Screenshot,
    FileUrl = "https://vendor.com/evidence/123",
    Description = "Microsoft Defender alert",
    CollectedAt = DateTime.UtcNow
});

// WATCH Module - Get Risk Metrics
var kris = await shahin.WATCH.GetKRIsAsync(
    tenantId: "customer-tenant-id",
    timeRange: TimeRange.Last30Days
);

// FIX Module - Create Remediation Plan
var remediation = await shahin.FIX.CreateRemediationPlanAsync(new RemediationRequest {
    ControlId = "NCA-ECC-1-1-1",
    Issue = "Control not implemented",
    DueDate = DateTime.UtcNow.AddDays(30)
});

// REPORT Module - Generate Compliance Report
var report = await shahin.REPORT.GenerateAsync(new ReportRequest {
    Type = ReportType.Compliance,
    Framework = "NCA-ECC",
    Format = ReportFormat.PDF
});

// AI ENGINE Module - Analyze Document
var analysis = await shahin.AI.AnalyzeDocumentAsync(new DocumentAnalysisRequest {
    DocumentUrl = "https://vendor.com/policy.pdf",
    AnalysisType = AnalysisType.ControlExtraction
});
```

**Pricing Model:**
- Per-seat: $500/month per 1000 end-users
- Per-tenant: $2000/month per tenant
- Enterprise: Custom pricing

---

### Option 3: Embedded UI Components (White-Label)

**Best For:** Platform vendors, security suites, large integrations

**Features:**
- Embed Shahin UI components via iframe
- Custom themes and branding
- Revenue share model (15-25%)
- Full module functionality
- SSO integration

**Embeddable Components:**

1. **Control Library Widget (MAP)**
```html
<iframe 
    src="https://embed.shahin.ai/map/controls
          ?tenant={tenant_id}
          &token={embed_token}
          &theme=microsoft-brand
          &framework=NCA-ECC"
    width="100%" 
    height="600px"
    frameborder="0">
</iframe>
```

2. **Risk Matrix Widget (WATCH)**
```html
<iframe 
    src="https://embed.shahin.ai/watch/risk-matrix
          ?tenant={tenant_id}
          &token={embed_token}
          &theme=ibm-brand"
    width="100%" 
    height="500px">
</iframe>
```

3. **Evidence Upload Widget (PROVE)**
```html
<iframe 
    src="https://embed.shahin.ai/prove/upload
          ?tenant={tenant_id}
          &token={embed_token}
          &control_id={control_id}
          &theme=dell-brand"
    width="100%" 
    height="400px">
</iframe>
```

4. **Compliance Score Widget (REPORT)**
```html
<iframe 
    src="https://embed.shahin.ai/report/score
          ?tenant={tenant_id}
          &token={embed_token}
          &framework=NCA-ECC
          &theme=sap-brand"
    width="100%" 
    height="300px">
</iframe>
```

5. **AI Assistant Chatbot (AI ENGINE)**
```html
<iframe 
    src="https://embed.shahin.ai/ai/chatbot
          ?tenant={tenant_id}
          &token={embed_token}
          &theme=oracle-brand"
    width="400px" 
    height="600px">
</iframe>
```

**Theme Customization:**
```json
{
  "theme": "vendor-brand",
  "colors": {
    "primary": "#0078d4",
    "secondary": "#106ebe",
    "accent": "#00bcf2"
  },
  "logo": "https://vendor.com/logo.png",
  "hideShahinBranding": true
}
```

**Pricing Model:**
- Revenue share: 20% of customer subscription
- Minimum: $1000/month per vendor
- Custom terms for large vendors

---

### Option 4: Full White-Label / OEM License

**Best For:** Large vendors, strategic partners, enterprise solutions

**Features:**
- Full platform white-label
- Custom domain (e.g., `grc.ibm.com`)
- Vendor branding throughout
- Vendor manages their customers
- Annual license + revenue share
- Dedicated support

**Example:**
- IBM → "IBM Security GRC" (powered by Shahin)
- Microsoft → "Microsoft Compliance Center" (powered by Shahin)
- SAP → "SAP GRC Cloud" (powered by Shahin)

**Pricing Model:**
- Annual license: $100,000/year
- Revenue share: 10% of customer subscriptions
- Minimum commitment: $500K/year
- Custom terms available

---

## 💼 Vendor Integration Examples

### Microsoft Integration

**Solution:** Microsoft Defender + Shahin GRC

**Integration Points:**
1. **Microsoft Defender → Shahin PROVE**
   - Security alerts automatically submitted as evidence
   - Control mapping: Defender alerts → NCA ECC controls
   - Auto-tagging: "Security Control ECC-2-3-1"

2. **Microsoft Sentinel → Shahin WATCH**
   - Security events feed into KRI dashboard
   - Real-time risk monitoring
   - Threshold alerts

3. **Microsoft Compliance Manager → Shahin MAP**
   - Bi-directional sync of control status
   - Framework mapping
   - Compliance scoring

**Implementation:**
```csharp
// Microsoft Defender webhook → Shahin API
public async Task OnDefenderAlert(DefenderAlert alert)
{
    var shahin = new ShahinClient(apiKey, tenantId);
    
    // Map alert to control
    var control = await shahin.MAP.FindControlByKeywordAsync(
        alert.Title, 
        framework: "NCA-ECC"
    );
    
    // Submit as evidence
    await shahin.PROVE.SubmitEvidenceAsync(new EvidenceRequest {
        ControlId = control.Id,
        Type = EvidenceType.SecurityAlert,
        Source = "Microsoft Defender",
        AlertId = alert.Id,
        Severity = alert.Severity
    });
}
```

**Value Proposition:** "Compliance built into Microsoft Security"

---

### IBM Integration

**Solution:** IBM QRadar + Shahin GRC

**Integration Points:**
1. **QRadar SIEM → Shahin WATCH**
   - Security events → KRI monitoring
   - Real-time compliance tracking
   - Risk threshold alerts

2. **Watson AI → Shahin AI ENGINE**
   - Enhanced document analysis
   - Risk prediction
   - Natural language queries

3. **QRadar Findings → Shahin PROVE**
   - Security incidents → Evidence
   - Auto-mapping to controls

**Implementation:**
```java
// IBM QRadar → Shahin SDK (Java)
ShahinClient shahin = new ShahinClient(apiKey, tenantId);

// QRadar event → Shahin KRI
QRadarEvent event = qradar.getEvent();
shahin.watch().updateKRI("failed_logins", event.getCount());

// Watson analysis → Shahin AI
WatsonAnalysis analysis = watson.analyze(policyDocument);
shahin.ai().submitAnalysis(analysis);
```

**Value Proposition:** "Watson-powered GRC in QRadar"

---

### Dell Integration

**Solution:** Dell Secureworks + Shahin GRC

**Integration Points:**
1. **Secureworks Taegis → Shahin PROVE**
   - XDR alerts → Evidence
   - Vulnerability findings → Control evidence

2. **Dell PowerProtect → Shahin PROVE**
   - Backup status → DR evidence
   - Backup compliance → Control testing

3. **Dell VxRail → Shahin MAP**
   - Infrastructure assets → Control mapping
   - Hardware compliance

**Implementation:**
```python
# Dell Secureworks → Shahin SDK (Python)
from shahin_grc import ShahinClient

shahin = ShahinClient(api_key, tenant_id)

# Secureworks alert → Evidence
alert = secureworks.get_alert()
shahin.prove.submit_evidence(
    control_id="NCA-ECC-2-3-1",
    evidence_type="security_alert",
    source="Secureworks Taegis",
    alert_id=alert.id
)

# Backup status → Evidence
backup_status = powerprotect.get_backup_status()
shahin.prove.submit_evidence(
    control_id="NCA-ECC-3-2-1",
    evidence_type="backup_status",
    source="Dell PowerProtect",
    status=backup_status.status
)
```

**Value Proposition:** "Security + Compliance unified"

---

### SAP Integration

**Solution:** SAP S/4HANA + Shahin GRC

**Integration Points:**
1. **SAP User Access → Shahin PROVE**
   - User access matrix → SoD evidence
   - Role assignments → Control testing

2. **SAP Financial Controls → Shahin MAP**
   - Financial controls → Control library
   - Transaction monitoring → Control testing

3. **SAP GRC Module → Shahin (Bi-directional)**
   - Risk/control sync
   - Assessment data exchange

**Implementation:**
```javascript
// SAP S/4HANA → Shahin SDK (Node.js)
const shahin = require('@shahin/grc-sdk');

const client = new shahin.Client(apiKey, tenantId);

// SAP user access → Evidence
const userAccess = await sap.getUserAccessMatrix();
await client.prove.submitEvidence({
    controlId: 'NCA-ECC-1-5-1',
    evidenceType: 'user_access_review',
    source: 'SAP S/4HANA',
    data: userAccess
});

// Financial controls → Control mapping
const financialControls = await sap.getFinancialControls();
await client.map.syncControls(financialControls);
```

**Value Proposition:** "GRC native in S/4HANA"

---

### Oracle Integration

**Solution:** Oracle ERP Cloud + Shahin GRC

**Integration Points:**
1. **Oracle GRC Cloud → Shahin (Bi-directional)**
   - Risk and compliance data sync
   - Assessment results exchange

2. **Oracle ERP Cloud → Shahin PROVE**
   - Financial controls → Evidence
   - User access → SoD evidence

3. **Oracle HCM Cloud → Shahin PROVE**
   - Employee training → Compliance evidence
   - Certifications → Control evidence

**Implementation:**
```java
// Oracle ERP → Shahin SDK (Java)
ShahinClient shahin = new ShahinClient(apiKey, tenantId);

// Oracle GRC sync
OracleGRCData grcData = oracleGRC.getData();
shahin.map().syncControls(grcData.getControls());
shahin.prove().syncEvidence(grcData.getEvidence());

// Financial controls
FinancialControls controls = oracleERP.getFinancialControls();
shahin.prove().submitEvidence(controls);
```

**Value Proposition:** "Complete GRC in Oracle Cloud"

---

## 🏗️ Technical Architecture

### API Gateway Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SHAHIN API GATEWAY                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    AUTHENTICATION LAYER                      │   │
│  │  • OAuth2 / API Keys                                         │   │
│  │  • Tenant Validation                                         │   │
│  │  • Rate Limiting                                             │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    MODULE ENDPOINTS                          │   │
│  │                                                               │   │
│  │  /api/v1/map/*          → MAP Module (Control Library)       │   │
│  │  /api/v1/apply/*        → APPLY Module (Applicability)       │   │
│  │  /api/v1/prove/*        → PROVE Module (Evidence)            │   │
│  │  /api/v1/watch/*        → WATCH Module (Monitoring)          │   │
│  │  /api/v1/fix/*          → FIX Module (Remediation)           │   │
│  │  /api/v1/vault/*        → VAULT Module (Repository)          │   │
│  │  /api/v1/report/*       → REPORT Module (Reporting)           │   │
│  │  /api/v1/ai/*           → AI ENGINE Module (AI Analysis)     │   │
│  │                                                               │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    EMBEDDABLE UI SERVICE                     │   │
│  │  • iFrame hosting                                            │   │
│  │  • Theme engine                                              │   │
│  │  • CORS handling                                             │   │
│  │  • SSO integration                                           │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Vendor Onboarding Process

### Step 1: Registration
1. Vendor registers at `https://partners.shahin.ai`
2. Submit company information
3. Choose integration tier (API, SDK, Embedded, White-Label)
4. Review and sign partnership agreement

### Step 2: API Key Generation
1. Receive API keys (development + production)
2. Access developer portal
3. Review API documentation
4. Download SDK (if applicable)

### Step 3: Integration Development
1. Use sandbox environment for testing
2. Integrate using chosen method (API/SDK/UI)
3. Test with sample data
4. Submit for review

### Step 4: Production Deployment
1. Production API keys issued
2. Integration certified
3. Listed in marketplace
4. Go live!

---

## 🛠️ Developer Resources

### API Documentation
- **Swagger UI:** `https://api.shahin.ai/swagger`
- **Postman Collection:** Available in developer portal
- **OpenAPI Spec:** `https://api.shahin.ai/openapi.json`

### SDK Documentation
- **.NET:** `https://docs.shahin.ai/sdk/dotnet`
- **Java:** `https://docs.shahin.ai/sdk/java`
- **Python:** `https://docs.shahin.ai/sdk/python`
- **Node.js:** `https://docs.shahin.ai/sdk/nodejs`

### Sample Code
- **GitHub:** `https://github.com/shahin-ai/integration-samples`
- **Examples:** Microsoft, IBM, Dell, SAP, Oracle

### Support
- **Developer Portal:** `https://partners.shahin.ai`
- **Support Email:** `partners@shahin.ai`
- **Slack Channel:** `#shahin-partners`

---

## 💰 Pricing Summary

| Integration Tier | Setup Fee | Monthly Fee | Revenue Share | Best For |
|------------------|-----------|-------------|---------------|----------|
| **API Access** | Free | $0 | N/A | Small integrations |
| **SDK License** | $1,000 | $500-2000 | N/A | Product integrations |
| **Embedded UI** | $5,000 | $1,000+ | 20% | Platform vendors |
| **White-Label** | $25,000 | $8,333+ | 10% | Large vendors |

---

## 🎯 Value Proposition for Vendors

### For Technology Vendors:
- ✅ **New Revenue Stream** - Add GRC capabilities to existing products
- ✅ **Market Differentiation** - Stand out with compliance features
- ✅ **Customer Stickiness** - Integrated GRC increases retention
- ✅ **Faster Time-to-Market** - No need to build GRC from scratch
- ✅ **Co-Marketing** - Joint marketing opportunities
- ✅ **Marketplace Listing** - Featured in Shahin partner marketplace

### For GRC Platform:
- ✅ **Ecosystem Growth** - Large vendor network
- ✅ **Market Reach** - Access to vendor customer base
- ✅ **Revenue Share** - Recurring revenue from integrations
- ✅ **Feature Validation** - Real-world usage feedback
- ✅ **Competitive Advantage** - Unique embeddable platform

---

## 📊 Integration Status

| Vendor | Integration Type | Status | Modules Used |
|--------|----------------|--------|--------------|
| Microsoft | Embedded UI | 🔜 Coming Soon | MAP, WATCH, PROVE |
| IBM | SDK + API | 🔜 Coming Soon | WATCH, AI, REPORT |
| Dell | API | 🔜 Coming Soon | PROVE, VAULT |
| SAP | Embedded UI | 🔜 Coming Soon | MAP, APPLY, PROVE |
| Oracle | SDK | 🔜 Coming Soon | All modules |
| ServiceNow | Embedded UI | 🔜 Coming Soon | FIX, WATCH |
| Palo Alto | API | 🔜 Coming Soon | WATCH, PROVE |
| CrowdStrike | API | 🔜 Coming Soon | WATCH, MAP |

---

## 🚀 Next Steps for Vendors

1. **Register** at `https://partners.shahin.ai`
2. **Choose Integration Tier** (API, SDK, Embedded, White-Label)
3. **Get API Keys** and access developer portal
4. **Start Integration** using documentation and samples
5. **Test** in sandbox environment
6. **Deploy** to production

---

**Status:** ✅ **INTEGRATION GUIDE COMPLETE**

**Ready For:** Vendor onboarding and integration development
