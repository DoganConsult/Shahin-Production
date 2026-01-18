# 🚀 Railway Production Setup Guide

## ✅ Code Successfully Pushed to Production Repository

**Repository:** https://github.com/doganlap/shahin-ai-producion.git  
**Branch:** develop  
**Status:** ✅ All code including 321-table migration pushed successfully

---

## 📋 Quick Setup Steps (15 minutes total)

### Step 1: Add Application Service to Railway (5 minutes)

1. **Go to Railway Dashboard**
   - Open: https://railway.app
   - Select project: "Shahin-ai.com"

2. **Click "+ New Service"**
   - Select: "GitHub Repo"

3. **Connect New Production Repository**
   - Repository: `doganlap/shahin-ai-producion`
   - Branch: `develop`
   - Root Directory: `Shahin-Jan-2026/src/GrcMvc`

4. **Railway Auto-Detection**
   - Railway will detect it's a .NET 8 application
   - Build command: `dotnet publish -c Release -o /app/publish`
   - Start command: `dotnet /app/publish/GrcMvc.dll`

---

### Step 2: Configure Environment Variables (3 minutes)

Add these variables to your **new application service**:

#### Required Variables (Must Have)
```bash
DATABASE_URL = ${{ Postgres.DATABASE_URL }}
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:5000
JWT_SECRET = etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx
```

#### Recommended Variables
```bash
JwtSettings__Issuer = https://portal.shahin-ai.com
JwtSettings__Audience = https://portal.shahin-ai.com
Redis__ConnectionString = ${{ Redis.REDIS_URL }}
Redis__Enabled = true
```

#### How to Add Variables in Railway Dashboard
1. Select your new application service
2. Go to "Variables" tab
3. Click "+ New Variable"
4. Add each variable name and value
5. Click "Add"

---

### Step 3: Deploy! (7 minutes)

1. **Click "Deploy"** in Railway Dashboard
2. **Watch the Build Process**
   - Railway clones your repository
   - Restores NuGet packages
   - Compiles the application
   - Creates deployment package

3. **Monitor Deployment Logs**
   - Click on the deployment
   - Watch logs in real-time
   - Look for migration success messages

---

## 📊 What Will Happen During Deployment

### Build Phase (3-5 minutes)
```
✅ Clone repository: doganlap/shahin-ai-producion
✅ Checkout branch: develop
✅ Navigate to: Shahin-Jan-2026/src/GrcMvc
✅ Restore NuGet packages
✅ Compile C# code
✅ Build migration files (321 tables)
✅ Create deployment package
```

### Deploy Phase (2-3 minutes)
```
✅ Deploy to Railway infrastructure
✅ Start application
✅ Connect to PostgreSQL database
✅ Run auto-migration: ApplyDatabaseMigrationsAsync()
✅ Create all 321 tables
✅ Record migration history
✅ Application ready!
```

---

## ✅ Expected Logs (Success Indicators)

### During Startup
```
[CONFIG] ========================================
[CONFIG] Resolving Connection Strings
[CONFIG] ========================================
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
[CONFIG] ✅ Using database connection from: Environment Variable (Railway)
[CONFIG] 📊 Database: postgres.railway.internal:5432 / postgres@railway
```

### During Migration
```
[DB] 🔄 Applying database migrations...
[DB] 📦 Found pending migration: 20260118105126_InitialCreate
[DB] 🔄 Creating 321 tables...
[DB] ✅ Main database migrations applied successfully
[DB] ✅ Auth database migrations applied successfully
```

### Application Ready
```
✅ Application started successfully
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🔍 Verification Steps

### Step 1: Check Application Logs
```bash
# In Railway Dashboard
1. Go to your application service
2. Click "Deployments"
3. Click on the latest deployment
4. View logs
```

Look for:
- ✅ `[DB] ✅ Main database migrations applied successfully`
- ✅ `Application started successfully`

### Step 2: Verify Tables Created
```bash
# SSH into Railway
railway ssh

# Count tables
psql $DATABASE_URL -c "SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema = 'public';"

# Expected output: 321+ tables
```

### Step 3: Check Migration History
```bash
# Inside Railway SSH
psql $DATABASE_URL -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"

# Expected: 20260118105126_InitialCreate
```

### Step 4: List Sample Tables
```bash
psql $DATABASE_URL -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name LIMIT 20;"

