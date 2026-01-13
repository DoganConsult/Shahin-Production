# 🗄️ Database Separation Guide

**Version:** 1.0
**Date:** 2026-01-13
**Status:** ✅ **FIXED - Database Duplication Issue Resolved**

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Issue Fixed](#issue-fixed)
4. [Setup Instructions](#setup-instructions)
5. [Migration Guide](#migration-guide)
6. [Verification](#verification)
7. [Troubleshooting](#troubleshooting)
8. [Security Benefits](#security-benefits)

---

## 🎯 Overview

The Shahin AI GRC System uses **two separate PostgreSQL databases** for security isolation:

| Database | Purpose | DbContext |
|----------|---------|-----------|
| **GrcMvcDb** | Main application data (entities, workflows, GRC data) | `GrcDbContext` |
| **GrcAuthDb** | Authentication data (ASP.NET Identity tables) | `GrcAuthDbContext` |

**Why Separate Databases?**
- **Security Isolation**: Authentication data is separated from application data
- **Compliance**: Required for some regulatory frameworks (SOC 2, ISO 27001)
- **Disaster Recovery**: Independent backup/restore strategies
- **Performance**: Isolated query performance and indexing strategies
- **Access Control**: Different permission models for auth vs. application data

---

## 🏗️ Architecture

### Database Structure

```
PostgreSQL Server (postgres:15-alpine)
├── GrcMvcDb                    # Main Application Database
│   ├── public schema
│   │   ├── Risks
│   │   ├── Controls
│   │   ├── Assessments
│   │   ├── Tenants
│   │   ├── Workspaces
│   │   └── ... (200+ entities)
│   ├── audit schema
│   │   └── Audit logs
│   └── analytics schema
│       └── Analytics tables
│
└── GrcAuthDb                   # Authentication Database
    └── public schema
        ├── AspNetUsers
        ├── AspNetRoles
        ├── AspNetUserRoles
        ├── AspNetUserClaims
        ├── AspNetUserLogins
        ├── AspNetUserTokens
        └── AspNetRoleClaims
```

### DbContext Mapping

**GrcDbContext.cs** → `GrcMvcDb`
- Connection String: `ConnectionStrings:DefaultConnection`
- Purpose: All application entities
- Migrations: `src/GrcMvc/Migrations/`

**GrcAuthDbContext.cs** → `GrcAuthDb`
- Connection String: `ConnectionStrings:GrcAuthDb`
- Purpose: ASP.NET Identity tables only
- Migrations: `src/GrcMvc/Data/Migrations/Auth/` (if exists)

---

## ⚠️ Issue Fixed

### Previous Configuration (WRONG)

**appsettings.json** (Before Fix):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;...",
    "GrcAuthDb": "Host=localhost;Database=GrcMvcDb;..."  // ❌ WRONG!
  }
}
```

**Problem:**
- Both `GrcDbContext` and `GrcAuthDbContext` pointed to **same database** (GrcMvcDb)
- All tables (application + auth) were in one database
- No security isolation
- Defeated the purpose of having two DbContexts

### Current Configuration (FIXED)

**appsettings.json** (After Fix):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;...",
    "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;..."  // ✅ CORRECT!
  }
}
```

**What Changed:**
- `GrcAuthDb` now points to a separate database: `Database=GrcAuthDb`
- Proper security isolation implemented
- Each DbContext has its own database

---

## 🚀 Setup Instructions

### Option 1: Automated Setup (Recommended)

#### Step 1: Run Database Creation Script

```bash
# From project root
./scripts/create-databases.sh
```

This script will:
1. Check if PostgreSQL container is running
2. Create `GrcMvcDb` database (if not exists)
3. Create `GrcAuthDb` database (if not exists)
4. Install required PostgreSQL extensions
5. Create schemas and grant permissions

#### Step 2: Run Migrations

```bash
cd src/GrcMvc

# Apply migrations to GrcMvcDb
dotnet ef database update --context GrcDbContext

# Apply migrations to GrcAuthDb
dotnet ef database update --context GrcAuthDbContext
```

#### Step 3: Start Application

```bash
# From src/GrcMvc
dotnet run
```

---

### Option 2: Manual Setup

#### Step 1: Start PostgreSQL

```bash
# Start PostgreSQL container
docker-compose up -d db

# Wait for PostgreSQL to be ready
docker-compose ps db
```

#### Step 2: Create Databases Manually

```bash
# Connect to PostgreSQL
docker exec -it grc-db psql -U postgres

# Create GrcMvcDb
CREATE DATABASE "GrcMvcDb"
    WITH OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TEMPLATE = template0;

# Create GrcAuthDb
CREATE DATABASE "GrcAuthDb"
    WITH OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TEMPLATE = template0;

# Exit psql
\q
```

#### Step 3: Run Initialization Script

```bash
docker exec -i grc-db psql -U postgres < scripts/init-db.sql
```

#### Step 4: Run Migrations

```bash
cd src/GrcMvc

# Apply migrations to GrcMvcDb
dotnet ef database update --context GrcDbContext

# Apply migrations to GrcAuthDb
dotnet ef database update --context GrcAuthDbContext
```

---

## 🔄 Migration Guide (Existing Installations)

If you already have an existing installation with both databases pointing to `GrcMvcDb`, follow these steps:

### Step 1: Backup Existing Data

```bash
# Backup GrcMvcDb
docker exec grc-db pg_dump -U postgres GrcMvcDb > backup_grcmvc_$(date +%Y%m%d_%H%M%S).sql
```

### Step 2: Create GrcAuthDb Database

```bash
./scripts/create-databases.sh
```

### Step 3: Migrate Auth Tables to GrcAuthDb

```bash
# Connect to PostgreSQL
docker exec -it grc-db psql -U postgres

# Switch to GrcMvcDb
\c GrcMvcDb

# Export ASP.NET Identity tables
\copy "AspNetUsers" TO '/tmp/aspnetusers.csv' WITH CSV HEADER;
\copy "AspNetRoles" TO '/tmp/aspnetroles.csv' WITH CSV HEADER;
\copy "AspNetUserRoles" TO '/tmp/aspnetuserroles.csv' WITH CSV HEADER;
\copy "AspNetUserClaims" TO '/tmp/aspnetuserclaims.csv' WITH CSV HEADER;
\copy "AspNetUserLogins" TO '/tmp/aspnetuserlogins.csv' WITH CSV HEADER;
\copy "AspNetUserTokens" TO '/tmp/aspnetusertokens.csv' WITH CSV HEADER;
\copy "AspNetRoleClaims" TO '/tmp/aspnetroleclaims.csv' WITH CSV HEADER;

# Switch to GrcAuthDb
\c GrcAuthDb

# Run auth migrations first
\q
```

```bash
# Apply auth migrations
cd src/GrcMvc
dotnet ef database update --context GrcAuthDbContext
```

```bash
# Import data into GrcAuthDb
docker exec -it grc-db psql -U postgres GrcAuthDb

\copy "AspNetUsers" FROM '/tmp/aspnetusers.csv' WITH CSV HEADER;
\copy "AspNetRoles" FROM '/tmp/aspnetroles.csv' WITH CSV HEADER;
\copy "AspNetUserRoles" FROM '/tmp/aspnetuserroles.csv' WITH CSV HEADER;
\copy "AspNetUserClaims" FROM '/tmp/aspnetuserclaims.csv' WITH CSV HEADER;
\copy "AspNetUserLogins" FROM '/tmp/aspnetuserlogins.csv' WITH CSV HEADER;
\copy "AspNetUserTokens" FROM '/tmp/aspnetusertokens.csv' WITH CSV HEADER;
\copy "AspNetRoleClaims" FROM '/tmp/aspnetroleclaims.csv' WITH CSV HEADER;

\q
```

### Step 4: Drop Auth Tables from GrcMvcDb (Optional)

**⚠️ CAUTION: Only do this AFTER verifying GrcAuthDb is working!**

```bash
docker exec -it grc-db psql -U postgres GrcMvcDb

-- Drop ASP.NET Identity tables from GrcMvcDb
DROP TABLE IF EXISTS "AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;

\q
```

### Step 5: Restart Application

```bash
docker-compose restart grcmvc
# or
cd src/GrcMvc && dotnet run
```

---

## ✅ Verification

### 1. Check Databases Exist

```bash
docker exec -it grc-db psql -U postgres -c "\l" | grep Grc
```

Expected output:
```
 GrcAuthDb | postgres | UTF8     | en_US.utf8 | en_US.utf8 |
 GrcMvcDb  | postgres | UTF8     | en_US.utf8 | en_US.utf8 |
```

### 2. Check Table Location

**Verify GrcMvcDb has application tables:**
```bash
docker exec -it grc-db psql -U postgres GrcMvcDb -c "\dt" | grep Risks
```

Expected: Should show `Risks`, `Controls`, `Assessments`, etc.

**Verify GrcMvcDb does NOT have AspNet tables:**
```bash
docker exec -it grc-db psql -U postgres GrcMvcDb -c "\dt" | grep AspNet
```

Expected: Should return nothing (or empty)

**Verify GrcAuthDb has AspNet tables:**
```bash
docker exec -it grc-db psql -U postgres GrcAuthDb -c "\dt" | grep AspNet
```

Expected output:
```
 AspNetRoles
 AspNetRoleClaims
 AspNetUsers
 AspNetUserClaims
 AspNetUserLogins
 AspNetUserRoles
 AspNetUserTokens
```

### 3. Test Application Login

```bash
# Start application
cd src/GrcMvc && dotnet run

# Open browser: http://localhost:5010
# Try to login with admin credentials
```

If login works, the database separation is successful!

---

## 🔧 Troubleshooting

### Issue: "Cannot connect to GrcAuthDb"

**Solution:**
```bash
# Check if GrcAuthDb exists
docker exec -it grc-db psql -U postgres -c "\l" | grep GrcAuthDb

# If not, create it
./scripts/create-databases.sh
```

### Issue: "Table 'AspNetUsers' does not exist in GrcAuthDb"

**Solution:**
```bash
# Run auth migrations
cd src/GrcMvc
dotnet ef database update --context GrcAuthDbContext
```

### Issue: "Login fails after migration"

**Solution:**
```bash
# Check if users were migrated
docker exec -it grc-db psql -U postgres GrcAuthDb -c "SELECT COUNT(*) FROM \"AspNetUsers\";"

# If 0, re-import the auth data (see Migration Guide Step 3)
```

### Issue: "Connection string not found"

**Solution:**
```bash
# Check appsettings.json
grep -A2 "ConnectionStrings" src/GrcMvc/appsettings.json

# Should show both DefaultConnection and GrcAuthDb
```

---

## 🔐 Security Benefits

### 1. **Data Isolation**
- Authentication data physically separated from application data
- Reduces attack surface for credential theft
- Limits blast radius if one database is compromised

### 2. **Access Control**
- Different database users/roles can be assigned
- Application service account doesn't need access to auth DB for queries
- Auth DB can have stricter permissions

### 3. **Audit & Compliance**
- Separate audit trails for auth vs. application operations
- Easier to prove compliance with regulations (SOC 2, ISO 27001)
- Simplified forensics in case of security incident

### 4. **Backup & Recovery**
- Independent backup schedules
- Auth DB can have more frequent backups (critical data)
- Faster recovery of just auth data if needed

### 5. **Performance**
- Isolated query performance
- Auth DB can be optimized for read-heavy workload
- Application DB can be optimized for write-heavy workload
- Independent indexing strategies

---

## 📊 Database Sizes (Typical)

| Database | Typical Size | Growth Rate |
|----------|--------------|-------------|
| **GrcMvcDb** | 100 MB - 10 GB | High (depends on tenants/data) |
| **GrcAuthDb** | 10 MB - 100 MB | Low (grows with users only) |

---

## 🔄 Connection String Reference

### Development (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432",
    "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432"
  }
}
```

### Production (.env)

```bash
# Main Application Database
GRCMVC_DB_CONNECTION=Host=grc-db;Database=GrcMvcDb;Username=postgres;Password=YOUR_SECURE_PASSWORD;Port=5432

# Authentication Database
GRCAUTH_DB_CONNECTION=Host=grc-db;Database=GrcAuthDb;Username=postgres;Password=YOUR_SECURE_PASSWORD;Port=5432
```

### Docker Compose (Environment Variables)

```yaml
environment:
  - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
  - ConnectionStrings__GrcAuthDb=${CONNECTION_STRING_GrcAuthDb}
```

---

## 📚 Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and setup
- [BUILD_AND_RUN_GUIDE.md](BUILD_AND_RUN_GUIDE.md) - Build and run instructions
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) - Production deployment
- [SECURITY_GUIDE.md](SECURITY_GUIDE.md) - Security best practices

---

## ✅ Summary

**Before Fix:**
- ❌ Both DbContexts pointed to same database (GrcMvcDb)
- ❌ No security isolation
- ❌ Duplicate configuration

**After Fix:**
- ✅ Two separate databases (GrcMvcDb + GrcAuthDb)
- ✅ Proper security isolation
- ✅ Correct connection string configuration
- ✅ Automated setup scripts provided

---

**Last Updated:** 2026-01-13
**Status:** ✅ **PRODUCTION READY**
