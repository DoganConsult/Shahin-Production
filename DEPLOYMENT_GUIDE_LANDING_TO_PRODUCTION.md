# 🚀 COMPLETE END-TO-END DEPLOYMENT PROCESS
## From Landing Page to Full Production GRC Platform

**Created:** January 10, 2026  
**Version:** 1.0  
**Status:** Production Deployment Guide

---

## **ARCHITECTURE OVERVIEW**

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           USER JOURNEY FLOW                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                   │
│  │   LANDING    │───▶│    TRIAL     │───▶│  ACTIVATION  │                   │
│  │    PAGE      │    │   SIGNUP     │    │    EMAIL     │                   │
│  │ shahin-ai.com│    │  /StartTrial │    │   48hr link  │                   │
│  └──────────────┘    └──────────────┘    └──────────────┘                   │
│         │                   │                   │                            │
│         ▼                   ▼                   ▼                            │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                   │
│  │ QUESTIONNAIRE│───▶│   TENANT     │───▶│    LOGIN     │                   │
│  │  5 sections  │    │  CREATION    │    │ First Access │                   │
│  │  30+ fields  │    │  DB Provision│    │              │                   │
│  └──────────────┘    └──────────────┘    └──────────────┘                   │
│                                                 │                            │
│                                                 ▼                            │
│  ┌──────────────────────────────────────────────────────────────────┐       │
│  │                    ONBOARDING WIZARD (3 Steps)                    │       │
│  ├──────────────────────────────────────────────────────────────────┤       │
│  │  Step 1: OrgProfile  │  Step 2: ReviewScope  │  Step 3: CreatePlan│       │
│  │  - Industry          │  - Framework selection│  - First assessment│       │
│  │  - Size              │  - AI recommendations │  - Target dates    │       │
│  │  - Location          │  - Baseline creation  │  - Team assignment │       │
│  └──────────────────────────────────────────────────────────────────┘       │
│                                                 │                            │
│                                                 ▼                            │
│  ┌──────────────────────────────────────────────────────────────────┐       │
│  │                    AUTO-PROVISIONING (Background)                 │       │
│  ├──────────────────────────────────────────────────────────────────┤       │
│  │  • 5 Default Teams (GRC-CORE, SEC-OPS, IT-OPS, RISK-MGT, AUDIT)  │       │
│  │  • RACI Matrix (8 control families mapped)                        │       │
│  │  • Admin assigned to GRC-CORE with APPROVER role                  │       │
│  └──────────────────────────────────────────────────────────────────┘       │
│                                                 │                            │
│                                                 ▼                            │
│  ┌──────────────────────────────────────────────────────────────────┐       │
│  │                    PRODUCTION GRC PLATFORM                        │       │
│  ├──────────────────────────────────────────────────────────────────┤       │
│  │  Dashboard │ Assessments │ Controls │ Risks │ Evidence │ Reports │       │
│  └──────────────────────────────────────────────────────────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## **📍 PHASE 1: LANDING PAGE DEPLOYMENT**

### 1.1 Build Process
```bash
cd /home/Shahin-ai/Shahin-Jan-2026/shahin-ai-website
npm install
npm run build
```

### 1.2 Deployment Options

#### **Option A: Vercel (Recommended)**
```bash
npm i -g vercel
vercel login
vercel --prod

# During setup:
# - Project Name: shahin-ai-website
# - Framework: Next.js
# - Build Command: npm run build
# - Output Directory: .next
```

#### **Option B: Docker Self-Hosted**
```dockerfile
# Create Dockerfile in shahin-ai-website/
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:18-alpine
WORKDIR /app
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]
```

```bash
docker build -t shahin-landing .
docker run -p 3000:3000 shahin-landing
```

### 1.3 Domain Configuration
```yaml
# Cloudflare DNS Settings
Type: A
Name: @
Value: [Vercel IP or Server IP]
Proxy: ON

Type: CNAME
Name: www
Value: shahin-ai.com
Proxy: ON
```

### 1.4 Files Involved
| File | Purpose |
|------|---------|
| `shahin-ai-website/package.json` | Dependencies & scripts |
| `shahin-ai-website/next.config.js` | Next.js configuration |
| `shahin-ai-website/app/page.tsx` | Main landing page |
| `shahin-ai-website/components/sections/` | All sections |