# Expected tables:
# - Tenants
# - Risks
# - Controls
# - Assessments
# - Policies
# - Workflows
# - Evidence
# - etc.
```

---

## 🎯 Railway Service Configuration

### Service Settings
```
Name: GRC-Portal (or your preferred name)
Source: GitHub - doganlap/shahin-ai-producion
Branch: develop
Root Directory: Shahin-Jan-2026/src/GrcMvc
```

### Build Settings
```
Builder: Nixpacks (auto-detected)
Build Command: dotnet publish -c Release -o /app/publish
```

### Deploy Settings
```
Start Command: dotnet /app/publish/GrcMvc.dll
Port: 5000
Health Check: /health (if you have health endpoint)
```

### Resource Settings
```
Memory: 512 MB (minimum)
CPU: Shared
Region: us-west1 (or your preferred region)
```

---

## 💰 Railway Pricing

### Hobby Plan ($5/month)
```
✅ PostgreSQL database (5GB storage)
✅ Redis cache (5GB storage)
✅ Application hosting
✅ 500 execution hours/month
✅ Automatic backups
✅ SSL certificates
✅ Custom domains
```

**Your Usage:**
- PostgreSQL: ~200 MB (after migration)
- Redis: ~10 MB
- Application: ~100 MB
- **Total: ~310 MB / 5GB available**

---

## 🚨 Troubleshooting

### If Build Fails

**Check Build Logs:**
1. Go to deployment in Railway
2. Click "Build Logs"
3. Look for error messages

**Common Issues:**
- Missing NuGet packages → Check .csproj file
- Compilation errors → Check code syntax
- Missing files → Check repository structure

**Solution:**
```bash
# Test build locally first
cd Shahin-Jan-2026/src/GrcMvc
dotnet build -c Release
```

### If Migration Fails

**Check Application Logs:**
Look for error messages like:
```
[DB] ❌ Migration failed: [error message]
```

**Common Issues:**
- Database connection failed → Check DATABASE_URL
- Migration timeout → Increase Railway timeout
- Duplicate tables → Database not empty

**Solution:**
```bash
# SSH into Railway
railway ssh

# Check database connection
psql $DATABASE_URL -c "SELECT version();"

# Check existing tables
psql $DATABASE_URL -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';"
```

### If Application Won't Start

**Check Logs for:**
- Port binding issues
- Missing environment variables
- Configuration errors

**Solution:**
1. Verify all environment variables are set
2. Check ASPNETCORE_URLS = http://0.0.0.0:5000
3. Verify DATABASE_URL is set correctly

---

## 📝 Post-Deployment Checklist

- [ ] Application service created in Railway
- [ ] GitHub repository connected (shahin-ai-producion)
- [ ] Environment variables configured
- [ ] Deployment successful
- [ ] Build logs show success
- [ ] Application logs show migration success
- [ ] 321 tables created in database
- [ ] Migration history recorded
- [ ] Application accessible via Railway URL
- [ ] Health check passing (if configured)

---

## 🎉 Success Criteria

### All Green When:
1. ✅ Build completes without errors
2. ✅ Deployment shows "SUCCESS" status
3. ✅ Logs show: `[DB] ✅ Main database migrations applied successfully`
4. ✅ Logs show: `Application started successfully`
5. ✅ Database has 321+ tables
6. ✅ Migration history shows InitialCreate
7. ✅ Application responds to requests

---

## 🚀 Next Steps After Successful Deployment

### 1. Set Up Custom Domain (Optional)
```
Railway Dashboard → Service → Settings → Domains
Add: portal.shahin-ai.com
```

### 2. Configure Auto-Deploy
```
✅ Already configured via GitHub integration
Every push to 'develop' branch will auto-deploy
```

### 3. Monitor Application
```
Railway Dashboard → Service → Metrics
- CPU usage
- Memory usage
- Request count
- Response times
```

### 4. Set Up Alerts (Optional)
```
Railway Dashboard → Service → Settings → Notifications
- Deployment failures
- High resource usage
- Application crashes
```

---

## 📊 Timeline Summary

| Phase | Duration | Status |
|-------|----------|--------|
| Code Push to Production Repo | Complete | ✅ Done |
| Add Application Service | 5 minutes | ⏳ Next |
| Configure Variables | 3 minutes | ⏳ Next |
| Deploy & Build | 7 minutes | ⏳ Next |
| **Total** | **15 minutes** | **Ready to Start** |

---

## 🎯 Current Status

### ✅ Completed
- [x] Migration files created (321 tables)
- [x] Code fixes applied
- [x] Committed to git
- [x] Pushed to production repository
- [x] Railway database ready
- [x] Railway Redis ready
- [x] Environment variables prepared

### ⏳ Next Actions
- [ ] Add application service to Railway
- [ ] Configure environment variables
- [ ] Deploy application
- [ ] Verify migration success
- [ ] Test application

---

**Ready to deploy!** Just follow the 3 steps above and your application with all 321 tables will be live on Railway in 15 minutes! 🚀
