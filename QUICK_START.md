# Quick Start - GRC Platform

## 🚀 One-Command Startup

```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026"
.\scripts\startup-all.ps1
```

This will start all services in the correct order:
1. Database (PostgreSQL) + Redis
2. Backend API (ASP.NET Core)
3. Frontend (Next.js)

---

## 📋 Manual Startup Sequence

### Step 1: Database (Required First)
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026"
docker-compose up -d db redis
```

**Wait for:** Database health check (10-15 seconds)

**Verify:**
```powershell
docker exec grc-db pg_isready -U postgres
# Should return: /var/run/postgresql:5432 - accepting connections
```

---

### Step 2: Backend API (After Database)
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
dotnet run --urls "http://localhost:5010"
```

**Wait for:** Application to start (15-30 seconds)

**Verify:**
```powershell
curl http://localhost:5010/health
# Or open in browser: http://localhost:5010/health
```

---

### Step 3: Frontend (After API)
```powershell
cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
npm run dev
```

**Wait for:** Next.js dev server (5-10 seconds)

**Verify:**
```powershell
# Open in browser: http://localhost:3003
```

---

## 🎯 Service Ports

| Service | Port | URL |
|---------|------|-----|
| Database | 5432 | `localhost:5432` |
| Redis | 6379 | `localhost:6379` |
| Backend API | 5010 | `http://localhost:5010` |
| Frontend | 3003 | `http://localhost:3003` |

---

## ⚡ Quick Commands

### Start Everything
```powershell
.\scripts\startup-all.ps1
```

### Start Only Database
```powershell
.\scripts\startup-all.ps1 -SkipBackend -SkipFrontend
```

### Start Only Backend
```powershell
.\scripts\startup-all.ps1 -SkipDatabase -SkipFrontend
```

### Start Only Frontend
```powershell
.\scripts\startup-all.ps1 -SkipDatabase -SkipBackend
```

### Stop Everything
```powershell
# Stop frontend/backend: Ctrl+C in their windows
docker-compose down
```

---

## 🔍 Health Checks

### Database
```powershell
docker exec grc-db pg_isready -U postgres
```

### Backend API
```powershell
curl http://localhost:5010/health
Invoke-WebRequest -Uri "http://localhost:5010/health" -UseBasicParsing
```

### Frontend
```powershell
Invoke-WebRequest -Uri "http://localhost:3003" -UseBasicParsing
```

---

## 📝 Startup Sequence Summary

```
1. Database (PostgreSQL)  → Port 5432
   ↓ (wait 10-15 seconds)
2. Redis                  → Port 6379
   ↓ (wait 2 seconds)
3. Backend API            → Port 5010
   ↓ (wait 15-30 seconds)
4. Frontend               → Port 3003
```

**Total Time:** ~30-60 seconds

---

## ⚠️ Troubleshooting

### Database Not Starting
```powershell
# Check if port 5432 is in use
netstat -ano | findstr :5432

# Check Docker logs
docker-compose logs db
```

### Backend Can't Connect to Database
```powershell
# Verify database is accessible
docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"

# Check connection string
cat .env | Select-String "DB_"
```

### Frontend Can't Reach API
```powershell
# Verify API is running
curl http://localhost:5010/health

# Check CORS configuration in backend
```

---

**For detailed information, see:** `STARTUP_SEQUENCE.md`
