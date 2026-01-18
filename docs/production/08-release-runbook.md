# Release Runbook

**Generated:** 2026-01-17  
**Purpose:** Step-by-step procedures for deploying, verifying, and rolling back releases

---

## Pre-Deployment Checklist

### 1. Environment Verification

```bash
# Check disk space (need at least 5GB free)
df -h /
# Expected: > 5GB available

# Check memory
free -h
# Expected: > 1GB available

# Check running containers
docker compose -f infra/compose/docker-compose.prod.yml ps
# Expected: All services "Up" and "healthy"
```

### 2. Backup Verification

```bash
# Verify recent backup exists
ls -la /opt/shahin-ai/backups/
# Expected: Backup file from today or yesterday

# If no recent backup, create one now
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  pg_dump -U grc_admin -d GrcMvcDb -F c -f /tmp/pre-deploy-backup.dump

docker cp shahin-grc-db:/tmp/pre-deploy-backup.dump \
  /opt/shahin-ai/backups/pre-deploy-$(date +%Y%m%d-%H%M%S).dump
```

### 3. Environment File Check

```bash
# Verify .env.production exists and has required vars
cat .env.production | grep -E "^(DB_|JWT_|AZURE_)" | wc -l
# Expected: At least 5 lines

# Check for empty required values
grep -E "^[A-Z_]+=\s*$" .env.production
# Expected: No output (no empty required values)
```

### 4. Health Check Before Deploy

```bash
# API health
curl -sf https://portal.shahin-ai.com/health | jq -r '.status'
# Expected: "Healthy"

# Frontend health
curl -sf -o /dev/null -w "%{http_code}" https://www.shahin-ai.com/
# Expected: 200
```

---

## Deployment Steps

### Standard Deployment

```bash
# 1. Navigate to project directory
cd /opt/shahin-ai/Shahin-Jan-2026

# 2. Pull latest code
git fetch origin
git status
# Review changes before pulling
git pull origin main

# 3. Note current image tags (for rollback)
docker images | grep shahin > /tmp/current-images.txt
cat /tmp/current-images.txt

# 4. Build new images
docker compose -f infra/compose/docker-compose.prod.yml build --no-cache grc-api grc-web

# 5. Stop and restart services (with zero-downtime if possible)
docker compose -f infra/compose/docker-compose.prod.yml up -d --force-recreate grc-api grc-web

# 6. Wait for services to be healthy (up to 2 minutes)
echo "Waiting for services to be healthy..."
sleep 30
docker compose -f infra/compose/docker-compose.prod.yml ps

# 7. Apply database migrations (if any)
docker compose -f infra/compose/docker-compose.prod.yml exec grc-api \
  dotnet ef database update --context GrcDbContext

docker compose -f infra/compose/docker-compose.prod.yml exec grc-api \
  dotnet ef database update --context GrcAuthDbContext
```

### Database-Only Migration

```bash
# For migrations without code changes
cd /opt/shahin-ai/Shahin-Jan-2026

# 1. Backup first
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  pg_dump -U grc_admin -d GrcMvcDb -F c -f /tmp/pre-migration.dump

# 2. Apply migrations
docker compose -f infra/compose/docker-compose.prod.yml exec grc-api \
  dotnet ef database update --context GrcDbContext

# 3. Verify
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;"
```

### Hotfix Deployment

```bash
# For urgent fixes - faster process
cd /opt/shahin-ai/Shahin-Jan-2026

# 1. Pull specific commit/tag
git fetch origin
git checkout <commit-hash-or-tag>

# 2. Quick rebuild of affected service only
docker compose -f infra/compose/docker-compose.prod.yml build --no-cache grc-api

# 3. Restart only affected service
docker compose -f infra/compose/docker-compose.prod.yml up -d --force-recreate grc-api

# 4. Immediate verification
curl -sf https://portal.shahin-ai.com/health
```

---

## Post-Deployment Verification

### 1. Health Checks

```bash
# API health (all checks)
curl -sf https://portal.shahin-ai.com/health | jq .
# Expected: All entries "Healthy"

# Readiness check
curl -sf https://portal.shahin-ai.com/health/ready
# Expected: 200 OK

# Liveness check
curl -sf https://portal.shahin-ai.com/health/live
# Expected: 200 OK
```

### 2. Functional Verification

```bash
# Homepage loads
curl -sf -o /dev/null -w "%{http_code}" https://portal.shahin-ai.com/
# Expected: 200

# Login page loads
curl -sf -o /dev/null -w "%{http_code}" https://portal.shahin-ai.com/Account/Login
# Expected: 200

# API endpoint responds
curl -sf https://portal.shahin-ai.com/api/health
# Expected: JSON response

# Landing page loads
curl -sf -o /dev/null -w "%{http_code}" https://www.shahin-ai.com/
# Expected: 200
```

### 3. Log Verification

```bash
# Check for errors in last 5 minutes
docker compose -f infra/compose/docker-compose.prod.yml logs --since 5m grc-api | grep -i error

# Check for startup issues
docker compose -f infra/compose/docker-compose.prod.yml logs --tail=50 grc-api | grep -E "(Started|Error|Exception)"
```

### 4. Database Verification

```bash
# Check migrations applied
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";"

# Check database connectivity
docker compose -f infra/compose/docker-compose.prod.yml exec grc-api \
  curl -sf http://localhost:80/health/ready
```

---

## Rollback Procedures

### Quick Rollback (Previous Image)

