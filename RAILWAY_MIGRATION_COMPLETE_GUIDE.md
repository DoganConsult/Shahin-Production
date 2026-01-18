# 🚂 Complete Railway Database Migration Guide

## 📊 Executive Summary

**Task:** Migrate all database tables to the new Railway PostgreSQL database

**Current Status:**
- ✅ Railway PostgreSQL database provisioned and configured
- ✅ Connection string configured via `DATABASE_URL` template variable
- ✅ Application auto-migration enabled on startup
- ✅ Auth database migrations exist (3 migrations)
- ❌ **Main GRC database migrations NOT created yet** ← **This is what we need to do!**

**Solution:** Create initial migration for main database, commit, and deploy to Railway

---

## 🎯 What You Need to Do

### Quick Start (3 Simple Steps)

```powershell
# Step 1: Create migrations
.\create-railway-migrations.ps1

# Step 2: Commit and push
git add .
git commit -m "Add initial database migrations for Railway"
git push

# Step 3: Monitor deployment
railway logs --service 0cb7da15-a249-4cba-a197-677e800c306a --follow
```

That's it! The application will automatically apply migrations when it starts on Railway.

---

## 📋 Detailed Explanation

### Understanding Your Database Setup

Your application uses **TWO separate database contexts**:

#### 1. GrcAuthDbContext (Authentication Database)
- **Purpose:** User authentication, identity, security
- **Tables:** AspNetUsers, AspNetRoles, PasswordHistory, RefreshTokens, LoginAttempts, etc.
- **Migrations:** ✅ **Already exist** (3 migrations in `Migrations/Auth/`)
- **Status:** Ready to deploy

#### 2. GrcDbContext (Main GRC Application Database)
- **Purpose:** All GRC business data
- **Tables:** 200+ tables including:
  - Tenants, TenantUsers, OrganizationProfiles
  - Risks, Controls, Assessments, Audits
  - Policies, Workflows, Evidence
  - Teams, Workspaces, Assets
  - And 190+ more tables...
- **Migrations:** ❌ **NOT created yet** ← **This is the problem!**
- **Status:** Needs initial migration

### Why Migrations Are Needed

Entity Framework Core uses migrations to:
1. **Track schema changes** over time
2. **Version control** your database schema
3. **Apply changes safely** without data loss
4. **Enable rollbacks** if needed
5. **Ensure consistency** across environments

Without migrations, EF Core doesn't know how to create the database schema.

---

## 🔧 The Migration Process

### What the Script Does

The `create-railway-migrations.ps1` script will:

1. ✅ Build your project to ensure no compilation errors
2. ✅ Check existing migrations (shows auth migrations exist)
3. ✅ Create initial migration for GrcDbContext (200+ tables)
4. ✅ Generate SQL scripts for review (optional)
5. ✅ Verify all migration files are created

### What Gets Created

After running the script, you'll have:

```
Shahin-Jan-2026/src/GrcMvc/Migrations/
├── Auth/                                    (Already exists)
│   ├── 20260115064458_AddApplicationUserCustomColumns.cs
│   ├── 20260115074215_CreateMissingIdentityTables.cs
│   ├── 20260117191403_FixAuthCustomColumns.cs
│   ├── GrcAuthDbContextModelSnapshot.cs
│   └── AllAuthMigrations.sql               (New - SQL script)
│
└── Main/                                    (NEW - Created by script)
    ├── YYYYMMDDHHMMSS_InitialCreate.cs     (Migration code)
    ├── YYYYMMDDHHMMSS_InitialCreate.Designer.cs
    ├── GrcDbContextModelSnapshot.cs        (Current schema snapshot)
    └── InitialCreate.sql                   (SQL script for review)
```

### What Happens on Railway Deployment

When you push to Railway:

1. **Railway detects changes** and triggers deployment
2. **Application starts** on Railway
3. **Auto-migration runs** (configured in `Program.cs`):
   ```csharp
   await app.ApplyDatabaseMigrationsAsync();
   ```
4. **EF Core checks** `__EFMigrationsHistory` table
5. **Applies pending migrations**:
   - Auth migrations (if not already applied)
   - Main database migration (InitialCreate)
6. **Creates all 200+ tables** in Railway PostgreSQL
7. **Application starts successfully**

---

## 📝 Step-by-Step Instructions

### Step 1: Create Migrations (Local)

Run the PowerShell script:

```powershell
.\create-railway-migrations.ps1
```

