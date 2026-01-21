# 🚀 Phase 1 Setup Guide - Critical Infrastructure

**Estimated Time:** 1 hour  
**Priority:** P0 - Critical  
**Goal:** Achieve 98% completion and production readiness

---

## 📋 Prerequisites

- Docker and Docker Compose installed
- .NET 8.0 SDK installed
- Access to the server/machine
- Admin/sudo privileges

---

## ⚡ Quick Start (25 Minutes)

### Step 1: Generate SSL Certificate (5 minutes)

```bash
# Make script executable
chmod +x scripts/generate-ssl-certificate.sh

# Run the script
./scripts/generate-ssl-certificate.sh
```

**Expected Output:**
```
🔐 Generating SSL Certificate for GrcMvc...
📝 Generating certificate...
🔒 Trusting certificate...
✅ Certificate generated successfully!
📁 Location: src/GrcMvc/certificates/aspnetapp.pfx
🔑 Password: SecurePassword123!
```

**Verification:**
```bash
ls -lh src/GrcMvc/certificates/aspnetapp.pfx
```

---

### Step 2: Configure Environment Variables (10 minutes)

```bash
# Copy template to production file
cp .env.grcmvc.production.template .env.grcmvc.production

# Generate strong passwords
echo "DB_PASSWORD: $(openssl rand -base64 32)"
echo "ADMIN_PASSWORD: $(openssl rand -base64 32)"
echo "JWT_SECRET: $(openssl rand -base64 48)"

# Edit the file
nano .env.grcmvc.production
```

**Required Changes:**
1. Replace `<GENERATE_STRONG_PASSWORD>` for DB_PASSWORD
2. Replace `<GENERATE_STRONG_PASSWORD>` for ADMIN_PASSWORD
3. Replace `<GENERATE_STRONG_SECRET_MIN_32_CHARS>` for JWT_SECRET
4. Add SMTP credentials (EMAIL_USERNAME, EMAIL_PASSWORD)
5. Verify CERT_PASSWORD matches the generated certificate

**Minimum Required Variables:**
```bash
DB_PASSWORD=<your-generated-password>
ADMIN_PASSWORD=<your-generated-password>
JWT_SECRET=<your-generated-secret>
CERT_PASSWORD=SecurePassword123!
EMAIL_USERNAME=<your-smtp-username>
EMAIL_PASSWORD=<your-smtp-password>
```

---

### Step 3: Rebuild Docker Containers (5 minutes)

```bash
# Stop existing containers
docker-compose -f docker-compose.grcmvc.yml down

# Build with no cache
docker-compose -f docker-compose.grcmvc.yml build --no-cache

# Start containers
docker-compose -f docker-compose.grcmvc.yml up -d

# Wait for startup (30 seconds)
sleep 30
```

**Verification:**
```bash
# Check container status
docker-compose -f docker-compose.grcmvc.yml ps

# Check logs
docker logs grcmvc-app --tail 50

# Expected: No errors, "Application started" message
```

---

### Step 4: Verify HTTPS and Health (5 minutes)

```bash
# Test HTTP health endpoint
curl http://localhost:5137/health

# Test HTTPS health endpoint
curl -k https://localhost:5138/health

# Test detailed health
curl -k https://localhost:5138/health/ready

# Test database connection
curl -k https://localhost:5138/health/db
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123",
  "entries": {
    "database": {
      "status": "Healthy"
    }
  }
}
```

---

## 🗄️ Phase 2: Database Backups (4 hours)

### Step 1: Setup Backup Scripts (30 minutes)

```bash
# Make scripts executable
chmod +x scripts/backup-database.sh
chmod +x scripts/restore-database.sh

# Create backup directory
sudo mkdir -p /backups/postgresql
sudo chown $USER:$USER /backups/postgresql

# Test backup
./scripts/backup-database.sh
```

**Expected Output:**
```
🗄️  Starting PostgreSQL Backup...
Database: GrcMvcDb
Timestamp: 20260120_143022
📦 Creating backup...
✅ Backup completed successfully!
File: /backups/postgresql/grcmvc_20260120_143022.sql.gz
Size: 2.3M
```

---

### Step 2: Configure Automated Backups (30 minutes)

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /path/to/Shahin-Jan-2026/scripts/backup-database.sh >> /var/log/grc-backup.log 2>&1

# Add weekly cleanup
0 3 * * 0 find /backups/postgresql -name "grcmvc_*.sql.gz" -mtime +30 -delete
```

**Verification:**
```bash
# List cron jobs
crontab -l

# Test cron job manually
/path/to/Shahin-Jan-2026/scripts/backup-database.sh
```

---

### Step 3: Test Restore Process (30 minutes)

```bash
# List available backups
ls -lh /backups/postgresql/

# Test restore (WARNING: This will replace the database!)
./scripts/restore-database.sh /backups/postgresql/grcmvc_20260120_143022.sql.gz