---

## **📍 PHASE 2: API INTEGRATION**

### 2.1 Update OnboardingQuestionnaire Component
**File:** `shahin-ai-website/components/sections/OnboardingQuestionnaire.tsx`

```typescript
// Line 571-578 - Update handleSubmit function
const handleSubmit = async () => {
  setIsSubmitting(true);
  
  try {
    const response = await fetch('https://app.shahin-ai.com/api/Landing/StartTrial', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        Email: formData.contact_email,
        FullName: formData.contact_name,
        CompanyName: formData.org_name,
        PhoneNumber: formData.contact_phone,
        CompanySize: formData.employee_count,
        Industry: formData.industry,
        TrialPlan: 'PROFESSIONAL',
        Locale: formData.preferred_language
      })
    });
    
    const result = await response.json();
    
    if (result.success) {
      // Redirect to trial registration
      window.location.href = result.redirectUrl;
    } else {
      // Handle error - show message to user
      setError(result.messageEn || 'Signup failed. Please try again.');
    }
  } catch (error) {
    console.error('Trial signup failed:', error);
    setError('Network error. Please check your connection.');
  }
  
  setIsSubmitting(false);
};
```

### 2.2 Configure CORS in GrcMvc
**File:** `src/GrcMvc/Program.cs`

```csharp
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLandingPage",
        policy =>
        {
            policy.WithOrigins(
                    "https://shahin-ai.com",
                    "https://www.shahin-ai.com",
                    "http://localhost:3000"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Use CORS (before UseRouting)
app.UseCors("AllowLandingPage");
```

### 2.3 API Endpoint Details
**Controller:** `src/GrcMvc/Controllers/LandingController.cs`

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/Landing/StartTrial` | POST | Initial trial signup |
| `/api/Landing/ContactUs` | POST | Contact form |
| `/api/Landing/RequestDemo` | POST | Demo request |

---

## **📍 PHASE 3: TENANT CREATION FLOW**

### 3.1 Trial Signup Storage
**Endpoint:** `POST /api/Landing/StartTrial`  
**Controller:** `src/GrcMvc/Controllers/LandingController.cs:313-401`

```csharp
// Request DTO
public class TrialSignupDto
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CompanySize { get; set; }
    public string? Industry { get; set; }
    public string? TrialPlan { get; set; }
    public string? Locale { get; set; }
}

// Response
{
    "success": true,
    "messageEn": "Trial signup received!",
    "messageAr": "تم استلام طلب التجربة!",
    "redirectUrl": "/grc-free-trial?email=xxx&name=xxx",
    "signupId": "guid"
}
```

### 3.2 Full Registration Page
**URL:** `/grc-free-trial?email=xxx&name=xxx`  
**Controller:** `src/GrcMvc/Controllers/TrialController.cs`

```csharp
// Registration flow
[HttpPost]
public async Task<IActionResult> Register(TrialRegistrationModel model)
{
    // 1. Generate tenant ID and slug
    var tenantId = Guid.NewGuid();
    var tenantSlug = GenerateTenantSlug(model.OrganizationName);
    
    // 2. Create Tenant record
    var tenant = new Tenant
    {
        Id = tenantId,
        OrganizationName = model.OrganizationName,
        TenantSlug = tenantSlug,
        AdminEmail = model.Email,
        Status = "Active",
        SubscriptionTier = "Trial",
        TrialExpiresAt = DateTime.UtcNow.AddDays(14)
    };
    
    // 3. Create Identity User
    var user = new ApplicationUser
    {
        UserName = model.Email,
        Email = model.Email,
        FullName = model.FullName
    };
    await _userManager.CreateAsync(user, model.Password);
    
    // 4. Create TenantUser link
    var tenantUser = new TenantUser
    {
        TenantId = tenantId,
        UserId = user.Id,
        RoleCode = "ADMIN"
    };
    
    // 5. Provision database (async)
    await _provisioningService.ProvisionTenantAsync(tenantId);
    
    // 6. Send activation email
    await _emailService.SendActivationEmailAsync(tenant);
    
    return RedirectToAction("RegistrationComplete");
}
```

### 3.3 Database Provisioning
**Service:** `TenantProvisioningService.ProvisionTenantAsync()`

```sql
-- For multi-tenant with isolated databases:
CREATE DATABASE [Tenant_{slug}];

