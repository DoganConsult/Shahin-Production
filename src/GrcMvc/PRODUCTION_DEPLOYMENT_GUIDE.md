# Production Deployment Guide for GRC MVC Application

## 🔒 Security Configuration Requirements

### Critical: Environment Variables for Production

**NEVER commit secrets to source control!** All sensitive configuration must be provided via environment variables or secure vaults.

## Required Environment Variables

### 1. Database Connection (REQUIRED)
```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=your-prod-server;Database=GrcProdDb;User Id=grc_app_user;Password=STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"

# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=your-prod-server;Database=GrcProdDb;User Id=grc_app_user;Password=STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"

# Docker/Kubernetes
-e ConnectionStrings__DefaultConnection="..."
```

### 2. JWT Authentication (REQUIRED)
```bash
# Generate secure secret (Linux/macOS):
openssl rand -base64 32

# Set environment variables:
export JwtSettings__Secret="YOUR_GENERATED_32+_CHARACTER_SECRET_HERE"
export JwtSettings__Issuer="https://your-domain.com"
export JwtSettings__Audience="https://your-domain.com"
export JwtSettings__ExpirationInMinutes="60"
```

### 3. Host Filtering (REQUIRED for Production)
```bash
export AllowedHosts="your-domain.com;www.your-domain.com"
```

### 4. Database Seeding
The application automatically seeds:
- **Roles**: Admin, ComplianceOfficer, RiskManager, Auditor, User
- **Admin User**:
  - Email: `Info@doganconsult.com`
  - Password: `AhmEma$123456` (Change immediately!)
export ApplicationSettings__MaxFileUploadSize="10485760"
```

## Pre-Deployment Checklist

### ✅ Configuration Validation
- [ ] All environment variables are set
- [ ] Connection string uses dedicated SQL user (NOT sa)
- [ ] Connection string has `Encrypt=True;TrustServerCertificate=False`
- [ ] JWT secret is unique and >= 32 characters
- [ ] JWT secret is different from development
- [ ] AllowedHosts is restricted to your domain(s)

### ✅ Database Preparation
```bash
# 1. Create production database
sqlcmd -S your-prod-server -U sa -P admin_password -Q "CREATE DATABASE GrcProdDb"

# 2. Create dedicated application user
sqlcmd -S your-prod-server -U sa -P admin_password -Q "
CREATE LOGIN grc_app_user WITH PASSWORD = 'STRONG_PASSWORD_HERE';
USE GrcProdDb;
CREATE USER grc_app_user FOR LOGIN grc_app_user;
ALTER ROLE db_owner ADD MEMBER grc_app_user;
"

# 3. Run migrations
cd src/GrcMvc
dotnet ef database update
```

### ✅ Security Hardening
- [ ] Remove or secure default admin account
- [ ] Enable HTTPS only
- [ ] Configure firewall rules
- [ ] Enable SQL Server encryption
- [ ] Set up backup strategy
- [ ] Configure monitoring/alerting
- [ ] Review and rotate all secrets

## Deployment Methods

### Option 1: IIS Deployment

#### Prerequisites
- IIS with ASP.NET Core Hosting Bundle installed
- .NET 8 Runtime

#### Steps
```powershell
# 1. Publish application
cd src/GrcMvc
dotnet publish -c Release -o C:\inetpub\GrcMvc

# 2. Create IIS application pool
# - .NET CLR Version: No Managed Code
# - Pipeline Mode: Integrated
# - Identity: ApplicationPoolIdentity

# 3. Set environment variables in IIS
# In web.config or Application Pool > Advanced Settings > Environment Variables
```

**web.config example:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <aspNetCore processPath="dotnet"
                arguments=".\GrcMvc.dll"
                stdoutLogEnabled="true"
                stdoutLogFile=".\logs\stdout">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        <environmentVariable name="ConnectionStrings__DefaultConnection" value="YOUR_PROD_CONNECTION" />
        <environmentVariable name="JwtSettings__Secret" value="YOUR_PROD_SECRET" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

### Option 2: Docker Deployment

#### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GrcMvc/GrcMvc.csproj", "GrcMvc/"]
RUN dotnet restore "GrcMvc/GrcMvc.csproj"
COPY src/GrcMvc/ GrcMvc/
WORKDIR "/src/GrcMvc"
RUN dotnet build "GrcMvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrcMvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrcMvc.dll"]
```

