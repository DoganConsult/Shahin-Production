# Startup Sequence - Database → API Server → Endpoints

## Proper Startup Order

The GRC platform requires services to start in a specific order to ensure dependencies are met.

---

## 🗄️ STEP 1: Database (PostgreSQL)

**Must start FIRST** - All other services depend on it.

### Docker Compose (Recommended)
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026"
docker-compose up -d db redis
```

### Manual PostgreSQL (Local Development)
```powershell
# If using local PostgreSQL
# Ensure PostgreSQL service is running
Get-Service postgresql*
# Or start manually:
net start postgresql-x64-15
```

### Verification
```powershell
# Check if database is accessible
docker exec -it grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"
# Or for local:
psql -U postgres -d GrcMvcDb -c "SELECT 1;"
```

**Wait for:** Database health check to pass (pg_isready)

---

## 🔴 STEP 2: Redis (Optional but Recommended)

**Start after database** - Used for caching and session storage.

### Docker Compose
```powershell
docker-compose up -d redis
```

### Verification
```powershell
docker exec -it grc-redis redis-cli ping
# Should return: PONG
```

**Wait for:** Redis to respond to PING

---

## 🚀 STEP 3: Backend API Server (ASP.NET Core)

**Start after database is ready** - Depends on PostgreSQL connection.

### Local Development
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
dotnet run --urls "http://localhost:5010"
```

### Docker Compose
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026"
docker-compose up -d grcmvc
```

### Verification
```powershell
# Check health endpoint
curl http://localhost:5010/health
# Or in browser:
# http://localhost:5010/health
```

**Wait for:**
- Database migrations to complete
- Application to start listening on port 5010
- Health endpoint to return 200 OK

**Expected Logs:**
```
[ENV] Loaded environment variables from: ...
[DB] Database connection successful
[GOLDEN_PATH] Application started
Now listening on: http://localhost:5010
```

---

## 🌐 STEP 4: Frontend Endpoint (Next.js)

**Start after API server is ready** - Frontend calls backend APIs.

### Local Development
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
npm run dev
```

### Docker Compose (if configured)
```powershell
docker-compose up -d frontend
```

### Verification
```powershell
# Check if frontend is running
curl http://localhost:3003
# Or in browser:
# http://localhost:3003
```

**Wait for:**
- Next.js dev server to start
- Port 3003 to be listening
- Frontend to load without errors

---

## 📋 Complete Startup Script

### PowerShell Script (All-in-One)
```powershell
# startup-all.ps1
Write-Host "🚀 Starting GRC Platform..." -ForegroundColor Green

# Step 1: Database
Write-Host "`n📊 Step 1: Starting Database..." -ForegroundColor Cyan
cd "C:\Shahin-ai\Shahin-Jan-2026"
docker-compose up -d db redis
Start-Sleep -Seconds 10

# Verify database
Write-Host "   Verifying database..." -ForegroundColor Yellow
$dbReady = $false
for ($i = 0; $i -lt 30; $i++) {
    try {
        docker exec grc-db pg_isready -U postgres | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $dbReady = $true
            Write-Host "   ✅ Database is ready" -ForegroundColor Green
            break
        }
    } catch {
        Start-Sleep -Seconds 2
    }
}
if (-not $dbReady) {
    Write-Host "   ❌ Database failed to start" -ForegroundColor Red
    exit 1
}

# Step 2: Backend API
Write-Host "`n🔧 Step 2: Starting Backend API..." -ForegroundColor Cyan
cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --urls 'http://localhost:5010'"
Start-Sleep -Seconds 15

# Verify API
Write-Host "   Verifying API server..." -ForegroundColor Yellow
$apiReady = $false
for ($i = 0; $i -lt 30; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5010/health" -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
            Write-Host "   ✅ API server is ready" -ForegroundColor Green
            break
        }
    } catch {
        Start-Sleep -Seconds 2
    }
}
if (-not $apiReady) {
    Write-Host "   ❌ API server failed to start" -ForegroundColor Red
    exit 1
}