-- For shared database with TenantId isolation:
-- All tables have TenantId column
-- Row-level security enforced
```

### 3.4 Activation Email Template
**File:** `Views/EmailTemplates/ActivationEmail.cshtml`

```html
<!DOCTYPE html>
<html>
<head>
    <style>
        .btn { background: #10b981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; }
    </style>
</head>
<body>
    <h1>مرحباً / Welcome</h1>
    <p>Dear @Model.AdminName,</p>
    <p>Your organization "@Model.OrganizationName" has been created on Shahin AI GRC Platform.</p>
    <p>Click below to activate your account:</p>
    <a href="@Model.ActivationUrl" class="btn">Activate Account</a>
    <p>This link expires in 48 hours.</p>
    <hr>
    <p dir="rtl">تم إنشاء منظمتك "@Model.OrganizationNameAr" على منصة شاهين للحوكمة.</p>
</body>
</html>
```

---

## **📍 PHASE 4: ACTIVATION & LOGIN**

### 4.1 Activation Flow
**URL:** `/auth/activate?slug=acme-corp&token=xxx`  
**Controller:** `src/GrcMvc/Controllers/OnboardingController.cs:107-141`

```csharp
[HttpPost("activate")]
public async Task<IActionResult> ActivateAsync([FromBody] ActivateTenantDto request)
{
    // 1. Find tenant by slug
    var tenant = await _tenantService.GetBySlugAsync(request.TenantSlug);
    
    // 2. Validate activation token
    if (tenant.ActivationToken != request.ActivationToken)
        return BadRequest("Invalid activation token");
    
    // 3. Check expiry (48 hours)
    if (tenant.ActivationTokenExpiry < DateTime.UtcNow)
        return BadRequest("Activation link has expired");
    
    // 4. Activate tenant
    tenant.Status = "Active";
    tenant.ActivationToken = null;
    tenant.ActivatedAt = DateTime.UtcNow;
    await _unitOfWork.SaveChangesAsync();
    
    // 5. Log audit event
    await _auditService.LogEventAsync(tenantId: tenant.Id, 
        eventType: "TenantActivated", action: "Activate");
    
    // 6. Redirect to login
    return Ok(new { 
        message = "Tenant activated successfully",
        redirectUrl = "/Account/Login"
    });
}
```

### 4.2 First Login Flow
**URL:** `/Account/Login`  
**Controller:** `AccountController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    var result = await _signInManager.PasswordSignInAsync(
        model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
    
    if (result.Succeeded)
    {
        // Load tenant context
        var tenantUser = await _context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.UserId == user.Id);
        
        HttpContext.Session.SetString("TenantId", tenantUser.TenantId.ToString());
        HttpContext.Session.SetString("TenantSlug", tenantUser.Tenant.TenantSlug);
        
        // Check onboarding status
        if (tenantUser.Tenant.OnboardingStatus != "COMPLETED")
        {
            return RedirectToAction("Welcome", "Onboarding");
        }
        
        return RedirectToAction("Index", "Dashboard");
    }
    
    ModelState.AddModelError("", "Invalid login attempt");
    return View(model);
}
```

---

## **📍 PHASE 5: ONBOARDING WIZARD**

### 5.1 Wizard State Management
**Controller:** `src/GrcMvc/Controllers/OnboardingWizardController.cs`

```csharp
public enum OnboardingStep
{
    Welcome = 0,
    OrgProfile = 1,
    ReviewScope = 2,
    CreatePlan = 3,
    Complete = 4
}

