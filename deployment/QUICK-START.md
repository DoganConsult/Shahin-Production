# Quick Start - Complete Server Setup

## One Command Setup

```bash
cd deployment
./setup-server-complete.sh
```

This script will:
1. ✅ Push all images to Docker Hub
2. ✅ Connect via SSH to server
3. ✅ Install Docker and Docker Compose on server
4. ✅ Upload all deployment files
5. ✅ Deploy all 6 containers
6. ✅ Verify all services

## Prerequisites

### 1. Environment File
```bash
# Copy template
cp .env.production.template .env.production

# Edit with ACTUAL production keys
nano .env.production
```

**Required:**
- `DB_PASSWORD` - Strong password
- `JWT_SECRET` - Strong secret (min 32 chars)
- `CLAUDE_API_KEY` - Your API key
- `DOCKERHUB_USER` - Your Docker Hub username

### 2. SSH Key
```bash
# Set SSH key
export SSH_KEY=~/.ssh/id_ed25519

# Test connection
ssh -i ~/.ssh/id_ed25519 root@212.147.229.36
```

### 3. Docker Hub Login
```bash
docker login
```

## What Gets Deployed

### 6 Containers:
1. `landing` - Frontend (Next.js) - Port 3000
2. `portal` - Backend (ASP.NET Core) - Port 5000
3. `postgres` - Database (PostgreSQL 16) - Port 5432
4. `redis` - Cache (Redis 7) - Port 6379
5. `nginx` - Reverse Proxy - Ports 80, 443
6. `ollama` - LLM Server - Port 11434

**Note:** 3 LLM model containers already exist on server

## After Deployment

### Access URLs:
- Landing: http://212.147.229.36:3000
- Portal: http://212.147.229.36:5000
- Admin Login: http://212.147.229.36:5000/admin/login
- Platform Admin: http://212.147.229.36:5000/platform-admin
- API Docs: http://212.147.229.36:5000/api-docs

### DNS Domains:
- www.shahin-ai.com → Landing
- portal.shahin-ai.com → Portal
- admin.shahin-ai.com → Admin Portal
- login.shahin-ai.com → Login

## Troubleshooting

### Check Logs
```bash
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose logs"
```

### Restart Services
```bash
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose restart"
```

### Check Status
```bash
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose ps"
```
