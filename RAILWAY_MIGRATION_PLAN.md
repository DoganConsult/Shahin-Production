# 🚂 Railway Database Migration Plan

## 📋 Current Status

### ✅ What's Already Done
- Railway PostgreSQL database provisioned
- `DATABASE_URL` environment variable configured: `${{ Postgres.DATABASE_URL }}`
- Application configured to auto-detect and convert Railway connection string
- Auto-migration on startup enabled in `Program.cs`
- Auth migrations exist (3 migrations for `GrcAuthDbContext`)

### ❌ What's Missing
- **Initial migration for main GRC database (`GrcDbContext`)** - This is the critical missing piece!
- The main database has 200+ tables defined but NO migration files created yet

---

## 🎯 Migration Strategy

### Two Database Contexts

Your application uses **two separate database contexts**:

1. **GrcAuthDbContext** (Authentication/Identity)
   - Tables: AspNetUsers, AspNetRoles, PasswordHistory, RefreshTokens, etc.
   - Migrations: ✅ 3 migrations exist in `Migrations/Auth/`
   - Status: **Ready to migrate**

2. **GrcDbContext** (Main GRC Application)
   - Tables: 200+ tables (Tenants, Risks, Controls, Assessments, etc.)
   - Migrations: ❌ **NO migrations created yet!**
   - Status: **Needs initial migration**

---

## 📝 Step-by-Step Migration Plan

### Phase 1: Create Initial Migrations (Local)

#### Step 1.1: Create Initial Migration for GrcDbContext

```bash
cd Shahin-Jan-2026/src/GrcMvc

# Create initial migration for main GRC database
dotnet ef migrations add InitialCreate --context GrcDbContext --output-dir Migrations/Main
```

**Expected Output:**
- Creates `Migrations/Main/YYYYMMDDHHMMSS_InitialCreate.cs`
- Creates `Migrations/Main/YYYYMMDDHHMMSS_InitialCreate.Designer.cs`
- Creates `Migrations/Main/GrcDbContextModelSnapshot.cs`

#### Step 1.2: Verify Auth Migrations Exist

```bash
# List existing auth migrations
dotnet ef migrations list --context GrcAuthDbContext
```

**Expected Output:**
```
20260115064458_AddApplicationUserCustomColumns
20260115074215_CreateMissingIdentityTables
20260117191403_FixAuthCustomColumns
```

---

### Phase 2: Test Migrations Locally (Optional but Recommended)

#### Step 2.1: Test with Local PostgreSQL (if available)

```bash
# Set local connection string
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=grc_test;Username=postgres;Password=yourpassword"
$env:ConnectionStrings__GrcAuthDb = "Host=localhost;Database=grc_auth_test;Username=postgres;Password=yourpassword"

# Run application (migrations auto-apply on startup)
dotnet run
```

#### Step 2.2: Verify Migration SQL (without applying)

```bash
# Generate SQL script for GrcDbContext
dotnet ef migrations script --context GrcDbContext --output Migrations/Main/InitialCreate.sql

# Generate SQL script for GrcAuthDbContext
dotnet ef migrations script --context GrcAuthDbContext --output Migrations/Auth/AllAuthMigrations.sql
```

---

### Phase 3: Deploy to Railway

#### Step 3.1: Commit Migration Files

```bash
git add Shahin-Jan-2026/src/GrcMvc/Migrations/
git commit -m "Add initial database migrations for Railway deployment"
git push
```

#### Step 3.2: Deploy to Railway

**Option A: Automatic Deployment (Recommended)**
- Railway auto-deploys on git push
- Application starts and auto-applies migrations via `ApplyDatabaseMigrationsAsync()`

**Option B: Manual Deployment**
```bash
railway up --service 0cb7da15-a249-4cba-a197-677e800c306a
```

#### Step 3.3: Monitor Deployment Logs

```bash
railway logs --service 0cb7da15-a249-4cba-a197-677e800c306a --follow
```

**Look for these log messages:**

```
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
🔄 Applying database migrations...
📦 Found X pending migrations
✅ Main database migrations applied successfully
✅ Auth database migrations applied successfully
```

---

### Phase 4: Verify Migration Success

#### Step 4.1: Check via Railway SSH

```bash
# Connect to Railway service
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a

# Once connected, check database
psql $DATABASE_URL -c "\dt"
```

**Expected Output:** List of all tables including:
- AspNetUsers, AspNetRoles (from GrcAuthDbContext)
- Tenants, Risks, Controls, Assessments, etc. (from GrcDbContext)
- __EFMigrationsHistory (migration tracking table)

#### Step 4.2: Verify Migration History

```bash
psql $DATABASE_URL -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
```

**Expected Output:**
```
MigrationId                              | ProductVersion
-----------------------------------------|---------------
20260115064458_AddApplicationUserCustomColumns | 8.0.x
20260115074215_CreateMissingIdentityTables     | 8.0.x
20260117191403_FixAuthCustomColumns            | 8.0.x
YYYYMMDDHHMMSS_InitialCreate                   | 8.0.x
```

#### Step 4.3: Count Tables

```bash
psql $DATABASE_URL -c "SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema = 'public';"
```

**Expected:** ~200+ tables

---

## 🔧 Troubleshooting

### Issue 1: Build Errors When Creating Migration