**Expected Output:**
```
========================================
Railway Database Migration Setup
========================================

📂 Navigating to project: Shahin-Jan-2026/src/GrcMvc

========================================
Step 1: Build Project
========================================
🔨 Building project...
✅ Build successful!

========================================
Step 2: Check Existing Migrations
========================================
🔍 Checking GrcAuthDbContext migrations...
20260115064458_AddApplicationUserCustomColumns
20260115074215_CreateMissingIdentityTables
20260117191403_FixAuthCustomColumns

🔍 Checking GrcDbContext migrations...
ℹ️  No migrations found for GrcDbContext (expected)

========================================
Step 3: Create Initial Migration for GrcDbContext
========================================
📝 Creating initial migration for main GRC database...
   This will create migrations for 200+ tables...

✅ Created directory: Migrations/Main
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
✅ Initial migration created successfully!

========================================
Step 4: Generate SQL Scripts (Optional)
========================================
📄 Generating SQL script for review...
✅ SQL script generated: Migrations/Main/InitialCreate.sql
✅ SQL script generated: Migrations/Auth/AllAuthMigrations.sql

========================================
Step 5: Verify Migration Files
========================================
📋 Migration files created:

Main Database (GrcDbContext):
  ✅ 20260118123456_InitialCreate.cs
  ✅ 20260118123456_InitialCreate.Designer.cs
  ✅ GrcDbContextModelSnapshot.cs
  ✅ InitialCreate.sql

Auth Database (GrcAuthDbContext):
  ✅ 20260115064458_AddApplicationUserCustomColumns.cs
  ✅ 20260115074215_CreateMissingIdentityTables.cs
  ✅ 20260117191403_FixAuthCustomColumns.cs
  ✅ GrcAuthDbContextModelSnapshot.cs
  ✅ AllAuthMigrations.sql

========================================
✅ Migration Setup Complete!
========================================
```

### Step 2: Review Migration Files (Optional)

You can review the generated SQL to see what will be created:

```powershell
# View the SQL script
code Shahin-Jan-2026/src/GrcMvc/Migrations/Main/InitialCreate.sql
```

This shows all CREATE TABLE statements for your 200+ tables.

### Step 3: Commit and Push

```powershell
# Add migration files
git add Shahin-Jan-2026/src/GrcMvc/Migrations/

# Commit
git commit -m "Add initial database migrations for Railway deployment

- Created initial migration for GrcDbContext (200+ tables)
- Includes all tenant, GRC, workflow, and integration tables
- Auth migrations already exist and ready to deploy
- Migrations will auto-apply on Railway startup"

# Push to trigger Railway deployment
git push
```

### Step 4: Monitor Railway Deployment

```powershell
# Watch deployment logs in real-time
railway logs --service 0cb7da15-a249-4cba-a197-677e800c306a --follow
```

**Look for these success messages:**

```
[CONFIG] ========================================
[CONFIG] Resolving Connection Strings
[CONFIG] ========================================
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
[CONFIG] ✅ Using database connection from: Environment Variable (Railway/Docker)
[CONFIG] 📊 Database: monorail.proxy.rlwy.net:5432 / postgres@railway

🔄 Applying database migrations...
📦 Found 1 pending migrations for main database
📦 Found 3 pending migrations for auth database
✅ Main database migrations applied successfully
✅ Auth database migrations applied successfully

Application started. Press Ctrl+C to shut down.
```

### Step 5: Verify Database Tables

```powershell
# SSH into Railway service
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a

# Once connected, check tables
psql $DATABASE_URL -c "\dt"
```

**Expected Output:** List of 200+ tables including:
- Tenants
- TenantUsers
- OrganizationProfiles
- Risks
- Controls
- Assessments
- Audits
- Policies
- Workflows
- AspNetUsers
- AspNetRoles
- And 190+ more...

### Step 6: Verify Migration History

```powershell
# Check which migrations were applied
psql $DATABASE_URL -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
```

**Expected Output:**
```
                MigrationId                 | ProductVersion
--------------------------------------------+---------------
20260115064458_AddApplicationUserCustomColumns | 8.0.x
20260115074215_CreateMissingIdentityTables     | 8.0.x
20260117191403_FixAuthCustomColumns            | 8.0.x
20260118123456_InitialCreate                   | 8.0.x
```

---

## 🔍 Troubleshooting

### Issue 1: Build Errors

**Error:**
```
Build failed. Use dotnet build to see the errors.
```

**Solution:**
```powershell
cd Shahin-Jan-2026/src/GrcMvc
dotnet build
```

