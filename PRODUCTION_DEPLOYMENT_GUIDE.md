# 🚀 Production Deployment Guide - Shahin AI GRC Platform

**Last Updated:** January 12, 2026  
**Build Status:** ✅ Production Ready

---

## 📋 Pre-Deployment Checklist

### ✅ Completed
- [x] Build succeeds with 0 errors, 0 warnings
- [x] Production binaries generated (36.27 MB DLL)
- [x] RBAC system fully implemented (15 roles, 60+ permissions)
- [x] Multi-tenancy configured (Trial, Platform Admin, API flows)
- [x] Onboarding redirect middleware implemented
- [x] Database migrations ready
- [x] Docker Compose configuration complete
- [x] Marketing site dependency fixed (`next-intl` added)

### ⚠️ Before Deploying
- [ ] Verify `.env.production` file exists and is configured
- [ ] Ensure PostgreSQL database is accessible
- [ ] Verify Redis is accessible
- [ ] Check SSL certificates (if using HTTPS)
- [ ] Review environment variables

---

## 🚀 Quick Start Deployment

### Option 1: Automated Script (Recommended)

```powershell
# Navigate to project root
cd C:\Shahin-ai\Shahin-Jan-2026

# Full rebuild and deploy
.\scripts\deploy-production.ps1 rebuild-full
.\scripts\deploy-production.ps1 deploy

# Monitor deployment
.\scripts\deploy-production.ps1 status
.\scripts\deploy-production.ps1 logs
```

### Option 2: Manual Steps

```powershell
# 1. Build production binaries
dotnet build src\GrcMvc\GrcMvc.csproj -c Release
dotnet publish src\GrcMvc\GrcMvc.csproj -c Release -o src\GrcMvc\bin\Release\net8.0\publish

# 2. Build Docker images
docker-compose -f docker-compose.production.yml build

# 3. Start services
docker-compose -f docker-compose.production.yml --env-file .env.production up -d

# 4. Check status
docker-compose -f docker-compose.production.yml ps
```

---

## 🔧 Configuration

### Environment Variables (.env.production)

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD
ConnectionStrings__GrcAuthDb=Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD

# JWT Authentication
JWT__SecretKey=YOUR_SECRET_KEY_MIN_32_CHARS
JWT__Issuer=https://your-domain.com
JWT__Audience=https://your-domain.com

# Redis Cache
Redis__ConnectionString=localhost:6379

# Claude AI (Optional)
ClaudeAgents__Enabled=false

# Application URLs
ASPNETCORE_URLS=http://+:80
ASPNETCORE_ENVIRONMENT=Production
```

### Port Configuration

| Service | Container Port | Host Port | Purpose |
|---------|---------------|-----------|---------|
| GRC Portal | 80 | 5000, 8080 | Main application |
| Marketing Site | 3000 | 3000 | Public website |
| PostgreSQL | 5432 | 5433 | Database (internal) |
| Redis | 6379 | 6380 | Cache (internal) |

---

## 📊 Service Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Production Stack                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────┐      ┌──────────────┐               │
│  │   GRC Portal │──────│  PostgreSQL  │               │
│  │  (Port 5000) │      │  (Port 5433)  │               │
│  └──────────────┘      └──────────────┘               │
│         │                                              │
│         │                                              │
│  ┌──────────────┐      ┌──────────────┐               │
│  │ Marketing    │      │    Redis     │               │
│  │ Site (3000)  │      │  (Port 6380)  │               │
│  └──────────────┘      └──────────────┘               │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🔐 Security Configuration

### 1. Database Security
- ✅ PostgreSQL not exposed to public (internal Docker network)
- ✅ Strong password required
- ✅ Connection string encrypted in environment variables

### 2. Application Security
- ✅ JWT authentication configured
- ✅ Password requirements: 12+ chars, uppercase, lowercase, number, special
- ✅ Lockout after 3 failed attempts
- ✅ Email confirmation required in production

### 3. Network Security
- ✅ Services communicate via internal Docker network
- ✅ Only necessary ports exposed
- ✅ Health checks configured

---

## 🧪 Testing Deployment

### 1. Health Check
```powershell
# Check application health
Invoke-WebRequest -Uri "http://localhost:5000/health"

# Expected: HTTP 200 OK
```

### 2. Service Status
```powershell
# Check all containers
docker-compose -f docker-compose.production.yml ps

# Expected: All services "Up" and "healthy"
```

### 3. Test Tenant Creation

#### Trial Signup Flow
1. Navigate to: `http://localhost:5000/trial`
2. Fill registration form
3. Submit
4. Should redirect to: `/Onboarding/Start/{tenant-slug}`

