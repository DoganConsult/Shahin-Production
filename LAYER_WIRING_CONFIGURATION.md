# Layer Wiring Configuration

## Overview
This document describes how all layers of the GRC platform are wired together.

---

## 🏗️ Architecture Layers

```
┌─────────────────────────────────────────┐
│         Frontend (Next.js)               │
│         Port: 3003                       │
│         http://localhost:3003            │
└──────────────┬──────────────────────────┘
               │ HTTP/REST API
               │ NEXT_PUBLIC_API_URL
               ▼
┌─────────────────────────────────────────┐
│      Backend API (ASP.NET Core)         │
│      Port: 5010                          │
│      http://localhost:5010              │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┐
       │                │
       ▼                ▼
┌─────────────┐  ┌─────────────┐
│ PostgreSQL  │  │    Redis    │
│ Port: 5432  │  │ Port: 6379  │
└─────────────┘  └─────────────┘
```

---

## 🔌 Frontend → Backend Wiring

### Environment Variables
**File**: `grc-frontend/.env.local`

```env
NEXT_PUBLIC_API_URL=http://localhost:5010
NEXT_PUBLIC_FRONTEND_URL=http://localhost:3003
```

### API Client Configuration
**Files**:
- `grc-frontend/src/lib/api/client.ts` - Main API client
- `grc-frontend/src/components/providers/api-provider.tsx` - API context provider
- `grc-frontend/src/lib/api/auth.ts` - Authentication API
- `grc-frontend/src/app/(auth)/login/page.tsx` - Login page
- `grc-frontend/src/app/(auth)/trial/page.tsx` - Trial signup page
- `grc-frontend/src/app/(auth)/forgot-password/page.tsx` - Password reset
- `grc-frontend/src/app/(auth)/invitation/page.tsx` - Invitation acceptance

**All files use**: `process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'`

### Fixed Issues
✅ **Before**: Inconsistent API URLs (3006, 5000, 5010)
✅ **After**: All files standardized to `http://localhost:5010`

---

## 🔧 Backend → Database Wiring

### Connection String Resolution (Priority Order)

1. **Environment Variable**: `ConnectionStrings__DefaultConnection`
2. **Environment Variable**: `DATABASE_URL` (parsed)
3. **Individual Variables**: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
4. **Development Fallback**: `Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432`

**File**: `src/GrcMvc/Program.cs` (lines 195-233)

### Database Context
- **Main DB**: `GrcDbContext` → `GrcMvcDb`
- **Auth DB**: `GrcAuthDbContext` → `GrcAuthDb` (or same as main)

---

## 🔴 Backend → Redis Wiring

### Configuration
**File**: `src/GrcMvc/Program.cs`

Redis is used for:
- Session storage
- Caching
- Background job queues

**Connection**: `localhost:6379` (default)

---

## 🌐 Host-Based Routing

### Middleware Order
1. `HostRoutingMiddleware` - Routes based on hostname
2. `TenantResolutionMiddleware` - Resolves tenant from request
3. `OnboardingRedirectMiddleware` - Redirects incomplete onboarding

**File**: `src/GrcMvc/Program.cs`

### Host Routing Rules

| Hostname | Purpose | Port | Skip Tenant Resolution |
|----------|---------|------|----------------------|
| `admin.shahin-ai.com` | Platform admin only | 5010 | ✅ Yes |
| `login.shahin-ai.com` | All users login | 5010 | ✅ Yes |
| `shahin-ai.com` | Landing page (proxied to frontend) | 5010 → 3003 | ❌ No |

**File**: `src/GrcMvc/Middleware/HostRoutingMiddleware.cs`

---

## ✅ Verification Checklist

### Frontend
- [x] All API URLs point to `http://localhost:5010`
- [x] `.env.local` file created with correct configuration
- [x] API client uses environment variable with fallback

### Backend
- [x] Health endpoint: `/health`
- [x] Database connection string resolution working
- [x] Redis connection configured
- [x] Host routing middleware active

### Database
- [x] PostgreSQL running on port 5432
- [x] Database `GrcMvcDb` exists
- [x] Migrations applied

### Redis
- [x] Redis running on port 6379
- [x] Connection testable via `redis-cli ping`

---

## 🚀 Startup Sequence

1. **Database & Redis** (Docker)
   ```powershell
   docker-compose up -d db redis
   ```

2. **Backend API**
   ```powershell
   cd src/GrcMvc
   dotnet run --urls "http://localhost:5010"
   ```

3. **Frontend**
   ```powershell
   cd grc-frontend
   npm run dev
   ```

---

## 🔍 Troubleshooting

### Backend Not Responding
1. Check if process is running: `Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" }`
2. Check if port is listening: `netstat -ano | Select-String ":5010"`
3. Check backend window for errors
4. Verify database connection: `docker exec grc-db pg_isready -U postgres`

### Frontend Can't Connect to Backend
1. Verify `NEXT_PUBLIC_API_URL` in `.env.local`
2. Check browser console for CORS errors
3. Verify backend is running on port 5010
4. Check `next.config.mjs` for proxy/redirect configuration

### Database Connection Issues
1. Verify Docker containers: `docker ps`
2. Check connection string in `appsettings.json` or environment variables
3. Test connection: `docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"`

---

## 📝 Notes

- All API calls from frontend should use the centralized API client (`src/lib/api/client.ts`)
- Backend uses ASP.NET Core middleware pipeline for request processing
- Host-based routing allows different entry points for different user types
- Environment variables take precedence over hardcoded values

---

**Last Updated**: 2026-01-20
