# Database-Per-Tenant Architecture Implementation

## Overview

This implementation provides **maximum isolation** by giving each tenant its own dedicated PostgreSQL database. This is the most secure multi-tenant architecture pattern.

## Architecture

### Master Database
- **Purpose:** Stores tenant metadata only
- **Tables:** `Tenants`, `TenantUsers`, `OrganizationProfiles`
- **Connection:** Uses default `DefaultConnection` string
- **Access:** All authenticated users can query (for tenant resolution)

### Tenant Databases
- **Purpose:** Stores all tenant-specific data
- **Naming:** `grcmvc_tenant_{tenantId}` (lowercase, no hyphens)
- **Connection:** Dynamic per tenant
- **Isolation:** 100% - No way to access another tenant's data

## Key Components

### 1. ITenantDatabaseResolver
Resolves connection strings for tenant databases.

```csharp
var connectionString = _databaseResolver.GetConnectionString(tenantId);
var databaseName = _databaseResolver.GetDatabaseName(tenantId); // grcmvc_tenant_{guid}
```

### 2. TenantAwareDbContextFactory
Creates DbContext instances with tenant-specific connections.

```csharp
var context = _factory.CreateDbContext(); // Automatically uses current tenant's DB
```

### 3. TenantProvisioningService
Creates and migrates tenant databases.

```csharp
await _provisioningService.ProvisionTenantAsync(tenantId);
```

## Database Creation Flow

1. **New Tenant Created** → Call `ProvisionTenantAsync(tenantId)`
2. **Create Database** → `CREATE DATABASE grcmvc_tenant_{guid}`
3. **Run Migrations** → Apply all EF Core migrations
4. **Seed Data** (optional) → Initial tenant data

## Usage in Services

### Before (Shared Database):
```csharp
public class RiskService
{
    private readonly GrcDbContext _context;
    
    public async Task<List<Risk>> GetRisksAsync()
    {
        // Manual filtering required - EASY TO FORGET!
        var tenantId = _tenantContext.GetCurrentTenantId();
        return await _context.Risks
            .Where(r => r.TenantId == tenantId) // ❌ Easy to forget!
            .ToListAsync();
    }
}
```

### After (Database-Per-Tenant):
```csharp
public class RiskService
{
    private readonly IDbContextFactory<GrcDbContext> _contextFactory;
    
    public async Task<List<Risk>> GetRisksAsync()
    {
        // Automatic isolation - NO filtering needed!
        using var context = _contextFactory.CreateDbContext();
        return await context.Risks.ToListAsync(); // ✅ Only sees current tenant's data
    }
}
```

## Migration Steps

### Step 1: Update Existing Services

Replace direct `GrcDbContext` injection with `IDbContextFactory<GrcDbContext>`:

```csharp
// OLD:
public MyService(GrcDbContext context) { _context = context; }

// NEW:
public MyService(IDbContextFactory<GrcDbContext> contextFactory) 
{ 
    _contextFactory = contextFactory; 
}

// Usage:
using var context = _contextFactory.CreateDbContext();
```

### Step 2: Update Tenant Creation

When creating a new tenant, provision the database:

```csharp
// Create tenant record in master DB
var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Acme Corp" };
_masterContext.Tenants.Add(tenant);
await _masterContext.SaveChangesAsync();

// Provision tenant database
await _provisioningService.ProvisionTenantAsync(tenant.Id);
```

### Step 3: Remove TenantId Filters

Since each tenant has its own database, you can remove all `Where(r => r.TenantId == tenantId)` filters.

## Security Benefits

✅ **100% Data Isolation** - Physically impossible to access another tenant's data  
✅ **No Query Filter Bugs** - Can't forget to filter by TenantId  
✅ **Database-Level Security** - PostgreSQL user permissions per database  
✅ **Backup Per Tenant** - Easy to backup/restore individual tenants  
✅ **Compliance** - Meets strictest regulatory requirements  

## Performance Considerations

### Connection Pooling
- Each tenant database has its own connection pool
- Configured in `TenantDatabaseResolver.GetConnectionString()`
- Default: MinPoolSize=2, MaxPoolSize=20

### Database Limits
- PostgreSQL supports thousands of databases
- Monitor total database count
- Consider archiving inactive tenants

## Monitoring

### Health Checks
```csharp
// Check master database
builder.Services.AddHealthChecks()
    .AddNpgSql(masterConnectionString, name: "master-db");

// Check tenant database (per tenant)
var tenantConnection = _databaseResolver.GetConnectionString(tenantId);
// Add health check for tenant DB
```

### Metrics to Track
- Number of tenant databases
- Database sizes per tenant
- Connection pool usage
- Migration status per tenant

## Backup Strategy

### Per-Tenant Backups
```bash
# Backup specific tenant
pg_dump -h localhost -U postgres grcmvc_tenant_{guid} > tenant_backup.sql

# Restore tenant
psql -h localhost -U postgres grcmvc_tenant_{guid} < tenant_backup.sql
```

### Master Database Backup
```bash
# Backup master (tenant metadata)
pg_dump -h localhost -U postgres GrcMvcDb > master_backup.sql
```

## Troubleshooting

### Database Not Found
**Error:** `database "grcmvc_tenant_xxx" does not exist`

**Solution:**
```csharp
if (!await _databaseResolver.DatabaseExistsAsync(tenantId))
{
    await _provisioningService.ProvisionTenantAsync(tenantId);
}
```

### Migration Pending
**Error:** `There are pending migrations`

**Solution:**
```csharp
await _databaseResolver.MigrateTenantDatabaseAsync(tenantId);
```

### Connection String Issues
**Error:** `Connection string is null`

**Check:**
1. TenantId is valid (not Guid.Empty)
2. `ITenantContextService.GetCurrentTenantId()` returns valid ID
3. User is authenticated and associated with tenant

## Testing

### Unit Tests
```csharp
[Test]
public async Task TenantA_CannotAccess_TenantB_Database()
{
    // Create two tenants
    var tenantA = await CreateTenantAsync("TenantA");
    var tenantB = await CreateTenantAsync("TenantB");
    
    // Create context for TenantA
    var contextA = CreateTenantContext(tenantA.Id);
    var risksA = await contextA.Risks.ToListAsync();
    
    // Verify TenantA cannot see TenantB's data
    // (This is impossible with database-per-tenant, but test anyway)
    Assert.That(risksA, Does.Not.Contain(tenantBRisk));
}
```

## Next Steps

1. ✅ **Implemented:** Core infrastructure
2. ⏳ **TODO:** Update all services to use `IDbContextFactory`
3. ⏳ **TODO:** Update tenant creation to provision databases
4. ⏳ **TODO:** Add health checks per tenant
5. ⏳ **TODO:** Add monitoring and alerting
6. ⏳ **TODO:** Create backup automation

---

**Status:** ✅ Core implementation complete  
**Next:** Update services to use tenant-aware contexts
