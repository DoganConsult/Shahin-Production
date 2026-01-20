# Complete Deployment Guide

## Quick Deploy (All Steps)

```bash
cd deployment
./deploy-and-verify.sh
```

This script will:
1. ✅ Push code to GitHub
2. ✅ Build and push images to Docker Hub
3. ✅ Deploy to production server (212.147.229.36)
4. ✅ Verify all services and test paths
5. ✅ Check health endpoints
6. ✅ Test database and Redis connections

## Prerequisites

### 1. Environment File
```bash
# Copy template
cp .env.production.template .env.production

# Edit with ACTUAL production keys
nano .env.production
```

**Required Variables:**
- `DB_PASSWORD` - Strong PostgreSQL password
- `JWT_SECRET` - Strong JWT secret (min 32 chars)
- `CLAUDE_API_KEY` - Your Claude API key
- `DOCKERHUB_USER` - Your Docker Hub username

### 2. SSH Key
```bash
# Set SSH key path
export SSH_KEY=~/.ssh/id_ed25519

# Test connection
ssh -i ~/.ssh/id_ed25519 root@212.147.229.36
```

### 3. Docker Hub Login
```bash
docker login
```

## Manual Steps

### Step 1: Push to GitHub
```bash
./push-to-github.sh
```

### Step 2: Push to Docker Hub
```bash
export DOCKERHUB_USER=your-username
./push-to-dockerhub.sh
```

### Step 3: Deploy to Server
```bash
./deploy-to-production.sh rebuild
```

### Step 4: Verify Services
```bash
# Check container status
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose ps"

# Test endpoints
curl http://212.147.229.36:3000/health
curl http://212.147.229.36:5000/health
curl http://212.147.229.36:11434/api/tags
```

## Services Deployed

### Core (5):
1. `landing` - Frontend (Next.js) - Port 3000
2. `portal` - Backend (ASP.NET Core) - Port 5000
3. `postgres` - Database (PostgreSQL 16) - Port 5432
4. `redis` - Cache (Redis 7) - Port 6379
5. `nginx` - Reverse Proxy - Ports 80, 443

### LLM (1):
6. `ollama` - LLM Server - Port 11434
   - Note: 3 LLM model containers already exist on server

## DNS Configuration

Your DNS records are configured:
- `www.shahin-ai.com` / `shahin-ai.com` → Landing Page
- `portal.shahin-ai.com` → Portal/API
- `admin.shahin-ai.com` → Admin Portal
- `login.shahin-ai.com` → Login Page

All point to: `212.147.229.36`

## Health Check Endpoints

- Landing: `http://212.147.229.36:3000/health`
- Portal: `http://212.147.229.36:5000/health`
- API: `http://212.147.229.36:5000/api/health`
- Ollama: `http://212.147.229.36:11434/api/tags`
- Nginx: `http://212.147.229.36/health`

## Troubleshooting

### Deployment Fails
```bash
# Check logs
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose logs"

# Check specific service
ssh root@212.147.229.36 "docker logs shahin-portal-prod"
```

### Services Not Starting
```bash
# Restart all services
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose restart"

# Rebuild specific service
ssh root@212.147.229.36 "cd /opt/shin-ai && docker-compose up -d --build portal"
```

### Environment Variables Not Set
```bash
# Verify .env file on server
ssh root@212.147.229.36 "cat /opt/shahin-ai/.env"

# Update and redeploy
scp .env.production root@212.147.229.36:/opt/shahin-ai/.env
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose up -d"
```

## Post-Deployment Checklist

- [ ] All containers running (`docker ps`)
- [ ] Health endpoints responding (200 OK)
- [ ] Database accepting connections
- [ ] Redis responding (PONG)
- [ ] DNS domains resolving correctly
- [ ] SSL certificates configured (if using HTTPS)
- [ ] Backups configured
- [ ] Monitoring set up

## Support

For issues, check:
1. Container logs: `docker logs <container-name>`
2. Service status: `docker-compose ps`
3. Network connectivity: `docker network inspect shahin-network-prod`