#### Platform Admin Flow
1. Login as platform admin
2. Navigate to: `/admin/tenants`
3. Create new tenant
4. Assign admin user
5. Admin should be redirected to onboarding on first login

#### API Flow
```powershell
# Create tenant via API
$body = @{
    organizationName = "Test Company"
    adminEmail = "admin@test.com"
    password = "SecureP@ss123!"
    acceptTerms = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/agent/tenant/create" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

---

## 📝 Post-Deployment Verification

### Application Checks
- [ ] Health endpoint responds: `http://localhost:5000/health`
- [ ] Home page loads: `http://localhost:5000/`
- [ ] Login page accessible: `http://localhost:5000/Account/Login`
- [ ] Trial signup works: `http://localhost:5000/trial`
- [ ] Marketing site loads: `http://localhost:3000/`

### Database Checks
- [ ] Database migrations applied
- [ ] Seed data initialized
- [ ] Permissions seeded
- [ ] Roles created

### RBAC Checks
- [ ] Permissions defined in database
- [ ] Roles assigned correctly
- [ ] Feature flags working
- [ ] Menu visibility based on permissions

### Multi-Tenancy Checks
- [ ] Tenant creation works
- [ ] Tenant isolation enforced
- [ ] Onboarding redirect works
- [ ] First admin user created correctly

---

## 🐛 Troubleshooting

### Issue: Container Won't Start

**Check logs:**
```powershell
docker-compose -f docker-compose.production.yml logs grcmvc-prod
```

**Common causes:**
- Missing environment variables
- Database connection failed
- Port already in use

### Issue: Health Check Fails

**Verify:**
```powershell
# Check if application is listening
docker exec shahin-grc-production curl http://localhost:80/health

# Check environment variables
docker exec shahin-grc-production env | grep ASPNETCORE
```

### Issue: Database Connection Failed

**Verify:**
```powershell
# Check database container
docker-compose -f docker-compose.production.yml ps db-prod

# Test connection
docker exec shahin-grc-db-prod psql -U postgres -d GrcMvcDb -c "SELECT 1"
```

### Issue: Onboarding Redirect Not Working

**Check:**
1. Verify `OnboardingRedirectMiddleware` is registered in `Program.cs`
2. Check tenant `OnboardingStatus` in database
3. Verify `FirstAdminUserId` is set correctly
4. Check middleware logs

---

## 📈 Monitoring

### Application Logs
```powershell
# View real-time logs
.\scripts\deploy-production.ps1 logs grcmvc-prod

# View last 100 lines
docker-compose -f docker-compose.production.yml logs --tail=100 grcmvc-prod
```

### Database Logs
```powershell
docker-compose -f docker-compose.production.yml logs db-prod
```

### Health Monitoring
```powershell
# Check health every 30 seconds
while ($true) {
    Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
    Start-Sleep -Seconds 30
}
```

---

## 🔄 Maintenance

### Update Application
```powershell
# 1. Pull latest code
git pull

# 2. Rebuild
.\scripts\deploy-production.ps1 rebuild-full

# 3. Redeploy
.\scripts\deploy-production.ps1 restart
```

### Backup Database
```powershell
# Create backup
docker exec shahin-grc-db-prod pg_dump -U postgres GrcMvcDb > backup_$(Get-Date -Format "yyyyMMdd_HHmmss").sql

# Restore backup
docker exec -i shahin-grc-db-prod psql -U postgres GrcMvcDb < backup.sql
```

### View Container Stats
```powershell
docker stats shahin-grc-production
```

---

## ✅ Success Criteria

### Deployment Successful When:
- ✅ All containers running and healthy
- ✅ Health endpoint returns 200 OK
- ✅ Database migrations applied
- ✅ Seed data initialized
- ✅ Can create tenant via Trial signup
- ✅ Can create tenant via Platform Admin
- ✅ Can create tenant via API
- ✅ Onboarding redirect works for first admin
- ✅ Permissions and roles seeded correctly
- ✅ Marketing site loads successfully

---

## 📞 Support

### Build Issues
- Check `PRODUCTION_BUILD_SUMMARY.md` for build details
- Review `DEPLOYMENT_ISSUES_AND_FIXES.md` for known issues

### Runtime Issues
- Check application logs: `.\scripts\deploy-production.ps1 logs`
- Review health checks: `.\scripts\deploy-production.ps1 status`
- Verify environment variables in `.env.production`

### Database Issues
- Check database logs: `docker-compose logs db-prod`
- Verify connection string in `.env.production`
- Test connection: `docker exec shahin-grc-db-prod psql -U postgres -d GrcMvcDb`

---

**Ready for Production Deployment!** 🚀
