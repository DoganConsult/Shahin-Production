# Test Database Connection - Quick Guide

## 🧪 How to Test Database Connection

### Method 1: Using dotnet run (Recommended)

**Step 1: Set Connection String**
```powershell
# For local PostgreSQL
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"

# For Railway/Cloud (if you have DATABASE_URL)
# Railway automatically sets this, but you can test with:
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"
```

**Step 2: Run Test**
```powershell
cd src\GrcMvc
dotnet run -- TestDb
```

### Method 2: Check Current Configuration First

```powershell
# Check what connection strings are currently set
Write-Host "Checking environment variables..."
Write-Host "ConnectionStrings__DefaultConnection: $([Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection'))"
Write-Host "CONNECTION_STRING: $([Environment]::GetEnvironmentVariable('CONNECTION_STRING'))"
Write-Host "DATABASE_URL: $([Environment]::GetEnvironmentVariable('DATABASE_URL'))"
```

### Method 3: Test with PowerShell Script

```powershell
.\test-db-simple.ps1 -ConnectionString "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"
```

---

## 📊 Expected Test Output

### ✅ Success:
```
========================================
Database Connection Test
========================================

Testing DefaultConnection...
  Connection String: Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=***

Connecting... ✅ Connected

📊 Counting tables in database...

✅ Connection Test Successful!
   Database: GrcMvcDb
   User: postgres
   PostgreSQL: PostgreSQL 15.0 on x86_64-pc-linux-gnu...

📊 Database Tables:
   Total Tables: 45

   Table Names:
     - AspNetRoleClaims
     - AspNetRoles
     - ...
```

### ❌ Failure:
```
❌ Connection Failed!
   Error: Connection refused
   SQL State: 08001

Troubleshooting:
  1. Verify PostgreSQL server is running
  2. Check connection string format
  3. Verify database exists
  4. Check user credentials and permissions
  5. Verify firewall/network settings
```

---

## 🔍 What the Test Does

1. ✅ Reads connection string from:
   - Environment variable: `ConnectionStrings__DefaultConnection`
   - Environment variable: `CONNECTION_STRING`
   - Environment variable: `DATABASE_URL` (Railway format - auto-converted)
   - `appsettings.json`: `ConnectionStrings.DefaultConnection`

2. ✅ Validates connection string format

3. ✅ Connects to PostgreSQL database

4. ✅ Runs test query: `SELECT version(), current_database(), current_user`

5. ✅ Counts tables in database

6. ✅ Lists all table names

7. ✅ Logs results to: `c:\Shahin-ai\.cursor\debug.log`

---

## 🚀 Quick Test Commands

### Test with Local PostgreSQL:
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"
cd src\GrcMvc
dotnet run -- TestDb
```

### Test with Railway DB:
```powershell
# Railway automatically sets DATABASE_URL
# Just run:
cd src\GrcMvc
dotnet run -- TestDb
```

### Test Connection String Resolution (without connecting):
```powershell
cd src\GrcMvc
dotnet run
# Watch for [CONFIG] logs showing connection string resolution
```
