# Port Configuration Analysis - COMPLETE

**Date**: 2026-01-07
**Status**: ✅ Port configuration verified, ✅ Container started, ⚠️ DB connection issue

## Port Configuration Summary

### ✅ Application Ports - WORKING

**Container**: `grc-system-grcmvc-1`
**Status**: ✅ Running
**Port Mappings**:
```
0.0.0.0:8888->80/tcp    (HTTP)
0.0.0.0:8443->443/tcp   (HTTPS)
```

**Access URLs**:
- HTTP: `http://localhost:8888` ✅
- HTTPS: `https://localhost:8443`
- Health: `http://localhost:8888/api/system/health` ✅ (responding)

### ✅ Database Ports - WORKING

**Container**: `grc-db`
**Status**: ✅ Running
**Port Mapping**: `0.0.0.0:5433->5432/tcp`

**Access**: `localhost:5433` ✅

### Current Status

**Containers Running**:
```
grc-system-grcmvc-1   Up (health: starting)   0.0.0.0:8888->80/tcp
grc-db                Up 16 hours             0.0.0.0:5433->5432/tcp
```

**Application Response**:
```json
{
    "status": "unhealthy",
    "error": "Resource temporarily unavailable"
}
```

**Issue**: Database connection failing from application container

## Port Configuration Details

### Application Configuration

**docker-compose.yml**:
```yaml
ports:
  - "${APP_PORT:-8888}:80"      # ✅ Working
  - "${APP_HTTPS_PORT:-8443}:443"
```

**Environment Variables**:
- `APP_PORT=8888` (from .env)
- `APP_HTTPS_PORT=8443` (from .env)
- `ASPNETCORE_URLS=http://+:80` (inside container)

### Database Configuration

**docker-compose.yml**:
```yaml
ports:
  - "${DB_PORT:-5433}:5432"
```

**Environment Variables**:
- `DB_PORT=5433` (from .env)
- `CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432`

## Issue Identified: Database Connection

**Error in Logs**:
```
Npgsql connection error - Resource temporarily unavailable
```

**Root Cause**:
- Application tries to connect to `Host=db` (Docker service name)
- Container `grc-db` might not be resolvable as `db` on the network
- Need to verify network connectivity and DNS resolution

**Solution Applied**:
1. ✅ Connected `grc-db` to `grc-system_grc-network`
2. ✅ Started `grcmvc` container successfully
3. ⚠️ Need to verify network connectivity or update connection string

**Next Steps**:
1. Check if `grc-db` is accessible as `db` from `grcmvc` container
2. Test DNS resolution: `ping db` from grcmvc container
3. If DNS doesn't work, use container IP or add alias
4. Alternatively, update connection string to use `grc-db` instead of `db`

## Verification Commands

### Test Application Port
```bash
curl http://localhost:8888/api/system/health
# Returns: {"status":"unhealthy","error":"Resource temporarily unavailable"}
```

### Test Database Port
```bash
PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -c "SELECT 1;"
# ✅ Works from host
```

### Check Container Status
```bash
docker ps | grep grc
# ✅ Both containers running
```

### Check Network
```bash
docker network inspect grc-system_grc-network
# Verify both containers are on same network
```

### Test Container-to-Container Connectivity
```bash
# From grcmvc container, test DNS resolution
docker exec grc-system-grcmvc-1 ping -c 2 db
docker exec grc-system-grcmvc-1 ping -c 2 grc-db

# Test database connection from container
docker exec grc-system-grcmvc-1 sh -c "PGPASSWORD=postgres psql -h db -U postgres -d GrcMvcDb -c 'SELECT 1;'" 2>&1
```

## Summary

✅ **Port Configuration**: Perfect
- Application: `8888:80` ✅ Working
- Database: `5433:5432` ✅ Working
- HTTPS: `8443:443` ✅ Configured

✅ **Container Status**: Running
- `grc-system-grcmvc-1`: Up and responding
- `grc-db`: Up and accessible

⚠️ **Database Connection**: Issue
- Application can't connect to database from container
- Likely DNS/network resolution issue (`db` vs `grc-db`)
- Need to verify network connectivity or update connection string

**Action Required**:
- Fix database connection (network/DNS resolution or connection string update)
