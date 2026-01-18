# WSL ABP Migration Test Guide

## 🐧 WSL/Linux Environment

**Current Environment:** WSL (Windows Subsystem for Linux)  
**Path:** `/mnt/c/Shahin-ai`  
**User:** `dogan@Dogan-lap06`

---

## 🧪 Test ABP Migrations in WSL

### Step 1: Set Connection String

```bash
# For Railway
export DATABASE_URL="postgresql://postgres:password@host.railway.app:5432/railway"

# Or standard format
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
```

### Step 2: Navigate to Project

```bash
cd /mnt/c/Shahin-ai/Shahin-Jan-2026/src/GrcMvc
```

### Step 3: Run Migration Tests

**List all migrations:**
```bash
cd /mnt/c/Shahin-ai
./test-abp-migrations.sh --list
```

**Check pending migrations:**
```bash
./test-abp-migrations.sh --check-pending
```

**Test migration (dry-run):**
```bash
./test-abp-migrations.sh --test
```

**Full test:**
```bash
./test-abp-migrations.sh
```

---

## 📋 Direct ABP Migration Commands (WSL)

### List Migrations

```bash
cd /mnt/c/Shahin-ai/Shahin-Jan-2026/src/GrcMvc

# GrcDbContext
dotnet ef migrations list --context GrcDbContext

# GrcAuthDbContext
dotnet ef migrations list --context GrcAuthDbContext
```

### Check Pending Migrations

```bash
dotnet ef migrations list --context GrcDbContext | grep -i pending
dotnet ef migrations list --context GrcAuthDbContext | grep -i pending
```

### Apply Migrations

```bash
# Set connection string
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"

# Apply GrcDbContext migrations
dotnet ef database update --context GrcDbContext

# Apply GrcAuthDbContext migrations
dotnet ef database update --context GrcAuthDbContext
```

---

## 🔧 WSL Environment Setup

### Check .NET SDK

```bash
dotnet --version
```

### Check EF Core Tools

```bash
dotnet ef --version
```

If not installed:
```bash
dotnet tool install --global dotnet-ef
```

### Check PostgreSQL Connection

```bash
# Test connection
psql "$ConnectionStrings__DefaultConnection" -c "SELECT version();"
```

---

## 🚂 Railway Connection from WSL

### Set Railway DATABASE_URL

```bash
export DATABASE_URL="postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@centerbeam.proxy.rlwy.net:11539/railway"
```

### Test Connection

```bash
psql "$DATABASE_URL" -c "SELECT version(), current_database(), current_user;"
```

---

## ✅ ABP Migration Test Checklist (WSL)

- [ ] .NET SDK installed (`dotnet --version`)
- [ ] EF Core tools installed (`dotnet ef --version`)
- [ ] Connection string set (`ConnectionStrings__DefaultConnection` or `DATABASE_URL`)
- [ ] Project builds (`dotnet build`)
- [ ] Migrations listed (`dotnet ef migrations list`)
- [ ] No pending migrations (or expected)
- [ ] Migration dry-run succeeds

---

## 🎯 Quick Test Commands

```bash
# 1. Set connection string
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"

# 2. Navigate to project
cd /mnt/c/Shahin-ai/Shahin-Jan-2026/src/GrcMvc

# 3. List migrations
dotnet ef migrations list --context GrcDbContext
dotnet ef migrations list --context GrcAuthDbContext

# 4. Check pending
dotnet ef migrations list --context GrcDbContext | grep -i pending

# 5. Test migration (dry-run)
dotnet ef database update --context GrcDbContext --dry-run
```

---

**Ready to test ABP migrations in WSL!** 🐧
