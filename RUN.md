# ▶️ RUN - GRC MANAGEMENT SYSTEM

## ⚡ FASTEST PATH TO RUNNING THE APPLICATION

### 1️⃣ PREREQUISITES (first time only)
```bash
# Ensure you have:
# - .NET 6+ or 7+ installed
# - PostgreSQL 12+ running
# - Git installed

# Verify installations:
dotnet --version
psql --version
git --version
```

### 2️⃣ CLONE/NAVIGATE (first time only)
```bash
cd /home/dogan/grc-system
```

### 3️⃣ BUILD
```bash
# One-command build (clean + restore + build)
dotnet clean && dotnet build -c Release
```

### 4️⃣ DATABASE SETUP (first time only)
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database (in psql terminal):
CREATE DATABASE grc_system;
\q

# Apply all migrations (from /src/GrcMvc)
cd src/GrcMvc
dotnet ef database update --context GrcDbContext

# This applies:
# - Phase 1 tables (Framework, HRIS, Audit, Rules)
# - Phase 2 tables (Workflows)
# - Phase 3 tables (RBAC)
```

### 5️⃣ RUN APPLICATION
```bash
# From src/GrcMvc directory
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to stop
```

### 6️⃣ OPEN IN BROWSER
```
URL: https://localhost:5001
Email: Info@doganconsult.com
Password: AhmEma$123456
```

---

## 🎯 THAT'S IT!

### First Time: ~5 minutes
1. Build (2 min)
2. Setup database (1 min)
3. Run (30 sec)
4. Access (30 sec)

### Subsequent Times: ~30 seconds
```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet run
```

---

## 📋 FULL STEP-BY-STEP (Copy & Paste)

```bash
# ====== SETUP (First Time Only) ======

# 1. Navigate to project
cd /home/dogan/grc-system

# 2. Clean and build
dotnet clean
dotnet build -c Release

# 3. Create PostgreSQL database (if not exists)
psql -U postgres -c "CREATE DATABASE grc_system;"

# 4. Apply migrations
cd src/GrcMvc
dotnet ef database update --context GrcDbContext

# 5. Run application
dotnet run

# ====== RUNNING AFTERWARDS ======
# Just:
# cd /home/dogan/grc-system/src/GrcMvc
# dotnet run
```

---

## 🐛 QUICK TROUBLESHOOTING

### Issue: "Connection refused" to PostgreSQL
```bash
# Start PostgreSQL
sudo systemctl start postgresql

# Verify connection
psql -U postgres -c "SELECT 1;"
```

### Issue: "Database does not exist"
```bash
# Create it
psql -U postgres -c "CREATE DATABASE grc_system;"
```

### Issue: Migration errors
```bash
# Reset (WARNING: deletes all data)
dotnet ef database drop --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

### Issue: Port 5001 already in use
```bash
# Use different port
dotnet run --urls "https://localhost:5002"
```

### Issue: Can't login
```bash
# Reset admin password
dotnet run -- --reset-admin-password
```

---

## 🔐 LOGIN CREDENTIALS (Default)

```
Email: Info@doganconsult.com
Password: AhmEma$123456
```

**⚠️ Change these in production!**

---

## 📍 APPLICATION URL

```
Local: https://localhost:5001
```

**Note**: HTTPS with self-signed certificate (ignore browser warning in development)

---

## 📊 WHAT'S RUNNING

### Phase 1: Foundation ✅
- Framework management
- HRIS integration
- Audit trail logging
- Rules engine

### Phase 2: 10 Workflows ✅
- Control Implementation
- Risk Assessment
- Approval/Sign-off
- Evidence Collection
- Compliance Testing
- Remediation
- Policy Review
- Training Assignment
- Audit
- Exception Handling

### Phase 3: RBAC ✅
- 40+ Permissions
- 12 Features
- 5 Roles
- Multi-tenant access control

### Database: 23 Tables
- 11 Phase 1
- 5 Phase 2
- 7 Phase 3

### Services: 20 Services (170+ Methods)
- 4 Phase 1 (42 methods)
- 10 Phase 2 (94 methods)
- 6 Phase 3 (50+ methods)

---

## ⌨️ KEYBOARD SHORTCUTS

| Shortcut | Action |
|----------|--------|
| Ctrl+C | Stop application |
| Ctrl+L | Clear terminal |
| F5 (browser) | Refresh page |
| F12 (browser) | Developer tools |

---

## 📱 BROWSER COMPATIBILITY

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+

---

## 🔄 DEVELOPMENT WORKFLOW

```bash
# 1. Make code changes
# 2. Stop app (Ctrl+C)
# 3. Rebuild
dotnet build
# 4. Run again
dotnet run
```

Or use:
```bash
# Auto-rebuild on file changes
dotnet watch run
```

---

## 📊 SYSTEM RESOURCES

### Minimum Requirements
- RAM: 512 MB
- CPU: 1 core
- Disk: 1 GB

### Recommended
- RAM: 2 GB
- CPU: 2+ cores
- Disk: 5 GB

---

## 🚀 PRODUCTION DEPLOYMENT

For production:
1. Use `dotnet publish -c Release`
2. Deploy to Azure App Service, AWS, Docker, etc.
3. Configure HTTPS with real certificates
4. Update connection strings
5. Set environment variables
6. Enable monitoring/logging

See `DEPLOYMENT_GUIDE.md` for detailed instructions.

---

## 📖 NEED MORE INFO?

- **Getting Started**: See `DEPLOYMENT_GUIDE.md`
- **System Overview**: See `FINAL_STATUS_REPORT.md`
- **Full Documentation**: See `INDEX.md`
- **Architecture**: See `SYSTEM_ARCHITECTURE.md`
- **Workflows**: See `PHASE_2_WORKFLOWS_COMPLETE.md`
- **RBAC System**: See `RBAC_IMPLEMENTATION_GUIDE.md`
- **Quick Reference**: See `QUICK_REFERENCE.md`

---

## ✅ VERIFY APPLICATION IS RUNNING

1. **Check in browser**:
   - Go to https://localhost:5001
   - Should see login page
   - If SSL warning, click "Advanced" → "Continue"

2. **Check in terminal**:
   - Should see: "Now listening on: https://localhost:5001"
   - Should see: "Application started"

3. **Try logging in**:
   - Email: Info@doganconsult.com
   - Password: AhmEma$123456
   - Should redirect to dashboard

4. **Check permissions**:
   - Go to Workflows menu
   - Should see workflow list
   - Permissions enforced per role

---

## 🎉 YOU'RE RUNNING!

The complete GRC Management System is now running with:
- ✅ 3 phases (Foundation + Workflows + RBAC)
- ✅ 23 database tables
- ✅ 20 services
- ✅ 10 workflow types
- ✅ 40+ permissions
- ✅ 12 features

**Ready to test, customize, or deploy to production!**

---

## 💡 TIPS

1. **Development**: Use `dotnet watch run` for auto-reload
2. **Debugging**: Set breakpoints in Visual Studio
3. **Testing**: Create test workflows in the UI
4. **Logging**: Check `/app/logs/` for application logs
5. **Database**: Use `psql` to query database directly

---

**All set!** Application running at https://localhost:5001 🚀
