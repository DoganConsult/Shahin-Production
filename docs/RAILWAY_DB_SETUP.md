# Railway Database Configuration Guide

## 🚂 Railway DB Environment Variables Setup

This guide shows how to configure Railway PostgreSQL database connection strings that work seamlessly across all application points without any hardcoding.

---

## ✅ Zero Hardcoding Guarantee

**All connection strings are read from environment variables.** No hardcoded values anywhere in the codebase.

**Supported Sources (Priority Order):**
1. **ABP Settings** (Database - Encrypted) ← Admin UI managed
2. **Environment Variables** (Railway, Docker, System) ← Railway DB here
3. **Configuration Files** (appsettings.json) ← Development only

---

## 🔧 Railway Database Setup

### Step 1: Get Railway Database Connection String

From your Railway dashboard, copy the PostgreSQL connection string. It typically looks like:

```
postgresql://postgres:password@hostname.proxy.rlwy.net:port/railway
```

### Step 2: Convert to PostgreSQL Connection String Format

Convert Railway's connection string to PostgreSQL format:

```
Host=hostname.proxy.rlwy.net;Port=port;Database=railway;Username=postgres;Password=password;SSL Mode=Require;Trust Server Certificate=true
```

**Example Railway DB Connection String:**
```
Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require;Trust Server Certificate=true
```

### Step 3: Set Environment Variable

**Option A: Railway Platform (Recommended)**

In Railway dashboard, add environment variable:

```
ConnectionStrings__DefaultConnection=Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
```

**Option B: Local Development (.env file)**

Create or update `.env` file:

```bash
ConnectionStrings__DefaultConnection=Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
```

**Option C: System Environment Variable**

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
```

---

## 🔄 How It Works: Automatic Reflection to All Points

### Connection String Resolution Flow

```
┌─────────────────────────────────────────┐
│  Railway Environment Variable Set       │
│  ConnectionStrings__DefaultConnection   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  ResolveConnectionStrings()              │
│  (WebApplicationBuilderExtensions.cs)    │
│  • Reads from environment variable       │
│  • Sets in IConfiguration                │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  IConfiguration                         │
│  ConnectionStrings:DefaultConnection    │
└──────────────┬──────────────────────────┘
               │
               ├─────────────────┬─────────────────┬─────────────────┐
               ▼                 ▼                 ▼                 ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
    │ GrcDbContext │  │GrcAuthDbContext│ │ Hangfire     │  │ All Services │
    │ (Main DB)    │  │ (Auth DB)     │  │ (Jobs DB)    │  │ (via DI)     │
    └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
```

### All Points That Use Connection Strings

**1. Main Database Context (GrcDbContext)**
- **Location:** `GrcMvcAbpModule.cs` (ABP configuration)
- **Source:** `IConfiguration.GetConnectionString("DefaultConnection")`
- **Status:** ✅ Automatically uses Railway DB env var

**2. Auth Database Context (GrcAuthDbContext)**
- **Location:** `ServiceCollectionExtensions.cs` → `AddDatabaseContexts()`
- **Source:** `IConfiguration.GetConnectionString("GrcAuthDb")`
- **Status:** ✅ Automatically uses Railway DB env var (or derives from main)

**3. Hangfire Background Jobs**
- **Location:** Hangfire configuration
- **Source:** `IConfiguration.GetConnectionString("HangfireConnection")`
- **Status:** ✅ Automatically uses Railway DB env var

**4. Entity Framework Migrations**
- **Location:** `GrcDbContextFactory.cs`
- **Source:** `IConfiguration.GetConnectionString("DefaultConnection")`
- **Status:** ✅ Automatically uses Railway DB env var

**5. All Services via Dependency Injection**
- **Location:** Any service that injects `IConfiguration` or `DbContext`
- **Source:** `IConfiguration.GetConnectionString(...)`
- **Status:** ✅ Automatically uses Railway DB env var

**6. ABP Settings System**
- **Location:** `ConnectionStringSettingValueProvider.cs`
- **Source:** `IConfiguration.GetConnectionString(...)`
- **Status:** ✅ Automatically uses Railway DB env var

---

## 🎯 Railway-Specific Environment Variables

### Standard Format (Recommended)

```bash
ConnectionStrings__DefaultConnection=Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=password;SSL Mode=Require;Trust Server Certificate=true
```

### Alternative Formats (Also Supported)

```bash
# Alternative format 1
CONNECTION_STRING=Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=password;SSL Mode=Require;Trust Server Certificate=true

