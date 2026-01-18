# How to Test Database Connection

## Quick Test Methods

### Method 1: Using dotnet run (Recommended)

**Step 1: Set Connection String**
```powershell
# For local PostgreSQL
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=grcmvc;Username=postgres;Password=yourpassword"

# For Railway/Cloud
$env:ConnectionStrings__DefaultConnection = "Host=hostname.railway.app;Port=5432;Database=railway;Username=postgres;Password=password"
```

**Step 2: Run Test**
```powershell
cd Shahin-Jan-2026\src\GrcMvc
dotnet run -- TestDb
```

**Expected Output:**
```
========================================
Database Connection Test
========================================

Testing DefaultConnection...
  Connection String: Host=localhost;Port=5432;Database=grcmvc;Username=postgres;Password=***

Connecting... ✅ Connected

✅ Connection Test Successful!
   Database: grcmvc
   User: postgres
   PostgreSQL: PostgreSQL 15.0 on x86_64-pc-linux-gnu...
```

---

### Method 2: Using PowerShell Script

```powershell
.\test-db-simple.ps1 -ConnectionString "Host=localhost;Port=5432;Database=grcmvc;Username=postgres;Password=yourpassword"
```

---

### Method 3: Check appsettings.json

If connection string is in `appsettings.json` or `appsettings.Development.json`:

```powershell
# Just run the test (no need to set environment variable)
dotnet run -- TestDb
```

---

## What the Test Does

1. ✅ Reads connection string from:
   - Environment variable: `ConnectionStrings__DefaultConnection`
   - Environment variable: `CONNECTION_STRING`
   - `appsettings.json`: `ConnectionStrings.DefaultConnection`
   - `appsettings.Development.json`: `ConnectionStrings.DefaultConnection`

2. ✅ Connects to PostgreSQL database

3. ✅ Runs test query: `SELECT version(), current_database(), current_user`

4. ✅ Logs results to: `c:\Shahin-ai\.cursor\debug.log`

5. ✅ Displays:
   - Connection status (✅ Success or ❌ Failed)
   - Database name
   - PostgreSQL version
   - Current user

---

## Troubleshooting

### ❌ Connection Failed

**Error: "Connection string not found"**
- Solution: Set environment variable or add to `appsettings.json`

**Error: "Connection refused" or "Host not found"**
- Solution: Verify PostgreSQL server is running
- Check host/port in connection string

**Error: "Database does not exist"**
- Solution: Create database first: `CREATE DATABASE grcmvc;`

**Error: "Password authentication failed"**
- Solution: Verify username and password

**Error: "Timeout"**
- Solution: Check firewall/network settings
- Verify PostgreSQL is accessible

---

## Example Connection Strings

### Local PostgreSQL
```
Host=localhost;Port=5432;Database=grcmvc;Username=postgres;Password=mypassword
```

### Railway
```
Host=containers-us-west-xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx
```

### Docker
```
Host=postgres;Port=5432;Database=grcmvc;Username=postgres;Password=mypassword
```

### AWS RDS
```
Host=mydb.xxxxx.us-east-1.rds.amazonaws.com;Port=5432;Database=grcmvc;Username=admin;Password=xxx
```

---

## Test Files Created

- ✅ `TestDbConnection.cs` - C# test implementation
- ✅ `test-db-simple.ps1` - PowerShell helper script
- ✅ Modified `Program.cs` - Added `TestDb` command support

---

## Next Steps

After successful connection test:
1. ✅ Database is accessible
2. ✅ Credentials are correct
3. ✅ Network/firewall allows connection
4. ✅ Ready to run migrations: `dotnet ef database update`
