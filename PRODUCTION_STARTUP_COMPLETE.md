# Production Application Startup Complete

**Date**: January 15, 2026  
**Status**: Application Started in Production Mode

---

## ✅ Application Started

The application has been started in **Production** mode with the following configuration:

### Environment Variables Set:
- ✅ `ASPNETCORE_ENVIRONMENT` = Production
- ✅ `JWT_SECRET` = Generated (64 characters)
- ✅ `ConnectionStrings__DefaultConnection` = Configured
- ✅ `ConnectionStrings__GrcAuthDb` = Configured
- ✅ `ASPNETCORE_URLS` = http://0.0.0.0:5000

### Application Process:
- **Status**: Running
- **Log Files**: 
  - `startup.log` - Standard output
  - `startup-errors.log` - Error output

---

## 🔍 How to Monitor Logs in Real-Time

### Method 1: Watch Log File Continuously

```powershell
# Navigate to application directory
cd src\GrcMvc\bin\Release\net8.0

# Watch logs in real-time (last 20 lines, updates automatically)
Get-Content startup.log -Wait -Tail 20
```

**Press Ctrl+C to stop monitoring**

### Method 2: Check Logs Periodically

```powershell
# View last 30 lines
Get-Content startup.log -Tail 30

# Search for migration messages
Get-Content startup.log | Select-String -Pattern "migration|Migration|Auth database|applied"
```

### Method 3: Filter for Specific Messages

```powershell
# Watch only migration-related messages
Get-Content startup.log -Wait | Where-Object { $_ -match "migration|Migration|Auth database|applied" }
```

---

## ✅ What to Look For in Logs

### Success Messages:

**Migration Applied:**
```
🔄 Applying Auth database migrations...
✅ Auth database migrations applied
```

**Or:**
```
Applying Auth database migrations...
Auth database migrations applied
Done.
```

**Application Started:**
```
Now listening on: http://0.0.0.0:5000
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
```

### Error Messages (If Any):

**Database Connection:**
```
Unable to connect to database
Connection refused
```

**Configuration:**
```
JWT_SECRET environment variable is required
Connection string not found
```

---

## 🧪 Next Steps: Test the Application

### 1. Verify Application is Running

```powershell
# Check if process is running
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*GrcMvc*" }

# Test HTTP endpoint (if curl available)
curl http://localhost:5000/health/ready
```

### 2. Access the Application

Open in browser:
- **URL**: `http://localhost:5000`
- **Health Check**: `http://localhost:5000/health/ready`
- **Swagger**: `http://localhost:5000/api-docs` (if enabled)

### 3. Test User Forms

1. **Create New User:**
   - Navigate to: `/Users/Create` or `/Account/Register`
   - Fill in all fields including:
     - First Name, Last Name
     - Department, Job Title
     - Abilities: `["Ability1", "Ability2"]` (JSON array)
     - Assigned Scope
     - KSA Competency Level
   - Save and verify success

2. **Edit Existing User:**
   - Navigate to: `/Users/Edit/{userId}`
   - Verify all fields load correctly
   - Modify fields and save
   - Verify changes persist

### 4. Verify Database Schema

```powershell
# Check migration status
cd ..\..\..\src\GrcMvc
dotnet ef migrations list --context GrcAuthDbContext
```

Should show: `20260115064458_AddApplicationUserCustomColumns` (not "Pending")

---

## 📊 Monitoring Commands

### View Full Log
```powershell
Get-Content startup.log
```

### Search for Specific Text
```powershell
# Find migration messages
Get-Content startup.log | Select-String -Pattern "migration|Migration"

# Find errors
Get-Content startup-errors.log | Select-String -Pattern "error|Error|Exception"
```

### Check Application Status
```powershell
# Find process
Get-Process -Name "dotnet" | Where-Object { $_.Path -like "*GrcMvc*" }

# Check if listening on port
netstat -an | Select-String "5000"
```

### Stop Application
```powershell
# Find and stop the process
Get-Process -Name "dotnet" | Where-Object { $_.Path -like "*GrcMvc*" } | Stop-Process
```

---

## 🔧 Troubleshooting

### Application Won't Start

1. **Check Error Log:**
   ```powershell
   Get-Content startup-errors.log
   ```

2. **Verify Environment Variables:**
   ```powershell
   $env:JWT_SECRET
   $env:ConnectionStrings__GrcAuthDb
   $env:ASPNETCORE_ENVIRONMENT
   ```

3. **Check Database Connection:**
   - Verify database is running
   - Test connection string
   - Check database permissions

### Migration Not Applied

1. **Check Migration Status:**
   ```powershell
   cd ..\..\..\src\GrcMvc
   dotnet ef migrations list --context GrcAuthDbContext
   ```

2. **Apply Migration Manually (if needed):**
   ```powershell
   dotnet ef database update --context GrcAuthDbContext
   ```

### Can't See Migration Message

- Migration may have already been applied (check migration list)
- Application may still be initializing (wait a few more seconds)
- Check full log file for all messages

---

## ✅ Verification Checklist

- [ ] Application process is running
- [ ] Logs show "Auth database migrations applied" (or migration already applied)
- [ ] Application responds to HTTP requests
- [ ] Health check endpoint works
- [ ] User creation form works
- [ ] User editing form works
- [ ] All ApplicationUser properties save/load correctly

---

## 🎉 Production Application Status

**Application**: ✅ Started  
**Environment**: Production  
**Port**: 5000 (http://0.0.0.0:5000)  
**Logs**: `startup.log` and `startup-errors.log`  
**Status**: Ready for testing

---

**💡 Tip**: Keep the log monitoring running in one terminal while testing the application in another terminal or browser.

**Next**: Monitor the logs to confirm migrations are applied, then test the user forms!