# Step 3: Frontend
Write-Host "`n🌐 Step 3: Starting Frontend..." -ForegroundColor Cyan
cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "npm run dev"

Write-Host "`n✅ All services started!" -ForegroundColor Green
Write-Host "`n📌 URLs:" -ForegroundColor Yellow
Write-Host "   Backend API: http://localhost:5010" -ForegroundColor White
Write-Host "   Frontend:    http://localhost:3003" -ForegroundColor White
Write-Host "   Database:    localhost:5432" -ForegroundColor White
```

---

## 🔄 Quick Commands

### Start Everything (Docker)
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026"
docker-compose up -d db redis grcmvc
# Then start frontend separately (not in docker-compose yet)
cd grc-frontend
npm run dev
```

### Start Everything (Local Development)
```powershell
# Terminal 1: Database
docker-compose up -d db redis

# Terminal 2: Backend API
cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
dotnet run --urls "http://localhost:5010"

# Terminal 3: Frontend
cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
npm run dev
```

### Stop Everything
```powershell
# Stop frontend (Ctrl+C in terminal)

# Stop backend (Ctrl+C in terminal)

# Stop database
docker-compose down
```

---

## ⚠️ Common Issues

### Issue 1: Database Not Ready
**Symptom:** Backend fails with "connection refused" or "database does not exist"

**Solution:**
```powershell
# Wait for database health check
docker exec grc-db pg_isready -U postgres
# Should return: /var/run/postgresql:5432 - accepting connections
```

### Issue 2: API Server Can't Connect to Database
**Symptom:** Backend logs show database connection errors

**Solution:**
1. Check database is running: `docker ps | grep grc-db`
2. Check connection string in `.env` or `appsettings.json`
3. Verify database exists: `docker exec grc-db psql -U postgres -l`

### Issue 3: Frontend Can't Reach API
**Symptom:** Frontend shows API errors or CORS issues

**Solution:**
1. Verify API is running: `curl http://localhost:5010/health`
2. Check API URL in frontend `.env.local` or `next.config.mjs`
3. Ensure CORS is configured in backend `Program.cs`

---

## 📊 Health Check Endpoints

### Database
```powershell
docker exec grc-db pg_isready -U postgres
```

### Backend API
```powershell
curl http://localhost:5010/health
curl http://localhost:5010/health/ready
```

### Frontend
```powershell
curl http://localhost:3003
```

---

## 🔍 Verification Checklist

Before proceeding to next step, verify:

- [ ] **Database**: `pg_isready` returns success
- [ ] **Redis**: `redis-cli ping` returns PONG
- [ ] **Backend API**: `/health` endpoint returns 200
- [ ] **Backend API**: Logs show "Application started"
- [ ] **Frontend**: Port 3003 is listening
- [ ] **Frontend**: No console errors in browser

---

## 📝 Startup Sequence Summary

```
1. Database (PostgreSQL)     → Port 5432
   ↓ (wait for health check)
2. Redis (Optional)          → Port 6379
   ↓ (wait for ping)
3. Backend API (ASP.NET)     → Port 5010
   ↓ (wait for /health)
4. Frontend (Next.js)        → Port 3003
```

**Total Startup Time:** ~30-60 seconds (depending on migrations)

---

## 🎯 Quick Reference

| Service | Port | Health Check | Start Command |
|---------|------|--------------|---------------|
| PostgreSQL | 5432 | `pg_isready` | `docker-compose up -d db` |
| Redis | 6379 | `redis-cli ping` | `docker-compose up -d redis` |
| Backend API | 5010 | `curl /health` | `dotnet run --urls "http://localhost:5010"` |
| Frontend | 3003 | `curl /` | `npm run dev` |

---

**Last Updated:** 2026-01-19
