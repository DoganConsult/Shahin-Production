# 🚀 QUICK START - BUILD & RUN

## Linux/Mac - One Command

```bash
cd /home/dogan/grc-system
chmod +x run.sh
./run.sh
```

## Windows - One Command

```cmd
cd C:\path\to\grc-system
run.bat
```

## Manual Steps

### 1. Clean & Restore
```bash
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet restore src/GrcMvc/GrcMvc.csproj
```

### 2. Build
```bash
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

### 3. Migrate Database
```bash
cd src/GrcMvc
dotnet ef database update --context GrcDbContext
```

### 4. Run
```bash
dotnet run
```

---

## 🎯 What Happens

```
✅ Clean old artifacts
✅ Restore NuGet packages
✅ Build project
✅ Apply database migrations (11 Phase 1 tables)
✅ Start application
```

---

## 📍 Application URLs

| URL | Purpose |
|-----|---------|
| https://localhost:5001 | Main application |
| https://localhost:5001/health | Health check |
| https://localhost:5001/health/ready | Database readiness |
| https://localhost:5001/health/live | Liveness probe |

---

## 🔐 Default Credentials

**Email**: Info@doganconsult.com
**Password**: AhmEma$123456

⚠️ **CHANGE IN PRODUCTION!**

---

## 📊 Expected Output

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop
```

✅ Application is running!

---

## 🐛 Troubleshooting

### "Build failed"
```bash
dotnet clean
dotnet restore
dotnet build -c Release
```

### "Database connection failed"
```bash
# Set connection string
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_db;Username=postgres;Password=yourpassword"
```

### "Port 5001 already in use"
```bash
# Use different port
dotnet run --urls "https://localhost:5002"
```

### "Dependencies not found"
```bash
# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

---

## 📝 What Gets Created

### Database Tables (11 tables)
- Framework, FrameworkVersion
- Control, ControlOwnership, ControlEvidence
- Baseline, BaselineControl
- HRISIntegration, HRISEmployee
- AuditLog, ComplianceSnapshot, ControlTestResult

### Services (42 methods)
- FrameworkService (18 methods)
- HRISService (12 methods)
- AuditTrailService (8 methods)
- RulesEngineService (4 methods)

### Logs
- `/app/logs/grcmvc-YYYY-MM-DD.log`
- `/app/logs/grcmvc-errors-YYYY-MM-DD.log`

---

## ⏱️ Timeline

| Step | Time |
|------|------|
| Clean | ~5s |
| Restore | ~15s |
| Build | ~30s |
| Migration | ~5s |
| Start | ~3s |
| **Total** | **~60 seconds** |

---

## ✅ Success Criteria

- [x] Build completes without errors
- [x] Database migration applies
- [x] Application starts
- [x] Can access https://localhost:5001
- [x] Health check passes
- [x] Logs show no errors

---

## 🎉 You're Ready!

**Run**: `./run.sh` (Linux/Mac) or `run.bat` (Windows)

**Then**: Open https://localhost:5001 in your browser

**Done!** ✅

---

**Need help?** See BUILD_AND_RUN_GUIDE.md or 00_EXECUTIVE_SUMMARY.md
