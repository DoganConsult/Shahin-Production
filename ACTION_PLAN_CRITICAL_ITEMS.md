# CRITICAL ITEMS - ACTION PLAN
**Date:** January 10, 2026
**Priority:** 🔴 URGENT - Must complete before production
**Estimated Time:** 38 hours (Week 1-2)
**Team Size:** 2 developers
**Timeline:** 5 working days

---

## 🎯 OBJECTIVE

Complete all **critical blockers** and **high-priority gaps** to achieve **95%+ production readiness** for Stages 1-3 (core GRC functionality).

---

## 📋 WEEK 1: CRITICAL BLOCKERS (Day 1-3)

### Day 1: Infrastructure Security (3 hours)

#### Task 1.1: Generate SSL Certificates ⏱️ 15 minutes

**Location:** `src/GrcMvc/certificates/`

**Steps:**
```bash
cd /home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc
mkdir -p certificates
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "SecurePassword123!"
dotnet dev-certs https --trust
```

**Verification:**
```bash
ls -la certificates/aspnetapp.pfx
openssl pkcs12 -info -in certificates/aspnetapp.pfx -noout
```

**Files to Update:**
- `appsettings.Production.json` - Add certificate path
- `.env.grcmvc.secure` - Add certificate password

---

#### Task 1.2: Configure SMTP OAuth2 ⏱️ 2 hours

**Required Environment Variables:**

Create `.env.grcmvc.secure` with:
```bash
# Azure AD Configuration
AZURE_TENANT_ID=<your-azure-tenant-id>

# SMTP OAuth2 (Microsoft Graph)
SMTP_CLIENT_ID=<smtp-app-registration-client-id>
SMTP_CLIENT_SECRET=<smtp-app-registration-secret>

# Microsoft Graph API
MSGRAPH_CLIENT_ID=<graph-app-registration-client-id>
MSGRAPH_CLIENT_SECRET=<graph-app-registration-secret>
MSGRAPH_APP_ID_URI=api://<graph-app-id>

# SMTP Settings
SMTP_SERVER=smtp.office365.com
SMTP_PORT=587
SMTP_FROM_EMAIL=noreply@shahin-ai.com
SMTP_FROM_NAME=Shahin GRC
```

**Azure Portal Steps:**
1. Navigate to Azure Active Directory
2. App Registrations → New registration
3. Name: "Shahin GRC SMTP Service"
4. API Permissions → Microsoft Graph → Mail.Send
5. Certificates & secrets → New client secret
6. Copy Client ID and Secret to .env file

**Files to Update:**
- `src/GrcMvc/Services/Implementations/EmailService.cs` - Verify OAuth2 integration
- `src/GrcMvc/appsettings.Production.json` - Reference env vars

**Testing:**
```bash
# Test email sending
curl -X POST https://localhost:5001/api/test/send-email \
  -H "Content-Type: application/json" \
  -d '{"to":"test@example.com","subject":"Test","body":"Test email"}'
```

---

### Day 2: Database Backups (3 hours)

#### Task 2.1: Create Backup Configuration ⏱️ 1 hour

**Create:** `backup-config.yml`

```yaml
backup:
  enabled: true
  schedule: "0 2 * * *"  # Daily at 2 AM
  retention:
    daily: 7
    weekly: 4
    monthly: 12

  destinations:
    - type: azure_blob
      container: grc-backups
      connection_string: ${BACKUP_STORAGE_CONNECTION}

    - type: local
      path: /var/backups/grc
      max_size_gb: 100

  databases:
    - name_pattern: "GrcDb_*"
      type: full
      compression: true

    - name_pattern: "GrcDb_*"
      type: incremental
      schedule: "0 */6 * * *"  # Every 6 hours

  notifications:
    on_success: false
    on_failure: true
    email: admin@shahin-ai.com
    webhook: ${BACKUP_WEBHOOK_URL}
```

**Environment Variables:**
```bash
BACKUP_STORAGE_CONNECTION=<azure-storage-connection-string>
BACKUP_SCHEDULE_CRON="0 2 * * *"
BACKUP_RETENTION_DAYS=30
BACKUP_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/WEBHOOK/URL
```

---

#### Task 2.2: Create Backup Script ⏱️ 1 hour

**Create:** `scripts/backup-databases.sh`

