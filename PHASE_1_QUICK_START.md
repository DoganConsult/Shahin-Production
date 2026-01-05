# PHASE 1 QUICK START GUIDE

## 🚀 Get Phase 1 Running in 5 Steps

### Step 1: Build the Project (2 minutes)
```bash
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet restore src/GrcMvc/GrcMvc.csproj
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

**Expected Result**: Build succeeded

---

### Step 2: Apply Database Migration (1 minute)
```bash
cd src/GrcMvc
dotnet ef database update --context GrcDbContext
```

**Expected Result**: Applied migration '20250115_Phase1FrameworkHRISAuditTables'

---

### Step 3: Start the Application
```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet run
```

**Expected Result**: Application running at https://localhost:5001

---

### Step 4: Verify Services Are Registered
Add this code to verify services are available:
```csharp
// In Program.cs or a controller
var frameworkService = serviceProvider.GetRequiredService<IFrameworkService>();
var hrisService = serviceProvider.GetRequiredService<IHRISService>();
var auditTrail = serviceProvider.GetRequiredService<IAuditTrailService>();
var rulesEngine = serviceProvider.GetRequiredService<IRulesEngineService>();

Console.WriteLine("✅ All Phase 1 services initialized successfully!");
```

---

### Step 5: Test Framework Service
Create a test in your test project:
```csharp
[Fact]
public async Task TestFrameworkCreation()
{
    var tenantId = Guid.NewGuid();
    
    // Create framework
    var framework = await _frameworkService.CreateFrameworkAsync(
        tenantId: tenantId,
        name: "ISO 27001",
        code: "ISO27001",
        description: "Information Security Management"
    );

    // Verify it was created
    Assert.NotNull(framework);
    Assert.Equal("ISO 27001", framework.FrameworkName);
    
    // Verify audit log was created
    var auditLogs = await _auditTrail.GetEntityAuditHistoryAsync(framework.FrameworkId);
    Assert.NotEmpty(auditLogs);
    Assert.True(auditLogs.Any(al => al.Action == "Created"));
}
```

---

## 📊 Quick Validation

### Check Database Tables (PostgreSQL)
```sql
-- Connect to your PostgreSQL database
\dt public.*

-- You should see these 11 new tables:
-- Frameworks
-- Controls
-- ControlOwnerships
-- ControlEvidences
-- Baselines
-- BaselineControls
-- HRISIntegrations
-- HRISEmployees
-- AuditLogs
-- ComplianceSnapshots
-- ControlTestResults
```

---

## 🔍 Service API Quick Reference

### FrameworkService
```csharp
// Inject into controller or service
private readonly IFrameworkService _frameworkService;

// Create framework
var framework = await _frameworkService.CreateFrameworkAsync(
    tenantId: Guid.NewGuid(),
    name: "NIST Cybersecurity Framework",
    code: "NIST",
    description: "Federal framework for managing cybersecurity risk"
);

// Get all frameworks
var frameworks = await _frameworkService.GetAllFrameworksAsync(tenantId);

// Create control
var control = await _frameworkService.CreateControlAsync(
    tenantId: tenantId,
    frameworkId: framework.FrameworkId,
    controlCode: "ID.AM-1",
    controlName: "Physical Devices and Inventory",
    description: "All devices and software..."
);

// Assign control to user
var ownership = await _frameworkService.AssignControlOwnerAsync(
    tenantId: tenantId,
    controlId: control.ControlId,
    userId: userId
);

// Record control test
var testResult = await _frameworkService.RecordControlTestAsync(
    controlId: control.ControlId,
    testedByUserId: userId,
    result: "Passed",
    score: 95.0
);

// Get controls owned by user
var myControls = await _frameworkService.GetControlsByOwnerAsync(
    tenantId: tenantId,
    userId: userId
);
```

### HRISService
```csharp
// Inject into controller or service
private readonly IHRISService _hrisService;

// Create HRIS integration
var integration = await _hrisService.CreateIntegrationAsync(
    tenantId: tenantId,
    system: "SAP",
    endpoint: "https://sap.company.com/api",
    authType: "OAuth2"
);

// Test connection
var isConnected = await _hrisService.TestConnectionAsync(integration.IntegrationId);

// Sync employees
var syncedCount = await _hrisService.SyncEmployeesAsync(integration.IntegrationId);

// Get all employees
var employees = await _hrisService.GetAllEmployeesAsync(tenantId, activeOnly: true);

// Create users from HRIS
var createdCount = await _hrisService.CreateUsersFromHRISAsync(tenantId, integration.IntegrationId);

// Get job title mapping
var role = await _hrisService.MapJobTitleToRoleAsync("Senior Manager");
// Returns: "Manager"
```

### AuditTrailService
```csharp
// Inject into controller or service
private readonly IAuditTrailService _auditTrail;

// Log a change manually
await _auditTrail.LogChangeAsync(
    tenantId: tenantId,
    entityType: "Control",
    entityId: controlId,
    action: "Updated",
    fieldName: "ComplianceStatus",
    oldValue: "Planned",
    newValue: "In Progress",
    userId: userId
);

// Get entity history
var history = await _auditTrail.GetEntityAuditHistoryAsync(controlId);
foreach (var log in history)
{
    Console.WriteLine($"{log.Action} - {log.FieldName}: {log.OldValue} → {log.NewValue}");
}

// Get user's recent changes
var userActivity = await _auditTrail.GetUserAuditHistoryAsync(userId, days: 30);

