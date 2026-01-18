# Deployment Status Check Guide

## Quick Status Check

### Check All Environments
```bash
./scripts/check-env-status.sh
```

### Check Specific Environment
```bash
# Development
./scripts/check-env-status.sh dev

# Staging
./scripts/check-env-status.sh staging

# Production
./scripts/check-env-status.sh production
```

---

## Manual Status Checks

### 1. Docker Status
```bash
# Check Docker is running
docker ps

# Check all containers (including stopped)
docker ps -a

# Check Docker Compose services
docker-compose ps
```

### 2. Environment-Specific Checks

#### Development
```bash
# Check containers
docker ps --filter "name=grcmvc"

# Check health
curl http://localhost:5137/health

# Check logs
docker-compose logs -f grcmvc
```

#### Staging
```bash
# Check containers
docker ps --filter "name=shahin-grc-staging"

# Check health
curl http://localhost:8080/health

# Check logs
docker-compose -f docker-compose.staging.yml logs -f
```

#### Production
```bash
# Check containers
docker ps --filter "name=shahin-grc-production"

# Check health
curl http://localhost:5000/health
# Or via domain
curl https://portal.shahin-ai.com/health

# Check logs
docker-compose -f docker-compose.production.yml logs -f grcmvc-prod
```

### 3. Database Status
```bash
# Development
docker exec grcmvc-db pg_isready

# Staging
docker exec shahin-grc-db-staging pg_isready

# Production
docker exec shahin-grc-db-production pg_isready
```

### 4. Redis Status
```bash
# Development
docker exec grcmvc-redis redis-cli ping

# Staging
docker exec shahin-grc-redis-staging redis-cli ping

# Production
docker exec shahin-grc-redis-production redis-cli ping
```

### 5. Port Status
```bash
# Check which ports are listening
netstat -tuln | grep -E ":(5000|5137|8080|5432|6379)"
# Or
ss -tuln | grep -E ":(5000|5137|8080|5432|6379)"
```

### 6. Environment Variables
```bash
# Check if env files exist
ls -la .env* .env.production .env.staging

# View (without exposing secrets)
grep -v "PASSWORD\|SECRET\|KEY" .env.production
```

---

## Deployment Status by Environment

### Development Environment
- **Compose File**: `docker-compose.yml`
- **Port**: 5137
- **Database**: `grcmvc-db` (port 5432)
- **Redis**: `grcmvc-redis` (port 6379)
- **Status**: ✅ Active for local development

### Staging Environment
- **Compose File**: `docker-compose.staging.yml`
- **Port**: 8080
- **Database**: `shahin-grc-db-staging` (port 5434)
- **Redis**: `shahin-grc-redis-staging` (port 6381)
- **Status**: ✅ Active for pre-production testing

### Production Environment
- **Compose File**: `docker-compose.production.yml`
- **Port**: 5000 (internal), 8080 (direct)
- **Domain**: `portal.shahin-ai.com`
- **Database**: `shahin-grc-db-production` (port 5432)
- **Redis**: `shahin-grc-redis-production` (port 6379)
- **Status**: ✅ Active for production traffic

---

## Health Check Endpoints

| Environment | Health Endpoint | Expected Response |
|------------|----------------|------------------|
| Development | http://localhost:5137/health | `{"status":"Healthy"}` |
| Staging | http://localhost:8080/health | `{"status":"Healthy"}` |
| Production | https://portal.shahin-ai.com/health | `{"status":"Healthy"}` |

---

## Troubleshooting

### Container Not Starting
```bash
# Check logs
docker logs <container-name>

# Check resource usage
docker stats

# Restart container
docker restart <container-name>
```

### Database Connection Issues
```bash
# Test connection
docker exec <db-container> psql -U <username> -d <database> -c "SELECT 1;"

# Check database logs
docker logs <db-container>
```

### Port Already in Use
```bash
# Find process using port
lsof -i :5000
# Or
netstat -tulpn | grep :5000

# Kill process (if needed)
kill -9 <PID>
```

### Environment Variables Missing
```bash
# Check required variables
cat .env.production | grep -E "DB_PASSWORD|JWT_SECRET|CLAUDE_API_KEY"

# Copy from template
cp .env.production.template .env.production
# Then edit and set values
```

---

## Quick Deployment Commands

### Deploy Development
```bash
docker-compose up -d --build
```

### Deploy Staging
```bash
docker-compose -f docker-compose.staging.yml up -d --build
```

### Deploy Production
```bash
docker-compose -f docker-compose.production.yml up -d --build
```

### Stop All Environments
```bash
# Stop specific
docker-compose -f docker-compose.production.yml down

# Stop all
docker stop $(docker ps -q)
```

---

## Monitoring Commands

### View Real-time Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f grcmvc-prod
```

### Resource Usage
```bash
# Container stats
docker stats

# Disk usage
docker system df
```

### Network Status
```bash
# List networks
docker network ls

# Inspect network
docker network inspect <network-name>
```

---

## Next Steps After Status Check

1. ✅ All green: System is healthy
2. ⚠️ Warnings: Review logs and fix issues
3. ❌ Errors: Check troubleshooting section above
4. 🔄 Restart: Use deployment commands to restart services
