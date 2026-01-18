# 🚂 Test Railway Database Connection

## ✅ Railway DB Support Status

**Status:** ✅ **FULLY SUPPORTED**

The application automatically detects and converts Railway's `DATABASE_URL` format to PostgreSQL connection strings.

---

## 🧪 How to Test Railway Connection

### Method 1: Test with Railway DATABASE_URL (Automatic)

Railway automatically sets `DATABASE_URL` when you deploy. To test locally:

```powershell
# Set Railway DATABASE_URL format
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"

# Test the connection
cd Shahin-Jan-2026\src\GrcMvc
dotnet run -- TestDb
```

### Method 2: Test Connection String Conversion

```powershell
# Set Railway URL
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"

# Test format conversion
.\test-connection-now.ps1
```

### Method 3: Test Application Startup (Full Test)

```powershell
# Set Railway URL
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"

# Run application - it will auto-convert and connect
cd Shahin-Jan-2026\src\GrcMvc
dotnet run
```

Watch for these log messages:
```
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
[CONFIG] ✅ Using database connection from: Environment Variable
```

---

## 🔄 How Railway URL Conversion Works

**Railway Format:**
```
postgresql://username:password@host.railway.app:5432/database
```

**Auto-Converted To:**
```
Host=host.railway.app;Database=database;Username=username;Password=password;Port=5432
```

**Code Location:** `Shahin-Jan-2026/src/GrcMvc/Extensions/WebApplicationBuilderExtensions.cs` (lines 191-218)

---

## 📊 Expected Test Output

### ✅ Success:
```
========================================
Database Connection Test
========================================

Testing DefaultConnection...
  Connection String: Host=host.railway.app;Port=5432;Database=railway;Username=postgres;Password=***

Connecting... ✅ Connected

✅ Connection Test Successful!
   Database: railway
   User: postgres
   PostgreSQL: PostgreSQL 15.0...
```

### Configuration Logs:
```
[CONFIG] ========================================
[CONFIG] Resolving Connection Strings
[CONFIG] ========================================
[CONFIG] 🔍 Checking environment variables:
[CONFIG]   ❌ ConnectionStrings__DefaultConnection = (not set)
[CONFIG]   ❌ CONNECTION_STRING = (not set)
[CONFIG] 🔍 Resolving connection string: DefaultConnection
[CONFIG]   ❌ Not found: ConnectionStrings__DefaultConnection
[CONFIG]   ❌ Not found: CONNECTION_STRING
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
[CONFIG] ✅ Using database connection from: Environment Variable (Railway/Docker)
[CONFIG] 📊 Database: host.railway.app:5432 / postgres@railway
```

---

## 🎯 Railway Deployment Checklist

### ✅ Automatic (Railway Sets These):
- [x] `DATABASE_URL` environment variable
- [x] Connection string auto-conversion
- [x] Format validation

### ✅ What You Need to Do:
1. Deploy application to Railway
2. Add PostgreSQL service to Railway project
3. Link database to application service
4. Railway automatically sets `DATABASE_URL`
5. Application auto-converts and connects

**That's it!** No manual configuration needed. ✅

---

## 🔍 Verify Railway Connection

### Check if DATABASE_URL is Set:
```powershell
[Environment]::GetEnvironmentVariable('DATABASE_URL')
```

### Test Conversion Manually:
```powershell
$railwayUrl = "postgresql://postgres:password@host.railway.app:5432/railway"
$uri = [System.Uri]::new($railwayUrl)
$userInfo = $uri.UserInfo -split ':'
$decodedUser = [System.Uri]::UnescapeDataString($userInfo[0])
$decodedPass = [System.Uri]::UnescapeDataString($userInfo[1])
$dbName = $uri.LocalPath.TrimStart('/')
$connString = "Host=$($uri.Host);Database=$dbName;Username=$decodedUser;Password=$decodedPass;Port=$($uri.Port)"
Write-Host $connString
```

---

## 🚀 Quick Test Command

```powershell
# Set Railway URL (replace with your actual Railway URL)
$env:DATABASE_URL = "postgresql://postgres:YOUR_PASSWORD@YOUR_HOST.railway.app:5432/railway"

# Test
cd Shahin-Jan-2026\src\GrcMvc
dotnet run -- TestDb
```

---

**Status:** ✅ **RAILWAY DB READY TO TEST**

Just set `DATABASE_URL` and run the test! 🚂
