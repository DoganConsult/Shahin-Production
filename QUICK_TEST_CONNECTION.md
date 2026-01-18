# 🧪 Quick Test Database Connection

## ✅ Current Status

**Connection String:** ❌ **NOT CONFIGURED**

You need to set a connection string before testing.

---

## 🚀 Quick Test (3 Steps)

### Step 1: Set Connection String

**For Local PostgreSQL:**
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"
```

**For Railway DB:**
```powershell
# Railway automatically sets DATABASE_URL, but you can test with:
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"
```

### Step 2: Test Connection String Format

```powershell
.\test-connection-now.ps1
```

This will:
- ✅ Check if connection string is set
- ✅ Validate format
- ✅ Test actual connection (if Npgsql.dll is available)

### Step 3: Test Full Connection (Recommended)

```powershell
cd Shahin-Jan-2026\src\GrcMvc
dotnet run -- TestDb
```

This will:
- ✅ Read connection string from all sources
- ✅ Connect to database
- ✅ Run test queries
- ✅ Show database info and table count

---

## 📊 Expected Results

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
   PostgreSQL: PostgreSQL 15.0...

📊 Database Tables:
   Total Tables: 45
```

### ❌ If Connection Fails:
```
❌ Connection Failed!
   Error: Connection refused
   
Troubleshooting:
  1. Verify PostgreSQL server is running
  2. Check connection string format
  3. Verify database exists
  4. Check user credentials
```

---

## 🔧 Common Connection Strings

### Local Development (Docker):
```
Host=localhost;Port=5433;Database=GrcMvcDb;Username=postgres;Password=postgres
```

### Local Development (Direct):
```
Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres
```

### Docker Container (from inside container):
```
Host=db;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres
```

### Railway (Auto-converted from DATABASE_URL):
```
postgresql://postgres:password@host.railway.app:5432/railway
```

---

## 🎯 What Gets Tested

1. ✅ **Connection String Resolution**
   - Environment variables
   - appsettings.json
   - Railway DATABASE_URL (auto-converted)

2. ✅ **Connection String Format**
   - Required fields (Host, Database)
   - Valid PostgreSQL format

3. ✅ **Actual Database Connection**
   - Network connectivity
   - Authentication
   - Database access

4. ✅ **Database Information**
   - PostgreSQL version
   - Database name
   - Current user
   - Table count

---

## 📝 Quick Commands Reference

```powershell
# Set connection string
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"

# Test format only
.\test-connection-now.ps1

# Test full connection
cd Shahin-Jan-2026\src\GrcMvc
dotnet run -- TestDb

# Check current config
.\test-connection-now.ps1
```

---

**Ready to test?** Set your connection string and run the test! 🚀
