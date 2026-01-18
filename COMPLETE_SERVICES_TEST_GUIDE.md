# Complete Services Test Guide

**Date**: January 15, 2026  
**Application**: Running on http://localhost:5000  
**Status**: All services configured and ready for testing

---

## ✅ Environment Variables Configured

### Critical Services
- ✅ **JWT_SECRET** - Authentication
- ✅ **Database Connections** - GrcMvcDb & GrcAuthDb
- ✅ **Claude AI** - API key configured
- ✅ **Microsoft Graph** - Email operations
- ✅ **Copilot Agent** - Microsoft Copilot Studio
- ✅ **SMTP Settings** - Email sending
- ✅ **Azure Tenant** - c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5

---

## 🧪 How to Test All Services

### Option 1: Automated Test Script

```powershell
# Run comprehensive test script
cd Shahin-Jan-2026
powershell -ExecutionPolicy Bypass -File scripts\test-all-services.ps1
```

This will test:
- Health endpoints
- Database connectivity
- Integration health (Claude, Copilot, Email)
- Public API endpoints
- Authentication endpoints
- API documentation

### Option 2: Manual Testing

#### 1. Health Checks
```powershell
# Basic health
Invoke-WebRequest -Uri "http://localhost:5000/health" -Method GET

# Readiness check
Invoke-WebRequest -Uri "http://localhost:5000/health/ready" -Method GET

# System test
Invoke-WebRequest -Uri "http://localhost:5000/api/system-test/health" -Method GET
```

#### 2. Database Tests
```powershell
# Database connection test
Invoke-WebRequest -Uri "http://localhost:5000/api/system-test/database" -Method GET

# Schema test
Invoke-WebRequest -Uri "http://localhost:5000/api/schema-test/tables" -Method GET
```

#### 3. Integration Health
```powershell
# All integrations
Invoke-WebRequest -Uri "http://localhost:5000/api/integration-health" -Method GET

# Claude AI
Invoke-WebRequest -Uri "http://localhost:5000/api/integration-health/claude" -Method GET

# Copilot
Invoke-WebRequest -Uri "http://localhost:5000/api/integration-health/copilot" -Method GET

# Email
Invoke-WebRequest -Uri "http://localhost:5000/api/integration-health/email" -Method GET
```

#### 4. Public API Endpoints
```powershell
# List controls
Invoke-WebRequest -Uri "http://localhost:5000/api/controls" -Method GET

# List risks
Invoke-WebRequest -Uri "http://localhost:5000/api/risks" -Method GET

# List evidence
Invoke-WebRequest -Uri "http://localhost:5000/api/evidence" -Method GET
```

#### 5. Swagger/API Docs
```powershell
# Open in browser
Start-Process "http://localhost:5000/api-docs"
Start-Process "http://localhost:5000/swagger/v1/swagger.json"
```

---

## 📋 Complete Service List

### Core GRC Services
1. **Control Service** - Control management
2. **Evidence Service** - Evidence collection
3. **Risk Service** - Risk assessment
4. **Plan Service** - Plan management
5. **Assessment Service** - Assessment management
6. **Audit Service** - Audit management
7. **Policy Service** - Policy management

### AI Services
8. **Claude Agent Service** - Claude AI integration
9. **Support Agent Service** - Customer support
10. **Diagnostic Agent Service** - System diagnostics
11. **Email AI Service** - Email classification

### Integration Services
12. **Email Service** - Email sending (SMTP/Microsoft Graph)
13. **Microsoft Graph Service** - Microsoft 365 integration
14. **Copilot Service** - Copilot Studio integration
15. **Workflow Service** - Workflow management

### User Services
16. **User Service** - User management
17. **Role Service** - Role management
18. **Permission Service** - Permission management
19. **Authentication Service** - Authentication

### Reporting Services
20. **Report Service** - Report generation
21. **Dashboard Service** - Dashboard metrics
22. **Analytics Service** - Analytics & insights

---

## 🔍 Testing Checklist

### Application Status
- [ ] Application running (PID: check with Get-Process)
- [ ] No errors in startup-errors.log
- [ ] Health endpoint responds
- [ ] Migration applied successfully

### Database
- [ ] All Identity tables exist (AspNetUsers, AspNetRoles, etc.)
- [ ] All 17 ApplicationUser columns present
- [ ] Database connection test passes

### Integrations
- [ ] Claude AI health check passes
- [ ] Copilot health check passes
- [ ] Email service health check passes
- [ ] Microsoft Graph configured

### API Endpoints
- [ ] Health endpoints work
- [ ] Public GET endpoints work
- [ ] Authentication endpoints work
- [ ] Swagger documentation accessible

### User Forms
- [ ] User creation form works
- [ ] User editing form works
- [ ] All ApplicationUser properties save/load

---

## 🚀 Quick Test Commands

### Check Application Status
```powershell
Get-Process -Name "dotnet" | Where-Object { $_.Path -like "*GrcMvc*" }
```

### View Logs
```powershell
Get-Content startup.log -Tail 30
Get-Content startup.log -Wait -Tail 20  # Real-time
```

### Test Health
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/health/ready"
```

### Open Swagger
```powershell
Start-Process "http://localhost:5000/api-docs"
```

---

## 📊 Expected Results

### Health Checks
- Status: 200 OK
- Response: `{"status":"Healthy"}`

### Integration Health
- Claude: Should show "Configured" or "Error - No API Key"
- Copilot: Should show configuration status
- Email: Should show SMTP/Graph status

### API Endpoints
- Public GET: 200 OK (may return empty arrays)
- Auth Required: 401 Unauthorized (expected)
- Health: 200 OK

---

## 🎯 Next Steps

1. **Run Test Script**: `scripts\test-all-services.ps1`
2. **Check Logs**: Monitor `startup.log` for any issues
3. **Test User Forms**: Create/edit users via web UI
4. **Verify Database**: Run SQL queries to confirm schema
5. **Test Integrations**: Use integration health endpoints

---

**All services are configured and ready for testing!**