# Verify restore
docker exec grcmvc-db psql -U postgres -d GrcMvcDb -c "\dt"
```

---

### Step 4: Document Backup Procedures (2.5 hours)

Create comprehensive documentation:
- Backup schedule
- Restore procedures
- Disaster recovery plan
- Testing procedures
- Monitoring and alerts

---

## ✅ Verification Checklist

### SSL Certificate
- [ ] Certificate file exists at `src/GrcMvc/certificates/aspnetapp.pfx`
- [ ] Certificate password is set in `.env.grcmvc.production`
- [ ] HTTPS endpoint responds: `curl -k https://localhost:5138/health`
- [ ] No SSL errors in browser (if testing locally)

### Environment Variables
- [ ] `.env.grcmvc.production` file exists
- [ ] All `<PLACEHOLDER>` values replaced
- [ ] DB_PASSWORD is strong (32+ characters)
- [ ] ADMIN_PASSWORD is strong (32+ characters)
- [ ] JWT_SECRET is strong (48+ characters)
- [ ] SMTP credentials configured
- [ ] File permissions set to 600: `chmod 600 .env.grcmvc.production`

### Docker Containers
- [ ] Containers are running: `docker-compose ps`
- [ ] No errors in logs: `docker logs grcmvc-app`
- [ ] Database is accessible: `docker exec grcmvc-db psql -U postgres -l`
- [ ] Application responds to HTTP: `curl http://localhost:5137/health`
- [ ] Application responds to HTTPS: `curl -k https://localhost:5138/health`

### Database Backups
- [ ] Backup script is executable
- [ ] Restore script is executable
- [ ] Backup directory exists and is writable
- [ ] Manual backup works: `./scripts/backup-database.sh`
- [ ] Backup file created in `/backups/postgresql/`
- [ ] Cron job configured for automated backups
- [ ] Restore tested successfully
- [ ] Backup retention policy working (30 days)

### Health Checks
- [ ] `/health` endpoint returns "Healthy"
- [ ] `/health/ready` endpoint returns detailed status
- [ ] Database health check passes
- [ ] All services show as healthy

---

## 🎯 Success Criteria

After completing Phase 1, you should have:

1. ✅ **HTTPS Working**
   - Valid SSL certificate installed
   - HTTPS endpoint accessible
   - No certificate warnings

2. ✅ **Secure Configuration**
   - All secrets properly configured
   - Strong passwords generated
   - Environment variables secured

3. ✅ **Automated Backups**
   - Daily backups scheduled
   - Retention policy active
   - Restore process tested

4. ✅ **System Health**
   - All containers running
   - No errors in logs
   - Health checks passing

5. ✅ **Production Ready**
   - System at 98% completion
   - Ready for production deployment
   - Disaster recovery plan in place

---

## 🚨 Troubleshooting

### Issue: Certificate Generation Fails

```bash
# Clean existing certificates
dotnet dev-certs https --clean

# Regenerate
./scripts/generate-ssl-certificate.sh
```

### Issue: Container Won't Start

```bash
# Check logs
docker logs grcmvc-app

# Check environment variables
docker exec grcmvc-app env | grep -E "DB_|CERT_|JWT_"

# Rebuild
docker-compose -f docker-compose.grcmvc.yml build --no-cache
docker-compose -f docker-compose.grcmvc.yml up -d
```

### Issue: Database Connection Fails

```bash
# Check database container
docker logs grcmvc-db

# Test connection
docker exec grcmvc-db psql -U postgres -c "SELECT version();"

# Verify credentials in .env file
grep DB_ .env.grcmvc.production
```

### Issue: Backup Fails

```bash
# Check container is running
docker ps | grep grcmvc-db

# Check permissions
ls -ld /backups/postgresql

# Test manually
docker exec grcmvc-db pg_dump -U postgres GrcMvcDb > test.sql
```

---

## 📊 Progress Tracking

| Task | Status | Time | Notes |
|------|--------|------|-------|
| SSL Certificate | ⏳ | 5 min | |
| Environment Variables | ⏳ | 10 min | |
| Rebuild Containers | ⏳ | 5 min | |
| Verify HTTPS | ⏳ | 5 min | |
| Setup Backups | ⏳ | 30 min | |
| Configure Cron | ⏳ | 30 min | |
| Test Restore | ⏳ | 30 min | |
| Documentation | ⏳ | 2.5 hrs | |
| **TOTAL** | **0%** | **5 hrs** | |

---

## 🎉 Next Steps

After completing Phase 1:

1. **Verify Production Readiness**
   - Run security audit
   - Test all endpoints
   - Verify RBAC enforcement

2. **Optional: Phase 2 (Agent Services)**
   - Implement AI agents
   - Add automation features
   - Enhance user experience

3. **Optional: Phase 3 (Test Coverage)**
   - Add unit tests
   - Add integration tests
   - Achieve 30%+ coverage

---

**Guide Version:** 1.0  
**Last Updated:** 2026-01-20  
**Contact:** Info@doganconsult.com