Fix any compilation errors, then re-run the migration script.

### Issue 2: Migration Already Exists

**Error:**
```
The migration 'InitialCreate' already exists.
```

**Solution:**
Migration was already created. You can:
- Skip to Step 3 (commit and push)
- Or remove and recreate:
  ```powershell
  dotnet ef migrations remove --context GrcDbContext
  dotnet ef migrations add InitialCreate --context GrcDbContext --output-dir Migrations/Main
  ```

### Issue 3: Connection String Not Found on Railway

**Error in Railway logs:**
```
No connection string named 'DefaultConnection' was found
```

**Solution:**
Verify `DATABASE_URL` is set in Railway:

```powershell
railway variable list --service 0cb7da15-a249-4cba-a197-677e800c306a
```

Should show:
```
DATABASE_URL = ${{ Postgres.DATABASE_URL }}
```

If missing, add it in Railway Dashboard:
1. Go to your service → Variables
2. Add: `DATABASE_URL = ${{ Postgres.DATABASE_URL }}`
3. Redeploy

### Issue 4: Tables Already Exist

**Error:**
```
relation "Tenants" already exists
```

**Cause:** Tables were created manually or by previous deployment

**Solution:** Mark migration as applied without running it:

```powershell
railway ssh --service 0cb7da15-a249-4cba-a197-677e800c306a

# Get the migration ID from your migration file name
# Example: 20260118123456_InitialCreate.cs → 20260118123456_InitialCreate

psql $DATABASE_URL -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260118123456_InitialCreate', '8.0.0');"
```

---

## ✅ Success Checklist

After completing all steps, verify:

- [ ] Migration script ran successfully
- [ ] Migration files created in `Migrations/Main/`
- [ ] Changes committed and pushed to git
- [ ] Railway deployment triggered
- [ ] Deployment logs show: "✅ Main database migrations applied successfully"
- [ ] Deployment logs show: "✅ Auth database migrations applied successfully"
- [ ] Database contains 200+ tables (verified via SSH)
- [ ] `__EFMigrationsHistory` shows all 4 migrations applied
- [ ] Application starts without errors
- [ ] Can access application at Railway URL

---

## 📚 Additional Resources

### Files Created by This Process

1. **RAILWAY_MIGRATION_PLAN.md** - Detailed technical migration plan
2. **create-railway-migrations.ps1** - Automated migration creation script
3. **RAILWAY_MIGRATION_COMPLETE_GUIDE.md** - This comprehensive guide

### Existing Documentation

- **RAILWAY_DATABASE_SETUP.md** - Railway database configuration
- **RAILWAY_VARIABLES_ADDED.md** - Environment variables setup
- **DATABASE_CONFIGURATION_STATUS_REPORT.md** - Database config status

### Key Code Files

- **Program.cs** - Application startup, calls `ApplyDatabaseMigrationsAsync()`
- **WebApplicationExtensions.cs** - Migration application logic
- **GrcDbContext.cs** - Main database context (200+ tables)
- **GrcAuthDbContext.cs** - Auth database context
- **WebApplicationBuilderExtensions.cs** - Connection string resolution

---

## 🎯 Summary

### What We're Doing

Creating the initial database migration for your main GRC application database so that all 200+ tables can be created in Railway PostgreSQL.

### Why It's Needed

Without migrations, Entity Framework Core doesn't know how to create your database schema. The migration files tell EF Core exactly what tables, columns, indexes, and relationships to create.

### How It Works

1. **Locally:** Create migration files that describe your database schema
2. **Git:** Commit and push migration files to repository
3. **Railway:** Auto-deploys on push, application starts
4. **Startup:** Application automatically applies migrations to Railway database
5. **Result:** All 200+ tables created and ready to use

### Time Required

- **Step 1 (Create migrations):** 2-5 minutes
- **Step 2 (Review - optional):** 5 minutes
- **Step 3 (Commit/push):** 1 minute
- **Step 4 (Deploy/monitor):** 5-10 minutes
- **Step 5 (Verify):** 2 minutes

**Total:** ~15-25 minutes

---

## 🚀 Ready to Start?

Run this command to begin:

```powershell
.\create-railway-migrations.ps1
```

Then follow the on-screen instructions!

---

**Questions?** Check the troubleshooting section or review RAILWAY_MIGRATION_PLAN.md for more details.

**Status:** 🟢 **READY TO EXECUTE**

Good luck! 🚂