```bash
#!/bin/bash

# GRC Database Backup Script
# Runs daily at 2 AM via cron

set -e

BACKUP_DIR="/var/backups/grc"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=30

# Load environment variables
source /home/Shahin-ai/Shahin-Jan-2026/.env.grcmvc.secure

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Function to backup single database
backup_database() {
    local db_name=$1
    local backup_file="$BACKUP_DIR/${db_name}_${TIMESTAMP}.bak"

    echo "Backing up $db_name..."

    # SQL Server backup command
    sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q \
        "BACKUP DATABASE [$db_name] TO DISK = N'$backup_file' WITH COMPRESSION, STATS = 10"

    # Verify backup
    if [ -f "$backup_file" ]; then
        echo "✅ Backup successful: $backup_file"

        # Upload to Azure Blob Storage
        if [ ! -z "$BACKUP_STORAGE_CONNECTION" ]; then
            az storage blob upload \
                --connection-string "$BACKUP_STORAGE_CONNECTION" \
                --container-name grc-backups \
                --file "$backup_file" \
                --name "$(basename $backup_file)"
            echo "✅ Uploaded to Azure: $backup_file"
        fi
    else
        echo "❌ Backup failed: $db_name"
        send_failure_notification "$db_name"
        exit 1
    fi
}

# Function to send failure notification
send_failure_notification() {
    local db_name=$1

    if [ ! -z "$BACKUP_WEBHOOK_URL" ]; then
        curl -X POST "$BACKUP_WEBHOOK_URL" \
            -H "Content-Type: application/json" \
            -d "{\"text\":\"❌ Backup failed for database: $db_name\"}"
    fi
}

# Get list of GRC databases
databases=$(sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -h -1 -W \
    -Q "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name LIKE 'GrcDb_%'")

# Backup each database
for db in $databases; do
    backup_database "$db"
done

# Cleanup old backups (older than retention period)
find "$BACKUP_DIR" -name "*.bak" -mtime +$RETENTION_DAYS -delete
echo "✅ Cleanup complete: removed backups older than $RETENTION_DAYS days"

echo "✅ All backups completed successfully!"
```

**Make executable:**
```bash
chmod +x scripts/backup-databases.sh
```

---

#### Task 2.3: Setup Cron Job ⏱️ 30 minutes

**Add to crontab:**
```bash
crontab -e

# Add this line:
0 2 * * * /home/Shahin-ai/Shahin-Jan-2026/scripts/backup-databases.sh >> /var/log/grc-backup.log 2>&1
```

**Test backup manually:**
```bash
./scripts/backup-databases.sh
```

**Verify:**
```bash
ls -lh /var/backups/grc/
az storage blob list --connection-string "$BACKUP_STORAGE_CONNECTION" --container-name grc-backups
```

---

#### Task 2.4: Create Restore Script ⏱️ 30 minutes

**Create:** `scripts/restore-database.sh`

```bash
#!/bin/bash

# GRC Database Restore Script

if [ $# -ne 2 ]; then
    echo "Usage: $0 <backup_file> <target_database_name>"
    exit 1
fi

BACKUP_FILE=$1
TARGET_DB=$2

source /home/Shahin-ai/Shahin-Jan-2026/.env.grcmvc.secure

echo "Restoring $TARGET_DB from $BACKUP_FILE..."

sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q \
    "RESTORE DATABASE [$TARGET_DB] FROM DISK = N'$BACKUP_FILE' WITH REPLACE, STATS = 10"

if [ $? -eq 0 ]; then
    echo "✅ Restore successful: $TARGET_DB"
else
    echo "❌ Restore failed: $TARGET_DB"
    exit 1
fi
```

**Make executable:**
```bash
chmod +x scripts/restore-database.sh
```

---

### Day 3-4: Remediation Module (16 hours)

#### Task 3.1: Create RemediationController ⏱️ 2 hours

**File:** `src/GrcMvc/Controllers/RemediationController.cs`