#### Docker Run Command
```bash
docker build -t grcmvc:latest .
docker run -d \
  -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="..." \
  -e JwtSettings__Secret="..." \
  -e JwtSettings__Issuer="..." \
  -e JwtSettings__Audience="..." \
  --name grcmvc \
  grcmvc:latest
```

### Option 3: Azure App Service

```bash
# 1. Create App Service
az webapp create --resource-group myRG --plan myPlan --name myGrcApp --runtime "DOTNET|8.0"

# 2. Set environment variables
az webapp config appsettings set --resource-group myRG --name myGrcApp --settings \
  ConnectionStrings__DefaultConnection="..." \
  JwtSettings__Secret="..." \
  JwtSettings__Issuer="https://myGrcApp.azurewebsites.net" \
  JwtSettings__Audience="https://myGrcApp.azurewebsites.net"

# 3. Deploy
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r deploy.zip .
az webapp deployment source config-zip --resource-group myRG --name myGrcApp --src deploy.zip
```

## Post-Deployment Verification

### 1. Health Check
```bash
curl https://your-domain.com/health
# Expected: 200 OK
```

### 2. Database Connectivity
```bash
# Check application logs for successful DB connection
tail -f /var/log/grcmvc/app.log | grep "Database"
```

### 3. Authentication Test
```bash
# Test login endpoint
curl -X POST https://your-domain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@grcmvc.com","password":"Admin@123456"}'
```

### 4. File Upload Test (if applicable)
```bash
# Test file upload with size and extension validation
curl -X POST https://your-domain.com/api/upload \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@test.pdf"
```

## Troubleshooting

### Common Issues and Solutions

#### 1. Application won't start
**Check:** Application logs for startup errors
```bash
journalctl -u grcmvc -n 100  # Linux systemd
docker logs grcmvc            # Docker
```

**Common causes:**
- Missing environment variables (especially ConnectionStrings__DefaultConnection)
- Invalid JWT secret (< 32 chars)
- Database connection failure

#### 2. 500 Internal Server Error
**Check:** Detailed error logs
```bash
# Enable detailed errors temporarily
export ASPNETCORE_ENVIRONMENT=Development
# Restart app and check logs
```

#### 3. Authentication failures (401/403)
**Check:** JWT configuration
- Verify Secret matches between token generation and validation
- Check Issuer/Audience match
- Verify token expiration and clock skew

#### 4. Database connection timeouts
**Check:** Connection string and network
```bash
# Test connection from app server
sqlcmd -S your-server -U grc_app_user -P password -Q "SELECT 1"
```

## Monitoring Recommendations

### Essential Metrics
- Application health endpoint status
- Response time percentiles (p50, p95, p99)
- Error rate (4xx, 5xx)
- Database connection pool usage
- Memory and CPU usage
- File upload success/failure rate

### Recommended Tools
- Application Insights (Azure)
- Prometheus + Grafana
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Serilog with appropriate sinks

## Security Reminders

⚠️ **NEVER:**
- Use `sa` account in production
- Set `TrustServerCertificate=True` in production
- Commit secrets to source control
- Use default/weak passwords
- Expose detailed errors to end users

✅ **ALWAYS:**
- Rotate secrets regularly
- Use HTTPS everywhere
- Validate and sanitize all inputs
- Log security events
- Keep dependencies updated
- Perform security scans

## Rollback Plan

If deployment fails:
1. **Immediate:** Switch load balancer to previous version
2. **Database:** Have migration rollback scripts ready
3. **Configuration:** Keep previous environment variables documented
4. **Monitoring:** Alert on rollback triggers (error rate > 5%, response time > 2s)

## Contact for Issues

- **Application Issues:** Check logs first, then contact DevOps team
- **Database Issues:** DBA team
- **Security Concerns:** Security team immediately
- **Configuration:** Refer to this guide or contact DevOps

---

**Last Updated:** 2024
**Version:** 1.0
**Status:** READY FOR PRODUCTION