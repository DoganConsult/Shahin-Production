# Database Connection String Configuration Fix

## Problem Summary

The application was connecting to `127.0.0.1:5432` instead of the Docker container name `db`, causing database connection failures during startup seeding.

## Root Cause

1. **Docker Compose** sets: `ConnectionStrings__DefaultConnection=Host=db;...`
2. **Program.cs** was reading individual environment variables (`DB_HOST`, `DB_PORT`, etc.) that didn't exist
3. Code defaulted to `localhost` when variables were missing
4. This hardcoded `localhost` value **overrode** the correct Docker Compose value

## Solution (ASP.NET Core Best Practices)

### ✅ What We Fixed

1. **Use ASP.NET Core's Built-in Configuration System**
   - `WebApplication.CreateBuilder(args)` automatically loads configuration in this order:
     1. `appsettings.json`
     2. `appsettings.{Environment}.json`
     3. Environment variables (`ConnectionStrings__DefaultConnection`)
     4. Command-line arguments

2. **Support Multiple Configuration Formats**
   - **Standard**: `ConnectionStrings__DefaultConnection` (Docker Compose format) ✅ **PRIORITY**
   - **Individual**: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD` (fallback)
   - **File-based**: `appsettings.json` defaults (development only)

3. **Removed Hardcoded Defaults**
   - No more `localhost` fallback that overrides correct values
   - Only builds connection string from `DB_*` variables if `DB_HOST` is explicitly set

### Code Changes

**Before (❌ Problematic):**
```csharp
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"; // ❌ Always defaults to localhost
var connectionString = $"Host={dbHost};...";
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString; // ❌ Overrides correct value
```

**After (✅ Best Practice):**
```csharp
// 1. Try standard configuration first (respects Docker Compose)
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Only build from individual variables if not already set
if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    // Only build if DB_HOST is explicitly set (no localhost fallback)
    if (!string.IsNullOrWhiteSpace(dbHost))
    {
        connectionString = $"Host={dbHost};...";
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
    }
}
```

## Configuration Precedence

The application now respects this order (highest to lowest priority):

1. **Environment Variables** (`ConnectionStrings__DefaultConnection`) ← Docker Compose uses this ✅
2. **appsettings.Production.json** (`ConnectionStrings.DefaultConnection`)
3. **appsettings.json** (`ConnectionStrings.DefaultConnection`)
4. **Individual DB_* Variables** (only if standard format not found)

## Docker Compose Configuration

The `docker-compose.grcmvc.yml` already uses the correct format:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432
```

✅ **No changes needed** - this is the standard ASP.NET Core format.

## Benefits

1. ✅ **Follows ASP.NET Core Best Practices** - Uses built-in configuration system
2. ✅ **Flexible** - Supports multiple configuration formats
3. ✅ **No Hardcoded Defaults** - Won't override correct values
4. ✅ **Docker-Friendly** - Works with standard Docker Compose environment variables
5. ✅ **Maintainable** - Clear configuration precedence and documentation

## Testing

After deployment, verify:

1. **Check logs** - Should see connection to `db` container, not `127.0.0.1`
2. **Health endpoint** - `/health` should show database as healthy
3. **Startup seeding** - Should complete without connection errors

## Migration Notes

- ✅ **Backward Compatible** - Still supports `DB_*` individual variables
- ✅ **No Breaking Changes** - Existing configurations continue to work
- ✅ **Improved Error Messages** - Clear guidance on how to configure

---

**Date**: 2026-01-09  
**Status**: ✅ Implemented  
**Impact**: Fixes database connection issues in Docker deployments