```bash
# 1. Stop current containers
docker compose -f infra/compose/docker-compose.prod.yml stop grc-api grc-web

# 2. Revert to previous git commit
cd /opt/shahin-ai/Shahin-Jan-2026
git log --oneline -5  # Find previous commit
git checkout <previous-commit-hash>

# 3. Rebuild with previous code
docker compose -f infra/compose/docker-compose.prod.yml build grc-api grc-web

# 4. Start services
docker compose -f infra/compose/docker-compose.prod.yml up -d grc-api grc-web

# 5. Verify
curl -sf https://portal.shahin-ai.com/health
```

### Database Rollback

```bash
# WARNING: This will lose data created after the backup

# 1. Stop API to prevent new writes
docker compose -f infra/compose/docker-compose.prod.yml stop grc-api

# 2. Restore from backup
docker cp /opt/shahin-ai/backups/pre-deploy-YYYYMMDD-HHMMSS.dump shahin-grc-db:/tmp/restore.dump

docker compose -f infra/compose/docker-compose.prod.yml exec db \
  pg_restore -U grc_admin -d GrcMvcDb -c -F c /tmp/restore.dump

# 3. Restart API
docker compose -f infra/compose/docker-compose.prod.yml start grc-api

# 4. Verify
curl -sf https://portal.shahin-ai.com/health
```

### Full Rollback (Nuclear Option)

```bash
# Complete rollback to known good state

# 1. Stop all services
docker compose -f infra/compose/docker-compose.prod.yml down

# 2. Restore database from backup
# (Follow Database Rollback steps above)

# 3. Checkout known good commit
git checkout <known-good-commit>

# 4. Rebuild everything
docker compose -f infra/compose/docker-compose.prod.yml build --no-cache

# 5. Start all services
docker compose -f infra/compose/docker-compose.prod.yml up -d

# 6. Verify everything
docker compose -f infra/compose/docker-compose.prod.yml ps
curl -sf https://portal.shahin-ai.com/health
```

---

## Incident Response

### Service Down

```bash
# 1. Check container status
docker compose -f infra/compose/docker-compose.prod.yml ps -a

# 2. Check logs for errors
docker compose -f infra/compose/docker-compose.prod.yml logs --tail=100 grc-api

# 3. Restart affected service
docker compose -f infra/compose/docker-compose.prod.yml restart grc-api

# 4. If still down, check resources
docker stats --no-stream
df -h
free -h

# 5. If resource issue, restart all
docker compose -f infra/compose/docker-compose.prod.yml down
docker compose -f infra/compose/docker-compose.prod.yml up -d
```

### High Error Rate

```bash
# 1. Check recent errors
docker compose -f infra/compose/docker-compose.prod.yml logs --since 10m grc-api | grep -i error | tail -20

# 2. Check database connectivity
docker compose -f infra/compose/docker-compose.prod.yml exec db pg_isready

# 3. Check Redis
docker compose -f infra/compose/docker-compose.prod.yml exec redis redis-cli ping

# 4. If external service issue, check connectivity
curl -sf https://graph.microsoft.com/v1.0/$metadata -o /dev/null && echo "Graph OK" || echo "Graph FAIL"
```

### Database Connection Issues

```bash
# 1. Check database container
docker compose -f infra/compose/docker-compose.prod.yml logs --tail=50 db

# 2. Check connection pool
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "SELECT count(*) FROM pg_stat_activity;"

# 3. Kill idle connections if needed
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "
    SELECT pg_terminate_backend(pid) 
    FROM pg_stat_activity 
    WHERE state = 'idle' 
    AND query_start < now() - interval '10 minutes';"

# 4. Restart API to reset connection pool
docker compose -f infra/compose/docker-compose.prod.yml restart grc-api
```

---

## Maintenance Windows

### Scheduled Maintenance

```bash
# 1. Notify users (if applicable)
# Update status page or send notification

# 2. Enable maintenance mode (if implemented)
# curl -X POST https://portal.shahin-ai.com/api/admin/maintenance/enable

# 3. Perform maintenance tasks
# ...

# 4. Disable maintenance mode
# curl -X POST https://portal.shahin-ai.com/api/admin/maintenance/disable

# 5. Verify and notify users
```

### Database Maintenance

```bash
# Run during low-traffic period

# 1. VACUUM ANALYZE (reclaim space, update statistics)
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "VACUUM ANALYZE;"

# 2. Reindex if needed
docker compose -f infra/compose/docker-compose.prod.yml exec db \
  psql -U grc_admin -d GrcMvcDb -c "REINDEX DATABASE GrcMvcDb;"
```

---

## Release Checklist

### Before Release

- [ ] Code reviewed and approved
- [ ] Tests passing in staging
- [ ] Database migrations tested
- [ ] Backup created
- [ ] Disk space verified (> 5GB)
- [ ] Memory available (> 1GB)
- [ ] All services healthy
- [ ] Rollback plan ready

### During Release

- [ ] Pull latest code
- [ ] Note current image tags
- [ ] Build new images
- [ ] Apply migrations
- [ ] Restart services
- [ ] Monitor logs for errors

### After Release

- [ ] Health checks passing
- [ ] Functional tests passing
- [ ] No errors in logs
- [ ] Performance acceptable
- [ ] Notify stakeholders
- [ ] Update release notes

---

## Contact Information

| Role | Contact | When to Contact |
|------|---------|-----------------|
| On-Call Engineer | oncall@shahin-ai.com | Any production issue |
| Database Admin | dba@shahin-ai.com | Database issues |
| Security Team | security@shahin-ai.com | Security incidents |
| Management | management@shahin-ai.com | Major outages |

---

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-17 | 1.0 | Initial runbook |