**Error:**
```
Build failed. Use dotnet build to see the errors.
```

**Solution:**
```bash
# Build first to identify errors
dotnet build

# Fix any compilation errors
# Then retry migration creation
dotnet ef migrations add InitialCreate --context GrcDbContext --output-dir Migrations/Main
```

### Issue 2: Migration Already Applied

**Error:**
```
The migration 'XXXXX' has already been applied to the database.
```

**Solution:**
This is normal if migrations auto-applied. Verify with:
```bash
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a
psql $DATABASE_URL -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

### Issue 3: Connection String Not Found

**Error:**
```
No connection string named 'DefaultConnection' was found
```

**Solution:**
Verify `DATABASE_URL` is set in Railway:
```bash
railway variable list --service 0cb7da15-a249-4cba-a197-677e800c306a
```

Should show:
```
DATABASE_URL = ${{ Postgres.DATABASE_URL }}
```

### Issue 4: Tables Already Exist

**Error:**
```
relation "Tenants" already exists
```

**Solution:**
If tables exist but migration history doesn't, you need to baseline:

```bash
# Connect to Railway database
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a

# Mark migration as applied without running it
psql $DATABASE_URL -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('YYYYMMDDHHMMSS_InitialCreate', '8.0.0');"
```

---

## 📊 Migration File Structure

After completing Phase 1, your structure should be:

```
Shahin-Jan-2026/src/GrcMvc/Migrations/
├── Auth/
│   ├── 20260115064458_AddApplicationUserCustomColumns.cs
│   ├── 20260115064458_AddApplicationUserCustomColumns.Designer.cs
│   ├── 20260115074215_CreateMissingIdentityTables.cs
│   ├── 20260115074215_CreateMissingIdentityTables.Designer.cs
│   ├── 20260117191403_FixAuthCustomColumns.cs
│   ├── 20260117191403_FixAuthCustomColumns.Designer.cs
│   ├── GrcAuthDbContextModelSnapshot.cs
│   └── AllAuthMigrations.sql (optional)
└── Main/
    ├── YYYYMMDDHHMMSS_InitialCreate.cs
    ├── YYYYMMDDHHMMSS_InitialCreate.Designer.cs
    ├── GrcDbContextModelSnapshot.cs
    └── InitialCreate.sql (optional)
```

---

## ✅ Success Criteria

Migration is successful when:

1. ✅ Initial migration created for `GrcDbContext`
2. ✅ Application builds without errors
3. ✅ Application deploys to Railway successfully
4. ✅ Logs show: "✅ Main database migrations applied successfully"
5. ✅ Logs show: "✅ Auth database migrations applied successfully"
6. ✅ Database contains 200+ tables
7. ✅ `__EFMigrationsHistory` table shows all migrations applied
8. ✅ Application starts and runs without database errors

---

## 🚀 Quick Start Commands

### Create Migrations (Run First!)

```bash
cd Shahin-Jan-2026/src/GrcMvc

# Create main database migration
dotnet ef migrations add InitialCreate --context GrcDbContext --output-dir Migrations/Main

# Verify auth migrations exist
dotnet ef migrations list --context GrcAuthDbContext
```

### Deploy to Railway

```bash
# Commit and push
git add .
git commit -m "Add initial database migrations"
git push

# Monitor deployment
railway logs --service 0cb7da15-a249-4cba-a197-677e800c306a --follow
```

### Verify Success

```bash
# SSH into Railway
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a

# Check tables
psql $DATABASE_URL -c "\dt"

# Check migration history
psql $DATABASE_URL -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

---

## 📝 Important Notes

### Auto-Migration on Startup

Your application is configured to **automatically apply migrations** on startup:

**File:** `Shahin-Jan-2026/src/GrcMvc/Program.cs`
```csharp
await app.ApplyDatabaseMigrationsAsync();
```

**File:** `Shahin-Jan-2026/src/GrcMvc/Extensions/WebApplicationExtensions.cs`
```csharp
public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
{
    // Applies migrations for both GrcDbContext and GrcAuthDbContext
    await dbContext.Database.MigrateAsync();
    await authContext.Database.MigrateAsync();
}
```

This means:
- ✅ No manual migration commands needed on Railway
- ✅ Migrations apply automatically on each deployment
- ✅ Safe to redeploy - EF Core tracks applied migrations
- ⚠️ Ensure migrations are tested before deploying to production

### Two Separate Databases (Optional)

Currently configured for **single database** (both contexts use same connection).

To use **separate databases** (recommended for production):

1. Add second database in Railway
2. Set environment variables:
   ```
   DATABASE_URL = ${{ Postgres.DATABASE_URL }}           # Main GRC database
   AUTH_DATABASE_URL = ${{ PostgresAuth.DATABASE_URL }}  # Auth database
   ```
3. Update connection string resolution in `WebApplicationBuilderExtensions.cs`

---

## 🎯 Next Steps

1. **Create initial migration** for GrcDbContext (Phase 1)
2. **Test locally** if possible (Phase 2 - optional)
3. **Deploy to Railway** (Phase 3)
4. **Verify success** (Phase 4)
5. **Monitor application** logs for any issues

---

**Status:** 🟡 **READY TO CREATE MIGRATIONS**

Run the commands in Phase 1 to create the initial migration! 🚀
