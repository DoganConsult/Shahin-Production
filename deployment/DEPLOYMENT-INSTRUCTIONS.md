# Production Deployment Instructions
## Server: 212.147.229.36 (Ubuntu Server 24.04 LTS)

### Prerequisites

1. **SSH Access**: You have the SSH private key for the server
2. **Local Docker**: Docker and Docker Compose installed locally
3. **Environment File**: Create `.env.production` with your configuration

### Quick Start

```bash
# 1. Navigate to deployment directory
cd Shahin-Jan-2026/deployment

# 2. Copy environment template
cp .env.production.template .env.production

# 3. Edit .env.production with your values
nano .env.production

# 4. Set SSH key path (or use default ~/.ssh/id_ed25519)
export SSH_KEY=~/.ssh/id_ed25519

# 5. Make script executable
chmod +x deploy-to-production.sh

# 6. Rebuild and deploy all 5 containers
./deploy-to-production.sh rebuild
```

### The 5 Containers

1. **landing** - Frontend (Next.js) - Port 3000
2. **portal** - Backend (ASP.NET Core) - Port 5000
3. **postgres** - Database (PostgreSQL 16) - Port 5432
4. **redis** - Cache (Redis 7) - Port 6379
5. **nginx** - Reverse Proxy (Nginx) - Ports 80, 443

### Deployment Commands

```bash
# Build containers locally
./deploy-to-production.sh build

# Deploy to server (without rebuilding)
./deploy-to-production.sh deploy

# Rebuild and deploy everything
./deploy-to-production.sh rebuild

# Check container status
./deploy-to-production.sh status

# View logs
./deploy-to-production.sh logs
./deploy-to-production.sh logs portal  # Specific service

# Stop all containers
./deploy-to-production.sh stop
```

### Access URLs

After deployment, access services at:

- **Landing Page**: http://212.147.229.36:3000
- **Portal/API**: http://212.147.229.36:5000
- **API Docs**: http://212.147.229.36:5000/api-docs
- **Health Check**: http://212.147.229.36:5000/health

### Environment Variables

Required variables in `.env.production`:

- `DB_PASSWORD` - Strong PostgreSQL password
- `JWT_SECRET` - Strong JWT secret (min 32 chars)
- `CLAUDE_API_KEY` - Your Claude AI API key

Optional:
- `REDIS_PASSWORD` - Redis password (leave empty for no auth)
- `SMTP_*` - Email configuration

### SSH Key Setup

The deployment script uses SSH to connect to the server. Set your SSH key:

```bash
# Option 1: Use environment variable
export SSH_KEY=/path/to/your/private/key

# Option 2: Place key at default location
cp your-private-key ~/.ssh/id_ed25519
chmod 600 ~/.ssh/id_ed25519
```

### Troubleshooting

**SSH Connection Failed**
```bash
# Test SSH connection manually
ssh -i ~/.ssh/id_ed25519 root@212.147.229.36
```

**Containers Not Starting**
```bash
# Check logs on server
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose logs"
```

**Database Connection Issues**
- Verify `DB_PASSWORD` in `.env.production`
- Check PostgreSQL container logs: `docker logs shahin-postgres-prod`

**Port Already in Use**
```bash
# Check what's using the port
ssh root@212.147.229.36 "netstat -tulpn | grep :3000"
```

### Manual Deployment

If you prefer to deploy manually:

```bash
# 1. SSH to server
ssh root@212.147.229.36

# 2. Create directory
mkdir -p /opt/shahin-ai
cd /opt/shahin-ai

# 3. Upload files (from local machine)
scp docker-compose.production-server.yml root@212.147.229.36:/opt/shahin-ai/
scp .env.production root@212.147.229.36:/opt/shahin-ai/.env

# 4. Deploy
docker-compose -f docker-compose.production-server.yml up -d --build
```

### Backup Database

```bash
# On server
docker exec shahin-postgres-prod pg_dump -U shahin_admin shahin_grc > backup.sql

# Copy to local
scp root@212.147.229.36:/opt/shahin-ai/backup.sql ./
```

### Update Single Container

```bash
# Rebuild and restart specific service
ssh root@212.147.229.36 "cd /opt/shahin-ai && docker-compose up -d --build portal"
```
