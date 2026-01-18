# How to Monitor Application Logs in Real-Time

## Method 1: Using the Monitoring Script (Recommended)

```powershell
# Run the monitoring script
powershell -ExecutionPolicy Bypass -File scripts\start-and-monitor.ps1
```

This script will:
- Start the application
- Monitor logs automatically
- Show you when "Auth database migrations applied" appears
- Display the last 10 log lines in real-time

## Method 2: Manual Start with Log Monitoring

### Step 1: Start Application with Log Redirection

```powershell
# Navigate to release directory
cd src\GrcMvc\bin\Release\net8.0

# Set environment variables
$env:JWT_SECRET="your-secret-here"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432"
$env:ASPNETCORE_ENVIRONMENT="Production"

# Start application and redirect output
Start-Process -FilePath "dotnet" -ArgumentList "GrcMvc.dll" -NoNewWindow -RedirectStandardOutput "startup.log" -RedirectStandardError "startup-errors.log"
```

### Step 2: Monitor Logs in Real-Time

**Option A: Watch log file continuously**
```powershell
# Monitor last 20 lines, updates automatically
Get-Content startup.log -Wait -Tail 20
```

**Option B: Check log periodically**
```powershell
# Check last 30 lines
Get-Content startup.log -Tail 30

# Search for migration messages
Get-Content startup.log | Select-String -Pattern "migration|Migration|Auth database|applied"
```

**Option C: Filter for specific messages**
```powershell
# Watch only migration-related messages
Get-Content startup.log -Wait | Where-Object { $_ -match "migration|Migration|Auth database|applied" }
```

### Step 3: Look for These Messages

**Success Messages:**
```
🔄 Applying Auth database migrations...
✅ Auth database migrations applied
```

**Or:**
```
Applying Auth database migrations...
Auth database migrations applied
```

**Error Messages to Watch For:**
```
❌ Database connection failed
❌ Migration failed
❌ JWT_SECRET environment variable is required
```

## Method 3: Using PowerShell Jobs (Advanced)

```powershell
# Start application as background job
$job = Start-Job -ScriptBlock {
    Set-Location "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\bin\Release\net8.0"
    $env:JWT_SECRET="your-secret"
    $env:ASPNETCORE_ENVIRONMENT="Production"
    dotnet GrcMvc.dll
}

# Monitor job output
Receive-Job $job -Wait
```

## Method 4: Direct Console Output

If you want to see output directly in the console:

```powershell
cd src\GrcMvc\bin\Release\net8.0
$env:JWT_SECRET="your-secret"
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet GrcMvc.dll
```

This will show all output directly, but you can't easily search/filter it.

---

## Quick Commands Reference

### Check if Application is Running
```powershell
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*GrcMvc*" }
```

### View Full Log File
```powershell
Get-Content startup.log
```

### Search Log for Specific Text
```powershell
# Find migration messages
Get-Content startup.log | Select-String -Pattern "migration|Migration"

# Find errors
Get-Content startup-errors.log | Select-String -Pattern "error|Error|Exception"
```

### Stop Application
```powershell
# Find and stop the process
Get-Process -Name "dotnet" | Where-Object { $_.Path -like "*GrcMvc*" } | Stop-Process
```

---

## What to Look For

### ✅ Success Indicators

1. **Migration Applied:**
   - "Auth database migrations applied"
   - "Applying Auth database migrations..."
   - "Done." (from EF Core)

2. **Application Started:**
   - "Now listening on: http://..."
   - "Application started"
   - "Hosting environment: Production"

### ❌ Error Indicators

1. **Database Connection:**
   - "Unable to connect to database"
   - "Connection refused"
   - "Authentication failed"

2. **Migration Issues:**
   - "Migration failed"
   - "Table already exists"
   - "Column already exists"

3. **Configuration:**
   - "JWT_SECRET environment variable is required"
   - "Connection string not found"

---

## Troubleshooting

### Log File Not Created

**Problem:** `startup.log` doesn't exist after starting application

**Solution:**
- Check if application actually started
- Check `startup-errors.log` for errors
- Verify you're in the correct directory
- Check file permissions

### Can't See Migration Message

**Problem:** Log shows application started but no migration message

**Solution:**
1. Check if migration was already applied:
   ```bash
   dotnet ef migrations list --context GrcAuthDbContext
   ```
2. If migration shows as "Pending", it will be applied on startup
3. If migration is already applied, you won't see the message (it only shows when applying)

### Application Crashes Immediately

**Check:**
1. `startup-errors.log` for error details
2. Environment variables are set correctly
3. Database is accessible
4. Port is not in use

---

## Example: Complete Monitoring Session

```powershell
# 1. Start application
cd src\GrcMvc\bin\Release\net8.0
$env:JWT_SECRET="your-secret"
$env:ASPNETCORE_ENVIRONMENT="Production"
Start-Process -FilePath "dotnet" -ArgumentList "GrcMvc.dll" -NoNewWindow -RedirectStandardOutput "startup.log"

# 2. Wait a few seconds
Start-Sleep -Seconds 5

# 3. Monitor logs
Get-Content startup.log -Wait -Tail 20

# 4. In another terminal, search for migration
Get-Content startup.log | Select-String -Pattern "migration|applied"
```

---

**💡 Tip:** Keep the monitoring script running in one terminal, and use another terminal to test the application or run database queries.