**Required Actions:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Controllers
{
    [Authorize]
    public class RemediationController : Controller
    {
        private readonly IRemediationService _remediationService;

        public RemediationController(IRemediationService remediationService)
        {
            _remediationService = remediationService;
        }

        // GET: /Remediation
        public async Task<IActionResult> Index()
        {
            var plans = await _remediationService.GetAllAsync();
            return View(plans);
        }

        // GET: /Remediation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Remediation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRemediationDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _remediationService.CreateAsync(dto);
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }

        // Additional actions: Edit, Details, Delete, Dashboard, Tracking, Statistics
    }
}
```

---

#### Task 3.2: Create Remediation Views ⏱️ 10 hours

**Views to Create:**

1. **Index.cshtml** (2 hours)
   - Remediation plans listing with filtering
   - Priority badges (Critical/High/Medium/Low)
   - Status indicators (Planned/InProgress/Completed)
   - Progress bars for each plan

2. **Create.cshtml** (1.5 hours)
   - Form for creating remediation plan
   - Fields: Title, Description, Priority, Due Date, Assignee
   - Related findings/issues selector
   - Action items list builder

3. **Edit.cshtml** (1.5 hours)
   - Edit remediation plan
   - Update status workflow
   - Add/remove action items

4. **Details.cshtml** (2 hours)
   - Full remediation plan details
   - Timeline visualization
   - Action items with checkboxes
   - Comments/notes section
   - Attachments/evidence

5. **Dashboard.cshtml** (2 hours)
   - Remediation metrics dashboard
   - Plans by status (pie chart)
   - Plans by priority (bar chart)
   - Overdue plans alert
   - Completion rate trend

6. **Tracking.cshtml** (1 hour)
   - Track remediation progress
   - Gantt chart visualization
   - Milestone tracking

---

#### Task 3.3: Create IRemediationService ⏱️ 2 hours

**File:** `src/GrcMvc/Services/Interfaces/IRemediationService.cs`

```csharp
public interface IRemediationService
{
    Task<RemediationPlanDto> CreateAsync(CreateRemediationDto dto);
    Task<RemediationPlanDto> UpdateAsync(Guid id, UpdateRemediationDto dto);
    Task<RemediationPlanDto> GetByIdAsync(Guid id);
    Task<List<RemediationPlanDto>> GetAllAsync();
    Task<List<RemediationPlanDto>> GetByPriorityAsync(Priority priority);
    Task<List<RemediationPlanDto>> GetOverdueAsync();
    Task<RemediationStatisticsDto> GetStatisticsAsync();
    Task UpdateProgressAsync(Guid id, int progressPercent);
    Task CompleteAsync(Guid id);
    Task DeleteAsync(Guid id);
}
```

---

#### Task 3.4: Implement RemediationService ⏱️ 2 hours

**File:** `src/GrcMvc/Services/Implementations/RemediationService.cs`

```csharp
public class RemediationService : IRemediationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemediationService> _logger;
    private readonly INotificationService _notificationService;

    // Implement all interface methods with proper error handling
    // Include notifications for status changes
    // Track history/audit trail
}
```

---

## 📋 WEEK 2: HIGH PRIORITY GAPS (Day 5-7)

### Day 5: Assessment Specialized Views (8 hours)

#### Task 4.1: StatusWorkflow.cshtml ⏱️ 3 hours

**File:** `src/GrcMvc/Views/Assessment/StatusWorkflow.cshtml`

**Features:**
- Visual workflow diagram (Draft → InProgress → Review → Approved)
- Current status highlighted
- Valid transitions shown as buttons
- Status history timeline
- Approval comments section

**UI Components:**
- SVG workflow diagram
- Status badges with colors
- Transition buttons (enabled/disabled based on permissions)
- Audit trail table

---

#### Task 4.2: Scoring.cshtml ⏱️ 3 hours

**File:** `src/GrcMvc/Views/Assessment/Scoring.cshtml`

**Features:**
- Scoring matrix (Likelihood × Impact)
- 5×5 grid with color coding
- Click to select score
- Auto-calculate risk level
- Score justification text area
- Historical scores comparison

**UI Components:**
- Interactive matrix grid
- Score visualization
- Chart.js line chart for trends

---

#### Task 4.3: ReviewQueue.cshtml ⏱️ 2 hours

**File:** `src/GrcMvc/Views/Assessment/ReviewQueue.cshtml`

**Features:**
- Pending assessments requiring review
- Filter by priority/age/assignee
- Bulk approve/reject
- Review workload dashboard
- SLA countdown timers

---

### Day 6: Evidence & Control Views (7 hours)

#### Task 5.1: Evidence VerificationQueue.cshtml ⏱️ 3 hours

**File:** `src/GrcMvc/Views/Evidence/VerificationQueue.cshtml`

**Features:**
- Evidence verification workflow
- Preview evidence files (PDF/images)
- Approve/Reject with comments
- Verification checklist
- Batch operations

---

#### Task 5.2: Control TestingWorkflow.cshtml ⏱️ 4 hours

**File:** `src/GrcMvc/Views/Controls/TestingWorkflow.cshtml`

**Features:**
- Control testing schedule
- Testing procedures/scripts
- Results entry form
- Pass/Fail/Partial status
- Findings linkage
- Retest tracking

---

### Day 7: Localization (2 hours)

#### Task 6.1: Risk Module Localization ⏱️ 2 hours

**Files to Create:**

1. **Resources/Risk.en.resx** (1 hour)

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Risk_Name_Required" xml:space="preserve">
    <value>Risk name is required</value>
  </data>
  <data name="Risk_Name_MinLength" xml:space="preserve">
    <value>Risk name must be at least 3 characters</value>
  </data>
  <!-- 15-20 more translation keys -->
</root>
```