// Search audit logs
var creations = await _auditTrail.SearchAuditLogsAsync(
    tenantId: tenantId,
    entityType: "Control",
    action: "Created"
);
```

### RulesEngineService
```csharp
// Inject into controller or service
private readonly IRulesEngineService _rulesEngine;

// Derive applicable frameworks
var frameworks = await _rulesEngine.DeriveApplicableFrameworksAsync(
    tenantId: tenantId,
    country: "Saudi Arabia",
    sector: "Finance",
    dataType: "PII"
);
// Returns: [SAMA, PDPL, SOC2, GDPR, ISO27001]

// Derive applicable controls
var controls = await _rulesEngine.DeriveApplicableControlsAsync(
    tenantId: tenantId,
    frameworkIds: frameworks.Select(f => f.FrameworkId).ToList()
);

// Select baseline
var baseline = await _rulesEngine.SelectBaselineAsync(
    tenantId: tenantId,
    frameworkId: frameworkId,
    businessSize: "Enterprise",
    maturityGoal: 3
);
```

---

## 📝 Common Tasks

### Task 1: Set Up ISO 27001 Framework
```csharp
var tenantId = Guid.NewGuid();

// Create framework
var iso = await _frameworkService.CreateFrameworkAsync(
    tenantId,
    "ISO 27001",
    "ISO27001",
    "Information Security Management Systems"
);

// Create baseline
var baseline = await _frameworkService.CreateBaselineAsync(
    tenantId,
    iso.FrameworkId,
    "Enterprise",
    "Financial Services"
);

// Import controls (manual for now, automated in Week 2)
for (int i = 1; i <= 114; i++)
{
    var control = await _frameworkService.CreateControlAsync(
        tenantId,
        iso.FrameworkId,
        $"A.{i}",
        $"Control {i}",
        "ISO 27001 Control"
    );
    
    await _frameworkService.AddControlToBaselineAsync(
        baseline.BaselineId,
        control.ControlId,
        priority: i <= 20 ? 1 : 2 // Top 20 are critical
    );
}
```

### Task 2: Set Up HRIS Integration
```csharp
var tenantId = Guid.NewGuid();

// Create integration
var hris = await _hrisService.CreateIntegrationAsync(
    tenantId,
    "SAP",
    "https://sap.company.com/api",
    "OAuth2"
);

// Test connection
var connected = await _hrisService.TestConnectionAsync(hris.IntegrationId);
if (!connected)
{
    Console.WriteLine("HRIS connection failed!");
    return;
}

// Sync employees
var count = await _hrisService.SyncEmployeesAsync(hris.IntegrationId);
Console.WriteLine($"Synced {count} employees");

// Create user accounts
var created = await _hrisService.CreateUsersFromHRISAsync(tenantId, hris.IntegrationId);
Console.WriteLine($"Created {created} user accounts");
```

### Task 3: Assign Control to User
```csharp
var tenantId = Guid.NewGuid();
var userId = Guid.NewGuid();
var controlId = Guid.NewGuid();

// Assign control
var ownership = await _frameworkService.AssignControlOwnerAsync(
    tenantId,
    controlId,
    userId
);

// Get all controls owned by user
var myControls = await _frameworkService.GetControlsByOwnerAsync(
    tenantId,
    userId
);

Console.WriteLine($"User owns {myControls.Count} controls");
```

### Task 4: Test Control
```csharp
// Record test result
var testResult = await _frameworkService.RecordControlTestAsync(
    controlId: controlId,
    testedByUserId: userId,
    result: "Passed",
    score: 92.5
);

// Calculate effectiveness (90-day average)
var effectiveness = await _frameworkService.CalculateControlEffectivenessAsync(
    controlId,
    days: 90
);

Console.WriteLine($"Control effectiveness: {effectiveness:P}");
```

---

## ⚙️ Configuration

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=grc_db;Username=postgres;Password=your_password"
  }
}
```

### Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "GrcMvc.Services": "Debug"
    }
  }
}
```

---

## 🐛 Troubleshooting

### Build Error: "Could not restore packages"
```bash
dotnet nuget add source https://api.nuget.org/v3/index.json
dotnet restore
```

### Migration Error: "Table already exists"
```bash
dotnet ef database update 0  # Revert all migrations
dotnet ef database update    # Re-apply migrations
```

### Service Not Registered Error
- Verify Program.cs has the 4 AddScoped lines
- Check for typos in service names
- Ensure NuGet packages are updated

### Database Connection Error
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Verify database exists
- Check user permissions

---

## 📞 Support

For issues, questions, or clarifications:
1. Check PHASE_1_BUILD_DEPLOYMENT.md for detailed setup
2. Review PHASE_1_IMPLEMENTATION_COMPLETE.md for full documentation
3. Check application logs for detailed error messages
4. Verify all files were created in correct locations

---

## ✅ Phase 1 is Ready!

### What You Have Now
- ✅ 11 new database tables
- ✅ 40+ service methods
- ✅ Framework management
- ✅ HRIS integration framework
- ✅ Complete audit trail
- ✅ Compliance scoring
- ✅ Rules engine for scope

### What's Next (Week 2)
- Framework data import (500+ controls)
- HRIS connector implementation
- Testing and validation
- Go/No-Go checkpoint

---

**Ready to build?** Start with Step 1 above! 🚀

You should be live in < 10 minutes!
