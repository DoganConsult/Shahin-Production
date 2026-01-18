# Complete Setup Guide - Shahin GRC Platform
**Date**: January 17, 2026

## Current Status ✅
- **Application**: Running on http://localhost:5000
- **Database**: Connected to Railway PostgreSQL
- **Issue**: Minor seeding error (FirstAdminUserId column) - NOT blocking functionality

## Quick Access URLs
- **Local Application**: http://localhost:5000
- **Owner Setup**: http://localhost:5000/OwnerSetup
- **Login Page**: http://localhost:5000/Account/Login

## Database Connection Details (Railway)
```
Server: caboose.proxy.rlwy.net
Port: 11527
Database: GrcMvcDb
Username: postgres
Password: QNcTvViWopMfCunsyIkkXwuDpufzhkLs
```

## Default Credentials
```
Admin Email: admin@grc.com
Admin Password: Admin@123456!

Demo Account: support@shahin-ai.com
Demo Password: Admin@123456!
```

## Setup Steps Completed ✅

### 1. Database Connection
- Railway PostgreSQL configured
- Connection string hardcoded in WebApplicationBuilderExtensions.cs
- Migrations applied successfully

### 2. Application Running
- Running on localhost:5000
- JWT authentication configured
- All services started (except Hangfire - non-critical)

### 3. Cloudflare Tunnel Setup
**Run as Administrator:**
```cmd
cloudflared.exe service install eyJhIjoiNjZiNTFhYzk2OTkxMWQ0MzY0ZjQ4M2Q4ODdhNjZjMGYiLCJ0IjoiODNiMzA4NjAtNTdjNS00OThlLTkxYjItMDM5M2E0MjJhZjk4IiwicyI6IlltVmxNelZoWm1FdE1USm1NQzAwWldJd0xUaGpNRFl0Tmprd01UZGhPRGc1T1RFMSJ9

cloudflared.exe tunnel run --url http://localhost:5000
```

## Next Steps

### 1. Access the Application
Open browser: http://localhost:5000

### 2. Complete Owner Setup (if needed)
Navigate to: http://localhost:5000/OwnerSetup

### 3. Login
Use one of the default credentials above

### 4. For Public Access (Cloudflare)
1. Run setup-cloudflare-tunnel.bat as Administrator
2. Get your public URL from Cloudflare output
3. Share the public URL for external access

## Troubleshooting

### If app stops running:
```cmd
dotnet run --project src/GrcMvc/GrcMvc.csproj --urls "http://localhost:5000"
```

### If database connection fails:
Check .env.local file has correct Railway credentials

### If Cloudflare tunnel fails:
Must run as Administrator (elevated privileges required)

## Application Features Available

✅ User Authentication & Management
✅ Dashboard
✅ Risk Management
✅ Control Management
✅ Assessment Management
✅ Compliance Tracking
✅ Audit Management
✅ Policy Management
✅ Vendor Management
✅ Workflow Engine
✅ Reporting

## API Documentation
Once running, access Swagger at:
http://localhost:5000/swagger

## Support
- Documentation: /docs
- Health Check: http://localhost:5000/health
- Metrics: http://localhost:5000/metrics

---
**Status**: READY FOR USE
**Database**: Connected to Railway
**Application**: Running on localhost:5000