// Progress tracking
public class OnboardingProgress
{
    public Guid TenantId { get; set; }
    public OnboardingStep CurrentStep { get; set; }
    public int CompletionPercentage { get; set; }
    public bool IsOrgProfileComplete { get; set; }
    public bool IsScopeReviewed { get; set; }
    public bool IsPlanCreated { get; set; }
}
```

### 5.2 Step 1: Organization Profile
**URL:** `/Onboarding/OrgProfile`

| Field | Type | Required | Stored In |
|-------|------|----------|-----------|
| Organization Name (AR) | text | ✅ | OrganizationProfile |
| Industry | select | ✅ | OrganizationProfile |
| Employee Count | select | ✅ | OrganizationProfile |
| Headquarters | select | ✅ | OrganizationProfile |
| Countries | multiselect | ❌ | OrganizationProfile.Countries (JSON) |
| Annual Revenue | select | ❌ | OrganizationProfile |
| Publicly Traded | toggle | ❌ | OrganizationProfile |
| Government Entity | toggle | ❌ | OrganizationProfile |
| Primary Regulators | multiselect | ✅ | OrganizationProfile.Regulators (JSON) |

### 5.3 Step 2: Review Compliance Scope
**URL:** `/Onboarding/ReviewScope`

```yaml
AI Framework Recommendations:
  Based on:
    - Industry (Financial → SAMA frameworks)
    - Location (Saudi → NCA-ECC mandatory)
    - Data types (PII → PDPL)
    - Size (Large → more controls)

  Recommendation Engine:
    1. Parse OrganizationProfile
    2. Match against framework applicability rules
    3. Return ranked list with reasons
    4. User accepts/rejects each

Creates:
  - TenantBaseline for each accepted framework
  - Links ~100-500 controls per framework
  - Sets compliance targets
```

### 5.4 Step 3: Create First Plan
**URL:** `/Onboarding/CreatePlan`

```yaml
Plan Creation Form:
  - Plan Name: "Q1 2026 Compliance Assessment"
  - Assessment Type: [Gap | Full | Surveillance]
  - Primary Framework: [Selected in scope]
  - Start Date: [Today]
  - Target Completion: [+30 days]
  - Assigned Team: [GRC-CORE]

Creates:
  - Plan record
  - PlanBaseline links
  - Assessment records (one per control)
  - Initial workflow tasks
```

### 5.5 Onboarding Completion
**File:** `OnboardingWizardController.cs:1420-1440`

```csharp
private async Task MarkOnboardingCompleteAsync(Guid tenantId, string userId)
{
    var profile = await _context.OrganizationProfiles
        .FirstOrDefaultAsync(p => p.TenantId == tenantId);
    
    if (profile != null)
    {
        profile.OnboardingStatus = "COMPLETED";
        profile.OnboardingCompletedAt = DateTime.UtcNow;
        profile.OnboardingCompletedBy = userId;
        profile.OnboardingProgressPercent = 100;
    }
    
    var tenant = await _context.Tenants.FindAsync(tenantId);
    if (tenant != null)
    {
        tenant.OnboardingStatus = "COMPLETED";
        tenant.OnboardingCompletedAt = DateTime.UtcNow;
    }
    
    await _context.SaveChangesAsync();
    
    // Trigger auto-provisioning
    await _provisioningService.ProvisionAllAsync(tenantId, userId);
}
```

---

## **📍 PHASE 6: AUTO-PROVISIONING**

### 6.1 Service Location
**File:** `src/GrcMvc/Services/Implementations/OnboardingProvisioningService.cs`

### 6.2 Default Teams Created
```csharp
private static readonly List<TeamTemplate> DefaultTeams = new()
{
    new TeamTemplate
    {
        TeamCode = "GRC-CORE",
        Name = "GRC Core Team",
        NameAr = "فريق الحوكمة الأساسي",
        Purpose = "Core GRC operations - frameworks, assessments, compliance",
        TeamType = "Governance",
        IsDefaultFallback = true,
        DefaultRoles = new[] { "CONTROL_OWNER", "ASSESSOR", "APPROVER" }
    },
    new TeamTemplate
    {
        TeamCode = "SEC-OPS",
        Name = "Security Operations",
        NameAr = "عمليات الأمن",
        Purpose = "Security controls, vulnerability management",
        TeamType = "Operational",
        DefaultRoles = new[] { "CONTROL_OWNER", "EVIDENCE_CUSTODIAN" }
    },
    new TeamTemplate
    {
        TeamCode = "IT-OPS",
        Name = "IT Operations",
        NameAr = "عمليات تقنية المعلومات",
        Purpose = "IT infrastructure, change management",
        TeamType = "Operational",
        DefaultRoles = new[] { "CONTROL_OWNER", "EVIDENCE_CUSTODIAN" }
    },
    new TeamTemplate
    {
        TeamCode = "RISK-MGT",
        Name = "Risk Management",
        NameAr = "إدارة المخاطر",
        Purpose = "Risk assessment and treatment",
        TeamType = "Governance",
        DefaultRoles = new[] { "ASSESSOR", "APPROVER" }
    },
    new TeamTemplate
    {
        TeamCode = "INT-AUDIT",
        Name = "Internal Audit",
        NameAr = "المراجعة الداخلية",
        Purpose = "Independent assurance and testing",
        TeamType = "Governance",
        DefaultRoles = new[] { "ASSESSOR", "VIEWER" }
    }
};
```

### 6.3 RACI Matrix Templates
```csharp
private static readonly List<RACITemplate> DefaultRACITemplates = new()
{
    // Access Control
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "IAM", 
        TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "IAM", 
        TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },
    
    // Network Security
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "NETWORK", 
        TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "NETWORK", 
        TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },
    
    // Change Management
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "CHANGE", 
        TeamCode = "IT-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "CHANGE", 
        TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },
    
    // Business Continuity
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "BCM", 
        TeamCode = "IT-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "BCM", 
        TeamCode = "RISK-MGT", RACI = "A", RoleCode = "APPROVER" },
    
    // Risk Management
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "RISK", 
        TeamCode = "RISK-MGT", RACI = "R", RoleCode = "ASSESSOR" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "RISK", 
        TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },
    
    // Vendor Management
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "VENDOR", 
        TeamCode = "GRC-CORE", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "VENDOR", 
        TeamCode = "RISK-MGT", RACI = "C", RoleCode = "ASSESSOR" },
    
    // Data Protection
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "DATA", 
        TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "DATA", 
        TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },
    
    // Audit
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "AUDIT", 
        TeamCode = "INT-AUDIT", RACI = "R", RoleCode = "ASSESSOR" },
    new RACITemplate { ScopeType = "ControlFamily", ScopeId = "AUDIT", 
        TeamCode = "GRC-CORE", RACI = "I", RoleCode = "VIEWER" }
};
```

---

## **📍 PHASE 7: PRODUCTION GRC PLATFORM**

### 7.1 Dashboard
**URL:** `/Dashboard`

```yaml
Dashboard Widgets:
  Compliance Score:
    - Overall: 0% (new)
    - By Framework: NCA-ECC 0%, PDPL 0%
    
  Active Assessments:
    - Count: 1
    - Due Soon: 0
    
  Tasks:
    - Pending: 114
    - Overdue: 0
    
  Risk Overview:
    - Total Risks: 0
    - Heat Map: Empty
    
  Recent Activity:
    - Empty (new tenant)
