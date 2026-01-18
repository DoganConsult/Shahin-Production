# ABP Framework Migration Test Guide

## 🎯 Purpose

Test database migrations following **ABP Framework's standard process only**.

---

## 📋 ABP Migration Process

ABP Framework uses **EF Core migrations** through its `EntityFrameworkCore` integration:

1. **Migrations are created** using `dotnet ef migrations add`
2. **Migrations are applied** using `dotnet ef database update`
3. **ABP's `AddAbpDbContext`** registers the DbContext with ABP
4. **ABP handles** connection string resolution and configuration

---

## 🔧 ABP Configuration

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs`

```csharp
// ABP Entity Framework Core configuration
Configure<AbpDbContextOptions>(options => {
    options.UseNpgsql(npgsqlOptions => {
        npgsqlOptions.CommandTimeout(300); // 5 minutes for migrations
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

// Register DbContext with ABP
context.Services.AddAbpDbContext<GrcDbContext>(options => {
    options.AddDefaultRepositories(includeAllEntities: true);
});
```

---

## 🧪 Test Commands

### 1. List All Migrations

```powershell
.\test-abp-migrations.ps1 -ListMigrations
```

**Or manually:**
```bash
cd Shahin-Jan-2026\src\GrcMvc

# List GrcDbContext migrations
dotnet ef migrations list --context GrcDbContext

# List GrcAuthDbContext migrations
dotnet ef migrations list --context GrcAuthDbContext
```

### 2. Check Pending Migrations

```powershell
.\test-abp-migrations.ps1 -CheckPending
```

**Checks if there are migrations that haven't been applied to the database.**

### 3. Test Migration (Dry Run)

```powershell
.\test-abp-migrations.ps1 -TestMigration
```

**Tests if migrations can be applied without actually applying them.**

### 4. Full Migration Test

```powershell
# Set connection string first
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"

# Or use Railway DATABASE_URL
$env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"

# Run test
.\test-abp-migrations.ps1
```

---

## 📊 ABP Migration Commands

### Create New Migration

```bash
cd Shahin-Jan-2026\src\GrcMvc

# For GrcDbContext (main database)
dotnet ef migrations add <MigrationName> --context GrcDbContext

# For GrcAuthDbContext (auth database)
dotnet ef migrations add <MigrationName> --context GrcAuthDbContext --output-dir Migrations/Auth
```

### Apply Migrations

```bash
cd Shahin-Jan-2026\src\GrcMvc

# Apply GrcDbContext migrations
dotnet ef database update --context GrcDbContext

# Apply GrcAuthDbContext migrations
dotnet ef database update --context GrcAuthDbContext
```

### Check Migration Status

```bash
# List applied migrations
dotnet ef migrations list --context GrcDbContext

# Check pending migrations
dotnet ef migrations list --context GrcDbContext | Select-String "Pending"
```

---

## 🔍 ABP Migration Locations

### Migration Files

- **GrcDbContext:** `Shahin-Jan-2026/src/GrcMvc/Migrations/`
- **GrcAuthDbContext:** `Shahin-Jan-2026/src/GrcMvc/Migrations/Auth/`

### Design-Time Factory

**File:** `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContextFactory.cs`

Used by EF Core tools to create DbContext instances for migrations.

---

## ✅ ABP Migration Test Checklist

- [ ] Connection string is configured
- [ ] Project builds successfully
- [ ] Migrations are listed correctly
- [ ] No pending migrations (or pending migrations are expected)
- [ ] Database connection works
- [ ] Migration dry-run succeeds
- [ ] Migration can be applied (if needed)

---

## 🚀 Railway Migration Test

### Via Railway SSH

```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

# Once connected:
cd /app
export ConnectionStrings__DefaultConnection="$DATABASE_URL"

# List migrations
dotnet ef migrations list --context GrcDbContext
dotnet ef migrations list --context GrcAuthDbContext

# Check pending
dotnet ef migrations list --context GrcDbContext | grep -i pending
```

---

## 📝 ABP Migration Best Practices

1. **Always use migrations** - Never use `EnsureCreated()` in production
2. **Test migrations first** - Use dry-run before applying
3. **Backup database** - Before applying migrations in production
4. **Check pending migrations** - Before deployment
5. **Use ABP's process** - Follow ABP's standard migration workflow

---

## 🎯 Summary

**ABP Migration Process:**
1. ✅ Uses EF Core migrations
2. ✅ Configured via `AddAbpDbContext`
3. ✅ Connection strings resolved through ABP
4. ✅ Standard `dotnet ef` commands
5. ✅ Design-time factory for tooling

**Test Script:** `test-abp-migrations.ps1`

**Status:** ✅ Ready to test ABP migrations!

---

**Run the test script to verify ABP migrations are working correctly!** 🚀