2. **Resources/Risk.ar.resx** (1 hour)

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Risk_Name_Required" xml:space="preserve">
    <value>اسم المخاطرة مطلوب</value>
  </data>
  <data name="Risk_Name_MinLength" xml:space="preserve">
    <value>يجب أن يكون اسم المخاطرة 3 أحرف على الأقل</value>
  </data>
  <!-- 15-20 more translation keys -->
</root>
```

3. **Update RiskValidators.cs**

```csharp
// Before:
.WithMessage("اسم المخاطرة مطلوب | Risk name is required")

// After:
.WithMessage(_localizer["Risk_Name_Required"])
```

---

## ✅ VERIFICATION CHECKLIST

### Week 1 Completion Criteria:

- [ ] SSL certificate generated and working
- [ ] HTTPS endpoint responding (https://localhost:5001)
- [ ] SMTP OAuth2 configured
- [ ] Test email sent successfully
- [ ] Backup script executes without errors
- [ ] Cron job scheduled
- [ ] Test restore completes successfully
- [ ] RemediationController created with all actions
- [ ] All 6 remediation views created and rendering
- [ ] Remediation service implemented
- [ ] Remediation module tested end-to-end

### Week 2 Completion Criteria:

- [ ] StatusWorkflow.cshtml renders workflow diagram
- [ ] Scoring.cshtml matrix interactive
- [ ] ReviewQueue.cshtml showing pending items
- [ ] VerificationQueue.cshtml functional
- [ ] TestingWorkflow.cshtml complete
- [ ] Risk.en.resx and Risk.ar.resx created
- [ ] Validators using localization
- [ ] All localized messages displaying correctly

---

## 🎯 SUCCESS METRICS

After completing this 2-week plan:

| Module | Before | After |
|--------|--------|-------|
| Risk | 98% | 100% |
| Assessment | 90% | 98% |
| Evidence | 95% | 100% |
| Control | 92% | 100% |
| Audit | 93% | 93% |
| Remediation | 20% | 95% |

**Overall Core Completion:** **81%** → **98%**

**Production Readiness:** ✅ **APPROVED**

---

## 📊 TIMELINE SUMMARY

```
Week 1: CRITICAL BLOCKERS
├─ Day 1: SSL + SMTP OAuth2 (3h)
├─ Day 2: Database Backups (3h)
└─ Day 3-4: Remediation Module (16h)
   Total: 22 hours

Week 2: HIGH PRIORITY
├─ Day 5: Assessment Views (8h)
├─ Day 6: Evidence + Control Views (7h)
└─ Day 7: Localization (2h)
   Total: 17 hours

TOTAL EFFORT: 39 hours
TEAM SIZE: 2 developers
DURATION: 5 working days (2 weeks)
```

---

## 🚀 NEXT STEPS AFTER COMPLETION

Once Week 1-2 is complete:

1. **Deploy to Staging** (Week 3)
   - Full QA testing
   - Security penetration testing
   - Performance testing
   - User acceptance testing

2. **Production Deployment** (Week 4)
   - Deploy Stages 1-3
   - Monitor for 1 week

3. **Phase 2: Advanced Features** (Weeks 5-13)
   - Resilience module (Weeks 5-7)
   - Excellence/Benchmarking (Weeks 8-10)
   - Sustainability (Weeks 11-13)

---

**Status:** ✅ READY TO START
**Priority:** 🔴 CRITICAL
**Owner:** Development Team
**Start Date:** January 11, 2026
**Target Completion:** January 22, 2026

---

**End of Action Plan**