```

### 7.2 Core Modules

| Module | URL | Purpose |
|--------|-----|---------|
| Dashboard | `/Dashboard` | Overview & KPIs |
| Assessments | `/Assessment` | Gap & compliance assessments |
| Controls | `/Controls` | Control library management |
| Risks | `/Risks` | Risk register & treatment |
| Evidence | `/Evidence` | Document management |
| Plans | `/Plans` | Assessment planning |
| Reports | `/Reports` | Compliance reporting |
| Teams | `/Teams` | Team & RACI management |
| Settings | `/Settings` | Tenant configuration |

### 7.3 Assessment Workflow
```
Control Selected
    │
    ▼
┌─────────────────┐
│ Implementation  │  ← Assessor selects status
│ Status          │    [Not Implemented | Partial | Implemented | N/A]
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Evidence        │  ← Upload supporting documents
│ Attachment      │    [PDF | Image | Config | Log]
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Findings        │  ← Document gaps (if any)
│ (Optional)      │    [Description | Severity | Recommendation]
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Submit for      │  ← Status: PendingApproval
│ Review          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Approver        │  ← Approve | Reject | Request Changes
│ Review          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Complete        │  ← Status: Approved
└─────────────────┘
```

---

## **📍 PHASE 8: PRODUCTION VERIFICATION**

### 8.1 Pre-Deployment Checklist

```yaml
Infrastructure:
  [ ] Server provisioned (Hetzner/AWS)
  [ ] Docker installed
  [ ] PostgreSQL database running
  [ ] Redis cache configured
  
Domain & SSL:
  [ ] shahin-ai.com DNS configured
  [ ] app.shahin-ai.com DNS configured
  [ ] SSL certificates valid
  [ ] Cloudflare proxy enabled
  