# Alternative format 2 (if using separate variables)
DB_HOST=caboose.proxy.rlwy.net
DB_PORT=11527
DB_NAME=railway
DB_USER=postgres
DB_PASSWORD=password
```

**Note:** The application automatically constructs connection string from individual `DB_*` variables if `ConnectionStrings__DefaultConnection` is not set.

---

## 🔐 Security: Using ABP Settings (Recommended for Production)

### Migrate Railway DB to ABP Settings

**Why:** Encrypted storage, UI management, zero-downtime updates

**Steps:**

1. **Set Railway DB in Environment Variable** (initial setup)
   ```bash
   ConnectionStrings__DefaultConnection=Host=caboose.proxy.rlwy.net;Port=11527;Database=railway;Username=postgres;Password=password;SSL Mode=Require;Trust Server Certificate=true
   ```

2. **Access Admin UI**
   - Navigate to: `/admin/environment-variables`
   - Login as: PlatformAdmin

3. **Migrate to ABP Settings**
   - Click "Migrate to ABP Settings"
   - Railway DB connection string is encrypted in database
   - Can now be updated via UI without restart

4. **Future Updates**
   - Update via Admin UI (encrypted, immediate effect)
   - Or update environment variable (requires restart)

---

## ✅ Verification

### Check Connection String is Loaded

**1. Check Application Logs**

On startup, you should see:
```
[CONFIG] Using database connection from: Environment Variable (Railway/Docker)
[CONFIG] Database: caboose.proxy.rlwy.net:11527 / postgres@railway
```

**2. Check via Admin UI**

Navigate to `/admin/environment-variables`:
- Should show Railway DB connection string
- Source indicator shows "Env" badge
- Can be migrated to ABP Settings (encrypted)

**3. Test Database Connection**

```bash
# Via application health check
curl http://localhost/health/db

# Should return: {"status":"Healthy","database":"Connected"}
```

---

## 🔄 Railway DB Update Process

### Scenario: Railway Database Changed

**Option 1: Update Environment Variable (Requires Restart)**

```bash
# Update in Railway dashboard or .env file
ConnectionStrings__DefaultConnection=Host=new-host.proxy.rlwy.net;Port=12345;Database=newdb;Username=postgres;Password=newpass;SSL Mode=Require;Trust Server Certificate=true

# Restart application
```

**Option 2: Update via ABP Settings (Zero Downtime)**

1. Navigate to `/admin/environment-variables`
2. Update `ConnectionStrings__DefaultConnection` value
3. Click "Save to ABP Settings"
4. **Changes take effect immediately** - no restart needed

---

## 📋 Railway DB Environment Variables Checklist

- [ ] Railway database created
- [ ] Connection string copied from Railway dashboard
- [ ] Converted to PostgreSQL format
- [ ] Set as `ConnectionStrings__DefaultConnection` environment variable
- [ ] Application starts successfully
- [ ] Database connection verified in logs
- [ ] (Optional) Migrated to ABP Settings for encrypted storage

---

## 🎯 Key Points

✅ **Zero Hardcoding:** All connection strings come from environment variables  
✅ **Railway Compatible:** Works with Railway PostgreSQL out of the box  
✅ **Automatic Reflection:** All application points use the same connection string  
✅ **ABP Integration:** Can be migrated to encrypted ABP Settings  
✅ **Multiple Formats:** Supports various environment variable formats  
✅ **Fallback Chain:** Environment variables → Configuration → Defaults  

---

## 🚀 Quick Start

**For Railway Deployment:**

1. Copy Railway PostgreSQL connection string
2. Set as environment variable: `ConnectionStrings__DefaultConnection`
3. Deploy application
4. Verify connection in logs
5. (Optional) Migrate to ABP Settings via Admin UI

**That's it!** Railway DB connection is automatically used everywhere in the application.

---

*No hardcoding. No manual configuration. Just set the environment variable and it works everywhere.*