Application:
  [ ] GrcMvc deployed and running
  [ ] Landing page deployed
  [ ] API endpoints accessible
  [ ] CORS configured
  
Email:
  [ ] SMTP server configured
  [ ] SendGrid/Mailgun API key set
  [ ] Email templates in place
  [ ] Test email sent successfully
  
Database:
  [ ] Migrations applied
  [ ] Master data seeded
  [ ] Backup script configured
```

### 8.2 End-to-End Test Script

```bash
#!/bin/bash
# E2E Test Script

echo "=== Phase 8: Production Verification ==="

# 1. DNS Resolution
echo "Checking DNS..."
nslookup shahin-ai.com
nslookup app.shahin-ai.com

# 2. SSL Verification
echo "Checking SSL..."
echo | openssl s_client -connect shahin-ai.com:443 2>/dev/null | grep "Verify return code"
echo | openssl s_client -connect app.shahin-ai.com:443 2>/dev/null | grep "Verify return code"

# 3. Landing Page
echo "Checking landing page..."
curl -sI https://shahin-ai.com | head -2

# 4. API Health
echo "Checking API health..."
curl -s https://app.shahin-ai.com/health

# 5. Trial Signup API
echo "Testing trial signup..."
curl -X POST https://app.shahin-ai.com/api/Landing/StartTrial \
  -H "Content-Type: application/json" \
  -d '{"Email":"e2etest@example.com","FullName":"E2E Test","CompanyName":"Test Corp"}'

# 6. Login Page
echo "Checking login page..."
curl -sI https://app.shahin-ai.com/Account/Login | head -2

echo "=== Verification Complete ==="
```

### 8.3 Monitoring Setup

```yaml
Cloudflare Analytics:
  - Enable Analytics
  - Configure Page Rules
  - Set up Rate Limiting
  
Grafana Dashboards:
  - Application metrics
  - Database performance
  - Error rates
  - Response times
  
Alerts:
  - Downtime detection
  - Error rate threshold
  - Database connection issues
  - SSL expiry warning
```

### 8.4 Backup Configuration

```bash
# Daily database backup
0 2 * * * /home/Shahin-ai/Shahin-Jan-2026/scripts/backup-all-tenants.sh

# Weekly full backup
0 3 * * 0 tar -czf /backups/grc-full-$(date +\%Y\%m\%d).tar.gz /home/Shahin-ai/Shahin-Jan-2026/
```

---

## **🎯 PRODUCTION READINESS CHECKLIST**

### Critical Path
- [ ] Landing page live at `https://shahin-ai.com`
- [ ] API endpoint `POST /api/Landing/StartTrial` working
- [ ] CORS allowing cross-origin requests
- [ ] Email activation flow complete
- [ ] Onboarding wizard 3 steps functional
- [ ] Auto-provisioning creates teams/RACI
- [ ] Dashboard accessible after onboarding

### Secondary Requirements  
- [ ] Arabic/English localization
- [ ] Assessment workflow functional
- [ ] Evidence upload working
- [ ] Reports generating
- [ ] PDF export working
- [ ] Audit logging active

### Infrastructure
- [ ] SSL certificates valid (auto-renewal)
- [ ] Database backups scheduled
- [ ] Monitoring dashboards configured
- [ ] Error tracking (Sentry) active
- [ ] Log aggregation configured

---

## **🔧 TROUBLESHOOTING**

### Common Issues

| Issue | Solution |
|-------|----------|
| CORS error on trial signup | Check `Program.cs` CORS policy includes landing domain |
| Email not received | Verify SMTP settings in `.env`, check spam folder |
| 502 Bad Gateway | Check if GrcMvc container is running |
| Onboarding stuck | Check `OnboardingStatus` in Tenant table |
| Teams not created | Manually call `ProvisionAllAsync` |

### Debug Commands

```bash
# Check running containers
docker ps

# View application logs
docker logs grcmvc --tail 100

# Check database connection
docker exec -it postgres psql -U grc -d grc_db -c "SELECT 1"

# Test email
curl -X POST https://app.shahin-ai.com/api/test/email
```

---

**Document Version:** 1.0  
**Last Updated:** January 10, 2026  
**Author:** Shahin AI Platform Team
